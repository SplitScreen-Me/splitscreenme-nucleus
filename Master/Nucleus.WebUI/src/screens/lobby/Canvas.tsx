import { useEffect, useLayoutEffect, useRef, useState, type DragEvent, type FocusEvent as ReactFocusEvent, type KeyboardEvent, type ReactNode } from 'react';
import type { Edge, ScreenLayout, SetupDevice, SetupPlayer, SetupScreen, SetupSnapshot } from '@/bridge';
import { T, useT, type MessageKey } from '@/i18n';
import { deviceIcon, isGamepadKind, isKeyboardKind } from '@/lib/devices';
import { Button, cn, Icon, MenuButton, Pane, useConfirm } from '@/ui';
import type { LobbyCommands } from './commands';
import { dragKind, readDrop, startDrag, type Dragged } from './dnd';
import { sameSpot, type WaitingSpot } from './join-rules';
import {
  devicesOf,
  fitRig,
  guestsOf,
  hiddenInputs,
  isFree,
  layoutName,
  MORE_LAYOUTS,
  placeName,
  playerById,
  playerName,
  PRESETS,
  reduceMotion,
  relative,
  screenName,
  seatCount,
  slotOfPlayer,
  trayDevices,
  whereOf,
  type Box,
} from './model';
import { PlayerRegion, SlotCell } from './Region';
import { arrowResize, otherCorner, stepFracOf, type Corner } from './resize-core';

/** A player being resized with the keyboard or a gamepad, and the corner the arrows move. */
export interface ResizeMode {
  playerId: string;
  corner: Corner;
}

export interface CanvasProps {
  snap: SetupSnapshot;
  art?: string;
  commands: LobbyCommands;
  pings: Record<string, number>;
  flashes: Record<string, number>;
  justJoined: ReadonlySet<string>;
  announce: (message: string) => void;
  resizing: ResizeMode | null;
  onResizing: (mode: ResizeMode | null) => void;
  /** The empty place waiting for a controller, if any. */
  spot: WaitingSpot | null;
  /** An empty place was clicked or activated: wait for a controller there. */
  onPickSpot: (spot: WaitingSpot) => void;
  onCancelSpot: () => void;
}

/** Arrows with Shift move an edge by this many steps. */
const BIG_STEP = 5;

const CAPTION_H = 74;

/** How an empty place was activated: a pad's A reaches it as a synthetic Enter, the keyboard's is trusted. */
type Via = 'keyboard' | 'pad' | 'pointer';
const viaKey = (e: KeyboardEvent<HTMLElement>): Via => (e.nativeEvent.isTrusted ? 'keyboard' : 'pad');

