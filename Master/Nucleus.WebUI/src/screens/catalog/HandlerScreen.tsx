import { useMemo, useRef, useState, type ReactNode, type RefObject } from 'react';
import { Link, useParams } from 'react-router';
import { Markdown } from '@/components/Markdown';
import { bridge } from '@/bridge';
import { useAllHandlers, useHubComments, useHubHandler, variant, type HubCommentThread, type HubHandler } from '@/hub';
import { T, useI18n, type MessageKey } from '@/i18n';
import { igdbImage } from '@/lib/art';
import { useInstalledHandlers, useLibrary } from '@/lib/library';
import { igdbResize, useScreenshots } from '@/lib/screenshots';
import { useScreenChrome } from '@/shell/chrome';
import { Button, ButtonLink, Icon, InputSupport, inputSentence, Pane, Pips, PlayerTag, Spinner, Verified, cn, playerClass, type IconName } from '@/ui';
import { ActionPane } from './ActionPane';
import { HandlerCard } from './HandlerCard';
import { handlerInstallState, installStatuses } from './install-state';

export default function HandlerScreen() {
  const { handlerId = '' } = useParams();
  const { data: handler, isPending, isError, isPlaceholderData } = useHubHandler(handlerId);
  const { data: all = [] } = useAllHandlers();
  const { data: installed = [] } = useInstalledHandlers();
  const { data: library = [] } = useLibrary();
  const { t } = useI18n();
  const status = handler ? handlerInstallState(handler, installed, library).status : 'none';
  const installable = Boolean(handler) && (status === 'none' || status === 'update' || status === 'installed');
  const actionLabel = status === 'update' ? t('handler.hints.update') : status === 'installed' ? t('handler.hints.addGame') : t('handler.hints.install');

  useScreenChrome([{ label: t('catalog.title'), to: '/games' }, { label: handler?.gameName ?? t('handler.defaultTitle') }], {
    base: [
      { button: 'A', label: t('common.select') },
      ...(installable
        ? [{ button: 'X' as const, label: actionLabel, action: () => document.querySelector<HTMLButtonElement>('[data-install]:not(:disabled)')?.click() }]
        : []),
      { button: 'B', label: t('common.back') },
      { button: 'Y', label: t('common.search') },
    ],
  });

  if (isPending && !handler) {
    return (
      <Pane className="grid h-full place-items-center text-small text-ink-2">
        <span className="inline-flex items-center gap-2.5">
          <Spinner /> {t('handler.loading')}
        </span>
      </Pane>
    );
  }
  if (!handler) {
    return (
      <Pane className="grid h-full place-items-center p-8">
        <div className="grid max-w-[48ch] justify-items-start gap-4">
          <Icon name={isError ? 'offline' : 'info'} className="size-8 text-ink-3" />
          <h1 className="type-h2">{isError ? t('handler.unavailable.offlineTitle') : t('handler.unavailable.goneTitle')}</h1>
          <p className="text-ink-2">{isError ? t('handler.unavailable.offlineText') : t('handler.unavailable.goneText')}</p>
          <ButtonLink to="/games" variant="primary">
            <Icon name="arrow-l" />
            {t('handler.unavailable.back')}
          </ButtonLink>
        </div>
      </Pane>
    );
  }
  return <HandlerDetail handler={handler} all={all} refreshing={isPlaceholderData} />;
}

