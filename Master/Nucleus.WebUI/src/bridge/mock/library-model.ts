// The library and the installed handlers: lookups, the game page, adding, removing, artwork, update flags.
import type {
  AddGameRequest,
  AddGameResult,
  FindExecutableRequest,
  GameDetails,
  GameSummary,
  InstalledGame,
  SetExecutableRequest,
} from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict, invalidParams, notFound } from './failures.ts';
import { HUB_HANDLERS, NUCLEUS_FOLDER, acceptsExecutable, coverUrlOf, executableNamesOf, newGame, screenshotUrlsOf } from './fixtures.ts';
import { setupFixtureFor } from './fixtures-setup.ts';
import { profilesEnabled } from './profile-model.ts';
import type { MockGame, MockHandler, MockState } from './state.ts';
import { sleep } from './util.ts';

/** Time to fetch the artwork of one game. */
const ARTWORK_MS = 200;

/** A library game. Throws `notFound`. */
export function findGame(state: MockState, gameId: string): MockGame {
  return state.games.find((game) => game.summary.id === gameId) ?? notFound(`No game '${gameId}' in the library.`);
}

/** The installed handler of a game id. */
export const findHandler = (state: MockState, gameId: string): MockHandler | undefined =>
  state.handlers.find((handler) => handler.info.gameId === gameId);

/** Whether a game id is in the library. */
export const inLibrary = (state: MockState, gameId: string) => state.games.some((game) => game.summary.id === gameId);

/** Everything the game page shows. */
export function gameDetails(state: MockState, game: MockGame): GameDetails {
  const { summary } = game;
  const info = findHandler(state, summary.id)?.info;
  const setup = setupFixtureFor(summary.id);
  return {
    summary,
    notes: info?.notes,
    screenshotUrls: game.screenshotUrls,
    executablePath: game.executablePath,
    architecture: summary.executableFound ? (info?.architecture ?? 'unknown') : 'unknown',
    handlerFile: `${summary.id}.js`,
    maintainer: info?.maintainer,
    steamId: info?.steamAppId?.toString(),
    contentFolder: `${NUCLEUS_FOLDER}\\content\\${summary.id}`,
    profilesEnabled: profilesEnabled(state, summary.id),
    profileCount: state.profiles[summary.id]?.length ?? 0,
    hasOptions: setup.options.length + setup.customSteps.length > 0,
    showNotesFirst: summary.firstLaunch && info?.notes !== undefined && state.settings['misc.disableForcedNote'] !== 'true',
  };
}

/** Recomputes whether the hub version found by the last check is newer (and update checks are on for the game). */
export function refreshUpdateAvailable(state: MockState, game: MockGame): void {
  const installed = findHandler(state, game.summary.id)?.version;
  const latest = game.summary.latestHandlerVersion;
  game.summary.updateAvailable = !game.menu.disableHandlerUpdate && installed !== undefined && latest !== undefined && installed < latest;
}

const fileNameOf = (path: string) => path.slice(path.lastIndexOf('\\') + 1);

/** Emits `library.changed` for some games. */
export function libraryChanged(context: MockContext, ...gameIds: string[]): void {
  context.emit('library.changed', { gameIds });
}

/**
 * Adds a game from its executable, with the installed handlers that expect that file name. When none does, the mock
 * uses the first installed handler without a game, as if the user had picked its executable.
 */
export function addGame(context: MockContext, state: MockState, { executablePath, handlerGameId }: AddGameRequest): AddGameResult {
  if (!/\.exe$/i.test(executablePath)) invalidParams('Pick the game executable (.exe).');
  const fileName = fileNameOf(executablePath);
  const matching = state.handlers.filter((handler) => acceptsExecutable(handler.info, fileName));
  if (!handlerGameId && matching.length > 1) {
    return { candidates: matching.map(({ info }) => ({ gameId: info.gameId, name: info.gameName })) };
  }
  const handler = handlerGameId
    ? (findHandler(state, handlerGameId) ?? notFound(`No installed handler '${handlerGameId}'.`))
    : (matching[0] ??
      state.handlers.find((candidate) => !inLibrary(state, candidate.info.gameId)) ??
      notFound(`No installed handler runs ${fileName}. Install its handler from the hub first.`));
  const existing = state.games.find((game) => game.summary.id === handler.info.gameId);
  if (existing) {
    existing.executablePath = executablePath;
    existing.summary.executableFound = true;
    libraryChanged(context, existing.summary.id);
    return { game: existing.summary, candidates: [] };
  }
  const game = newGame(handler, executablePath);
  state.games.push(game);
  libraryChanged(context, game.summary.id);
  context.emit('handlers.changed', { gameIds: [game.summary.id] });
  return { game: game.summary, candidates: [] };
}

