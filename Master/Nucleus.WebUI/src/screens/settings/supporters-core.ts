// Pure rules of the supporters credits (SupportersScreen.tsx): which state the screen is in, the auto-scroll step,
// and the remembered mute. No DOM, no imports beyond types: src/bridge/__tests__/supporters.test.ts runs them in node.
import type { SupportersList } from '../../bridge/generated/contract.ts';

/** localStorage key of the music preference ('1' muted). */
export const MUTED_KEY = 'nucleus.supporters.muted';
/** Credits speed, a little slower than the legacy window's (4 px per 70 ms) since the names are smaller. */
export const CREDITS_PX_PER_S = 42;
/** The title stays still this long before the names start rolling. */
export const CREDITS_START_HOLD_MS = 1800;
/** After the user scrolls (wheel, stick, keys), the roll waits this long, then goes on from there. */
export const CREDITS_SCRUB_HOLD_MS = 2500;
/** Music volume, as the legacy window's default. */
export const CREDITS_VOLUME = 0.4;
/** Fade-in of the music. */
export const CREDITS_FADE_MS = 1500;
/** Author of the music (Pixabay), credited at the end as in the legacy window. */
export const MUSIC_CREDIT_URL = 'https://pixabay.com/users/scaperlee-23524192/';

export type SupportersView =
  | { kind: 'loading' }
  | { kind: 'error' }
  | { kind: 'ready'; names: string[]; savedAt: string | null; musicUrl: string | null };

/** What the screen shows for the state of `supporters.get`. A copy without a date still counts as a copy. */
export function supportersView(q: { data?: SupportersList; isError: boolean }): SupportersView {
  if (q.data) {
    const savedAt = q.data.fromCache ? (q.data.savedAt ?? '') : null;
    return { kind: 'ready', names: q.data.names, savedAt, musicUrl: q.data.musicUrl ?? null };
  }
  return q.isError ? { kind: 'error' } : { kind: 'loading' };
}

export interface ScrollState {
  /** Where the roll is (fractional pixels: scrollTop rounds). */
  pos: number;
  /** The roll waits until then (the user scrolled). */
  holdUntil: number;
}

export interface ScrollInput {
  /** The scroller's scrollTop now. */
  top: number;
  /** Its largest scrollTop. */
  max: number;
  /** Time since the last frame, ms. */
  dt: number;
  now: number;
  /** Hovered, focused inside, paused by the user, or the page hidden. */
  paused: boolean;
}

/**
 * One animation frame of the roll. A scrollTop that moved away from where the roll left it is the user's own scroll:
 * the roll waits, then goes on from there. Returns the new state, the scrollTop to set (null: leave it), and whether
 * the end is reached.
 */
export function creditsStep(state: ScrollState, input: ScrollInput): { state: ScrollState; write: number | null; ended: boolean } {
  const { top, max, dt, now, paused } = input;
  if (Math.abs(top - state.pos) > 1.5) {
    return { state: { pos: top, holdUntil: now + CREDITS_SCRUB_HOLD_MS }, write: null, ended: top >= max - 1 };
  }
  if (paused || now < state.holdUntil) return { state: { ...state, pos: top }, write: null, ended: false };
  if (state.pos >= max - 0.5) return { state, write: null, ended: true };
  const pos = Math.min(max, state.pos + (CREDITS_PX_PER_S * Math.min(dt, 100)) / 1000);
  return { state: { ...state, pos }, write: pos, ended: pos >= max - 0.5 };
}

/** Whether the music was muted on an earlier visit. */
export function readMuted(store: { getItem(key: string): string | null } | null | undefined): boolean {
  try {
    return store?.getItem(MUTED_KEY) === '1';
  } catch {
    return false;
  }
}

export function writeMuted(store: { setItem(key: string, value: string): void } | null | undefined, muted: boolean): void {
  try {
    store?.setItem(MUTED_KEY, muted ? '1' : '0');
  } catch {
    // Storage disabled: the choice holds for this visit.
  }
}
