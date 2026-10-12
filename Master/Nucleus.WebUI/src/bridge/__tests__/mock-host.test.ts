import assert from 'node:assert/strict';
import { test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { contractHash, protocolVersion, type WindowState } from '../generated/contract.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';

function mockBridge() {
  const client = new BridgeClient({ transport: createMockBridgeTransport() });
  return { client, bridge: createBridgeProxy(client) };
}

test('the mock host passes the handshake', async () => {
  const { bridge } = mockBridge();

  const hello = await bridge.app.hello();

  assert.equal(hello.contractHash, contractHash);
  assert.equal(hello.protocolVersion, protocolVersion);
});

test('the mock host serves every service', async () => {
  const { bridge } = mockBridge();

  assert.ok((await bridge.library.list()).length > 0);
  assert.ok((await bridge.handlers.listInstalled()).length > 0);
  assert.ok((await bridge.settings.getSchema()).sections.length > 0);
  assert.equal(await bridge.setup.get(), null);
  assert.equal((await bridge.session.get()).state, 'idle');
  assert.equal(typeof (await bridge.dialogs.pickFolder({ title: 'Folder' })), 'string');
});

test('the mock host emits events like the host', async () => {
  const { client, bridge } = mockBridge();
  const states: WindowState[] = [];
  client.on('window.stateChanged', (e) => states.push(e.state));

  await bridge.window.toggleMaximize();
  await new Promise((resolve) => setTimeout(resolve, 5));

  assert.deepEqual(
    states.map((s) => s.maximized),
    [true],
  );
});
