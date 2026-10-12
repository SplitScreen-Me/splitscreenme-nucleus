// Press to join: a free device that is used (or becomes revealed by being used) takes the next free slot.
// The host owns placement (setup.join); this hook only decides which activity means "I want to play".
import { useEffect, useRef } from 'react';
import { bridge, useBridgeEvent, type SetupDevice, type SetupSnapshot } from '@/bridge';
import { errorMessage, isCancelled, refusalReason } from '@/lib/bridge-ui';
import { isGamepadKind } from '@/lib/devices';
import { useToast } from '@/ui';
import { deviceById, hasLoneRawKeyboard, isFree, keyboardPressIsForUi, playerName, typedOn } from './model';
import { recentlyRemoved } from './removals';

/** A DOM key press this recent explains a keyboard's activity. */
const KEY_WINDOW_MS = 600;
/** A device revealed this soon after its activity was revealed by that press. */
const REVEAL_ACTIVITY_MS = 2500;

interface Options {
  snap: SetupSnapshot | null;
  /** False while joining makes no sense (launching, a dialog the pads should not answer). */
  enabled: boolean;
  /** A free device was used: flash it in the tray. */
  onFlash: (deviceId: string) => void;
  /** A device of a player was used: ping its region. */
  onPing: (playerId: string) => void;
  announce: (message: string) => void;
}

export function usePressToJoin({ snap, enabled, onFlash, onPing, announce }: Options) {
  const toast = useToast();
  const pending = useRef(new Set<string>());
  // Device id → state of the setup when the host refused it: not asked again until something changes.
  const refused = useRef(new Map<string, string>());
  const lastKey = useRef<{ forUi: boolean; at: number } | null>(null);
  // Device id → last time it reported activity.
  const lastActivity = useRef(new Map<string, number>());
  const live = useRef({ snap, enabled });
  live.current = { snap, enabled };

  // Judge a keyboard's activity by where focus was when the key went down: the host's event arrives later.
  useEffect(() => {
    const onKey = (e: KeyboardEvent) => {
      // Keys the gamepad navigation sends to widgets are not the keyboard.
      if (!e.isTrusted) return;
      lastKey.current = { forUi: keyboardPressIsForUi(e.key), at: performance.now() };
    };
    window.addEventListener('keydown', onKey, true);
    return () => window.removeEventListener('keydown', onKey, true);
  }, []);

  const signature = (s: SetupSnapshot) =>
    `${s.players.map((p) => `${p.id}:${p.deviceIds.length}`).join(',')}|${s.screens.map((x) => x.layout).join(',')}|${s.profile.loaded}`;

  /** Whether this activity of a free device means it wants to join. */
  const wantsToJoin = (s: SetupSnapshot, device: SetupDevice): boolean => {
    if (!device.revealed || !isFree(device) || recentlyRemoved(device.id)) return false;
    if (document.querySelector('dialog[open]')) return false;
    if (isGamepadKind(device.kind)) return true;
    if (device.kind === 'rawMouse') return hasLoneRawKeyboard(s);
    if (typedOn(device.kind)) {
      const key = lastKey.current;
      const recent = key !== null && performance.now() - key.at < KEY_WINDOW_MS;
      if (recent && key.forUi) return false;
      // The shared keyboard also reports the mouse: only a real key press counts.
      if (device.kind === 'keyboard' && !recent) return false;
      return true;
    }
    return false;
  };

  const tryJoin = async (deviceId: string) => {
    const { snap: s, enabled: on } = live.current;
    if (!on || !s || pending.current.has(deviceId)) return;
    const device = deviceById(s, deviceId);
    if (!device || !wantsToJoin(s, device)) return;
    const state = signature(s);
    if (refused.current.get(deviceId) === state) return;
    pending.current.add(deviceId);
    try {
      const next = await bridge.setup.join({ deviceId });
      refused.current.delete(deviceId);
      const player = next.players.find((p) => p.deviceIds.includes(deviceId) || p.guestDeviceIds.includes(deviceId));
      if (player) announce(`${device.name} joined: ${playerName(player)}, player ${player.number}.`);
    } catch (error) {
      if (isCancelled(error)) return;
      refused.current.set(deviceId, state);
      const reason = refusalReason(error);
      // A profile that is not waiting for this kind of device is not worth a toast on every press.
      if (reason !== 'profileLocked' || !s.profile.loaded) toast(errorMessage(error), { icon: 'info', durationMs: 4200 });
    } finally {
      pending.current.delete(deviceId);
    }
  };

  useBridgeEvent('setup.deviceActivity', ({ deviceId, active }) => {
    const s = live.current.snap;
    if (active) lastActivity.current.set(deviceId, performance.now());
    if (!active || !s) return;
    const device = deviceById(s, deviceId);
    if (!device) return;
    const owner = device.playerId ?? device.guestOfPlayerId;
    if (owner) {
      onPing(owner);
      return;
    }
    onFlash(deviceId);
    void tryJoin(deviceId);
  });

  // A device revealed by being used (a key typed, a gamepad identified) is a press too. Only when it was used just
  // now: a device revealed while the lobby was closed shows up in the first fresh snapshot, and must not join by itself.
  const revealed = useRef<{ gameId: string; ids: Set<string> } | null>(null);
  useEffect(() => {
    if (!snap) return;
    const now = new Set(snap.devices.filter((d) => d.revealed).map((d) => d.id));
    const before = revealed.current?.gameId === snap.gameId ? revealed.current.ids : null;
    revealed.current = { gameId: snap.gameId, ids: now };
    if (!before) return;
    const at = performance.now();
    for (const id of now) {
      if (!before.has(id) && at - (lastActivity.current.get(id) ?? -Infinity) < REVEAL_ACTIVITY_MS) void tryJoin(id);
    }
  });
}
