// "Search my games" (Nucleus 2.5 automatic search): the game folders are searched for the executables of the installed
// handlers, offline, then the games found are added to the library.
import { useCallback, useEffect, useRef, useState, type ReactNode } from 'react';
import { useNavigate } from 'react-router';
import type { FolderSearchCandidate } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { SearchProgress, useAddGameFolder, useGameFolders } from '@/features/game-folders';
import { useI18n } from '@/i18n';
import { errorMessage } from '@/lib/bridge-ui';
import { defaultPicks, duplicatedGames, pickAll, togglePick, useFolderSearch } from '@/lib/game-search';
import { Button, ButtonLink, Checkbox, Dialog, Icon, Spinner, cn, useToast, type IconName } from '@/ui';

type RowStep = 'waiting' | 'adding' | 'added' | 'failed';

export interface GameSearchDialogProps {
  open: boolean;
  onClose: () => void;
  /** After games were added, with their ids (first added first). */
  onAdded?: (gameIds: string[]) => void;
  /** Show the "Edit folders" button, which opens the settings (not when the dialog is opened from them). */
  editFoldersLink?: boolean;
}

/**
 * "Search my games": searches the game folders for the installed handlers' executables (with progress and a way to
 * stop), then lists the games found to add them. Games already in the library are shown, not offered.
 */
export function GameSearchDialog({ open, onClose, onAdded, editFoldersLink = true }: GameSearchDialogProps) {
  const { t } = useI18n();
  return (
    <Dialog open={open} onClose={onClose} title={t('features.gameSearch.title')} icon="search" className="w-[min(680px,92vw)]">
      {open && <SearchFlow onClose={onClose} onAdded={onAdded} editFoldersLink={editFoldersLink} />}
    </Dialog>
  );
}

