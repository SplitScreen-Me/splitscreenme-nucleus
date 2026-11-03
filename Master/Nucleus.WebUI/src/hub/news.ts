// News of the SplitScreen.Me site (`GET <hubUrl>/api/v3/news`), shown by the title bar ticker (src/shell/NewsTicker.tsx).
// The API sends the live items only, sorted, at most 10, with CORS `*` and an ETag. This file validates the answer and
// holds the polling rules; it imports nothing that needs the browser, so the node tests load it with a stubbed fetch.
import { hubApiUrl } from './urls.ts';

export type NewsSeverity = 'info' | 'warning' | 'critical';

export const newsSeverityValues: readonly NewsSeverity[] = ['info', 'warning', 'critical'];

/** One news item, checked and cleaned. */
export interface NewsItem {
  id: string;
  /** One line of plain text (line breaks and runs of spaces collapsed). */
  message: string;
  /** An http(s) page about it ("Learn more"), or null. */
  url: string | null;
  severity: NewsSeverity;
  /** Epoch ms; null when the API gave none or an unreadable one. */
  startsAt: number | null;
  endsAt: number | null;
  /** ISO string as sent: part of the dismissed key (a changed item shows again). */
  updatedAt: string;
}

/** Raw shape of `GET /api/v3/news`. */
export interface NewsAnswerRaw {
  items?: unknown;
}

/** Poll every 10 minutes, as the legacy news feed did. */
export const NEWS_POLL_MS = 10 * 60_000;
/** A focus or a reconnection refetches when the copy is older than this. */
export const NEWS_STALE_MS = 60_000;
/** A request that takes longer is dropped (the next poll tries again). */
export const NEWS_TIMEOUT_MS = 15_000;
export const MAX_NEWS = 10;
const MAX_MESSAGE_LENGTH = 500;

const isRecord = (v: unknown): v is Record<string, unknown> => typeof v === 'object' && v !== null && !Array.isArray(v);

function time(value: unknown): number | null {
  if (typeof value !== 'string' || !value) return null;
  const ms = Date.parse(value);
  return Number.isFinite(ms) ? ms : null;
}

function webUrl(value: unknown): string | null {
  if (typeof value !== 'string' || !value.trim()) return null;
  try {
    const url = new URL(value.trim());
    return url.protocol === 'https:' || url.protocol === 'http:' ? url.href : null;
  } catch {
    return null;
  }
}

/** A message as one line: control characters and line breaks become spaces, long ones are cut. */
export function cleanMessage(value: unknown): string {
  if (typeof value !== 'string') return '';
  const line = value.replace(/[\p{Cc}\s]+/gu, ' ').trim();
  return line.length > MAX_MESSAGE_LENGTH ? `${line.slice(0, MAX_MESSAGE_LENGTH - 1).trimEnd()}…` : line;
}

/** The usable items of an answer, in its order: an id and a message are required, the rest falls back. */
export function normalizeNews(body: unknown): NewsItem[] {
  const raw = isRecord(body) && Array.isArray(body.items) ? body.items : [];
  const seen = new Set<string>();
  const items: NewsItem[] = [];
  for (const entry of raw) {
    if (!isRecord(entry) || typeof entry.id !== 'string' || !entry.id || seen.has(entry.id)) continue;
    const message = cleanMessage(entry.message);
    if (!message) continue;
    seen.add(entry.id);
    items.push({
      id: entry.id,
      message,
      url: webUrl(entry.url),
      severity: newsSeverityValues.includes(entry.severity as NewsSeverity) ? (entry.severity as NewsSeverity) : 'info',
      startsAt: time(entry.startsAt),
      endsAt: time(entry.endsAt),
      updatedAt: typeof entry.updatedAt === 'string' ? entry.updatedAt : '',
    });
    if (items.length === MAX_NEWS) break;
  }
  return items;
}

/** Items live at `now`: started (or no start) and not ended. The API sends live items; this drops those that end later. */
export function liveNews(items: readonly NewsItem[], now: number): NewsItem[] {
  return items.filter((n) => (n.startsAt === null || n.startsAt <= now) && (n.endsAt === null || n.endsAt > now));
}

/** When the set of live items changes next (an item starts or ends), or null. */
export function nextNewsChange(items: readonly NewsItem[], now: number): number | null {
  let next: number | null = null;
  for (const n of items) {
    for (const at of [n.startsAt, n.endsAt]) if (at !== null && at > now && (next === null || at < next)) next = at;
  }
  return next;
}

/**
 * The news of a site. Revalidates with the ETag on every call (`no-cache`: a 304 when nothing changed). Any failure
 * rejects; the ticker then stays as it was, without an error on screen.
 */
export async function fetchNews(site: string, signal?: AbortSignal): Promise<NewsItem[]> {
  const timeout = AbortSignal.timeout(NEWS_TIMEOUT_MS);
  const res = await fetch(hubApiUrl(site, '/news'), {
    signal: signal ? AbortSignal.any([signal, timeout]) : timeout,
    credentials: 'omit',
    cache: 'no-cache',
    headers: { Accept: 'application/json' },
  });
  if (!res.ok) throw new Error(`news: HTTP ${res.status}`);
  return normalizeNews(await res.json());
}

/**
 * TanStack Query options of the news (src/hub/queries.ts `useHubNews`): polled every 10 minutes, paused while a game
 * session is live (`paused`), refetched on focus and reconnection when older than a minute, no retries (the next poll
 * is the retry), never an error on screen. Pure, so the tests drive it with a QueryObserver.
 */
export function newsQueryOptions(paused: boolean, fetcher: (signal: AbortSignal) => Promise<NewsItem[]>) {
  return {
    queryKey: ['hub', 'news'] as const,
    queryFn: ({ signal }: { signal: AbortSignal }) => fetcher(signal),
    enabled: !paused,
    networkMode: 'online' as const,
    refetchInterval: paused ? (false as const) : NEWS_POLL_MS,
    refetchIntervalInBackground: false,
    refetchOnWindowFocus: true,
    refetchOnReconnect: true,
    staleTime: NEWS_STALE_MS,
    retry: false,
    // Keep the last good answer through a failed poll; an unreachable site never shows an error.
    throwOnError: false,
  };
}
