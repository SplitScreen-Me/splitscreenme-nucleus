import { Fragment, useEffect, useRef, useState } from 'react';
import { Link, useLocation } from 'react-router';
import { bridge, useBridgeEvent, type SetupDevice, type SetupPlayer } from '@/bridge';
import { deviceIcon, devicesIcon, isGamepadKind } from '@/lib/devices';
import { isSessionLive, useSessionSnapshot, useSetupSnapshot, useWindowState } from '@/lib/live';
import { useI18n, useT } from '@/i18n';
import { cn, Icon, Kbd, Logo, playerClass } from '@/ui';
import { useChromeState } from './chrome';

/** Frameless title bar: the drag region of the window. Controls opt out with app-no-drag. */
export function TitleBar({ onOpenPalette }: { onOpenPalette: () => void }) {
  const { crumbs } = useChromeState();
  const t = useT();

  // The native window title stays "Nucleus Co-op" (the host finds the window by it); the document title follows the screen.
  const last = crumbs[crumbs.length - 1]?.label;
  useEffect(() => {
    document.title = last ? t('shell.titleBar.documentTitle', { screen: last }) : 'Nucleus Co-op';
  }, [last, t]);

  // Out of pad reach (data-nav="off"): window buttons (Close!), chips and links that Y, LB / RB and B already cover.
  return (
    <header data-nav="off" className="app-drag flex min-w-0 items-center gap-4 pl-3.5 select-none [grid-area:title]">
      <Link to="/library" className="app-no-drag shrink-0 rounded-ctl no-underline" aria-label={t('shell.titleBar.home')}>
        <Logo />
      </Link>
      <nav aria-label={t('shell.titleBar.breadcrumb')} className="flex max-w-[34%] min-w-0 shrink-0 items-center gap-1.5 text-small text-ink-3">
        {crumbs.map((c, i) => (
          <Fragment key={`${c.label}-${i}`}>
            <Icon name="chev-r" className="size-3.5" />
            {i === crumbs.length - 1 || !c.to ? (
              <b aria-current={i === crumbs.length - 1 ? 'page' : undefined} className="truncate font-semibold text-ink">
                {c.label}
              </b>
            ) : (
              <Link to={c.to} className="app-no-drag truncate rounded text-inherit no-underline hover:text-ink">
                {c.label}
              </Link>
            )}
          </Fragment>
        ))}
      </nav>
      <button
        type="button"
        data-command-palette
        onClick={onOpenPalette}
        className="app-no-drag mx-auto flex h-8 w-[min(420px,34vw)] min-w-[150px] shrink items-center gap-2.5 rounded-ctl border-0 bg-pane pr-2.5 pl-3 text-small text-ink-3 hover:bg-pane-2 hover:text-ink-2"
      >
        <Icon name="search" className="size-4" />
        <span className="truncate">{t('shell.titleBar.search')}</span>
        <Kbd className="ml-auto h-5 text-11">Ctrl K</Kbd>
      </button>
      <DeviceTray />
      <WindowControls />
    </header>
  );
}

const chipClass =
  'relative inline-flex h-[30px] items-center gap-1.5 rounded-tag border-0 bg-pane pr-[9px] pl-2 text-12 font-[650] stretch-narrow hover:bg-pane-2';

/** Restarts the pulse animation of a chip. */
function usePulse(count: number) {
  const ref = useRef<HTMLButtonElement & HTMLSpanElement>(null);
  useEffect(() => {
    const el = ref.current;
    if (!el || !count) return;
    el.classList.remove('animate-dev-pulse');
    void el.offsetWidth;
    el.classList.add('animate-dev-pulse');
  }, [count]);
  return ref;
}

/**
 * Connected inputs. While a setup is open (lobby, session) the host's devices show with their player colour; elsewhere
 * no host device exists, so the tray shows the gamepads the browser sees.
 */
function DeviceTray() {
  const { pathname } = useLocation();
  const { data: session } = useSessionSnapshot();
  const inSetup = /\/setup$/.test(pathname) || isSessionLive(session?.state);
  return inSetup ? <SetupDeviceTray /> : <GamepadTray />;
}

