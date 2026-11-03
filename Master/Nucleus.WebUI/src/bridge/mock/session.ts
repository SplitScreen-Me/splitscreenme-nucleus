import {
  sessionActionValues,
  type BridgeHandlers,
  type Rect,
  type SessionAction,
  type SessionActionInfo,
  type SessionInstance,
  type SessionLogLevel,
  type SessionSnapshot,
  type SessionSummaryPlayer,
} from '../generated/contract.ts';
import { BridgeError } from '../errors.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict, invalidParams } from './failures.ts';
import { MONITORS, setupFixtureFor } from './fixtures-setup.ts';
import { findHandler } from './library-model.ts';
import { launchProfileSettings } from './profile-model.ts';
import { findDevice, profileFromSetup, requireSetup } from './setup-model.ts';
import { flowOf, publishSetup, setupSnapshot } from './setup-snapshot.ts';
import type { MockSession, MockSetup, MockState } from './state.ts';
import { clearTimers, schedule } from './timers.ts';

/** Delay before the first instance starts, then between two instances. */
const LAUNCH_INTERVAL_MS = 800;
/** Time an instance takes from process start to a placed window. */
const WINDOW_READY_MS = 700;
/** Keyboard sessions lock the inputs once running; the mock unlocks them after this delay (the host waits for the hotkey). */
const INPUTS_LOCKED_MS = 5000;
const NOTICE_MS = 4000;
/** With `nucleus.mock.instance`, a copy quits this long after the session started running. */
const QUIT_AFTER_MS = 2500;
/** Time a restart takes until the new process is found. */
const RESTART_MS = 1500;
/** Most error lines a session summary keeps (as the host). */
const MAX_SUMMARY_ERRORS = 20;

/** Label and hotkey setting of each session action. */
const ACTIONS: Readonly<Record<SessionAction, { label: string; hotkey: string }>> = {
  toggleTopmost: { label: 'Keep the game windows on top', hotkey: 'hotkeys.topMost' },
  setFocus: { label: 'Bring the game windows to the front', hotkey: 'hotkeys.setFocus' },
  resetWindows: { label: 'Put the windows back in place', hotkey: 'hotkeys.resetWindows' },
  cutscenesMode: { label: 'Cutscene mode', hotkey: 'hotkeys.cutscenes' },
  switchLayout: { label: 'Switch layout', hotkey: 'hotkeys.switch' },
  toggleShortcutsReminder: { label: 'Show the shortcuts', hotkey: 'hotkeys.shortcutsReminder' },
  switchMergerForeground: { label: 'Next window in the merged window', hotkey: 'hotkeys.switchMergerForeground' },
};

/** Tilings `switchLayout` goes through on one monitor: the setup's, then columns, then rows. */
const TILINGS = ['as set up', 'side by side', 'top and bottom'] as const;

function isAvailable(state: MockState, action: SessionAction): boolean {
  const { session } = state;
  if (session.phase !== 'running') return false;
  if (action === 'switchMergerForeground') return state.settings['customLayout.windowsMerger'] === 'true';
  if (action === 'switchLayout') {
    return session.instances.length > 1 && new Set(session.instances.map((instance) => instance.screenIndex)).size === 1;
  }
  return true;
}

const actionsOf = (state: MockState): SessionActionInfo[] =>
  sessionActionValues.map((action) => ({
    action,
    label: ACTIONS[action].label,
    hotkey: state.settings[ACTIONS[action].hotkey],
    available: isAvailable(state, action),
  }));

/** Messages of the host's refusals (host.session.restart.*, in English like every mock host text). */
const RESTART_REFUSALS: Readonly<Record<string, string>> = {
  noSession: 'No session is running: there is no copy to restart.',
  notReady: 'The copies are still starting. Wait until every window is in place, then try again.',
  busy: 'Another copy is being restarted. Wait for it to finish, then try again.',
  notSupported: "This handler can't restart a copy of the game. Stop the session and play again.",
};

/** Why a copy can't be started again now (the host's refusal reasons, in its order), or undefined when it can. */
function restartRefusal(state: MockState): string | undefined {
  const { session } = state;
  if (!session.phase || session.phase === 'ending') return 'noSession';
  if (session.phase === 'launching') return 'notReady';
  if (session.instances.some((instance) => instance.stage === 'restarting')) return 'busy';
  return setupFixtureFor(session.gameId ?? '').cantRestartInstances ? 'notSupported' : undefined;
}