/** Screen canvas: the monitors, to scale, split into slots and player areas; devices waiting below. */
export function Canvas(props: CanvasProps) {
  const { snap, commands, announce, resizing, onResizing, spot, onPickSpot, onCancelSpot } = props;
  const t = useT();
  const confirm = useConfirm();
  const rigRef = useRef<HTMLDivElement>(null);
  const [size, setSize] = useState({ w: 0, h: 0 });
  const [ready, setReady] = useState(false);
  const [lifted, setLifted] = useState<string | null>(null);
  const [dragging, setDragging] = useState<Dragged | null>(null);
  const [target, setTarget] = useState<string | null>(null);

  useLayoutEffect(() => {
    const el = rigRef.current;
    if (!el) return;
    const measure = () => setSize({ w: el.clientWidth, h: el.clientHeight });
    measure();
    const ro = new ResizeObserver(measure);
    ro.observe(el);
    return () => ro.disconnect();
  }, []);

  // Animate re-scaling only after the first placement, so the rig does not fly in from 0.
  useEffect(() => {
    if (!size.w || ready) return;
    const id = requestAnimationFrame(() => setReady(true));
    return () => cancelAnimationFrame(id);
  }, [size.w, ready]);

  // A lifted player that left (unplugged, removed) is dropped.
  useEffect(() => {
    if (lifted && !playerById(snap, lifted)) setLifted(null);
  }, [snap, lifted]);

  // Resize mode ends when its player leaves or can no longer be resized (a profile was loaded, the layout changed).
  const stepOf = (p: SetupPlayer) => {
    const screen = snap.screens.find((s) => s.index === p.screenIndex);
    return p.canResize && screen ? stepFracOf(screen) : null;
  };
  const resizingPlayer = playerById(snap, resizing?.playerId);
  const resizable = Boolean(resizingPlayer && stepOf(resizingPlayer));
  useEffect(() => {
    if (resizing && !resizable) onResizing(null);
  }, [resizing, resizable, onResizing]);

  // The hint bar reads the focused element's data-hints on focus: tell it the focused area changed mode.
  useLayoutEffect(() => {
    const active = document.activeElement;
    if (active instanceof HTMLElement && active.dataset.playerId && rigRef.current?.contains(active)) {
      active.dispatchEvent(new FocusEvent('focusin', { bubbles: true }));
    }
  }, [resizing?.playerId]);

  const rig = fitRig(snap.screens, snap.totalBounds, size.w, size.h, CAPTION_H);
  const liftedPlayer = playerById(snap, lifted ?? undefined);

  // ---------- Resizing a player: edge handles (mouse), resize mode (keyboard and gamepads) ----------
  const sizeOf = (p: SetupPlayer) => ({ player: playerName(p), width: String(p.bounds.width), height: String(p.bounds.height) });
  const resizeBy = async (p: SetupPlayer, edge: Edge, steps: number) => {
    const next = await commands.resize(p.id, edge, steps);
    const after = next && playerById(next, p.id);
    if (after) announce(t('lobby.resize.size', sizeOf(after)));
  };
  const startResizing = (p: SetupPlayer) => {
    setLifted(null);
    onResizing({ playerId: p.id, corner: 'bottomRight' });
    announce(t('lobby.resize.started', sizeOf(p)));
  };
  const resizeKeys = (p: SetupPlayer, mode: ResizeMode, e: KeyboardEvent<HTMLDivElement>) => {
    const move = arrowResize(mode.corner, e.key);
    if (move) {
      e.preventDefault();
      void resizeBy(p, move.edge, e.shiftKey ? move.steps * BIG_STEP : move.steps);
    } else if (e.key === 'x' || e.key === 'X') {
      e.preventDefault();
      const corner = otherCorner(mode.corner);
      onResizing({ ...mode, corner });
      announce(t(corner === 'bottomRight' ? 'lobby.resize.cornerBottomRight' : 'lobby.resize.cornerTopLeft'));
    } else if (e.key === 'Enter' || e.key === ' ' || e.key === 'Escape') {
      e.preventDefault();
      onResizing(null);
      announce(t('lobby.resize.done', { player: playerName(p) }));
    } else if (e.key === 'Tab') {
      onResizing(null);
      return;
    } else if (e.key !== 'Shift') {
      return;
    }
    e.stopPropagation();
  };

  // ---------- Moving players and devices ----------
  const dropOnSlot = (dragged: Dragged, screen: SetupScreen, slotIndex: number) => {
    if (dragged.kind === 'device') return void commands.assignToSlot(dragged.id, screen.index, slotIndex);
    const p = playerById(snap, dragged.id);
    if (p) void commands.movePlayer(p, screen.index, slotIndex);
  };
  const dropAt = (dragged: Dragged, screen: SetupScreen, x: number, y: number) => {
    if (dragged.kind === 'device') return void commands.assignAt(dragged.id, screen.index, x, y);
    const p = playerById(snap, dragged.id);
    if (p) void commands.movePlayerAt(p, screen.index, x, y);
  };
  const takeOut = async (p: SetupPlayer) => {
    setLifted((l) => (l === p.id ? null : l));
    if (await commands.removePlayer(p)) announce(t('lobby.announce.playerLeft', { player: playerName(p) }));
  };

  // ---------- Keyboard: pick up, place, take out, move focus ----------
  const moveFocus = (from: HTMLElement, step: number) => {
    const list = [...(rigRef.current?.querySelectorAll<HTMLElement>('[data-region]') ?? [])];
    const k = list.indexOf(from);
    list[(k + step + list.length) % list.length]?.focus();
  };
  const commonKeys = (e: KeyboardEvent<HTMLDivElement>) => {
    if (e.key === 'Escape' && lifted) {
      e.preventDefault();
      e.stopPropagation();
      setLifted(null);
      announce(t('lobby.move.cancelled'));
      return true;
    }
    if (e.key.startsWith('Arrow')) {
      e.preventDefault();
      moveFocus(e.currentTarget, e.key === 'ArrowRight' || e.key === 'ArrowDown' ? 1 : -1);
      return true;
    }
    return false;
  };
  const onPlayerKey = (p: SetupPlayer, screen: SetupScreen) => (e: KeyboardEvent<HTMLDivElement>) => {
    if (resizing?.playerId === p.id) {
      resizeKeys(p, resizing, e);
      return;
    }
    if ((e.key === 'r' || e.key === 'R') && !e.ctrlKey && !e.altKey && !e.metaKey && stepOf(p)) {
      e.preventDefault();
      startResizing(p);
      return;
    }
    if (commonKeys(e)) return;
    if (e.key === 'Enter' || e.key === ' ') {
      e.preventDefault();
      if (!liftedPlayer) {
        setLifted(p.id);
        announce(t('lobby.move.pickedUp', { player: playerName(p) }));
      } else if (liftedPlayer.id === p.id) {
        setLifted(null);
        announce(t('lobby.move.putBack'));
      } else {
        const slot = slotOfPlayer(screen, p.id);
        setLifted(null);
        if (slot !== undefined) void commands.movePlayer(liftedPlayer, screen.index, slot);
        else if (screen.layout === 'manual') void commands.movePlayerAt(liftedPlayer, screen.index, 0.5, 0.5);
        announce(t('lobby.move.swapped', { first: playerName(liftedPlayer), second: playerName(p) }));
      }
    } else if (e.key === 'Delete' || e.key === 'Backspace') {
      e.preventDefault();
      void takeOut(p);
    }
  };
  const freeKeyboard = snap.devices.find((d) => isFree(d) && d.revealed && isKeyboardKind(d.kind));
  const pick = (screen: SetupScreen, slotIndex: number | undefined, x: number, y: number, via: Via) =>
    onPickSpot({ screenIndex: screen.index, slotIndex, x, y, layout: screen.layout, at: performance.now(), byPad: via === 'pad' });
  /** An empty slot activated: the lifted player moves there; else the keyboard joins there (Enter), or it waits for a controller. */
  const activateSlot = (screen: SetupScreen, slotIndex: number, via: Via) => {
    if (liftedPlayer) {
      setLifted(null);
      void commands.movePlayer(liftedPlayer, screen.index, slotIndex);
      announce(t('lobby.move.moved', { player: playerName(liftedPlayer), place: placeName(screen, slotIndex) }));
    } else if (sameSpot(spot, { screenIndex: screen.index, slotIndex })) {
      onCancelSpot();
    } else if (via === 'keyboard' && freeKeyboard) {
      void commands.assignToSlot(freeKeyboard.id, screen.index, slotIndex);
    } else {
      pick(screen, slotIndex, 0.5, 0.5, via);
    }
  };
  /** A spot of a manual screen activated: the lifted player goes there, else it waits for a controller. */
  const activateManual = (screen: SetupScreen, x: number, y: number, via: Via) => {
    if (liftedPlayer) {
      setLifted(null);
      void commands.movePlayerAt(liftedPlayer, screen.index, x, y);
    } else if (via !== 'pointer' && sameSpot(spot, { screenIndex: screen.index, slotIndex: undefined })) {
      onCancelSpot();
    } else {
      pick(screen, undefined, x, y, via);
    }
  };
  const onSlotKey = (screen: SetupScreen, slotIndex: number) => (e: KeyboardEvent<HTMLDivElement>) => {
    if (commonKeys(e)) return;
    if (e.key !== 'Enter' && e.key !== ' ') return;
    e.preventDefault();
    activateSlot(screen, slotIndex, viaKey(e));
  };
  const onManualKey = (screen: SetupScreen) => (e: KeyboardEvent<HTMLDivElement>) => {
    if (commonKeys(e)) return;
    if (e.key !== 'Enter' && e.key !== ' ') return;
    e.preventDefault();
    activateManual(screen, 0.5, 0.5, viaKey(e));
  };
  /** Focus leaving the waiting place (a click or Tab elsewhere) stops the wait; the window losing focus does not. */
  const leaveSpot = (e: ReactFocusEvent<HTMLElement>) => {
    if (!e.currentTarget.contains(e.relatedTarget as Node | null) && document.hasFocus()) onCancelSpot();
  };

  // ---------- Drag and drop ----------
  const endDrag = () => {
    setDragging(null);
    setTarget(null);
  };
  const over = (key: string) => (e: DragEvent<HTMLElement>) => {
    if (!dragKind(e)) return;
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
    if (target !== key) setTarget(key);
  };
  const leave = (key: string) => (e: DragEvent<HTMLElement>) => {
    if (target === key && !e.currentTarget.contains(e.relatedTarget as Node | null)) setTarget(null);
  };
  const drop = (act: (dragged: Dragged, e: DragEvent<HTMLElement>) => void) => (e: DragEvent<HTMLElement>) => {
    const dragged = readDrop(e);
    if (!dragged) return;
    e.preventDefault();
    e.stopPropagation();
    endDrag();
    act(dragged, e);
  };
  const playerDragStart = (p: SetupPlayer) => (e: DragEvent<HTMLElement>) => {
    startDrag(e, { kind: 'player', id: p.id });
    requestAnimationFrame(() => setDragging({ kind: 'player', id: p.id }));
  };

  const draggingPlayer = dragging?.kind === 'player';
  const waiting = trayDevices(snap);
  const hidden = hiddenInputs(snap);
  const keyboardsSupported = snap.capabilities.keyboard || snap.capabilities.multipleKeyboardsAndMice;
  const seats = seatCount(snap);

  return (
    <Pane as="section" aria-labelledby="lobby-screens-title" className="flex min-h-0 min-w-0 flex-col gap-3 px-[22px] pt-[16px] pb-[18px]">
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0">
          <h2 id="lobby-screens-title" className="type-h3">
            {t('lobby.canvas.title')}
          </h2>
          <p className="mt-1 text-small text-ink-2">{t('lobby.canvas.description')}</p>
        </div>
        <span className="shrink-0 text-15 font-semibold whitespace-nowrap text-ink-2" aria-live="polite">
          <T k="lobby.canvas.seats" values={{ seats, count: snap.capabilities.maxPlayers }} tags={{ b: (c) => <b className="font-[750] text-ink">{c}</b> }} />
        </span>
      </div>

      <div className="relative min-h-0 flex-1">
        <div ref={rigRef} className="absolute inset-0" data-own-arrows data-autofocus>
          {snap.screens.map((screen) => {
            const box = rig?.boxes.get(screen.index);
            const players = snap.players.filter((p) => p.screenIndex === screen.index);
            const manual = screen.layout === 'manual';
            const manualSpot = manual && spot?.screenIndex === screen.index && spot.slotIndex === undefined ? spot : null;
            return (
              <Monitor key={screen.index} screen={screen} box={box} animate={ready}>
                {manual && (
                  <div
                    role="button"
                    tabIndex={0}
                    data-region
                    aria-label={
                      manualSpot
                        ? t('lobby.spot.label', { place: screenName(screen) })
                        : t(liftedPlayer ? 'lobby.manual.labelMoving' : 'lobby.manual.label', { screen: screenName(screen) })
                    }
                    onKeyDown={onManualKey(screen)}
                    onClick={(e) => {
                      const r = e.currentTarget.getBoundingClientRect();
                      const x = Math.min(1, Math.max(0, (e.clientX - r.left) / r.width));
                      const y = Math.min(1, Math.max(0, (e.clientY - r.top) / r.height));
                      activateManual(screen, x, y, 'pointer');
                    }}
                    onBlur={manualSpot ? leaveSpot : undefined}
                    onDragOver={over(`m${screen.index}`)}
                    onDragLeave={leave(`m${screen.index}`)}
                    onDrop={drop((dragged, e) => {
                      const r = e.currentTarget.getBoundingClientRect();
                      const x = Math.min(1, Math.max(0, (e.clientX - r.left) / r.width));
                      const y = Math.min(1, Math.max(0, (e.clientY - r.top) / r.height));
                      dropAt(dragged, screen, x, y);
                    })}
                    className={cn(
                      'absolute inset-0 grid place-items-center rounded-[inherit] bg-pane-2/60 text-center text-small text-ink-3 focus-visible:outline-2 focus-visible:-outline-offset-[5px] focus-visible:outline-ink',
                      target === `m${screen.index}` && 'shadow-[inset_0_0_0_3px_var(--color-ink)]',
                    )}
                  >
                    <span className={cn('px-3', (players.length > 0 || manualSpot) && 'sr-only')}>
                      {t('lobby.manual.hint')}
                    </span>
                    {manualSpot && <WaitingMarker x={manualSpot.x} y={manualSpot.y} />}
                  </div>
                )}
                {!manual &&
                  screen.slots
                    .filter((slot) => !slot.playerId)
                    .map((slot) => {
                      const key = `s${screen.index}:${slot.index}`;
                      const where = placeName(screen, slot.index);
                      const waiting = !liftedPlayer && sameSpot(spot, { screenIndex: screen.index, slotIndex: slot.index });
                      return (
                        <SlotCell
                          key={key}
                          frac={relative(slot.bounds, screen.bounds)}
                          label={
                            waiting
                              ? t('lobby.spot.label', { place: where })
                              : t(slotLabel(slot.expected, Boolean(liftedPlayer)), { place: where })
                          }
                          expected={slot.expected}
                          dropMode={Boolean(liftedPlayer)}
                          waiting={waiting}
                          target={target === key}
                          regionRef={null}
                          onKeyDown={onSlotKey(screen, slot.index)}
                          onClick={() => activateSlot(screen, slot.index, 'pointer')}
                          onBlur={waiting ? leaveSpot : undefined}
                          onDragOver={over(key)}
                          onDragLeave={leave(key)}
                          onDrop={drop((dragged) => dropOnSlot(dragged, screen, slot.index))}
                        />
                      );
                    })}
                {players.map((p) => {
                  const key = `p${p.id}`;
                  const slot = slotOfPlayer(screen, p.id);
                  return (
                    <PlayerRegion
                      key={p.id}
                      player={p}
                      devices={devicesOf(snap, p)}
                      guests={guestsOf(snap, p).length}
                      frac={relative(p.bounds, screen.bounds)}
                      where={whereOf(snap, p)}
                      art={props.art}
                      lifted={lifted === p.id}
                      dragging={dragging?.kind === 'player' && dragging.id === p.id}
                      target={target === key}
                      justJoined={props.justJoined.has(p.id)}
                      ping={props.pings[p.id] ?? 0}
                      regionRef={null}
                      step={stepOf(p)}
                      monitor={screen.bounds}
                      others={players.filter((o) => o.id !== p.id).map((o) => relative(o.bounds, screen.bounds))}
                      resizeCorner={resizing?.playerId === p.id ? resizing.corner : null}
                      onResize={(edge, steps) => void resizeBy(p, edge, steps)}
                      onBlur={() => {
                        if (resizing?.playerId === p.id) onResizing(null);
                      }}
                      onKeyDown={onPlayerKey(p, screen)}
                      onDragStart={playerDragStart(p)}
                      onDragEnd={endDrag}
                      onDragOver={over(key)}
                      onDragLeave={leave(key)}
                      onDrop={drop((dragged, e) => {
                        if (dragged.kind === 'player' && dragged.id === p.id) return;
                        if (slot !== undefined) dropOnSlot(dragged, screen, slot);
                        else if (manual) {
                          const r = (e.currentTarget.parentElement ?? e.currentTarget).getBoundingClientRect();
                          dropAt(dragged, screen, (e.clientX - r.left) / r.width, (e.clientY - r.top) / r.height);
                        }
                      })}
                    />
                  );
                })}
              </Monitor>
            );
          })}
          {rig && (
            <div
              className={cn('pointer-events-none absolute inset-x-0', rig.captionsInRow && 'flex flex-wrap justify-center gap-x-8 gap-y-3')}
              style={{ top: rig.captionTop }}
            >
              {snap.screens.map((screen) => (
                <Caption
                  key={screen.index}
                  screen={screen}
                  box={rig.captionsInRow ? undefined : rig.boxes.get(screen.index)}
                  animate={ready}
                  onSetLayout={async (layout) => {
                    const done = await commands.setLayout(screen.index, layout);
                    if (done) announce(t('lobby.layouts.changed', { screen: screenName(screen), layout: layoutName(layout) }));
                  }}
                />
              ))}
            </div>
          )}
        </div>
      </div>

      <div
        onDragOver={(e) => {
          if (dragKind(e) !== 'player') return;
          e.preventDefault();
          e.dataTransfer.dropEffect = 'move';
          if (target !== 'tray') setTarget('tray');
        }}
        onDragLeave={leave('tray')}
        onDrop={drop((dragged) => {
          const p = dragged.kind === 'player' ? playerById(snap, dragged.id) : undefined;
          if (p) void takeOut(p);
        })}
        className={cn(
          'grid gap-2.5 rounded-[2px] border-t border-line pt-3.5 outline-[1.5px] outline-offset-[6px] transition-[outline-color] duration-(--dur-1)',
          draggingPlayer ? (target === 'tray' ? 'outline-dashed outline-ink' : 'outline-dashed outline-ink-3') : 'outline-transparent',
        )}
      >
        <div className="flex flex-wrap items-center gap-x-3 gap-y-1.5">
          <h3 className="text-15 font-[680]">{t('lobby.tray.title')}</h3>
          <p className="min-w-0 flex-1 truncate text-13 text-ink-3">
            {draggingPlayer ? (
              <b className="font-[650] text-ink">{t('lobby.tray.dropToRemove')}</b>
            ) : waiting.length > 0 ? (
              t(joinSentence(waiting))
            ) : keyboardsSupported && hidden.length > 0 ? (
              t('lobby.tray.hiddenInputs')
            ) : (
              t('lobby.tray.allJoined')
            )}
          </p>
          <div className="flex shrink-0 gap-1.5">
            <Button variant="quiet" size="sm" className="min-h-[30px] px-2.5 text-13 [&_svg]:size-4" onClick={() => void commands.refreshDevices()} data-nav>
              <Icon name="refresh" />
              {t('lobby.tray.rescan')}
            </Button>
            <Button
              variant="quiet"
              size="sm"
              className="min-h-[30px] px-2.5 text-13 [&_svg]:size-4"
              disabled={!waiting.some((d) => d.revealed)}
              onClick={() => void commands.autoFill()}
              data-nav
            >
              <Icon name="users" />
              {t('lobby.tray.placeEveryone')}
            </Button>
            <Button
              variant="quiet"
              size="sm"
              className="min-h-[30px] px-2.5 text-13 [&_svg]:size-4"
              disabled={snap.players.length === 0 && !snap.profile.loaded}
              onClick={async () => {
                const ok = await confirm({
                  title: t('lobby.tray.clearConfirm.title'),
                  message: t(snap.profile.loaded ? 'lobby.tray.clearConfirm.messageProfile' : 'lobby.tray.clearConfirm.message'),
                  confirmLabel: t('lobby.tray.clearConfirm.confirm'),
                  danger: true,
                });
                if (ok && (await commands.clearAll(snap.players.flatMap((p) => [...p.deviceIds, ...p.guestDeviceIds])))) announce(t('lobby.tray.cleared'));
              }}
              data-nav
            >
              <Icon name="x" />
              {t('lobby.tray.clear')}
            </Button>
          </div>
        </div>
        <div className={cn('scroll-quiet -m-1.5 flex min-h-[64px] items-center gap-2 overflow-x-auto p-1.5', draggingPlayer && 'opacity-35')}>
          {waiting.map((d) => (
            <WaitingDevice
              key={d.id}
              device={d}
              flash={props.flashes[d.id] ?? 0}
              onJoin={() => void commands.join(d.id)}
              onRumble={() => void commands.rumble(d.id)}
              onDragStart={(e) => {
                startDrag(e, { kind: 'device', id: d.id });
                setDragging({ kind: 'device', id: d.id });
              }}
              onDragEnd={endDrag}
            />
          ))}
          {waiting.length > 0 && keyboardsSupported && hidden.length > 0 && (
            <span className="shrink-0 text-13 whitespace-nowrap text-ink-3">{t('lobby.tray.hiddenInputsShort')}</span>
          )}
        </div>
      </div>
    </Pane>
  );
}

