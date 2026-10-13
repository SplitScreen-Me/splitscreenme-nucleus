// Static data of the mock host: the hub handlers it knows, the user's library, handler notes.
// Names, hub ids, maintainers, player counts and art come from the validated mockups and the live hub.
import type { GameInputs, GameSummary } from '../generated/contract.ts';
import { hubScreenshotUrls, igdbImage } from './fixtures-screenshots.ts';
import type { MockGame, MockHandler } from './state.ts';

/** What the mock knows of a handler: hub metadata plus where its game is installed. */
export interface HandlerInfo {
  /** Handler GUID (`Game.GUID` in the handler: a folder name, letters and spaces). */
  gameId: string;
  gameName: string;
  /** Hub handler id; none for handlers installed from a file. */
  handlerId?: string;
  /** Current version on the hub. */
  hubVersion?: number;
  maintainer?: string;
  maxPlayers: number;
  inputs: GameInputs;
  executableName: string;
  /** Other executable names the handler accepts (launchers, anti-cheat variants). */
  otherExecutables?: string[];
  /** Folder the game is installed in. */
  installFolder: string;
  /** Path of the executable inside `installFolder`. */
  executableSubpath: string;
  steamAppId?: number;
  igdbCover?: string;
  /** Steam has no portrait capsule for this app: the IGDB cover is used. */
  noCapsule?: boolean;
  architecture: 'x64' | 'x86';
  notes?: string;
}

export const NUCLEUS_FOLDER = 'C:\\Nucleus Co-op';
const STEAM_COMMON = 'D:\\SteamLibrary\\steamapps\\common';

const PADS_KEYBOARD: GameInputs = { xInput: true, dInput: true, keyboard: true, multipleKeyboards: false };
const PADS_KEYBOARDS: GameInputs = { xInput: true, dInput: true, keyboard: true, multipleKeyboards: true };
const XINPUT_KEYBOARD: GameInputs = { xInput: true, dInput: false, keyboard: true, multipleKeyboards: false };

const ELDEN_RING_NOTES = `This handler includes the *Seamless Co-op* mod by LukeYui - https://www.nexusmods.com/eldenring/mods/510 and *Mod Loader* by TechieW - https://www.nexusmods.com/eldenring/mods/117. Credit to them for their great work.

[Split-screen setup guide]{https://www.splitscreen.me/docs/usage}

*Let all instances load to the title screen before using inputs.|255,196,0* If you are using at least one mouse and keyboard press the *END* key (default keybinding) to lock/unlock the input. Make sure to alt-tab to a non-game window before locking.

The host must use the *Tiny Great Pot* to create a session. At that point, the other players can join using the *Effigy of Meleina*. Any player can leave at any time using the Separating Mist - *if the host uses this item, the session will be disbanded and all players will disconnect.|230,80,80* Make sure to complete the tutorial before attempting to connect the instances.

To use Mod Loader simply enable the launch option in Nucleus, as long as your mods are located in the original game folder's "mods" folder they should work.

Handler by [PoundlandBacon]{https://v3.splitscreen.me/hub/authors/PoundlandBacon} and [Talos91]{https://v3.splitscreen.me/hub/authors/Talos91}, special thanks to LukeYui, TechieW and Mr Goldberg.`;

const MINECRAFT_NOTES = `Requires *Java 21|255,140,60* and an offline-capable launcher profile per instance.

Pick the game version in the next step; every instance uses the same one. Worlds are shared through LAN: the first player opens the world to LAN, the others join from *Multiplayer*.

Troubleshooting: [FAQ]{https://www.splitscreen.me/docs/faq}`;

const GENERIC_NOTES = `Let every instance reach the *title screen* before anyone presses a button.

If you use keyboards and mice, press *END* once every window is in place to give each one its own cursor. Press *END* again to release it.

One player hosts a co-op game; the other instances join from the in-game menu.`;