/** The session state: launching, running or ending; else `setup` when a setup is open, `idle` otherwise. */
function sessionSnapshot(state: MockState): SessionSnapshot {
  const { session, setup } = state;
  const common = {
    restartRefusal: restartRefusal(state),
    inputsLocked: session.inputsLocked,
    error: session.error,
    errorDetails: session.errorDetails,
    actions: actionsOf(state),
    stopHotkey: state.settings['hotkeys.stop'],
    lockInputsKey: state.settings['hotkeys.lockKey'],
    lastSession: session.lastSession,
  };
  if (session.phase) {
    const { phase, gameId, gameName, startedAt, instances } = session;
    return { ...common, state: phase, gameId, gameName, startedAt, instances };
  }
  return setup
    ? { ...common, state: 'setup', gameId: setup.gameId, gameName: setup.gameName, instances: [] }
    : { ...common, state: 'idle', instances: [] };
}

/** Emits `session.changed` with the current snapshot and returns it. */
export function publishSession(context: MockContext, state: MockState): SessionSnapshot {
  const snapshot = sessionSnapshot(state);
  context.emit('session.changed', { snapshot });
  return snapshot;
}

/** Appends a line to the session log and emits `session.log`. */
function log(context: MockContext, state: MockState, text: string, level: SessionLogLevel = 'info', playerNumber?: number): void {
  state.logSeq += 1;
  const entry = { seq: state.logSeq, at: new Date().toISOString(), level, playerNumber, text };
  state.session.log.push(entry);
  context.emit('session.log', { entries: [entry] });
}

const notice = (context: MockContext, text: string, durationMs = NOTICE_MS) => context.emit('session.notice', { text, durationMs });

/** Saves the open setup as a new profile (`setup.saveProfileOnLaunch`). */
function saveLaunchProfile(context: MockContext, state: MockState, setup: MockSetup): void {
  const profiles = (state.profiles[setup.gameId] ??= []);
  profiles.push(profileFromSetup(setup, launchProfileSettings(state, setup)));
  setup.pendingSettings = null;
  context.emit('profiles.changed', { gameId: setup.gameId });
  context.emit('library.changed', { gameIds: [setup.gameId] });
}

/** The placed players with the names of their devices, guests last (the host takes them from the setup at Play). */
function summaryPlayersOf(state: MockState, setup: MockSetup): SessionSummaryPlayer[] {
  return setupSnapshot(state, setup).players.map((player) => ({
    number: player.number,
    nickname: player.nickname,
    screenIndex: player.screenIndex,
    devices: [
      ...player.deviceIds.map((id) => ({ id, guest: false })),
      ...player.guestDeviceIds.map((id) => ({ id, guest: true })),
    ].map(({ id, guest }) => {
      const device = findDevice(setup, id);
      return { kind: device.kind, name: device.name, guest };
    }),
  }));
}

/** One instance per player, in player order, waiting to start; with the names of the devices bound to each. */
function instancesOf(state: MockState, setup: MockSetup): { instance: SessionInstance; binding: string }[] {
  return setupSnapshot(state, setup).players.map((player) => {
    const devices = [...player.deviceIds, ...player.guestDeviceIds].map((id) => findDevice(setup, id));
    return {
      instance: {
        playerNumber: player.number,
        nickname: player.nickname,
        screenIndex: player.screenIndex,
        bounds: player.bounds,
        devices: devices.map((device) => device.kind),
        stage: 'waiting',
        crashed: false,
        restartFailed: false,
      },
      binding: devices.map((device) => device.name).join(' + '),
    };
  });
}

/**
 * Starts the session with the open setup (`session.play`, or a profile with auto play once matched). Instances then
 * start one after the other, and the session runs once every window is ready.
 */
export function startSession(context: MockContext, state: MockState): SessionSnapshot {
  if (state.session.phase) conflict('sessionRunning', 'A session is already running. Stop it first.');
  const setup = requireSetup(state);
  if (!flowOf(state, setup).canPlay) conflict('notReady', setupSnapshot(state, setup).hint?.text ?? 'The setup is not complete yet.');
  const players = summaryPlayersOf(state, setup);
  if (setup.saveProfileOnLaunch && !setup.profile) saveLaunchProfile(context, state, setup);
  const launches = instancesOf(state, setup);
  const instances = launches.map(({ instance }) => instance);
  Object.assign(state.session, {
    players,
    reachedRunning: false,
    lastSession: undefined,
    phase: 'launching',
    gameId: setup.gameId,
    gameName: setup.gameName,
    startedAt: new Date().toISOString(),
    instances,
    inputsLocked: false,
    error: undefined,
    errorDetails: undefined,
    log: [],
    topmost: true,
    cutscenes: false,
    layoutIndex: 0,
    setupBounds: instances.map((instance) => ({ ...instance.bounds })),
  } satisfies Partial<MockSession>);
  logLaunch(context, state, setup);
  const executable = findHandler(state, setup.gameId)?.info.executableName ?? 'game.exe';
  launches.forEach(({ instance, binding }, index) =>
    launchInstance(context, state, instance, { executable, binding, delay: LAUNCH_INTERVAL_MS * (index + 1) }),
  );
  publishSetup(context, state, setup);
  return publishSession(context, state);
}

