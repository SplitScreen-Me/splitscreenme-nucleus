// The state of one copy of the game on the session screen, and its Restart control (Nucleus 2.5 restarts crashed copies;
// `session.restartInstance` restarts one on request). Erasable TypeScript with relative imports, so
// src/bridge/__tests__/session-restart.test.ts runs it under `node --test`. Texts are in the language in use; the
// components that show them re-render on a change through their own i18n hooks.
import type { SessionInstance, SessionSnapshot } from '../../bridge/generated/contract.ts';
import { t } from '../../i18n/store.ts';
import type { MessageKey } from '../../i18n/types.ts';

/** What a copy's card says: its launch step, or how it ended (closed, crashed, a restart that failed), or a restart. */
export type InstanceStatus = 'waiting' | 'starting' | 'ready' | 'restarting' | 'closed' | 'crashed' | 'restartFailed';

/** The status of a copy. Once the session ended every copy reads as closed. */
export function instanceStatus(i: Pick<SessionInstance, 'stage' | 'crashed' | 'restartFailed'>, ended: boolean): InstanceStatus {
  if (ended) return 'closed';
  if (i.stage !== 'exited') return i.stage;
  if (i.restartFailed) return 'restartFailed';
  return i.crashed ? 'crashed' : 'closed';
}

/** Whether a status means the copy no longer runs (its card is dimmed and offers a restart). */
export const isClosed = (status: InstanceStatus) => status === 'closed' || status === 'crashed' || status === 'restartFailed';

/** Whether a status reads as a problem (the danger tint). */
export const isFailure = (status: InstanceStatus) => status === 'crashed' || status === 'restartFailed';

const LABELS: Record<InstanceStatus, MessageKey> = {
  waiting: 'session.instance.stage.waiting',
  starting: 'session.instance.stage.starting',
  ready: 'session.instance.stage.ready',
  restarting: 'session.instance.stage.restarting',
  closed: 'session.instance.stage.closed',
  crashed: 'session.instance.stage.crashed',
  restartFailed: 'session.instance.stage.restartFailed',
};

/** Short words for the status of a copy, in the language in use. */
export const statusLabel = (status: InstanceStatus) => t(LABELS[status]);

/**
 * The Restart control of a copy's card: `none` (it runs, the session ended, or the handler can't restart copies),
 * `available`, or `busy` (shown disabled while another copy is being started again).
 */
export type RestartOffer = 'none' | 'available' | 'busy';

export function restartOffer(status: InstanceStatus, s: Pick<SessionSnapshot, 'restartRefusal'>, ended: boolean): RestartOffer {
  if (ended || !isClosed(status)) return 'none';
  if (s.restartRefusal === undefined) return 'available';
  return s.restartRefusal === 'busy' ? 'busy' : 'none';
}

/** Why a closed copy has no Restart, when the handler is the reason (said under its status). */
export function restartNote(status: InstanceStatus, s: Pick<SessionSnapshot, 'restartRefusal'>, ended: boolean): string | undefined {
  if (ended || !isClosed(status)) return undefined;
  if (s.restartRefusal === 'notSupported') return t('session.instance.restartUnsupported');
  return s.restartRefusal === 'busy' ? t('session.instance.restartBusy') : undefined;
}
