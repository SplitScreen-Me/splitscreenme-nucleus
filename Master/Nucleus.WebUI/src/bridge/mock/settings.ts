import type { BridgeHandlers, PlayerIdentities, SettingField, ValidationError } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { invalidParams } from './failures.ts';
import { GAMEPAD_BUTTONS, PLAYER_SLOTS, SETTINGS_SCHEMA, SETTING_FIELDS } from './fixtures-settings.ts';
import { publishSetup } from './setup-snapshot.ts';
import type { MockState } from './state.ts';
import { sleep } from './util.ts';
import { nicknameProblem, pathProblem, rangeProblem, steamIdProblem } from './validation.ts';

const TEXT_MAX = 100;
const HISTORY_MAX = 20;
/** Time the user takes to press the buttons, and what they press: A, or LB+RB. */
const CAPTURE_MS = 2000;
const CAPTURED_COMBOS: Readonly<Record<number, string>> = { 1: '4096', 2: '256+512' };
const HOTKEY = /^(?:(?:Ctrl|Alt|Shift|Win)\+)*(?:[A-Z0-9]|F\d{1,2}|End|Home|Insert|Delete|PageUp|PageDown|Pause|Space|Tab)$/;

/** A combination of `count` distinct button values joined by "+", for example `1024+16`. */
function comboProblem(value: string, count: number): string | undefined {
  const buttons = value.split('+');
  const unknown = buttons.find((button) => !GAMEPAD_BUTTONS.some((known) => known.value === button));
  if (unknown !== undefined) return `Unknown button '${unknown}'.`;
  if (buttons.length !== count) return count === 1 ? 'Use one button.' : `Use ${count} buttons, for example Guide+Start.`;
  return new Set(buttons).size === buttons.length ? undefined : 'Use each button once.';
}

function textProblem(field: SettingField, value: string): string | undefined {
  const max = field.maxLength ?? TEXT_MAX;
  if (value.length > max) return `Can be ${max} characters at most.`;
  return field.type === 'password' && value === '' ? 'Enter a password.' : undefined;
}

/** What is wrong with a value for a field, according to its type. */
function valueProblem(field: SettingField, value: unknown): string | undefined {
  if (typeof value !== 'string') return 'Values are sent as strings.';
  switch (field.type) {
    case 'toggle':
      return value === 'true' || value === 'false' ? undefined : 'Must be "true" or "false".';
    case 'number':
      if (value.trim() === '') return 'Enter a number.';
      return rangeProblem(Number(value), field.min ?? 0, field.max ?? Number.MAX_SAFE_INTEGER, field.step);
    case 'text':
    case 'password':
      return textProblem(field, value);
    case 'choice':
      return field.choices.some((choice) => choice.value === value) ? undefined : 'Pick a value from the list.';
    case 'hotkey':
      return HOTKEY.test(value) ? undefined : 'Use modifiers and a key, for example Ctrl+Alt+K.';
    case 'gamepadCombo':
      return comboProblem(value, field.comboButtons ?? 2);
    case 'filePath':
    case 'folderPath':
      return pathProblem(value);
  }
}

/** Invalid values (by key), including shortcuts already used by another field of the same type. */
function validateValues(current: Record<string, string>, values: Record<string, string>): ValidationError[] {
  const merged = { ...current, ...values };
  return Object.entries(values).flatMap(([key, value]): ValidationError[] => {
    const field = SETTING_FIELDS.find((candidate) => candidate.key === key);
    const problem = field ? valueProblem(field, value) : 'Unknown setting.';
    if (problem) return [{ field: key, message: problem }];
    const shortcut = field?.type === 'hotkey' || field?.type === 'gamepadCombo';
    const clash = shortcut && SETTING_FIELDS.find((other) => other.key !== key && other.type === field.type && merged[other.key] === value);
    return clash ? [{ field: key, message: `Already used by "${clash.label}".` }] : [];
  });
}

function validatePlayers({ players }: PlayerIdentities): ValidationError[] {
  const errors: ValidationError[] = [];
  if (players.length !== PLAYER_SLOTS) {
    errors.push({ field: 'players', message: `Expected ${PLAYER_SLOTS} player slots, got ${players.length}.` });
  }
  players.forEach((player, index) => {
    const problems: [string, string | undefined][] = [
      ['players.number', player.number === index + 1 ? undefined : 'Players must be numbered 1, 2, 3… in order.'],
      ['players.nickname', nicknameProblem(player.nickname)],
      ['players.steamId', steamIdProblem(player.steamId)],
    ];
    for (const [field, problem] of problems) {
      if (problem) errors.push({ field, index, message: `Player ${index + 1}: ${problem}` });
    }
  });
  return errors;
}

/**
 * Mock of the `settings` service: a schema modelled on Settings.ini (every field type), values validated per type
 * on save, the global player slots, and a gamepad combination "pressed" after two seconds.
 */
export function createSettingsMock(context: MockContext, state: MockState): BridgeHandlers['settings'] {
  return {
    getSchema: () => SETTINGS_SCHEMA,
    getValues: () => state.settings,
    setValues: ({ values }) => {
      const errors = validateValues(state.settings, values);
      if (errors.length > 0) return errors;
      const keys = Object.keys(values).filter((key) => state.settings[key] !== values[key]);
      Object.assign(state.settings, values);
      if (keys.length > 0) context.emit('settings.changed', { keys });
      const shownInSetup = keys.some((key) => key.startsWith('audio.') || key.startsWith('hotkeys.'));
      if (state.setup && shownInSetup) publishSetup(context, state, state.setup);
      return [];
    },
    getPlayers: () => state.players,
    setPlayers: (identities) => {
      const errors = validatePlayers(identities);
      if (errors.length > 0) return errors;
      const { nicknameHistory } = state.players;
      const players = identities.players.map((player) => ({
        number: player.number,
        nickname: player.nickname.trim(),
        steamId: player.steamId.trim(),
      }));
      const newNicknames = players.map((player) => player.nickname).filter((nickname) => nickname && !nicknameHistory.includes(nickname));
      state.players = { players, nicknameHistory: [...newNicknames, ...nicknameHistory].slice(0, HISTORY_MAX) };
      context.emit('settings.changed', { keys: ['players'] });
      if (state.setup) publishSetup(context, state, state.setup);
      return [];
    },
    captureGamepadCombo: async ({ buttons }) => {
      const combo = CAPTURED_COMBOS[buttons] ?? invalidParams('A combination has 1 or 2 buttons.');
      await sleep(CAPTURE_MS);
      return combo;
    },
  };
}