function logLaunch(context: MockContext, state: MockState, setup: MockSetup): void {
  const info = findHandler(state, setup.gameId)?.info;
  const count = state.session.instances.length;
  const handler = info?.hubVersion ? `handler v${info.hubVersion}` : 'handler';
  log(context, state, `Session started: ${setup.gameName}, ${handler}${info?.maintainer ? ` by ${info.maintainer}` : ''}`);
  const { mod } = setupFixtureFor(setup.gameId);
  if (mod) log(context, state, `${mod.name} mod found in the game folder`, 'success');
  log(context, state, `Steam emulation ready: ${count} accounts created (Goldberg)`);
  log(context, state, `Linked game files into ${count} instance folders`);
}

function launchInstance(
  context: MockContext,
  state: MockState,
  instance: SessionInstance,
  { executable, binding, delay }: { executable: string; binding: string; delay: number },
): void {
  const { session } = state;
  const number = instance.playerNumber;
  schedule(session.timers, delay, () => {
    instance.stage = 'starting';
    instance.processId = 9000 + number * 412 + Math.floor(Math.random() * 300);
    log(context, state, `Instance ${number} started: ${executable} (PID ${instance.processId})`, 'info', number);
    publishSession(context, state);
  });
  schedule(session.timers, delay + WINDOW_READY_MS, () => {
    instance.stage = 'ready';
    const { x, y, width, height } = instance.bounds;
    log(context, state, `Window ${number} placed at ${x},${y} ${width}×${height}, input bound to ${binding}`, 'success', number);
    if (session.instances.every((candidate) => candidate.stage === 'ready')) enterRunning(context, state);
    else publishSession(context, state);
  });
}

/** Every window is ready: the session runs. Keyboard and mouse sessions lock the inputs to the game windows. */
function enterRunning(context: MockContext, state: MockState): void {
  const { session } = state;
  const lockKey = state.settings['hotkeys.lockKey'] ?? 'End';
  const keyboards = session.instances.some((instance) => instance.devices.some((kind) => kind === 'rawKeyboard' || kind === 'rawMouse'));
  session.phase = 'running';
  session.reachedRunning = true;
  session.inputsLocked = keyboards;
  const ready = `All ${session.instances.length} players are ready.`;
  log(context, state, ready, 'success');
  if (keyboards) log(context, state, `Inputs locked: each keyboard and mouse goes to its window (${lockKey} to unlock)`);
  publishSession(context, state);
  notice(context, keyboards ? `${ready} Inputs are locked to the game windows: press ${lockKey} to unlock them.` : `${ready} Have fun!`);
  scheduleQuit(context, state);
  if (!keyboards) return;
  schedule(session.timers, INPUTS_LOCKED_MS, () => {
    session.inputsLocked = false;
    log(context, state, 'Inputs unlocked: the Windows cursor is released');
    publishSession(context, state);
    notice(context, 'Inputs unlocked.');
  });
}

// Instance restarts (Nucleus 2.5) ------------------------------------------------------------------------

/**
 * A copy that quits after the session started: `nucleus.mock.instance` = `crash:<player>` or `close:<player>`, with an
 * optional delay in ms (`crash:2@300`; 2.5 s by default).
 */
export function parseInstanceQuit(value: string | undefined): MockSession['quit'] {
  const match = /^(crash|close):(\d+)(?:@(\d+))?$/.exec(value?.trim() ?? '');
  if (!match) return undefined;
  return { kind: match[1] === 'crash' ? 'crash' : 'close', player: Number(match[2]), afterMs: match[3] ? Number(match[3]) : undefined };
}