/** The spot of a manual screen waiting for a controller. */
function WaitingMarker({ x, y }: { x: number; y: number }) {
  const t = useT();
  return (
    <span
      aria-hidden="true"
      style={{ left: `${x * 100}%`, top: `${y * 100}%` }}
      className="pointer-events-none absolute z-[3] grid max-w-[90%] -translate-x-1/2 -translate-y-1/2 justify-items-center gap-1 rounded-ctl border-[1.5px] border-ink bg-pane-3 px-3 py-2 text-center animate-wait"
    >
      <Icon name="pad" className="size-5 text-ink" />
      <strong className="text-13 leading-tight font-[650] text-ink">{t('lobby.spot.pressButton')}</strong>
      <span className="text-12 text-ink-3">{t('lobby.spot.cancelHint')}</span>
    </span>
  );
}

/** How the devices of the tray join: Start on a controller, any key on a keyboard, or a drag. */
function joinSentence(devices: readonly SetupDevice[]): MessageKey {
  const pads = devices.some((d) => isGamepadKind(d.kind));
  const keyboards = devices.some((d) => isKeyboardKind(d.kind));
  if (pads && keyboards) return 'lobby.tray.joinPadsAndKeyboards';
  if (pads) return 'lobby.tray.joinPads';
  if (keyboards) return 'lobby.tray.joinKeyboards';
  return 'lobby.tray.joinDrag';
}

