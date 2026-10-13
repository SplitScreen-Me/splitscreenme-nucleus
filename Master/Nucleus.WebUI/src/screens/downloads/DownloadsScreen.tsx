import { useMemo, useState, type ReactNode } from 'react';
import { Link } from 'react-router';
import { bridge, type GameSummary } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { useAllHandlers, type HubHandler } from '@/hub';
import { T, useI18n, type TFunction } from '@/i18n';
import { igdbImage } from '@/lib/art';
import { useRun } from '@/lib/bridge-ui';
import {
  cancelDownload,
  clearFinishedDownloads,
  downloadProgress,
  installHandler,
  isActiveStage,
  updateHandler,
  useActiveDownload,
  useDownloads,
  type DownloadItem,
} from '@/lib/downloads';
import { setupPath, useLibrary } from '@/lib/library';
import { useAppInfo } from '@/lib/live';
import { useScreenChrome } from '@/shell/chrome';
import { Button, ButtonLink, Cover, Icon, Pane, Spinner, cn, useToast } from '@/ui';
import { stageText } from './stage-text';

/** What a row shows of the game an item installs, from the item, the library or the hub catalogue. */
interface Resolved {
  name: string;
  cover?: string;
  game?: GameSummary;
  hub?: HubHandler;
}

function resolve(t: TFunction, item: DownloadItem, library: readonly GameSummary[], hub: readonly HubHandler[]): Resolved {
  const gameId = item.gameId ?? item.result?.gameId;
  const game = gameId ? library.find((g) => g.id === gameId) : library.find((g) => g.handlerId === item.source);
  const handler = hub.find((h) => h.id === item.source || (game?.handlerId !== undefined && h.id === game.handlerId));
  const fileName = item.kind === 'file' ? item.source.slice(Math.max(item.source.lastIndexOf('\\'), item.source.lastIndexOf('/')) + 1) : undefined;
  return {
    name: item.result?.gameName ?? item.name ?? game?.name ?? handler?.gameName ?? fileName ?? t('downloads.row.defaultName'),
    cover: item.coverUrl ?? game?.coverUrl ?? igdbImage(handler?.coverImageId, 'cover_big'),
    game,
    hub: handler,
  };
}