function HandlerDetail({ handler: h, all, refreshing }: { handler: HubHandler; all: HubHandler[]; refreshing: boolean }) {
  const { t } = useI18n();
  const v = variant(h);
  const inputs = inputSentence(h.inputs);
  const howMany = h.maxPlayers ? t('handler.hero.upToPlayers', { count: h.maxPlayers }) : t('handler.hero.splitScreen');
  const commentsRef = useRef<HTMLElement>(null);

  const byAuthor = useMemo(() => (h.authorId ? all.filter((x) => x.authorId === h.authorId) : [h]), [all, h]);
  const related = useMemo(() => relatedHandlers(h, all), [h, all]);
  const { data: installed = [] } = useInstalledHandlers();
  const { data: library = [] } = useLibrary();
  const statuses = useMemo(() => installStatuses(installed, library, related), [installed, library, related]);

  return (
    <div className="grid gap-gutter">
      <section aria-labelledby="game-title" className="grid min-h-[440px] gap-gutter grid-cols-[minmax(340px,0.82fr)_minmax(0,1.18fr)]">
        <Pane className="flex flex-col gap-6 px-8 pt-6 pb-8">
          <Link to="/games" className="inline-flex items-center gap-1.5 self-start rounded text-small text-ink-2 no-underline hover:text-ink">
            <Icon name="arrow-l" className="size-4" />
            {t('handler.hero.allGames')}
          </Link>
          <div className="mt-auto grid gap-4">
            <h1
              id="game-title"
              className={cn(
                'leading-[0.98] font-[800] tracking-[-0.025em] text-balance stretch-wide',
                h.gameName.length > 26 ? 'text-[clamp(1.75rem,2.6vw,2.6rem)]' : 'text-[clamp(2rem,3.3vw,3.4rem)]',
              )}
            >
              {h.gameName}
              {v && <span className="block font-[700] text-ink-2">{v}</span>}
            </h1>
            <p className="max-w-[40ch] text-17 text-ink-2">
              {inputs ? t('handler.hero.summaryWithInputs', { players: howMany, inputs }) : t('handler.hero.summary', { players: howMany })}
            </p>
            <div className="flex flex-wrap items-center gap-x-6 gap-y-4 border-t border-line pt-4">
              <Pips players={h.maxPlayers} size="lg" />
              <InputSupport flags={h.inputs} size="lg" />
              {h.verified && <Verified>{t('handler.hero.verified')}</Verified>}
            </div>
            <p className="text-small text-ink-3">
              <T
                k="handler.hero.byline"
                values={{
                  title: h.title,
                  author: h.authorUrl ? (
                    <ExternalText href={h.authorUrl} className="text-ink-2">
                      {h.author}
                    </ExternalText>
                  ) : (
                    <span className="text-ink-2">{h.author}</span>
                  ),
                  version: String(h.version),
                }}
              />
            </p>
          </div>
        </Pane>
        <SplitPreview handler={h} />
      </section>

      <div className="grid items-start gap-gutter grid-cols-[minmax(0,1fr)_340px] min-[1500px]:grid-cols-[minmax(0,1fr)_360px]">
        <div className="grid min-w-0 gap-gutter">
          <Pane as="section" aria-labelledby="h-compat" className="@container p-8">
            <h2 id="h-compat" className="type-h3 mb-6">
              {t('handler.compat.title')}
            </h2>
            <Compat handler={h} />
          </Pane>

          <Pane as="section" aria-labelledby="h-notes" className="p-8">
            <h2 id="h-notes" className="type-h2 mb-6">
              {t('handler.notes.title')}
            </h2>
            {h.description.trim() ? (
              <Markdown>{h.description}</Markdown>
            ) : refreshing ? (
              <p className="flex items-center gap-2 text-ink-2">
                <Spinner /> {t('handler.notes.loading')}
              </p>
            ) : (
              <p className="max-w-[68ch] text-ink-2">{t('handler.notes.none', { author: h.author })}</p>
            )}
            {h.gameDescription && (
              <div className="mt-8 border-t border-line pt-6">
                <h3 className="mb-2 text-16">{t('handler.notes.about', { game: h.gameName })}</h3>
                <p className="max-w-[68ch] text-ink-2">{h.gameDescription}</p>
              </div>
            )}
          </Pane>

          <Pane as="section" aria-labelledby="h-steps" className="@container p-8">
            <h2 id="h-steps" className="type-h2 mb-6">
              {t('handler.steps.title', { game: h.gameName })}
            </h2>
            <Steps handler={h} />
          </Pane>

          <Comments handler={h} sectionRef={commentsRef} />
        </div>

        <aside aria-label={t('handler.installPane')} className="sticky top-0">
          <ActionPane handler={h} byAuthor={byAuthor} onComments={() => commentsRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })} />
        </aside>
      </div>

      {related.length > 0 && (
        <Pane as="section" aria-labelledby="h-related" className="p-8">
          <div className="mb-6 flex items-end justify-between gap-4">
            <h2 id="h-related" className="type-h2">
              {t('handler.related.title')}
            </h2>
            <Link to="/games" className="rounded text-small font-semibold text-ink-2 no-underline hover:text-ink">
              {t('handler.related.browseAll')}
            </Link>
          </div>
          <ul className="m-0 grid list-none grid-cols-[repeat(auto-fill,minmax(152px,1fr))] gap-x-[18px] gap-y-[34px] p-0">
            {related.map((x) => (
              <li key={x.id}>
                <HandlerCard handler={x} status={statuses.get(x.id)} />
              </li>
            ))}
          </ul>
        </Pane>
      )}
    </div>
  );
}

