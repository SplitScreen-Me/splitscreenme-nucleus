import { useId, useState } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey } from '@/bridge';
import { folderName, useAddGameFolder, useGameFolders } from '@/features/game-folders';
import { GameSearchDialog } from '@/features/game-search';
import { useT } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { Button, cn, Icon, Spinner, useToast } from '@/ui';

/**
 * The game folders of the automatic search (Nucleus 2.5), under the section's switch: add one with the folder picker,
 * remove one, and search them now. Each change is saved at once.
 */
export function GameFoldersEditor() {
  const { data, isPending, isError } = useGameFolders();
  const addFolder = useAddGameFolder();
  const t = useT();
  const run = useRun();
  const toast = useToast();
  const queryClient = useQueryClient();
  const titleId = useId();
  const [removing, setRemoving] = useState<string | null>(null);
  const [adding, setAdding] = useState(false);
  const [searching, setSearching] = useState(false);

  const remove = async (path: string) => {
    setRemoving(path);
    const folders = await run(() => bridge.settings.removeGameFolder({ path }));
    setRemoving(null);
    if (!folders) return;
    queryClient.setQueryData(bridgeQueryKey('settings.getGameFolders'), folders);
    toast(t('settings.gameFolders.removed', { folder: folderName(path) }), { icon: 'folder' });
  };

  const add = async () => {
    setAdding(true);
    await addFolder(data?.folders.find((f) => f.exists)?.path);
    setAdding(false);
  };

  if (isPending) {
    return (
      <p className="flex items-center gap-2 border-t border-line py-4 text-small text-ink-2">
        <Spinner /> {t('settings.gameFolders.loading')}
      </p>
    );
  }
  if (isError || !data) {
    return <p className="border-t border-line py-4 text-small text-ink-2">{t('settings.gameFolders.error')}</p>;
  }

  const searchable = data.folders.some((f) => f.exists);
  return (
    <div className="border-t border-line pt-4 pb-5 first:border-t-0 first:pt-1" role="group" aria-labelledby={titleId}>
      <div className="mb-3 flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <h3 id={titleId} className="text-15 font-semibold">
          {t('settings.gameFolders.editorTitle')}
        </h3>
        <p className="text-13 text-ink-3">{t('settings.gameFolders.editorHint')}</p>
      </div>
      {data.folders.length === 0 ? (
        <p className="rounded-ctl bg-pane-2 px-4 py-3 text-15 text-ink-2">{t('settings.gameFolders.empty')}</p>
      ) : (
        <ul className="m-0 grid list-none gap-1.5 p-0">
          {data.folders.map((f) => (
            <li key={f.path} className="flex min-w-0 items-center gap-3 rounded-ctl bg-pane-2 py-1.5 pr-1.5 pl-3.5">
              <Icon name="folder" className={cn('size-4 shrink-0', f.exists ? 'text-ink-3' : 'text-warn')} />
              <span className={cn('data min-w-0 flex-1 truncate text-13', f.exists ? 'text-ink' : 'text-ink-3')} title={f.path}>
                {f.path}
              </span>
              {!f.exists && (
                <span className="shrink-0 rounded-tag bg-pane-3 px-2 py-0.5 text-12 font-[650] text-warn" title={t('settings.gameFolders.missingHint')}>
                  {t('settings.gameFolders.missing')}
                </span>
              )}
              <Button
                variant="quiet"
                size="icon-sm"
                onClick={() => void remove(f.path)}
                disabled={removing !== null}
                aria-label={t('settings.gameFolders.removeLabel', { folder: f.path })}
                title={t('settings.gameFolders.remove')}
                data-nav
              >
                {removing === f.path ? <Spinner /> : <Icon name="x" />}
              </Button>
            </li>
          ))}
        </ul>
      )}
      <div className="mt-4 flex flex-wrap items-center gap-2">
        <Button variant="ghost" size="sm" onClick={() => void add()} disabled={adding} data-nav>
          {adding ? <Spinner /> : <Icon name="plus" />}
          {t('settings.gameFolders.add')}
        </Button>
        <Button variant="ghost" size="sm" onClick={() => setSearching(true)} disabled={!searchable} data-nav className="ml-auto">
          <Icon name="search" />
          {t('settings.gameFolders.searchNow')}
        </Button>
      </div>
      <GameSearchDialog open={searching} onClose={() => setSearching(false)} editFoldersLink={false} />
    </div>
  );
}
