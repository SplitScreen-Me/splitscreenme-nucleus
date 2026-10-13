// The per-game menu: the items that apply to a game right now, and what running each one does.
import type { FileFilter, GameMenuItem, RunMenuActionRequest } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { invalidParams, notFound } from './failures.ts';
import { STEAM_LANGUAGES } from './fixtures-settings.ts';
import {
  downloadArtwork,
  findGame,
  findHandler,
  libraryChanged,
  refreshUpdateAvailable,
  removeGame,
  setExecutable,
} from './library-model.ts';
import { setUseApiIndex } from './setup-model.ts';
import { publishSetup, refreshSetupOf } from './setup-snapshot.ts';
import type { MockGame, MockState } from './state.ts';

const EXECUTABLE_FILTERS: FileFilter[] = [{ name: 'Game executable', extensions: ['exe'] }];
const ICON_FILTERS: FileFilter[] = [{ name: 'Images', extensions: ['png', 'jpg', 'jpeg', 'ico'] }];

const item = (kind: GameMenuItem['kind'], id: string, label: string, extras: Partial<GameMenuItem> = {}): GameMenuItem => ({
  id,
  kind,
  label,
  choices: [],
  choiceLabels: [],
  danger: false,
  children: [],
  fileFilters: [],
  ...extras,
});

const action = (id: string, label: string, extras?: Partial<GameMenuItem>) => item('action', id, label, extras);
const toggle = (id: string, label: string, checked: boolean, description: string) => item('toggle', id, label, { checked, description });
const separator = (id: string) => item('separator', id, '');

/** Items of a game's menu, in order. */
export function menuItems(state: MockState, game: MockGame): GameMenuItem[] {
  const { summary, menu } = game;
  const backups = game.backups.map((player) =>
    action(`openBackup:${player}`, player, { description: `Opens the save backup of ${player}.` }),
  );
  return [
    ...(findHandler(state, summary.id)?.info.notes ? [action('notes', 'Handler notes', { clientAction: 'showNotes' })] : []),
    action('details', 'Game details', { clientAction: 'showDetails' }),
    separator('separator0'),
    action('openHandler', 'Open handler file', { description: 'Opens the handler in the text editor.' }),
    ...(summary.executableFound ? [action('openExecutableFolder', 'Open game folder')] : []),
    action('changeExecutable', 'Change executable…', {
      description: 'Points the game at its executable after it was moved.',
      clientAction: 'pickFile',
      fileFilters: EXECUTABLE_FILTERS,
    }),
    ...(game.contentFolderExists
      ? [
          action('openContentFolder', 'Open content folder', { description: 'Folder holding the game instances.' }),
          action('deleteContentFolder', 'Delete content folder', {
            danger: true,
            confirm: `Delete the content folder of ${summary.name}? The instances are rebuilt at the next launch.`,
          }),
        ]
      : []),
    ...(backups.length > 0
      ? [
          item('submenu', 'backups', 'Save backups', {
            children: [
              ...backups,
              separator('backupsSeparator'),
              action('deleteBackups', 'Delete every backup', {
                danger: true,
                confirm: `Delete every save backup of ${summary.name}? This cannot be undone.`,
              }),
            ],
          }),
        ]
      : []),
    separator('separator1'),
    toggle('keepInstancesFolder', 'Keep instances folder', menu.keepInstancesFolder, 'Keeps the instance files when the session ends.'),
    toggle('useApiIndex', 'Identify gamepads by API index', game.useApiIndex, 'Places gamepads without waiting for a button press.'),
    toggle('disableProfiles', 'Disable profiles', menu.disableProfiles, 'Hides the saved setups of this game.'),
    ...(summary.handlerId
      ? [toggle('disableHandlerUpdate', 'Disable handler updates', menu.disableHandlerUpdate, 'Stops looking for a newer handler.')]
      : []),
    item('choice', 'steamLanguage', 'Steam language', { choices: [...STEAM_LANGUAGES], value: menu.steamLanguage }),
    separator('separator2'),
    action('changeIcon', 'Change icon…', { clientAction: 'pickFile', fileFilters: ICON_FILTERS }),
    action('downloadArtwork', 'Download cover and screenshots'),
    separator('separator3'),
    action('removeGame', 'Remove from library', {
      danger: true,
      confirm: `Remove ${summary.name} from the library? Its handler stays installed.`,
    }),
  ];
}