// ---------------------------------------------------------------- hero: the game split for its players

function SplitPreview({ handler: h }: { handler: HubHandler }) {
  const { t } = useI18n();
  const { data: shots } = useScreenshots(h.id);
  const seats = Math.min(h.maxPlayers ?? 2, 4);
  const views = Math.max(seats, 2);
  const images = (shots ?? []).flatMap((url) => igdbResize(url, views > 2 ? '720p' : '1080p') ?? []);
  const cover = igdbImage(h.coverImageId, '1080p');
  // Every copy renders its own view: different screenshots, or offsets into one image.
  const OFFSETS: Array<[string, string]> = images.length
    ? [['cover', '50% 50%'], ['cover', '50% 50%'], ['cover', '50% 50%'], ['cover', '50% 50%']]
    : [['cover', '50% 22%'], ['cover', '50% 55%'], ['190%', '30% 40%'], ['210%', '70% 65%']];
  const fallbackOffsets: Array<[string, string]> = [['cover', '50% 45%'], ['250%', '12% 35%'], ['270%', '88% 40%'], ['185%', '30% 70%']];
  return (
    <div
      role="img"
      aria-label={t('handler.hero.previewLabel', { game: h.gameName, count: seats })}
      className={cn('grid gap-gutter', views === 2 ? 'grid-cols-1 grid-rows-2' : 'grid-cols-2 grid-rows-2')}
    >
      {Array.from({ length: views }, (_, i) => {
        const img = images.length ? images[i % images.length] : cover;
        const reuse = images.length > 0 && i >= images.length;
        const [size, pos] = reuse ? fallbackOffsets[i]! : OFFSETS[i]!;
        return (
          <div
            key={i}
            className={cn(
              playerClass(i + 1),
              'relative overflow-hidden rounded-pane bg-pane-2 bg-no-repeat',
              views === 3 && i === 0 && 'col-span-2',
            )}
            style={img ? { backgroundImage: `url("${img}")`, backgroundSize: size, backgroundPosition: pos } : undefined}
          >
            <span aria-hidden="true" className="absolute inset-0 bg-[linear-gradient(180deg,rgba(0,0,0,0.45),transparent_32%)]" />
            <PlayerTag n={i + 1} className="absolute top-3 left-3 z-[1]" />
            {i === views - 1 && (h.maxPlayers ?? 0) > 4 && (
              <span className="absolute right-3 bottom-3 z-[1] rounded-tag bg-black/70 px-2.5 py-1 text-micro font-semibold text-ink backdrop-blur-sm">
                {t('handler.hero.moreSeats', { count: (h.maxPlayers ?? 0) - 4 })}
              </span>
            )}
          </div>
        );
      })}
    </div>
  );
}

// ---------------------------------------------------------------- compatibility

