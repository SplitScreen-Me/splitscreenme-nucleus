// "Found on this PC": games installed through Steam and Epic, matched with the hub catalogue by name.
// The host only lists installed games (library.scanInstalledGames); the matching is the UI's job.
import type { GameSummary, InstalledGame, InstalledHandler } from '@/bridge';
import type { HubHandler } from '@/hub';
import { steamArt } from '@/lib/art';
import { normalizeName } from '@/lib/library';

/** An installed game with the hub handler that runs it. */
export interface FoundCandidate {
  /** Stable key (store + store id). */
  key: string;
  game: InstalledGame;
  handler: HubHandler;
}

/** Edition and platform suffixes that stores add and the hub usually leaves out. */
const SUFFIXES = [
  /\s*[:\-–—]?\s*game of the year(?: edition)?$/i,
  /\s*[:\-–—]?\s*goty(?: edition)?$/i,
  /\s*[:\-–—]?\s*(?:definitive|complete|enhanced|special|deluxe|ultimate|gold|legendary|standard|digital deluxe|director'?s cut|remastered|anniversary)(?: edition)?$/i,
  /\s*[:\-–—]?\s*(?:pc|windows)(?: edition| version)?$/i,
  /\s*\((?:\d{4}|pc|windows|classic)\)$/i,
];

/** Normalized names a game can be known by: as written, then without its edition suffix. */
export function nameKeys(name: string): string[] {
  const clean = name.replace(/[®™©]/g, '').trim();
  const keys = [normalizeName(clean)];
  let stripped = clean;
  for (let pass = 0; pass < 2; pass++) {
    for (const suffix of SUFFIXES) stripped = stripped.replace(suffix, '').trim();
  }
  const short = normalizeName(stripped);
  if (short && !keys.includes(short)) keys.push(short);
  return keys.filter(Boolean);
}

/** Verified handlers first, then the most downloaded. */
const better = (a: HubHandler, b: HubHandler) => Number(b.verified) - Number(a.verified) || b.downloads - a.downloads;

/**
 * Installed games that have a hub handler and are not in the library yet. Games of the library (by hub handler id or
 * by name) and handlers already installed for a library game are left out.
 */
export function matchInstalledGames(
  installed: readonly InstalledGame[],
  hub: readonly HubHandler[],
  library: readonly GameSummary[],
  installedHandlers: readonly InstalledHandler[],
): FoundCandidate[] {
  const byName = new Map<string, HubHandler[]>();
  for (const handler of hub) {
    for (const key of nameKeys(handler.gameName)) {
      const list = byName.get(key);
      if (list) list.push(handler);
      else byName.set(key, [handler]);
    }
  }
  const ownedIds = new Set<string>([
    ...library.flatMap((g) => (g.handlerId ? [g.handlerId] : [])),
    ...installedHandlers.flatMap((h) => (h.inLibrary && h.handlerId ? [h.handlerId] : [])),
  ]);
  const ownedNames = new Set(library.flatMap((g) => nameKeys(g.name)));

  const found: FoundCandidate[] = [];
  const seen = new Set<string>();
  for (const game of installed) {
    const keys = nameKeys(game.name);
    if (keys.some((key) => ownedNames.has(key))) continue;
    // Exact name first; the suffix-less name only when nothing matches exactly.
    const exact = (byName.get(keys[0] ?? '') ?? []).filter((h) => !ownedIds.has(h.id));
    const loose = keys.slice(1).flatMap((key) => byName.get(key) ?? []).filter((h) => !ownedIds.has(h.id));
    const pool = exact.length ? exact : loose;
    const handler = [...pool].sort(better)[0];
    if (!handler || seen.has(handler.id)) continue;
    if (nameKeys(handler.gameName).some((key) => ownedNames.has(key))) continue;
    seen.add(handler.id);
    found.push({ key: `${game.store}:${game.storeId}`, game, handler });
  }
  return found.sort((a, b) => a.game.name.localeCompare(b.game.name, 'en'));
}

/** Store art of an installed game (Steam header), when the store has a public one. */
export function storeArt(game: InstalledGame): string | undefined {
  return game.store === 'steam' && /^\d+$/.test(game.storeId) ? steamArt(Number(game.storeId), 'header') : undefined;
}

export const STORE_NAMES: Record<InstalledGame['store'], string> = { steam: 'Steam', epic: 'Epic' };
