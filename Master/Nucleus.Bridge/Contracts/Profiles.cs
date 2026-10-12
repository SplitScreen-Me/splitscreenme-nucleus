using System.Collections.Generic;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>Saved setups (profiles) of the game whose setup is open.</summary>
    [BridgeService("profiles")]
    public interface IProfilesService
    {
        /// <summary>Profiles of the game whose setup is open.</summary>
        List<ProfileSummary> List();

        /// <summary>Loads a profile: restores layouts and options, then matches devices to its players as they are used.</summary>
        SetupSnapshot Load(ProfileRequest request);

        /// <summary>Unloads the profile and goes back to a free setup.</summary>
        SetupSnapshot Unload();

        /// <summary>Deletes a profile. The following profiles are renumbered.</summary>
        void Delete(ProfileRequest request);

        /// <summary>Settings of the loaded profile, or of the profile that will be saved at launch.</summary>
        ProfileSettings GetSettings();

        /// <summary>Saves profile settings. Returns the invalid fields; nothing is saved when the list is not empty.</summary>
        List<ValidationError> SaveSettings(ProfileSettings settings);
    }

    /// <summary>Identifies a profile of the current game.</summary>
    public sealed class ProfileRequest
    {
        /// <summary>Profile number.</summary>
        public int ProfileId { get; set; }
    }

    /// <summary>A saved profile, as listed.</summary>
    public sealed class ProfileSummary
    {
        /// <summary>Profile number.</summary>
        public int Id { get; set; }

        /// <summary>Title given by the user. Empty means "Profile {Id}".</summary>
        public string Title { get; set; } = "";

        /// <summary>Notes.</summary>
        public string Notes { get; set; } = "";

        /// <summary>Number of players.</summary>
        public int Players { get; set; }

        /// <summary>Number of gamepad players.</summary>
        public int Gamepads { get; set; }

        /// <summary>Number of keyboard and mouse players.</summary>
        public int KeyboardMice { get; set; }

        /// <summary>Starts by itself once every player is matched.</summary>
        public bool AutoPlay { get; set; }

        /// <summary>Players, in order.</summary>
        public List<ProfilePlayer> PlayerList { get; set; } = new List<ProfilePlayer>();
    }

    /// <summary>A player stored in a profile.</summary>
    public sealed class ProfilePlayer
    {
        /// <summary>Player number, 1-based.</summary>
        public int Number { get; set; }

        /// <summary>Nickname.</summary>
        public string? Nickname { get; set; }

        /// <summary>Kind of device expected.</summary>
        public DeviceKind Device { get; set; }

        /// <summary>Monitor index.</summary>
        public int ScreenIndex { get; set; }

        /// <summary>Number of guests.</summary>
        public int Guests { get; set; }
    }

    /// <summary>Per-player settings stored in a profile.</summary>
    public sealed class ProfileSlot
    {
        /// <summary>Nickname. Empty means the default.</summary>
        public string Nickname { get; set; } = "";

        /// <summary>Steam ID. Empty means the default.</summary>
        public string SteamId { get; set; } = "";

        /// <summary>Whether the handler imposes Steam IDs.</summary>
        public bool SteamIdLocked { get; set; }

        /// <summary>Ideal processor, "*" for any.</summary>
        public string IdealProcessor { get; set; } = "*";

        /// <summary>Processor affinity mask, empty for all.</summary>
        public string Affinity { get; set; } = "";

        /// <summary>Process priority class, for example "Normal".</summary>
        public string PriorityClass { get; set; } = "Normal";

        /// <summary>Audio output endpoint id, or "Default".</summary>
        public string AudioDevice { get; set; } = "Default";
    }

    /// <summary>A choice offered by a settings field.</summary>
    public sealed class SettingChoice
    {
        /// <summary>Stored value.</summary>
        public string Value { get; set; } = "";

        /// <summary>Label.</summary>
        public string Label { get; set; } = "";
    }

    /// <summary>Settings of a profile, edited as one object.</summary>
    public sealed class ProfileSettings
    {
        /// <summary>Whether these settings belong to a profile that does not exist yet (it will be created at launch).</summary>
        public bool IsNew { get; set; }

        /// <summary>Title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Notes.</summary>
        public string Notes { get; set; } = "";

        /// <summary>Starts by itself once every player is matched.</summary>
        public bool AutoPlay { get; set; }

        /// <summary>Uses the nicknames in game.</summary>
        public bool UseNicknames { get; set; }

        /// <summary>Adjusts Windows scaling automatically.</summary>
        public bool AutoDesktopScaling { get; set; }

        /// <summary>Draws a background behind the game windows.</summary>
        public bool SplitDivider { get; set; }

        /// <summary>Hides only the desktop instead of drawing the background.</summary>
        public bool SplitDividerHideDesktopOnly { get; set; }

        /// <summary>Background colour name.</summary>
        public string SplitDividerColor { get; set; } = "";

        /// <summary>Custom layout: horizontal lines (0 to 5).</summary>
        public int CustomLayoutHorizontalLines { get; set; }

        /// <summary>Custom layout: vertical lines (0 to 5).</summary>
        public int CustomLayoutVerticalLines { get; set; }

        /// <summary>Custom layout: maximum players.</summary>
        public int CustomLayoutMaxPlayers { get; set; }

        /// <summary>Delay given to each window to settle, in milliseconds.</summary>
        public int WindowSetupDelayMs { get; set; }

        /// <summary>Pause between two instance launches, in seconds.</summary>
        public int PauseBetweenInstancesSec { get; set; }

        /// <summary>Network interface, or "Automatic".</summary>
        public string Network { get; set; } = "Automatic";

        /// <summary>Uses per-player audio outputs (<see cref="ProfileSlot.AudioDevice"/>).</summary>
        public bool CustomAudio { get; set; }

        /// <summary>Cutscenes: keep the aspect ratio.</summary>
        public bool CutscenesKeepAspectRatio { get; set; }

        /// <summary>Cutscenes: only mute the audio.</summary>
        public bool CutscenesMuteAudioOnly { get; set; }

        /// <summary>Cutscenes: unfocus the other windows.</summary>
        public bool CutscenesUnfocus { get; set; }

        /// <summary>Disables gamepad navigation of the app during the session.</summary>
        public bool StopUiNavigation { get; set; }

        /// <summary>Merges the game windows into one window.</summary>
        public bool WindowsMerger { get; set; }

        /// <summary>Uses the lossless hook with the windows merger.</summary>
        public bool LosslessHook { get; set; }

        /// <summary>Resolution of the merged window, for example "1920X1080".</summary>
        public string MergerResolution { get; set; } = "";

        /// <summary>Per-player settings, in player order.</summary>
        public List<ProfileSlot> Slots { get; set; } = new List<ProfileSlot>();

        /// <summary>Available network interfaces (read-only).</summary>
        public List<SettingChoice> NetworkChoices { get; set; } = new List<SettingChoice>();

        /// <summary>Available audio outputs (read-only).</summary>
        public List<SettingChoice> AudioChoices { get; set; } = new List<SettingChoice>();

        /// <summary>Available merger resolutions (read-only).</summary>
        public List<SettingChoice> MergerResolutionChoices { get; set; } = new List<SettingChoice>();

        /// <summary>Available background colours (read-only).</summary>
        public List<SettingChoice> SplitDividerColorChoices { get; set; } = new List<SettingChoice>();
    }

    /// <summary>Profiles of the current game were added, deleted or renamed.</summary>
    [BridgeEvent("profiles.changed", Invalidates = new[] { "profiles.list", "profiles.getSettings", "library.get" })]
    public sealed class ProfilesChanged
    {
        /// <summary>Game whose profiles changed.</summary>
        public string GameId { get; set; } = "";
    }
}
