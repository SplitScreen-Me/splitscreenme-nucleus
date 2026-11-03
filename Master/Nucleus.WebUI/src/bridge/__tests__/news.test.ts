// The site news and the title bar ticker: the answer of `GET /api/v3/news` (src/hub/news.ts), the polling rules run
// by TanStack Query with a stubbed fetch (paused during a game session), and the ticker's rules (src/shell/news-core.ts):
// where it shows, the order, the dismiss, the scrolling and what screen readers hear. Lives with the bridge tests so
// `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { afterEach, mock, test } from 'node:test';
import { mockNewsAnswer, mockNewsItems } from '../mock/news.ts';
import {
  cleanMessage,
  fetchNews,
  liveNews,
  MAX_NEWS,
  NEWS_POLL_MS,
  newsQueryOptions,
  nextNewsChange,
  normalizeNews,
  type NewsItem,
} from '../../hub/news.ts';
import {
  currentNews,
  DISMISSED_KEY,
  dismissNews,
  dwellMs,
  DWELL_MS,
  freshNews,
  marqueePlan,
  MAX_DISMISSED,
  newsKey,
  nextNewsId,
  pendingNews,
  readDismissed,
  REDUCED_DWELL_MS,
  SCROLL_GAP_PX,
  SCROLL_PX_PER_S,
  tickerAllowed,
  writeDismissed,
  type KeyValueStore,
} from '../../shell/news-core.ts';

// TanStack Query runs no interval on a server (no `window`): give it one before it loads. Each test file runs in its
// own process, so this stays here.
(globalThis as { window?: unknown }).window ??= globalThis;
const { QueryClient, QueryObserver } = await import('@tanstack/react-query');

const SITE = 'https://hub.test';
const NOW = Date.parse('2026-11-03T12:00:00Z');
const realFetch = globalThis.fetch;

afterEach(() => {
  globalThis.fetch = realFetch;
  mock.timers.reset();
});

function item(id: string, extra: Partial<NewsItem> = {}): NewsItem {
  return { id, message: `Message ${id}`, url: null, severity: 'info', startsAt: null, endsAt: null, updatedAt: '2026-11-01T00:00:00Z', ...extra };
}

function memoryStore(initial: Record<string, string> = {}): KeyValueStore & { data: Map<string, string> } {
  const data = new Map(Object.entries(initial));
  return { data, getItem: (k) => data.get(k) ?? null, setItem: (k, v) => void data.set(k, v) };
}

// ---------- The API answer ----------

test('an answer is checked item by item, in its order', () => {
  const items = normalizeNews({
    items: [
      { id: 'a', message: 'Hello', url: 'https://example.com/a', severity: 'warning', startsAt: '2026-11-01T00:00:00Z', endsAt: null, updatedAt: '2026-11-01T10:00:00Z' },
      { id: 'b', message: '  Two\nlines\t here  ', url: 'javascript:alert(1)', severity: 'loud', updatedAt: 'x' },
      { id: '', message: 'no id' },
      { id: 'c', message: '   ' },
      { id: 'a', message: 'duplicate id' },
      'junk',
      { id: 'd', message: 'Critical', severity: 'critical', url: 'not a url', startsAt: 'never', endsAt: '2026-12-01T00:00:00Z' },
    ],
  });
  assert.deepEqual(
    items.map((n) => [n.id, n.message, n.url, n.severity]),
    [
      ['a', 'Hello', 'https://example.com/a', 'warning'],
      ['b', 'Two lines here', null, 'info'],
      ['d', 'Critical', null, 'critical'],
    ],
  );
  assert.equal(items[0]!.startsAt, Date.parse('2026-11-01T00:00:00Z'));
  assert.equal(items[0]!.updatedAt, '2026-11-01T10:00:00Z');
  assert.equal(items[2]!.startsAt, null);
  assert.equal(items[2]!.endsAt, Date.parse('2026-12-01T00:00:00Z'));
  assert.deepEqual(normalizeNews(null), []);
  assert.deepEqual(normalizeNews({ items: 'nope' }), []);
  assert.deepEqual(normalizeNews([{ id: 'a', message: 'array body' }]), []);
});

