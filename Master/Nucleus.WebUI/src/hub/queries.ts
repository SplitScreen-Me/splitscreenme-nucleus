// TanStack Query hooks for the public hub API v3.
import { useEffect, useRef } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { isMockBridge } from '@/bridge';
import { fetchCatalogue, fetchComments, fetchHandler } from './api';
import { fetchNews, newsQueryOptions, normalizeNews, type NewsItem } from './news';
import { threadComments } from './normalize';
import { loadCatalogue, saveCatalogue } from './persist';
import { hubSite } from './site';
import type { HubHandler } from './types';

export const hubKeys = {
  all: ['hub', 'catalog'] as const,
  handler: (id: string) => ['hub', 'handler', id] as const,
  comments: (id: string) => ['hub', 'comments', id] as const,
  news: ['hub', 'news'] as const,
};

/** The catalogue changes slowly (the API caches it 5 minutes); refresh it at most every 15 minutes while the app runs. */
const CATALOGUE_STALE_MS = 15 * 60_000;

// Read once at startup: the app opens with the last known catalogue, even offline.
const persisted = loadCatalogue();

/** All listed handlers. Starts from the persisted copy, refetches when stale, saves every fresh copy. */
export function useAllHandlers() {
  return useQuery({
    networkMode: 'online',
    queryKey: hubKeys.all,
    queryFn: async ({ signal }) => {
      const handlers = await fetchCatalogue(await hubSite(), signal);
      saveCatalogue(handlers);
      return handlers;
    },
    initialData: persisted?.handlers,
    initialDataUpdatedAt: persisted?.savedAt,
    staleTime: CATALOGUE_STALE_MS,
    gcTime: Infinity,
    retry: 2,
    retryDelay: (n) => Math.min(8000, 1000 * 2 ** n),
  });
}

/** One handler. Renders instantly from the catalogue, then refreshes from /handlers/:id (notes, game summary). */
export function useHubHandler(id: string | undefined) {
  const qc = useQueryClient();
  return useQuery({
    networkMode: 'online',
    queryKey: hubKeys.handler(id ?? ''),
    queryFn: async ({ signal }) => fetchHandler(await hubSite(), id!, signal),
    enabled: Boolean(id),
    staleTime: 5 * 60_000,
    placeholderData: () => qc.getQueryData<HubHandler[]>(hubKeys.all)?.find((h) => h.id === id),
    retry: 1,
  });
}

/** Comments of a handler, threaded (replies under their parent). */
export function useHubComments(id: string | undefined) {
  return useQuery({
    networkMode: 'online',
    queryKey: hubKeys.comments(id ?? ''),
    queryFn: async ({ signal }) => fetchComments(await hubSite(), id!, signal),
    enabled: Boolean(id),
    staleTime: 2 * 60_000,
    select: threadComments,
    retry: 1,
  });
}

/** The site's news; the in-browser mock answers with its own (src/bridge/mock/news.ts) unless asked for the live site. */
async function loadNews(signal: AbortSignal): Promise<NewsItem[]> {
  if (isMockBridge) {
    const answer = (await import('@/bridge/mock/news')).mockNewsAnswer();
    if (answer) return normalizeNews(answer);
  }
  return fetchNews(await hubSite(), signal);
}

/**
 * News of the site for the title bar ticker, polled every 10 minutes (see newsQueryOptions). `paused` while a game
 * session is live: no polling then, and a refetch as soon as it ends. Focusing the window refetches a copy older than a
 * minute (WebView2 seldom reports the page hidden, so TanStack's own focus refetch rarely fires).
 */
export function useHubNews(paused: boolean) {
  const qc = useQueryClient();
  const query = useQuery(newsQueryOptions(paused, loadNews));
  const wasPaused = useRef(paused);

  useEffect(() => {
    if (wasPaused.current && !paused) void qc.refetchQueries({ queryKey: hubKeys.news, type: 'active' }, { cancelRefetch: false });
    wasPaused.current = paused;
  }, [paused, qc]);

  useEffect(() => {
    if (paused) return;
    const onFocus = () => void qc.refetchQueries({ queryKey: hubKeys.news, type: 'active', stale: true }, { cancelRefetch: false });
    window.addEventListener('focus', onFocus);
    return () => window.removeEventListener('focus', onFocus);
  }, [paused, qc]);

  return query;
}
