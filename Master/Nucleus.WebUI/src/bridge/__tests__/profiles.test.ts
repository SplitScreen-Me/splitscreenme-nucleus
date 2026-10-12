// Profile settings form (src/screens/lobby/profile-form.ts) and the mock's profiles service: delete, settings and
// their validation. Lives with the bridge tests so `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  errorsByField,
  fieldKey,
  generalErrors,
  isAdvancedError,
  isDirty,
  parseWhole,
  slotKey,
  withField,
  withoutError,
  withSlot,
} from '../../screens/lobby/profile-form.ts';
import { BridgeClient } from '../client.ts';
import type { ProfileSettings } from '../generated/contract.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';
import { delay } from './helpers.ts';

function mockBridge() {
  const client = new BridgeClient({ transport: createMockBridgeTransport() });
  return { client, bridge: createBridgeProxy(client) };
}

const settings = (patch: Partial<ProfileSettings> = {}): ProfileSettings => ({
  isNew: false,
  title: 'Friday night',
  notes: '',
  autoPlay: false,
  useNicknames: true,
  autoDesktopScaling: false,
  splitDivider: false,
  splitDividerHideDesktopOnly: false,
  splitDividerColor: 'Black',
  customLayoutHorizontalLines: 1,
  customLayoutVerticalLines: 1,
  customLayoutMaxPlayers: 4,
  windowSetupDelayMs: 0,
  pauseBetweenInstancesSec: 0,
  network: 'Automatic',
  customAudio: false,
  cutscenesKeepAspectRatio: false,
  cutscenesMuteAudioOnly: false,
  cutscenesUnfocus: false,
  stopUiNavigation: false,
  windowsMerger: false,
  losslessHook: false,
  mergerResolution: '',
  slots: [
    { nickname: 'Rémi', steamId: '', steamIdLocked: false, idealProcessor: '*', affinity: '', priorityClass: 'Normal', audioDevice: 'Default' },
    { nickname: 'Léa', steamId: '', steamIdLocked: false, idealProcessor: '*', affinity: '', priorityClass: 'Normal', audioDevice: 'Default' },
  ],
  networkChoices: [{ value: 'Automatic', label: 'Automatic' }],
  audioChoices: [],
  mergerResolutionChoices: [],
  splitDividerColorChoices: [],
  ...patch,
});

test('validation messages are keyed by field, and by player for slot fields', () => {
  const errors = [
    { field: 'title', message: 'Too long.' },
    { field: 'slots.steamId', index: 1, message: 'A Steam ID must be 17 digits.' },
    { field: 'slots.steamId', index: 1, message: 'Each player needs a different Steam ID.' },
    { field: 'slots', message: 'Expected 2 players.' },
  ];
  const byField = errorsByField(errors);

  assert.equal(byField.title, 'Too long.');
  assert.equal(byField[slotKey('steamId', 1)], 'A Steam ID must be 17 digits. Each player needs a different Steam ID.');
  assert.equal(slotKey('steamId', 1), 'slots.steamId#1');
  assert.equal(fieldKey('title', undefined), 'title');
  assert.deepEqual(
    generalErrors(errors, (key) => key in byField && key !== 'slots'),
    ['Expected 2 players.'],
  );
  assert.deepEqual(withoutError(errors, 'slots.steamId#1').map((e) => e.field), ['title', 'slots']);
  assert.ok(isAdvancedError({ field: 'slots.affinity', index: 0, message: '' }));
  assert.ok(!isAdvancedError({ field: 'slots.nickname', index: 0, message: '' }));
});

test('edits make a new draft, and only edits make it dirty', () => {
  const saved = settings();
  const renamed = withField(saved, 'title', 'Saturday');
  const slot = withSlot(saved, 1, { nickname: 'Max' });

  assert.equal(saved.title, 'Friday night');
  assert.equal(slot.slots[1]?.nickname, 'Max');
  assert.equal(saved.slots[1]?.nickname, 'Léa');
  assert.ok(isDirty(saved, renamed));
  assert.ok(isDirty(saved, slot));
  assert.ok(!isDirty(saved, { ...saved, audioChoices: [{ value: 'x', label: 'x' }] }), 'choice lists do not count');
  assert.ok(!isDirty(saved, null));
});

test('numbers are taken once they are whole numbers', () => {
  assert.equal(parseWhole(' 250 '), 250);
  assert.equal(parseWhole('-3'), -3);
  assert.equal(parseWhole('2.5'), undefined);
  assert.equal(parseWhole(''), undefined);
});

test('the mock deletes a profile, renumbers the next ones and resets the setup like the host', async () => {
  const { client, bridge } = mockBridge();
  const changed: string[] = [];
  client.on('profiles.changed', (e) => changed.push(e.gameId));
  try {
    await bridge.setup.open({ gameId: 'EldenRing' });
    await bridge.setup.join({ deviceId: 'xinput:0' });
    const before = await bridge.profiles.list();
    assert.equal(before.length, 3);

    await bridge.profiles.delete({ profileId: 1 });
    await delay(5);

    const after = await bridge.profiles.list();
    assert.deepEqual(
      after.map((p) => [p.id, p.title]),
      [
        [1, before[1]!.title],
        [2, before[2]!.title],
      ],
    );
    assert.deepEqual(changed, ['EldenRing']);
    assert.equal((await bridge.setup.get())?.players.length, 0, 'every player went back to the tray');
    await assert.rejects(bridge.profiles.delete({ profileId: 3 }), { kind: 'notFound' });
  } finally {
    await bridge.setup.close();
  }
});

test('the mock saves the settings of the loaded profile, or returns the invalid fields', async () => {
  const { bridge } = mockBridge();
  try {
    await bridge.setup.open({ gameId: 'EldenRing' });
    await bridge.profiles.load({ profileId: 2 });
    const current = await bridge.profiles.getSettings();
    assert.equal(current.isNew, false);
    assert.equal(current.slots.length, 2);

    const bad = withSlot(withField(current, 'customLayoutHorizontalLines', 9), 1, { steamId: '123', priorityClass: 'Idle' });
    const errors = await bridge.profiles.saveSettings(bad);
    const byField = errorsByField(errors);
    assert.ok(byField.customLayoutHorizontalLines);
    assert.ok(byField[slotKey('steamId', 1)]);
    assert.ok(byField[slotKey('priorityClass', 1)], 'the host only takes Normal, AboveNormal, High and RealTime');
    assert.equal((await bridge.profiles.getSettings()).customLayoutHorizontalLines, current.customLayoutHorizontalLines, 'nothing saved');

    assert.deepEqual(await bridge.profiles.saveSettings(withField(current, 'title', 'Duo')), []);
    assert.equal((await bridge.profiles.list())[1]?.title, 'Duo');
  } finally {
    await bridge.setup.close();
  }
});

test('without a loaded profile the settings are those of the next profile', async () => {
  const { bridge } = mockBridge();
  try {
    await bridge.setup.open({ gameId: 'Minecraft' });
    const next = await bridge.profiles.getSettings();
    assert.equal(next.isNew, true);
    assert.deepEqual(await bridge.profiles.saveSettings(withField(next, 'notes', 'Survival')), []);
    assert.equal((await bridge.profiles.getSettings()).notes, 'Survival');
  } finally {
    await bridge.setup.close();
  }
});
