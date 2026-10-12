using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.App.Settings;

namespace Nucleus.Coop.Services.Settings;

/// <summary>What has to happen after a setting was saved.</summary>
[Flags]
public enum SettingEffects
{
    /// <summary>Nothing.</summary>
    None = 0,

    /// <summary>Redraw <c>KbShortcutsReminder.jpeg</c>.</summary>
    KeyboardReminder = 1,

    /// <summary>Refresh the gamepad shortcut thread (<c>GamepadShortcuts.UpdateShortcutsValue</c>).</summary>
    GamepadShortcuts = 2,

    // 4 was GamepadNavigation: the legacy gamepad-as-mouse thread, removed (the web UI navigates with gamepads itself).

    /// <summary>Redraw <c>XShortcutsReminder.jpeg</c>.</summary>
    GamepadReminder = 8,

    /// <summary>Profiles were turned on or off globally: every game page and menu changes.</summary>
    GameProfiles = 16,

    /// <summary>Recompute <c>[CustomLayout] MaxPlayers</c> from the divisions.</summary>
    LayoutMaxPlayers = 32,
}

/// <summary>One setting: schema, validation, and how it maps onto the App_* settings classes (whose setters write Settings.ini).</summary>
public sealed class SettingDefinition
{
    /// <summary>Section id.</summary>
    public string Section { get; set; } = "";

    /// <summary>Schema sent to the UI (choices filled at request time for dynamic lists).</summary>
    public SettingField Field { get; set; } = new SettingField();

    /// <summary>Validation.</summary>
    public SettingRule Rule { get; set; } = new SettingRule();

    /// <summary>Reads the current value, in the normalized string form.</summary>
    public Func<string> Get { get; set; } = () => "";

    /// <summary>Writes a validated, normalized value.</summary>
    public Action<string> Set { get; set; } = _ => { };

    /// <summary>Side effects of a change.</summary>
    public SettingEffects Effects { get; set; }

    /// <summary>Choices computed from the PC (network adapters, audio devices, screens).</summary>
    public Func<List<SettingChoice>>? DynamicChoices { get; set; }
}

/// <summary>A section of the schema.</summary>
public sealed class SettingSectionDefinition
{
    /// <summary>Id.</summary>
    public string Id { get; set; } = "";

    /// <summary>Title.</summary>
    public string Title { get; set; } = "";

    /// <summary>Description.</summary>
    public string? Description { get; set; }
}

/// <summary>
/// Every user-facing setting of the legacy Settings form (Settings, hotkeys, Players options, Audio, Layout tabs)
/// and of the gamepad shortcuts window (XInputShortcutsSetup), with Settings.ini defaults from the shipped file.
/// Theme, window positions and other WinForms-only options are left out (see docs/webui/library-services.md).
/// </summary>
public static class SettingsCatalog
{
    /// <summary>Sections, in order.</summary>
    public static readonly IReadOnlyList<SettingSectionDefinition> Sections = new[]
    {
        new SettingSectionDefinition { Id = "general", Title = "General", Description = "Languages, network and Windows user accounts used by game instances." },
        new SettingSectionDefinition { Id = "players", Title = "Players", Description = "How gamepads are assigned to players. Nicknames and Steam IDs are edited with the player list." },
        new SettingSectionDefinition { Id = "hotkeys", Title = "Keyboard shortcuts", Description = "Shortcuts available during a game session. Changes apply from the next session." },
        new SettingSectionDefinition { Id = "gamepad", Title = "Gamepad shortcuts", Description = "Button combinations available during a game session (XInput gamepads). Steam Input may interfere with them. The Nucleus window itself works with any gamepad: D-pad or left stick to move, A to select, B to go back, right stick to scroll." },
        new SettingSectionDefinition { Id = "audio", Title = "Audio", Description = "Playback device of each game instance. This may not work with every game." },
        new SettingSectionDefinition { Id = "layout", Title = "Layout", Description = "Defaults of the custom screen layout, split-screen background, cutscenes mode and Windows merger. Profiles keep their own copy." },
    };

