// Lobby commands: every setup call the screen makes, with refusals toasted by useRun. Results come back as
// `setup.changed` snapshots, so callers never apply them locally.
import { useMemo, useState } from 'react';
import { bridge, type ScreenLayout, type SetupPlayer, type SetupSnapshot } from '@/bridge';
import { refusalReason, useRun } from '@/lib/bridge-ui';
import { slotOfPlayer } from './model';
import { forgetRemoved, markRemoved } from './removals';

export function useLobbyCommands(onNeedsPicker: () => Promise<void>) {
  const run = useRun();
  const [busyChecks, setBusyChecks] = useState<ReadonlySet<string>>(new Set());

  const commands = useMemo(() => {
    const setup = bridge.setup;

    /** Puts the other devices of a moved player back with its first device (the host moves one device of a pair). */
    const regroup = async (snap: SetupSnapshot | undefined, player: SetupPlayer) => {
      const [first, ...rest] = player.deviceIds;
      for (const id of rest) {
        const owner = snap?.players.find((p) => p.deviceIds.includes(first ?? ''));
        if (!snap || !owner || owner.deviceIds.includes(id)) continue;
        const screen = snap.screens.find((s) => s.index === owner.screenIndex);
        const slot = screen && slotOfPlayer(screen, owner.id);
        if (slot === undefined) continue;
        snap = await run(() => setup.assignToSlot({ deviceId: id, screenIndex: owner.screenIndex, slotIndex: slot }));
      }
    };

    return {
      join: (deviceId: string) => {
        forgetRemoved(deviceId);
        return run(() => setup.join({ deviceId }));
      },
      assignToSlot: (deviceId: string, screenIndex: number, slotIndex: number) => {
        forgetRemoved(deviceId);
        return run(() => setup.assignToSlot({ deviceId, screenIndex, slotIndex }));
      },
      assignAt: (deviceId: string, screenIndex: number, x: number, y: number) => {
        forgetRemoved(deviceId);
        return run(() => setup.assignAt({ deviceId, screenIndex, x, y }));
      },
      movePlayer: async (player: SetupPlayer, screenIndex: number, slotIndex: number) => {
        const first = player.deviceIds[0];
        if (!first) return;
        const snap = await run(() => setup.assignToSlot({ deviceId: first, screenIndex, slotIndex }));
        if (snap) await regroup(snap, player);
      },
      movePlayerAt: async (player: SetupPlayer, screenIndex: number, x: number, y: number) => {
        const first = player.deviceIds[0];
        if (!first) return;
        const snap = await run(() => setup.assignAt({ deviceId: first, screenIndex, x, y }));
        if (snap) await regroup(snap, player);
      },
      removePlayer: async (player: SetupPlayer) => {
        markRemoved([...player.deviceIds, ...player.guestDeviceIds]);
        for (const deviceId of [...player.deviceIds].reverse()) {
          const done = await run(() => setup.unassign({ deviceId }));
          if (!done) return false;
        }
        return true;
      },
      unassign: (deviceId: string) => {
        markRemoved([deviceId]);
        return run(() => setup.unassign({ deviceId }));
      },
      setLayout: (screenIndex: number, layout: ScreenLayout) => run(() => setup.setLayout({ screenIndex, layout })),
      setIdentity: (playerId: string, change: { nickname?: string; steamId?: string }) =>
        run(() => setup.setPlayerIdentity({ playerId, ...change })),
      setAudio: (playerId: string, audioDeviceId: string) => run(() => setup.setPlayerAudio({ playerId, audioDeviceId })),
      addGuest: (playerId: string, deviceId: string) => run(() => setup.addGuest({ playerId, deviceId })),
      removeGuest: (deviceId: string) => {
        markRemoved([deviceId]);
        return run(() => setup.removeGuest({ deviceId }));
      },
      toggleExpand: (playerId: string) => run(() => setup.toggleExpand({ playerId })),
      rumble: (deviceId: string) => run(() => setup.rumble({ deviceId })),
      refreshDevices: () => run(() => setup.refreshDevices(), { success: 'Devices scanned again' }),
      autoFill: () => run(() => setup.autoFill()),
      clearAll: (deviceIds: readonly string[] = []) => {
        markRemoved(deviceIds);
        return run(() => setup.clearAll());
      },
      setOptionChoice: (key: string, index: number) => run(() => setup.setOptionChoice({ key, index })),
      setOptionText: (key: string, text: string) => run(() => setup.setOptionText({ key, text })),
      selectStepItem: (stepIndex: number, itemIndex: number) => run(() => setup.selectStepItem({ stepIndex, itemIndex })),
      next: () => run(() => setup.next()),
      back: () => run(() => setup.back()),
      setSaveProfileOnLaunch: (enabled: boolean) => run(() => setup.setSaveProfileOnLaunch({ enabled })),
      setUseApiIndex: (enabled: boolean) => run(() => setup.setUseApiIndex({ enabled })),
      loadProfile: (profileId: number) => run(() => bridge.profiles.load({ profileId })),
      unloadProfile: () => run(() => bridge.profiles.unload()),
      play: () => run(() => bridge.session.play()),
      runCheck: async (checkId: string) => {
        setBusyChecks((s) => new Set(s).add(checkId));
        let picker = false;
        try {
          return await run(() => setup.runCheckAction({ checkId }), {
            onError: (error) => {
              picker = refusalReason(error) === 'needsPicker';
              return picker;
            },
          });
        } finally {
          setBusyChecks((s) => {
            const next = new Set(s);
            next.delete(checkId);
            return next;
          });
          if (picker) await onNeedsPicker();
        }
      },
    };
  }, [run, onNeedsPicker]);

  return { ...commands, busyChecks };
}

export type LobbyCommands = ReturnType<typeof useLobbyCommands>;
