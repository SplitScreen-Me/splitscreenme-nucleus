// The lobby's gamepad hints, the pure part: a button is only tied to what it does whatever has focus.
// src/bridge/__tests__/lobby-hints.test.ts runs it under `node --test`: keep this file erasable TypeScript, importing the
// i18n store by its relative path (labels are read when the hints are built, in the language in use).
import { t } from '../../i18n/store.ts';

/** One hint of the bar: the shape of shell/chrome's Hint, restated so this file needs no app imports. */
export interface LobbyHint {
  button?: 'A' | 'B' | 'X' | 'Y' | 'Menu';
  key?: string;
  label: string;
  action?: () => void;
}

/** The `data-hints` of a player area: one that can be resized, and one in resize mode. */
export const PLAYER_RESIZABLE = 'player-resizable';
export const PLAYER_RESIZING = 'resizing';
/** A player area whose copy takes a guest controller, and one that can be resized too. */
const PLAYER_GUESTS = 'player-guests';
const PLAYER_RESIZABLE_GUESTS = 'player-resizable-guests';

/** The `data-hints` of a player area, by what it can do (none: the screen's base hints). */
export function playerHintsKey(area: { resizable: boolean; guestRoom: boolean; resizing?: boolean }): string | undefined {
  if (area.resizing) return PLAYER_RESIZING;
  if (area.guestRoom) return area.resizable ? PLAYER_RESIZABLE_GUESTS : PLAYER_GUESTS;
  return area.resizable ? PLAYER_RESIZABLE : undefined;
}

export interface LobbyHintActions {
  /** Stops waiting for a controller. */
  cancelWait: () => void;
  /** Starts resizing the focused player area (its R key). */
  resize: () => void;
  /** Switches the corner the arrows move in resize mode (its X key). */
  otherCorner: () => void;
  /** Makes the focused player's copy wait for a guest controller (its G key). */
  addGuest: () => void;
}

/**
 * The lobby's hints, by the focused element's `data-hints`. Start joins (the host's press, never the UI) and A
 * selects; on a player area that can be resized X resizes, and in resize mode A is done. On a player area whose copy
 * takes more players, Y waits for a guest controller. While a place waits for a
 * controller the pads only cancel: B is the one hint, whatever has focus.
 */
export function lobbyHints(waiting: boolean, actions: LobbyHintActions): { base: LobbyHint[]; byFocus?: Record<string, LobbyHint[]> } {
  if (waiting) return { base: [{ button: 'B', label: t('common.cancel'), action: actions.cancelWait }] };
  const join: LobbyHint = { button: 'Menu', label: t('lobby.hints.join') };
  const select: LobbyHint = { button: 'A', label: t('common.select') };
  const resize: LobbyHint = { button: 'X', label: t('lobby.hints.resize'), action: actions.resize };
  const guest: LobbyHint = { button: 'Y', label: t('lobby.hints.addController'), action: actions.addGuest };
  return {
    base: [join, select],
    byFocus: {
      [PLAYER_RESIZABLE]: [join, select, resize],
      [PLAYER_GUESTS]: [join, select, guest],
      [PLAYER_RESIZABLE_GUESTS]: [join, select, resize, guest],
      [PLAYER_RESIZING]: [
        { key: t('lobby.hints.arrowsKey'), label: t('lobby.hints.moveCorner') },
        { button: 'X', label: t('lobby.hints.otherCorner'), action: actions.otherCorner },
        { button: 'A', label: t('common.done') },
      ],
    },
  };
}
