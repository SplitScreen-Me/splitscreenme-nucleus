import { bridgeErrorCodes, type BridgeHandlers } from '../generated/contract.ts';
import type { BridgeTransport } from '../client.ts';
import { BridgeError, errorToWire } from '../errors.ts';
import { CANCEL_METHOD, FILE_PATHS_METHOD, type RequestMessage, type WireError } from '../protocol.ts';
import type { EventName, EventPayload } from '../types.ts';

/** Options of {@link createMockTransport}. */
export interface MockTransportOptions {
  /** Delay of every response and event, in milliseconds, to feel like a real round trip. Default 0. */
  latencyMs?: number;
}

/** A transport that runs the contract in-process with mock handlers, and can push events. */
export interface MockTransport extends BridgeTransport {
  /** Sends an event to the UI, as the host would. */
  emit<E extends EventName>(event: E, payload: EventPayload<E>): void;
}

/** What mock handlers receive to talk back to the UI. */
export interface MockContext {
  /** Sends an event to the UI. */
  emit: MockTransport['emit'];
}

type AnyHandler = (params?: unknown) => unknown;

/**
 * Serves the contract from `handlers`, through JSON like the real host: results are copies, `undefined` becomes
 * `null`, a thrown {@link BridgeError} reaches the UI with its kind and data (any other error becomes `internal`).
 * Honours `$/cancel` and answers `$/filePaths` with the file names.
 */
export function createMockTransport(handlers: BridgeHandlers, options: MockTransportOptions = {}): MockTransport {
  const latencyMs = options.latencyMs ?? 0;
  const listeners = new Set<(message: string) => void>();
  const inFlight = new Set<number>();

  const deliver = (message: object) => {
    const raw = JSON.stringify(message);
    setTimeout(() => [...listeners].forEach((listener) => listener(raw)), latencyMs);
  };

  const respond = (id: number, outcome: { result: unknown } | { error: WireError }) => {
    // A cancelled call has already been answered.
    if (inFlight.delete(id)) deliver({ jsonrpc: '2.0', id, ...outcome });
  };

  const handle = (raw: string, files: readonly File[] = []) => {
    const message = JSON.parse(raw) as Partial<RequestMessage>;
    if (message.method === CANCEL_METHOD) {
      const { id } = message.params as { id: number };
      respond(id, { error: errorToWire(new BridgeError('cancelled', 'The request was cancelled.')) });
      return;
    }
    if (typeof message.id !== 'number' || typeof message.method !== 'string') return;

    const { id, method, params } = message;
    inFlight.add(id);
    if (method === FILE_PATHS_METHOD) {
      respond(id, { result: files.map((file) => file.name) });
      return;
    }
    const handler = findHandler(handlers, method);
    if (!handler) {
      respond(id, { error: { code: bridgeErrorCodes.methodNotFound, message: `Unknown method '${method}'.` } });
      return;
    }
    Promise.resolve()
      .then(() => handler(params))
      .then(
        (result) => respond(id, { result: result ?? null }),
        (error: unknown) => {
          if (!(error instanceof BridgeError)) console.error(`[bridge mock] ${method} threw`, error);
          respond(id, { error: errorToWire(error) });
        },
      );
  };

  return {
    send: (message) => handle(message),
    sendWithFiles: (message, files) => handle(message, files),
    subscribe(listener) {
      listeners.add(listener);
      return () => {
        listeners.delete(listener);
      };
    },
    emit(event, payload) {
      deliver({ jsonrpc: '2.0', method: event, params: payload });
    },
  };
}

function findHandler(handlers: BridgeHandlers, method: string): AnyHandler | undefined {
  const dot = method.indexOf('.');
  const services = handlers as unknown as Record<string, Record<string, AnyHandler | undefined> | undefined>;
  return services[method.slice(0, dot)]?.[method.slice(dot + 1)];
}
