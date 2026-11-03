// The hub status in the UI (src/lib/hub-status-core.ts): the offline banner follows the host both ways, "Retry now",
// WebView requests feeding the status without storms, and the update flags hidden while offline (the host and the mock).
// Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  ASK_INTERVAL_MS,
  cameBackOnline,
  hubKnown,
  offlineBannerState,
  reachedHub,
  shouldAskHost,
} from '../../lib/hub-status-core.ts';
import { BridgeClient } from '../client.ts';
import { eventInvalidations } from '../generated/contract.ts';
import { createMockBridgeTransport, type MockBridgeOptions } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

function mockBridge(options: MockBridgeOptions = {}) {
  const client = new BridgeClient({ transport: createMockBridgeTransport(options) });
  return { client, bridge: createBridgeProxy(client) };
}

test('the host status is unknown until its first check', () => {
  assert.equal(hubKnown(undefined), 'unknown');
  assert.equal(hubKnown({ hubOnline: false, hubChecked: false }), 'unknown', 'Hub.Connected starts false: not offline yet');
  assert.equal(hubKnown({ hubOnline: false, hubChecked: true }), 'offline');
  assert.equal(hubKnown({ hubOnline: true, hubChecked: true }), 'online');
});

test('the banner shows while offline only, with the state of "Retry now"', () => {
  assert.equal(offlineBannerState('unknown', 'idle'), 'hidden', 'no banner flash at startup');
  assert.equal(offlineBannerState('online', 'failed'), 'hidden');
  assert.equal(offlineBannerState('offline', 'idle'), 'offline');
  assert.equal(offlineBannerState('offline', 'checking'), 'checking');
  assert.equal(offlineBannerState('offline', 'failed'), 'stillOffline');
  assert.equal(cameBackOnline('offline', 'online'), true);
  assert.equal(cameBackOnline('unknown', 'online'), false, 'the first check is not a comeback');
  assert.equal(cameBackOnline('online', 'online'), false);
});

test('a WebView hub request reached the hub when it got an answer below 500', () => {
  assert.equal(reachedHub(null), true);
  assert.equal(reachedHub({ name: 'HubError' }), false, 'no answer (network failure)');
  assert.equal(reachedHub({ name: 'HubError', status: 503 }), false, 'the proxy of a site that is down');
  assert.equal(reachedHub({ name: 'HubError', status: 404 }), true);
  assert.equal(reachedHub({ name: 'HubError', status: 429 }), true, 'busy, but up');
  assert.equal(reachedHub({ name: 'AbortError' }), undefined, 'a cancelled request says nothing');
  assert.equal(reachedHub(undefined), undefined);
});

test('the UI asks the host to check only when it disagrees, at most every 30 s', () => {
  const now = 1_000_000;
  assert.equal(shouldAskHost('online', true, undefined, now), false, 'agrees');
  assert.equal(shouldAskHost('offline', false, undefined, now), false, 'agrees');
  assert.equal(shouldAskHost('unknown', false, undefined, now), false, 'the host checks first by itself');
  assert.equal(shouldAskHost('online', undefined, undefined, now), false);
  assert.equal(shouldAskHost('online', false, undefined, now), true, 'a failure while online');
  assert.equal(shouldAskHost('offline', true, undefined, now), true, 'it works while offline');
  assert.equal(shouldAskHost('online', false, now - 1000, now), false, 'no probe storm');
  assert.equal(shouldAskHost('online', false, now - ASK_INTERVAL_MS, now), true);
});

test('app.hubStatusChanged refetches the app info and the library (its update flags)', () => {
  const invalidated = eventInvalidations['app.hubStatusChanged'] as readonly string[];
  assert.ok(invalidated.includes('app.getInfo'));
  assert.ok(invalidated.includes('library.list'));
  assert.ok(invalidated.includes('library.get'));
});

test('offline, the mock hides the update flags; a forced check brings the hub and the flags back', async () => {
  const { client, bridge } = mockBridge({ hub: 'offline' });
  const events: boolean[] = [];
  client.on('app.hubStatusChanged', (e) => events.push(e.online));

  const offline = await bridge.app.getInfo();
  assert.equal(offline.hubOnline, false);
  assert.equal(offline.hubChecked, true);
  assert.ok((await bridge.library.list()).every((g) => !g.updateAvailable), 'no update badge or button while offline');

  assert.equal(await bridge.handlers.checkHub({ force: false }), false, 'a WebView report does not move the simulated hub');
  assert.equal(await bridge.handlers.checkHub({ force: true }), true, 'Retry now');
  await delay(5);

  assert.deepEqual(events, [true]);
  assert.equal((await bridge.app.getInfo()).hubOnline, true);
  assert.ok((await bridge.library.list()).some((g) => g.updateAvailable), 'the update flags are back');
  const peak = await bridge.library.get({ gameId: 'Peak' });
  assert.equal(peak.summary.updateAvailable, true);
});

test('a hub that stays down answers the retry with offline, without an event', async () => {
  const { client, bridge } = mockBridge({ hub: 'down' });
  const events: boolean[] = [];
  client.on('app.hubStatusChanged', (e) => events.push(e.online));

  assert.equal(await bridge.handlers.checkHub({ force: true }), false);
  await delay(5);
  assert.deepEqual(events, []);
  assert.equal((await bridge.app.getInfo()).hubOnline, false);
});

test('the setup does not offer a handler update while the hub is offline', async () => {
  const { bridge } = mockBridge({ hub: 'offline' });
  try {
    const setup = await bridge.setup.open({ gameId: 'Peak' });
    assert.notEqual(setup.checks.find((c) => c.id === 'handlerUpToDate')?.state, 'warning');
    await bridge.handlers.checkHub({ force: true });
    const online = await bridge.setup.get();
    assert.equal(online?.checks.find((c) => c.id === 'handlerUpToDate')?.state, 'warning');
  } finally {
    await bridge.setup.close();
  }
});