function Compat({ handler: h }: { handler: HubHandler }) {
  const { t } = useI18n();
  const cell = (on: boolean | undefined, icon: IconName, label: MessageKey, yes: MessageKey, no: MessageKey, last = false) => (
    <li className={cn('grid content-start gap-2.5 bg-pane p-4', last && 'col-span-2 @min-[840px]:col-span-1')}>
      <span className="min-h-[2.6em] text-small leading-[1.3] text-ink-3">{t(label)}</span>
      <span className={cn('flex items-center gap-2 font-[650]', !on && 'font-medium text-ink-3')}>
        <Icon name={icon} className={cn('size-[22px]', !on && 'opacity-40')} />
        {on === undefined ? t('handler.compat.notStated') : t(on ? yes : no)}
      </span>
    </li>
  );
  return (
    <ul className="m-0 grid list-none grid-cols-3 gap-px overflow-hidden rounded-[10px] bg-line p-px @min-[840px]:grid-cols-5">
      <li className="grid content-start gap-2.5 bg-pane p-4">
        <span className="min-h-[2.6em] text-small leading-[1.3] text-ink-3">{t('handler.compat.players')}</span>
        <span className="flex items-center gap-2 font-[650]">
          <Pips players={h.maxPlayers} size="lg" />
        </span>
      </li>
      {cell(h.inputs.controllers, 'pad', 'handler.compat.controllers', 'handler.compat.supported', 'handler.compat.notSupported')}
      {cell(h.inputs.keyboardMouse, 'kbm', 'handler.compat.keyboardMouse', 'handler.compat.supported', 'handler.compat.notSupported')}
      {cell(h.inputs.multiKeyboardMouse, 'multi', 'handler.compat.multiKeyboardMouse', 'handler.compat.onePerPlayer', 'handler.compat.notSupported')}
      {cell(h.verified, 'seal', 'handler.compat.moderators', 'handler.compat.verified', 'handler.compat.notVerified', true)}
    </ul>
  );
}

// ---------------------------------------------------------------- steps

function Steps({ handler: h }: { handler: HubHandler }) {
  const { t } = useI18n();
  const steps: Array<{ id: string; title: string; body: ReactNode; tip?: ReactNode }> = [
    { id: 'install', title: t('handler.steps.installTitle'), body: t('handler.steps.installBody', { game: h.gameName }) },
    { id: 'locate', title: t('handler.steps.locateTitle'), body: t('handler.steps.locateBody', { game: h.gameName }) },
    {
      id: 'seat',
      title: t('handler.steps.seatTitle'),
      body: t('handler.steps.seatBody'),
      tip: h.inputs.multiKeyboardMouse ? <T k="handler.steps.seatTip" values={{ key: 'End' }} /> : undefined,
    },
    { id: 'connect', title: t('handler.steps.connectTitle'), body: t('handler.steps.connectBody') },
  ];
  return (
    <ol className="m-0 grid list-none grid-cols-2 gap-6 p-0 @min-[860px]:grid-cols-4">
      {steps.map((s, i) => (
        <li key={s.id} className="grid content-start gap-2">
          <span aria-hidden="true" className="mb-2 grid size-9 place-items-center rounded-[10px] bg-pane-3 font-[800] text-ink stretch-wide">
            {i + 1}
          </span>
          <b className="text-16 font-[680]">{s.title}</b>
          <p className="text-15 text-ink-2">{s.body}</p>
          {s.tip && <p className="mt-1 text-small text-ink-3">{s.tip}</p>}
        </li>
      ))}
    </ol>
  );
}

// ---------------------------------------------------------------- comments

function Comments({ handler: h, sectionRef }: { handler: HubHandler; sectionRef: RefObject<HTMLElement | null> }) {
  const { data: threads, isPending, isError, refetch } = useHubComments(h.id);
  const { t } = useI18n();
  const [all, setAll] = useState(false);
  const FIRST = 5;
  const count = threads ? threads.reduce((n, thread) => n + 1 + thread.replies.length, 0) : h.comments;
  return (
    <Pane as="section" aria-labelledby="h-comments" className="scroll-mt-1 p-8" ref={sectionRef}>
      <div className="mb-6 flex flex-wrap items-baseline gap-3">
        <h2 id="h-comments" className="type-h3">
          <T k="handler.comments.title" values={{ number: count }} tags={{ count: (c) => <span className="data font-medium text-ink-3">{c}</span> }} />
        </h2>
        <Button size="sm" variant="ghost" className="ml-auto" disabled={!h.url} onClick={() => void bridge.app.openExternal({ url: h.url }).catch(() => undefined)}>
          <Icon name="comment" />
          {t('handler.comments.commentOnHub')}
        </Button>
      </div>
      {isPending ? (
        <p className="flex items-center gap-2 text-small text-ink-2">
          <Spinner /> {t('handler.comments.loading')}
        </p>
      ) : isError ? (
        <p className="flex items-center gap-3 text-small text-ink-2">
          {t('handler.comments.loadError')}
          <Button size="sm" variant="ghost" onClick={() => void refetch()}>
            {t('common.tryAgain')}
          </Button>
        </p>
      ) : !threads?.length ? (
        <p className="text-15 text-ink-2">{t('handler.comments.none')}</p>
      ) : (
        <ul className="m-0 grid max-w-[76ch] list-none gap-6 p-0">
          {(all ? threads : threads.slice(0, FIRST)).map((thread) => (
            <CommentItem key={thread.id} comment={thread} />
          ))}
        </ul>
      )}
      {threads && threads.length > FIRST && !all && (
        <Button variant="ghost" className="mt-6" onClick={() => setAll(true)} data-nav>
          {t('handler.comments.showMore', { count: threads.length - FIRST })}
        </Button>
      )}
    </Pane>
  );
}

