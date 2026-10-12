import { BridgeClient } from './client.ts';
import { createBridgeProxy } from './proxy.ts';
import { createLazyTransport } from './transports/lazy.ts';
import { createWebView2Transport, getWebView } from './transports/webview2.ts';

const webview = getWebView();

/** Whether the app runs on the in-browser mock (normal browser) rather than inside Nucleus Co-op. */
export const isMockBridge = webview === undefined;

/**
 * The app-wide bridge client: WebView2 inside Nucleus Co-op, the mock (loaded on demand) in a normal browser.
 * Set `localStorage['nucleus.bridge.trace'] = '1'` and reload to log every message in the console.
 */
export const bridgeClient = new BridgeClient({
  transport: webview
    ? createWebView2Transport(webview)
    : createLazyTransport(async () => (await import('./mock/index.ts')).createMockBridgeTransport({ latencyMs: 40 })),
  trace: readTraceFlag() ? logMessage : undefined,
});

/** Typed proxy of {@link bridgeClient}: `await bridge.library.setFavorite({ gameId, favorite: true })`. */
export const bridge = createBridgeProxy(bridgeClient);

// Dev tools: `await __bridge.app.getInfo()`.
Object.assign(globalThis, { __bridge: bridge });

function readTraceFlag(): boolean {
  try {
    return globalThis.localStorage?.getItem('nucleus.bridge.trace') === '1';
  } catch {
    return false;
  }
}

function logMessage(direction: 'send' | 'receive', message: string): void {
  console.debug(direction === 'send' ? '[bridge] →' : '[bridge] ←', JSON.parse(message));
}