function SearchFlow({ onClose, onAdded, editFoldersLink }: Omit<GameSearchDialogProps, 'open'>) {
  const { t } = useI18n();
  const folders = useGameFolders();
  const addFolder = useAddGameFolder();
  const search = useFolderSearch();
  const { addGameAt } = useAddGame();
  const toast = useToast();
  const navigate = useNavigate();
  const [picked, setPicked] = useState<ReadonlySet<string>>(new Set());
  const [steps, setSteps] = useState<Record<string, RowStep>>({});
  const [adding, setAdding] = useState(false);
  const started = useRef(false);

  const existing = folders.data?.folders.filter((f) => f.exists) ?? [];
  const saved = folders.data?.folders.length ?? 0;
  const ready = existing.length > 0;
  const { start, cancel } = search;

  const run = useCallback(async () => {
    setSteps({});
    const result = await start();
    if (result) setPicked(defaultPicks(result.candidates));
  }, [start]);

  // Search as soon as the dialog opens, when there is somewhere to search (once: later searches are the user's).
  useEffect(() => {
    if (!ready || started.current) return;
    started.current = true;
    void run();
    return () => {
      // Unmounted (or remounted by StrictMode) before the user did anything: the next mount searches again.
      started.current = false;
      cancel();
    };
  }, [ready, run, cancel]);

  const editFolders = () => {
    onClose();
    navigate('/settings#gameFolders');
  };

  const addAndSearch = async () => {
    const next = await addFolder(existing[0]?.path);
    if (next?.folders.some((f) => f.exists)) {
      started.current = true;
      void run();
    }
  };

  const found = search.state.status === 'done' ? search.state.result.candidates : [];
  const addable = found.filter((c) => !c.inLibrary);
  // New games first; games already in the library after them, for information.
  const candidates = [...addable, ...found.filter((c) => c.inLibrary)];
  const newGames = new Set(addable.map((c) => c.gameId.toLowerCase())).size;
  const foundGames = new Set(found.map((c) => c.gameId.toLowerCase())).size;
  const chosen = candidates.filter((c) => picked.has(c.executablePath));
  const finished = Object.keys(steps).length > 0 && !adding;

  const addPicked = async () => {
    if (!chosen.length || adding) return;
    setAdding(true);
    setSteps(Object.fromEntries(chosen.map((c) => [c.executablePath, 'waiting' as RowStep])));
    const added: string[] = [];
    for (const c of chosen) {
      setSteps((s) => ({ ...s, [c.executablePath]: 'adding' }));
      const game = await addGameAt(c.executablePath, { handlerGameId: c.gameId, quiet: true });
      setSteps((s) => ({ ...s, [c.executablePath]: game ? 'added' : 'failed' }));
      if (game) added.push(game.id);
    }
    setAdding(false);
    if (added.length) {
      toast(t('features.gameSearch.toasts.added', { count: added.length }));
      onAdded?.(added);
    } else toast(t('features.gameSearch.toasts.noneAdded'), { icon: 'info' });
  };

  const footer = (main: ReactNode) => (
    <div className="mt-5 flex flex-wrap items-center gap-2">
      {editFoldersLink && (
        <Button variant="quiet" size="sm" onClick={editFolders} data-nav disabled={adding}>
          <Icon name="sliders" />
          {t('features.gameSearch.editFolders')}
        </Button>
      )}
      <span className="ml-auto flex flex-wrap justify-end gap-2">{main}</span>
    </div>
  );

  if (folders.isPending) {
    return (
      <p className="flex items-center gap-2 py-6 text-small text-ink-2" role="status">
        <Spinner /> {t('features.gameSearch.loading')}
      </p>
    );
  }

  if (folders.isError || existing.length === 0) {
    return (
      <div className="grid justify-items-start gap-4 py-1">
        <div className="grid gap-1.5">
          <h3 className="text-18 font-[700]">{t('features.gameSearch.noFolders.title')}</h3>
          <p className="max-w-[58ch] text-15 leading-[1.5] text-ink-2">
            {folders.isError ? errorMessage(folders.error) : saved > 0 ? t('features.gameSearch.noFolders.allMissing') : t('features.gameSearch.noFolders.text')}
          </p>
        </div>
        <div className="flex flex-wrap gap-2">
          <Button variant="primary" onClick={() => void addAndSearch()} data-nav autoFocus>
            <Icon name="folder" />
            {t('features.gameSearch.noFolders.add')}
          </Button>
          {editFoldersLink && saved > 0 && (
            <Button variant="ghost" onClick={editFolders} data-nav>
              {t('features.gameSearch.editFolders')}
            </Button>
          )}
        </div>
      </div>
    );
  }

  if (search.state.status === 'searching' || search.state.status === 'idle') {
    return <SearchProgress state={search.state} title={t('features.gameSearch.searching.title', { count: existing.length })} onStop={search.cancel} />;
  }

  if (search.state.status === 'failed' || search.state.status === 'cancelled') {
    const failure = search.state.status === 'failed' ? search.state.error : undefined;
    const failed = failure !== undefined;
    return (
      <div className="grid gap-2 py-1">
        <p className={cn('flex items-start gap-2 text-15', failed ? 'text-ink' : 'text-ink-2')} role={failed ? 'alert' : 'status'}>
          <Icon name={failed ? 'warning' : 'info'} className={cn('mt-0.5 size-4 shrink-0', failed ? 'text-warn' : 'text-ink-3')} />
          {failed ? errorMessage(failure) : t('features.gameSearch.stopped')}
        </p>
        {footer(
          <Button variant="primary" onClick={() => void run()} data-nav autoFocus>
            <Icon name="refresh" />
            {t('features.gameSearch.searchAgain')}
          </Button>,
        )}
      </div>
    );
  }

  if (addable.length === 0) {
    return (
      <div className="grid gap-2 py-1">
        <h3 className="text-18 font-[700]">{t('features.gameSearch.nothing.title')}</h3>
        <p className="max-w-[60ch] text-15 leading-[1.5] text-ink-2">
          {candidates.length > 0 ? t('features.gameSearch.nothing.allInLibrary', { count: foundGames }) : t('features.gameSearch.nothing.text')}
        </p>
        {footer(
          <>
            <ButtonLink to="/games" variant="ghost" onClick={onClose} data-nav>
              <Icon name="compass" />
              {t('features.gameSearch.nothing.getGames')}
            </ButtonLink>
            <Button variant="primary" onClick={() => void run()} data-nav autoFocus>
              <Icon name="refresh" />
              {t('features.gameSearch.searchAgain')}
            </Button>
          </>,
        )}
      </div>
    );
  }

  const duplicates = duplicatedGames(candidates);
  return (
    <div className="grid gap-3">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <p className="text-15 text-ink-2" aria-live="polite">
          {finished
            ? t('features.gameSearch.results.finished')
            : t('features.gameSearch.results.found', { count: newGames })}
        </p>
        {!adding && !finished && newGames > 1 && (
          <button
            type="button"
            onClick={() => setPicked(pickAll(candidates, chosen.length === 0))}
            className="ml-auto rounded border-0 bg-transparent p-0 text-13 font-semibold text-ink-3 hover:text-ink"
          >
            {chosen.length ? t('features.gameSearch.results.selectNone') : t('features.gameSearch.results.selectAll')}
          </button>
        )}
      </div>
      <ul className="scroll-quiet -mx-1.5 m-0 grid max-h-[46vh] list-none content-start gap-1 overflow-y-auto p-0 px-1.5">
        {candidates.map((c) => (
          <CandidateRow
            key={`${c.gameId}|${c.executablePath}`}
            candidate={c}
            checked={picked.has(c.executablePath)}
            step={steps[c.executablePath]}
            locked={adding || finished}
            duplicate={duplicates.has(c.gameId.toLowerCase())}
            onChange={(on) => setPicked(togglePick(picked, candidates, c.executablePath, on))}
          />
        ))}
      </ul>
      {duplicates.size > 0 && !finished && (
        <p className="flex items-start gap-2 text-13 leading-[1.4] text-ink-3">
          <Icon name="info" className="mt-px size-4 shrink-0" />
          {t('features.gameSearch.results.duplicates')}
        </p>
      )}
      {footer(
        finished ? (
          <Button variant="primary" onClick={onClose} data-nav autoFocus>
            <Icon name="check" />
            {t('common.done')}
          </Button>
        ) : (
          <>
            <Button variant="ghost" onClick={() => void run()} disabled={adding} data-nav>
              <Icon name="refresh" />
              {t('features.gameSearch.searchAgain')}
            </Button>
            <Button variant="primary" onClick={() => void addPicked()} disabled={!chosen.length || adding} data-nav autoFocus>
              {adding ? <Spinner className="border-on-light/30 border-t-on-light" /> : <Icon name="plus" />}
              {adding
                ? t('features.gameSearch.results.adding')
                : chosen.length
                  ? t('features.gameSearch.results.addCount', { count: chosen.length })
                  : t('features.gameSearch.results.selectToAdd')}
            </Button>
          </>
        ),
      )}
    </div>
  );
}

