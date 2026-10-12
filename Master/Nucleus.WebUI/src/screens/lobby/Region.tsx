import { useEffect, useRef, useState, type DragEvent, type KeyboardEvent, type PointerEvent, type Ref } from 'react';
import type { Edge, Rect, SetupDevice, SetupPlayer } from '@/bridge';
import { devicesIcon } from '@/lib/devices';
import { cn, Icon, playerClass } from '@/ui';
import { ART_VIEW, deviceKinds, devicesLabel, playerName, reduceMotion } from './model';
import { cornerName, dragSteps, movedFrac, pixelSize, type Corner, type Frac, type StepFrac } from './resize-core';

export type { Frac } from './resize-core';

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
  /** One resize step as shares of the monitor, when this player can be resized (`setup.resize`); edge handles then show. */
  step?: StepFrac | null;
  /** Monitor size in pixels, to label the size of the area while it is resized. */
  monitor: Pick<Rect, 'width' | 'height'>;
  /** Areas of the other players of the monitor: a dragged edge stops at them. */
  others?: readonly Frac[];
  /** Resize mode of the keyboard and gamepads: the corner the arrows move. */
  resizeCorner?: Corner | null;
  /** An edge handle was dragged and released: move that edge by `steps` (positive grows). */
  onResize?: (edge: Edge, steps: number) => void;
  /** Leaves resize mode when the area loses focus. */
  onBlur?: () => void;
}

const EDGES: readonly Edge[] = ['left', 'right', 'top', 'bottom'];

/** Grab strips along each edge (inside the area), and the pill drawn in their middle. */
const HANDLE: Record<Edge, { strip: string; pill: string }> = {
  left: { strip: 'inset-y-[14px] left-0 w-[10px] cursor-ew-resize', pill: 'top-1/2 left-[3px] h-[clamp(16px,26%,46px)] w-1 -translate-y-1/2' },
  right: { strip: 'inset-y-[14px] right-0 w-[10px] cursor-ew-resize', pill: 'top-1/2 right-[3px] h-[clamp(16px,26%,46px)] w-1 -translate-y-1/2' },
  top: { strip: 'inset-x-[14px] top-0 h-[10px] cursor-ns-resize', pill: 'top-[3px] left-1/2 h-1 w-[clamp(16px,26%,46px)] -translate-x-1/2' },
  bottom: { strip: 'inset-x-[14px] bottom-0 h-[10px] cursor-ns-resize', pill: 'bottom-[3px] left-1/2 h-1 w-[clamp(16px,26%,46px)] -translate-x-1/2' },
};

