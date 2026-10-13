using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Settings;

/// <summary>How one setting is validated and normalized.</summary>
public sealed class SettingRule
{
    /// <summary>Field key.</summary>
    public string Key { get; set; } = "";

    /// <summary>Label, used in messages.</summary>
    public string Label { get; set; } = "";

    /// <summary>Editor type.</summary>
    public SettingType Type { get; set; }

    /// <summary>Minimum of a number.</summary>
    public double? Min { get; set; }

    /// <summary>Maximum of a number.</summary>
    public double? Max { get; set; }

    /// <summary>Allowed values of a choice (the current value is always accepted, for example an unplugged device).</summary>
    public IReadOnlyCollection<string>? AllowedValues { get; set; }

    /// <summary>Maximum length of a text.</summary>
    public int? MaxLength { get; set; }

    /// <summary>Buttons in a gamepad combination: 2 for shortcuts, 1 for UI navigation clicks.</summary>
    public int ComboSize { get; set; } = 2;

    /// <summary>Fields of the same group must not have the same value (shortcuts).</summary>
    public string? UniqueGroup { get; set; }
}

/// <summary>Two toggles that can't both be on.</summary>
public sealed class ExclusiveRule
{
    /// <summary>Creates the rule.</summary>
    public ExclusiveRule(string first, IReadOnlyList<string> others, string message)
    {
        First = first;
        Others = others;
        Message = message;
    }

    /// <summary>The toggle that excludes the others.</summary>
    public string First { get; }

    /// <summary>Toggles that must be off when <see cref="First"/> is on.</summary>
    public IReadOnlyList<string> Others { get; }

    /// <summary>Error message.</summary>
    public string Message { get; }
}

/// <summary>
/// Validates a settings save before anything is written (the legacy Settings form wrote nicknames and audio
/// devices before its hotkey checks could fail). Pure: works on value dictionaries.
/// </summary>
public static class SettingsValidator
{
    /// <summary>
    /// Validates <paramref name="changes"/> against the rules, merged with <paramref name="current"/> values for the
    /// rules between fields. Problems that already exist between unchanged fields are not reported.
    /// </summary>
    public static List<ValidationError> Validate(
        IReadOnlyDictionary<string, string> changes,
        IReadOnlyDictionary<string, string> current,
        IReadOnlyList<SettingRule> rules,
        IReadOnlyList<ExclusiveRule> exclusive)
    {
        var errors = new List<ValidationError>();
        var byKey = rules.ToDictionary(r => r.Key, StringComparer.Ordinal);
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> pair in current)
        {
            merged[pair.Key] = pair.Value;
        }

