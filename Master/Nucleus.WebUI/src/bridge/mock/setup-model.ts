// The setup simulation: commands on the setup state. Each validates first and throws like the host
// (`conflict` with a reason, `notFound`, `invalidParams`), then mutates. Snapshots are built in setup-snapshot.ts.
import { edgeValues, screenLayoutValues, type DeviceKind, type Edge, type ScreenLayout } from '../generated/contract.ts';
import { conflict, invalidParams, notFound } from './failures.ts';
import { LATE_GAMEPAD, MONITORS, detectedDevice, devicesFor, isGamepad, setupFixtureFor } from './fixtures-setup.ts';
import { areaAt, cellCount, expandArea, fits, layoutGrid, moveEdge, overlaps, sameArea, slotArea, slotIndexOf } from './setup-grid.ts';
import type {
  Grid,
  GridArea,
  MockDevice,
  MockGame,
  MockPlayer,
  MockProfile,
  MockScreen,
  MockSetup,
  MockState,
  ProfileSeat,
  StoredProfileSettings,
} from './state.ts';

/** Layout of the primary monitor when a setup opens. */
function defaultLayout(maxPlayers: number): ScreenLayout {
  if (maxPlayers >= 4) return 'fourPlayers';
  return maxPlayers >= 2 ? 'dualVertical' : 'fullScreen';
}

/** A new setup for a game: both monitors, the detected devices, the handler's options and custom steps. */
export function createSetup(game: MockGame, customGrid: Grid): MockSetup {
  const { id, name, inputs, maxPlayers } = game.summary;
  const fixture = setupFixtureFor(id);
  return {
    gameId: id,
    gameName: name,
    revision: 0,
    step: 0,
    screens: MONITORS.map((monitor, index) => ({
      ...monitor,
      index,
      bounds: { ...monitor.bounds },
      layout: index === 0 ? defaultLayout(maxPlayers) : 'fullScreen',
      layoutLocked: false,
    })),
    customGrid,
    devices: devicesFor(inputs, game.useApiIndex),
    players: [],
    options: fixture.options,
    customSteps: fixture.customSteps,
    capabilities: {
      maxPlayers,
      gamepads: inputs.xInput || inputs.dInput,
      keyboard: inputs.keyboard,
      multipleKeyboardsAndMice: inputs.multipleKeyboards,
      playersPerInstance: fixture.playersPerInstance,
    },
    useApiIndex: game.useApiIndex,
    saveProfileOnLaunch: game.saveProfileOnLaunch,
    profile: null,
    pendingSettings: null,
    busyChecks: new Set(),
    steamClosed: false,
    timers: new Set(),
  };
}

// Lookups ---------------------------------------------------------------------------------------------

/** The open setup. Throws `conflict` (reason `noSetup`) when none is open. */
export function requireSetup(state: MockState): MockSetup {
  return state.setup ?? conflict('noSetup', 'Open the setup of a game first.');
}

/** The open setup, when it can change: refused while a session runs. */
export function requireEditable(state: MockState): MockSetup {
  const setup = requireSetup(state);
  if (state.session.phase) conflict('sessionRunning', 'A session is running: stop it before changing the setup.');
  return setup;
}

/** Id of a player: the id of its first device. */
export const playerIdOf = (player: MockPlayer) => player.deviceIds[0] ?? '';

/** Player driven by a device (not as a guest). */
export const ownerOf = (setup: MockSetup, deviceId: string) => setup.players.find((player) => player.deviceIds.includes(deviceId));

/** Player a guest device joins. */
export const hostOf = (setup: MockSetup, deviceId: string) => setup.players.find((player) => player.guestDeviceIds.includes(deviceId));

const isAssigned = (setup: MockSetup, deviceId: string) => ownerOf(setup, deviceId) !== undefined || hostOf(setup, deviceId) !== undefined;

/** Grid of a monitor's current layout. */
export const gridOf = (setup: MockSetup, screen: MockScreen) => layoutGrid(screen.layout, setup.customGrid);

/** A device of the setup. Throws `notFound`. */
export function findDevice(setup: MockSetup, deviceId: string): MockDevice {
  return setup.devices.find((device) => device.id === deviceId) ?? notFound(`No device '${deviceId}' in this setup.`);
}