/** Label of an empty slot: whether the profile expects a player there, and whether a picked-up player can move there. */
function slotLabel(expected: boolean, moving: boolean) {
  if (expected) return moving ? 'lobby.slot.labelExpectedMoving' : 'lobby.slot.labelExpected';
  return moving ? 'lobby.slot.labelMoving' : 'lobby.slot.label';
}

const boxStyle = (b: Box | undefined) =>
  b ? { left: b.left, top: b.top, width: b.width, height: b.height } : { left: 0, top: 0, width: 0, height: 0, visibility: 'hidden' as const };

function Monitor({ screen, box, animate, children }: { screen: SetupScreen; box: Box | undefined; animate: boolean; children: ReactNode }) {
  const t = useT();
  const size = { screen: screenName(screen), width: String(screen.bounds.width), height: String(screen.bounds.height) };
  return (
    <div
      style={boxStyle(box)}
      className={cn('absolute', animate ? 'transition-[left,top,width,height] duration-(--dur-3) ease-snap' : undefined)}
    >
      <div
        role="group"
        aria-label={t(screen.isPrimary ? 'lobby.monitor.labelPrimary' : 'lobby.monitor.label', size)}
        className="absolute inset-0 overflow-hidden rounded-[9px] bg-bezel outline outline-1 outline-offset-[3px] outline-line-strong"
      >
        {children}
      </div>
    </div>
  );
}