function SetupDeviceTray() {
  const { data: setup } = useSetupSnapshot();
  const t = useT();
  const [pulse, setPulse] = useState<Record<string, number>>({});
  useBridgeEvent('setup.deviceActivity', ({ deviceId, active }) => {
    if (active) setPulse((p) => ({ ...p, [deviceId]: (p[deviceId] ?? 0) + 1 }));
  });
  if (!setup) return <GamepadTray />;
  const players = [...setup.players].sort((a, b) => a.number - b.number);
  const placed = new Set(players.flatMap((p) => [...p.deviceIds, ...p.guestDeviceIds]));
  const free = setup.devices.filter((d) => d.revealed && !placed.has(d.id));
  // Nothing detected or used yet: the browser's view of the gamepads says more than an empty tray.
  if (players.length === 0 && free.length === 0) return <GamepadTray />;
  const pulses = (ids: readonly string[]) => ids.reduce((n, id) => n + (pulse[id] ?? 0), 0);
  return (
    <div className="app-no-drag flex shrink-0 items-center gap-1" role="group" aria-label={t('shell.titleBar.setupDevices')}>
      {players.map((p) => {
        const devices = setup.devices.filter((d) => p.deviceIds.includes(d.id) || p.guestDeviceIds.includes(d.id));
        return <PlayerChip key={p.id} player={p} devices={devices} pulse={pulses(devices.map((d) => d.id))} />;
      })}
      {free.length > 0 && <FreeDevicesChip devices={free} pulse={pulses(free.map((d) => d.id))} />}
    </div>
  );
}

/** One chip per player: its colour and its devices (a keyboard and mouse pair reads as one). Pads vibrate on click. */
function PlayerChip({ player, devices, pulse }: { player: SetupPlayer; devices: SetupDevice[]; pulse: number }) {
  const ref = usePulse(pulse);
  const t = useT();
  const pad = devices.find((d) => isGamepadKind(d.kind));
  const names = devices.map((d) => d.name).join(' + ') || t('shell.titleBar.noDevice');
  const label = player.nickname
    ? t('shell.titleBar.playerNamed', { number: player.number, nickname: player.nickname, devices: names })
    : t('shell.titleBar.player', { number: player.number, devices: names });
  return (
    <button
      ref={ref}
      type="button"
      title={pad ? t('shell.titleBar.vibrateHint', { label }) : label}
      aria-label={pad ? t('shell.titleBar.vibrateLabel', { label }) : label}
      onClick={() => {
        if (pad) void bridge.setup.rumble({ deviceId: pad.id }).catch(() => undefined);
      }}
      className={cn(chipClass, playerClass(player.number), 'text-ink-2')}
    >
      <Icon name={devicesIcon(devices.map((d) => d.kind))} className="size-[17px] text-pc" />
      <span>{t('shell.titleBar.playerShort', { number: player.number })}</span>
    </button>
  );
}

/** Revealed devices without a player, as one chip so the title bar keeps room for the breadcrumb. */
function FreeDevicesChip({ devices, pulse }: { devices: SetupDevice[]; pulse: number }) {
  const ref = usePulse(pulse);
  const { t, format } = useI18n();
  const names = format.list(devices.map((d) => d.name));
  const kinds = [...new Set(devices.map((d) => deviceIcon(d.kind)))].slice(0, 3);
  const count = devices.length;
  return (
    <span
      ref={ref}
      role="img"
      title={t('shell.titleBar.freeDevicesHint', { names })}
      aria-label={t('shell.titleBar.freeDevicesLabel', { count, names })}
      className={cn(chipClass, 'cursor-default text-ink-3 hover:bg-pane')}
    >
      {kinds.map((icon) => (
        <Icon key={icon} name={icon} className="size-[17px]" />
      ))}
      <span>{t('shell.titleBar.freeDevices', { count })}</span>
    </span>
  );
}

interface Pad {
  index: number;
  /** Name the browser gives, without its vendor and product ids; empty when it gives none. */
  name: string;
}

