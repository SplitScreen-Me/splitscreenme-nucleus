// How the last session went, from the host's summary (`SessionSnapshot.lastSession`): shown when it ends, and on the
// session screen until the next one starts.
import { useEffect, useId, useRef, useState } from 'react';
import type { SessionLogEntry, SessionSummary } from '@/bridge';
import { useI18n, useT } from '@/i18n';
import { deviceIcon } from '@/lib/devices';
import { setupPath } from '@/lib/library';
import { Button, ButtonLink, cn, Icon, PlayerTag, playerClass } from '@/ui';
import { errorsLabel, summaryDevices, summaryDuration, summaryOutcome, summaryPlayerName, summaryTitle } from './summary';

/**
 * The summary card: what ended and how, the players with their colour and devices, the errors, and the way back.
 * `errorDetails` is the crash report of a failed launch, behind "Show details".
 */
export function SessionSummaryCard({ summary, errorDetails, className }: { summary: SessionSummary; errorDetails?: string; className?: string }) {
  const { t, format } = useI18n();
  const primary = useRef<HTMLAnchorElement>(null);
  const titleId = useId();
  useEffect(() => {
    primary.current?.focus({ preventScroll: true });
  }, []);
  const outcome = summaryOutcome(summary);
  const duration = summaryDuration(summary);
  const started = format.time(summary.startedAt);
  const ended = format.time(summary.endedAt);

  return (
    <section aria-labelledby={titleId} className={cn('grid w-full max-w-[68ch] gap-5 text-left', className)}>
      <div className="grid gap-1.5">
        <h3 id={titleId} className="flex items-center gap-2.5 text-22 leading-tight font-[780] stretch-wide">
          {outcome === 'failed' && <Icon name="warning" className="size-6 shrink-0 text-danger" />}
          {summaryTitle(summary)}
        </h3>
        <dl className="m-0 flex flex-wrap gap-x-6 gap-y-1 text-13 text-ink-3">
          <div className="flex gap-1.5">
            <dt>{t('session.summary.game')}</dt>
            <dd className="m-0 font-[650] text-ink">{summary.gameName}</dd>
          </div>
          <div className="flex gap-1.5">
            <dt>{t('session.summary.duration')}</dt>
            <dd className="data m-0 font-[650] text-ink" aria-label={duration}>
              {duration}
            </dd>
          </div>
          {started && (
            <div className="flex gap-1.5">
              <dt>{t('session.summary.played')}</dt>
              <dd className="data m-0 text-ink-2">{t('session.summary.timeRange', { start: started, end: ended })}</dd>
            </div>
          )}
        </dl>
      </div>

      {summary.players.length > 0 && (
        <ul className="m-0 grid list-none gap-0 p-0" aria-label={t('session.summary.players')}>
          {summary.players.map((p) => {
            const { own, guests } = summaryDevices(p);
            return (
              <li key={p.number} className={cn(playerClass(p.number), 'flex min-w-0 items-center gap-3 border-t border-line py-2.5 first:border-t-0')}>
                <PlayerTag n={p.number} size="lg" />
                <strong className="min-w-[8ch] truncate text-16 font-[700] stretch-semi">{summaryPlayerName(p)}</strong>
                <span className="flex min-w-0 flex-1 flex-wrap items-center gap-x-3 gap-y-0.5 text-13 text-ink-2">
                  {own.map((d, i) => (
                    <span key={`${d.name}-${i}`} className="inline-flex min-w-0 items-center gap-1.5">
                      <Icon name={deviceIcon(d.kind)} className="size-4 shrink-0 text-pc" />
                      <span className="truncate">{d.name}</span>
                    </span>
                  ))}
                  {guests.map((d, i) => (
                    <span key={`guest-${d.name}-${i}`} className="inline-flex min-w-0 items-center gap-1.5 text-ink-3">
                      <Icon name="users" className="size-4 shrink-0" />
                      <span className="truncate">{t('session.summary.guest', { device: d.name })}</span>
                    </span>
                  ))}
                  {own.length + guests.length === 0 && <span className="text-ink-3">{t('session.summary.noDevice')}</span>}
                </span>
              </li>
            );
          })}
        </ul>
      )}

      <Problems summary={summary} errorDetails={errorDetails} />

      <div className="flex flex-wrap gap-2">
        {summary.gameId && (
          <ButtonLink ref={primary} variant="primary" to={setupPath(summary.gameId)} data-nav>
            <Icon name="users" />
            {outcome === 'failed' ? t('session.links.lobby') : t('session.links.playAgain')}
          </ButtonLink>
        )}
        <ButtonLink variant="ghost" to="/library" data-nav>
          <Icon name="library" />
          {t('session.links.library')}
        </ButtonLink>
      </div>
    </section>
  );
}

