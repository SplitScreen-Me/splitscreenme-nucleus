import { bridgeServices } from './generated/contract.ts';
import type { BridgeClient } from './client.ts';
import type { BridgeServicesProxy, CallOptions } from './types.ts';

/** The typed proxy (`bridge.library.list()`), plus bridge-level helpers. */
export type BridgeProxy = BridgeServicesProxy & {
  /**
   * Absolute paths of dropped or picked files, which page scripts cannot read (WebView2 passes the `File` objects
   * to the host). The mock returns the file names.
   */
  getFilePaths(files: FileList | readonly File[], options?: CallOptions): Promise<string[]>;
};

type LooseCall = (method: string, params?: unknown, options?: CallOptions) => Promise<unknown>;

/** Builds the typed proxy of a client from the generated service list. */
export function createBridgeProxy(client: BridgeClient): BridgeProxy {
  const call = client.call.bind(client) as LooseCall;
  const services = Object.entries(bridgeServices).map(([service, methods]) => {
    const functions = Object.entries(methods).map(([name, { params }]) => {
      const method = `${service}.${name}`;
      const fn = params
        ? (p: unknown, options?: CallOptions) => call(method, p, options)
        : (options?: CallOptions) => call(method, undefined, options);
      return [name, fn] as const;
    });
    return [service, Object.freeze(Object.fromEntries(functions))] as const;
  });
  return Object.freeze({
    ...Object.fromEntries(services),
    getFilePaths: (files: FileList | readonly File[], options?: CallOptions) => client.getFilePaths(files, options),
  }) as unknown as BridgeProxy;
}
