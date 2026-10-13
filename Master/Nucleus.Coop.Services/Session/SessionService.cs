using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Notices;
using Nucleus.Coop.Services.Setup;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Localization;
using Nucleus.Gaming.Tools.GlobalWindowMethods;

namespace Nucleus.Coop.Services.Session
{
    /// <summary>
    /// The play session on the bridge. Replaces Core_Interface.PlayClicked / I_GameHandlerEndFunc / Handler_Ended:
    /// launch through GameProfile.InitializeHandlerStartup (native launch dialogs stay), stop on a worker thread (End
    /// sleeps about 9 s), and after every end (stop, hotkey, games closed, launch error) a full reset of the setup,
    /// because the launch changed the setup objects. UI thread, except the documented callbacks.
    /// </summary>
    internal sealed class SessionService : ISessionService
    {
        private readonly SetupSession setup;
        private readonly IBridgeEvents events;
        private readonly IHostShell shell;
        private readonly NoticeBoard? notices;
        private readonly SessionLog log;
        private readonly Timer pollTimer;

        private SessionState? active;//Launching, Running or Ending while a session exists
        private IGameHandler? handler;
        private DateTime? startedAt;
        private string? lastError;
        private string? lastErrorDetails;
        private string? lastSignature;
        private int endedOnce;
        private int pollQueued;

        //What the summary of the session needs from the moment it started.
        private string sessionGameId = "";
        private string sessionGameName = "";
        private List<SessionSummaryPlayer> sessionPlayers = new List<SessionSummaryPlayer>();
        private bool reachedRunning;
        private SessionSummary? lastSession;

        public SessionService(SetupSession setup, IBridgeEvents events, IHostShell shell, NoticeBoard? notices = null)
        {
            this.setup = setup;
            this.events = events;
            this.shell = shell;
            this.notices = notices;
            log = new SessionLog(events);
            pollTimer = new Timer(_ => OnPoll(), null, Timeout.Infinite, Timeout.Infinite);

            setup.OpenChanged += () => EmitIfChanged(force: true);
            setup.AutoPlayRequested = AutoPlay;
            //Every message of the on-screen display (hotkeys, launch, profile code) during a session. Any thread.
            WPF_OSD.MessageShown += (duration, text) =>
            {
                if (active != null && !string.IsNullOrEmpty(text))
                {
                    events.Emit(new SessionNotice { Text = text, DurationMs = duration });
                }
            };
        }

        public SessionSnapshot Get() => Build();

        public List<SessionLogEntry> GetLog() => log.Entries();

        // ---------------------------------------------------------------- play

        public SessionSnapshot Play()
        {
            if (active != null)
            {
                throw SetupSession.Conflict("sessionRunning", I18n.T("host.session.errors.alreadyRunning"));
            }

            setup.EnsureOpen();

            SetupSnapshot snapshot = setup.CommitCommand();//pump once more: identities are fresh

            if (!snapshot.Flow.CanPlay)
            {
                throw SetupSession.Conflict("cannotPlay", I18n.T("host.session.errors.notReady"));
            }

            lastError = null;
            lastErrorDetails = null;
            lastSession = null;
            sessionGameId = snapshot.GameId;
            sessionGameName = snapshot.GameName;
            sessionPlayers = SessionSummaryBuilder.Players(snapshot);
            reachedRunning = false;
            notices?.TakeLaunchFailure();//a stale report of an earlier launch
            log.Clear();
            log.Start();
            setup.SessionActive = true;
            active = SessionState.Launching;
            startedAt = DateTime.UtcNow;
            Interlocked.Exchange(ref endedOnce, 0);

            try
            {
                //Disposes the device timers, reloads the handler script, clones the setup (CleanClone) and starts the
                //handler's threads; Ended is subscribed before the handler starts (R10).
                GameProfile.InitializeHandlerStartup(OnHandlerEnded);
                handler = GameProfile.I_GameHandler;
            }
            catch (Exception e)
            {
                ServiceLog.Error("session: launch failed", e);
                lastError = e.Message;
                Finish();
                throw new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.session.errors.startFailed", new { error = e.Message }), new { reason = "launchFailed" });
            }

            shell.MinimizeWindow();
            pollTimer.Change(500, 500);
            EmitIfChanged(force: true);

            return Build();
        }