const steamGame = (folder: string, executableSubpath: string) => ({ installFolder: `${STEAM_COMMON}\\${folder}`, executableSubpath });

/** Handlers on the hub, by game id. */
export const HUB_HANDLERS: readonly HandlerInfo[] = [
  { gameId: 'EldenRing', gameName: 'Elden Ring', handlerId: 'CgTFu6jc83pMKPZBy', hubVersion: 40, maintainer: 'PoundlandBacon', maxPlayers: 4, inputs: PADS_KEYBOARDS, executableName: 'eldenring.exe', otherExecutables: ['start_protected_game.exe'], ...steamGame('ELDEN RING', 'Game\\eldenring.exe'), steamAppId: 1245620, igdbCover: 'co4jni', architecture: 'x64', notes: ELDEN_RING_NOTES },
  { gameId: 'Peak', gameName: 'Peak', handlerId: 'rLSxwziuvTcuF3GTp', hubVersion: 10, maintainer: 'Talos91', maxPlayers: 4, inputs: PADS_KEYBOARD, executableName: 'PEAK.exe', ...steamGame('PEAK', 'PEAK.exe'), steamAppId: 3527290, igdbCover: 'coa1i1', noCapsule: true, architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'Minecraft', gameName: 'Minecraft', handlerId: 'QC7TTYjjZzaQ6Y6Nw', hubVersion: 32, maintainer: 'dr.oldboi', maxPlayers: 16, inputs: PADS_KEYBOARDS, executableName: 'javaw.exe', installFolder: 'D:\\Games\\Minecraft', executableSubpath: 'runtime\\bin\\javaw.exe', igdbCover: 'co1x7j', architecture: 'x64', notes: MINECRAFT_NOTES },
  { gameId: 'HaloMCC', gameName: 'Halo: The Master Chief Collection', handlerId: 'xjBSwFNj3ndRntsZh', hubVersion: 30, maintainer: 'PoundlandBacon', maxPlayers: 16, inputs: XINPUT_KEYBOARD, executableName: 'MCC-Win64-Shipping.exe', otherExecutables: ['MCC-Win64-Shipping-EAC.exe'], ...steamGame('Halo The Master Chief Collection', 'MCC\\Binaries\\Win64\\MCC-Win64-Shipping.exe'), steamAppId: 976730, igdbCover: 'co1t95', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'DarkSouls3', gameName: 'Dark Souls III', handlerId: '8ToryPKXFmff7D8h7', hubVersion: 11, maintainer: 'Talos91', maxPlayers: 6, inputs: PADS_KEYBOARD, executableName: 'DarkSoulsIII.exe', ...steamGame('DARK SOULS III', 'Game\\DarkSoulsIII.exe'), steamAppId: 374320, igdbCover: 'co1vcf', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'TheForest', gameName: 'The Forest', handlerId: 'wXJ7GwedvhiBe2syS', hubVersion: 8, maintainer: 'Talos91', maxPlayers: 8, inputs: PADS_KEYBOARDS, executableName: 'TheForest.exe', ...steamGame('The Forest', 'TheForest.exe'), steamAppId: 242760, igdbCover: 'co20x5', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'Borderlands 2', gameName: 'Borderlands 2', handlerId: 'YztSFvxEtWgjuGwTT', hubVersion: 13, maintainer: 'Talos91', maxPlayers: 4, inputs: PADS_KEYBOARDS, executableName: 'Borderlands2.exe', ...steamGame('Borderlands 2', 'Binaries\\Win32\\Borderlands2.exe'), steamAppId: 49520, igdbCover: 'co1rbh', architecture: 'x86', notes: GENERIC_NOTES },
  { gameId: 'Terraria', gameName: 'Terraria', handlerId: 'J5vdAQFvS9i3AZCRE', hubVersion: 9, maintainer: 'Talos91', maxPlayers: 8, inputs: PADS_KEYBOARD, executableName: 'Terraria.exe', ...steamGame('Terraria', 'Terraria.exe'), steamAppId: 105600, igdbCover: 'co1rbo', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'ResidentEvil5', gameName: 'Resident Evil 5', handlerId: 'q926yWzpZZNiv7Cck', hubVersion: 15, maintainer: 'Talos91', maxPlayers: 4, inputs: PADS_KEYBOARD, executableName: 're5dx9.exe', ...steamGame('Resident Evil 5', 're5dx9.exe'), steamAppId: 21690, igdbCover: 'co1nb6', architecture: 'x86', notes: GENERIC_NOTES },
  { gameId: 'Payday2', gameName: 'Payday 2', handlerId: 'o8ixYAJ9Q382LJfLB', hubVersion: 17, maintainer: 'Talos91', maxPlayers: 4, inputs: PADS_KEYBOARDS, executableName: 'payday2_win32_release.exe', ...steamGame('PAYDAY 2', 'payday2_win32_release.exe'), steamAppId: 218620, igdbCover: 'co20u7', architecture: 'x86', notes: GENERIC_NOTES },
  { gameId: 'HollowKnight', gameName: 'Hollow Knight', handlerId: 'sWgnoTqjmk8M49QML', hubVersion: 11, maintainer: 'Talos91', maxPlayers: 5, inputs: PADS_KEYBOARD, executableName: 'hollow_knight.exe', ...steamGame('Hollow Knight', 'hollow_knight.exe'), steamAppId: 367520, igdbCover: 'co1rgi', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'SonsOfTheForest', gameName: 'Sons of the Forest', handlerId: 'gNrREmQFCTRgYeBdd', hubVersion: 4, maintainer: 'Talos91', maxPlayers: 8, inputs: PADS_KEYBOARDS, executableName: 'SonsOfTheForest.exe', ...steamGame('Sons Of The Forest', 'SonsOfTheForest.exe'), steamAppId: 1326470, igdbCover: 'co67g5', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'Left 4 Dead 2', gameName: 'Left 4 Dead 2', handlerId: 'wFsXDjoAkHpL4XZm8', hubVersion: 17, maintainer: 'Talos91', maxPlayers: 6, inputs: PADS_KEYBOARDS, executableName: 'left4dead2.exe', ...steamGame('Left 4 Dead 2', 'left4dead2.exe'), steamAppId: 550, igdbCover: 'co1qrl', architecture: 'x86', notes: GENERIC_NOTES },
  { gameId: 'Valheim', gameName: 'Valheim', handlerId: '6hkBwKMXqpnjiWTdi', hubVersion: 27, maintainer: 'PoundlandBacon', maxPlayers: 10, inputs: PADS_KEYBOARDS, executableName: 'valheim.exe', ...steamGame('Valheim', 'valheim.exe'), steamAppId: 892970, igdbCover: 'co1xrc', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'DontStarveTogether', gameName: "Don't Starve Together", handlerId: 'HJtQAkDh9nyRd6z6J', hubVersion: 15, maintainer: 'Talos91', maxPlayers: 6, inputs: PADS_KEYBOARD, executableName: 'dontstarve_steam_x64.exe', ...steamGame("Don't Starve Together", 'bin64\\dontstarve_steam_x64.exe'), steamAppId: 322330, igdbCover: 'co1qt1', architecture: 'x64', notes: GENERIC_NOTES },
  { gameId: 'Palworld', gameName: 'Palworld', handlerId: 'fDmTaAoYKJmiWi3s3', hubVersion: 6, maintainer: 'PoundlandBacon', maxPlayers: 4, inputs: PADS_KEYBOARD, executableName: 'Palworld-Win64-Shipping.exe', ...steamGame('Palworld', 'Pal\\Binaries\\Win64\\Palworld-Win64-Shipping.exe'), steamAppId: 1623730, igdbCover: 'co7n02', architecture: 'x64', notes: GENERIC_NOTES },
];

