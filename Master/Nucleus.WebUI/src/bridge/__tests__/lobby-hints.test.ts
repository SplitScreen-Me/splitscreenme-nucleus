// Lobby hint bar (src/screens/lobby/hints.ts): joining, the place waiting for a controller and resize mode share it.
// Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import { lobbyHints, PLAYER_RESIZABLE, PLAYER_RESIZING, playerHintsKey, type LobbyHint } from '../../screens/lobby/hints.ts';

function actions() {
  const calls: string[] = [];
  return {
    calls,
    actions: {
      cancelWait: () => calls.push('cancel'),
      resize: () => calls.push('resize'),
      otherCorner: () => calls.push('corner'),
      addGuest: () => calls.push('addGuest'),
    },
  };
}

const shown = (hints: readonly LobbyHint[]) => hints.map((h) => `${h.button ?? h.key} ${h.label}`);
const run = (hints: readonly LobbyHint[] | undefined, button: LobbyHint['button']) => hints?.find((h) => h.button === button)?.action?.();
/** The `data-hints` key of a player area that has one. */
function hintsKeyOf(area: Parameters<typeof playerHintsKey>[0]): string {
  const key = playerHintsKey(area);
  assert.ok(key);
  return key;
}

test('Start joins and A selects, whatever has focus', () => {
  const { actions: a } = actions();
  const hints = lobbyHints(false, a);
  assert.deepEqual(shown(hints.base), ['Menu Join', 'A Select']);
  // Neither runs a hint action: Start is the host's press, A presses the focused control.
  assert.ok(hints.base.every((h) => !h.action));
});

test('a player area that can be resized adds X Resize, which starts resize mode', () => {
  const { calls, actions: a } = actions();
  const hints = lobbyHints(false, a).byFocus?.[PLAYER_RESIZABLE];
  assert.deepEqual(shown(hints ?? []), ['Menu Join', 'A Select', 'X Resize']);
  run(hints, 'X');
  assert.deepEqual(calls, ['resize']);
});

test('resize mode: arrows move the corner, X switches corners, A is done', () => {
  const { calls, actions: a } = actions();
  const hints = lobbyHints(false, a).byFocus?.[PLAYER_RESIZING];
  assert.deepEqual(shown(hints ?? []), ['Arrows Move the corner', 'X Other corner', 'A Done']);
  run(hints, 'X');
  assert.deepEqual(calls, ['corner']);
  // A reaches the area as Enter (data-pad-keys), which ends resize mode: no hint action.
  assert.equal(hints?.find((h) => h.button === 'A')?.action, undefined);
});

test('a place waiting for a controller leaves only B Cancel, even on a player area', () => {
  const { calls, actions: a } = actions();
  const hints = lobbyHints(true, a);
  assert.deepEqual(shown(hints.base), ['B Cancel']);
  assert.equal(hints.byFocus, undefined);
  run(hints.base, 'B');
  assert.deepEqual(calls, ['cancel']);
});

test('outside resize mode, A only ever says Select', () => {
  const { actions: a } = actions();
  const hints = lobbyHints(false, a);
  const all = [hints.base, ...Object.entries(hints.byFocus ?? {}).filter(([k]) => k !== PLAYER_RESIZING).map(([, v]) => v)];
  for (const set of all) assert.deepEqual(set.filter((h) => h.button === 'A').map((h) => h.label), ['Select']);
});

test('a player area whose copy has room for a guest adds Y Add a controller, which starts waiting for one', () => {
  const { calls, actions: a } = actions();
  const byFocus = lobbyHints(false, a).byFocus;
  const guests = byFocus?.[hintsKeyOf({ resizable: false, guestRoom: true })];
  assert.deepEqual(shown(guests ?? []), ['Menu Join', 'A Select', 'Y Add a controller']);
  run(guests, 'Y');
  assert.deepEqual(calls, ['addGuest']);
  const both = byFocus?.[hintsKeyOf({ resizable: true, guestRoom: true })];
  assert.deepEqual(shown(both ?? []), ['Menu Join', 'A Select', 'X Resize', 'Y Add a controller']);
});

test('the hints of a player area follow what it can do', () => {
  assert.equal(playerHintsKey({ resizable: false, guestRoom: false }), undefined);
  assert.equal(playerHintsKey({ resizable: true, guestRoom: false }), PLAYER_RESIZABLE);
  assert.equal(playerHintsKey({ resizable: true, guestRoom: false, resizing: true }), PLAYER_RESIZING);
  assert.equal(playerHintsKey({ resizable: true, guestRoom: true, resizing: true }), PLAYER_RESIZING);
  const keys = new Set([PLAYER_RESIZABLE, PLAYER_RESIZING, playerHintsKey({ resizable: false, guestRoom: true }), playerHintsKey({ resizable: true, guestRoom: true })]);
  assert.equal(keys.size, 4, 'one hint set each');
});
