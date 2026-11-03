using System;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>
/// Nucleus 2.5: the custom openxinput1_3.dll gives the device path of each XInput slot (OpenXInputGetDevicePath), so an
/// XInput pad can get the identity of its DirectInput device when it is detected, without a button press (a saved profile
/// then assigns it at once). The path names the XInput (XUSB) interface, which is usually another device node than the
/// HID view DirectInput lists: GamepadIdentity.FindByXInputPath must only bind when the match is certain.
/// </summary>
public class XInputDevicePathTests
{
    private const string XusbPath = @"\\?\usb#vid_045e&pid_028e#1234abcd#{ec87f1e3-c13b-4100-b5f7-8b84d54260cb}";
    private const string Xbox360View = @"\\?\hid#vid_045e&pid_028e&ig_00#7&2b9a8f2d&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
    private const string SecondXbox360View = @"\\?\hid#vid_045e&pid_028e&ig_01#7&11aa22bb&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
    private const string XboxOneView = @"\\?\hid#vid_045e&pid_02ff&ig_00#8&3c4d5e6f&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";

    private static Guid ProductGuid(ushort vid, ushort pid) => new Guid($"{pid:X4}{vid:X4}-0000-0000-0000-504944564944");

    private static D_Joystick Joystick(string path, ushort vid, ushort pid)
    {
        return new D_Joystick { InstanceGuid = Guid.NewGuid(), ProductGuid = ProductGuid(vid, pid), InterfacePath = path, InstanceName = "pad" };
    }

    private static PlayerInfo Pad(int slot) => new PlayerInfo { IsXInput = true, GamepadId = slot, DeviceKey = "xinput:" + slot };

    [Fact]
    public void The_same_path_matches()
    {
        D_Joystick view = Joystick(Xbox360View, 0x045E, 0x028E);
        PlayerInfo pad = Pad(0);

        Assert.Same(view, GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { view }, Xbox360View.ToUpperInvariant()));
    }

    [Fact]
    public void An_xusb_path_matches_the_only_xinput_view_with_its_usb_ids()
    {
        D_Joystick view = Joystick(Xbox360View, 0x045E, 0x028E);
        D_Joystick other = Joystick(XboxOneView, 0x045E, 0x02FF);
        PlayerInfo pad = Pad(0);

        Assert.Same(view, GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { other, view }, XusbPath));
    }

    [Fact]
    public void Two_identical_pads_are_not_guessed()
    {
        D_Joystick first = Joystick(Xbox360View, 0x045E, 0x028E);
        D_Joystick second = Joystick(SecondXbox360View, 0x045E, 0x028E);
        PlayerInfo pad = Pad(0);

        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { first, second }, XusbPath));
    }

    [Fact]
    public void Once_one_identical_pad_is_identified_the_other_one_is_the_only_candidate()
    {
        D_Joystick first = Joystick(Xbox360View, 0x045E, 0x028E);
        D_Joystick second = Joystick(SecondXbox360View, 0x045E, 0x028E);
        PlayerInfo identified = Pad(0);
        GamepadIdentity.Bind(identified, first);
        PlayerInfo pad = Pad(1);

        Assert.Same(second, GamepadIdentity.FindByXInputPath(pad, new[] { identified, pad }, new[] { first, second }, XusbPath));
    }

    [Fact]
    public void A_device_that_is_not_an_xinput_view_is_never_bound_by_usb_ids()
    {
        D_Joystick dinputOnly = Joystick(@"\\?\hid#vid_045e&pid_028e#7&2b9a8f2d&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}", 0x045E, 0x028E);
        PlayerInfo pad = Pad(0);

        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { dinputOnly }, XusbPath));
    }

    [Fact]
    public void Other_usb_ids_or_no_path_never_match()
    {
        D_Joystick other = Joystick(XboxOneView, 0x045E, 0x02FF);
        PlayerInfo pad = Pad(0);

        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { other }, XusbPath));
        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { pad }, new[] { other }, ""));
        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { pad }, null!, XusbPath));
    }

    [Fact]
    public void A_view_taken_by_another_pad_is_not_bound_again()
    {
        D_Joystick view = Joystick(Xbox360View, 0x045E, 0x028E);
        PlayerInfo owner = Pad(0);
        GamepadIdentity.Bind(owner, view);
        PlayerInfo pad = Pad(1);

        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { owner, pad }, new[] { view }, XusbPath));
        Assert.Null(GamepadIdentity.FindByXInputPath(pad, new[] { owner, pad }, new[] { view }, Xbox360View));
    }

    [Theory]
    [InlineData(XusbPath, 0x045E, 0x028E)]
    [InlineData(@"HID\VID_054C&PID_09CC&MI_03\8&1F5C3C5A", 0x054C, 0x09CC)]
    [InlineData(@"usb\vid_28de&pid_11ff\0", 0x28DE, 0x11FF)]
    public void Usb_ids_are_read_from_a_path(string path, int vid, int pid)
    {
        Assert.True(GamepadIdentity.TryUsbIdsOfPath(path, out ushort vendorId, out ushort productId));
        Assert.Equal(vid, vendorId);
        Assert.Equal(pid, productId);
    }

    [Theory]
    [InlineData(@"\\?\root#vgmaster#0000")]
    [InlineData(@"VID_04")]
    [InlineData("")]
    [InlineData(null)]
    public void No_usb_ids_without_both_of_them(string? path)
    {
        Assert.False(GamepadIdentity.TryUsbIdsOfPath(path!, out _, out _));
    }

    [Fact]
    public void An_usb_device_path_gets_its_device_instance_id()
    {
        Assert.Equal(@"USB\VID_045E&PID_028E\1234ABCD", GamepadIdentity.HidIdOf(XusbPath));
    }
}
