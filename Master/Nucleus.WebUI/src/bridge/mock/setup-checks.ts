// "Before you launch" checks of the open setup, and the fixes `setup.runCheckAction` runs.
import type { CheckState, SetupCheck } from '../generated/contract.ts';
import type { MockContext } from '../transports/mock.ts';
import { setupFixtureFor } from './fixtures-setup.ts';
import { installHandler } from './handler-install.ts';
import { findHandler, libraryChanged, refreshUpdateAvailable } from './library-model.ts';
import type { MockGame, MockHandler, MockSetup, MockState } from './state.ts';
import { sleep } from './util.ts';

/** Order of the checks: blocking ones first. */
const SEVERITY: Readonly<Record<CheckState, number>> = { blocking: 0, busy: 1, warning: 1, info: 2, ok: 3 };

const check = (id: string, state: CheckState, title: string, detail?: string, actionLabel?: string): SetupCheck => ({
  id,
  state,
  title,
  detail,
  actionLabel,
});

function gameCheck(game: MockGame | undefined): SetupCheck {
  return game?.summary.executableFound
    ? check('gameFound', 'ok', 'Game found', game.executablePath, 'Change')
    : check('gameFound', 'blocking', 'Game not found', `Nucleus cannot find ${game?.executablePath ?? 'the executable'}.`, 'Locate');
}

function handlerCheck(game: MockGame | undefined, handler: MockHandler | undefined): SetupCheck {
  const installed = handler?.version;
  const latest = game?.summary.latestHandlerVersion;
  if (!handler?.info.handlerId) return check('handlerUpToDate', 'ok', 'Handler installed from a file', 'It is not updated from the hub.');
  if (latest === undefined) return check('handlerUpToDate', 'info', 'Handler not checked for updates', `Version ${installed}`, 'Check now');
  if (game?.summary.updateAvailable) {
    const detail = `Version ${installed} installed, version ${latest} on the hub.`;
    return check('handlerUpToDate', 'warning', 'Handler update available', detail, 'Update');
  }
  return check('handlerUpToDate', 'ok', 'Handler is up to date', `Version ${installed}`);
}

function devicesCheck(state: MockState, setup: MockSetup): SetupCheck {
  const players = setup.players.length;
  if (players === 0) return check('devices', 'info', 'No player yet', 'Press a button on a gamepad, or drag a device onto a screen.');
  if (setup.players.some((player) => player.deviceIds.some((deviceId) => deviceId.startsWith('raw')))) {
    const detail = `Once every window is in place, press ${state.settings['hotkeys.lockKey'] ?? 'End'} to give each mouse its own cursor.`;
    return check('devices', 'info', 'Keyboards and mice joined', detail);
  }
  return check('devices', 'ok', players === 1 ? '1 player ready' : `${players} players ready`);
}

/** Checks that only some handlers need. */
function handlerSpecificChecks(state: MockState, setup: MockSetup): SetupCheck[] {
  const { requiresAdmin, mod, closeSteam, savesInOneDrive } = setupFixtureFor(setup.gameId);
  const checks: SetupCheck[] = [];
  if (requiresAdmin) {
    checks.push(
      state.elevated
        ? check('admin', 'ok', 'Running as administrator')
        : check('admin', 'blocking', 'Administrator rights needed', 'This handler needs Nucleus to run as admin.', 'Restart as admin'),
    );
  }
  if (mod) checks.push(check('mod', 'ok', `${mod.name} mod found`, `${mod.file} is in the game folder.`));
  if (closeSteam) {
    checks.push(
      setup.steamClosed
        ? check('steam', 'ok', 'Steam is closed', 'Nucleus keeps it closed until the session ends.')
        : check('steam', 'warning', 'Steam is running', 'Close it so each instance signs in with its own emulated account.', 'Close Steam'),
    );
  }
  if (savesInOneDrive) {
    const detail = "The game saves in Documents: OneDrive may mix up the instances' saves.";
    checks.push(check('oneDrive', 'warning', 'Documents are synced by OneDrive', detail));
  }
  return checks;
}

/** The checks of the open setup, blocking ones first. A check whose fix is running is `busy`. */
export function checksOf(state: MockState, setup: MockSetup): SetupCheck[] {
  const game = state.games.find((candidate) => candidate.summary.id === setup.gameId);
  const checks = [
    gameCheck(game),
    handlerCheck(game, findHandler(state, setup.gameId)),
    ...handlerSpecificChecks(state, setup),
    devicesCheck(state, setup),
  ];
  return checks
    .map((item) => (setup.busyChecks.has(item.id) ? check(item.id, 'busy', item.title, item.detail) : item))
    .sort((a, b) => SEVERITY[a.state] - SEVERITY[b.state]);
}

/** Runs the fix of a check (the host opens pickers or restarts; the mock pretends, after a delay). */
export async function fixCheck(context: MockContext, state: MockState, setup: MockSetup, checkId: string): Promise<void> {
  const game = state.games.find((candidate) => candidate.summary.id === setup.gameId);
  const handler = findHandler(state, setup.gameId);
  switch (checkId) {
    case 'gameFound':
      // Refused before getting here (needsPicker): the UI owns the file picker.
      return;
    case 'handlerUpToDate':
      if (game?.summary.latestHandlerVersion !== undefined && handler?.info.handlerId) {
        await installHandler(context, state, handler.info.handlerId, handler.info, { download: true });
        return;
      }
      await sleep(600);
      if (game) {
        game.summary.latestHandlerVersion = handler?.info.hubVersion;
        refreshUpdateAvailable(state, game);
        libraryChanged(context, game.summary.id);
      }
      return;
    case 'admin':
      await sleep(800);
      console.info('[bridge mock] restart as administrator');
      state.elevated = true;
      return;
    case 'steam':
      await sleep(1100);
      setup.steamClosed = true;
      return;
  }
}
