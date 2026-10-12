// Lobby press-to-join rules (src/screens/lobby/join-rules.ts). Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { PICK_GRACE_MS, pressJoins, pressTakesSpot, sameSpot, spotStillOpen, type SpotScreen, type WaitingSpot } from '../../screens/lobby/join-rules.ts';

const slotSpot = (over: Partial<WaitingSpot> = {}): WaitingSpot => ({
  screenIndex: 0,
  slotIndex: 1,
  x: 0.5,
  y: 0.5,
  layout: 'dualVertical',
  at: 1000,
  byPad: false,
  ...over,
});

const screens = (over: Partial<SpotScreen> = {}): SpotScreen[] => [
  { index: 0, layout: 'dualVertical', slots: [{ index: 0, playerId: 'xinput:0' }, { index: 1 }], ...over },
  { index: 1, layout: 'manual', slots: [] },
];

test('only Start joins: A, B, the d-pad and unreadable presses do not', () => {
  assert.equal(pressJoins(['start']), true);
  assert.equal(pressJoins(['a', 'start']), true);
  assert.equal(pressJoins(['a']), false);
  assert.equal(pressJoins(['b']), false);
  assert.equal(pressJoins(['dPadDown']), false);
  assert.equal(pressJoins(['other']), false);
  assert.equal(pressJoins([]), false);
  assert.equal(pressJoins(undefined), false);
});

test('a waiting place takes the next press of any button, Start included', () => {
  const spot = slotSpot();
  for (const buttons of [['a'], ['start'], ['x'], ['dPadUp'], ['other'], ['b', 'a']]) {
    assert.equal(pressTakesSpot(spot, buttons, 1001), true, buttons.join('+'));
  }
});

test('B alone cancels the wait instead of taking the place', () => {
  assert.equal(pressTakesSpot(slotSpot(), ['b'], 5000), false);
});

test('nothing is taken without a waiting place, or without a readable press', () => {
  assert.equal(pressTakesSpot(null, ['a'], 5000), false);
  assert.equal(pressTakesSpot(slotSpot(), undefined, 5000), false);
  assert.equal(pressTakesSpot(slotSpot(), [], 5000), false);
});

test('the pad press that picked the place, reported late by the host, does not take it', () => {
  const spot = slotSpot({ byPad: true, at: 1000 });
  assert.equal(pressTakesSpot(spot, ['a'], 1000 + PICK_GRACE_MS - 1), false);
  assert.equal(pressTakesSpot(spot, ['a'], 1000 + PICK_GRACE_MS), true);
  // A place picked with the mouse or the keyboard has no pad press on its way.
  assert.equal(pressTakesSpot(slotSpot({ byPad: false, at: 1000 }), ['a'], 1001), true);
});

test('a waiting slot stays open while it exists, is empty and its screen keeps its layout', () => {
  assert.equal(spotStillOpen(slotSpot(), screens()), true);
  assert.equal(spotStillOpen(slotSpot({ slotIndex: 0 }), screens()), false, 'taken');
  assert.equal(spotStillOpen(slotSpot({ slotIndex: 3 }), screens()), false, 'gone');
  assert.equal(spotStillOpen(slotSpot(), screens({ layout: 'fourPlayers' })), false, 'layout changed');
  assert.equal(spotStillOpen(slotSpot({ screenIndex: 7 }), screens()), false, 'screen gone');
});

test('a waiting point of a manual screen stays open while the screen stays manual', () => {
  const point = slotSpot({ screenIndex: 1, slotIndex: undefined, layout: 'manual', x: 0.2, y: 0.7 });
  assert.equal(spotStillOpen(point, screens()), true);
  assert.equal(spotStillOpen({ ...point, layout: 'manual' }, [{ index: 1, layout: 'fullScreen', slots: [{ index: 0 }] }]), false);
});

test('the same place is recognised, to cancel when it is activated again', () => {
  assert.equal(sameSpot(slotSpot(), { screenIndex: 0, slotIndex: 1 }), true);
  assert.equal(sameSpot(slotSpot(), { screenIndex: 0, slotIndex: 0 }), false);
  assert.equal(sameSpot(slotSpot(), { screenIndex: 1, slotIndex: 1 }), false);
  assert.equal(sameSpot(null, { screenIndex: 0, slotIndex: 1 }), false);
  assert.equal(sameSpot(slotSpot({ screenIndex: 1, slotIndex: undefined }), { screenIndex: 1, slotIndex: undefined }), true);
});
