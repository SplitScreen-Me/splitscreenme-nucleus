// Couch navigation: arrow keys and the gamepad d-pad / left stick move focus spatially between
// [data-nav] elements; A presses, B goes back, X / Y / Menu run the screen's hint actions,
// LB / RB step through the rail. Ported from the library mockup's spatial navigation.
//
// Screens where gamepads belong to players (lobby, session) suspend it: their buttons mean "join",
// "ping" or in-game input there, and arrive as the host's `setup.deviceActivity` events instead.
import { createContext, useCallback, useContext, useEffect, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { useChromeApi, type Hint } from './chrome';
import type { PadButton } from '@/ui';

type Dir = 'left' | 'right' | 'up' | 'down';

const SuspendCtx = createContext<(on: boolean) => void>(() => {});

/** Suspends gamepad navigation while the calling screen is mounted. */
export function useSuspendCouchNav(suspended = true): void {
  const set = useContext(SuspendCtx);
  useEffect(() => {
    if (!suspended) return;
    set(true);
    return () => set(false);
  }, [set, suspended]);
}

const KEYS: Record<string, Dir> = { ArrowLeft: 'left', ArrowRight: 'right', ArrowUp: 'up', ArrowDown: 'down' };
const BUTTONS: Partial<Record<number, PadButton>> = { 1: 'B', 2: 'X', 3: 'Y', 4: 'LB', 5: 'RB', 8: 'View', 9: 'Menu' };

const reduceMotion = () => matchMedia('(prefers-reduced-motion: reduce)').matches;

function scope(): ParentNode {
  return document.querySelector('dialog[open]') ?? document;
}

function navItems(): HTMLElement[] {
  return [...scope().querySelectorAll<HTMLElement>('[data-nav]')].filter(
    (el) => el.offsetParent !== null && !(el as HTMLButtonElement).disabled && el.getAttribute('aria-hidden') !== 'true',
  );
}

const center = (r: DOMRect) => ({ x: r.left + r.width / 2, y: r.top + r.height / 2 });

/** Moves focus to the nearest [data-nav] element in a direction. Returns false when nothing is there. */
export function moveFocus(dir: Dir): boolean {
  document.documentElement.classList.add('nav-input');
  const items = navItems();
  const cur = (document.activeElement as HTMLElement | null)?.closest<HTMLElement>('[data-nav]');
  if (!cur || !items.includes(cur)) {
    const first = items[0];
    first?.focus();
    return Boolean(first);
  }
  const a = cur.getBoundingClientRect();
  const ca = center(a);
  let best: HTMLElement | null = null;
  let bestScore = Infinity;
  for (const el of items) {
    if (el === cur) continue;
    const b = el.getBoundingClientRect();
    const cb = center(b);
    const dx = cb.x - ca.x;
    const dy = cb.y - ca.y;
    const ok =
      dir === 'left' ? b.right <= a.left + 2 : dir === 'right' ? b.left >= a.right - 2 : dir === 'up' ? b.bottom <= a.top + 2 : b.top >= a.bottom - 2;
    if (!ok) continue;
    const horizontal = dir === 'left' || dir === 'right';
    const along = horizontal ? Math.abs(dx) : Math.abs(dy);
    const across = horizontal ? Math.abs(dy) : Math.abs(dx);
    const score = along + across * 2.2;
    if (score < bestScore) {
      bestScore = score;
      best = el;
    }
  }
  if (!best) return false;
  best.focus({ preventScroll: true });
  best.scrollIntoView({ block: 'nearest', inline: 'nearest', behavior: reduceMotion() ? 'auto' : 'smooth' });
  return true;
}

export function CouchNavProvider({ enabled, children }: { enabled: boolean; children: ReactNode }) {
  const [suspendCount, setSuspendCount] = useState(0);
  const suspend = useCallback((on: boolean) => setSuspendCount((n) => Math.max(0, n + (on ? 1 : -1))), []);
  const chrome = useChromeApi();
  const navigate = useNavigate();
  const live = useRef({ enabled, suspended: false, chrome, navigate });
  live.current = { enabled, suspended: suspendCount > 0, chrome, navigate };

  // Keyboard: arrows move between [data-nav] items unless a control needs them.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const dir = KEYS[e.key];
      if (dir) {
        if (e.defaultPrevented || e.altKey || e.ctrlKey || e.metaKey) return;
        const t = e.target as HTMLElement;
        if (t.closest('input, textarea, select, [role="menu"], [role="slider"], [role="listbox"], [data-own-arrows]')) return;
        // Only steer from a navigable item or from nowhere: other controls keep their own arrow behaviour.
        const fromNowhere = t === document.body || t.id === 'stage';
        if (!fromNowhere && !t.closest('[data-nav]')) return;
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
    const REPEAT_FIRST = 380;
    const REPEAT_NEXT = 110;
    const held = new Map<string, { next: number }>();
    const pressed = new Set<string>();
    let raf = 0;

    const repeat = (name: string, on: boolean, fn: () => void, now: number) => {
      const h = held.get(name);
      if (!on) {
        held.delete(name);
        return;
      }
      if (!h) {
        held.set(name, { next: now + REPEAT_FIRST });
        fn();
      } else if (now >= h.next) {
        h.next = now + REPEAT_NEXT;
        fn();
      }
    };
    const edge = (name: string, on: boolean, fn: () => void) => {
      if (on && !pressed.has(name)) {
        pressed.add(name);
        fn();
      } else if (!on) pressed.delete(name);
    };

    const runHint = (button: PadButton) => {
      const hint: Hint | undefined = live.current.chrome.activeHints().find((h) => h.button === button && h.action);
      if (hint?.action) {
        hint.action();
        return true;
      }
      return false;
    };

    const stepRail = (delta: number) => {
      const links = [...document.querySelectorAll<HTMLAnchorElement>('[data-rail-link]')];
      const cur = links.findIndex((l) => l.getAttribute('aria-current') === 'page');
      const next = links[(cur + delta + links.length) % links.length];
      next?.click();
    };

    const poll = (now: number) => {
      raf = 0;
      const pads = navigator.getGamepads ? [...navigator.getGamepads()].filter((p): p is Gamepad => Boolean(p)) : [];
      const { enabled: on, suspended } = live.current;
      if (on && !suspended && document.hasFocus()) {
        for (const gp of pads) {
          const b = (i: number) => Boolean(gp.buttons[i]?.pressed);
          const ax = gp.axes[0] ?? 0;
          const ay = gp.axes[1] ?? 0;
          const id = gp.index;
          repeat(`u${id}`, b(12) || ay < -0.6, () => moveFocus('up'), now);
          repeat(`d${id}`, b(13) || ay > 0.6, () => moveFocus('down'), now);
          repeat(`l${id}`, b(14) || ax < -0.6, () => moveFocus('left'), now);
          repeat(`r${id}`, b(15) || ax > 0.6, () => moveFocus('right'), now);
          edge(`a${id}`, b(0), () => {
            document.documentElement.classList.add('nav-input');
            if (!runHint('A')) (document.activeElement as HTMLElement | null)?.click();
          });
          for (const [index, button] of Object.entries(BUTTONS)) {
            edge(`${button}${id}`, b(Number(index)), () => {
              if (runHint(button!)) return;
              if (button === 'B') {
                const dialog = document.querySelector('dialog[open]');
                if (dialog) (dialog as HTMLDialogElement).close();
                else if (window.history.length > 1) live.current.navigate(-1);
              } else if (button === 'LB') stepRail(-1);
              else if (button === 'RB') stepRail(1);
              else if (button === 'Y') document.querySelector<HTMLButtonElement>('[data-command-palette]')?.click();
            });
          }
        }
      } else {
        held.clear();
        // Keep edges in sync so a button held while suspended does not fire on resume.
        for (const gp of pads) gp.buttons.forEach((btn, i) => btn.pressed && pressed.add(`${BUTTONS[i] ?? (i === 0 ? 'a' : 'x')}${gp.index}`));
      }
      if (pads.length) raf = requestAnimationFrame(poll);
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

  return <SuspendCtx value={suspend}>{children}</SuspendCtx>;
}
