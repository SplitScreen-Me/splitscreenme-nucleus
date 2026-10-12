import type { BridgeTransport } from '../client.ts';

/**
 * A transport that becomes another one once `load` resolves; messages sent before are delivered then, in order.
 * Lets the app load the mock (and its fixtures) in a separate chunk, only when it runs outside WebView2.
 */
export function createLazyTransport(load: () => Promise<BridgeTransport>): BridgeTransport {
  const ready = load();
  ready.catch((error: unknown) => console.error('[bridge] the transport failed to load', error));
  return {
    send(message) {
      void ready.then((transport) => transport.send(message));
    },
    sendWithFiles(message, files) {
      void ready.then((transport) => (transport.sendWithFiles ? transport.sendWithFiles(message, files) : transport.send(message)));
    },
    subscribe(listener) {
      let active = true;
      let unsubscribe: (() => void) | undefined;
      void ready.then((transport) => {
        if (active) unsubscribe = transport.subscribe(listener);
      });
      return () => {
        active = false;
        unsubscribe?.();
      };
    },
  };
}
