// Lobby: "press Start to join" session setup for one game.
// The host owns the setup (screens, layouts, devices, players, steps); this screen renders its snapshot and sends
// commands. Only interaction state lives here: drags, a player picked up with the keyboard, an empty place waiting for
// a controller, typing, and the flashes that answer a device's activity.
import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeEvent, useBridgeQuery, type SetupSnapshot } from '@/bridge';
import { HandlerNotes } from '@/components/HandlerNotes';
import { useAddGame } from '@/features/add-game';
import { T, useI18n, type MessageKey } from '@/i18n';
import { errorMessage } from '@/lib/bridge-ui';
import { heroOf, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot, useSetupSnapshot } from '@/lib/live';
import { useScreenChrome } from '@/shell/chrome';
import { useCapturedCouchNav, useSharedCouchNav } from '@/shell/couch-nav';
import { ButtonLink, Dialog, Icon, Pane, Spinner, useToast } from '@/ui';
import { Canvas, type ResizeMode } from './Canvas';
import { useLobbyCommands } from './commands';
import { spotStillOpen, type WaitingSpot } from './join-rules';
import { lobbyHints } from './hints';
import { LobbyHeader, profileTitle } from './LobbyHeader';
import { deviceById, deviceGroup, placeName, playerName, type DeviceGroup } from './model';
import { ProfilesManager } from './ProfilesDialog';
import { LaunchPane, SidePanel } from './SidePanel';
import { CustomStep, OptionsStep } from './Steps';
import { usePressToJoin } from './usePressToJoin';

const notesSeenKey = (gameId: string) => `nucleus.notesShown.${gameId}`;

/** What a player who lost a device can do, by the kind of device: one sentence each. */
const DEVICE_LOST: Record<DeviceGroup, { known: MessageKey; unknown: MessageKey }> = {
  gamepad: { known: 'lobby.deviceLost.gamepad', unknown: 'lobby.deviceLost.unknownPlayer.gamepad' },
  keyboard: { known: 'lobby.deviceLost.keyboard', unknown: 'lobby.deviceLost.unknownPlayer.keyboard' },
  mouse: { known: 'lobby.deviceLost.mouse', unknown: 'lobby.deviceLost.unknownPlayer.mouse' },
  keyboardMouse: { known: 'lobby.deviceLost.keyboardMouse', unknown: 'lobby.deviceLost.unknownPlayer.keyboardMouse' },
};

function notesSeen(gameId: string): boolean {
  try {
    return sessionStorage.getItem(notesSeenKey(gameId)) === '1';
  } catch {
    return false;
  }
}

function markNotesSeen(gameId: string): void {
  try {
    sessionStorage.setItem(notesSeenKey(gameId), '1');
  } catch {
    // Storage unavailable: the notes may show again next visit.
  }
}

