// Hub API v3 client (src/hub): site URLs, the CSP check, mapping of captured answers (fixtures/, trimmed and
// anonymised) and the fetch calls, with a stubbed fetch. Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import { afterEach, test } from 'node:test';
import { fetchCatalogue, fetchComments, fetchHandler, HubError } from '../../hub/api.ts';
import { normalizeHandler, threadComments } from '../../hub/normalize.ts';
import type { HubCatalogRaw, HubHandlerDetailRaw } from '../../hub/types.ts';
import { connectAllowed, cspConnectSources, DEFAULT_HUB_URL, hubApiUrl, hubCatalogueUrl, normalizeHubUrl } from '../../hub/urls.ts';

const fixture = <T>(name: string): T => JSON.parse(readFileSync(new URL(`./fixtures/${name}`, import.meta.url), 'utf8')) as T;

const SITE = 'https://hub.test';
const realFetch = globalThis.fetch;
const requests: Array<{ url: string; credentials?: RequestCredentials }> = [];

function stubFetch(answer: (url: string) => Response | Promise<Response>) {
  requests.length = 0;
  globalThis.fetch = (async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = String(input);
    requests.push({ url, credentials: init?.credentials });
    return answer(url);
  }) as typeof fetch;
}

const json = (body: unknown, status = 200, headers: Record<string, string> = {}) =>
  new Response(JSON.stringify(body), { status, headers: { 'Content-Type': 'application/json', ...headers } });

afterEach(() => {
  globalThis.fetch = realFetch;
});

test('hub site values are normalised like HubSite.Normalize (C#)', () => {
  assert.equal(DEFAULT_HUB_URL, 'https://v3.splitscreen.me');
  assert.equal(normalizeHubUrl('https://v3.splitscreen.me/'), 'https://v3.splitscreen.me');
  assert.equal(normalizeHubUrl('  http://localhost:3000/api/v3/  '), 'http://localhost:3000');
  assert.equal(normalizeHubUrl('HTTPS://Example.COM:443/site/?x=1#y'), 'https://example.com/site');
  assert.equal(normalizeHubUrl('https://example.com/site/api/v3'), 'https://example.com/site');
  for (const bad of ['', '   ', undefined, null, 'v3.splitscreen.me', 'ftp://example.com', 'https://user:secret@example.com']) {
    assert.equal(normalizeHubUrl(bad), undefined, String(bad));
  }
  assert.equal(hubApiUrl(DEFAULT_HUB_URL, '/catalog'), 'https://v3.splitscreen.me/api/v3/catalog');
  assert.equal(hubCatalogueUrl(DEFAULT_HUB_URL), 'https://v3.splitscreen.me/hub');
});

test('the packaged CSP allows the default hub and nothing else unless the build adds it', () => {
  const policy = "default-src 'self'; img-src 'self' data: blob: https:; connect-src 'self' https://v3.splitscreen.me; object-src 'none'";
  const sources = cspConnectSources(policy);
  assert.deepEqual(sources, ["'self'", 'https://v3.splitscreen.me']);
  assert.equal(connectAllowed(DEFAULT_HUB_URL, sources), true);
  assert.equal(connectAllowed('https://v3.splitscreen.me/sub', sources), true);
  assert.equal(connectAllowed('http://localhost:3000', sources), false);
  assert.equal(connectAllowed('https://hub.splitscreen.me', sources), false);
  assert.equal(connectAllowed('http://localhost:3000', cspConnectSources(`${policy.replace('https://v3.splitscreen.me', 'https://v3.splitscreen.me http://localhost:3000')}`)), true);
  assert.equal(connectAllowed('http://localhost:3000', ['http://localhost:*']), true);
  assert.equal(connectAllowed('http://localhost:3000', ['https:']), false);
  assert.equal(connectAllowed('https://any.example', ['https:']), true);
  assert.equal(cspConnectSources(undefined), undefined);
  assert.equal(connectAllowed('http://localhost:3000', undefined), true); // dev server: no CSP
  assert.deepEqual(cspConnectSources("default-src 'self' https://a.example"), ["'self'", 'https://a.example']);
});

test('a handler detail maps to the UI shape', () => {
  const h = normalizeHandler(fixture<HubHandlerDetailRaw>('hub-handler.json'));
  assert.equal(h.id, 'fDmTaAoYKJmiWi3s3');
  assert.equal(h.title, 'Palworld');
  assert.equal(h.url, 'https://v3.splitscreen.me/hub/palworld');
  assert.equal(h.gameSlug, 'palworld');
  assert.equal(h.authorId, 'AuthorAAAAAAAAAA1');
  assert.equal(h.author, 'ExampleAuthor');
  assert.equal(h.authorUrl, 'https://v3.splitscreen.me/hub/authors/ExampleAuthor');
  assert.match(h.description, /STEAM VERSION/);
  assert.match(h.gameDescription, /survival crafting/);
  assert.equal(h.coverImageId, 'co7n02');
  assert.equal(h.igdbGameId, 151665);
  assert.deepEqual(h.genres, ['Shooter', 'Role-playing (RPG)', "Hack and slash/Beat 'em up"]);
  assert.equal(h.version, 6);
  assert.equal(h.downloads, 87215);
  assert.equal(h.comments, 2);
  assert.equal(h.maxPlayers, 4);
  assert.deepEqual(h.inputs, { controllers: true, keyboardMouse: true, multiKeyboardMouse: true });
  assert.equal(h.publishedAt, '2024-02-01T01:10:51.794Z');
  assert.equal(h.trendScore, 372);
});

