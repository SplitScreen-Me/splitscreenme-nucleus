// Dialogs of the add-game flows (features/add-game.tsx): the handler chooser, the report of an executable whose
// folder the handlers refuse, and the automatic search after a handler install (asked once, then the search).
import { useEffect, useRef } from 'react';
import type { ExecutableContextMismatch, FolderSearchCandidate, HandlerCandidate, RejectedHandler } from '@/bridge';
import { SearchProgress } from '@/features/game-folders';
import { T, useI18n, type RichTags } from '@/i18n';
import { errorMessage } from '@/lib/bridge-ui';
import { useFolderSearch } from '@/lib/game-search';
import { Button, Dialog, Icon, useToast } from '@/ui';

const FILE_NAME_TAG: RichTags = { b: (children) => <b className="font-[650] text-ink">{children}</b> };

/** Several handlers run this executable: which game is it? Resolves with the handler's game id, or null. */
export function HandlerChooserDialog({
  choice,
  onSettle,
}: {
  choice: { fileName: string; candidates: HandlerCandidate[] } | null;
  onSettle: (gameId: string | null) => void;
}) {
  const { t } = useI18n();
  return (
    <Dialog open={choice !== null} onClose={() => onSettle(null)} title={t('features.addGame.chooser.title')} icon="library" className="w-[min(560px,90vw)]">
      {choice && (
        <div className="grid gap-4">
          <p className="text-15 text-ink-2">
            <T k="features.addGame.chooser.message" values={{ file: choice.fileName }} tags={FILE_NAME_TAG} />
          </p>
          <ul className="m-0 grid list-none gap-1.5 p-0">
            {choice.candidates.map((c, i) => (
              <li key={c.gameId}>
                <Button variant="default" className="w-full justify-start" onClick={() => onSettle(c.gameId)} data-nav autoFocus={i === 0}>
                  <Icon name="library" />
                  <span className="truncate">{c.name}</span>
                </Button>
              </li>
            ))}
          </ul>
          <div className="flex justify-end">
            <Button variant="ghost" onClick={() => onSettle(null)} data-nav>
              {t('common.cancel')}
            </Button>
          </div>
        </div>
      )}
    </Dialog>
  );
}

export interface ContextReport {
  /** The executable picked. */
  path: string;
  data: ExecutableContextMismatch;
}

const folderOfPath = (path: string) => path.slice(0, Math.max(path.lastIndexOf('\\'), path.lastIndexOf('/'))) || path;

function Entries({ entries, tone }: { entries: readonly string[]; tone: 'missing' | 'excluded' }) {
  return (
    <ul className="m-0 flex list-none flex-wrap gap-1.5 p-0">
      {entries.map((entry) => (
        <li
          key={entry}
          className={
            tone === 'missing'
              ? 'data rounded-md bg-pane-3 px-2 py-0.5 text-13 text-ink'
              : 'data rounded-md bg-pane-3 px-2 py-0.5 text-13 text-warn shadow-[inset_0_0_0_1px_color-mix(in_oklab,var(--color-warn),transparent_60%)]'
          }
        >
          {entry}
        </li>
      ))}
    </ul>
  );
}

function RejectedCard({ handler }: { handler: RejectedHandler }) {
  const { t } = useI18n();
  return (
    <li className="grid gap-2.5 rounded-ctl bg-pane-2 px-4 py-3">
      <div className="flex min-w-0 flex-wrap items-baseline gap-x-2.5 gap-y-0.5">
        <b className="text-15 font-[650]">{handler.name}</b>
        {handler.file && <span className="data truncate text-13 text-ink-3">{t('features.addGame.context.handlerFile', { file: handler.file })}</span>}
      </div>
      {handler.missing.length > 0 && (
        <div className="grid gap-1.5">
          <span className="text-13 font-[600] text-ink-2">{t('features.addGame.context.missing')}</span>
          <Entries entries={handler.missing} tone="missing" />
        </div>
      )}
      {handler.excluded.length > 0 && (
        <div className="grid gap-1.5">
          <span className="text-13 font-[600] text-ink-2">{t('features.addGame.context.excluded')}</span>
          <Entries entries={handler.excluded} tone="excluded" />
          <span className="text-12 text-ink-3">{t('features.addGame.context.excludedHint')}</span>
        </div>
      )}
    </li>
  );
}

/**
 * Handlers know the executable's file name but refuse the folder it is in (`Game.ExecutableContext`): per handler,
 * what is missing next to it and what it refuses to find there. Resolves true to pick another file.
 */
export function ContextReportDialog({ report, onSettle }: { report: ContextReport | null; onSettle: (pickAgain: boolean) => void }) {
  const { t } = useI18n();
  const data = report?.data;
  const onlyExcluded = data ? data.handlers.every((h) => h.missing.length === 0) : false;
  const file = data?.executable || (report ? report.path.slice(report.path.lastIndexOf('\\') + 1) : '');
  return (
    <Dialog
      open={report !== null}
      onClose={() => onSettle(false)}
      title={t('features.addGame.context.title')}
      icon="warning"
      iconClassName="text-warn"
      className="w-[min(620px,92vw)]"
    >
      {report && data && (
        <div className="grid gap-4">
          <div className="grid gap-1.5">
            <p className="text-15 leading-[1.5] text-ink-2">
              {data.handlers.length === 1 ? (
                <T k="features.addGame.context.introOne" values={{ game: data.handlers[0]!.name, file }} tags={FILE_NAME_TAG} />
              ) : (
                <T k="features.addGame.context.introMany" values={{ file }} tags={FILE_NAME_TAG} />
              )}
            </p>
            <p className="data truncate text-13 text-ink-3" title={report.path}>
              {folderOfPath(report.path)}
            </p>
          </div>
          <ul className="m-0 grid max-h-[40vh] list-none gap-2 overflow-y-auto p-0">
            {data.handlers.map((h) => (
              <RejectedCard key={`${h.gameId}|${h.file}`} handler={h} />
            ))}
          </ul>
          <p className="flex items-start gap-2 text-15 leading-[1.5] text-ink">
            <Icon name="info" className="mt-1 size-4 shrink-0 text-ink-3" />
            {onlyExcluded ? t('features.addGame.context.hintExcluded') : t('features.addGame.context.hintMissing')}
          </p>
          <div className="flex justify-end gap-2">
            <Button variant="ghost" onClick={() => onSettle(false)} data-nav>
              {t('common.close')}
            </Button>
            <Button variant="primary" onClick={() => onSettle(true)} data-nav autoFocus>
              <Icon name="file" />
              {t('features.addGame.context.pickAgain')}
            </Button>
          </div>
        </div>
      )}
    </Dialog>
  );
}

