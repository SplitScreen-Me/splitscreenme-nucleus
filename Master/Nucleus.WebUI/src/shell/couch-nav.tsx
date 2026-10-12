// Couch navigation: arrow keys and the gamepad d-pad / left stick move focus spatially; A presses, B closes or goes
// back, X / Y / View / Menu run the screen's hint actions, LB / RB step through the rail, the right stick scrolls.
// Ported from the library mockup's spatial navigation. The pure rules live in couch-nav-core.ts.
//
// Targets: arrow keys move between [data-nav] elements (other controls keep their own arrows). Pads also reach every
// focusable control (buttons, links, fields, [tabindex], ARIA widgets), so no screen has dead ends. Hidden or disabled
// elements, [inert] subtrees and anything inside [data-nav="off"] (set on an element or a whole subtree, like the
// title bar) are skipped. With nothing focused, the first move lands on [data-autofocus] (or the first target inside
// it), else on the first target of the stage, never on the rail.
//
// Pads and players: the running session suspends pad navigation (useSuspendCouchNav: pads are playing the game). The
// lobby shares it (useSharedCouchNav): Start on a free pad joins there (the host's setup.deviceActivity), and players
// press buttons to find their pad, so every pad may move focus, but only the pad that moved last presses buttons, and B
// never leaves the screen. While the lobby waits for a controller on a place (useCapturedCouchNav), the next press of a
// free pad belongs to that place: pads neither move nor press anything, a button only runs the screen's hint action
// for it (B cancels).
import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { useLocation, useNavigate } from 'react-router';
import { useChromeApi, type Hint } from './chrome';
import {
  backStep,
  createPadTracker,
  forwardedKey,
  hasRoom,
  isScrollable,
  isTopLevel,
  mayAct,
  navOrigin,
  padReach,
  pageStep,
  pickInDirection,
  readPad,
  shouldPage,
  stepIndex,
  stickDelta,
  wholePixels,
  type Box,
  type Dir,
  type KeyWidget,
  type Lead,
  type PadAction,
} from './couch-nav-core';

interface Modes {
  suspend: (on: boolean) => void;
  share: (on: boolean) => void;
  capture: (on: boolean) => void;
}

const ModesCtx = createContext<Modes>({ suspend: () => {}, share: () => {}, capture: () => {} });

function useMode(mode: keyof Modes, on: boolean): void {
  const set = useContext(ModesCtx)[mode];
  useEffect(() => {
    if (!on) return;
    set(true);
    return () => set(false);
  }, [set, on]);
}

/** Suspends gamepad navigation while the calling screen is mounted (pads are playing a game). */
export function useSuspendCouchNav(suspended = true): void {
  useMode('suspend', suspended);
}

/**
 * Shares gamepad navigation with players while the calling screen is mounted (the lobby, where Start on a free pad
 * joins and players press buttons to find their pad): every pad moves focus and scrolls, only the pad that moved last
 * presses buttons, B only closes a menu or a dialog.
 */
export function useSharedCouchNav(shared = true): void {
  useMode('share', shared);
}

/**
 * Captures the pads while `captured` (the lobby waiting for a controller on a place, where the next press of a free pad
 * takes the place): they don't move focus, scroll or press anything; any pad's button only runs the screen's hint
 * action for that button, so B can cancel. An open dialog lifts the capture.
 */
export function useCapturedCouchNav(captured: boolean): void {
  useMode('capture', captured);
}

const KEYS: Record<string, Dir> = { ArrowLeft: 'left', ArrowRight: 'right', ArrowUp: 'up', ArrowDown: 'down' };

/** What the pad can reach besides [data-nav]: everything focusable, and ARIA widgets with a roving tabindex. */
const FOCUSABLE = [
  'button',
  'a[href]',
  'input:not([type="hidden"])',
  'select',
  'textarea',
  'summary',
  '[tabindex]:not([tabindex="-1"])',
  '[role="switch"]',
  '[role="tab"]',
  '[role="option"]',
  '[role="menuitem"]',
  '[role="checkbox"]',
  '[role="radio"]',
].join(', ');

type Reach = 'keys' | 'pad';

const reduceMotion = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
const motion = (): ScrollBehavior => (reduceMotion() ? 'auto' : 'smooth');

/** The open modal dialog: the one holding focus, else the last one. */
function topDialog(): HTMLDialogElement | null {
  const open = [...document.querySelectorAll<HTMLDialogElement>('dialog[open]')];
  const a = document.activeElement;
  return open.find((d) => a && d.contains(a)) ?? open[open.length - 1] ?? null;
}

