// Pure rules of couch navigation, without the DOM: reading a pad, auto-repeat and button edges, the lead pad of
// shared screens, what B does, which keys a widget takes from the pad, scroll steps and the choice of the next
// target. couch-nav.tsx applies them to the page; src/bridge/__tests__/couch-nav-core.test.ts runs them under
// `node --test` (keep this file erasable TypeScript with no imports).

export type Dir = 'left' | 'right' | 'up' | 'down';
/** Same names as the hint glyphs (`PadButton` in @/ui). */
export type PadAction = 'A' | 'B' | 'X' | 'Y' | 'LB' | 'RB' | 'View' | 'Menu';

/** A held direction fires once, then again after this delay... */
export const REPEAT_FIRST_MS = 380;
/** ...and then at this interval. */
export const REPEAT_NEXT_MS = 110;
/** Left stick deflection that counts as a d-pad press. */
export const NAV_AXIS = 0.6;
/** On a shared screen, the pad that moved focus last may press buttons for this long after its last move. */
export const LEAD_MS = 10_000;
/** Right stick dead zone. */
export const SCROLL_DEAD_ZONE = 0.25;
/** Right stick scroll speed at full tilt, in pixels per 60 Hz frame. */
export const SCROLL_PX_PER_FRAME = 20;
/** A d-pad press with nothing further in that direction scrolls by this share of the visible height. */
export const PAGE_FRACTION = 0.6;

const FRAME_MS = 1000 / 60;

/** The parts of a Gamepad (browser Gamepad API) the navigation reads. */
export interface PadLike {
  index: number;
  mapping: string;
  axes: readonly number[];
  buttons: ReadonlyArray<{ pressed: boolean }>;
}

export interface PadState {
  up: boolean;
  down: boolean;
  left: boolean;
  right: boolean;
  /** Pressed state of each action button this pad maps. */
  actions: Array<[PadAction, boolean]>;
  /** Right stick, -1…1 (0 when the pad has no standard mapping). */
  scrollX: number;
  scrollY: number;
}

const STANDARD: ReadonlyArray<readonly [number, PadAction]> = [
  [0, 'A'],
  [1, 'B'],
  [2, 'X'],
  [3, 'Y'],
  [4, 'LB'],
  [5, 'RB'],
  [8, 'View'],
  [9, 'Menu'],
];
/** Pads without the standard mapping: only the first two buttons are a safe guess. */
const NON_STANDARD: ReadonlyArray<readonly [number, PadAction]> = [
  [0, 'A'],
  [1, 'B'],
];

/**
 * Reads a pad. With the standard mapping: d-pad (12-15) or left stick moves, right stick (axes 2/3) scrolls, and the
 * face, shoulder, View and Menu buttons act. Other pads number their buttons and axes freely: the left stick moves
 * and buttons 0/1 are A/B, nothing else (a trigger resting at -1 on axis 2 or 3 must not scroll forever).
 */
export function readPad(gp: PadLike): PadState {
  const standard = gp.mapping === 'standard';
  const pressed = (i: number) => Boolean(gp.buttons[i]?.pressed);
  const dpad = (i: number) => standard && pressed(i);
  const axis = (i: number) => {
    const v = gp.axes[i] ?? 0;
    return Number.isFinite(v) ? v : 0;
  };
  const ax = axis(0);
  const ay = axis(1);
  return {
    up: dpad(12) || ay < -NAV_AXIS,
    down: dpad(13) || ay > NAV_AXIS,
    left: dpad(14) || ax < -NAV_AXIS,
    right: dpad(15) || ax > NAV_AXIS,
    actions: (standard ? STANDARD : NON_STANDARD).map(([i, action]) => [action, pressed(i)]),
    scrollX: standard ? axis(2) : 0,
    scrollY: standard ? axis(3) : 0,
  };
}

export interface PadTracker {
  /** A held direction: true on the press, then after REPEAT_FIRST_MS, then every REPEAT_NEXT_MS. */
  repeat(key: string, on: boolean, now: number): boolean;
  /** True once when a button goes down. Call it on every poll, even when the press may not act, so a button held
   *  while navigation was off (or while another pad led) never fires late. */
  edge(key: string, on: boolean): boolean;
  /** While navigation is off: a direction held now waits for its release before it fires again. */
  hold(key: string, on: boolean): void;
}