test('at most ten items, messages on one line and not endless', () => {
  const many = { items: Array.from({ length: 14 }, (_, i) => ({ id: `n${i}`, message: `News ${i}` })) };
  assert.equal(normalizeNews(many).length, MAX_NEWS);
  assert.equal(cleanMessage('a\r\nb' + String.fromCharCode(7) + 'c'), 'a b c');
  const long = cleanMessage('x'.repeat(900));
  assert.equal(long.length, 500);
  assert.ok(long.endsWith('…'));
  assert.equal(cleanMessage(42), '');
});

test('items outside their window are dropped, and the next change is known', () => {
  const items = [
    item('live'),
    item('ended', { endsAt: NOW - 1 }),
    item('later', { startsAt: NOW + 60_000 }),
    item('ending', { startsAt: NOW - 1000, endsAt: NOW + 30_000 }),
  ];
  assert.deepEqual(liveNews(items, NOW).map((n) => n.id), ['live', 'ending']);
  assert.equal(nextNewsChange(items, NOW), NOW + 30_000);
  assert.equal(nextNewsChange([item('a')], NOW), null);
});

test('the news are read from <site>/api/v3/news, without cookies, revalidating with the ETag', async () => {
  const seen: Array<{ url: string; init?: RequestInit }> = [];
  globalThis.fetch = (async (input: RequestInfo | URL, init?: RequestInit) => {
    seen.push({ url: String(input), init });
    return new Response(JSON.stringify({ items: [{ id: 'a', message: 'Hi', severity: 'info', updatedAt: 'u' }] }), {
      headers: { 'Content-Type': 'application/json', ETag: '"1"' },
    });
  }) as typeof fetch;
  const items = await fetchNews(SITE);
  assert.deepEqual(items.map((n) => n.message), ['Hi']);
  assert.equal(seen[0]!.url, 'https://hub.test/api/v3/news');
  assert.equal(seen[0]!.init?.credentials, 'omit');
  assert.equal(seen[0]!.init?.cache, 'no-cache');
  assert.ok(seen[0]!.init?.signal, 'every request has a timeout');
});

test('a failed request rejects (the ticker keeps what it had, silently)', async () => {
  globalThis.fetch = (async () => new Response('down', { status: 503 })) as typeof fetch;
  await assert.rejects(fetchNews(SITE), /HTTP 503/);
  globalThis.fetch = (async () => {
    throw new TypeError('Failed to fetch');
  }) as typeof fetch;
  await assert.rejects(fetchNews(SITE), TypeError);
  globalThis.fetch = (async () => new Response('<html>', { status: 200 })) as typeof fetch;
  await assert.rejects(fetchNews(SITE));
});

// ---------- Polling (TanStack Query, real QueryObserver) ----------

async function flush() {
  for (let i = 0; i < 5; i++) await new Promise<void>((resolve) => queueMicrotask(resolve));
}

test('news are polled every 10 minutes, not during a game session, and fetched again when it ends', async () => {
  mock.timers.enable({ apis: ['setTimeout', 'setInterval', 'Date'] });
  let calls = 0;
  const fetcher = async () => {
    calls++;
    return [item(`n${calls}`)];
  };
  const client = new QueryClient();
  const observer = new QueryObserver(client, newsQueryOptions(false, fetcher));
  const stop = observer.subscribe(() => {});
  await flush();
  assert.equal(calls, 1, 'fetched when the ticker mounts');

  mock.timers.tick(NEWS_POLL_MS - 1);
  await flush();
  assert.equal(calls, 1);
  mock.timers.tick(1);
  await flush();
  assert.equal(calls, 2, 'polled after 10 minutes');

  // A session starts: no polling at all.
  observer.setOptions(newsQueryOptions(true, fetcher));
  mock.timers.tick(NEWS_POLL_MS * 3);
  await flush();
  assert.equal(calls, 2, 'paused while a session is live');
  assert.deepEqual(observer.getCurrentResult().data?.map((n) => n.id), ['n2'], 'the last news stay');

  // The session ends: the copy is stale, so enabling the query fetches at once (useHubNews also refetches explicitly).
  observer.setOptions(newsQueryOptions(false, fetcher));
  await flush();
  assert.equal(calls, 3, 'fetched again when the session ends');
  mock.timers.tick(NEWS_POLL_MS);
  await flush();
  assert.equal(calls, 4, 'and polled again');

  stop();
  client.clear();
});

