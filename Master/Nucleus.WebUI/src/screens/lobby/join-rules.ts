// Press to join, the pure rules: a free pad joins with Start; an empty place picked first ("waiting for a controller")
// takes the next press of any free pad instead. src/bridge/__tests__/lobby-join.test.ts runs them under `node --test`
// (keep this file erasable TypeScript with no imports).

/** An empty place picked to wait for a controller: a slot of a grid layout, or a point of a manual screen. */
export interface WaitingSpot {
  screenIndex: number;
  /** Slot of a grid layout; undefined for a point of a manual screen. */
  slotIndex?: number;
  /** Point of a manual screen, as fractions of it. */
  x: number;
  y: number;
  /** Layout of the screen when the place was picked: another layout makes it stale. */
  layout: string;
  /** performance.now() when it was picked. */
  at: number;
  /** Picked with a pad: the host may report that press after the place was picked. */
  byPad: boolean;
}

/**
 * After a pad picks a place, its presses reach the host's event this late at most (the host polls pads every 150 ms):
 * a press reported sooner is the one that picked the place, not the next one.
 */
export const PICK_GRACE_MS = 450;

/** Whether a gamepad press asks to join: it pushed Start. */
export function pressJoins(buttons: readonly string[] | undefined): boolean {
  return Boolean(buttons?.includes('start'));
}

/**
 * Whether a free pad's press takes the waiting place: a new press the host could read (with buttons), not B alone
 * (B cancels the wait), and not the press that picked the place.
 */
export function pressTakesSpot(spot: WaitingSpot | null, buttons: readonly string[] | undefined, now: number, grace = PICK_GRACE_MS): boolean {
  if (!spot || !buttons?.length) return false;
  if (buttons.length === 1 && buttons[0] === 'b') return false;
  return !(spot.byPad && now - spot.at < grace);
}

/** The parts of a setup screen a waiting place depends on. */
export interface SpotScreen {
  index: number;
  layout: string;
  slots: ReadonlyArray<{ index: number; playerId?: string }>;
}

/** Whether a waiting place can still be taken: its screen keeps its layout, and its slot still exists and is empty. */
export function spotStillOpen(spot: WaitingSpot, screens: readonly SpotScreen[]): boolean {
  const screen = screens.find((s) => s.index === spot.screenIndex);
  if (!screen || screen.layout !== spot.layout) return false;
  if (spot.slotIndex === undefined) return screen.layout === 'manual';
  const slot = screen.slots.find((s) => s.index === spot.slotIndex);
  return Boolean(slot && !slot.playerId);
}

/** Whether two waiting places are the same place (activating the waiting place again cancels it). */
export function sameSpot(a: WaitingSpot | null, b: Pick<WaitingSpot, 'screenIndex' | 'slotIndex'>): boolean {
  return Boolean(a && a.screenIndex === b.screenIndex && a.slotIndex === b.slotIndex);
}
