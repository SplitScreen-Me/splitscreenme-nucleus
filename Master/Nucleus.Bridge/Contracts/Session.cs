using System;
using System.Collections.Generic;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>The play session: launch, live state, stop.</summary>
    [BridgeService("session")]
    public interface ISessionService
    {
        /// <summary>Current session state.</summary>
        SessionSnapshot Get();

        /// <summary>Starts the session with the current setup. Requires <see cref="SetupFlow.CanPlay"/>; throws <c>conflict</c> otherwise or when a session is running.</summary>
        SessionSnapshot Play();

        /// <summary>Stops the session and closes every instance. Throws <c>conflict</c> while inputs are locked (unlock them first with the hotkey).</summary>
        SessionSnapshot Stop();

        /// <summary>Runs a session action, the same as its hotkey. Throws <c>conflict</c> when no session is running or the action doesn't apply (see <see cref="SessionSnapshot.Actions"/>).</summary>
        SessionSnapshot RunAction(SessionActionRequest request);

        /// <summary>Log lines of the current or last session.</summary>
        List<SessionLogEntry> GetLog();
    }

    /// <summary>An action available during a session, also bound to a hotkey.</summary>
    public enum SessionAction
    {
        /// <summary>Keeps the game windows above other windows, or stops doing so.</summary>
        ToggleTopmost,
        /// <summary>Gives focus back to the game windows.</summary>
        SetFocus,
        /// <summary>Puts every game window back in its area.</summary>
        ResetWindows,
        /// <summary>Cutscenes mode: shows one window full size, or goes back to the split.</summary>
        CutscenesMode,
        /// <summary>Switches to the next layout.</summary>
        SwitchLayout,
        /// <summary>Shows or hides the shortcuts reminder.</summary>
        ToggleShortcutsReminder,
        /// <summary>Brings the next window to the front in the merged window.</summary>
        SwitchMergerForeground,
    }

    /// <summary>An action and how to trigger it.</summary>
    public sealed class SessionActionInfo
    {
        /// <summary>Action.</summary>
        public SessionAction Action { get; set; }

        /// <summary>Label.</summary>
        public string Label { get; set; } = "";

        /// <summary>Keyboard hotkey, for example "Ctrl+T".</summary>
        public string? Hotkey { get; set; }

        /// <summary>Whether it applies to this session.</summary>
        public bool Available { get; set; }
    }

    /// <summary>Parameters of <see cref="ISessionService.RunAction"/>.</summary>
    public sealed class SessionActionRequest
    {
        /// <summary>Action to run.</summary>
        public SessionAction Action { get; set; }
    }

    /// <summary>Severity of a session log line.</summary>
    public enum SessionLogLevel
    {
        /// <summary>Progress.</summary>
        Info,
        /// <summary>A step succeeded.</summary>
        Success,
        /// <summary>Something unexpected.</summary>
        Warning,
        /// <summary>A failure.</summary>
        Error,
    }

    /// <summary>One line of the session log.</summary>
    public sealed class SessionLogEntry
    {
        /// <summary>Sequence number, increasing within the app's lifetime.</summary>
        public long Seq { get; set; }

        /// <summary>When (UTC).</summary>
        public DateTime At { get; set; }

        /// <summary>Severity.</summary>
        public SessionLogLevel Level { get; set; }

        /// <summary>Player the line is about, when it is about one.</summary>
        public int? PlayerNumber { get; set; }

        /// <summary>Text.</summary>
        public string Text { get; set; } = "";
    }

    /// <summary>New session log lines.</summary>
    [BridgeEvent("session.log")]
    public sealed class SessionLogAppended
    {
        /// <summary>New lines, oldest first.</summary>
        public List<SessionLogEntry> Entries { get; set; } = new List<SessionLogEntry>();
    }

    /// <summary>Lifecycle of the session.</summary>
    public enum SessionState
    {
        /// <summary>No game selected.</summary>
        Idle,
        /// <summary>A setup is open, nothing running.</summary>
        Setup,
        /// <summary>Instances are being started and placed.</summary>
        Launching,
        /// <summary>Every instance is running.</summary>
        Running,
        /// <summary>Instances are being closed and the setup restored.</summary>
        Ending,
    }

    /// <summary>Progress of one game instance.</summary>
    public enum InstanceStage
    {
        /// <summary>Not started yet.</summary>
        Waiting,
        /// <summary>Process started, window not ready.</summary>
        Starting,
        /// <summary>Window found and placed, input bound.</summary>
        Ready,
        /// <summary>Process exited.</summary>
        Exited,
    }

    /// <summary>One game instance in the session.</summary>
    public sealed class SessionInstance
    {
        /// <summary>Player number, 1-based.</summary>
        public int PlayerNumber { get; set; }

        /// <summary>Nickname, when known.</summary>
        public string? Nickname { get; set; }

        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>Area of the game window.</summary>
        public Rect Bounds { get; set; } = new Rect();

        /// <summary>Kinds of the devices driving this instance.</summary>
        public List<DeviceKind> Devices { get; set; } = new List<DeviceKind>();

        /// <summary>Progress.</summary>
        public InstanceStage Stage { get; set; }

        /// <summary>Process id, once started.</summary>
        public int? ProcessId { get; set; }
    }

    /// <summary>Everything the session screen shows.</summary>
    public sealed class SessionSnapshot
    {
        /// <summary>Lifecycle state.</summary>
        public SessionState State { get; set; }

        /// <summary>Game of the open setup or running session.</summary>
        public string? GameId { get; set; }

        /// <summary>Display name of that game.</summary>
        public string? GameName { get; set; }

        /// <summary>When the session started (UTC).</summary>
        public DateTime? StartedAt { get; set; }

        /// <summary>Instances, in player order. Empty outside a session.</summary>
        public List<SessionInstance> Instances { get; set; } = new List<SessionInstance>();

        /// <summary>Whether inputs are locked to the game windows (the stop button is refused until they are unlocked).</summary>
        public bool InputsLocked { get; set; }

        /// <summary>Last error, for example why the launch failed.</summary>
        public string? Error { get; set; }

        /// <summary>Actions and their hotkeys.</summary>
        public List<SessionActionInfo> Actions { get; set; } = new List<SessionActionInfo>();

        /// <summary>Hotkey that stops the session, for example "Ctrl+E".</summary>
        public string? StopHotkey { get; set; }

        /// <summary>Key that locks and unlocks inputs, for example "End".</summary>
        public string? LockInputsKey { get; set; }
    }

    /// <summary>The session state changed.</summary>
    [BridgeEvent("session.changed")]
    public sealed class SessionChanged
    {
        /// <summary>New snapshot.</summary>
        public SessionSnapshot Snapshot { get; set; } = new SessionSnapshot();
    }

    /// <summary>A short message the app shows on screen during a session (mirror of the on-screen display).</summary>
    [BridgeEvent("session.notice")]
    public sealed class SessionNotice
    {
        /// <summary>Message text.</summary>
        public string Text { get; set; } = "";

        /// <summary>How long it is shown.</summary>
        public int DurationMs { get; set; }
    }
}
