// Lobby: "press a button to join" session setup for one game.
// The host owns the setup (screens, layouts, devices, players, steps); this screen renders its snapshot and sends
// commands. Only interaction state lives here: drags, a player picked up with the keyboard, typing, and the flashes
// that answer a device's activity.
import { useCallback, useEffect, useRef, useState } from 'react';
import { useNavigate, useParams } from 'react-router';
import { useQueryClient } from '@tanstack/react-query';
import { bridge, bridgeQueryKey, useBridgeEvent, useBridgeQuery, type SetupSnapshot } from '@/bridge';
import { HandlerNotes } from '@/components/HandlerNotes';
import { useAddGame } from '@/features/add-game';
import { errorMessage } from '@/lib/bridge-ui';
import { deviceNoun } from '@/lib/devices';
import { heroOf, useLibrary } from '@/lib/library';
import { isSessionLive, useSessionSnapshot, useSetupSnapshot } from '@/lib/live';
import { useScreenChrome } from '@/shell/chrome';
import { useSharedCouchNav } from '@/shell/couch-nav';
import { ButtonLink, Dialog, Icon, Pane, Spinner, useToast } from '@/ui';
import { Canvas, type ResizeMode } from './Canvas';
import { useLobbyCommands } from './commands';
import { LobbyHeader, profileTitle } from './LobbyHeader';
import { playerName } from './model';
import { ProfilesManager } from './ProfilesDialog';
import { LaunchPane, SidePanel } from './SidePanel';
import { CustomStep, OptionsStep } from './Steps';
import { usePressToJoin } from './usePressToJoin';

const notesSeenKey = (gameId: string) => `nucleus.notesShown.${gameId}`;

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

  // Pads belong to players here (any press on a free one joins): every pad moves focus, only the one that moved last
  // presses buttons, and B only closes a menu or a dialog.
  useSharedCouchNav();

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
      { id: gameId, name: snap?.gameName ?? summary?.name ?? 'this game' },
      { executableNames: fileName ? [fileName] : undefined, initialDirectory: cut > 0 ? executablePath?.slice(0, cut) : undefined },
    );
  }, [locateGame, gameId, snap?.gameName, summary?.name, executablePath]);
  const commands = useLobbyCommands(onNeedsPicker);

  const loadProfile = async (profileId: number) => {
    const loaded = await commands.loadProfile(profileId);
    const p = profiles.data?.find((x) => x.id === profileId);
    if (loaded) toast(`Loaded ${p ? profileTitle(p) : 'the profile'}. Players join as they press a button.`);
    return Boolean(loaded);
  };
  const unloadProfile = async () => {
    const unloaded = await commands.unloadProfile();
    if (unloaded) toast('Profile unloaded. Place players freely.');
    return Boolean(unloaded);
  };

  // ---------- Device activity: press to join, flashes, pings ----------
  usePressToJoin({
    snap,
    enabled: Boolean(snap) && !launching,
    onFlash: (deviceId) => setFlashes((f) => ({ ...f, [deviceId]: (f[deviceId] ?? 0) + 1 })),
    onPing: (playerId) => setPings((s) => ({ ...s, [playerId]: (s[playerId] ?? 0) + 1 })),
    announce,
  });

  useBridgeEvent('setup.deviceLost', ({ kind, playerNumber, nickname }) => {
    const who = playerNumber ? `${nickname?.trim() || `Player ${playerNumber}`}${nickname ? ` (player ${playerNumber})` : ''}` : 'A player';
    toast(`${who} lost their ${deviceNoun(kind)}. Plug it back in, or press a button on another one.`, { icon: 'warning', durationMs: 5000 });
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
    if (gone.length) announce(`${gone.map(([, name]) => name).join(', ')} left.`);
    if (!fresh.length) return;
    setJustJoined((s) => new Set([...s, ...fresh]));
    const t = window.setTimeout(() => setJustJoined((s) => new Set([...s].filter((id) => !fresh.includes(id)))), 900);
    return () => window.clearTimeout(t);
  }, [snap, announce]);

  // A pad's X reaches the focused player area as the key the keyboard uses: R starts resizing, X switches corners.
  const keyToFocusedPlayer = (key: string) => () => {
    const area = document.activeElement?.closest<HTMLElement>('[data-player-id]');
    area?.dispatchEvent(new KeyboardEvent('keydown', { key, bubbles: true, cancelable: true }));
  };
  useScreenChrome([{ label: 'Library', to: '/library' }, { label: snap?.gameName ?? summary?.name ?? 'Setting up' }], {
    base: [{ button: 'A', label: 'Any button on a free controller joins; move, then A selects' }],
    byFocus: {
      'player-resizable': [
        { button: 'A', label: 'Pick up or place' },
        { button: 'X', label: 'Resize', action: keyToFocusedPlayer('r') },
      ],
      resizing: [
        { key: 'Arrows', label: 'Move the corner' },
        { button: 'X', label: 'Other corner', action: keyToFocusedPlayer('x') },
        { button: 'A', label: 'Done' },
      ],
    },
    aside: (
      <span>
        Keyboard: <b>Enter</b> picks up a player, <b>R</b> resizes, <b>Delete</b> takes them out
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
          <h1 className="type-h2">This game can't be set up</h1>
          <p className="text-ink-2">{openError ?? errorMessage(detailsError)}</p>
          <ButtonLink to="/library" variant="primary" data-nav>
            <Icon name="arrow-l" />
            Back to library
          </ButtonLink>
        </div>
      </Pane>
    );
  }

  if (!snap) {
    return (
      <Pane className="grid h-full place-items-center">
        <span className="inline-flex items-center gap-2.5 text-small text-ink-2" role="status">
          <Spinner /> Opening the setup of {summary?.name ?? 'the game'}
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
        />
      )}

      <aside aria-label="Session" className="grid min-h-0 grid-rows-[minmax(0,1fr)_auto] gap-gutter">
        <SidePanel snap={snap} commands={commands} pings={pings} announce={announce} />
        <LaunchPane snap={snap} commands={commands} launching={launching} onLaunch={() => void launch()} />
      </aside>

      <Dialog open={notesOpen} onClose={() => setNotesOpen(false)} title="Handler notes" icon="book" className="w-[min(720px,90vw)]">
        <p className="mb-4 text-13 text-ink-3">
          {snap.gameName}
          {details?.summary.handlerVersion !== undefined ? `, handler version ${details.summary.handlerVersion}` : ''}
          {details?.maintainer ? ` by ${details.maintainer}` : ''}
        </p>
        {details?.notes ? <HandlerNotes>{details.notes}</HandlerNotes> : <p className="text-ink-2">This handler has no notes.</p>}
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
