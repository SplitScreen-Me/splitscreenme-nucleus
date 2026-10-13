// UI glue around the bridge: error wording, refusal reasons, and commands that toast their failures.
import { useCallback } from 'react';
import { useToast } from '@/ui';
import { errorMessage, isCancelled } from './bridge-errors';

export { errorMessage, isCancelled, refusalReason } from './bridge-errors';

export interface RunOptions {
  /** Toast shown when the call succeeds. */
  success?: string;
  /** Replaces the error toast; return true when the error was handled (no toast). */
  onError?: (error: unknown) => boolean | void;
}

/**
 * Runs bridge calls from event handlers: resolves with the result, or with `undefined` after toasting the error
 * (cancellations stay silent).
 *
 * ```ts
 * const run = useRun();
 * const snapshot = await run(() => bridge.setup.join({ deviceId }));
 * ```
 */
export function useRun() {
  const toast = useToast();
  return useCallback(
    async <T>(call: () => Promise<T>, options: RunOptions = {}): Promise<T | undefined> => {
      try {
        const result = await call();
        if (options.success) toast(options.success);
        return result;
      } catch (error) {
        if (isCancelled(error)) return undefined;
        if (options.onError?.(error) === true) return undefined;
        toast(errorMessage(error), { icon: 'warning', durationMs: 4200 });
        return undefined;
      }
    },
    [toast],
  );
}
