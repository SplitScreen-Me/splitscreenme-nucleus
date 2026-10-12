import { useMemo, useState } from 'react';
import { Link } from 'react-router';
import type { GameSummary } from '@/bridge';
import { useAllHandlers, type HubHandler } from '@/hub';
import { igdbImage } from '@/lib/art';
import { fmtCount } from '@/lib/format';
import { useInstalledHandlers } from '@/lib/library';
import { igdbResize, useScreenshots } from '@/lib/screenshots';
import { Pane } from '@/ui';
import { nameKeys } from './found-matching';

/** Discover teaser: popular handlers for 4 players or more that the user doesn't have yet. */
export function HubStrip({ owned, offered }: { owned: readonly GameSummary[]; offered: ReadonlySet<string> }) {
  const { data: handlers = [] } = useAllHandlers();
  const { data: installed = [] } = useInstalledHandlers();
  const picks = useMemo(() => {
    // Skip what the user has, and what "Found on this PC" already offers.
    const ownedIds = new Set([...owned.flatMap((g) => (g.handlerId ? [g.handlerId] : [])), ...installed.flatMap((h) => (h.handlerId ? [h.handlerId] : [])), ...offered]);
    const ownedNames = new Set(owned.flatMap((g) => nameKeys(g.name)));
    const seen = new Set<string>();
    return [...handlers]
      .sort((a, b) => b.trendScore - a.trendScore || b.downloads - a.downloads)
      .filter((h) => {
        const keys = nameKeys(h.gameName);
        if ((h.maxPlayers ?? 0) < 4 || ownedIds.has(h.id) || keys.some((k) => ownedNames.has(k) || seen.has(k))) return false;
        keys.forEach((k) => seen.add(k));
        return true;
      })
      .slice(0, 5);
  }, [handlers, owned, installed, offered]);

  if (!picks.length) return null;
  return (
    <Pane as="section" aria-labelledby="pop-title" className="col-span-full px-6 pt-[22px] pb-6">
      <div className="mb-4 flex items-center gap-3.5">
        <h2 id="pop-title" className="type-h3">
          Popular for 4 players or more on the Hub
        </h2>
        <Link to="/games?players=3-4" className="ml-auto rounded text-small font-semibold text-ink-2 no-underline hover:text-ink">
          Open Get games
        </Link>
      </div>
      <ul className="m-0 grid list-none grid-cols-4 gap-3 p-0 min-[1321px]:grid-cols-5">
        {picks.map((h, i) => (
          <li key={h.id} className={i === 4 ? 'hidden min-[1321px]:block' : undefined}>
            <HubTile handler={h} />
          </li>
        ))}
      </ul>
    </Pane>
  );
}

function HubTile({ handler: h }: { handler: HubHandler }) {
  const { data: shots } = useScreenshots(h.id);
  const [failed, setFailed] = useState(false);
  const shot = igdbResize(shots?.[0], 'screenshot_big');
  const src = !failed && shot ? shot : igdbImage(h.coverImageId, 'cover_big_2x');
  return (
    <Link
      to={`/games/${encodeURIComponent(h.id)}`}
      data-nav
      data-nav-frame="own"
      data-hints="hub"
      data-hub-id={h.id}
      aria-label={`${h.gameName} on the Hub`}
      className="group relative block aspect-video overflow-hidden rounded-cover bg-pane-2 text-ink no-underline outline-3 outline-offset-3 outline-transparent transition-[outline-color] duration-(--dur-1) nav-focus:outline-ink"
    >
      {src && (
        <img
          src={src}
          alt=""
          loading="lazy"
          onError={() => setFailed(true)}
          className="size-full object-cover transition-transform duration-(--dur-3) ease-snap group-hover:scale-[1.03]"
          style={!shot || failed ? { objectPosition: '50% 30%' } : undefined}
        />
      )}
      <span
        aria-hidden="true"
        className="absolute inset-0"
        style={{ background: 'linear-gradient(0deg, rgba(7,8,10,0.94) 0%, rgba(7,8,10,0.55) 45%, rgba(7,8,10,0) 80%)' }}
      />
      <span className="absolute inset-x-3 bottom-2.5 z-[1]">
        <b className="block truncate text-small leading-tight font-[650]">{h.gameName}</b>
        <span className="data mt-0.5 block text-12 text-ink-2">
          Up to {h.maxPlayers} players, {fmtCount(h.downloads)} downloads
        </span>
      </span>
    </Link>
  );
}
