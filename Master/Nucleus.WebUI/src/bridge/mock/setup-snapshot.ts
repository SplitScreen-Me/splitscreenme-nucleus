// Snapshots of the setup as the UI sees them: screens and slots, devices, numbered players, flow, hint, profile match.
import type {
  DeviceKind,
  ProfileMatch,
  ProfileSlot,
  SetupDevice,
  SetupFlow,
  SetupHint,
  SetupPlayer,
  SetupCheck,
  SetupScreen,
  SetupSnapshot,
  SetupStep,
} from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { AUDIO_OUTPUTS, isGamepad } from './fixtures-setup.ts';
import { checksOf } from './setup-checks.ts';
import { areaRect, cellCount, overlaps, sameArea, slotArea, unionRect } from './setup-grid.ts';
import { expansionOf, findDevice, findScreen, gridOf, hostOf, nextSeat, numberedPlayers, ownerOf, playerIdOf } from './setup-model.ts';
import type { MockDevice, MockScreen, MockSetup, MockState } from './state.ts';

const KIND_NAMES: Readonly<Record<DeviceKind, string>> = {
  xInput: 'gamepad',
  dInput: 'gamepad',
  sdl: 'gamepad',
  keyboard: 'keyboard',
  rawKeyboard: 'keyboard',
  rawMouse: 'mouse',
  keyboardMouse: 'keyboard and mouse',
};

/** State of the loaded profile while devices are matched to its players. */
export function profileMatchOf(setup: MockSetup): ProfileMatch {
  if (!setup.profile) return { loaded: false, assigned: 0, total: 0, ready: false, autoPlay: false };
  const { id, seats, autoPlay } = setup.profile;
  const waiting = nextSeat(setup);
  return {
    loaded: true,
    profileId: id,
    assigned: setup.players.filter((player) => player.profileNumber !== undefined).length,
    total: seats.length,
    awaitingPlayer: waiting?.number,
    awaitingKind: waiting?.device,
    ready: waiting === undefined,
    autoPlay,
  };
}

/** Steps shown: players, options (skipped with a profile), then the custom steps a profile did not answer. */
function stepsOf(setup: MockSetup): SetupStep[] {
  const steps: SetupStep[] = [{ kind: 'players', title: 'Players' }];
  if (!setup.profile && setup.options.length > 0) steps.push({ kind: 'options', title: 'Options' });
  for (const step of setup.customSteps) {
    if (!step.autoSelected) steps.push({ kind: 'custom', customStepIndex: step.index, title: step.title });
  }
  return steps;
}

function isStepDone(setup: MockSetup, step: SetupStep, match: ProfileMatch): boolean {
  if (step.kind === 'players') return setup.players.length > 0 && (!match.loaded || match.ready);
  const custom = setup.customSteps.find((candidate) => candidate.index === step.customStepIndex);
  return !custom?.required || custom.selectedIndex !== undefined;
}

/** Whether nothing prevents a launch: no blocking check (the game found among them), no fix running, no session. */
const canLaunch = (state: MockState, checks: readonly SetupCheck[]) =>
  state.session.phase === null && !checks.some((check) => check.state === 'blocking' || check.state === 'busy');

/** Steps and navigation state. Play is allowed on the last step, once every step is done. */
export function flowOf(state: MockState, setup: MockSetup): SetupFlow {
  const match = profileMatchOf(setup);
  const steps = stepsOf(setup);
  const current = Math.min(setup.step, steps.length - 1);
  const done = steps.map((step) => isStepDone(setup, step, match));
  return {
    steps,
    current,
    canGoBack: current > 0,
    canGoNext: current < steps.length - 1 && (done[current] ?? false),
    canPlay: canLaunch(state, checksOf(state, setup)) && current === steps.length - 1 && done.every(Boolean),
  };
}

const hint = (code: string, text: string, args: string[] = []): SetupHint => ({ code, args, text });

function hintOf(setup: MockSetup, flow: SetupFlow, match: ProfileMatch, checks: readonly SetupCheck[]): SetupHint | undefined {
  const step = flow.steps[flow.current];
  const blocking = checks.find((check) => check.state === 'blocking');
  if (blocking?.id === 'gameFound') return hint('executableMissing', `${setup.gameName} was not found. Locate its executable.`);
  if (blocking) return hint('fixBlockingCheck', `${blocking.title}: fix it before launching.`, [blocking.id]);
  if (step?.kind === 'custom') {
    const custom = setup.customSteps.find((candidate) => candidate.index === step.customStepIndex);
    if (custom?.required && custom.selectedIndex === undefined) return hint('pickStepItem', `Pick one: ${custom.title}.`, [custom.title]);
  }
  if (step?.kind === 'players') {
    const { awaitingPlayer, awaitingKind } = match;
    if (awaitingPlayer !== undefined && awaitingKind) {
      const text = `Player ${awaitingPlayer}: press a button on a ${KIND_NAMES[awaitingKind]}.`;
      return hint('profileWaitingForPlayer', text, [String(awaitingPlayer), awaitingKind]);
    }
    if (setup.devices.some((device) => device.needsButtonPress)) {
      return hint('pressButtonToIdentify', 'Press a button on each gamepad to identify it.');
    }
    if (setup.players.length === 0) return hint('dragPlayersToScreens', 'Drag devices onto the screens to add players.');
  }
  if (!flow.canPlay) return undefined;
  if (match.loaded) return hint('profileReady', 'Every player of the profile is here. Press Play.');
  return hint('readyToPlay', 'Ready. Press Play.');
}