/** Caption under each monitor: name, resolution, layouts. */
function Caption({
  screen,
  box,
  animate,
  onSetLayout,
}: {
  screen: SetupScreen;
  box: Box | undefined;
  animate: boolean;
  onSetLayout: (layout: ScreenLayout) => void;
}) {
  const t = useT();
  const locked = screen.layoutLocked;
  const other = MORE_LAYOUTS.find((l) => l.layout === screen.layout);
  const lockNote = t('lobby.layouts.lockedNote');
  return (
    <div
      style={box ? { left: box.left, width: Math.max(box.width, 240) } : undefined}
      className={cn('pointer-events-auto grid gap-2', box && 'absolute', box && animate && 'transition-[left,width] duration-(--dur-3) ease-snap')}
    >
      <div className="flex items-baseline gap-2.5 text-small whitespace-nowrap">
        <strong className="font-[680]">{screenName(screen)}</strong>
        <span className="data text-14 text-ink-2">
          {screen.bounds.width} × {screen.bounds.height}
        </span>
        {screen.isPrimary && <span className="text-13 text-ink-3">{t('lobby.monitor.primary')}</span>}
        {locked && <Icon name="lock" className="size-3.5 self-center text-ink-3" label={t('lobby.layouts.lockedIcon')} />}
      </div>
      <div role="group" aria-label={t('lobby.layouts.groupLabel', { screen: screenName(screen) })} className="flex items-center gap-1" title={locked ? lockNote : undefined}>
        {PRESETS.map((p) => (
          <button
            key={p.layout}
            type="button"
            aria-pressed={screen.layout === p.layout}
            aria-label={t(p.label)}
            title={locked ? lockNote : t(p.label)}
            disabled={locked}
            data-nav
            onClick={() => onSetLayout(p.layout)}
            className={cn(
              'grid h-[30px] w-[38px] place-items-center rounded-[7px] border-0 transition-colors duration-(--dur-1) disabled:cursor-not-allowed disabled:opacity-45',
              screen.layout === p.layout ? 'bg-ink text-on-light' : 'bg-pane-2 text-ink-2 enabled:hover:bg-pane-3 enabled:hover:text-ink',
            )}
          >
            <Icon name={p.icon} />
          </button>
        ))}
        {locked ? (
          <span className="ml-1 truncate text-12 text-ink-3">{t(other ? other.short : 'lobby.layouts.setByProfile')}</span>
        ) : (
          <MenuButton
            label={t(other ? other.short : 'common.more')}
            menuLabel={t('lobby.layouts.moreMenu', { screen: screenName(screen) })}
            variant={other ? 'primary' : 'quiet'}
            className="min-h-[30px] px-2.5 text-12"
            align="start"
            items={MORE_LAYOUTS.map((l) => ({ id: l.layout, label: t(l.label), description: t(l.description), checked: screen.layout === l.layout }))}
            onSelect={(id) => onSetLayout(id as ScreenLayout)}
          />
        )}
      </div>
    </div>
  );
}