export function createPadTracker(first = REPEAT_FIRST_MS, next = REPEAT_NEXT_MS): PadTracker {
  const held = new Map<string, number>();
  const down = new Set<string>();
  return {
    repeat(key, on, now) {
      if (!on) {
        held.delete(key);
        return false;
      }
      const at = held.get(key);
      if (at === undefined) {
        held.set(key, now + first);
        return true;
      }
      if (now < at) return false;
      held.set(key, now + next);
      return true;
    },
    edge(key, on) {
      if (!on) {
        down.delete(key);
        return false;
      }
      if (down.has(key)) return false;
      down.add(key);
      return true;
    },
    hold(key, on) {
      if (on) held.set(key, Infinity);
      else held.delete(key);
    },
  };
}

/** The pad that moved focus last, and when. */
export interface Lead {
  index: number;
  at: number;
}

/**
 * Whether a pad's buttons may act. Everywhere but shared screens, every pad may. On a shared screen (the lobby, where
 * Start on a free pad joins) only the pad that moved focus within the last LEAD_MS does, so players pressing buttons to
 * join or to find their pad don't press the focused control.
 */
export function mayAct(shared: boolean, lead: Lead | null, index: number, now: number, ttl = LEAD_MS): boolean {
  if (!shared) return true;
  return lead !== null && lead.index === index && now - lead.at <= ttl;
}

/**
 * What pads may do on this poll: `all`, `hints` only (a screen waiting for a controller captures them: no moves, no
 * presses, only the screen's hint actions, so B can cancel; an open dialog lifts the capture), or `none` (navigation
 * off, suspended, or the window has no focus).
 */
export type PadReach = 'all' | 'hints' | 'none';

export function padReach(s: { active: boolean; captured: boolean; dialogOpen: boolean }): PadReach {
  if (!s.active) return 'none';
  return s.captured && !s.dialogOpen ? 'hints' : 'all';
}

export type BackStep = 'close-menu' | 'close-dialog' | 'history' | 'none';

/**
 * What B does once the screen's own B hint and the focused widget passed on it: close the innermost open menu, then
 * the open dialog. Otherwise go back in history, except on shared screens (B must never leave the lobby) and on the
 * rail's top-level screens (going back from there can reopen a setup).
 */
export function backStep(s: { menuOpen: boolean; dialogOpen: boolean; shared: boolean; topLevel: boolean; canGoBack: boolean }): BackStep {
  if (s.menuOpen) return 'close-menu';
  if (s.dialogOpen) return 'close-dialog';
  if (s.shared || s.topLevel || !s.canGoBack) return 'none';
  return 'history';
}

/** Whether a route path is one of the rail's top-level screens (or the root). */
export function isTopLevel(pathname: string, roots: readonly string[]): boolean {
  const path = pathname.replace(/\/+$/, '') || '/';
  return path === '/' || roots.some((r) => (r.replace(/\/+$/, '') || '/') === path);
}

/**
 * Focused widgets that read keys themselves, and which pad inputs reach them as key presses:
 * - `list`: menus, list boxes, combo boxes and `[data-pad-keys]` widgets (a lobby player area in resize mode) take
 *   arrows, Enter and Escape;
 * - `region`: lobby canvas regions take Enter (pick up, place) and Escape (cancel); the d-pad moves between them
 *   spatially, since their own arrows cycle through the regions forever and the pad could not leave the canvas;
 * - `capture`: other `[data-own-arrows]` controls (a shortcut recorder) only take Escape: an arrow or Enter would be
 *   recorded as the shortcut.
 */
export type KeyWidget = 'list' | 'region' | 'capture';

const ARROW: Record<Dir, string> = { up: 'ArrowUp', down: 'ArrowDown', left: 'ArrowLeft', right: 'ArrowRight' };

/** The key a pad input is sent as to the focused widget, or null when the pad handles it itself. */
export function forwardedKey(widget: KeyWidget | null, input: Dir | 'A' | 'B'): string | null {
  if (!widget) return null;
  if (input === 'B') return 'Escape';
  if (input === 'A') return widget === 'capture' ? null : 'Enter';
  return widget === 'list' ? ARROW[input] : null;
}

