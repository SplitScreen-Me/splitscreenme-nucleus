// TanStack Query hooks for the public Hub API.
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { fetchAllHandlers, fetchComments, fetchHandler } from './api';
import { threadComments } from './normalize';
import { loadCatalogue, saveCatalogue } from './persist';
import type { HubHandler } from './types';

export const hubKeys = {
  all: ['hub', 'allhandlers'] as const,
  handler: (id: string) => ['hub', 'handler', id] as const,
  comments: (id: string) => ['hub', 'comments', id] as const,
};

/** The catalogue changes slowly; refresh it at most every 15 minutes while the app runs. */
const CATALOGUE_STALE_MS = 15 * 60_000;

// Read once at startup: the app opens with the last known catalogue, even offline.
const persisted = loadCatalogue();

/** All public handlers. Starts from the persisted copy, refetches when stale, saves every fresh copy. */
export function useAllHandlers() {
  return useQuery({
    networkMode: 'online',
    queryKey: hubKeys.all,
    queryFn: async ({ signal }) => {
      const handlers = await fetchAllHandlers(signal);
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

/** One handler. Renders instantly from the catalogue, then refreshes from /handler/:id. */
export function useHubHandler(id: string | undefined) {
  const qc = useQueryClient();
  return useQuery({
    networkMode: 'online',
    queryKey: hubKeys.handler(id ?? ''),
    queryFn: ({ signal }) => fetchHandler(id!, signal),
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
    queryFn: ({ signal }) => fetchComments(id!, signal),
    enabled: Boolean(id),
    staleTime: 2 * 60_000,
    select: threadComments,
    retry: 1,
  });
}