/** A player of the setup. Throws `notFound`. */
function findPlayer(setup: MockSetup, playerId: string): MockPlayer {
  return setup.players.find((player) => playerIdOf(player) === playerId) ?? notFound(`No player '${playerId}' in this setup.`);
}

/** A monitor of the setup. Throws `notFound`. */
export function findScreen(setup: MockSetup, screenIndex: number): MockScreen {
  return setup.screens.find((screen) => screen.index === screenIndex) ?? notFound(`No screen ${screenIndex}.`);
}

/** Players in launch order with their numbers: the profile's, else their position (screen, then row, then column). */
export function numberedPlayers(setup: MockSetup): { player: MockPlayer; number: number }[] {
  const byPosition = [...setup.players].sort((a, b) => a.screenIndex - b.screenIndex || a.area.row - b.area.row || a.area.col - b.area.col);
  return byPosition.map((player, index) => ({ player, number: player.profileNumber ?? index + 1 })).sort((a, b) => a.number - b.number);
}

/** Number of a player. Throws `notFound`. */
export function playerNumber(setup: MockSetup, playerId: string): number {
  const numbered = numberedPlayers(setup).find(({ player }) => playerIdOf(player) === playerId);
  return numbered?.number ?? notFound(`No player '${playerId}' in this setup.`);
}

/** Whether no other player of the monitor covers part of `area`. */
const isFree = (setup: MockSetup, player: MockPlayer, area: GridArea) =>
  !setup.players.some((other) => other !== player && other.screenIndex === player.screenIndex && overlaps(other.area, area));

/** Area a player would cover once expanded over the free neighbouring cells. */
export function expansionOf(setup: MockSetup, player: MockPlayer): GridArea {
  const grid = gridOf(setup, findScreen(setup, player.screenIndex));
  return expandArea(player.area, grid, (candidate) => isFree(setup, player, candidate));
}

// Placement -------------------------------------------------------------------------------------------

const seatCount = (setup: MockSetup) => setup.players.reduce((count, player) => count + 1 + player.guestDeviceIds.length, 0);

function requireUnlocked(setup: MockSetup): void {
  if (setup.profile) {
    conflict('profileLocked', 'A profile is loaded: its players are matched as their devices are used. Unload it to place players freely.');
  }
}

function requireRevealed(device: MockDevice): void {
  if (!device.revealed) {
    const action = device.needsButtonPress ? `Press a button on ${device.name} first.` : `Use ${device.name} once so it can be placed.`;
    conflict('deviceNotRevealed', action);
  }
}

/** Refuses a device that would become one more player when the handler's maximum is reached. */
function requireSeatFor(setup: MockSetup, deviceId: string): void {
  const owner = ownerOf(setup, deviceId);
  const addsSeat = !hostOf(setup, deviceId) && (!owner || owner.deviceIds.length > 1);
  if (addsSeat && seatCount(setup) >= setup.capabilities.maxPlayers) {
    conflict('maxPlayersReached', `${setup.gameName} supports up to ${setup.capabilities.maxPlayers} players.`);
  }
}

/** Takes a device off its player or host. A player left without devices is removed; its guests go back to the tray. */
function detach(setup: MockSetup, deviceId: string): void {
  const host = hostOf(setup, deviceId);
  if (host) host.guestDeviceIds = host.guestDeviceIds.filter((id) => id !== deviceId);
  const owner = ownerOf(setup, deviceId);
  if (!owner) return;
  owner.deviceIds = owner.deviceIds.filter((id) => id !== deviceId);
  if (owner.deviceIds.length === 0) setup.players = setup.players.filter((player) => player !== owner);
}

/** Whether a device completes a player into a raw keyboard plus a raw mouse. */
function completesPair(setup: MockSetup, player: MockPlayer, device: MockDevice): boolean {
  const [only, ...others] = player.deviceIds;
  if (only === undefined || others.length > 0) return false;
  const kinds = new Set<DeviceKind>([findDevice(setup, only).kind, device.kind]);
  return kinds.has('rawKeyboard') && kinds.has('rawMouse');
}

/** Exchanges the places of two players. */
function swapPlaces(a: MockPlayer, b: MockPlayer): void {
  const { screenIndex, area, home } = a;
  Object.assign(a, { screenIndex: b.screenIndex, area: b.area, home: b.home });
  Object.assign(b, { screenIndex, area, home });
}

