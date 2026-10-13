import { useEffect, useId, useMemo, useRef, useState } from 'react';
import { useNavigate } from 'react-router';
import { useAllHandlers, norm } from '@/hub';
import { useT, type MessageKey } from '@/i18n';
import { setupPath, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot } from '@/lib/live';
import { igdbImage } from '@/lib/art';
import { cn, Icon, Kbd, type IconName } from '@/ui';

type Group = 'goTo' | 'yourGames' | 'getGames' | 'settings';

const GROUP_LABELS: Record<Group, MessageKey> = {
  goTo: 'shell.palette.groups.goTo',
  yourGames: 'shell.palette.groups.yourGames',
  getGames: 'shell.palette.groups.getGames',
  settings: 'shell.palette.groups.settings',
};

interface Entry {
  id: string;
  group: Group;
  label: string;
  hint?: string;
  icon?: IconName;
  image?: string;
  to: string;
  text: string;
}

/** A fixed entry: its texts are keys, translated when the list is built. */
interface FixedEntry {
  id: string;
  label: MessageKey;
  hint?: MessageKey;
  icon: IconName;
  to: string;
}

const SCREENS: FixedEntry[] = [
  { id: 's-library', label: 'shell.palette.screens.library', icon: 'library', to: '/library' },
  { id: 's-games', label: 'shell.palette.screens.games', hint: 'shell.palette.screens.gamesHint', icon: 'compass', to: '/games' },
  { id: 's-downloads', label: 'shell.palette.screens.downloads', icon: 'download', to: '/downloads' },
  { id: 's-settings', label: 'shell.palette.screens.settings', icon: 'sliders', to: '/settings' },
];

const SETTINGS: FixedEntry[] = [
  { id: 'set-general', label: 'shell.palette.settings.general', icon: 'sliders', to: '/settings#general' },
  { id: 'set-players', label: 'shell.palette.settings.players', icon: 'users', to: '/settings#players' },
  { id: 'set-hotkeys', label: 'shell.palette.settings.hotkeys', icon: 'keyboard', to: '/settings#hotkeys' },
  { id: 'set-gamepad', label: 'shell.palette.settings.gamepad', icon: 'pad', to: '/settings#gamepad' },
  { id: 'set-audio', label: 'shell.palette.settings.audio', icon: 'speaker', to: '/settings#audio' },
  { id: 'set-layout', label: 'shell.palette.settings.layout', icon: 'layers', to: '/settings#layout' },
  { id: 'set-about', label: 'shell.palette.settings.about', icon: 'info', to: '/settings#about' },
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
  const t = useT();

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
    const fixed = ({ label, hint, ...rest }: FixedEntry) => ({ ...rest, label: t(label), hint: hint ? t(hint) : undefined });
    const screens: Array<Omit<Entry, 'text' | 'group'>> = SCREENS.map(fixed);
    if (session && isSessionLive(session.state)) {
      screens.unshift({
        id: 's-session',
        label: session.gameName ? t('shell.palette.screens.session', { game: session.gameName }) : t('shell.palette.screens.sessionUnnamed'),
        hint: t('shell.palette.screens.sessionHint'),
        icon: 'play',
        to: '/session',
      });
    }
    const settingsWord = t(GROUP_LABELS.settings);
    return [
      ...screens.map((s) => ({ ...s, group: 'goTo' as const, text: norm(`${s.label} ${s.hint ?? ''}`) })),
      ...library.map((g) => ({
        id: `g-${g.id}`,
        group: 'yourGames' as const,
        label: g.name,
        hint: t('shell.palette.gameHint', { count: g.maxPlayers }),
        image: g.coverUrl ?? g.iconUrl,
        to: setupPath(g.id),
        text: norm(g.name),
      })),
      ...hub.map((h) => ({
        id: `h-${h.id}`,
        group: 'getGames' as const,
        label: h.gameName,
        hint: t('shell.palette.handlerHint', { author: h.author }),
        image: igdbImage(h.coverImageId, 'cover_small_2x'),
        to: `/games/${encodeURIComponent(h.id)}`,
        text: norm(`${h.gameName} ${h.title} ${h.author}`),
      })),
      ...SETTINGS.map(fixed).map((s) => ({ ...s, group: 'settings' as const, text: norm(`${settingsWord} ${s.label}`) })),
    ];
  }, [library, hub, session, t]);

  const results = useMemo(() => {
    const words = norm(q).split(/\s+/).filter(Boolean);
    if (!words.length) return entries.filter((e) => e.group === 'goTo' || e.group === 'yourGames').slice(0, 9);
    const hits = entries.filter((e) => words.every((w) => e.text.includes(w)));
    // Keep each group short so the list stays scannable.
    const cap: Record<Group, number> = { goTo: 4, yourGames: 5, getGames: 6, settings: 3 };
    const seen: Record<string, number> = {};
    return hits.filter((e) => (seen[e.group] = (seen[e.group] ?? 0) + 1) <= cap[e.group]);
  }, [entries, q]);

  useEffect(() => setActive(0), [q]);

  const go = (e: Entry | undefined) => {
    if (!e) return;
    onClose();
    navigate(e.to);
  };

  let lastGroup: Group | undefined;
  return (
    <dialog
      ref={ref}
      aria-label={t('shell.palette.label')}
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
          aria-label={t('shell.palette.search')}
          placeholder={t('shell.palette.search')}
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
      <ul id={listId} role="listbox" aria-label={t('shell.palette.results')} className="m-0 max-h-[56vh] list-none overflow-auto p-2">
        {results.length === 0 && <li className="px-3 py-6 text-center text-small text-ink-3">{t('shell.palette.noMatch', { query: q })}</li>}
        {results.map((e, i) => {
          const header = e.group !== lastGroup;
          lastGroup = e.group;
          return (
            <li key={e.id} role="presentation">
              {header && <div className="px-3 pt-2.5 pb-1 text-12 font-[650] text-ink-3">{t(GROUP_LABELS[e.group])}</div>}
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