/** Once running, the copy of `session.quit` quits: closed, or crashed (its automatic restart then fails, as on a broken game). */
function scheduleQuit(context: MockContext, state: MockState): void {
  const { session } = state;
  const instance = session.instances.find((candidate) => candidate.playerNumber === session.quit?.player);
  if (!session.quit || !instance) return;
  const crash = session.quit.kind === 'crash';
  const number = instance.playerNumber;
  schedule(session.timers, session.quit.afterMs ?? QUIT_AFTER_MS, () => {
    Object.assign(instance, { stage: 'exited', crashed: crash, restartFailed: false } satisfies Partial<SessionInstance>);
    if (crash) log(context, state, `Instance of player ${number} quit (pid ${instance.processId}, exit code -1073741819): Restart`, 'error', number);
    else log(context, state, `Instance of player ${number} closed (pid ${instance.processId}, exit code 0): not restarted`, 'info', number);
    publishSession(context, state);
    // Every copy closed: the session ends, as with the host.
    if (!crash && session.instances.every((candidate) => candidate.stage === 'exited')) return void schedule(session.timers, 300, () => endSession(context, state));
    if (!crash || setupFixtureFor(session.gameId ?? '').cantRestartInstances) return;
    schedule(session.timers, 500, () => {
      instance.stage = 'restarting';
      notice(context, 'Restarting the instance…', 1000);
      publishSession(context, state);
    });
    schedule(session.timers, 500 + RESTART_MS, () => {
      Object.assign(instance, { stage: 'exited', crashed: true, restartFailed: true } satisfies Partial<SessionInstance>);
      log(context, state, `Error: restart of the instance of player ${number} failed, it is left closed.`, 'error', number);
      notice(context, 'The instance could not be restarted');
      publishSession(context, state);
    });
  });
}

/** `session.restartInstance`: the same refusals as the host, then the copy starts again (closed first if it runs). */
function restartInstance(context: MockContext, state: MockState, playerNumber: number): SessionSnapshot {
  const { session } = state;
  const refusal = restartRefusal(state);
  if (refusal === 'noSession' || refusal === 'notReady') conflict(refusal, RESTART_REFUSALS[refusal] ?? refusal);
  const instance = session.instances.find((candidate) => candidate.playerNumber === playerNumber);
  if (!instance) throw new BridgeError('notFound', 'This player has no copy of the game to restart.', { reason: 'unknownPlayer' });
  if (refusal) conflict(refusal, RESTART_REFUSALS[refusal] ?? refusal);
  Object.assign(instance, { stage: 'restarting', crashed: false, restartFailed: false } satisfies Partial<SessionInstance>);
  log(context, state, `Restart requested for the instance of player ${playerNumber}`, 'info', playerNumber);
  notice(context, 'Restarting the instance…', 1000);
  schedule(session.timers, RESTART_MS, () => {
    instance.stage = 'starting';
    instance.processId = 9000 + playerNumber * 412 + Math.floor(Math.random() * 300);
    log(context, state, `Relaunch started game (PID ${instance.processId})`, 'info', playerNumber);
    publishSession(context, state);
  });
  schedule(session.timers, RESTART_MS + WINDOW_READY_MS, () => {
    instance.stage = 'ready';
    log(context, state, `Window ${playerNumber} placed again`, 'success', playerNumber);
    publishSession(context, state);
  });
  return publishSession(context, state);
}

/** Window areas of a tiling on the monitor of the instances. */
function tile(state: MockState, layoutIndex: number): Rect[] {
  const { instances, setupBounds } = state.session;
  const count = instances.length;
  const screen = MONITORS[instances[0]?.screenIndex ?? 0]?.bounds;
  if (!screen || TILINGS[layoutIndex] === 'as set up') return setupBounds.map((bounds) => ({ ...bounds }));
  const columns = TILINGS[layoutIndex] === 'side by side';
  return instances.map((_, index) => {
    const start = Math.round((index * (columns ? screen.width : screen.height)) / count);
    const end = Math.round(((index + 1) * (columns ? screen.width : screen.height)) / count);
    return columns
      ? { x: screen.x + start, y: screen.y, width: end - start, height: screen.height }
      : { x: screen.x, y: screen.y + start, width: screen.width, height: end - start };
  });
}

function placeWindows(state: MockState): void {
  const tiling = tile(state, state.session.layoutIndex);
  state.session.instances.forEach((instance, index) => {
    instance.bounds = tiling[index] ?? instance.bounds;
  });
}

