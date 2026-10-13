using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Localization;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Localization;
using static Nucleus.Gaming.Localization.I18n;

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
    /// <summary>Key of the app language setting, the first one of General (saved by <see cref="LanguageSettings"/>).</summary>
    public const string LanguageKey = "misc.appLanguage";

    /// <summary>Sections, in order, in the language in use (built at each call: texts follow the language).</summary>
    public static IReadOnlyList<SettingSectionDefinition> Sections => new[]
    {
        new SettingSectionDefinition { Id = "general", Title = T("host.settings.sections.general.title"), Description = T("host.settings.sections.general.description") },
        new SettingSectionDefinition { Id = "players", Title = T("host.settings.sections.players.title"), Description = T("host.settings.sections.players.description") },
        new SettingSectionDefinition { Id = "hotkeys", Title = T("host.settings.sections.hotkeys.title"), Description = T("host.settings.sections.hotkeys.description") },
        new SettingSectionDefinition { Id = "gamepad", Title = T("host.settings.sections.gamepad.title"), Description = T("host.settings.sections.gamepad.description") },
        new SettingSectionDefinition { Id = "audio", Title = T("host.settings.sections.audio.title"), Description = T("host.settings.sections.audio.description") },
        new SettingSectionDefinition { Id = "layout", Title = T("host.settings.sections.layout.title"), Description = T("host.settings.sections.layout.description") },
    };

    /// <summary>Description of the Audio section that also names the Windows default playback device.</summary>
    public static string AudioSectionDescription(string defaultDevice) => T("host.settings.sections.audio.descriptionWithDefault", new { device = defaultDevice });

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

    /// <summary>Split-screen background colours (<c>BackgroundColors.ColorsDictionnary</c>): the values Settings.ini stores.</summary>
    public static readonly IReadOnlyList<string> SplitColors = new[] { "Black", "Gray", "White", "Dark Blue", "Blue", "Purple", "Pink", "Red", "Orange", "Yellow", "Green" };

    /// <summary>Label of a split-screen background colour in the language in use (the stored value when it is not a known colour).</summary>
    public static string SplitColorLabel(string color) => color switch
    {
        "Black" => T("host.settings.colors.black"),
        "Gray" => T("host.settings.colors.gray"),
        "White" => T("host.settings.colors.white"),
        "Dark Blue" => T("host.settings.colors.darkBlue"),
        "Blue" => T("host.settings.colors.blue"),
        "Purple" => T("host.settings.colors.purple"),
        "Pink" => T("host.settings.colors.pink"),
        "Red" => T("host.settings.colors.red"),
        "Orange" => T("host.settings.colors.orange"),
        "Yellow" => T("host.settings.colors.yellow"),
        "Green" => T("host.settings.colors.green"),
        _ => color,
    };

    /// <summary>Toggles that exclude each other (messages in the language in use).</summary>
    public static IReadOnlyList<ExclusiveRule> ExclusiveRules => new[]
    {
        new ExclusiveRule("players.assignGamepadsByIndex", new[] { "players.assignGamepadsOnButtonPress" }, T("host.settings.rules.gamepadAssignment")),
        new ExclusiveRule("layout.cutscenesMuteAudioOnly", new[] { "layout.cutscenesKeepAspectRatio", "layout.cutscenesUnfocus" }, T("host.settings.rules.cutscenesMuteAudioOnly")),
    };

    /// <summary>Keyboard shortcut rows of the reminder image, in order: label (in the language in use), key.</summary>
    public static IReadOnlyList<(string Label, string Key)> KeyboardReminderRows => new[]
    {
        (T("host.settings.reminderImage.rows.closeApp"), "hotkeys.closeApp"), (T("host.settings.reminderImage.rows.stopSession"), "hotkeys.stopSession"),
        (T("host.settings.reminderImage.rows.topMost"), "hotkeys.topMost"), (T("host.settings.reminderImage.rows.setFocus"), "hotkeys.setFocus"),
        (T("host.settings.reminderImage.rows.resetWindows"), "hotkeys.resetWindows"), (T("host.settings.reminderImage.rows.cutscenesMode"), "hotkeys.cutscenesMode"),
        (T("host.settings.reminderImage.rows.switchLayout"), "hotkeys.switchLayout"), (T("host.settings.reminderImage.rows.shortcutsReminder"), "hotkeys.shortcutsReminder"),
        (T("host.settings.reminderImage.rows.switchMergerWindow"), "hotkeys.switchMergerWindow"), (T("host.settings.reminderImage.rows.lockInputs"), "hotkeys.lockInputs"),
    };

    /// <summary>Gamepad shortcut rows of the reminder image, in order: label (in the language in use), key.</summary>
    public static IReadOnlyList<(string Label, string Key)> GamepadReminderRows => new[]
    {
        (T("host.settings.reminderImage.rows.setFocus"), "gamepad.setFocus"), (T("host.settings.reminderImage.rows.closeApp"), "gamepad.close"),
        (T("host.settings.reminderImage.rows.stopSession"), "gamepad.stopSession"), (T("host.settings.reminderImage.rows.topMost"), "gamepad.topMost"),
        (T("host.settings.reminderImage.rows.resetWindows"), "gamepad.resetWindows"), (T("host.settings.reminderImage.rows.cutscenesMode"), "gamepad.cutscenesMode"),
        (T("host.settings.reminderImage.rows.switchLayout"), "gamepad.switchLayout"), (T("host.settings.reminderImage.rows.lockInputs"), "gamepad.lockInputs"),
        ("Alt+Tab", "gamepad.releaseCursor"), // key names: not translated
    };

    /// <summary>Asked before turning "Keep Nucleus user accounts" off (the legacy Settings window asked it too).</summary>
    public static SettingConfirmation KeepAccountsConfirmation => new SettingConfirmation
    {
        Values = { "false" },
        Title = T("host.settings.fields.misc.keepAccounts.confirm.title"),
        Message = T("host.settings.fields.misc.keepAccounts.confirm.message"),
        ConfirmLabel = T("host.settings.fields.misc.keepAccounts.confirm.confirmLabel"),
    };

    /// <summary>Builds the definitions, with labels and descriptions in the language in use (built again when it changes).</summary>
    public static List<SettingDefinition> Create()
    {
        var list = new List<SettingDefinition>();

        // ---- General (Settings tab) ----
        // The app's language comes first: its choices are the files of the i18n folder. Saving it switches the app at once.
        SettingDefinition language = Choice("general", LanguageKey, T("host.settings.fields.misc.appLanguage.label"), T("host.settings.fields.misc.appLanguage.description"),
            Translator.DefaultLocale, new List<SettingChoice>(), () => I18n.Locale, v => LanguageSettings.Choose(v));
        language.DynamicChoices = () => I18nService.Choices(I18n.Current);
        list.Add(language);
        // The emulator languages are what the game sees: their names stay as they are, only "Automatic" is a UI text.
        list.Add(Choice("general", "misc.steamLanguage", T("host.settings.fields.misc.steamLanguage.label"), T("host.settings.fields.misc.steamLanguage.description"), "Automatic",
            SteamLanguages.Select(l => new SettingChoice { Value = l.Value, Label = l.Value == "Automatic" ? T("host.settings.fields.misc.steamLanguage.automatic") : l.Label }).ToList(),
            () => App_Misc.SteamLang ?? "", v => App_Misc.SteamLang = v));
        list.Add(Choice("general", "misc.epicLanguage", T("host.settings.fields.misc.epicLanguage.label"), T("host.settings.fields.misc.epicLanguage.description"), "English",
            EpicLanguages.Select(l => new SettingChoice { Value = l, Label = l }).ToList(),
            () => App_Misc.EpicLang ?? "", v => App_Misc.EpicLang = v));
        SettingDefinition network = Choice("general", "misc.network", T("host.settings.fields.misc.network.label"), T("host.settings.fields.misc.network.description"), "Automatic",
            new List<SettingChoice>(), () => App_Misc.Network ?? "", v => App_Misc.Network = v);
        network.DynamicChoices = SystemChoices.NetworkAdapters;
        list.Add(network);
        list.Add(Toggle("general", "misc.useNicknames", T("host.settings.fields.misc.useNicknames.label"), T("host.settings.fields.misc.useNicknames.description"), true,
            () => App_Misc.UseNicksInGame, v => App_Misc.UseNicksInGame = v));
        SettingDefinition keepAccounts = Toggle("general", "misc.keepAccounts", T("host.settings.fields.misc.keepAccounts.label"),
            T("host.settings.fields.misc.keepAccounts.description"), false,
            () => App_Misc.KeepAccounts, v => App_Misc.KeepAccounts = v);
        keepAccounts.Field.Confirm = KeepAccountsConfirmation;
        list.Add(keepAccounts);
        SettingDefinition password = Password("general", "misc.nucleusAccountPassword", T("host.settings.fields.misc.nucleusAccountPassword.label"),
            T("host.settings.fields.misc.nucleusAccountPassword.description"), "12345", 127,
            () => App_Misc.NucleusAccountPassword ?? "", v => App_Misc.NucleusAccountPassword = v);
        list.Add(password);
        list.Add(Toggle("general", "misc.autoDesktopScaling", T("host.settings.fields.misc.autoDesktopScaling.label"), T("host.settings.fields.misc.autoDesktopScaling.description"), false,
            () => App_Misc.AutoDesktopScaling, v => App_Misc.AutoDesktopScaling = v));
        list.Add(Toggle("general", "misc.fastLaunch", T("host.settings.fields.misc.fastLaunch.label"),
            T("host.settings.fields.misc.fastLaunch.description"), true,
            () => App_Misc.FastLaunch, v => App_Misc.FastLaunch = v));
        list.Add(Toggle("general", "misc.ignoreInputLockReminder", T("host.settings.fields.misc.ignoreInputLockReminder.label"), T("host.settings.fields.misc.ignoreInputLockReminder.description"), true,
            () => App_Misc.IgnoreInputLockReminder, v => App_Misc.IgnoreInputLockReminder = v));
        list.Add(Toggle("general", "misc.debugLog", T("host.settings.fields.misc.debugLog.label"), T("host.settings.fields.misc.debugLog.description"), true,
            () => App_Misc.DebugLog, v => App_Misc.DebugLog = v));
        SettingDefinition profiles = Toggle("general", "misc.disableGameProfiles", T("host.settings.fields.misc.disableGameProfiles.label"), T("host.settings.fields.misc.disableGameProfiles.description"), false,
            () => App_Misc.DisableGameProfiles, v => App_Misc.DisableGameProfiles = v);
        profiles.Effects = SettingEffects.GameProfiles;
        list.Add(profiles);

        // ---- Players tab options ----
        list.Add(Toggle("players", "players.assignGamepadsByIndex", T("host.settings.fields.players.assignGamepadsByIndex.label"),
            T("host.settings.fields.players.assignGamepadsByIndex.description"), false,
            () => App_Misc.UseXinputIndex, v => App_Misc.UseXinputIndex = v));
        list.Add(Toggle("players", "players.assignGamepadsOnButtonPress", T("host.settings.fields.players.assignGamepadsOnButtonPress.label"),
            T("host.settings.fields.players.assignGamepadsOnButtonPress.description"), false,
            () => App_Misc.ProfileAssignGamepadByButonPress, v => App_Misc.ProfileAssignGamepadByButonPress = v));

        // ---- Keyboard shortcuts ----
        list.Add(Hotkey("hotkeys.closeApp", T("host.settings.fields.hotkeys.closeApp.label"), "Ctrl+Q", () => App_Hotkeys.CloseApp, v => App_Hotkeys.CloseApp = v));
        list.Add(Hotkey("hotkeys.stopSession", T("host.settings.fields.hotkeys.stopSession.label"), "Ctrl+E", () => App_Hotkeys.StopSession, v => App_Hotkeys.StopSession = v));
        list.Add(Hotkey("hotkeys.topMost", T("host.settings.fields.hotkeys.topMost.label"), "Ctrl+T", () => App_Hotkeys.TopMost, v => App_Hotkeys.TopMost = v));
        list.Add(Hotkey("hotkeys.setFocus", T("host.settings.fields.hotkeys.setFocus.label"), "Ctrl+H", () => App_Hotkeys.SetFocus, v => App_Hotkeys.SetFocus = v));
        list.Add(Hotkey("hotkeys.resetWindows", T("host.settings.fields.hotkeys.resetWindows.label"), "Ctrl+R", () => App_Hotkeys.ResetWindows, v => App_Hotkeys.ResetWindows = v));
        list.Add(Hotkey("hotkeys.cutscenesMode", T("host.settings.fields.hotkeys.cutscenesMode.label"), "Ctrl+M", () => App_Hotkeys.CutscenesMode, v => App_Hotkeys.CutscenesMode = v));
        list.Add(Hotkey("hotkeys.switchLayout", T("host.settings.fields.hotkeys.switchLayout.label"), "Ctrl+N", () => App_Hotkeys.SwitchLayout, v => App_Hotkeys.SwitchLayout = v));
        list.Add(Hotkey("hotkeys.shortcutsReminder", T("host.settings.fields.hotkeys.shortcutsReminder.label"), "Alt+R", () => App_Hotkeys.ShortcutsReminder, v => App_Hotkeys.ShortcutsReminder = v));
        SettingDefinition merger = Hotkey("hotkeys.switchMergerWindow", T("host.settings.fields.hotkeys.switchMergerWindow.label"), "Alt+A", () => App_Hotkeys.SwitchMergerForeGroundChild, v => App_Hotkeys.SwitchMergerForeGroundChild = v);
        merger.Field.Description = T("host.settings.fields.hotkeys.switchMergerWindow.description");
        list.Add(merger);
        // Key names ("End", "F1", "Numpad 0") are key caps: not translated.
        SettingDefinition lockKey = Choice("hotkeys", "hotkeys.lockInputs", T("host.settings.fields.hotkeys.lockInputs.label"), T("host.settings.fields.hotkeys.lockInputs.description"), "End",
            LockKeys.Select(k => new SettingChoice { Value = k, Label = k }).ToList(), () => App_Hotkeys.LockInputs ?? "", v => App_Hotkeys.LockInputs = v);
        lockKey.Effects = SettingEffects.KeyboardReminder;
        list.Add(lockKey);

        // ---- Gamepad shortcuts (XInputShortcutsSetup) ----
        list.Add(Pad("gamepad.setFocus", T("host.settings.fields.gamepad.setFocus.label"), "1024+32768", () => App_GamePadShortcuts.SetFocus, v => App_GamePadShortcuts.SetFocus = v));
        list.Add(Pad("gamepad.close", T("host.settings.fields.gamepad.close.label"), "1024+16", () => App_GamePadShortcuts.Close, v => App_GamePadShortcuts.Close = v));
        list.Add(Pad("gamepad.stopSession", T("host.settings.fields.gamepad.stopSession.label"), "1024+32", () => App_GamePadShortcuts.StopSession, v => App_GamePadShortcuts.StopSession = v));
        list.Add(Pad("gamepad.topMost", T("host.settings.fields.gamepad.topMost.label"), "1024+16384", () => App_GamePadShortcuts.TopMost, v => App_GamePadShortcuts.TopMost = v));
        list.Add(Pad("gamepad.resetWindows", T("host.settings.fields.gamepad.resetWindows.label"), "1024+8", () => App_GamePadShortcuts.ResetWindows, v => App_GamePadShortcuts.ResetWindows = v));
        list.Add(Pad("gamepad.cutscenesMode", T("host.settings.fields.gamepad.cutscenesMode.label"), "1024+8192", () => App_GamePadShortcuts.CutscenesMode, v => App_GamePadShortcuts.CutscenesMode = v));
        list.Add(Pad("gamepad.switchLayout", T("host.settings.fields.gamepad.switchLayout.label"), "1024+512", () => App_GamePadShortcuts.SwitchLayout, v => App_GamePadShortcuts.SwitchLayout = v));
        list.Add(Pad("gamepad.lockInputs", T("host.settings.fields.gamepad.lockInputs.label"), "1024+128", () => App_GamePadShortcuts.LockInputs, v => App_GamePadShortcuts.LockInputs = v));
        list.Add(Pad("gamepad.releaseCursor", "Alt+Tab", "1024+256", () => App_GamePadShortcuts.ReleaseCursor, v => App_GamePadShortcuts.ReleaseCursor = v,
            T("host.settings.fields.gamepad.releaseCursor.description")));

        // The legacy "Gamepad control of the Nucleus window" (mouse cursor on the left stick, its click buttons, dead zone,
        // "Lock UI control" and "On-screen keyboard" combos) is gone: it fought the web UI's own gamepad navigation.
        SettingDefinition padType = Choice("gamepad", "gamepad.type", T("host.settings.fields.gamepad.type.label"), T("host.settings.fields.gamepad.type.description"), "Xbox",
            new List<SettingChoice> { new SettingChoice { Value = "Xbox", Label = "Xbox" }, new SettingChoice { Value = "Playstation", Label = "PlayStation" } },
            () => App_GamePadNavigation.Type ?? "", v => App_GamePadNavigation.Type = v);
        padType.Effects = SettingEffects.GamepadReminder;
        list.Add(padType);

        // ---- Audio ----
        list.Add(Choice("audio", "audio.mode", T("host.settings.fields.audio.mode.label"), null, "1",
            new List<SettingChoice>
            {
                new SettingChoice { Value = "0", Label = T("host.settings.fields.audio.mode.choices.sameDevice") },
                new SettingChoice { Value = "1", Label = T("host.settings.fields.audio.mode.choices.perInstance") },
            },
            () => App_Audio.Custom ?? "", v => App_Audio.Custom = v));
        for (int i = 1; i <= 8; i++)
        {
            string iniKey = "AudioInstance" + i;
            SettingDefinition instance = Choice("audio", "audio.instance" + i, T("host.settings.fields.audio.instance.label", new { number = i }),
                i == 1 ? T("host.settings.fields.audio.instance.description") : null, "Default",
                new List<SettingChoice>(),
                () => App_Audio.Instances_AudioOutput.TryGetValue(iniKey, out string? device) && !string.IsNullOrEmpty(device) ? device : "Default",
                v => App_Audio.SaveAudioOutput(iniKey, v));
            instance.DynamicChoices = SystemChoices.AudioOutputs;
            list.Add(instance);
        }

        // ---- Layout ----
        SettingDefinition vertical = Number("layout", "layout.verticalLines", T("host.settings.fields.layout.verticalLines.label"), T("host.settings.fields.layout.verticalLines.description"), "1", 0, 5, 1,
            () => App_Layouts.VerticalLines, v => App_Layouts.VerticalLines = v);
        vertical.Effects = SettingEffects.LayoutMaxPlayers;
        list.Add(vertical);
        SettingDefinition horizontal = Number("layout", "layout.horizontalLines", T("host.settings.fields.layout.horizontalLines.label"), T("host.settings.fields.layout.horizontalLines.description"), "1", 0, 5, 1,
            () => App_Layouts.HorizontalLines, v => App_Layouts.HorizontalLines = v);
        horizontal.Effects = SettingEffects.LayoutMaxPlayers;
        list.Add(horizontal);
        list.Add(Toggle("layout", "layout.splitDiv", T("host.settings.fields.layout.splitDiv.label"), T("host.settings.fields.layout.splitDiv.description"), false,
            () => App_Layouts.SplitDiv, v => App_Layouts.SplitDiv = v));
        list.Add(Choice("layout", "layout.splitDivColor", T("host.settings.fields.layout.splitDivColor.label"), null, "Black",
            SplitColors.Select(c => new SettingChoice { Value = c, Label = SplitColorLabel(c) }).ToList(), () => App_Layouts.SplitDivColor ?? "", v => App_Layouts.SplitDivColor = v));
        list.Add(Toggle("layout", "layout.hideDesktopOnly", T("host.settings.fields.layout.hideDesktopOnly.label"), T("host.settings.fields.layout.hideDesktopOnly.description"), false,
            () => App_Layouts.HideOnly, v => App_Layouts.HideOnly = v));
        list.Add(Toggle("layout", "layout.cutscenesMuteAudioOnly", T("host.settings.fields.layout.cutscenesMuteAudioOnly.label"), T("host.settings.fields.layout.cutscenesMuteAudioOnly.description"), false,
            () => App_Layouts.Cts_MuteAudioOnly, v => App_Layouts.Cts_MuteAudioOnly = v));
        list.Add(Toggle("layout", "layout.cutscenesKeepAspectRatio", T("host.settings.fields.layout.cutscenesKeepAspectRatio.label"), null, false,
            () => App_Layouts.Cts_KeepAspectRatio, v => App_Layouts.Cts_KeepAspectRatio = v));
        list.Add(Toggle("layout", "layout.cutscenesUnfocus", T("host.settings.fields.layout.cutscenesUnfocus.label"), null, false,
            () => App_Layouts.Cts_Unfocus, v => App_Layouts.Cts_Unfocus = v));
        list.Add(Toggle("layout", "layout.windowsMerger", T("host.settings.fields.layout.windowsMerger.label"), T("host.settings.fields.layout.windowsMerger.description"), false,
            () => App_Layouts.WindowsMerger, v => App_Layouts.WindowsMerger = v));
        list.Add(Toggle("layout", "layout.losslessHook", T("host.settings.fields.layout.losslessHook.label"), T("host.settings.fields.layout.losslessHook.description"), true,
            () => App_Layouts.LosslessHook, v => App_Layouts.LosslessHook = v));
        SettingDefinition mergerRes = Choice("layout", "layout.mergerResolution", T("host.settings.fields.layout.mergerResolution.label"), T("host.settings.fields.layout.mergerResolution.description"), "",
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
