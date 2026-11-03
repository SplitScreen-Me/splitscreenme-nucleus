// The Patreon supporters credits: the states of the screen (src/screens/settings/supporters-core.ts), the auto-scroll
// step, the remembered mute, the mock `supporters` service and the English strings. Lives with the bridge tests so
// `npm run test:bridge` runs it.
import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  CREDITS_PX_PER_S,
  CREDITS_SCRUB_HOLD_MS,
  creditsStep,
  MUSIC_CREDIT_URL,
  MUTED_KEY,
  readMuted,
  supportersView,
  writeMuted,
  type ScrollState,
} from '../../screens/settings/supporters-core.ts';
import { english } from '../../i18n/english.ts';
import { activateLanguage, t } from '../../i18n/store.ts';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createSupportersMock, MOCK_MUSIC_URL, MOCK_SUPPORTERS } from '../mock/supporters.ts';
import { createBridgeProxy } from '../proxy.ts';

const list = (extra: Partial<{ fromCache: boolean; savedAt: string; musicUrl: string }> = {}) => ({
  names: ['Ann', 'Ben'],
  fromCache: false,
  ...extra,
});

test('the screen waits, fails or shows the names', () => {
  assert.deepEqual(supportersView({ isError: false }), { kind: 'loading' });
  assert.deepEqual(supportersView({ isError: true }), { kind: 'error' });
  assert.deepEqual(supportersView({ isError: false, data: list({ musicUrl: 'https://assets.nucleus.example/audio/x.mp3' }) }), {
    kind: 'ready',
    names: ['Ann', 'Ben'],
    savedAt: null,
    musicUrl: 'https://assets.nucleus.example/audio/x.mp3',
  });
  // Offline: the saved copy, with its date (or without one), and no music when the file is missing.
  assert.deepEqual(supportersView({ isError: false, data: list({ fromCache: true, savedAt: '2026-10-28T19:42:00Z' }) }), {
    kind: 'ready',
    names: ['Ann', 'Ben'],
    savedAt: '2026-10-28T19:42:00Z',
    musicUrl: null,
  });
  const undated = supportersView({ isError: false, data: list({ fromCache: true }) });
  assert.equal(undated.kind === 'ready' && undated.savedAt, '');
  // A refetch that fails keeps the names on screen.
  assert.equal(supportersView({ isError: true, data: list() }).kind, 'ready');
});

const start: ScrollState = { pos: 0, holdUntil: 1000 };
const frame = (top: number, now: number, paused = false, dt = 16, max = 5000) => ({ top, max, dt, now, paused });

test('the names roll up at a steady speed once the title has been seen', () => {
  let out = creditsStep(start, frame(0, 500));
  assert.equal(out.write, null, 'the title stays still first');
  out = creditsStep(out.state, frame(0, 1000, false, 100));
  assert.equal(out.write, CREDITS_PX_PER_S / 10, 'speed in px per second');
  const first = out.write!;
  out = creditsStep(out.state, frame(Math.round(first), 1016, false, 16));
  assert.ok(Math.abs(out.write! - (first + (CREDITS_PX_PER_S * 16) / 1000)) < 1e-9, 'fractional positions survive a rounded scrollTop');
  out = creditsStep({ pos: 100, holdUntil: 0 }, frame(100, 5000, false, 5000));
  assert.equal(out.write, 100 + (CREDITS_PX_PER_S * 100) / 1000, 'a long frame (tab hidden) does not jump');
});

test('hover, focus or a pause stop the roll where it is', () => {
  const out = creditsStep({ pos: 300, holdUntil: 0 }, frame(300, 9000, true));
  assert.equal(out.write, null);
  assert.equal(out.state.pos, 300);
  assert.equal(out.ended, false);
});

