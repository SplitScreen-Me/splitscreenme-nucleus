// Rules of the game folder search and of adding games, without React: picking candidates, what to do after a handler
// install, reading the add-game context refusal. Relative imports only: the node tests load this file.
import { BridgeError } from '../bridge/errors.ts';
import type { ExecutableContextMismatch, FolderSearchCandidate, GameFolders } from '../bridge/generated/contract.ts';

const key = (gameId: string) => gameId.toLowerCase();

/**
 * Which candidates are picked at first: those not in the library, one copy per game (the first found). Several copies
 * of one game can't all be added: the library keeps one executable per game.
 */
export function defaultPicks(candidates: readonly FolderSearchCandidate[]): Set<string> {
  const picked = new Set<string>();
  const games = new Set<string>();
  for (const c of candidates) {
    if (c.inLibrary || games.has(key(c.gameId))) continue;
    games.add(key(c.gameId));
    picked.add(c.executablePath);
  }
  return picked;
}

/** Picks or unpicks a candidate; picking one copy of a game unpicks its other copies. Library games can't be picked. */
export function togglePick(picked: ReadonlySet<string>, candidates: readonly FolderSearchCandidate[], path: string, on: boolean): Set<string> {
  const next = new Set(picked);
  const target = candidates.find((c) => c.executablePath === path);
  if (!target || target.inLibrary) return next;
  if (on) {
    for (const c of candidates) if (key(c.gameId) === key(target.gameId)) next.delete(c.executablePath);
    next.add(path);
  } else next.delete(path);
  return next;
}

/** Ids (lower case) of the games found in more than one place. */
export function duplicatedGames(candidates: readonly FolderSearchCandidate[]): Set<string> {
  const count = new Map<string, number>();
  for (const c of candidates) count.set(key(c.gameId), (count.get(key(c.gameId)) ?? 0) + 1);
  return new Set([...count].filter(([, n]) => n > 1).map(([id]) => id));
}

/** Picks or unpicks every candidate that can be added (one copy per game). */
export function pickAll(candidates: readonly FolderSearchCandidate[], on: boolean): Set<string> {
  return on ? defaultPicks(candidates) : new Set();
}

/**
 * What happens after a handler install, before the executable picker (Nucleus 2.5): `ask` whether to search the game
 * folders (never asked), `search` them (on, with a folder that exists), else `pick` the executable straight away.
 * Unknown folders (the call failed) mean `pick`.
 */
export function afterInstallStep(folders: GameFolders | null | undefined): 'ask' | 'search' | 'pick' {
  if (!folders) return 'pick';
  if (folders.autoSearch === undefined || folders.autoSearch === null) return 'ask';
  return folders.autoSearch && folders.folders.some((f) => f.exists) ? 'search' : 'pick';
}

/** The data of an add-game refusal because the executable's folder doesn't match the handlers (reason `contextMissing`). */
export function contextMismatchOf(error: unknown): ExecutableContextMismatch | null {
  if (!BridgeError.is(error, 'notFound')) return null;
  const data = error.data as Partial<ExecutableContextMismatch> | null | undefined;
  if (data?.reason !== 'contextMissing' || !Array.isArray(data.handlers) || data.handlers.length === 0) return null;
  return {
    reason: 'contextMissing',
    executable: typeof data.executable === 'string' ? data.executable : '',
    gameId: typeof data.gameId === 'string' ? data.gameId : '',
    missing: Array.isArray(data.missing) ? data.missing : [],
    excluded: Array.isArray(data.excluded) ? data.excluded : [],
    handlers: data.handlers.map((h) => ({
      gameId: h.gameId ?? '',
      name: h.name || h.gameId || '',
      file: h.file ?? '',
      missing: Array.isArray(h.missing) ? h.missing : [],
      excluded: Array.isArray(h.excluded) ? h.excluded : [],
    })),
  };
}

/** The folder of a Windows path ("D:\\Games\\x.exe" → "D:\\Games"), or undefined. */
export function folderOf(path: string): string | undefined {
  const cut = Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/'));
  return cut > 0 ? path.slice(0, cut) : undefined;
}
