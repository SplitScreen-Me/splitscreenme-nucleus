import { useEffect, useId, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { useAllHandlers, norm } from '@/hub';
import { setupPath, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot } from '@/lib/live';
import { igdbImage } from '@/lib/art';
import { cn, Icon, Kbd, type IconName } from '@/ui';

interface Entry {
  id: string;
  group: 'Go to' | 'Your games' | 'Get games' | 'Settings';
  label: string;
  hint?: string;
  icon?: IconName;
  image?: string;
  to: string;
  text: string;
}

const SCREENS: Array<Omit<Entry, 'text' | 'group'>> = [
  { id: 's-library', label: 'Library', icon: 'library', to: '/library' },
  { id: 's-games', label: 'Get games', hint: 'Browse every handler on the Hub', icon: 'compass', to: '/games' },
  { id: 's-downloads', label: 'Downloads', icon: 'download', to: '/downloads' },
  { id: 's-settings', label: 'Settings', icon: 'sliders', to: '/settings' },
];

const SETTINGS: Array<Omit<Entry, 'text' | 'group'>> = [
  { id: 'set-general', label: 'General: languages, network, nicknames, debug log', icon: 'sliders', to: '/settings#general' },
  { id: 'set-players', label: 'Players: nicknames and Steam IDs', icon: 'users', to: '/settings#players' },
  { id: 'set-hotkeys', label: 'Keyboard shortcuts and input lock key', icon: 'keyboard', to: '/settings#hotkeys' },
  { id: 'set-gamepad', label: 'Gamepad shortcuts and controller type', icon: 'pad', to: '/settings#gamepad' },
  { id: 'set-audio', label: 'Audio: playback device of each instance', icon: 'speaker', to: '/settings#audio' },
  { id: 'set-layout', label: 'Layout: custom grid, background, windows merger', icon: 'layers', to: '/settings#layout' },
  { id: 'set-about', label: 'About, handlers folder, updates', icon: 'info', to: '/settings#about' },
];

/** Ctrl K: jump to a screen, a game in the library, a handler on the Hub or a setting. */
export function CommandPalette({ open, onClose }: { open: boolean; onClose: () => void }) {
  const ref = useRef<HTMLDialogElement>(null);
  const input = useRef<HTMLInputElement>(null);
  const [q, setQ] = useState('');
  const [active, setActive] = useState(0);
  const listId = useId();
  const navigate = useNavigate();
  const { data: library = [] } = useLibrary();
  const { data: hub = [] } = useAllHandlers();
  const { data: session } = useSessionSnapshot();

  useEffect(() => {
    const el = ref.current;
    if (!el) return;
    if (open && !el.open) {
      setQ('');
      setActive(0);
      el.showModal();
      requestAnimationFrame(() => input.current?.focus());
    } else if (!open && el.open) el.close();
  }, [open]);

  const entries = useMemo<Entry[]>(() => {
    const screens = [...SCREENS];
    if (session && isSessionLive(session.state)) {
      screens.unshift({ id: 's-session', label: `${session.gameName ?? 'Game'} session`, hint: 'Running now', icon: 'play', to: '/session' });
    }
    return [
      ...screens.map((s) => ({ ...s, group: 'Go to' as const, text: norm(`${s.label} ${s.hint ?? ''}`) })),
      ...library.map((g) => ({
        id: `g-${g.id}`,
        group: 'Your games' as const,
        label: g.name,
        hint: `Set up a session, up to ${g.maxPlayers} players`,
        image: g.coverUrl ?? g.iconUrl,
        to: setupPath(g.id),
        text: norm(g.name),
      })),
      ...hub.map((h) => ({
        id: `h-${h.id}`,
        group: 'Get games' as const,
        label: h.gameName,
        hint: `Handler by ${h.author}`,
        image: igdbImage(h.coverImageId, 'cover_small_2x'),
        to: `/games/${encodeURIComponent(h.id)}`,
        text: norm(`${h.gameName} ${h.title} ${h.author}`),
      })),
      ...SETTINGS.map((s) => ({ ...s, group: 'Settings' as const, text: norm(`settings ${s.label}`) })),
    ];
  }, [library, hub, session]);

  const results = useMemo(() => {
    const words = norm(q).split(/\s+/).filter(Boolean);
    if (!words.length) return entries.filter((e) => e.group === 'Go to' || e.group === 'Your games').slice(0, 9);
    const hits = entries.filter((e) => words.every((w) => e.text.includes(w)));
    // Keep each group short so the list stays scannable.
    const cap: Record<Entry['group'], number> = { 'Go to': 4, 'Your games': 5, 'Get games': 6, Settings: 3 };
    const seen: Record<string, number> = {};
    return hits.filter((e) => (seen[e.group] = (seen[e.group] ?? 0) + 1) <= cap[e.group]);
  }, [entries, q]);

  useEffect(() => setActive(0), [q]);

  const go = (e: Entry | undefined) => {
    if (!e) return;
    onClose();
    navigate(e.to);
  };

  let lastGroup = '';
  return (
    <dialog
      ref={ref}
      aria-label="Search"
      onClose={onClose}
      onCancel={(e) => {
        e.preventDefault();
        onClose();
      }}
      onClick={(e) => e.target === e.currentTarget && onClose()}
      className="mx-auto mt-[12vh] w-[min(640px,92vw)] overflow-hidden rounded-pane border-0 bg-pane p-0 text-ink shadow-[0_0_0_1px_var(--color-line-strong),0_30px_80px_rgba(0,0,0,0.6)] backdrop:bg-black/70 open:animate-fade-in"
    >
      <label className="flex h-14 items-center gap-3 border-b border-line px-4 text-ink-2">
        <Icon name="search" />
        <input
          ref={input}
          role="combobox"
          aria-expanded="true"
          aria-controls={listId}
          aria-activedescendant={results[active] ? `${listId}-${results[active].id}` : undefined}
          aria-label="Search games, handlers, settings"
          placeholder="Search games, handlers, settings"
          value={q}
          onChange={(e) => setQ(e.target.value)}
          onKeyDown={(e) => {
            if (e.key === 'ArrowDown') {
              e.preventDefault();
              setActive((a) => Math.min(results.length - 1, a + 1));
            } else if (e.key === 'ArrowUp') {
              e.preventDefault();
              setActive((a) => Math.max(0, a - 1));
            } else if (e.key === 'Enter') {
              e.preventDefault();
              go(results[active]);
            }
          }}
          className="min-w-0 flex-1 border-0 bg-transparent text-17 text-ink outline-none placeholder:text-ink-3"
        />
        <Kbd>Esc</Kbd>
      </label>
      <ul id={listId} role="listbox" aria-label="Results" className="m-0 max-h-[56vh] list-none overflow-auto p-2">
        {results.length === 0 && <li className="px-3 py-6 text-center text-small text-ink-3">Nothing matches “{q}”.</li>}
        {results.map((e, i) => {
          const header = e.group !== lastGroup;
          lastGroup = e.group;
          return (
            <li key={e.id} role="presentation">
              {header && <div className="px-3 pt-2.5 pb-1 text-12 font-[650] text-ink-3">{e.group}</div>}
              <div
                id={`${listId}-${e.id}`}
                role="option"
                aria-selected={i === active}
                onMouseMove={() => setActive(i)}
                onClick={() => go(e)}
                className={cn('flex cursor-pointer items-center gap-3 rounded-ctl px-3 py-2', i === active ? 'bg-pane-3' : '')}
              >
                {e.image ? (
                  <img src={e.image} alt="" className="h-9 w-[27px] shrink-0 rounded-[4px] bg-pane-2 object-cover" loading="lazy" />
                ) : (
                  <span className="grid size-[27px] shrink-0 place-items-center text-ink-2">
                    <Icon name={e.icon ?? 'chev-r'} className="size-[18px]" />
                  </span>
                )}
                <span className="min-w-0">
                  <span className="block truncate text-15 font-semibold">{e.label}</span>
                  {e.hint && <span className="block truncate text-13 text-ink-3">{e.hint}</span>}
                </span>
                {i === active && <Icon name="arrow-r" className="ml-auto size-4 text-ink-2" />}
              </div>
            </li>
          );
        })}
      </ul>
    </dialog>
  );
}
