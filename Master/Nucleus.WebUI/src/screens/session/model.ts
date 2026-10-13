// Session view-model helpers: pure derivations from the host's session snapshot.
import type { InstanceStage, SessionInstance, SessionSnapshot, SessionSummary } from '@/bridge';
import type { MessageKey, TFunction } from '@/i18n';

/** What the session screen renders: the live snapshot, or the last one of a session that just ended. */
export interface SessionView {
  snapshot: SessionSnapshot;
  /** The session ended while this screen was open (the snapshot is the last live one). */
  ended: boolean;
  /** When it ended (local clock), to freeze the elapsed time. */
  endedAt?: number;
  /** Why the launch failed, when it did. */
  error?: string;
  /** Crash report of the failed launch, shown on demand. */
  errorDetails?: string;
  /** The host's summary of the session that ended. */
  summary?: SessionSummary;
}

/** Whether a keyboard or a mouse drives one of the copies (then the inputs can be locked). */
export const usesKeyboards = (s: Pick<SessionSnapshot, 'instances'>) =>
  s.instances.some((i) => i.devices.some((d) => d === 'rawKeyboard' || d === 'rawMouse' || d === 'keyboardMouse'));

/** Launch steps of one copy, as the progress bars show them. */
export const STEPS = ['waiting', 'starting', 'ready'] as const;

/** Names of the launch steps. */
export const STEP_LABEL: Record<(typeof STEPS)[number], MessageKey> = {
  waiting: 'session.instance.steps.waiting',
  starting: 'session.instance.steps.starting',
  ready: 'session.instance.steps.ready',
};

const STEP_INDEX: Record<InstanceStage, number> = { waiting: 0, starting: 1, ready: 2, exited: -1 };

/** Index of the step a copy has reached (-1 once it exited). */
export const stepOf = (stage: InstanceStage) => STEP_INDEX[stage];

const STAGE_LABEL: Record<Exclude<InstanceStage, 'exited'>, MessageKey> = {
  waiting: 'session.instance.stage.waiting',
  starting: 'session.instance.stage.starting',
  ready: 'session.instance.stage.ready',
};

/** Short words for the stage of a copy. */
export function stageLabel(stage: InstanceStage, ended: boolean, t: TFunction): string {
  return ended || stage === 'exited' ? t('session.instance.stage.closed') : t(STAGE_LABEL[stage]);
}

/** Nickname of a copy, or "Player N". */
export const nicknameOf = (i: Pick<SessionInstance, 'nickname' | 'playerNumber'>, t: TFunction) =>
  i.nickname || t('session.player', { number: i.playerNumber });