/** Points a game at a moved executable, which must be one its handler accepts. */
export function setExecutable(context: MockContext, state: MockState, { gameId, executablePath }: SetExecutableRequest): void {
  const game = findGame(state, gameId);
  const info = findHandler(state, gameId)?.info;
  const fileName = fileNameOf(executablePath);
  if (info && !acceptsExecutable(info, fileName)) {
    conflict('executableMismatch', `${game.summary.name} runs ${executableNamesOf(info).join(' or ')}, not ${fileName}.`);
  }
  game.executablePath = executablePath;
  game.summary.executableFound = true;
  libraryChanged(context, gameId);
}

/** Games installed through Steam and Epic: the catalogue's Steam games plus a few without a handler. */
export function installedGames(): InstalledGame[] {
  const steam = HUB_HANDLERS.flatMap((info): InstalledGame[] =>
    info.steamAppId && info.gameId !== 'Palworld'
      ? [{ store: 'steam', storeId: String(info.steamAppId), name: info.gameName, installFolder: info.installFolder }]
      : [],
  );
  const steamFolder = 'D:\\SteamLibrary\\steamapps\\common';
  return [
    ...steam,
    { store: 'steam', storeId: '730', name: 'Counter-Strike 2', installFolder: `${steamFolder}\\Counter-Strike Global Offensive` },
    { store: 'steam', storeId: '413150', name: 'Stardew Valley', installFolder: `${steamFolder}\\Stardew Valley` },
    { store: 'epic', storeId: 'Pal', name: 'Palworld', installFolder: 'C:\\Program Files\\Epic Games\\Palworld' },
    { store: 'epic', storeId: 'Sugar', name: 'Rocket League', installFolder: 'C:\\Program Files\\Epic Games\\rocketleague' },
  ];
}

/** Where an executable lies under a folder: found when a known handler accepts that file name (next to its main one). */
export function findExecutable(state: MockState, { folder, executableName }: FindExecutableRequest): string | null {
  const known = [...state.handlers.map((handler) => handler.info), ...HUB_HANDLERS].find((info) => acceptsExecutable(info, executableName));
  return known ? `${folder.replace(/\\+$/, '')}\\${known.executableSubpath.replace(/[^\\]+$/, executableName)}` : null;
}

/** Removes a game from the library; its handler stays installed. */
export function removeGame(context: MockContext, state: MockState, gameId: string): void {
  findGame(state, gameId);
  state.games = state.games.filter((game) => game.summary.id !== gameId);
  libraryChanged(context, gameId);
  context.emit('handlers.changed', { gameIds: [gameId] });
}

/** Downloads cover and screenshots of one game, or of every game, with `library.artworkProgress` after each. */
export async function downloadArtwork(context: MockContext, state: MockState, gameId?: string): Promise<void> {
  const games = gameId ? [findGame(state, gameId)] : [...state.games];
  for (const [index, game] of games.entries()) {
    await sleep(ARTWORK_MS);
    const info = findHandler(state, game.summary.id)?.info;
    if (info) {
      game.screenshotUrls = screenshotUrlsOf(info);
      Object.assign(game.summary, { coverUrl: coverUrlOf(info), heroUrl: game.screenshotUrls[0] } satisfies Partial<GameSummary>);
    }
    context.emit('library.artworkProgress', { gameId: game.summary.id, done: index + 1, total: games.length });
  }
  libraryChanged(context, ...games.map((game) => game.summary.id));
}