/** Gamepads the browser sees (Gamepad API), pulsing on button presses. */
function GamepadTray() {
  const t = useT();
  const [pads, setPads] = useState<Pad[]>([]);
  const [pulse, setPulse] = useState<Record<number, number>>({});

  useEffect(() => {
    let raf = 0;
    const pressed = new Map<number, boolean>();
    const read = () => (navigator.getGamepads ? [...navigator.getGamepads()].filter((p): p is Gamepad => Boolean(p?.connected)) : []);
    const sync = () => {
      const list = read().map((p) => ({ index: p.index, name: p.id.replace(/\s*\(.*$/, '').trim() }));
      setPads((old) => (old.length === list.length && old.every((p, i) => p.index === list[i]?.index) ? old : list));
      return list.length > 0;
    };
    const poll = () => {
      raf = 0;
      for (const p of read()) {
        const down = p.buttons.some((b) => b.pressed);
        if (down && !pressed.get(p.index)) setPulse((s) => ({ ...s, [p.index]: (s[p.index] ?? 0) + 1 }));
        pressed.set(p.index, down);
      }
      if (sync()) raf = requestAnimationFrame(poll);
    };
    const start = () => {
      if (sync() && !raf) raf = requestAnimationFrame(poll);
    };
    window.addEventListener('gamepadconnected', start);
    window.addEventListener('gamepaddisconnected', start);
    start();
    return () => {
      window.removeEventListener('gamepadconnected', start);
      window.removeEventListener('gamepaddisconnected', start);
      if (raf) cancelAnimationFrame(raf);
    };
  }, []);

  if (pads.length === 0) {
    return (
      <span
        className={cn(chipClass, 'app-no-drag cursor-default text-ink-3 hover:bg-pane')}
        title={t('shell.titleBar.noControllerHint')}
      >
        <Icon name="pad" className="size-[17px]" />
        <span>{t('shell.titleBar.noController')}</span>
      </span>
    );
  }
  return (
    <div className="app-no-drag flex shrink-0 items-center gap-1" role="group" aria-label={t('shell.titleBar.controllers')}>
      {pads.map((p) => (
        <GamepadChip key={p.index} pad={p} pulse={pulse[p.index] ?? 0} />
      ))}
    </div>
  );
}

function GamepadChip({ pad, pulse }: { pad: Pad; pulse: number }) {
  const ref = usePulse(pulse);
  const t = useT();
  const number = pad.index + 1;
  const name = pad.name || t('shell.titleBar.controllerFallback', { number });
  return (
    <span ref={ref} title={name} aria-label={t('shell.titleBar.controllerConnected', { name })} role="img" className={cn(chipClass, 'cursor-default text-ink-2')}>
      <Icon name="pad" className="size-[17px] text-ink-2" />
      <span>{t('shell.titleBar.controllerShort', { number })}</span>
    </span>
  );
}

function WindowControls() {
  const { data: state } = useWindowState();
  const t = useT();
  const maximized = state?.maximized ?? false;
  const resize = maximized ? t('shell.window.restore') : t('shell.window.maximize');
  const btn =
    'grid w-[46px] place-items-center border-0 bg-transparent text-ink-2 hover:bg-pane-2 hover:text-ink focus-visible:-outline-offset-2';
  return (
    <div className="app-no-drag ml-2 flex self-stretch">
      <button
        type="button"
        aria-label={t('shell.window.minimize')}
        title={t('shell.window.minimize')}
        className={btn}
        onClick={() => void bridge.window.minimize().catch(() => undefined)}
      >
        <Icon name="win-min" strokeWidth={1.4} className="size-4" />
      </button>
      <button type="button" aria-label={resize} title={resize} className={btn} onClick={() => void bridge.window.toggleMaximize().catch(() => undefined)}>
        <Icon name={maximized ? 'win-restore' : 'win-max'} strokeWidth={1.4} className="size-4" />
      </button>
      <button
        type="button"
        aria-label={t('common.close')}
        title={t('common.close')}
        className={cn(btn, 'hover:bg-win-close hover:text-white')}
        onClick={() => void bridge.window.close().catch(() => undefined)}
      >
        <Icon name="x" strokeWidth={1.4} className="size-4" />
      </button>
    </div>
  );
}
