// Sorting and filtering of the game list (the UI's job), with the saved preferences of library.getPreferences.
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeQuery, type GameSummary, type LibraryPreferences, type LibrarySort } from '@/bridge';
import { useRun } from '@/lib/bridge-ui';
import { gameStatus } from '@/lib/library';

export const DEFAULT_PREFERENCES: LibraryPreferences = {
  sort: 'lastPlayed',
  favoritesOnly: false,
  filterGamepads: false,
  filterKeyboard: false,
  filterMultipleKeyboards: false,
};

export const SORT_OPTIONS: ReadonlyArray<{ value: LibrarySort; label: string }> = [
  { value: 'lastPlayed', label: 'Last played' },
  { value: 'name', label: 'Name, A to Z' },
  { value: 'nameDescending', label: 'Name, Z to A' },
  { value: 'maxPlayers', label: 'Most players' },
  { value: 'playtime', label: 'Most played' },
];

export type InputFilterKey = 'filterGamepads' | 'filterKeyboard' | 'filterMultipleKeyboards';

export const INPUT_FILTERS: ReadonlyArray<{ key: InputFilterKey; label: string; icon: 'pad' | 'kbm' | 'multi'; test: (g: GameSummary) => boolean }> = [
  { key: 'filterGamepads', label: 'Controllers', icon: 'pad', test: (g) => g.inputs.xInput || g.inputs.dInput },
  { key: 'filterKeyboard', label: 'Keyboard and mouse', icon: 'kbm', test: (g) => g.inputs.keyboard },
  { key: 'filterMultipleKeyboards', label: 'Several keyboards', icon: 'multi', test: (g) => g.inputs.multipleKeyboards },
];

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
