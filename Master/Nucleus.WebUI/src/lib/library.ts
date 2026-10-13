// The user's library as the screens read it: live queries plus small derivations of `GameSummary`.
import { useBridgeQuery, type GameInputs, type GameSummary, type InstalledHandler } from '@/bridge';
import { t } from '@/i18n';

/** Every library game (refetched on `library.changed` / `handlers.changed`). */
export const useLibrary = () => useBridgeQuery('library.list');

/** Handlers installed on this PC (refetched on `handlers.changed` / `library.changed`). */
export const useInstalledHandlers = () => useBridgeQuery('handlers.listInstalled');

/**
 * What a library card says about a game, most urgent first:
 * `missing` (executable not found), `update` (newer handler on the hub), `new` (never launched), `ready`.
 */
export type GameStatus = 'missing' | 'update' | 'new' | 'ready';

export function gameStatus(game: GameSummary): GameStatus {
  if (!game.executableFound) return 'missing';
  if (game.updateAvailable) return 'update';
  if (game.firstLaunch) return 'new';
  return 'ready';
}

/** Portrait art of a game (downloaded cover), when there is one. */
export const coverOf = (game: Pick<GameSummary, 'coverUrl'>) => game.coverUrl;

/** Wide art of a game: the first screenshot, else the cover. */
export const heroOf = (game: Pick<GameSummary, 'heroUrl' | 'coverUrl'>) => game.heroUrl ?? game.coverUrl;

/** Route of a game's setup (lobby). */
export const setupPath = (gameId: string) => `/library/${encodeURIComponent(gameId)}/setup`;

/**
 * "Controllers, keyboard and mouse" from the inputs a handler supports, in the language in use (callers re-render on a
 * change of language through their own hooks).
 */
export function inputsPhrase(inputs: GameInputs): string {
  const pads = inputs.xInput || inputs.dInput;
  if (pads && inputs.multipleKeyboards) return t('common.inputs.summary.controllersMultiKeyboardMouse');
  if (pads && inputs.keyboard) return t('common.inputs.summary.controllersKeyboardMouse');
  if (pads) return t('common.inputs.summary.controllers');
  if (inputs.multipleKeyboards) return t('common.inputs.summary.multiKeyboardMouse');
  if (inputs.keyboard) return t('common.inputs.summary.keyboardMouse');
  return t('common.inputs.unknown');
}

/** The `InputSupport` flags of the design system, from the inputs a handler supports. */
export const inputFlags = (inputs: GameInputs) => ({
  controllers: inputs.xInput || inputs.dInput,
  keyboardMouse: inputs.keyboard,
  multiKeyboardMouse: inputs.multipleKeyboards,
});

/** Installed handler of a hub handler id. */
export const installedFor = (installed: readonly InstalledHandler[], handlerId: string | undefined) =>
  handlerId ? installed.find((h) => h.handlerId === handlerId) : undefined;

/** Lower-case letters and digits only, for matching game names across sources ("Halo: MCC" = "halo mcc"). */
export function normalizeName(name: string): string {
  return name
    .normalize('NFKD')
    .replace(/[̀-ͯ]/g, '')
    .toLowerCase()
    .replace(/&/g, 'and')
    .replace(/[®™©]/g, '')
    .replace(/[^a-z0-9]+/g, '');
}
