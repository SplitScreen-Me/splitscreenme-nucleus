import { createContext, createElement, useContext, useEffect, useEffectEvent, useRef, type ReactNode } from 'react';
import {
  useMutation,
  useQuery,
  useQueryClient,
  type UseMutationOptions,
  type UseMutationResult,
  type UseQueryOptions,
  type UseQueryResult,
} from '@tanstack/react-query';
import { contractHash, protocolVersion, type HelloResponse } from './generated/contract.ts';
import type { BridgeClient } from './client.ts';
import type { BridgeError } from './errors.ts';
import { bridgeClient } from './instance.ts';
import type { CallOptions, EventName, EventPayload, MethodName, NoParamsMethod, ParamsOf, ResultOf } from './types.ts';

type LooseCall = (method: string, params?: unknown, options?: CallOptions) => Promise<unknown>;

const BridgeContext = createContext<BridgeClient | null>(null);

/** Query key of a method's result: `[method]` for methods without params, `[method, params]` otherwise. */
export type BridgeQueryKey<M extends MethodName> = readonly [M] | readonly [M, ParamsOf<M>];

/** Builds the query key of a method's result, for `queryClient.setQueryData` and friends. */
export function bridgeQueryKey<M extends MethodName>(method: M, params?: ParamsOf<M>): BridgeQueryKey<M> {
  return params === undefined ? [method] : [method, params];
}

/** Options of {@link useBridgeQuery}: TanStack Query options, plus the call timeout. */
export type BridgeQueryOptions<M extends MethodName, TData = ResultOf<M>> = Omit<
  UseQueryOptions<ResultOf<M>, BridgeError, TData, BridgeQueryKey<M>>,
  'queryKey' | 'queryFn'
> &
  Pick<CallOptions, 'timeoutMs'>;

/** Options of {@link useBridgeMutation}: TanStack Query options, plus the call timeout. */
export type BridgeMutationOptions<M extends MethodName, TContext = unknown> = Omit<
  UseMutationOptions<ResultOf<M>, BridgeError, ParamsOf<M>, TContext>,
  'mutationFn'
> &
  Pick<CallOptions, 'timeoutMs'>;

/** What {@link BridgeProvider} shows when the UI and the app were built from different contracts. */
export interface BridgeMismatch {
  /** What this UI was generated from. */
  ui: { protocolVersion: number; contractHash: string };
  /** What the app answered. */
  host: HelloResponse;
}

/** Props of {@link BridgeProvider}. */
export interface BridgeProviderProps {
  children: ReactNode;
  /** Client to use. Default: the app-wide `bridgeClient`. */
  client?: BridgeClient;
  /** Shown during the handshake. Default: nothing. */
  connecting?: ReactNode;
  /** Shown instead of the app when the UI and the app don't match (stale `webui` folder). */
  renderMismatch?: (mismatch: BridgeMismatch) => ReactNode;
  /** Shown instead of the app when the handshake fails. */
  renderError?: (error: BridgeError) => ReactNode;
}

/**
 * Connects the UI to the host. Place it inside `QueryClientProvider`: it performs the `app.hello` handshake (children
 * render only when the contract hashes match), and makes events invalidate TanStack queries automatically.
 */
export function BridgeProvider({
  children,
  client = bridgeClient,
  connecting = null,
  renderMismatch = MismatchScreen,
  renderError = ErrorScreen,
}: BridgeProviderProps): ReactNode {
  const queryClient = useQueryClient();
  useEffect(() => {
    client.setInvalidator((method) => void queryClient.invalidateQueries({ queryKey: [method] }));
    return () => client.setInvalidator(undefined);
  }, [client, queryClient]);

  const hello = useQuery<HelloResponse, BridgeError>({
    queryKey: bridgeQueryKey('app.hello'),
    queryFn: ({ signal }) => client.call('app.hello', undefined, { signal }),
    staleTime: Infinity,
    retry: false,
  });

  let content: ReactNode;
  if (hello.isPending) content = connecting;
  else if (hello.isError) content = renderError(hello.error);
  else if (hello.data.contractHash !== contractHash || hello.data.protocolVersion !== protocolVersion) {
    content = renderMismatch({ ui: { protocolVersion, contractHash }, host: hello.data });
  } else content = children;
  return createElement(BridgeContext.Provider, { value: client }, content);
}

/** `client.call`, bound: the client keeps its state in private fields, so an unbound call would throw. */
function useBoundCall(): LooseCall {
  const client = useBridgeClient();
  return client.call.bind(client) as LooseCall;
}

/** The client of the closest {@link BridgeProvider}. */
export function useBridgeClient(): BridgeClient {
  const client = useContext(BridgeContext);
  if (!client) throw new Error('Bridge hooks must be used inside <BridgeProvider>.');
  return client;
}

