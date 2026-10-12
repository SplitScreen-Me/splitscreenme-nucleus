import { useMemo, useState, type ReactNode } from 'react';
import { Link } from 'react-router';
import { bridge, type GameSummary } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { useAllHandlers, type HubHandler } from '@/hub';
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
import { fmtAgo, plural } from '@/lib/format';
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

function resolve(item: DownloadItem, library: readonly GameSummary[], hub: readonly HubHandler[]): Resolved {
  const gameId = item.gameId ?? item.result?.gameId;
  const game = gameId ? library.find((g) => g.id === gameId) : library.find((g) => g.handlerId === item.source);
  const handler = hub.find((h) => h.id === item.source || (game?.handlerId !== undefined && h.id === game.handlerId));
  const fileName = item.kind === 'file' ? item.source.slice(Math.max(item.source.lastIndexOf('\\'), item.source.lastIndexOf('/')) + 1) : undefined;
  return {
    name: item.result?.gameName ?? item.name ?? game?.name ?? handler?.gameName ?? fileName ?? 'Handler',
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
  const [updatingAll, setUpdatingAll] = useState(false);
  const [checking, setChecking] = useState(false);
  const offline = info?.hubOnline === false;

  const outdated = useMemo(() => library.filter((g) => g.updateAvailable).sort((a, b) => a.name.localeCompare(b.name, 'en')), [library]);
  const newestFirst = useMemo(() => [...items].reverse(), [items]);
  const active = newestFirst.filter((d) => isActiveStage(d.stage));
  const finished = newestFirst.filter((d) => !isActiveStage(d.stage));

  const update = (g: GameSummary) =>
    run(() => updateHandler(g.id, { name: g.name, coverUrl: g.coverUrl, handlerId: g.handlerId, version: g.latestHandlerVersion }), {
      success: `${g.name} handler updated${g.latestHandlerVersion ? ` to version ${g.latestHandlerVersion}` : ''}`,
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
    if (done) toast('Checked the Hub for handler updates', { icon: 'update' });
  };

  useScreenChrome([{ label: 'Downloads' }], {
    base: [
      { button: 'A', label: 'Select' },
      ...(outdated.length && !offline ? [{ button: 'X' as const, label: 'Update all', action: () => void updateAll() }] : []),
      { button: 'Y', label: 'Search' },
    ],
  });

  return (
    <div className="grid content-start gap-gutter">
      <Pane as="header" className="flex flex-wrap items-end gap-6 px-8 pt-8 pb-7">
        <div className="grid gap-2">
          <h1 className="type-h1">Downloads</h1>
          <p className="max-w-[60ch] text-ink-2">Handlers you install or update from the Hub. Games stay playable while their handler updates.</p>
        </div>
        <div className="ml-auto flex flex-wrap gap-2">
          <Button variant="ghost" onClick={() => void checkUpdates()} disabled={checking || offline} data-nav title={offline ? 'The Hub is offline' : undefined}>
            {checking ? <Spinner /> : <Icon name="refresh" />}
            Check for updates
          </Button>
          {finished.length > 0 && (
            <Button variant="ghost" onClick={clearFinishedDownloads} data-nav>
              <Icon name="check" />
              Clear finished
            </Button>
          )}
        </div>
      </Pane>

      {outdated.length > 0 && (
        <Pane as="section" aria-labelledby="updates-title" className="px-6 pt-6 pb-4">
          <div className="mb-3 flex items-center gap-3">
            <h2 id="updates-title" className="type-h3">
              Updates available
            </h2>
            <span className="data text-15 text-ink-3">{outdated.length}</span>
            <Button size="sm" className="ml-auto" onClick={() => void updateAll()} disabled={updatingAll || offline} data-nav>
              {updatingAll ? <Spinner /> : <Icon name="update" />}
              {updatingAll ? 'Updating…' : 'Update all'}
            </Button>
          </div>
          {offline && (
            <p className="mb-2 flex items-center gap-2 text-13 text-ink-2">
              <Icon name="offline" className="size-4 text-warn" />
              The Hub is offline. Updates work again as soon as Nucleus reaches it.
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
            {active.length ? 'In progress' : 'Recent downloads'}
          </h2>
          {items.length > 0 && <span className="data text-15 text-ink-3">{plural(items.length, 'item')}</span>}
        </div>
        {items.length === 0 ? (
          <div className="grid justify-items-start gap-4 py-8">
            <p className="max-w-[56ch] text-ink-2">
              Nothing downloaded since Nucleus Co-op started. Handlers you install from Get games, or drop on the window as .nc files, show up here with their progress.
            </p>
            <ButtonLink to="/games" variant="primary" data-nav>
              <Icon name="compass" />
              Get games
            </ButtonLink>
          </div>
        ) : (
          <ul className="m-0 list-none p-0" aria-live="polite">
            {[...active, ...finished].map((d) => (
              <DownloadRow key={d.id} item={d} resolved={resolve(d, library, hub)} />
            ))}
          </ul>
        )}
      </Pane>
    </div>
  );
}

function UpdateRow({ game: g, disabled, onUpdate }: { game: GameSummary; disabled: boolean; onUpdate: () => void }) {
  const active = useActiveDownload({ gameId: g.id, handlerId: g.handlerId });
  return (
    <li className="grid grid-cols-[40px_minmax(0,1fr)_auto] items-center gap-4 border-t border-line py-3 first:border-t-0">
      <Cover src={g.coverUrl} title={g.name} ratio="3/4" className="w-10 rounded-md" />
      <div className="min-w-0">
        <b className="block truncate text-15 font-[650]">{g.name}</b>
        {active ? (
          <span className="data text-13 text-ink-2">{stageText(active)}</span>
        ) : (
          <span className="data text-13 text-warn">
            Handler update{g.handlerVersion !== undefined ? ` v${g.handlerVersion}` : ''} to v{g.latestHandlerVersion ?? '?'}
          </span>
        )}
      </div>
      <Button size="sm" variant="ghost" disabled={Boolean(active) || disabled} onClick={onUpdate} data-nav aria-label={`Update the ${g.name} handler`}>
        {active ? <Spinner /> : <Icon name="update" />}
        Update
      </Button>
    </li>
  );
}

function DownloadRow({ item: d, resolved }: { item: DownloadItem; resolved: Resolved }) {
  const run = useRun();
  const { afterInstall } = useAddGame();
  const [adding, setAdding] = useState(false);
  const running = isActiveStage(d.stage);
  const progress = downloadProgress(d);
  const pct = Math.round(progress * 100);
  const { name, cover, game } = resolved;
  const what =
    d.kind === 'update'
      ? `Handler update${d.version ? ` to version ${d.version}` : ''}`
      : d.kind === 'file'
        ? 'From a file'
        : `Handler${d.version ? `, version ${d.version}` : ''}`;
  const needsGame = d.stage === 'done' && d.result !== undefined && !d.result.inLibrary && !game;

  const retry = () => {
    if (d.kind === 'update' && d.gameId) void run(() => updateHandler(d.gameId!, { name, coverUrl: cover, version: d.version, handlerId: resolved.hub?.id }));
    else if (d.kind === 'install') void run(() => installHandler(d.source, { name, coverUrl: cover, version: d.version }));
  };

  let action: ReactNode = <span />;
  if (running && d.cancellable) {
    action = (
      <Button size="sm" variant="ghost" onClick={() => cancelDownload(d.id)} data-nav aria-label={`Cancel the ${name} download`}>
        Cancel
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
        Add the game
      </Button>
    );
  } else if (d.stage === 'done' && game) {
    action = (
      <Link to={setupPath(game.id)} className="rounded text-small font-semibold text-ink-2 no-underline hover:text-ink" data-nav>
        Set up session
      </Link>
    );
  } else if ((d.stage === 'failed' || d.stage === 'cancelled') && d.kind !== 'file') {
    action = (
      <Button size="sm" variant="ghost" onClick={retry} data-nav aria-label={`Try the ${name} download again`}>
        <Icon name="refresh" />
        Try again
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
              aria-label={`${name}: ${stageText(d)}`}
            >
              <div className="h-full origin-left bg-ink transition-transform duration-(--dur-2)" style={{ transform: `scaleX(${Math.max(0.03, progress)})` }} />
            </div>
            <span className="data w-36 shrink-0 text-right text-13 text-ink-2">{stageText(d)}</span>
          </div>
        ) : (
          <span className={cn('flex min-w-0 items-center gap-1.5 text-13', d.stage === 'done' ? 'text-ink-2' : d.stage === 'cancelled' ? 'text-ink-3' : 'text-warn')}>
            <Icon name={d.stage === 'done' ? 'check' : d.stage === 'cancelled' ? 'x' : 'warning'} className="size-3.5 shrink-0" />
            <span className="truncate">
              {d.stage === 'failed' ? `Failed: ${d.error ?? 'the install did not finish.'}` : needsGame ? 'Installed. The game is not in your library yet.' : stageText(d)}{' '}
              <span className="text-ink-3">{fmtAgo(new Date(ended).toISOString())}</span>
            </span>
          </span>
        )}
      </div>
      {action}
    </li>
  );
}