/** A handler installed from a file (not from the hub) that runs the same executable as the Minecraft one. */
const MULTIMC_HANDLER: HandlerInfo = {
  gameId: 'MinecraftMultiMC',
  gameName: 'Minecraft (MultiMC)',
  maintainer: 'dr.oldboi',
  maxPlayers: 8,
  inputs: PADS_KEYBOARDS,
  executableName: 'javaw.exe',
  installFolder: 'D:\\Games\\MultiMC',
  executableSubpath: 'java\\bin\\javaw.exe',
  igdbCover: 'co1x7j',
  architecture: 'x64',
  notes: MINECRAFT_NOTES,
};

/** Initial state of a library game. */
interface LibrarySeed {
  gameId: string;
  /** Installed handler version; the hub version when omitted. */
  version?: number;
  favorite?: boolean;
  /** No update check has run yet: the hub version is unknown. */
  unchecked?: boolean;
  playtimeHours?: number;
  /** Never launched from Nucleus when omitted. */
  playedDaysAgo?: number;
  executableFound?: boolean;
  /** Cover and screenshots not downloaded yet. */
  noArtwork?: boolean;
  /** Players with a save backup (dynamic `openBackup:` menu items). */
  backups?: string[];
}

const LIBRARY: readonly LibrarySeed[] = [
  { gameId: 'EldenRing', favorite: true, playtimeHours: 61.5, playedDaysAgo: 1, backups: ['Player 1', 'Player 2'] },
  { gameId: 'Peak', version: 9, playtimeHours: 6.2, playedDaysAgo: 9 },
  { gameId: 'Minecraft', favorite: true, playtimeHours: 148, playedDaysAgo: 3, backups: ['Player 2'] },
  { gameId: 'HaloMCC', playtimeHours: 22.4, playedDaysAgo: 7 },
  { gameId: 'DarkSouls3', version: 10, unchecked: true, playtimeHours: 14.8, playedDaysAgo: 14 },
  { gameId: 'TheForest', playtimeHours: 9.1, playedDaysAgo: 40, executableFound: false },
  { gameId: 'Borderlands 2', playtimeHours: 31, playedDaysAgo: 25 },
  { gameId: 'Terraria', playtimeHours: 12.6, playedDaysAgo: 55 },
  { gameId: 'ResidentEvil5', playtimeHours: 4.3, playedDaysAgo: 60 },
  { gameId: 'Payday2' },
  { gameId: 'HollowKnight' },
  { gameId: 'SonsOfTheForest', noArtwork: true },
  { gameId: 'Valheim', favorite: true, playtimeHours: 40.7, playedDaysAgo: 2 },
];

