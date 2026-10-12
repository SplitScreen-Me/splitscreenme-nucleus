// Session view-model helpers: pure derivations from the host's session snapshot.
import type { InstanceStage, SessionInstance, SessionSnapshot } from '@/bridge';

/** What the session screen renders: the live snapshot, or the last one of a session that just ended. */
export interface SessionView {
  snapshot: SessionSnapshot;
  /** The session ended while this screen was open (the snapshot is the last live one). */
  ended: boolean;
  /** When it ended (local clock), to freeze the elapsed time. */
  endedAt?: number;
  /** Why the launch failed, when it did. */
  error?: string;
}

/** Whether a keyboard or a mouse drives one of the copies (then the inputs can be locked). */
export const usesKeyboards = (s: Pick<SessionSnapshot, 'instances'>) =>
  s.instances.some((i) => i.devices.some((d) => d === 'rawKeyboard' || d === 'rawMouse' || d === 'keyboardMouse'));

/** Launch steps of one copy, as the progress bars show them. */
export const STEPS = ['Waiting', 'Starting', 'Ready'] as const;

const STEP_INDEX: Record<InstanceStage, number> = { waiting: 0, starting: 1, ready: 2, exited: -1 };

/** Index of the step a copy has reached (-1 once it exited). */
export const stepOf = (stage: InstanceStage) => STEP_INDEX[stage];

/** Short words for the stage of a copy. */
export function stageLabel(stage: InstanceStage, ended: boolean): string {
  if (ended || stage === 'exited') return 'Closed';
  switch (stage) {
    case 'waiting':
      return 'Waiting to start…';
    case 'starting':
      return 'Starting, placing the window…';
    case 'ready':
      return 'Ready';
  }
}

/** Nickname of a copy, or "Player N". */
export const nicknameOf = (i: Pick<SessionInstance, 'nickname' | 'playerNumber'>) => i.nickname || `Player ${i.playerNumber}`;
