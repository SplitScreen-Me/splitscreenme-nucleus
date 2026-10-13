import { useEffect, useRef, useState, type ReactNode } from 'react';
import type { CheckState, SetupCheck, SetupPlayer, SetupSnapshot, SetupStep, SetupStepKind } from '@/bridge';
import { useT, type MessageKey, type TFunction } from '@/i18n';
import { deviceIcon, isGamepadKind } from '@/lib/devices';
import { Button, cn, Icon, MiniField, MiniInput, MiniSelect, Pane, PlayerTag, playerClass, Spinner, Switch, type IconName } from '@/ui';
import type { LobbyCommands } from './commands';
import { startDrag } from './dnd';
import { byNumber, devicesOf, guestsOf, hintText, isFree, playerName, reduceMotion, seatCount, whereOf } from './model';

export interface SidePanelProps {
  snap: SetupSnapshot;
  commands: LobbyCommands;
  pings: Record<string, number>;
  announce: (message: string) => void;
}

/** Side column: players with their identity and audio, then the pre-launch checks. Scrolls on its own. */
export function SidePanel({ snap, commands, pings, announce }: SidePanelProps) {
  const t = useT();
  const max = snap.capabilities.maxPlayers;
  const seats = seatCount(snap);
  const freeRevealed = snap.devices.filter((d) => isFree(d) && d.revealed);
  const note =
    seats >= max
      ? t('lobby.players.full', { game: snap.gameName, count: max })
      : t(freeRevealed.length ? 'lobby.players.moreCanJoinFree' : 'lobby.players.moreCanJoin', { count: max - seats });

  return (
    <Pane className="min-h-0 overflow-y-auto">
      <section aria-labelledby="lobby-players-title" className="border-b border-line px-[18px] py-4">
        <div className="flex items-baseline justify-between gap-3">
          <h2 id="lobby-players-title" className="type-h3">
            {t('lobby.players.title')}
          </h2>
          <span className="data text-15 text-ink-2">{t('lobby.players.count', { seats, max })}</span>
        </div>
        <ol className="m-0 mt-2 list-none p-0">
          {byNumber(snap.players).map((p) => (
            <PlayerRow key={p.id} player={p} snap={snap} commands={commands} ping={pings[p.id] ?? 0} announce={announce} />
          ))}
        </ol>
        <p className="mt-2.5 rounded-ctl bg-pane-2 px-3 py-2.5 text-13 leading-[1.45] text-ink-2">{note}</p>
        {snap.capabilities.gamepads && (
          <label className="mt-3 flex items-start justify-between gap-3 text-13 leading-[1.4] text-ink-2">
            <span>
              <b className="block font-[650] text-ink">{t('lobby.players.apiIndex.label')}</b>
              {t('lobby.players.apiIndex.description')}
            </span>
            <Switch
              checked={snap.useApiIndex}
              onChange={(on) => void commands.setUseApiIndex(on)}
              aria-label={t('lobby.players.apiIndex.label')}
              className="mt-0.5"
            />
          </label>
        )}
      </section>

      <section aria-labelledby="lobby-checks-title" className="px-[18px] py-4">
        <h2 id="lobby-checks-title" className="type-h3">
          {t('setup.checks.title')}
        </h2>
        <ul className="m-0 mt-2.5 list-none p-0">
          {snap.checks.map((c) => (
            <CheckRow key={c.id} check={c} busy={commands.busyChecks.has(c.id)} onAction={() => void commands.runCheck(c.id)} />
          ))}
        </ul>
      </section>
    </Pane>
  );
}

