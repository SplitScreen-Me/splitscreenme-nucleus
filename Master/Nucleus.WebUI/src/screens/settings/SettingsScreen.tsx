import { useEffect, useMemo, useState, type ReactNode } from 'react';
import { useLocation } from 'react-router';
import { useBridgeQuery, type SettingsSection } from '@/bridge';
import { useSettingsValues } from '@/lib/live';
import { useScreenChrome } from '@/shell/chrome';
import { cn, Icon, Pane, Spinner } from '@/ui';
import { About } from './About';
import { SettingRow } from './fields';
import { holds, sectionIcon, useSettingsSaver } from './model';
import { PlayersEditor } from './PlayersEditor';

interface NavEntry {
  id: string;
  title: string;
  description?: string;
  section?: SettingsSection;
}

const PLAYERS_FALLBACK: NavEntry = {
  id: 'players',
  title: 'Players',
  description: 'The nickname and Steam ID each player number uses in game.',
};

/**
 * Settings, rendered from the host's schema (`settings.getSchema`): adding a setting only takes C#. Values are strings
 * as stored in Settings.ini; each change is saved on its own and validated by the host.
 */
export default function SettingsScreen() {
  const schema = useBridgeQuery('settings.getSchema', undefined, { staleTime: Infinity });
  const values = useSettingsValues();
  const { hash } = useLocation();
  const [current, setCurrent] = useState('general');
  const ready = Boolean(schema.data && values.data);

  useScreenChrome([{ label: 'Settings' }], {
    base: [
      { button: 'A', label: 'Change' },
      { button: 'B', label: 'Back' },
      { button: 'Y', label: 'Search' },
    ],
  });

  // The players editor belongs to the "players" section; the schema may not have one.
  const entries = useMemo<NavEntry[]>(() => {
    const sections = schema.data?.sections ?? [];
    const list: NavEntry[] = sections.map((s) => ({ id: s.id, title: s.title, description: s.description, section: s }));
    if (!list.some((e) => e.id === 'players')) {
      const at = list.findIndex((e) => e.id === 'general');
      list.splice(at + 1, 0, PLAYERS_FALLBACK);
    }
    return [...list, { id: 'about', title: 'About' }];
  }, [schema.data]);

  const keys = useMemo(() => new Set((schema.data?.sections ?? []).flatMap((s) => s.fields.map((f) => f.key))), [schema.data]);
  const saver = useSettingsSaver(keys);

  // Deep links (#hotkeys, #audio…) from the command palette.
  useEffect(() => {
    const id = hash.replace('#', '');
    if (!id || !ready) return;
    setCurrent(id);
    requestAnimationFrame(() => document.getElementById(`set-${id}`)?.scrollIntoView({ block: 'start' }));
  }, [hash, ready]);

  // Highlight the section in view.
  useEffect(() => {
    if (!ready) return;
    const stage = document.getElementById('stage');
    const io = new IntersectionObserver(
      (seen) => {
        const top = seen.filter((e) => e.isIntersecting).sort((a, b) => a.boundingClientRect.top - b.boundingClientRect.top)[0];
        if (top) setCurrent(top.target.id.replace('set-', ''));
      },
      { root: stage, rootMargin: '0px 0px -65% 0px' },
    );
    entries.forEach((e) => {
      const el = document.getElementById(`set-${e.id}`);
      if (el) io.observe(el);
    });
    return () => io.disconnect();
  }, [ready, entries]);

  if (schema.isError || values.isError) {
    return (
      <Pane className="grid h-full place-items-center p-8 text-center">
        <div className="grid justify-items-center gap-3">
          <Icon name="warning" className="size-7 text-warn" />
          <h1 className="type-h3">Settings could not be read</h1>
          <p className="max-w-[46ch] text-small text-ink-2">Nucleus Co-op did not return its settings. Restart the app; if it keeps happening, check the app log.</p>
        </div>
      </Pane>
    );
  }
  if (!schema.data || !values.data) {
    return (
      <Pane className="grid h-full place-items-center text-small text-ink-2">
        <span className="inline-flex items-center gap-2.5">
          <Spinner /> Loading settings
        </span>
      </Pane>
    );
  }

  const current_values = values.data;
  const buttons = schema.data.gamepadButtons;
  const reduce = () => matchMedia('(prefers-reduced-motion: reduce)').matches;

  return (
    <div className="grid items-start gap-gutter grid-cols-[232px_minmax(0,1fr)] max-[1300px]:grid-cols-[208px_minmax(0,1fr)]">
      <Pane as="nav" aria-label="Settings sections" className="sticky top-0 grid gap-0.5 p-2.5">
        <h1 className="px-3 pt-3 pb-3 text-22 font-[800] tracking-[-0.02em] stretch-wide">Settings</h1>
        {entries.map((s) => (
          <a
            key={s.id}
            href={`#set-${s.id}`}
            data-nav
            aria-current={current === s.id ? 'location' : undefined}
            onClick={(e) => {
              e.preventDefault();
              setCurrent(s.id);
              document.getElementById(`set-${s.id}`)?.scrollIntoView({ behavior: reduce() ? 'auto' : 'smooth', block: 'start' });
            }}
            className={cn(
              'flex items-center gap-3 rounded-ctl px-3 py-2.5 text-15 font-[560] no-underline',
              current === s.id ? 'bg-pane-3 text-ink' : 'text-ink-2 hover:bg-pane-2 hover:text-ink',
            )}
          >
            <Icon name={sectionIcon(s.id)} className="size-[18px]" />
            <span className="truncate">{s.title}</span>
          </a>
        ))}
      </Pane>

      <div className="grid min-w-0 gap-gutter">
        {entries.map((entry) => {
          if (entry.id === 'about') return <About key="about" />;
          const fields = (entry.section?.fields ?? []).filter((f) => holds(f.visibleWhen, current_values));
          return (
            <Section key={entry.id} id={entry.id} title={entry.title} description={entry.description}>
              {fields.map((f) => (
                <SettingRow
                  key={f.key}
                  field={f}
                  value={current_values[f.key] ?? f.default ?? ''}
                  disabled={!holds(f.enabledWhen, current_values)}
                  saver={saver}
                  gamepadButtons={buttons}
                />
              ))}
              {entry.id === 'players' && <PlayersEditor />}
            </Section>
          );
        })}
      </div>
    </div>
  );
}

function Section({ id, title, description, children }: { id: string; title: string; description?: string; children: ReactNode }) {
  return (
    <Pane as="section" id={`set-${id}`} aria-labelledby={`set-${id}-title`} className="scroll-mt-0 px-7 pt-6 pb-2">
      <h2 id={`set-${id}-title`} className="type-h3">
        {title}
      </h2>
      {description && <p className="mt-1 mb-3 text-small text-ink-3">{description}</p>}
      <div className={cn(!description && 'mt-3')}>{children}</div>
    </Pane>
  );
}
