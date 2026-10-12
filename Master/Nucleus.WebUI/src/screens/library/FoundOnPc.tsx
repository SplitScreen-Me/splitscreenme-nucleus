import { useEffect, useMemo, useRef, useState, type RefObject } from 'react';
import { bridge, useBridgeQuery, type GameSummary, type InstallResult } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { useAllHandlers } from '@/hub';
import { igdbImage } from '@/lib/art';
import { errorMessage, isCancelled } from '@/lib/bridge-ui';
import { downloadProgress, installHandler, useActiveDownload } from '@/lib/downloads';
import { fmtAgo, listJoin, plural } from '@/lib/format';
import { useInstalledHandlers } from '@/lib/library';
import { useAppInfo } from '@/lib/live';
import { Button, Checkbox, Icon, Pane, Spinner, cn, useToast, type IconName } from '@/ui';
import { STORE_NAMES, matchInstalledGames, storeArt, type FoundCandidate } from './found-matching';

type Step = 'waiting' | 'installing' | 'searching' | 'picking' | 'adding' | 'added' | 'installed' | 'failed';

const STEP_TEXT: Record<Step, string> = {
  waiting: 'Waiting',
  installing: 'Installing the handler',
  searching: 'Looking for the game',
  picking: 'Pick the executable',
  adding: 'Adding',
  added: 'Added to your library',
  installed: 'Handler installed, game not added',
  failed: 'Failed',
};

/** Up to this many found games are selected by default. */
const DEFAULT_ALL = 5;

/** Whether a scrolling list has more below: its bottom edge then fades out. */
function useOverflowFade(ref: RefObject<HTMLElement | null>, contentKey: unknown): boolean {
  const [fade, setFade] = useState(false);
  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    const update = () => setFade(el.scrollHeight - el.clientHeight - el.scrollTop > 2);
    update();
    el.addEventListener('scroll', update, { passive: true });
    const ro = new ResizeObserver(update);
    ro.observe(el);
    return () => {
      el.removeEventListener('scroll', update);
      ro.disconnect();
    };
  }, [ref, contentKey]);
  return fade;
}

const STEP_ICON: Partial<Record<Step, IconName>> = { added: 'check', installed: 'info', failed: 'warning', waiting: 'clock' };

/**
 * "Found on this PC": Steam and Epic games that have a ready-made handler on the Hub. Adding one installs its handler,
 * finds its executable in the install folder (or asks), then adds it to the library.
 */