/** The option index a pad left/right moves a <select> to, skipping disabled options; `current` when it can't move. */
export function stepIndex(disabled: readonly boolean[], current: number, delta: 1 | -1): number {
  for (let i = current + delta; i >= 0 && i < disabled.length; i += delta) {
    if (!disabled[i]) return i;
  }
  return current;
}

/** Whether an element with this computed overflow and these sizes scrolls on that axis. */
export function isScrollable(overflow: string, scrollSize: number, clientSize: number): boolean {
  return (overflow === 'auto' || overflow === 'scroll' || overflow === 'overlay') && scrollSize > clientSize + 1;
}

/** Whether a scroller at `pos` can still move in the direction of `sign`. */
export function hasRoom(pos: number, scrollSize: number, clientSize: number, sign: number): boolean {
  return sign < 0 ? pos > 0.5 : pos + clientSize < scrollSize - 1;
}

/** Right stick deflection to pixels for a frame of `dtMs`: 0 inside the dead zone, then linear up to full speed. */
export function stickDelta(value: number, dtMs: number, dead = SCROLL_DEAD_ZONE, speed = SCROLL_PX_PER_FRAME): number {
  const m = Math.abs(value);
  if (!(m > dead)) return 0;
  const t = Math.min(1, (m - dead) / (1 - dead));
  return Math.sign(value) * t * speed * (Math.min(Math.max(dtMs, 0), 50) / FRAME_MS);
}

/** Splits accumulated scrolling into whole pixels to apply now and the remainder to keep for the next frame. */
export function wholePixels(acc: number): [number, number] {
  const whole = Math.trunc(acc);
  return [whole, acc - whole];
}

/** A d-pad page: PAGE_FRACTION of the visible height, signed. */
export function pageStep(clientSize: number, sign: number): number {
  return Math.sign(sign) * Math.max(40, Math.round(clientSize * PAGE_FRACTION));
}

export interface Box {
  left: number;
  top: number;
  right: number;
  bottom: number;
}

const centre = (b: Box) => ({ x: (b.left + b.right) / 2, y: (b.top + b.bottom) / 2 });

/**
 * The nearest candidate in a direction from `from`: it must lie past the edge of `from` on that side; the score is the
 * distance along the direction plus 2.2 times the distance across it.
 */
export function pickInDirection<T>(from: Box, dir: Dir, candidates: Iterable<{ item: T; box: Box }>): T | null {
  const a = centre(from);
  let best: T | null = null;
  let bestScore = Infinity;
  for (const { item, box: b } of candidates) {
    const ok =
      dir === 'left'
        ? b.right <= from.left + 2
        : dir === 'right'
          ? b.left >= from.right - 2
          : dir === 'up'
            ? b.bottom <= from.top + 2
            : b.top >= from.bottom - 2;
    if (!ok) continue;
    const c = centre(b);
    const horizontal = dir === 'left' || dir === 'right';
    const along = Math.abs(horizontal ? c.x - a.x : c.y - a.y);
    const across = Math.abs(horizontal ? c.y - a.y : c.x - a.x);
    const score = along + across * 2.2;
    if (score < bestScore) {
      bestScore = score;
      best = item;
    }
  }
  return best;
}

/**
 * Where an up/down move starts. When the focused element has scrolled out of view (right stick, d-pad pages), the move
 * starts from the edge of what is visible, so it lands on something on screen instead of jumping back.
 */
export function navOrigin(current: Box, view: Box, dir: Dir): Box {
  if (dir === 'left' || dir === 'right') return current;
  const outside = current.bottom <= view.top || current.top >= view.bottom;
  if (!outside) return current;
  const y = dir === 'down' ? view.top : view.bottom;
  return { left: current.left, right: current.right, top: y, bottom: y };
}

/**
 * Whether an up/down move should scroll a page instead of focusing `target`: when there is nothing that way, or when
 * the target lies more than a whole visible height past the edge, so reading a long text never skips a part of it.
 */
export function shouldPage(dir: 'up' | 'down', view: Box, target: Box | null): boolean {
  if (!target) return true;
  const height = view.bottom - view.top;
  const beyond = dir === 'down' ? target.bottom - view.bottom : view.top - target.top;
  return beyond > height;
}
