using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// App settings, described by a schema so the UI renders them generically: adding a setting only takes C#.
    /// Values travel as strings (as stored in Settings.ini); the field type says how to edit and parse them.
    /// </summary>
    [BridgeService("settings")]
    public interface ISettingsService
    {
        /// <summary>Sections and fields.</summary>
        SettingsSchema GetSchema();

        /// <summary>Current values, by field key.</summary>
        Dictionary<string, string> GetValues();

        /// <summary>Saves values. Returns the invalid fields; nothing is saved when the list is not empty.</summary>
        List<ValidationError> SetValues(SetSettingsRequest request);

        /// <summary>Global player nicknames and Steam IDs.</summary>
        PlayerIdentities GetPlayers();

        /// <summary>Saves global player nicknames and Steam IDs. Returns the invalid fields; nothing is saved when the list is not empty.</summary>
        List<ValidationError> SetPlayers(PlayerIdentities players);

        /// <summary>Waits for the user to press a gamepad button combination and returns it (for gamepad shortcut fields). Cancel to stop waiting.</summary>
        Task<string> CaptureGamepadCombo(CaptureComboRequest request, CancellationToken cancellationToken);
    }

    /// <summary>Parameters of <see cref="ISettingsService.CaptureGamepadCombo"/>.</summary>
    public sealed class CaptureComboRequest
    {
        /// <summary>Number of buttons to capture, 1 or 2 (see <see cref="SettingField.ComboButtons"/>).</summary>
        public int Buttons { get; set; } = 2;
    }

    /// <summary>Shows or enables a field only when another field has a given value.</summary>
    public sealed class SettingCondition
    {
        /// <summary>Key of the other field.</summary>
        public string Key { get; set; } = "";

        /// <summary>Values of the other field for which the condition holds.</summary>
        public List<string> Values { get; set; } = new List<string>();
    }

    /// <summary>How a setting is edited.</summary>
    public enum SettingType
    {
        /// <summary>"true" or "false".</summary>
        Toggle,
        /// <summary>A number between <see cref="SettingField.Min"/> and <see cref="SettingField.Max"/>.</summary>
        Number,
        /// <summary>Free text.</summary>
        Text,
        /// <summary>Free text shown masked.</summary>
        Password,
        /// <summary>One of <see cref="SettingField.Choices"/>.</summary>
        Choice,
        /// <summary>A keyboard shortcut, for example "Ctrl+Alt+K".</summary>
        Hotkey,
        /// <summary>A gamepad button combination, captured with <see cref="ISettingsService.CaptureGamepadCombo"/>.</summary>
        GamepadCombo,
        /// <summary>A file path (picked with <c>dialogs.pickFile</c>).</summary>
        FilePath,
        /// <summary>A folder path (picked with <c>dialogs.pickFolder</c>).</summary>
        FolderPath,
    }

    /// <summary>One setting.</summary>
    public sealed class SettingField
    {
        /// <summary>Key, unique across sections, for example <c>misc.useNicknames</c>.</summary>
        public string Key { get; set; } = "";

        /// <summary>Label.</summary>
        public string Label { get; set; } = "";

        /// <summary>Help text.</summary>
        public string? Description { get; set; }

        /// <summary>How it is edited.</summary>
        public SettingType Type { get; set; }

        /// <summary>Allowed values (choice fields).</summary>
        public List<SettingChoice> Choices { get; set; } = new List<SettingChoice>();

        /// <summary>Minimum (number fields).</summary>
        public double? Min { get; set; }

        /// <summary>Maximum (number fields).</summary>
        public double? Max { get; set; }

        /// <summary>Increment (number fields).</summary>
        public double? Step { get; set; }

        /// <summary>Unit shown after the value, for example "ms".</summary>
        public string? Unit { get; set; }

        /// <summary>Default value.</summary>
        public string? Default { get; set; }

        /// <summary>The change applies after restarting Nucleus.</summary>
        public bool RequiresRestart { get; set; }

        /// <summary>Maximum length (text and password fields).</summary>
        public int? MaxLength { get; set; }

        /// <summary>Buttons in the combination (gamepad combo fields): 1 or 2.</summary>
        public int? ComboButtons { get; set; }

        /// <summary>Shown only when this condition holds.</summary>
        public SettingCondition? VisibleWhen { get; set; }

        /// <summary>Editable only when this condition holds.</summary>
        public SettingCondition? EnabledWhen { get; set; }

        /// <summary>Ask the user to confirm before saving one of these values (for example turning off "Keep Nucleus user accounts").</summary>
        public SettingConfirmation? Confirm { get; set; }
    }

    /// <summary>A confirmation the UI asks before saving some values of a setting.</summary>
    public sealed class SettingConfirmation
    {
        /// <summary>Values that need the confirmation.</summary>
        public List<string> Values { get; set; } = new List<string>();

        /// <summary>Title of the confirmation.</summary>
        public string Title { get; set; } = "";

        /// <summary>What will happen, in plain words.</summary>
        public string Message { get; set; } = "";

        /// <summary>Label of the confirming button, a verb, for example "Turn off".</summary>
        public string ConfirmLabel { get; set; } = "";
    }

    /// <summary>A group of settings.</summary>
    public sealed class SettingsSection
    {
        /// <summary>Id, for example <c>general</c>, <c>hotkeys</c>, <c>audio</c>, <c>layout</c>.</summary>
        public string Id { get; set; } = "";

        /// <summary>Title.</summary>
        public string Title { get; set; } = "";

        /// <summary>Short explanation.</summary>
        public string? Description { get; set; }

        /// <summary>Fields, in order.</summary>
        public List<SettingField> Fields { get; set; } = new List<SettingField>();
    }

    /// <summary>All settings.</summary>
    public sealed class SettingsSchema
    {
        /// <summary>Sections, in order.</summary>
        public List<SettingsSection> Sections { get; set; } = new List<SettingsSection>();

        /// <summary>Gamepad buttons: value as stored in a combo (for example "1024") and label (for example "Guide"). Combos are stored as values joined by "+".</summary>
        public List<SettingChoice> GamepadButtons { get; set; } = new List<SettingChoice>();
    }

    /// <summary>Parameters of <see cref="ISettingsService.SetValues"/>.</summary>
    public sealed class SetSettingsRequest
    {
        /// <summary>Values to save, by field key. Keys not listed keep their value.</summary>
        public Dictionary<string, string> Values { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>Nickname and Steam ID of one player slot.</summary>
    public sealed class PlayerIdentity
    {
        /// <summary>Player number, 1-based.</summary>
        public int Number { get; set; }

        /// <summary>Nickname. Empty means the default.</summary>
        public string Nickname { get; set; } = "";

        /// <summary>Steam ID. Empty means the default.</summary>
        public string SteamId { get; set; } = "";
    }

    /// <summary>Global player nicknames and Steam IDs.</summary>
    public sealed class PlayerIdentities
    {
        /// <summary>Player slots, in order.</summary>
        public List<PlayerIdentity> Players { get; set; } = new List<PlayerIdentity>();

        /// <summary>Nicknames used before, offered as suggestions (read-only).</summary>
        public List<string> NicknameHistory { get; set; } = new List<string>();
    }

    /// <summary>Settings were saved.</summary>
    [BridgeEvent("settings.changed", Invalidates = new[] { "settings.getValues", "settings.getPlayers" })]
    public sealed class SettingsChanged
    {
        /// <summary>Keys that changed.</summary>
        public List<string> Keys { get; set; } = new List<string>();
    }
}
