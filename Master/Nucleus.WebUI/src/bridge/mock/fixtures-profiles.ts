// Saved profiles at startup: three for Elden Ring, two for Minecraft.
import type { DeviceKind, PlayerIdentities, ScreenLayout } from '../generated/contract.ts';
import { defaultProfileSettings } from './profile-model.ts';
import type { MockProfile, ProfileSeat } from './state.ts';

const seat = (number: number, device: DeviceKind, screenIndex: number, slotIndex: number, nickname?: string): ProfileSeat => ({
  number,
  nickname,
  device,
  screenIndex,
  slotIndex,
  guests: 0,
});

/** Creates the saved profiles, by game id. Their settings start from the global settings. */
export function createProfiles(settings: Record<string, string>, players: PlayerIdentities): Record<string, MockProfile[]> {
  const profile = (
    about: { title: string; notes: string; autoPlay?: boolean },
    layouts: ScreenLayout[],
    seats: ProfileSeat[],
    choices: Pick<MockProfile, 'options' | 'stepItems'> = { options: {}, stepItems: {} },
  ): MockProfile => {
    const stored = defaultProfileSettings(settings, players, seats.length);
    stored.slots.forEach((slot, index) => {
      slot.nickname = seats[index]?.nickname ?? slot.nickname;
    });
    return { settings: { ...stored, ...about, autoPlay: about.autoPlay ?? false }, seats, layouts, ...choices };
  };

  return {
    EldenRing: [
      profile(
        { title: 'Friday night', notes: 'Four players on the desk monitor. The DualSense goes bottom left.' },
        ['fourPlayers', 'fullScreen'],
        [seat(1, 'xInput', 0, 0, 'Rémi'), seat(2, 'xInput', 0, 1, 'Léa'), seat(3, 'dInput', 0, 2, 'Max'), seat(4, 'rawKeyboard', 0, 3, 'Sam')],
        { options: { password: 'tarnished', language: 0, modLoader: 1 }, stepItems: {} },
      ),
      profile(
        { title: 'Campaign for two', notes: 'Side by side. Starts by itself once both gamepads are in.', autoPlay: true },
        ['dualVertical', 'fullScreen'],
        [seat(1, 'xInput', 0, 0, 'Rémi'), seat(2, 'xInput', 0, 1, 'Léa')],
        { options: { password: 'duo', language: 1, modLoader: 0 }, stepItems: {} },
      ),
      profile(
        { title: '', notes: 'Two on the desk monitor, one on the TV.' },
        ['dualHorizontal', 'fullScreen'],
        [seat(1, 'xInput', 0, 0), seat(2, 'dInput', 0, 1), seat(3, 'rawKeyboard', 1, 0)],
      ),
    ],
    Minecraft: [
      profile(
        { title: 'Survival server', notes: 'Three players on 1.21 with Fabric.' },
        ['fourPlayers', 'fullScreen'],
        [seat(1, 'xInput', 0, 0, 'Rémi'), seat(2, 'xInput', 0, 1, 'Léa'), seat(3, 'rawKeyboard', 0, 2, 'Sam')],
        { options: { memory: 2 }, stepItems: { 0: 0, 1: 1 } },
      ),
      profile({ title: 'Creative duo', notes: '' }, ['dualHorizontal', 'fullScreen'], [seat(1, 'xInput', 0, 0), seat(2, 'dInput', 0, 1)], {
        options: { memory: 1 },
        stepItems: { 0: 2 },
      }),
    ],
  };
}
