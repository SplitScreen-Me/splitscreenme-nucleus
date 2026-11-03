// Adding a game to the library from its executable, and pointing a moved game at its executable again.
// Shared by the library ("Add a game", missing games, "Search my games"), Get games (after an install), Found on
// this PC, file drops and the lobby's "Game found" check. After a handler install, the game folders are searched
// first when the user turned that on (Nucleus 2.5, asked once). An executable whose folder the handlers refuse
// (Game.ExecutableContext) gets a report of what is missing instead of a toast.
import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react';
import { bridge, type AddGameResult, type GameSummary, type HandlerCandidate, type InstallResult } from '@/bridge';
import { useAddGameFolder } from '@/features/game-folders';
import { useI18n } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { afterInstallStep, contextMismatchOf, folderOf } from '@/lib/game-search';
import { useToast } from '@/ui';
import {
  AutoSearchAskDialog,
  ContextReportDialog,
  FindGameDialog,
  HandlerChooserDialog,
  type AutoSearchAnswer,
  type ContextReport,
  type FindOutcome,
} from './add-game-dialogs';

export interface PickExecutableOptions {
  /** Game name, for the picker title. */
  gameName?: string;
  /** Executable names the handler accepts (shown in the picker's file type label). */
  executableNames?: readonly string[];
  /** Folder the picker starts in (for example `InstallResult.suggestedFolder`). */
  initialDirectory?: string;
}

export interface AddGameApi {
  /** Asks for the executable, then adds the game (choosing the handler when several match). Null when cancelled or refused. */
  addGame(options?: PickExecutableOptions & { handlerGameId?: string }): Promise<GameSummary | null>;
  /** Adds the game of a known executable path (choosing the handler when several match). */
  addGameAt(executablePath: string, options?: { handlerGameId?: string; quiet?: boolean }): Promise<GameSummary | null>;
  /** Asks for the executable of a game whose file moved, then relinks it. True when done. */
  locateGame(game: { id: string; name: string }, options?: Omit<PickExecutableOptions, 'gameName'>): Promise<boolean>;
  /**
   * After a handler install: adds its game when it is not in the library yet. The game folders are searched first when
   * the automatic search is on (asked once); else the picker opens, filtered and started in the suggested folder.
   */
  afterInstall(result: InstallResult): Promise<GameSummary | null>;
  /** Opens the executable picker only. Null when cancelled. */
  pickExecutable(options?: PickExecutableOptions): Promise<string | null>;
}

const AddGameContext = createContext<AddGameApi | null>(null);

/** One dialog of the flows at a time, each answered through a promise. */
type FlowDialog =
  | { kind: 'choose'; fileName: string; candidates: HandlerCandidate[] }
  | { kind: 'context'; report: ContextReport }
  | { kind: 'ask'; game: string; hasFolders: boolean }
  | { kind: 'find'; gameId: string; gameName: string };

