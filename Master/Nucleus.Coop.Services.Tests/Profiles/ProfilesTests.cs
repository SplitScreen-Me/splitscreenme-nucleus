using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Profiles;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Profiles;

public class ProfileSummaryTests
{
    private const string Profile = @"{
      ""Title"": ""Couch"", ""Notes"": ""friday"", ""Player(s)"": 3, ""AutoPlay"": { ""Enabled"": true },
      ""Data"": [
        { ""PlayerID"": 1, ""Nickname"": ""Bob"", ""GamepadGuid"": ""00000000-0000-0000-0000-200000000002"", ""IsDInput"": false, ""IsXInput"": true, ""IsSDL2"": false,
          ""Guests"": { ""GUIDS"": ""00000000-0000-0000-0000-200000000003,00000000-0000-0000-0000-200000000004,"" }, ""ScreenIndex"": 1 },
        { ""PlayerID"": 0, ""Nickname"": ""Ann"", ""GamepadGuid"": ""10000000-1000-1000-1000-100000000000"", ""IsDInput"": false, ""IsXInput"": false, ""ScreenIndex"": 0 },
        { ""PlayerID"": 2, ""Nickname"": ""Cid"", ""GamepadGuid"": ""00000000-0000-0000-0000-000000000000"", ""IsDInput"": false, ""IsXInput"": false, ""IsRawMouse"": true,
          ""HIDDeviceID"": [ ""kb"", ""mouse"" ], ""ScreenIndex"": 0 }
      ]
    }";

    [Fact]
    public void Summary_reads_the_profile_file()
    {
        ProfileSummary summary = ProfilesService.Summary(4, JObject.Parse(Profile));

        Assert.Equal(4, summary.Id);
        Assert.Equal("Couch", summary.Title);
        Assert.Equal("friday", summary.Notes);
        Assert.True(summary.AutoPlay);
        Assert.Equal(3, summary.Players);
        Assert.Equal(1, summary.Gamepads);
        Assert.Equal(2, summary.KeyboardMice);
    }

    [Fact]
    public void Players_are_listed_in_player_order_with_their_device_and_guests()
    {
        List<ProfilePlayer> players = ProfilesService.Summary(1, JObject.Parse(Profile)).PlayerList;

        Assert.Equal(new[] { "Ann", "Bob", "Cid" }, players.Select(p => p.Nickname));
        Assert.Equal(new[] { DeviceKind.Keyboard, DeviceKind.XInput, DeviceKind.KeyboardMouse }, players.Select(p => p.Device));
        Assert.Equal(2, players[1].Guests);
        Assert.Equal(1, players[1].ScreenIndex);
    }

    [Fact]
    public void An_unreadable_profile_is_still_listed()
    {
        ProfileSummary summary = ProfilesService.Summary(2, null);

        Assert.Equal(2, summary.Id);
        Assert.Empty(summary.PlayerList);
        Assert.NotEmpty(summary.Notes);
    }
}

public class ProfileSettingsValidationTests
{
    private static ProfileSettingsModel Valid(int slots = 2)
    {
        ProfileSettingsModel model = new ProfileSettingsModel { CustomLayoutHor = 1, CustomLayoutVer = 1, CustomLayoutMax = 4, MergerResolution = "1920X1080" };

        for (int i = 0; i < slots; i++)
        {
            model.Slots.Add(new ProfileSlotModel { Nickname = "P" + i, SteamId = "7656119902312543" + i });
        }

        return model;
    }

    private static string[] Fields(ProfileSettingsModel model) => model.Validate().Select(e => e.Field + (e.Index.HasValue ? "[" + e.Index + "]" : "")).ToArray();

    [Fact]
    public void Valid_settings_have_no_error()
    {
        Assert.Empty(Valid().Validate());
    }

    [Fact]
    public void Nicknames_are_required()
    {
        ProfileSettingsModel model = Valid();
        model.Slots[1].Nickname = " ";

        Assert.Equal(new[] { "slots.nickname[1]" }, Fields(model));
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("-1", true)]
    [InlineData("76561199023125438", true)]
    [InlineData("7656119902312543", false)]
    [InlineData("7656119902312543x", false)]
    public void Steam_ids_are_empty_minus_one_or_17_digits(string steamId, bool valid)
    {
        ProfileSettingsModel model = Valid(1);
        model.Slots[0].SteamId = steamId;

        Assert.Equal(valid, model.Validate().Count == 0);
    }

    [Fact]
    public void Steam_ids_must_differ_between_players()
    {
        ProfileSettingsModel model = Valid();
        model.Slots[1].SteamId = model.Slots[0].SteamId;

        Assert.Equal(new[] { "slots.steamId[0]", "slots.steamId[1]" }, Fields(model));
    }

    [Fact]
    public void Processor_settings_are_bounded_by_the_core_count()
    {
        int tooMany = Environment.ProcessorCount + 1;
        ProfileSettingsModel model = Valid();
        model.Slots[0].IdealProcessor = tooMany.ToString();
        model.Slots[1].Affinity = "1," + tooMany;

        Assert.Equal(new[] { "slots.idealProcessor[0]", "slots.affinity[1]" }, Fields(model));

        model.Slots[0].IdealProcessor = "*";
        model.Slots[1].Affinity = "1,2";
        model.Slots[1].PriorityClass = "Turbo";

        Assert.Equal(new[] { "slots.priorityClass[1]" }, Fields(model));
    }

    [Fact]
    public void Shared_settings_are_checked()
    {
        ProfileSettingsModel model = Valid();
        model.CustomLayoutHor = 6;
        model.CustomLayoutVer = -1;
        model.CustomLayoutMax = 0;
        model.WindowsSetupTimingMs = -5;
        model.SplitDivColor = "Ultraviolet";
        model.MergerResolution = "big";

        Assert.Equal(new[] { "customLayoutHorizontalLines", "customLayoutVerticalLines", "customLayoutMaxPlayers", "windowSetupDelayMs", "splitDividerColor", "mergerResolution" }, Fields(model));
    }
}
