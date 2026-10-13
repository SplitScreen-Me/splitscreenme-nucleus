import type { ReactNode } from 'react';
import { Link } from 'react-router';
import type { GameSummary, LibraryPreferences, LibrarySort } from '@/bridge';
import { useI18n, useT, type TFunction } from '@/i18n';
import { downloadProgress, useActiveDownload } from '@/lib/downloads';
import { gameStatus, setupPath } from '@/lib/library';
import { Button, Chip, Cover, Icon, Pane, Select, Spinner, cn } from '@/ui';
import { INPUT_FILTERS, SORT_OPTIONS, needsAttention, visibleGames } from './library-view';

export interface YourGamesProps {
  games: GameSummary[];
  prefs: LibraryPreferences;
  onPrefs: (patch: Partial<LibraryPreferences>) => void;
  attentionOnly: boolean;
  onAttentionOnly: (on: boolean) => void;
  onAddGame: () => void;
  adding: boolean;
  onLocate: (g: GameSummary) => void;
  onUpdate: (g: GameSummary) => void;
  onFavorite: (g: GameSummary) => void;
}

/** Every game of the library, sorted and filtered as the user saved it. */
export function YourGames({ games, prefs, onPrefs, attentionOnly, onAttentionOnly, onAddGame, adding, onLocate, onUpdate, onFavorite }: YourGamesProps) {
  const t = useT();
  const attention = games.filter(needsAttention).length;
  const favorites = games.filter((g) => g.favorite).length;
  const visible = visibleGames(games, prefs, attentionOnly);
  const filtered = prefs.favoritesOnly || attentionOnly || INPUT_FILTERS.some((f) => prefs[f.key]);

  return (
    <Pane as="section" aria-labelledby="games-title" className="col-span-full px-6 pt-6 pb-7">
      <div className="mb-3.5 flex flex-wrap items-center gap-3.5">
        <h2 id="games-title" className="type-h2 mr-0.5">
          {t('library.games.title')}
        </h2>
        <span className="data text-16 text-ink-3">
          {filtered ? t('library.games.countFiltered', { shown: visible.length, total: games.length }) : t('library.games.count', { count: games.length })}
        </span>
        <div className="ml-auto flex items-center gap-2">
          <label className="flex items-center gap-2 text-small text-ink-3">
            {t('library.games.sort')}
            <Select
              value={prefs.sort}
              onChange={(e) => onPrefs({ sort: e.target.value as LibrarySort })}
              aria-label={t('library.games.sortLabel')}
              data-nav
              data-hints="chip"
            >
              {SORT_OPTIONS.map((o) => (
                <option key={o.value} value={o.value}>
                  {t(o.label)}
                </option>
              ))}
            </Select>
          </label>
          <Button variant="ghost" size="sm" onClick={onAddGame} disabled={adding} data-nav data-hints="add">
            {adding ? <Spinner /> : <Icon name="plus" />}
            {t('library.games.addGame')}
          </Button>
        </div>
      </div>
      <div className="mb-5 flex flex-wrap gap-1.5" role="toolbar" aria-label={t('library.games.filtersLabel')}>
        <Chip pressed={prefs.favoritesOnly} count={favorites || undefined} onClick={() => onPrefs({ favoritesOnly: !prefs.favoritesOnly })} data-nav data-hints="chip">
          <Icon name="star" fill={prefs.favoritesOnly ? 'currentColor' : 'none'} />
          {t('library.games.favourites')}
        </Chip>
        <span aria-hidden="true" className="mx-1 my-1.5 w-px bg-line" />
        {INPUT_FILTERS.map((f) => (
          <Chip key={f.key} pressed={prefs[f.key]} onClick={() => onPrefs({ [f.key]: !prefs[f.key] })} data-nav data-hints="chip">
            <Icon name={f.icon} />
            {t(f.label)}
          </Chip>
        ))}
        {(attention > 0 || attentionOnly) && (
          <>
            <span aria-hidden="true" className="mx-1 my-1.5 w-px bg-line" />
            <Chip pressed={attentionOnly} count={attention || undefined} onClick={() => onAttentionOnly(!attentionOnly)} data-nav data-hints="chip">
              <Icon name="warning" />
              {t('library.games.needsAttention')}
            </Chip>
          </>
        )}
      </div>
      {visible.length ? (
        <ul className="m-0 grid list-none grid-cols-[repeat(auto-fill,minmax(146px,1fr))] gap-x-4 gap-y-[22px] p-0">
          {visible.map((g) => (
            <li key={g.id}>
              <GameCard game={g} onLocate={onLocate} onUpdate={onUpdate} onFavorite={onFavorite} />
            </li>
          ))}
        </ul>
      ) : (
        <div className="grid justify-items-start gap-3 py-8">
          <p className="text-small text-ink-2">
            {attentionOnly && !needsAttentionAny(games)
              ? t('library.games.allReady')
              : t('library.games.noMatch')}
          </p>
          <Button
            variant="ghost"
            size="sm"
            onClick={() => {
              onAttentionOnly(false);
              onPrefs({ favoritesOnly: false, filterGamepads: false, filterKeyboard: false, filterMultipleKeyboards: false });
            }}
            data-nav
          >
            {t('library.games.showAll')}
          </Button>
        </div>
      )}
    </Pane>
  );
}

