// Couch navigation rules (src/shell/couch-nav-core.ts). Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  backStep,
  createPadTracker,
  forwardedKey,
  hasRoom,
  isScrollable,
  isTopLevel,
  LEAD_MS,
  mayAct,
  navOrigin,
  pageStep,
  pickInDirection,
  readPad,
  REPEAT_FIRST_MS,
  REPEAT_NEXT_MS,
  shouldPage,
  stepIndex,
  stickDelta,
  wholePixels,
  type Box,
  type PadLike,
} from '../../shell/couch-nav-core.ts';

const pad = (pressed: number[] = [], axes: number[] = [0, 0, 0, 0], mapping = 'standard', index = 0): PadLike => ({
  index,
  mapping,
  axes,
  buttons: Array.from({ length: 17 }, (_, i) => ({ pressed: pressed.includes(i) })),
});

const box = (left: number, top: number, width = 100, height = 40): Box => ({ left, top, right: left + width, bottom: top + height });

test('a standard pad maps the d-pad, the left stick, the action buttons and the right stick', () => {
  const s = readPad(pad([13, 0, 4, 9], [0.7, 0, 0.1, -0.5]));
  assert.deepEqual([s.up, s.down, s.left, s.right], [false, true, false, true]);
  assert.deepEqual(new Map(s.actions), new Map([['A', true], ['B', false], ['X', false], ['Y', false], ['LB', true], ['RB', false], ['View', false], ['Menu', true]]));
  assert.equal(s.scrollX, 0.1);
  assert.equal(s.scrollY, -0.5);
});

test('a pad without the standard mapping only moves with the left stick and presses A and B', () => {
  const s = readPad(pad([0, 2, 12, 13], [0, -0.9, -1, -1], ''));
  assert.deepEqual([s.up, s.down, s.left, s.right], [true, false, false, false]);
  assert.deepEqual(s.actions, [['A', true], ['B', false]]);
  // Axes 2/3 are often triggers resting at -1 there: they must not scroll.
  assert.equal(s.scrollX, 0);
  assert.equal(s.scrollY, 0);
});

test('a small left stick deflection is not a move, and broken axes read as centred', () => {
  const s = readPad(pad([], [0.5, Number.NaN, 0, 0]));
  assert.deepEqual([s.up, s.down, s.left, s.right], [false, false, false, false]);
});

test('a held direction fires on the press, after the first delay, then at the repeat interval', () => {
  const t = createPadTracker();
  const fired: number[] = [];
  for (let now = 0; now <= 800; now += 10) if (t.repeat('d0', true, now)) fired.push(now);
  assert.deepEqual(fired, [0, REPEAT_FIRST_MS, REPEAT_FIRST_MS + REPEAT_NEXT_MS, REPEAT_FIRST_MS + 2 * REPEAT_NEXT_MS, REPEAT_FIRST_MS + 3 * REPEAT_NEXT_MS]);
  assert.equal(t.repeat('d0', false, 810), false);
  assert.equal(t.repeat('d0', true, 820), true, 'a new press fires at once');
});

test('a button fires once per press', () => {
  const t = createPadTracker();
  assert.equal(t.edge('A0', true), true);
  assert.equal(t.edge('A0', true), false);
  assert.equal(t.edge('A0', false), false);
  assert.equal(t.edge('A0', true), true);
});

test('a direction held while navigation was off waits for its release', () => {
  const t = createPadTracker();
  t.hold('u0', true);
  assert.equal(t.repeat('u0', true, 0), false);
  assert.equal(t.repeat('u0', true, 5000), false);
  assert.equal(t.repeat('u0', false, 5010), false);
  assert.equal(t.repeat('u0', true, 5020), true);
});

