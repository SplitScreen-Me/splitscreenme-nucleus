import assert from 'node:assert/strict';
import { test } from 'node:test';
import { BridgeClient } from '../client.ts';
import { createMockBridgeTransport, type MockBridgeOptions } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

function mockBridge(options: MockBridgeOptions = { notices: false }) {
  const client = new BridgeClient({ transport: createMockBridgeTransport(options) });
  return { client, bridge: createBridgeProxy(client) };
}

test('the mock host starts without notices, and lists and dismisses seeded ones', async () => {
  assert.deepEqual(await mockBridge().bridge.notices.list(), []);

  const { client, bridge } = mockBridge({ notices: true });
  const counts: number[] = [];
  client.on('notices.changed', (e) => counts.push(e.count));
  const [first, ...rest] = await bridge.notices.list();
  assert.ok(first && rest.length > 0);
  assert.equal(first.level, 'error');
  assert.match(first.message, /^There is an error in the game handler Broken\.js\./);

  await bridge.notices.dismiss({ id: first.id });
  await bridge.notices.dismiss({ id: 'unknown' });
  await delay(5);

  assert.deepEqual(await bridge.notices.list(), rest);
  assert.deepEqual(counts, [rest.length]);
});

test('turning off "Keep Nucleus user accounts" asks for a confirmation', async () => {
  const { bridge } = mockBridge();

  const fields = (await bridge.settings.getSchema()).sections.flatMap((section) => section.fields);
  const keepAccounts = fields.find((field) => field.key === 'misc.keepAccounts');

  assert.deepEqual(keepAccounts?.confirm?.values, ['false']);
  assert.equal(keepAccounts?.confirm?.confirmLabel, 'Turn off');
});

test('"Don\'t show again" removes the input lock reminder from the setup checks', async () => {
  const { client, bridge } = mockBridge();
  const changed: string[][] = [];
  client.on('settings.changed', (e) => changed.push(e.keys));
  const game = (await bridge.library.list()).find((candidate) => candidate.executableFound);
  assert.ok(game);

  const setup = await bridge.setup.open({ gameId: game.id });
  assert.equal(setup.checks.find((check) => check.id === 'inputLock')?.state, 'info');

  const after = await bridge.setup.runCheckAction({ checkId: 'inputLock' });
  await delay(5);

  assert.equal(after.checks.find((check) => check.id === 'inputLock'), undefined);
  assert.equal((await bridge.settings.getValues())['misc.ignoreInputLockReminder'], 'true');
  assert.ok(changed.some((keys) => keys.includes('misc.ignoreInputLockReminder')));
  await bridge.setup.close();
});
