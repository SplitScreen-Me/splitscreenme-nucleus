// Lobby view-model helpers: pure derivations from the host's setup snapshot. Commands go through bridge.setup.*.
import type { DeviceKind, Rect, ScreenLayout, SetupDevice, SetupHint, SetupPlayer, SetupScreen, SetupSnapshot } from '@/bridge';
import { deviceNoun, isGamepadKind, isKeyboardKind } from '@/lib/devices';
import type { IconName } from '@/ui';

// ---------------------------------------------------------------- lookups

export const deviceById = (snap: SetupSnapshot, id: string) => snap.devices.find((d) => d.id === id);

export const playerById = (snap: SetupSnapshot, id: string | undefined) => (id ? snap.players.find((p) => p.id === id) : undefined);

/** Player a device drives, or whose instance it joins as a guest. */
export const playerOfDevice = (snap: SetupSnapshot, device: SetupDevice) => playerById(snap, device.playerId ?? device.guestOfPlayerId);

/** Devices of a player (not its guests), in order. */
export const devicesOf = (snap: SetupSnapshot, player: SetupPlayer) =>
  player.deviceIds.map((id) => deviceById(snap, id)).filter((d): d is SetupDevice => Boolean(d));

export const guestsOf = (snap: SetupSnapshot, player: SetupPlayer) =>
  player.guestDeviceIds.map((id) => deviceById(snap, id)).filter((d): d is SetupDevice => Boolean(d));

/** A device with no player (neither its own nor as a guest). */
export const isFree = (d: SetupDevice) => !d.playerId && !d.guestOfPlayerId;

/** Devices waiting in the tray: free and revealed, or waiting for a button press to be identified. */
export function trayDevices(snap: SetupSnapshot): SetupDevice[] {
  return snap.devices.filter((d) => isFree(d) && (d.revealed || d.needsButtonPress));
}

/** Free keyboards and mice that are not shown yet (they appear once used). */
export const hiddenInputs = (snap: SetupSnapshot) =>
  snap.devices.filter((d) => isFree(d) && !d.revealed && !d.needsButtonPress && !isGamepadKind(d.kind));

/** Slot of a screen where a player sits (its first slot when it spans several). */
export function slotOfPlayer(screen: SetupScreen, playerId: string): number | undefined {
  return screen.slots.find((s) => s.playerId === playerId)?.index;
}

/** Players sorted by number. */
export const byNumber = (players: readonly SetupPlayer[]) => [...players].sort((a, b) => a.number - b.number);

/** Seats taken: players plus their guests. */
export const seatCount = (snap: SetupSnapshot) => snap.players.reduce((n, p) => n + 1 + p.guestDeviceIds.length, 0);

/** A player whose only device is a raw keyboard (a raw mouse can complete it). */
export function hasLoneRawKeyboard(snap: SetupSnapshot): boolean {
  return snap.players.some((p) => p.deviceIds.length === 1 && deviceById(snap, p.deviceIds[0] ?? '')?.kind === 'rawKeyboard');
}

// ---------------------------------------------------------------- names

export function screenName(screen: SetupScreen): string {
  return `Display ${screen.displayIndex}`;
}

export function playerName(p: SetupPlayer): string {
  return p.nickname?.trim() || `Player ${p.number}`;
}

/** "Xbox Wireless Controller + Logitech G502" */
export function devicesLabel(devices: readonly SetupDevice[]): string {
  return devices.map((d) => d.name).join(' + ') || 'No device';
}

export const deviceKinds = (devices: readonly SetupDevice[]): DeviceKind[] => devices.map((d) => d.kind);

/** Where a player sits: "Display 1, top left". */
export function whereOf(snap: SetupSnapshot, p: SetupPlayer): string {
  const screen = snap.screens.find((s) => s.index === p.screenIndex);
  if (!screen) return 'a screen';
  const slot = slotOfPlayer(screen, p.id);
  return slot === undefined ? screenName(screen) : `${screenName(screen)}, ${slotName(screen, slot)}`;
}

/** Name of a slot in its grid: "top left", "left", "row 2, column 3". */
export function slotName(screen: SetupScreen, index: number): string {
  const { rows, columns } = screen;
  if (rows * columns <= 1) return 'full screen';
  const row = Math.floor(index / columns);
  const col = index % columns;
  if (rows === 1 && columns === 2) return col === 0 ? 'left' : 'right';
  if (rows === 2 && columns === 1) return row === 0 ? 'top' : 'bottom';
  if (rows === 2 && columns === 2) return `${row === 0 ? 'top' : 'bottom'} ${col === 0 ? 'left' : 'right'}`;
  return `row ${row + 1}, column ${col + 1}`;
}

// ---------------------------------------------------------------- layouts

export const PRESETS: ReadonlyArray<{ layout: ScreenLayout; icon: IconName; label: string }> = [
  { layout: 'fullScreen', icon: 'layout-1', label: 'One player, full screen' },
  { layout: 'dualHorizontal', icon: 'layout-2h', label: 'Two players, top and bottom' },
  { layout: 'dualVertical', icon: 'layout-2v', label: 'Two players, side by side' },
  { layout: 'fourPlayers', icon: 'layout-4', label: 'Four players, 2 × 2' },
];

