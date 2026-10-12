// Host-owned state that several parts of the app read live.
import { useBridgeQuery, useBridgeSnapshot } from '@/bridge';

/** The open setup (lobby), or null. Live while mounted (`setup.changed`). */
export const useSetupSnapshot = () => useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot);

/** The session state (idle, setup, launching, running, ending). Live while mounted (`session.changed`). */
export const useSessionSnapshot = () => useBridgeSnapshot('session.get', 'session.changed', (e) => e.snapshot);

/** State of the host window (maximized, focused…). Live while mounted (`window.stateChanged`). */
export const useWindowState = () => useBridgeSnapshot('window.getState', 'window.stateChanged', (e) => e.state);

/** Versions, paths, hub connectivity (refetched on `app.hubStatusChanged`). */
export const useAppInfo = () => useBridgeQuery('app.getInfo');

/** Current settings values, by key (refetched on `settings.changed`). */
export const useSettingsValues = () => useBridgeQuery('settings.getValues');

/** Whether a session is launching, running or ending. */
export const isSessionLive = (state: string | undefined) => state === 'launching' || state === 'running' || state === 'ending';
