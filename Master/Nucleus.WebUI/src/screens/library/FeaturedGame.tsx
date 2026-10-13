import { useState } from 'react';
import type { GameSummary } from '@/bridge';
import { useI18n, type TFunction } from '@/i18n';
import { useActiveDownload, downloadProgress } from '@/lib/downloads';
import { gameStatus, heroOf, setupPath } from '@/lib/library';
import { Button, ButtonLink, Icon, Pane, Spinner } from '@/ui';
import { inputsPhrase } from './library-view';

export interface FeaturedGameProps {
  game: GameSummary;
  onOptions: () => void;
  onLocate: () => void;
  onUpdate: () => void;
}

/** "Handler version 40", "Handler version 9, version 10 on the Hub". */
export function handlerLine(t: TFunction, game: GameSummary): string {
  if (game.handlerVersion === undefined) return game.handlerId ? t('library.handlerLine.hub') : t('library.handlerLine.fromFile');
  const version = String(game.handlerVersion);
  return game.updateAvailable && game.latestHandlerVersion !== undefined
    ? t('library.handlerLine.versionWithLatest', { version, latest: String(game.latestHandlerVersion) })
    : t('library.handlerLine.version', { version });
}

/** The game you played last, one press away from a session. */
export function FeaturedGame({ game, onOptions, onLocate, onUpdate }: FeaturedGameProps) {
  const { t, format } = useI18n();
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
            {t('library.upToPlayers', { count: game.maxPlayers })}
          </li>
          <li className="inline-flex items-center gap-[7px]">
            <Icon name="pad" className="size-[17px] text-ink-3" />
            {inputsPhrase(t, game.inputs)}
          </li>
          <li className="inline-flex items-center gap-[7px]">
            <Icon name={status === 'update' ? 'update' : 'seal'} className={status === 'update' ? 'size-[17px] text-warn' : 'size-[17px] text-ink-3'} />
            {handlerLine(t, game)}
          </li>
        </ul>
        <div className="mb-[26px] flex flex-wrap gap-2.5">
          <ButtonLink to={setupPath(game.id)} variant="primary" size="lg" data-nav data-hints="setup" data-id={game.id} data-autofocus>
            <Icon name="play" />
            {t('library.featured.setUpSession')}
          </ButtonLink>
          <Button variant="ghost" size="lg" data-nav data-hints="options" data-id={game.id} onClick={onOptions}>
            <Icon name="sliders" />
            {t('library.featured.gameOptions')}
          </Button>
          {status === 'missing' && (
            <Button variant="ghost" size="lg" data-nav data-hints="missing" data-id={game.id} onClick={onLocate}>
              <Icon name="search" />
              {t('library.featured.locateGame')}
            </Button>
          )}
          {status === 'update' && (
            <Button variant="ghost" size="lg" data-nav data-hints="update" data-id={game.id} onClick={onUpdate} disabled={Boolean(updating)}>
              {updating ? <Spinner /> : <Icon name="update" />}
              {updating
                ? t('library.featured.updating', { percent: format.percent(downloadProgress(updating)) })
                : t('library.featured.updateTo', { version: String(game.latestHandlerVersion) })}
            </Button>
          )}
        </div>
        {status === 'missing' ? (
          <p className="flex items-start gap-2 text-small text-ink-2">
            <Icon name="warning" className="mt-0.5 size-4 shrink-0 text-warn" />
            {t('library.featured.missing')}
          </p>
        ) : played ? (
          <dl className="m-0 flex flex-wrap gap-x-9 gap-y-2" aria-label={t('library.featured.yourPlay')}>
            <div>
              <dt className="text-13 text-ink-3">{t('library.featured.lastPlayed')}</dt>
              <dd className="m-0 text-17 font-[680] first-letter:uppercase">{format.when(game.lastPlayedAt) || t('library.featured.notRecorded')}</dd>
            </div>
            <div>
              <dt className="text-13 text-ink-3">{t('library.featured.playTime')}</dt>
              <dd className="data m-0 text-17 font-[680]">{game.playtimeSeconds > 0 ? format.duration(game.playtimeSeconds) : t('library.featured.underAMinute')}</dd>
            </div>
          </dl>
        ) : (
          <p className="text-small text-ink-2">{t('library.featured.notPlayed')}</p>
        )}
      </div>
    </Pane>
  );
}
