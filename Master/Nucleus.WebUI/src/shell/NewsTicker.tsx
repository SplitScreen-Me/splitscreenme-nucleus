// The site's news in the title bar: one line that scrolls when it is too long, items in turn, "Learn more" and a
// dismiss button. The legacy news feed (NewsFeed.cs) did the same at the top of the main window. Rules: news-core.ts.
import { useCallback, useEffect, useLayoutEffect, useMemo, useRef, useState } from 'react';
import { useLocation } from 'react-router';
import { bridge } from '@/bridge';
import { liveNews, nextNewsChange, useHubNews, type NewsItem, type NewsSeverity } from '@/hub';
import { useT } from '@/i18n';
import { useRun } from '@/lib/bridge-ui';
import { isSessionLive, useSessionSnapshot } from '@/lib/live';
import { useReducedMotion } from '@/lib/motion';
import { cn, Icon, type IconName } from '@/ui';
import {
  currentNews,
  dismissNews,
  dwellMs,
  freshNews,
  marqueePlan,
  newsKey,
  nextNewsId,
  pendingNews,
  readDismissed,
  SCROLL_GAP_PX,
  SCROLL_HOLD_MS,
  tickerAllowed,
  writeDismissed,
  type MarqueePlan,
} from './news-core';

function storage(): Storage | null {
  try {
    return globalThis.localStorage ?? null;
  } catch {
    return null;
  }
}

export interface NewsTickerState {
  /** The items to show now, in order; empty when the ticker is hidden. */
  items: NewsItem[];
  /** Hides these items until one changes or a new one arrives. */
  dismiss: () => void;
}

/**
 * The news the title bar shows now. Polls the site (paused during a game session), drops items that ended or were
 * dismissed (kept in localStorage: a UI preference of this install, like the language of the first screen), and
 * hides everything on the lobby and session screens.
 */
export function useNewsTicker(): NewsTickerState {
  const { pathname } = useLocation();
  const { data: session } = useSessionSnapshot();
  const { data } = useHubNews(isSessionLive(session?.state));
  const [dismissed, setDismissed] = useState(() => readDismissed(storage()));
  const [tick, setTick] = useState(0);

  // Wake up when an item starts or ends.
  useEffect(() => {
    const now = Date.now();
    const next = nextNewsChange(data ?? [], now);
    if (next === null) return;
    const id = setTimeout(() => setTick((n) => n + 1), Math.min(next - now + 250, 2_000_000_000));
    return () => clearTimeout(id);
  }, [data, tick]);

  const allowed = tickerAllowed(pathname, session?.state);
  const items = useMemo(
    () => (allowed && data ? pendingNews(liveNews(data, Date.now()), dismissed) : []),
    // `tick` re-reads the clock.
    [allowed, data, dismissed, tick],
  );

  const dismiss = useCallback(() => {
    setDismissed((old) => {
      const next = dismissNews(old, items);
      writeDismissed(storage(), next);
      return next;
    });
  }, [items]);

  return { items, dismiss };
}

/**
 * Speaks news when they arrive (polite), once per item: not on each scroll loop, not when the ticker comes back after
 * the lobby. Always rendered, so the region exists before its text changes.
 */
export function NewsAnnouncer({ items }: { items: readonly NewsItem[] }) {
  const t = useT();
  const announced = useRef(new Set<string>());
  const [text, setText] = useState('');
  const signature = items.map(newsKey).join('\n');

  useEffect(() => {
    if (!items.length) {
      setText('');
      return;
    }
    const fresh = freshNews(items, announced.current);
    if (!fresh.length) return;
    for (const n of fresh) announced.current.add(newsKey(n));
    setText(t('shell.news.announcement', { messages: fresh.map((n) => n.message).join(' ') }));
    // Only a change of the items speaks (signature), not a new t or a new array of the same items.
  }, [signature]);

  return (
    <p role="status" aria-live="polite" className="sr-only">
      {text}
    </p>
  );
}

/** Status tints (never a player colour): info is plain, warning and critical get a light wash and a coloured icon. */
const SEVERITY: Record<NewsSeverity, { icon: IconName; box: string; tone: string }> = {
  info: { icon: 'info', box: 'bg-pane text-ink-2', tone: 'text-ink-3' },
  warning: {
    icon: 'warning',
    box: 'bg-warn/10 text-ink shadow-[inset_0_0_0_1px_color-mix(in_oklab,var(--color-warn),transparent_70%)]',
    tone: 'text-warn',
  },
  critical: {
    icon: 'warning',
    box: 'bg-danger/12 text-ink shadow-[inset_0_0_0_1px_color-mix(in_oklab,var(--color-danger),transparent_58%)]',
    tone: 'text-danger',
  },
};

const smallButton =
  'app-no-drag grid h-6 shrink-0 place-items-center rounded-[6px] border-0 bg-transparent text-12 font-[650] hover:bg-pane-3 hover:text-ink';

/**
 * The ticker. Hovering it or focusing one of its buttons pauses it. Its buttons are reachable with the pad (up from
 * the rail) and the keyboard, but nothing moves focus to it when news arrive.
 */
