import { useEffect, useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { useBridgeEvent, type SessionSnapshot } from '@/bridge';
import { heroOf, setupPath, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot, useSetupSnapshot } from '@/lib/live';
import { useScreenChrome, type Hint } from '@/shell/chrome';
import { useSuspendCouchNav } from '@/shell/couch-nav';
import { Button, ButtonLink, cn, Icon, Pane, Spinner, useToast } from '@/ui';
import { EventLog } from './EventLog';
import type { SessionView } from './model';
import { ScreenPreview } from './ScreenPreview';
import { SessionHeader } from './SessionHeader';
import { ActionsPane, NotesPane } from './SidePanes';
import { useSessionCommands } from './useSessionCommands';
import { useSessionLog } from './useSessionLog';

/**
 * The running session: every copy of the game and its launch progress, the input lock, the session actions, the event
 * log and the handler notes. Host-owned state: rendered from `session.changed` snapshots. When the session ends while
 * the screen is open, its last state stays on screen with the way back to the lobby.
 */
export default function SessionScreen() {
  const { data: session, isPending } = useSessionSnapshot();
  const live = isSessionLive(session?.state);
  // While copies run, gamepads are playing the game: they must not drive the app.
  useSuspendCouchNav(live);
  useNotices();

  // The last live snapshot, kept when the session ends so the screen can say how it went.
  const [last, setLast] = useState<{ snapshot: SessionSnapshot; endedAt?: number } | null>(null);
  if (session && live && last?.snapshot !== session) setLast({ snapshot: session });
  if (session && !live && last && last.endedAt === undefined) setLast({ ...last, endedAt: Date.now() });

  const view: SessionView | null =
    session && live
      ? { snapshot: session, ended: false }
      : last?.endedAt !== undefined
        ? { snapshot: last.snapshot, ended: true, endedAt: last.endedAt, error: session?.error, errorDetails: session?.errorDetails }
        : null;
  const log = useSessionLog(view?.snapshot.startedAt);

  if (isPending) return <Loading />;
  if (!view) return <NoSession session={session} />;
  return <SessionView view={view} log={log} />;
}

/** On-screen messages of the session (hotkeys, launch steps) mirrored as toasts. */
function useNotices() {
  const toast = useToast();
  useBridgeEvent('session.notice', ({ text, durationMs }) =>
    toast(text.replace(/\s*\n\s*/g, ' · '), { icon: 'info', durationMs: Math.min(8000, Math.max(2000, durationMs || 0)) }),
  );
}

function SessionView({ view, log }: { view: SessionView; log: ReturnType<typeof useSessionLog> }) {
  const navigate = useNavigate();
  const s = view.snapshot;
  const commands = useSessionCommands(s);
  const { data: setup } = useSetupSnapshot();
  const { data: library = [] } = useLibrary();
  const game = library.find((g) => g.id === s.gameId);
  const backToLibrary = () => navigate('/library');

  const keyHints: Hint[] = view.ended
    ? []
    : [
        ...(s.lockInputsKey ? [{ key: s.lockInputsKey, label: s.inputsLocked ? 'Unlock inputs' : 'Lock inputs' }] : []),
        ...(s.stopHotkey ? [{ key: s.stopHotkey.replace(/\+/g, ' '), label: 'Stop session' }] : []),
      ];
  // While copies run, the pads play the game and drive nothing here: only the keyboard's keys are hinted.
  const back: Hint = view.ended ? { button: 'B', label: 'Back to library', action: backToLibrary } : { key: 'Esc', label: 'Back to library' };
  useScreenChrome([{ label: 'Library', to: '/library' }, { label: `${s.gameName ?? 'Game'} session` }], {
    base: [...(view.ended ? [{ button: 'A' as const, label: 'Select' }] : []), back, ...keyHints],
  });
  useBackKey(backToLibrary);

  return (
    <div className="grid h-full min-h-[540px] grid-cols-[minmax(0,1fr)_368px] grid-rows-[auto_minmax(0,1fr)] gap-gutter max-[1320px]:grid-cols-[minmax(0,1fr)_320px]">
      <SessionHeader view={view} cover={game?.coverUrl ?? game?.iconUrl} commands={commands} />
      <div className="flex min-h-0 min-w-0 flex-col gap-gutter">
        <div className="relative flex min-h-0 flex-1 flex-col">
          <ScreenPreview view={view} setup={setup} art={game ? heroOf(game) : undefined} />
          {view.ended && <EndedOverlay view={view} />}
        </div>
        <EventLog entries={log} />
      </div>
      <div className="flex min-h-0 min-w-0 flex-col gap-gutter">
        <NotesPane gameId={s.gameId} />
        <ActionsPane view={view} commands={commands} />
      </div>
    </div>
  );
}

/** Escape / Backspace go back to the library (not while typing or in a dialog). The session hotkeys belong to the host. */
function useBackKey(back: () => void) {
  const live = useRef(back);
  live.current = back;
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      const t = e.target as HTMLElement | null;
      if (t?.closest('input, textarea, select, [contenteditable="true"]') || document.querySelector('dialog[open]')) return;
      if ((e.key === 'Escape' || e.key === 'Backspace') && !e.ctrlKey && !e.altKey && !e.metaKey) {
        e.preventDefault();
        live.current();
      }
    };
    document.addEventListener('keydown', onKey);
    return () => document.removeEventListener('keydown', onKey);
  }, []);
}