test('a failed poll keeps the last news, shows no error to retry, and waits for the next poll', async () => {
  mock.timers.enable({ apis: ['setTimeout', 'setInterval', 'Date'] });
  let calls = 0;
  const fetcher = async () => {
    calls++;
    if (calls === 2) throw new Error('offline');
    return [item('kept')];
  };
  const client = new QueryClient();
  const observer = new QueryObserver(client, newsQueryOptions(false, fetcher));
  const stop = observer.subscribe(() => {});
  await flush();
  mock.timers.tick(NEWS_POLL_MS);
  await flush();
  mock.timers.tick(30_000); // no retry in between
  await flush();
  assert.equal(calls, 2, 'no retries storm');
  const result = observer.getCurrentResult();
  assert.equal(result.isError, true);
  assert.deepEqual(result.data?.map((n) => n.id), ['kept']);
  mock.timers.tick(NEWS_POLL_MS);
  await flush();
  assert.equal(calls, 3);
  stop();
  client.clear();
});

test('the polling options', () => {
  const live = newsQueryOptions(false, async () => []);
  const paused = newsQueryOptions(true, async () => []);
  assert.deepEqual(live.queryKey, ['hub', 'news']);
  assert.equal(live.refetchInterval, 10 * 60_000);
  assert.equal(live.enabled, true);
  assert.equal(live.retry, false);
  assert.equal(live.refetchOnWindowFocus, true);
  assert.equal(live.refetchOnReconnect, true);
  assert.equal(live.networkMode, 'online');
  assert.equal(paused.enabled, false);
  assert.equal(paused.refetchInterval, false);
});

// ---------- Where the ticker shows ----------

test('the ticker stays out of the lobby, the session screen and live sessions', () => {
  assert.equal(tickerAllowed('/library', 'idle'), true);
  assert.equal(tickerAllowed('/games', undefined), true);
  assert.equal(tickerAllowed('/settings/supporters', 'setup'), true);
  assert.equal(tickerAllowed('/library/StardewValley/setup', 'setup'), false);
  assert.equal(tickerAllowed('/session', 'idle'), false);
  assert.equal(tickerAllowed('/session/', 'running'), false);
  for (const state of ['launching', 'running', 'ending']) assert.equal(tickerAllowed('/library', state), false, state);
});

// ---------- Dismiss ----------

test('dismiss hides the items on show until one changes or a new one comes', () => {
  const a = item('a');
  const b = item('b');
  const dismissed = dismissNews([], [a, b]);
  assert.deepEqual(dismissed, [newsKey(a), newsKey(b)]);
  assert.deepEqual(pendingNews([a, b], dismissed), []);
  const c = item('c');
  const bUpdated = item('b', { updatedAt: '2026-11-03T08:00:00Z', message: 'Updated' });
  assert.deepEqual(pendingNews([a, bUpdated, c], dismissed).map((n) => n.id), ['b', 'c'], 'an updated item and a new one show');
  assert.deepEqual(dismissNews(dismissed, [a]), dismissed, 'no duplicates');
});

