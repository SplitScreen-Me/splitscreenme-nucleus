using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

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
            error = "Use one modifier and one key, for example Ctrl+Q.";
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
            error = "The modifier must be Ctrl, Alt or Shift.";
            return null;
        }

        string key = parts[1].ToUpperInvariant();
        if (key.Length != 1 || !((key[0] >= 'A' && key[0] <= 'Z') || (key[0] >= '0' && key[0] <= '9')))
        {
            error = "The key must be a single letter or digit.";
            return null;
        }

        return modifier + "+" + key;
    }
}

/// <summary>
/// Gamepad buttons as <c>[XShortcuts]</c> and <c>[XUINav]</c> store them: XInput button flags, plus 9999 for the
/// right trigger and 10000 for the left trigger. A shortcut is two of them joined with "+" (<c>1024+32768</c> =
/// Guide + Y); the UI navigation clicks are one.
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

    /// <summary>Name of a button for a gamepad type ("Xbox" or "Playstation").</summary>
    public static string Name(int code, string? gamepadType)
    {
        var button = Buttons.FirstOrDefault(b => b.Code == code);
        if (button.Code == 0)
        {
            return code.ToString(CultureInfo.InvariantCulture);
        }

        return string.Equals(gamepadType, "Playstation", StringComparison.OrdinalIgnoreCase) ? button.PlayStation : button.Xbox;
    }

    /// <summary>Parses a stored or captured combination of <paramref name="size"/> buttons (1 or 2). Null with a reason when invalid.</summary>
    public static int[]? Parse(string? value, int size, out string? error)
    {
        error = null;
        string[] parts = (value ?? "").Split('+').Select(p => p.Trim()).Where(p => p.Length > 0).ToArray();
        if (parts.Length != size)
        {
            error = size == 1 ? "Use a single button." : "Use a combination of two buttons, for example Guide + Y.";
            return null;
        }

        var codes = new int[size];
        for (int i = 0; i < size; i++)
        {
            if (!int.TryParse(parts[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out codes[i]) || !IsKnown(codes[i]))
            {
                error = $"'{parts[i]}' is not a gamepad button code.";
                return null;
            }
        }

        if (size == 2)
        {
            if (codes[0] == codes[1])
            {
                error = "Use two different buttons.";
                return null;
            }

            if (codes.Contains(RightTrigger) && codes.Contains(LeftTrigger))
            {
                error = "Both triggers can't be combined: use a trigger with a button.";
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
