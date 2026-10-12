import type { BridgeEvents, BridgeHandlers, BridgeMethods } from './generated/contract.ts';

/** Wire name of a bridge method, for example `'library.list'`. */
export type MethodName = keyof BridgeMethods;

/** Params of a method: a request object, or `void` when it takes none. */
export type ParamsOf<M extends MethodName> = BridgeMethods[M]['params'];

/** Result of a method (`void` when it returns nothing; the wire carries `null`). */
export type ResultOf<M extends MethodName> = BridgeMethods[M]['result'];

/** Methods that take no params, such as `'library.list'`. */
export type NoParamsMethod = { [M in MethodName]: [ParamsOf<M>] extends [void] ? M : never }[MethodName];

/** Name of a bridge event, for example `'library.changed'`. */
export type EventName = keyof BridgeEvents;

/** Payload of an event. */
export type EventPayload<E extends EventName> = BridgeEvents[E];

/** Per-call options. */
export interface CallOptions {
  /** Rejects with a `timeout` error after this many milliseconds. Default: the client's (30 s). 0 waits forever. */
  timeoutMs?: number;
  /** Aborting rejects the call with a `cancelled` error and tells the host to stop. */
  signal?: AbortSignal;
}

/** Arguments of a call after the method name: the params when the method takes some, then the options. */
export type CallArgs<M extends MethodName> = [ParamsOf<M>] extends [void]
  ? [params?: undefined, options?: CallOptions]
  : [params: ParamsOf<M>, options?: CallOptions];

/** A proxy method: `(options?)` for methods without params, `(params, options?)` otherwise. */
export type ProxyMethod<M extends MethodName> = [ParamsOf<M>] extends [void]
  ? (options?: CallOptions) => Promise<ResultOf<M>>
  : (params: ParamsOf<M>, options?: CallOptions) => Promise<ResultOf<M>>;

/**
 * One object per service, one function per method: `bridge.library.setFavorite({ gameId, favorite })`.
 * Mapped from the generated {@link BridgeHandlers}, so editor hovers show the C# documentation.
 */
export type BridgeServicesProxy = {
  readonly [S in keyof BridgeHandlers]: {
    readonly [M in keyof BridgeHandlers[S]]: ProxyMethod<`${S & string}.${M & string}` & MethodName>;
  };
};
