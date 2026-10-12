import type { ReactNode } from 'react';
import { bridge, useBridgeQuery, type GameDetails } from '@/bridge';
import { HandlerNotes } from '@/components/HandlerNotes';
import { useRun } from '@/lib/bridge-ui';
import { plural } from '@/lib/format';
import { inputsPhrase } from '@/lib/library';
import { Button, Dialog, Icon, Spinner } from '@/ui';

/** library.get of a game while a dialog shows it. */
function useDetails(gameId: string | null) {
  return useBridgeQuery('library.get', { gameId: gameId ?? '' }, { enabled: Boolean(gameId) });
}

function Loading({ error }: { error?: boolean }) {
  return (
    <p className="flex items-center gap-2.5 py-6 text-small text-ink-2">
      {error ? <Icon name="warning" className="size-4 text-warn" /> : <Spinner />}
      {error ? 'Nucleus Co-op could not read this game.' : 'Loading'}
    </p>
  );
}

/** The handler notes of a game (Nucleus markup). */
export function NotesDialog({ gameId, onClose }: { gameId: string | null; onClose: () => void }) {
  const { data, isError } = useDetails(gameId);
  const d = data?.summary.id === gameId ? data : undefined;
  return (
    <Dialog open={gameId !== null} onClose={onClose} title={d ? `${d.summary.name}: handler notes` : 'Handler notes'} icon="book" className="w-[min(760px,92vw)]">
      {!d ? (
        <Loading error={isError} />
      ) : (
        <div className="grid gap-4">
          <p className="text-13 text-ink-3">
            {d.handlerFile}
            {d.maintainer ? `, by ${d.maintainer}` : ''}
            {d.summary.handlerVersion !== undefined ? `, version ${d.summary.handlerVersion}` : ''}
          </p>
          {d.notes ? <HandlerNotes>{d.notes}</HandlerNotes> : <p className="text-ink-2">This handler has no notes.</p>}
        </div>
      )}
    </Dialog>
  );
}

/** Where a game lives and what its handler declares. */
export function DetailsDialog({ gameId, onClose }: { gameId: string | null; onClose: () => void }) {
  const { data, isError } = useDetails(gameId);
  const d = data?.summary.id === gameId ? data : undefined;
  return (
    <Dialog open={gameId !== null} onClose={onClose} title={d ? `${d.summary.name}: details` : 'Game details'} icon="info" className="w-[min(720px,92vw)]">
      {!d ? <Loading error={isError} /> : <DetailsBody details={d} />}
    </Dialog>
  );
}

function DetailsBody({ details: d }: { details: GameDetails }) {
  const s = d.summary;
  const version =
    s.handlerVersion === undefined
      ? 'Unknown'
      : s.updateAvailable && s.latestHandlerVersion !== undefined
        ? `${s.handlerVersion} (version ${s.latestHandlerVersion} on the Hub)`
        : String(s.handlerVersion);
  return (
    <dl className="m-0 grid border-t border-line">
      <Row label="Executable">
        <Path path={d.executablePath} missing={!s.executableFound} />
      </Row>
      <Row label="Architecture">{d.architecture === 'unknown' ? 'Unknown' : d.architecture}</Row>
      <Row label="Handler file">
        <span className="font-mono text-13">{d.handlerFile}</span>
      </Row>
      <Row label="Handler version">
        <span className="data">{version}</span>
      </Row>
      {d.maintainer && <Row label="Maintainer">{d.maintainer}</Row>}
      {d.steamId && (
        <Row label="Steam app">
          <span className="data">{d.steamId}</span>
        </Row>
      )}
      <Row label="Players">Up to {s.maxPlayers}</Row>
      <Row label="Inputs">{inputsPhrase(s.inputs)}</Row>
      <Row label="Profiles">{d.profilesEnabled ? (d.profileCount ? plural(d.profileCount, 'saved profile') : 'None saved yet') : 'Turned off'}</Row>
      <Row label="Options">{d.hasOptions ? 'The handler has options or setup steps' : 'None'}</Row>
      <Row label="Instances folder">
        <Path path={d.contentFolder} />
      </Row>
    </dl>
  );
}

function Path({ path, missing }: { path: string; missing?: boolean }) {
  const run = useRun();
  return (
    <span className="inline-flex max-w-full items-start justify-end gap-2">
      <span className={missing ? 'font-mono text-13 break-all text-warn' : 'font-mono text-13 break-all text-ink-2'}>{path || 'Not set'}</span>
      {path && !missing && (
        <Button
          variant="quiet"
          size="icon-sm"
          title="Show in File Explorer"
          aria-label="Show in File Explorer"
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