function PlayerRow({
  player: p,
  snap,
  commands,
  ping,
  announce,
}: {
  player: SetupPlayer;
  snap: SetupSnapshot;
  commands: LobbyCommands;
  ping: number;
  announce: (message: string) => void;
}) {
  const t = useT();
  const tag = useRef<HTMLSpanElement>(null);
  const [nick, setNick] = useState<string | null>(null);
  const [steam, setSteam] = useState<string | null>(null);
  const devices = devicesOf(snap, p);
  const guests = guestsOf(snap, p);
  const freePads = snap.devices.filter((d) => isFree(d) && d.revealed && isGamepadKind(d.kind));
  const canAddGuest = p.maxGuests > guests.length && freePads.length > 0;
  const audioChoices = snap.audioOutputs.some((a) => a.value === 'Default')
    ? snap.audioOutputs
    : [{ value: 'Default', label: t('lobby.player.defaultAudio') }, ...snap.audioOutputs];

  useEffect(() => {
    if (!ping || !tag.current || reduceMotion()) return;
    tag.current.animate([{ transform: 'scale(1.18)' }, { transform: 'scale(1)' }], { duration: 320, easing: 'cubic-bezier(0.2, 0.9, 0.1, 1)' });
  }, [ping]);

  const commitNick = async () => {
    const value = nick?.trim();
    setNick(null);
    if (value === undefined || value === '' || value === (p.nickname ?? '')) return;
    await commands.setIdentity(p.id, { nickname: value });
  };
  const commitSteam = async () => {
    const value = steam?.trim();
    setSteam(null);
    if (value === undefined || value === (p.steamId ?? '')) return;
    await commands.setIdentity(p.id, { steamId: value });
  };
  const removeAll = async () => {
    if (await commands.removePlayer(p)) announce(t('lobby.announce.playerLeft', { player: playerName(p) }));
  };

  return (
    <li
      className={cn(playerClass(p.number), 'grid grid-cols-[auto_minmax(0,1fr)] gap-x-3 border-t border-line py-3 first:border-t-0')}
    >
      <span
        ref={tag}
        draggable
        onDragStart={(e) => startDrag(e, { kind: 'player', id: p.id })}
        title={t('lobby.player.dragHint')}
        className="mt-1.5 inline-flex cursor-grab self-start active:cursor-grabbing"
      >
        <PlayerTag n={p.number} size="lg" />
      </span>
      <div className="min-w-0">
        <div className="flex items-center gap-1">
          <input
            value={nick ?? p.nickname ?? ''}
            placeholder={t('lobby.player.name', { number: p.number })}
            aria-label={t('lobby.player.nicknameLabel', { number: p.number })}
            spellCheck={false}
            maxLength={24}
            onChange={(e) => setNick(e.target.value)}
            onBlur={() => void commitNick()}
            onKeyDown={(e) => {
              if (e.key === 'Enter') e.currentTarget.blur();
              if (e.key === 'Escape') {
                const el = e.currentTarget;
                setNick(null);
                requestAnimationFrame(() => el.blur());
              }
            }}
            className="-ml-2 h-[34px] min-w-0 flex-1 rounded-md border-0 bg-transparent px-2 text-17 font-[720] text-ink stretch-semi placeholder:text-ink-3 hover:bg-pane-2 focus:bg-pane-2 focus:shadow-[inset_0_0_0_1.5px_var(--color-ink)] focus:outline-none"
          />
          {p.canExpand && (
            <Button
              variant="quiet"
              size="icon-sm"
              aria-label={t('lobby.player.expandLabel', { number: p.number })}
              title={t('lobby.player.expandHint')}
              onClick={() => void commands.toggleExpand(p.id)}
              className="text-ink-3"
            >
              <Icon name="layers" />
            </Button>
          )}
          <Button
            variant="quiet"
            size="icon-sm"
            aria-label={t('lobby.player.removeLabel', { number: p.number })}
            title={t('lobby.player.remove')}
            onClick={() => void removeAll()}
            className="text-ink-3"
          >
            <Icon name="x" />
          </Button>
        </div>
        <ul className="m-0 mt-px mb-[9px] grid list-none gap-0.5 p-0 text-13 text-ink-2">
          {devices.map((d) => (
            <li key={d.id} className="flex items-center gap-1.5 whitespace-nowrap">
              <Icon name={deviceIcon(d.kind)} className="size-4 shrink-0 text-pc" />
              <span className="min-w-0 truncate">{d.name}</span>
              {devices.length > 1 && (
                <button
                  type="button"
                  aria-label={t('lobby.player.removeDeviceLabel', { device: d.name, number: p.number })}
                  title={t('lobby.player.removeDevice')}
                  onClick={() => void commands.unassign(d.id)}
                  className="grid size-5 shrink-0 place-items-center rounded border-0 bg-transparent text-ink-3 hover:bg-pane-3 hover:text-ink"
                >
                  <Icon name="x" className="size-3" />
                </button>
              )}
            </li>
          ))}
          {guests.map((d) => (
            <li key={d.id} className="flex items-center gap-1.5 whitespace-nowrap">
              <Icon name="users" className="size-4 shrink-0 text-ink-3" />
              <span className="min-w-0 truncate">{t('lobby.player.guest', { device: d.name })}</span>
              <button
                type="button"
                aria-label={t('lobby.player.removeGuestLabel', { device: d.name })}
                title={t('lobby.player.removeGuest')}
                onClick={() => void commands.removeGuest(d.id)}
                className="grid size-5 shrink-0 place-items-center rounded border-0 bg-transparent text-ink-3 hover:bg-pane-3 hover:text-ink"
              >
                <Icon name="x" className="size-3" />
              </button>
            </li>
          ))}
          <li className="text-ink-3">{whereOf(snap, p)}</li>
        </ul>
        <div className="grid grid-cols-[1.15fr_1fr] gap-1.5">
          <MiniField title={t(snap.customAudio ? 'lobby.player.audioHint' : 'lobby.player.audioHintOff')}>
            <Icon name={p.audioDeviceId && p.audioDeviceId !== 'Default' ? 'headphones' : 'speaker'} />
            <MiniSelect
              aria-label={t('lobby.player.audioLabel', { number: p.number })}
              value={p.audioDeviceId ?? 'Default'}
              onChange={(e) => void commands.setAudio(p.id, e.target.value)}
            >
              {audioChoices.map((a) => (
                <option key={a.value} value={a.value}>
                  {a.label}
                </option>
              ))}
            </MiniSelect>
          </MiniField>
          <MiniField title={t('lobby.player.steamIdHint')}>
            Steam
            <MiniInput
              value={steam ?? p.steamId ?? ''}
              placeholder={t('common.default')}
              inputMode="numeric"
              aria-label={t('lobby.player.steamIdLabel', { number: p.number })}
              spellCheck={false}
              onChange={(e) => setSteam(e.target.value)}
              onBlur={() => void commitSteam()}
              onKeyDown={(e) => e.key === 'Enter' && e.currentTarget.blur()}
            />
          </MiniField>
        </div>
        {canAddGuest && (
          <MiniField className="mt-1.5" title={t('lobby.player.addGuestHint')}>
            <Icon name="users" />
            <MiniSelect
              aria-label={t('lobby.player.addGuestLabel', { number: p.number })}
              value=""
              onChange={(e) => e.target.value && void commands.addGuest(p.id, e.target.value)}
            >
              <option value="">{t('lobby.player.addGuest')}</option>
              {freePads.map((d) => (
                <option key={d.id} value={d.id}>
                  {d.name}
                </option>
              ))}
            </MiniSelect>
          </MiniField>
        )}
      </div>
    </li>
  );
}