export type AutoSearchAnswer = 'on' | 'off' | 'later';

/** Asked once, after the first handler install (Nucleus 2.5): search the game folders from now on? */
export function AutoSearchAskDialog({
  ask,
  onSettle,
}: {
  ask: { game: string; hasFolders: boolean } | null;
  onSettle: (answer: AutoSearchAnswer) => void;
}) {
  const { t } = useI18n();
  return (
    <Dialog open={ask !== null} onClose={() => onSettle('later')} title={t('features.addGame.autoSearch.ask.title')} icon="search" className="w-[min(560px,90vw)]">
      {ask && (
        <div className="grid gap-4">
          <p className="text-15 leading-[1.55] text-ink-2">{t('features.addGame.autoSearch.ask.text', { game: ask.game })}</p>
          <p className="text-13 text-ink-3">{t('features.addGame.autoSearch.ask.hint')}</p>
          <div className="flex flex-wrap justify-end gap-2">
            <Button variant="ghost" onClick={() => onSettle('off')} data-nav>
              {t('features.addGame.autoSearch.ask.noThanks')}
            </Button>
            <Button variant="primary" onClick={() => onSettle('on')} data-nav autoFocus>
              <Icon name={ask.hasFolders ? 'search' : 'folder'} />
              {ask.hasFolders ? t('features.addGame.autoSearch.ask.turnOnSearch') : t('features.addGame.autoSearch.ask.turnOnFolders')}
            </Button>
          </div>
        </div>
      )}
    </Dialog>
  );
}

/** What the search after an install leads to: add the copy found, pick a file (starting in `folder`), or nothing. */
export type FindOutcome = { kind: 'add'; candidate: FolderSearchCandidate } | { kind: 'pick'; folder?: string; notFound?: boolean } | { kind: 'cancel' };

/**
 * After a handler install, with the automatic search on: looks for the game in the game folders (first copy), then
 * offers to add it, or to pick another file starting in its folder. Not found: picks straight away.
 */
export function FindGameDialog({ find, onSettle }: { find: { gameId: string; gameName: string } | null; onSettle: (outcome: FindOutcome) => void }) {
  const { t } = useI18n();
  const search = useFolderSearch();
  const toast = useToast();
  const { start, reset } = search;
  const settled = useRef(onSettle);
  settled.current = onSettle;

  useEffect(() => {
    if (!find) return;
    let live = true;
    void start(find.gameId).then((result) => {
      if (!live || !result) return;
      if (result.candidates.length === 0) settled.current({ kind: 'pick', notFound: true });
    });
    return () => {
      live = false;
      reset();
    };
  }, [find, start, reset]);

  const { state } = search;
  useEffect(() => {
    if (find && state.status === 'failed') {
      toast(errorMessage(state.error), { icon: 'warning', durationMs: 4200 });
      settled.current({ kind: 'pick' });
    }
  }, [find, state, toast]);

  const found = search.state.status === 'done' ? search.state.result.candidates[0] : undefined;
  return (
    <Dialog
      open={find !== null}
      onClose={() => onSettle({ kind: 'cancel' })}
      title={found ? t('features.addGame.autoSearch.found.title', { game: find?.gameName ?? '' }) : t('features.addGame.autoSearch.find.title', { game: find?.gameName ?? '' })}
      icon={found ? 'check' : 'search'}
      iconClassName={found ? 'text-ok' : undefined}
      className="w-[min(600px,92vw)]"
    >
      {find &&
        (found ? (
          <div className="grid gap-4">
            <p className="text-15 leading-[1.5] text-ink-2">{t('features.addGame.autoSearch.found.text', { game: find.gameName })}</p>
            <p className="data rounded-ctl bg-pane-2 px-4 py-3 text-13 leading-[1.5] text-ink break-all">{found.executablePath}</p>
            <div className="flex flex-wrap justify-end gap-2">
              <Button variant="ghost" onClick={() => onSettle({ kind: 'pick', folder: found.installFolder })} data-nav>
                <Icon name="file" />
                {t('features.addGame.autoSearch.found.other')}
              </Button>
              <Button variant="primary" onClick={() => onSettle({ kind: 'add', candidate: found })} data-nav autoFocus>
                <Icon name="plus" />
                {t('features.addGame.autoSearch.found.add')}
              </Button>
            </div>
          </div>
        ) : (
          <SearchProgress
            state={search.state}
            title={t('features.addGame.autoSearch.find.text')}
            onStop={() => onSettle({ kind: 'pick' })}
            stopLabel={t('features.addGame.autoSearch.find.skip')}
          />
        ))}
    </Dialog>
  );
}
