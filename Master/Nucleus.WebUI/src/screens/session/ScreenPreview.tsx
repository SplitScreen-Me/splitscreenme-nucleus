import type { CSSProperties, ReactNode } from 'react';
import type { Rect, SessionInstance, SetupScreen, SetupSnapshot } from '@/bridge';
import { T, useT, type TFunction } from '@/i18n';
import { deviceNoun, devicesIcon } from '@/lib/devices';
import { cn, Icon, Pane, PlayerTag, playerClass, Spinner } from '@/ui';
import { STEP_LABEL, STEPS, nicknameOf, stageLabel, stepOf, type SessionView } from './model';
import { useElementSize } from './useElementSize';

/** Every copy renders its own view of the game: offset the art so the windows don't look cloned. */
const ART_POSITIONS = ['47% 84%', '50% 16%', '66% 30%', '30% 56%', '18% 40%', '82% 62%', '58% 74%', '12% 22%'];

/** "1920 × 1080": digits as they are, like a resolution is written. */
const res = (r: Pick<Rect, 'width' | 'height'>, t: TFunction) =>
  t('session.preview.resolution', { width: String(r.width), height: String(r.height) });

/** Read by screen readers only (a step's state after its visible name). */
const SR_ONLY = { sr: (c: ReactNode) => <span className="sr-only">{c}</span> };

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
  const t = useT();
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
          {multi
            ? t('session.preview.screenCount', { count: screens.length })
            : primary
              ? t('session.preview.display', { number: primary.displayIndex })
              : t('session.preview.windowsTitle')}
        </h2>
        {multi
          ? screens.map((sc) => <ScreenCaption key={sc.index} screen={sc} used={used.has(sc.index)} />)
          : primary && (
              <span className="data text-small text-ink-3">
                {primary.isPrimary
                  ? t('session.preview.resolutionPrimary', { resolution: res(primary.bounds, t) })
                  : res(primary.bounds, t)}
              </span>
            )}
        <span className="ml-auto text-small text-ink-2">{t('session.preview.windowCount', { count: s.instances.length })}</span>
      </div>

      <div ref={wrapRef} className="relative min-h-[180px] flex-1">
        {size.width > 0 &&
          screens.map((sc) => (
            <div
              key={sc.index}
              role="group"
              aria-label={t(sc.isPrimary ? 'session.preview.displayLabelPrimary' : 'session.preview.displayLabel', {
                number: sc.displayIndex,
                resolution: res(sc.bounds, t),
              })}
              className="absolute rounded-xl bg-bezel shadow-[0_0_0_1px_var(--color-line)]"
              style={place(sc.bounds, multi ? 3 : 0)}
            >
              {!used.has(sc.index) && (
                <span className="absolute inset-0 grid place-items-center text-13 text-ink-3">
                  {multi ? t('session.preview.displayUnused', { number: sc.displayIndex }) : t('session.preview.unused')}
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
              {ended ? t('session.preview.noWindowOpened') : t('session.preview.preparing')}
            </span>
          </div>
        )}
      </div>
    </Pane>
  );
}

const CAPTION_TAGS = { b: (c: ReactNode) => <b className="font-[650]">{c}</b> };

function ScreenCaption({ screen: sc, used }: { screen: SetupScreen; used: boolean }) {
  const t = useT();
  return (
    <span className={cn('text-13', used ? 'text-ink-2' : 'text-ink-3')}>
      <T
        k={sc.isPrimary ? 'session.preview.captionPrimary' : 'session.preview.caption'}
        values={{ number: sc.displayIndex, resolution: <span className="data">{res(sc.bounds, t)}</span> }}
        tags={CAPTION_TAGS}
      />
    </span>
  );
}

function InstancePane({ inst, ended, art, style }: { inst: SessionInstance; ended: boolean; art?: string; style: CSSProperties }) {
  const t = useT();
  const step = ended ? -1 : stepOf(inst.stage);
  const ready = !ended && inst.stage === 'ready';
  const closed = ended || inst.stage === 'exited';
  const label = stageLabel(inst.stage, ended, t);
  const name = nicknameOf(inst, t);
  const devices = inst.devices.map(deviceNoun);

  return (
    <article
      aria-label={t('session.instance.label', { number: inst.playerNumber, name, stage: label })}
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
          <span className="data ml-auto shrink-0 text-12 text-ink-3 @max-[220px]:hidden">
            {t('session.instance.pid', { pid: String(inst.processId) })}
          </span>
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
            <span className="truncate">{devices.length ? [...new Set(devices)].join(' + ') : t('session.instance.noDevice')}</span>
          </span>
          <span className="data ml-auto whitespace-nowrap text-ink-3 @max-[300px]:hidden">{res(inst.bounds, t)}</span>
        </div>
        <ol className="m-0 grid list-none grid-cols-3 gap-[3px] p-0" aria-label={t('session.instance.progress')}>
          {STEPS.map((stepId, i) => {
            const done = i <= step;
            const current = i === step;
            return (
              <li key={stepId} className="grid gap-1" aria-current={current ? 'step' : undefined}>
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
                  <T
                    k={done ? 'session.instance.stepDone' : 'session.instance.stepPending'}
                    values={{ step: t(STEP_LABEL[stepId]) }}
                    tags={SR_ONLY}
                  />
                </span>
              </li>
            );
          })}
        </ol>
      </div>
    </article>
  );
}
