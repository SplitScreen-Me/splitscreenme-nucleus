using Nucleus.Bridge.Contracts;

namespace Nucleus.Coop.Services.Library;

/// <summary>Input flags of a handler, the values <see cref="GameInputRules"/> reads.</summary>
public sealed class HandlerInputFlags
{
    /// <summary><c>Game.Hook.XInputEnabled</c>.</summary>
    public bool XInputEnabled { get; set; }

    /// <summary><c>Game.Hook.XInputReroute</c>.</summary>
    public bool XInputReroute { get; set; }

    /// <summary><c>Game.Hook.DInputEnabled</c>.</summary>
    public bool DInputEnabled { get; set; }

    /// <summary><c>Game.Hook.SDL2Enabled</c>.</summary>
    public bool Sdl2Enabled { get; set; }

    /// <summary><c>Game.ProtoInput.XinputHook</c>.</summary>
    public bool ProtoXinputHook { get; set; }

    /// <summary><c>Game.ProtoInput.DinputDeviceHook</c>.</summary>
    public bool ProtoDinputDeviceHook { get; set; }

    /// <summary><c>Game.SupportsKeyboard</c>.</summary>
    public bool SupportsKeyboard { get; set; }

    /// <summary><c>Game.SupportsMultipleKeyboardsAndMice</c>.</summary>
    public bool SupportsMultipleKeyboardsAndMice { get; set; }
}

/// <summary>
/// Which input icons a game shows, the rules of <c>InputIcons.SetInputsIcons</c> (game page), and the gamepad filter
/// of <c>SortGameFunction</c>.
/// </summary>
public static class GameInputRules
{
    /// <summary>Supported inputs, as the game page icons show them.</summary>
    public static GameInputs ToInputs(HandlerInputFlags f) => new GameInputs
    {
        // SDL2 shows both gamepad icons; XInput otherwise needs XInput hooks without DInput rerouting.
        XInput = f.Sdl2Enabled || (f.XInputEnabled && !f.XInputReroute && !f.ProtoDinputDeviceHook) || f.ProtoXinputHook,
        // Both DInput branches of InputIcons reduce to this condition.
        DInput = f.Sdl2Enabled || f.DInputEnabled || f.XInputReroute || f.ProtoDinputDeviceHook,
        Keyboard = f.SupportsKeyboard,
        MultipleKeyboards = f.SupportsMultipleKeyboardsAndMice,
    };

    /// <summary>The "Supports gamepads" filter of the legacy sort panel (broader than the icons).</summary>
    public static bool SupportsGamepads(HandlerInputFlags f) =>
        f.XInputEnabled || f.ProtoXinputHook || f.DInputEnabled || f.XInputReroute || f.ProtoDinputDeviceHook || f.Sdl2Enabled;
}
