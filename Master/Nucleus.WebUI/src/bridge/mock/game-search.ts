// The Nucleus 2.5 automatic game search: game folders (settings), the folder search with progress (library), the
// add-game context report, and handler scripts with errors (handlers). Drives are faked: C: and D: exist, others are
// "unplugged".
import { BridgeError } from '../errors.ts';
import type {
  ExecutableContextMismatch,
  FolderSearchCandidate,
  FolderSearchResult,
  GameFolders,
  HandlerFileError,
  SearchGamesRequest,
} from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { invalidParams, notFound } from './failures.ts';
import { executablePathOf } from './fixtures.ts';
import type { MockHandler, MockState } from './state.ts';
import { sleep } from './util.ts';

/** Game folders at startup: two on D:, one on a drive that isn't plugged in. */
export const GAME_FOLDERS: readonly string[] = ['D:\\Games', 'D:\\SteamLibrary\\steamapps\\common', 'F:\\Old games'];

/** Time spent in each folder of a search. */
const FOLDER_MS = 110;

/** Other copies of some games on the fake disks, so the search finds a game twice. */
const EXTRA_COPIES: Readonly<Record<string, readonly string[]>> = {
  'Left 4 Dead 2': ['D:\\Games\\Backups\\Left 4 Dead 2\\left4dead2.exe'],
};

/** Folders without games, to make the search take a moment. */
const OTHER_FOLDERS = ['Tools', 'Mods', 'Emulators', 'Old saves', 'Patches', 'Benchmarks'];

const lower = (path: string) => path.toLowerCase();
const trimSlash = (path: string) => (/^[a-z]:\\$/i.test(path) ? path : path.replace(/[\\/]+$/, ''));

/** Whether a fake folder exists right now: C: and D: are there, other drives are unplugged. */
export const folderExists = (path: string) => /^[cd]:\\/i.test(path);

const under = (path: string, root: string) => lower(path).startsWith(`${lower(trimSlash(root))}\\`);

function readFlag(name: string): string | null {
  try {
    return globalThis.localStorage?.getItem(name) ?? null;
  } catch {
    return null;
  }
}

/** `settings.getGameFolders`: the switch is `misc.autoSearchGame` ('' while never asked). */
export function gameFolders(state: MockState): GameFolders {
  const value = state.settings['misc.autoSearchGame'];
  return {
    autoSearch: value === 'true' ? true : value === 'false' ? false : undefined,
    folders: [...state.gameFolders].sort((a, b) => a.localeCompare(b, 'en', { sensitivity: 'base' })).map((path) => ({ path, exists: folderExists(path) })),
  };
}

const changed = (context: MockContext) => context.emit('settings.changed', { keys: ['gameFolders'] });

/** `settings.addGameFolder`: the folder must exist; a folder already saved is kept once. */
export function addGameFolder(context: MockContext, state: MockState, path: string): GameFolders {
  const folder = trimSlash(path.trim());
  if (!/^[a-z]:\\|^\\\\/i.test(folder)) invalidParams('path is not a valid folder path.');
  if (!folderExists(folder)) throw new BridgeError('notFound', 'The folder does not exist.', { reason: 'folderNotFound', path: folder });
  if (!state.gameFolders.some((known) => lower(known) === lower(folder))) {
    state.gameFolders.push(folder);
    changed(context);
  }
  return gameFolders(state);
}

/** `settings.removeGameFolder`: unknown folders are ignored. */
export function removeGameFolder(context: MockContext, state: MockState, path: string): GameFolders {
  const folder = lower(trimSlash(path.trim()));
  const before = state.gameFolders.length;
  state.gameFolders = state.gameFolders.filter((known) => lower(known) !== folder);
  if (state.gameFolders.length !== before) changed(context);
  return gameFolders(state);
}

/** Where the installed handlers' games are on the fake disks. */
function diskCopies(handlers: readonly MockHandler[]): { handler: MockHandler; path: string }[] {
  return handlers.flatMap((handler) =>
    [executablePathOf(handler.info), ...(EXTRA_COPIES[handler.info.gameId] ?? [])].map((path) => ({ handler, path })),
  );
}

/** The game folder (first level under a search root) a path is in. */
function gameFolderOf(path: string, root: string): string {
  const rest = path.slice(trimSlash(root).length + 1);
  return `${trimSlash(root)}\\${rest.split('\\')[0]}`;
}

/**
 * `library.searchGames`: walks the existing game folders one sub-folder at a time with progress events, and finds the
 * installed handlers' executables under them (with `gameId`, that handler only, stopping at its first copy).
 */