/** The error of a failed launch and the error lines of the log; a plain "No errors" otherwise. */
function Problems({ summary, errorDetails }: { summary: SessionSummary; errorDetails?: string }) {
  const t = useT();
  if (!summary.error && summary.errorCount === 0) {
    return (
      <p className="flex items-center gap-2 text-13 text-ink-2">
        <span className="grid size-5 place-items-center rounded-full bg-ok/16 text-ok">
          <Icon name="check" strokeWidth={2.4} className="size-3" />
        </span>
        {errorsLabel(summary)}
      </p>
    );
  }
  const more = summary.errorCount - summary.errors.length;
  return (
    <div role="group" aria-label={t('session.summary.errors')} className="grid gap-2.5 rounded-ctl bg-pane-2 px-4 py-3.5">
      {summary.error && (
        <p className="flex items-start gap-2 text-15 text-ink">
          <Icon name="warning" className="mt-0.5 size-[18px] shrink-0 text-danger" />
          <span>{summary.error}</span>
        </p>
      )}
      {summary.errorCount > 0 && (
        <>
          <p className="text-13 font-[650] text-ink-2">{errorsLabel(summary)}</p>
          <ol className="m-0 grid max-h-[22vh] list-none gap-1 overflow-auto p-0 font-mono text-12 leading-normal text-ink-2">
            {summary.errors.map((e) => (
              <ErrorLine key={e.seq} entry={e} />
            ))}
          </ol>
          {more > 0 && <p className="text-12 text-ink-3">{t('session.summary.moreErrors', { count: more })}</p>}
        </>
      )}
      {errorDetails && <ErrorDetails text={errorDetails} onRaised />}
    </div>
  );
}

function ErrorLine({ entry }: { entry: SessionLogEntry }) {
  const { t, format } = useI18n();
  return (
    <li className="grid grid-cols-[auto_auto_minmax(0,1fr)] items-baseline gap-x-2">
      <span className="text-ink-3">{format.time(entry.at)}</span>
      {entry.playerNumber ? (
        <span className={cn(playerClass(entry.playerNumber), 'font-[700] text-pc')}>
          {t('session.summary.playerShort', { number: entry.playerNumber })}
        </span>
      ) : (
        <span aria-hidden="true" />
      )}
      <span className="wrap-anywhere text-danger">{entry.text}</span>
    </li>
  );
}

/** The crash report of a failed launch, behind "Show details". `onRaised` when it sits on a raised surface (bg-pane-2). */
export function ErrorDetails({ text, className, onRaised }: { text: string; className?: string; onRaised?: boolean }) {
  const t = useT();
  const [open, setOpen] = useState(false);
  return (
    <div className={cn('grid w-full justify-items-start gap-2', className)}>
      <Button variant="quiet" size="sm" aria-expanded={open} onClick={() => setOpen((o) => !o)} data-nav>
        <Icon name={open ? 'chev-d' : 'chev-r'} />
        {open ? t('session.errorDetails.hide') : t('session.errorDetails.show')}
      </Button>
      {open && (
        <pre
          className={cn(
            'max-h-[40vh] w-full overflow-auto rounded-ctl px-3.5 py-3 text-left font-mono text-12 leading-normal whitespace-pre-wrap text-ink-2 wrap-anywhere',
            onRaised ? 'bg-pane' : 'bg-pane-2',
          )}
        >
          {text}
        </pre>
      )}
    </div>
  );
}
