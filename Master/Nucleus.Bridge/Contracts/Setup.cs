using System.Collections.Generic;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// The game setup (lobby): devices, screens and layouts, player placement, handler options and custom steps.
    /// The host owns the state. Every command returns the new snapshot and also emits <see cref="SetupChanged"/>.
    /// Refused commands throw a <c>conflict</c> error whose data is <c>{ reason }</c> (for example <c>slotOccupied</c>, <c>profileLocked</c>, <c>deviceNotRevealed</c>).
    /// </summary>
    [BridgeService("setup")]
    public interface ISetupService
    {
        /// <summary>Opens the setup of a game: starts device detection, builds the screens, applies the handler defaults. Closes any other open setup first.</summary>
        SetupSnapshot Open(GameRequest request);

        /// <summary>Closes the setup and stops device detection.</summary>
        void Close();

        /// <summary>The current snapshot, or null when no setup is open.</summary>
        SetupSnapshot? Get();

        /// <summary>Goes to the next step (options, then custom steps).</summary>
        SetupSnapshot Next();

        /// <summary>Goes back one step.</summary>
        SetupSnapshot Back();

        /// <summary>Sets the layout of a monitor. Players on that monitor are unassigned.</summary>
        SetupSnapshot SetLayout(SetLayoutRequest request);

        /// <summary>Press-to-join: places a revealed device's player in the first free slot (monitors in order), switching a monitor to a layout with more slots when every slot is taken. Throws <c>conflict</c> (reason <c>full</c>) when no slot can be made.</summary>
        SetupSnapshot Join(DeviceRequest request);

        /// <summary>Places a device's player in a layout slot. A raw keyboard and a raw mouse dropped on the same slot become one player. When another player holds the slot, the two swap places.</summary>
        SetupSnapshot AssignToSlot(AssignToSlotRequest request);

        /// <summary>Places a device's player at a point of a monitor (used by the manual layout, where the slot grid is fine-grained).</summary>
        SetupSnapshot AssignAt(AssignAtRequest request);

        /// <summary>Removes a device from its player and sends it back to the tray.</summary>
        SetupSnapshot Unassign(DeviceRequest request);

        /// <summary>Adds a gamepad as a guest of another gamepad's instance (handlers with several players per instance).</summary>
        SetupSnapshot AddGuest(AddGuestRequest request);

        /// <summary>Removes a guest gamepad from its host instance.</summary>
        SetupSnapshot RemoveGuest(DeviceRequest request);

        /// <summary>Expands a player to the free neighbouring slots, or shrinks it back.</summary>
        SetupSnapshot ToggleExpand(PlayerRequest request);

        /// <summary>Moves one edge of a player's area by a number of grid steps (layouts with 4 slots or more).</summary>
        SetupSnapshot Resize(ResizePlayerRequest request);

        /// <summary>Places every revealed, unassigned device in the free slots, in order.</summary>
        SetupSnapshot AutoFill();

        /// <summary>Unassigns every player and unloads the profile.</summary>
        SetupSnapshot ClearAll();

        /// <summary>Picks a value of a choice option.</summary>
        SetupSnapshot SetOptionChoice(SetOptionChoiceRequest request);

        /// <summary>Sets the value of a text option.</summary>
        SetupSnapshot SetOptionText(SetOptionTextRequest request);

        /// <summary>Picks an item in a custom step.</summary>
        SetupSnapshot SelectStepItem(SelectStepItemRequest request);

        /// <summary>Sets the nickname and Steam ID used by a player (saved in the global player slots, as the old Players settings did).</summary>
        SetupSnapshot SetPlayerIdentity(SetPlayerIdentityRequest request);

        /// <summary>Routes a player's game audio to an output. Turns per-player audio on.</summary>
        SetupSnapshot SetPlayerAudio(SetPlayerAudioRequest request);

        /// <summary>Runs the fix offered by a pre-launch check (for example locate the game, update the handler, restart as administrator).</summary>
        System.Threading.Tasks.Task<SetupSnapshot> RunCheckAction(CheckActionRequest request);

        /// <summary>Uses controller API indexes instead of button presses to identify gamepads (saved per game).</summary>
        SetupSnapshot SetUseApiIndex(SetEnabledRequest request);

        /// <summary>Saves the setup as a new profile when the session starts (saved per game).</summary>
        SetupSnapshot SetSaveProfileOnLaunch(SetEnabledRequest request);

        /// <summary>Rescans gamepads, keyboards and mice.</summary>
        SetupSnapshot RefreshDevices();

        /// <summary>Makes a gamepad vibrate briefly so the user can find it.</summary>
        void Rumble(DeviceRequest request);
    }

    /// <summary>Everything the setup screen shows.</summary>
    public sealed class SetupSnapshot
    {
        /// <summary>Game being set up.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Display name of the game.</summary>
        public string GameName { get; set; } = "";

        /// <summary>Increases on every change. The UI ignores a snapshot older than the one it has.</summary>
        public long Revision { get; set; }

        /// <summary>Steps and navigation state.</summary>
        public SetupFlow Flow { get; set; } = new SetupFlow();

        /// <summary>What the user should do next, when there is something to say.</summary>
        public SetupHint? Hint { get; set; }

        /// <summary>Union of all monitors, to scale the drawing.</summary>
        public Rect TotalBounds { get; set; } = new Rect();

        /// <summary>Monitors with their layouts and slots.</summary>
        public List<SetupScreen> Screens { get; set; } = new List<SetupScreen>();

        /// <summary>Detected input devices, assigned or not.</summary>
        public List<SetupDevice> Devices { get; set; } = new List<SetupDevice>();

        /// <summary>Players in launch order (player 1 first).</summary>
        public List<SetupPlayer> Players { get; set; } = new List<SetupPlayer>();

        /// <summary>Handler options shown on the options step (options driven by a custom step are not listed here).</summary>
        public List<SetupOption> Options { get; set; } = new List<SetupOption>();

        /// <summary>Custom steps declared by the handler.</summary>
        public List<SetupCustomStep> CustomSteps { get; set; } = new List<SetupCustomStep>();

        /// <summary>State of the loaded profile, if any.</summary>
        public ProfileMatch Profile { get; set; } = new ProfileMatch();

        /// <summary>What the handler supports.</summary>
        public SetupCapabilities Capabilities { get; set; } = new SetupCapabilities();

        /// <summary>The per-game "identify gamepads by API index" setting. Some handlers ignore it (device reordering, single device file), so it may not be in effect.</summary>
        public bool UseApiIndex { get; set; }

        /// <summary>Whether a profile is saved when the session starts.</summary>
        public bool SaveProfileOnLaunch { get; set; }

        /// <summary>"Before you launch" checks, blocking ones first.</summary>
        public List<SetupCheck> Checks { get; set; } = new List<SetupCheck>();

        /// <summary>Audio outputs players can be routed to.</summary>
        public List<SettingChoice> AudioOutputs { get; set; } = new List<SettingChoice>();

        /// <summary>Whether per-player audio routing is on.</summary>
        public bool CustomAudio { get; set; }
    }

    /// <summary>Outcome of a pre-launch check.</summary>
    public enum CheckState
    {
        /// <summary>Fine.</summary>
        Ok,
        /// <summary>Information only.</summary>
        Info,
        /// <summary>May cause problems; launch is still possible.</summary>
        Warning,
        /// <summary>Launch is impossible until fixed.</summary>
        Blocking,
        /// <summary>A fix is running.</summary>
        Busy,
    }

    /// <summary>A pre-launch check, for example "Game found", "Handler up to date", "Administrator rights".</summary>
    public sealed class SetupCheck
    {
        /// <summary>Stable id, for example <c>gameFound</c>, <c>handlerUpToDate</c>, <c>admin</c>, <c>oneDrive</c>, <c>devices</c>.</summary>
        public string Id { get; set; } = "";

        /// <summary>Outcome.</summary>
        public CheckState State { get; set; }

        /// <summary>Short title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Detail, for example a path or a version.</summary>
        public string? Detail { get; set; }

        /// <summary>Label of the fix button, when a fix exists (<see cref="ISetupService.RunCheckAction"/>).</summary>
        public string? ActionLabel { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.RunCheckAction"/>.</summary>
    public sealed class CheckActionRequest
    {
        /// <summary>Check id.</summary>
        public string CheckId { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="ISetupService.SetPlayerIdentity"/>.</summary>
    public sealed class SetPlayerIdentityRequest
    {
        /// <summary>Player id.</summary>
        public string PlayerId { get; set; } = "";

        /// <summary>Nickname. Null keeps the current one.</summary>
        public string? Nickname { get; set; }

        /// <summary>Steam ID. Null keeps the current one.</summary>
        public string? SteamId { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.SetPlayerAudio"/>.</summary>
    public sealed class SetPlayerAudioRequest
    {
        /// <summary>Player id.</summary>
        public string PlayerId { get; set; } = "";

        /// <summary>Output id from <see cref="SetupSnapshot.AudioOutputs"/>, or "Default".</summary>
        public string AudioDeviceId { get; set; } = "Default";
    }

    /// <summary>Kind of a setup step.</summary>
    public enum SetupStepKind
    {
        /// <summary>Players, devices and screens.</summary>
        Players,
        /// <summary>Handler options.</summary>
        Options,
        /// <summary>A handler-defined step (pick one item).</summary>
        Custom,
    }

    /// <summary>One step of the setup.</summary>
    public sealed class SetupStep
    {
        /// <summary>Kind of step.</summary>
        public SetupStepKind Kind { get; set; }

        /// <summary>For custom steps, index in <see cref="SetupSnapshot.CustomSteps"/>.</summary>
        public int? CustomStepIndex { get; set; }

        /// <summary>Title to show.</summary>
        public string Title { get; set; } = "";
    }

    /// <summary>Steps that will be shown and navigation state. Replaces the old Next/Previous/Play buttons logic.</summary>
    public sealed class SetupFlow
    {
        /// <summary>Only the steps that will actually be shown.</summary>
        public List<SetupStep> Steps { get; set; } = new List<SetupStep>();

        /// <summary>Index of the current step in <see cref="Steps"/>.</summary>
        public int Current { get; set; }

        /// <summary>Whether <see cref="ISetupService.Back"/> is allowed.</summary>
        public bool CanGoBack { get; set; }

        /// <summary>Whether <see cref="ISetupService.Next"/> is allowed.</summary>
        public bool CanGoNext { get; set; }

        /// <summary>Whether the session can start now.</summary>
        public bool CanPlay { get; set; }
    }

    /// <summary>A hint for the user. The UI owns the wording; <see cref="Text"/> is the host's English fallback.</summary>
    public sealed class SetupHint
    {
        /// <summary>Stable code, for example <c>pressButtonToIdentify</c>, <c>dragPlayersToScreens</c>, <c>profileWaitingForPlayer</c>.</summary>
        public string Code { get; set; } = "";

        /// <summary>Values used by the message, for example a player number.</summary>
        public List<string> Args { get; set; } = new List<string>();

        /// <summary>English message.</summary>
        public string Text { get; set; } = "";
    }

    /// <summary>How a monitor is split.</summary>
    public enum ScreenLayout
    {
        /// <summary>One player on the whole monitor.</summary>
        FullScreen,
        /// <summary>Two players, top and bottom.</summary>
        DualHorizontal,
        /// <summary>Two players, side by side.</summary>
        DualVertical,
        /// <summary>2 × 2.</summary>
        FourPlayers,
        /// <summary>2 rows × 3 columns.</summary>
        SixPlayers,
        /// <summary>2 rows × 4 columns.</summary>
        EightPlayers,
        /// <summary>4 × 4.</summary>
        SixteenPlayers,
        /// <summary>Rows and columns from the custom layout settings.</summary>
        Custom,
        /// <summary>Free placement on a fine grid.</summary>
        Manual,
    }

    /// <summary>A monitor.</summary>
    public sealed class SetupScreen
    {
        /// <summary>Index used by commands.</summary>
        public int Index { get; set; }

        /// <summary>Windows display number.</summary>
        public int DisplayIndex { get; set; }

        /// <summary>Monitor rectangle.</summary>
        public Rect Bounds { get; set; } = new Rect();

        /// <summary>Whether it is the primary monitor.</summary>
        public bool IsPrimary { get; set; }

        /// <summary>Current layout.</summary>
        public ScreenLayout Layout { get; set; }

        /// <summary>Rows of the layout grid (1 for full screen).</summary>
        public int Rows { get; set; }

        /// <summary>Columns of the layout grid (1 for full screen).</summary>
        public int Columns { get; set; }

        /// <summary>Slots, row by row. Empty for the manual layout (use <see cref="ISetupService.AssignAt"/>).</summary>
        public List<SetupSlot> Slots { get; set; } = new List<SetupSlot>();

        /// <summary>True when a loaded profile fixes the layout.</summary>
        public bool LayoutLocked { get; set; }
    }

    /// <summary>A place for one player on a monitor.</summary>
    public sealed class SetupSlot
    {
        /// <summary>Index used by commands.</summary>
        public int Index { get; set; }

        /// <summary>Rectangle of the slot.</summary>
        public Rect Bounds { get; set; } = new Rect();

        /// <summary>Player in this slot, if any.</summary>
        public string? PlayerId { get; set; }

        /// <summary>True when a loaded profile expects a player here.</summary>
        public bool Expected { get; set; }
    }

    /// <summary>Kind of input device.</summary>
    public enum DeviceKind
    {
        /// <summary>XInput gamepad.</summary>
        XInput,
        /// <summary>DirectInput gamepad.</summary>
        DInput,
        /// <summary>Gamepad read through SDL.</summary>
        Sdl,
        /// <summary>The shared keyboard and mouse, for handlers with a single keyboard player.</summary>
        Keyboard,
        /// <summary>One physical keyboard (handlers that support several keyboards and mice).</summary>
        RawKeyboard,
        /// <summary>One physical mouse (handlers that support several keyboards and mice).</summary>
        RawMouse,
        /// <summary>A raw keyboard and a raw mouse merged into one device (by a loaded profile, or expected by one).</summary>
        KeyboardMouse,
    }

    /// <summary>A detected input device.</summary>
    public sealed class SetupDevice
    {
        /// <summary>Stable id while the device stays plugged in, for example <c>dinput:{guid}</c>, <c>xinput:2</c>, <c>rawkb:{hid}</c>, <c>kb:single</c>.</summary>
        public string Id { get; set; } = "";

        /// <summary>Kind of device.</summary>
        public DeviceKind Kind { get; set; }

        /// <summary>Product name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Index given by the input API (XInput slot, DirectInput list position…).</summary>
        public int ApiIndex { get; set; }

        /// <summary>Whether the device can be placed. Keyboards and mice appear once used, gamepads once identified.</summary>
        public bool Revealed { get; set; }

        /// <summary>Whether the user must press a button on it before it can be placed.</summary>
        public bool NeedsButtonPress { get; set; }

        /// <summary>Whether it is a virtual device (no physical handle).</summary>
        public bool IsVirtual { get; set; }

        /// <summary>Player this device belongs to, if assigned.</summary>
        public string? PlayerId { get; set; }

        /// <summary>When the device is a guest, the player whose instance it joins.</summary>
        public string? GuestOfPlayerId { get; set; }
    }

    /// <summary>A player: one game instance, placed on a monitor area.</summary>
    public sealed class SetupPlayer
    {
        /// <summary>Stable id while the player exists (the id of its first device).</summary>
        public string Id { get; set; } = "";

        /// <summary>Player number, 1-based, in launch order. The UI colours players by number.</summary>
        public int Number { get; set; }

        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>Area of the game window.</summary>
        public Rect Bounds { get; set; } = new Rect();

        /// <summary>Devices driving this player: one gamepad, the shared keyboard, or a raw keyboard plus a raw mouse.</summary>
        public List<string> DeviceIds { get; set; } = new List<string>();

        /// <summary>Guest gamepads sharing this instance.</summary>
        public List<string> GuestDeviceIds { get; set; } = new List<string>();

        /// <summary>How many guests this instance accepts.</summary>
        public int MaxGuests { get; set; }

        /// <summary>Nickname used in game, when known.</summary>
        public string? Nickname { get; set; }

        /// <summary>Steam ID used in game, when known.</summary>
        public string? SteamId { get; set; }

        /// <summary>Audio output of this player's instance, when per-player audio is on.</summary>
        public string? AudioDeviceId { get; set; }

        /// <summary>Whether <see cref="ISetupService.ToggleExpand"/> is possible.</summary>
        public bool CanExpand { get; set; }

        /// <summary>Whether <see cref="ISetupService.Resize"/> is possible.</summary>
        public bool CanResize { get; set; }
    }

    /// <summary>Kind of handler option.</summary>
    public enum OptionKind
    {
        /// <summary>Pick one value in a list.</summary>
        Choice,
        /// <summary>Free text.</summary>
        Text,
        /// <summary>Declared in a way the app cannot edit; the handler default is used.</summary>
        Unsupported,
    }

    /// <summary>One value of a choice option or one item of a custom step.</summary>
    public sealed class OptionChoice
    {
        /// <summary>Index used by commands.</summary>
        public int Index { get; set; }

        /// <summary>Label.</summary>
        public string Label { get; set; } = "";

        /// <summary>Longer description (custom step items).</summary>
        public string? Details { get; set; }

        /// <summary>Picture provided by the handler (custom step items), as a URL the WebView can load.</summary>
        public string? ImageUrl { get; set; }
    }

    /// <summary>A handler option.</summary>
    public sealed class SetupOption
    {
        /// <summary>Key used by commands and by the handler.</summary>
        public string Key { get; set; } = "";

        /// <summary>Name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Description.</summary>
        public string Description { get; set; } = "";

        /// <summary>Kind of option.</summary>
        public OptionKind Kind { get; set; }

        /// <summary>Values (choice options).</summary>
        public List<OptionChoice> Choices { get; set; } = new List<OptionChoice>();

        /// <summary>Selected value (choice options).</summary>
        public int? SelectedIndex { get; set; }

        /// <summary>Current text (text options).</summary>
        public string? Text { get; set; }
    }

    /// <summary>A handler-defined step: pick one item.</summary>
    public sealed class SetupCustomStep
    {
        /// <summary>Index used by commands.</summary>
        public int Index { get; set; }

        /// <summary>Title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Option the selection is stored in.</summary>
        public string OptionKey { get; set; } = "";

        /// <summary>Whether the step must be completed. Optional steps are skipped.</summary>
        public bool Required { get; set; }

        /// <summary>Items to pick from.</summary>
        public List<OptionChoice> Items { get; set; } = new List<OptionChoice>();

        /// <summary>Selected item.</summary>
        public int? SelectedIndex { get; set; }

        /// <summary>True when the selection was restored from a profile.</summary>
        public bool AutoSelected { get; set; }
    }

    /// <summary>State of the loaded profile while devices are matched to its players.</summary>
    public sealed class ProfileMatch
    {
        /// <summary>Whether a profile is loaded.</summary>
        public bool Loaded { get; set; }

        /// <summary>Loaded profile id.</summary>
        public int? ProfileId { get; set; }

        /// <summary>Players already matched.</summary>
        public int Assigned { get; set; }

        /// <summary>Players in the profile.</summary>
        public int Total { get; set; }

        /// <summary>Number of the next player waiting for a device.</summary>
        public int? AwaitingPlayer { get; set; }

        /// <summary>Kind of device the next player expects.</summary>
        public DeviceKind? AwaitingKind { get; set; }

        /// <summary>Whether all players are matched.</summary>
        public bool Ready { get; set; }

        /// <summary>Whether the profile starts the session by itself once ready.</summary>
        public bool AutoPlay { get; set; }
    }

    /// <summary>What the handler supports.</summary>
    public sealed class SetupCapabilities
    {
        /// <summary>Maximum number of players declared by the handler.</summary>
        public int MaxPlayers { get; set; }

        /// <summary>Gamepads supported.</summary>
        public bool Gamepads { get; set; }

        /// <summary>One keyboard and mouse player supported.</summary>
        public bool Keyboard { get; set; }

        /// <summary>Several keyboards and mice supported.</summary>
        public bool MultipleKeyboardsAndMice { get; set; }

        /// <summary>Players per instance (1 unless the game has its own local split screen).</summary>
        public int PlayersPerInstance { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.SetLayout"/>.</summary>
    public sealed class SetLayoutRequest
    {
        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>New layout.</summary>
        public ScreenLayout Layout { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.AssignToSlot"/>.</summary>
    public sealed class AssignToSlotRequest
    {
        /// <summary>Device to place.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>Slot index.</summary>
        public int SlotIndex { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.AssignAt"/>.</summary>
    public sealed class AssignAtRequest
    {
        /// <summary>Device to place.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>Horizontal position, 0 to 1 across the monitor.</summary>
        public double X { get; set; }

        /// <summary>Vertical position, 0 to 1 down the monitor.</summary>
        public double Y { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.AddGuest"/>.</summary>
    public sealed class AddGuestRequest
    {
        /// <summary>Player whose instance the guest joins.</summary>
        public string PlayerId { get; set; } = "";

        /// <summary>Guest gamepad.</summary>
        public string DeviceId { get; set; } = "";
    }

    /// <summary>Edge of a player's area.</summary>
    public enum Edge
    {
        /// <summary>Left edge.</summary>
        Left,
        /// <summary>Right edge.</summary>
        Right,
        /// <summary>Top edge.</summary>
        Top,
        /// <summary>Bottom edge.</summary>
        Bottom,
    }

    /// <summary>Parameters of <see cref="ISetupService.Resize"/>.</summary>
    public sealed class ResizePlayerRequest
    {
        /// <summary>Player to resize.</summary>
        public string PlayerId { get; set; } = "";

        /// <summary>Edge to move.</summary>
        public Edge Edge { get; set; }

        /// <summary>Grid steps, positive to grow, negative to shrink.</summary>
        public int Steps { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.SetOptionChoice"/>.</summary>
    public sealed class SetOptionChoiceRequest
    {
        /// <summary>Option key.</summary>
        public string Key { get; set; } = "";

        /// <summary>Index in <see cref="SetupOption.Choices"/>.</summary>
        public int Index { get; set; }
    }

    /// <summary>Parameters of <see cref="ISetupService.SetOptionText"/>.</summary>
    public sealed class SetOptionTextRequest
    {
        /// <summary>Option key.</summary>
        public string Key { get; set; } = "";

        /// <summary>New text.</summary>
        public string Text { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="ISetupService.SelectStepItem"/>.</summary>
    public sealed class SelectStepItemRequest
    {
        /// <summary>Index in <see cref="SetupSnapshot.CustomSteps"/>.</summary>
        public int StepIndex { get; set; }

        /// <summary>The <see cref="OptionChoice.Index"/> of the picked item (its index in the handler's list, which may differ from its position in <see cref="SetupCustomStep.Items"/>).</summary>
        public int ItemIndex { get; set; }
    }

    /// <summary>The setup changed (device plugged, button pressed, player placed, profile matched…). Coalesced to about 10 per second.</summary>
    [BridgeEvent("setup.changed")]
    public sealed class SetupChanged
    {
        /// <summary>New snapshot.</summary>
        public SetupSnapshot Snapshot { get; set; } = new SetupSnapshot();
    }

    /// <summary>A device is being used (button pressed, key typed, mouse moved). Lets the UI flash it. At most a few per second per device.</summary>
    [BridgeEvent("setup.deviceActivity")]
    public sealed class DeviceActivity
    {
        /// <summary>Device id.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>True when activity starts, false when it stops.</summary>
        public bool Active { get; set; }
    }

    /// <summary>A device assigned to a player was unplugged.</summary>
    [BridgeEvent("setup.deviceLost")]
    public sealed class DeviceLost
    {
        /// <summary>Device id.</summary>
        public string DeviceId { get; set; } = "";

        /// <summary>Kind of device.</summary>
        public DeviceKind Kind { get; set; }

        /// <summary>Number of the player it belonged to.</summary>
        public int? PlayerNumber { get; set; }

        /// <summary>Nickname of that player, when known.</summary>
        public string? Nickname { get; set; }
    }
}
