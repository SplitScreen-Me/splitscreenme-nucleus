// Catalogue filtering and sorting, ported from redesign-draft/style-1/web/games.html + catalog.js.
// Pure functions over the normalised handler list; the screen keeps the state in the URL.
import { t, type MessageKey } from '@/i18n';
import type { HubHandler } from './types';

export type PlayersFilter = '' | '2' | '3-4' | '5-8' | '9';
export type InputFilter = 'pad' | 'kbm' | 'multi';
export type SortKey = 'trending' | 'downloads' | 'updated' | 'az';

export interface CatalogState {
  q: string;
  players: PlayersFilter;
  inputs: InputFilter[];
  verified: boolean;
  genre: string;
  sort: SortKey;
}

export const EMPTY_FILTERS: Omit<CatalogState, 'sort'> = { q: '', players: '', inputs: [], verified: false, genre: '' };

/** Player ranges: `range` is digits ("3–4"), empty for any number (shown as `hub.players.any`). */
export const PLAYERS: ReadonlyArray<{ key: PlayersFilter; range: string; test: (n: number) => boolean }> = [
  { key: '', range: '', test: () => true },
  { key: '2', range: '2', test: (n) => n <= 2 },
  { key: '3-4', range: '3–4', test: (n) => n >= 3 && n <= 4 },
  { key: '5-8', range: '5–8', test: (n) => n >= 5 && n <= 8 },
  { key: '9', range: '9+', test: (n) => n >= 9 },
];

export const INPUTS: ReadonlyArray<{ key: InputFilter; icon: 'pad' | 'kbm' | 'multi'; label: MessageKey; test: (h: HubHandler) => boolean }> = [
  { key: 'pad', icon: 'pad', label: 'common.inputs.controllers', test: (h) => h.inputs.controllers === true },
  { key: 'kbm', icon: 'kbm', label: 'common.inputs.keyboardMouse', test: (h) => h.inputs.keyboardMouse === true },
  { key: 'multi', icon: 'multi', label: 'common.inputs.multiKeyboardMouse', test: (h) => h.inputs.multiKeyboardMouse === true },
];

export const SORTS: ReadonlyArray<{ key: SortKey; label: MessageKey; compare: (a: HubHandler, b: HubHandler) => number }> = [
  { key: 'trending', label: 'hub.sort.trending', compare: (a, b) => b.trendScore - a.trendScore || b.downloads - a.downloads },
  { key: 'downloads', label: 'hub.sort.downloads', compare: (a, b) => b.downloads - a.downloads },
  { key: 'updated', label: 'hub.sort.updated', compare: (a, b) => b.publishedAt.localeCompare(a.publishedAt) },
  { key: 'az', label: 'hub.sort.az', compare: (a, b) => a.gameName.localeCompare(b.gameName, 'en') },
];

/** Hub genres are IGDB names: translated (and the long ones shortened) for chips and lists; an unknown one shows as is. */
const GENRE_LABELS: Record<string, MessageKey> = {
  Adventure: 'hub.genres.adventure',
  Arcade: 'hub.genres.arcade',
  'Card & Board Game': 'hub.genres.cardAndBoardGame',
  Fighting: 'hub.genres.fighting',
  "Hack and slash/Beat 'em up": 'hub.genres.hackAndSlash',
  Indie: 'hub.genres.indie',
  MOBA: 'hub.genres.moba',
  Music: 'hub.genres.music',
  Pinball: 'hub.genres.pinball',
  Platform: 'hub.genres.platform',
  'Point-and-click': 'hub.genres.pointAndClick',
  Puzzle: 'hub.genres.puzzle',
  'Quiz/Trivia': 'hub.genres.quizTrivia',
  Racing: 'hub.genres.racing',
  'Real Time Strategy (RTS)': 'hub.genres.realTimeStrategy',
  'Role-playing (RPG)': 'hub.genres.rolePlaying',
  Shooter: 'hub.genres.shooter',
  Simulator: 'hub.genres.simulator',
  Sport: 'hub.genres.sport',
  Strategy: 'hub.genres.strategy',
  Tactical: 'hub.genres.tactical',
  'Turn-based strategy (TBS)': 'hub.genres.turnBasedStrategy',
  'Visual Novel': 'hub.genres.visualNovel',
};
/** Label of a hub genre in the language in use (components calling it re-render through their own `useI18n`). */
export const genreLabel = (g: string) => {
  const key = Object.hasOwn(GENRE_LABELS, g) ? GENRE_LABELS[g] : undefined;
  return key ? t(key) : g;
};

