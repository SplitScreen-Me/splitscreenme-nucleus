import { useEffect, useMemo, useRef, useState } from 'react';
import { useSearchParams } from 'react-router';
import {
  buildIndex,
  countWith,
  EMPTY_FILTERS,
  genreLabel,
  genreStats,
  INPUTS,
  PLAYERS,
  readState,
  results,
  SORTS,
  useAllHandlers,
  writeState,
  type CatalogState,
  type HubHandler,
  type InputFilter,
  type SortKey,
} from '@/hub';
import { bridge } from '@/bridge';
import { LINKS } from '@/lib/links';
import { igdbImage } from '@/lib/art';
import { fmtDate, plural } from '@/lib/format';
import { useInstalledHandlers, useLibrary } from '@/lib/library';
import { useScreenChrome } from '@/shell/chrome';
import { Button, Field, Icon, Pane, Segmented, Select, Spinner, Switch, cn } from '@/ui';
import { HandlerCard, HandlerCardSkeleton } from './HandlerCard';
import { installStatuses } from './install-state';

const PAGE = 48;
const SHORTCUTS = 6;

export default function CatalogScreen() {
  const { data: handlers, isPending, isError, error, refetch, isFetching, dataUpdatedAt } = useAllHandlers();
  const { data: installed = [] } = useInstalledHandlers();
  const { data: library = [] } = useLibrary();
  const [params, setParams] = useSearchParams();
  const all = useMemo(() => handlers ?? [], [handlers]);

  const genres = useMemo(() => genreStats(all), [all]);
  const genreSet = useMemo(() => new Set(genres.map((g) => g.genre)), [genres]);
  const paramsKey = params.toString();
  const state = useMemo(() => readState(new URLSearchParams(paramsKey), all.length ? genreSet : undefined), [paramsKey, genreSet, all.length]);
  const index = useMemo(() => buildIndex(all), [all]);
  const list = useMemo(() => results(index, state), [index, state]);
  const statuses = useMemo(() => installStatuses(installed, library, all), [installed, library, all]);

  const [shown, setShown] = useState(PAGE);
  const [q, setQ] = useState(state.q);
  const [prevQ, setPrevQ] = useState(state.q);
  const search = useRef<HTMLInputElement>(null);
  const shelf = useRef<HTMLUListElement>(null);
  const typing = useRef<number | undefined>(undefined);

  // The URL is the source of truth: follow it when it changes from outside (palette, links, tokens).
  if (state.q !== prevQ) {
    setPrevQ(state.q);
    if (state.q !== q.trim()) setQ(state.q);
  }
  useEffect(() => () => window.clearTimeout(typing.current), []);

  const update = (patch: Partial<CatalogState>) => {
    setShown(PAGE);
    setParams(writeState({ ...state, ...patch }), { replace: true });
  };

  // Typing filters after a short pause; the URL keeps the query.
  const onType = (value: string) => {
    setQ(value);
    window.clearTimeout(typing.current);
    typing.current = window.setTimeout(() => update({ q: value.trim() }), 120);
  };

  useScreenChrome([{ label: 'Get games' }], {
    base: [
      { button: 'A', label: 'Select' },
      { button: 'Y', label: 'Search', action: () => search.current?.focus() },
      { button: 'LB', label: 'Previous section' },
      { button: 'RB', label: 'Next section' },
    ],
  });

  const tokens: Array<{ key: string; label: string; clear: Partial<CatalogState> }> = [];
  if (state.q) tokens.push({ key: 'q', label: `“${state.q}”`, clear: { q: '' } });
  if (state.players) tokens.push({ key: 'players', label: `${PLAYERS.find((p) => p.key === state.players)?.label} players`, clear: { players: '' } });
  for (const k of state.inputs) tokens.push({ key: `input-${k}`, label: INPUTS.find((i) => i.key === k)!.label, clear: { inputs: state.inputs.filter((x) => x !== k) } });
  if (state.verified) tokens.push({ key: 'verified', label: 'Verified', clear: { verified: false } });
  if (state.genre) tokens.push({ key: 'genre', label: genreLabel(state.genre), clear: { genre: '' } });

  const clearAll = () => {
    setQ('');
    update({ ...EMPTY_FILTERS });
  };
  const total = all.length;
  const left = list.length - shown;

  return (
    <div className="grid gap-gutter">
      {/* ---------- Search pane ---------- */}
      <Pane as="section" aria-labelledby="cat-title" className="grid gap-6 px-8 pt-10 pb-8">
        <div className="grid items-end gap-8 grid-cols-[minmax(0,1fr)_minmax(0,0.8fr)]">
          <h1 id="cat-title" className="type-h1 max-w-[14ch]">
            Find a game to play together
          </h1>
          <p className="m-0 max-w-[46ch] justify-self-end text-ink-2">
            Every game here has a community handler. Pick one, install it in one click, and hand out the controllers.
          </p>
        </div>
        <Field
          ref={search}
          size="lg"
          icon="search"
          type="search"
          value={q}
          onChange={(e) => onType(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'Enter') update({ q: q.trim() });
            if (e.key === 'ArrowDown') shelf.current?.querySelector<HTMLElement>('a')?.focus();
          }}
          placeholder="Search by game or handler author"
          autoComplete="off"
          aria-label="Search games"
          aria-describedby="cat-count"
          trailing={
            <span id="cat-count" className="data shrink-0 text-15 whitespace-nowrap text-ink-2" aria-live="polite">
              {total ? (list.length === total ? `${total} games` : `${list.length} of ${total}`) : ''}
            </span>
          }
        />
        {genres.length > 0 && (
          <div className="-mx-1 flex gap-2 overflow-x-auto px-1 pb-0.5" role="group" aria-label="Genres">
            {genres.slice(0, SHORTCUTS).map(({ genre, count }) => (
              <GenreShortcut
                key={genre}
                genre={genre}
                count={count}
                covers={top3(all, genre)}
                pressed={state.genre === genre}
                onClick={() => update({ genre: state.genre === genre ? '' : genre })}
              />
            ))}
          </div>
        )}
      </Pane>

      {/* ---------- Body: filters + results ---------- */}
      <div className="grid items-start gap-gutter grid-cols-[272px_minmax(0,1fr)] min-[1400px]:grid-cols-[288px_minmax(0,1fr)]">
        <Pane as="aside" aria-label="Filters" className="sticky top-0 grid max-h-[calc(100vh-88px)] gap-6 overflow-y-auto p-6">
          <div role="group" aria-labelledby="f-players" className="grid min-w-0">
            <span id="f-players" className="mb-3 block text-small font-[650] text-ink">
              Maximum players
            </span>
            <Segmented
              label="Maximum players"
              value={state.players}
              onChange={(players) => update({ players })}
              options={PLAYERS.map((p) => ({ value: p.key, label: p.label, ariaLabel: p.key ? `${p.label} players` : 'Any number of players' }))}
            />
            <p className="mt-2 text-micro text-ink-3">The most players the handler supports at once.</p>
          </div>

          <div role="group" aria-labelledby="f-inputs" className="grid min-w-0 border-t border-line pt-6">
            <span id="f-inputs" className="mb-3 block text-small font-[650] text-ink">
              Works with
            </span>
            {INPUTS.map((i) => {
              const on = state.inputs.includes(i.key);
              const n = countWith(index, state, { inputs: [...new Set<InputFilter>([...state.inputs, i.key])] });
              return (
                <label key={i.key} className="-mx-2.5 flex cursor-pointer items-center gap-2.5 rounded-ctl px-2.5 py-2 text-14 text-ink-2 hover:bg-pane-2 hover:text-ink min-[1400px]:gap-3 min-[1400px]:text-15">
                  <input
                    type="checkbox"
                    className="peer sr-only"
                    checked={on}
                    onChange={(e) => update({ inputs: e.target.checked ? [...state.inputs, i.key] : state.inputs.filter((x) => x !== i.key) })}
                  />
                  <span className="grid size-[18px] shrink-0 place-items-center rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)] peer-checked:bg-ink peer-checked:shadow-none peer-focus-visible:outline-2 peer-focus-visible:outline-offset-3 peer-focus-visible:outline-ink">
                    <Icon name="check" strokeWidth={2.5} className={cn('size-3.5 text-on-light', !on && 'opacity-0')} />
                  </span>
                  <Icon name={i.icon} className="size-[18px] text-ink-3" />
                  <span className={cn(on && 'text-ink')}>{i.label}</span>
                  <span className="data ml-auto text-small text-ink-3">{n}</span>
                </label>
              );
            })}
          </div>

          <div className="flex items-center justify-between gap-3 border-t border-line pt-6">
            <span id="f-verified" className="text-15 text-ink">
              Verified only
              <small className="block text-micro text-ink-3">Checked by Hub moderators</small>
            </span>
            <Switch checked={state.verified} onChange={(verified) => update({ verified })} aria-labelledby="f-verified" />
          </div>

          <div role="group" aria-labelledby="f-genres" className="grid min-w-0 border-t border-line pt-6">
            <span id="f-genres" className="mb-3 block text-small font-[650] text-ink">
              Genres
            </span>
            <ul className="m-0 grid list-none gap-0.5 p-0">
              {genres.map(({ genre }) => (
                <li key={genre}>
                  <button
                    type="button"
                    aria-pressed={state.genre === genre}
                    onClick={() => update({ genre: state.genre === genre ? '' : genre })}
                    className={cn(
                      '-mx-2.5 flex w-[calc(100%+20px)] items-center justify-between gap-2 rounded-ctl border-0 px-2.5 py-2 text-left text-15',
                      state.genre === genre ? 'bg-pane-3 font-semibold text-ink' : 'bg-transparent text-ink-2 hover:bg-pane-2 hover:text-ink',
                    )}
                  >
                    <span>{genreLabel(genre)}</span>
                    <span className="data text-small text-ink-3">{countWith(index, state, { genre }, 'genre')}</span>
                  </button>
                </li>
              ))}
            </ul>
          </div>

          {tokens.length > 0 && (
            <button type="button" onClick={clearAll} className="justify-self-start border-0 bg-transparent p-0 text-small font-semibold text-ink-2 underline underline-offset-3 hover:text-ink">
              Clear all filters
            </button>
          )}
        </Pane>

        <Pane as="section" aria-labelledby="res-summary" className="min-h-[70vh] px-8 pt-6 pb-12">
          {isError && handlers?.length ? (
            <div role="status" className="mb-6 flex items-center gap-3 rounded-ctl bg-pane-2 px-4 py-3 text-small text-ink-2">
              <Icon name="offline" className="size-[18px] text-warn" />
              <span>
                The Hub is unreachable. Showing the catalogue saved on <b className="font-semibold text-ink">{fmtDate(new Date(dataUpdatedAt).toISOString())}</b>.
              </span>
              <Button size="sm" variant="ghost" className="ml-auto" onClick={() => void refetch()} disabled={isFetching}>
                {isFetching ? <Spinner /> : <Icon name="refresh" />}
                Try again
              </Button>
            </div>
          ) : null}
          <div className="mb-8 flex flex-wrap items-center gap-3">
            <p id="res-summary" className="text-15 text-ink-2" aria-live="polite">
              <b className="font-[650] text-ink">
                {isPending ? 'Loading the catalogue…' : list.length === total ? `All ${total} games` : list.length ? plural(list.length, 'game') : 'No games'}
              </b>
            </p>
            <div className="flex flex-wrap gap-1.5">
              {tokens.map((t) => (
                <button
                  key={t.key}
                  type="button"
                  onClick={() => {
                    if (t.key === 'q') setQ('');
                    update(t.clear);
                  }}
                  aria-label={`Remove filter ${t.label}`}
                  className="group/token inline-flex h-[30px] items-center gap-1.5 rounded-tag border-0 bg-pane-3 pr-2 pl-3 text-13 font-[560] text-ink"
                >
                  {t.label}
                  <Icon name="x" className="size-3.5 text-ink-2 group-hover/token:text-ink" />
                </button>
              ))}
            </div>
            <label className="ml-auto flex items-center gap-2 text-small text-ink-3">
              Sort by
              <Select value={state.sort} onChange={(e) => update({ sort: e.target.value as SortKey })} aria-label="Sort games">
                {SORTS.map((s) => (
                  <option key={s.key} value={s.key}>
                    {s.label}
                  </option>
                ))}
              </Select>
            </label>
          </div>

          {isPending ? (
            <ul className="m-0 grid list-none grid-cols-[repeat(auto-fill,minmax(152px,1fr))] gap-x-[18px] gap-y-[34px] p-0" aria-hidden="true">
              {Array.from({ length: 12 }, (_, i) => (
                <li key={i}>
                  <HandlerCardSkeleton />
                </li>
              ))}
            </ul>
          ) : isError && !handlers?.length ? (
            <Unreachable message={(error as Error | null)?.message} onRetry={() => void refetch()} retrying={isFetching} />
          ) : list.length ? (
            <ul ref={shelf} className="m-0 grid list-none grid-cols-[repeat(auto-fill,minmax(152px,1fr))] gap-x-[18px] gap-y-[34px] p-0">
              {list.slice(0, shown).map((h, i) => (
                <li key={h.id}>
                  <HandlerCard handler={h} eager={i < 12} status={statuses.get(h.id)} />
                </li>
              ))}
            </ul>
          ) : (
            <NoMatch query={state.q} onClear={clearAll} />
          )}

          {left > 0 && (
            <div className="mt-12 flex flex-col items-center gap-3">
              <Button
                variant="ghost"
                size="lg"
                data-nav
                onClick={() => {
                  const first = shown;
                  setShown((s) => s + PAGE);
                  requestAnimationFrame(() => shelf.current?.children[first]?.querySelector<HTMLElement>('a')?.focus());
                }}
              >
                Show {Math.min(PAGE, left)} more
              </Button>
              <p className="text-small text-ink-3">
                Showing {Math.min(shown, list.length)} of {list.length}
              </p>
            </div>
          )}
        </Pane>
      </div>
    </div>
  );
}