/** One player's area of a monitor, drawn from its bounds. The black between regions is the brand gutter. */
export function PlayerRegion(props: PlayerRegionProps) {
  const { player: p, devices, lifted, dragging, target, justJoined, ping, step, resizeCorner } = props;
  const ring = useRef<HTMLDivElement>(null);
  const art = useRef<HTMLDivElement>(null);
  const name = playerName(p);

  // ---------- Resizing with the mouse: an edge handle follows the pointer by whole steps, released once ----------
  const [sizing, setSizing] = useState<{ edge: Edge; steps: number } | null>(null);
  const start = useRef<{ x: number; y: number; w: number; h: number } | null>(null);
  const beginResize = (edge: Edge) => (e: PointerEvent<HTMLDivElement>) => {
    const monitor = e.currentTarget.closest('[data-region]')?.parentElement?.getBoundingClientRect();
    if (!step || !monitor || e.button !== 0) return;
    e.stopPropagation();
    e.currentTarget.setPointerCapture(e.pointerId);
    start.current = { x: e.clientX, y: e.clientY, w: monitor.width, h: monitor.height };
    setSizing({ edge, steps: 0 });
  };
  const followResize = (e: PointerEvent<HTMLDivElement>) => {
    const from = start.current;
    if (!sizing || !from || !step || !from.w || !from.h) return;
    const steps = dragSteps(props.frac, sizing.edge, (e.clientX - from.x) / from.w, (e.clientY - from.y) / from.h, step, props.others);
    if (steps !== sizing.steps) setSizing({ ...sizing, steps });
  };
  const endResize = (commit: boolean) => () => {
    const done = sizing;
    start.current = null;
    setSizing(null);
    if (commit && done && done.steps !== 0) props.onResize?.(done.edge, done.steps);
  };
  const shown = sizing && step ? movedFrac(props.frac, sizing.edge, sizing.steps, step) : props.frac;
  const resizing = Boolean(sizing || resizeCorner);
  const size = pixelSize(shown, props.monitor);

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
  const label = resizeCorner
    ? `Resizing player ${p.number}, ${name}: ${size.width} by ${size.height}. Arrows move the ${cornerName(resizeCorner)} corner, X switches corners, Enter when done.`
    : `Player ${p.number}, ${name}, ${devicesLabel(devices)}, ${props.where}. Enter to move, ${step ? 'R to resize, ' : ''}Delete to take out.`;

  return (
    <div
      ref={props.regionRef}
      role="button"
      tabIndex={0}
      aria-label={label}
      aria-pressed={lifted}
      draggable={!resizing}
      data-region
      data-player-id={p.id}
      // Resize mode takes the pad's d-pad, A and B as arrow keys, Enter and Escape (see couch-nav keyWidget).
      data-pad-keys={resizeCorner ? '' : undefined}
      data-hints={resizeCorner ? 'resizing' : step ? 'player-resizable' : undefined}
      onKeyDown={props.onKeyDown}
      onBlur={props.onBlur}
      onDragStart={(e) => {
        if (start.current) e.preventDefault();
        else props.onDragStart(e);
      }}
      onDragEnd={props.onDragEnd}
      onDragOver={props.onDragOver}
      onDragLeave={props.onDragLeave}
      onDrop={props.onDrop}
      style={place(shown)}
      className={cn(
        playerClass(p.number),
        'group/region absolute cursor-grab overflow-hidden rounded-md bg-pane-3 select-none [container-type:size] active:cursor-grabbing',
        sizing ? 'z-[3] transition-none' : 'z-[2] transition-[left,top,width,height,opacity,transform] duration-(--dur-3) ease-snap',
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
          lifted || resizing
            ? 'shadow-[inset_0_0_0_2px_var(--pc),inset_0_0_0_5px_var(--color-bezel),inset_0_0_0_7px_var(--color-ink)]'
            : 'shadow-[inset_0_0_0_2px_var(--pc)]',
        )}
      />
      <div
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute inset-[6px] z-[4] rounded-[4px] opacity-0 shadow-[0_0_0_2px_var(--color-ink)]',
          !resizing && 'group-focus-visible/region:opacity-100',
        )}
      />
      {target && <div aria-hidden="true" className="pointer-events-none absolute inset-0 z-[3] rounded-[inherit] shadow-[inset_0_0_0_3px_var(--color-ink)]" />}
      {resizeCorner && (
        <span
          aria-hidden="true"
          className={cn(
            'pointer-events-none absolute z-[6] size-4 border-ink',
            resizeCorner === 'bottomRight' ? 'right-[3px] bottom-[3px] border-r-[3px] border-b-[3px]' : 'top-[3px] left-[3px] border-t-[3px] border-l-[3px]',
          )}
        />
      )}
      {resizing && (
        <span
          aria-hidden="true"
          className="data pointer-events-none absolute top-1/2 left-1/2 z-[6] -translate-x-1/2 -translate-y-1/2 rounded-tag bg-black/70 px-2 py-0.5 text-[clamp(10px,5cqi,14px)] font-[650] whitespace-nowrap text-ink tabular-nums"
        >
          {size.width} × {size.height}
        </span>
      )}
      {step &&
        EDGES.map((edge) => (
          <div
            key={edge}
            aria-hidden="true"
            data-resize-handle={edge}
            onPointerDown={beginResize(edge)}
            onPointerMove={followResize}
            onPointerUp={endResize(true)}
            onPointerCancel={endResize(false)}
            // No text selection, no drag of the whole area, no focus change from a handle.
            onMouseDown={(e) => e.preventDefault()}
            className={cn('absolute z-[5] touch-none', HANDLE[edge].strip)}
          >
            <span
              className={cn(
                'absolute rounded-full bg-ink shadow-[0_0_0_1.5px_rgba(0,0,0,0.55)] transition-opacity duration-(--dur-1)',
                resizeCorner || sizing?.edge === edge
                  ? 'opacity-100'
                  : 'opacity-0 group-hover/region:opacity-80 group-focus-visible/region:opacity-80',
                HANDLE[edge].pill,
              )}
            />
          </div>
        ))}
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
  target: boolean;
  regionRef: Ref<HTMLDivElement>;
  onKeyDown: (e: KeyboardEvent<HTMLDivElement>) => void;
}

/** An empty slot of a layout: where the next player joins, or where a dragged one lands. */
export function SlotCell(props: SlotCellProps) {
  const { frac, expected, dropMode, target } = props;
  return (
    <div
      ref={props.regionRef}
      role="button"
      tabIndex={0}
      aria-label={props.label}
      data-region
      onKeyDown={props.onKeyDown}
      onDragOver={props.onDragOver}
      onDragLeave={props.onDragLeave}
      onDrop={props.onDrop}
      style={place(frac)}
      className={cn(
        'group/region absolute z-[1] overflow-hidden rounded-md bg-pane-2 select-none [container-type:size]',
        'transition-[left,top,width,height,opacity] duration-(--dur-3) ease-snap',
        'focus-visible:outline-none',
      )}
    >
      <div className="absolute inset-0 grid place-content-center justify-items-center gap-[clamp(3px,3cqi,10px)] p-1.5 text-center">
        <span aria-hidden="true" className="flex gap-2 text-ink-3 [@container(max-height:84px)]:hidden [@container(max-width:96px)]:hidden">
          <Icon name="pad" className="size-[clamp(16px,10cqi,30px)]" />
          <Icon name="keyboard" className="size-[clamp(16px,10cqi,30px)]" />
        </span>
        <strong className="text-[clamp(10px,5.6cqi,16px)] leading-[1.2] font-[650] text-ink-2 [@container(max-height:40px)]:hidden">
          {dropMode ? 'Move here' : expected ? 'Waiting for a player' : 'Press any button to join'}
        </strong>
        {expected && !dropMode && (
          <span className="text-[clamp(10px,4.4cqi,13px)] text-ink-3 [@container(max-height:64px)]:hidden">Set by the profile</span>
        )}
      </div>
      <div
        aria-hidden="true"
        className={cn(
          'pointer-events-none absolute inset-0 rounded-[inherit] border-[1.5px]',
          expected ? 'border-solid border-ink-3' : 'animate-idle border-dashed border-line-strong',
        )}
      />
      <div aria-hidden="true" className="pointer-events-none absolute inset-[6px] z-[4] rounded-[4px] opacity-0 shadow-[0_0_0_2px_var(--color-ink)] group-focus-visible/region:opacity-100" />
      {target && <div aria-hidden="true" className="pointer-events-none absolute inset-0 z-[3] rounded-[inherit] shadow-[inset_0_0_0_3px_var(--color-ink)]" />}
    </div>
  );
}