function screenView(setup: MockSetup, screen: MockScreen): SetupScreen {
  const grid = gridOf(setup, screen);
  const seats = setup.profile?.seats ?? [];
  const slots =
    screen.layout === 'manual'
      ? []
      : Array.from({ length: cellCount(grid) }, (_, index) => {
          const area = slotArea(grid, index);
          const player = setup.players.find((candidate) => candidate.screenIndex === screen.index && overlaps(candidate.area, area));
          return {
            index,
            bounds: areaRect(screen.bounds, grid, area),
            playerId: player && playerIdOf(player),
            expected: seats.some((seat) => seat.screenIndex === screen.index && seat.slotIndex === index),
          };
        });
  const { index, displayIndex, bounds, isPrimary, layout, layoutLocked } = screen;
  // The mock moves edges by whole cells (`setup.resize`), so a step is a cell.
  const resizable = setup.profile === null && cellCount(grid) >= 4;
  return {
    index,
    displayIndex,
    bounds,
    isPrimary,
    layout,
    rows: grid.rows,
    columns: grid.columns,
    slots,
    layoutLocked,
    resizeStepWidth: resizable ? Math.round(bounds.width / grid.columns) : 0,
    resizeStepHeight: resizable ? Math.round(bounds.height / grid.rows) : 0,
  };
}

function deviceView(setup: MockSetup, device: MockDevice): SetupDevice {
  const owner = ownerOf(setup, device.id);
  const host = hostOf(setup, device.id);
  return { ...device, playerId: owner && playerIdOf(owner), guestOfPlayerId: host && playerIdOf(host) };
}

/**
 * Per-player settings that override the global identities and audio outputs: the loaded profile's, else those
 * of the profile saved at launch once edited. Mutable: identity and audio commands update them too.
 */
export function profileSlots(state: MockState, setup: MockSetup): ProfileSlot[] {
  if (setup.profile) return state.profiles[setup.gameId]?.[setup.profile.id - 1]?.settings.slots ?? [];
  return setup.pendingSettings?.slots ?? [];
}

const customAudioOf = (state: MockState) => state.settings['audio.custom'] === 'true';

function playerViews(state: MockState, setup: MockSetup): SetupPlayer[] {
  const slots = profileSlots(state, setup);
  const locked = setup.profile !== null;
  return numberedPlayers(setup).map(({ player, number }) => {
    const grid = gridOf(setup, findScreen(setup, player.screenIndex));
    const identity = state.players.players[number - 1];
    const gamepad = player.deviceIds.some((id) => isGamepad(findDevice(setup, id).kind));
    const audio = slots[number - 1]?.audioDevice ?? state.settings[`audio.instance${number}`] ?? 'Default';
    return {
      id: playerIdOf(player),
      number,
      screenIndex: player.screenIndex,
      bounds: areaRect(findScreen(setup, player.screenIndex).bounds, grid, player.area),
      deviceIds: [...player.deviceIds],
      guestDeviceIds: [...player.guestDeviceIds],
      maxGuests: gamepad ? setup.capabilities.playersPerInstance - 1 : 0,
      nickname: slots[number - 1]?.nickname || identity?.nickname || undefined,
      steamId: slots[number - 1]?.steamId || identity?.steamId || undefined,
      audioDeviceId: customAudioOf(state) ? audio : undefined,
      canExpand: !locked && (!sameArea(player.area, player.home) || !sameArea(expansionOf(setup, player), player.area)),
      canResize: !locked && cellCount(grid) >= 4,
    };
  });
}

/** Everything the setup screen shows. */
export function setupSnapshot(state: MockState, setup: MockSetup): SetupSnapshot {
  const flow = flowOf(state, setup);
  const match = profileMatchOf(setup);
  const checks = checksOf(state, setup);
  return {
    gameId: setup.gameId,
    gameName: setup.gameName,
    revision: setup.revision,
    flow,
    hint: hintOf(setup, flow, match, checks),
    totalBounds: unionRect(setup.screens.map((screen) => screen.bounds)),
    screens: setup.screens.map((screen) => screenView(setup, screen)),
    devices: setup.devices.map((device) => deviceView(setup, device)),
    players: playerViews(state, setup),
    options: setup.options,
    customSteps: setup.customSteps,
    profile: match,
    capabilities: setup.capabilities,
    useApiIndex: setup.useApiIndex,
    saveProfileOnLaunch: setup.saveProfileOnLaunch,
    checks,
    audioOutputs: [...AUDIO_OUTPUTS],
    customAudio: customAudioOf(state),
  };
}

/** Publishes the open setup when it belongs to a game whose library state (shown by its checks) changed. */
export function refreshSetupOf(context: MockContext, state: MockState, gameId: string): void {
  if (state.setup?.gameId === gameId) publishSetup(context, state, state.setup);
}

/** Records a change of the setup: bumps the revision, emits `setup.changed` and returns the new snapshot. */
export function publishSetup(context: MockContext, state: MockState, setup: MockSetup): SetupSnapshot {
  state.setupRevision += 1;
  setup.revision = state.setupRevision;
  const snapshot = setupSnapshot(state, setup);
  context.emit('setup.changed', { snapshot });
  return snapshot;
}
