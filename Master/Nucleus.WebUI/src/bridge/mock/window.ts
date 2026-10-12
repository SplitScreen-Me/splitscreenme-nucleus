import type { BridgeHandlers, WindowState } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';

/** Mock of the `window` service: keeps a window state and emits `window.stateChanged` like the host. */
export function createWindowMock({ emit }: MockContext): BridgeHandlers['window'] {
  const state: WindowState = { maximized: false, minimized: false, focused: true, dpiScale: globalThis.devicePixelRatio ?? 1 };
  const change = (patch: Partial<WindowState>) => {
    Object.assign(state, patch);
    emit('window.stateChanged', { state });
  };

  return {
    getState: () => state,
    minimize: () => change({ minimized: true }),
    toggleMaximize: () => change({ maximized: !state.maximized, minimized: false }),
    close: () => console.info('[bridge mock] window.close'),
  };
}
