// The per-game menu (library.getMenu), rendered generically: the host decides the items, their kinds, labels,
// confirmations and file types. Ids are never interpreted here, except as keys.
import { useState } from 'react';
import { bridge, type GameMenuItem } from '@/bridge';
import { useRun } from '@/lib/bridge-ui';
import { Icon, MiniField, MiniSelect, Spinner, Switch, cn, useConfirm, useToast } from '@/ui';

export interface GameMenuProps {
  gameId: string;
  gameName: string;
  items: GameMenuItem[];
  onShowNotes: () => void;
  onShowDetails: () => void;
}

/** Drops leading, trailing and doubled separators. */
function tidy(items: readonly GameMenuItem[]): GameMenuItem[] {
  const out: GameMenuItem[] = [];
  for (const item of items) {
    if (item.kind === 'separator' && (out.length === 0 || out[out.length - 1]?.kind === 'separator')) continue;
    if (item.kind === 'submenu' && item.children.every((c) => c.kind === 'separator')) continue;
    out.push(item);
  }
  while (out[out.length - 1]?.kind === 'separator') out.pop();
  return out;
}

const plainLabel = (label: string) => label.replace(/(?:\.\.\.|…)$/, '').trim();

/** The menu of one game as rows: actions, switches, choices, expandable groups. */
export function GameMenu(props: GameMenuProps) {
  const run = useRun();
  const confirm = useConfirm();
  const toast = useToast();
  const [pending, setPending] = useState<ReadonlySet<string>>(new Set());

  const busy = (id: string, on: boolean) =>
    setPending((s) => {
      const next = new Set(s);
      if (on) next.add(id);
      else next.delete(id);
      return next;
    });

  const activate = async (item: GameMenuItem, value?: string) => {
    if (pending.has(item.id)) return;
    if (item.confirm) {
      const ok = await confirm({
        title: plainLabel(item.label),
        message: item.confirm,
        confirmLabel: plainLabel(item.label),
        danger: item.danger,
      });
      if (!ok) return;
    }
    if (item.clientAction === 'showNotes') return props.onShowNotes();
    if (item.clientAction === 'showDetails') return props.onShowDetails();
    let picked = value;
    if (item.clientAction === 'pickFile') {
      const path = await run(() =>
        bridge.dialogs.pickFile({ title: `${plainLabel(item.label)}: ${props.gameName}`, filters: item.fileFilters }, { timeoutMs: 0 }),
      );
      if (!path) return;
      picked = path;
    }
    busy(item.id, true);
    const done = await run(() =>
      bridge.library.runMenuAction({ gameId: props.gameId, itemId: item.id, value: picked }, { timeoutMs: 120_000 }).then(() => true),
    );
    busy(item.id, false);
    if (done && item.kind === 'action' && (item.danger || item.clientAction === 'pickFile')) toast(`${plainLabel(item.label)}: done`);
  };

  return <MenuList items={tidy(props.items)} pending={pending} onActivate={(item, value) => void activate(item, value)} depth={0} />;
}

function MenuList({
  items,
  pending,
  onActivate,
  depth,
}: {
  items: GameMenuItem[];
  pending: ReadonlySet<string>;
  onActivate: (item: GameMenuItem, value?: string) => void;
  depth: number;
}) {
  return (
    <ul className={cn('m-0 grid list-none gap-0.5 p-0', depth > 0 && 'mt-1 ml-3 border-l border-line pl-2')}>
      {items.map((item) =>
        item.kind === 'separator' ? (
          <li key={item.id} role="separator" className="my-1.5 h-px bg-line" />
        ) : (
          <li key={item.id}>
            <MenuRow item={item} pending={pending} onActivate={onActivate} depth={depth} />
          </li>
        ),
      )}
    </ul>
  );
}

const rowText = (item: GameMenuItem) => (
  <span className="min-w-0">
    <span className={cn('block text-15 font-[600]', item.danger ? 'text-danger' : 'text-ink')}>{item.label}</span>
    {item.description && <span className="block text-13 leading-[1.4] text-ink-3">{item.description}</span>}
  </span>
);

