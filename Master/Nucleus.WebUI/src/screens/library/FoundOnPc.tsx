import { useEffect, useMemo, useRef, useState, type RefObject } from 'react';
import { bridge, useBridgeQuery, type GameSummary, type InstallResult } from '@/bridge';
import { useAddGame } from '@/features/add-game';
import { useAllHandlers } from '@/hub';
import { useI18n, type MessageKey } from '@/i18n';
import { igdbImage } from '@/lib/art';
import { errorMessage, isCancelled } from '@/lib/bridge-ui';
import { downloadProgress, installHandler, useActiveDownload } from '@/lib/downloads';
import { useInstalledHandlers } from '@/lib/library';
import { useAppInfo } from '@/lib/live';
import { Button, Checkbox, Icon, Pane, Spinner, cn, useToast, type IconName } from '@/ui';
import { STORE_NAMES, matchInstalledGames, storeArt, type FoundCandidate } from './found-matching';

type Step = 'waiting' | 'installing' | 'searching' | 'picking' | 'adding' | 'added' | 'installed' | 'failed';

const STEP_TEXT: Record<Step, MessageKey> = {
  waiting: 'library.found.steps.waiting',
  installing: 'library.found.steps.installing',
  searching: 'library.found.steps.searching',
  picking: 'library.found.steps.picking',
  adding: 'library.found.steps.adding',
  added: 'library.found.steps.added',
  installed: 'library.found.steps.installed',
  failed: 'library.found.steps.failed',
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
  const { t, format } = useI18n();
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
  const stores = [...new Set((scan.data ?? []).map((g) => STORE_NAMES[g.store]))].filter(Boolean);
  const sources = format.list(stores);

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
      setStep(f.key, 'failed', isCancelled(error) ? t('library.found.steps.cancelled') : errorMessage(error));
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
      setStep(f.key, 'installed', t('library.found.steps.addLater'));
      return null;
    }
    setStep(f.key, 'adding');
    const game = await addGameAt(path, { handlerGameId: result.gameId, quiet: true });
    setStep(f.key, game ? 'added' : 'failed', game ? undefined : t('library.found.steps.addFailed'));
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
      toast(t('library.found.toasts.added', { count: added.length }));
      onAdded(added[0]!);
    } else toast(t('library.found.toasts.noneAdded'), { icon: 'info' });
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
      toast(count ? t('library.found.toasts.found', { count }) : t('library.found.toasts.nothingNew'), { icon: count ? 'check' : 'info' });
    }
  };

  const scanning = scan.isFetching;
  const matching = hubPending && hub.length === 0;

  return (
    <Pane as="section" aria-labelledby="found-title" className="relative min-h-[300px]">
      <div className="absolute inset-0 flex flex-col px-5 pt-[22px] pb-5">
        <div className="mb-1.5 flex items-center gap-2">
          <h2 id="found-title" className="type-h3">
            {t('library.found.title')}
          </h2>
          {!batch && found.length > 1 && (
            <button
              type="button"
              onClick={() => setChoice(selected.length ? new Set() : new Set(found.map((f) => f.key)))}
              className="ml-auto rounded border-0 bg-transparent p-0 text-13 font-semibold text-ink-3 hover:text-ink"
            >
              {selected.length ? t('library.found.selectNone') : t('library.found.selectAll')}
            </button>
          )}
        </div>

        {scan.isPending || matching ? (
          <p className="mb-4 flex items-center gap-2 text-small text-ink-3" role="status">
            <Spinner /> {scan.isPending ? t('library.found.readingLibraries') : t('library.found.matching')}
          </p>
        ) : scan.isError ? (
          <div className="my-auto grid justify-items-start gap-3 py-4">
            <p className="text-small text-ink-2">
              <b className="mb-1 block text-16 text-ink">{t('library.found.readError')}</b>
              {errorMessage(scan.error)}
            </p>
            <Button variant="ghost" data-nav data-hints="found-scan" onClick={() => void rescan()}>
              <Icon name="refresh" />
              {t('common.tryAgain')}
            </Button>
          </div>
        ) : shown.length ? (
          <>
            <p className="mb-3 text-small leading-[1.45] text-ink-3" aria-live="polite">
              {batch
                ? running
                  ? t('library.found.running')
                  : t('library.found.finished')
                : stores.length
                  ? t('library.found.scannedStores', { count: stores.length, stores: sources, ago: format.ago(scan.dataUpdatedAt) })
                  : t('library.found.scanned', { ago: format.ago(scan.dataUpdatedAt) })}
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
                {t('library.found.offline')}
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
                  {t('common.done')}
                </Button>
              ) : (
                <Button className="flex-1" data-nav data-hints="found" disabled={!selected.length || running || offline} onClick={() => void add()}>
                  {running ? <Spinner /> : <Icon name="plus" />}
                  <span>
                    {running
                      ? t('library.found.addingGames')
                      : selected.length
                        ? t('library.found.addCount', { count: selected.length })
                        : t('library.found.selectToAdd')}
                  </span>
                </Button>
              )}
              <Button
                variant="ghost"
                size="icon"
                className="size-11 min-h-11"
                aria-label={t('library.found.scanAgain')}
                title={t('library.found.scanAgain')}
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
              <b className="mb-1 block text-16 text-ink">{t('library.found.emptyTitle')}</b>
              {(scan.data?.length ?? 0) > 0
                ? stores.length
                  ? t('library.found.noneNew', { count: scan.data?.length ?? 0, stores: sources })
                  : t('library.found.noneNewAnyStore', { count: scan.data?.length ?? 0 })
                : t('library.found.noGames')}
            </p>
            <Button variant="ghost" data-nav data-hints="found-scan" disabled={scanning} onClick={() => void rescan()} className="self-start">
              {scanning ? <Spinner /> : <Icon name="refresh" />}
              {t('library.found.scanAgain')}
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
  const { t, format } = useI18n();
  const [imgOk, setImgOk] = useState(true);
  const download = useActiveDownload({ handlerId: progress?.step === 'installing' ? f.handler.id : undefined });
  const art = storeArt(f.game) ?? igdbImage(f.handler.coverImageId, 'screenshot_med');
  const players = f.handler.maxPlayers ? t('library.found.item.upToPlayers', { count: f.handler.maxPlayers }) : t('library.found.item.playersUnknown');
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
              ? t('library.found.steps.installingProgress', { percent: format.percent(downloadProgress(download)) })
              : t(STEP_TEXT[progress.step])}
        </span>
      ) : (
        <span className="data block truncate text-13 text-ink-3">
          {t(f.handler.verified ? 'library.found.item.metaVerified' : 'library.found.item.meta', { store: STORE_NAMES[f.game.store], players })}
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
        <Checkbox checked={checked} onChange={(e) => onChange(e.target.checked)} data-nav data-hints="found-item" aria-label={t('library.found.item.addLabel', { game: f.game.name })} />
        {thumb}
        {text}
      </label>
    </li>
  );
}
