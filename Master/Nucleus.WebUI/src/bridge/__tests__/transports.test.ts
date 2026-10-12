import assert from 'node:assert/strict';
import { describe, test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import type { BridgeHandlers } from '../generated/contract.ts';
import { createLazyTransport } from '../transports/lazy.ts';
import { createMockTransport } from '../transports/mock.ts';
import { createWebView2Transport, type WebViewApi } from '../transports/webview2.ts';
import { delay, fakeTransport } from './helpers.ts';

/** A mock transport serving only the handlers a test needs. */
function mockClient(handlers: Record<string, Record<string, (params: never) => unknown>>) {
  const transport = createMockTransport(handlers as unknown as BridgeHandlers);
  return { transport, client: new BridgeClient({ transport }) };
}

describe('mock transport', () => {
  test('serves handlers through JSON, with null for void results', async () => {
    const game = { id: 'g1', name: 'Halo' };
    const { client } = mockClient({
      library: { get: ({ gameId }: { gameId: string }) => ({ summary: { ...game, id: gameId } }), setFavorite: () => undefined },
    });

    const details = await client.call('library.get', { gameId: 'g2' });
    const done = await client.call('library.setFavorite', { gameId: 'g2', favorite: true });

    assert.deepEqual(details, { summary: { id: 'g2', name: 'Halo' } });
    assert.equal(done, null);
  });

  test('passes BridgeErrors through and turns other errors into internal ones', async (t) => {
    t.mock.method(console, 'error', () => {});
    const { client } = mockClient({
      setup: {
        assignToSlot: () => {
          throw new BridgeError('conflict', 'Slot taken', { reason: 'slotOccupied' });
        },
        autoFill: () => {
          throw new Error('bug');
        },
      },
    });

    await assert.rejects(client.call('setup.assignToSlot', { deviceId: 'd', screenIndex: 0, slotIndex: 0 }), (error: unknown) => {
      assert.ok(BridgeError.is(error, 'conflict'));
      assert.deepEqual(error.data, { reason: 'slotOccupied' });
      return true;
    });
    await assert.rejects(client.call('setup.autoFill'), (error: unknown) => BridgeError.is(error, 'internal') && error.message === 'bug');
  });

  test('answers unknown methods with methodNotFound', async () => {
    const { client } = mockClient({});

    await assert.rejects(client.call('library.list'), (error: unknown) => BridgeError.is(error, 'methodNotFound'));
  });

  test('honours cancellation', async () => {
    const { client } = mockClient({ settings: { captureGamepadCombo: () => delay(50).then(() => 'LB+RB') } });
    const controller = new AbortController();

    const call = client.call('settings.captureGamepadCombo', { buttons: 2 }, { signal: controller.signal });
    controller.abort();

    await assert.rejects(call, (error: unknown) => BridgeError.is(error, 'cancelled'));
  });

  test('emits events asynchronously', async () => {
    const { transport, client } = mockClient({});
    const received: unknown[] = [];
    client.on('window.stateChanged', (e) => received.push(e.state.maximized));

    transport.emit('window.stateChanged', { state: { maximized: true, minimized: false, focused: true, dpiScale: 1 } });
    assert.deepEqual(received, []);
    await delay(5);

    assert.deepEqual(received, [true]);
  });

  test('answers $/filePaths with the file names', async () => {
    const { client } = mockClient({});

    assert.deepEqual(await client.getFilePaths([new File([''], 'handler.nc')]), ['handler.nc']);
  });
});

describe('lazy transport', () => {
  test('delivers messages sent before the transport is loaded, in order', async () => {
    const inner = fakeTransport();
    const client = new BridgeClient({ transport: createLazyTransport(() => delay(10).then(() => inner)), timeoutMs: 0 });

    const first = client.call('library.list');
    void client.call('window.getState');
    await delay(20);
    inner.receive({ jsonrpc: '2.0', id: 1, result: [] });

    assert.deepEqual(
      inner.sent.map((m) => m.method),
      ['library.list', 'window.getState'],
    );
    assert.deepEqual(await first, []);
  });
});

describe('WebView2 transport', () => {
  test('posts strings, attaches files, and forwards string messages only', () => {
    const posted: unknown[] = [];
    const listeners = new Set<(event: { data: unknown }) => void>();
    const webview: WebViewApi = {
      postMessage: (message) => posted.push(message),
      postMessageWithAdditionalObjects: (message, objects) => posted.push([message, objects.length]),
      addEventListener: (_type, listener) => listeners.add(listener),
      removeEventListener: (_type, listener) => listeners.delete(listener),
    };
    const transport = createWebView2Transport(webview);
    const received: string[] = [];

    const off = transport.subscribe((message) => received.push(message));
    transport.send('{"a":1}');
    transport.sendWithFiles?.('{"b":2}', [new File([''], 'x')]);
    listeners.forEach((listener) => [{ data: '{"c":3}' }, { data: { notAString: true } }].forEach((e) => listener(e)));
    off();

    assert.deepEqual(posted, ['{"a":1}', ['{"b":2}', 1]]);
    assert.deepEqual(received, ['{"c":3}']);
    assert.equal(listeners.size, 0);
  });
});
