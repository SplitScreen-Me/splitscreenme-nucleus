using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Nucleus.Gaming;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.ProtoInput;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Setup;

/// <summary>
/// Several gamepads in one instance (handlers with Game.PlayersPerInstance above 1): a placed gamepad hosts guest
/// gamepads, and the launch hands their controller indexes to its instance. Drives the real BoundsFunctions commands on
/// one 1920x1080 screen, without hardware and without the handler script (see <see cref="SetupEngineHarness"/>).
/// </summary>
[Collection(SetupEngineCollection.Name)]
public class GuestsTests
{
    private static PlayerInfo XInputPad(int slot) => new PlayerInfo { IsXInput = true, GamepadId = slot, IsInputUsed = true, DeviceKey = "xinput:" + slot };

    [Fact]
    public void A_host_moved_to_an_empty_slot_keeps_its_guests()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 4, UserScreenType.DualVertical);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo guest = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AddGuest(host, guest));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 1));

        Assert.Equal(setup.Slot(1), host.MonitorBounds);
        Assert.Equal(new[] { guest }, host.InstanceGuests);
    }

    [Fact]
    public void A_host_moved_to_a_point_of_a_manual_screen_keeps_its_guests()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo guest = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AddGuest(host, guest));
        setup.Screen.Type = UserScreenType.Manual;

        Assert.Null(BoundsFunctions.AssignAt(host, 0, 0.1, 0.1));

        Assert.Equal(new[] { guest }, host.InstanceGuests);
    }

    [Fact]
    public void Swapping_two_hosts_keeps_the_guests_of_each()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.DualVertical);
        PlayerInfo first = setup.Add(XInputPad(0));
        PlayerInfo second = setup.Add(XInputPad(1));
        PlayerInfo firstGuest = setup.Add(XInputPad(2));
        PlayerInfo secondGuest = setup.Add(XInputPad(3));

        Assert.Null(BoundsFunctions.AssignToSlot(first, 0, 0));
        Assert.Null(BoundsFunctions.AssignToSlot(second, 0, 1));
        Assert.Null(BoundsFunctions.AddGuest(first, firstGuest));
        Assert.Null(BoundsFunctions.AddGuest(second, secondGuest));

        Assert.Null(BoundsFunctions.AssignToSlot(first, 0, 1));

        Assert.Equal(setup.Slot(1), first.MonitorBounds);
        Assert.Equal(setup.Slot(0), second.MonitorBounds);
        Assert.Equal(new[] { firstGuest }, first.InstanceGuests);
        Assert.Equal(new[] { secondGuest }, second.InstanceGuests);
    }

    [Fact]
    public void A_guest_leaves_its_slot_and_shares_the_instance_of_its_host()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.DualVertical);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo other = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AssignToSlot(other, 0, 1));

        Assert.Null(BoundsFunctions.AddGuest(host, other));

        Assert.Equal(new[] { host }, GameProfile.AssignedDevices);//one instance
        Assert.Equal(1, GameProfile.TotalAssignedPlayers);
        Assert.Equal(-1, other.ScreenIndex);
        Assert.Equal(new[] { other }, host.InstanceGuests);
    }

    [Fact]
    public void The_guest_limit_counts_every_guest()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        //An SDL pad SDL gives no player index: its GamepadId is -1.
        PlayerInfo unindexed = setup.Add(new PlayerInfo { IsSDL2 = true, GamepadId = -1, IsInputUsed = true, DeviceKey = "sdl:h1" });
        PlayerInfo third = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AddGuest(host, unindexed));

        Assert.Equal(BoundsFunctions.Refusal.GuestLimitReached, BoundsFunctions.AddGuest(host, third));
        Assert.Equal(new[] { unindexed }, host.InstanceGuests);
    }

    [Fact]
    public void Guests_are_refused_when_the_handler_runs_one_player_per_instance()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 0, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo guest = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));

        Assert.Equal(BoundsFunctions.Refusal.GuestsNotSupported, BoundsFunctions.AddGuest(host, guest));
        Assert.Empty(host.InstanceGuests);
    }

    [Fact]
    public void A_guest_must_be_a_revealed_gamepad_of_no_other_instance()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 4, UserScreenType.DualVertical);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo otherHost = setup.Add(XInputPad(1));
        PlayerInfo guest = setup.Add(XInputPad(2));
        PlayerInfo hidden = setup.Add(new PlayerInfo { IsXInput = true, GamepadId = 3, DeviceKey = "xinput:3" });
        PlayerInfo keyboard = setup.Add(new PlayerInfo { IsKeyboardPlayer = true, IsInputUsed = true, DeviceKey = "kb:single" });

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AssignToSlot(otherHost, 0, 1));
        Assert.Null(BoundsFunctions.AddGuest(otherHost, guest));

        Assert.Equal(BoundsFunctions.Refusal.AlreadyGuest, BoundsFunctions.AddGuest(host, guest));
        Assert.Equal(BoundsFunctions.Refusal.DeviceNotRevealed, BoundsFunctions.AddGuest(host, hidden));
        Assert.Equal(BoundsFunctions.Refusal.NotAGamepad, BoundsFunctions.AddGuest(host, keyboard));
        Assert.Equal(BoundsFunctions.Refusal.NotAGamepad, BoundsFunctions.AddGuest(host, host));
        Assert.Equal(BoundsFunctions.Refusal.NotAssigned, BoundsFunctions.AddGuest(guest, hidden));
        Assert.Empty(host.InstanceGuests);
    }

    [Fact]
    public void Removing_a_host_sends_its_guests_back_to_the_tray()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo guest = setup.Add(XInputPad(1));

        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        Assert.Null(BoundsFunctions.AddGuest(host, guest));

        Assert.Null(BoundsFunctions.Unassign(host));

        Assert.Empty(host.InstanceGuests);
        Assert.Empty(GameProfile.AssignedDevices);
        Assert.Equal(-1, guest.ScreenIndex);
    }

    [Fact]
    public void With_a_profile_a_joining_gamepad_fills_the_copy_still_waiting_for_its_guests()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo pad = setup.Add(XInputPad(1));
        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        setup.LoadProfile(players: 1);
        host.CurrentMaxGuests = 1;//the profile saved one guest with this player

        Assert.Null(BoundsFunctions.Join(pad));

        Assert.Equal(new[] { pad }, host.InstanceGuests);
        Assert.Equal(-1, pad.ScreenIndex);
    }

    [Fact]
    public void With_a_profile_a_joining_gamepad_is_refused_when_no_copy_waits_for_guests()
    {
        using SetupEngineHarness setup = new SetupEngineHarness(playersPerInstance: 2, UserScreenType.FullScreen);
        PlayerInfo host = setup.Add(XInputPad(0));
        PlayerInfo pad = setup.Add(XInputPad(1));
        Assert.Null(BoundsFunctions.AssignToSlot(host, 0, 0));
        setup.LoadProfile(players: 1);//saved without guests: CurrentMaxGuests 0

        Assert.Equal("full", BoundsFunctions.Join(pad));
        Assert.Empty(host.InstanceGuests);
    }

    [Fact]
    public void ProtoInput_gives_the_instance_its_host_and_guest_controllers()
    {
        GenericGameInfo game = SetupEngineHarness.Handler(playersPerInstance: 4);
        game.ProtoInput = new ProtoInputOptions { MultipleProtoControllers = true };
        PlayerInfo host = XInputPad(2);
        host.InstanceGuests.Add(XInputPad(0));
        host.InstanceGuests.Add(XInputPad(5));

        ProtoInputLauncher.SetProtoControllersIndex(host, game);

        //ProtoInput controller indexes start at 1 (0 = none).
        Assert.Equal((3, 1, 6, 0), (host.ProtoController1, host.ProtoController2, host.ProtoController3, host.ProtoController4));
    }
}

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SetupEngineCollection
{
    /// <summary>Tests that drive the setup engine's statics (BoundsFunctions, GameProfile).</summary>
    public const string Name = "Setup engine statics";
}

