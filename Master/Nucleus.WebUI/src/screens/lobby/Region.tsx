import { useEffect, useRef, type DragEvent, type FocusEvent, type KeyboardEvent, type Ref } from 'react';
import type { SetupDevice, SetupPlayer } from '@/bridge';
import { devicesIcon } from '@/lib/devices';
import { cn, Icon, playerClass } from '@/ui';
import { ART_VIEW, deviceKinds, devicesLabel, playerName, reduceMotion } from './model';

/** Position inside a monitor, as fractions. */
export interface Frac {
  left: number;
  top: number;
  width: number;
  height: number;
}

const pct = (v: number) => `${v * 100}%`;
const place = (f: Frac) => ({
  left: `calc(${pct(f.left)} + 2px)`,
  top: `calc(${pct(f.top)} + 2px)`,
  width: `calc(${pct(f.width)} - 4px)`,
  height: `calc(${pct(f.height)} - 4px)`,
});

interface DropHandlers {
  onDragOver?: (e: DragEvent<HTMLDivElement>) => void;
  onDragLeave?: (e: DragEvent<HTMLDivElement>) => void;
  onDrop?: (e: DragEvent<HTMLDivElement>) => void;
}

export interface PlayerRegionProps extends DropHandlers {
  player: SetupPlayer;
  devices: SetupDevice[];
  guests: number;
  frac: Frac;
  where: string;
  art?: string;
  lifted: boolean;
  dragging: boolean;
  target: boolean;
  justJoined: boolean;
  ping: number;
  regionRef: Ref<HTMLDivElement>;
  onKeyDown: (e: KeyboardEvent<HTMLDivElement>) => void;
  onDragStart: (e: DragEvent<HTMLDivElement>) => void;
  onDragEnd: () => void;
}

/** One player's area of a monitor, drawn from its bounds. The black between regions is the brand gutter. */
export function PlayerRegion(props: PlayerRegionProps) {
  const { player: p, devices, lifted, dragging, target, justJoined, ping } = props;
  const ring = useRef<HTMLDivElement>(null);
  const art = useRef<HTMLDivElement>(null);
  const name = playerName(p);

  // A device of this player pressed a button: light the region.
  useEffect(() => {
    const el = ring.current;
    if (!ping || !el || reduceMotion()) return;
    const c = getComputedStyle(el).getPropertyValue('--pc').trim() || '#fff';
    el.animate(
      [{ boxShadow: `inset 0 0 0 2px ${c}` }, { boxShadow: `inset 0 0 0 9px ${c}`, offset: 0.3 }, { boxShadow: `inset 0 0 0 2px ${c}` }],
      { duration: 560, easing: 'cubic-bezier(0.2, 0.9, 0.1, 1)' },
    );
  }, [ping]);

  // Joining snaps in: the art lands, the name rises.
  useEffect(() => {
    if (!justJoined || !art.current || reduceMotion()) return;
    art.current.animate(
      [{ filter: 'saturate(1.1) brightness(1.05)', transform: 'scale(1.1)' }, { filter: 'saturate(0.8) brightness(0.55)', transform: 'scale(1)' }],
      { duration: 700, easing: 'cubic-bezier(0.2, 0.9, 0.1, 1)' },
    );
  }, [justJoined]);

  const view = ART_VIEW[(p.number - 1) % ART_VIEW.length] ?? ART_VIEW[0]!;
  const label = `Player ${p.number}, ${name}, ${devicesLabel(devices)}, ${props.where}. Enter to move, Delete to take out.`;

  return (
    <div
      ref={props.regionRef}
      role="button"
      tabIndex={0}
      aria-label={label}
      aria-pressed={lifted}
      draggable
      data-region
      data-player={p.id}
      onKeyDown={props.onKeyDown}
      onDragStart={props.onDragStart}
      onDragEnd={props.onDragEnd}
      onDragOver={props.onDragOver}
      onDragLeave={props.onDragLeave}
      onDrop={props.onDrop}
      style={place(props.frac)}
      className={cn(
        playerClass(p.number),
        'group/region absolute z-[2] cursor-grab overflow-hidden rounded-md bg-pane-3 select-none [container-type:size] active:cursor-grabbing',
        'transition-[left,top,width,height,opacity,transform] duration-(--dur-3) ease-snap',
        'focus-visible:outline-none',
        lifted && 'scale-95',
        dragging && 'opacity-45',
      )}
    >
      <div
        ref={art}
        aria-hidden="true"
        className="absolute inset-0 bg-no-repeat [filter:saturate(0.8)_brightness(0.55)]"
        style={props.art ? { backgroundImage: `url("${props.art}")`, backgroundSize: view[0], backgroundPosition: view[1] } : undefined}
      />
      <div aria-hidden="true" className="absolute inset-0 bg-[linear-gradient(to_top,rgba(0,0,0,0.72),rgba(0,0,0,0)_62%)]" />
      <span
        aria-hidden="true"
        className={cn(
          'absolute top-[clamp(5px,3.4cqi,12px)] left-[clamp(5px,3.4cqi,12px)] inline-flex h-[clamp(18px,11cqi,32px)] min-w-[clamp(26px,15cqi,46px)] items-center justify-center rounded-md bg-pc px-[6px]',
          'text-[clamp(11px,6.4cqi,18px)] leading-none font-[800] text-on-light tabular-nums stretch-narrow',
          justJoined && 'animate-rise-in [animation-delay:80ms]',
        )}
      >
        P{p.number}
      </span>
      {props.guests > 0 && (
        <span
          aria-hidden="true"
          className="absolute top-[clamp(6px,4cqi,14px)] right-[clamp(6px,4cqi,14px)] inline-flex items-center gap-1 rounded-tag bg-black/55 px-2 py-0.5 text-12 font-[650] text-ink [@container(max-width:150px)]:hidden"
        >
          <Icon name="users" className="size-3.5" />+{props.guests}
        </span>
      )}
      <div
        aria-hidden="true"
        className={cn(
          'absolute right-[clamp(6px,4cqi,14px)] bottom-[clamp(5px,3.6cqi,13px)] left-[clamp(6px,4cqi,14px)] grid gap-[3px]',
          justJoined && 'animate-rise-in [animation-delay:80ms]',
        )}
      >
        <strong className="truncate text-[clamp(12px,8.6cqi,30px)] leading-[1.02] font-[800] tracking-[-0.015em] stretch-wide">{name}</strong>
        <span className="flex min-w-0 items-center gap-1.5 text-[clamp(10px,4.6cqi,13px)] whitespace-nowrap text-ink/80 [@container(max-height:70px)]:hidden">
          <Icon name={devicesIcon(deviceKinds(devices))} className="size-[15px] shrink-0 text-pc" />
          <span className="truncate">{devicesLabel(devices)}</span>
        </span>
      </div>
      {/* Player ring, also the target of the ping flash. Lifted adds a white frame inside. */}
      <div
        ref={ring}
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute inset-0 rounded-[inherit]',
          lifted
            ? 'shadow-[inset_0_0_0_2px_var(--pc),inset_0_0_0_5px_var(--color-bezel),inset_0_0_0_7px_var(--color-ink)]'
            : 'shadow-[inset_0_0_0_2px_var(--pc)]',
        )}
      />
      <div aria-hidden="true" className="pointer-events-none absolute inset-[6px] z-[4] rounded-[4px] opacity-0 shadow-[0_0_0_2px_var(--color-ink)] group-focus-visible/region:opacity-100" />
      {target && <div aria-hidden="true" className="pointer-events-none absolute inset-0 z-[3] rounded-[inherit] shadow-[inset_0_0_0_3px_var(--color-ink)]" />}
    </div>
  );
}

