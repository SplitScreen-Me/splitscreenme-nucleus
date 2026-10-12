import { useEffect, useId, useLayoutEffect, useRef, useState } from 'react';
import type { SessionLogEntry, SessionLogLevel } from '@/bridge';
import { Button, cn, Icon, Pane, playerClass, type IconName } from '@/ui';

const LEVEL: Record<SessionLogLevel, { icon: IconName; word: string; tone: string }> = {
  info: { icon: 'info', word: 'Info', tone: '' },
  success: { icon: 'check', word: 'Done', tone: 'text-ink' },
  warning: { icon: 'warning', word: 'Warning', tone: 'text-warn' },
  error: { icon: 'x', word: 'Error', tone: 'text-danger' },
};

/** "21:04:14.580" in local time. */
function stamp(iso: string): string {
  const d = new Date(iso);
  if (Number.isNaN(d.getTime())) return '';
  return `${d.toTimeString().slice(0, 8)}.${String(d.getMilliseconds()).padStart(3, '0')}`;
}

/**
 * Live event log of the session: every step of every copy, in monospace, newest at the bottom.
 * A disclosure (not <details>) so the list can grow into the space the row gives it.
 */
export function EventLog({ entries }: { entries: SessionLogEntry[] }) {
  const list = useRef<HTMLOListElement>(null);
  const stick = useRef(true);
  const listId = useId();
  const [open, setOpen] = useState(true);
  const [scrolled, setScrolled] = useState(false);
  const [copied, setCopied] = useState(false);

  // Follow new lines while the reader is at the bottom; leave them alone if they scrolled up.
  useLayoutEffect(() => {
    const el = list.current;
    if (el && stick.current) el.scrollTop = el.scrollHeight;
  }, [entries.length, open]);

  useEffect(() => {
    if (!copied) return;
    const id = window.setTimeout(() => setCopied(false), 2000);
    return () => window.clearTimeout(id);
  }, [copied]);

  const copy = async () => {
    const text = entries.map((e) => `${stamp(e.at)}  ${e.level.padEnd(7)}  ${e.playerNumber ? `[P${e.playerNumber}] ` : ''}${e.text}`).join('\n');
    try {
      await navigator.clipboard.writeText(text);
      setCopied(true);
    } catch {
      /* clipboard unavailable: the button just does not confirm */
    }
  };

  return (
    <Pane as="section" aria-label="Event log" className="flex shrink-0 flex-col">
      <div className="flex items-center gap-2.5 py-3 pr-[18px] pl-2.5">
        <button
          type="button"
          aria-expanded={open}
          aria-controls={listId}
          onClick={() => setOpen((o) => !o)}
          className="flex items-center gap-2.5 rounded-ctl border-0 bg-transparent px-2 py-1 text-15 font-[650] text-ink hover:bg-pane-2"
        >
          <Icon name="chev-d" className={cn('size-4 text-ink-3 transition-transform duration-(--dur-2) ease-snap', open && 'rotate-180')} />
          Event log
          <span className="data text-small font-medium text-ink-3">
            {entries.length} {entries.length === 1 ? 'event' : 'events'}
          </span>
        </button>
        <Button variant="ghost" size="sm" className="ml-auto" disabled={!entries.length} onClick={() => void copy()}>
          <Icon name={copied ? 'check' : 'file'} />
          {copied ? 'Copied' : 'Copy log'}
        </Button>
      </div>
      <ol
        ref={list}
        id={listId}
        hidden={!open}
        aria-label="Session events"
        tabIndex={0}
        onScroll={(e) => {
          const el = e.currentTarget;
          stick.current = el.scrollHeight - el.scrollTop - el.clientHeight < 24;
          setScrolled(el.scrollTop > 2);
        }}
        className={cn(
          'm-0 h-[170px] list-none overflow-auto px-[18px] pb-3.5 font-mono text-12 leading-[1.75] text-ink-2 focus-visible:-outline-offset-2',
          '[@media(max-height:820px)]:h-[118px] [@media(min-height:1000px)]:h-[230px]',
          scrolled && '[mask-image:linear-gradient(180deg,transparent_0,#000_22px)]',
        )}
      >
        {entries.length === 0 && <li className="py-2 font-sans text-small text-ink-3">Nothing logged yet. Every step of every copy shows up here.</li>}
        {entries.map((e) => {
          const level = LEVEL[e.level] ?? LEVEL.info;
          return (
            <li key={e.seq} className={cn('grid grid-cols-[92px_16px_14px_minmax(0,1fr)] items-center', level.tone)}>
              <time dateTime={e.at} className="text-ink-3">
                {stamp(e.at)}
              </time>
              <Icon name={level.icon} label={level.word} strokeWidth={2.2} className={cn('size-3', e.level === 'info' ? 'text-ink-3' : '')} />
              <span aria-hidden="true" className={cn('size-1.5 rounded-full', e.playerNumber ? cn(playerClass(e.playerNumber), 'bg-pc') : 'bg-line-strong')} />
              <span className="min-w-0 break-words">
                {e.playerNumber ? <span className="sr-only">Player {e.playerNumber}: </span> : null}
                {e.text}
              </span>
            </li>
          );
        })}
      </ol>
    </Pane>
  );
}
