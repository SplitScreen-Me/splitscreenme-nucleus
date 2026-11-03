import assert from 'node:assert/strict';
import { test } from 'node:test';
import {
  afterInstallStep,
  contextMismatchOf,
  defaultPicks,
  duplicatedGames,
  folderOf,
  pickAll,
  togglePick,
} from '../../lib/game-search-core.ts';
import { BridgeClient } from '../client.ts';
import { BridgeError } from '../errors.ts';
import type { FolderSearchCandidate, FolderSearchProgress } from '../generated/contract.ts';
import { createMockBridgeTransport } from '../mock/index.ts';
import { createBridgeProxy } from '../proxy.ts';

const candidate = (gameId: string, executablePath: string, inLibrary = false): FolderSearchCandidate => ({
  gameId,
  gameName: gameId,
  handlerFile: `${gameId}.js`,
  executablePath,
  installFolder: folderOf(executablePath) ?? '',
  inLibrary,
});

const found = [
  candidate('Alpha', 'D:\\Games\\Alpha\\alpha.exe'),
  candidate('Beta', 'D:\\Games\\Beta\\beta.exe', true),
  candidate('Gamma', 'D:\\Games\\Gamma\\gamma.exe'),
  candidate('gamma', 'E:\\Backup\\Gamma\\gamma.exe'),
];

test('one copy of each game not in the library is picked at first', () => {
  assert.deepEqual([...defaultPicks(found)], ['D:\\Games\\Alpha\\alpha.exe', 'D:\\Games\\Gamma\\gamma.exe']);
  assert.deepEqual([...duplicatedGames(found)], ['gamma']);
  assert.deepEqual([...pickAll(found, false)], []);
  assert.deepEqual(pickAll(found, true), defaultPicks(found));
});

test('picking a copy of a game unpicks its other copies; library games stay unpicked', () => {
  const picks = defaultPicks(found);

  const other = togglePick(picks, found, 'E:\\Backup\\Gamma\\gamma.exe', true);
  assert.deepEqual([...other].sort(), ['D:\\Games\\Alpha\\alpha.exe', 'E:\\Backup\\Gamma\\gamma.exe']);

  assert.deepEqual([...togglePick(other, found, 'D:\\Games\\Alpha\\alpha.exe', false)], ['E:\\Backup\\Gamma\\gamma.exe']);
  assert.deepEqual(togglePick(picks, found, 'D:\\Games\\Beta\\beta.exe', true), picks);
  assert.deepEqual(togglePick(picks, found, 'X:\\unknown.exe', true), picks);
});

test('after a handler install: ask once, search when on with a folder, else pick', () => {
  const folder = (exists: boolean) => ({ path: 'D:\\Games', exists });

  assert.equal(afterInstallStep({ folders: [] }), 'ask');
  assert.equal(afterInstallStep({ autoSearch: true, folders: [folder(true)] }), 'search');
  assert.equal(afterInstallStep({ autoSearch: true, folders: [folder(false)] }), 'pick');
  assert.equal(afterInstallStep({ autoSearch: true, folders: [] }), 'pick');
  assert.equal(afterInstallStep({ autoSearch: false, folders: [folder(true)] }), 'pick');
  assert.equal(afterInstallStep(null), 'pick');
});

test('the context refusal of library.addGame is read from the error data', () => {
  const data = {
    reason: 'contextMissing',
    executable: 'game.exe',
    gameId: 'A',
    missing: ['data.pak'],
    excluded: [],
    handlers: [
      { gameId: 'A', name: 'Alpha', file: 'Alpha.js', missing: ['data.pak'], excluded: [] },
      { gameId: 'B', name: '', file: 'B.js', missing: [], excluded: ['packs'] },
    ],
  };

  const read = contextMismatchOf(new BridgeError('notFound', 'refused', data));
  assert.equal(read?.handlers.length, 2);
  assert.equal(read?.handlers[1]?.name, 'B'); // a handler without a name shows its id
  assert.deepEqual(read?.handlers[1]?.excluded, ['packs']);

  assert.equal(contextMismatchOf(new BridgeError('notFound', 'no handler', { reason: 'noHandler', executable: 'game.exe' })), null);
  assert.equal(contextMismatchOf(new BridgeError('conflict', 'refused', data)), null);
  assert.equal(contextMismatchOf(new BridgeError('notFound', 'refused', { ...data, handlers: [] })), null);
  assert.equal(contextMismatchOf(new Error('boom')), null);
});