export interface SlotCellProps extends DropHandlers {
  frac: Frac;
  label: string;
  /** A loaded profile expects a player here. */
  expected: boolean;
  /** A player is lifted with the keyboard: empty slots read as drop targets. */
  dropMode: boolean;
  /** Waiting for a controller: the next press of a free one takes this slot. */
  waiting: boolean;
  target: boolean;
  regionRef: Ref<HTMLDivElement>;
  onKeyDown: (e: KeyboardEvent<HTMLDivElement>) => void;
  onClick: () => void;
  onBlur?: (e: FocusEvent<HTMLDivElement>) => void;
}

/** An empty slot of a layout: where the next player joins, where a dragged one lands, or where a controller is awaited. */
export function SlotCell(props: SlotCellProps) {
  const { frac, expected, dropMode, waiting, target } = props;
  const [title, note] = dropMode
    ? ['Move here', undefined]
    : waiting
      ? ['Press a button on a controller', 'B or Esc cancels']
      : expected
        ? ['Waiting for a player', 'Set by the profile']
        : ['Press Start to join', 'Or select it, then press any button'];
  return (
    <div
      ref={props.regionRef}
      role="button"
      tabIndex={0}
      aria-label={props.label}
      data-region
      onKeyDown={props.onKeyDown}
      onClick={props.onClick}
      onBlur={props.onBlur}
      onDragOver={props.onDragOver}
      onDragLeave={props.onDragLeave}
      onDrop={props.onDrop}
      style={place(frac)}
      className={cn(
        'group/region absolute z-[1] cursor-pointer overflow-hidden rounded-md select-none [container-type:size]',
        'transition-[left,top,width,height,opacity,background-color] duration-(--dur-3) ease-snap',
        'focus-visible:outline-none',
        waiting ? 'bg-pane-3' : 'bg-pane-2',
      )}
    >
      <div className="absolute inset-0 grid place-content-center justify-items-center gap-[clamp(3px,3cqi,10px)] p-1.5 text-center">
        <span
          aria-hidden="true"
          className={cn('flex gap-2 [@container(max-height:84px)]:hidden [@container(max-width:96px)]:hidden', waiting ? 'text-ink' : 'text-ink-3')}
        >
          <Icon name="pad" className="size-[clamp(16px,10cqi,30px)]" />
          {!waiting && <Icon name="keyboard" className="size-[clamp(16px,10cqi,30px)]" />}
        </span>
        <strong
          className={cn(
            'text-[clamp(10px,5.6cqi,16px)] leading-[1.2] font-[650] [@container(max-height:40px)]:hidden',
            waiting ? 'text-ink' : 'text-ink-2',
          )}
        >
          {title}
        </strong>
        {note && <span className="text-[clamp(10px,4.4cqi,13px)] text-ink-3 [@container(max-height:64px)]:hidden">{note}</span>}
      </div>
      <div
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute inset-0 rounded-[inherit]',
          waiting
            ? 'animate-wait border-2 border-solid border-ink'
            : expected
              ? 'border-[1.5px] border-solid border-ink-3'
              : 'animate-idle border-[1.5px] border-dashed border-line-strong',
        )}
      />
      <div aria-hidden="true" className="pointer-events-none absolute inset-[6px] z-[4] rounded-[4px] opacity-0 shadow-[0_0_0_2px_var(--color-ink)] group-focus-visible/region:opacity-100" />
      {target && <div aria-hidden="true" className="pointer-events-none absolute inset-0 z-[3] rounded-[inherit] shadow-[inset_0_0_0_3px_var(--color-ink)]" />}
    </div>
  );
}
