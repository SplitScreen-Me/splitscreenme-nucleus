import { eventInvalidations } from './generated/contract.ts';
import { BridgeError, errorFromWire } from './errors.ts';
import { CANCEL_METHOD, FILE_PATHS_METHOD, type NotificationMessage, type RequestMessage, type WireError } from './protocol.ts';
import type { CallArgs, CallOptions, EventName, EventPayload, MethodName, ResultOf } from './types.ts';

/** Moves serialized JSON-RPC messages between the UI and the host. */
export interface BridgeTransport {
  /** Sends one message to the host. */
  send(message: string): void;
  /** Sends one message with DOM `File` objects attached (WebView2 `postMessageWithAdditionalObjects`). Optional. */
  sendWithFiles?(message: string, files: readonly File[]): void;
  /** Calls `listener` with every message from the host. Returns a function that stops listening. */
  subscribe(listener: (message: string) => void): () => void;
}

/** Refetches the cached results of a method (all params). `BridgeProvider` plugs TanStack Query in. */
export type Invalidator = (method: MethodName) => void;

/** Options of {@link BridgeClient}. */
export interface BridgeClientOptions {
  transport: BridgeTransport;
  /** Default timeout of calls, in milliseconds. Default 30 000; 0 waits forever. */
  timeoutMs?: number;
  /** Called for each method an incoming event invalidates. Can also be set later with `setInvalidator`. */
  invalidate?: Invalidator;
  /** Receives every raw message, for debugging. */
  trace?: (direction: 'send' | 'receive', message: string) => void;
}

interface Pending {
  method: string;
  resolve(value: unknown): void;
  reject(error: BridgeError): void;
  /** Clears the timeout and the abort listener. */
  settle(): void;
}

type Listener = (payload: unknown) => void;

/**
 * JSON-RPC client of the host: calls with ids, timeouts and cancellation, event dispatch, automatic invalidation.
 * The app uses the shared instance (`bridgeClient`, and its typed proxy `bridge`); tests create their own.
 */
export class BridgeClient {
  readonly #transport: BridgeTransport;
  readonly #timeoutMs: number;
  readonly #trace: BridgeClientOptions['trace'];
  readonly #pending = new Map<number, Pending>();
  readonly #listeners = new Map<string, Set<Listener>>();
  readonly #unsubscribe: () => void;
  #invalidate: Invalidator | undefined;
  #nextId = 1;

  constructor(options: BridgeClientOptions) {
    this.#transport = options.transport;
    this.#timeoutMs = options.timeoutMs ?? 30_000;
    this.#invalidate = options.invalidate;
    this.#trace = options.trace;
    this.#unsubscribe = this.#transport.subscribe((message) => this.#receive(message));
  }

  /**
   * Calls a method: `call('library.list')`, `call('library.get', { gameId }, { signal })`.
   * Rejects with a {@link BridgeError}.
   */
  call<M extends MethodName>(method: M, ...[params, options]: CallArgs<M>): Promise<ResultOf<M>> {
    return this.#request(method, params, options) as Promise<ResultOf<M>>;
  }

  /**
   * Absolute paths of DOM `File` objects (dropped or picked), which page scripts cannot read.
   * Rejects with `unavailable` when the transport cannot attach files.
   */
  getFilePaths(files: FileList | readonly File[], options?: CallOptions): Promise<string[]> {
    const list = Array.from(files);
    const transport = this.#transport;
    if (list.length === 0) return Promise.resolve([]);
    if (!transport.sendWithFiles) {
      return Promise.reject(new BridgeError('unavailable', 'This transport cannot read file paths.', { reason: 'noFilePaths' }));
    }
    return this.#request(FILE_PATHS_METHOD, undefined, options, (message) => transport.sendWithFiles!(message, list)) as Promise<
      string[]
    >;
  }

  /** Calls `listener` for every `event` from the host. Returns a function that unsubscribes. */
  on<E extends EventName>(event: E, listener: (payload: EventPayload<E>) => void): () => void {
    let listeners = this.#listeners.get(event);
    if (!listeners) {
      listeners = new Set();
      this.#listeners.set(event, listeners);
    }
    const entry = listener as Listener;
    listeners.add(entry);
    return () => {
      listeners.delete(entry);
    };
  }

