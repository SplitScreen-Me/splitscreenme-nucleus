// Mutable state of the mock host, shared by the services that interact (library and handlers, setup,
// profiles and session, settings). Each transport gets its own: tests create several.
import type {
  AppNotice,
  DeviceKind,
  GameSummary,
  LibraryPreferences,
  PlayerIdentities,
  ProfilePlayer,
  ProfileSettings,
  Rect,
  ScreenLayout,
  SessionInstance,
  SessionLogEntry,
  SessionState,
  SessionSummary,
  SessionSummaryPlayer,
  SetupCapabilities,
  SetupCustomStep,
  SetupOption,
} from '../generated/contract.ts';
import { createInstalledHandlers, createLibrary, type HandlerInfo } from './fixtures.ts';
import { createProfiles } from './fixtures-profiles.ts';
import { createPlayers, createSettingsValues } from './fixtures-settings.ts';
import { initialMockLanguage } from './i18n.ts';
import type { TimerSet } from './timers.ts';

/** A game in the library: the summary the UI lists, plus what the game page and its menu show. */
export interface MockGame {
  summary: GameSummary;
  executablePath: string;
  /** Whether the content folder (instances) exists: drives the `deleteContentFolder` menu item. */
  contentFolderExists: boolean;
  /** Players with a save backup: one `openBackup:{player}` menu item each. */
  backups: string[];
  screenshotUrls: string[];
  /** Per-game menu toggles and choices. */
  menu: { keepInstancesFolder: boolean; disableProfiles: boolean; disableHandlerUpdate: boolean; steamLanguage: string };
  /** Saved per game by `setup.setUseApiIndex`. */
  useApiIndex: boolean;
  /** Saved per game by `setup.setSaveProfileOnLaunch`. */
  saveProfileOnLaunch: boolean;
}

/** An installed handler. */
export interface MockHandler {
  info: HandlerInfo;
  /** Installed version, unknown for handlers installed from a file. */
  version?: number;
}

/** Rows and columns a monitor is split into. */
export interface Grid {
  rows: number;
  columns: number;
}

/** A rectangle of grid cells on a monitor. */
export interface GridArea {
  col: number;
  row: number;
  colSpan: number;
  rowSpan: number;
}

/** A monitor of the open setup. */
export interface MockScreen {
  index: number;
  displayIndex: number;
  bounds: Rect;
  isPrimary: boolean;
  layout: ScreenLayout;
  layoutLocked: boolean;
}

/** An input device of the open setup (its player is derived from the players). */
export interface MockDevice {
  id: string;
  kind: DeviceKind;
  name: string;
  apiIndex: number;
  revealed: boolean;
  needsButtonPress: boolean;
  isVirtual: boolean;
}

/** A player of the open setup: one instance, placed on an area of a monitor's grid. */
export interface MockPlayer {
  deviceIds: string[];
  guestDeviceIds: string[];
  screenIndex: number;
  area: GridArea;
  /** Area it was placed in, restored when an expanded player shrinks back. */
  home: GridArea;
  /** Number given by the loaded profile. */
  profileNumber?: number;
}

/** Where a profile player sits. */
export interface ProfileSeat extends ProfilePlayer {
  slotIndex: number;
}

/** Profile settings as stored (the read-only choice lists are added when read). */
export type StoredProfileSettings = Omit<
  ProfileSettings,
  'isNew' | 'networkChoices' | 'audioChoices' | 'mergerResolutionChoices' | 'splitDividerColorChoices'
>;

/** A saved profile. Its number is its position in the game's list, plus one. */
export interface MockProfile {
  /** Title, notes, auto play and the rest of the settings. */
  settings: StoredProfileSettings;
  seats: ProfileSeat[];
  /** Layout of each monitor, by monitor index. */
  layouts: ScreenLayout[];
  /** Choice index or text of each option, by option key. */
  options: Record<string, number | string>;
  /** Selected item of each custom step, by step index. */
  stepItems: Record<number, number>;
}