/**
 * Result of a bridge method as a TanStack query, keyed `[method, params]`. Events listed in the contract
 * (`Invalidates`) refetch it automatically.
 *
 * ```ts
 * const games = useBridgeQuery('library.list');
 * const game = useBridgeQuery('library.get', { gameId }, { enabled: !!gameId });
 * ```
 */
export function useBridgeQuery<M extends NoParamsMethod, TData = ResultOf<M>>(
  method: M,
  params?: undefined,
  options?: BridgeQueryOptions<M, TData>,
): UseQueryResult<TData, BridgeError>;
export function useBridgeQuery<M extends Exclude<MethodName, NoParamsMethod>, TData = ResultOf<M>>(
  method: M,
  params: ParamsOf<M>,
  options?: BridgeQueryOptions<M, TData>,
): UseQueryResult<TData, BridgeError>;
export function useBridgeQuery(
  method: MethodName,
  params?: unknown,
  { timeoutMs, ...options }: BridgeQueryOptions<MethodName, unknown> = {},
): UseQueryResult<unknown, BridgeError> {
  const call = useBoundCall();
  return useQuery({
    ...options,
    queryKey: bridgeQueryKey(method, params as never),
    queryFn: ({ signal }) => call(method, params, { signal, timeoutMs }) as Promise<ResultOf<MethodName>>,
  });
}

/**
 * A bridge method as a TanStack mutation: `mutate(params)`, `mutateAsync(params)`.
 *
 * ```ts
 * const setFavorite = useBridgeMutation('library.setFavorite');
 * setFavorite.mutate({ gameId, favorite: true });
 * ```
 */
export function useBridgeMutation<M extends MethodName, TContext = unknown>(
  method: M,
  { timeoutMs, ...options }: BridgeMutationOptions<M, TContext> = {},
): UseMutationResult<ResultOf<M>, BridgeError, ParamsOf<M>, TContext> {
  const call = useBoundCall();
  return useMutation({
    mutationKey: [method],
    ...options,
    mutationFn: (params) => call(method, params, { timeoutMs }) as Promise<ResultOf<M>>,
  });
}

/** Calls `handler` for every `event` while the component is mounted. The latest `handler` is always used. */
export function useBridgeEvent<E extends EventName>(event: E, handler: (payload: EventPayload<E>) => void): void {
  const client = useBridgeClient();
  const onEvent = useEffectEvent(handler);
  useEffect(() => client.on(event, (payload) => onEvent(payload)), [client, event]);
}

/** `select` is optional when the event payload is the snapshot itself, required otherwise. */
type SnapshotSelect<M extends MethodName, E extends EventName> = [EventPayload<E>] extends [ResultOf<M>]
  ? [select?: (payload: EventPayload<E>) => ResultOf<M>]
  : [select: (payload: EventPayload<E>) => ResultOf<M>];

/**
 * Host-owned state, live: the snapshot returned by `method`, replaced by the one each `event` carries.
 *
 * ```ts
 * const setup = useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot);
 * ```
 */
export function useBridgeSnapshot<M extends NoParamsMethod, E extends EventName>(
  method: M,
  event: E,
  ...[select]: SnapshotSelect<M, E>
): UseQueryResult<ResultOf<M>, BridgeError> {
  const call = useBoundCall();
  const queryClient = useQueryClient();
  const queryKey = bridgeQueryKey(method);
  const events = useRef(0);
  useBridgeEvent(event, (payload) => {
    events.current++;
    queryClient.setQueryData(queryKey, select ? select(payload) : payload);
  });
  return useQuery({
    queryKey,
    queryFn: async ({ signal }) => {
      const seen = events.current;
      const snapshot = (await call(method, undefined, { signal })) as ResultOf<M>;
      // An event that arrived meanwhile carries a newer snapshot than this answer.
      return seen === events.current ? snapshot : (queryClient.getQueryData<ResultOf<M>>(queryKey) as ResultOf<M>);
    },
  });
}

function MismatchScreen({ ui, host }: BridgeMismatch): ReactNode {
  return createElement(
    'div',
    { role: 'alert', style: screenStyle },
    createElement('h1', { style: { fontSize: 20, margin: '0 0 12px' } }, "UI and app versions don't match"),
    createElement(
      'p',
      { style: { margin: 0, maxWidth: 560, lineHeight: 1.5, opacity: 0.8 } },
      `The interface files (webui folder) were not built for Nucleus Co-op ${host.appVersion}. Reinstall Nucleus Co-op. ` +
        `(UI contract ${ui.contractHash}, protocol ${ui.protocolVersion}; app contract ${host.contractHash}, protocol ${host.protocolVersion})`,
    ),
  );
}

function ErrorScreen(error: BridgeError): ReactNode {
  return createElement('div', { role: 'alert', style: screenStyle }, `Nucleus Co-op did not answer: ${error.message}`);
}

const screenStyle = {
  minHeight: '100vh',
  padding: 32,
  boxSizing: 'border-box',
  fontFamily: 'system-ui, sans-serif',
  color: '#e8e8e8',
  background: '#0b0b0c',
} as const;
