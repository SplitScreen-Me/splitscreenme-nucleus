using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;

namespace Nucleus.Coop.Services.Settings;

/// <summary>
/// <c>settings.*</c>: the legacy Settings form and gamepad shortcuts window as a schema. Values map onto the App_*
/// settings classes, whose setters write Settings.ini, so both UIs share the same file. A save is validated
/// completely before anything is written.
/// </summary>
public sealed class SettingsService : ISettingsService
{
    private readonly NucleusCore core;
    private readonly List<SettingDefinition> catalog;
    private readonly Dictionary<string, SettingDefinition> byKey;
    private readonly IGamepadReader gamepads;
    private readonly object reminderGate = new object();

    /// <summary>Creates the service and draws the shortcut reminder images when they are missing.</summary>
    public SettingsService(NucleusCore core, IGamepadReader? gamepads = null)
    {
        this.core = core;
        this.gamepads = gamepads ?? new XInputGamepadReader();
        catalog = SettingsCatalog.Create();
        byKey = catalog.ToDictionary(d => d.Field.Key, StringComparer.Ordinal);

        bool keyboardMissing = !File.Exists(KeyboardReminderPath);
        bool gamepadMissing = !File.Exists(GamepadReminderPath);
        if (keyboardMissing || gamepadMissing)
        {
            RenderRemindersInBackground(keyboardMissing, gamepadMissing);
        }
    }

    private static string KeyboardReminderPath => Path.Combine(NucleusPaths.Shortcuts, "KbShortcutsReminder.jpeg");

    private static string GamepadReminderPath => Path.Combine(NucleusPaths.Shortcuts, "XShortcutsReminder.jpeg");

    /// <inheritdoc />
    public SettingsSchema GetSchema()
    {
        var schema = new SettingsSchema();
        foreach (SettingSectionDefinition section in SettingsCatalog.Sections)
        {
            var dto = new SettingsSection { Id = section.Id, Title = section.Title, Description = section.Description };
            if (section.Id == "audio")
            {
                string? device = SystemChoices.DefaultAudioOutput();
                if (device != null)
                {
                    dto.Description += $" Default playback device: {device}.";
                }
            }

            foreach (SettingDefinition definition in catalog.Where(d => d.Section == section.Id))
            {
                SettingField field = Copy(definition.Field);
                if (definition.DynamicChoices != null)
                {
                    field.Choices = definition.DynamicChoices();
                    AddCurrentChoice(field, SafeGet(definition));
                }

                dto.Fields.Add(field);
            }

            schema.Sections.Add(dto);
        }

        // Button names follow the gamepad type setting (Xbox or PlayStation).
        string type = App_GamePadNavigation.Type ?? "Xbox";
        schema.GamepadButtons = GamepadComboFormat.Buttons
            .Select(b => new SettingChoice { Value = b.Code.ToString(System.Globalization.CultureInfo.InvariantCulture), Label = GamepadComboFormat.Name(b.Code, type) })
            .ToList();
        return schema;
    }

    /// <inheritdoc />
    public Dictionary<string, string> GetValues() => catalog.ToDictionary(d => d.Field.Key, SafeGet, StringComparer.Ordinal);

