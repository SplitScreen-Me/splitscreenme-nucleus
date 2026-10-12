import { useMemo, useRef, useState, type ReactNode, type RefObject } from 'react';
import { Link, useParams } from 'react-router';
import { Markdown } from '@/components/Markdown';
import { bridge } from '@/bridge';
import { hubHandlerUrl, hubUserUrl, useAllHandlers, useHubComments, useHubHandler, variant, type HubCommentThread, type HubHandler } from '@/hub';
import { igdbImage } from '@/lib/art';
import { fmtAgo, fmtDate } from '@/lib/format';
import { useInstalledHandlers, useLibrary } from '@/lib/library';
import { igdbResize, useScreenshots } from '@/lib/screenshots';
import { useScreenChrome } from '@/shell/chrome';
import { Button, ButtonLink, Icon, InputSupport, inputSentence, Kbd, Pane, Pips, PlayerTag, Spinner, Verified, cn, playerClass, type IconName } from '@/ui';
import { ActionPane } from './ActionPane';
import { HandlerCard } from './HandlerCard';
import { handlerInstallState, installStatuses } from './install-state';

export default function HandlerScreen() {
  const { handlerId = '' } = useParams();
  const { data: handler, isPending, isError, isPlaceholderData } = useHubHandler(handlerId);
  const { data: all = [] } = useAllHandlers();
  const { data: installed = [] } = useInstalledHandlers();
  const { data: library = [] } = useLibrary();
  const status = handler ? handlerInstallState(handler, installed, library).status : 'none';
  const installable = Boolean(handler) && (status === 'none' || status === 'update' || status === 'installed');
  const actionLabel = status === 'update' ? 'Update handler' : status === 'installed' ? 'Add the game' : 'Install handler';

  useScreenChrome([{ label: 'Get games', to: '/games' }, { label: handler?.gameName ?? 'Handler' }], {
    base: [
      { button: 'A', label: 'Select' },
      ...(installable
        ? [{ button: 'X' as const, label: actionLabel, action: () => document.querySelector<HTMLButtonElement>('[data-install]:not(:disabled)')?.click() }]
        : []),
      { button: 'B', label: 'Back' },
      { button: 'Y', label: 'Search' },
    ],
  });

  if (isPending && !handler) {
    return (
      <Pane className="grid h-full place-items-center text-small text-ink-2">
        <span className="inline-flex items-center gap-2.5">
          <Spinner /> Loading the handler
        </span>
      </Pane>
    );
  }
  if (!handler) {
    return (
      <Pane className="grid h-full place-items-center p-8">
        <div className="grid max-w-[48ch] justify-items-start gap-4">
          <Icon name={isError ? 'offline' : 'info'} className="size-8 text-ink-3" />
          <h1 className="type-h2">{isError ? 'The Hub is out of reach' : 'This handler is not on the Hub any more'}</h1>
          <p className="text-ink-2">
            {isError
              ? 'Nucleus Co-op could not load this handler. Check your connection and try again.'
              : 'Its author may have removed it or made it private. Other handlers for the same game may exist.'}
          </p>
          <ButtonLink to="/games" variant="primary">
            <Icon name="arrow-l" />
            Back to Get games
          </ButtonLink>
        </div>
      </Pane>
    );
  }
  return <HandlerDetail handler={handler} all={all} refreshing={isPlaceholderData} />;
}

