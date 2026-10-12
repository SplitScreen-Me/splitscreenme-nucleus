// Compile-only checks of the public API's typing (checked by tsc, never run).
import {
  bridge,
  bridgeQueryKey,
  BridgeError,
  useBridgeEvent,
  useBridgeMutation,
  useBridgeQuery,
  useBridgeSnapshot,
  type GameDetails,
  type GameSummary,
  type SessionSnapshot,
  type SetupSnapshot,
  type WindowState,
} from '../index.ts';

export async function proxy(): Promise<void> {
  const games: GameSummary[] = await bridge.library.list();
  const path: string | null = await bridge.dialogs.pickFile({ title: 'Game', filters: [] });
  await bridge.library.setFavorite({ gameId: games[0]?.id ?? '', favorite: true }, { timeoutMs: 1_000 });
  await bridge.window.minimize({ signal: AbortSignal.timeout(500) });
  const paths: string[] = await bridge.getFilePaths([]);
  // @ts-expect-error params are required
  await bridge.library.setFavorite();
  // @ts-expect-error methods without params take options only
  await bridge.library.list({ gameId: 'x' });
  // @ts-expect-error unknown method
  await bridge.library.nope();
  void path;
  void paths;
}

export function hooks(): void {
  const list = useBridgeQuery('library.list');
  const listData: GameSummary[] | undefined = list.data;
  const details = useBridgeQuery('library.get', { gameId: 'g1' }, { enabled: true, timeoutMs: 5_000 });
  const detailsData: GameDetails | undefined = details.data;
  const count = useBridgeQuery('library.list', undefined, { select: (games) => games.length });
  const countData: number | undefined = count.data;
  // @ts-expect-error params are required
  useBridgeQuery('library.get');
  const error: BridgeError | null = list.error;

  const setFavorite = useBridgeMutation('library.setFavorite');
  setFavorite.mutate({ gameId: 'g1', favorite: true });
  const reload = useBridgeMutation('library.reload', { onSuccess: () => undefined });
  reload.mutate();
  // @ts-expect-error wrong params
  setFavorite.mutate({ gameId: 1 });

  const setup = useBridgeSnapshot('setup.get', 'setup.changed', (e) => e.snapshot);
  const setupData: SetupSnapshot | null | undefined = setup.data;
  const session = useBridgeSnapshot('session.get', 'session.changed', (e) => e.snapshot);
  const sessionData: SessionSnapshot | undefined = session.data;
  const windowState = useBridgeSnapshot('window.getState', 'window.stateChanged', (e) => e.state);
  const windowData: WindowState | undefined = windowState.data;
  // @ts-expect-error the payload is not the snapshot: a selector is required
  useBridgeSnapshot('setup.get', 'setup.changed');

  useBridgeEvent('setup.deviceActivity', (e) => {
    const id: string = e.deviceId;
    void id;
  });
  // @ts-expect-error unknown event
  useBridgeEvent('setup.nope', () => undefined);

  const key = bridgeQueryKey('library.get', { gameId: 'g1' });
  void [listData, detailsData, countData, error, setupData, sessionData, windowData, key];
}
