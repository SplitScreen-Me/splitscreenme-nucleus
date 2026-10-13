// The summary of the last session (`SessionSnapshot.lastSession`) in words. Erasable TypeScript with relative
// imports, so src/bridge/__tests__/session-summary.test.ts runs it under `node --test`. Texts are in the language in
// use; the components that show them re-render on a change through `useI18n`.
import type { SessionSummary, SessionSummaryDevice, SessionSummaryPlayer } from '../../bridge/generated/contract.ts';
import { format, t } from '../../i18n/store.ts';

/** How the session went: it failed to start, stopped before every window was ready, or ran and ended. */
export type SummaryOutcome = 'failed' | 'stoppedEarly' | 'ended';

export function summaryOutcome(s: Pick<SessionSummary, 'error' | 'launched'>): SummaryOutcome {
  if (s.error) return 'failed';
  return s.launched ? 'ended' : 'stoppedEarly';
}

/** Heading of the summary, in the language in use. */
export function summaryTitle(s: Pick<SessionSummary, 'error' | 'launched'>): string {
  switch (summaryOutcome(s)) {
    case 'failed':
      return t('session.outcome.failed');
    case 'stoppedEarly':
      return t('session.outcome.stoppedEarly');
    case 'ended':
      return t('session.outcome.ended');
  }
}

/** Milliseconds from Play to the end (0 when the dates can't be read). */
export function summaryMs(s: Pick<SessionSummary, 'startedAt' | 'endedAt'>): number {
  const start = Date.parse(s.startedAt);
  const end = Date.parse(s.endedAt);
  return Number.isNaN(start) || Number.isNaN(end) ? 0 : Math.max(0, end - start);
}

/** "1:02:03". */
export const summaryDuration = (s: Pick<SessionSummary, 'startedAt' | 'endedAt'>) => format.clock(summaryMs(s));

/** "Léa", or "Player 2" without a nickname. */
export const summaryPlayerName = (p: Pick<SessionSummaryPlayer, 'nickname' | 'number'>) =>
  p.nickname?.trim() || t('session.player', { number: p.number });

/** The devices a player played with, guests apart: "Xbox Controller + Logitech G502". */
export function summaryDevices(p: Pick<SessionSummaryPlayer, 'devices'>): { own: SessionSummaryDevice[]; guests: SessionSummaryDevice[] } {
  return { own: p.devices.filter((d) => !d.guest), guests: p.devices.filter((d) => d.guest) };
}

/**
 * "No errors in the session log.", "1 error in the session log", "3 errors in the session log" (from the whole log, not
 * only the lines the summary keeps).
 */
export function errorsLabel(s: Pick<SessionSummary, 'errorCount'>): string {
  return s.errorCount === 0 ? t('session.summary.noErrors') : t('session.summary.logErrors', { count: s.errorCount });
}
