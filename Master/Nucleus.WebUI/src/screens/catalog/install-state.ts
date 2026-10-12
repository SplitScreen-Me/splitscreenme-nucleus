// What this PC has of a hub handler: installed (and which version), its game in the library, an update.
import type { GameSummary, InstalledHandler } from '@/bridge';
import type { HubHandler } from '@/hub';
import { installedFor } from '@/lib/library';

/**
 * `none`: not installed. `installed`: installed, its game not in the library yet. `library`: installed and its game
 * is in the library. `update`: the hub has a newer version than the installed one.
 */
export type HandlerInstallStatus = 'none' | 'installed' | 'library' | 'update';

export interface HandlerInstallState {
  status: HandlerInstallStatus;
  /** The installed handler, when installed. */
  mine?: InstalledHandler;
  /** Its library game, when in the library. */
  game?: GameSummary;
}

/** Install state of a hub handler, from `handlers.listInstalled` and `library.list`. */
export function handlerInstallState(
  h: Pick<HubHandler, 'id' | 'version'>,
  installed: readonly InstalledHandler[],
  library: readonly GameSummary[],
): HandlerInstallState {
  const mine = installedFor(installed, h.id);
  const game = (mine && library.find((g) => g.id === mine.gameId)) ?? library.find((g) => g.handlerId === h.id);
  if (!mine) return game ? { status: 'library', game } : { status: 'none' };
  if (mine.version !== undefined && h.version > 0 && mine.version < h.version) return { status: 'update', mine, game };
  return { status: mine.inLibrary || game ? 'library' : 'installed', mine, game };
}

/** Install states of every hub handler id, for the catalogue cards. */
export function installStatuses(installed: readonly InstalledHandler[], library: readonly GameSummary[], handlers: readonly HubHandler[]) {
  const byId = new Map<string, HandlerInstallStatus>();
  if (installed.length === 0 && library.length === 0) return byId;
  const wanted = new Set([...installed.map((i) => i.handlerId), ...library.map((g) => g.handlerId)]);
  for (const h of handlers) {
    if (!wanted.has(h.id)) continue;
    const { status } = handlerInstallState(h, installed, library);
    if (status !== 'none') byId.set(h.id, status);
  }
  return byId;
}
