import { useEffect, useState } from 'react';
import { T, useI18n, type TFunction } from '@/i18n';
import { cn, Icon, Kbd, Pane } from '@/ui';
import { HoldToStop } from './HoldToStop';
import { usesKeyboards, type SessionView } from './model';
import type { SessionCommands } from './useSessionCommands';

function statusText(view: SessionView, t: TFunction): string {
  const count = view.snapshot.instances.length;
  if (view.ended) return view.error ? t('session.header.status.failed') : t('session.header.status.ended');
  switch (view.snapshot.state) {
    case 'launching':
      return t('session.header.status.launching', { count });
    case 'running':
      return t('session.header.status.running', { count });
    case 'ending':
      return t('session.header.status.ending');
    default:
      return t('session.header.status.setup');
  }
}

/** The line under the lock and stop controls: what the keys do now. */
function noteText(view: SessionView, locked: boolean, keyboards: boolean, t: TFunction): string {
  const { lockInputsKey: key, stopHotkey } = view.snapshot;
  if (view.ended) return view.error ? t('session.header.note.failed') : t('session.header.note.ended');
  if (locked) return key ? t('session.header.note.locked', { key }) : t('session.header.note.lockedNoKey');
  if (keyboards) return key ? t('session.header.note.lockHint', { key }) : t('session.header.note.lockHintNoKey');
  return stopHotkey ? t('session.header.note.padsStopKey', { key: stopHotkey }) : t('session.header.note.pads');
}

/** Elapsed time since launch, frozen when the session ends. */
function useElapsed(startedAt: string | undefined, endedAt: number | undefined): string | undefined {
  const { format } = useI18n();
  const [now, setNow] = useState(() => Date.now());
  useEffect(() => {
    if (endedAt !== undefined || !startedAt) return;
    const id = window.setInterval(() => setNow(Date.now()), 1000);
    return () => window.clearInterval(id);
  }, [endedAt, startedAt]);
  if (!startedAt) return undefined;
  const start = Date.parse(startedAt);
  return Number.isNaN(start) ? undefined : format.clock((endedAt ?? now) - start);
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
  const { t } = useI18n();
  const { snapshot: s, ended } = view;
  const elapsed = useElapsed(s.startedAt, view.endedAt);
  const [coverFailed, setCoverFailed] = useState(false);
  const locked = !ended && s.inputsLocked;
  const keyboards = usesKeyboards(s);
  const note = noteText(view, locked, keyboards, t);

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
          {s.gameName ?? t('session.header.untitled')}
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
            <b className="font-[650] text-ink">{statusText(view, t)}</b>
          </span>
          {elapsed && (
            <span className="data text-ink-3" aria-label={t('session.header.elapsed', { time: elapsed })}>
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
              {locked ? t('session.header.inputsLocked') : t('session.header.inputsUnlocked')}
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
              <T k="session.header.stopKeys" values={{ keys: <Keys combo={s.stopHotkey} /> }} />
            </span>
          )}
        </span>
      </div>
    </Pane>
  );
}