        var changed = new HashSet<string>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, string> change in changes)
        {
            if (!byKey.TryGetValue(change.Key, out SettingRule? rule))
            {
                errors.Add(Error(change.Key, I18n.T("host.settings.errors.unknownSetting")));
                continue;
            }

            current.TryGetValue(change.Key, out string? currentValue);
            string? normalized = Normalize(rule, change.Value, currentValue, out string? error);
            if (normalized == null)
            {
                errors.Add(Error(change.Key, error ?? I18n.T("host.settings.errors.invalidValue")));
                continue;
            }

            merged[change.Key] = normalized;
            changed.Add(change.Key);
        }

        // Duplicate shortcuts. Gamepad combinations compare like the runtime does: by the sum of their codes.
        foreach (IGrouping<string, SettingRule> group in rules.Where(r => r.UniqueGroup != null).GroupBy(r => r.UniqueGroup!))
        {
            var seen = new Dictionary<string, SettingRule>(StringComparer.OrdinalIgnoreCase);
            foreach (SettingRule rule in group)
            {
                if (!merged.TryGetValue(rule.Key, out string? value) || string.IsNullOrEmpty(value))
                {
                    continue;
                }

                string identity = Identity(rule, value);
                if (seen.TryGetValue(identity, out SettingRule? other))
                {
                    if (changed.Contains(rule.Key) || changed.Contains(other.Key))
                    {
                        string key = changed.Contains(rule.Key) ? rule.Key : other.Key;
                        string otherLabel = key == rule.Key ? other.Label : rule.Label;
                        errors.Add(Error(key, I18n.T("host.settings.errors.alreadyUsed", new { label = otherLabel })));
                    }
                }
                else
                {
                    seen[identity] = rule;
                }
            }
        }

        foreach (ExclusiveRule rule in exclusive)
        {
            bool involved = changed.Contains(rule.First) || rule.Others.Any(changed.Contains);
            if (involved && IsOn(merged, rule.First) && rule.Others.Any(o => IsOn(merged, o)))
            {
                string key = changed.Contains(rule.First) ? rule.First : rule.Others.First(changed.Contains);
                errors.Add(Error(key, rule.Message));
            }
        }

        return errors;
    }

    /// <summary>The stored form of a valid value, or null with the reason.</summary>
    public static string? Normalize(SettingRule rule, string? value, string? currentValue, out string? error)
    {
        error = null;
        value ??= "";
        switch (rule.Type)
        {
            case SettingType.Toggle:
                if (bool.TryParse(value.Trim(), out bool flag))
                {
                    return flag ? "true" : "false";
                }

                error = I18n.T("host.settings.errors.toggle");
                return null;

            case SettingType.Number:
                if (!int.TryParse(value.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
                {
                    error = I18n.T("host.settings.errors.wholeNumber");
                    return null;
                }

                if ((rule.Min.HasValue && number < rule.Min.Value) || (rule.Max.HasValue && number > rule.Max.Value))
                {
                    error = I18n.T("host.settings.errors.numberRange", new { min = rule.Min ?? int.MinValue, max = rule.Max ?? int.MaxValue });
                    return null;
                }

                return number.ToString(CultureInfo.InvariantCulture);

            case SettingType.Choice:
                if ((rule.AllowedValues != null && rule.AllowedValues.Contains(value)) || (currentValue != null && value == currentValue))
                {
                    return value;
                }

                error = I18n.T("host.settings.errors.choice");
                return null;

            case SettingType.Hotkey:
                return HotkeyFormat.Normalize(value, out error);

            case SettingType.GamepadCombo:
                int[]? codes = GamepadComboFormat.Parse(value, rule.ComboSize, out error);
                return codes == null ? null : GamepadComboFormat.Format(codes);

            default:
                if (rule.MaxLength.HasValue && value.Length > rule.MaxLength.Value)
                {
                    error = I18n.T("host.settings.errors.maxLength", new { count = rule.MaxLength.Value });
                    return null;
                }

                if (value.IndexOfAny(new[] { '\r', '\n', '\0' }) >= 0)
                {
                    error = I18n.T("host.settings.errors.lineBreaks");
                    return null;
                }

                if (!FitsIniCodePage(value))
                {
                    error = I18n.T("host.settings.errors.codePage");
                    return null;
                }

                return value;
        }
    }

    /// <summary>Whether a string survives Settings.ini (written with the ANSI <c>WritePrivateProfileString</c>).</summary>
    public static bool FitsIniCodePage(string value)
    {
        Encoding ansi = Encoding.Default;
        return ansi.GetString(ansi.GetBytes(value)) == value;
    }

    private static string Identity(SettingRule rule, string value)
    {
        if (rule.Type == SettingType.GamepadCombo)
        {
            int[]? codes = GamepadComboFormat.Parse(value, rule.ComboSize, out _);
            return codes == null ? value : "pad:" + GamepadComboFormat.RuntimeValue(codes);
        }

        return value.ToUpperInvariant();
    }

    private static bool IsOn(Dictionary<string, string> values, string key) =>
        values.TryGetValue(key, out string? value) && string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);

    private static ValidationError Error(string field, string message) => new ValidationError { Field = field, Message = message };
}
