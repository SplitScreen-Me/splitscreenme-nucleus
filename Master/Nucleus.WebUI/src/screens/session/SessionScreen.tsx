import { useEffect, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import { useBridgeEvent, type SessionLogEntry, type SessionSnapshot, type SessionSummary } from '@/bridge';
import { T, useT } from '@/i18n';
import { heroOf, setupPath, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot, useSetupSnapshot } from '@/lib/live';
import { useScreenChrome, type Hint } from '@/shell/chrome';
import { useSuspendCouchNav } from '@/shell/couch-nav';
import { ButtonLink, cn, Icon, Pane, Spinner, useToast } from '@/ui';
import { EventLog } from './EventLog';
import type { SessionView } from './model';
import { ScreenPreview } from './ScreenPreview';
import { ErrorDetails, SessionSummaryCard } from './SessionSummary';
import { SessionHeader } from './SessionHeader';
import { ActionsPane, NotesPane } from './SidePanes';
import { useSessionCommands } from './useSessionCommands';
import { useSessionLog } from './useSessionLog';

/**
 * The running session: every copy of the game and its launch progress, the input lock, the session actions, the event
 * log and the handler notes. Host-owned state: rendered from `session.changed` snapshots. When the session ends while
 * the screen is open, its last state stays under the host's summary of it; later, the summary shows on its own until
 * the next session starts.
 */
export default function SessionScreen() {
  const { data: session, isPending } = useSessionSnapshot();
  const live = isSessionLive(session?.state);
  // While copies run, gamepads are playing the game: they must not drive the app.
  useSuspendCouchNav(live);
  useNotices();

  // The last live snapshot, kept when the session ends so the screen keeps the windows under the summary.
  const [last, setLast] = useState<{ snapshot: SessionSnapshot; endedAt?: number } | null>(null);
  if (session && live && last?.snapshot !== session) setLast({ snapshot: session });
  if (session && !live && last && last.endedAt === undefined) {
    const hostEnd = session.lastSession ? Date.parse(session.lastSession.endedAt) : NaN;
    setLast({ ...last, endedAt: Number.isNaN(hostEnd) ? Date.now() : hostEnd });
  }

  const view: SessionView | null =
    session && live
      ? { snapshot: session, ended: false }
      : last?.endedAt !== undefined
        ? {
            snapshot: last.snapshot,
            ended: true,
            endedAt: last.endedAt,
            error: session?.lastSession?.error ?? session?.error,
            errorDetails: session?.errorDetails,
            summary: session?.lastSession,
          }
        : null;
  const log = useSessionLog(view?.snapshot.startedAt ?? session?.lastSession?.startedAt);

  if (isPending) return <Loading />;
  if (!view) return <NoSession session={session} log={log} />;
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
  const t = useT();
  const s = view.snapshot;
  const commands = useSessionCommands(s);
  const { data: setup } = useSetupSnapshot();
  const { data: library = [] } = useLibrary();
  const game = library.find((g) => g.id === s.gameId);
  const backToLibrary = () => navigate('/library');

  const keyHints: Hint[] = view.ended
    ? []
    : [
        ...(s.lockInputsKey
          ? [{ key: s.lockInputsKey, label: s.inputsLocked ? t('session.hints.unlockInputs') : t('session.hints.lockInputs') }]
          : []),
        ...(s.stopHotkey ? [{ key: s.stopHotkey.replace(/\+/g, ' '), label: t('session.hints.stop') }] : []),
      ];
  // While copies run, the pads play the game and drive nothing here: only the keyboard's keys are hinted.
  const back: Hint = view.ended
    ? { button: 'B', label: t('session.hints.backToLibrary'), action: backToLibrary }
    : { key: 'Esc', label: t('session.hints.backToLibrary') };
  const crumb = s.gameName ? t('session.chrome.gameSession', { game: s.gameName }) : t('session.chrome.unknownGameSession');
  useScreenChrome([{ label: t('session.chrome.library'), to: '/library' }, { label: crumb }], {
    base: [...(view.ended ? [{ button: 'A' as const, label: t('common.select') }] : []), back, ...keyHints],
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
  if (view.summary) {
    return (
      <div className="absolute inset-0 z-10 grid animate-fade-in place-items-center overflow-auto rounded-pane bg-[rgba(7,8,10,0.92)] p-6">
        <SessionSummaryCard summary={view.summary} errorDetails={view.errorDetails} />
      </div>
    );
  }
  return <EndedNotice view={view} />;
}

/** The end of a session without the host's summary: what the screen saw. */
function EndedNotice({ view }: { view: SessionView }) {
  const t = useT();
  const primary = useRef<HTMLAnchorElement>(null);
  useEffect(() => {
    primary.current?.focus({ preventScroll: true });
  }, []);
  const s = view.snapshot;
  const count = s.instances.length;
  const failed = Boolean(view.error);
  const closed = s.gameName
    ? t('session.ended.copiesClosed', { count, game: s.gameName })
    : t('session.ended.copiesClosedUnknownGame', { count });
  return (
    <div className="absolute inset-0 z-10 grid animate-fade-in place-items-center overflow-auto rounded-pane bg-[rgba(7,8,10,0.88)] p-6 text-center">
      <div className={cn('grid justify-items-center gap-1.5', view.errorDetails ? 'w-full max-w-[72ch]' : 'max-w-[52ch]')}>
        {failed && <Icon name="warning" className="mb-1 size-7 text-danger" />}
        <h3 className="text-22 leading-tight font-[780] stretch-wide">
          {failed ? t('session.outcome.failed') : t('session.outcome.ended')}
        </h3>
        <p className="mb-3 text-small text-ink-2">{failed ? view.error : closed}</p>
        {failed && view.errorDetails && <ErrorDetails text={view.errorDetails} className="mb-3 justify-items-center" />}
        <div className="flex justify-center gap-2">
          {s.gameId && (
            <ButtonLink ref={primary} variant="primary" to={setupPath(s.gameId)} data-nav>
              <Icon name="users" />
              {t('session.links.lobby')}
            </ButtonLink>
          )}
          <ButtonLink variant="ghost" to="/library" data-nav>
            <Icon name="library" />
            {t('session.links.library')}
          </ButtonLink>
        </div>
      </div>
    </div>
  );
}

function Loading() {
  const t = useT();
  return (
    <Pane className="grid h-full place-items-center text-small text-ink-2">
      <span className="inline-flex items-center gap-2.5">
        <Spinner /> {t('session.loading')}
      </span>
    </Pane>
  );
}

/** No session: send the user to the lobby they were setting up, or to the library. */
function NoSession({ session, log }: { session: SessionSnapshot | undefined; log: SessionLogEntry[] }) {
  if (session?.lastSession) return <LastSession summary={session.lastSession} errorDetails={session.errorDetails} log={log} />;
  return <Idle session={session} />;
}

/** No session running, but one ended: how it went, its log, and the way back. */
function LastSession({ summary, errorDetails, log }: { summary: SessionSummary; errorDetails?: string; log: SessionLogEntry[] }) {
  const navigate = useNavigate();
  const t = useT();
  const { data: library = [] } = useLibrary();
  const game = library.find((g) => g.id === summary.gameId);
  useScreenChrome(
    [{ label: t('session.chrome.library'), to: '/library' }, { label: t('session.chrome.gameSession', { game: summary.gameName }) }],
    {
      base: [
        { button: 'A', label: t('common.select') },
        { button: 'B', label: t('session.hints.backToLibrary'), action: () => navigate('/library') },
      ],
    },
  );
  useBackKey(() => navigate('/library'));
  const art = game ? heroOf(game) : undefined;

  return (
    <div className="flex h-full min-h-[540px] flex-col gap-gutter">
      <Pane as="div" className="relative grid min-h-0 flex-1 place-items-center overflow-auto p-8">
        {art && (
          <div
            aria-hidden="true"
            className="pointer-events-none absolute inset-0 bg-cover bg-center opacity-[0.08] [mask-image:linear-gradient(to_bottom,black,transparent_75%)]"
            style={{ backgroundImage: `url("${art}")` }}
          />
        )}
        <SessionSummaryCard summary={summary} errorDetails={errorDetails} className="relative" />
      </Pane>
      <EventLog entries={log} />
    </div>
  );
}

const FAILED_TAGS = { b: (c: ReactNode) => <b className="font-[650] text-ink">{c}</b> };

function Idle({ session }: { session: SessionSnapshot | undefined }) {
  const navigate = useNavigate();
  const t = useT();
  const { data: library = [] } = useLibrary();
  const recent = [...library]
    .filter((g) => g.lastPlayedAt && g.executableFound)
    .sort((a, b) => (b.lastPlayedAt ?? '').localeCompare(a.lastPlayedAt ?? ''))[0];
  const target =
    session?.state === 'setup' && session.gameId
      ? {
          id: session.gameId,
          label: session.gameName ? t('session.links.gameLobby', { game: session.gameName }) : t('session.links.lobby'),
        }
      : recent
        ? { id: recent.id, label: t('session.links.setUpGame', { game: recent.name }) }
        : null;

  useScreenChrome([{ label: t('session.chrome.library'), to: '/library' }, { label: t('session.chrome.session') }], {
    base: [
      { button: 'A', label: t('common.select') },
      { button: 'B', label: t('session.hints.backToLibrary'), action: () => navigate('/library') },
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
          {t('session.idle.title')}
        </h1>
        {session?.error ? (
          <>
            <p className="flex items-start gap-2 rounded-ctl bg-pane-2 px-3.5 py-3 text-15 text-ink-2">
              <Icon name="warning" className="mt-0.5 size-[18px] shrink-0 text-danger" />
              <span>
                <T k="session.idle.lastLaunchFailed" values={{ error: session.error }} tags={FAILED_TAGS} />
              </span>
            </p>
            {session.errorDetails && <ErrorDetails text={session.errorDetails} />}
          </>
        ) : (
          <p className="text-ink-2">{t('session.idle.hint')}</p>
        )}
        <div className="flex flex-wrap gap-2">
          {target && (
            <ButtonLink to={setupPath(target.id)} variant="primary" data-nav data-autofocus>
              <Icon name="users" />
              {target.label}
            </ButtonLink>
          )}
          <ButtonLink to="/library" variant={target ? 'ghost' : 'primary'} data-nav>
            <Icon name="library" />
            {t('session.links.goToLibrary')}
          </ButtonLink>
        </div>
      </div>
    </Pane>
  );
}