export async function searchGames(context: MockContext, state: MockState, { searchId, gameId }: SearchGamesRequest): Promise<FolderSearchResult> {
  if (!searchId) invalidParams('searchId is required.');
  const handlers = gameId
    ? [state.handlers.find((handler) => handler.info.gameId === gameId) ?? notFound('No installed handler for this game.')]
    : state.handlers;
  const roots = state.gameFolders.filter(folderExists);
  const copies = diskCopies(handlers).filter((copy) => roots.some((root) => under(copy.path, root)));
  const units = roots.flatMap((root) => {
    const games = copies.filter((copy) => under(copy.path, root)).map((copy) => gameFolderOf(copy.path, root));
    return [...new Set([...games, ...OTHER_FOLDERS.map((name) => `${trimSlash(root)}\\${name}`)])].sort((a, b) => a.localeCompare(b, 'en'));
  });

  const found: { handler: MockHandler; path: string }[] = [];
  for (const [index, unit] of units.entries()) {
    context.emit('library.searchProgress', { searchId, percent: Math.min(99, Math.round((index / units.length) * 100)), currentFolder: unit, found: found.length });
    await sleep(FOLDER_MS);
    found.push(...copies.filter((copy) => lower(gameFolderOf(copy.path, roots.find((root) => under(copy.path, root)) ?? '')) === lower(unit)));
    if (gameId && found.length > 0) break;
  }

  const candidates: FolderSearchCandidate[] = (gameId ? found.slice(0, 1) : found)
    .map(({ handler, path }) => ({
      gameId: handler.info.gameId,
      gameName: handler.info.gameName,
      handlerFile: `${handler.info.gameName.replace(/[^\w ]/g, '')}.js`,
      executablePath: path,
      installFolder: path.slice(0, path.lastIndexOf('\\')),
      inLibrary: state.games.some((game) => game.summary.id === handler.info.gameId || lower(game.executablePath) === lower(path)),
    }))
    .sort((a, b) => a.gameName.localeCompare(b.gameName, 'en') || a.executablePath.localeCompare(b.executablePath, 'en'));
  context.emit('library.searchProgress', { searchId, percent: 100, found: candidates.length });
  return { candidates, foldersSearched: roots.length };
}

/**
 * Data of the add-game `contextMissing` refusal, forced with `localStorage['nucleus.mock.addGame']`: `context` (files
 * missing next to the executable, two handlers) or `excluded` (a file the handler refuses). Null otherwise.
 */
export function forcedContextMismatch(fileName: string): { message: string; data: ExecutableContextMismatch } | null {
  const mode = readFlag('nucleus.mock.addGame');
  if (mode === 'context') {
    const handlers = [
      { gameId: 'EldenRing', name: 'Elden Ring', file: 'Elden Ring.js', missing: ['regulation.bin', 'Data0.bdt'], excluded: [] },
      { gameId: 'EldenRingSeamless', name: 'Elden Ring (Seamless Co-op)', file: 'Elden Ring Seamless.js', missing: ['SeamlessCoop\\'], excluded: [] },
    ];
    return {
      message: `The Elden Ring handler expects regulation.bin, Data0.bdt next to ${fileName}. Pick the executable inside the game's install folder.`,
      data: { reason: 'contextMissing', executable: fileName, gameId: 'EldenRing', missing: handlers[0]!.missing, excluded: [], handlers },
    };
  }
  if (mode === 'excluded') {
    const handler = { gameId: 'Terraria', name: 'Terraria', file: 'Terraria.js', missing: [], excluded: ['tModLoader.dll'] };
    return {
      message: `The Terraria handler doesn't accept ${fileName} when tModLoader.dll is next to it (another edition or version of the game). Pick the executable of the edition this handler is for.`,
      data: { reason: 'contextMissing', executable: fileName, gameId: 'Terraria', missing: [], excluded: handler.excluded, handlers: [handler] },
    };
  }
  return null;
}

/** Handler scripts with errors, shown with `localStorage['nucleus.mock.notices'] = '1'` (like the broken-handler notice). */
export function seedHandlerErrors(state: MockState): void {
  state.handlerErrors = [
    {
      file: 'Broken.js',
      previousVersionInUse: false,
      error: 'SyntaxError: Unexpected token ")" at line 42, column 17',
    },
    {
      file: 'Valheim.js',
      gameName: 'Valheim',
      gameId: 'Valheim',
      previousVersionInUse: true,
      error: 'ReferenceError: Contxt is not defined (line 118: Contxt.ModifySaveFile(...))',
    },
  ];
}

/** `handlers.listErrors`. */
export const handlerErrors = (state: MockState): HandlerFileError[] => state.handlerErrors;