/** Runs a session action, as its hotkey would. */
function runAction(context: MockContext, state: MockState, action: SessionAction): SessionSnapshot {
  const { session } = state;
  if (!sessionActionValues.includes(action)) invalidParams(`Unknown session action '${action}'.`);
  if (session.phase !== 'running') conflict('notRunning', 'No session is running.');
  if (!isAvailable(state, action)) conflict('actionUnavailable', `"${ACTIONS[action].label}" does not apply to this session.`);
  switch (action) {
    case 'toggleTopmost':
      session.topmost = !session.topmost;
      notice(context, session.topmost ? 'Game windows kept on top.' : 'Game windows no longer kept on top.');
      log(context, state, session.topmost ? 'Game windows kept on top' : 'Game windows no longer kept on top');
      break;
    case 'setFocus':
      log(context, state, 'Game windows brought to the front');
      break;
    case 'resetWindows':
      placeWindows(state);
      log(context, state, 'Windows put back in place');
      break;
    case 'cutscenesMode':
      session.cutscenes = !session.cutscenes;
      notice(context, session.cutscenes ? 'Cutscene mode: player 1 full size, the others muted.' : 'Cutscene mode off.');
      log(context, state, session.cutscenes ? 'Cutscene mode on' : 'Cutscene mode off');
      break;
    case 'switchLayout':
      session.layoutIndex = (session.layoutIndex + 1) % TILINGS.length;
      placeWindows(state);
      notice(context, `Layout: ${TILINGS[session.layoutIndex]}.`);
      log(context, state, `Layout switched to ${TILINGS[session.layoutIndex]}`);
      break;
    case 'toggleShortcutsReminder':
      notice(context, actionsOf(state).map(({ label, hotkey }) => `${hotkey ?? '—'}: ${label}`).join('\n'), 6000);
      break;
    case 'switchMergerForeground':
      log(context, state, 'Next window brought to the front of the merged window');
      break;
  }
  return publishSession(context, state);
}

/** A duration as h:mm:ss. */
const elapsed = (ms: number) => {
  const seconds = Math.max(0, Math.round(ms / 1000));
  const twoDigits = (value: number) => String(value).padStart(2, '0');
  return `${Math.floor(seconds / 3600)}:${twoDigits(Math.floor((seconds % 3600) / 60))}:${twoDigits(seconds % 60)}`;
};

/** Stops the session: instances exit, play time is recorded, the setup comes back. */
function stopSession(context: MockContext, state: MockState): SessionSnapshot {
  const { session } = state;
  if (!session.phase || session.phase === 'ending') return sessionSnapshot(state);
  if (session.inputsLocked) {
    const lockKey = state.settings['hotkeys.lockKey'] ?? 'End';
    conflict('inputsLocked', `Inputs are locked to the game windows. Press ${lockKey} to unlock them first.`);
  }
  clearTimers(session.timers);
  session.phase = 'ending';
  log(context, state, 'Stopping the session');
  const snapshot = publishSession(context, state);
  schedule(session.timers, 300, () => {
    for (const instance of session.instances) {
      instance.stage = 'exited';
      log(context, state, `Instance ${instance.playerNumber} closed`, 'info', instance.playerNumber);
    }
    publishSession(context, state);
  });
  schedule(session.timers, 900, () => endSession(context, state));
  return snapshot;
}

function endSession(context: MockContext, state: MockState): void {
  const { session } = state;
  const game = state.games.find((candidate) => candidate.summary.id === session.gameId);
  const duration = session.startedAt ? Date.now() - Date.parse(session.startedAt) : 0;
  if (game) {
    game.summary.playtimeSeconds += Math.round(duration / 1000);
    game.summary.lastPlayedAt = new Date().toISOString();
    game.summary.firstLaunch = false;
    game.contentFolderExists = true;
    context.emit('library.changed', { gameIds: [game.summary.id] });
  }
  log(context, state, `Session ended after ${elapsed(duration)}`, 'success');
  const errors = session.log.filter((entry) => entry.level === 'error');
  const endedAt = new Date().toISOString();
  session.lastSession = {
    gameId: session.gameId ?? '',
    gameName: session.gameName ?? '',
    startedAt: session.startedAt ?? endedAt,
    endedAt,
    launched: session.reachedRunning,
    players: session.players,
    error: session.error,
    errors: errors.slice(0, MAX_SUMMARY_ERRORS),
    errorCount: errors.length,
  };
  Object.assign(session, {
    phase: null,
    gameId: undefined,
    gameName: undefined,
    startedAt: undefined,
    instances: [],
    inputsLocked: false,
  } satisfies Partial<MockSession>);
  publishSession(context, state);
  if (state.setup) publishSetup(context, state, state.setup);
}

/** Mock of the `session` service: launches instances on timers from the open setup, runs actions, keeps a log. */
export function createSessionMock(context: MockContext, state: MockState): BridgeHandlers['session'] {
  return {
    get: () => sessionSnapshot(state),
    play: () => startSession(context, state),
    stop: () => stopSession(context, state),
    runAction: ({ action }) => runAction(context, state, action),
    getLog: () => state.session.log,
    restartInstance: ({ playerNumber }) => restartInstance(context, state, playerNumber),
  };
}
