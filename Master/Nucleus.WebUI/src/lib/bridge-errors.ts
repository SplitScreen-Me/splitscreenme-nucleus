// What to tell the user about a failed bridge call. No React and relative imports: the node tests load this file.
import { BridgeError } from '../bridge/errors.ts';
import { t } from '../i18n/store.ts';
import type { MessageKey } from '../i18n/types.ts';

/** `data.reason` of a refused command (`conflict`, sometimes `notFound`), for example `slotOccupied`. */
export function refusalReason(error: unknown): string | undefined {
  if (!BridgeError.is(error)) return undefined;
  const data = error.data as { reason?: unknown } | null | undefined;
  return typeof data?.reason === 'string' ? data.reason : undefined;
}

/** Whether a call was cancelled (aborted signal, unmount, page reload): nothing to tell the user. */
export const isCancelled = (error: unknown) => BridgeError.is(error, 'cancelled');

/**
 * Failures of the bridge itself, whose messages are English for the log: the C# runtime (a service that failed to
 * start, params that don't bind) and this client (a message that could not be sent, a transport without file paths).
 */
const BRIDGE_FAILURES: Readonly<Record<string, MessageKey>> = {
  serviceFailed: 'common.errors.serviceFailed',
  badParams: 'common.errors.internal',
  notSent: 'common.errors.unreachable',
  noFilePaths: 'common.errors.failed',
};

/**
 * What to tell the user about a failed call. Host messages of expected failures (`conflict`, `notFound`,
 * `unavailable`, `invalidParams`) are built for users in the language in use; failures of the bridge itself and the
 * other kinds get a plain explanation.
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
      default: {
        const reason = refusalReason(error);
        if (reason !== undefined && Object.hasOwn(BRIDGE_FAILURES, reason)) return t(BRIDGE_FAILURES[reason]!);
        return error.message || t('common.errors.failed');
      }
    }
  }
  if (error instanceof Error && error.message) return error.message;
  return t('common.errors.unknown');
}
