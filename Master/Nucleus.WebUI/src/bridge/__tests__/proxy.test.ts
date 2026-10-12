import assert from 'node:assert/strict';
import { test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { bridgeServices } from '../generated/contract.ts';
import { createBridgeProxy } from '../proxy.ts';
import { fakeTransport } from './helpers.ts';

test('the proxy has one function per method and calls the matching wire method', () => {
  const transport = fakeTransport();
  const bridge = createBridgeProxy(new BridgeClient({ transport, timeoutMs: 0 }));

  void bridge.library.list();
  void bridge.library.setFavorite({ gameId: 'g1', favorite: true }, { timeoutMs: 0 });
  void bridge.window.toggleMaximize({ timeoutMs: 0 });

  assert.deepEqual(transport.sent, [
    { jsonrpc: '2.0', id: 1, method: 'library.list' },
    { jsonrpc: '2.0', id: 2, method: 'library.setFavorite', params: { gameId: 'g1', favorite: true } },
    { jsonrpc: '2.0', id: 3, method: 'window.toggleMaximize' },
  ]);
  for (const [service, methods] of Object.entries(bridgeServices)) {
    assert.deepEqual(Object.keys((bridge as unknown as Record<string, object>)[service] ?? {}), Object.keys(methods));
  }
});

test('the proxy exposes getFilePaths', async () => {
  const transport = fakeTransport();
  const bridge = createBridgeProxy(new BridgeClient({ transport, timeoutMs: 0 }));

  const paths = bridge.getFilePaths([new File([''], 'a.nc')]);
  transport.receive({ jsonrpc: '2.0', id: 1, result: ['C:\\a.nc'] });

  assert.deepEqual(await paths, ['C:\\a.nc']);
});
