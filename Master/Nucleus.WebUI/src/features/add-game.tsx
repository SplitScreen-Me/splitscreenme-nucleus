// Adding a game to the library from its executable, and pointing a moved game at its executable again.
// Shared by the library ("Add a game", missing games), Get games (after an install), Found on this PC, file drops
// and the lobby's "Game found" check.
import { createContext, useCallback, useContext, useMemo, useRef, useState, type ReactNode } from 'react';
import { bridge, type GameSummary, type HandlerCandidate, type InstallResult } from '@/bridge';
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

/** Provides {@link useAddGame}. Renders the handler chooser when an executable matches several handlers. */
export function AddGameProvider({ children }: { children: ReactNode }) {
  const run = useRun();
  const toast = useToast();
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
      const label = names.length ? `${gameName ?? 'Game'} executable (${names.join(', ')})` : 'Game executable';
      const title = gameName ? `Where is ${gameName}? Pick ${names[0] ?? 'its executable'}` : 'Pick the game executable';
      const path = await run(() =>
        bridge.dialogs.pickFile({ title, filters: [{ name: label, extensions: ['exe'] }], initialDirectory }, { timeoutMs: 0 }),
      );
      return path ?? null;
    },
    [run],
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
      if (game && !quiet) toast(`${game.name} is in your library`);
      return game;
    },
    [run, chooseHandler, toast],
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
        if (done) toast(`${game.name} found. Ready to play.`);
        return done ?? false;
      },
      afterInstall: async (result) => {
        if (result.inLibrary) return null;
        const names = result.executableNames.length ? result.executableNames : result.executableName ? [result.executableName] : [];
        const path = await pickExecutable({ gameName: result.gameName, executableNames: names, initialDirectory: result.suggestedFolder });
        return path ? addGameAt(path, { handlerGameId: result.gameId }) : null;
      },
    }),
    [pickExecutable, addGameAt, run, toast],
  );

  return (
    <AddGameContext value={api}>
      {children}
      <Dialog open={choice !== null} onClose={() => settle(null)} title="Which game is it?" icon="library" className="w-[min(560px,90vw)]">
        {choice && (
          <div className="grid gap-4">
            <p className="text-15 text-ink-2">
              Several installed handlers run <b className="font-[650] text-ink">{choice.fileName}</b>. Pick the game this executable belongs to.
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
                Cancel
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
