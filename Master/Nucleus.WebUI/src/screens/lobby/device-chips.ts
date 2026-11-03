// The devices of a keyboard and mouse player as chips on its area, each grabbed on its own (Nucleus 2.5 split the
// keyboard and mouse icon of the setup screen). The pure rules: src/bridge/__tests__/lobby-chips.test.ts runs them under
// `node --test` (keep this file erasable TypeScript with no imports).
//
// Dragging the area still moves the whole player; dragging a chip moves only its device (`setup.assignToSlot` /
// `assignAt` move one device of a pair, the host's drag semantics): to an empty place it becomes a player of its own, on
// a lone keyboard or mouse it completes it, on another player the two players swap; dropped on the tray it leaves its
// player (`setup.unassign`), as the × of its row in the side panel does.

/** The parts of a setup device the chips read. */
export interface ChipDevice {
  id: string;
  /** Player it drives. */
  playerId?: string;
  /** Player whose copy it joins as a guest. */
  guestOfPlayerId?: string;
}

/** The parts of a setup player the chips read. */
export interface ChipPlayer {
  id: string;
  deviceIds: readonly string[];
}

/** The devices of a player shown as separate chips: all of them when it has more than one (a raw keyboard and mouse). */
export function deviceChips<D extends ChipDevice>(player: ChipPlayer, devices: readonly D[]): D[] {
  if (player.deviceIds.length < 2) return [];
  return player.deviceIds.map((id) => devices.find((d) => d.id === id)).filter((d): d is D => d !== undefined);
}

/** Where a dragged device is dropped: on a player's area, on an empty place (a slot, a spot of a manual screen), or on the tray. */
export type DeviceDropTarget = { kind: 'player'; playerId: string } | { kind: 'place' } | { kind: 'tray' };

/**
 * What a dropped device does: nothing (back on its own player, or a free device back on the tray), joins a player's
 * copy as a guest (`guestDrop`: a free controller on a copy with room, see join-rules.ts), takes the place (assign:
 * the host completes, swaps or splits), or leaves its player (unassign).
 */
export type DeviceDropAction = 'none' | 'guest' | 'place' | 'unassign';

export function deviceDropAction(device: ChipDevice, target: DeviceDropTarget, guestDrop = false): DeviceDropAction {
  const placed = Boolean(device.playerId || device.guestOfPlayerId);
  if (target.kind === 'tray') return placed ? 'unassign' : 'none';
  if (target.kind === 'player') {
    if (device.playerId === target.playerId) return 'none';
    if (guestDrop) return 'guest';
  }
  return 'place';
}

/** Whether the tray takes what is dragged: a player, or a device that has a player (a free one is already there). */
export function trayTakes(kind: 'player' | 'device' | null, device: ChipDevice | undefined): boolean {
  if (kind === 'player') return true;
  return kind === 'device' && device !== undefined && deviceDropAction(device, { kind: 'tray' }) === 'unassign';
}