/** Handler downloads (this session) and pending handler updates. */
export default function DownloadsScreen() {
  const items = useDownloads();
  const { data: library = [] } = useLibrary();
  const { data: hub = [] } = useAllHandlers();
  const { data: info } = useAppInfo();
  const run = useRun();
  const toast = useToast();
  const { t } = useI18n();
  const [updatingAll, setUpdatingAll] = useState(false);
  const [checking, setChecking] = useState(false);
  const offline = info?.hubOnline === false;

  const outdated = useMemo(() => library.filter((g) => g.updateAvailable).sort((a, b) => a.name.localeCompare(b.name, 'en')), [library]);
  const newestFirst = useMemo(() => [...items].reverse(), [items]);
  const active = newestFirst.filter((d) => isActiveStage(d.stage));
  const finished = newestFirst.filter((d) => !isActiveStage(d.stage));

  const update = (g: GameSummary) =>
    run(() => updateHandler(g.id, { name: g.name, coverUrl: g.coverUrl, handlerId: g.handlerId, version: g.latestHandlerVersion }), {
      success: g.latestHandlerVersion
        ? t('downloads.toasts.handlerUpdatedTo', { game: g.name, version: String(g.latestHandlerVersion) })
        : t('downloads.toasts.handlerUpdated', { game: g.name }),
    });

  const updateAll = async () => {
    setUpdatingAll(true);
    try {
      for (const g of outdated) await update(g);
    } finally {
      setUpdatingAll(false);
    }
  };

  const checkUpdates = async () => {
    setChecking(true);
    const done = await run(() => bridge.handlers.checkUpdates({ timeoutMs: 120_000 }).then(() => true));
    setChecking(false);
    if (done) toast(t('downloads.toasts.checked'), { icon: 'update' });
  };

  useScreenChrome([{ label: t('downloads.title') }], {
    base: [
      { button: 'A', label: t('common.select') },
      ...(outdated.length && !offline ? [{ button: 'X' as const, label: t('downloads.hints.updateAll'), action: () => void updateAll() }] : []),
      { button: 'Y', label: t('common.search') },
    ],
  });

  return (
    <div className="grid content-start gap-gutter">
      <Pane as="header" className="flex flex-wrap items-end gap-6 px-8 pt-8 pb-7">
        <div className="grid gap-2">
          <h1 className="type-h1">{t('downloads.title')}</h1>
          <p className="max-w-[60ch] text-ink-2">{t('downloads.intro')}</p>
        </div>
        <div className="ml-auto flex flex-wrap gap-2">
          <Button variant="ghost" onClick={() => void checkUpdates()} disabled={checking || offline} data-nav title={offline ? t('downloads.hubOffline') : undefined}>
            {checking ? <Spinner /> : <Icon name="refresh" />}
            {t('downloads.checkUpdates')}
          </Button>
          {finished.length > 0 && (
            <Button variant="ghost" onClick={clearFinishedDownloads} data-nav>
              <Icon name="check" />
              {t('downloads.clearFinished')}
            </Button>
          )}
        </div>
      </Pane>

      {outdated.length > 0 && (
        <Pane as="section" aria-labelledby="updates-title" className="px-6 pt-6 pb-4">
          <div className="mb-3 flex items-center gap-3">
            <h2 id="updates-title" className="type-h3">
              {t('downloads.updates.title')}
            </h2>
            <span className="data text-15 text-ink-3">{outdated.length}</span>
            <Button size="sm" className="ml-auto" onClick={() => void updateAll()} disabled={updatingAll || offline} data-nav>
              {updatingAll ? <Spinner /> : <Icon name="update" />}
              {updatingAll ? t('downloads.updates.updating') : t('downloads.updates.updateAll')}
            </Button>
          </div>
          {offline && (
            <p className="mb-2 flex items-center gap-2 text-13 text-ink-2">
              <Icon name="offline" className="size-4 text-warn" />
              {t('downloads.updates.offline')}
            </p>
          )}
          <ul className="m-0 list-none p-0">
            {outdated.map((g) => (
              <UpdateRow key={g.id} game={g} disabled={offline} onUpdate={() => void update(g)} />
            ))}
          </ul>
        </Pane>
      )}

      <Pane as="section" aria-labelledby="dl-title" className="px-6 pt-6 pb-4">
        <div className="mb-3 flex items-baseline gap-3">
          <h2 id="dl-title" className="type-h3">
            {active.length ? t('downloads.list.inProgress') : t('downloads.list.recent')}
          </h2>
          {items.length > 0 && <span className="data text-15 text-ink-3">{t('downloads.list.count', { count: items.length })}</span>}
        </div>
        {items.length === 0 ? (
          <div className="grid justify-items-start gap-4 py-8">
            <p className="max-w-[56ch] text-ink-2">{t('downloads.list.empty')}</p>
            <ButtonLink to="/games" variant="primary" data-nav>
              <Icon name="compass" />
              {t('downloads.list.getGames')}
            </ButtonLink>
          </div>
        ) : (
          <ul className="m-0 list-none p-0" aria-live="polite">
            {[...active, ...finished].map((d) => (
              <DownloadRow key={d.id} item={d} resolved={resolve(t, d, library, hub)} />
            ))}
          </ul>
        )}
      </Pane>
    </div>
  );
}

function UpdateRow({ game: g, disabled, onUpdate }: { game: GameSummary; disabled: boolean; onUpdate: () => void }) {
  const active = useActiveDownload({ gameId: g.id, handlerId: g.handlerId });
  const { t, format } = useI18n();
  const to = g.latestHandlerVersion !== undefined ? String(g.latestHandlerVersion) : '?';
  return (
    <li className="grid grid-cols-[40px_minmax(0,1fr)_auto] items-center gap-4 border-t border-line py-3 first:border-t-0">
      <Cover src={g.coverUrl} title={g.name} ratio="3/4" className="w-10 rounded-md" />
      <div className="min-w-0">
        <b className="block truncate text-15 font-[650]">{g.name}</b>
        {active ? (
          <span className="data text-13 text-ink-2">{stageText(t, format, active)}</span>
        ) : (
          <span className="data text-13 text-warn">
            {g.handlerVersion !== undefined ? t('downloads.updates.fromTo', { from: String(g.handlerVersion), to }) : t('downloads.updates.to', { to })}
          </span>
        )}
      </div>
      <Button size="sm" variant="ghost" disabled={Boolean(active) || disabled} onClick={onUpdate} data-nav aria-label={t('downloads.updates.updateLabel', { game: g.name })}>
        {active ? <Spinner /> : <Icon name="update" />}
        {t('common.update')}
      </Button>
    </li>
  );
}

