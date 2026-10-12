import type { BridgeHandlers, SetPlayerAudioRequest, SetPlayerIdentityRequest, SetupSnapshot } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict, invalidParams, notFound } from './failures.ts';
import { AUDIO_OUTPUTS, isGamepad } from './fixtures-setup.ts';
import { publishSession, startSession } from './session.ts';
import { checksOf, fixCheck } from './setup-checks.ts';
import {
  addGuest,
  assignAt,
  assignToSlot,
  autoFill,
  clearAll,
  createSetup,
  findDevice,
  join,
  matchDevice,
  playerNumber,
  removeGuest,
  requireEditable,
  requireSetup,
  rescanDevices,
  resize,
  reveal,
  selectStepItem,
  setLayout,
  setOptionChoice,
  setOptionText,
  setUseApiIndex,
  toggleExpand,
  unassign,
} from './setup-model.ts';
import { flowOf, profileMatchOf, profileSlots, publishSetup, setupSnapshot } from './setup-snapshot.ts';
import type { Grid, MockDevice, MockSetup, MockState } from './state.ts';
import { clearTimers, schedule } from './timers.ts';
import { nicknameProblem, steamIdProblem } from './validation.ts';

/** First simulated button press after the setup opens, then one every interval, for each device not yet revealed. */
const REVEAL_FIRST_MS = 1000;
const REVEAL_INTERVAL_MS = 800;
/** Revealed devices are "used" in turn, one every interval; each use lasts a moment. */
const ACTIVITY_INTERVAL_MS = 1600;
const ACTIVITY_LENGTH_MS = 250;

/** Grid of the custom layout, from the settings (lines between rows and columns). */
function customGridOf(settings: Record<string, string>): Grid {
  const lines = (key: string) => Math.min(5, Math.max(0, Math.round(Number(settings[key] ?? 1)) || 0));
  return { rows: lines('customLayout.horizontalLines') + 1, columns: lines('customLayout.verticalLines') + 1 };
}

/** Saves a player's nickname and Steam ID in the global player slots, and in the profile's slot when there is one. */
function setPlayerIdentity(state: MockState, setup: MockSetup, { playerId, nickname, steamId }: SetPlayerIdentityRequest): void {
  const problem = (nickname === undefined ? undefined : nicknameProblem(nickname)) ?? (steamId && steamIdProblem(steamId));
  if (problem) invalidParams(problem);
  const number = playerNumber(setup, playerId);
  const { players } = state.players;
  while (players.length < number) players.push({ number: players.length + 1, nickname: '', steamId: '' });
  for (const identity of [players[number - 1], profileSlots(state, setup)[number - 1]]) {
    if (!identity) continue;
    if (nickname !== undefined) identity.nickname = nickname.trim();
    if (steamId !== undefined) identity.steamId = steamId.trim();
  }
}

/** Routes a player's audio (global setting `audio.instanceN`, and the profile's slot) and turns per-player audio on. Returns the key. */
function setPlayerAudio(state: MockState, setup: MockSetup, { playerId, audioDeviceId }: SetPlayerAudioRequest): string {
  if (!AUDIO_OUTPUTS.some((output) => output.value === audioDeviceId)) invalidParams(`Unknown audio output '${audioDeviceId}'.`);
  const number = playerNumber(setup, playerId);
  const key = `audio.instance${number}`;
  if (!(key in state.settings)) conflict('noAudioSlot', 'Per-player audio covers players 1 to 8.');
  Object.assign(state.settings, { [key]: audioDeviceId, 'audio.custom': 'true' });
  const slot = profileSlots(state, setup)[number - 1];
  if (slot) slot.audioDevice = audioDeviceId;
  return key;
}

/**
 * Mock of the `setup` service. Opening a setup starts the device detection: devices that need a button press or
 * that appear once used are revealed one by one, revealed devices show activity in turn, and a loaded profile
 * matches the devices used to its waiting players. Every command returns the new snapshot and emits `setup.changed`.
 */