        private void AutoPlay()
        {
            try
            {
                Play();
            }
            catch (BridgeException e)
            {
                ServiceLog.Info("session: profile auto play not started: " + e.Message);
            }
        }

        // ---------------------------------------------------------------- stop

        public SessionSnapshot Stop()
        {
            if (active == null || handler == null)
            {
                throw SetupSession.Conflict("noSession", I18n.T("host.session.errors.notRunning"));
            }

            if (active == SessionState.Ending)
            {
                return Build();
            }

            //End returns at once while inputs are locked (and the session goes on).
            if (LockInputRuntime.IsLocked)
            {
                throw SetupSession.Conflict("inputsLocked", I18n.T("host.session.errors.inputsLocked", new { key = App_Hotkeys.LockInputs }));
            }

            IGameHandler stopping = handler;
            SessionState previous = active.Value;
            active = SessionState.Ending;
            EmitIfChanged(force: true);

            Task.Run(() =>
            {
                try
                {
                    stopping.End(true);
                }
                catch (Exception e)
                {
                    ServiceLog.Error("session: End failed", e);
                }

                setup.Post(() =>
                {
                    //End returned early (inputs locked in the meantime): the session goes on.
                    if (!stopping.HasEnded && active == SessionState.Ending && handler == stopping)
                    {
                        active = previous;
                        lastError = I18n.T("host.session.errors.stopLocked");
                        EmitIfChanged(force: true);
                    }
                });
            });

            return Build();
        }

        /// <summary>Handler Ended event: raised on the thread that ended the session (worker, hotkey, update tick).</summary>
        private void OnHandlerEnded()
        {
            if (Interlocked.Exchange(ref endedOnce, 1) == 0)
            {
                setup.Post(Finish);
            }
        }

        /// <summary>UI thread: the session is over (or failed to start). Always resets the setup.</summary>
        private void Finish()
        {
            pollTimer.Change(Timeout.Infinite, Timeout.Infinite);

            if (handler is GenericGameHandler generic && !string.IsNullOrEmpty(generic.error))
            {
                lastError = generic.error;
            }

            //The crash report of a failed launch (LogManager.LogExceptionFile), shown with the error.
            string? report = notices?.TakeLaunchFailure();
            if (report != null)
            {
                lastErrorDetails = report;
                lastError ??= I18n.T("host.session.errors.launchFailed");
            }

            lastSession = SessionSummaryBuilder.Build(
                sessionGameId,
                sessionGameName,
                startedAt ?? DateTime.UtcNow,
                DateTime.UtcNow,
                reachedRunning,
                sessionPlayers,
                lastError,
                log.Entries());

            GameProfile.I_GameHandler = null;
            handler = null;
            active = null;
            setup.SessionActive = false;
            log.Stop();

            try
            {
                setup.ResetAfterSession();
            }
            catch (Exception e)
            {
                ServiceLog.Error("session: setup reset failed", e);
            }

            shell.RestoreWindow();
            setup.ProfilesChanged();//the launch may have saved a profile
            EmitIfChanged(force: true);
        }

        private void OnPoll()
        {
            if (Interlocked.Exchange(ref pollQueued, 1) == 0)
            {
                setup.Post(() =>
                {
                    Interlocked.Exchange(ref pollQueued, 0);

                    if (active == null)
                    {
                        return;
                    }

                    //All windows set up (SaveGameProfile ran at the end of Play): the session runs.
                    if (active == SessionState.Launching && GameProfile.Saved)
                    {
                        active = SessionState.Running;
                        reachedRunning = true;
                    }

                    //The handler ended without raising Ended (it failed before starting its threads).
                    if (handler != null && handler.HasEnded && Volatile.Read(ref endedOnce) == 0)
                    {
                        OnHandlerEnded();
                    }

                    EmitIfChanged(force: false);
                });
            }
        }

        // ---------------------------------------------------------------- actions

