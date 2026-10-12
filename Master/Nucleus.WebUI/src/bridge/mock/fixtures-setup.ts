// Static data of the setup simulation: monitors, input devices, handler options and custom steps,
// and the audio, network and resolution lists shared by settings and profiles.
import type { DeviceKind, GameInputs, Rect, SettingChoice, SetupCustomStep, SetupOption } from '../generated/contract.ts';
import { igdbImage } from './fixtures-screenshots.ts';
import type { MockDevice } from './state.ts';

/** The PC's monitors: a 1440p primary and a 1080p secondary on its right, bottom-aligned. */
export const MONITORS: readonly { displayIndex: number; bounds: Rect; isPrimary: boolean }[] = [
  { displayIndex: 1, bounds: { x: 0, y: 0, width: 2560, height: 1440 }, isPrimary: true },
  { displayIndex: 2, bounds: { x: 2560, y: 360, width: 1920, height: 1080 }, isPrimary: false },
];

type DeviceTemplate = Pick<MockDevice, 'id' | 'kind' | 'name' | 'apiIndex'>;

const XINPUT_PADS: readonly DeviceTemplate[] = [
  { id: 'xinput:0', kind: 'xInput', name: 'Xbox Wireless Controller', apiIndex: 0 },
  { id: 'xinput:1', kind: 'xInput', name: '8BitDo Pro 2', apiIndex: 1 },
];
const DINPUT_PAD: DeviceTemplate = { id: 'dinput:{6f1d2b61-d5a0-11cf-bfc7-444553540000}', kind: 'dInput', name: 'DualSense Wireless Controller', apiIndex: 0 };
const SHARED_KEYBOARD: DeviceTemplate = { id: 'kb:single', kind: 'keyboard', name: 'Keyboard and mouse', apiIndex: 0 };
const RAW_KEYBOARDS_AND_MICE: readonly DeviceTemplate[] = [
  { id: 'rawkb:046d-c33e', kind: 'rawKeyboard', name: 'Logitech G915', apiIndex: 0 },
  { id: 'rawmouse:046d-c08b', kind: 'rawMouse', name: 'Logitech G502', apiIndex: 0 },
  { id: 'rawkb:3434-0221', kind: 'rawKeyboard', name: 'Keychron K2', apiIndex: 1 },
  { id: 'rawmouse:1532-0084', kind: 'rawMouse', name: 'Razer DeathAdder V2', apiIndex: 1 },
];
/** Gamepad that `setup.refreshDevices` finds, as if it had just been plugged in. */
export const LATE_GAMEPAD: DeviceTemplate = { id: 'sdl:0', kind: 'sdl', name: 'Nintendo Switch Pro Controller', apiIndex: 0 };

/** Whether a device kind is a gamepad. */
export const isGamepad = (kind: DeviceKind) => kind === 'xInput' || kind === 'dInput' || kind === 'sdl';

/**
 * A detected device in its initial state. XInput pads are identified by their slot; other gamepads need a
 * button press unless API indexes are used; keyboards and mice appear once used.
 */
export function detectedDevice(template: DeviceTemplate, useApiIndex: boolean): MockDevice {
  const pressToIdentify = template.kind === 'dInput' || template.kind === 'sdl';
  return {
    ...template,
    revealed: template.kind === 'xInput' || (pressToIdentify && useApiIndex),
    needsButtonPress: pressToIdentify && !useApiIndex,
    isVirtual: false,
  };
}

/** Devices detected for a handler: gamepads, then the shared keyboard or the raw keyboards and mice. */
export function devicesFor(inputs: GameInputs, useApiIndex: boolean): MockDevice[] {
  const templates = [
    ...(inputs.xInput || inputs.dInput ? XINPUT_PADS : []),
    ...(inputs.dInput ? [DINPUT_PAD] : []),
    ...(inputs.multipleKeyboards ? RAW_KEYBOARDS_AND_MICE : inputs.keyboard ? [SHARED_KEYBOARD] : []),
  ];
  return templates.map((template) => detectedDevice(template, useApiIndex));
}

const choices = (labels: readonly string[]) => labels.map((label, index) => ({ index, label }));

const minecraftShot = (imageId: string) => igdbImage(imageId, 'screenshot_big');

/** Handler options, custom steps, players per instance and pre-launch checks of a game. */
export interface GameSetupFixture {
  options: SetupOption[];
  customSteps: SetupCustomStep[];
  playersPerInstance: number;
  /** The handler only runs with administrator rights. */
  requiresAdmin: boolean;
  /** The game saves in Documents, which OneDrive syncs. */
  savesInOneDrive: boolean;
  /** Mod the handler ships, checked in the game folder. */
  mod?: { name: string; file: string };
  /** Steam must be closed so each instance signs in with its own emulated account. */
  closeSteam: boolean;
}

