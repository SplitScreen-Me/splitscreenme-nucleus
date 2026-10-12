import { useState } from 'react';
import { Link } from 'react-router';
import { useBridgeQuery, type SessionActionInfo, type SessionAction } from '@/bridge';
import { HandlerNotes } from '@/components/HandlerNotes';
import { setupPath } from '@/lib/library';
import { cn, Icon, Pane, Spinner, type IconName } from '@/ui';
import { Keys } from './SessionHeader';
import type { SessionView } from './model';
import type { SessionCommands } from './useSessionCommands';

/** The handler notes: how to connect the copies in game, written by the handler author. */
export function NotesPane({ gameId }: { gameId?: string }) {
  const { data: details, isPending, isError } = useBridgeQuery('library.get', { gameId: gameId ?? '' }, { enabled: Boolean(gameId) });
  const notes = details?.notes?.trim();
  return (
    <Pane
      as="section"
      aria-labelledby="notes-title"
      className="flex min-h-[150px] flex-1 flex-col px-5 pt-5 pb-[18px] [@media(max-height:820px)]:px-[18px] [@media(max-height:820px)]:pt-4 [@media(max-height:820px)]:pb-3.5"
    >
      <h2 id="notes-title" className="type-h3 mb-1">
        Handler notes
      </h2>
      <p className="mb-3.5 text-13 text-ink-3">
        {details?.maintainer ? `How to connect the copies, by ${details.maintainer}` : 'How to connect the copies, from the handler author'}
      </p>
      <div className="min-h-0 flex-1 overflow-y-auto pr-1 text-small" tabIndex={notes ? 0 : undefined} aria-label={notes ? 'Handler notes text' : undefined}>
        {gameId && isPending ? (
          <p className="flex items-center gap-2 text-ink-2">
            <Spinner /> Reading the notes…
          </p>
        ) : notes ? (
          <HandlerNotes className="text-small leading-[1.55]">{notes}</HandlerNotes>
        ) : (
          <p className="text-ink-2">
            {isError
              ? 'The notes could not be read.'
              : 'The handler author left no notes. Usually one player hosts a co-op game and the others join it from the in-game menu.'}
          </p>
        )}
      </div>
      {gameId && (
        <Link
          to={setupPath(gameId)}
          className="mt-3 inline-flex items-center gap-1.5 self-start rounded text-small font-semibold text-ink-2 no-underline hover:text-ink [@media(max-height:760px)]:hidden"
        >
          <Icon name="users" className="size-[15px]" />
          Open the lobby
        </Link>
      )}
    </Pane>
  );
}

const ACTION_ICON: Record<SessionAction, IconName> = {
  toggleTopmost: 'layers',
  setFocus: 'window',
  resetWindows: 'refresh',
  cutscenesMode: 'play',
  switchLayout: 'swap',
  toggleShortcutsReminder: 'keyboard',
  switchMergerForeground: 'multi',
};

function unavailableReason(a: SessionActionInfo, view: SessionView): string | undefined {
  const s = view.snapshot;
  if (view.ended) return 'The session has ended.';
  if (s.state !== 'running') return 'Available once every copy is running.';
  if (!a.available) return 'Does not apply to this session.';
  if (s.inputsLocked && a.action !== 'cutscenesMode') return `Inputs are locked: press ${s.lockInputsKey ?? 'the lock key'} first.`;
  return undefined;
}

/** Session actions, the same as their hotkeys, plus the hotkeys that only exist as keys. */
export function ActionsPane({ view, commands }: { view: SessionView; commands: SessionCommands }) {
  const s = view.snapshot;
  const [busy, setBusy] = useState<SessionAction | null>(null);
  const run = async (action: SessionAction) => {
    setBusy(action);
    await commands.runAction(action);
    setBusy(null);
  };
  const keyOnly = [
    s.lockInputsKey ? { label: 'Lock or unlock keyboards and mice', keys: s.lockInputsKey } : null,
    s.stopHotkey ? { label: 'Stop the session', keys: s.stopHotkey } : null,
  ].filter((k): k is { label: string; keys: string } => k !== null);

  return (
    <Pane
      as="section"
      aria-labelledby="actions-title"
      className="max-h-[62%] shrink-0 overflow-y-auto px-3.5 pt-3.5 pb-3 [@media(max-height:820px)]:pt-3"
    >
      <h2 id="actions-title" className="type-h3 mb-2 px-1">
        Controls
      </h2>
      <ul className="m-0 grid list-none grid-cols-1 gap-0.5 p-0">
        {s.actions.map((a) => {
          const reason = unavailableReason(a, view);
          return (
            <li key={a.action}>
              <button
                type="button"
                disabled={Boolean(reason) || busy !== null}
                title={reason ?? a.label}
                aria-describedby={reason ? `act-${a.action}-why` : undefined}
                onClick={() => void run(a.action)}
                data-nav
                className={cn(
                  'flex min-h-8 w-full items-center gap-2.5 rounded-ctl border-0 bg-transparent px-2 py-1 text-left text-13 text-ink',
                  'hover:bg-pane-2 disabled:cursor-not-allowed disabled:text-ink-3 disabled:hover:bg-transparent',
                )}
              >
                {busy === a.action ? <Spinner /> : <Icon name={ACTION_ICON[a.action] ?? 'bolt'} className="size-4 shrink-0 text-ink-2" />}
                <span className="min-w-0 flex-1 truncate">{a.label}</span>
                {a.hotkey && <Keys combo={a.hotkey} />}
                {reason && (
                  <span id={`act-${a.action}-why`} className="sr-only">
                    {reason}
                  </span>
                )}
              </button>
            </li>
          );
        })}
      </ul>
      {keyOnly.length > 0 && (
        <dl className="m-0 mt-2 grid grid-cols-[minmax(0,1fr)_auto] items-center gap-x-3 gap-y-1.5 border-t border-line px-2 pt-2.5 text-small">
          {keyOnly.map((k) => (
            <div key={k.label} className="contents">
              <dt className="truncate text-ink-2">{k.label}</dt>
              <dd className="m-0">
                <Keys combo={k.keys} />
              </dd>
            </div>
          ))}
        </dl>
      )}
      {s.actions.length === 0 && keyOnly.length === 0 && <p className="px-1 text-small text-ink-3">No controls for this session.</p>}
      {(s.actions.length > 0 || keyOnly.length > 0) && (
        <p className="mt-2 px-2 text-12 text-ink-3">The keys also work while a game window has the focus.</p>
      )}
    </Pane>
  );
}
