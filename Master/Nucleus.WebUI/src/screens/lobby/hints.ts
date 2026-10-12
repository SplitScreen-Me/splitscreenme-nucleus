// The lobby's gamepad hints, the pure part: a button is only tied to what it does whatever has focus.
// src/bridge/__tests__/lobby-hints.test.ts runs it under `node --test` (keep this file erasable TypeScript with no imports).

/** One hint of the bar: the shape of shell/chrome's Hint, restated so this file needs no imports. */
export interface LobbyHint {
  button?: 'A' | 'B' | 'X' | 'Menu';
  key?: string;
  label: string;
  action?: () => void;
}

/** The `data-hints` of a player area: one that can be resized, and one in resize mode. */
export const PLAYER_RESIZABLE = 'player-resizable';
export const PLAYER_RESIZING = 'resizing';

export interface LobbyHintActions {
  /** Stops waiting for a controller. */
  cancelWait: () => void;
  /** Starts resizing the focused player area (its R key). */
  resize: () => void;
  /** Switches the corner the arrows move in resize mode (its X key). */
  otherCorner: () => void;
}

/**
 * The lobby's hints, by the focused element's `data-hints`. Start joins (the host's press, never the UI) and A
 * selects; on a player area that can be resized X resizes, and in resize mode A is done. While a place waits for a
 * controller the pads only cancel: B is the one hint, whatever has focus.
 */
export function lobbyHints(waiting: boolean, actions: LobbyHintActions): { base: LobbyHint[]; byFocus?: Record<string, LobbyHint[]> } {
  if (waiting) return { base: [{ button: 'B', label: 'Cancel', action: actions.cancelWait }] };
  const join: LobbyHint = { button: 'Menu', label: 'Join' };
  const select: LobbyHint = { button: 'A', label: 'Select' };
  return {
    base: [join, select],
    byFocus: {
      [PLAYER_RESIZABLE]: [join, select, { button: 'X', label: 'Resize', action: actions.resize }],
      [PLAYER_RESIZING]: [
        { key: 'Arrows', label: 'Move the corner' },
        { button: 'X', label: 'Other corner', action: actions.otherCorner },
        { button: 'A', label: 'Done' },
      ],
    },
  };
}
