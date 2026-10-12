using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

/// <summary>
/// How a button press identifies an XInput pad (GamepadIdentity), the setup hint when it can't, and the DirectInput
/// watcher refresh. The 7 Days to Die bug: an XInput-only handler showed the pad (rumble worked) but every placement was
/// refused with deviceNotRevealed, because a press only counted when exactly one DirectInput device saw it.
/// </summary>
public class GamepadIdentityTests
{
    private const string Xbox360Path = @"\\?\hid#vid_045e&pid_028e&ig_00#7&2b9a8f2d&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";
    private const string Ds4Path = @"\\?\hid#vid_054c&pid_09cc&mi_03#8&1f5c3c5a&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}";

    private static D_Joystick Joystick(string path, string name = "pad")
    {
        return new D_Joystick { InstanceGuid = Guid.NewGuid(), ProductGuid = Guid.NewGuid(), InterfacePath = path, InstanceName = name };
    }

    private static PlayerInfo Pad(int slot) => new PlayerInfo { IsXInput = true, GamepadId = slot, DeviceKey = "xinput:" + slot };

    private static readonly DateTime Now = new DateTime(2026, 10, 12, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(0, "00000000-0000-0000-0000-200000000001")]
    [InlineData(3, "00000000-0000-0000-0000-200000000004")]
    [InlineData(9, "00000000-0000-0000-0000-200000000010")]
    [InlineData(31, "00000000-0000-0000-0000-200000000032")]
    public void Slot_guid_is_the_slot_mode_guid(int slot, string expected)
    {
        Assert.Equal(new Guid(expected), GamepadIdentity.SlotGuid(slot));
    }

    [Theory]
    [InlineData(new[] { false, true, false }, new[] { -1 }, true)]
    [InlineData(new[] { false, false }, new[] { -1 }, false)]
    [InlineData(new[] { false, false }, new[] { 65535 }, false)]//centered hat, low word only
    [InlineData(new[] { false, false }, new[] { 9000 }, true)]//D-pad right
    [InlineData(new[] { false, false }, new int[0], false)]
    public void A_press_is_a_button_or_a_hat_off_center(bool[] buttons, int[] povs, bool pressed)
    {
        Assert.Equal(pressed, GamepadIdentity.IsPressed(buttons, povs));
    }

    [Fact]
    public void Null_states_are_not_a_press()
    {
        Assert.False(GamepadIdentity.IsPressed(null!, null!));
    }