/** The open setup (lobby). */
export interface MockSetup {
  gameId: string;
  gameName: string;
  revision: number;
  /** Index of the current step in the flow. */
  step: number;
  screens: MockScreen[];
  /** Grid of the `custom` layout, from the settings when the setup was opened. */
  customGrid: Grid;
  devices: MockDevice[];
  players: MockPlayer[];
  options: SetupOption[];
  customSteps: SetupCustomStep[];
  capabilities: SetupCapabilities;
  useApiIndex: boolean;
  saveProfileOnLaunch: boolean;
  /** The loaded profile: its number and its seats. */
  profile: { id: number; seats: ProfileSeat[]; autoPlay: boolean } | null;
  /** Settings of the profile saved at launch, once edited. */
  pendingSettings: StoredProfileSettings | null;
  /** Pre-launch checks whose fix is running. */
  busyChecks: Set<string>;
  /** Whether the "close Steam" fix has run. */
  steamClosed: boolean;
  /** Device detection: reveals and activity. */
  timers: TimerSet;
}

/** The play session. `phase` is null outside a session (the state is then `setup` or `idle`). */
export interface MockSession {
  phase: Extract<SessionState, 'launching' | 'running' | 'ending'> | null;
  gameId?: string;
  gameName?: string;
  startedAt?: string;
  instances: SessionInstance[];
  inputsLocked: boolean;
  error?: string;
  /** Crash report of the failed launch, shown with `error`. */
  errorDetails?: string;
  /** Log of the current or last session. */
  log: SessionLogEntry[];
  topmost: boolean;
  cutscenes: boolean;
  /** Index of the current tiling (`switchLayout`); 0 is the setup's. */
  layoutIndex: number;
  /** Window areas as set up, in player order. */
  setupBounds: Rect[];
  /** Players and their devices as placed at Play, for the summary. */
  players: SessionSummaryPlayer[];
  /** Whether every window got ready (the session ran). */
  reachedRunning: boolean;
  /** Summary of the last session, kept until the next one starts. */
  lastSession?: SessionSummary;
  timers: TimerSet;
}

/** Everything the mock host remembers. */
export interface MockState {
  games: MockGame[];
  handlers: MockHandler[];
  preferences: LibraryPreferences;
  /** Saved profiles, by game id. */
  profiles: Record<string, MockProfile[]>;
  /** Settings values, by field key, as stored in Settings.ini. */
  settings: Record<string, string>;
  players: PlayerIdentities;
  setup: MockSetup | null;
  /** Last setup revision, across setups: a new setup never goes back to a revision the UI has seen. */
  setupRevision: number;
  session: MockSession;
  /** Last session log sequence number, across sessions. */
  logSeq: number;
  /** Sources (hub id or file path) being installed. */
  installing: Set<string>;
  /** Whether the "restart as administrator" fix has run. */
  elevated: boolean;
  /** Notices waiting to be dismissed, oldest first. */
  notices: AppNotice[];
  /** Last notice id number. */
  noticeSeq: number;
  /** Language of the app, and whether the user has chosen it (`i18n`, the language setting). */
  language: { locale: string; chosen: boolean };
}

/** Creates the startup state: a library of 13 games, profiles for two of them, default settings, no setup. */
export function createMockState(): MockState {
  const handlers = createInstalledHandlers();
  const language = initialMockLanguage();
  const settings = { ...createSettingsValues(), 'misc.appLanguage': language.locale };
  const players = createPlayers();
  return {
    language,
    games: createLibrary(handlers),
    handlers,
    preferences: { sort: 'lastPlayed', favoritesOnly: false, filterGamepads: false, filterKeyboard: false, filterMultipleKeyboards: false },
    profiles: createProfiles(settings, players),
    settings,
    players,
    setup: null,
    setupRevision: 0,
    session: {
      phase: null,
      instances: [],
      inputsLocked: false,
      log: [],
      topmost: true,
      cutscenes: false,
      layoutIndex: 0,
      setupBounds: [],
      players: [],
      reachedRunning: false,
      timers: new Set(),
    },
    logSeq: 0,
    installing: new Set(),
    elevated: false,
    notices: [],
    noticeSeq: 0,
  };
}