test('folderOf cuts the file name of a Windows path', () => {
  assert.equal(folderOf('D:\\Games\\Alpha\\alpha.exe'), 'D:\\Games\\Alpha');
  assert.equal(folderOf('alpha.exe'), undefined);
});

function mockBridge() {
  const client = new BridgeClient({ transport: createMockBridgeTransport() });
  return { client, bridge: createBridgeProxy(client) };
}

test('the mock folder search reports its progress under its id and ends at 100', async () => {
  const { client, bridge } = mockBridge();
  const progress: FolderSearchProgress[] = [];
  client.on('library.searchProgress', (p) => progress.push(p));

  const result = await bridge.library.searchGames({ searchId: 'test-1' });
  await new Promise((resolve) => setTimeout(resolve, 5));

  assert.equal(result.foldersSearched, 2); // F: is unplugged
  assert.ok(result.candidates.some((c) => c.gameId === 'Left 4 Dead 2' && !c.inLibrary));
  assert.ok(result.candidates.some((c) => c.inLibrary));
  assert.equal(result.candidates.filter((c) => c.gameId === 'Left 4 Dead 2').length, 2); // a second copy
  assert.ok(progress.length > 2);
  assert.ok(progress.every((p) => p.searchId === 'test-1'));
  assert.deepEqual(progress.at(-1), { searchId: 'test-1', percent: 100, found: result.candidates.length });

  const one = await bridge.library.searchGames({ searchId: 'test-2', gameId: 'Left 4 Dead 2' });
  assert.equal(one.candidates.length, 1);
});

test('the mock game folders: never asked at first, unplugged drives refused, duplicates kept once', async () => {
  const { bridge } = mockBridge();

  const start = await bridge.settings.getGameFolders();
  assert.equal(start.autoSearch, undefined);
  assert.deepEqual(
    start.folders.map((f) => f.exists),
    [true, true, false],
  );

  await assert.rejects(bridge.settings.addGameFolder({ path: 'G:\\Nope' }), (e) => BridgeError.is(e, 'notFound'));
  const added = await bridge.settings.addGameFolder({ path: 'C:\\Games\\' });
  assert.ok(added.folders.some((f) => f.path === 'C:\\Games'));
  assert.equal((await bridge.settings.addGameFolder({ path: 'c:\\games' })).folders.length, added.folders.length);
  assert.equal((await bridge.settings.removeGameFolder({ path: 'C:\\GAMES' })).folders.length, added.folders.length - 1);

  await bridge.settings.setValues({ values: { 'misc.autoSearchGame': 'true' } });
  assert.equal((await bridge.settings.getGameFolders()).autoSearch, true);
});

test('the mock add-game refusal can be forced to show the context report', async () => {
  const { bridge } = mockBridge();
  const storage = globalThis.localStorage;
  const values: Record<string, string> = { 'nucleus.mock.addGame': 'context' };
  Object.defineProperty(globalThis, 'localStorage', { value: { getItem: (k: string) => values[k] ?? null }, configurable: true });
  try {
    const error = await bridge.library.addGame({ executablePath: 'D:\\Games\\ELDEN RING\\eldenring.exe' }).catch((e: unknown) => e);
    const data = contextMismatchOf(error);
    assert.equal(data?.executable, 'eldenring.exe');
    assert.equal(data?.handlers.length, 2);
  } finally {
    Object.defineProperty(globalThis, 'localStorage', { value: storage, configurable: true });
  }
  assert.deepEqual(await bridge.handlers.listErrors(), []);
});
