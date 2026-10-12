// Profiles: defaults taken from the global settings, the listed summary, the editable settings and their validation.
import type {
  PlayerIdentities,
  ProfileSettings,
  ProfileSlot,
  ProfileSummary,
  SettingChoice,
  ValidationError,
} from '../generated/contract.ts';
import { AUDIO_OUTPUTS, MERGER_RESOLUTIONS, NETWORKS, SPLIT_COLORS, isGamepad } from './fixtures-setup.ts';
import type { MockProfile, MockSetup, MockState, StoredProfileSettings } from './state.ts';
import { nicknameProblem, rangeProblem, steamIdProblem } from './validation.ts';

const TITLE_MAX = 32;
const NOTES_MAX = 500;
const PROCESSOR_COUNT = 16;
/** The process priorities the host accepts (ProfileSettingsModel.PriorityClasses). */
const PRIORITY_CLASSES = ['Normal', 'AboveNormal', 'High', 'RealTime'];

/** Per-player settings of a new profile: the global nickname, Steam ID and audio output of that player number. */
function defaultSlot(settings: Record<string, string>, players: PlayerIdentities, index: number): ProfileSlot {
  const identity = players.players[index];
  return {
    nickname: identity?.nickname ?? '',
    steamId: identity?.steamId ?? '',
    steamIdLocked: false,
    idealProcessor: '*',
    affinity: '',
    priorityClass: 'Normal',
    audioDevice: settings[`audio.instance${index + 1}`] ?? 'Default',
  };
}

/** Settings of a new profile for `slotCount` players, taken from the global settings. */
export function defaultProfileSettings(
  settings: Record<string, string>,
  players: PlayerIdentities,
  slotCount: number,
): StoredProfileSettings {
  const flag = (key: string) => settings[key] === 'true';
  const count = (key: string) => Number(settings[key] ?? 0);
  return {
    title: '',
    notes: '',
    autoPlay: false,
    useNicknames: flag('misc.useNicksInGame'),
    autoDesktopScaling: flag('misc.autoDesktopScaling'),
    splitDivider: flag('customLayout.splitDiv'),
    splitDividerHideDesktopOnly: flag('customLayout.hideOnly'),
    splitDividerColor: settings['customLayout.splitDivColor'] ?? 'Black',
    customLayoutHorizontalLines: count('customLayout.horizontalLines'),
    customLayoutVerticalLines: count('customLayout.verticalLines'),
    customLayoutMaxPlayers: count('customLayout.maxPlayers'),
    windowSetupDelayMs: count('misc.windowSetupDelay'),
    pauseBetweenInstancesSec: count('misc.pauseBetweenInstances'),
    network: settings['misc.network'] ?? 'Automatic',
    customAudio: flag('audio.custom'),
    cutscenesKeepAspectRatio: flag('customLayout.ctsKeepAspectRatio'),
    cutscenesMuteAudioOnly: flag('customLayout.ctsMuteAudioOnly'),
    cutscenesUnfocus: flag('customLayout.ctsUnfocus'),
    stopUiNavigation: false,
    windowsMerger: flag('customLayout.windowsMerger'),
    losslessHook: flag('customLayout.losslessHook'),
    mergerResolution: settings['customLayout.windowsMergerRes'] ?? '',
    slots: Array.from({ length: slotCount }, (_, index) => defaultSlot(settings, players, index)),
  };
}

/** Settings of the profile saved at launch: those edited so far (else the defaults), one slot per placed player. */
export function launchProfileSettings(state: MockState, setup: MockSetup): StoredProfileSettings {
  const settings = structuredClone(setup.pendingSettings ?? defaultProfileSettings(state.settings, state.players, 0));
  const slots = setup.players.map((_, index) => settings.slots[index] ?? defaultSlot(state.settings, state.players, index));
  return { ...settings, slots };
}

/** Whether profiles are enabled for a game: globally (settings) and in the game's menu. */
export function profilesEnabled(state: MockState, gameId: string): boolean {
  const game = state.games.find((candidate) => candidate.summary.id === gameId);
  return state.settings['misc.disableGameProfiles'] !== 'true' && !game?.menu.disableProfiles;
}

/** Stored settings as the UI edits them, with the read-only choice lists. */
export function editableSettings(stored: StoredProfileSettings, isNew: boolean): ProfileSettings {
  return {
    ...structuredClone(stored),
    isNew,
    networkChoices: [...NETWORKS],
    audioChoices: [...AUDIO_OUTPUTS],
    mergerResolutionChoices: [...MERGER_RESOLUTIONS],
    splitDividerColorChoices: [...SPLIT_COLORS],
  };
}

