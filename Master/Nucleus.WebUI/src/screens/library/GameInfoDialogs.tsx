import type { ReactNode } from 'react';
import { bridge, useBridgeQuery, type GameDetails } from '@/bridge';
import { HandlerNotes } from '@/components/HandlerNotes';
import { useT, type TFunction } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { Button, Dialog, Icon, Spinner } from '@/ui';
import { inputsPhrase } from './library-view';

/** library.get of a game while a dialog shows it. */
function useDetails(gameId: string | null) {
  return useBridgeQuery('library.get', { gameId: gameId ?? '' }, { enabled: Boolean(gameId) });
}

function Loading({ error }: { error?: boolean }) {
  const t = useT();
  return (
    <p className="flex items-center gap-2.5 py-6 text-small text-ink-2">
      {error ? <Icon name="warning" className="size-4 text-warn" /> : <Spinner />}
      {error ? t('library.gameInfo.readError') : t('common.loading')}
    </p>
  );
}

/** "MyGame.js, by Someone, version 4" above the notes. */
function notesMeta(t: TFunction, d: GameDetails): string {
  const file = d.handlerFile;
  const version = d.summary.handlerVersion !== undefined ? String(d.summary.handlerVersion) : undefined;
  if (d.maintainer && version !== undefined) return t('library.notes.metaByVersion', { file, maintainer: d.maintainer, version });
  if (d.maintainer) return t('library.notes.metaBy', { file, maintainer: d.maintainer });
  if (version !== undefined) return t('library.notes.metaVersion', { file, version });
  return file;
}

/** The handler notes of a game (Nucleus markup). */
export function NotesDialog({ gameId, onClose }: { gameId: string | null; onClose: () => void }) {
  const t = useT();
  const { data, isError } = useDetails(gameId);
  const d = data?.summary.id === gameId ? data : undefined;
  return (
    <Dialog
      open={gameId !== null}
      onClose={onClose}
      title={d ? t('library.notes.title', { game: d.summary.name }) : t('library.notes.defaultTitle')}
      icon="book"
      className="w-[min(760px,92vw)]"
    >
      {!d ? (
        <Loading error={isError} />
      ) : (
        <div className="grid gap-4">
          <p className="text-13 text-ink-3">{notesMeta(t, d)}</p>
          {d.notes ? <HandlerNotes>{d.notes}</HandlerNotes> : <p className="text-ink-2">{t('library.notes.none')}</p>}
        </div>
      )}
    </Dialog>
  );
}

/** Where a game lives and what its handler declares. */
export function DetailsDialog({ gameId, onClose }: { gameId: string | null; onClose: () => void }) {
  const t = useT();
  const { data, isError } = useDetails(gameId);
  const d = data?.summary.id === gameId ? data : undefined;
  return (
    <Dialog
      open={gameId !== null}
      onClose={onClose}
      title={d ? t('library.details.title', { game: d.summary.name }) : t('library.details.defaultTitle')}
      icon="info"
      className="w-[min(720px,92vw)]"
    >
      {!d ? <Loading error={isError} /> : <DetailsBody details={d} />}
    </Dialog>
  );
}

function DetailsBody({ details: d }: { details: GameDetails }) {
  const t = useT();
  const s = d.summary;
  const version =
    s.handlerVersion === undefined
      ? t('common.unknown')
      : s.updateAvailable && s.latestHandlerVersion !== undefined
        ? t('library.details.versionWithLatest', { version: String(s.handlerVersion), latest: String(s.latestHandlerVersion) })
        : String(s.handlerVersion);
  return (
    <dl className="m-0 grid border-t border-line">
      <Row label={t('library.details.executable')}>
        <Path path={d.executablePath} missing={!s.executableFound} />
      </Row>
      <Row label={t('library.details.architecture')}>{d.architecture === 'unknown' ? t('common.unknown') : d.architecture}</Row>
      <Row label={t('library.details.handlerFile')}>
        <span className="font-mono text-13">{d.handlerFile}</span>
      </Row>
      <Row label={t('library.details.handlerVersion')}>
        <span className="data">{version}</span>
      </Row>
      {d.maintainer && <Row label={t('library.details.maintainer')}>{d.maintainer}</Row>}
      {d.steamId && (
        <Row label={t('library.details.steamApp')}>
          <span className="data">{d.steamId}</span>
        </Row>
      )}
      <Row label={t('library.details.players')}>{t('library.details.playersValue', { count: s.maxPlayers })}</Row>
      <Row label={t('library.details.inputs')}>{inputsPhrase(t, s.inputs)}</Row>
      <Row label={t('library.details.profiles')}>
        {d.profilesEnabled
          ? d.profileCount
            ? t('library.details.profileCount', { count: d.profileCount })
            : t('library.details.noProfiles')
          : t('library.details.profilesOff')}
      </Row>
      <Row label={t('library.details.options')}>{d.hasOptions ? t('library.details.hasOptions') : t('common.none')}</Row>
      <Row label={t('library.details.instancesFolder')}>
        <Path path={d.contentFolder} />
      </Row>
    </dl>
  );
}

function Path({ path, missing }: { path: string; missing?: boolean }) {
  const t = useT();
  const run = useRun();
  return (
    <span className="inline-flex max-w-full items-start justify-end gap-2">
      <span className={missing ? 'font-mono text-13 break-all text-warn' : 'font-mono text-13 break-all text-ink-2'}>{path || t('library.details.notSet')}</span>
      {path && !missing && (
        <Button
          variant="quiet"
          size="icon-sm"
          title={t('library.details.showInExplorer')}
          aria-label={t('library.details.showInExplorer')}
          onClick={() => void run(() => bridge.app.openPath({ path }))}
        >
          <Icon name="folder" />
        </Button>
      )}
    </span>
  );
}

function Row({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div className="flex items-start justify-between gap-6 border-b border-line py-[11px]">
      <dt className="shrink-0 pt-0.5 text-small text-ink-3">{label}</dt>
      <dd className="m-0 min-w-0 text-right text-15 font-[600]">{children}</dd>
    </div>
  );
}