function WaitingDevice({
  device: d,
  flash,
  onJoin,
  onRumble,
  onDragStart,
  onDragEnd,
}: {
  device: SetupDevice;
  flash: number;
  onJoin: () => void;
  onRumble: () => void;
  onDragStart: (e: DragEvent<HTMLButtonElement>) => void;
  onDragEnd: () => void;
}) {
  const t = useT();
  const icon = useRef<HTMLSpanElement>(null);
  useEffect(() => {
    if (!flash || !icon.current || reduceMotion()) return;
    icon.current.animate(
      [
        { backgroundColor: getComputedStyle(document.body).color, color: '#07080a' },
        { backgroundColor: getComputedStyle(icon.current).backgroundColor, color: getComputedStyle(icon.current).color },
      ],
      { duration: 420, easing: 'cubic-bezier(0.2, 0.9, 0.1, 1)' },
    );
  }, [flash]);
  const pad = isGamepadKind(d.kind);
  const identify = !d.revealed && d.needsButtonPress;
  return (
    <span className="relative inline-flex shrink-0">
      <button
        type="button"
        draggable={!identify}
        data-nav
        onClick={identify ? undefined : onJoin}
        onDragStart={onDragStart}
        onDragEnd={onDragEnd}
        aria-disabled={identify || undefined}
        aria-label={t(identify ? 'lobby.tray.device.identifyLabel' : 'lobby.tray.device.joinLabel', { device: d.name })}
        className={cn(
          'group/wd flex h-[52px] shrink-0 items-center gap-2.5 rounded-[10px] border-0 bg-pane-2 pl-2 text-left whitespace-nowrap transition-colors duration-(--dur-1)',
          pad ? 'pr-11' : 'pr-4',
          identify ? 'cursor-default' : 'cursor-grab hover:bg-pane-3',
        )}
      >
        <span
          ref={icon}
          className={cn(
            'grid size-9 shrink-0 place-items-center rounded-full bg-pane-3 text-ink-2 [&_svg]:size-5',
            !identify && 'group-hover/wd:bg-line-strong',
            identify && 'shadow-[inset_0_0_0_1.5px_var(--color-line-strong)] bg-transparent animate-idle',
          )}
        >
          <Icon name={deviceIcon(d.kind)} />
        </span>
        <span className="min-w-0">
          <strong className="block max-w-[190px] truncate text-14 font-[650]">{d.name}</strong>
          <span className="block text-12 text-ink-3">
            {t(
              identify
                ? 'lobby.tray.device.identify'
                : isKeyboardKind(d.kind)
                  ? 'lobby.tray.device.pressKey'
                  : d.kind === 'rawMouse'
                    ? 'lobby.tray.device.dragMouse'
                    : 'lobby.tray.device.pressStart',
            )}
          </span>
        </span>
      </button>
      {pad && (
        <button
          type="button"
          onClick={onRumble}
          // Sits over the device button; a pad finds itself by pressing a button anyway.
          data-nav="off"
          aria-label={t('lobby.tray.device.vibrateLabel', { device: d.name })}
          title={t('lobby.tray.device.vibrateHint')}
          className="absolute top-1/2 right-2 grid size-7 -translate-y-1/2 place-items-center rounded-md border-0 bg-transparent text-ink-3 hover:bg-pane-3 hover:text-ink"
        >
          <Icon name="bolt" className="size-4" />
        </button>
      )}
    </span>
  );
}
