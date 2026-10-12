import { useCallback, useEffect, useMemo, useRef, useState } from 'react';
import { bridge, useBridgeQuery, type GameSummary } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { useAllHandlers } from '@/hub';
import { useRun, errorMessage, isCancelled } from '@/lib/bridge-ui';
import { updateHandler } from '@/lib/downloads';
import { gameStatus, useInstalledHandlers, useLibrary } from '@/lib/library';
import { useScreenChrome, type Hint } from '@/shell/chrome';
import { Button, ButtonLink, Icon, Pane, Spinner, useToast } from '@/ui';
import { FeaturedGame } from './FeaturedGame';
import { FoundOnPc } from './FoundOnPc';
import { matchInstalledGames } from './found-matching';
import { DetailsDialog, NotesDialog } from './GameInfoDialogs';
import { GameOptionsDialog } from './GameOptionsDialog';
import { HubStrip } from './HubStrip';
import { useLibraryPreferences } from './library-view';
import { YourGames } from './YourGames';

const focusedId = () => (document.activeElement as HTMLElement | null)?.closest<HTMLElement>('[data-id]')?.dataset.id;
const focusGame = (id: string) =>
  requestAnimationFrame(() => document.querySelector<HTMLElement>(`[data-id="${CSS.escape(id)}"]`)?.focus());

export default function LibraryScreen() {
  const { data: games, isPending, isError, error, refetch } = useLibrary();
  const { prefs, update: updatePrefs } = useLibraryPreferences();
  const [attentionOnly, setAttentionOnly] = useState(false);
  const [optionsFor, setOptionsFor] = useState<string | null>(null);
  const [notesFor, setNotesFor] = useState<string | null>(null);
  const [detailsFor, setDetailsFor] = useState<string | null>(null);
  const [adding, setAdding] = useState(false);
  const toast = useToast();
  const run = useRun();
  const { addGame, locateGame } = useAddGame();

  // What "Found on this PC" offers, so the Hub strip doesn't offer it twice (same cached query).
  const scan = useBridgeQuery('library.scanInstalledGames', undefined, { staleTime: Infinity, gcTime: Infinity });
  const { data: hub = [] } = useAllHandlers();
  const { data: installed = [] } = useInstalledHandlers();
  const offered = useMemo(
    () => new Set(matchInstalledGames(scan.data ?? [], hub, games ?? [], installed).map((f) => f.handler.id)),
    [scan.data, hub, games, installed],
  );

  // The featured game is the one played last (or the first one, alphabetically).
  const featured = useMemo(() => {
    if (!games?.length) return undefined;
    const time = (g: GameSummary) => (g.lastPlayedAt ? Date.parse(g.lastPlayedAt) || 0 : 0);
    return [...games].sort((a, b) => time(b) - time(a) || a.name.localeCompare(b.name, 'en'))[0];
  }, [games]);
  const byId = useCallback((id: string | undefined) => games?.find((g) => g.id === id), [games]);

  const runUpdate = async (g: GameSummary | undefined) => {
    if (!g || !g.updateAvailable) return;
    try {
      const result = await updateHandler(g.id, { name: g.name, coverUrl: g.coverUrl, handlerId: g.handlerId, version: g.latestHandlerVersion });
      toast(`${result.gameName} handler updated${g.latestHandlerVersion !== undefined ? ` to v${g.latestHandlerVersion}` : ''}`);
      focusGame(g.id);
    } catch (e) {
      if (!isCancelled(e)) toast(errorMessage(e), { icon: 'warning', durationMs: 4200 });
    }
  };
  const runLocate = async (g: GameSummary | undefined) => {
    if (!g) return;
    // The old path tells which file to look for, and where to start (the picker ignores a folder that is gone).
    const old = (await bridge.library.get({ gameId: g.id }).catch(() => undefined))?.executablePath ?? '';
    const cut = Math.max(old.lastIndexOf('\\'), old.lastIndexOf('/'));
    const executableNames = cut >= 0 && old.slice(cut + 1) ? [old.slice(cut + 1)] : undefined;
    if (await locateGame({ id: g.id, name: g.name }, { executableNames, initialDirectory: cut > 0 ? old.slice(0, cut) : undefined })) {
      focusGame(g.id);
    }
  };
  const runAdd = async () => {
    setAdding(true);
    const game = await addGame();
    setAdding(false);
    if (game) focusGame(game.id);
  };
  const toggleFavorite = (g: GameSummary | undefined) => {
    if (!g) return;
    void run(() => bridge.library.setFavorite({ gameId: g.id, favorite: !g.favorite }), {
      success: g.favorite ? `${g.name} removed from favourites` : `${g.name} added to favourites`,
    });
  };

  const openOptions = () => setOptionsFor(focusedId() ?? featured?.id ?? null);
  const closeOptions = useCallback(() => setOptionsFor(null), []);
  const search: Hint = { button: 'Y', label: 'Search' };
  const menu: Hint = { button: 'Menu', label: 'Menu' };
  const options: Hint = { button: 'X', label: 'Game options', action: openOptions };
  const favourite: Hint = { button: 'View', label: 'Favourite', action: () => toggleFavorite(byId(focusedId())) };
  useScreenChrome([{ label: 'Library' }], {
    base: games?.length ? [{ button: 'A', label: 'Set up session' }, options, search, menu] : [{ button: 'A', label: 'Select' }, search, menu],
    byFocus: {
      setup: [{ button: 'A', label: 'Set up session' }, options, search, menu],
      options: [{ button: 'A', label: 'Open game options' }, search],
      game: [{ button: 'A', label: 'Set up session' }, options, favourite, search],
      update: [
        { button: 'A', label: 'Set up session' },
        { button: 'X', label: 'Update handler', action: () => void runUpdate(byId(focusedId())) },
        favourite,
        search,
      ],
      missing: [{ button: 'A', label: 'Locate game' }, options, favourite, search],
      add: [{ button: 'A', label: 'Add a game' }, search],
      found: [{ button: 'A', label: 'Add selected games' }, search],
      'found-item': [{ button: 'A', label: 'Select or skip game' }, search],
      'found-scan': [{ button: 'A', label: 'Scan again' }, search],
      chip: [{ button: 'A', label: 'Filter' }, search],
      hub: [{ button: 'A', label: 'View handler' }, search],
    },
  });

  // Land on the featured game's main action, like a console home screen.
  const landed = useRef(false);
  useEffect(() => {
    if (landed.current || !games) return;
    landed.current = true;
    requestAnimationFrame(() => document.querySelector<HTMLElement>('[data-autofocus]')?.focus({ preventScroll: true }));
  }, [games]);

  if (isPending) {
    return (
      <Pane className="grid h-full place-items-center text-small text-ink-2">
        <span className="inline-flex items-center gap-2.5" role="status">
          <Spinner /> Loading your library
        </span>
      </Pane>
    );
  }
  if (isError || !games) {
    return (
      <Pane className="grid h-full place-items-center p-8 text-center">
        <div className="grid justify-items-center gap-3">
          <Icon name="warning" className="size-7 text-warn" />
          <h1 className="type-h3">Your library could not be read</h1>
          <p className="max-w-[52ch] text-small text-ink-2">{errorMessage(error)}</p>
          <Button variant="ghost" onClick={() => void refetch()} data-nav>
            <Icon name="refresh" />
            Try again
          </Button>
        </div>
      </Pane>
    );
  }

  return (
    <div className="grid content-start gap-gutter grid-cols-[minmax(0,1fr)_344px] min-[1321px]:grid-cols-[minmax(0,1fr)_392px]">
      {featured ? (
        <FeaturedGame
          game={featured}
          onOptions={() => setOptionsFor(featured.id)}
          onLocate={() => void runLocate(featured)}
          onUpdate={() => void runUpdate(featured)}
        />
      ) : (
        <EmptyLibrary adding={adding} onAddGame={() => void runAdd()} />
      )}
      <FoundOnPc library={games} onAdded={focusGame} />
      {games.length > 0 && (
        <YourGames
          games={games}
          prefs={prefs}
          onPrefs={updatePrefs}
          attentionOnly={attentionOnly}
          onAttentionOnly={setAttentionOnly}
          onAddGame={() => void runAdd()}
          adding={adding}
          onLocate={(g) => void runLocate(g)}
          onUpdate={(g) => void runUpdate(g)}
          onFavorite={toggleFavorite}
        />
      )}
      <HubStrip owned={games} offered={offered} />
      <GameOptionsDialog
        gameId={optionsFor}
        games={games}
        onClose={closeOptions}
        onShowNotes={setNotesFor}
        onShowDetails={setDetailsFor}
        onUpdate={(g) => void runUpdate(g)}
        onLocate={(g) => void runLocate(g)}
      />
      <NotesDialog gameId={notesFor} onClose={() => setNotesFor(null)} />
      <DetailsDialog gameId={detailsFor} onClose={() => setDetailsFor(null)} />
      <AttentionSummary games={games} />
    </div>
  );
}