    /// <summary>Steam languages of the global setting (value, label).</summary>
    public static readonly IReadOnlyList<(string Value, string Label)> SteamLanguages = new[]
    {
        ("Automatic", "Automatic (Steam client language)"), ("Arabic", "Arabic"), ("Brazilian", "Portuguese (Brazil)"), ("Bulgarian", "Bulgarian"),
        ("Schinese", "Chinese (Simplified)"), ("Tchinese", "Chinese (Traditional)"), ("Czech", "Czech"), ("Danish", "Danish"), ("Dutch", "Dutch"),
        ("English", "English"), ("Finnish", "Finnish"), ("French", "French"), ("German", "German"), ("Greek", "Greek"), ("Hungarian", "Hungarian"),
        ("Italian", "Italian"), ("Japanese", "Japanese"), ("Koreana", "Korean"), ("Latam", "Spanish (Latin America)"), ("Norwegian", "Norwegian"),
        ("Polish", "Polish"), ("Portuguese", "Portuguese"), ("Romanian", "Romanian"), ("Russian", "Russian"), ("Spanish", "Spanish (Spain)"),
        ("Swedish", "Swedish"), ("Thai", "Thai"), ("Turkish", "Turkish"), ("Ukrainian", "Ukrainian"),
    };

    /// <summary>Epic / GOG emulator languages.</summary>
    public static readonly IReadOnlyList<string> EpicLanguages = new[]
    {
        "Arabic", "Brazilian", "Bulgarian", "Chinese", "Czech", "Danish", "Dutch", "English", "Finnish", "French", "German", "Greek", "Hungarian",
        "Italian", "Japanese", "Koreana", "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Spanish", "Swedish", "Thai", "Turkish", "Ukrainian",
    };

    /// <summary>Keys accepted as input lock key (<c>App_Hotkeys.lockKeysDict</c>).</summary>
    public static readonly IReadOnlyList<string> LockKeys = new[]
    {
        "End", "Home", "Delete", "Multiply", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "+", "-",
        "Numpad 0", "Numpad 1", "Numpad 2", "Numpad 3", "Numpad 4", "Numpad 5", "Numpad 6", "Numpad 7", "Numpad 8", "Numpad 9",
    };

    /// <summary>Split-screen background colours (<c>BackgroundColors.ColorsDictionnary</c>).</summary>
    public static readonly IReadOnlyList<string> SplitColors = new[] { "Black", "Gray", "White", "Dark Blue", "Blue", "Purple", "Pink", "Red", "Orange", "Yellow", "Green" };

    /// <summary>Toggles that exclude each other.</summary>
    public static readonly IReadOnlyList<ExclusiveRule> ExclusiveRules = new[]
    {
        new ExclusiveRule("players.assignGamepadsByIndex", new[] { "players.assignGamepadsOnButtonPress" }, "\"Assign gamepads by index\" and \"Assign gamepads on button press\" can't both be on."),
        new ExclusiveRule("layout.cutscenesMuteAudioOnly", new[] { "layout.cutscenesKeepAspectRatio", "layout.cutscenesUnfocus" }, "\"Mute audio only\" can't be combined with the other cutscenes mode options."),
    };

    /// <summary>Keyboard shortcut rows of the reminder image, in order: label, key.</summary>
    public static readonly IReadOnlyList<(string Label, string Key)> KeyboardReminderRows = new[]
    {
        ("Close Nucleus", "hotkeys.closeApp"), ("Stop session", "hotkeys.stopSession"), ("Toggle top most", "hotkeys.topMost"),
        ("Toggle unfocus", "hotkeys.setFocus"), ("Reset windows", "hotkeys.resetWindows"), ("Cutscenes mode", "hotkeys.cutscenesMode"),
        ("Switch layout", "hotkeys.switchLayout"), ("Shortcuts reminder", "hotkeys.shortcutsReminder"), ("Switch merged window", "hotkeys.switchMergerWindow"),
        ("Lock / unlock inputs", "hotkeys.lockInputs"),
    };