function HandlerDetail({ handler: h, all, refreshing }: { handler: HubHandler; all: HubHandler[]; refreshing: boolean }) {
  const v = variant(h);
  const inputs = inputSentence(h.inputs);
  const howMany = h.maxPlayers ? `Up to ${h.maxPlayers} players on one PC` : 'Split-screen on one PC';
  const commentsRef = useRef<HTMLElement>(null);

  const byAuthor = useMemo(() => all.filter((x) => x.ownerId === h.ownerId), [all, h.ownerId]);
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
            All games
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
              {howMany}
              {inputs ? `, ${inputs}` : ''}.
            </p>
            <div className="flex flex-wrap items-center gap-x-6 gap-y-4 border-t border-line pt-4">
              <Pips players={h.maxPlayers} size="lg" />
              <InputSupport flags={h.inputs} size="lg" />
              {h.verified && <Verified>Verified handler</Verified>}
            </div>
            <p className="text-small text-ink-3">
              Handler “{h.title}” by{' '}
              <ExternalText href={hubUserUrl(h.ownerId)} className="text-ink-2">
                {h.author}
              </ExternalText>
              , version {h.version}
            </p>
          </div>
        </Pane>
        <SplitPreview handler={h} />
      </section>

      <div className="grid items-start gap-gutter grid-cols-[minmax(0,1fr)_340px] min-[1500px]:grid-cols-[minmax(0,1fr)_360px]">
        <div className="grid min-w-0 gap-gutter">
          <Pane as="section" aria-labelledby="h-compat" className="@container p-8">
            <h2 id="h-compat" className="type-h3 mb-6">
              Compatibility
            </h2>
            <Compat handler={h} />
          </Pane>

          <Pane as="section" aria-labelledby="h-notes" className="p-8">
            <h2 id="h-notes" className="type-h2 mb-6">
              Handler notes
            </h2>
            {h.description.trim() ? (
              <Markdown>{h.description}</Markdown>
            ) : refreshing ? (
              <p className="flex items-center gap-2 text-ink-2">
                <Spinner /> Loading notes…
              </p>
            ) : (
              <p className="max-w-[68ch] text-ink-2">
                {h.author} hasn't written notes for this handler yet. Nucleus shows any notes from the author before you launch, and the comments below often have tips from other players.
              </p>
            )}
            {h.gameDescription && (
              <div className="mt-8 border-t border-line pt-6">
                <h3 className="mb-2 text-16">About {h.gameName}</h3>
                <p className="max-w-[68ch] text-ink-2">{h.gameDescription}</p>
              </div>
            )}
          </Pane>

          <Pane as="section" aria-labelledby="h-steps" className="@container p-8">
            <h2 id="h-steps" className="type-h2 mb-6">
              Play {h.gameName} together
            </h2>
            <Steps handler={h} />
          </Pane>

          <Comments handler={h} sectionRef={commentsRef} />
        </div>

        <aside aria-label="Install this handler" className="sticky top-0">
          <ActionPane handler={h} byAuthor={byAuthor} onComments={() => commentsRef.current?.scrollIntoView({ behavior: 'smooth', block: 'start' })} />
        </aside>
      </div>

      {related.length > 0 && (
        <Pane as="section" aria-labelledby="h-related" className="p-8">
          <div className="mb-6 flex items-end justify-between gap-4">
            <h2 id="h-related" className="type-h2">
              Play these next
            </h2>
            <Link to="/games" className="rounded text-small font-semibold text-ink-2 no-underline hover:text-ink">
              Browse all games
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
      aria-label={`${h.gameName} split into ${seats} player views, the way Nucleus Co-op arranges the windows`}
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
                {(h.maxPlayers ?? 0) - 4} more seats
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
  const cell = (on: boolean | undefined, icon: IconName, label: string, yes: string, no: string, last = false) => (
    <li className={cn('grid content-start gap-2.5 bg-pane p-4', last && 'col-span-2 @min-[840px]:col-span-1')}>
      <span className="min-h-[2.6em] text-small leading-[1.3] text-ink-3">{label}</span>
      <span className={cn('flex items-center gap-2 font-[650]', !on && 'font-medium text-ink-3')}>
        <Icon name={icon} className={cn('size-[22px]', !on && 'opacity-40')} />
        {on === undefined ? 'Not stated' : on ? yes : no}
      </span>
    </li>
  );
  return (
    <ul className="m-0 grid list-none grid-cols-3 gap-px overflow-hidden rounded-[10px] bg-line p-px @min-[840px]:grid-cols-5">
      <li className="grid content-start gap-2.5 bg-pane p-4">
        <span className="min-h-[2.6em] text-small leading-[1.3] text-ink-3">Players</span>
        <span className="flex items-center gap-2 font-[650]">
          <Pips players={h.maxPlayers} size="lg" />
        </span>
      </li>
      {cell(h.inputs.controllers, 'pad', 'Controllers', 'Supported', 'Not supported')}
      {cell(h.inputs.keyboardMouse, 'kbm', 'Keyboard and mouse', 'Supported', 'Not supported')}
      {cell(h.inputs.multiKeyboardMouse, 'multi', 'Several keyboards and mice', 'One per player', 'Not supported')}
      {cell(h.verified, 'seal', 'Hub moderators', 'Verified', 'Not verified yet', true)}
    </ul>
  );
}

// ---------------------------------------------------------------- steps

function Steps({ handler: h }: { handler: HubHandler }) {
  const steps: Array<{ title: string; body: ReactNode; tip?: ReactNode }> = [
    { title: 'Install the handler', body: `One click. Nucleus downloads it from the Hub and adds ${h.gameName} to your library.` },
    { title: 'Point to the game', body: `The first time, Nucleus finds ${h.gameName} on this PC or asks once where it is installed.` },
    {
      title: 'Take a seat',
      body: 'Each player presses Start on their controller to join. Nucleus opens one window per player and tiles them.',
      tip: h.inputs.multiKeyboardMouse ? (
        <>
          Using keyboards and mice? Press <Kbd>End</Kbd> once everything is in place to give each window its own cursor.
        </>
      ) : undefined,
    },
    { title: 'Connect in game', body: 'Follow the handler notes to link the windows: usually one player hosts and the others join.' },
  ];
  return (
    <ol className="m-0 grid list-none grid-cols-2 gap-6 p-0 @min-[860px]:grid-cols-4">
      {steps.map((s, i) => (
        <li key={s.title} className="grid content-start gap-2">
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
  const [all, setAll] = useState(false);
  const FIRST = 5;
  const count = threads ? threads.reduce((n, t) => n + 1 + t.replies.length, 0) : h.comments;
  return (
    <Pane as="section" aria-labelledby="h-comments" className="scroll-mt-1 p-8" ref={sectionRef}>
      <div className="mb-6 flex flex-wrap items-baseline gap-3">
        <h2 id="h-comments" className="type-h3">
          Comments <span className="data font-medium text-ink-3">{count}</span>
        </h2>
        <Button size="sm" variant="ghost" className="ml-auto" onClick={() => void bridge.app.openExternal({ url: hubHandlerUrl(h.id) }).catch(() => undefined)}>
          <Icon name="comment" />
          Comment on the Hub
        </Button>
      </div>
      {isPending ? (
        <p className="flex items-center gap-2 text-small text-ink-2">
          <Spinner /> Loading comments…
        </p>
      ) : isError ? (
        <p className="flex items-center gap-3 text-small text-ink-2">
          Comments could not be loaded.
          <Button size="sm" variant="ghost" onClick={() => void refetch()}>
            Try again
          </Button>
        </p>
      ) : !threads?.length ? (
        <p className="text-15 text-ink-2">No comments yet. Played it? Share what worked for your group on the Hub.</p>
      ) : (
        <ul className="m-0 grid max-w-[76ch] list-none gap-6 p-0">
          {(all ? threads : threads.slice(0, FIRST)).map((t) => (
            <CommentItem key={t.id} comment={t} />
          ))}
        </ul>
      )}
      {threads && threads.length > FIRST && !all && (
        <Button variant="ghost" className="mt-6" onClick={() => setAll(true)} data-nav>
          Show {threads.length - FIRST} more {threads.length - FIRST === 1 ? 'thread' : 'threads'}
        </Button>
      )}
    </Pane>
  );
}

function CommentItem({ comment: c }: { comment: HubCommentThread }) {
  return (
    <li className="grid grid-cols-[36px_minmax(0,1fr)] gap-3">
      <Avatar name={c.author} />
      <div className="min-w-0">
        <div className="flex flex-wrap items-baseline gap-2">
          <b className="text-15 font-[650]">{c.author}</b>
          <time dateTime={c.createdAt} title={fmtDate(c.createdAt)} className="text-small text-ink-3">
            {fmtAgo(c.createdAt)}
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
                    <time dateTime={r.createdAt} title={fmtDate(r.createdAt)} className="text-13 text-ink-3">
                      {fmtAgo(r.createdAt)}
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