/** Screen-reader summary of what needs attention (the cards say it visually). */
function AttentionSummary({ games }: { games: GameSummary[] }) {
  const missing = games.filter((g) => gameStatus(g) === 'missing').length;
  const updates = games.filter((g) => gameStatus(g) === 'update').length;
  if (!missing && !updates) return null;
  const parts = [missing ? `${missing} game${missing === 1 ? '' : 's'} not found` : '', updates ? `${updates} handler update${updates === 1 ? '' : 's'}` : ''].filter(Boolean);
  return <p className="sr-only">{parts.join(', ')}.</p>;
}

function EmptyLibrary({ adding, onAddGame }: { adding: boolean; onAddGame: () => void }) {
  return (
    <Pane as="section" aria-labelledby="empty-title" className="flex min-h-[372px] flex-col justify-end gap-4 px-9 pt-8 pb-[30px]">
      <div aria-hidden="true" className="grid aspect-[16/10] w-[132px] grid-cols-2 gap-1">
        <i className="rounded-[5px] bg-pane-3" />
        <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
      </div>
      <h1 id="empty-title" className="type-h1 max-w-[16ch]">
        Your library is empty
      </h1>
      <p className="max-w-[54ch] text-ink-2">
        Add the games Nucleus found on this PC, pick a handler on the Hub, or point Nucleus at a game you have. Each game gets a ready-made
        split-screen setup.
      </p>
      <div className="flex flex-wrap gap-2.5">
        <ButtonLink to="/games" variant="primary" size="lg" data-nav data-autofocus>
          <Icon name="compass" />
          Get games
        </ButtonLink>
        <Button variant="ghost" size="lg" data-nav data-hints="add" onClick={onAddGame} disabled={adding}>
          {adding ? <Spinner /> : <Icon name="plus" />}
          Add a game
        </Button>
      </div>
    </Pane>
  );
}
