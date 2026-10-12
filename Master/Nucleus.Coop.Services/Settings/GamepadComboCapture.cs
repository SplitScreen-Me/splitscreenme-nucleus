using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SharpDX.XInput;

namespace Nucleus.Coop.Services.Settings;

/// <summary>Reads gamepad state for <see cref="GamepadComboCapture"/>.</summary>
public interface IGamepadReader
{
    /// <summary>Number of slots.</summary>
    int SlotCount { get; }

    /// <summary>Whether a gamepad is connected in a slot.</summary>
    bool IsConnected(int slot);

    /// <summary>Current buttons (XInput flags, Guide included when available) and trigger values (0-255).</summary>
    bool TryRead(int slot, out int buttons, out int leftTrigger, out int rightTrigger);
}

/// <summary>XInput slots 0-3, through the same calls as the legacy capture (<c>GamepadState</c>: Guide button via <c>XInputGetStateSecret</c>).</summary>
public sealed class XInputGamepadReader : IGamepadReader
{
    private bool secretUnavailable;

    /// <inheritdoc />
    public int SlotCount => GamepadState.Controllers.Length;

    /// <inheritdoc />
    public bool IsConnected(int slot)
    {
        try
        {
            return GamepadState.Controllers[slot].IsConnected;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <inheritdoc />
    public bool TryRead(int slot, out int buttons, out int leftTrigger, out int rightTrigger)
    {
        buttons = leftTrigger = rightTrigger = 0;
        if (!IsConnected(slot))
        {
            return false;
        }

        State state = default;
        if (!secretUnavailable)
        {
            try
            {
                // xinput1_4.dll ordinal #100 also reports the Guide button (1024).
                GamepadState.XInputGetStateSecret(slot, ref state);
            }
            catch (Exception e) when (e is DllNotFoundException || e is EntryPointNotFoundException)
            {
                secretUnavailable = true; // Windows 7: no xinput1_4, no Guide button
            }
        }

        if (secretUnavailable && !GamepadState.Controllers[slot].GetState(out state))
        {
            return false;
        }

        buttons = (int)state.Gamepad.Buttons;
        leftTrigger = state.Gamepad.LeftTrigger;
        rightTrigger = state.Gamepad.RightTrigger;
        return true;
    }
}

/// <summary>
/// Headless version of the capture done in <c>XInputShortcutsSetup.GamepadTopFront_Paint</c>: polls the gamepads on
/// a worker thread and returns the buttons the user pressed, in the stored format. It first waits until every button
/// is released, then records the buttons of one gamepad in the order they are pressed. For a pair the second button
/// completes it (<c>1024+32768</c>; releasing a lone button starts over); for a single button the first press is
/// returned (<c>512</c>). Triggers count as buttons 9999 (RT) and 10000 (LT).
/// </summary>
public static class GamepadComboCapture
{
    /// <summary>A trigger counts as pressed above this value (0-255).</summary>
    public const int TriggerThreshold = 30;

    private static readonly int[] ButtonFlags = { 1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 4096, 8192, 16384, 32768 };

    /// <summary>Whether any gamepad is connected.</summary>
    public static bool AnyConnected(IGamepadReader reader) => Enumerable.Range(0, reader.SlotCount).Any(reader.IsConnected);

    /// <summary>Blocks until a combination is captured, <paramref name="cancellationToken"/> is cancelled or <paramref name="timeout"/> passes (<see cref="TimeoutException"/>).</summary>
    public static string Capture(IGamepadReader reader, int buttons, CancellationToken cancellationToken, TimeSpan? pollInterval = null, TimeSpan? timeout = null)
    {
        if (buttons != 1 && buttons != 2)
        {
            throw new ArgumentOutOfRangeException(nameof(buttons), "1 or 2 buttons.");
        }

        TimeSpan interval = pollInterval ?? TimeSpan.FromMilliseconds(15);
        DateTime deadline = DateTime.UtcNow + (timeout ?? TimeSpan.FromMinutes(2));
        bool released = false;
        var order = new Dictionary<int, List<int>>();

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (DateTime.UtcNow > deadline)
            {
                throw new TimeoutException("No gamepad button was pressed.");
            }

            var pressedBySlot = new Dictionary<int, List<int>>();
            for (int slot = 0; slot < reader.SlotCount; slot++)
            {
                if (reader.TryRead(slot, out int flags, out int left, out int right))
                {
                    List<int> pressed = Decode(flags, left, right);
                    if (pressed.Count > 0)
                    {
                        pressedBySlot[slot] = pressed;
                    }
                }
            }

            if (!released)
            {
                released = pressedBySlot.Count == 0;
            }
            else
            {
                foreach (KeyValuePair<int, List<int>> slot in pressedBySlot)
                {
                    if (!order.TryGetValue(slot.Key, out List<int>? sequence))
                    {
                        order[slot.Key] = sequence = new List<int>();
                    }

                    // Buttons pressed in the same poll keep the flag order, which matches the legacy single bitmask.
                    foreach (int code in slot.Value.Where(c => !sequence.Contains(c)))
                    {
                        sequence.Add(code);
                    }

                    if (buttons == 1 && sequence.Count >= 1)
                    {
                        return GamepadComboFormat.Format(sequence.Take(1));
                    }

                    if (sequence.Count >= 2)
                    {
                        if (sequence[0] + sequence[1] == GamepadComboFormat.RightTrigger + GamepadComboFormat.LeftTrigger)
                        {
                            sequence.RemoveAt(1); // both triggers can't be combined: keep waiting for a button
                            continue;
                        }

                        return GamepadComboFormat.Format(sequence.Take(2));
                    }
                }

                // A pair was expected but the button was released alone: start over on that gamepad.
                foreach (KeyValuePair<int, List<int>> slot in order)
                {
                    if (!pressedBySlot.ContainsKey(slot.Key))
                    {
                        slot.Value.Clear();
                    }
                }
            }

            Thread.Sleep(interval);
        }
    }

    /// <summary>Button codes of a state.</summary>
    public static List<int> Decode(int buttons, int leftTrigger, int rightTrigger)
    {
        var codes = ButtonFlags.Where(flag => (buttons & flag) != 0).ToList();
        if (rightTrigger > TriggerThreshold)
        {
            codes.Add(GamepadComboFormat.RightTrigger);
        }

        if (leftTrigger > TriggerThreshold)
        {
            codes.Add(GamepadComboFormat.LeftTrigger);
        }

        return codes;
    }
}
