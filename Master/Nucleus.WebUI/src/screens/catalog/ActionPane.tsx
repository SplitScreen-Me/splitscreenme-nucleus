// The handler page's action pane: install, update, add the game, or set up a session, with the download progress.
import { useState, type ReactNode } from 'react';
import { bridge } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import type { HubHandler } from '@/hub';
import { igdbImage } from '@/lib/art';
import { useRun } from '@/lib/bridge-ui';
import { cancelDownload, downloadProgress, installHandler, updateHandler, useActiveDownload, useDownloads } from '@/lib/downloads';
import { fmtCount, fmtDate, fmtNumber, plural } from '@/lib/format';
import { setupPath, useInstalledHandlers, useLibrary } from '@/lib/library';
import { useAppInfo } from '@/lib/live';
import { Button, ButtonLink, Icon, Pane, Spinner, useToast } from '@/ui';
import { stageText } from '@/screens/downloads/stage-text';
import { handlerInstallState } from './install-state';

const openExternal = (url: string) => void bridge.app.openExternal({ url }).catch(() => undefined);

export function ActionPane({ handler: h, byAuthor, onComments }: { handler: HubHandler; byAuthor: HubHandler[]; onComments: () => void }) {
  const run = useRun();
  const toast = useToast();
  const { afterInstall, addGame, locateGame } = useAddGame();
  const { data: installed = [] } = useInstalledHandlers();
  const { data: library = [] } = useLibrary();
  const { data: info } = useAppInfo();
  const downloads = useDownloads();
  const [adding, setAdding] = useState(false);

  const offline = info?.hubOnline === false;
  const { status, mine, game } = handlerInstallState(h, installed, library);
  const active = useActiveDownload({ handlerId: h.id, gameId: game?.id ?? mine?.gameId });
  // The answer of the last install of this handler: its executable names and suggested folder help the picker.
  const lastResult = [...downloads]
    .reverse()
    .find((d) => d.stage === 'done' && d.result && (d.source === h.id || (mine && d.result.gameId === mine.gameId)))?.result;

  const meta = { name: h.gameName, coverUrl: igdbImage(h.coverImageId, 'cover_big'), version: h.version };

  const install = async () => {
    const updating = status === 'update';
    const result = await run(() => (updating && game ? updateHandler(game.id, { ...meta, handlerId: h.id }) : installHandler(h.id, meta)));
    if (!result) return;
    if (result.inLibrary) {
      toast(updating ? `${h.gameName} handler updated to version ${h.version}` : `${h.gameName} handler installed`);
      return;
    }
    toast(`Handler installed. Show Nucleus where ${result.gameName} is.`, { icon: 'download' });
    setAdding(true);
    try {
      await afterInstall(result);
    } finally {
      setAdding(false);
    }
  };

  const addTheGame = async () => {
    setAdding(true);
    try {
      if (lastResult && !lastResult.inLibrary) await afterInstall(lastResult);
      else if (mine) await addGame({ gameName: mine.gameName, handlerGameId: mine.gameId });
    } finally {
      setAdding(false);
    }
  };

  const authorDownloads = byAuthor.reduce((n, x) => n + x.downloads, 0);
  const facts: Array<[string, string]> = [
    ['Version', String(h.version)],
    ['Updated', fmtDate(h.publishedAt)],
    ['Downloads', fmtNumber(h.downloads)],
    ['Stars', fmtNumber(h.stars)],
    ['Comments', fmtNumber(h.comments)],
  ];

  let primary: ReactNode;
  let note: ReactNode;
  if (active) {
    const progress = downloadProgress(active);
    primary = (
      <div className="grid gap-2" role="status" aria-live="polite">
        <Button variant="primary" size="lg" disabled className="w-full">
          <Spinner />
          {stageText(active)}
        </Button>
        <div className="flex min-h-[34px] items-center gap-3">
          <div className="h-1 flex-1 overflow-hidden rounded-full bg-pane-3" aria-hidden="true">
            <div className="h-full origin-left bg-ink transition-transform duration-(--dur-2)" style={{ transform: `scaleX(${Math.max(0.04, progress)})` }} />
          </div>
          {active.cancellable && (
            <Button variant="quiet" size="sm" onClick={() => cancelDownload(active.id)} data-nav aria-label={`Cancel the ${h.gameName} download`}>
              <Icon name="x" />
              Cancel
            </Button>
          )}
        </div>
      </div>
    );
    note = active.kind === 'update' ? 'Updating the handler. The game stays playable meanwhile.' : `Installing the ${h.gameName} handler from the Hub.`;
  } else if (adding) {
    primary = (
      <Button variant="primary" size="lg" disabled className="w-full">
        <Spinner />
        Waiting for the game file…
      </Button>
    );
    note = `Pick the ${h.gameName} executable in the window Nucleus opened.`;
  } else if (status === 'library' && game) {
    if (!game.executableFound) {
      primary = (
        <Button variant="primary" size="lg" className="w-full" data-nav onClick={() => void locateGame(game, { executableNames: lastResult?.executableNames })}>
          <Icon name="search" />
          Locate the game
        </Button>
      );
      note = `Installed, version ${mine?.version ?? game.handlerVersion ?? '?'}. Nucleus can't find ${game.name} any more: show it where the game is.`;
    } else {
      primary = (
        <ButtonLink to={setupPath(game.id)} variant="primary" size="lg" className="w-full" data-nav>
          <Icon name="play" />
          Set up session
        </ButtonLink>
      );
      note = `Installed, version ${mine?.version ?? game.handlerVersion ?? '?'}. ${game.name} is in your library.`;
    }
  } else if (status === 'installed' && mine) {
    primary = (
      <Button variant="primary" size="lg" className="w-full" data-install data-nav onClick={() => void addTheGame()}>
        <Icon name="plus" />
        Add the game
      </Button>
    );
    note = `The handler is installed${mine.version !== undefined ? ` (version ${mine.version})` : ''}, but ${mine.gameName} is not in your library yet. Show Nucleus where the game is.`;
  } else {
    const updating = status === 'update';
    primary = (
      <Button variant="primary" size="lg" className="w-full" data-install data-nav disabled={offline} onClick={() => void install()}>
        <Icon name={updating ? 'update' : 'download'} />
        {updating ? `Update to version ${h.version}` : 'Install handler'}
      </Button>
    );
    note = offline ? (
      <span className="flex items-start gap-1.5">
        <Icon name="offline" className="mt-0.5 size-4 shrink-0 text-warn" />
        The Hub is offline. Installing works again as soon as Nucleus reaches it.
      </span>
    ) : updating ? (
      `Version ${mine?.version ?? '?'} is installed. The update keeps your profiles and settings.`
    ) : (
      `Downloads the handler from the Hub and adds ${h.gameName} to your library. Nucleus asks once where the game is installed.`
    );
  }

  return (
    <Pane className="grid gap-4 p-6">
      {primary}
      <p className="-mt-1 text-small text-ink-2">{note}</p>
      <Button variant="ghost" disabled={!h.url} onClick={() => openExternal(h.url)}>
        <Icon name="external" />
        Open on the Hub
      </Button>
      <dl className="m-0 grid border-t border-line">
        {facts.map(([k, val]) => (
          <div key={k} className="flex justify-between gap-3 border-b border-line py-[11px]">
            <dt className="text-small text-ink-3">{k}</dt>
            <dd className="data m-0 text-15 font-[650]">{val}</dd>
          </div>
        ))}
      </dl>
      <button
        type="button"
        disabled={!h.authorUrl}
        onClick={() => h.authorUrl && openExternal(h.authorUrl)}
        className="group flex items-center gap-3 rounded-ctl border-0 bg-transparent py-1 text-left text-ink"
      >
        <span aria-hidden="true" className="grid size-10 shrink-0 place-items-center rounded-full bg-pane-3 font-[750] stretch-wide">
          {h.author[0]?.toUpperCase()}
        </span>
        <span>
          <b className="block font-[650] group-hover:underline group-hover:underline-offset-3">{h.author}</b>
          <small className="block text-small text-ink-3">
            {plural(byAuthor.length || 1, 'handler')}, {fmtCount(authorDownloads || h.downloads)} downloads
          </small>
        </span>
      </button>
      <p className="text-micro text-ink-3">
        Handler not working after a game update?{' '}
        <button type="button" onClick={onComments} className="border-0 bg-transparent p-0 text-ink-2 underline underline-offset-2 hover:text-ink">
          Tell the author in the comments
        </button>
        .
      </p>
    </Pane>
  );
}