        public SessionSnapshot RunAction(SessionActionRequest request)
        {
            if (active == null || GenericGameHandler.Instance == null || GenericGameHandler.Instance.HasEnded)
            {
                throw SetupSession.Conflict("noSession", I18n.T("host.session.errors.notRunning"));
            }

            SessionActionInfo info = Actions().First(a => a.Action == request.Action);

            if (!info.Available)
            {
                throw SetupSession.Conflict("notAvailable", I18n.T("host.session.errors.actionUnavailable", new { action = info.Label }));
            }

            //As HotkeyListener: only the cutscenes mode passes through locked inputs.
            if (LockInputRuntime.IsLocked && request.Action != SessionAction.CutscenesMode)
            {
                throw SetupSession.Conflict("inputsLocked", I18n.T("host.session.errors.inputsLocked", new { key = App_Hotkeys.LockInputs }));
            }

            Action action = HotkeyFunction(request.Action);

            //The hotkeys run on the hotkey listener's STA thread: these functions wait and touch other windows, keep them off the UI thread.
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    ServiceLog.Warn($"session: action {request.Action} failed", e);
                }
            })
            {
                IsBackground = true,
                Name = "Nucleus session action",
            };

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return Build();
        }

        /// <summary>The function each hotkey calls (HotkeyListener.WndProc).</summary>
        private static Action HotkeyFunction(SessionAction action)
        {
            switch (action)
            {
                case SessionAction.ToggleTopmost:
                    return GlobalWindowMethods.ShowHideWindows;
                case SessionAction.SetFocus:
                    return () =>
                    {
                        GlobalWindowMethods.ChangeForegroundWindow();
                        Globals.MainOSD?.Show(2000, I18n.T("engine.osd.windowsUnfocused"));
                    };
                case SessionAction.ResetWindows:
                    return () => GlobalWindowMethods.ResetingWindows = true;
                case SessionAction.CutscenesMode:
                    return GlobalWindowMethods.ToggleCutScenesMode;
                case SessionAction.SwitchLayout:
                    return GlobalWindowMethods.SwitchLayout;
                case SessionAction.ToggleShortcutsReminder:
                    return () =>
                    {
                        foreach (var reminder in GenericGameHandler.Instance?.shortcutsReminders.ToList() ?? new List<Nucleus.Coop.Forms.ShortcutsReminder>())
                        {
                            reminder.Toggle(7);
                        }
                    };
                default:
                    return () => WindowsMerger.Instance?.SwitchChildFocus();
            }
        }

        private static string? Hotkey(string[]? keys)
        {
            if (keys == null || keys.Length < 2 || string.IsNullOrEmpty(keys[1]))
            {
                return null;
            }

            return string.IsNullOrEmpty(keys[0]) || keys[0] == "None" ? keys[1] : $"{keys[0]}+{keys[1]}";
        }

        private List<SessionActionInfo> Actions()
        {
            GenericGameHandler? instance = GenericGameHandler.Instance;
            bool running = active != null && active != SessionState.Ending && instance != null && !instance.HasEnded;
            bool ready = running && GameProfile.Saved;//layout and cutscenes need the windows set up

            SessionActionInfo Info(SessionAction action, string label, string[]? keys, bool available) =>
                new SessionActionInfo { Action = action, Label = label, Hotkey = Hotkey(keys), Available = available };

            return new List<SessionActionInfo>
            {
                Info(SessionAction.ToggleTopmost, I18n.T("host.session.actions.toggleTopmost"), App_Hotkeys.TopMost, running),
                Info(SessionAction.SetFocus, I18n.T("host.session.actions.setFocus"), App_Hotkeys.SetFocus, running),
                Info(SessionAction.ResetWindows, I18n.T("host.session.actions.resetWindows"), App_Hotkeys.ResetWindows, running),
                Info(SessionAction.CutscenesMode, I18n.T("host.session.actions.cutscenesMode"), App_Hotkeys.CutscenesMode, ready),
                Info(SessionAction.SwitchLayout, I18n.T("host.session.actions.switchLayout"), App_Hotkeys.SwitchLayout, ready),
                Info(SessionAction.ToggleShortcutsReminder, I18n.T("host.session.actions.shortcutsReminder"), App_Hotkeys.ShortcutsReminder, running && instance!.shortcutsReminders.Count > 0),
                Info(SessionAction.SwitchMergerForeground, I18n.T("host.session.actions.switchMergerForeground"), App_Hotkeys.SwitchMergerForeGroundChild, running && WindowsMerger.Instance != null),
            };
        }

        // ---------------------------------------------------------------- snapshot

        private SessionSnapshot Build()
        {
            UserGameInfo? game = setup.Game ?? (active != null ? GameProfile.GameInfo : null);

            SessionSnapshot snapshot = new SessionSnapshot
            {
                State = active ?? (setup.IsOpen ? SessionState.Setup : SessionState.Idle),
                GameId = game?.GameGuid,
                GameName = game?.Game?.GameName,
                StartedAt = active != null ? startedAt : null,
                InputsLocked = LockInputRuntime.IsLocked,
                Error = lastError,
                ErrorDetails = lastError != null ? lastErrorDetails : null,
                Actions = Actions(),
                StopHotkey = Hotkey(App_Hotkeys.StopSession),
                LockInputsKey = string.IsNullOrEmpty(App_Hotkeys.LockInputs) ? "End" : App_Hotkeys.LockInputs,
                LastSession = lastSession,
            };

            if (active != null && handler is GenericGameHandler generic)
            {
                snapshot.Instances = Instances(generic);
            }

            return snapshot;
        }

        /// <summary>
        /// The launch players (the cloned profile's list), with a stage derived from their runtime fields: Waiting before
        /// a process exists, Starting until the window is placed (ProcessData.Setted), Ready after, Exited when the process
        /// ended. Approximate: the launch doesn't report its progress per instance.
        /// </summary>
        private static List<SessionInstance> Instances(GenericGameHandler generic)
        {
            List<SessionInstance> instances = new List<SessionInstance>();
            List<PlayerInfo>? players = generic.profile?.DevicesList;

            for (int i = 0; players != null && i < players.Count; i++)
            {
                PlayerInfo player;

                try
                {
                    player = players[i];
                }
                catch (ArgumentOutOfRangeException)
                {
                    break;//the launch thread changed the list
                }

                SessionInstance instance = new SessionInstance
                {
                    PlayerNumber = i + 1,
                    Nickname = string.IsNullOrEmpty(player.Nickname) ? null : player.Nickname,
                    ScreenIndex = player.ScreenIndex,
                    Bounds = SnapshotMath.ToRect(player.MonitorBounds),
                    Stage = StageOf(player, out int? processId),
                    ProcessId = processId,
                };

                instance.Devices.Add(DeviceIds.KindOf(player.InputType));//KBM after CleanClone: KeyboardMouse

                foreach (PlayerInfo guest in player.InstanceGuests.ToList())
                {
                    instance.Devices.Add(DeviceIds.KindOf(guest.InputType));
                }

                instances.Add(instance);
            }

            return instances;
        }

        private static InstanceStage StageOf(PlayerInfo player, out int? processId)
        {
            processId = null;
            ProcessData? data = player.ProcessData;
            Process? process = data?.Process;

            try
            {
                if (process != null)
                {
                    processId = process.Id;

                    if (process.HasExited)
                    {
                        return InstanceStage.Exited;
                    }
                }
                else if (player.ProcessID != 0)
                {
                    processId = player.ProcessID;
                }
            }
            catch (Exception)
            {
                return InstanceStage.Exited;
            }

            if (data == null && processId == null)
            {
                return InstanceStage.Waiting;
            }

            return data != null && (data.Setted || data.Finished) ? InstanceStage.Ready : InstanceStage.Starting;
        }

        private void EmitIfChanged(bool force)
        {
            SessionSnapshot snapshot = Build();
            string signature = JsonConvert.SerializeObject(snapshot);

            if (force || signature != lastSignature)
            {
                lastSignature = signature;
                events.Emit(new SessionChanged { Snapshot = snapshot });
            }
        }
    }
}
