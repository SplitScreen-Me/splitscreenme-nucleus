// Settings schema of the mock host, modelled on Settings.ini, with the initial values and player identities.
import type { PlayerIdentities, SettingChoice, SettingCondition, SettingField, SettingsSchema } from '../generated/contract.ts';
import { AUDIO_OUTPUTS, MERGER_RESOLUTIONS, NETWORKS, SPLIT_COLORS } from './fixtures-setup.ts';

/**
 * Gamepad buttons, by the value stored in a combination (XInput button flags; Guide and the triggers are Nucleus
 * values), as in the `[XShortcuts]` section of Settings.ini, for example `1024+16` for Guide+Start.
 */
export const GAMEPAD_BUTTONS: readonly SettingChoice[] = [
  { value: '4096', label: 'A' },
  { value: '8192', label: 'B' },
  { value: '16384', label: 'X' },
  { value: '32768', label: 'Y' },
  { value: '256', label: 'LB' },
  { value: '512', label: 'RB' },
  { value: '9999', label: 'LT' },
  { value: '10000', label: 'RT' },
  { value: '32', label: 'Back' },
  { value: '16', label: 'Start' },
  { value: '1024', label: 'Guide' },
  { value: '64', label: 'Left stick' },
  { value: '128', label: 'Right stick' },
  { value: '1', label: 'D-pad up' },
  { value: '2', label: 'D-pad down' },
  { value: '4', label: 'D-pad left' },
  { value: '8', label: 'D-pad right' },
];

/** Number of global player slots. */
export const PLAYER_SLOTS = 8;

/** Languages the Steam emulator can report to games. */
export const STEAM_LANGUAGES: readonly string[] = [
  'Automatic', 'English', 'French', 'German', 'Spanish', 'Italian', 'Japanese', 'Brazilian', 'Russian', 'Schinese',
];

type FieldExtras = Partial<Omit<SettingField, 'key' | 'label' | 'type' | 'default'>>;

const field = (type: SettingField['type'], key: string, label: string, value: string, extras: FieldExtras = {}): SettingField => ({
  key,
  label,
  type,
  choices: [],
  default: value,
  requiresRestart: false,
  ...extras,
});

const toggle = (key: string, label: string, value: boolean, extras?: FieldExtras) => field('toggle', key, label, String(value), extras);
const hotkey = (key: string, label: string, value: string) => field('hotkey', key, label, value);
const combo = (key: string, label: string, value: string, extras?: FieldExtras) =>
  field('gamepadCombo', key, label, value, { comboButtons: value.split('+').length, ...extras });
const choice = (key: string, label: string, value: string, options: readonly SettingChoice[], extras?: FieldExtras) =>
  field('choice', key, label, value, { choices: [...options], ...extras });
const number = (key: string, label: string, value: number, range: Pick<SettingField, 'min' | 'max' | 'step' | 'unit'>, extras?: FieldExtras) =>
  field('number', key, label, String(value), { ...range, ...extras });

/** Condition that holds while a toggle is on. */
const whenOn = (key: string): SettingCondition => ({ key, values: ['true'] });