function EndedOverlay({ view }: { view: SessionView }) {
  const primary = useRef<HTMLAnchorElement>(null);
  useEffect(() => {
    primary.current?.focus({ preventScroll: true });
  }, []);
  const s = view.snapshot;
  const n = s.instances.length;
  const failed = Boolean(view.error);
  return (
    <div className="absolute inset-0 z-10 grid animate-fade-in place-items-center overflow-auto rounded-pane bg-[rgba(7,8,10,0.88)] p-6 text-center">
      <div className={cn('grid justify-items-center gap-1.5', view.errorDetails ? 'w-full max-w-[72ch]' : 'max-w-[52ch]')}>
        {failed && <Icon name="warning" className="mb-1 size-7 text-danger" />}
        <h3 className="text-22 leading-tight font-[780] stretch-wide">{failed ? 'The session could not start' : 'Session ended'}</h3>
        <p className="mb-3 text-small text-ink-2">
          {failed
            ? view.error
            : `${n === 1 ? 'The copy' : `All ${n} copies`} of ${s.gameName ?? 'the game'} ${n === 1 ? 'is' : 'are'} closed. The setup is still open: play again with the same players.`}
        </p>
        {failed && view.errorDetails && <ErrorDetails text={view.errorDetails} className="mb-3 justify-items-center" />}
        <div className="flex justify-center gap-2">
          {s.gameId && (
            <ButtonLink ref={primary} variant="primary" to={setupPath(s.gameId)} data-nav>
              <Icon name="users" />
              Back to the lobby
            </ButtonLink>
          )}
          <ButtonLink variant="ghost" to="/library" data-nav>
            <Icon name="library" />
            Library
          </ButtonLink>
        </div>
      </div>
    </div>
  );
}

/** The crash report of a failed launch, behind "Show details". */
function ErrorDetails({ text, className }: { text: string; className?: string }) {
  const [open, setOpen] = useState(false);
  return (
    <div className={cn('grid w-full justify-items-start gap-2', className)}>
      <Button variant="quiet" size="sm" aria-expanded={open} onClick={() => setOpen((o) => !o)} data-nav>
        <Icon name={open ? 'chev-d' : 'chev-r'} />
        {open ? 'Hide details' : 'Show details'}
      </Button>
      {open && (
        <pre className="max-h-[40vh] w-full overflow-auto rounded-ctl bg-pane-2 px-3.5 py-3 text-left font-mono text-12 leading-normal whitespace-pre-wrap text-ink-2 wrap-anywhere">
          {text}
        </pre>
      )}
    </div>
  );
}

function Loading() {
  return (
    <Pane className="grid h-full place-items-center text-small text-ink-2">
      <span className="inline-flex items-center gap-2.5">
        <Spinner /> Loading the session
      </span>
    </Pane>
  );
}

/** No session: send the user to the lobby they were setting up, or to the library. */
function NoSession({ session }: { session: SessionSnapshot | undefined }) {
  const navigate = useNavigate();
  const { data: library = [] } = useLibrary();
  const recent = [...library]
    .filter((g) => g.lastPlayedAt && g.executableFound)
    .sort((a, b) => (b.lastPlayedAt ?? '').localeCompare(a.lastPlayedAt ?? ''))[0];
  const target =
    session?.state === 'setup' && session.gameId
      ? { id: session.gameId, name: session.gameName ?? 'the game', resume: true }
      : recent
        ? { id: recent.id, name: recent.name, resume: false }
        : null;

  useScreenChrome([{ label: 'Library', to: '/library' }, { label: 'Session' }], {
    base: [
      { button: 'A', label: 'Select' },
      { button: 'B', label: 'Back to library', action: () => navigate('/library') },
    ],
  });

  return (
    <Pane as="section" aria-labelledby="no-session-title" className="grid h-full place-items-center p-8">
      <div className="grid max-w-[52ch] justify-items-start gap-4">
        <div
          aria-hidden="true"
          className="grid aspect-[16/10] w-[132px] grid-cols-2 grid-rows-2 gap-1 rounded-xl bg-bezel p-1 shadow-[0_0_0_1px_var(--color-line)]"
        >
          <i className="rounded-[5px] bg-pane-3" />
          <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
          <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
          <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
        </div>
        <h1 id="no-session-title" className="type-h2">
          No session running
        </h1>
        {session?.error ? (
          <>
            <p className="flex items-start gap-2 rounded-ctl bg-pane-2 px-3.5 py-3 text-15 text-ink-2">
              <Icon name="warning" className="mt-0.5 size-[18px] shrink-0 text-danger" />
              <span>
                <b className="font-[650] text-ink">The last launch failed.</b> {session.error}
              </span>
            </p>
            {session.errorDetails && <ErrorDetails text={session.errorDetails} />}
          </>
        ) : (
          <p className="text-ink-2">
            Pick a game in your library, let everyone press a button on their controller to join, then launch. Every copy of the game and
            its progress shows up here.
          </p>
        )}
        <div className="flex flex-wrap gap-2">
          {target && (
            <ButtonLink to={setupPath(target.id)} variant="primary" data-nav data-autofocus>
              <Icon name="users" />
              {target.resume ? `Back to the ${target.name} lobby` : `Set up ${target.name}`}
            </ButtonLink>
          )}
          <ButtonLink to="/library" variant={target ? 'ghost' : 'primary'} data-nav>
            <Icon name="library" />
            Go to library
          </ButtonLink>
        </div>
      </div>
    </Pane>
  );
}
