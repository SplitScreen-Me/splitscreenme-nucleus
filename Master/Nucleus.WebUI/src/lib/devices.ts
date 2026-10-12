// Input device kinds as the UI names and draws them.
import type { DeviceKind } from '@/bridge';
import type { IconName } from '@/ui';

/** Whether a device kind is a gamepad (XInput, DirectInput or SDL). */
export const isGamepadKind = (kind: DeviceKind) => kind === 'xInput' || kind === 'dInput' || kind === 'sdl';

/** Whether a device kind is typed on (the shared keyboard, a raw keyboard, a keyboard and mouse pair). */
export const isKeyboardKind = (kind: DeviceKind) => kind === 'keyboard' || kind === 'rawKeyboard' || kind === 'keyboardMouse';

const ICONS: Record<DeviceKind, IconName> = {
  xInput: 'pad',
  dInput: 'pad',
  sdl: 'pad',
  keyboard: 'kbm',
  rawKeyboard: 'keyboard',
  rawMouse: 'mouse',
  keyboardMouse: 'kbm',
};

/** Icon of a device kind. */
export const deviceIcon = (kind: DeviceKind | undefined): IconName => (kind ? ICONS[kind] : 'pad');

const NOUNS: Record<DeviceKind, string> = {
  xInput: 'gamepad',
  dInput: 'gamepad',
  sdl: 'gamepad',
  keyboard: 'keyboard and mouse',
  rawKeyboard: 'keyboard',
  rawMouse: 'mouse',
  keyboardMouse: 'keyboard and mouse',
};

/** "gamepad", "keyboard", "mouse", "keyboard and mouse". */
export const deviceNoun = (kind: DeviceKind) => NOUNS[kind];

/** Icon for a set of device kinds driving one player (a raw keyboard plus a raw mouse reads as one pair). */
export function devicesIcon(kinds: readonly DeviceKind[]): IconName {
  if (kinds.some(isGamepadKind)) return 'pad';
  const keyboard = kinds.includes('rawKeyboard');
  const mouse = kinds.includes('rawMouse');
  if ((keyboard && mouse) || kinds.includes('keyboard') || kinds.includes('keyboardMouse')) return 'kbm';
  return keyboard ? 'keyboard' : mouse ? 'mouse' : 'pad';
}