/** Every section and field, and the gamepad buttons combinations use. */
export const SETTINGS_SCHEMA: SettingsSchema = {
  gamepadButtons: [...GAMEPAD_BUTTONS],
  sections: [
    {
      id: 'general',
      title: 'General',
      description: 'Nicknames, language, network and profiles.',
      fields: [
        toggle('misc.useNicksInGame', 'Use nicknames in game', true, {
          description: "Players' nicknames replace the default names in games that support it.",
        }),
        choice(
          'misc.steamLang',
          'Steam language',
          'Automatic',
          STEAM_LANGUAGES.map((value) => ({ value, label: value === 'Schinese' ? 'Simplified Chinese' : value })),
        ),
        choice('misc.network', 'Network interface', 'Automatic', NETWORKS, { description: 'Interface the instances use to find each other.' }),
        toggle('misc.disableGameProfiles', 'Disable profiles', false, { description: 'Hides saved setups for every game.' }),
        toggle('misc.disableForcedNote', 'Skip handler notes on first launch', false),
        toggle('misc.autoDesktopScaling', 'Adjust Windows scaling automatically', false),
        toggle('misc.ignoreInputLockReminder', "Don't remind me to lock inputs", true, {
          description: 'When off, the lobby explains how to lock keyboard and mouse input.',
        }),
        toggle('misc.keepAccounts', 'Keep Nucleus user accounts', false, {
          description:
            'Keeps the Windows user accounts Nucleus creates for some games. When off, the next game that uses them deletes them with their files and saves.',
          confirm: {
            values: ['false'],
            title: 'Turn off Keep Nucleus user accounts?',
            message:
              'The next time you run a game that uses separate Windows user accounts, Nucleus deletes every account it created, with their files and saves.',
            confirmLabel: 'Turn off',
          },
        }),
        field('password', 'misc.nucleusAccountPassword', 'Password of the player accounts', '12345', {
          description: 'Some handlers create a Windows account per player. They all use this password.',
          maxLength: 32,
        }),
      ],
    },
    {
      id: 'hotkeys',
      title: 'Hotkeys',
      description: 'Keyboard shortcuts available during a session.',
      fields: [
        hotkey('hotkeys.close', 'Close Nucleus and every instance', 'Ctrl+Q'),
        hotkey('hotkeys.stop', 'Stop the session', 'Ctrl+E'),
        hotkey('hotkeys.topMost', 'Keep the game windows on top', 'Ctrl+T'),
        hotkey('hotkeys.setFocus', 'Bring the game windows to the front', 'Ctrl+H'),
        hotkey('hotkeys.resetWindows', 'Put the windows back in place', 'Ctrl+R'),
        hotkey('hotkeys.cutscenes', 'Cutscene mode', 'Ctrl+M'),
        hotkey('hotkeys.switch', 'Switch layout', 'Ctrl+N'),
        hotkey('hotkeys.shortcutsReminder', 'Show the shortcuts', 'Alt+R'),
        hotkey('hotkeys.switchMergerForeground', 'Next window in the merged window', 'Alt+A'),
        field('hotkey', 'hotkeys.lockKey', 'Lock or unlock the inputs', 'End', {
          description: 'Gives each keyboard and mouse to its game window.',
          requiresRestart: true,
        }),
      ],
    },
    {
      id: 'gamepad',
      title: 'Gamepad',
      description: 'Gamepad shortcuts during a session. The app itself works with any gamepad.',
      fields: [
        combo('xShortcuts.close', 'Close Nucleus and every instance', '1024+16'),
        combo('xShortcuts.stop', 'Stop the session', '1024+32'),
        combo('xShortcuts.topMost', 'Keep the game windows on top', '1024+16384'),
        combo('xShortcuts.setFocus', 'Bring the game windows to the front', '1024+32768'),
        combo('xShortcuts.resetWindows', 'Put the windows back in place', '1024+8'),
        combo('xShortcuts.cutscenes', 'Cutscene mode', '1024+8192'),
        combo('xShortcuts.switch', 'Switch layout', '1024+512'),
        combo('xShortcuts.lockInputs', 'Lock or unlock the inputs', '1024+128'),
        choice('gamepad.type', 'Gamepad type', 'Xbox', [
          { value: 'Xbox', label: 'Xbox' },
          { value: 'Playstation', label: 'PlayStation' },
        ]),
      ],
    },
    {
      id: 'audio',
      title: 'Audio',
      description: "Where each player's sound goes.",
      fields: [
        toggle('audio.custom', 'Per-player audio outputs', true),
        ...Array.from({ length: PLAYER_SLOTS }, (_, i) =>
          choice(`audio.instance${i + 1}`, `Player ${i + 1}`, 'Default', AUDIO_OUTPUTS, { enabledWhen: whenOn('audio.custom') }),
        ),
      ],
    },
    {
      id: 'layout',
      title: 'Layout',
      description: 'Custom layout, background and cutscenes.',
      fields: [
        number('customLayout.horizontalLines', 'Custom layout: horizontal lines', 1, { min: 0, max: 5, step: 1 }),
        number('customLayout.verticalLines', 'Custom layout: vertical lines', 1, { min: 0, max: 5, step: 1 }),
        number('customLayout.maxPlayers', 'Custom layout: players per screen', 4, { min: 1, max: 16, step: 1 }),
        toggle('customLayout.splitDiv', 'Draw a background behind the windows', false),
        toggle('customLayout.hideOnly', 'Hide the desktop only', false, { enabledWhen: whenOn('customLayout.splitDiv') }),
        choice('customLayout.splitDivColor', 'Background colour', 'Black', SPLIT_COLORS, {
          enabledWhen: { key: 'customLayout.hideOnly', values: ['false'] },
          visibleWhen: whenOn('customLayout.splitDiv'),
        }),
        toggle('customLayout.windowsMerger', 'Merge the game windows into one', false),
        choice('customLayout.windowsMergerRes', 'Merged window resolution', '1920X1080', MERGER_RESOLUTIONS, {
          visibleWhen: whenOn('customLayout.windowsMerger'),
        }),
        toggle('customLayout.losslessHook', 'Use the lossless hook with the merger', true, { visibleWhen: whenOn('customLayout.windowsMerger') }),
        toggle('customLayout.ctsKeepAspectRatio', 'Cutscenes: keep the aspect ratio', false),
        toggle('customLayout.ctsMuteAudioOnly', 'Cutscenes: only mute the audio', false),
        toggle('customLayout.ctsUnfocus', 'Cutscenes: unfocus the other windows', false),
      ],
    },
    {
      id: 'advanced',
      title: 'Advanced',
      description: 'Launch timing, logs, tools and folders.',
      fields: [
        number('misc.windowSetupDelay', 'Window setup delay', 1000, { min: 0, max: 10000, step: 100, unit: 'ms' }),
        number('misc.pauseBetweenInstances', 'Pause between instances', 0, { min: 0, max: 60, step: 1, unit: 's' }),
        toggle('misc.debugLog', 'Write a debug log', false, { requiresRestart: true }),
        toggle('misc.nucleusMultiInstances', 'Allow several Nucleus windows', false, { requiresRestart: true }),
        field('filePath', 'dev.textEditorPath', 'Text editor', 'C:\\Windows\\notepad.exe', { description: 'Opens handler files.' }),
        field('folderPath', 'misc.autoSearchLocation', 'Games folder', 'D:\\SteamLibrary\\steamapps\\common', {
          description: 'Where the executable picker opens.',
        }),
      ],
    },
  ],
};