function scope(): ParentNode {
  return topDialog() ?? document;
}

/** The focused element, or null when focus is nowhere (body). */
function activeEl(): HTMLElement | null {
  const a = document.activeElement;
  return a instanceof HTMLElement && a !== document.body ? a : null;
}

function navigable(el: HTMLElement): boolean {
  if (el.offsetParent === null || el.matches(':disabled')) return false;
  if (el.closest('[aria-hidden="true"], [inert], [data-nav="off"]')) return false;
  if (el.closest('details:not([open])') && !el.matches('details:not([open]) > summary')) return false;
  if (el.checkVisibility?.({ visibilityProperty: true }) === false) return false;
  // A role alone does not make an element focusable (list options driven by aria-activedescendant).
  return el.tabIndex >= 0 || el.hasAttribute('tabindex');
}

function navItems(reach: Reach): HTMLElement[] {
  return [...scope().querySelectorAll<HTMLElement>(reach === 'pad' ? `[data-nav], ${FOCUSABLE}` : '[data-nav]')].filter(navigable);
}

/** The target holding focus: the focused element itself, or the innermost target around it. */
function currentItem(items: HTMLElement[]): HTMLElement | null {
  const a = activeEl();
  if (!a) return null;
  let best: HTMLElement | null = null;
  for (const el of items) {
    if (el === a) return el;
    if (el.contains(a) && (!best || best.contains(el))) best = el;
  }
  return best;
}

/**
 * Where a first move lands: [data-autofocus] (or the first target inside it), then the first target of the stage
 * (one in view if any, so the page does not jump), then anything.
 */
function fallbackItem(items: HTMLElement[]): HTMLElement | null {
  for (const auto of scope().querySelectorAll<HTMLElement>('[data-autofocus]')) {
    const hit = items.find((el) => el === auto) ?? items.find((el) => auto.contains(el));
    if (hit) return hit;
  }
  const stage = document.getElementById('stage');
  const staged = stage ? items.filter((el) => stage.contains(el)) : [];
  if (stage && staged.length) {
    const view = viewOf(stage);
    const inView = staged.find((el) => {
      const r = el.getBoundingClientRect();
      return r.bottom > view.top && r.top < view.bottom;
    });
    return inView ?? staged[0]!;
  }
  return items[0] ?? null;
}

/** Up/down moves stay in their area: the stage, a dialog, or the rest of the shell (the rail). */
const areaOf = (el: Element) => el.closest('#stage, dialog');

function focusItem(el: HTMLElement): void {
  // focusVisible: pad focus is not a keyboard event, yet it must show the focus frame like one.
  el.focus({ preventScroll: true, focusVisible: true });
  el.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: motion() });
}

const toBox = (r: DOMRect): Box => ({ left: r.left, top: r.top, right: r.right, bottom: r.bottom });

type Axis = 'x' | 'y';

function scrolls(el: Element, axis: Axis): boolean {
  const s = getComputedStyle(el);
  return axis === 'y' ? isScrollable(s.overflowY, el.scrollHeight, el.clientHeight) : isScrollable(s.overflowX, el.scrollWidth, el.clientWidth);
}

function room(el: Element, axis: Axis, sign: number): boolean {
  return axis === 'y' ? hasRoom(el.scrollTop, el.scrollHeight, el.clientHeight, sign) : hasRoom(el.scrollLeft, el.scrollWidth, el.clientWidth, sign);
}

/** The nearest scroller around `from` (itself included), staying inside the open dialog. */
function scrollerAround(from: Element | null, axis: Axis, sign = 0): HTMLElement | null {
  const dialog = topDialog();
  for (let el = from instanceof HTMLElement ? from : null; el && el !== document.body; el = el.parentElement) {
    if (scrolls(el, axis) && (!sign || room(el, axis, sign))) return el;
    if (el === dialog) break;
  }
  return null;
}

/**
 * What a scroll moves: the nearest scroller around the focus that can still go that way, else the open dialog's
 * scrolling body, else the stage.
 */
function scrollTarget(axis: Axis, sign: number): HTMLElement | null {
  const near = scrollerAround(activeEl(), axis, sign);
  if (near) return near;
  const dialog = topDialog();
  const roots: Array<HTMLElement | null> = dialog ? [dialog, ...dialog.querySelectorAll<HTMLElement>('*')] : [document.getElementById('stage')];
  for (const el of roots) if (el && scrolls(el, axis) && room(el, axis, sign)) return el;
  return null;
}