export const MORE_LAYOUTS: ReadonlyArray<{ layout: ScreenLayout; label: string; short: string; description: string }> = [
  { layout: 'sixPlayers', label: 'Six players', short: '6 players', description: '2 rows, 3 columns' },
  { layout: 'eightPlayers', label: 'Eight players', short: '8 players', description: '2 rows, 4 columns' },
  { layout: 'sixteenPlayers', label: 'Sixteen players', short: '16 players', description: '4 rows, 4 columns' },
  { layout: 'custom', label: 'Custom grid', short: 'Custom', description: 'Rows and columns from Settings, Layout' },
  { layout: 'manual', label: 'Manual', short: 'Manual', description: 'Drop players anywhere on the screen' },
];

export function layoutLabel(layout: ScreenLayout): string {
  return (PRESETS.find((p) => p.layout === layout)?.label ?? MORE_LAYOUTS.find((l) => l.layout === layout)?.label ?? layout).toLowerCase();
}

// ---------------------------------------------------------------- geometry

/** A rectangle as fractions of another (CSS percentages). */
export function relative(rect: Rect, frame: Rect) {
  return {
    left: (rect.x - frame.x) / frame.width,
    top: (rect.y - frame.y) / frame.height,
    width: rect.width / frame.width,
    height: rect.height / frame.height,
  };
}

export interface Box {
  left: number;
  top: number;
  width: number;
  height: number;
}

/** Gap kept between neighbouring monitors in the drawing. */
const MONITOR_GAP = 14;

/**
 * Monitors drawn to scale inside the canvas: the union of the screens fitted into `width` × `height` (minus the caption
 * band), centred. Captions go under the drawing at each monitor's x, or in a row when monitors overlap horizontally.
 */
export function fitRig(
  screens: readonly SetupScreen[],
  total: Rect,
  width: number,
  height: number,
  captionHeight: number,
): { boxes: Map<number, Box>; captionTop: number; captionsInRow: boolean } | null {
  const W = width - 8;
  const H = height - captionHeight - 8;
  if (W <= 0 || H <= 0 || total.width <= 0 || total.height <= 0) return null;
  const s = Math.min(W / total.width, H / total.height);
  const ox = 4 + (W - total.width * s) / 2;
  const oy = 4 + Math.max(0, (H - total.height * s) / 2);
  const boxes = new Map<number, Box>();
  const half = screens.length > 1 ? MONITOR_GAP / 2 : 0;
  for (const screen of screens) {
    const b = screen.bounds;
    boxes.set(screen.index, {
      left: ox + (b.x - total.x) * s + half,
      top: oy + (b.y - total.y) * s + half,
      width: Math.max(0, b.width * s - 2 * half),
      height: Math.max(0, b.height * s - 2 * half),
    });
  }
  const spans = [...boxes.values()].sort((a, b) => a.left - b.left);
  const captionsInRow = spans.some((b, i) => i > 0 && b.left < (spans[i - 1]?.left ?? 0) + (spans[i - 1]?.width ?? 0) - 1);
  return { boxes, captionTop: oy + total.height * s + 14, captionsInRow };
}

/** Every copy renders its own view of the game: offset the art so regions don't look cloned. */
export const ART_VIEW: ReadonlyArray<readonly [string, string]> = [
  ['cover', '18% 35%'],
  ['auto 150%', '88% 30%'],
  ['auto 190%', '38% 78%'],
  ['auto 170%', '62% 22%'],
  ['auto 160%', '24% 70%'],
  ['auto 200%', '76% 64%'],
  ['auto 180%', '50% 18%'],
  ['auto 150%', '10% 52%'],
];

// ---------------------------------------------------------------- hints

/** The host's hint, in the lobby's words when the code is known. */
export function hintText(hint: SetupHint | undefined): string | undefined {
  if (!hint) return undefined;
  switch (hint.code) {
    case 'pressButtonToIdentify':
      return 'Press a button on each gamepad so Nucleus can tell them apart.';
    case 'dragPlayersToScreens':
      return 'Press Start on a controller to join, or drag a device onto a screen.';
    case 'profileWaitingForPlayer': {
      const [n, kind] = hint.args;
      return n && kind ? `Player ${n} of the profile: press a button on a ${deviceNoun(kind as DeviceKind)}.` : hint.text;
    }
    case 'profileReady':
      return 'Every player of the profile is here. Launch when you are ready.';
    case 'readyToPlay':
      return 'Everyone is in. Launch when you are ready.';
    default:
      return hint.text;
  }
}

// ---------------------------------------------------------------- keyboard presses

/** Keys that operate the UI itself; they never count as "the keyboard pressed a button to join". */
const UI_KEYS = new Set([
  'Enter',
  ' ',
  'ArrowUp',
  'ArrowDown',
  'ArrowLeft',
  'ArrowRight',
  'Delete',
  'Backspace',
  'Home',
  'End',
  'PageUp',
  'PageDown',
  'Tab',
  'Escape',
]);
const MODIFIERS = new Set(['Shift', 'Control', 'Alt', 'Meta', 'CapsLock', 'AltGraph', 'OS']);

/** Whether a key press, judged where focus is when it goes down, operates the UI rather than joining. */
export function keyboardPressIsForUi(key: string): boolean {
  if (document.querySelector('dialog[open], [role="menu"]')) return true;
  const a = document.activeElement as HTMLElement | null;
  if (a && (a.isContentEditable || a.matches('input, textarea, select'))) return true;
  if (MODIFIERS.has(key) || key === 'Tab' || key === 'Escape') return true;
  const onControl = Boolean(a && a !== document.body && a.id !== 'stage' && a.matches('button, a, [tabindex], summary, [role="button"]'));
  return onControl && UI_KEYS.has(key);
}

/** Device kinds typed on: their activity is judged by the last DOM key press. */
export const typedOn = (kind: DeviceKind) => isKeyboardKind(kind);

export const reduceMotion = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
