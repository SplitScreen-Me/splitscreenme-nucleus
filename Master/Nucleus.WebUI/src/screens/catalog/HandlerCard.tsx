import { Link } from 'react-router';
import { variant, type HubHandler } from '@/hub';
import { igdbImage } from '@/lib/art';
import { fmtCount, fmtNumber } from '@/lib/format';
import { cn, Cover, Icon, InputSupport, Pips, type IconName } from '@/ui';
import type { HandlerInstallStatus } from './install-state';

const BADGES: Partial<Record<HandlerInstallStatus, { icon: IconName; label: string; title: string; solid: boolean }>> = {
  library: { icon: 'check', label: 'In library', title: 'Installed, the game is in your library', solid: true },
  installed: { icon: 'download', label: 'Installed', title: 'Handler installed, the game is not in your library yet', solid: false },
  update: { icon: 'update', label: 'Update', title: 'A newer version of this handler is on the Hub', solid: true },
};

/** A game on the shelf: cover, name, author, downloads, seats and inputs. */
export function HandlerCard({ handler: h, eager, status }: { handler: HubHandler; eager?: boolean; status?: HandlerInstallStatus }) {
  const v = variant(h);
  const badge = status ? BADGES[status] : undefined;
  return (
    <Link
      to={`/games/${encodeURIComponent(h.id)}`}
      data-nav
      data-nav-frame="own"
      data-hints="card"
      className="group flex flex-col gap-3 rounded-xl text-ink no-underline outline-none"
    >
      <Cover
        src={igdbImage(h.coverImageId)}
        title={h.gameName}
        eager={eager}
        className="transition-shadow duration-(--dur-1) ease-snap group-hover:shadow-[0_0_0_2px_var(--color-bezel),0_0_0_4px_var(--color-line-strong)] group-nav-focus:shadow-[0_0_0_2px_var(--color-bezel),0_0_0_4px_var(--color-ink)]"
      >
        {h.verified && (
          <span title="Verified handler" className="absolute top-2 left-2 grid size-7 place-items-center rounded-ctl bg-black/75 text-ink backdrop-blur-sm">
            <Icon name="seal" label="Verified handler" className="size-[17px]" />
          </span>
        )}
        {badge && (
          <span
            title={badge.title}
            className={cn(
              'absolute top-2 right-2 inline-flex h-6 items-center gap-1 rounded-tag pr-2 pl-1.5 text-12 font-[750] stretch-narrow',
              badge.solid ? 'bg-ink text-on-light' : 'bg-black/75 text-ink shadow-[inset_0_0_0_1px_var(--color-line-strong)] backdrop-blur-sm',
            )}
          >
            <Icon name={badge.icon} strokeWidth={2.4} className="size-3.5" />
            {badge.label}
          </span>
        )}
      </Cover>
      <div className="grid gap-1.5 px-0.5">
        <h3 className="line-clamp-2 text-15 leading-tight font-[650] tracking-[-0.005em]">
          {h.gameName}
          {v && <span className="font-[480] text-ink-2"> {v}</span>}
        </h3>
        <div className="flex min-w-0 items-center justify-between gap-2">
          <span className="truncate text-small text-ink-3">{h.author}</span>
          <span className="inline-flex shrink-0 items-center gap-1 text-small text-ink-2" title={`${fmtNumber(h.downloads)} downloads`}>
            <Icon name="download" className="size-3.5" />
            <span className="data">{fmtCount(h.downloads)}</span>
          </span>
        </div>
        <div className="flex min-w-0 items-center justify-between gap-2">
          <Pips players={h.maxPlayers} />
          <InputSupport flags={h.inputs} size="sm" />
        </div>
      </div>
    </Link>
  );
}

export function HandlerCardSkeleton() {
  return (
    <div aria-hidden="true" className="flex flex-col gap-3">
      <div className="aspect-[3/4] animate-pulse rounded-cover bg-pane-2" />
      <div className="h-4 w-3/4 rounded bg-pane-2" />
      <div className="h-3 w-1/2 rounded bg-pane-2" />
    </div>
  );
}