    /// <inheritdoc />
    public List<ValidationError> SetValues(SetSettingsRequest request)
    {
        if (request?.Values == null)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "values are required.");
        }

        Dictionary<string, string> current = GetValues();
        List<SettingRule> rules = catalog.Select(d => RuleFor(d, request.Values.ContainsKey(d.Field.Key))).ToList();

        List<ValidationError> errors = SettingsValidator.Validate(request.Values, current, rules, SettingsCatalog.ExclusiveRules);
        if (errors.Count > 0)
        {
            return errors;
        }

        // Write only what changed, in catalog order.
        var changed = new List<SettingDefinition>();
        foreach (SettingDefinition definition in catalog)
        {
            if (!request.Values.TryGetValue(definition.Field.Key, out string? raw))
            {
                continue;
            }

            current.TryGetValue(definition.Field.Key, out string? before);
            string normalized = SettingsValidator.Normalize(rules.First(r => r.Key == definition.Field.Key), raw, before, out _)!;
            if (normalized == before)
            {
                continue;
            }

            definition.Set(normalized);
            changed.Add(definition);
        }

        if (changed.Count == 0)
        {
            return errors;
        }

        ApplyEffects(changed.Aggregate(SettingEffects.None, (effects, d) => effects | d.Effects));
        ServiceLog.Info("settings saved: " + string.Join(", ", changed.Select(d => d.Field.Key)));
        core.Events.Emit(new SettingsChanged { Keys = changed.Select(d => d.Field.Key).ToList() });
        return errors;
    }

    /// <inheritdoc />
    public PlayerIdentities GetPlayers()
    {
        var players = CurrentPlayers();
        var history = PlayersIdentityCache.NicknamesBackup
            .Concat(players.Select(p => p.Nickname))
            .Where(n => !string.IsNullOrWhiteSpace(n) && !IsDefaultNickname(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(n => n, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        return new PlayerIdentities { Players = players, NicknameHistory = history };
    }

    /// <inheritdoc />
    public List<ValidationError> SetPlayers(PlayerIdentities players)
    {
        if (players?.Players == null)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "players are required.");
        }

        (List<ValidationError> errors, List<PlayerIdentity> result) = PlayerIdentitiesRules.Validate(players.Players, CurrentPlayers());
        if (errors.Count > 0)
        {
            return errors;
        }

        Directory.CreateDirectory(Globals.GameProfilesFolder);
        foreach (PlayerIdentity player in result)
        {
            int index = player.Number - 1;
            if (PlayersIdentityCache.GetNicknameAt(index) != player.Nickname)
            {
                PlayersIdentityCache.SetNicknameAt(index, player.Nickname);
            }

            string storedSteamId = index < PlayersIdentityCache.PlayersSteamId.Count ? PlayersIdentityCache.PlayersSteamId[index] : "";
            if (storedSteamId != player.SteamId)
            {
                PlayersIdentityCache.SetSteamIdAt(index, player.SteamId);
            }
        }

        PlayersIdentityCache.BackupNicknames();
        PlayersIdentityCache.BackupSteamIds();
        ServiceLog.Info("player nicknames and Steam IDs saved");
        core.Events.Emit(new SettingsChanged { Keys = new List<string> { "players" } });
        return errors;
    }

    /// <inheritdoc />
    public async Task<string> CaptureGamepadCombo(CaptureComboRequest request, CancellationToken cancellationToken)
    {
        int buttons = request?.Buttons ?? 2;
        if (buttons != 1 && buttons != 2)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "buttons must be 1 or 2.");
        }

        if (!GamepadComboCapture.AnyConnected(gamepads))
        {
            throw new BridgeException(BridgeErrorKind.Unavailable, "No XInput gamepad is connected.", new { reason = "noGamepad" });
        }

        try
        {
            return await Task.Run(() => GamepadComboCapture.Capture(gamepads, buttons, cancellationToken), cancellationToken);
        }
        catch (TimeoutException)
        {
            throw new BridgeException(BridgeErrorKind.Unavailable, "No gamepad button was pressed.", new { reason = "timeout" });
        }
    }

    /// <summary>Draws the reminder images from the current values (also after the legacy UI saved settings).</summary>
    public void RenderReminders(bool keyboard, bool gamepad)
    {
        Dictionary<string, string> values = GetValues();
        string type = values.TryGetValue("gamepad.type", out string? t) && t.Length > 0 ? t : "Xbox";

        lock (reminderGate)
        {
            try
            {
                if (keyboard)
                {
                    var rows = SettingsCatalog.KeyboardReminderRows.Select(r => (r.Label, values.TryGetValue(r.Key, out string? v) ? v : "")).ToList();
                    ShortcutsReminderRenderer.RenderKeyboard(KeyboardReminderPath, rows);
                }

                if (gamepad)
                {
                    var rows = SettingsCatalog.GamepadReminderRows
                        .Select(r => (r.Label, GamepadComboFormat.Parse(values.TryGetValue(r.Key, out string? v) ? v : "", 2, out _) ?? new int[0]))
                        .ToList();
                    string images = Path.Combine(NucleusPaths.GamepadImages, type);
                    ShortcutsReminderRenderer.RenderGamepad(GamepadReminderPath, rows, type, images);
                }
            }
            catch (Exception e)
            {
                // The reminder window keeps the image open while it is shown during a session.
                ServiceLog.Warn("shortcut reminder images could not be drawn", e);
            }
        }
    }

    private static bool IsDefaultNickname(string nickname) =>
        Enumerable.Range(1, PlayerIdentitiesRules.MaxPlayers).Any(n => string.Equals(nickname, PlayerIdentitiesRules.DefaultNickname(n), StringComparison.OrdinalIgnoreCase));

    private static List<PlayerIdentity> CurrentPlayers() =>
        Enumerable.Range(1, PlayerIdentitiesRules.MaxPlayers).Select(n =>
        {
            int index = n - 1;
            string nickname = PlayersIdentityCache.GetNicknameAt(index) ?? "";
            string steamId = index < PlayersIdentityCache.PlayersSteamId.Count ? PlayersIdentityCache.PlayersSteamId[index] ?? "" : "";
            return new PlayerIdentity
            {
                Number = n,
                Nickname = nickname == PlayerIdentitiesRules.DefaultNickname(n) ? "" : nickname,
                SteamId = steamId == PlayerIdentitiesRules.DefaultSteamId(n) ? "" : steamId,
            };
        }).ToList();

    private static SettingField Copy(SettingField field) => new SettingField
    {
        Key = field.Key,
        Label = field.Label,
        Description = field.Description,
        Type = field.Type,
        Choices = field.Choices.Select(c => new SettingChoice { Value = c.Value, Label = c.Label }).ToList(),
        Min = field.Min,
        Max = field.Max,
        Step = field.Step,
        Unit = field.Unit,
        Default = field.Default,
        RequiresRestart = field.RequiresRestart,
        MaxLength = field.MaxLength,
        ComboButtons = field.ComboButtons,
        VisibleWhen = Copy(field.VisibleWhen),
        EnabledWhen = Copy(field.EnabledWhen),
        Confirm = field.Confirm == null ? null : new SettingConfirmation
        {
            Values = field.Confirm.Values.ToList(),
            Title = field.Confirm.Title,
            Message = field.Confirm.Message,
            ConfirmLabel = field.Confirm.ConfirmLabel,
        },
    };

    private static SettingCondition? Copy(SettingCondition? condition) =>
        condition == null ? null : new SettingCondition { Key = condition.Key, Values = condition.Values.ToList() };

    // A stored value the PC no longer offers (unplugged audio device, adapter down) stays selectable.
    private static void AddCurrentChoice(SettingField field, string current)
    {
        if (!string.IsNullOrEmpty(current) && field.Choices.All(c => c.Value != current))
        {
            field.Choices.Add(new SettingChoice { Value = current, Label = current + " (not available)" });
        }
    }

    private static string SafeGet(SettingDefinition definition)
    {
        try
        {
            return definition.Get() ?? "";
        }
        catch (Exception e)
        {
            ServiceLog.Warn($"setting {definition.Field.Key} could not be read", e);
            return "";
        }
    }

    // Dynamic choices are only listed for the fields being saved (enumerating devices takes time).
    private static SettingRule RuleFor(SettingDefinition definition, bool beingSaved)
    {
        if (definition.DynamicChoices == null || !beingSaved)
        {
            return definition.Rule;
        }

        return new SettingRule
        {
            Key = definition.Rule.Key,
            Label = definition.Rule.Label,
            Type = definition.Rule.Type,
            AllowedValues = definition.DynamicChoices().Select(c => c.Value).ToList(),
        };
    }

    private void ApplyEffects(SettingEffects effects)
    {
        if (effects.HasFlag(SettingEffects.LayoutMaxPlayers))
        {
            App_Layouts.MaxPlayers = (App_Layouts.HorizontalLines + 1) * (App_Layouts.VerticalLines + 1);
        }

        if (effects.HasFlag(SettingEffects.GamepadShortcuts))
        {
            TryRun("GamepadShortcuts.UpdateShortcutsValue", GamepadShortcuts.UpdateShortcutsValue);
        }

        if (effects.HasFlag(SettingEffects.GamepadNavigation))
        {
            TryRun("GamepadNavigation.UpdateUINavSettings", GamepadNavigation.UpdateUINavSettings);
        }

        if (effects.HasFlag(SettingEffects.GameProfiles))
        {
            core.Notifier.Everything();
            LibraryHooks.RaiseGameProfilesSettingChanged(App_Misc.DisableGameProfiles);
        }

        bool keyboard = effects.HasFlag(SettingEffects.KeyboardReminder);
        bool gamepad = effects.HasFlag(SettingEffects.GamepadReminder);
        if (keyboard || gamepad)
        {
            RenderRemindersInBackground(keyboard, gamepad);
        }
    }

    private void RenderRemindersInBackground(bool keyboard, bool gamepad) =>
        Task.Run(() => RenderReminders(keyboard, gamepad));

    private static void TryRun(string name, Action action)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            ServiceLog.Warn(name + " failed", e);
        }
    }
}
