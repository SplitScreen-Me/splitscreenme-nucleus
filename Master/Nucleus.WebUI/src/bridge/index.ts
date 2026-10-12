// The bridge to the Nucleus Co-op host. Everything the UI needs is exported here; see docs/webui/bridge-howto.md.
export { bridge, bridgeClient, isMockBridge } from './instance.ts';
export {
  BridgeProvider,
  bridgeQueryKey,
  useBridgeClient,
  useBridgeEvent,
  useBridgeMutation,
  useBridgeQuery,
  useBridgeSnapshot,
  type BridgeMismatch,
  type BridgeMutationOptions,
  type BridgeProviderProps,
  type BridgeQueryKey,
  type BridgeQueryOptions,
} from './react.ts';
export { BridgeClient, type BridgeClientOptions, type BridgeTransport, type Invalidator } from './client.ts';
export { BridgeError, type ErrorKind } from './errors.ts';
export type { BridgeProxy } from './proxy.ts';
export type * from './types.ts';
export * from './generated/contract.ts';