test('catalogue items without author, cover or input answers map to undefined', () => {
  const [, h] = fixture<HubCatalogRaw>('hub-catalog.json').items!.map(normalizeHandler);
  assert.ok(h);
  assert.equal(h.authorId, undefined);
  assert.equal(h.author, 'Deleted user');
  assert.equal(h.authorUrl, undefined);
  assert.equal(h.coverImageId, undefined);
  assert.equal(h.igdbGameId, undefined);
  assert.equal(h.maxPlayers, undefined);
  assert.equal(h.description, ''); // the catalogue has no notes
  assert.deepEqual(h.inputs, { controllers: undefined, keyboardMouse: false, multiKeyboardMouse: undefined });
});

test('fetchCatalogue reads /catalog without cookies', async () => {
  stubFetch(() => json({ ...fixture<HubCatalogRaw>('hub-catalog.json'), items: [...fixture<HubCatalogRaw>('hub-catalog.json').items!, { title: 'no id' }] }));
  const handlers = await fetchCatalogue(SITE);
  assert.deepEqual(requests, [{ url: 'https://hub.test/api/v3/catalog', credentials: 'omit' }]);
  assert.deepEqual(handlers.map((h) => h.id), ['ye9NKnb7eBFREecZc', 'Zz9Example0000001']);
});

test('fetchHandler: the detail, or null for a handler that is not listed', async () => {
  stubFetch((url) => (url.endsWith('/fDmTaAoYKJmiWi3s3') ? json(fixture('hub-handler.json')) : json({ error: { code: 'not_found', message: 'Not found' } }, 404)));
  assert.equal((await fetchHandler(SITE, 'fDmTaAoYKJmiWi3s3'))?.gameName, 'Palworld');
  assert.equal(await fetchHandler(SITE, 'a/b'), null);
  assert.equal(requests[1]?.url, 'https://hub.test/api/v3/handlers/a%2Fb');
});

test('fetchComments follows nextCursor and threads replies under their root', async () => {
  const page = fixture<{ items: unknown[]; nextCursor: string | null }>('hub-comments.json');
  stubFetch((url) =>
    new URL(url).searchParams.has('before')
      ? json({ items: [{ id: 'old1', handlerId: 'h', parentId: null, authorId: null, authorName: 'Someone', body: 'First!', createdAt: '2023-01-01T00:00:00.000Z', editedAt: null }], nextCursor: null })
      : json({ ...page, nextCursor: '2024-02-13T23:05:46.905Z' }),
  );
  const comments = await fetchComments(SITE, 'fDmTaAoYKJmiWi3s3');
  assert.deepEqual(
    requests.map((r) => r.url),
    [
      'https://hub.test/api/v3/handlers/fDmTaAoYKJmiWi3s3/comments?limit=200',
      'https://hub.test/api/v3/handlers/fDmTaAoYKJmiWi3s3/comments?limit=200&before=2024-02-13T23%3A05%3A46.905Z',
    ],
  );
  assert.equal(comments.length, 3);
  const [reply] = comments;
  assert.equal(reply?.content, 'The store version has different files.\nThis handler is for the Steam version.');
  assert.equal(reply?.replyTo, 'xkCG2Yg7N3PAdWML4');
  const threads = threadComments(comments);
  assert.deepEqual(threads.map((t) => [t.id, t.replies.map((r) => r.id)]), [
    ['xkCG2Yg7N3PAdWML4', ['tm9TwqHLxzvt2Ajo9']],
    ['old1', []],
  ]);
  assert.equal(threads[0]?.authorId, undefined);
  assert.equal(threads[0]?.editedAt, '2024-02-14T08:00:00.000Z');
});

test('failures become HubErrors with a readable message', async () => {
  stubFetch(() => json({ error: { code: 'rate_limited' } }, 429, { 'Retry-After': '42' }));
  await assert.rejects(fetchCatalogue(SITE), (e: unknown) => e instanceof HubError && e.status === 429 && /in 42 s/.test(e.message));

  stubFetch(() => json({ error: { code: 'internal' } }, 500));
  await assert.rejects(fetchHandler(SITE, 'abc'), (e: unknown) => e instanceof HubError && e.status === 500);

  stubFetch(() => {
    throw new TypeError('Failed to fetch');
  });
  await assert.rejects(fetchComments(SITE, 'abc'), (e: unknown) => e instanceof HubError && /unreachable/.test(e.message));
});
