import { bridgeErrorCodes, type BridgeErrorKind } from './generated/contract.ts';
import type { WireError } from './protocol.ts';

/** Kind of a {@link BridgeError}: a kind the host returns, or `timeout` when it did not answer in time. */
export type ErrorKind = BridgeErrorKind | 'timeout';

const errorCodes: Readonly<Record<ErrorKind, number>> = { ...bridgeErrorCodes, timeout: -32099 };

const kindsByCode = new Map(Object.entries(errorCodes).map(([kind, code]) => [code, kind as ErrorKind]));

/**
 * A failed bridge call. Mirrors the C# `BridgeException`: services throw
 * `new BridgeException(BridgeErrorKind.NotFound, "Game not found", data)` and the UI catches
 * `BridgeError { kind: 'notFound', message: 'Game not found', data }`. Mocks throw it the same way.
 */
export class BridgeError extends Error {
  /** What went wrong. */
  readonly kind: ErrorKind;
  /** JSON-RPC error code of `kind`. `timeout` (-32099) is produced by the client, never by the host. */
  readonly code: number;
  /** Details from the host (C# `BridgeException` data), for example `{ reason: 'slotOccupied' }`. */
  readonly data: unknown;

  constructor(kind: ErrorKind, message: string, data?: unknown) {
    super(message);
    this.name = 'BridgeError';
    this.kind = kind;
    this.code = errorCodes[kind];
    this.data = data;
  }

  /** Type guard: whether `error` is a {@link BridgeError}, optionally of the given kind. */
  static is(error: unknown, kind?: ErrorKind): error is BridgeError {
    return error instanceof BridgeError && (kind === undefined || error.kind === kind);
  }
}

/** Builds the error of a failed response. Unknown codes become `internal`. */
export function errorFromWire(error: WireError): BridgeError {
  const kind = kindsByCode.get(error.code);
  return kind
    ? new BridgeError(kind, error.message, error.data)
    : new BridgeError('internal', `${error.message} (unknown error code ${error.code})`, error.data);
}

/** The error member of a response, for any thrown value (used by in-process transports). */
export function errorToWire(error: unknown): WireError {
  if (error instanceof BridgeError) {
    return error.data === undefined
      ? { code: error.code, message: error.message }
      : { code: error.code, message: error.message, data: error.data };
  }
  return { code: bridgeErrorCodes.internal, message: error instanceof Error ? error.message : String(error) };
}
