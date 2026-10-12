using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

public class DeviceIdsTests
{
    [Theory]
    [InlineData(InputType.XInput, DeviceKind.XInput)]
    [InlineData(InputType.DInput, DeviceKind.DInput)]
    [InlineData(InputType.SDL2, DeviceKind.Sdl)]
    [InlineData(InputType.SingleKB, DeviceKind.Keyboard)]
    [InlineData(InputType.KB, DeviceKind.RawKeyboard)]
    [InlineData(InputType.Mouse, DeviceKind.RawMouse)]
    [InlineData(InputType.KBM, DeviceKind.KeyboardMouse)]
    public void Kinds_map_to_the_contract(InputType type, DeviceKind kind)
    {
        Assert.Equal(kind, DeviceIds.KindOf(type));
    }

    [Fact]
    public void Pad_buttons_get_their_contract_names_face_buttons_first()
    {
        Assert.Empty(DeviceIds.ButtonsOf(PadButtons.None));
        Assert.Equal(new[] { GamepadButton.Start }, DeviceIds.ButtonsOf(PadButtons.Start));
        Assert.Equal(
            new[] { GamepadButton.A, GamepadButton.Y, GamepadButton.LeftBumper, GamepadButton.Back, GamepadButton.DPadLeft, GamepadButton.Other },
            DeviceIds.ButtonsOf(PadButtons.Other | PadButtons.DPadLeft | PadButtons.Back | PadButtons.LeftShoulder | PadButtons.Y | PadButtons.A));
    }

    [Fact]
    public void Every_pad_button_has_a_contract_name()
    {
        foreach (PadButtons button in Enum.GetValues(typeof(PadButtons)).Cast<PadButtons>().Where(b => b != PadButtons.None))
        {
            Assert.Single(DeviceIds.ButtonsOf(button));
        }
    }

    [Fact]
    public void Profile_players_expect_the_kind_their_flags_describe()
    {
        Assert.Equal(DeviceKind.XInput, DeviceIds.TemplateKind(true, false, false, false, false, 0));
        Assert.Equal(DeviceKind.Sdl, DeviceIds.TemplateKind(false, false, true, false, false, 0));
        Assert.Equal(DeviceKind.Keyboard, DeviceIds.TemplateKind(false, false, false, true, false, 0));
        Assert.Equal(DeviceKind.KeyboardMouse, DeviceIds.TemplateKind(false, false, false, false, true, 2));
        Assert.Equal(DeviceKind.RawMouse, DeviceIds.TemplateKind(false, false, false, false, true, 1));
        Assert.Equal(DeviceKind.RawKeyboard, DeviceIds.TemplateKind(false, false, false, false, false, 1));
    }

    [Fact]
    public void Fallback_keys_use_the_same_scheme_as_detection()
    {
        Guid guid = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef");

        Assert.Equal("dinput:01234567-89ab-cdef-0123-456789abcdef", DeviceIds.FallbackKey(InputType.DInput, 0, guid, null));
        Assert.Equal("xinput:2", DeviceIds.FallbackKey(InputType.XInput, 2, Guid.Empty, null));
        Assert.Equal("sdl:1", DeviceIds.FallbackKey(InputType.SDL2, 1, Guid.Empty, null));
        Assert.Equal("kb:single", DeviceIds.FallbackKey(InputType.SingleKB, 99, Guid.Empty, null));
        Assert.Equal("rawkb:\\\\?\\HID#VID_1", DeviceIds.FallbackKey(InputType.KB, 100, Guid.Empty, "\\\\?\\HID#VID_1"));
        Assert.Equal("virtual-mouse", DeviceIds.FallbackKey(InputType.Mouse, 101, Guid.Empty, "MouseHandleZero"));
        Assert.Equal("virtual-kb", DeviceIds.FallbackKey(InputType.KB, 102, Guid.Empty, "KeyboardHandleZero"));
    }

    [Fact]
    public void Duplicate_ids_get_a_suffix_and_the_first_keeps_its_id()
    {
        List<string> unique = DeviceIds.MakeUnique(new[] { "rawmouse:", "xinput:0", "rawmouse:", "rawmouse:" });

        Assert.Equal(new[] { "rawmouse:", "xinput:0", "rawmouse:#2", "rawmouse:#3" }, unique);
    }

    [Fact]
    public void Names_prefer_the_product_name_then_readable_ids()
    {
        Assert.Equal("Wireless Gamepad", DeviceIds.Name(InputType.DInput, 0, " Wireless Gamepad ", null));
        Assert.Equal("XInput controller 3", DeviceIds.Name(InputType.XInput, 2, null, "Not required"));
        Assert.Equal("Keyboard (046D:C539)", DeviceIds.Name(InputType.KB, 100, null, @"\\?\HID#VID_046D&PID_C539&MI_00#7&1eb#{884b96c3}"));
        Assert.Equal("Mouse", DeviceIds.Name(InputType.Mouse, 100, null, ""));
        Assert.Equal("Virtual keyboard", DeviceIds.Name(InputType.KB, 100, null, "KeyboardHandleZero"));
        Assert.Equal("Keyboard and mouse", DeviceIds.Name(InputType.SingleKB, 99, null, null));
        Assert.True(DeviceIds.IsVirtual("MouseHandleZero"));
        Assert.False(DeviceIds.IsVirtual(@"\\?\HID#VID_046D"));
    }
}

