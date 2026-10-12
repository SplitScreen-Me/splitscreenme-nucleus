import { NavLink, useLocation } from 'react-router';
import { useActiveDownloadCount } from '@/lib/downloads';
import { cn, Icon, type IconName } from '@/ui';

interface RailItem {
  to: string;
  label: string;
  icon: IconName;
  /** Other path prefixes that belong to this section. */
  also?: string[];
}

const TOP: RailItem[] = [
  { to: '/library', label: 'Library', icon: 'library', also: ['/session'] },
  { to: '/games', label: 'Get games', icon: 'compass' },
  { to: '/downloads', label: 'Downloads', icon: 'download' },
];
const BOTTOM: RailItem[] = [{ to: '/settings', label: 'Settings', icon: 'sliders' }];

/** Main navigation: a narrow pane on the left. The current section is the white tile. */
export function Rail() {
  const active = useActiveDownloadCount();
  return (
    <nav aria-label="Main" className="flex flex-col items-stretch gap-0.5 rounded-pane bg-pane px-2 py-2.5 [grid-area:rail]">
      {TOP.map((item) => (
        <RailLink key={item.to} item={item} badge={item.to === '/downloads' ? active : 0} />
      ))}
      <span className="flex-1" />
      {BOTTOM.map((item) => (
        <RailLink key={item.to} item={item} />
      ))}
    </nav>
  );
}

function RailLink({ item, badge = 0 }: { item: RailItem; badge?: number }) {
  const { pathname } = useLocation();
  const current = pathname.startsWith(item.to) || Boolean(item.also?.some((p) => pathname.startsWith(p)));
  return (
    <NavLink
      to={item.to}
      data-nav
      data-rail-link
      aria-current={current ? 'page' : undefined}
      aria-label={badge ? `${item.label}, ${badge} in progress` : undefined}
      className={cn(
        'relative flex flex-col items-center gap-[5px] rounded-[10px] px-1 pt-2.5 pb-[9px] text-center text-11 leading-tight font-[650] no-underline',
        current ? 'bg-ink text-on-light' : 'text-ink-3 hover:bg-pane-2 hover:text-ink',
      )}
    >
      <Icon name={item.icon} className="size-[22px]" />
      {item.label}
      {badge > 0 && (
        <span className="absolute top-1.5 left-1/2 ml-1.5 grid h-4 min-w-4 place-items-center rounded-lg bg-ink px-1 text-10 font-[800] text-on-light shadow-[0_0_0_2px_var(--color-pane)]">
          {badge}
        </span>
      )}
    </NavLink>
  );
}
