using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Newtonsoft.Json;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Generic.Step;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// State shared by the setup, profiles and session services, and the headless replacement of what the WinForms
    /// setup screen and Core_Interface did: open a game (UI_Actions.ChangeGame), run the device/screen helpers without a
    /// control, the "pump" that replaces the paint side effects (profile matching, player identities), the step machine
    /// and the handler options. Every member runs on the UI thread except the documented thread-safe entry points.
    /// </summary>
    internal sealed class SetupSession
    {
        private readonly IBridgeEvents events;

        /// <summary>Bridge events (for services sharing this session).</summary>
        public IBridgeEvents Events => events;
        private readonly SynchronizationContext ui;
        private readonly Timer refreshTimer;
        private readonly HashSet<string> autoSelectedSteps = new HashSet<string>();
        private readonly Dictionary<string, bool> lastActivity = new Dictionary<string, bool>();
        private readonly Action<GameProfile.NoticeKind, string, string, int> nativeNotify;

        private bool[] stepRequired = new bool[0];
        private int refreshQueued;
        private bool pumpQueued;
        private string? currentSignature;
        private SetupSnapshot? lastSnapshot;
        private long revision;
        private long emittedRevision;
        private List<string>? capturedErrors;
        private DateTime suppressDeviceLostUntil;

        public SetupSession(IBridgeEvents events, IHostShell shell)
        {
            this.events = events;
            Shell = shell;
            ui = shell.UiContext ?? SynchronizationContext.Current ?? new SynchronizationContext();
            refreshTimer = new Timer(_ => ui.Post(__ => Safe(EmitIfChanged), null), null, Timeout.Infinite, Timeout.Infinite);
            Audio = new AudioOutputs();

            //Headless mode of the device/screen helpers: their timers post their work to the UI thread.
            DevicesFunctions.Dispatch = action => ui.Post(_ => Safe(action), null);

            DevicesFunctions.DevicesUpdated += OnDevicesUpdated;
            DevicesFunctions.AssignedDeviceLost += OnAssignedDeviceLost;
            BoundsFunctions.ScreensChanged += OnScreensChanged;
            PlayerInfo.FlashChanged += OnFlashChanged;
            GameProfile.CanPlayChanged += OnCanPlayChanged;
            GameProfile.OnReadyAutoPlayTrigger += OnAutoPlayTrigger;
            GameProfile.OnReset += OnProfileReset;

            nativeNotify = GameProfile.Notify;
            GameProfile.Notify = OnNotify;
        }

        public IHostShell Shell { get; }

        /// <summary>Audio outputs (cached).</summary>
        public AudioOutputs Audio { get; }

        /// <summary>Handler updates (handlers service), when available.</summary>
        public IHandlerUpdater? HandlerUpdater { get; set; }

        /// <summary>The game whose setup is open, or null.</summary>
        public UserGameInfo? Game { get; private set; }

        public bool IsOpen => Game != null && GameProfile.Instance != null;

        /// <summary>Set by the session service from Play to the end of the session: the setup objects belong to the launch.</summary>
        public bool SessionActive { get; set; }

        /// <summary>Current step (identity, independent of which steps are shown).</summary>
        public StepId CurrentStep { get; set; } = StepId.Players;

        /// <summary>Checks with a running fix (shown as busy).</summary>
        public HashSet<string> BusyChecks { get; } = new HashSet<string>();

        /// <summary>Custom step option keys whose card was restored from a loaded profile.</summary>
        public IReadOnlyCollection<string> AutoSelectedSteps => autoSelectedSteps;

        /// <summary>Raised when a setup opens or closes.</summary>
        public event Action? OpenChanged;

        /// <summary>A loaded profile matched every player with AutoPlay on and the flow allows playing: start the session.</summary>
        public Action? AutoPlayRequested { get; set; }

        /// <summary>Profiles of the open game were saved, deleted or renamed.</summary>
        public void ProfilesChanged()
        {
            if (Game != null)
            {
                events.Emit(new Bridge.Contracts.ProfilesChanged { GameId = Game.GameGuid ?? "" });
            }
        }

        // ---------------------------------------------------------------- errors

        public static BridgeException Conflict(string reason, string message) => new BridgeException(BridgeErrorKind.Conflict, message, new { reason });

        public void EnsureOpen()
        {
            if (!IsOpen)
            {
                throw Conflict("noSetup", "No game setup is open.");
            }
        }

        public void EnsureEditable()
        {
            EnsureOpen();

            if (SessionActive)
            {
                throw Conflict("sessionRunning", "A session is running.");
            }
        }

        private static void Safe(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                ServiceLog.Error("setup: unexpected error", e);
            }
        }

        public void Post(Action action) => ui.Post(_ => Safe(action), null);

        // ---------------------------------------------------------------- open / close / reset

        /// <summary>
        /// Opens the setup of a game (UI_Actions.ChangeGame + Core_Interface.Current_UserGameInfo + SetupScreenControl.Initialize):
        /// content cleanup, new GameProfile, headless screens and devices, handler option defaults, players step.
        /// Requirements are no longer checked here: they are reported as pre-launch checks.
        /// </summary>
        public void Open(UserGameInfo game)
        {
            if (IsOpen)
            {
                Close();
            }

            GameProfile.GameInfo = game;
            GameProfile.Game = game.Game;

            GenericGameInfo gen = game.Game;

            if (!gen.KeepSymLinkOnExit && !gen.MetaInfo.KeepSymLink)
            {
                try
                {
                    _ = CleanGameContent.CleanContentFolderAsync(gen);
                }
                catch (Exception e)
                {
                    ServiceLog.Warn("setup: content folder cleanup failed", e);
                }
            }

            InitializeProfile(game, null);
            autoSelectedSteps.Clear();
            Audio.Refresh();

            OpenChanged?.Invoke();
        }

        /// <summary>New GameProfile, screens, device detection; option values kept from <paramref name="keepOptions"/> when given.</summary>
        private void InitializeProfile(UserGameInfo game, IDictionary<string, object>? keepOptions)
        {
            Game = game;

            GameProfile profile = new GameProfile();
            profile.InitializeDefault();

            GenericGameInfo gen = GameProfile.Game;

            if (gen.UseHandlerSteamIds && gen.PlayerSteamIDs != null && gen.PlayerSteamIDs.Length > 0)
            {
                GameProfile.GenMissingIdFromPlayerSteamIDs();
            }

            BoundsFunctions.Initialize(game, profile);
            profile.SyncScreens();
            DevicesFunctions.Initialize(game, profile);

            if (keepOptions != null)
            {
                foreach (KeyValuePair<string, object> option in keepOptions)
                {
                    if (profile.Options.ContainsKey(option.Key))
                    {
                        profile.Options[option.Key] = option.Value;
                    }
                }
            }

            OptionsCodec.Merge(gen.Options, profile.Options);

            CurrentStep = StepId.Players;
            lastActivity.Clear();
            RefreshRequired();
            Pump();
        }

        /// <summary>Closes the setup (RefreshUI(true)): stops device detection and the screens watcher.</summary>
        public void Close()
        {
            DevicesFunctions.DisposeGamePadTimer();
            BoundsFunctions.StopScreensWatcher();
            Game = null;
            currentSignature = null;
            lastSnapshot = null;
            OpenChanged?.Invoke();
        }

        /// <summary>
        /// After every session end: the launch changed the setup objects (CleanClone merges keyboard/mouse pairs, empties
        /// the screens' slots, prunes screens, and the devices keep runtime state), so the setup starts again from a new
        /// GameProfile on the reloaded handler, as when the game is selected again. Option values are kept.
        /// </summary>
        public void ResetAfterSession()
        {
            UserGameInfo? game = Game;

            if (game == null)
            {
                return;
            }

            Dictionary<string, object> options = new Dictionary<string, object>(GameProfile.Instance?.Options ?? new Dictionary<string, object>());

            //InitializeHandlerStartup reloaded the handler script: use the new instance (as a new game selection does).
            GameProfile.Game = game.Game ?? GameProfile.Game;

            InitializeProfile(game, options);
            autoSelectedSteps.Clear();
            RequestRefresh();
        }

        /// <summary>Unassigns everything and unloads the profile (GameProfile.Reset), keeping the option values when no profile was loaded.</summary>
        public void ResetSetup()
        {
            Dictionary<string, object>? keep = GameProfile.Loaded ? null : new Dictionary<string, object>(GameProfile.Instance.Options);

            GameProfile.Instance.Reset();
            AfterProfileReset(keep);
        }

        /// <summary>Common work after GameProfile.Reset / LoadGameProfile: options merge, tray layout, players step.</summary>
        public void AfterProfileReset(IDictionary<string, object>? keepOptions)
        {
            Dictionary<string, object> options = GameProfile.Instance.Options;

            if (keepOptions != null)
            {
                foreach (KeyValuePair<string, object> option in keepOptions)
                {
                    if (options.ContainsKey(option.Key))
                    {
                        options[option.Key] = option.Value;
                    }
                }
            }

            autoSelectedSteps.Clear();

            foreach (string key in OptionsCodec.Merge(GameProfile.Game.Options, options))
            {
                if (GameProfile.Loaded)
                {
                    autoSelectedSteps.Add(key);
                }
            }

            GameProfile.Instance.SyncScreens();
            DevicesFunctions.UpdateDevices();
            CurrentStep = StepId.Players;
            RefreshRequired();
            Pump();
        }

        /// <summary>The user picked a card: it is no longer "restored from the profile".</summary>
        public void ClearAutoSelected(string optionKey) => autoSelectedSteps.Remove(optionKey);

        /// <summary>Device lost events are ignored for a moment (an explicit rescan replaces the device objects).</summary>
        public void SuppressDeviceLost(TimeSpan duration) => suppressDeviceLostUntil = DateTime.UtcNow + duration;

        // ---------------------------------------------------------------- pump

        /// <summary>
        /// Profile matching and player identities for every device: what SetupScreenControl.OnPaint did. Called after
        /// each command, after each device tick and on each input flash.
        /// </summary>
        public void Pump()
        {
            if (!IsOpen || SessionActive)
            {
                return;
            }

            try
            {
                GameProfile.ProcessProfileAssignment();
            }
            catch (Exception e)
            {
                ServiceLog.Warn("setup: profile matching failed", e);
            }
        }

        private void RequestPump()
        {
            if (pumpQueued)
            {
                return;
            }

            pumpQueued = true;
            Post(() =>
            {
                pumpQueued = false;
                Pump();
                RequestRefresh();
            });
        }

        // ---------------------------------------------------------------- snapshot

        /// <summary>
        /// Builds the snapshot; the revision increases when anything changed since the previous one. Schedules the
        /// coalesced <c>setup.changed</c>. Null when no setup is open.
        /// </summary>
        public SetupSnapshot? Commit()
        {
            if (!IsOpen)
            {
                return null;
            }

            //During a session the launch threads change the setup objects (CleanClone, PlayerID, process data): keep
            //showing the setup as it was when Play was pressed.
            if (SessionActive && lastSnapshot != null)
            {
                return lastSnapshot;
            }

            SetupSnapshot snapshot = SetupSnapshotBuilder.Build(this);
            snapshot.Revision = 0;
            string signature = JsonConvert.SerializeObject(snapshot);

            if (signature != currentSignature)
            {
                currentSignature = signature;
                revision++;
            }

            snapshot.Revision = revision;
            lastSnapshot = snapshot;

            if (revision != emittedRevision)
            {
                RequestRefresh();
            }

            return snapshot;
        }

        /// <summary>Commits and returns the snapshot of a command (never null: commands require an open setup).</summary>
        public SetupSnapshot CommitCommand()
        {
            Pump();
            return Commit() ?? throw Conflict("noSetup", "No game setup is open.");
        }

        /// <summary>Thread-safe: schedules a commit and a <c>setup.changed</c> event in about 100 ms (coalesced).</summary>
        public void RequestRefresh()
        {
            if (Interlocked.Exchange(ref refreshQueued, 1) == 0)
            {
                refreshTimer.Change(100, Timeout.Infinite);
            }
        }

        private void EmitIfChanged()
        {
            Interlocked.Exchange(ref refreshQueued, 0);

            if (SessionActive)
            {
                return;
            }

            SetupSnapshot? snapshot = Commit();

            if (snapshot != null && snapshot.Revision != emittedRevision)
            {
                emittedRevision = snapshot.Revision;
                events.Emit(new SetupChanged { Snapshot = snapshot });
            }
        }

        // ---------------------------------------------------------------- flow

        /// <summary>Re-evaluates which custom steps are required (the handler's UpdateRequired callbacks).</summary>
        public void RefreshRequired()
        {
            List<CustomStep> steps = GameProfile.Game?.CustomSteps ?? new List<CustomStep>();
            bool[] required = new bool[steps.Count];

            for (int i = 0; i < steps.Count; i++)
            {
                try
                {
                    steps[i].UpdateRequired?.Invoke();
                }
                catch (Exception e)
                {
                    ServiceLog.Warn($"setup: UpdateRequired of custom step {i} failed", e);
                }

                required[i] = steps[i].Required;
            }

            stepRequired = required;
        }

        public bool IsStepRequired(int index) => index >= 0 && index < stepRequired.Length && stepRequired[index];

        /// <summary>Whether the players step is valid (the pure counterpart of DevicesFunctions.CanProceed, plus the guests of a profile).</summary>
        public static bool PlayersValid()
        {
            if (GameProfile.Loaded)
            {
                return GameProfile.TotalAssignedPlayers == GameProfile.TotalProfilePlayers &&
                       GameProfile.AssignedDevices.All(p => p.InstanceGuests.Count == p.CurrentMaxGuests);
            }

            return GameProfile.Instance?.DevicesList?.Count(d => d.ScreenIndex != -1) >= 1;
        }

        public FlowInput FlowInput()
        {
            GenericGameInfo gen = GameProfile.Game;
            Dictionary<string, object> options = GameProfile.Instance.Options;

            return new FlowInput
            {
                PlayersValid = PlayersValid(),
                HasVisibleOptions = gen.Options.Any(o => !o.Hidden && OptionsCodec.KindOf(o) != OptionKind.Unsupported),
                CustomSteps = gen.CustomSteps.Select((step, i) => new CustomStepState(
                    step.Title,
                    IsStepRequired(i),
                    step.Option != null && OptionsCodec.ResolveStepItem(step.Option, options.TryGetValue(step.Option.Key, out object value) ? value : null) != null)).ToList(),
            };
        }

        /// <summary>The flow of the current step; Play also needs every blocking pre-launch check to pass.</summary>
        public SetupFlow ComputeFlow(IReadOnlyList<SetupCheck>? checks = null)
        {
            SetupFlow flow = SetupFlowRules.Compute(FlowInput(), CurrentStep);

            if (flow.CanPlay && (checks ?? SetupChecks.Compute(this)).Any(c => c.State == CheckState.Blocking))
            {
                flow.CanPlay = false;
            }

            return flow;
        }

        /// <summary>
        /// A loaded profile matched every player (GameProfile.CanPlayChanged(true, true) / OnReadyAutoPlayTrigger): go to
        /// the first required custom step without a card, or the last step; with AutoPlay, start when the flow allows it
        /// (a custom step that couldn't be restored stops the auto play, instead of launching with a saved JSON text).
        /// </summary>
        private void OnProfileMatched(bool autoPlay)
        {
            if (!IsOpen || SessionActive)
            {
                return;
            }

            RefreshRequired();
            FlowInput input = FlowInput();

            if (CurrentStep.Kind == SetupStepKind.Players)
            {
                CurrentStep = SetupFlowRules.AfterProfileMatched(input);
            }

            if (autoPlay && ComputeFlow().CanPlay)
            {
                AutoPlayRequested?.Invoke();
            }

            RequestRefresh();
        }

        // ---------------------------------------------------------------- legacy events

        private void OnDevicesUpdated()
        {
            if (IsOpen && !SessionActive)
            {
                RequestPump();
            }
        }

        private void OnFlashChanged(PlayerInfo device, bool active)
        {
            //Any thread (raw input thread, polling, thread pool).
            Post(() =>
            {
                if (!IsOpen || SessionActive || !GameProfile.Instance.DevicesList.Contains(device))
                {
                    return;
                }

                string id = SetupSnapshotBuilder.KeyOf(device);

                if (!lastActivity.TryGetValue(id, out bool last) || last != active)
                {
                    lastActivity[id] = active;
                    events.Emit(new DeviceActivity { DeviceId = id, Active = active });
                }

                if (active)
                {
                    RequestPump();//keyboards are revealed by input; button-press profile matching
                }
            });
        }

        private void OnAssignedDeviceLost(PlayerInfo device, int playerIndex, bool guest)
        {
            if (!IsOpen || SessionActive)
            {
                return;
            }

            if (DateTime.UtcNow >= suppressDeviceLostUntil)
            {
                events.Emit(new DeviceLost
                {
                    DeviceId = SetupSnapshotBuilder.KeyOf(device),
                    Kind = DeviceIds.KindOf(device.InputType),
                    PlayerNumber = playerIndex >= 0 ? playerIndex + 1 : (int?)null,
                    Nickname = playerIndex >= 0 && playerIndex < GameProfile.AssignedDevices.Count ? GameProfile.AssignedDevices[playerIndex].Nickname : device.Nickname,
                });
            }

            //As the WinForms UI did: back to the players step, the profile no longer auto-advances.
            if (CurrentStep.Kind != SetupStepKind.Players)
            {
                GameProfile.Ready = false;
                CurrentStep = StepId.Players;
            }

            RequestRefresh();
        }

        private void OnScreensChanged()
        {
            if (IsOpen && !SessionActive)
            {
                GameProfile.Instance.SyncScreens();
                CurrentStep = StepId.Players;
                RequestRefresh();
            }
        }

        /// <summary>
        /// GameProfile.Reset rebuilt the options from the raw handler values ("" for lists). Resets that don't go through
        /// a command (monitor change, profile matching errors) get the defaults merge here; it is idempotent for the others.
        /// </summary>
        private void OnProfileReset()
        {
            Post(() =>
            {
                if (IsOpen && !SessionActive && GameProfile.Game != null)
                {
                    OptionsCodec.Merge(GameProfile.Game.Options, GameProfile.Instance.Options);
                    RequestRefresh();
                }
            });
        }

        private void OnCanPlayChanged(bool canProceed, bool autoProceed)
        {
            if (canProceed && autoProceed)
            {
                Post(() => OnProfileMatched(false));
            }
        }

        private void OnAutoPlayTrigger()
        {
            Post(() => OnProfileMatched(true));
        }

        // ---------------------------------------------------------------- notices

        private void OnNotify(GameProfile.NoticeKind kind, string title, string text, int durationMs)
        {
            List<string>? capture = capturedErrors;

            if (kind == GameProfile.NoticeKind.Error && capture != null)
            {
                capture.Add(text);
                return;
            }

            //Info messages reach session.notice through the on-screen display (WPF_OSD.MessageShown).
            nativeNotify(kind, title, text, durationMs);
        }

        /// <summary>Runs <paramref name="action"/> with the profile error boxes captured instead of shown.</summary>
        public T CaptureErrors<T>(Func<T> action, out List<string> errors)
        {
            List<string> capture = new List<string>();
            capturedErrors = capture;

            try
            {
                return action();
            }
            finally
            {
                capturedErrors = null;
                errors = capture;
            }
        }
    }
}