/**
 * Places a device on an area of a monitor: moves its player when it is the player's first device (swapping with
 * the player already there), joins a raw keyboard and a raw mouse into one player, or creates a player.
 */
function place(setup: MockSetup, device: MockDevice, screenIndex: number, area: GridArea): void {
  requireRevealed(device);
  const owner = ownerOf(setup, device.id);
  const movesPlayer = owner !== undefined && playerIdOf(owner) === device.id;
  const blocking = setup.players.filter((player) => player !== owner && player.screenIndex === screenIndex && overlaps(player.area, area));
  const [occupant, ...others] = blocking;
  if (occupant && others.length === 0 && completesPair(setup, occupant, device)) {
    detach(setup, device.id);
    occupant.deviceIds.push(device.id);
    return;
  }
  if (occupant && others.length === 0 && movesPlayer) {
    swapPlaces(owner, occupant);
    return;
  }
  if (occupant) conflict('slotOccupied', 'That place is taken: unassign its player or pick another one.');
  if (movesPlayer) {
    Object.assign(owner, { screenIndex, area, home: area });
    return;
  }
  if (owner && owner.screenIndex === screenIndex && overlaps(owner.area, area)) return;
  requireSeatFor(setup, device.id);
  detach(setup, device.id);
  setup.players.push({ deviceIds: [device.id], guestDeviceIds: [], screenIndex, area, home: area });
}

/** `setup.assignToSlot`. */
export function assignToSlot(setup: MockSetup, deviceId: string, screenIndex: number, slotIndex: number): void {
  requireUnlocked(setup);
  const device = findDevice(setup, deviceId);
  const screen = findScreen(setup, screenIndex);
  if (screen.layout === 'manual') conflict('manualLayout', 'This screen uses the manual layout: place players with assignAt.');
  const grid = gridOf(setup, screen);
  if (!Number.isInteger(slotIndex) || slotIndex < 0 || slotIndex >= cellCount(grid)) {
    notFound(`No slot ${slotIndex} on screen ${screenIndex}.`);
  }
  place(setup, device, screenIndex, slotArea(grid, slotIndex));
}

/** `setup.assignAt`: the slot under the point, or a manual placement centred on it. */
export function assignAt(setup: MockSetup, deviceId: string, screenIndex: number, x: number, y: number): void {
  requireUnlocked(setup);
  const device = findDevice(setup, deviceId);
  const screen = findScreen(setup, screenIndex);
  if (!(x >= 0 && x <= 1 && y >= 0 && y <= 1)) invalidParams('x and y must be between 0 and 1.');
  place(setup, device, screenIndex, areaAt(gridOf(setup, screen), x, y, screen.layout === 'manual'));
}

/** `setup.unassign`. Allowed with a profile loaded: its player waits for a device again. */
export function unassign(setup: MockSetup, deviceId: string): void {
  findDevice(setup, deviceId);
  detach(setup, deviceId);
}

/** `setup.addGuest`. */
export function addGuest(setup: MockSetup, playerId: string, deviceId: string): void {
  requireUnlocked(setup);
  const { playersPerInstance } = setup.capabilities;
  if (playersPerInstance <= 1) conflict('guestsNotSupported', `${setup.gameName} runs one player per instance.`);
  const host = findPlayer(setup, playerId);
  const device = findDevice(setup, deviceId);
  requireRevealed(device);
  if (host.guestDeviceIds.includes(deviceId)) return;
  if (!isGamepad(device.kind) || !host.deviceIds.some((id) => isGamepad(findDevice(setup, id).kind))) {
    conflict('notAGamepad', 'Only a gamepad can join the instance of a gamepad player.');
  }
  if (host.deviceIds.includes(deviceId)) conflict('samePlayer', 'A gamepad cannot be its own guest.');
  if (host.guestDeviceIds.length >= playersPerInstance - 1) {
    conflict('instanceFull', `An instance holds ${playersPerInstance} players at most.`);
  }
  requireSeatFor(setup, deviceId);
  detach(setup, deviceId);
  host.guestDeviceIds.push(deviceId);
}

/** `setup.removeGuest`. */
export function removeGuest(setup: MockSetup, deviceId: string): void {
  findDevice(setup, deviceId);
  if (!hostOf(setup, deviceId)) conflict('notAGuest', 'This gamepad is not a guest.');
  detach(setup, deviceId);
}