test('a button held while it could not act does not fire once it can', () => {
  // The poll tracks edges whether or not the press may act; mayAct only gates the action.
  const t = createPadTracker();
  const acted: number[] = [];
  const frame = (pressed: boolean, allowed: boolean, now: number) => {
    if (t.edge('A1', pressed) && allowed) acted.push(now);
  };
  frame(true, false, 0); // pressed while another pad leads: ignored
  frame(true, true, 16); // still held when it becomes the lead: nothing
  frame(false, true, 32);
  frame(true, true, 48); // a new press acts
  assert.deepEqual(acted, [48]);
});

test('on a shared screen only the pad that moved last may press, for a while', () => {
  assert.equal(mayAct(false, null, 2, 0), true, 'not shared: every pad');
  assert.equal(mayAct(true, null, 0, 0), false, 'shared, nobody moved yet');
  const lead = { index: 1, at: 1000 };
  assert.equal(mayAct(true, lead, 1, 1000 + LEAD_MS), true);
  assert.equal(mayAct(true, lead, 0, 1500), false, 'another pad');
  assert.equal(mayAct(true, lead, 1, 1001 + LEAD_MS), false, 'too long ago');
});

test('B closes menus then dialogs, and only goes back where it cannot leave a shared or top-level screen', () => {
  const s = { menuOpen: false, dialogOpen: false, shared: false, topLevel: false, canGoBack: true };
  assert.equal(backStep({ ...s, menuOpen: true, dialogOpen: true }), 'close-menu');
  assert.equal(backStep({ ...s, dialogOpen: true, shared: true }), 'close-dialog');
  assert.equal(backStep(s), 'history');
  assert.equal(backStep({ ...s, shared: true }), 'none');
  assert.equal(backStep({ ...s, topLevel: true }), 'none');
  assert.equal(backStep({ ...s, canGoBack: false }), 'none');
});

test('top-level screens are the rail roots, not the pages under them', () => {
  const roots = ['/library', '/games', '/downloads', '/settings/'];
  assert.equal(isTopLevel('/library', roots), true);
  assert.equal(isTopLevel('/settings', roots), true);
  assert.equal(isTopLevel('/library/', roots), true);
  assert.equal(isTopLevel('/', roots), true);
  assert.equal(isTopLevel('/library/g1/setup', roots), false);
  assert.equal(isTopLevel('/games/abc', roots), false);
  assert.equal(isTopLevel('/session', roots), false);
});

test('widgets get the keys they handle and nothing they would misread', () => {
  assert.equal(forwardedKey(null, 'A'), null);
  assert.equal(forwardedKey('list', 'down'), 'ArrowDown');
  assert.equal(forwardedKey('list', 'left'), 'ArrowLeft');
  assert.equal(forwardedKey('list', 'A'), 'Enter');
  assert.equal(forwardedKey('list', 'B'), 'Escape');
  assert.equal(forwardedKey('region', 'up'), null, 'the d-pad moves between regions spatially');
  assert.equal(forwardedKey('region', 'A'), 'Enter');
  assert.equal(forwardedKey('region', 'B'), 'Escape');
  assert.equal(forwardedKey('capture', 'right'), null);
  assert.equal(forwardedKey('capture', 'A'), null, 'Enter would be recorded as the shortcut');
  assert.equal(forwardedKey('capture', 'B'), 'Escape');
});

test('select stepping skips disabled options and stops at the ends', () => {
  const disabled = [false, true, false, false];
  assert.equal(stepIndex(disabled, 0, 1), 2);
  assert.equal(stepIndex(disabled, 2, -1), 0);
  assert.equal(stepIndex(disabled, 3, 1), 3);
  assert.equal(stepIndex(disabled, 0, -1), 0);
  assert.equal(stepIndex([true, false], -1, 1), 1, 'nothing selected yet');
});