const STEP_ICON: Record<RowStep, IconName> = { waiting: 'clock', adding: 'clock', added: 'check', failed: 'warning' };

function CandidateRow({
  candidate: c,
  checked,
  step,
  locked,
  duplicate,
  onChange,
}: {
  candidate: FolderSearchCandidate;
  checked: boolean;
  step?: RowStep;
  locked: boolean;
  duplicate: boolean;
  onChange: (on: boolean) => void;
}) {
  const { t } = useI18n();
  const status = c.inLibrary
    ? t('features.gameSearch.row.inLibrary')
    : step === 'adding'
      ? t('features.gameSearch.row.adding')
      : step === 'added'
        ? t('features.gameSearch.row.added')
        : step === 'failed'
          ? t('features.gameSearch.row.failed')
          : duplicate
            ? t('features.gameSearch.row.anotherCopy')
            : undefined;
  const text = (
    <span className="min-w-0">
      <b className="block truncate text-15 leading-tight font-[650]">{c.gameName}</b>
      <span className="data block truncate text-13 text-ink-3" title={c.executablePath}>
        {c.executablePath}
      </span>
    </span>
  );
  const badge = status && (
    <span
      className={cn(
        'shrink-0 rounded-tag bg-pane-3 px-2 py-0.5 text-12 font-[650]',
        step === 'added' ? 'text-ok' : step === 'failed' ? 'text-danger' : 'text-ink-2',
      )}
    >
      {status}
    </span>
  );

  if (c.inLibrary || step || locked) {
    return (
      <li className={cn('grid grid-cols-[22px_minmax(0,1fr)_auto] items-center gap-3 rounded-[10px] py-2 pr-2 pl-1.5', c.inLibrary && 'opacity-70')}>
        <span className="grid size-5 place-items-center">
          {step === 'adding' ? (
            <Spinner />
          ) : c.inLibrary || step ? (
            <Icon name={c.inLibrary ? 'library' : STEP_ICON[step ?? 'waiting']} className={cn('size-4', step === 'added' ? 'text-ok' : 'text-ink-3')} />
          ) : null}
        </span>
        {text}
        {badge}
      </li>
    );
  }
  return (
    <li>
      <label className="grid cursor-pointer grid-cols-[22px_minmax(0,1fr)_auto] items-center gap-3 rounded-[10px] py-2 pr-2 pl-1.5 hover:bg-pane-2">
        <Checkbox
          checked={checked}
          disabled={locked}
          onChange={(e) => onChange(e.target.checked)}
          data-nav
          aria-label={t('features.gameSearch.row.addLabel', { game: c.gameName, path: c.executablePath })}
        />
        {text}
        {badge}
      </label>
    </li>
  );
}