/** `setup.toggleExpand`: grows over the free neighbouring cells, or shrinks back to where it was placed. */
export function toggleExpand(setup: MockSetup, playerId: string): void {
  requireUnlocked(setup);
  const player = findPlayer(setup, playerId);
  if (!sameArea(player.area, player.home)) {
    if (!isFree(setup, player, player.home)) conflict('slotOccupied', 'Another player took the original place of this player.');
    player.area = player.home;
    return;
  }
  const expanded = expansionOf(setup, player);
  if (sameArea(expanded, player.area)) conflict('cannotExpand', 'There is no free space next to this player.');
  player.area = expanded;
}

/** `setup.resize`: moves one edge of a player's area by whole cells. */
export function resize(setup: MockSetup, playerId: string, edge: Edge, steps: number): void {
  requireUnlocked(setup);
  const player = findPlayer(setup, playerId);
  if (!edgeValues.includes(edge)) invalidParams(`Unknown edge '${edge}'.`);
  if (!Number.isInteger(steps)) invalidParams('steps must be a whole number.');
  const grid = gridOf(setup, findScreen(setup, player.screenIndex));
  if (cellCount(grid) < 4) conflict('cannotResize', 'Resizing needs a layout with 4 slots or more.');
  const resized = moveEdge(player.area, edge, steps);
  if (!fits(resized, grid)) conflict('outOfBounds', 'A player cannot grow past the screen edge or shrink below one cell.');
  if (!isFree(setup, player, resized)) conflict('slotOccupied', 'Another player is in the way.');
  player.area = resized;
}

/** `setup.setLayout`: players on that monitor go back to the tray. */
export function setLayout(setup: MockSetup, screenIndex: number, layout: ScreenLayout): void {
  const screen = findScreen(setup, screenIndex);
  if (!screenLayoutValues.includes(layout)) invalidParams(`Unknown layout '${layout}'.`);
  if (screen.layoutLocked) conflict('profileLocked', 'The loaded profile fixes this layout. Unload it first.');
  setup.players = setup.players.filter((player) => player.screenIndex !== screenIndex);
  screen.layout = layout;
}

/** Free slots of every monitor (manual layouts excluded), monitor by monitor, row by row. */
function freeSlots(setup: MockSetup): { screenIndex: number; area: GridArea }[] {
  return setup.screens
    .filter((screen) => screen.layout !== 'manual')
    .flatMap((screen) => {
      const grid = gridOf(setup, screen);
      return Array.from({ length: cellCount(grid) }, (_, index) => slotArea(grid, index))
        .filter((area) => !setup.players.some((player) => player.screenIndex === screen.index && overlaps(player.area, area)))
        .map((area) => ({ screenIndex: screen.index, area }));
    });
}

/** Revealed, unassigned devices grouped into future players: a gamepad, the shared keyboard, a raw keyboard with a raw mouse. */
function unplacedGroups(setup: MockSetup): string[][] {
  const available = setup.devices.filter((device) => device.revealed && !isAssigned(setup, device.id));
  const mice = available.filter((device) => device.kind === 'rawMouse');
  return available
    .filter((device) => device.kind !== 'rawMouse')
    .map((device) => {
      const mouse = device.kind === 'rawKeyboard' ? mice.shift() : undefined;
      return mouse ? [device.id, mouse.id] : [device.id];
    });
}

/** `setup.autoFill`: every revealed, unassigned device in the free slots. With a profile, matches its waiting players. */
export function autoFill(setup: MockSetup): void {
  if (setup.profile) {
    matchAll(setup);
    return;
  }
  const slots = freeSlots(setup);
  for (const deviceIds of unplacedGroups(setup)) {
    const slot = slots.shift();
    if (!slot || seatCount(setup) >= setup.capabilities.maxPlayers) return;
    setup.players.push({ deviceIds, guestDeviceIds: [], screenIndex: slot.screenIndex, area: slot.area, home: slot.area });
  }
}

/** Layouts a monitor goes through when press-to-join needs more slots. */
const GROWTH: readonly ScreenLayout[] = ['fullScreen', 'dualVertical', 'fourPlayers', 'sixPlayers', 'eightPlayers', 'sixteenPlayers'];