const CHECK_ICON: Record<CheckState, IconName> = { ok: 'check', warning: 'warning', blocking: 'x', info: 'info', busy: 'refresh' };
const CHECK_TONE: Record<CheckState, string> = {
  ok: 'bg-ok/16 text-ok',
  warning: 'bg-warn/18 text-warn',
  blocking: 'bg-danger/18 text-danger',
  info: 'bg-pane-3 text-ink-2',
  busy: 'bg-pane-3 text-ink-2',
};
const CHECK_WORD: Record<CheckState, MessageKey> = {
  ok: 'setup.checks.state.ok',
  warning: 'setup.checks.state.warning',
  blocking: 'setup.checks.state.blocking',
  info: 'setup.checks.state.info',
  busy: 'setup.checks.state.busy',
};

function CheckRow({ check: c, busy, onAction }: { check: SetupCheck; busy: boolean; onAction: () => void }) {
  const t = useT();
  const state: CheckState = busy ? 'busy' : c.state;
  return (
    <li className="grid grid-cols-[22px_minmax(0,1fr)_auto] items-start gap-x-2.5 py-2">
      <span className={cn('grid size-[22px] place-items-center rounded-full', CHECK_TONE[state])}>
        <Icon name={CHECK_ICON[state]} strokeWidth={2.4} className={cn('size-[13px]', state === 'busy' && 'animate-spin')} label={t(CHECK_WORD[state])} />
      </span>
      <div className="min-w-0">
        <div className="text-14 leading-[1.35] font-semibold">{c.title}</div>
        {c.detail && <div className="text-12 leading-[1.4] [overflow-wrap:anywhere] text-ink-3">{c.detail}</div>}
      </div>
      {c.actionLabel && state !== 'busy' && (
        <Button variant="ghost" onClick={onAction} className="min-h-[30px] px-[11px] text-12" data-nav>
          {c.actionLabel}
        </Button>
      )}
    </li>
  );
}