export function FoundOnPc({ library, onAdded }: { library: readonly GameSummary[]; onAdded: (gameId: string) => void }) {
  const scan = useBridgeQuery('library.scanInstalledGames', undefined, { staleTime: Infinity, gcTime: Infinity });
  const { data: hub = [], isPending: hubPending } = useAllHandlers();
  const { data: installedHandlers = [] } = useInstalledHandlers();
  const { data: info } = useAppInfo();
  const { pickExecutable, addGameAt } = useAddGame();
  const toast = useToast();
  // Explicit choices; until the user picks, a short list is all selected and a long one none (each add installs a handler).
  const [choice, setChoice] = useState<ReadonlySet<string> | null>(null);
  const [batch, setBatch] = useState<FoundCandidate[] | null>(null);
  const [steps, setSteps] = useState<Record<string, { step: Step; detail?: string }>>({});
  const [running, setRunning] = useState(false);
  const clearTimer = useRef<number | undefined>(undefined);

  const found = useMemo(
    () => matchInstalledGames(scan.data ?? [], hub, library, installedHandlers),
    [scan.data, hub, library, installedHandlers],
  );
  const shown = batch ?? found;
  const isChecked = (key: string) => (choice ? choice.has(key) : found.length <= DEFAULT_ALL);
  const selected = found.filter((f) => isChecked(f.key));
  const setChecked = (key: string, on: boolean) => {
    const next = new Set(found.filter((f) => isChecked(f.key)).map((f) => f.key));
    if (on) next.add(key);
    else next.delete(key);
    setChoice(next);
  };
  const list = useRef<HTMLUListElement>(null);
  const fade = useOverflowFade(list, shown.length);
  const offline = info ? !info.hubOnline : false;
  const sources = listJoin([...new Set((scan.data ?? []).map((g) => STORE_NAMES[g.store]))]);

  const setStep = (key: string, step: Step, detail?: string) => setSteps((s) => ({ ...s, [key]: { step, detail } }));

  /** Installs the handler of one found game, then adds the game. Returns the added game id. */
  const addOne = async (f: FoundCandidate): Promise<string | null> => {
    setStep(f.key, 'installing');
    let result: InstallResult;
    try {
      result = await installHandler(f.handler.id, {
        name: f.handler.gameName,
        coverUrl: igdbImage(f.handler.coverImageId, 'cover_big_2x'),
        version: f.handler.version,
      });
    } catch (error) {
      setStep(f.key, 'failed', isCancelled(error) ? 'Cancelled' : errorMessage(error));
      return null;
    }
    if (result.inLibrary) {
      setStep(f.key, 'added');
      return result.gameId;
    }
    setStep(f.key, 'searching');
    const names = result.executableNames.length ? result.executableNames : result.executableName ? [result.executableName] : [];
    let path: string | null = null;
    for (const executableName of names) {
      path = await bridge.library
        .findExecutable({ folder: f.game.installFolder, executableName }, { timeoutMs: 120_000 })
        .catch(() => null);
      if (path) break;
    }
    if (!path) {
      setStep(f.key, 'picking');
      path = await pickExecutable({ gameName: result.gameName, executableNames: names, initialDirectory: f.game.installFolder });
    }
    if (!path) {
      setStep(f.key, 'installed', 'Add the game later from Your games');
      return null;
    }
    setStep(f.key, 'adding');
    const game = await addGameAt(path, { handlerGameId: result.gameId, quiet: true });
    setStep(f.key, game ? 'added' : 'failed', game ? undefined : 'The game could not be added');
    return game?.id ?? null;
  };

  const add = async () => {
    const list = selected;
    if (!list.length || running) return;
    window.clearTimeout(clearTimer.current);
    setBatch(list);
    setSteps(Object.fromEntries(list.map((f) => [f.key, { step: 'waiting' as Step }])));
    setRunning(true);
    const added: string[] = [];
    for (const f of list) {
      const id = await addOne(f);
      if (id) added.push(id);
    }
    setRunning(false);
    if (added.length) {
      toast(`${plural(added.length, 'game')} added to your library`);
      onAdded(added[0]!);
    } else toast('No game was added', { icon: 'info' });
    // Keep the outcome on screen for a moment, then go back to what is left to add.
    clearTimer.current = window.setTimeout(() => {
      setBatch(null);
      setSteps({});
    }, 6000);
  };

  const rescan = async () => {
    const result = await scan.refetch();
    if (result.data && !result.isError) {
      const count = matchInstalledGames(result.data, hub, library, installedHandlers).length;
      toast(count ? `${plural(count, 'game')} found with a handler on the Hub` : 'Nothing new on this PC', { icon: count ? 'check' : 'info' });
    }
  };

  const scanning = scan.isFetching;
  const matching = hubPending && hub.length === 0;

  return (
    <Pane as="section" aria-labelledby="found-title" className="relative min-h-[300px]">
      <div className="absolute inset-0 flex flex-col px-5 pt-[22px] pb-5">
        <div className="mb-1.5 flex items-center gap-2">
          <h2 id="found-title" className="type-h3">
            Found on this PC
          </h2>
          {!batch && found.length > 1 && (
            <button
              type="button"
              onClick={() => setChoice(selected.length ? new Set() : new Set(found.map((f) => f.key)))}
              className="ml-auto rounded border-0 bg-transparent p-0 text-13 font-semibold text-ink-3 hover:text-ink"
            >
              {selected.length ? 'Select none' : 'Select all'}
            </button>
          )}
        </div>

        {scan.isPending || matching ? (
          <p className="mb-4 flex items-center gap-2 text-small text-ink-3" role="status">
            <Spinner /> {scan.isPending ? 'Reading your Steam and Epic libraries…' : 'Matching with the Hub catalogue…'}
          </p>
        ) : scan.isError ? (
          <div className="my-auto grid justify-items-start gap-3 py-4">
            <p className="text-small text-ink-2">
              <b className="mb-1 block text-16 text-ink">Your game libraries could not be read</b>
              {errorMessage(scan.error)}
            </p>
            <Button variant="ghost" data-nav data-hints="found-scan" onClick={() => void rescan()}>
              <Icon name="refresh" />
              Try again
            </Button>
          </div>
        ) : shown.length ? (
          <>
            <p className="mb-3 text-small leading-[1.45] text-ink-3" aria-live="polite">
              {batch
                ? running
                  ? 'Installing handlers and adding games. Keep Nucleus open.'
                  : 'Done. Games that need you are listed with what to do.'
                : `${sources ? `Scanned your ${sources} ${sources.includes(' and ') ? 'libraries' : 'library'}` : 'Scanned'} ${fmtAgo(new Date(scan.dataUpdatedAt).toISOString())}. These games have a ready-made handler on the Hub.`}
            </p>
            <ul
              ref={list}
              className={cn(
                'scroll-quiet -mx-1.5 m-0 mb-3 grid min-h-0 flex-1 list-none content-start gap-1 overflow-y-auto p-0 px-1.5',
                fade && '[mask-image:linear-gradient(to_bottom,black_calc(100%-28px),transparent)]',
              )}
            >
              {shown.map((f) => (
                <FoundItem
                  key={f.key}
                  found={f}
                  checked={isChecked(f.key)}
                  progress={batch ? (steps[f.key] ?? { step: 'waiting' }) : undefined}
                  onChange={(on) => setChecked(f.key, on)}
                />
              ))}
            </ul>
            {offline && !batch && (
              <p className="mb-3 flex items-start gap-2 text-13 leading-[1.4] text-ink-2">
                <Icon name="offline" className="mt-px size-4 shrink-0 text-warn" />
                The Hub is offline: handlers can't be downloaded right now.
              </p>
            )}
            <div className="mt-auto flex gap-2">
              {batch && !running ? (
                <Button
                  className="flex-1"
                  data-nav
                  data-hints="found"
                  onClick={() => {
                    window.clearTimeout(clearTimer.current);
                    setBatch(null);
                    setSteps({});
                  }}
                >
                  <Icon name="check" />
                  Done
                </Button>
              ) : (
                <Button className="flex-1" data-nav data-hints="found" disabled={!selected.length || running || offline} onClick={() => void add()}>
                  {running ? <Spinner /> : <Icon name="plus" />}
                  <span>{running ? 'Adding games…' : selected.length ? `Add ${plural(selected.length, 'game')}` : 'Select games to add'}</span>
                </Button>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="size-11 min-h-11"
                aria-label="Scan again"
                title="Scan again"
                data-nav
                data-hints="found-scan"
                disabled={scanning || running}
                onClick={() => void rescan()}
              >
                {scanning ? <Spinner /> : <Icon name="refresh" />}
              </Button>
            </div>
          </>
        ) : (
          <>
            <p className="my-auto py-6 text-small text-ink-2" aria-live="polite">
              <b className="mb-1 block text-16 text-ink">Nothing new on this PC</b>
              {(scan.data?.length ?? 0) > 0
                ? `None of the ${plural(scan.data?.length ?? 0, 'game')} installed through ${sources || 'your stores'} has a handler you don't already have.`
                : 'Nucleus looks for games installed through Steam and Epic. Install a game, then scan again.'}
            </p>
            <Button variant="ghost" data-nav data-hints="found-scan" disabled={scanning} onClick={() => void rescan()} className="self-start">
              {scanning ? <Spinner /> : <Icon name="refresh" />}
              Scan again
            </Button>
          </>
        )}
      </div>
    </Pane>
  );
}

function FoundItem({
  found: f,
  checked,
  progress,
  onChange,
}: {
  found: FoundCandidate;
  checked: boolean;
  progress?: { step: Step; detail?: string };
  onChange: (on: boolean) => void;
}) {
  const [imgOk, setImgOk] = useState(true);
  const download = useActiveDownload({ handlerId: progress?.step === 'installing' ? f.handler.id : undefined });
  const art = storeArt(f.game) ?? igdbImage(f.handler.coverImageId, 'screenshot_med');
  const players = f.handler.maxPlayers ? `up to ${f.handler.maxPlayers} players` : 'players unknown';
  const step = progress?.step;
  const busy = step === 'installing' || step === 'searching' || step === 'picking' || step === 'adding';
  const thumb =
    art && imgOk ? (
      <img src={art} alt="" loading="lazy" onError={() => setImgOk(false)} className="h-[41px] w-[88px] rounded-[5px] bg-pane-2 object-cover" />
    ) : (
      <span className="h-[41px] w-[88px] rounded-[5px] bg-pane-2" />
    );
  const text = (
    <span className="min-w-0">
      <b className="block truncate text-small leading-tight font-[650]">{f.game.name}</b>
      {progress ? (
        <span
          className={cn(
            'data flex items-center gap-1.5 truncate text-13',
            step === 'failed' ? 'text-danger' : step === 'installed' || step === 'picking' ? 'text-warn' : step === 'added' ? 'text-ok' : 'text-ink-3',
          )}
          title={progress.detail}
        >
          {progress.detail && step === 'failed'
            ? progress.detail
            : step === 'installing' && download
              ? `${STEP_TEXT.installing}, ${Math.round(downloadProgress(download) * 100)}%`
              : STEP_TEXT[progress.step]}
        </span>
      ) : (
        <span className="data block truncate text-13 text-ink-3">
          {STORE_NAMES[f.game.store]}, {players}
          {f.handler.verified ? ', verified' : ''}
        </span>
      )}
    </span>
  );

  if (progress) {
    return (
      <li className="grid grid-cols-[22px_88px_minmax(0,1fr)] items-center gap-3 rounded-[10px] py-1.5 pr-2 pl-1.5">
        <span className="grid size-5 place-items-center">
          {busy ? <Spinner /> : <Icon name={STEP_ICON[progress.step] ?? 'check'} className={cn('size-4', step === 'added' ? 'text-ok' : 'text-ink-3')} />}
        </span>
        {thumb}
        {text}
      </li>
    );
  }
  return (
    <li>
      <label className="grid cursor-pointer grid-cols-[22px_88px_minmax(0,1fr)] items-center gap-3 rounded-[10px] py-1.5 pr-2 pl-1.5 hover:bg-pane-2">
        <Checkbox checked={checked} onChange={(e) => onChange(e.target.checked)} data-nav data-hints="found-item" aria-label={`Add ${f.game.name}`} />
        {thumb}
        {text}
      </label>
    </li>
  );
}
