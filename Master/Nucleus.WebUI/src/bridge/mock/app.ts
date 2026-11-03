import { contractHash, protocolVersion, type BridgeHandlers, type LogLevel } from '../generated/contract.ts';
import { BridgeError } from '../errors.ts';
import type { MockContext } from '../transports/mock.ts';
import type { MockState } from './state.ts';
import { DEFAULT_HUB_URL, normalizeHubUrl } from '../../hub/urls.ts';

/**
 * The real hub; VITE_HUB_URL (in .env.local) points the browser build at another site, a local one for instance.
 * import.meta.env is undefined under node, where the bridge tests run the mock too.
 */
export const MOCK_HUB_URL = normalizeHubUrl((import.meta as { env?: Record<string, string | undefined> }).env?.VITE_HUB_URL) ?? DEFAULT_HUB_URL;

export const MOCK_APP_VERSION = '3.0.0-mock';

const consoleMethods: Record<LogLevel, 'debug' | 'info' | 'warn' | 'error'> = {
  debug: 'debug',
  info: 'info',
  warning: 'warn',
  error: 'error',
};

const browserOnline = () => globalThis.navigator?.onLine ?? true;

/** How the mock's hub starts: `online`, `offline` (a "Retry now" brings it back) or `down` (it stays offline). */
export type MockHubMode = 'online' | 'offline' | 'down';

/** Default: `localStorage['nucleus.mock.hub']`, else online. */
export function readMockHubMode(): MockHubMode {
  try {
    const value = globalThis.localStorage?.getItem('nucleus.mock.hub');
    return value === 'offline' || value === 'down' ? value : 'online';
  } catch {
    return 'online';
  }
}

/** Sets the mock's hub state and sends `app.hubStatusChanged` when it changed (as the host does). */
export function setMockHub(context: MockContext, state: MockState, online: boolean): void {
  if (state.hub.online === online) return;
  state.hub.online = online;
  context.emit('app.hubStatusChanged', { online });
}

/**
 * Mock of the `app` service. `hello` answers with the UI's own contract hash, so the handshake passes; the hub status
 * (`app.hubStatusChanged`) follows the browser's online/offline events, or starts offline with `nucleus.mock.hub`
 * (see `handlers.checkHub`). `restartAsAdmin` marks the state elevated.
 */
export function createAppMock(context: MockContext, state: MockState): BridgeHandlers['app'] {
  for (const type of ['online', 'offline']) {
    globalThis.addEventListener?.(type, () => setMockHub(context, state, browserOnline() && !state.hub.down));
  }

  return {
    hello: () => ({ protocolVersion, contractHash, appVersion: MOCK_APP_VERSION, debug: true }),
    getInfo: () => ({
      appVersion: MOCK_APP_VERSION,
      installPath: 'C:\\Nucleus Co-op',
      handlersPath: 'C:\\Nucleus Co-op\\handlers',
      hubUrl: MOCK_HUB_URL,
      hubOnline: state.hub.online,
      hubChecked: true,
      webViewVersion: globalThis.navigator?.userAgent.match(/Edg\/([\d.]+)/)?.[1] ?? 'browser',
      isAdmin: state.elevated,
    }),
    openExternal: ({ url }) => {
      if (!/^https?:\/\//i.test(url)) throw new BridgeError('invalidParams', 'Only web links can be opened.');
      globalThis.open?.(url, '_blank', 'noopener,noreferrer');
    },
    openPath: ({ path }) => console.info('[bridge mock] open in File Explorer:', path),
    log: ({ level, message, source }) => console[consoleMethods[level]]('[ui]', source ?? '', message),
    restartAsAdmin: () => {
      console.info('[bridge mock] restart as administrator');
      state.elevated = true;
    },
  };
}
