import type { CSSProperties } from 'react';
import type { Rect, SessionInstance, SetupScreen, SetupSnapshot } from '@/bridge';
import { deviceNoun, devicesIcon } from '@/lib/devices';
import { cn, Icon, Pane, PlayerTag, playerClass, Spinner } from '@/ui';
import { STEPS, nicknameOf, stageLabel, stepOf, type SessionView } from './model';
import { useElementSize } from './useElementSize';

/** Every copy renders its own view of the game: offset the art so the windows don't look cloned. */
const ART_POSITIONS = ['47% 84%', '50% 16%', '66% 30%', '30% 56%', '18% 40%', '82% 62%', '58% 74%', '12% 22%'];

const res = (r: Pick<Rect, 'width' | 'height'>) => `${r.width} × ${r.height}`;

function union(rects: readonly Rect[]): Rect | undefined {
  if (rects.length === 0) return undefined;
  const left = Math.min(...rects.map((r) => r.x));
  const top = Math.min(...rects.map((r) => r.y));
  const right = Math.max(...rects.map((r) => r.x + r.width));
  const bottom = Math.max(...rects.map((r) => r.y + r.height));
  return { x: left, y: top, width: right - left, height: bottom - top };
}

/**
 * The monitors, to scale and in their desktop arrangement, with one pane per game window. The session snapshot has no
 * monitors: they come from the setup snapshot taken at Play; without it the windows are drawn on their own.
 */
export function ScreenPreview({ view, setup, art }: { view: SessionView; setup: SetupSnapshot | null | undefined; art?: string }) {
  const [wrapRef, size] = useElementSize<HTMLDivElement>();
  const { snapshot: s, ended } = view;
  const screens = setup?.screens ?? [];
  const total = union([...screens.map((sc) => sc.bounds), ...s.instances.map((i) => i.bounds)]);
  const scale = total ? Math.max(0, Math.min(size.width / total.width, size.height / total.height)) : 0;
  const offsetX = total ? (size.width - total.width * scale) / 2 : 0;
  const offsetY = total ? (size.height - total.height * scale) / 2 : 0;
  const place = (r: Rect, inset = 0): CSSProperties =>
    total
      ? {
          left: Math.round(offsetX + (r.x - total.x) * scale) + inset,
          top: Math.round(offsetY + (r.y - total.y) * scale) + inset,
          width: Math.round(r.width * scale) - inset * 2,
          height: Math.round(r.height * scale) - inset * 2,
        }
      : {};
  const used = new Set(s.instances.map((i) => i.screenIndex));
  const multi = screens.length > 1;
  const primary = screens.find((sc) => sc.isPrimary) ?? screens[0];

  return (
    <Pane
      as="section"
      aria-labelledby="session-screen-title"
      className="flex min-h-0 flex-1 flex-col px-5 pt-[18px] pb-5 [@media(max-height:820px)]:pt-4 [@media(max-height:820px)]:pb-4"
    >
      <div className="mb-3.5 flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <h2 id="session-screen-title" className="text-16 leading-tight font-[650]">
          {multi ? `${screens.length} screens` : primary ? `Display ${primary.displayIndex}` : 'Game windows'}
        </h2>
        {multi
          ? screens.map((sc) => <ScreenCaption key={sc.index} screen={sc} used={used.has(sc.index)} />)
          : primary && (
              <span className="data text-small text-ink-3">
                {res(primary.bounds)}
                {primary.isPrimary ? ', primary' : ''}
              </span>
            )}
        <span className="ml-auto text-small text-ink-2">
          {s.instances.length > 0 ? `${s.instances.length} ${s.instances.length === 1 ? 'window' : 'windows'}` : 'No window yet'}
        </span>
      </div>

      <div ref={wrapRef} className="relative min-h-[180px] flex-1">
        {size.width > 0 &&
          screens.map((sc) => (
            <div
              key={sc.index}
              role="group"
              aria-label={`Display ${sc.displayIndex}, ${res(sc.bounds)}${sc.isPrimary ? ', primary' : ''}`}
              className="absolute rounded-xl bg-bezel shadow-[0_0_0_1px_var(--color-line)]"
              style={place(sc.bounds, multi ? 3 : 0)}
            >
              {!used.has(sc.index) && (
                <span className="absolute inset-0 grid place-items-center text-13 text-ink-3">
                  {multi ? `Display ${sc.displayIndex}: not used` : 'Not used'}
                </span>
              )}
            </div>
          ))}
        {size.width > 0 &&
          s.instances.map((inst) => (
            <InstancePane key={inst.playerNumber} inst={inst} ended={ended} art={art} style={place(inst.bounds, 2)} />
          ))}
        {size.width > 0 && s.instances.length === 0 && (
          <div className="absolute inset-0 grid place-items-center text-small text-ink-2">
            <span className="inline-flex items-center gap-2.5">
              {!ended && <Spinner />}
              {ended ? 'No game window was opened.' : 'Preparing the instances…'}
            </span>
          </div>
        )}
      </div>
    </Pane>
  );
}