test('scrollers: overflow, room left, and page size', () => {
  assert.equal(isScrollable('auto', 900, 400), true);
  assert.equal(isScrollable('scroll', 900, 400), true);
  assert.equal(isScrollable('hidden', 900, 400), false);
  assert.equal(isScrollable('visible', 900, 400), false);
  assert.equal(isScrollable('auto', 401, 400), false, 'a pixel of rounding is not scrolling');
  assert.equal(hasRoom(0, 900, 400, -1), false);
  assert.equal(hasRoom(0, 900, 400, 1), true);
  assert.equal(hasRoom(500, 900, 400, 1), false);
  assert.equal(hasRoom(500, 900, 400, -1), true);
  assert.equal(pageStep(500, 1), 300);
  assert.equal(pageStep(500, -1), -300);
  assert.equal(pageStep(20, 1), 40, 'tiny scrollers still move');
});

test('right stick: dead zone, linear speed, frame time', () => {
  assert.equal(stickDelta(0.2, 1000 / 60), 0);
  assert.equal(stickDelta(-0.25, 1000 / 60), 0);
  assert.equal(stickDelta(1, 1000 / 60), 20);
  assert.equal(stickDelta(-1, 1000 / 60), -20);
  assert.ok(Math.abs(stickDelta(0.625, 1000 / 60) - 10) < 1e-9, 'half way past the dead zone is half speed');
  assert.ok(Math.abs(stickDelta(1, 1000 / 120) - 10) < 1e-9, 'a 120 Hz frame scrolls half as far');
  assert.ok(Math.abs(stickDelta(1, 500) - 60) < 1e-9, 'a long pause scrolls at most 50 ms worth');
  assert.equal(stickDelta(Number.NaN, 16), 0);
});

test('fractions of a pixel accumulate', () => {
  let acc = 0;
  let moved = 0;
  for (let i = 0; i < 10; i++) {
    const [whole, rest] = wholePixels(acc + 0.4);
    moved += whole;
    acc = rest;
  }
  assert.equal(moved, 4);
  assert.deepEqual(wholePixels(-2.5), [-2, -0.5]);
});

test('the nearest target in a direction wins, favouring alignment', () => {
  const from = box(100, 100);
  const items = [
    { item: 'below-far', box: box(100, 400) },
    { item: 'below-offset', box: box(400, 160) },
    { item: 'below', box: box(110, 200) },
    { item: 'right', box: box(260, 100) },
    { item: 'above', box: box(100, 20) },
  ];
  assert.equal(pickInDirection(from, 'down', items), 'below');
  assert.equal(pickInDirection(from, 'up', items), 'above');
  assert.equal(pickInDirection(from, 'right', items), 'right');
  assert.equal(pickInDirection(from, 'left', items), null);
});

test('a move from a focused element scrolled out of view starts at the visible edge', () => {
  const view: Box = { left: 0, top: 1000, right: 800, bottom: 1500 };
  const above = box(50, 200);
  assert.deepEqual(navOrigin(above, view, 'down'), { left: 50, right: 150, top: 1000, bottom: 1000 });
  assert.deepEqual(navOrigin(above, view, 'up'), { left: 50, right: 150, top: 1500, bottom: 1500 });
  assert.deepEqual(navOrigin(above, view, 'right'), above);
  const inside = box(50, 1100);
  assert.deepEqual(navOrigin(inside, view, 'down'), inside);
  // From the visible edge, the first target on screen is chosen, not the one next to the old focus.
  const items = [
    { item: 'near old focus', box: box(50, 300) },
    { item: 'on screen', box: box(50, 1200) },
  ];
  assert.equal(pickInDirection(navOrigin(above, view, 'down'), 'down', items), 'on screen');
});

test('up/down pages instead of jumping past more than a screen of content', () => {
  const view: Box = { left: 0, top: 0, right: 800, bottom: 500 };
  assert.equal(shouldPage('down', view, null), true);
  assert.equal(shouldPage('down', view, box(0, 600)), false, 'just below the edge');
  assert.equal(shouldPage('down', view, box(0, 1200)), true, 'more than a screen below');
  assert.equal(shouldPage('up', view, box(0, -400)), false);
  assert.equal(shouldPage('up', view, box(0, -900)), true);
});
