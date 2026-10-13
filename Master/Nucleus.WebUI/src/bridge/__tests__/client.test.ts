import assert from 'node:assert/strict';
import { describe, test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import { eventInvalidations } from '../generated/contract.ts';
import type { MethodName } from '../types.ts';
import { delay, fakeTransport } from './helpers.ts';

/** A client on a fake transport. Calls never time out unless `timeoutMs` is given (unanswered calls stay pending). */
function setup(options: { withFiles?: boolean; timeoutMs?: number } = {}) {
  const transport = fakeTransport(options);
  const invalidated: MethodName[] = [];
  const client = new BridgeClient({ transport, timeoutMs: options.timeoutMs ?? 0, invalidate: (method) => invalidated.push(method) });
  return { transport, client, invalidated };
}

describe('calls', () => {
  test('send a JSON-RPC request and resolve with the result', async () => {
    const { transport, client } = setup();

    const info = client.call('library.get', { gameId: 'g1' });
    transport.receive({ jsonrpc: '2.0', id: 1, result: { summary: { name: 'Halo' } } });

    assert.deepEqual(transport.sent, [{ jsonrpc: '2.0', id: 1, method: 'library.get', params: { gameId: 'g1' } }]);
    assert.deepEqual(await info, { summary: { name: 'Halo' } });
  });

  test('omit params for methods without params, and number ids in order', async () => {
    const { transport, client } = setup();

    void client.call('library.list');
    void client.call('window.getState');

    assert.deepEqual(transport.sent, [
      { jsonrpc: '2.0', id: 1, method: 'library.list' },
      { jsonrpc: '2.0', id: 2, method: 'window.getState' },
    ]);
  });

  test('resolve void methods with null', async () => {
    const { transport, client } = setup();

    const done = client.call('window.minimize');
    transport.receive({ jsonrpc: '2.0', id: 1, result: null });

    assert.equal(await done, null);
  });

  test('reject with a BridgeError carrying kind, code and data', async () => {
    const { transport, client } = setup();

    const call = client.call('setup.assignToSlot', { deviceId: 'xinput:0', screenIndex: 0, slotIndex: 1 });
    transport.receive({ jsonrpc: '2.0', id: 1, error: { code: -32002, message: 'Slot taken', data: { reason: 'slotOccupied' } } });

    await assert.rejects(call, (error: unknown) => {
      assert.ok(BridgeError.is(error, 'conflict'));
      assert.equal(error.code, -32002);
      assert.equal(error.message, 'Slot taken');
      assert.deepEqual(error.data, { reason: 'slotOccupied' });
      return true;
    });
  });

  test('map unknown error codes to internal', async () => {
    const { transport, client } = setup();

    const call = client.call('library.list');
    transport.receive({ jsonrpc: '2.0', id: 1, error: { code: 42, message: 'Odd' } });

    await assert.rejects(call, (error: unknown) => BridgeError.is(error, 'internal') && error.message.includes('42'));
  });
});

describe('timeouts and cancellation', () => {
  test('time out, tell the host to cancel, and ignore the late answer', async () => {
    const { transport, client } = setup({ timeoutMs: 10 });

    const call = client.call('library.list');

    await assert.rejects(call, (error: unknown) => BridgeError.is(error, 'timeout') && error.code === -32099);
    assert.deepEqual(transport.sent[1], { jsonrpc: '2.0', method: '$/cancel', params: { id: 1 } });
    transport.receive({ jsonrpc: '2.0', id: 1, result: [] });
  });

  test('let a call override the default timeout', async () => {
    const { transport, client } = setup({ timeoutMs: 10 });

    const call = client.call('library.list', undefined, { timeoutMs: 0 });
    await delay(30);
    transport.receive({ jsonrpc: '2.0', id: 1, result: [] });

    assert.deepEqual(await call, []);
  });

  test('cancel through an AbortSignal', async () => {
    const { transport, client } = setup();
    const controller = new AbortController();

    const call = client.call('settings.captureGamepadCombo', { buttons: 2 }, { signal: controller.signal });
    controller.abort();

    await assert.rejects(call, (error: unknown) => BridgeError.is(error, 'cancelled'));
    assert.deepEqual(transport.sent[1], { jsonrpc: '2.0', method: '$/cancel', params: { id: 1 } });
  });

  test('reject at once, without sending, when the signal is already aborted', async () => {
    const { transport, client } = setup();

    await assert.rejects(client.call('library.list', undefined, { signal: AbortSignal.abort() }), (error: unknown) =>
      BridgeError.is(error, 'cancelled'),
    );
    assert.equal(transport.sent.length, 0);
  });

  test('reject pending calls on dispose', async () => {
    const { client } = setup();

    const call = client.call('library.list');
    client.dispose();

    await assert.rejects(call, (error: unknown) => BridgeError.is(error, 'cancelled'));
  });
});

describe('events', () => {
  test('reach their listeners until they unsubscribe', () => {
    const { transport, client } = setup();
    const received: string[] = [];

    const off = client.on('setup.deviceActivity', (e) => received.push(e.deviceId));
    transport.receive({ jsonrpc: '2.0', method: 'setup.deviceActivity', params: { deviceId: 'xinput:0', active: true } });
    off();
    transport.receive({ jsonrpc: '2.0', method: 'setup.deviceActivity', params: { deviceId: 'xinput:1', active: true } });

    assert.deepEqual(received, ['xinput:0']);
  });

  test('invalidate the methods listed in the contract, after the listeners ran', () => {
    const { transport, client, invalidated } = setup();
    const order: string[] = [];
    client.on('library.changed', () => order.push(`listener (${invalidated.length} invalidated)`));

    transport.receive({ jsonrpc: '2.0', method: 'library.changed', params: { gameIds: [] } });

    assert.deepEqual(invalidated, [...eventInvalidations['library.changed']]);
    assert.ok(invalidated.includes('library.list'));
    assert.deepEqual(order, ['listener (0 invalidated)']);
  });

  test('keep dispatching when a listener throws', (t) => {
    const { transport, client } = setup();
    const error = t.mock.method(console, 'error', () => {});
    let reached = false;
    client.on('session.notice', () => {
      throw new Error('boom');
    });
    client.on('session.notice', () => (reached = true));

    transport.receive({ jsonrpc: '2.0', method: 'session.notice', params: { text: 'Hi', durationMs: 1000 } });

    assert.ok(reached);
    assert.equal(error.mock.callCount(), 1);
  });

  test('ignore malformed messages', (t) => {
    const { transport } = setup();
    const warn = t.mock.method(console, 'warn', () => {});

    transport.receive('not json');
    transport.receive('42');
    transport.receive({ jsonrpc: '2.0', id: null, error: { code: -32700, message: 'Parse error' } });

    assert.equal(warn.mock.callCount(), 3);
  });
});

describe('file paths', () => {
  const file = (name: string) => new File(['x'], name);

  test('send $/filePaths with the files attached', async () => {
    const { transport, client } = setup();

    const paths = client.getFilePaths([file('a.nc'), file('b.zip')]);
    transport.receive({ jsonrpc: '2.0', id: 1, result: ['C:\\a.nc', 'C:\\b.zip'] });

    assert.deepEqual(await paths, ['C:\\a.nc', 'C:\\b.zip']);
    assert.deepEqual(transport.sent, [{ jsonrpc: '2.0', id: 1, method: '$/filePaths' }]);
    assert.deepEqual(
      transport.files[0]?.map((f) => f.name),
      ['a.nc', 'b.zip'],
    );
  });

  test('resolve an empty list without a round trip', async () => {
    const { transport, client } = setup();

    assert.deepEqual(await client.getFilePaths([]), []);
    assert.equal(transport.sent.length, 0);
  });

  test('reject with unavailable when the transport cannot attach files', async () => {
    const { client } = setup({ withFiles: false });

    await assert.rejects(
      client.getFilePaths([file('a.nc')]),
      (error: unknown) => BridgeError.is(error, 'unavailable') && (error.data as { reason?: string }).reason === 'noFilePaths',
    );
  });
});
