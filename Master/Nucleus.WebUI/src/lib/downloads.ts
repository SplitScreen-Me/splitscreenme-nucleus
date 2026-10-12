// Handler downloads: a small client store fed by the install calls this UI makes and by `handlers.progress`.
// The host has no download list; this store is what the Downloads screen, the rail badge and the install buttons
// read. Installs started elsewhere (a setup check that updates the handler) appear as soon as their progress does.
import { useSyncExternalStore } from 'react';
import { bridge, bridgeClient, BridgeError, type HandlerProgress, type InstallResult } from '@/bridge';
import { refusalReason } from './bridge-ui';

export type DownloadStage = 'queued' | 'downloading' | 'extracting' | 'done' | 'failed' | 'cancelled';
export type DownloadKind = 'install' | 'update' | 'file';

export interface DownloadItem {
  /** Local id. */
  id: string;
  kind: DownloadKind;
  /** Hub handler id, or the file path for installs from a file (as in `HandlerProgress.source`). */
  source: string;
  /** Library game being updated, or the game the handler installed. */
  gameId?: string;
  /** Game name, when known (the screens fall back to the library or the hub catalogue). */
  name?: string;
  /** Cover art, when known. */
  coverUrl?: string;
  /** Version being installed, when known. */
  version?: number;
  stage: DownloadStage;
  /** Percentage of the stage, when known. */
  percent?: number;
  error?: string;
  startedAt: number;
  endedAt?: number;
  result?: InstallResult;
  /** Started by this UI, so it can be cancelled (installs from a file cannot). */
  cancellable: boolean;
}

/** Stages of an install still running. */
export const isActiveStage = (stage: DownloadStage) => stage === 'queued' || stage === 'downloading' || stage === 'extracting';

/** Overall progress of an item, 0 to 1 (download is the first 80 %, extraction the rest). */
export function downloadProgress(item: Pick<DownloadItem, 'stage' | 'percent'>): number {
  const p = (item.percent ?? 0) / 100;
  switch (item.stage) {
    case 'queued':
      return 0;
    case 'downloading':
      return 0.8 * p;
    case 'extracting':
      return 0.8 + 0.2 * p;
    default:
      return 1;
  }
}

const MAX_FINISHED = 30;

let items: readonly DownloadItem[] = [];
const listeners = new Set<() => void>();
const controllers = new Map<string, AbortController>();
const running = new Map<string, Promise<InstallResult>>();
/** Sources this UI cancelled: their late progress (the host's final 'failed: Cancelled.') must not come back as a new row. */
const cancelledSources = new Map<string, number>();
const CANCEL_MEMORY_MS = 120_000;
let seq = 0;

function commit(next: readonly DownloadItem[]): void {
  const finished = next.filter((item) => !isActiveStage(item.stage));
  const drop = new Set(finished.slice(0, Math.max(0, finished.length - MAX_FINISHED)).map((item) => item.id));
  items = drop.size ? next.filter((item) => !drop.has(item.id)) : next;
  listeners.forEach((listener) => listener());
}

function patch(id: string, change: Partial<DownloadItem>): void {
  commit(items.map((item) => (item.id === id ? { ...item, ...change } : item)));
}

function add(item: Omit<DownloadItem, 'id' | 'startedAt' | 'stage'>): DownloadItem {
  seq += 1;
  const created: DownloadItem = { ...item, id: `dl-${seq}`, stage: 'queued', startedAt: Date.now() };
  commit([...items, created]);
  return created;
}

/** Applies a `handlers.progress` event to the item it belongs to, or records an install started elsewhere. */
function onProgress(progress: HandlerProgress): void {
  const target =
    items.find((item) => isActiveStage(item.stage) && item.source === progress.source) ??
    (progress.gameId ? items.find((item) => isActiveStage(item.stage) && item.gameId === progress.gameId) : undefined);
  const stage: DownloadStage = progress.stage === 'done' ? 'done' : progress.stage === 'failed' ? 'failed' : progress.stage;
  const ended = stage === 'done' || stage === 'failed';
  if (target) {
    // A UI-started item ends with its call (the result carries the installed game); progress only moves it along.
    if (target.cancellable && ended) {
      if (stage === 'failed' && !controllers.get(target.id)?.signal.aborted) patch(target.id, { error: progress.error });
      return;
    }
    patch(target.id, {
      stage,
      percent: progress.percent,
      error: progress.error,
      gameId: target.gameId ?? progress.gameId,
      endedAt: ended ? Date.now() : undefined,
    });
    return;
  }
  const cancelledKey = [progress.source, progress.gameId].find((key) => key !== undefined && cancelledSources.has(key));
  if (cancelledKey !== undefined) {
    const recent = Date.now() - (cancelledSources.get(cancelledKey) ?? 0) <= CANCEL_MEMORY_MS;
    if (ended || !recent) cancelledSources.delete(cancelledKey);
    if (recent) return;
  }
  if (ended) {
    // The end of an install this store never saw start (or already finished): nothing to show.
    return;
  }
  const item = add({ kind: progress.gameId ? 'update' : 'install', source: progress.source, gameId: progress.gameId, cancellable: false });
  patch(item.id, { stage, percent: progress.percent });
}

