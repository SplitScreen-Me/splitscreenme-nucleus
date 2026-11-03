// Press to join, the pure rules: a free pad joins with Start; an empty place picked first ("waiting for a controller")
// takes the next press of any free pad instead, and so does a player's copy waiting for a guest controller (handlers with
// several players per copy). src/bridge/__tests__/lobby-join.test.ts runs them under `node --test`
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
  /**
   * Not a place: this player's copy waits for a guest controller (several players per copy). The screen fields are the
   * player's, and don't matter: the guest joins the player wherever it sits.
   */
  guestOf?: string;
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

/** The parts of a setup player the guest rules read. */
export interface SpotPlayer {
  id: string;
  /** How many guests its copy takes (0: one player per copy, or not a gamepad player). */
  maxGuests: number;
  guestDeviceIds: readonly string[];
}

/** Whether a player's copy of the game takes one more controller. */
export function hasGuestRoom(player: SpotPlayer): boolean {
  return player.maxGuests > player.guestDeviceIds.length;
}

/** The parts of a setup device a drop reads. */
export interface DropDevice {
  kind: string;
  revealed: boolean;
  playerId?: string;
  guestOfPlayerId?: string;
}

const PAD_KINDS = new Set(['xInput', 'dInput', 'sdl']);

/**
 * Whether a device dropped on a player joins that player's copy as a guest: a free, identified controller, and a copy
 * with room. Otherwise the drop takes the player's place (they swap).
 */
export function dropMakesGuest(player: SpotPlayer, device: DropDevice): boolean {
  return hasGuestRoom(player) && PAD_KINDS.has(device.kind) && device.revealed && !device.playerId && !device.guestOfPlayerId;
}

/**
 * Whether a waiting place can still be taken: its screen keeps its layout, and its slot still exists and is empty. A copy
 * waiting for a guest stays open while its player is there with room.
 */
export function spotStillOpen(spot: WaitingSpot, screens: readonly SpotScreen[], players: readonly SpotPlayer[] = []): boolean {
  if (spot.guestOf !== undefined) {
    const host = players.find((p) => p.id === spot.guestOf);
    return Boolean(host && hasGuestRoom(host));
  }
  const screen = screens.find((s) => s.index === spot.screenIndex);
  if (!screen || screen.layout !== spot.layout) return false;
  if (spot.slotIndex === undefined) return screen.layout === 'manual';
  const slot = screen.slots.find((s) => s.index === spot.slotIndex);
  return Boolean(slot && !slot.playerId);
}

/** Whether two waiting places are the same place (activating the waiting place again cancels it). */
export function sameSpot(a: WaitingSpot | null, b: Pick<WaitingSpot, 'screenIndex' | 'slotIndex' | 'guestOf'>): boolean {
  return Boolean(a && a.guestOf === b.guestOf && a.screenIndex === b.screenIndex && a.slotIndex === b.slotIndex);
}
