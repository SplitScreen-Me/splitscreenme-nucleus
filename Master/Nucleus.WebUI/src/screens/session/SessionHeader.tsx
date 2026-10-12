import { useEffect, useState } from 'react';
import { fmtClock, plural } from '@/lib/format';
import { cn, Icon, Kbd, Pane } from '@/ui';
import { HoldToStop } from './HoldToStop';
import { usesKeyboards, type SessionView } from './model';
import type { SessionCommands } from './useSessionCommands';

function statusText(view: SessionView): string {
  const n = view.snapshot.instances.length;
  if (view.ended) return view.error ? 'Launch failed' : 'Session ended';
  switch (view.snapshot.state) {
    case 'launching':
      return `Launching ${plural(n, 'player')}`;
    case 'running':
      return `${plural(n, 'player')} running`;
    case 'ending':
      return 'Stopping the session';
    default:
      return 'Setting up';
  }
}

/** Elapsed time since launch, frozen when the session ends. */
function useElapsed(startedAt: string | undefined, endedAt: number | undefined): string | undefined {
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    if (endedAt !== undefined || !startedAt) return;
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [endedAt, startedAt]);
  if (!startedAt) return undefined;
  const start = Date.parse(startedAt);
  return Number.isNaN(start) ? undefined : fmtClock((endedAt ?? now) - start);
}

/** "Ctrl+E" as key caps. */
export function Keys({ combo, className }: { combo: string; className?: string }) {
  return (
    <span className={cn('inline-flex shrink-0 gap-1', className)}>
      {combo.split('+').map((k, i) => (
        <Kbd key={`${k}-${i}`}>{k}</Kbd>
      ))}
    </span>
  );
}

export function SessionHeader({ view, cover, commands }: { view: SessionView; cover?: string; commands: SessionCommands }) {
  const { snapshot: s, ended } = view;
  const elapsed = useElapsed(s.startedAt, view.endedAt);
  const [coverFailed, setCoverFailed] = useState(false);
  const locked = !ended && s.inputsLocked;
  const keyboards = usesKeyboards(s);
  const lockKey = s.lockInputsKey ?? 'the lock key';

  const note = ended
    ? view.error
      ? 'The launch stopped before every copy was ready.'
      : 'Every copy is closed.'
    : locked
      ? `Inputs are locked to the game windows. Press ${lockKey} to unlock them.`
      : keyboards
        ? `Press ${lockKey} once every window is in place to give each keyboard and mouse to its player.`
        : s.stopHotkey
          ? `The game windows keep the controllers. ${s.stopHotkey} also stops the session.`
          : 'The game windows keep the controllers.';

  return (
    <Pane as="section" aria-labelledby="session-game-title" className="flex items-center gap-[18px] py-4 pr-[18px] pl-4 col-span-full">
      {cover && !coverFailed ? (
        <img src={cover} alt="" onError={() => setCoverFailed(true)} className="h-[61px] w-[46px] shrink-0 rounded-md bg-pane-2 object-cover" />
      ) : (
        <span aria-hidden="true" className="h-[61px] w-[46px] shrink-0 rounded-md bg-pane-2" />
      )}
      <div className="min-w-0">
        <h1
          id="session-game-title"
          className="mb-1.5 truncate text-28 leading-[1.05] font-[780] tracking-[-0.02em] stretch-wide max-[1320px]:text-[1.5rem]"
        >
          {s.gameName ?? 'Session'}
        </h1>
        <p className="flex items-center gap-2.5 text-small text-ink-2">
          <span role="status" aria-live="polite" className="inline-flex items-center gap-2.5">
            <span
              aria-hidden="true"
              className={cn(
                'size-2 rounded-full',
                ended ? (view.error ? 'bg-danger' : 'bg-ink-3') : s.state === 'running' ? 'bg-ok' : 'bg-warn',
              )}
            />
            <b className="font-[650] text-ink">{statusText(view)}</b>
          </span>
          {elapsed && (
            <span className="data text-ink-3" aria-label={`Elapsed time ${elapsed}`}>
              {elapsed}
            </span>
          )}
        </p>
      </div>

      <div className="ml-auto grid shrink-0 justify-items-end gap-[7px]">
        <div className="flex items-center gap-2">
          {!ended && (keyboards || locked) && (
            <span
              className={cn(
                'inline-flex h-11 items-center gap-2 rounded-ctl px-3.5 text-15 font-[650]',
                locked ? 'bg-ink text-on-light' : 'bg-pane-2 text-ink-2 shadow-[inset_0_0_0_1px_var(--color-line)]',
              )}
            >
              <Icon name={locked ? 'lock' : 'unlock'} className="size-[18px]" />
              {locked ? 'Inputs locked' : 'Inputs unlocked'}
              {s.lockInputsKey && (
                <Kbd className={cn('ml-0.5', locked && 'bg-on-light text-ink shadow-none')}>{s.lockInputsKey}</Kbd>
              )}
            </span>
          )}
          <HoldToStop
            stopped={ended}
            stopping={s.state === 'ending'}
            blocked={locked}
            onBlocked={() => void commands.stop()}
            onConfirm={() => void commands.stop()}
          />
        </div>
        <span id="lock-note" className="flex items-center gap-2 text-13 text-ink-3">
          {note}
          {!ended && !locked && s.stopHotkey && keyboards && (
            <span className="inline-flex items-center gap-1.5 whitespace-nowrap">
              Stop: <Keys combo={s.stopHotkey} />
            </span>
          )}
        </span>
      </div>
    </Pane>
  );
}