/** Provides {@link useAddGame} and renders the dialogs of its flows. */
export function AddGameProvider({ children }: { children: ReactNode }) {
  const run = useRun();
  const toast = useToast();
  const addFolder = useAddGameFolder();
  const { t, format } = useI18n();
  const [dialog, setDialog] = useState<FlowDialog | null>(null);
  const pending = useRef<{ resolve: (value: unknown) => void; dismissed: unknown } | null>(null);

  /** Shows a dialog; resolves with its answer, or with `dismissed` when another dialog replaces it. */
  const ask = useCallback(
    <A,>(next: FlowDialog, dismissed: A) =>
      new Promise<A>((resolve) => {
        const previous = pending.current;
        pending.current = { resolve: resolve as (value: unknown) => void, dismissed };
        previous?.resolve(previous.dismissed);
        setDialog(next);
      }),
    [],
  );
  const settle = useCallback((value: unknown) => {
    const current = pending.current;
    pending.current = null;
    setDialog(null);
    current?.resolve(value);
  }, []);

  const pickExecutable = useCallback(
    async ({ gameName, executableNames = [], initialDirectory }: PickExecutableOptions = {}) => {
      const names = [...new Set(executableNames.filter(Boolean))];
      const files = format.list(names, 'unit');
      const label = !names.length
        ? t('features.addGame.picker.filter')
        : gameName
          ? t('features.addGame.picker.filterNamed', { game: gameName, files })
          : t('features.addGame.picker.filterFiles', { files });
      const title = !gameName
        ? t('features.addGame.picker.title')
        : names[0]
          ? t('features.addGame.picker.titleWithFile', { game: gameName, file: names[0] })
          : t('features.addGame.picker.titleNamed', { game: gameName });
      const path = await run(() =>
        bridge.dialogs.pickFile({ title, filters: [{ name: label, extensions: ['exe'] }], initialDirectory }, { timeoutMs: 0 }),
      );
      return path ?? null;
    },
    [run, t, format],
  );

  /**
   * Runs an add or relink call. A refusal for the executable's folder shows the report; "Pick another file" opens the
   * picker in that folder, then `retry` runs with the new path. Other failures are toasted. Null when not done.
   */
  const withReport = useCallback(
    async <R,>(path: string, call: (path: string) => Promise<R>, retry: (path: string) => Promise<R | null>, gameName?: string): Promise<R | null> => {
      const refusal: { report?: ContextReport } = {};
      const result = await run(() => call(path), {
        onError: (error) => {
          const data = contextMismatchOf(error);
          if (data) refusal.report = { path, data };
          return data !== null;
        },
      });
      if (result !== undefined) return result;
      const refused = refusal.report;
      if (!refused) return null;
      if (!(await ask<boolean>({ kind: 'context', report: refused }, false))) return null;
      const single = refused.data.handlers.length === 1 ? refused.data.handlers[0]?.name : undefined;
      const next = await pickExecutable({
        gameName: gameName ?? single,
        executableNames: refused.data.executable ? [refused.data.executable] : [],
        initialDirectory: folderOf(path),
      });
      return next ? retry(next) : null;
    },
    [run, ask, pickExecutable],
  );

  const addGameAt: AddGameApi['addGameAt'] = useCallback<AddGameApi['addGameAt']>(
    async (executablePath, { handlerGameId, quiet = false } = {}) => {
      // A file picked again after a refusal goes through the whole flow (chooser included); this call says "added".
      const add = (path: string, handler: string | undefined): Promise<AddGameResult | null> =>
        withReport(
          path,
          (target) => bridge.library.addGame({ executablePath: target, handlerGameId: handler }),
          (target) => addGameAt(target, { handlerGameId: handler, quiet: true }).then((game) => (game ? { game, candidates: [] } : null)),
        );
      let result = await add(executablePath, handlerGameId);
      if (result && !result.game && result.candidates.length > 0) {
        const fileName = executablePath.slice(executablePath.lastIndexOf('\\') + 1);
        const picked = await ask<string | null>({ kind: 'choose', fileName, candidates: result.candidates }, null);
        if (!picked) return null;
        result = await add(executablePath, picked);
      }
      const game = result?.game ?? null;
      if (game && !quiet) toast(t('features.addGame.added', { game: game.name }));
      return game;
    },
    [withReport, ask, toast, t],
  );

  const locateGame: AddGameApi['locateGame'] = useCallback<AddGameApi['locateGame']>(
    async (game, options = {}) => {
      const path = await pickExecutable({ ...options, gameName: game.name });
      if (!path) return false;
      const relinkAt = (target: string): Promise<boolean | null> =>
        withReport(target, (file) => bridge.library.setExecutable({ gameId: game.id, executablePath: file }).then(() => true), relinkAt, game.name);
      const done = await relinkAt(path);
      if (done === true) toast(t('features.addGame.located', { game: game.name }));
      return done === true;
    },
    [pickExecutable, withReport, toast, t],
  );

  /** Saves the automatic search choice (`misc.autoSearchGame`). */
  const saveAutoSearch = useCallback(
    (on: boolean) => run(() => bridge.settings.setValues({ values: { 'misc.autoSearchGame': on ? 'true' : 'false' } })),
    [run],
  );

  const afterInstall = useCallback<AddGameApi['afterInstall']>(
    async (result) => {
      if (result.inLibrary) return null;
      const names = result.executableNames.length ? result.executableNames : result.executableName ? [result.executableName] : [];
      let initialDirectory = result.suggestedFolder;
      const folders = await bridge.settings.getGameFolders().catch(() => null);
      let step = afterInstallStep(folders);

      if (step === 'ask' && folders) {
        const hasFolders = folders.folders.some((f) => f.exists);
        const answer = await ask<AutoSearchAnswer>({ kind: 'ask', game: result.gameName, hasFolders }, 'later');
        if (answer === 'later') step = 'pick';
        else {
          await saveAutoSearch(answer === 'on');
          if (answer === 'off') step = 'pick';
          else {
            const current = hasFolders ? folders : await addFolder(initialDirectory);
            step = current?.folders.some((f) => f.exists) ? 'search' : 'pick';
          }
        }
      }

      if (step === 'search') {
        const outcome = await ask<FindOutcome>({ kind: 'find', gameId: result.gameId, gameName: result.gameName }, { kind: 'cancel' });
        if (outcome.kind === 'cancel') return null;
        if (outcome.kind === 'add') return addGameAt(outcome.candidate.executablePath, { handlerGameId: result.gameId });
        if (outcome.folder) initialDirectory = outcome.folder;
        else if (outcome.notFound) toast(t('features.addGame.autoSearch.notFound', { game: result.gameName }), { icon: 'info', durationMs: 4200 });
      }

      const path = await pickExecutable({ gameName: result.gameName, executableNames: names, initialDirectory });
      return path ? addGameAt(path, { handlerGameId: result.gameId }) : null;
    },
    [ask, saveAutoSearch, addFolder, addGameAt, pickExecutable, toast, t],
  );

  const api = useMemo<AddGameApi>(
    () => ({
      pickExecutable,
      addGameAt,
      addGame: async ({ handlerGameId, ...pick } = {}) => {
        const path = await pickExecutable(pick);
        return path ? addGameAt(path, { handlerGameId }) : null;
      },
      locateGame,
      afterInstall,
    }),
    [pickExecutable, addGameAt, locateGame, afterInstall],
  );

  return (
    <AddGameContext value={api}>
      {children}
      <HandlerChooserDialog choice={dialog?.kind === 'choose' ? dialog : null} onSettle={settle} />
      <ContextReportDialog report={dialog?.kind === 'context' ? dialog.report : null} onSettle={settle} />
      <AutoSearchAskDialog ask={dialog?.kind === 'ask' ? dialog : null} onSettle={settle} />
      <FindGameDialog find={dialog?.kind === 'find' ? dialog : null} onSettle={settle} />
    </AddGameContext>
  );
}

/** Add-game and locate flows. Must be used under {@link AddGameProvider} (mounted by the shell). */
export function useAddGame(): AddGameApi {
  const api = useContext(AddGameContext);
  if (!api) throw new Error('useAddGame must be used inside <AddGameProvider>.');
  return api;
}