const needsAttentionAny = (games: readonly GameSummary[]) => games.some(needsAttention);

const artClass =
  'outline-3 outline-offset-3 outline-transparent transition-[transform,outline-color] duration-(--dur-2) ease-snap ' +
  'group-hover:-translate-y-[3px] group-nav-focus:-translate-y-[5px] group-nav-focus:outline-ink';

/**
 * A box the size of the cover, above the card link, for the card's own buttons. Pads use the card's hints instead
 * (View favourite, X update), so the buttons are out of their reach.
 */
function CoverOverlay({ children }: { children: ReactNode }) {
  return (
    <div data-nav="off" className="pointer-events-none absolute inset-x-0 top-0 aspect-[2/3]">
      {children}
    </div>
  );
}

function FavoriteButton({ game: g, onFavorite }: { game: GameSummary; onFavorite: (g: GameSummary) => void }) {
  const t = useT();
  return (
    <button
      type="button"
      aria-pressed={g.favorite}
      aria-label={g.favorite ? t('library.card.removeFavourite', { game: g.name }) : t('library.card.addFavourite', { game: g.name })}
      title={g.favorite ? t('library.card.favouriteTitle') : t('library.card.addFavouriteTitle')}
      onClick={() => onFavorite(g)}
      className={cn(
        'pointer-events-auto absolute top-2 right-2 grid size-8 place-items-center rounded-full border-0 bg-[rgba(7,8,10,0.72)] text-ink backdrop-blur-sm',
        'transition-opacity duration-(--dur-1) hover:bg-[rgba(7,8,10,0.9)] focus-visible:opacity-100',
        g.favorite ? 'opacity-100' : 'opacity-0 group-hover/card:opacity-100 group-focus-within/card:opacity-100',
      )}
    >
      <Icon name="star" fill={g.favorite ? 'currentColor' : 'none'} className="size-[17px]" />
    </button>
  );
}

/** What a screen reader says of a card: "Hades, favourite, handler update available". */
function cardLabel(t: TFunction, g: GameSummary, flag: 'update' | 'new' | undefined): string {
  const game = g.name;
  if (flag === 'update') return g.favorite ? t('library.card.labelFavouriteUpdate', { game }) : t('library.card.labelUpdate', { game });
  if (flag === 'new') return g.favorite ? t('library.card.labelFavouriteNew', { game }) : t('library.card.labelNew', { game });
  return g.favorite ? t('library.card.labelFavourite', { game }) : game;
}