/// <summary>
/// Puts the setup engine (BoundsFunctions, GameProfile statics) on one screen with a handler that has only the fields
/// the placement commands read, and restores the previous state on dispose. BoundsFunctions.Initialize would read the
/// real monitors and start the screens watcher, and GenericGameInfo's constructor runs a handler script and writes the
/// game's meta info: the fields they set are set directly instead.
/// </summary>
internal sealed class SetupEngineHarness : IDisposable
{
    private static readonly FieldInfo BoundsProfile = typeof(BoundsFunctions).GetField("profile", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly FieldInfo BoundsGame = typeof(BoundsFunctions).GetField("userGameInfo", BindingFlags.NonPublic | BindingFlags.Static)!;
    private static readonly FieldInfo DeviceList = typeof(GameProfile).GetField("deviceList", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private readonly object? savedProfile = BoundsProfile.GetValue(null);
    private readonly object? savedGame = BoundsGame.GetValue(null);
    private readonly UserScreen[] savedScreens = BoundsFunctions.Screens;
    private readonly GameProfile savedInstance = GameProfile.Instance;
    private readonly Action<Action> savedDispatch = DevicesFunctions.Dispatch;

    public readonly GameProfile Profile = new GameProfile();
    public readonly UserScreen Screen;

    public SetupEngineHarness(int playersPerInstance, UserScreenType layout)
    {
        UserGameInfo game = new UserGameInfo();
        game.InitializeDefault(Handler(playersPerInstance), @"C:\Games\Test\test.exe");

        DeviceList.SetValue(Profile, new List<PlayerInfo>());
        Screen = new UserScreen(new Rectangle(0, 0, 1920, 1080)) { Index = 0, UIBounds = new RectangleF(10, 20, 192, 108) };
        Screen.Type = layout;

        DevicesFunctions.Dispatch = _ => { };
        BoundsProfile.SetValue(null, Profile);
        BoundsGame.SetValue(null, game);
        BoundsFunctions.Screens = new[] { Screen };
        GameProfile.Instance = Profile;
        ClearAssignments();
    }

    /// <summary>A handler with the given players per instance and nothing else set.</summary>
    public static GenericGameInfo Handler(int playersPerInstance)
    {
        GenericGameInfo game = (GenericGameInfo)FormatterServices.GetUninitializedObject(typeof(GenericGameInfo));
        game.GUID = "Guests test";
        game.PlayersPerInstance = playersPerInstance;
        return game;
    }

    /// <summary>A detected device, in the tray.</summary>
    public PlayerInfo Add(PlayerInfo device)
    {
        Profile.DevicesList.Add(device);
        return device;
    }

    /// <summary>Desktop bounds of a slot of the screen.</summary>
    public Rectangle Slot(int index) => Screen.SubScreensBounds.ElementAt(index).Key;

    /// <summary>
    /// Makes a profile of this many players look loaded, every one of them already matched to a placed device (their
    /// expected places are the slots of the screen, in order).
    /// </summary>
    public void LoadProfile(int players)
    {
        TotalProfilePlayers.SetValue(null, players);
        GameProfile.TotalAssignedPlayers = players;

        for (int i = 0; i < players; i++)
        {
            KeyValuePair<Rectangle, RectangleF> slot = Screen.SubScreensBounds.ElementAt(i);
            GameProfile.ProfilePlayersList.Add(new PlayerInfo { IsXInput = true, ScreenIndex = 0 });
            GameProfile.GhostBounds.Add((slot.Key, slot.Value));
        }
    }

    private static readonly FieldInfo TotalProfilePlayers = typeof(GameProfile).GetField("totalProfilePlayers", BindingFlags.NonPublic | BindingFlags.Static)!;

    private static void ClearAssignments()
    {
        GameProfile.AssignedDevices.Clear();
        GameProfile.DevicesToMerge.Clear();
        GameProfile.TotalAssignedPlayers = 0;
        GameProfile.ProfilePlayersList.Clear();
        GameProfile.GhostBounds.Clear();
        TotalProfilePlayers.SetValue(null, 0);
    }

    public void Dispose()
    {
        ClearAssignments();
        BoundsProfile.SetValue(null, savedProfile);
        BoundsGame.SetValue(null, savedGame);
        BoundsFunctions.Screens = savedScreens;
        GameProfile.Instance = savedInstance;
        DevicesFunctions.Dispatch = savedDispatch;
    }
}
