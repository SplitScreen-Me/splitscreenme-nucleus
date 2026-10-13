// Public hub API v3 client, called straight from the WebView: every endpoint answers any origin (CORS `*`) and never
// reads cookies. Plain GETs; the browser cache revalidates with the ETags. Screenshots go through the host
// (`handlers.screenshots`, which also caches them for the library); packages are downloaded and checked by the host.
// No bridge import: `site` is passed in (see site.ts), so the node tests load this file with a stubbed fetch.
import { isHandler, normalizeComment, normalizeHandler } from './normalize.ts';
import type { HubCatalogRaw, HubComment, HubCommentPageRaw, HubHandler } from './types.ts';
import { hubApiUrl } from './urls.ts';

export class HubError extends Error {
  readonly status?: number;
  constructor(message: string, status?: number) {
    super(message);
    this.name = 'HubError';
    this.status = status;
  }
}

/** Comments asked per page (the API maximum), and pages read at most (1000 comments). */
const COMMENTS_PER_PAGE = 200;
const MAX_COMMENT_PAGES = 5;

async function getJson<T>(url: string, signal: AbortSignal | undefined, notFound: 'null' | 'error' = 'error'): Promise<T | null> {
  let res: Response;
  try {
    res = await fetch(url, { signal, credentials: 'omit' });
  } catch (err) {
    if ((err as Error).name === 'AbortError') throw err;
    throw new HubError('The Hub is unreachable. Check your connection.');
  }
  if (res.status === 404 && notFound === 'null') return null;
  if (res.status === 429) {
    const wait = Number(res.headers.get('Retry-After'));
    throw new HubError(`The Hub is busy right now. Try again ${wait > 0 ? `in ${Math.ceil(wait)} s` : 'in a moment'}.`, 429);
  }
  if (!res.ok) throw new HubError(`The Hub answered ${res.status}.`, res.status);
  try {
    return (await res.json()) as T;
  } catch {
    throw new HubError('The Hub sent an unreadable answer.');
  }
}

/** Every listed handler (`GET /catalog`, ~900 items, gzip). Fetch once and filter client-side. */
export async function fetchCatalogue(site: string, signal?: AbortSignal): Promise<HubHandler[]> {
  const body = await getJson<HubCatalogRaw>(hubApiUrl(site, '/catalog'), signal);
  return (Array.isArray(body?.items) ? body.items : []).filter(isHandler).map(normalizeHandler);
}

/** One handler with its notes (`GET /handlers/{id}`), or null when it is not listed (private, deleted, unknown). */
export async function fetchHandler(site: string, id: string, signal?: AbortSignal): Promise<HubHandler | null> {
  const body = await getJson<unknown>(hubApiUrl(site, `/handlers/${encodeURIComponent(id)}`), signal, 'null');
  return isHandler(body) ? normalizeHandler(body) : null;
}

/** Comments of a handler, newest first (`GET /handlers/{id}/comments`, following `nextCursor`). */
export async function fetchComments(site: string, handlerId: string, signal?: AbortSignal): Promise<HubComment[]> {
  const comments: HubComment[] = [];
  let before: string | null | undefined;
  for (let page = 0; page < MAX_COMMENT_PAGES; page++) {
    const query = new URLSearchParams({ limit: String(COMMENTS_PER_PAGE) });
    if (before) query.set('before', before);
    const body = await getJson<HubCommentPageRaw>(hubApiUrl(site, `/handlers/${encodeURIComponent(handlerId)}/comments?${query}`), signal, 'null');
    for (const raw of body?.items ?? []) if (typeof raw?.id === 'string') comments.push(normalizeComment(raw));
    before = body?.nextCursor;
    if (!before) break;
  }
  return comments;
}
