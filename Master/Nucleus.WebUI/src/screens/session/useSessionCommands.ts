import { useMemo } from 'react';
import { bridge, type SessionAction, type SessionSnapshot } from '@/bridge';
import { t } from '@/i18n';
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
    const onLocked = (error: unknown) => {
      if (refusalReason(error) !== 'inputsLocked') return false;
      // Built when it shows, in the language in use then.
      const message = lockKey ? t('session.commands.locked', { key: lockKey }) : t('session.commands.lockedNoKey');
      toast(message, { icon: 'lock', durationMs: 4200 });
      return true;
    };
    return {
      stop: () => run(() => bridge.session.stop(), { onError: onLocked }),
      runAction: (action: SessionAction) => run(() => bridge.session.runAction({ action }), { onError: onLocked }),
    };
  }, [run, toast, lockKey]);
}

export type SessionCommands = ReturnType<typeof useSessionCommands>;