const DAY_MS = 86_400_000;

/** A handler the mock knows by hub id (hub handlers only). */
export function findHubHandler(handlerId: string): HandlerInfo | undefined {
  return HUB_HANDLERS.find((handler) => handler.handlerId === handlerId);
}

/** Handlers installed at startup: those of the library games, Left 4 Dead 2 (not in the library) and a file install. */
export function createInstalledHandlers(): MockHandler[] {
  const installed = HUB_HANDLERS.filter((info) => info.gameId === 'Left 4 Dead 2' || LIBRARY.some((seed) => seed.gameId === info.gameId));
  return [
    ...installed.map((info) => ({ info, version: LIBRARY.find((seed) => seed.gameId === info.gameId)?.version ?? info.hubVersion })),
    { info: MULTIMC_HANDLER },
  ];
}

/** Library games at startup, built from the installed handlers. */
export function createLibrary(handlers: readonly MockHandler[], now = Date.now()): MockGame[] {
  return LIBRARY.flatMap((seed) => {
    const handler = handlers.find((candidate) => candidate.info.gameId === seed.gameId);
    return handler ? [seededGame(handler, seed, now)] : [];
  });
}

function seededGame(handler: MockHandler, seed: LibrarySeed, now: number): MockGame {
  const game = newGame(handler, executablePathOf(handler.info));
  const played = seed.playedDaysAgo !== undefined;
  const latest = seed.unchecked ? undefined : handler.info.hubVersion;
  Object.assign(game.summary, {
    favorite: seed.favorite ?? false,
    latestHandlerVersion: latest,
    updateAvailable: latest !== undefined && handler.version !== undefined && handler.version < latest,
    playtimeSeconds: Math.round((seed.playtimeHours ?? 0) * 3600),
    lastPlayedAt: played ? new Date(now - (seed.playedDaysAgo ?? 0) * DAY_MS).toISOString() : undefined,
    firstLaunch: !played,
    executableFound: seed.executableFound ?? true,
  } satisfies Partial<GameSummary>);
  if (seed.noArtwork) {
    Object.assign(game.summary, { coverUrl: undefined, heroUrl: undefined } satisfies Partial<GameSummary>);
    game.screenshotUrls = [];
  }
  game.contentFolderExists = played;
  game.backups = seed.backups ?? [];
  return game;
}