/** Covers of the three most popular games in a genre (one per game: several handlers can share a cover). */
function top3(all: HubHandler[], genre: string): string[] {
  const covers = new Set<string>();
  for (const h of [...all].sort((a, b) => b.trendScore - a.trendScore)) {
    if (h.genres.includes(genre) && h.coverImageId) covers.add(h.coverImageId);
    if (covers.size === 3) break;
  }
  return [...covers].map((id) => igdbImage(id, 'cover_small_2x')!);
}

function GenreShortcut({ genre, count, covers, pressed, onClick }: { genre: string; count: number; covers: string[]; pressed: boolean; onClick: () => void }) {
  return (
    <button
      type="button"
      aria-pressed={pressed}
      onClick={onClick}
      data-nav
      className={cn(
        'inline-flex h-14 shrink-0 items-center gap-3 rounded-xl border-0 pr-4 pl-2 text-left transition-colors duration-(--dur-1)',
        pressed ? 'bg-ink text-on-light' : 'bg-pane-2 text-ink hover:bg-pane-3',
      )}
    >
      <span className="flex" aria-hidden="true">
        {covers.map((src, i) => (
          <img
            key={src}
            src={src}
            alt=""
            loading="lazy"
            className={cn('h-10 w-[30px] rounded-[5px] object-cover', i > 0 && '-ml-3', pressed ? 'shadow-[0_0_0_2px_var(--color-ink)]' : 'shadow-[0_0_0_2px_var(--color-pane-2)]')}
          />
        ))}
      </span>
      <span>
        <b className="block text-15 leading-tight font-[650]">{genreLabel(genre)}</b>
        <small className={cn('data block text-micro', pressed ? 'text-black/60' : 'text-ink-3')}>{count} games</small>
      </span>
    </button>
  );
}