/** The visible part of a scroller, in viewport coordinates. */
function viewOf(el: HTMLElement): Box {
  const r = el.getBoundingClientRect();
  const top = r.top + el.clientTop;
  return { left: r.left, right: r.right, top, bottom: top + el.clientHeight };
}

function page(el: HTMLElement, sign: number): void {
  el.scrollBy({ top: pageStep(el.clientHeight, sign), behavior: motion() });
}

/**
 * Pad up/down, scroller by scroller from the focus outwards (a side pane, then the stage): the next target inside the
 * scroller, or a page of it when nothing is that way or the target is more than a screen away; once a scroller is at
 * its end, the next one out. Then the whole area, and last a page of whatever can still scroll (the stage, from the
 * rail).
 */
function padVertical(cur: HTMLElement, dir: 'up' | 'down', inArea: HTMLElement[]): boolean {
  const sign = dir === 'down' ? 1 : -1;
  const boxOf = (el: HTMLElement) => toBox(el.getBoundingClientRect());
  let from = boxOf(cur);
  for (let s = scrollerAround(cur, 'y'); s; s = scrollerAround(s.parentElement, 'y')) {
    const scroller = s;
    const view = viewOf(scroller);
    from = navOrigin(boxOf(cur), view, dir);
    const inside = inArea.filter((el) => scroller.contains(el)).map((item) => ({ item, box: boxOf(item) }));
    const target = pickInDirection(from, dir, inside);
    if (target && !shouldPage(dir, view, boxOf(target))) {
      focusItem(target);
      return true;
    }
    if (room(scroller, 'y', sign)) {
      page(scroller, sign);
      return true;
    }
  }
  const target = pickInDirection(
    from,
    dir,
    inArea.map((item) => ({ item, box: boxOf(item) })),
  );
  if (target) {
    focusItem(target);
    return true;
  }
  const pager = scrollTarget('y', sign);
  if (pager) page(pager, sign);
  return Boolean(pager);
}

/**
 * Moves focus to the nearest target in a direction; up and down stay in the current area (stage, dialog, rail), left
 * and right cross between them. Arrow keys (`keys`) only reach [data-nav] elements. Pads (`pad`) reach every focusable
 * control, and up/down also scroll (see padVertical). Returns false when nothing moved.
 */
export function moveFocus(dir: Dir, reach: Reach = 'keys'): boolean {
  document.documentElement.classList.add('nav-input');
  const items = navItems(reach);
  const cur = currentItem(items);
  if (!cur) {
    const first = fallbackItem(items);
    if (first) focusItem(first);
    return Boolean(first);
  }
  const vertical = dir === 'up' || dir === 'down';
  const area = areaOf(cur);
  const others = items.filter((el) => el !== cur && (!vertical || areaOf(el) === area));
  if (reach === 'pad' && vertical) return padVertical(cur, dir, others);
  const target = pickInDirection(
    toBox(cur.getBoundingClientRect()),
    dir,
    others.map((item) => ({ item, box: toBox(item.getBoundingClientRect()) })),
  );
  if (!target) return false;
  focusItem(target);
  return true;
}

/** Which focused widgets read keys themselves (see KeyWidget). */
function keyWidget(el: Element | null): KeyWidget | null {
  if (!el) return null;
  if (el.closest('[role="menu"], [role="listbox"], [role="combobox"]')) return 'list';
  if (el.closest('[data-region]')) return 'region';
  if (el.closest('[data-own-arrows]')) return 'capture';
  return null;
}

/** Key presses the pad sends to widgets; the keyboard handler below leaves them to the pad's own fallback. */
const padKeys = new WeakSet<Event>();

/** Sends a key press to an element. True when something handled it (prevented its default). */
function sendKey(target: Element, key: string): boolean {
  const e = new KeyboardEvent('keydown', { key, code: key, bubbles: true, cancelable: true, composed: true });
  padKeys.add(e);
  return !target.dispatchEvent(e);
}

const selectValue = Object.getOwnPropertyDescriptor(HTMLSelectElement.prototype, 'value')?.set;