/** The settings to store from edited ones (drops `isNew` and the choice lists). */
export function storedSettings(edited: ProfileSettings): StoredProfileSettings {
  const {
    isNew: _isNew,
    networkChoices: _networks,
    audioChoices: _audio,
    mergerResolutionChoices: _resolutions,
    splitDividerColorChoices: _colors,
    ...stored
  } = edited;
  return structuredClone(stored);
}

/** A profile as listed. */
export function profileSummary(profile: MockProfile, index: number): ProfileSummary {
  const { seats, settings } = profile;
  return {
    id: index + 1,
    title: settings.title,
    notes: settings.notes,
    players: seats.length,
    gamepads: seats.filter((seat) => isGamepad(seat.device)).length,
    keyboardMice: seats.filter((seat) => !isGamepad(seat.device)).length,
    autoPlay: settings.autoPlay,
    playerList: seats.map(({ slotIndex: _slotIndex, ...player }) => player),
  };
}

const isChoice = (choices: readonly SettingChoice[], value: string) => choices.some((choice) => choice.value === value);

/** Invalid fields of edited profile settings, for a setup with `slotCount` players. Empty when they can be saved. */
export function validateProfileSettings(settings: ProfileSettings, slotCount: number): ValidationError[] {
  const errors: ValidationError[] = [];
  const check = (field: keyof ProfileSettings, problem: string | undefined) => {
    if (problem) errors.push({ field, message: problem });
  };
  check('title', settings.title.length > TITLE_MAX ? `The title can be ${TITLE_MAX} characters at most.` : undefined);
  check('notes', settings.notes.length > NOTES_MAX ? `Notes can be ${NOTES_MAX} characters at most.` : undefined);
  check('customLayoutHorizontalLines', rangeProblem(settings.customLayoutHorizontalLines, 0, 5, 1));
  check('customLayoutVerticalLines', rangeProblem(settings.customLayoutVerticalLines, 0, 5, 1));
  check('customLayoutMaxPlayers', rangeProblem(settings.customLayoutMaxPlayers, 1, 16, 1));
  check('windowSetupDelayMs', rangeProblem(settings.windowSetupDelayMs, 0, 10_000, 1));
  check('pauseBetweenInstancesSec', rangeProblem(settings.pauseBetweenInstancesSec, 0, 60, 1));
  check('network', isChoice(NETWORKS, settings.network) ? undefined : 'Pick a network interface from the list.');
  check('splitDividerColor', isChoice(SPLIT_COLORS, settings.splitDividerColor) ? undefined : 'Unknown colour.');
  const resolutionMissing = settings.windowsMerger && !isChoice(MERGER_RESOLUTIONS, settings.mergerResolution);
  check('mergerResolution', resolutionMissing ? 'Pick a resolution for the merged window.' : undefined);
  if (settings.slots.length !== slotCount) {
    errors.push({ field: 'slots', message: `Expected the settings of ${slotCount} players, got ${settings.slots.length}.` });
  }
  settings.slots.forEach((slot, index) => errors.push(...slotErrors(slot, index)));
  return errors;
}

function slotErrors(slot: ProfileSlot, index: number): ValidationError[] {
  const processor = slot.idealProcessor === '*' ? undefined : rangeProblem(Number(slot.idealProcessor), 0, PROCESSOR_COUNT - 1, 1);
  const affinity = slot.affinity.split(',').filter(Boolean);
  const problems: [string, string | undefined][] = [
    ['nickname', nicknameProblem(slot.nickname)],
    ['steamId', slot.steamIdLocked ? undefined : steamIdProblem(slot.steamId)],
    ['idealProcessor', processor && `Use * or a processor number from 0 to ${PROCESSOR_COUNT - 1}.`],
    [
      'affinity',
      affinity.every((core) => /^\d+$/.test(core) && Number(core) < PROCESSOR_COUNT)
        ? undefined
        : `List processor numbers from 0 to ${PROCESSOR_COUNT - 1}, separated by commas.`,
    ],
    ['priorityClass', PRIORITY_CLASSES.includes(slot.priorityClass) ? undefined : `Use one of ${PRIORITY_CLASSES.join(', ')}.`],
    ['audioDevice', isChoice(AUDIO_OUTPUTS, slot.audioDevice) ? undefined : 'Pick an audio output from the list.'],
  ];
  return problems.flatMap(([field, message]) =>
    message ? [{ field: `slots.${field}`, index, message }] : [],
  );
}