    /// <summary>Gamepad shortcut rows of the reminder image, in order: label, key.</summary>
    public static readonly IReadOnlyList<(string Label, string Key)> GamepadReminderRows = new[]
    {
        ("Toggle unfocus", "gamepad.setFocus"), ("Close Nucleus", "gamepad.close"), ("Stop session", "gamepad.stopSession"),
        ("Toggle top most", "gamepad.topMost"), ("Reset windows", "gamepad.resetWindows"), ("Cutscenes mode", "gamepad.cutscenesMode"),
        ("Switch layout", "gamepad.switchLayout"), ("Lock / unlock inputs", "gamepad.lockInputs"), ("Alt+Tab", "gamepad.releaseCursor"),
    };

    /// <summary>Asked before turning "Keep Nucleus user accounts" off (the legacy Settings window asked it too).</summary>
    public static SettingConfirmation KeepAccountsConfirmation => new SettingConfirmation
    {
        Values = { "false" },
        Title = "Turn off Keep Nucleus user accounts?",
        Message = "The next time you run a game that uses separate Windows user accounts, Nucleus deletes every account it created, with their files and saves.",
        ConfirmLabel = "Turn off",
    };

    /// <summary>Builds the definitions.</summary>
    public static List<SettingDefinition> Create()
    {
        var list = new List<SettingDefinition>();

        // ---- General (Settings tab) ----
        list.Add(Choice("general", "misc.steamLanguage", "Steam language", "Language reported by the Steam emulator. A game can override it from its menu.", "Automatic",
            SteamLanguages.Select(l => new SettingChoice { Value = l.Value, Label = l.Label }).ToList(),
            () => App_Misc.SteamLang ?? "", v => App_Misc.SteamLang = v));
        list.Add(Choice("general", "misc.epicLanguage", "Epic / GOG language", "Language reported by the Epic and GOG emulators.", "English",
            EpicLanguages.Select(l => new SettingChoice { Value = l, Label = l }).ToList(),
            () => App_Misc.EpicLang ?? "", v => App_Misc.EpicLang = v));
        SettingDefinition network = Choice("general", "misc.network", "Network adapter", "Adapter used by handlers that change the IP address of each instance.", "Automatic",
            new List<SettingChoice>(), () => App_Misc.Network ?? "", v => App_Misc.Network = v);
        network.DynamicChoices = SystemChoices.NetworkAdapters;
        list.Add(network);
        list.Add(Toggle("general", "misc.useNicknames", "Use nicknames in game", "Player nicknames are passed to the game when the handler supports it.", true,
            () => App_Misc.UseNicksInGame, v => App_Misc.UseNicksInGame = v));
        SettingDefinition keepAccounts = Toggle("general", "misc.keepAccounts", "Keep Nucleus user accounts",
            "Keeps the Windows user accounts Nucleus creates for some games. When off, the next game that uses them deletes them with their files and saves.", false,
            () => App_Misc.KeepAccounts, v => App_Misc.KeepAccounts = v);
        keepAccounts.Field.Confirm = KeepAccountsConfirmation;
        list.Add(keepAccounts);
        SettingDefinition password = Password("general", "misc.nucleusAccountPassword", "Nucleus user accounts password",
            "Password of the Windows accounts Nucleus creates. Stored in plain text in Settings.ini. 12345 or empty uses the built-in password.", "12345", 127,
            () => App_Misc.NucleusAccountPassword ?? "", v => App_Misc.NucleusAccountPassword = v);
        list.Add(password);
        list.Add(Toggle("general", "misc.autoDesktopScaling", "Set desktop scale to 100% during sessions", "Some games need 100% display scaling to place their windows correctly.", false,
            () => App_Misc.AutoDesktopScaling, v => App_Misc.AutoDesktopScaling = v));
        list.Add(Toggle("general", "misc.fastLaunch", "Fast launch",
            "Starts each instance as soon as the previous one is ready instead of always waiting the handler's full pauses. Turn it off if a game stops launching correctly.", true,
            () => App_Misc.FastLaunch, v => App_Misc.FastLaunch = v));
        list.Add(Toggle("general", "misc.ignoreInputLockReminder", "Don't remind me to lock inputs", "When off, a message at launch explains how to lock keyboard and mouse input.", true,
            () => App_Misc.IgnoreInputLockReminder, v => App_Misc.IgnoreInputLockReminder = v));
        list.Add(Toggle("general", "misc.debugLog", "Enable debug log", "Writes debug-log.txt in the Nucleus folder. Turned off automatically when the log grows past 150 KB.", true,
            () => App_Misc.DebugLog, v => App_Misc.DebugLog = v));
        SettingDefinition profiles = Toggle("general", "misc.disableGameProfiles", "Disable game profiles", "Nucleus uses these global settings instead of per-game profiles.", false,
            () => App_Misc.DisableGameProfiles, v => App_Misc.DisableGameProfiles = v);
        profiles.Effects = SettingEffects.GameProfiles;
        list.Add(profiles);

        // ---- Players tab options ----
        list.Add(Toggle("players", "players.assignGamepadsByIndex", "Assign gamepads by index",
            "Default for new games: profiles store gamepad API indexes instead of hardware ids (some handlers turn it off). Switching modes can prevent older profiles from loading.", false,
            () => App_Misc.UseXinputIndex, v => App_Misc.UseXinputIndex = v));
        list.Add(Toggle("players", "players.assignGamepadsOnButtonPress", "Assign gamepads on button press",
            "Default for new games: profiles assign gamepads when players press a button instead of reusing the saved ones. Works well with custom nicknames.", false,
            () => App_Misc.ProfileAssignGamepadByButonPress, v => App_Misc.ProfileAssignGamepadByButonPress = v));

        // ---- Keyboard shortcuts ----
        list.Add(Hotkey("hotkeys.closeApp", "Close Nucleus", "Ctrl+Q", () => App_Hotkeys.CloseApp, v => App_Hotkeys.CloseApp = v));
        list.Add(Hotkey("hotkeys.stopSession", "Stop session", "Ctrl+E", () => App_Hotkeys.StopSession, v => App_Hotkeys.StopSession = v));
        list.Add(Hotkey("hotkeys.topMost", "Toggle top most", "Ctrl+T", () => App_Hotkeys.TopMost, v => App_Hotkeys.TopMost = v));
        list.Add(Hotkey("hotkeys.setFocus", "Toggle unfocus", "Ctrl+H", () => App_Hotkeys.SetFocus, v => App_Hotkeys.SetFocus = v));
        list.Add(Hotkey("hotkeys.resetWindows", "Reset windows", "Ctrl+R", () => App_Hotkeys.ResetWindows, v => App_Hotkeys.ResetWindows = v));
        list.Add(Hotkey("hotkeys.cutscenesMode", "Cutscenes mode", "Ctrl+M", () => App_Hotkeys.CutscenesMode, v => App_Hotkeys.CutscenesMode = v));
        list.Add(Hotkey("hotkeys.switchLayout", "Switch layout", "Ctrl+N", () => App_Hotkeys.SwitchLayout, v => App_Hotkeys.SwitchLayout = v));
        list.Add(Hotkey("hotkeys.shortcutsReminder", "Show shortcuts reminder", "Alt+R", () => App_Hotkeys.ShortcutsReminder, v => App_Hotkeys.ShortcutsReminder = v));
        SettingDefinition merger = Hotkey("hotkeys.switchMergerWindow", "Switch merged window", "Alt+A", () => App_Hotkeys.SwitchMergerForeGroundChild, v => App_Hotkeys.SwitchMergerForeGroundChild = v);
        merger.Field.Description = "With the Windows merger: each press brings another game window to the front (like Alt+Tab).";
        list.Add(merger);
        SettingDefinition lockKey = Choice("hotkeys", "hotkeys.lockInputs", "Lock inputs key", "Locks and unlocks keyboard and mouse input during a session (no modifier).", "End",
            LockKeys.Select(k => new SettingChoice { Value = k, Label = k }).ToList(), () => App_Hotkeys.LockInputs ?? "", v => App_Hotkeys.LockInputs = v);
        lockKey.Effects = SettingEffects.KeyboardReminder;
        list.Add(lockKey);

        // ---- Gamepad shortcuts (XInputShortcutsSetup) ----
        list.Add(Pad("gamepad.setFocus", "Toggle unfocus", "1024+32768", () => App_GamePadShortcuts.SetFocus, v => App_GamePadShortcuts.SetFocus = v));
        list.Add(Pad("gamepad.close", "Close Nucleus", "1024+16", () => App_GamePadShortcuts.Close, v => App_GamePadShortcuts.Close = v));
        list.Add(Pad("gamepad.stopSession", "Stop session", "1024+32", () => App_GamePadShortcuts.StopSession, v => App_GamePadShortcuts.StopSession = v));
        list.Add(Pad("gamepad.topMost", "Toggle top most", "1024+16384", () => App_GamePadShortcuts.TopMost, v => App_GamePadShortcuts.TopMost = v));
        list.Add(Pad("gamepad.resetWindows", "Reset windows", "1024+8", () => App_GamePadShortcuts.ResetWindows, v => App_GamePadShortcuts.ResetWindows = v));
        list.Add(Pad("gamepad.cutscenesMode", "Cutscenes mode", "1024+8192", () => App_GamePadShortcuts.CutscenesMode, v => App_GamePadShortcuts.CutscenesMode = v));
        list.Add(Pad("gamepad.switchLayout", "Switch layout", "1024+512", () => App_GamePadShortcuts.SwitchLayout, v => App_GamePadShortcuts.SwitchLayout = v));
        list.Add(Pad("gamepad.lockInputs", "Lock / unlock inputs", "1024+128", () => App_GamePadShortcuts.LockInputs, v => App_GamePadShortcuts.LockInputs = v));
        list.Add(Pad("gamepad.releaseCursor", "Alt+Tab", "1024+256", () => App_GamePadShortcuts.ReleaseCursor, v => App_GamePadShortcuts.ReleaseCursor = v,
            "Sends Alt+Tab to get the cursor out of a game window."));

        // The legacy "Gamepad control of the Nucleus window" (mouse cursor on the left stick, its click buttons, dead zone,
        // "Lock UI control" and "On-screen keyboard" combos) is gone: it fought the web UI's own gamepad navigation.
        SettingDefinition padType = Choice("gamepad", "gamepad.type", "Gamepad type", "Button names and pictures shown for the shortcuts.", "Xbox",
            new List<SettingChoice> { new SettingChoice { Value = "Xbox", Label = "Xbox" }, new SettingChoice { Value = "Playstation", Label = "PlayStation" } },
            () => App_GamePadNavigation.Type ?? "", v => App_GamePadNavigation.Type = v);
        padType.Effects = SettingEffects.GamepadReminder;
        list.Add(padType);

        // ---- Audio ----
        list.Add(Choice("audio", "audio.mode", "Playback devices", null, "1",
            new List<SettingChoice>
            {
                new SettingChoice { Value = "0", Label = "Use the default playback device for every instance" },
                new SettingChoice { Value = "1", Label = "Choose a device for each instance" },
            },
            () => App_Audio.Custom ?? "", v => App_Audio.Custom = v));
        for (int i = 1; i <= 8; i++)
        {
            string iniKey = "AudioInstance" + i;
            SettingDefinition instance = Choice("audio", "audio.instance" + i, "Instance " + i, i == 1 ? "Used when \"Choose a device for each instance\" is selected." : null, "Default",
                new List<SettingChoice>(),
                () => App_Audio.Instances_AudioOutput.TryGetValue(iniKey, out string? device) && !string.IsNullOrEmpty(device) ? device : "Default",
                v => App_Audio.SaveAudioOutput(iniKey, v));
            instance.DynamicChoices = SystemChoices.AudioOutputs;
            list.Add(instance);
        }

        // ---- Layout ----
        SettingDefinition vertical = Number("layout", "layout.verticalLines", "Vertical divisions", "Columns of the custom layout grid, minus one.", "1", 0, 5, 1,
            () => App_Layouts.VerticalLines, v => App_Layouts.VerticalLines = v);
        vertical.Effects = SettingEffects.LayoutMaxPlayers;
        list.Add(vertical);
        SettingDefinition horizontal = Number("layout", "layout.horizontalLines", "Horizontal divisions", "Rows of the custom layout grid, minus one. Maximum players = (horizontal + 1) × (vertical + 1).", "1", 0, 5, 1,
            () => App_Layouts.HorizontalLines, v => App_Layouts.HorizontalLines = v);
        horizontal.Effects = SettingEffects.LayoutMaxPlayers;
        list.Add(horizontal);
        list.Add(Toggle("layout", "layout.splitDiv", "Split-screen division", "Fills the screen behind the game windows with a colour. May not work for all games.", false,
            () => App_Layouts.SplitDiv, v => App_Layouts.SplitDiv = v));
        list.Add(Choice("layout", "layout.splitDivColor", "Background colour", null, "Black",
            SplitColors.Select(c => new SettingChoice { Value = c, Label = c }).ToList(), () => App_Layouts.SplitDivColor ?? "", v => App_Layouts.SplitDivColor = v));
        list.Add(Toggle("layout", "layout.hideDesktopOnly", "Hide desktop only", "Only shows the background window, without resizing or moving the game windows.", false,
            () => App_Layouts.HideOnly, v => App_Layouts.HideOnly = v));
        list.Add(Toggle("layout", "layout.cutscenesMuteAudioOnly", "Cutscenes mode: mute audio only", "Can't be combined with the two options below.", false,
            () => App_Layouts.Cts_MuteAudioOnly, v => App_Layouts.Cts_MuteAudioOnly = v));
        list.Add(Toggle("layout", "layout.cutscenesKeepAspectRatio", "Cutscenes mode: keep original window size", null, false,
            () => App_Layouts.Cts_KeepAspectRatio, v => App_Layouts.Cts_KeepAspectRatio = v));
        list.Add(Toggle("layout", "layout.cutscenesUnfocus", "Cutscenes mode: unfocus windows", null, false,
            () => App_Layouts.Cts_Unfocus, v => App_Layouts.Cts_Unfocus = v));
        list.Add(Toggle("layout", "layout.windowsMerger", "Windows merger", "Merges the game windows into one window, so Lossless Scaling can be used. Multiple monitors are not supported yet.", false,
            () => App_Layouts.WindowsMerger, v => App_Layouts.WindowsMerger = v));
        list.Add(Toggle("layout", "layout.losslessHook", "Lossless hook", "Lossless Scaling keeps upscaling when another window gets the focus (for games that need real focus to receive input).", true,
            () => App_Layouts.LosslessHook, v => App_Layouts.LosslessHook = v));
        SettingDefinition mergerRes = Choice("layout", "layout.mergerResolution", "Windows merger resolution", "Size of the merged window. Defaults to the primary screen.", "",
            new List<SettingChoice>(),
            () => string.IsNullOrEmpty(App_Layouts.WindowsMergerRes) ? SystemChoices.PrimaryScreenResolution() : App_Layouts.WindowsMergerRes,
            v => App_Layouts.WindowsMergerRes = v);
        mergerRes.DynamicChoices = SystemChoices.MergerResolutions;
        list.Add(mergerRes);

        // Dependent fields (the legacy forms hid or disabled these controls).
        When(list, "players.assignGamepadsByIndex", visible: ("misc.disableGameProfiles", "false"));
        When(list, "players.assignGamepadsOnButtonPress", visible: ("misc.disableGameProfiles", "false"));
        for (int i = 1; i <= 8; i++)
        {
            When(list, "audio.instance" + i, enabled: ("audio.mode", "1"));
        }

        When(list, "layout.splitDivColor", enabled: ("layout.splitDiv", "true"));
        When(list, "layout.hideDesktopOnly", enabled: ("layout.splitDiv", "true"));
        When(list, "layout.cutscenesKeepAspectRatio", enabled: ("layout.cutscenesMuteAudioOnly", "false"));
        When(list, "layout.cutscenesUnfocus", enabled: ("layout.cutscenesMuteAudioOnly", "false"));
        When(list, "layout.losslessHook", enabled: ("layout.windowsMerger", "true"));
        When(list, "layout.mergerResolution", enabled: ("layout.windowsMerger", "true"));

        return list;
    }