test('the user scrolling (wheel, stick, keys) takes over, then the roll goes on from there', () => {
  let out = creditsStep({ pos: 300, holdUntil: 0 }, frame(900, 9000));
  assert.equal(out.write, null);
  assert.deepEqual(out.state, { pos: 900, holdUntil: 9000 + CREDITS_SCRUB_HOLD_MS });
  out = creditsStep(out.state, frame(900, 9000 + CREDITS_SCRUB_HOLD_MS - 1));
  assert.equal(out.write, null, 'still waiting');
  out = creditsStep(out.state, frame(900, 9000 + CREDITS_SCRUB_HOLD_MS, false, 100));
  assert.equal(out.write, 900 + CREDITS_PX_PER_S / 10);
  // Scrolling back up works the same.
  out = creditsStep({ pos: 900, holdUntil: 0 }, frame(100, 20_000));
  assert.equal(out.state.pos, 100);
});

test('the roll stops at the end (the thanks and the links)', () => {
  let out = creditsStep({ pos: 4998, holdUntil: 0 }, frame(4998, 9000, false, 100));
  assert.equal(out.write, 5000);
  assert.equal(out.ended, true);
  out = creditsStep(out.state, frame(5000, 9016));
  assert.equal(out.write, null);
  assert.equal(out.ended, true);
  // A list shorter than the pane has nothing to roll.
  assert.equal(creditsStep({ pos: 0, holdUntil: 0 }, frame(0, 9000, false, 16, 0)).ended, true);
});

test('the mute is remembered, and broken storage only forgets it', () => {
  const data = new Map<string, string>();
  const store = { getItem: (k: string) => data.get(k) ?? null, setItem: (k: string, v: string) => void data.set(k, v) };
  assert.equal(readMuted(store), false, 'music on by default');
  writeMuted(store, true);
  assert.equal(data.get(MUTED_KEY), '1');
  assert.equal(readMuted(store), true);
  writeMuted(store, false);
  assert.equal(readMuted(store), false);
  const broken = {
    getItem: (): string | null => {
      throw new Error('denied');
    },
    setItem: () => {
      throw new Error('full');
    },
  };
  assert.equal(readMuted(broken), false);
  assert.doesNotThrow(() => writeMuted(broken, true));
  assert.equal(readMuted(null), false);
});

test('the mock serves invented names, a saved copy, or the unavailable error', async () => {
  const fresh = await createSupportersMock(() => null).get();
  assert.deepEqual(fresh.names, MOCK_SUPPORTERS);
  assert.equal(fresh.fromCache, false);
  assert.equal(fresh.musicUrl, MOCK_MUSIC_URL);
  const cached = await createSupportersMock(() => 'cache').get();
  assert.equal(cached.fromCache, true);
  assert.ok(cached.savedAt);
  await assert.rejects(async () => createSupportersMock(() => 'error').get(), (e: unknown) => BridgeError.is(e, 'unavailable'));
});

test('supporters.get goes through the mock bridge like the host', async () => {
  const bridge = createBridgeProxy(new BridgeClient({ transport: createMockBridgeTransport({ notices: false }) }));
  const answer = await bridge.supporters.get();
  assert.equal(answer.names.length, MOCK_SUPPORTERS.length);
  assert.equal(answer.fromCache, false);
});

test('the credits read well in English', () => {
  activateLanguage('en', english);
  assert.equal(t('settings.supporters.count', { count: 1 }), '1 supporter');
  assert.equal(t('settings.supporters.count', { count: 489 }), '489 supporters');
  assert.equal(t('settings.supporters.thanks'), '♥♥ THANK YOU ♥♥');
  assert.equal(t('settings.supporters.musicCredit', { link: MUSIC_CREDIT_URL }), 'Music: https://pixabay.com/users/scaperlee-23524192/');
  assert.equal(t('shell.news.dismiss', { count: 1 }), 'Hide this message until something new is posted');
  assert.equal(t('shell.news.dismiss', { count: 3 }), 'Hide these 3 messages until something new is posted');
});