function GameCard({
  game: g,
  onLocate,
  onUpdate,
  onFavorite,
}: {
  game: GameSummary;
  onLocate: (g: GameSummary) => void;
  onUpdate: (g: GameSummary) => void;
  onFavorite: (g: GameSummary) => void;
}) {
  const { t, format } = useI18n();
  const status = gameStatus(g);
  const updating = useActiveDownload({ gameId: g.id });

  if (status === 'missing') {
    return (
      <div className="group/card relative">
        <button
          type="button"
          onClick={() => onLocate(g)}
          data-nav
          data-nav-frame="own"
          data-hints="missing"
          data-id={g.id}
          aria-label={t('library.card.missingLabel', { game: g.name })}
          className="group block w-full rounded-xl border-0 bg-transparent p-0 text-left text-inherit outline-none"
        >
          <Cover src={g.coverUrl} title={g.name} ratio="2/3" className={artClass} imgClassName="grayscale brightness-[0.45]">
            <div className="absolute inset-x-2 bottom-2 grid gap-2 rounded-ctl bg-[rgba(7,8,10,0.82)] p-2.5 text-13 leading-[1.3] text-ink backdrop-blur-sm">
              <span className="flex items-start gap-1.5">
                <Icon name="warning" className="mt-px size-[15px] shrink-0 text-warn" />
                {t('library.card.notFound')}
              </span>
              <span className="grid h-7 place-items-center rounded-md bg-ink text-13 font-bold text-on-light">{t('library.card.locate')}</span>
            </div>
          </Cover>
          <div className="mt-2.5 truncate text-15 leading-tight font-[650]">{g.name}</div>
          <div className="data mt-0.5 text-13 text-warn">{t('library.card.executableMissing')}</div>
        </button>
        <CoverOverlay>
          <FavoriteButton game={g} onFavorite={onFavorite} />
        </CoverOverlay>
      </div>
    );
  }

  const update = status === 'update';
  const progress = format.percent(updating ? downloadProgress(updating) : 0);
  return (
    <div className="group/card relative">
      <Link
        to={setupPath(g.id)}
        data-nav
        data-nav-frame="own"
        data-hints={update ? 'update' : 'game'}
        data-id={g.id}
        aria-label={cardLabel(t, g, update ? 'update' : status === 'new' ? 'new' : undefined)}
        className="group block rounded-xl text-inherit no-underline outline-none"
      >
        <Cover src={g.coverUrl} title={g.name} ratio="2/3" className={artClass}>
          {update && (
            <span className="absolute top-2 left-2 inline-flex h-6 items-center gap-[5px] rounded-tag bg-ink pr-[9px] pl-[7px] text-12 font-[750] text-on-light stretch-narrow">
              {updating ? <Spinner className="size-3 border-on-light/30 border-t-on-light" /> : <Icon name="update" strokeWidth={2.2} className="size-3.5" />}
              {updating ? progress : t('library.card.versionBadge', { version: String(g.latestHandlerVersion) })}
            </span>
          )}
          {status === 'new' && (
            <span className="absolute top-2 left-2 inline-flex h-6 items-center rounded-tag bg-pane px-[9px] text-12 font-[750] text-ink shadow-[inset_0_0_0_1px_var(--color-line-strong)] stretch-narrow">
              {t('library.card.new')}
            </span>
          )}
        </Cover>
        <div className="mt-2.5 truncate text-15 leading-tight font-[650]">{g.name}</div>
        <div className={cn('data mt-0.5 text-13', update ? 'text-warn' : 'text-ink-3')}>
          {update
            ? updating
              ? t('library.card.updatingHandler', { percent: progress })
              : t('library.card.handlerUpdate', { from: g.handlerVersion !== undefined ? String(g.handlerVersion) : '?', to: String(g.latestHandlerVersion) })
            : t('library.upToPlayers', { count: g.maxPlayers })}
        </div>
      </Link>
      <CoverOverlay>
        <FavoriteButton game={g} onFavorite={onFavorite} />
        {update && !updating && (
          <button
            type="button"
            onClick={() => onUpdate(g)}
            aria-label={t('library.card.updateLabel', { game: g.name, version: String(g.latestHandlerVersion) })}
            className="pointer-events-auto absolute inset-x-2 bottom-2 grid h-8 place-items-center rounded-md border-0 bg-ink text-13 font-bold text-on-light opacity-0 transition-opacity duration-(--dur-1) group-hover/card:opacity-100 focus-visible:opacity-100"
          >
            {t('library.card.updateTo', { version: String(g.latestHandlerVersion) })}
          </button>
        )}
      </CoverOverlay>
    </div>
  );
}