function CommentItem({ comment: c }: { comment: HubCommentThread }) {
  const { format } = useI18n();
  return (
    <li className="grid grid-cols-[36px_minmax(0,1fr)] gap-3">
      <Avatar name={c.author} />
      <div className="min-w-0">
        <div className="flex flex-wrap items-baseline gap-2">
          <b className="text-15 font-[650]">{c.author}</b>
          <time dateTime={c.createdAt} title={format.date(c.createdAt, 'medium')} className="text-small text-ink-3">
            {format.ago(c.createdAt)}
          </time>
        </div>
        <Markdown compact className="mt-1">
          {c.content}
        </Markdown>
        {c.replies.length > 0 && (
          <ul className="m-0 mt-4 grid list-none gap-4 border-l border-line p-0 pl-4">
            {c.replies.map((r) => (
              <li key={r.id} className="grid grid-cols-[28px_minmax(0,1fr)] gap-2.5">
                <Avatar name={r.author} small />
                <div className="min-w-0">
                  <div className="flex flex-wrap items-baseline gap-2">
                    <b className="text-14 font-[650]">{r.author}</b>
                    <time dateTime={r.createdAt} title={format.date(r.createdAt, 'medium')} className="text-13 text-ink-3">
                      {format.ago(r.createdAt)}
                    </time>
                  </div>
                  <Markdown compact className="mt-0.5 text-14">
                    {r.content}
                  </Markdown>
                </div>
              </li>
            ))}
          </ul>
        )}
      </div>
    </li>
  );
}

function Avatar({ name, small }: { name: string; small?: boolean }) {
  return (
    <span
      aria-hidden="true"
      className={cn('grid shrink-0 place-items-center rounded-full bg-pane-3 font-[750] text-ink stretch-wide', small ? 'size-7 text-12' : 'size-9 text-14')}
    >
      {name[0]?.toUpperCase() ?? '?'}
    </span>
  );
}

function ExternalText({ href, children, className }: { href: string; children: ReactNode; className?: string }) {
  return (
    <a
      href={href}
      onClick={(e) => {
        e.preventDefault();
        void bridge.app.openExternal({ url: href }).catch(() => undefined);
      }}
      className={cn('underline underline-offset-2 hover:text-ink', className)}
    >
      {children}
    </a>
  );
}

// ---------------------------------------------------------------- related

function relatedHandlers(h: HubHandler, all: HubHandler[]): HubHandler[] {
  const seen = new Set([h.id]);
  const byTrend = [...all].sort((a, b) => b.trendScore - a.trendScore);
  const pick = (list: HubHandler[]) =>
    list.filter((x) => {
      if (seen.has(x.id)) return false;
      seen.add(x.id);
      return true;
    });
  return [
    ...pick(byTrend.filter((x) => x.gameName === h.gameName)),
    ...pick(byTrend.filter((x) => x.genres.some((g) => h.genres.includes(g)) && Math.abs((x.maxPlayers ?? 0) - (h.maxPlayers ?? 0)) <= 2)),
    ...pick(byTrend.filter((x) => Math.abs((x.maxPlayers ?? 0) - (h.maxPlayers ?? 0)) <= 2)),
  ].slice(0, 7);
}