/** Switches the first monitor that can grow to a layout with more slots, its players keeping their order. */
function growForOneMore(setup: MockSetup): boolean {
  for (const screen of setup.screens) {
    const cells = cellCount(gridOf(setup, screen));
    const larger = GROWTH.find((layout) => cellCount(layoutGrid(layout, setup.customGrid)) > cells);
    if (screen.layoutLocked || !GROWTH.includes(screen.layout) || !larger) continue;
    screen.layout = larger;
    const grid = gridOf(setup, screen);
    numberedPlayers(setup)
      .filter(({ player }) => player.screenIndex === screen.index)
      .forEach(({ player }, index) => Object.assign(player, { area: slotArea(grid, index), home: slotArea(grid, index) }));
    return true;
  }
  return false;
}

/** `setup.join`: press-to-join in the first free slot, growing a monitor's layout when every slot is taken. */
export function join(setup: MockSetup, deviceId: string): void {
  const device = findDevice(setup, deviceId);
  requireRevealed(device);
  if (isAssigned(setup, deviceId)) return;
  if (setup.profile) {
    if (!matchDevice(setup, device)) conflict('profileLocked', `The loaded profile is not waiting for ${device.name}.`);
    return;
  }
  // Like the host: a raw keyboard or mouse first completes a player that has only the other half of the pair.
  const partner = setup.players.find((player) => completesPair(setup, player, device));
  if (partner) {
    partner.deviceIds.push(deviceId);
    return;
  }
  requireSeatFor(setup, deviceId);
  const slot = freeSlots(setup)[0] ?? (growForOneMore(setup) ? freeSlots(setup)[0] : undefined);
  if (!slot) conflict('full', 'Every screen is full: pick layouts with more slots.');
  setup.players.push({ deviceIds: [deviceId], guestDeviceIds: [], screenIndex: slot.screenIndex, area: slot.area, home: slot.area });
}

/** `setup.clearAll`: every player back to the tray, profile unloaded. */
export function clearAll(setup: MockSetup): void {
  setup.players = [];
  unloadProfile(setup);
}

// Options and steps -----------------------------------------------------------------------------------

function findOption(setup: MockSetup, key: string) {
  return setup.options.find((option) => option.key === key) ?? notFound(`No option '${key}'.`);
}

/** `setup.setOptionChoice`. */
export function setOptionChoice(setup: MockSetup, key: string, index: number): void {
  const option = findOption(setup, key);
  if (option.kind !== 'choice') invalidParams(`'${option.name}' is not a choice.`);
  if (!option.choices.some((choice) => choice.index === index)) invalidParams(`'${option.name}' has no value ${index}.`);
  option.selectedIndex = index;
}

/** `setup.setOptionText`. */
export function setOptionText(setup: MockSetup, key: string, text: string): void {
  const option = findOption(setup, key);
  if (option.kind !== 'text') invalidParams(`'${option.name}' is not a text option.`);
  if (text.length > 200) invalidParams(`'${option.name}' can be 200 characters at most.`);
  option.text = text;
}

/** `setup.selectStepItem`. */
export function selectStepItem(setup: MockSetup, stepIndex: number, itemIndex: number): void {
  const step = setup.customSteps.find((candidate) => candidate.index === stepIndex) ?? notFound(`No custom step ${stepIndex}.`);
  if (!step.items.some((item) => item.index === itemIndex)) invalidParams(`'${step.title}' has no item ${itemIndex}.`);
  step.selectedIndex = itemIndex;
  step.autoSelected = false;
}

// Devices ---------------------------------------------------------------------------------------------

/** Marks a device as identified (button pressed, key typed, mouse moved). */
export function reveal(device: MockDevice): void {
  device.revealed = true;
  device.needsButtonPress = false;
}

/** `setup.setUseApiIndex`: with API indexes, every gamepad is identified at once. */
export function setUseApiIndex(setup: MockSetup, enabled: boolean): void {
  setup.useApiIndex = enabled;
  if (enabled) setup.devices.filter((device) => isGamepad(device.kind)).forEach(reveal);
}

/** Rescans devices: finds the gamepad plugged in late, once. Returns the devices found. */
export function rescanDevices(setup: MockSetup): MockDevice[] {
  if (!setup.capabilities.gamepads || setup.devices.some((device) => device.id === LATE_GAMEPAD.id)) return [];
  const device = detectedDevice(LATE_GAMEPAD, setup.useApiIndex);
  setup.devices.push(device);
  return [device];
}

