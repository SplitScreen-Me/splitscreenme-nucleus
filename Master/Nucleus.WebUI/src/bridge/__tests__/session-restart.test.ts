// Restarting a copy of the game (Nucleus 2.5): the card states and Restart control (src/screens/session/instance-status.ts)
// and `session.restartInstance` in the mock, with the host's refusal reasons. Lives with the bridge tests so
// `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { instanceStatus, isClosed, restartNote, restartOffer, statusLabel } from '../../screens/session/instance-status.ts';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import type { SessionInstance, SessionSnapshot } from '../generated/contract.ts';
import { createMockBridgeTransport, type MockBridgeOptions } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

const copy = (over: Partial<SessionInstance>): Pick<SessionInstance, 'stage' | 'crashed' | 'restartFailed'> => ({
  stage: 'ready',
  crashed: false,
  restartFailed: false,
  ...over,
});

test('a copy reads as closed, crashed, restarting or failed to restart', () => {
  assert.equal(instanceStatus(copy({ stage: 'starting' }), false), 'starting');
  assert.equal(instanceStatus(copy({ stage: 'exited' }), false), 'closed');
  assert.equal(instanceStatus(copy({ stage: 'exited', crashed: true }), false), 'crashed');
  assert.equal(instanceStatus(copy({ stage: 'exited', crashed: true, restartFailed: true }), false), 'restartFailed');
  assert.equal(instanceStatus(copy({ stage: 'restarting' }), false), 'restarting');
  assert.equal(instanceStatus(copy({ stage: 'exited', crashed: true }), true), 'closed', 'once the session ended, every copy is closed');
  assert.equal(statusLabel('crashed'), 'Crashed');
  assert.equal(statusLabel('restarting'), 'Restarting…');
  assert.equal(statusLabel('restartFailed'), "Couldn't restart");
  assert.ok(isClosed('restartFailed') && !isClosed('restarting'));
});

test('a closed copy offers Restart unless the host would refuse it', () => {
  assert.equal(restartOffer('crashed', {}, false), 'available');
  assert.equal(restartOffer('closed', {}, false), 'available');
  assert.equal(restartOffer('ready', {}, false), 'none', 'a running copy has no Restart');
  assert.equal(restartOffer('restarting', {}, false), 'none');
  assert.equal(restartOffer('crashed', {}, true), 'none', 'the session ended');
  assert.equal(restartOffer('crashed', { restartRefusal: 'busy' }, false), 'busy', 'disabled while another copy restarts');
  assert.equal(restartOffer('crashed', { restartRefusal: 'notSupported' }, false), 'none');
  assert.equal(restartNote('crashed', { restartRefusal: 'notSupported' }, false), "This handler can't restart a copy of the game.");
  assert.equal(restartNote('crashed', { restartRefusal: 'busy' }, false), 'Another copy is being restarted.');
  assert.equal(restartNote('ready', { restartRefusal: 'notSupported' }, false), undefined);
});

async function until(read: () => Promise<SessionSnapshot>, done: (s: SessionSnapshot) => boolean, ms = 8000): Promise<SessionSnapshot> {
  const end = Date.now() + ms;
  for (;;) {
    const s = await read();
    if (done(s) || Date.now() > end) return s;
    await delay(40);
  }
}

const reasonOf = (error: unknown) => (error instanceof BridgeError ? (error.data as { reason?: string } | undefined)?.reason : undefined);

/** A two-player Terraria session in the mock. */
async function twoPlayers(options: MockBridgeOptions = {}) {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport(options) }));
  await bridge.setup.open({ gameId: 'Terraria' });
  await bridge.setup.join({ deviceId: 'xinput:0' });
  await bridge.setup.join({ deviceId: 'xinput:1' });
  return bridge;
}

test('session.restartInstance refuses with the host reasons, then starts the copy again', async () => {
  const bridge = await twoPlayers();
  try {
    await assert.rejects(bridge.session.restartInstance({ playerNumber: 1 }), (e) => reasonOf(e) === 'noSession');

    const launching = await bridge.session.play();
    assert.equal(launching.restartRefusal, 'notReady');
    await assert.rejects(bridge.session.restartInstance({ playerNumber: 1 }), (e) => reasonOf(e) === 'notReady');

    const running = await until(() => bridge.session.get(), (s) => s.state === 'running');
    assert.equal(running.restartRefusal, undefined, 'a copy can be restarted');
    await assert.rejects(bridge.session.restartInstance({ playerNumber: 9 }), (e) => BridgeError.is(e, 'notFound') && reasonOf(e) === 'unknownPlayer');

    const restarting = await bridge.session.restartInstance({ playerNumber: 2 });
    assert.equal(restarting.instances[1]?.stage, 'restarting');
    assert.equal(restarting.restartRefusal, 'busy');
    await assert.rejects(bridge.session.restartInstance({ playerNumber: 1 }), (e) => reasonOf(e) === 'busy');

    const back = await until(() => bridge.session.get(), (s) => s.instances[1]?.stage === 'ready');
    assert.equal(back.instances[1]?.stage, 'ready');
    assert.equal(back.restartRefusal, undefined);
  } finally {
    await bridge.session.stop().catch(() => undefined);
    await until(() => bridge.session.get(), (s) => s.state === 'setup');
    await bridge.setup.close();
  }
});

test('a crashed copy restarts by itself; when that fails it waits closed for a Restart', async () => {
  const bridge = await twoPlayers({ instance: 'crash:2@50' });
  try {
    await bridge.session.play();
    const crashed = await until(() => bridge.session.get(), (s) => s.instances[1]?.stage === 'exited');
    assert.equal(crashed.instances[1]?.crashed, true);
    assert.equal(crashed.instances[0]?.stage, 'ready', 'the other copy plays on');

    await until(() => bridge.session.get(), (s) => s.instances[1]?.stage === 'restarting');
    const failed = await until(() => bridge.session.get(), (s) => s.instances[1]?.restartFailed === true);
    assert.equal(failed.instances[1]?.stage, 'exited');
    assert.equal(instanceStatus(failed.instances[1]!, false), 'restartFailed');
    assert.equal(restartOffer(instanceStatus(failed.instances[1]!, false), failed, false), 'available');

    const again = await bridge.session.restartInstance({ playerNumber: 2 });
    assert.equal(again.instances[1]?.restartFailed, false);
    const back = await until(() => bridge.session.get(), (s) => s.instances[1]?.stage === 'ready');
    assert.equal(back.instances[1]?.stage, 'ready');
  } finally {
    await bridge.session.stop().catch(() => undefined);
    await until(() => bridge.session.get(), (s) => s.state === 'setup');
    await bridge.setup.close();
  }
});

test('a copy closed normally is not restarted by itself, and reads as closed', async () => {
  const bridge = await twoPlayers({ instance: 'close:1@50' });
  try {
    await bridge.session.play();
    const closed = await until(() => bridge.session.get(), (s) => s.instances[0]?.stage === 'exited');
    await delay(600);
    const later = await bridge.session.get();
    assert.equal(later.instances[0]?.stage, 'exited', 'no automatic restart of a normal close');
    assert.equal(instanceStatus(closed.instances[0]!, false), 'closed');
    assert.equal(later.state, 'running', 'the other copy still plays');
  } finally {
    await bridge.session.stop().catch(() => undefined);
    await until(() => bridge.session.get(), (s) => s.state === 'setup');
    await bridge.setup.close();
  }
});
