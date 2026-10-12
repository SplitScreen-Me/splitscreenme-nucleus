// Public Hub API client. Called straight from the WebView: /api/v1/* answers with CORS `*`.
// Simple GETs only, and NO custom headers: anything that triggers a preflight would fail
// (the Hub has no OPTIONS route). Screenshots and .nc packages have no CORS: they go through `host`.
import { isListable, normalizeComment, normalizeHandler } from './normalize';
import type { HubComment, HubCommentRaw, HubHandler, HubHandlerRaw } from './types';

export const HUB_ORIGIN = 'https://hub.splitscreen.me';
const API = `${HUB_ORIGIN}/api/v1`;

export class HubError extends Error {
  readonly status?: number;
  constructor(message: string, status?: number) {
    super(message);
    this.name = 'HubError';
    this.status = status;
  }
}

async function getJson<T>(path: string, signal?: AbortSignal): Promise<T> {
  let res: Response;
  try {
    res = await fetch(`${API}${path}`, { signal, credentials: 'omit' });
  } catch (err) {
    if ((err as Error).name === 'AbortError') throw err;
    throw new HubError('The Hub is unreachable. Check your connection.');
  }
  if (!res.ok) throw new HubError(`The Hub answered ${res.status}.`, res.status);
  try {
    return (await res.json()) as T;
  } catch {
    throw new HubError('The Hub sent an unreadable answer.');
  }
}

/** All public handlers (about 600, ~1 MB uncompressed). No pagination: fetch once, filter client-side. */
export async function fetchAllHandlers(signal?: AbortSignal): Promise<HubHandler[]> {
  const body = await getJson<{ Handlers?: HubHandlerRaw[] }>('/allhandlers', signal);
  return (body.Handlers ?? []).filter(isListable).map(normalizeHandler);
}

/** One handler, or null when it does not exist (or is private). */
export async function fetchHandler(id: string, signal?: AbortSignal): Promise<HubHandler | null> {
  const body = await getJson<{ Handlers?: HubHandlerRaw[] }>(`/handler/${encodeURIComponent(id)}`, signal);
  const raw = body.Handlers?.[0];
  return raw && isListable(raw) ? normalizeHandler(raw) : null;
}

export async function fetchComments(handlerId: string, signal?: AbortSignal): Promise<HubComment[]> {
  const body = await getJson<{ Comments?: HubCommentRaw[] }>(`/comments/${encodeURIComponent(handlerId)}`, signal);
  return (body.Comments ?? []).map(normalizeComment);
}

/** Web page of a handler on the Hub (opened in the default browser through host.app.openExternal). */
export const hubHandlerUrl = (id: string) => `${HUB_ORIGIN}/handler/${encodeURIComponent(id)}`;
export const hubUserUrl = (ownerId: string) => `${HUB_ORIGIN}/user/${encodeURIComponent(ownerId)}`;