const GAME_SETUPS: Readonly<Record<string, Partial<GameSetupFixture>>> = {
  EldenRing: {
    options: [
      { key: 'password', name: 'Seamless Co-op password', description: 'Every instance uses it to find the host.', kind: 'text', choices: [], text: 'tarnished' },
      { key: 'language', name: 'Game language', description: 'Applied to every instance.', kind: 'choice', choices: choices(['English', 'Français', 'Deutsch', 'Español', 'Italiano', '日本語']), selectedIndex: 0 },
      { key: 'modLoader', name: 'Mod Loader', description: "Loads mods from the game's mods folder.", kind: 'choice', choices: choices(['Disabled', 'Enabled']), selectedIndex: 1 },
      { key: 'erscSettings', name: 'Seamless Co-op settings', description: 'Edited in ersc_settings.ini: the handler default is used.', kind: 'unsupported', choices: [] },
    ],
    mod: { name: 'Seamless Co-op', file: 'ersc.dll' },
    closeSteam: true,
  },
  Minecraft: {
    options: [
      { key: 'memory', name: 'Memory per instance', description: 'RAM given to each Java process.', kind: 'choice', choices: choices(['2 GB', '4 GB', '6 GB', '8 GB']), selectedIndex: 1 },
    ],
    customSteps: [
      {
        index: 0,
        title: 'Pick a game version',
        optionKey: 'version',
        required: true,
        autoSelected: false,
        items: [
          { index: 0, label: '1.21.1 Fabric', details: 'Latest release with Fabric and Sodium. Recommended.', imageUrl: minecraftShot('sclnvg') },
          { index: 1, label: '1.20.4 Forge', details: 'For modpacks that need Forge.', imageUrl: minecraftShot('sctlac') },
          { index: 2, label: '1.8.9 Vanilla', details: 'The classic PvP version, lightest on older PCs.', imageUrl: minecraftShot('sctlad') },
        ],
      },
      {
        index: 1,
        title: 'Pick a world',
        optionKey: 'world',
        required: false,
        autoSelected: false,
        items: [
          { index: 0, label: 'New world', details: 'Player 1 creates a world and opens it to LAN.' },
          { index: 1, label: 'Survival island', details: 'The saved survival world, shared from player 1.' },
          { index: 2, label: 'Creative flat', details: 'A flat creative world to build together.' },
        ],
      },
    ],
  },
  HaloMCC: {
    options: [
      {
        key: 'game',
        name: 'Game',
        description: 'Game started in every instance.',
        kind: 'choice',
        choices: choices(['Halo: Combat Evolved Anniversary', 'Halo 2: Anniversary', 'Halo 3', 'Halo 3: ODST', 'Halo: Reach', 'Halo 4']),
        selectedIndex: 2,
      },
    ],
    requiresAdmin: true,
  },
  'Borderlands 2': { savesInOneDrive: true },
  ResidentEvil5: { playersPerInstance: 2 },
};

/** A game's setup data, with fresh copies of its options and custom steps (none for most games). */
export function setupFixtureFor(gameId: string): GameSetupFixture {
  const fixture = GAME_SETUPS[gameId] ?? {};
  return {
    ...fixture,
    options: structuredClone(fixture.options ?? []),
    customSteps: structuredClone(fixture.customSteps ?? []),
    playersPerInstance: fixture.playersPerInstance ?? 1,
    requiresAdmin: fixture.requiresAdmin ?? false,
    savesInOneDrive: fixture.savesInOneDrive ?? false,
    closeSteam: fixture.closeSteam ?? false,
  };
}

/** Audio outputs (endpoint ids as stored in Settings.ini). */
export const AUDIO_OUTPUTS: readonly SettingChoice[] = [
  { value: 'Default', label: 'Default output' },
  { value: '{0.0.0.00000000}.{8f3b2c1a-5d4e-4f6a-9b7c-1e2d3f4a5b6c}', label: 'Headset (SteelSeries Arctis 7)' },
  { value: '{0.0.0.00000000}.{2a7c9e41-0b3d-4c58-8e6f-7a1b2c3d4e5f}', label: 'TV (NVIDIA High Definition Audio)' },
  { value: '{0.0.0.00000000}.{c4d5e6f7-1a2b-4c3d-8e9f-0a1b2c3d4e5f}', label: 'Speakers (Realtek(R) Audio)' },
  { value: '{0.0.0.00000000}.{5e6f7a8b-9c0d-4e1f-a2b3-c4d5e6f7a8b9}', label: 'Headphones (DualSense Wireless Controller)' },
];

/** Network interfaces. */
export const NETWORKS: readonly SettingChoice[] = [
  { value: 'Automatic', label: 'Automatic' },
  { value: 'Ethernet', label: 'Ethernet (Realtek PCIe 2.5GbE)' },
  { value: 'Wi-Fi', label: 'Wi-Fi (Intel Wi-Fi 6E AX211)' },
];

/** Resolutions of the merged window. */
export const MERGER_RESOLUTIONS: readonly SettingChoice[] = ['1920X1080', '2560X1440', '3440X1440', '3840X2160'].map((value) => ({
  value,
  label: value.replace('X', ' × '),
}));

/** Background colours behind the game windows. */
export const SPLIT_COLORS: readonly SettingChoice[] = [
  { value: 'Black', label: 'Black' },
  { value: 'Gray', label: 'Gray' },
  { value: 'White', label: 'White' },
  { value: 'DarkBlue', label: 'Dark blue' },
  { value: 'DarkGreen', label: 'Dark green' },
];
