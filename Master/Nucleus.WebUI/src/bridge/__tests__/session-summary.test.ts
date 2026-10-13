// The summary of the last session: its wording (src/screens/session/summary.ts) and the mock's
// `SessionSnapshot.lastSession`. Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  errorsLabel,
  summaryDevices,
  summaryDuration,
  summaryOutcome,
  summaryPlayerName,
  summaryTitle,
} from '../../screens/session/summary.ts';
import { BridgeClient } from '../client.ts';
import type { SessionSnapshot } from '../generated/contract.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

test('the summary says how the session went', () => {
  assert.equal(summaryOutcome({ error: 'The game was not found.', launched: false }), 'failed');
  assert.equal(summaryOutcome({ launched: false }), 'stoppedEarly');
  assert.equal(summaryOutcome({ launched: true }), 'ended');
  assert.equal(summaryTitle({ launched: true }), 'Session ended');
  assert.equal(summaryTitle({ error: 'x', launched: true }), 'The session could not start');
  assert.equal(summaryDuration({ startedAt: '2026-10-12T20:00:00Z', endedAt: '2026-10-12T21:02:03.900Z' }), '1:02:03');
  assert.equal(summaryDuration({ startedAt: 'never', endedAt: '2026-10-12T21:02:03Z' }), '0:00:00');
  assert.equal(errorsLabel({ errorCount: 0 }), 'No errors in the session log.');
  assert.equal(errorsLabel({ errorCount: 1 }), '1 error in the session log');
  assert.equal(errorsLabel({ errorCount: 21 }), '21 errors in the session log');
  assert.equal(errorsLabel({ errorCount: 1500 }), '1,500 errors in the session log');
});

test('players are named, and their guests listed apart', () => {
  const player = {
    number: 2,
    nickname: ' ',
    devices: [
      { kind: 'xInput' as const, name: 'Xbox Wireless Controller', guest: false },
      { kind: 'dInput' as const, name: 'DualSense', guest: true },
    ],
  };
  assert.equal(summaryPlayerName(player), 'Player 2');
  assert.equal(summaryPlayerName({ number: 1, nickname: 'Léa' }), 'Léa');
  const { own, guests } = summaryDevices(player);
  assert.deepEqual(
    own.map((d) => d.name),
    ['Xbox Wireless Controller'],
  );
  assert.deepEqual(
    guests.map((d) => d.name),
    ['DualSense'],
  );
});

async function until(read: () => Promise<SessionSnapshot>, done: (s: SessionSnapshot) => boolean, ms = 6000): Promise<SessionSnapshot> {
  const end = Date.now() + ms;
  for (;;) {
    const s = await read();
    if (done(s) || Date.now() > end) return s;
    await delay(50);
  }
}

test('the mock keeps a summary of the last session until the next one starts', async () => {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport() }));
  try {
    await bridge.setup.open({ gameId: 'Terraria' });
    await bridge.setup.join({ deviceId: 'xinput:0' });
    const placed = await bridge.setup.join({ deviceId: 'xinput:1' });
    assert.equal(placed.players.length, 2);

    const started = await bridge.session.play();
    assert.equal(started.lastSession, undefined);
    const running = await until(() => bridge.session.get(), (s) => s.state === 'running');
    assert.equal(running.state, 'running');

    await bridge.session.stop();
    const ended = await until(() => bridge.session.get(), (s) => s.state === 'setup');
    const summary = ended.lastSession;
    assert.ok(summary);
    assert.equal(summary.gameId, 'Terraria');
    assert.equal(summary.gameName, 'Terraria');
    assert.equal(summary.launched, true);
    assert.equal(summary.error, undefined);
    assert.equal(summary.errorCount, 0);
    assert.ok(Date.parse(summary.endedAt) >= Date.parse(summary.startedAt));
    assert.deepEqual(
      summary.players.map((p) => [p.number, p.devices.map((d) => d.name)]),
      [
        [1, ['Xbox Wireless Controller']],
        [2, ['8BitDo Pro 2']],
      ],
    );

    const again = await bridge.session.play();
    assert.equal(again.lastSession, undefined, 'a new session clears the summary');
    await bridge.session.stop();
    const stoppedEarly = await until(() => bridge.session.get(), (s) => s.state === 'setup');
    assert.equal(stoppedEarly.lastSession?.launched, false, 'stopped before every window was ready');
  } finally {
    await bridge.setup.close();
  }
});
