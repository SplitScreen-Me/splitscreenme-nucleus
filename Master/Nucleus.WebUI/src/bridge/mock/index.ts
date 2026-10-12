// The mock host: every service of the contract, in the browser. Used outside WebView2 (`npm run dev`, review URL)
// and by tests. One file per service; `BridgeHandlers` makes the compiler flag any missing method.
import { createMockTransport, type MockContext, type MockTransport, type MockTransportOptions } from '../transports/mock.ts';
import { createAppMock } from './app.ts';
import { createDialogsMock } from './dialogs.ts';
import { createHandlersMock } from './handlers.ts';
import { createLibraryMock } from './library.ts';
import { createNoticesMock, seedNotices } from './notices.ts';
import { createProfilesMock } from './profiles.ts';
import { createSessionMock } from './session.ts';
import { createSettingsMock } from './settings.ts';
import { createSetupMock } from './setup.ts';
import { createMockState } from './state.ts';
import { createWindowMock } from './window.ts';

export interface MockBridgeOptions extends MockTransportOptions {
  /**
   * Start with notices waiting (a handler script error, an unloaded profile), to see the notices dialog.
   * Default: whether `localStorage['nucleus.mock.notices']` is '1'.
   */
  notices?: boolean;
}

/** Creates a mock transport serving the whole contract, with fresh state. */
export function createMockBridgeTransport({ notices = readNoticesFlag(), ...options }: MockBridgeOptions = {}): MockTransport {
  const context: MockContext = { emit: (event, payload) => transport.emit(event, payload) };
  const state = createMockState();
  if (notices) seedNotices(state);
  const transport = createMockTransport(
    {
      app: createAppMock(context, state),
      dialogs: createDialogsMock(),
      handlers: createHandlersMock(context, state),
      library: createLibraryMock(context, state),
      notices: createNoticesMock(context, state),
      profiles: createProfilesMock(context, state),
      session: createSessionMock(context, state),
      settings: createSettingsMock(context, state),
      setup: createSetupMock(context, state),
      window: createWindowMock(context),
    },
    options,
  );
  return transport;
}

function readNoticesFlag(): boolean {
  try {
    return globalThis.localStorage?.getItem('nucleus.mock.notices') === '1';
  } catch {
    return false;
  }
}
