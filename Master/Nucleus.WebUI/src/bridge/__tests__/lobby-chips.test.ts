// The keyboard and the mouse of a player dragged on their own (src/screens/lobby/device-chips.ts) and what the mock does
// with them, like the host; and the lobby with an SDL handler (SDL2 or SDL3 pads are `sdl` devices). Lives with the
// bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { deviceChips, deviceDropAction, trayTakes } from '../../screens/lobby/device-chips.ts';
import { pressJoins, pressTakesSpot } from '../../screens/lobby/join-rules.ts';
import { BridgeClient } from '../client.ts';
import type { SetupSnapshot } from '../generated/contract.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

const KB = 'rawkb:046d-c33e';
const MOUSE = 'rawmouse:046d-c08b';

test('a keyboard and mouse player shows one chip per device; other players none', () => {
  const devices = [{ id: KB }, { id: MOUSE }, { id: 'xinput:0' }];
  assert.deepEqual(
    deviceChips({ id: KB, deviceIds: [KB, MOUSE] }, devices).map((d) => d.id),
    [KB, MOUSE],
  );
  assert.deepEqual(deviceChips({ id: 'xinput:0', deviceIds: ['xinput:0'] }, devices), []);
  assert.deepEqual(deviceChips({ id: KB, deviceIds: [KB] }, devices), [], 'a lone keyboard drags with its area');
});

test('a dropped device moves alone; back on its own player or a free one on the tray does nothing', () => {
  const mouse = { id: MOUSE, playerId: KB };
  assert.equal(deviceDropAction(mouse, { kind: 'player', playerId: KB }), 'none');
  assert.equal(deviceDropAction(mouse, { kind: 'player', playerId: 'xinput:0' }), 'place', 'the host swaps or completes');
  assert.equal(deviceDropAction(mouse, { kind: 'place' }), 'place');
  assert.equal(deviceDropAction(mouse, { kind: 'tray' }), 'unassign');
  assert.equal(deviceDropAction({ id: 'xinput:1' }, { kind: 'tray' }), 'none');
  assert.equal(deviceDropAction({ id: 'xinput:1' }, { kind: 'player', playerId: 'xinput:0' }, true), 'guest');
  assert.equal(trayTakes('player', undefined), true);
  assert.equal(trayTakes('device', mouse), true);
  assert.equal(trayTakes('device', { id: 'xinput:1' }), false, 'a free device is in the tray already');
  assert.equal(trayTakes(null, mouse), false, 'a file');
});

async function until(read: () => Promise<SetupSnapshot | null>, done: (s: SetupSnapshot) => boolean, ms = 6000): Promise<SetupSnapshot> {
  const end = Date.now() + ms;
  for (;;) {
    const s = await read();
    if (s && (done(s) || Date.now() > end)) return s;
    await delay(50);
  }
}

const slotOf = (s: SetupSnapshot, playerId: string) => s.screens[0]?.slots.find((slot) => slot.playerId === playerId)?.index;
const owner = (s: SetupSnapshot, deviceId: string) => s.players.find((p) => p.deviceIds.includes(deviceId));

test('the mock moves one device of a keyboard and mouse pair on its own, as the host does', async () => {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport() }));
  try {
    await bridge.setup.open({ gameId: 'EldenRing' });
    // Raw keyboards and mice appear once used (the mock uses them after a moment).
    await until(() => bridge.setup.get(), (s) => [KB, MOUSE].every((id) => s.devices.find((d) => d.id === id)?.revealed));
    await bridge.setup.join({ deviceId: KB });
    const pair = await bridge.setup.join({ deviceId: MOUSE });
    assert.deepEqual(pair.players.map((p) => p.deviceIds), [[KB, MOUSE]], 'the mouse completes the keyboard');
    const home = slotOf(pair, KB)!;

    // The mouse chip to an empty place: a player of its own, the keyboard stays.
    const empty = pair.screens[0]!.slots.find((slot) => !slot.playerId)!.index;
    const split = await bridge.setup.assignToSlot({ deviceId: MOUSE, screenIndex: 0, slotIndex: empty });
    assert.equal(split.players.length, 2);
    assert.deepEqual(owner(split, KB)?.deviceIds, [KB]);
    assert.deepEqual(owner(split, MOUSE)?.deviceIds, [MOUSE]);
    assert.equal(slotOf(split, KB), home);

    // Back on the lone keyboard: one player again.
    const joined = await bridge.setup.assignToSlot({ deviceId: MOUSE, screenIndex: 0, slotIndex: home });
    assert.deepEqual(joined.players.map((p) => p.deviceIds), [[KB, MOUSE]]);

    // The whole player moves with its area: first device, then the other one follows (the lobby's movePlayer).
    const target = joined.screens[0]!.slots.find((slot) => !slot.playerId)!.index;
    await bridge.setup.assignToSlot({ deviceId: KB, screenIndex: 0, slotIndex: target });
    const moved = await bridge.setup.assignToSlot({ deviceId: MOUSE, screenIndex: 0, slotIndex: target });
    assert.deepEqual(moved.players.map((p) => p.deviceIds), [[KB, MOUSE]]);
    assert.equal(slotOf(moved, KB), target);

    // The mouse chip on the tray: only the mouse leaves.
    const left = await bridge.setup.unassign({ deviceId: MOUSE });
    assert.deepEqual(left.players.map((p) => p.deviceIds), [[KB]]);
    assert.equal(left.devices.find((d) => d.id === MOUSE)?.playerId, undefined);
  } finally {
    await bridge.setup.close();
  }
});

test('an SDL handler lists SDL pads that join like other gamepads', async () => {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport() }));
  try {
    const setup = await bridge.setup.open({ gameId: 'Barony' });
    const pads = setup.devices.filter((d) => d.kind === 'sdl');
    assert.deepEqual(
      pads.map((d) => d.id),
      ['sdl:0', 'sdl:1'],
    );
    assert.ok(setup.devices.every((d) => d.kind !== 'xInput' && d.kind !== 'dInput'), 'no XInput or DirectInput view of the pads');
    assert.ok(pads.every((d) => d.needsButtonPress && !d.revealed), 'identified by a press, like DirectInput pads');
    assert.equal(setup.capabilities.gamepads, true);

    // Start on a revealed pad joins it, like any gamepad.
    assert.equal(pressJoins(['start']), true);
    assert.equal(pressTakesSpot(null, ['a'], 0), false);
    await bridge.setup.setUseApiIndex({ enabled: true });
    const joined = await bridge.setup.join({ deviceId: 'sdl:1' });
    assert.deepEqual(joined.players.map((p) => p.deviceIds), [['sdl:1']]);
    assert.equal(joined.devices.find((d) => d.id === 'sdl:1')?.kind, 'sdl');
  } finally {
    await bridge.setup.close();
  }
});