function findItem(items: readonly GameMenuItem[], itemId: string): GameMenuItem | undefined {
  for (const candidate of items) {
    const found = candidate.id === itemId ? candidate : findItem(candidate.children, itemId);
    if (found) return found;
  }
  return undefined;
}

/** Flips a toggle and applies what it changes. */
function setToggle(state: MockState, context: MockContext, game: MockGame, itemId: string, checked: boolean): void {
  if (itemId === 'useApiIndex') {
    game.useApiIndex = checked;
    if (state.setup?.gameId === game.summary.id && state.session.phase === null) {
      setUseApiIndex(state.setup, checked);
      publishSetup(context, state, state.setup);
    }
    return;
  }
  if (itemId === 'keepInstancesFolder' || itemId === 'disableProfiles' || itemId === 'disableHandlerUpdate') game.menu[itemId] = checked;
  refreshUpdateAvailable(state, game);
}

/** The picked path of a `pickFile` item, which must match the item's file types. */
function pickedPath(target: GameMenuItem, value: string | undefined): string {
  const extensions = target.fileFilters.flatMap((filter) => filter.extensions);
  const extension = value?.slice(value.lastIndexOf('.') + 1).toLowerCase() ?? '';
  if (!value || !extensions.includes(extension)) invalidParams(`Pick a file of type ${extensions.join(', ')}.`);
  return value;
}

/**
 * Runs a menu item: toggles flip, choices take `value`, `pickFile` actions take the picked path, other actions do
 * their job (folders open in the console). `showNotes` and `showDetails` items need no host call: they do nothing.
 */
export async function runMenuAction(
  context: MockContext,
  state: MockState,
  { gameId, itemId, value }: RunMenuActionRequest,
): Promise<void> {
  const game = findGame(state, gameId);
  const target = findItem(menuItems(state, game), itemId) ?? notFound(`${game.summary.name} has no menu item '${itemId}'.`);
  switch (target.kind) {
    case 'separator':
    case 'submenu':
      return invalidParams(`'${itemId}' is not an item that runs.`);
    case 'toggle':
      setToggle(state, context, game, itemId, !target.checked);
      break;
    case 'choice':
      if (value === undefined || !target.choices.includes(value)) invalidParams(`Pick one of: ${target.choices.join(', ')}.`);
      game.menu.steamLanguage = value;
      break;
    case 'action':
      return runAction(context, state, game, target, value);
  }
  libraryChanged(context, gameId);
}

async function runAction(context: MockContext, state: MockState, game: MockGame, target: GameMenuItem, value?: string): Promise<void> {
  const { id } = game.summary;
  switch (target.id) {
    case 'notes':
    case 'details':
      return;
    case 'changeExecutable':
      setExecutable(context, state, { gameId: id, executablePath: pickedPath(target, value) });
      return refreshSetupOf(context, state, id);
    case 'changeIcon':
      console.info(`[bridge mock] menu: new icon for ${game.summary.name}: ${pickedPath(target, value)}`);
      return;
    case 'removeGame':
      return removeGame(context, state, id);
    case 'downloadArtwork':
      return downloadArtwork(context, state, id);
    case 'deleteContentFolder':
      game.contentFolderExists = false;
      break;
    case 'deleteBackups':
      game.backups = [];
      break;
    default:
      console.info(`[bridge mock] menu: ${target.id} (${game.summary.name})`);
      return;
  }
  libraryChanged(context, id);
}