// Profiles --------------------------------------------------------------------------------------------

/** The next profile player waiting for a device. */
export function nextSeat(setup: MockSetup): ProfileSeat | undefined {
  return setup.profile?.seats.find((seat) => !setup.players.some((player) => player.profileNumber === seat.number));
}

const fitsSeat = (kind: DeviceKind, seat: ProfileSeat) => (isGamepad(seat.device) ? isGamepad(kind) : kind === seat.device);

/** Gives a used device to the next profile player waiting for its kind (a raw keyboard brings a free raw mouse). */
export function matchDevice(setup: MockSetup, device: MockDevice): boolean {
  const seat = nextSeat(setup);
  if (!seat || !device.revealed || isAssigned(setup, device.id) || !fitsSeat(device.kind, seat)) return false;
  const mouse =
    device.kind === 'rawKeyboard'
      ? setup.devices.find((candidate) => candidate.kind === 'rawMouse' && candidate.revealed && !isAssigned(setup, candidate.id))
      : undefined;
  const area = slotArea(gridOf(setup, findScreen(setup, seat.screenIndex)), seat.slotIndex);
  setup.players.push({
    deviceIds: mouse ? [device.id, mouse.id] : [device.id],
    guestDeviceIds: [],
    screenIndex: seat.screenIndex,
    area,
    home: area,
    profileNumber: seat.number,
  });
  return true;
}

/** Matches every revealed device it can to the waiting profile players. */
function matchAll(setup: MockSetup): void {
  let matched = true;
  while (matched) matched = setup.devices.some((device) => matchDevice(setup, device));
}

/** Loads a profile: its layouts (locked), options and step selections. Its players then wait for devices. */
export function applyProfile(setup: MockSetup, profile: MockProfile, id: number): void {
  setup.players = [];
  for (const screen of setup.screens) {
    screen.layout = profile.layouts[screen.index] ?? 'fullScreen';
    screen.layoutLocked = true;
  }
  for (const option of setup.options) {
    const saved = profile.options[option.key];
    if (option.kind === 'choice' && typeof saved === 'number') option.selectedIndex = saved;
    if (option.kind === 'text' && typeof saved === 'string') option.text = saved;
  }
  for (const step of setup.customSteps) {
    const item = profile.stepItems[step.index];
    if (item !== undefined) Object.assign(step, { selectedIndex: item, autoSelected: true });
  }
  setup.profile = { id, seats: structuredClone(profile.seats), autoPlay: profile.settings.autoPlay };
  setup.pendingSettings = null;
  setup.step = 0;
}

/** Unloads the profile: layouts unlocked, players kept but numbered by position again. */
export function unloadProfile(setup: MockSetup): void {
  setup.profile = null;
  for (const screen of setup.screens) screen.layoutLocked = false;
  for (const step of setup.customSteps) step.autoSelected = false;
  for (const player of setup.players) delete player.profileNumber;
}

/** The open setup as a new profile, with the given settings. */
export function profileFromSetup(setup: MockSetup, settings: StoredProfileSettings): MockProfile {
  const seats = numberedPlayers(setup).map(({ player, number }, index): ProfileSeat => {
    const screen = findScreen(setup, player.screenIndex);
    const kinds = player.deviceIds.map((id) => findDevice(setup, id).kind);
    return {
      number,
      nickname: settings.slots[index]?.nickname || undefined,
      device: kinds.find((kind) => kind !== 'rawMouse') ?? 'rawMouse',
      screenIndex: screen.index,
      guests: player.guestDeviceIds.length,
      slotIndex: screen.layout === 'manual' ? 0 : slotIndexOf(gridOf(setup, screen), player.home),
    };
  });
  const options = setup.options.flatMap((option): [string, number | string][] => {
    const value = option.kind === 'choice' ? option.selectedIndex : option.kind === 'text' ? option.text : undefined;
    return value === undefined ? [] : [[option.key, value]];
  });
  const stepItems = setup.customSteps.flatMap((step) => (step.selectedIndex === undefined ? [] : [[step.index, step.selectedIndex]]));
  return {
    settings: structuredClone(settings),
    seats,
    layouts: setup.screens.map((screen) => screen.layout),
    options: Object.fromEntries(options),
    stepItems: Object.fromEntries(stepItems),
  };
}