  /** Sets (or removes) the function that refetches the queries an event invalidates. */
  setInvalidator(invalidate: Invalidator | undefined): void {
    this.#invalidate = invalidate;
  }

  /** Stops listening to the transport and rejects every pending call with `cancelled`. */
  dispose(): void {
    this.#unsubscribe();
    for (const id of [...this.#pending.keys()]) {
      const pending = this.#take(id);
      pending?.reject(new BridgeError('cancelled', `${pending.method} was cancelled: the bridge client was disposed.`));
    }
  }

  #request(
    method: string,
    params: unknown,
    options: CallOptions = {},
    send?: (message: string) => void,
  ): Promise<unknown> {
    const { signal, timeoutMs = this.#timeoutMs } = options;
    if (signal?.aborted) return Promise.reject(new BridgeError('cancelled', `${method} was cancelled.`));

    const id = this.#nextId++;
    return new Promise((resolve, reject) => {
      const onAbort = () => this.#abandon(id, new BridgeError('cancelled', `${method} was cancelled.`));
      const timer =
        timeoutMs > 0
          ? setTimeout(() => this.#abandon(id, new BridgeError('timeout', `${method} did not answer within ${timeoutMs} ms.`)), timeoutMs)
          : undefined;
      signal?.addEventListener('abort', onAbort, { once: true });
      this.#pending.set(id, {
        method,
        resolve,
        reject,
        settle: () => {
          clearTimeout(timer);
          signal?.removeEventListener('abort', onAbort);
        },
      });

      const message: RequestMessage = params === undefined ? { jsonrpc: '2.0', id, method } : { jsonrpc: '2.0', id, method, params };
      try {
        this.#post(JSON.stringify(message), send);
      } catch (error) {
        this.#take(id)?.reject(new BridgeError('unavailable', `${method} could not be sent: ${String(error)}`, { reason: 'notSent' }));
      }
    });
  }

  /** Rejects a pending call locally and tells the host to stop working on it. */
  #abandon(id: number, error: BridgeError): void {
    const pending = this.#take(id);
    if (!pending) return;
    const cancel: NotificationMessage = { jsonrpc: '2.0', method: CANCEL_METHOD, params: { id } };
    try {
      this.#post(JSON.stringify(cancel));
    } finally {
      pending.reject(error);
    }
  }

  #take(id: number): Pending | undefined {
    const pending = this.#pending.get(id);
    if (pending) {
      this.#pending.delete(id);
      pending.settle();
    }
    return pending;
  }

  #post(message: string, send?: (message: string) => void): void {
    this.#trace?.('send', message);
    if (send) send(message);
    else this.#transport.send(message);
  }

  #receive(raw: string): void {
    this.#trace?.('receive', raw);
    let message: Record<string, unknown>;
    try {
      const parsed: unknown = JSON.parse(raw);
      if (typeof parsed !== 'object' || parsed === null) throw new TypeError('not an object');
      message = parsed as Record<string, unknown>;
    } catch {
      console.warn('[bridge] ignored a malformed message:', raw);
      return;
    }

    if (typeof message.id === 'number') {
      // Unknown ids belong to calls that timed out or were cancelled: their late answers are dropped.
      const pending = this.#take(message.id);
      if (message.error) pending?.reject(errorFromWire(message.error as WireError));
      else pending?.resolve(message.result ?? null);
    } else if (typeof message.method === 'string' && !('id' in message)) {
      this.#dispatch(message.method, message.params);
    } else {
      console.warn('[bridge] ignored an unexpected message:', message);
    }
  }

  #dispatch(event: string, payload: unknown): void {
    for (const listener of [...(this.#listeners.get(event) ?? [])]) {
      try {
        listener(payload);
      } catch (error) {
        console.error(`[bridge] a listener of ${event} threw`, error);
      }
    }
    for (const method of eventInvalidations[event as EventName] ?? []) {
      this.#invalidate?.(method);
    }
  }
}
