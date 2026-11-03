// Pure rules of the news ticker (NewsTicker.tsx): where it shows, what the dismiss button hides, the order items take
// turns in, and when a message scrolls. No DOM and no imports beyond types, so src/bridge/__tests__/news.test.ts runs
// them under `node --test`.
import type { NewsItem } from '../hub/news.ts';

/** localStorage key of the dismissed items (`id:updatedAt`), newest last. */
export const DISMISSED_KEY = 'nucleus.news.dismissed';
/** Dismissed keys kept; older ones are forgotten (the API sends 10 items at most). */
export const MAX_DISMISSED = 50;
/** A message that fits stays this long before the next one. */
export const DWELL_MS = 8000;
/** In reduced motion every message is static and stays this long. */
export const REDUCED_DWELL_MS = 10_000;
/** Scrolling speed, close to the legacy feed (1 px per 16 ms tick). */
export const SCROLL_PX_PER_S = 55;
/** A scrolling message first stays still this long, so it can be read from its start. */
export const SCROLL_HOLD_MS = 1200;
/** Space between a scrolling message and its copy. */
export const SCROLL_GAP_PX = 48;

/** The part of `Storage` the ticker uses (tests pass a Map-backed fake). */
export interface KeyValueStore {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
}

/** What makes an item new again: the same id with another `updatedAt` shows again after a dismiss. */
export const newsKey = (item: Pick<NewsItem, 'id' | 'updatedAt'>) => `${item.id}:${item.updatedAt}`;

/** Dismissed keys saved by an earlier run, or none (missing, corrupt or blocked storage). */
export function readDismissed(store: KeyValueStore | null | undefined): string[] {
  try {
    const raw = store?.getItem(DISMISSED_KEY);
    const parsed: unknown = raw ? JSON.parse(raw) : [];
    return Array.isArray(parsed) ? parsed.filter((k): k is string => typeof k === 'string').slice(-MAX_DISMISSED) : [];
  } catch {
    return [];
  }
}

export function writeDismissed(store: KeyValueStore | null | undefined, keys: readonly string[]): void {
  try {
    store?.setItem(DISMISSED_KEY, JSON.stringify(keys.slice(-MAX_DISMISSED)));
  } catch {
    // Storage full or disabled: the dismiss holds until the app restarts.
  }
}

/** The dismiss button hides every item it was showing, until one of them changes or a new one comes. */
export function dismissNews(dismissed: readonly string[], items: readonly NewsItem[]): string[] {
  const added = items.map(newsKey).filter((k) => !dismissed.includes(k));
  return [...dismissed, ...added].slice(-MAX_DISMISSED);
}

/** Items not dismissed, in the API's order. */
export function pendingNews(items: readonly NewsItem[], dismissed: readonly string[]): NewsItem[] {
  const hidden = new Set(dismissed);
  return items.filter((n) => !hidden.has(newsKey(n)));
}

/**
 * Whether the title bar has room for the ticker: not while a game session is live, and not on the lobby and session
 * screens, where the title bar shows the players' devices and the couch is busy.
 */
export function tickerAllowed(pathname: string, sessionState: string | undefined): boolean {
  if (sessionState === 'launching' || sessionState === 'running' || sessionState === 'ending') return false;
  const path = pathname.replace(/\/+$/, '');
  return path !== '/session' && !/^\/library\/[^/]+\/setup$/.test(path);
}

/** The item after `currentId` (back to the first after the last; the first when `currentId` is gone). */
export function nextNewsId(items: readonly NewsItem[], currentId: string | undefined): string | undefined {
  if (!items.length) return undefined;
  const at = items.findIndex((n) => n.id === currentId);
  return items[at < 0 ? 0 : (at + 1) % items.length]!.id;
}

/** The item on show: `currentId` when it is still there, else the first. */
export function currentNews(items: readonly NewsItem[], currentId: string | undefined): NewsItem | undefined {
  return items.find((n) => n.id === currentId) ?? items[0];
}

export type MarqueePlan = { scroll: false } | { scroll: true; distance: number; durationMs: number };

/**
 * How a message shows in a box: still when it fits (or in reduced motion), else scrolling left by its width plus the
 * gap, so its copy lands where it started and the loop is seamless.
 */
export function marqueePlan(textWidth: number, viewWidth: number, reducedMotion: boolean): MarqueePlan {
  if (reducedMotion || !(textWidth > viewWidth + 1) || viewWidth <= 0) return { scroll: false };
  const distance = Math.ceil(textWidth + SCROLL_GAP_PX);
  return { scroll: true, distance, durationMs: Math.round((distance / SCROLL_PX_PER_S) * 1000) };
}

/** How long a still message stays before the next one; null when it is the only one (it simply stays). */
export function dwellMs(count: number, reducedMotion: boolean): number | null {
  return count > 1 ? (reducedMotion ? REDUCED_DWELL_MS : DWELL_MS) : null;
}

/**
 * Items a screen reader has not been told about yet: the live region speaks when news arrive or change, never again
 * for the same item (not on each scroll loop, not when the ticker comes back after the lobby).
 */
export function freshNews(items: readonly NewsItem[], announced: ReadonlySet<string>): NewsItem[] {
  return items.filter((n) => !announced.has(newsKey(n)));
}