function NoMatch({ query, onClear }: { query: string; onClear: () => void }) {
  return (
    <div className="grid max-w-[52ch] justify-items-start gap-4 py-12">
      <div aria-hidden="true" className="grid aspect-[16/10] w-[120px] grid-cols-2 gap-1">
        <i className="rounded-[5px] bg-pane-3" />
        <i className="rounded-[5px] shadow-[inset_0_0_0_1.5px_var(--color-line-strong)]" />
      </div>
      <h2 className="type-h2 text-[1.5rem]">No handler matches yet</h2>
      <p className="text-ink-2">
        {query
          ? `Nothing for “${query}” with these filters. Check the spelling, try fewer filters, or ask the community: handler authors pick requests from Discord.`
          : 'Try fewer filters, or ask the community for the game you want: handler authors pick requests from Discord.'}
      </p>
      <div className="flex flex-wrap gap-2">
        <Button variant="primary" onClick={onClear} data-nav>
          Clear filters
        </Button>
        <Button variant="ghost" data-nav onClick={() => void bridge.app.openExternal({ url: LINKS.discord }).catch(() => undefined)}>
          <Icon name="discord" />
          Request it on Discord
        </Button>
      </div>
    </div>
  );
}

function Unreachable({ message, onRetry, retrying }: { message?: string; onRetry: () => void; retrying: boolean }) {
  return (
    <div className="grid max-w-[52ch] justify-items-start gap-4 py-12">
      <Icon name="offline" className="size-8 text-warn" />
      <h2 className="type-h2 text-[1.5rem]">The Hub is out of reach</h2>
      <p className="text-ink-2">
        {message ?? 'The catalogue could not be loaded.'} Your library still works offline. The catalogue appears here as soon as Nucleus Co-op reaches the Hub once.
      </p>
      <Button variant="primary" onClick={onRetry} disabled={retrying} data-nav>
        {retrying ? <Spinner /> : <Icon name="refresh" />}
        Try again
      </Button>
    </div>
  );
}