const rowClass =
  'grid min-h-11 w-full grid-cols-[22px_minmax(0,1fr)_auto] items-center gap-x-3 rounded-ctl border-0 bg-transparent px-3 py-2 text-left';

function MenuRow({
  item,
  pending,
  onActivate,
  depth,
}: {
  item: GameMenuItem;
  pending: ReadonlySet<string>;
  onActivate: (item: GameMenuItem, value?: string) => void;
  depth: number;
}) {
  const busy = pending.has(item.id);
  const [open, setOpen] = useState(false);

  switch (item.kind) {
    case 'toggle':
      return (
        <div className={cn(rowClass, 'hover:bg-pane-2')}>
          <Icon name="sliders" className="size-[18px] text-ink-3" />
          <label htmlFor={`menu-${item.id}`} className="cursor-pointer">
            {rowText(item)}
          </label>
          {busy ? (
            <Spinner />
          ) : (
            <Switch
              id={`menu-${item.id}`}
              checked={Boolean(item.checked)}
              onChange={() => onActivate(item)}
              aria-label={item.label}
              data-nav
            />
          )}
        </div>
      );
    case 'choice':
      return (
        <div className={cn(rowClass, 'hover:bg-pane-2')}>
          <Icon name="globe" className="size-[18px] text-ink-3" />
          <label htmlFor={`menu-${item.id}`}>{rowText(item)}</label>
          <span className="flex items-center gap-2">
            {busy && <Spinner />}
            <MiniField className="h-9 w-[180px]">
              <MiniSelect
                id={`menu-${item.id}`}
                value={item.value ?? ''}
                disabled={busy}
                onChange={(e) => onActivate(item, e.target.value)}
                className="text-13"
                data-nav
              >
                {item.value !== undefined && !item.choices.includes(item.value) && <option value={item.value}>{item.value}</option>}
                {item.choices.map((c) => (
                  <option key={c} value={c}>
                    {c}
                  </option>
                ))}
              </MiniSelect>
            </MiniField>
          </span>
        </div>
      );
    case 'submenu':
      return (
        <div>
          <button
            type="button"
            aria-expanded={open}
            onClick={() => setOpen((o) => !o)}
            className={cn(rowClass, 'hover:bg-pane-2')}
            data-nav
          >
            <Icon name="folder" className="size-[18px] text-ink-3" />
            {rowText(item)}
            <span className="flex items-center gap-2 text-13 text-ink-3">
              {item.children.filter((c) => c.kind !== 'separator').length}
              <Icon name="chev-d" className={cn('size-4 transition-transform duration-(--dur-2) ease-snap', open && 'rotate-180')} />
            </span>
          </button>
          {open && <MenuList items={tidy(item.children)} pending={pending} onActivate={onActivate} depth={depth + 1} />}
        </div>
      );
    default:
      return (
        <button
          type="button"
          onClick={() => onActivate(item)}
          disabled={busy}
          className={cn(rowClass, item.danger ? 'hover:bg-danger/10' : 'hover:bg-pane-2', 'disabled:opacity-60')}
          data-nav
        >
          {busy ? (
            <Spinner />
          ) : (
            <Icon name={actionIcon(item)} className={cn('size-[18px]', item.danger ? 'text-danger' : 'text-ink-3')} />
          )}
          {rowText(item)}
          {item.clientAction === 'pickFile' || item.confirm ? <Icon name="chev-r" className="size-4 text-ink-3" /> : <span />}
        </button>
      );
  }
}

/** A generic icon from what the item does (labels are the host's, so only broad hints). */
function actionIcon(item: GameMenuItem) {
  if (item.danger) return 'trash' as const;
  if (item.clientAction === 'showNotes') return 'book' as const;
  if (item.clientAction === 'showDetails') return 'info' as const;
  if (item.clientAction === 'pickFile') return 'file' as const;
  if (/folder|open/i.test(item.label)) return 'folder' as const;
  if (/download/i.test(item.label)) return 'download' as const;
  return 'chev-r' as const;
}