export function NewsTicker({ news, className }: { news: NewsTickerState; className?: string }) {
  const { items, dismiss } = news;
  const t = useT();
  const run = useRun();
  const reduced = useReducedMotion();
  const [currentId, setCurrentId] = useState<string>();
  const [cycle, setCycle] = useState(0);
  const [hovered, setHovered] = useState(false);
  const [focused, setFocused] = useState(false);
  const [plan, setPlan] = useState<MarqueePlan>({ scroll: false });
  const box = useRef<HTMLDivElement>(null);
  const paused = hovered || focused;
  const item = currentNews(items, currentId);
  const at = item ? items.indexOf(item) : -1;

  const advance = useCallback(() => {
    const next = nextNewsId(items, item?.id);
    if (next === item?.id) setCycle((c) => c + 1);
    else setCurrentId(next);
  }, [items, item?.id]);

  // A still message hands over after a while; a scrolling one when its loop ends (Marquee onLoop).
  const dwell = plan.scroll ? null : dwellMs(items.length, reduced);
  useEffect(() => {
    if (dwell === null || paused) return;
    const id = setTimeout(advance, dwell);
    return () => clearTimeout(id);
  }, [dwell, paused, advance, cycle]);

  if (!item) return null;
  const tone = SEVERITY[item.severity];
  const severity = item.severity === 'info' ? undefined : t(`shell.news.severity.${item.severity}`);

  const onDismiss = () => {
    // The buttons go away with the news: give the focus back to the rail, where the pad came from.
    const hadFocus = box.current?.contains(document.activeElement) ?? false;
    dismiss();
    if (hadFocus) document.querySelector<HTMLElement>('[data-rail-link][aria-current="page"], [data-rail-link]')?.focus({ focusVisible: true });
  };

  return (
    <div
      ref={box}
      role="group"
      aria-label={t('shell.news.label')}
      data-news-ticker
      className={cn('app-no-drag flex h-8 items-center gap-2 rounded-ctl pr-1 pl-2.5 text-13 font-[560]', tone.box, className)}
      onPointerEnter={() => setHovered(true)}
      onPointerLeave={() => setHovered(false)}
      onFocus={() => setFocused(true)}
      onBlur={(e) => {
        if (!e.currentTarget.contains(e.relatedTarget as Node | null)) setFocused(false);
      }}
    >
      <Icon name={tone.icon} label={severity} className={cn('size-4', tone.tone)} />
      <Marquee key={`${item.id}:${cycle}`} text={item.message} reduced={reduced} paused={paused} onPlan={setPlan} onLoop={advance} />
      {items.length > 1 && (
        <span aria-hidden="true" className="data shrink-0 text-12 text-ink-3">
          {t('shell.news.position', { index: at + 1, count: items.length })}
        </span>
      )}
      {item.url && (
        <button
          type="button"
          data-nav
          className={cn(smallButton, 'px-2 text-ink shadow-[inset_0_0_0_1px_var(--color-line-strong)]')}
          aria-label={t('shell.news.learnMoreLabel', { message: item.message })}
          title={item.url}
          onClick={() => {
            const url = item.url;
            if (url) void run(() => bridge.app.openExternal({ url }));
          }}
        >
          {t('common.learnMore')}
        </button>
      )}
      <button
        type="button"
        data-nav
        className={cn(smallButton, 'w-6 text-ink-3')}
        aria-label={t('shell.news.dismiss', { count: items.length })}
        title={t('shell.news.dismiss', { count: items.length })}
        onClick={onDismiss}
      >
        <Icon name="x" className="size-3.5" />
      </button>
    </div>
  );
}

const samePlan = (a: MarqueePlan, b: MarqueePlan) =>
  a.scroll === b.scroll && (!a.scroll || !b.scroll || (a.distance === b.distance && a.durationMs === b.durationMs));

/**
 * One message: still and cut with an ellipsis when it fits or motion is reduced, else scrolling left with a copy
 * behind it, once (`onLoop` then picks the next item, or the same one again). The copy is hidden from screen readers.
 */
function Marquee({
  text,
  reduced,
  paused,
  onPlan,
  onLoop,
}: {
  text: string;
  reduced: boolean;
  paused: boolean;
  onPlan: (plan: MarqueePlan) => void;
  onLoop: () => void;
}) {
  const view = useRef<HTMLSpanElement>(null);
  const measure = useRef<HTMLSpanElement>(null);
  const [plan, setPlan] = useState<MarqueePlan>({ scroll: false });

  useLayoutEffect(() => {
    const v = view.current;
    const m = measure.current;
    if (!v || !m) return;
    const update = () => {
      const next = marqueePlan(m.getBoundingClientRect().width, v.clientWidth, reduced);
      setPlan((old) => (samePlan(old, next) ? old : next));
    };
    update();
    const observer = new ResizeObserver(update);
    observer.observe(v);
    observer.observe(m);
    return () => observer.disconnect();
  }, [text, reduced]);

  useEffect(() => onPlan(plan), [plan, onPlan]);

  return (
    <span
      ref={view}
      title={text}
      className={cn(
        'relative block min-w-0 flex-1 overflow-hidden whitespace-nowrap',
        plan.scroll && '[mask-image:linear-gradient(90deg,transparent,#000_8px,#000_calc(100%-20px),transparent)]',
      )}
    >
      {/* Measures the text whatever is shown. */}
      <span ref={measure} aria-hidden="true" className="invisible absolute top-0 left-0 whitespace-nowrap">
        {text}
      </span>
      {plan.scroll ? (
        <span
          className="inline-flex"
          style={{
            animation: `news-marquee ${plan.durationMs}ms linear ${SCROLL_HOLD_MS}ms 1 both`,
            animationPlayState: paused ? 'paused' : 'running',
            ['--news-shift' as string]: `-${plan.distance}px`,
          }}
          onAnimationEnd={(e) => {
            if (e.animationName === 'news-marquee') onLoop();
          }}
        >
          <span>{text}</span>
          <span aria-hidden="true" style={{ paddingLeft: SCROLL_GAP_PX }}>
            {text}
          </span>
        </span>
      ) : (
        <span className="block animate-fade-in truncate">{text}</span>
      )}
    </span>
  );
}