export default function LobbyScreen() {
  const { gameId = '' } = useParams();
  const navigate = useNavigate();
  const qc = useQueryClient();
  const toast = useToast();
  const { t, format } = useI18n();
  const { locateGame } = useAddGame();
  const { data: setup, isPending: setupPending } = useSetupSnapshot();
  const { data: session, isPending: sessionPending } = useSessionSnapshot();
  const { data: details, error: detailsError } = useBridgeQuery('library.get', { gameId }, { enabled: Boolean(gameId) });
  const { data: library = [] } = useLibrary();
  const summary = library.find((g) => g.id === gameId);
  const snap: SetupSnapshot | null = setup && setup.gameId === gameId ? setup : null;

  const [openError, setOpenError] = useState<string | null>(null);
  const [notesOpen, setNotesOpen] = useState(false);
  const [launching, setLaunching] = useState(false);
  const [announcement, setAnnouncement] = useState('');
  const [pings, setPings] = useState<Record<string, number>>({});
  const [flashes, setFlashes] = useState<Record<string, number>>({});
  const [justJoined, setJustJoined] = useState<ReadonlySet<string>>(new Set());
  const [profilesOpen, setProfilesOpen] = useState(false);
  const [resizing, setResizing] = useState<ResizeMode | null>(null);
  // An empty place picked by clicking or activating it: the next press of a free pad takes it.
  const [spot, setSpot] = useState<WaitingSpot | null>(null);

  // Pads belong to players here (Start on a free one joins): every pad moves focus, only the one that moved last
  // presses buttons, and B only closes a menu or a dialog. While a place waits for a controller, the pads only cancel
  // (B): their next press belongs to that place.
  useSharedCouchNav();
  useCapturedCouchNav(spot !== null);

  // ---------- Open the setup of this game (once), unless a session runs ----------
  const live = isSessionLive(session?.state);
  useEffect(() => {
    if (live) navigate('/session', { replace: true });
  }, [live, navigate]);

  const opening = useRef<string | null>(null);
  useEffect(() => {
    if (!gameId || setupPending || sessionPending || live) return;
    if (setup && setup.gameId === gameId) return;
    if (opening.current === gameId) return;
    opening.current = gameId;
    setOpenError(null);
    bridge.setup
      .open({ gameId })
      .then((opened) => {
        // An event may already have brought a newer snapshot of this setup.
        qc.setQueryData<SetupSnapshot | null>(bridgeQueryKey('setup.get'), (current) =>
          current && current.gameId === opened.gameId && current.revision >= opened.revision ? current : opened,
        );
      })
      .catch((error: unknown) => {
        opening.current = null;
        setOpenError(errorMessage(error));
      });
  }, [gameId, setup, setupPending, sessionPending, live, qc]);

  // ---------- Handler notes: shown by themselves on the first visit ----------
  useEffect(() => {
    if (!details?.showNotesFirst || !details.notes || notesSeen(gameId)) return;
    markNotesSeen(gameId);
    setNotesOpen(true);
  }, [details, gameId]);

  const profiles = useBridgeQuery('profiles.list', undefined, { enabled: Boolean(snap) });
  // The profiles belong to the open setup: another game's list must not show while this one loads.
  const profilesGame = snap?.gameId;
  useEffect(() => {
    if (profilesGame) void qc.resetQueries({ queryKey: bridgeQueryKey('profiles.list') });
  }, [profilesGame, qc]);

  const announce = useCallback((message: string) => setAnnouncement(message), []);
  const executablePath = details?.executablePath;
  const onNeedsPicker = useCallback(async () => {
    // The last known path names the executable the handler expects, and the folder to start in.
    const cut = executablePath ? Math.max(executablePath.lastIndexOf('\\'), executablePath.lastIndexOf('/')) : -1;
    const fileName = executablePath && cut >= 0 ? executablePath.slice(cut + 1) : undefined;
    await locateGame(
      { id: gameId, name: snap?.gameName ?? summary?.name ?? t('lobby.thisGame') },
      { executableNames: fileName ? [fileName] : undefined, initialDirectory: cut > 0 ? executablePath?.slice(0, cut) : undefined },
    );
  }, [locateGame, gameId, snap?.gameName, summary?.name, executablePath, t]);
  const commands = useLobbyCommands(onNeedsPicker);

  const loadProfile = async (profileId: number) => {
    const loaded = await commands.loadProfile(profileId);
    const p = profiles.data?.find((x) => x.id === profileId);
    if (loaded) toast(p ? t('profiles.toasts.loaded', { profile: profileTitle(p) }) : t('profiles.toasts.loadedUnnamed'));
    return Boolean(loaded);
  };
  const unloadProfile = async () => {
    const unloaded = await commands.unloadProfile();
    if (unloaded) toast(t('profiles.toasts.unloaded'));
    return Boolean(unloaded);
  };
  // ---------- A place waiting for a controller ----------
  const liveSpot = useRef({ spot, snap });
  liveSpot.current = { spot, snap };
  const pickSpot = useCallback(
    (picked: WaitingSpot) => {
      setSpot(picked);
      const screen = liveSpot.current.snap?.screens.find((s) => s.index === picked.screenIndex);
      announce(screen ? t('lobby.spot.waiting', { place: placeName(screen, picked.slotIndex) }) : t('lobby.spot.waitingHere'));
    },
    [announce, t],
  );
  const cancelSpot = useCallback(() => {
    if (!liveSpot.current.spot) return;
    setSpot(null);
    announce(t('lobby.spot.cancelled'));
  }, [announce, t]);
  const takeSpot = useCallback(
    async (deviceId: string, at: WaitingSpot) => {
      setSpot(null);
      const name = liveSpot.current.snap ? deviceById(liveSpot.current.snap, deviceId)?.name : undefined;
      const next =
        at.slotIndex === undefined
          ? await commands.assignAt(deviceId, at.screenIndex, at.x, at.y)
          : await commands.assignToSlot(deviceId, at.screenIndex, at.slotIndex);
      const player = next?.players.find((p) => p.deviceIds.includes(deviceId));
      if (!player) return;
      const joined = { player: playerName(player), number: player.number };
      announce(name ? t('lobby.announce.joined', { device: name, ...joined }) : t('lobby.announce.controllerJoined', joined));
      // Focus follows the new player, so the keyboard and the pads carry on from there.
      requestAnimationFrame(() => document.querySelector<HTMLElement>(`[data-player-id="${CSS.escape(player.id)}"]`)?.focus({ preventScroll: true }));
    },
    [commands, announce, t],
  );

  // The place stops waiting when it is gone or taken, off the players step, and while launching.
  const stepKind = snap?.flow.steps[snap.flow.current]?.kind;
  const onCanvas = Boolean(snap) && stepKind !== 'options' && stepKind !== 'custom';
  useEffect(() => {
    if (spot && (!snap || !onCanvas || launching || !spotStillOpen(spot, snap.screens))) setSpot(null);
  }, [spot, snap, onCanvas, launching]);

  // Escape cancels wherever the focus is.
  useEffect(() => {
    if (!spot) return;
    const onKey = (e: KeyboardEvent) => {
      if (e.key !== 'Escape' || document.querySelector('dialog[open]')) return;
      e.preventDefault();
      e.stopPropagation();
      cancelSpot();
    };
    window.addEventListener('keydown', onKey, true);
    return () => window.removeEventListener('keydown', onKey, true);
  }, [spot, cancelSpot]);

  // ---------- Device activity: press to join, flashes, pings ----------
  usePressToJoin({
    snap,
    enabled: Boolean(snap) && !launching,
    spot,
    onTakeSpot: (deviceId, at) => void takeSpot(deviceId, at),
    onFlash: (deviceId) => setFlashes((f) => ({ ...f, [deviceId]: (f[deviceId] ?? 0) + 1 })),
    onPing: (playerId) => setPings((s) => ({ ...s, [playerId]: (s[playerId] ?? 0) + 1 })),
    announce,
  });

  useBridgeEvent('setup.deviceLost', ({ kind, playerNumber, nickname }) => {
    const keys = DEVICE_LOST[deviceGroup(kind)];
    const named = nickname?.trim();
    const who = !playerNumber
      ? undefined
      : named
        ? t('lobby.deviceLost.namedPlayer', { nickname: named, number: playerNumber })
        : t('lobby.player.name', { number: playerNumber });
    toast(who ? t(keys.known, { player: who }) : t(keys.unknown), { icon: 'warning', durationMs: 5000 });
  });

  // Players who just appeared get the join animation; joins and leaves are announced.
  const seated = useRef<{ gameId: string; players: Map<string, string> } | null>(null);
  useEffect(() => {
    if (!snap) return;
    const now = new Map(snap.players.map((p) => [p.id, playerName(p)]));
    const before = seated.current?.gameId === snap.gameId ? seated.current.players : null;
    seated.current = { gameId: snap.gameId, players: now };
    if (!before) return;
    const fresh = [...now.keys()].filter((id) => !before.has(id));
    const gone = [...before.entries()].filter(([id]) => !now.has(id));
    if (gone.length) announce(t('lobby.announce.playersLeft', { count: gone.length, names: format.list(gone.map(([, name]) => name)) }));
    if (!fresh.length) return;
    setJustJoined((s) => new Set([...s, ...fresh]));
    const timer = window.setTimeout(() => setJustJoined((s) => new Set([...s].filter((id) => !fresh.includes(id)))), 900);
    return () => window.clearTimeout(timer);
  }, [snap, announce]); // Not t or format: a change of language changes no player.

  // A pad's X reaches the focused player area as the key the keyboard uses: R starts resizing, X switches corners.
  const keyToFocusedPlayer = (key: string) => () => {
    const area = document.activeElement?.closest<HTMLElement>('[data-player-id]');
    area?.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true }));
  };
  // Only what the buttons do whatever has focus (see hints.ts); while a place waits for a controller, B cancels.
  useScreenChrome([{ label: t('lobby.breadcrumbs.library'), to: '/library' }, { label: snap?.gameName ?? summary?.name ?? t('lobby.breadcrumbs.settingUp') }], {
    ...lobbyHints(spot !== null, { cancelWait: cancelSpot, resize: keyToFocusedPlayer('r'), otherCorner: keyToFocusedPlayer('x') }),
    aside: spot ? (
      <span>
        <T k="lobby.keys.waiting" values={{ cancel: 'Esc' }} tags={{ b: (c) => <b>{c}</b> }} />
      </span>
    ) : (
      <span>
        <T k="lobby.keys.help" values={{ pickUp: 'Enter', resize: 'R', remove: 'Delete' }} tags={{ b: (c) => <b>{c}</b> }} />
      </span>
    ),
  });

  const launch = async () => {
    if (!snap?.flow.canPlay) return;
    setLaunching(true);
    const started = await commands.play();
    if (started) navigate('/session');
    else setLaunching(false);
  };

  // ---------- Render ----------
  if (openError || (detailsError && !snap)) {
    return (
      <Pane className="grid h-full place-items-center p-8">
        <div className="grid max-w-[48ch] justify-items-start gap-4">
          <Icon name="warning" className="size-7 text-warn" />
          <h1 className="type-h2">{t('lobby.openError.title')}</h1>
          <p className="text-ink-2">{openError ?? errorMessage(detailsError)}</p>
          <ButtonLink to="/library" variant="primary" data-nav>
            <Icon name="arrow-l" />
            {t('lobby.backToLibrary')}
          </ButtonLink>
        </div>
      </Pane>
    );
  }

  if (!snap) {
    return (
      <Pane className="grid h-full place-items-center">
        <span className="inline-flex items-center gap-2.5 text-small text-ink-2" role="status">
          <Spinner /> {summary?.name ? t('lobby.opening', { game: summary.name }) : t('lobby.openingUnnamed')}
        </span>
      </Pane>
    );
  }

  const step = snap.flow.steps[snap.flow.current];
  const customStep = step?.kind === 'custom' ? snap.customSteps.find((s) => s.index === step.customStepIndex) : undefined;
  const art = summary ? heroOf(summary) : (details?.summary.heroUrl ?? details?.summary.coverUrl);

  return (
    <div className="grid h-full min-h-[560px] grid-cols-[minmax(0,1fr)_360px] grid-rows-[auto_minmax(0,1fr)] gap-gutter max-[1300px]:grid-cols-[minmax(0,1fr)_330px]">
      <LobbyHeader
        snap={snap}
        details={details}
        summary={summary}
        profiles={profiles.data ?? []}
        onOpenNotes={() => setNotesOpen(true)}
        onLoadProfile={(profileId) => void loadProfile(profileId)}
        onUnloadProfile={() => void unloadProfile()}
        onManageProfiles={() => setProfilesOpen(true)}
      />

      {step?.kind === 'options' ? (
        <OptionsStep snap={snap} commands={commands} />
      ) : customStep ? (
        <CustomStep step={customStep} commands={commands} />
      ) : (
        <Canvas
          snap={snap}
          art={art}
          commands={commands}
          pings={pings}
          flashes={flashes}
          justJoined={justJoined}
          announce={announce}
          resizing={resizing}
          onResizing={setResizing}
          spot={spot}
          onPickSpot={pickSpot}
          onCancelSpot={cancelSpot}
        />
      )}

      <aside aria-label={t('lobby.asideLabel')} className="grid min-h-0 grid-rows-[minmax(0,1fr)_auto] gap-gutter">
        <SidePanel snap={snap} commands={commands} pings={pings} announce={announce} />
        <LaunchPane snap={snap} commands={commands} launching={launching} onLaunch={() => void launch()} />
      </aside>

      <Dialog open={notesOpen} onClose={() => setNotesOpen(false)} title={t('lobby.notes.title')} icon="book" className="w-[min(720px,90vw)]">
        <p className="mb-4 text-13 text-ink-3">{notesByline(t, snap.gameName, details?.summary.handlerVersion, details?.maintainer)}</p>
        {details?.notes ? <HandlerNotes>{details.notes}</HandlerNotes> : <p className="text-ink-2">{t('lobby.notes.empty')}</p>}
      </Dialog>

      <ProfilesManager
        snap={snap}
        profiles={profiles.data ?? []}
        open={profilesOpen}
        onClose={() => setProfilesOpen(false)}
        onLoad={loadProfile}
        onUnload={unloadProfile}
      />

      <div className="sr-only" aria-live="polite">
        {announcement}
      </div>
    </div>
  );
}

/** "Game, handler version 3 by someone": the parts known. */
function notesByline(t: ReturnType<typeof useI18n>['t'], game: string, version: number | undefined, maintainer: string | undefined): string {
  if (version !== undefined && maintainer) return t('lobby.notes.withVersionAndMaintainer', { game, version: String(version), maintainer });
  if (version !== undefined) return t('lobby.notes.withVersion', { game, version: String(version) });
  if (maintainer) return t('lobby.notes.withMaintainer', { game, maintainer });
  return game;
}
