// UI glue around the bridge: error wording, refusal reasons, and commands that toast their failures.
import { useCallback } from 'react';
import { BridgeError } from '@/bridge';
import { t } from '@/i18n';
import { useToast } from '@/ui';

/** `data.reason` of a refused command (`conflict`, sometimes `notFound`), for example `slotOccupied`. */
export function refusalReason(error: unknown): string | undefined {
  if (!BridgeError.is(error)) return undefined;
  const data = error.data as { reason?: unknown } | null | undefined;
  return typeof data?.reason === 'string' ? data.reason : undefined;
}

/** Whether a call was cancelled (aborted signal, unmount, page reload): nothing to tell the user. */
export const isCancelled = (error: unknown) => BridgeError.is(error, 'cancelled');

/**
 * What to tell the user about a failed call. Host messages of expected failures (`conflict`, `notFound`,
 * `unavailable`, `invalidParams`) are written for users; the others get a plain explanation.
 */
export function errorMessage(error: unknown): string {
  if (BridgeError.is(error)) {
    switch (error.kind) {
      case 'timeout':
        return t('common.errors.timeout');
      case 'internal':
        return t('common.errors.internal');
      case 'methodNotFound':
        return t('common.errors.methodNotFound');
      case 'cancelled':
        return t('common.errors.cancelled');
      default:
        // Host messages are built in the language in use.
        return error.message || t('common.errors.failed');
    }
  }
  if (error instanceof Error && error.message) return error.message;
  return t('common.errors.unknown');
}

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