function ScreenCaption({ screen: sc, used }: { screen: SetupScreen; used: boolean }) {
  return (
    <span className={cn('text-13', used ? 'text-ink-2' : 'text-ink-3')}>
      <b className="font-[650]">Display {sc.displayIndex}</b> <span className="data">{res(sc.bounds)}</span>
      {sc.isPrimary ? ', primary' : ''}
    </span>
  );
}

function InstancePane({ inst, ended, art, style }: { inst: SessionInstance; ended: boolean; art?: string; style: CSSProperties }) {
  const step = ended ? -1 : stepOf(inst.stage);
  const ready = !ended && inst.stage === 'ready';
  const closed = ended || inst.stage === 'exited';
  const label = stageLabel(inst.stage, ended);
  const name = nicknameOf(inst);
  const devices = inst.devices.map(deviceNoun);

  return (
    <article
      aria-label={`Player ${inst.playerNumber}, ${name}: ${label}`}
      className={cn(
        playerClass(inst.playerNumber),
        'absolute flex flex-col justify-between overflow-hidden rounded-lg bg-pane-2 bg-[length:auto_175%] bg-no-repeat px-3 py-2.5 [container-type:size]',
        'transition-[left,top,width,height] duration-(--dur-3) ease-snap',
      )}
      style={{
        ...style,
        backgroundImage: art ? `url("${art}")` : undefined,
        backgroundPosition: ART_POSITIONS[(inst.playerNumber - 1) % ART_POSITIONS.length],
      }}
    >
      {/* Legibility scrim: darker until the copy is ready, then it opens up to show the game. */}
      <span
        aria-hidden="true"
        className="absolute inset-0 bg-[linear-gradient(180deg,rgba(7,8,10,0.72)_0%,rgba(7,8,10,0.25)_40%,rgba(7,8,10,0.86)_100%)]"
      />
      <span
        aria-hidden="true"
        className={cn('absolute inset-0 bg-[rgba(7,8,10,0.62)] transition-opacity duration-(--dur-3)', ready ? 'opacity-0' : 'opacity-100')}
      />
      <span aria-hidden="true" className="pointer-events-none absolute inset-0 rounded-[inherit] shadow-[inset_0_0_0_2px_var(--pc)]" />

      <div className="relative flex items-center gap-2">
        <PlayerTag n={inst.playerNumber} />
        <b className="truncate text-15 font-bold @max-[150px]:hidden">{name}</b>
        {inst.processId !== undefined && !closed && (
          <span className="data ml-auto shrink-0 text-12 text-ink-3 @max-[220px]:hidden">PID {inst.processId}</span>
        )}
      </div>

      <div
        className={cn(
          'relative flex items-center justify-center gap-2 text-center text-small font-semibold text-ink-2 [@container_(max-height:150px)]:hidden',
          ready && 'invisible',
        )}
      >
        {!closed && inst.stage !== 'ready' && <Spinner />}
        {closed && <Icon name="stop" className="size-4 text-ink-3" />}
        <span>{label}</span>
      </div>

      <div className="relative grid gap-2">
        <div className="flex flex-wrap gap-x-3.5 gap-y-1 text-13 text-ink-2 [@container_(max-height:96px)]:hidden">
          <span className="inline-flex min-w-0 items-center gap-1.5 whitespace-nowrap">
            <Icon name={devicesIcon(inst.devices)} className="size-[15px] text-pc" />
            <span className="truncate">{devices.length ? [...new Set(devices)].join(' + ') : 'No device'}</span>
          </span>
          <span className="data ml-auto whitespace-nowrap text-ink-3 @max-[300px]:hidden">{res(inst.bounds)}</span>
        </div>
        <ol className="m-0 grid list-none grid-cols-3 gap-[3px] p-0" aria-label="Launch progress">
          {STEPS.map((stepName, i) => {
            const done = i <= step;
            const current = i === step;
            return (
              <li key={stepName} className="grid gap-1" aria-current={current ? 'step' : undefined}>
                <span className="block h-1 overflow-hidden rounded-[2px] bg-white/14">
                  <span
                    className="block h-full origin-left bg-pc transition-transform duration-300 ease-snap"
                    style={{ transform: `scaleX(${done ? 1 : 0})` }}
                  />
                </span>
                <span
                  className={cn(
                    'truncate text-11 stretch-narrow [@container_(max-height:150px)]:hidden',
                    current ? 'font-[650] text-ink' : done ? 'text-ink-2' : 'text-ink-3',
                  )}
                >
                  {stepName}
                  <span className="sr-only">{done ? ', done' : ', pending'}</span>
                </span>
              </li>
            );
          })}
        </ol>
      </div>
    </article>
  );
}