public class SnapshotMathTests
{
    [Theory]
    [InlineData(UserScreenType.FullScreen, 1, 1)]
    [InlineData(UserScreenType.DualHorizontal, 2, 1)]
    [InlineData(UserScreenType.DualVertical, 1, 2)]
    [InlineData(UserScreenType.FourPlayers, 2, 2)]
    [InlineData(UserScreenType.SixPlayers, 2, 3)]
    [InlineData(UserScreenType.EightPlayers, 2, 4)]
    [InlineData(UserScreenType.SixteenPlayers, 4, 4)]
    public void Grid_of_each_fixed_layout(UserScreenType type, int rows, int columns)
    {
        Assert.Equal((rows, columns), SnapshotMath.Grid(type, 0, 0, 0));
    }

    [Fact]
    public void Custom_and_manual_grids()
    {
        Assert.Equal((3, 4), SnapshotMath.Grid(UserScreenType.Custom, 2, 3, 12));
        Assert.Equal((40, 40), SnapshotMath.Grid(UserScreenType.Manual, 0, 0, 1600));
    }

    [Fact]
    public void Contract_layouts_and_screen_types_match_one_to_one()
    {
        foreach (UserScreenType type in Enum.GetValues(typeof(UserScreenType)).Cast<UserScreenType>())
        {
            Assert.Equal(type, SnapshotMath.ToScreenType(SnapshotMath.ToLayout(type)));
            Assert.Equal((int)type, (int)SnapshotMath.ToLayout(type));
        }

        Assert.Equal(Enum.GetValues(typeof(UserScreenType)).Length, Enum.GetValues(typeof(ScreenLayout)).Length);
    }

    [Fact]
    public void Expand_and_resize_rules()
    {
        Assert.False(SnapshotMath.CanExpand(UserScreenType.DualVertical, false));
        Assert.True(SnapshotMath.CanExpand(UserScreenType.FourPlayers, false));
        Assert.True(SnapshotMath.CanExpand(UserScreenType.Custom, false));
        Assert.False(SnapshotMath.CanExpand(UserScreenType.Manual, false));
        Assert.False(SnapshotMath.CanExpand(UserScreenType.FourPlayers, true));

        Assert.True(SnapshotMath.CanResize(4, false));
        Assert.False(SnapshotMath.CanResize(2, false));
        Assert.False(SnapshotMath.CanResize(16, true));
    }

    [Fact]
    public void A_resize_step_is_a_twentieth_of_a_slot_or_a_manual_cell()
    {
        Rectangle quarter = new Rectangle(0, 0, 960, 540);
        Rectangle manualCell = new Rectangle(0, 0, 48, 27);

        Assert.Equal(new Size(48, 27), SnapshotMath.ResizeStep(UserScreenType.FourPlayers, quarter, 4, false));
        Assert.Equal(new Size(48, 27), SnapshotMath.ResizeStep(UserScreenType.Manual, manualCell, 1600, false));
        Assert.Equal(new Size(32, 18), SnapshotMath.ResizeStep(UserScreenType.Custom, new Rectangle(0, 0, 640, 360), 9, false));
    }

    [Fact]
    public void No_resize_step_where_players_cannot_be_resized()
    {
        Assert.Equal(Size.Empty, SnapshotMath.ResizeStep(UserScreenType.DualHorizontal, new Rectangle(0, 0, 1920, 540), 2, false));
        Assert.Equal(Size.Empty, SnapshotMath.ResizeStep(UserScreenType.FourPlayers, new Rectangle(0, 0, 960, 540), 4, true));
    }

    [Fact]
    public void Slot_occupant_is_the_first_intersecting_player()
    {
        Rectangle slot = new Rectangle(960, 0, 960, 540);
        List<Rectangle> players = new List<Rectangle> { Rectangle.Empty, new Rectangle(0, 0, 960, 540), new Rectangle(0, 0, 1920, 540) };

        Assert.Equal(2, SnapshotMath.OccupantOf(slot, players));
        Assert.Equal(-1, SnapshotMath.OccupantOf(new Rectangle(0, 540, 960, 540), players));
    }

    [Fact]
    public void Custom_step_pictures_are_served_from_the_handlers_host()
    {
        Assert.Equal("https://handlers.nucleus.example/My%20Handler/images/card%201.png",
            SetupSnapshotBuilder.HandlerImageUrl("My Handler", @"Images\Card 1.png"));
    }
}
