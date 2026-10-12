// Resizing a lobby player (src/screens/lobby/resize-core.ts) and the mock's `setup.resize`. Lives with the bridge
// tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  arrowResize,
  clampSteps,
  clampToOthers,
  dragSteps,
  movedFrac,
  otherCorner,
  overlaps,
  pixelSize,
  stepFracOf,
  type Frac,
  type StepFrac,
} from '../../screens/lobby/resize-core.ts';
import { BridgeClient } from '../client.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';

// A quarter of a 4-player layout, resized by twentieths of a slot (the host's step).
const quarter: Frac = { left: 0, top: 0, width: 0.5, height: 0.5 };
const step: StepFrac = { x: 0.025, y: 0.025 };

const close = (actual: number, expected: number) => assert.ok(Math.abs(actual - expected) < 1e-9, `${actual} ≈ ${expected}`);

test('the step of a monitor comes from the host, as shares of the monitor', () => {
  assert.deepEqual(stepFracOf({ bounds: { width: 1920, height: 1080 }, resizeStepWidth: 48, resizeStepHeight: 27 }), { x: 0.025, y: 0.025 });
  assert.equal(stepFracOf({ bounds: { width: 1920, height: 1080 }, resizeStepWidth: 0, resizeStepHeight: 0 }), null);
});

test('a dragged edge moves by whole steps: outwards grows, inwards shrinks', () => {
  assert.equal(dragSteps(quarter, 'right', 0.06, 0.3, step), 2);
  assert.equal(dragSteps(quarter, 'right', -0.051, 0, step), -2);
  assert.equal(dragSteps(quarter, 'bottom', 0.4, 0.011, step), 0);
  assert.equal(dragSteps({ ...quarter, left: 0.5 }, 'left', -0.1, 0, step), 4);
  assert.equal(dragSteps({ ...quarter, top: 0.5 }, 'top', 0, 0.05, step), -2);
});

test('a drag never takes an area off its monitor or below one step', () => {
  assert.equal(dragSteps(quarter, 'left', -0.3, 0, step), 0, 'already at the left edge');
  assert.equal(dragSteps(quarter, 'right', 0.9, 0, step), 20, 'up to the right edge');
  assert.equal(dragSteps(quarter, 'right', -0.9, 0, step), -19, 'one step stays');
  assert.equal(clampSteps(quarter, 'bottom', 3.7, step), 3);
  assert.equal(clampSteps(quarter, 'bottom', 2, { x: 0, y: 0 }), 0);
});

test('a dragged edge stops at another player', () => {
  const neighbour: Frac = { left: 0.6, top: 0, width: 0.4, height: 0.5 };
  const below: Frac = { left: 0, top: 0.5, width: 0.5, height: 0.5 };
  assert.equal(dragSteps(quarter, 'right', 0.5, 0, step, [neighbour, below]), 4, 'up to the neighbour, not over it');
  assert.equal(dragSteps(quarter, 'bottom', 0, 0.2, step, [neighbour, below]), 0, 'touching below already');
  assert.equal(dragSteps(quarter, 'right', -0.1, 0, step, [neighbour]), -4, 'shrinking is never in the way');
  assert.equal(clampToOthers(quarter, 'right', 3, step, []), 3);
  assert.ok(!overlaps(quarter, below), 'a shared edge is not an overlap');
  assert.ok(overlaps(quarter, { left: 0.49, top: 0.49, width: 0.1, height: 0.1 }));
});

test('the preview moves the edge as the host would', () => {
  const right = movedFrac(quarter, 'right', 4, step);
  close(right.width, 0.6);
  assert.equal(right.left, 0);

  const left = movedFrac({ left: 0.5, top: 0.5, width: 0.5, height: 0.5 }, 'left', 2, step);
  close(left.left, 0.45);
  close(left.width, 0.55);

  const top = movedFrac({ left: 0, top: 0.5, width: 0.5, height: 0.5 }, 'top', -2, step);
  close(top.top, 0.55);
  close(top.height, 0.45);
});

test('arrows move the active corner', () => {
  assert.deepEqual(arrowResize('bottomRight', 'ArrowRight'), { edge: 'right', steps: 1 });
  assert.deepEqual(arrowResize('bottomRight', 'ArrowUp'), { edge: 'bottom', steps: -1 });
  assert.deepEqual(arrowResize('topLeft', 'ArrowLeft'), { edge: 'left', steps: 1 });
  assert.deepEqual(arrowResize('topLeft', 'ArrowDown'), { edge: 'top', steps: -1 });
  assert.equal(arrowResize('topLeft', 'Enter'), null);
  assert.equal(otherCorner(otherCorner('topLeft')), 'topLeft');
});

test('the size label reads the area in pixels', () => {
  assert.deepEqual(pixelSize({ left: 0, top: 0, width: 0.525, height: 0.5 }, { width: 1920, height: 1080 }), { width: 1008, height: 540 });
});

test('the mock reports its resize step (a cell) and resizes by cells', async () => {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport() }));
  try {
    await bridge.setup.open({ gameId: 'Terraria' });
    await bridge.setup.setLayout({ screenIndex: 0, layout: 'dualHorizontal' });
    let snap = await bridge.setup.join({ deviceId: 'xinput:0' });
    assert.equal(snap.screens[0]?.resizeStepWidth, 0, 'two slots: no resizing');
    assert.equal(snap.players[0]?.canResize, false);

    await bridge.setup.setLayout({ screenIndex: 0, layout: 'fourPlayers' });
    snap = await bridge.setup.join({ deviceId: 'xinput:0' });
    const screen = snap.screens[0]!;
    const player = snap.players[0]!;
    assert.equal(screen.resizeStepWidth, screen.bounds.width / 2);
    assert.equal(screen.resizeStepHeight, screen.bounds.height / 2);
    assert.ok(player.canResize);

    snap = await bridge.setup.resize({ playerId: player.id, edge: 'right', steps: 1 });
    assert.equal(snap.players[0]?.bounds.width, screen.bounds.width);
    await assert.rejects(bridge.setup.resize({ playerId: player.id, edge: 'right', steps: 1 }), { kind: 'conflict' });
  } finally {
    await bridge.setup.close();
  }
});