    private static void When(List<SettingDefinition> list, string key, (string Key, string Value)? visible = null, (string Key, string Value)? enabled = null)
    {
        SettingField field = list.Single(d => d.Field.Key == key).Field;
        if (visible.HasValue)
        {
            field.VisibleWhen = new SettingCondition { Key = visible.Value.Key, Values = new List<string> { visible.Value.Value } };
        }

        if (enabled.HasValue)
        {
            field.EnabledWhen = new SettingCondition { Key = enabled.Value.Key, Values = new List<string> { enabled.Value.Value } };
        }
    }

    private static SettingDefinition Toggle(string section, string key, string label, string? description, bool defaultValue, Func<bool> get, Action<bool> set) =>
        Define(section, key, label, description, SettingType.Toggle, defaultValue ? "true" : "false",
            () => get() ? "true" : "false", v => set(v == "true"));

    private static SettingDefinition Number(string section, string key, string label, string? description, string defaultValue, int min, int max, int step, Func<int> get, Action<int> set)
    {
        SettingDefinition definition = Define(section, key, label, description, SettingType.Number, defaultValue,
            () => get().ToString(CultureInfo.InvariantCulture), v => set(int.Parse(v, CultureInfo.InvariantCulture)));
        definition.Field.Min = definition.Rule.Min = min;
        definition.Field.Max = definition.Rule.Max = max;
        definition.Field.Step = step;
        return definition;
    }