/** Pad left/right on a native <select>: picks the previous or next option, as a user change React sees. */
function stepSelect(select: HTMLSelectElement, delta: 1 | -1): void {
  const options = [...select.options];
  const next = stepIndex(
    options.map((o) => o.disabled || o.hidden !== false),
    select.selectedIndex,
    delta,
  );
  const option = options[next];
  if (next === select.selectedIndex || !option) return;
  // Set it as the browser does for a user (the prototype setter, past anything React put on the element), then send
  // the events a user change sends: React's onChange listens to `change`.
  if (selectValue) selectValue.call(select, option.value);
  else select.selectedIndex = next;
  select.dispatchEvent(new Event('input', { bubbles: true }));
  select.dispatchEvent(new Event('change', { bubbles: true }));
}

function openMenu(): HTMLElement | null {
  return [...scope().querySelectorAll<HTMLElement>('[role="menu"]')].find((m) => m.offsetParent !== null) ?? null;
}

function railRoots(): string[] {
  return [...document.querySelectorAll<HTMLAnchorElement>('[data-rail-link]')].map((a) => a.hash.replace(/^#/, '') || a.pathname);
}

export function CouchNavProvider({ enabled, children }: { enabled: boolean; children: ReactNode }) {
  const [suspendCount, setSuspendCount] = useState(0);
  const [shareCount, setShareCount] = useState(0);
  const [captureCount, setCaptureCount] = useState(0);
  const suspend = useCallback((on: boolean) => setSuspendCount((n) => Math.max(0, n + (on ? 1 : -1))), []);
  const share = useCallback((on: boolean) => setShareCount((n) => Math.max(0, n + (on ? 1 : -1))), []);
  const capture = useCallback((on: boolean) => setCaptureCount((n) => Math.max(0, n + (on ? 1 : -1))), []);
  const [modes] = useState<Modes>(() => ({ suspend, share, capture }));
  const chrome = useChromeApi();
  const navigate = useNavigate();
  const { pathname } = useLocation();
  const live = useRef({ enabled, suspended: false, shared: false, captured: false, chrome, navigate, pathname });
  live.current = { enabled, suspended: suspendCount > 0, shared: shareCount > 0, captured: captureCount > 0, chrome, navigate, pathname };

  // Keyboard: arrows move between [data-nav] items unless a control needs them.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      if (padKeys.has(e)) return;
      const dir = KEYS[e.key];
      if (dir) {
        if (e.defaultPrevented || e.altKey || e.ctrlKey || e.metaKey) return;
        const t = e.target as HTMLElement;
        if (t.closest('input, textarea, select, [role="menu"], [role="slider"], [role="listbox"], [data-own-arrows]')) return;
        // Only steer from a navigable item or from nowhere: other controls keep their own arrow behaviour.
        const fromNowhere = t === document.body || t.id === 'stage';
        const item = t.closest('[data-nav]');
        if (!fromNowhere && (!item || item.closest('[data-nav="off"]'))) return;
        if (moveFocus(dir)) e.preventDefault();
      } else if (e.key === 'Tab') {
        document.documentElement.classList.add('nav-input');
      }
    };
    const onMouse = () => document.documentElement.classList.remove('nav-input');
    document.addEventListener('keydown', onKey);
    document.addEventListener('mousedown', onMouse);
    return () => {
      document.removeEventListener('keydown', onKey);
      document.removeEventListener('mousedown', onMouse);
    };
  }, []);

  // Gamepad: polled only while a pad is connected.
  useEffect(() => {
    const tracker = createPadTracker();
    let lead: Lead | null = null;
    let wasShared = false;
    let raf = 0;
    let last = 0;
    const carry = { x: 0, y: 0 };

    const runHint = (button: PadAction) => {
      const hint: Hint | undefined = live.current.chrome.activeHints().find((h) => h.button === button && h.action);
      if (!hint?.action) return false;
      hint.action();
      return true;
    };

    const stepRail = (delta: number) => {
      const links = [...document.querySelectorAll<HTMLAnchorElement>('[data-rail-link]')];
      const cur = links.findIndex((l) => l.getAttribute('aria-current') === 'page');
      const next = links[(cur + delta + links.length) % links.length];
      next?.click();
    };

    const move = (dir: Dir) => {
      document.documentElement.classList.add('nav-input');
      const a = activeEl();
      const key = forwardedKey(keyWidget(a), dir);
      if (a && key && sendKey(a, key)) return;
      if (a instanceof HTMLSelectElement && (dir === 'left' || dir === 'right')) {
        stepSelect(a, dir === 'right' ? 1 : -1);
        return;
      }
      moveFocus(dir, 'pad');
    };

    const press = (action: PadAction) => {
      const dialog = topDialog();
      // The screen's hints belong to the screen: not while a dialog is over it.
      if (!dialog && runHint(action)) return;
      const a = activeEl();
      if (action === 'A') {
        document.documentElement.classList.add('nav-input');
        if (!a) return;
        const key = forwardedKey(keyWidget(a), 'A');
        if (!(key && sendKey(a, key))) a.click();
      } else if (action === 'B') {
        const key = forwardedKey(keyWidget(a), 'B');
        if (a && key && sendKey(a, key)) return;
        const { shared, pathname, navigate } = live.current;
        const menu = openMenu();
        const step = backStep({
          menuOpen: Boolean(menu),
          dialogOpen: Boolean(dialog),
          shared,
          topLevel: isTopLevel(pathname, railRoots()),
          canGoBack: window.history.length > 1,
        });
        if (step === 'close-menu') sendKey(menu!, 'Escape');
        else if (step === 'close-dialog') dialog!.close();
        else if (step === 'history') void navigate(-1);
      } else if (!dialog) {
        if (action === 'LB') stepRail(-1);
        else if (action === 'RB') stepRail(1);
        else if (action === 'Y') document.querySelector<HTMLButtonElement>('[data-command-palette]')?.click();
      }
    };

    const scroll = (axis: Axis, value: number, dt: number) => {
      const delta = stickDelta(value, dt);
      if (!delta) {
        carry[axis] = 0;
        return;
      }
      const [whole, rest] = wholePixels(carry[axis] + delta);
      carry[axis] = rest;
      if (!whole) return;
      scrollTarget(axis, whole)?.scrollBy({ [axis === 'y' ? 'top' : 'left']: whole, behavior: 'instant' });
    };

    const poll = (now: number) => {
      const dt = last ? now - last : 1000 / 60;
      last = now;
      const pads = navigator.getGamepads ? [...navigator.getGamepads()].filter((p): p is Gamepad => Boolean(p?.connected)) : [];
      // Next frame first: an action that throws must not stop the polling.
      raf = pads.length ? requestAnimationFrame(poll) : 0;
      if (!pads.length) last = 0;
      const { enabled: on, suspended, shared, captured } = live.current;
      // A shared screen starts with no lead: the press that opened it is not a move on it.
      if (shared && !wasShared) lead = null;
      wasShared = shared;
      // Waiting for a controller: the next press belongs to it, only the screen's hint actions run (B cancels).
      const reach = padReach({ active: on && !suspended && document.hasFocus(), captured, dialogOpen: captured && Boolean(topDialog()) });
      let sx = 0;
      let sy = 0;
      for (const gp of pads) {
        const pad = readPad(gp);
        const id = gp.index;
        const dirs: Array<[Dir, boolean]> = [
          ['up', pad.up],
          ['down', pad.down],
          ['left', pad.left],
          ['right', pad.right],
        ];
        // Edges are tracked on every poll, so a button held while it could not act never fires late.
        const pressed = pad.actions.filter(([action, down]) => tracker.edge(`${action}${id}`, down)).map(([action]) => action);
        if (reach !== 'all') {
          for (const [dir, held] of dirs) tracker.hold(`${dir}${id}`, held);
          if (reach === 'hints') for (const action of pressed) runHint(action);
          continue;
        }
        for (const [dir, held] of dirs) {
          if (!tracker.repeat(`${dir}${id}`, held, now)) continue;
          lead = { index: id, at: now };
          move(dir);
        }
        for (const action of pressed) {
          if (!mayAct(shared, lead, id, now)) continue;
          lead = { index: id, at: now };
          press(action);
        }
        if (Math.abs(pad.scrollX) > Math.abs(sx)) sx = pad.scrollX;
        if (Math.abs(pad.scrollY) > Math.abs(sy)) sy = pad.scrollY;
      }
      scroll('y', sy, dt);
      scroll('x', sx, dt);
    };

    const start = () => {
      if (!raf) raf = requestAnimationFrame(poll);
    };
    window.addEventListener('gamepadconnected', start);
    if (navigator.getGamepads && [...navigator.getGamepads()].some(Boolean)) start();
    return () => {
      window.removeEventListener('gamepadconnected', start);
      if (raf) cancelAnimationFrame(raf);
    };
  }, []);

  return <ModesCtx value={modes}>{children}</ModesCtx>;
}