test('dismissed keys survive a restart in localStorage, bounded, and bad storage is ignored', () => {
  const store = memoryStore();
  writeDismissed(store, ['a:1', 'b:2']);
  assert.equal(store.data.get(DISMISSED_KEY), '["a:1","b:2"]');
  assert.deepEqual(readDismissed(store), ['a:1', 'b:2']);

  const keys = Array.from({ length: MAX_DISMISSED + 20 }, (_, i) => `k${i}:u`);
  writeDismissed(store, keys);
  assert.deepEqual(readDismissed(store), keys.slice(-MAX_DISMISSED), 'the newest are kept');
  assert.equal(dismissNews(keys.slice(0, MAX_DISMISSED), [item('new')]).length, MAX_DISMISSED);

  assert.deepEqual(readDismissed(memoryStore({ [DISMISSED_KEY]: '{oops' })), []);
  assert.deepEqual(readDismissed(memoryStore({ [DISMISSED_KEY]: '{"a":1}' })), []);
  assert.deepEqual(readDismissed(memoryStore({ [DISMISSED_KEY]: '["ok", 3, null]' })), ['ok']);
  assert.deepEqual(readDismissed(null), []);
  const throwing: KeyValueStore = {
    getItem: () => {
      throw new Error('denied');
    },
    setItem: () => {
      throw new Error('full');
    },
  };
  assert.deepEqual(readDismissed(throwing), []);
  assert.doesNotThrow(() => writeDismissed(throwing, ['a']));
});

// ---------- Sequence and motion ----------

test('several items take turns, in order, and the one on show survives a refresh', () => {
  const items = [item('a'), item('b'), item('c')];
  assert.equal(nextNewsId(items, 'a'), 'b');
  assert.equal(nextNewsId(items, 'c'), 'a');
  assert.equal(nextNewsId(items, 'gone'), 'a');
  assert.equal(nextNewsId([item('only')], 'only'), 'only', 'a single item loops');
  assert.equal(nextNewsId([], 'a'), undefined);
  assert.equal(currentNews(items, 'b')?.id, 'b');
  assert.equal(currentNews(items, 'gone')?.id, 'a', 'an item that ended gives way to the first');
  assert.equal(currentNews(items, undefined)?.id, 'a');
  assert.equal(dwellMs(1, false), null, 'one still item just stays');
  assert.equal(dwellMs(3, false), DWELL_MS);
  assert.equal(dwellMs(3, true), REDUCED_DWELL_MS);
});

test('a message scrolls only when it does not fit, and never in reduced motion', () => {
  assert.deepEqual(marqueePlan(200, 300, false), { scroll: false });
  assert.deepEqual(marqueePlan(300.5, 300, false), { scroll: false }, 'a rounding difference is not overflow');
  const plan = marqueePlan(800, 300, false);
  assert.equal(plan.scroll, true);
  if (plan.scroll) {
    assert.equal(plan.distance, 800 + SCROLL_GAP_PX, 'the copy lands where the text started');
    assert.equal(plan.durationMs, Math.round(((800 + SCROLL_GAP_PX) / SCROLL_PX_PER_S) * 1000));
  }
  assert.deepEqual(marqueePlan(800, 300, true), { scroll: false });
  assert.deepEqual(marqueePlan(800, 0, false), { scroll: false }, 'not laid out yet');
});

test('screen readers hear each item once', () => {
  const announced = new Set<string>();
  const first = [item('a'), item('b')];
  assert.deepEqual(freshNews(first, announced).map((n) => n.id), ['a', 'b']);
  for (const n of first) announced.add(newsKey(n));
  assert.deepEqual(freshNews(first, announced), [], 'not again on the next loop or after the lobby');
  assert.deepEqual(freshNews([item('a', { updatedAt: 'changed' }), item('b')], announced).map((n) => n.id), ['a'], 'an update is news');
});

// ---------- The mock site ----------

test('the dev mock has one item per severity, and can be emptied or sent to the live site', () => {
  const items = normalizeNews(mockNewsItems(NOW));
  assert.deepEqual(items.map((n) => n.severity), ['info', 'warning', 'critical']);
  assert.equal(liveNews(items, NOW).length, 3);
  assert.ok(items.some((n) => n.url) && items.some((n) => !n.url), 'with and without "Learn more"');
  assert.equal(mockNewsAnswer('live', NOW), null);
  assert.deepEqual(normalizeNews(mockNewsAnswer('none', NOW)), []);
  assert.equal(normalizeNews(mockNewsAnswer('one', NOW)).length, 1);
  assert.deepEqual(normalizeNews(mockNewsAnswer('[{"id":"x","message":"Custom"}]', NOW)).map((n) => n.message), ['Custom']);
  assert.equal(normalizeNews(mockNewsAnswer(null, NOW)).length, 3);
});
