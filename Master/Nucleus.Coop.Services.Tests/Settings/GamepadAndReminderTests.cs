using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Coop.Services.Settings;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Settings;

public class GamepadComboCaptureTests
{
    // Plays a script of states, one per poll; the last state repeats.
    private sealed class ScriptedReader : IGamepadReader
    {
        private readonly (int Slot, int Buttons, int Left, int Right)[][] polls;
        private int poll = -1;

        public ScriptedReader(params (int Slot, int Buttons, int Left, int Right)[][] polls) => this.polls = polls;

        public int SlotCount => 2;

        public bool IsConnected(int slot) => true;

        public bool TryRead(int slot, out int buttons, out int leftTrigger, out int rightTrigger)
        {
            if (slot == 0)
            {
                poll = Math.Min(poll + 1, polls.Length - 1);
            }

            buttons = leftTrigger = rightTrigger = 0;
            foreach (var state in polls[poll])
            {
                if (state.Slot == slot)
                {
                    buttons = state.Buttons;
                    leftTrigger = state.Left;
                    rightTrigger = state.Right;
                }
            }

            return true;
        }
    }

    private static readonly (int, int, int, int)[] Idle = new (int, int, int, int)[0];

    private static string Capture(ScriptedReader reader, int buttons) =>
        GamepadComboCapture.Capture(reader, buttons, CancellationToken.None, TimeSpan.Zero, TimeSpan.FromSeconds(5));

    [Fact]
    public void Pair_in_press_order()
    {
        var reader = new ScriptedReader(Idle, new[] { (0, 1024, 0, 0) }, new[] { (0, 1024 | 32768, 0, 0) });
        Assert.Equal("1024+32768", Capture(reader, 2));
    }

    [Fact]
    public void Waits_for_buttons_held_before_the_capture_to_be_released()
    {
        var reader = new ScriptedReader(new[] { (0, 4096, 0, 0) }, new[] { (0, 4096 | 8192, 0, 0) }, Idle, new[] { (0, 512, 0, 0) }, Idle);
        Assert.Equal("512", Capture(reader, 1));
    }

    [Fact]
    public void Single_button_after_release_and_triggers()
    {
        Assert.Equal("10000", Capture(new ScriptedReader(Idle, new[] { (1, 0, 255, 0) }, Idle), 1));
        Assert.Equal("1024+9999", Capture(new ScriptedReader(Idle, new[] { (0, 1024, 0, 0) }, new[] { (0, 1024, 0, 200) }), 2));
    }

    [Fact]
    public void Light_trigger_pressure_is_ignored()
    {
        Assert.Equal("16", Capture(new ScriptedReader(Idle, new[] { (0, 0, 10, 0) }, new[] { (0, 16, 10, 0) }, Idle), 1));
    }

    [Fact]
    public void Both_triggers_wait_for_a_button()
    {
        var reader = new ScriptedReader(Idle, new[] { (0, 0, 0, 255) }, new[] { (0, 0, 255, 255) }, new[] { (0, 32, 255, 255) });
        Assert.Equal("9999+32", Capture(reader, 2));
    }

    [Fact]
    public async Task Cancellation_stops_waiting()
    {
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));
        var reader = new ScriptedReader(Idle);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Task.Run(() => GamepadComboCapture.Capture(reader, 2, cancel.Token, TimeSpan.FromMilliseconds(5))));
    }

    [Fact]
    public void A_lone_button_does_not_complete_a_pair()
    {
        var reader = new ScriptedReader(Idle, new[] { (0, 512, 0, 0) }, Idle, new[] { (0, 1024, 0, 0) }, new[] { (0, 1024 | 1, 0, 0) });
        Assert.Equal("1024+1", Capture(reader, 2));
    }

    [Fact]
    public void Decode() => Assert.Equal(new List<int> { 1, 1024, 9999 }, GamepadComboCapture.Decode(1 | 1024, 0, 255));
}

public class ShortcutsReminderRendererTests
{
    [Fact]
    public void Images_have_the_reminder_proportions()
    {
        string folder = Path.Combine(Path.GetTempPath(), "nc-reminder-" + Guid.NewGuid().ToString("N"));
        try
        {
            string keyboard = Path.Combine(folder, "KbShortcutsReminder.jpeg");
            string gamepad = Path.Combine(folder, "XShortcutsReminder.jpeg");

            ShortcutsReminderRenderer.RenderKeyboard(keyboard, new List<(string, string)> { ("Close Nucleus", "Ctrl+Q"), ("Lock / unlock inputs", "+") });
            ShortcutsReminderRenderer.RenderGamepad(gamepad, new List<(string, int[])> { ("Toggle unfocus", new[] { 1024, 32768 }), ("Alt+Tab", new[] { 1024, 256 }) }, "Playstation", null);
            ShortcutsReminderRenderer.RenderKeyboard(keyboard, new List<(string, string)> { ("Close Nucleus", "Alt+X") }); // replaces the file

            using (var image = Image.FromFile(keyboard))
            {
                Assert.Equal(ShortcutsReminderRenderer.KeyboardSize, image.Size);
            }

            using (var image = Image.FromFile(gamepad))
            {
                Assert.Equal(ShortcutsReminderRenderer.GamepadSize, image.Size);
            }

            Assert.Equal("Triangle", GamepadComboFormat.Name(32768, "Playstation"));
            Assert.Equal("Y", GamepadComboFormat.Name(32768, "Xbox"));
        }
        finally
        {
            Directory.Delete(folder, true);
        }
    }
}
