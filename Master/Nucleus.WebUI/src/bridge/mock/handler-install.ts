// Installing a handler (from the hub or a file): `handlers.progress` stage by stage, then the installed handlers
// and the library updated, with `handlers.changed` and `library.changed`.
import type { HandlerInstallStage, HandlerProgress, InstallResult } from '../generated/contract.ts';
import { BridgeError } from '../errors.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict } from './failures.ts';
import { executableNamesOf, type HandlerInfo } from './fixtures.ts';
import { findHandler, inLibrary, libraryChanged, refreshUpdateAvailable } from './library-model.ts';
import type { MockState } from './state.ts';
import { sleep } from './util.ts';

const STEP_MS = 150;
const DOWNLOAD_STEPS = [0, 20, 45, 70, 90, 100];
const EXTRACT_STEPS = [0, 50, 100];

/** Emits the progress of one stage, step by step. `about` names the source and, for updates, the library game. */
async function report(
  context: MockContext,
  about: Pick<HandlerProgress, 'source' | 'gameId'>,
  stage: HandlerInstallStage,
  steps: readonly number[],
): Promise<void> {
  for (const percent of steps) {
    context.emit('handlers.progress', { ...about, stage, percent });
    await sleep(STEP_MS);
  }
}

/** Records an installed handler and updates its library game. */
function completeInstall(context: MockContext, state: MockState, info: HandlerInfo): InstallResult {
  const installed = findHandler(state, info.gameId);
  if (installed) Object.assign(installed, { info, version: info.hubVersion });
  else state.handlers.push({ info, version: info.hubVersion });
  const game = state.games.find((candidate) => candidate.summary.id === info.gameId);
  if (game) {
    Object.assign(game.summary, { handlerVersion: info.hubVersion, latestHandlerVersion: info.hubVersion });
    refreshUpdateAvailable(state, game);
    libraryChanged(context, info.gameId);
  }
  context.emit('handlers.changed', { gameIds: [info.gameId] });
  return {
    gameId: info.gameId,
    gameName: info.gameName,
    executableName: info.executableName,
    executableNames: executableNamesOf(info),
    inLibrary: game !== undefined,
    suggestedFolder: !game && info.steamAppId ? info.installFolder : undefined,
  };
}

/**
 * Installs a handler, downloading it first when it comes from the hub; progress names the library game when the
 * install updates one. One install per source at a time (`conflict`, reason `installInProgress`). Fails with
 * `unavailable` when the browser is offline.
 */
export async function installHandler(
  context: MockContext,
  state: MockState,
  source: string,
  info: HandlerInfo,
  { download }: { download: boolean },
): Promise<InstallResult> {
  if (state.installing.has(source)) conflict('installInProgress', `${info.gameName} is already being installed.`);
  const about = { source, gameId: inLibrary(state, info.gameId) ? info.gameId : undefined };
  state.installing.add(source);
  try {
    if (download) {
      if (globalThis.navigator?.onLine === false) {
        const error = 'The hub cannot be reached: check your connection.';
        context.emit('handlers.progress', { ...about, stage: 'failed', error });
        throw new BridgeError('unavailable', error);
      }
      await report(context, about, 'downloading', DOWNLOAD_STEPS);
    }
    await report(context, about, 'extracting', EXTRACT_STEPS);
    const result = completeInstall(context, state, info);
    context.emit('handlers.progress', { ...about, stage: 'done', percent: 100 });
    return result;
  } finally {
    state.installing.delete(source);
  }
}
