// The game folder search (`library.searchGames`, Nucleus 2.5): one call per search, its progress through
// `library.searchProgress` events that carry the search id, cancelled through the call's signal ($/cancel).
import { useCallback, useEffect, useRef, useState } from 'react';
import { bridge, bridgeClient, BridgeError, type FolderSearchProgress, type FolderSearchResult } from '@/bridge';

let seq = 0;

export interface FolderSearchRun {
  id: string;
  /** Resolves with the result; rejects with `cancelled` after {@link FolderSearchRun.cancel}. */
  done: Promise<FolderSearchResult>;
  cancel(): void;
}

/** Starts a search of the game folders: every installed handler, or one (`gameId`, first copy only). */
export function startFolderSearch(onProgress: (progress: FolderSearchProgress) => void, gameId?: string): FolderSearchRun {
  seq += 1;
  const id = `search-${Date.now().toString(36)}-${seq}`;
  const controller = new AbortController();
  const off = bridgeClient.on('library.searchProgress', (progress) => {
    if (progress.searchId === id && !controller.signal.aborted) onProgress(progress);
  });
  const done = bridge.library.searchGames({ searchId: id, gameId }, { signal: controller.signal, timeoutMs: 0 }).finally(off);
  return { id, done, cancel: () => controller.abort() };
}

export type FolderSearchState =
  | { status: 'idle' }
  | { status: 'searching'; progress?: FolderSearchProgress }
  | { status: 'done'; result: FolderSearchResult }
  | { status: 'cancelled' }
  | { status: 'failed'; error: unknown };

/** A search of the game folders owned by a component: stopped when it unmounts or starts another one. */
export function useFolderSearch() {
  const [state, setState] = useState<FolderSearchState>({ status: 'idle' });
  const run = useRef<FolderSearchRun | null>(null);

  const cancel = useCallback(() => {
    run.current?.cancel();
    run.current = null;
    setState((s) => (s.status === 'searching' ? { status: 'cancelled' } : s));
  }, []);

  const start = useCallback(async (gameId?: string): Promise<FolderSearchResult | null> => {
    run.current?.cancel();
    const current = startFolderSearch((progress) => {
      if (run.current === current) setState({ status: 'searching', progress });
    }, gameId);
    run.current = current;
    setState({ status: 'searching' });
    try {
      const result = await current.done;
      if (run.current !== current) return null;
      run.current = null;
      setState({ status: 'done', result });
      return result;
    } catch (error) {
      if (run.current !== current) return null;
      run.current = null;
      setState(BridgeError.is(error, 'cancelled') ? { status: 'cancelled' } : { status: 'failed', error });
      return null;
    }
  }, []);

  const reset = useCallback(() => {
    run.current?.cancel();
    run.current = null;
    setState({ status: 'idle' });
  }, []);

  useEffect(() => () => run.current?.cancel(), []);

  return { state, start, cancel, reset };
}

export { afterInstallStep, contextMismatchOf, defaultPicks, duplicatedGames, folderOf, pickAll, togglePick } from './game-search-core';