export function LaunchPane({
  snap,
  commands,
  launching,
  onLaunch,
}: {
  snap: SetupSnapshot;
  commands: LobbyCommands;
  launching: boolean;
  onLaunch: () => void;
}) {
  const t = useT();
  const { flow } = snap;
  const step = flow.steps[flow.current];
  const nextStep = flow.steps[flow.current + 1];
  const blocking = snap.checks.find((c) => c.state === 'blocking');
  const warning = snap.checks.find((c) => c.state === 'warning');
  const busy = snap.checks.find((c) => c.state === 'busy');
  const hint = hintText(snap.hint);
  const n = seatCount(snap);
  const reasonId = 'lobby-launch-reason';

  let note: ReactNode;
  if (launching) note = <span>{t('setup.launch.starting')}</span>;
  else if (blocking)
    note = (
      <>
        <Icon name="warning" className="size-4 shrink-0 text-danger" />
        <span>{t('setup.launch.blocking', { check: blocking.title, section: t('setup.checks.title') })}</span>
      </>
    );
  else if (busy)
    note = (
      <>
        <Spinner />
        <span>{t('setup.launch.busy', { check: busy.title })}</span>
      </>
    );
  else if (!flow.canPlay && nextStep && flow.canGoNext) note = <span>{nextLabel(t, nextStep)}</span>;
  else if (!flow.canPlay) note = <span>{hint ?? t(n === 0 ? 'setup.launch.noPlayers' : 'setup.launch.finishStep')}</span>;
  else if (warning)
    note = (
      <>
        <Icon name="warning" className="size-4 shrink-0 text-warn" />
        <span>{t('setup.launch.warning', { check: warning.title })}</span>
      </>
    );
  else
    note = (
      <>
        <span className="flex shrink-0 gap-[3px]">
          {byNumber(snap.players).map((p) => (
            <PlayerTag key={p.id} n={p.number} size="sm" />
          ))}
        </span>
        <span>{hint ?? t('setup.launch.ready', { count: n, game: snap.gameName })}</span>
      </>
    );

  return (
    <Pane as="section" aria-label={t('setup.launch.label')} className="grid gap-3 px-4 pt-3.5 pb-4">
      {flow.steps.length > 1 && (
        <ol aria-label={t('setup.flow.stepsLabel')} className="m-0 flex list-none gap-1 p-0">
          {flow.steps.map((s, i) => (
            <li
              key={`${s.kind}-${s.customStepIndex ?? i}`}
              aria-current={i === flow.current ? 'step' : undefined}
              className="grid min-w-0 flex-1 gap-1"
              title={s.title}
            >
              <span className={cn('h-1 rounded-full', i < flow.current ? 'bg-ink-2' : i === flow.current ? 'bg-ink' : 'bg-pane-3')} />
              <span className={cn('truncate text-12 font-[650]', i === flow.current ? 'text-ink' : 'text-ink-3')}>
                {t('setup.flow.step', { number: i + 1, title: s.title })}
              </span>
            </li>
          ))}
        </ol>
      )}
      <p id={reasonId} className="flex min-h-6 items-center gap-2.5 text-13 leading-[1.4] text-ink-2" aria-live="polite">
        {note}
      </p>
      {flow.steps.length > 1 && (
        <div className={cn('grid gap-2', nextStep ? 'grid-cols-2' : 'grid-cols-1')}>
          <Button variant="ghost" disabled={!flow.canGoBack} onClick={() => void commands.back()} data-nav>
            <Icon name="arrow-l" />
            {nextStep ? t('common.back') : backLabel(t, flow.steps[flow.current - 1])}
          </Button>
          {nextStep && (
            <Button variant="default" disabled={!flow.canGoNext} onClick={() => void commands.next()} data-nav>
              {t('setup.flow.next')}
              <Icon name="arrow-r" />
            </Button>
          )}
        </div>
      )}
      <Button
        variant="primary"
        size="lg"
        disabled={!flow.canPlay || launching}
        aria-describedby={reasonId}
        onClick={onLaunch}
        data-nav
        className="w-full [&_svg]:size-4"
      >
        {launching ? <Spinner /> : <Icon name="play" />}
        {launching ? t('setup.launch.launching') : t('setup.launch.button', { count: n })}
      </Button>
      <label className="flex items-center justify-between gap-3 text-13 text-ink-2">
        <span>{t('setup.launch.saveProfile')}</span>
        <Switch
          checked={snap.saveProfileOnLaunch}
          onChange={(on) => void commands.setSaveProfileOnLaunch(on)}
          aria-label={t('setup.launch.saveProfile')}
        />
      </label>
      {step?.kind !== 'players' && <span className="sr-only">{t('setup.flow.currentStep', { step: step?.title ?? '' })}</span>}
    </Pane>
  );
}

// The app's own steps are named by the UI, in the middle of a sentence; a handler's step goes by its title.
const NEXT_STEP: Partial<Record<SetupStepKind, MessageKey>> = { players: 'setup.launch.nextPlayers', options: 'setup.launch.nextOptions' };
const BACK_TO_STEP: Partial<Record<SetupStepKind, MessageKey>> = { players: 'setup.flow.backToPlayers', options: 'setup.flow.backToOptions' };

/** "Next: options. Then launch." */
function nextLabel(t: TFunction, next: SetupStep): string {
  const key = NEXT_STEP[next.kind];
  return key ? t(key) : t('setup.launch.nextStep', { step: next.title });
}

/** The back button of the last step: "Back to" the previous one. */
function backLabel(t: TFunction, previous: SetupStep | undefined): string {
  if (!previous) return t('setup.flow.backToPrevious');
  const key = BACK_TO_STEP[previous.kind];
  return key ? t(key) : t('setup.flow.backTo', { step: previous.title });
}