    [Fact]
    public void One_pressed_device_matches()
    {
        PlayerInfo pad = Pad(0);
        D_Joystick xbox = Joystick(Xbox360Path);
        D_Joystick idle = Joystick(Ds4Path);

        D_Joystick? match = GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(xbox, true), new DInputCandidate(idle, false) });

        Assert.Same(xbox, match);
    }

    [Fact]
    public void Two_pressed_devices_prefer_the_xinput_view()
    {
        //DS4Windows without HidHide: the DualShock and its virtual Xbox 360 pad both report the press.
        PlayerInfo pad = Pad(0);
        D_Joystick virtualX360 = Joystick(Xbox360Path);
        D_Joystick ds4 = Joystick(Ds4Path);

        D_Joystick? match = GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(ds4, true), new DInputCandidate(virtualX360, true) });

        Assert.Same(virtualX360, match);
    }

    [Fact]
    public void Two_pressed_xinput_views_are_ambiguous()
    {
        PlayerInfo pad = Pad(0);

        D_Joystick? match = GamepadIdentity.PickDInputMatch(pad, new[] { pad },
            new[] { new DInputCandidate(Joystick(Xbox360Path), true), new DInputCandidate(Joystick(Xbox360Path.Replace("ig_00", "ig_01")), true) });

        Assert.Null(match);
    }

    [Fact]
    public void A_device_already_identified_as_another_pad_is_skipped()
    {
        PlayerInfo first = Pad(0);
        PlayerInfo second = Pad(1);
        D_Joystick firstView = Joystick(Xbox360Path);
        D_Joystick secondView = Joystick(Xbox360Path.Replace("ig_00", "ig_01"));
        first.GamepadGuid = firstView.InstanceGuid;//identified earlier

        //Both players hold a button: only the free device can be the second pad.
        D_Joystick? match = GamepadIdentity.PickDInputMatch(second, new[] { first, second },
            new[] { new DInputCandidate(firstView, true), new DInputCandidate(secondView, true) });

        Assert.Same(secondView, match);
    }

    [Fact]
    public void A_match_copies_the_directinput_identity_and_reveals()
    {
        PlayerInfo pad = Pad(0);
        D_Joystick xbox = Joystick(Xbox360Path, "Controller (XBOX 360 For Windows)");

        IdentifyResult result = GamepadIdentity.Identify(pad, new[] { pad }, new[] { new DInputCandidate(xbox, true) }, needsDInputIdentity: true, Now);

        Assert.Equal(IdentifyResult.Matched, result);
        Assert.True(pad.IsInputUsed);
        Assert.Same(xbox, pad.DInputJoystick);
        Assert.Equal(xbox.InstanceGuid, pad.GamepadGuid);
        Assert.Equal(xbox.ProductGuid, pad.GamepadProductGuid);
        Assert.Equal("Controller (XBOX 360 For Windows)", pad.GamepadName);
        Assert.Equal(@"HID\VID_045E&PID_028E&IG_00\7&2B9A8F2D&0&0000", pad.HIDDeviceID[0]);
        Assert.Equal(DateTime.MinValue, pad.IdentificationFailedUtc);
    }

    [Theory]
    [InlineData(0)]//DirectInput didn't see the press (D-pad before, hidden device)
    [InlineData(2)]//two non-XInput devices saw it (stuck button, mirrored pad)
    public void Without_a_unique_match_a_slot_handler_reveals_the_pad_by_its_slot(int pressedDevices)
    {
        PlayerInfo pad = Pad(2);
        List<DInputCandidate> candidates = Enumerable.Range(0, pressedDevices).Select(_ => new DInputCandidate(Joystick(Ds4Path), true)).ToList();

        IdentifyResult result = GamepadIdentity.Identify(pad, new[] { pad }, candidates, needsDInputIdentity: false, Now);

        Assert.Equal(IdentifyResult.SlotFallback, result);
        Assert.True(pad.IsInputUsed);//the setup no longer refuses it with deviceNotRevealed
        Assert.Null(pad.DInputJoystick);
        Assert.Equal(GamepadIdentity.SlotGuid(2), pad.GamepadGuid);
    }

    [Fact]
    public void Without_a_unique_match_a_handler_that_needs_the_directinput_identity_keeps_the_pad_hidden()
    {
        PlayerInfo pad = Pad(0);

        IdentifyResult result = GamepadIdentity.Identify(pad, new[] { pad }, new List<DInputCandidate>(), needsDInputIdentity: true, Now);

        Assert.Equal(IdentifyResult.Failed, result);
        Assert.False(pad.IsInputUsed);
        Assert.Equal(Guid.Empty, pad.GamepadGuid);
        Assert.Equal(Now, pad.IdentificationFailedUtc);
    }

    [Fact]
    public void Seven_days_to_die_only_needs_the_slot()
    {
        //RkbcpmL5dNbrDcfjL: XInputEnabled + XInputPlus, no DInput, no reroute, no x360ce, no device reordering.
        Assert.False(GamepadIdentity.NeedsDInputIdentity(useX360ce: false, useDevReorder: false, createSingleDeviceFile: false, xinputReroute: false,
            dinputDeviceHook: false, dinputEnabled: false, scriptReadsIdentity: false));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Each_directinput_dependency_needs_the_identity(int flag)
    {
        bool[] f = new bool[7];
        f[flag] = true;

        Assert.True(GamepadIdentity.NeedsDInputIdentity(f[0], f[1], f[2], f[3], f[4], f[5], f[6]));
    }

    [Fact]
    public void A_handler_without_game_info_needs_the_identity()
    {
        Assert.True(GamepadIdentity.NeedsDInputIdentity((Nucleus.Gaming.GenericGameInfo)null!));
    }

    [Theory]
    [InlineData(Xbox360Path, @"HID\VID_045E&PID_028E&IG_00\7&2B9A8F2D&0&0000")]
    [InlineData(@"\\?\usb#vid_1234&pid_0001#abc", @"\\?\USB#VID_1234&PID_0001#ABC")]//not a HID path: kept (used to throw and abort the tick)
    [InlineData("", "")]
    [InlineData(null, "")]
    public void Hid_id_of_an_interface_path(string? path, string expected)
    {
        Assert.Equal(expected, GamepadIdentity.HidIdOf(path!));
    }

    [Theory]
    [InlineData(@"\\?\HID#VID_045E&PID_02FF&IG_00#8&1&0&0000#{x}", true)]
    [InlineData(Ds4Path, false)]
    [InlineData(null, false)]
    public void Xinput_views_are_recognised_by_ig(string? path, bool xinput)
    {
        Assert.Equal(xinput, GamepadIdentity.IsXInputHidPath(path!));
    }

    [Fact]
    public void The_setup_hints_about_a_pad_whose_press_failed_recently()
    {
        PlayerInfo failed = Pad(0);
        failed.IdentificationFailedUtc = Now.AddSeconds(-3);
        PlayerInfo idle = Pad(1);

        Assert.Same(failed, SetupSnapshotBuilder.UnidentifiedPad(new[] { idle, failed }, Now));
        Assert.Null(SetupSnapshotBuilder.UnidentifiedPad(new[] { idle, failed }, Now.AddSeconds(30)));//the hint goes away

        failed.IsInputUsed = true;//identified since (another press, or slot mode)
        Assert.Null(SetupSnapshotBuilder.UnidentifiedPad(new[] { idle, failed }, Now));
        Assert.Null(SetupSnapshotBuilder.UnidentifiedPad(null, Now));
    }

    // ---- SDL pads (the 8 hub handlers with Hook.SDL2Enabled): same rules, helped by what SDL knows of the pad ----

    private static Guid ProductGuid(ushort vid, ushort pid) => new Guid($"{pid:X4}{vid:X4}-0000-0000-0000-504944564944");

    private static D_Joystick Joystick(string path, ushort vid, ushort pid, string name = "pad")
    {
        return new D_Joystick { InstanceGuid = Guid.NewGuid(), ProductGuid = ProductGuid(vid, pid), InterfacePath = path, InstanceName = name };
    }

    private static PlayerInfo SdlPad(int playerIndex) => new PlayerInfo { IsSDL2 = true, GamepadId = playerIndex, DeviceKey = "sdl:" + playerIndex };

    private static PadHint Sdl(string? path, ushort vid, ushort pid, bool xbox = false) => new PadHint { Path = path, VendorId = vid, ProductId = pid, PreferXInputViews = xbox };

    [Theory]
    [InlineData(Ds4Path, @"\\?\HID#VID_054C&PID_09CC&MI_03#8&1F5C3C5A&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}", true)]//raw input spelling
    [InlineData(Ds4Path, @"\??\hid#vid_054c&pid_09cc&mi_03#8&1f5c3c5a&0&0000#{4d1e55b2-f16f-11cf-88cb-001111000030}", true)]
    [InlineData(Ds4Path, Xbox360Path, false)]
    [InlineData(Ds4Path, "XInput#0", false)]
    [InlineData("", "", false)]
    [InlineData(null, null, false)]
    public void Device_paths_compare_without_case_or_prefix(string? a, string? b, bool same)
    {
        Assert.Equal(same, GamepadIdentity.SamePath(a!, b!));
    }

    [Fact]
    public void Usb_ids_come_from_a_hid_product_guid()
    {
        Assert.True(GamepadIdentity.TryUsbIdsOf(new Guid("09CC054C-0000-0000-0000-504944564944"), out ushort vid, out ushort pid));
        Assert.Equal(0x054C, vid);
        Assert.Equal(0x09CC, pid);

        Assert.False(GamepadIdentity.TryUsbIdsOf(new Guid("6f1d2b61-d5a0-11cf-bfc7-444553540000"), out _, out _));//not a HID product GUID
    }

    [Fact]
    public void The_sdl_path_identifies_the_pad_without_a_directinput_press()
    {
        //SDL's HIDAPI driver reports the HID path DirectInput uses: exact, even while another player presses a button.
        PlayerInfo pad = SdlPad(0);
        D_Joystick ds4 = Joystick(Ds4Path, 0x054C, 0x09CC);
        D_Joystick other = Joystick(Xbox360Path, 0x045E, 0x028E);

        D_Joystick? match = GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(ds4, false), new DInputCandidate(other, true) },
            Sdl(Ds4Path.ToUpperInvariant(), 0x054C, 0x09CC));

        Assert.Same(ds4, match);
        Assert.Same(ds4, GamepadIdentity.FindByPath(pad, new[] { pad }, new[] { ds4, other }, Ds4Path));
    }

    [Fact]
    public void A_path_already_taken_by_another_pad_is_not_reused()
    {
        PlayerInfo first = SdlPad(0);
        PlayerInfo second = SdlPad(1);
        D_Joystick ds4 = Joystick(Ds4Path, 0x054C, 0x09CC);
        first.DInputJoystick = ds4;

        Assert.Null(GamepadIdentity.FindByPath(second, new[] { first, second }, new[] { ds4 }, Ds4Path));
        Assert.Null(GamepadIdentity.FindByPath(second, new[] { first, second }, new[] { ds4 }, null!));
        Assert.Null(GamepadIdentity.FindByPath(second, new[] { second }, new[] { ds4, Joystick(Ds4Path, 0x054C, 0x09CC) }, Ds4Path));//stale duplicate: don't guess
    }

    [Fact]
    public void Usb_ids_pick_the_pad_among_two_pressed_devices()
    {
        //DS4Windows without HidHide: SDL sees the DualShock (HIDAPI) and its virtual Xbox 360 pad, DirectInput both views.
        D_Joystick ds4 = Joystick(Ds4Path, 0x054C, 0x09CC);
        D_Joystick virtualX360 = Joystick(Xbox360Path, 0x045E, 0x028E);
        DInputCandidate[] both = { new DInputCandidate(ds4, true), new DInputCandidate(virtualX360, true) };

        PlayerInfo dualShock = SdlPad(0);
        Assert.Same(ds4, GamepadIdentity.PickDInputMatch(dualShock, new[] { dualShock }, both, Sdl("", 0x054C, 0x09CC)));

        PlayerInfo xbox = SdlPad(1);
        Assert.Same(virtualX360, GamepadIdentity.PickDInputMatch(xbox, new[] { xbox }, both, Sdl("XInput#0", 0x045E, 0x028E, xbox: true)));
    }

    [Fact]
    public void A_single_pressed_device_with_other_usb_ids_is_another_pad()
    {
        //The SDL pad is hidden from DirectInput (HidHide) while another player presses a DirectInput pad.
        PlayerInfo pad = SdlPad(0);
        D_Joystick someoneElse = Joystick(Xbox360Path, 0x045E, 0x028E);

        Assert.Null(GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(someoneElse, true) }, Sdl("", 0x054C, 0x09CC)));

        //Ids SDL doesn't know, or a product GUID without them: the only pressed device is still taken, as before.
        Assert.Same(someoneElse, GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(someoneElse, true) }, Sdl("", 0, 0)));
        D_Joystick noIds = Joystick(Ds4Path);
        Assert.Same(noIds, GamepadIdentity.PickDInputMatch(pad, new[] { pad }, new[] { new DInputCandidate(noIds, true) }, Sdl("", 0x054C, 0x09CC)));
    }

    [Fact]
    public void A_non_xbox_sdl_pad_does_not_guess_an_xinput_view()
    {
        PlayerInfo pad = SdlPad(0);
        DInputCandidate[] twoUnknown = { new DInputCandidate(Joystick(Ds4Path), true), new DInputCandidate(Joystick(Xbox360Path), true) };

        Assert.Null(GamepadIdentity.PickDInputMatch(pad, new[] { pad }, twoUnknown, Sdl("", 0, 0)));
        Assert.NotNull(GamepadIdentity.PickDInputMatch(pad, new[] { pad }, twoUnknown, Sdl("", 0, 0, xbox: true)));
    }

    [Fact]
    public void Without_a_match_an_sdl_pad_is_revealed_by_its_player_index()
    {
        //SDL handlers give the game the SDL player index (SDL2.ini) and none needs the DirectInput identity.
        PlayerInfo pad = SdlPad(2);
        pad.GamepadName = "PS4 Controller";

        IdentifyResult result = GamepadIdentity.Identify(pad, new[] { pad }, new List<DInputCandidate>(), Sdl("", 0x054C, 0x09CC), needsDInputIdentity: false, Now);

        Assert.Equal(IdentifyResult.SlotFallback, result);
        Assert.True(pad.IsInputUsed);
        Assert.Equal(GamepadIdentity.SlotGuid(2), pad.GamepadGuid);
        Assert.Equal("PS4 Controller", pad.GamepadName);
    }

    [Fact]
    public void An_sdl_match_copies_the_directinput_identity()
    {
        PlayerInfo pad = SdlPad(0);
        D_Joystick ds4 = Joystick(Ds4Path, 0x054C, 0x09CC, "Wireless Controller");

        IdentifyResult result = GamepadIdentity.Identify(pad, new[] { pad }, new[] { new DInputCandidate(ds4, false) }, Sdl(Ds4Path, 0x054C, 0x09CC), needsDInputIdentity: true, Now);

        Assert.Equal(IdentifyResult.Matched, result);
        Assert.True(pad.IsInputUsed);
        Assert.Equal(ds4.InstanceGuid, pad.GamepadGuid);
        Assert.Equal(@"HID\VID_054C&PID_09CC&MI_03\8&1F5C3C5A&0&0000", pad.HIDDeviceID[0]);
    }

    [Fact]
    public void Refreshing_directinput_keeps_a_single_watcher_thread()
    {
        //"Refresh devices" used to leave the previous watcher running: one more thread per refresh, all changing the
        //device lists without a lock (an exception there ends the process).
        SynchronizationContext context = new SynchronizationContext();
        DInputManager.Init(context);

        for (int i = 0; i < 5; i++)
        {
            DInputManager.Refresh(context);
        }

        Stopwatch watch = Stopwatch.StartNew();

        while (DInputManager.ActiveWatchers != 1 && watch.ElapsedMilliseconds < 5000)
        {
            Thread.Sleep(50);
        }

        Assert.Equal(1, DInputManager.ActiveWatchers);

        //The list is a copy: iterating it while the watcher works is safe.
        foreach (D_Joystick joystick in DInputManager.D_JoysticksList)
        {
            Assert.NotNull(joystick);
        }
    }
}
