using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Settings;

/// <summary>
/// Keyboard shortcuts as Settings.ini stores them (<c>[Hotkeys]</c>): one modifier (Ctrl, Alt or Shift), "+", one
/// letter or digit, for example <c>Ctrl+Q</c>. That is all the session hotkey registration supports.
/// </summary>
public static class HotkeyFormat
{
    /// <summary>Allowed modifiers.</summary>
    public static readonly IReadOnlyList<string> Modifiers = new[] { "Ctrl", "Alt", "Shift" };

    /// <summary>Parses and normalizes ("ctrl+q" → "Ctrl+Q"). Returns null with a reason when invalid.</summary>
    public static string? Normalize(string? value, out string? error)
    {
        error = null;
        string[] parts = (value ?? "").Split('+').Select(p => p.Trim()).ToArray();
        if (parts.Length != 2 || parts[0].Length == 0 || parts[1].Length == 0)
        {
            error = I18n.T("host.settings.errors.hotkeyFormat");
            return null;
        }

        string? modifier = parts[0].ToLowerInvariant() switch
        {
            "ctrl" or "control" => "Ctrl",
            "alt" => "Alt",
            "shift" => "Shift",
            _ => null,
        };
        if (modifier == null)
        {
            error = I18n.T("host.settings.errors.hotkeyModifier");
            return null;
        }

        string key = parts[1].ToUpperInvariant();
        if (key.Length != 1 || !((key[0] >= 'A' && key[0] <= 'Z') || (key[0] >= '0' && key[0] <= '9')))
        {
            error = I18n.T("host.settings.errors.hotkeyKey");
            return null;
        }

        return modifier + "+" + key;
    }
}

/// <summary>
/// Gamepad buttons as <c>[XShortcuts]</c> stores them: XInput button flags, plus 9999 for the right trigger and
/// 10000 for the left trigger. A shortcut is two of them joined with "+" (<c>1024+32768</c> = Guide + Y).
/// </summary>
public static class GamepadComboFormat
{
    /// <summary>Right trigger code.</summary>
    public const int RightTrigger = 9999;

    /// <summary>Left trigger code.</summary>
    public const int LeftTrigger = 10000;

    /// <summary>Guide button flag (read through the undocumented XInputGetStateSecret).</summary>
    public const int Guide = 1024;

    /// <summary>Button codes and their Xbox / PlayStation names and image (<c>gui\gamepads\&lt;type&gt;\&lt;image&gt;.png</c>).</summary>
    public static readonly IReadOnlyList<(int Code, string Xbox, string PlayStation, string Image)> Buttons = new[]
    {
        (1, "D-pad Up", "D-pad Up", "du"),
        (2, "D-pad Down", "D-pad Down", "dd"),
        (4, "D-pad Left", "D-pad Left", "dl"),
        (8, "D-pad Right", "D-pad Right", "dr"),
        (16, "Start", "Options", "start"),
        (32, "Back", "Share", "select"),
        (64, "Left Stick", "L3", "ls"),
        (128, "Right Stick", "R3", "rs"),
        (256, "LB", "L1", "lb"),
        (512, "RB", "R1", "rb"),
        (Guide, "Guide", "PS", "h"),
        (4096, "A", "Cross", "b1"),
        (8192, "B", "Circle", "b2"),
        (16384, "X", "Square", "b3"),
        (32768, "Y", "Triangle", "b4"),
        (RightTrigger, "RT", "R2", "rt"),
        (LeftTrigger, "LT", "L2", "lt"),
    };

    /// <summary>Whether a code is a known button.</summary>
    public static bool IsKnown(int code) => Buttons.Any(b => b.Code == code);

    /// <summary>
    /// Name of a button for a gamepad type ("Xbox" or "Playstation"), in the language in use: the names printed on the
    /// gamepads (A, LB, Start, L1, Options) stay as they are, the descriptive ones (D-pad Up, Left Stick, Cross) are translated.
    /// </summary>
    public static string Name(int code, string? gamepadType)
    {
        var button = Buttons.FirstOrDefault(b => b.Code == code);
        if (button.Code == 0)
        {
            return code.ToString(CultureInfo.InvariantCulture);
        }

        string name = string.Equals(gamepadType, "Playstation", StringComparison.OrdinalIgnoreCase) ? button.PlayStation : button.Xbox;
        return name switch
        {
            "D-pad Up" => I18n.T("host.settings.gamepadButtons.dpadUp"),
            "D-pad Down" => I18n.T("host.settings.gamepadButtons.dpadDown"),
            "D-pad Left" => I18n.T("host.settings.gamepadButtons.dpadLeft"),
            "D-pad Right" => I18n.T("host.settings.gamepadButtons.dpadRight"),
            "Left Stick" => I18n.T("host.settings.gamepadButtons.leftStick"),
            "Right Stick" => I18n.T("host.settings.gamepadButtons.rightStick"),
            "Cross" => I18n.T("host.settings.gamepadButtons.cross"),
            "Circle" => I18n.T("host.settings.gamepadButtons.circle"),
            "Square" => I18n.T("host.settings.gamepadButtons.square"),
            "Triangle" => I18n.T("host.settings.gamepadButtons.triangle"),
            _ => name,
        };
    }

    /// <summary>Parses a stored or captured combination of <paramref name="size"/> buttons (1 or 2). Null with a reason when invalid.</summary>
    public static int[]? Parse(string? value, int size, out string? error)
    {
        error = null;
        string[] parts = (value ?? "").Split('+').Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
        if (parts.Length != size)
        {
            error = size == 1 ? I18n.T("host.settings.errors.singleButton") : I18n.T("host.settings.errors.twoButtons");
            return null;
        }

        var codes = new int[size];
        for (int i = 0; i < size; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out codes[i]) || !IsKnown(codes[i]))
            {
                error = I18n.T("host.settings.errors.unknownButton", new { code = parts[i] });
                return null;
            }
        }

        if (size == 2)
        {
            if (codes[0] == codes[1])
            {
                error = I18n.T("host.settings.errors.differentButtons");
                return null;
            }

            if (codes.Contains(RightTrigger) && codes.Contains(LeftTrigger))
            {
                error = I18n.T("host.settings.errors.bothTriggers");
                return null;
            }
        }

        return codes;
    }

    /// <summary>Stored form: codes joined with "+".</summary>
    public static string Format(IEnumerable<int> codes) => string.Join("+", codes.Select(c => c.ToString(CultureInfo.InvariantCulture)));

    /// <summary>What the shortcut threads compare (sum of the codes): two combinations with the same sum collide.</summary>
    public static int RuntimeValue(int[] codes) => codes.Sum();
}
