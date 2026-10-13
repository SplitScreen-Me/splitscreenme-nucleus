// Sorting and filtering of the game list (the UI's job), with the saved preferences of library.getPreferences.
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeQuery, type GameInputs, type GameSummary, type LibraryPreferences, type LibrarySort } from '@/bridge';
import type { MessageKey, TFunction } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { gameStatus } from '@/lib/library';

export const DEFAULT_PREFERENCES: LibraryPreferences = {
  sort: 'lastPlayed',
  favoritesOnly: false,
  filterGamepads: false,
  filterKeyboard: false,
  filterMultipleKeyboards: false,
};

export const SORT_OPTIONS: ReadonlyArray<{ value: LibrarySort; label: MessageKey }> = [
  { value: 'lastPlayed', label: 'library.sort.lastPlayed' },
  { value: 'name', label: 'library.sort.name' },
  { value: 'nameDescending', label: 'library.sort.nameDescending' },
  { value: 'maxPlayers', label: 'library.sort.maxPlayers' },
  { value: 'playtime', label: 'library.sort.playtime' },
];

export type InputFilterKey = 'filterGamepads' | 'filterKeyboard' | 'filterMultipleKeyboards';

export const INPUT_FILTERS: ReadonlyArray<{ key: InputFilterKey; label: MessageKey; icon: 'pad' | 'kbm' | 'multi'; test: (g: GameSummary) => boolean }> = [
  { key: 'filterGamepads', label: 'library.filters.controllers', icon: 'pad', test: (g) => g.inputs.xInput || g.inputs.dInput },
  { key: 'filterKeyboard', label: 'library.filters.keyboard', icon: 'kbm', test: (g) => g.inputs.keyboard },
  { key: 'filterMultipleKeyboards', label: 'library.filters.multipleKeyboards', icon: 'multi', test: (g) => g.inputs.multipleKeyboards },
];

/** "Controllers, keyboard and mouse" from the inputs a handler supports. */
export function inputsPhrase(t: TFunction, inputs: GameInputs): string {
  const pads = inputs.xInput || inputs.dInput;
  if (pads && inputs.multipleKeyboards) return t('library.inputs.controllersKeyboardsMice');
  if (pads && inputs.keyboard) return t('library.inputs.controllersKeyboardMouse');
  if (pads) return t('library.inputs.controllers');
  if (inputs.multipleKeyboards) return t('library.inputs.keyboardsMice');
  if (inputs.keyboard) return t('library.inputs.keyboardMouse');
  return t('common.inputs.unknown');
}

const byName = (a: GameSummary, b: GameSummary) => a.name.localeCompare(b.name, 'en', { sensitivity: 'base' });
const time = (iso: string | undefined) => (iso ? Date.parse(iso) || 0 : 0);

const COMPARE: Record<LibrarySort, (a: GameSummary, b: GameSummary) => number> = {
  name: byName,
  nameDescending: (a, b) => byName(b, a),
  maxPlayers: (a, b) => b.maxPlayers - a.maxPlayers || byName(a, b),
  playtime: (a, b) => b.playtimeSeconds - a.playtimeSeconds || byName(a, b),
  lastPlayed: (a, b) => time(b.lastPlayedAt) - time(a.lastPlayedAt) || byName(a, b),
};

/** Whether a game needs the user: executable missing or handler update available. */
export const needsAttention = (g: GameSummary) => {
  const status = gameStatus(g);
  return status === 'missing' || status === 'update';
};

/**
 * The visible games: favourites only when asked; input filters keep games that support at least one of the checked
 * input kinds (as the legacy list did); then sorted.
 */
export function visibleGames(games: readonly GameSummary[], prefs: LibraryPreferences, attentionOnly: boolean): GameSummary[] {
  const inputs = INPUT_FILTERS.filter((f) => prefs[f.key]);
  return games
    .filter((g) => !prefs.favoritesOnly || g.favorite)
    .filter((g) => inputs.length === 0 || inputs.some((f) => f.test(g)))
    .filter((g) => !attentionOnly || needsAttention(g))
    .sort(COMPARE[prefs.sort] ?? COMPARE.lastPlayed);
}

/** Saved sort and filters, changed optimistically (the cache first, then library.setPreferences). */
export function useLibraryPreferences() {
  const queryClient = useQueryClient();
  const run = useRun();
  const { data } = useBridgeQuery('library.getPreferences');
  const prefs = data ?? DEFAULT_PREFERENCES;
  const update = (patch: Partial<LibraryPreferences>) => {
    const next = { ...prefs, ...patch };
    const key = bridgeQueryKey('library.getPreferences');
    queryClient.setQueryData(key, next);
    void run(() => bridge.library.setPreferences(next), {
      onError: () => {
        void queryClient.invalidateQueries({ queryKey: key });
      },
    });
  };
  return { prefs, update };
}