/** Every field of the schema. */
export const SETTING_FIELDS: readonly SettingField[] = SETTINGS_SCHEMA.sections.flatMap((section) => section.fields);

/** Values at startup: the defaults, with a few changed by the user. */
export function createSettingsValues(): Record<string, string> {
  return {
    ...Object.fromEntries(SETTING_FIELDS.map((setting) => [setting.key, setting.default ?? ''])),
    'misc.network': 'Ethernet',
    'misc.ignoreInputLockReminder': 'false',
    'misc.keepAccounts': 'true',
    'audio.instance1': AUDIO_OUTPUTS[1]?.value ?? 'Default',
    'audio.instance2': AUDIO_OUTPUTS[2]?.value ?? 'Default',
  };
}

/** Global player nicknames and Steam IDs at startup. */
export function createPlayers(): PlayerIdentities {
  const known: readonly (readonly [string, string])[] = [
    ['Rémi', '76561198012345671'],
    ['Léa', '76561198023456782'],
    ['Max', ''],
    ['Sam', '76561198045678904'],
  ];
  return {
    players: Array.from({ length: PLAYER_SLOTS }, (_, i) => ({ number: i + 1, nickname: known[i]?.[0] ?? '', steamId: known[i]?.[1] ?? '' })),
    nicknameHistory: ['Rémi', 'Léa', 'Max', 'Sam', 'Jules', 'Nina', 'Tarnished'],
  };
}