export function createSetupMock(context: MockContext, state: MockState): BridgeHandlers['setup'] {
  const update = (change: (setup: MockSetup) => void): SetupSnapshot => {
    const setup = requireEditable(state);
    change(setup);
    return publishSetup(context, state, setup);
  };

  /** A device was used: flashes it, matches it to the loaded profile, and starts an auto-play profile once complete. */
  const deviceUsed = (setup: MockSetup, device: MockDevice, revealed: boolean) => {
    context.emit('setup.deviceActivity', { deviceId: device.id, active: true });
    schedule(setup.timers, ACTIVITY_LENGTH_MS, () => context.emit('setup.deviceActivity', { deviceId: device.id, active: false }));
    const matched = state.session.phase === null && matchDevice(setup, device);
    if (revealed || matched) publishSetup(context, state, setup);
    if (matched && setup.profile?.autoPlay && profileMatchOf(setup).ready && flowOf(state, setup).canPlay) startSession(context, state);
  };

  const scheduleReveal = (setup: MockSetup, device: MockDevice, delay: number) =>
    schedule(setup.timers, delay, () => {
      if (device.revealed) return;
      reveal(device);
      deviceUsed(setup, device, true);
    });

  const scheduleActivity = (setup: MockSetup, turn: number) =>
    schedule(setup.timers, ACTIVITY_INTERVAL_MS, () => {
      const revealed = setup.devices.filter((device) => device.revealed);
      const device = revealed[turn % Math.max(1, revealed.length)];
      if (device && state.session.phase === null) deviceUsed(setup, device, false);
      scheduleActivity(setup, turn + 1);
    });

  return {
    open: ({ gameId }) => {
      if (state.session.phase) conflict('sessionRunning', 'A session is running: stop it before opening another setup.');
      const game = state.games.find((candidate) => candidate.summary.id === gameId) ?? notFound(`No game '${gameId}' in the library.`);
      if (state.setup) clearTimers(state.setup.timers);
      const setup = createSetup(game, customGridOf(state.settings));
      state.setup = setup;
      setup.devices
        .filter((device) => !device.revealed)
        .forEach((device, index) => scheduleReveal(setup, device, REVEAL_FIRST_MS + index * REVEAL_INTERVAL_MS));
      scheduleActivity(setup, 0);
      publishSession(context, state);
      return publishSetup(context, state, setup);
    },
    close: () => {
      if (!state.setup) return;
      if (state.session.phase) conflict('sessionRunning', 'Stop the session before closing the setup.');
      clearTimers(state.setup.timers);
      state.setup = null;
      publishSession(context, state);
    },
    get: () => (state.setup ? setupSnapshot(state, state.setup) : null),
    next: () =>
      update((setup) => {
        const { canGoNext, current, steps } = flowOf(state, setup);
        if (!canGoNext) conflict(current === steps.length - 1 ? 'lastStep' : 'stepIncomplete', 'Finish this step first.');
        setup.step = current + 1;
      }),
    back: () =>
      update((setup) => {
        const { canGoBack, current } = flowOf(state, setup);
        if (!canGoBack) conflict('firstStep', 'This is the first step.');
        setup.step = current - 1;
      }),
    setLayout: ({ screenIndex, layout }) => update((setup) => setLayout(setup, screenIndex, layout)),
    join: ({ deviceId }) => update((setup) => join(setup, deviceId)),
    assignToSlot: ({ deviceId, screenIndex, slotIndex }) => update((setup) => assignToSlot(setup, deviceId, screenIndex, slotIndex)),
    assignAt: ({ deviceId, screenIndex, x, y }) => update((setup) => assignAt(setup, deviceId, screenIndex, x, y)),
    unassign: ({ deviceId }) => update((setup) => unassign(setup, deviceId)),
    addGuest: ({ playerId, deviceId }) => update((setup) => addGuest(setup, playerId, deviceId)),
    removeGuest: ({ deviceId }) => update((setup) => removeGuest(setup, deviceId)),
    toggleExpand: ({ playerId }) => update((setup) => toggleExpand(setup, playerId)),
    resize: ({ playerId, edge, steps }) => update((setup) => resize(setup, playerId, edge, steps)),
    autoFill: () => update(autoFill),
    clearAll: () => update(clearAll),
    setOptionChoice: ({ key, index }) => update((setup) => setOptionChoice(setup, key, index)),
    setOptionText: ({ key, text }) => update((setup) => setOptionText(setup, key, text)),
    selectStepItem: ({ stepIndex, itemIndex }) => update((setup) => selectStepItem(setup, stepIndex, itemIndex)),
    setPlayerIdentity: (request) =>
      update((setup) => {
        setPlayerIdentity(state, setup, request);
        context.emit('settings.changed', { keys: ['players'] });
      }),
    setPlayerAudio: (request) =>
      update((setup) => context.emit('settings.changed', { keys: [setPlayerAudio(state, setup, request), 'audio.custom'] })),
    runCheckAction: async ({ checkId }) => {
      const setup = requireEditable(state);
      const check = checksOf(state, setup).find((candidate) => candidate.id === checkId) ?? notFound(`No check '${checkId}'.`);
      if (check.state === 'busy') conflict('checkBusy', `The fix of "${check.title}" is already running.`);
      if (!check.actionLabel) conflict('noFix', `"${check.title}" has nothing to fix.`);
      // Like the host: the file picker belongs to the UI, which then calls library.setExecutable.
      if (checkId === 'gameFound') conflict('needsPicker', 'Choose the game executable.');
      setup.busyChecks.add(checkId);
      publishSetup(context, state, setup);
      try {
        await fixCheck(context, state, setup, checkId);
      } finally {
        setup.busyChecks.delete(checkId);
      }
      return state.setup === setup ? publishSetup(context, state, setup) : setupSnapshot(state, setup);
    },
    setUseApiIndex: ({ enabled }) =>
      update((setup) => {
        setUseApiIndex(setup, enabled);
        const game = state.games.find((candidate) => candidate.summary.id === setup.gameId);
        if (game) game.useApiIndex = enabled;
      }),
    setSaveProfileOnLaunch: ({ enabled }) =>
      update((setup) => {
        setup.saveProfileOnLaunch = enabled;
        const game = state.games.find((candidate) => candidate.summary.id === setup.gameId);
        if (game) game.saveProfileOnLaunch = enabled;
      }),
    refreshDevices: () =>
      update((setup) => {
        for (const device of rescanDevices(setup).filter((found) => !found.revealed)) scheduleReveal(setup, device, REVEAL_FIRST_MS);
      }),
    rumble: ({ deviceId }) => {
      const device = findDevice(requireSetup(state), deviceId);
      if (!isGamepad(device.kind)) conflict('notAGamepad', 'Only gamepads can vibrate.');
      console.info('[bridge mock] rumble:', device.name);
    },
  };
}
