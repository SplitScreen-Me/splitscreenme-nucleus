import type { BridgeTransport } from '../client.ts';

/** The part of `window.chrome.webview` (injected by WebView2) the bridge uses. */
export interface WebViewApi {
  postMessage(message: unknown): void;
  postMessageWithAdditionalObjects(message: unknown, additionalObjects: ArrayLike<unknown>): void;
  addEventListener(type: 'message', listener: (event: { data: unknown }) => void): void;
  removeEventListener(type: 'message', listener: (event: { data: unknown }) => void): void;
}

/** The WebView2 API of the page, or undefined in a normal browser. */
export function getWebView(): WebViewApi | undefined {
  return (globalThis as { chrome?: { webview?: WebViewApi } }).chrome?.webview;
}

/** Transport to the Nucleus host through WebView2 web messages (JSON strings both ways). */
export function createWebView2Transport(webview: WebViewApi): BridgeTransport {
  return {
    send: (message) => webview.postMessage(message),
    sendWithFiles: (message, files) => webview.postMessageWithAdditionalObjects(message, files),
    subscribe(listener) {
      const onMessage = (event: { data: unknown }) => {
        if (typeof event.data === 'string') listener(event.data);
      };
      webview.addEventListener('message', onMessage);
      return () => webview.removeEventListener('message', onMessage);
    },
  };
}