/** Every executable name a handler accepts, the main one first. */
export const executableNamesOf = (info: HandlerInfo) => [info.executableName, ...(info.otherExecutables ?? [])];

/** Whether a handler accepts an executable file name (case-insensitive). */
export const acceptsExecutable = (info: HandlerInfo, fileName: string) =>
  executableNamesOf(info).some((name) => name.toLowerCase() === fileName.toLowerCase());

/** Full path of a handler's game executable. */
export function executablePathOf(info: HandlerInfo): string {
  return `${info.installFolder}\\${info.executableSubpath}`;
}

/** Cover URL of a handler's game: the Steam portrait capsule, else the IGDB cover. */
export function coverUrlOf(info: HandlerInfo): string | undefined {
  if (info.steamAppId && !info.noCapsule) return `https://cdn.akamai.steamstatic.com/steam/apps/${info.steamAppId}/library_600x900.jpg`;
  return info.igdbCover ? igdbImage(info.igdbCover, 'cover_big_2x') : undefined;
}

/** Screenshot URLs of a handler's game. */
export function screenshotUrlsOf(info: HandlerInfo): string[] {
  return info.handlerId ? hubScreenshotUrls(info.handlerId) : [];
}

/** A game just added to the library: never played, artwork downloaded, handler up to date. */
export function newGame(handler: MockHandler, executablePath: string): MockGame {
  const { info } = handler;
  const screenshotUrls = screenshotUrlsOf(info);
  return {
    summary: {
      id: info.gameId,
      name: info.gameName,
      handlerId: info.handlerId,
      handlerVersion: handler.version,
      maxPlayers: info.maxPlayers,
      inputs: { ...info.inputs },
      favorite: false,
      updateAvailable: false,
      latestHandlerVersion: info.hubVersion,
      heroUrl: screenshotUrls[0],
      iconUrl: info.igdbCover ? igdbImage(info.igdbCover, 'thumb') : undefined,
      coverUrl: coverUrlOf(info),
      playtimeSeconds: 0,
      firstLaunch: true,
      executableFound: true,
    },
    executablePath,
    contentFolderExists: false,
    backups: [],
    screenshotUrls,
    menu: { keepInstancesFolder: false, disableProfiles: false, disableHandlerUpdate: false, steamLanguage: 'Automatic' },
    useApiIndex: false,
    saveProfileOnLaunch: false,
  };
}

/** Description of a handler the mock has no data for (a hub id or a file it does not know). */
export function unknownHandler(gameId: string, gameName: string, handlerId?: string): HandlerInfo {
  return {
    gameId,
    gameName,
    handlerId,
    hubVersion: handlerId ? 1 : undefined,
    maxPlayers: 4,
    inputs: PADS_KEYBOARD,
    executableName: 'game.exe',
    installFolder: `D:\\Games\\${gameName}`,
    executableSubpath: 'game.exe',
    architecture: 'x64',
  };
}
