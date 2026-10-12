using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

/// <summary>
/// The parts of the Nucleus.Gaming setup engine (PlayerInfo, UserScreen, BoundsFunctions, DevicesFunctions) that can run
/// without hardware. The test host has no gui\theme folder: these objects used to load the WinForms setup screen's
/// pictures from it (and throw without it).
/// </summary>
public class SetupEngineTests
{
    public static IEnumerable<object[]> DeviceFlags()
    {
        yield return new object[] { new Action<PlayerInfo>(p => p.IsXInput = true), InputType.XInput, true };
        yield return new object[] { new Action<PlayerInfo>(p => p.IsDInput = true), InputType.DInput, true };
        yield return new object[] { new Action<PlayerInfo>(p => p.IsSDL2 = true), InputType.SDL2, true };
        yield return new object[] { new Action<PlayerInfo>(p => p.IsKeyboardPlayer = true), InputType.SingleKB, false };
        yield return new object[] { new Action<PlayerInfo>(p => p.IsRawKeyboard = true), InputType.KB, false };
        yield return new object[] { new Action<PlayerInfo>(p => p.IsRawMouse = true), InputType.Mouse, false };
        yield return new object[] { new Action<PlayerInfo>(p => { p.IsRawKeyboard = true; p.IsRawMouse = true; }), InputType.KBM, false };
    }

    [Theory]
    [MemberData(nameof(DeviceFlags))]
    public void Device_flags_set_the_input_type_without_theme_pictures(Action<PlayerInfo> detect, InputType type, bool controller)
    {
        PlayerInfo player = new PlayerInfo();

        detect(player);

        Assert.Equal(type, player.InputType);
        Assert.Equal(controller, player.IsController);
    }

    [Theory]
    [InlineData(UserScreenType.FullScreen, 1)]
    [InlineData(UserScreenType.DualHorizontal, 2)]
    [InlineData(UserScreenType.DualVertical, 2)]
    [InlineData(UserScreenType.FourPlayers, 4)]
    [InlineData(UserScreenType.SixPlayers, 6)]
    [InlineData(UserScreenType.EightPlayers, 8)]
    [InlineData(UserScreenType.SixteenPlayers, 16)]
    public void Layouts_split_a_screen_without_theme_pictures(UserScreenType type, int slots)
    {
        Rectangle monitor = new Rectangle(0, 0, 1920, 1080);
        UserScreen screen = new UserScreen(monitor) { UIBounds = new RectangleF(10, 20, 192, 108) };

        screen.Type = type;

        Assert.Equal(slots, screen.SubScreensBounds.Count);
        Assert.Equal(monitor.Width * monitor.Height, screen.SubScreensBounds.Keys.Sum(r => r.Width * r.Height));
        Assert.All(screen.SubScreensBounds.Keys, r => Assert.True(monitor.Contains(r)));
        Assert.All(screen.SubScreensBounds.Values, r => Assert.True(screen.UIBounds.Contains(r)));
    }

    [Fact]
    public void A_flash_reveals_a_keyboard_and_raises_on_then_off()
    {
        PlayerInfo keyboard = new PlayerInfo { IsRawKeyboard = true };
        List<bool> edges = new List<bool>();

        using ManualResetEventSlim off = new ManualResetEventSlim();

        void OnFlash(PlayerInfo player, bool on)
        {
            if (player != keyboard)
            {
                return;
            }

            lock (edges)
            {
                edges.Add(on);
            }

            if (!on)
            {
                off.Set();
            }
        }

        PlayerInfo.FlashChanged += OnFlash;

        try
        {
            Assert.False(keyboard.IsInputUsed);

            keyboard.FlashIcon();

            Assert.True(keyboard.IsInputUsed);
            Assert.True(keyboard.ShouldFlash);
            Assert.True(off.Wait(TimeSpan.FromSeconds(5)));
            Assert.False(keyboard.ShouldFlash);

            lock (edges)
            {
                Assert.Equal(new[] { true, false }, edges);
            }
        }
        finally
        {
            PlayerInfo.FlashChanged -= OnFlash;
        }
    }

    [Fact]
    public void A_flash_does_not_reveal_a_gamepad()
    {
        PlayerInfo pad = new PlayerInfo { IsXInput = true };

        pad.FlashIcon();

        Assert.False(pad.IsInputUsed);//gamepads are revealed by the polling (button press), not by the flash itself
        Assert.True(pad.ShouldFlash);
    }

    [Fact]
    public void The_setup_engine_needs_a_dispatcher()
    {
        Action<Action> saved = DevicesFunctions.Dispatch;
        DevicesFunctions.Dispatch = null!;

        try
        {
            Assert.Throws<InvalidOperationException>(() => BoundsFunctions.Initialize(null!, null!));
            Assert.Throws<InvalidOperationException>(() => DevicesFunctions.Initialize(null!, null!));
            Assert.False(DevicesFunctions.IsRunning);
        }
        finally
        {
            DevicesFunctions.Dispatch = saved;
        }
    }
}
