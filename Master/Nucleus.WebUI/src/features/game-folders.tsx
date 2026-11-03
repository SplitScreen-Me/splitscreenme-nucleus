// The game folders of the automatic game search (Nucleus 2.5): reading them, adding one with the folder picker, and
// the progress of a search. Shared by Settings > Game folders, "Search my games" and the add-game flows.
import { useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeQuery, type GameFolders } from '@/bridge';
import { useI18n } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import type { FolderSearchState } from '@/lib/game-search';
import { Button, Icon, Spinner, useToast } from '@/ui';

/** The saved game folders and the automatic search choice (`settings.getGameFolders`). */
export function useGameFolders() {
  return useBridgeQuery('settings.getGameFolders');
}

/** Last part of a folder path, for short messages ("D:\\Games\\Steam" → "Steam"). */
export const folderName = (path: string) => path.replace(/[\\/]+$/, '').split(/[\\/]/).pop() || path;

/**
 * Asks for a folder with the native picker and adds it to the game folders. Resolves with the new list, or null when
 * the user cancelled or the folder was refused (toasted).
 */
export function useAddGameFolder() {
  const run = useRun();
  const toast = useToast();
  const queryClient = useQueryClient();
  const { t } = useI18n();
  return useCallback(
    async (initialDirectory?: string): Promise<GameFolders | null> => {
      const path = await run(() => bridge.dialogs.pickFolder({ title: t('features.gameFolders.pickTitle'), initialDirectory }, { timeoutMs: 0 }));
      if (!path) return null;
      const key = bridgeQueryKey('settings.getGameFolders');
      const before = queryClient.getQueryData<GameFolders>(key);
      const folders = await run(() => bridge.settings.addGameFolder({ path }));
      if (!folders) return null;
      queryClient.setQueryData(key, folders);
      const normalized = path.replace(/[\\/]+$/, '').toLowerCase();
      const known = before?.folders.some((f) => f.path.toLowerCase() === normalized);
      toast(known ? t('features.gameFolders.already', { folder: folderName(path) }) : t('features.gameFolders.added', { folder: folderName(path) }), {
        icon: known ? 'info' : 'folder',
      });
      return folders;
    },
    [run, toast, queryClient, t],
  );
}

/** Progress of a running search: bar, folder being searched, games found so far, and a button to stop. */
export function SearchProgress({
  state,
  title,
  onStop,
  stopLabel,
}: {
  state: FolderSearchState;
  title: string;
  onStop: () => void;
  stopLabel?: string;
}) {
  const { t, format } = useI18n();
  const progress = state.status === 'searching' ? state.progress : undefined;
  const pct = progress?.percent ?? 0;
  return (
    <div className="grid gap-3 py-1">
      <p className="flex items-center gap-2.5 text-15 font-[600] text-ink" role="status">
        <Spinner />
        {title}
      </p>
      <div
        className="h-1.5 overflow-hidden rounded-full bg-pane-3"
        role="progressbar"
        aria-valuenow={pct}
        aria-valuemin={0}
        aria-valuemax={100}
        aria-label={t('features.gameSearch.searching.progressLabel')}
      >
        <div className="h-full origin-left bg-ink transition-transform duration-(--dur-2)" style={{ transform: `scaleX(${Math.max(0.02, pct / 100)})` }} />
      </div>
      <div className="flex min-w-0 items-baseline gap-3">
        <span className="data min-w-0 flex-1 truncate text-13 text-ink-3" title={progress?.currentFolder}>
          {progress?.currentFolder ?? '\u00a0'}
        </span>
        <span className="data shrink-0 text-13 text-ink-2" aria-live="polite">
          {t('features.gameSearch.searching.found', { count: progress?.found ?? 0, percent: format.percent(pct / 100) })}
        </span>
      </div>
      <div className="flex justify-end">
        <Button variant="ghost" onClick={onStop} data-nav autoFocus>
          <Icon name="stop" />
          {stopLabel ?? t('features.gameSearch.searching.stop')}
        </Button>
      </div>
    </div>
  );
}
