// Adding a game to the library from its executable, and pointing a moved game at its executable again.
// Shared by the library ("Add a game", missing games), Get games (after an install), Found on this PC, file drops
// and the lobby's "Game found" check.
import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react';
import { bridge, type GameSummary, type HandlerCandidate, type InstallResult } from '@/bridge';
import { T, useI18n, type RichTags } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { Button, Dialog, Icon, useToast } from '@/ui';

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
  /** After a handler install: adds its game when it is not in the library yet (picker filtered and started in the suggested folder). */
  afterInstall(result: InstallResult): Promise<GameSummary | null>;
  /** Opens the executable picker only. Null when cancelled. */
  pickExecutable(options?: PickExecutableOptions): Promise<string | null>;
}

const AddGameContext = createContext<AddGameApi | null>(null);

const FILE_NAME_TAG: RichTags = { b: (children) => <b className="font-[650] text-ink">{children}</b> };

/** Provides {@link useAddGame}. Renders the handler chooser when an executable matches several handlers. */
export function AddGameProvider({ children }: { children: ReactNode }) {
  const run = useRun();
  const toast = useToast();
  const { t, format } = useI18n();
  const [choice, setChoice] = useState<{ fileName: string; candidates: HandlerCandidate[] } | null>(null);
  const resolveChoice = useRef<((gameId: string | null) => void) | null>(null);

  const chooseHandler = useCallback(
    (fileName: string, candidates: HandlerCandidate[]) =>
      new Promise<string | null>((resolve) => {
        resolveChoice.current?.(null);
        resolveChoice.current = resolve;
        setChoice({ fileName, candidates });
      }),
    [],
  );
  const settle = (gameId: string | null) => {
    resolveChoice.current?.(gameId);
    resolveChoice.current = null;
    setChoice(null);
  };

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

  const addGameAt = useCallback<AddGameApi['addGameAt']>(
    async (executablePath, { handlerGameId, quiet = false } = {}) => {
      let result = await run(() => bridge.library.addGame({ executablePath, handlerGameId }));
      if (result && !result.game && result.candidates.length > 0) {
        const fileName = executablePath.slice(executablePath.lastIndexOf('\\') + 1);
        const picked = await chooseHandler(fileName, result.candidates);
        if (!picked) return null;
        result = await run(() => bridge.library.addGame({ executablePath, handlerGameId: picked }));
      }
      const game = result?.game ?? null;
      if (game && !quiet) toast(t('features.addGame.added', { game: game.name }));
      return game;
    },
    [run, chooseHandler, toast, t],
  );

  const api = useMemo<AddGameApi>(
    () => ({
      pickExecutable,
      addGameAt,
      addGame: async ({ handlerGameId, ...pick } = {}) => {
        const path = await pickExecutable(pick);
        return path ? addGameAt(path, { handlerGameId }) : null;
      },
      locateGame: async (game, options = {}) => {
        const path = await pickExecutable({ ...options, gameName: game.name });
        if (!path) return false;
        const done = await run(() => bridge.library.setExecutable({ gameId: game.id, executablePath: path }).then(() => true));
        if (done) toast(t('features.addGame.located', { game: game.name }));
        return done ?? false;
      },
      afterInstall: async (result) => {
        if (result.inLibrary) return null;
        const names = result.executableNames.length ? result.executableNames : result.executableName ? [result.executableName] : [];
        const path = await pickExecutable({ gameName: result.gameName, executableNames: names, initialDirectory: result.suggestedFolder });
        return path ? addGameAt(path, { handlerGameId: result.gameId }) : null;
      },
    }),
    [pickExecutable, addGameAt, run, toast, t],
  );

  return (
    <AddGameContext value={api}>
      {children}
      <Dialog open={choice !== null} onClose={() => settle(null)} title={t('features.addGame.chooser.title')} icon="library" className="w-[min(560px,90vw)]">
        {choice && (
          <div className="grid gap-4">
            <p className="text-15 text-ink-2">
              <T k="features.addGame.chooser.message" values={{ file: choice.fileName }} tags={FILE_NAME_TAG} />
            </p>
            <ul className="m-0 grid list-none gap-1.5 p-0">
              {choice.candidates.map((c, i) => (
                <li key={c.gameId}>
                  <Button
                    variant="default"
                    className="w-full justify-start"
                    onClick={() => settle(c.gameId)}
                    data-nav
                    autoFocus={i === 0}
                  >
                    <Icon name="library" />
                    <span className="truncate">{c.name}</span>
                  </Button>
                </li>
              ))}
            </ul>
            <div className="flex justify-end">
              <Button variant="ghost" onClick={() => settle(null)} data-nav>
                {t('common.cancel')}
              </Button>
            </div>
          </div>
        )}
      </Dialog>
    </AddGameContext>
  );
}

/** Add-game and locate flows. Must be used under {@link AddGameProvider} (mounted by the shell). */
export function useAddGame(): AddGameApi {
  const api = useContext(AddGameContext);
  if (!api) throw new Error('useAddGame must be used inside <AddGameProvider>.');
  return api;
}
