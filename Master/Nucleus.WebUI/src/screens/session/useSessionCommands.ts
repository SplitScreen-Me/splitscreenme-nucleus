import { useMemo } from 'react';
import { bridge, type SessionAction, type SessionSnapshot } from '@/bridge';
import { refusalReason, useRun } from '@/lib/bridge-ui';
import { useToast } from '@/ui';

/**
 * Session commands with error toasts. Results also come back as `session.changed` snapshots, so callers never apply
 * them locally. A stop refused because inputs are locked says which key unlocks them.
 */
export function useSessionCommands(session: Pick<SessionSnapshot, 'lockInputsKey'>) {
  const run = useRun();
  const toast = useToast();
  const lockKey = session.lockInputsKey;
  return useMemo(() => {
    const lockedMessage = `Inputs are locked to the game windows. Press ${lockKey ?? 'the lock key'} to unlock them first.`;
    const onLocked = (error: unknown) => {
      if (refusalReason(error) !== 'inputsLocked') return false;
      toast(lockedMessage, { icon: 'lock', durationMs: 4200 });
      return true;
    };
    return {
      stop: () => run(() => bridge.session.stop(), { onError: onLocked }),
      runAction: (action: SessionAction) => run(() => bridge.session.runAction({ action }), { onError: onLocked }),
    };
  }, [run, toast, lockKey]);
}

export type SessionCommands = ReturnType<typeof useSessionCommands>;
