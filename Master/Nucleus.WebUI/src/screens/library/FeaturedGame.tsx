import { useState } from 'react';
import type { GameSummary } from '@/bridge';
import { useActiveDownload, downloadProgress } from '@/lib/downloads';
import { fmtDuration, fmtWhen } from '@/lib/format';
import { gameStatus, heroOf, inputsPhrase, setupPath } from '@/lib/library';
import { Button, ButtonLink, Icon, Pane, Spinner } from '@/ui';

export interface FeaturedGameProps {
  game: GameSummary;
  onOptions: () => void;
  onLocate: () => void;
  onUpdate: () => void;
}

/** "Handler version 40", "Handler version 9, version 10 on the Hub". */
export function handlerLine(game: GameSummary): string {
  if (game.handlerVersion === undefined) return game.handlerId ? 'Hub handler' : 'Handler installed from a file';
  const latest = game.updateAvailable && game.latestHandlerVersion !== undefined ? `, version ${game.latestHandlerVersion} on the Hub` : '';
  return `Handler version ${game.handlerVersion}${latest}`;
}

/** The game you played last, one press away from a session. */
export function FeaturedGame({ game, onOptions, onLocate, onUpdate }: FeaturedGameProps) {
  const [artFailed, setArtFailed] = useState(false);
  const art = !artFailed ? heroOf(game) : undefined;
  const status = gameStatus(game);
  const updating = useActiveDownload({ gameId: game.id });
  const played = Boolean(game.lastPlayedAt) || game.playtimeSeconds > 0;

  return (
    <Pane as="section" aria-labelledby="feat-title" className="isolate flex min-h-[372px] items-end">
      {art && (
        <img
          src={art}
          alt=""
          onError={() => setArtFailed(true)}
          className="absolute inset-0 -z-20 size-full object-cover object-[70%_30%]"
        />
      )}
      <div
        aria-hidden="true"
        className="absolute inset-0 -z-10"
        style={{
          background:
            'linear-gradient(90deg, var(--color-pane) 0%, rgba(20,23,30,0.88) 34%, rgba(20,23,30,0.25) 68%, rgba(20,23,30,0) 100%), linear-gradient(0deg, rgba(20,23,30,0.9) 0%, rgba(20,23,30,0) 45%)',
        }}
      />
      <div className="max-w-[660px] px-9 pt-8 pb-[30px]">
        <h1 id="feat-title" className="type-display mb-3.5 text-[clamp(2.75rem,5.2vw,4.75rem)] [overflow-wrap:anywhere]">
          {game.name}
        </h1>
        <ul className="m-0 mb-[22px] flex list-none flex-wrap gap-x-5 gap-y-1.5 p-0 text-small text-ink-2">
          <li className="inline-flex items-center gap-[7px]">
            <Icon name="users" className="size-[17px] text-ink-3" />
            Up to {game.maxPlayers} players
          </li>
          <li className="inline-flex items-center gap-[7px]">
            <Icon name="pad" className="size-[17px] text-ink-3" />
            {inputsPhrase(game.inputs)}
          </li>
          <li className="inline-flex items-center gap-[7px]">
            <Icon name={status === 'update' ? 'update' : 'seal'} className={status === 'update' ? 'size-[17px] text-warn' : 'size-[17px] text-ink-3'} />
            {handlerLine(game)}
          </li>
        </ul>
        <div className="mb-[26px] flex flex-wrap gap-2.5">
          <ButtonLink to={setupPath(game.id)} variant="primary" size="lg" data-nav data-hints="setup" data-id={game.id} data-autofocus>
            <Icon name="play" />
            Set up session
          </ButtonLink>
          <Button variant="ghost" size="lg" data-nav data-hints="options" data-id={game.id} onClick={onOptions}>
            <Icon name="sliders" />
            Game options
          </Button>
          {status === 'missing' && (
            <Button variant="ghost" size="lg" data-nav data-hints="missing" data-id={game.id} onClick={onLocate}>
              <Icon name="search" />
              Locate game
            </Button>
          )}
          {status === 'update' && (
            <Button variant="ghost" size="lg" data-nav data-hints="update" data-id={game.id} onClick={onUpdate} disabled={Boolean(updating)}>
              {updating ? <Spinner /> : <Icon name="update" />}
              {updating ? `Updating ${Math.round(downloadProgress(updating) * 100)}%` : `Update to v${game.latestHandlerVersion}`}
            </Button>
          )}
        </div>
        {status === 'missing' ? (
          <p className="flex items-start gap-2 text-small text-ink-2">
            <Icon name="warning" className="mt-0.5 size-4 shrink-0 text-warn" />
            Nucleus can't find this game's executable. It may have moved: locate it to play again.
          </p>
        ) : played ? (
          <dl className="m-0 flex flex-wrap gap-x-9 gap-y-2" aria-label="Your play">
            <div>
              <dt className="text-13 text-ink-3">Last played</dt>
              <dd className="m-0 text-17 font-[680] first-letter:uppercase">{fmtWhen(game.lastPlayedAt) || 'Not recorded'}</dd>
            </div>
            <div>
              <dt className="text-13 text-ink-3">Play time</dt>
              <dd className="data m-0 text-17 font-[680]">{game.playtimeSeconds > 0 ? fmtDuration(game.playtimeSeconds) : 'Under a minute'}</dd>
            </div>
          </dl>
        ) : (
          <p className="text-small text-ink-2">Not played yet. Hand out the controllers: everyone presses Start to join.</p>
        )}
      </div>
    </Pane>
  );
}