    private static SettingDefinition Password(string section, string key, string label, string? description, string defaultValue, int maxLength, Func<string> get, Action<string> set)
    {
        SettingDefinition definition = Define(section, key, label, description, SettingType.Password, defaultValue, get, set);
        definition.Rule.MaxLength = maxLength;
        definition.Field.MaxLength = maxLength;
        return definition;
    }

    private static SettingDefinition Choice(string section, string key, string label, string? description, string defaultValue, List<SettingChoice> choices, Func<string> get, Action<string> set)
    {
        SettingDefinition definition = Define(section, key, label, description, SettingType.Choice, defaultValue, get, set);
        definition.Field.Choices = choices;
        definition.Rule.AllowedValues = choices.Select(c => c.Value).ToList();
        return definition;
    }

    private static SettingDefinition Hotkey(string key, string label, string defaultValue, Func<string[]> get, Action<string[]> set)
    {
        SettingDefinition definition = Define("hotkeys", key, label, null, SettingType.Hotkey, defaultValue,
            () =>
            {
                string[]? value = get();
                return value != null && value.Length == 2 ? value[0] + "+" + value[1] : "";
            },
            v => set(v.Split('+')));
        definition.Rule.UniqueGroup = "keyboard";
        definition.Effects = SettingEffects.KeyboardReminder;
        return definition;
    }

    private static SettingDefinition Pad(string key, string label, string defaultValue, Func<int[]> get, Action<int[]> set, string? description = null)
    {
        SettingDefinition definition = Define("gamepad", key, label, description, SettingType.GamepadCombo, defaultValue,
            () =>
            {
                int[]? value = get();
                return value != null && value.Length == 2 ? GamepadComboFormat.Format(value) : "";
            },
            v => set(GamepadComboFormat.Parse(v, 2, out _)!));
        definition.Rule.ComboSize = 2;
        definition.Field.ComboButtons = 2;
        definition.Rule.UniqueGroup = "gamepadShortcuts";
        definition.Effects = SettingEffects.GamepadShortcuts | SettingEffects.GamepadReminder;
        return definition;
    }

    private static SettingDefinition Define(string section, string key, string label, string? description, SettingType type, string defaultValue, Func<string> get, Action<string> set) =>
        new SettingDefinition
        {
            Section = section,
            Field = new SettingField { Key = key, Label = label, Description = description, Type = type, Default = defaultValue },
            Rule = new SettingRule { Key = key, Label = label, Type = type },
            Get = get,
            Set = set,
        };
}