function DownloadRow({ item: d, resolved }: { item: DownloadItem; resolved: Resolved }) {
  const run = useRun();
  const { afterInstall } = useAddGame();
  const { t, format } = useI18n();
  const [adding, setAdding] = useState(false);
  const running = isActiveStage(d.stage);
  const progress = downloadProgress(d);
  const pct = Math.round(progress * 100);
  const { name, cover, game } = resolved;
  const stage = stageText(t, format, d);
  const what =
    d.kind === 'update'
      ? d.version
        ? t('downloads.row.updateTo', { version: String(d.version) })
        : t('downloads.row.update')
      : d.kind === 'file'
        ? t('downloads.row.file')
        : d.version
          ? t('downloads.row.installVersion', { version: String(d.version) })
          : t('downloads.row.install');
  const needsGame = d.stage === 'done' && d.result !== undefined && !d.result.inLibrary && !game;

  const retry = () => {
    if (d.kind === 'update' && d.gameId) void run(() => updateHandler(d.gameId!, { name, coverUrl: cover, version: d.version, handlerId: resolved.hub?.id }));
    else if (d.kind === 'install') void run(() => installHandler(d.source, { name, coverUrl: cover, version: d.version }));
  };

  let action: ReactNode = <span />;
  if (running && d.cancellable) {
    action = (
      <Button size="sm" variant="ghost" onClick={() => cancelDownload(d.id)} data-nav aria-label={t('downloads.row.cancelLabel', { game: name })}>
        {t('common.cancel')}
      </Button>
    );
  } else if (needsGame) {
    action = (
      <Button
        size="sm"
        disabled={adding}
        data-nav
        onClick={async () => {
          setAdding(true);
          try {
            await afterInstall(d.result!);
          } finally {
            setAdding(false);
          }
        }}
      >
        {adding ? <Spinner /> : <Icon name="plus" />}
        {t('downloads.row.addGame')}
      </Button>
    );
  } else if (d.stage === 'done' && game) {
    action = (
      <Link to={setupPath(game.id)} className="rounded text-small font-semibold text-ink-2 no-underline hover:text-ink" data-nav>
        {t('downloads.row.setUpSession')}
      </Link>
    );
  } else if ((d.stage === 'failed' || d.stage === 'cancelled') && d.kind !== 'file') {
    action = (
      <Button size="sm" variant="ghost" onClick={retry} data-nav aria-label={t('downloads.row.retryLabel', { game: name })}>
        <Icon name="refresh" />
        {t('common.tryAgain')}
      </Button>
    );
  }

  const ended = d.endedAt ?? d.startedAt;
  return (
    <li className="grid grid-cols-[40px_minmax(0,1fr)_auto] items-center gap-4 border-t border-line py-3 first:border-t-0">
      <Cover src={cover} title={name} ratio="3/4" className="w-10 rounded-md" />
      <div className="grid min-w-0 gap-1.5">
        <div className="flex min-w-0 items-baseline gap-3">
          <b className="truncate text-15 font-[650]">{name}</b>
          <span className="data truncate text-13 text-ink-3">{what}</span>
        </div>
        {running ? (
          <div className="flex items-center gap-3">
            <div
              className="h-1 flex-1 overflow-hidden rounded-full bg-pane-3"
              role="progressbar"
              aria-valuenow={pct}
              aria-valuemin={0}
              aria-valuemax={100}
              aria-label={t('downloads.row.progressLabel', { game: name, stage })}
            >
              <div className="h-full origin-left bg-ink transition-transform duration-(--dur-2)" style={{ transform: `scaleX(${Math.max(0.03, progress)})` }} />
            </div>
            <span className="data w-36 shrink-0 text-right text-13 text-ink-2">{stage}</span>
          </div>
        ) : (
          <span className={cn('flex min-w-0 items-center gap-1.5 text-13', d.stage === 'done' ? 'text-ink-2' : d.stage === 'cancelled' ? 'text-ink-3' : 'text-warn')}>
            <Icon name={d.stage === 'done' ? 'check' : d.stage === 'cancelled' ? 'x' : 'warning'} className="size-3.5 shrink-0" />
            <span className="truncate">
              <T
                k="downloads.row.statusWhen"
                values={{
                  status:
                    d.stage === 'failed'
                      ? d.error
                        ? t('downloads.row.failed', { error: d.error })
                        : t('downloads.row.failedUnknown')
                      : needsGame
                        ? t('downloads.row.notInLibrary')
                        : stage,
                  ago: format.ago(ended),
                }}
                tags={{ when: (c) => <span className="text-ink-3">{c}</span> }}
              />
            </span>
          </span>
        )}
      </div>
      {action}
    </li>
  );
}
