import { librarySortValues, type BridgeHandlers } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { invalidParams } from './failures.ts';
import { menuItems, runMenuAction } from './library-menu.ts';
import {
  addGame,
  downloadArtwork,
  findExecutable,
  findGame,
  gameDetails,
  installedGames,
  libraryChanged,
  removeGame,
  setExecutable,
} from './library-model.ts';
import { refreshSetupOf } from './setup-snapshot.ts';
import type { MockState } from './state.ts';
import { sleep } from './util.ts';

/**
 * Mock of the `library` service: 13 games with artwork, favourites, play time and one missing executable; the
 * per-game menu; adding games (several handlers can match an executable, for example `javaw.exe`); store scans.
 */
export function createLibraryMock(context: MockContext, state: MockState): BridgeHandlers['library'] {
  return {
    list: () => state.games.map((game) => game.summary),
    get: ({ gameId }) => gameDetails(state, findGame(state, gameId)),
    setFavorite: ({ gameId, favorite }) => {
      findGame(state, gameId).summary.favorite = favorite;
      libraryChanged(context, gameId);
    },
    getMenu: ({ gameId }) => ({ gameId, items: menuItems(state, findGame(state, gameId)) }),
    runMenuAction: (request) => runMenuAction(context, state, request),
    addGame: async (request) => {
      await sleep(300);
      const result = addGame(context, state, request);
      if (result.game) refreshSetupOf(context, state, result.game.id);
      return result;
    },
    setExecutable: (request) => {
      setExecutable(context, state, request);
      refreshSetupOf(context, state, request.gameId);
    },
    remove: ({ gameId }) => removeGame(context, state, gameId),
    scanInstalledGames: async () => {
      await sleep(600);
      return installedGames();
    },
    findExecutable: async (request) => {
      await sleep(400);
      return findExecutable(state, request);
    },
    downloadArtwork: ({ gameId }) => downloadArtwork(context, state, gameId),
    reload: async () => {
      await sleep(500);
      libraryChanged(context);
    },
    getPreferences: () => state.preferences,
    setPreferences: ({ sort, favoritesOnly, filterGamepads, filterKeyboard, filterMultipleKeyboards }) => {
      if (!librarySortValues.includes(sort)) invalidParams(`Unknown sort order '${sort}'.`);
      state.preferences = { sort, favoritesOnly, filterGamepads, filterKeyboard, filterMultipleKeyboards };
    },
  };
}