/** Lower case, accents stripped: "Pokémon" matches "pokemon". */
export const norm = (s: string) =>
  s
    .toLowerCase()
    .normalize('NFD')
    .replace(/[̀-ͯ]/g, '');

export interface IndexedHandler {
  h: HubHandler;
  text: string;
}

export const buildIndex = (handlers: HubHandler[]): IndexedHandler[] =>
  handlers.map((h) => ({ h, text: norm(`${h.gameName} ${h.title} ${h.author}`) }));

type Facet = 'players' | 'inputs' | 'verified' | 'genre';

export function matches(s: Omit<CatalogState, 'sort'>, { h, text }: IndexedHandler, skip?: Facet): boolean {
  if (s.q) {
    const words = norm(s.q).split(/\s+/).filter(Boolean);
    if (!words.every((w) => text.includes(w))) return false;
  }
  if (skip !== 'players' && s.players) {
    const rule = PLAYERS.find((p) => p.key === s.players);
    if (rule && !(h.maxPlayers && rule.test(h.maxPlayers))) return false;
  }
  if (skip !== 'inputs' && s.inputs.some((k) => !INPUTS.find((i) => i.key === k)?.test(h))) return false;
  if (skip !== 'verified' && s.verified && !h.verified) return false;
  if (skip !== 'genre' && s.genre && !h.genres.includes(s.genre)) return false;
  return true;
}

export function results(index: IndexedHandler[], s: CatalogState): HubHandler[] {
  const cmp = SORTS.find((x) => x.key === s.sort)?.compare ?? SORTS[0]!.compare;
  return index
    .filter((x) => matches(s, x))
    .map((x) => x.h)
    .sort(cmp);
}

/** How many games a facet option would give, every other filter kept. */
export function countWith(index: IndexedHandler[], s: CatalogState, patch: Partial<CatalogState>, skip?: Facet): number {
  const next = { ...s, ...patch };
  let n = 0;
  for (const x of index) if (matches(next, x, skip)) n++;
  return n;
}

/** Genres by number of handlers, most common first. */
export function genreStats(handlers: HubHandler[]): Array<{ genre: string; count: number }> {
  const counts = new Map<string, number>();
  for (const h of handlers) for (const g of h.genres) counts.set(g, (counts.get(g) ?? 0) + 1);
  return [...counts.entries()].map(([genre, count]) => ({ genre, count })).sort((a, b) => b.count - a.count);
}

// ---------------------------------------------------------------- URL state

export function readState(p: URLSearchParams, genres?: ReadonlySet<string>): CatalogState {
  const players = p.get('players') ?? '';
  const sort = p.get('sort') ?? '';
  const genre = p.get('genre') ?? '';
  return {
    q: p.get('q') ?? '',
    players: PLAYERS.some((x) => x.key === players) ? (players as PlayersFilter) : '',
    inputs: (p.get('input') ?? '').split(',').filter((k): k is InputFilter => INPUTS.some((i) => i.key === k)),
    verified: p.get('verified') === '1',
    genre: !genres || genres.has(genre) ? genre : '',
    sort: SORTS.some((x) => x.key === sort) ? (sort as SortKey) : 'trending',
  };
}

export function writeState(s: CatalogState): URLSearchParams {
  const p = new URLSearchParams();
  if (s.q) p.set('q', s.q);
  if (s.players) p.set('players', s.players);
  if (s.inputs.length) p.set('input', s.inputs.join(','));
  if (s.verified) p.set('verified', '1');
  if (s.genre) p.set('genre', s.genre);
  if (s.sort !== 'trending') p.set('sort', s.sort);
  return p;
}

// ---------------------------------------------------------------- presentation helpers

const STOP = new Set(['game', 'edition', 'pc', 'the']);

/**
 * A handler title often repeats the game name plus a variant ("Zombies", "Seamless Co-op").
 * Return only that variant, and only when it carries meaning.
 */
export function variant(h: Pick<HubHandler, 'title' | 'gameName'>): string {
  if (!h.title || norm(h.title) === norm(h.gameName)) return '';
  if (!norm(h.title).startsWith(norm(h.gameName))) return '';
  const rest = h.title.slice(h.gameName.length).replace(/^[\s:–-]+/, '').trim();
  const words = rest.split(/\s+/).filter((w) => /[a-z]{3,}/i.test(w) && !STOP.has(w.toLowerCase()));
  return words.length ? rest : '';
}
