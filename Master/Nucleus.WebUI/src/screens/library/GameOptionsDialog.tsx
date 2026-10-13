import { useEffect, useRef } from 'react';
import { useBridgeQuery, type GameSummary } from '@/bridge';
import { useI18n } from '@/i18n';
import { downloadProgress, useActiveDownload } from '@/lib/downloads';
import { gameStatus, setupPath } from '@/lib/library';
import { Button, ButtonLink, Cover, Dialog, Icon, Spinner, useToast } from '@/ui';
import { handlerLine } from './FeaturedGame';
import { GameMenu } from './GameMenu';

export interface GameOptionsDialogProps {
  /** Game shown, or null when closed. */
  gameId: string | null;
  games: readonly GameSummary[] | undefined;
  onClose: () => void;
  onShowNotes: (gameId: string) => void;
  onShowDetails: (gameId: string) => void;
  onUpdate: (game: GameSummary) => void;
  onLocate: (game: GameSummary) => void;
}

/** Per-game sheet: the game, its handler, and the host's menu for it. */
export function GameOptionsDialog({ gameId, games, onClose, onShowNotes, onShowDetails, onUpdate, onLocate }: GameOptionsDialogProps) {
  const toast = useToast();
  const { t, format } = useI18n();
  const game = gameId ? games?.find((g) => g.id === gameId) : undefined;
  const menu = useBridgeQuery('library.getMenu', { gameId: gameId ?? '' }, { enabled: Boolean(game) });
  const updating = useActiveDownload({ gameId: gameId ?? undefined });

  // The game left the library while its sheet was open (its "remove" item, or elsewhere): close and say so.
  const shown = useRef<GameSummary | undefined>(undefined);
  useEffect(() => {
    if (game) shown.current = game;
    else if (gameId && games && shown.current?.id === gameId) {
      const name = shown.current.name;
      shown.current = undefined;
      onClose();
      toast(t('library.toasts.gameRemoved', { game: name }));
    }
  }, [game, gameId, games, onClose, toast, t]);

  const g = game ?? (shown.current?.id === gameId ? shown.current : undefined);
  const status = g ? gameStatus(g) : 'ready';
  return (
    <Dialog open={Boolean(gameId && g)} onClose={onClose} title={g ? t('library.options.title', { game: g.name }) : t('library.options.defaultTitle')} icon="sliders" className="w-[min(760px,92vw)]">
      {g && (
        <div className="grid gap-5">
          <div className="grid grid-cols-[64px_minmax(0,1fr)] items-center gap-4">
            <Cover src={g.coverUrl} title={g.name} ratio="2/3" className="w-16 rounded-md" />
            <div className="grid min-w-0 gap-2.5">
              <p className="text-small text-ink-2">
                {t('library.options.summary', { handler: handlerLine(t, g), count: g.maxPlayers })}
                {status === 'missing' && <span className="text-warn"> {t('library.options.executableMissing')}</span>}
              </p>
              <div className="flex flex-wrap gap-2">
                <ButtonLink to={setupPath(g.id)} variant="primary" size="sm" onClick={onClose} data-nav>
                  <Icon name="play" />
                  {t('library.options.setUpSession')}
                </ButtonLink>
                {status === 'missing' && (
                  <Button size="sm" onClick={() => onLocate(g)} data-nav>
                    <Icon name="search" />
                    {t('library.options.locateGame')}
                  </Button>
                )}
                {g.updateAvailable && (
                  <Button size="sm" disabled={Boolean(updating)} onClick={() => onUpdate(g)} data-nav>
                    {updating ? <Spinner /> : <Icon name="update" />}
                    {updating
                      ? t('library.options.updating', { percent: format.percent(downloadProgress(updating)) })
                      : t('library.options.updateTo', { version: String(g.latestHandlerVersion) })}
                  </Button>
                )}
                {g.handlerId && (
                  <ButtonLink to={`/games/${encodeURIComponent(g.handlerId)}`} variant="ghost" size="sm" onClick={onClose} data-nav>
                    <Icon name="compass" />
                    {t('library.options.handlerPage')}
                  </ButtonLink>
                )}
              </div>
            </div>
          </div>
          <div className="border-t border-line pt-3">
            {menu.isPending ? (
              <p className="flex items-center gap-2.5 py-4 text-small text-ink-2">
                <Spinner /> {t('library.options.loading', { game: g.name })}
              </p>
            ) : menu.isError || !menu.data ? (
              <p className="flex items-center gap-2.5 py-4 text-small text-ink-2">
                <Icon name="warning" className="size-4 text-warn" />
                {t('library.options.loadError')}
                <Button size="sm" variant="ghost" onClick={() => void menu.refetch()}>
                  {t('common.tryAgain')}
                </Button>
              </p>
            ) : (
              <GameMenu
                gameId={g.id}
                gameName={g.name}
                items={menu.data.items}
                onShowNotes={() => onShowNotes(g.id)}
                onShowDetails={() => onShowDetails(g.id)}
              />
            )}
          </div>
        </div>
      )}
    </Dialog>
  );
}