bridgeClient.on('handlers.progress', onProgress);

async function track(item: DownloadItem, call: (signal: AbortSignal) => Promise<InstallResult>): Promise<InstallResult> {
  const controller = new AbortController();
  controllers.set(item.id, controller);
  try {
    const result = await call(controller.signal);
    patch(item.id, { stage: 'done', percent: 100, result, gameId: result.gameId, name: item.name ?? result.gameName, endedAt: Date.now() });
    return result;
  } catch (error) {
    const cancelled = BridgeError.is(error, 'cancelled') || controller.signal.aborted;
    if (!cancelled && refusalReason(error) === 'alreadyInstalled') {
      // The caller asks whether to replace it: not a failure to keep in the list.
      commit(items.filter((candidate) => candidate.id !== item.id));
    } else {
      patch(item.id, {
        stage: cancelled ? 'cancelled' : 'failed',
        error: cancelled ? undefined : error instanceof Error ? error.message : String(error),
        endedAt: Date.now(),
      });
    }
    throw error;
  } finally {
    controllers.delete(item.id);
  }
}

function once(key: string, start: () => Promise<InstallResult>): Promise<InstallResult> {
  const pending = running.get(key);
  if (pending) return pending;
  const promise = start().finally(() => running.delete(key));
  running.set(key, promise);
  return promise;
}

/** What the screens know about the handler being installed, shown until the install answers. */
export interface DownloadMeta {
  name?: string;
  coverUrl?: string;
  version?: number;
}

/** Downloads a handler from the hub and installs it. Resolves when installed; rejects like `handlers.install`. */
export function installHandler(handlerId: string, meta: DownloadMeta = {}): Promise<InstallResult> {
  return once(`hub:${handlerId}`, () => {
    cancelledSources.delete(handlerId);
    const item = add({ kind: 'install', source: handlerId, cancellable: true, ...meta });
    return track(item, (signal) => bridge.handlers.install({ handlerId }, { signal, timeoutMs: 0 }));
  });
}

/** Updates the handler of a library game to the hub's version. */
export function updateHandler(gameId: string, meta: DownloadMeta & { handlerId?: string } = {}): Promise<InstallResult> {
  const { handlerId, ...rest } = meta;
  return once(`game:${gameId}`, () => {
    cancelledSources.delete(handlerId ?? gameId);
    const item = add({ kind: 'update', source: handlerId ?? gameId, gameId, cancellable: true, ...rest });
    return track(item, (signal) => bridge.handlers.update({ gameId }, { signal, timeoutMs: 0 }));
  });
}

/** Installs a handler from a .nc, .zip or .js file. Rejects with `conflict` (reason `alreadyInstalled`) unless `overwrite`. */
export function installHandlerFile(path: string, overwrite = false): Promise<InstallResult> {
  return once(`file:${path}`, () => {
    const name = path.slice(Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/')) + 1);
    const item = add({ kind: 'file', source: path, cancellable: false, name });
    return track(item, () => bridge.handlers.installFromFile({ path, overwrite }, { timeoutMs: 0 }));
  });
}

/** Stops an install started by this UI (the call rejects with `cancelled`). */
export function cancelDownload(id: string): void {
  const controller = controllers.get(id);
  if (!controller) return;
  const item = items.find((candidate) => candidate.id === id);
  if (item) {
    cancelledSources.set(item.source, Date.now());
    if (item.gameId) cancelledSources.set(item.gameId, Date.now());
  }
  controller.abort();
  patch(id, { stage: 'cancelled', endedAt: Date.now() });
}

/** Forgets finished, failed and cancelled items. */
export function clearFinishedDownloads(): void {
  commit(items.filter((item) => isActiveStage(item.stage)));
}

const subscribe = (listener: () => void) => {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
};

/** Every download, oldest first. */
export function useDownloads(): readonly DownloadItem[] {
  return useSyncExternalStore(subscribe, () => items);
}

/** Number of installs running or waiting (rail badge). */
export function useActiveDownloadCount(): number {
  return useSyncExternalStore(subscribe, () => items.filter((item) => isActiveStage(item.stage)).length);
}

/** The running install of a hub handler or a library game, if any. */
export function useActiveDownload(match: { handlerId?: string; gameId?: string }): DownloadItem | undefined {
  const all = useDownloads();
  return all.find(
    (item) =>
      isActiveStage(item.stage) &&
      ((match.handlerId !== undefined && item.source === match.handlerId) || (match.gameId !== undefined && item.gameId === match.gameId)),
  );
}
