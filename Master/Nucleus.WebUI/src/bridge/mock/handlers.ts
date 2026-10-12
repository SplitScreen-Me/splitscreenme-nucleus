import type { BridgeHandlers } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { conflict, invalidParams } from './failures.ts';
import { HUB_HANDLERS, findHubHandler, unknownHandler, type HandlerInfo } from './fixtures.ts';
import { hubScreenshotUrls } from './fixtures-screenshots.ts';
import { installHandler } from './handler-install.ts';
import { findGame, findHandler, inLibrary, libraryChanged, refreshUpdateAvailable } from './library-model.ts';
import type { MockState } from './state.ts';
import { sleep } from './util.ts';

const normalize = (text: string) => text.toLowerCase().replace(/[^a-z0-9]/g, '');

/** The handler a file holds: a hub handler whose game the file name mentions, else one named after the file. */
function handlerInFile(path: string): HandlerInfo {
  const baseName = path.slice(path.lastIndexOf('\\') + 1).replace(/\.[^.]+$/, '');
  const key = normalize(baseName);
  const known = HUB_HANDLERS.find((info) => key.includes(normalize(info.gameId)) || key.includes(normalize(info.gameName)));
  return known ?? unknownHandler(baseName.replace(/[^\w ]/g, '').trim() || 'Handler', baseName);
}

/**
 * Mock of the `handlers` service: installs and updates with progress events, installs from a file, update checks
 * that flag library games, and hub screenshots. Unknown hub ids install as a generic handler (the mock knows 16).
 */
export function createHandlersMock(context: MockContext, state: MockState): BridgeHandlers['handlers'] {
  return {
    listInstalled: () =>
      state.handlers.map(({ info, version }) => ({
        gameId: info.gameId,
        gameName: info.gameName,
        handlerId: info.handlerId,
        version,
        inLibrary: inLibrary(state, info.gameId),
      })),
    install: ({ handlerId }) => {
      const info = findHubHandler(handlerId) ?? unknownHandler(`Hub ${handlerId}`, `Hub handler ${handlerId}`, handlerId);
      return installHandler(context, state, handlerId, info, { download: true });
    },
    update: ({ gameId }) => {
      const game = findGame(state, gameId);
      const info = findHandler(state, gameId)?.info;
      if (!info?.handlerId) {
        conflict('notFromHub', `The handler of ${game.summary.name} was installed from a file: it cannot be updated from the hub.`);
      }
      return installHandler(context, state, info.handlerId, info, { download: true });
    },
    installFromFile: ({ path, overwrite }) => {
      if (!/\.(nc|zip|js)$/i.test(path)) invalidParams('Pick a handler file: .nc, .zip or .js.');
      const info = handlerInFile(path);
      if (!overwrite && findHandler(state, info.gameId)) {
        conflict('alreadyInstalled', `A handler for ${info.gameName} is already installed.`);
      }
      return installHandler(context, state, path, info, { download: false });
    },
    checkUpdates: async () => {
      await sleep(700);
      const changed = state.games.filter((game) => {
        const before = { ...game.summary };
        game.summary.latestHandlerVersion = findHandler(state, game.summary.id)?.info.hubVersion;
        refreshUpdateAvailable(state, game);
        return before.updateAvailable !== game.summary.updateAvailable || before.latestHandlerVersion !== game.summary.latestHandlerVersion;
      });
      if (changed.length > 0) libraryChanged(context, ...changed.map((game) => game.summary.id));
    },
    screenshots: async ({ handlerId }) => {
      await sleep(200);
      return hubScreenshotUrls(handlerId);
    },
  };
}
