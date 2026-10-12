using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Library;

public class PlaytimeParserTests
{
    [Theory]
    [InlineData(null, 0)]
    [InlineData("", 0)]
    [InlineData("...", 0)]
    [InlineData("3620", 3620)]
    [InlineData(" 40 ", 40)]
    [InlineData("1234.0", 1234)]
    [InlineData("-5", 0)]
    [InlineData("01h:00m:20s", 3620)]
    [InlineData("100h:05m:00s", 360300)]
    [InlineData("garbage", 0)]
    public void Seconds(string? stored, long expected) => Assert.Equal(expected, PlaytimeParser.ParseSeconds(stored));

    [Fact]
    public void Last_played_in_the_current_culture()
    {
        CultureInfo french = CultureInfo.GetCultureInfo("fr-FR");
        var local = new DateTime(2024, 3, 25, 21, 4, 5, DateTimeKind.Local);

        DateTime? parsed = PlaytimeParser.ParseLastPlayed(local.ToString(french), french);

        Assert.Equal(local.ToUniversalTime(), parsed);
        Assert.Equal(DateTimeKind.Utc, parsed!.Value.Kind);
    }

    [Fact]
    public void Last_played_written_in_another_culture()
    {
        var local = new DateTime(2024, 3, 25, 21, 4, 5, DateTimeKind.Local);
        string english = local.ToString(CultureInfo.GetCultureInfo("en-US")); // "3/25/2024 9:04:05 PM"

        // Day 25 can't be a month: the French parse fails and the US fallback reads it.
        DateTime? parsed = PlaytimeParser.ParseLastPlayed(english, CultureInfo.GetCultureInfo("fr-FR"));

        Assert.Equal(local.ToUniversalTime(), parsed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("...")]
    [InlineData("not a date")]
    public void Unknown_last_played(string? stored) => Assert.Null(PlaytimeParser.ParseLastPlayed(stored, CultureInfo.InvariantCulture));
}

public class LibraryPreferencesMapperTests
{
    [Fact]
    public void Empty_setting_is_name_order_without_filters()
    {
        // Settings.ini "GameSortingOpt=" loads as a list with one empty string.
        LibraryPreferences preferences = LibraryPreferencesMapper.FromLegacy(new List<string> { "" }, false);

        Assert.Equal(LibrarySort.Name, preferences.Sort);
        Assert.False(preferences.FilterGamepads || preferences.FilterKeyboard || preferences.FilterMultipleKeyboards || preferences.FavoritesOnly);
        Assert.Empty(LibraryPreferencesMapper.ToLegacy(preferences));
    }

    [Theory]
    [InlineData("Sort_1", LibrarySort.NameDescending)]
    [InlineData("Sort_2", LibrarySort.MaxPlayers)]
    [InlineData("Sort_3", LibrarySort.Playtime)]
    [InlineData("Sort_4", LibrarySort.LastPlayed)]
    [InlineData("Sort_1,Sort_2,Sort_4,Sort_3", LibrarySort.Playtime)]
    [InlineData("Sort_1,Sort_2,Sort_4", LibrarySort.LastPlayed)]
    [InlineData("Sort_1,Sort_2", LibrarySort.MaxPlayers)]
    public void Legacy_sort_tokens(string tokens, LibrarySort expected) =>
        Assert.Equal(expected, LibraryPreferencesMapper.FromLegacy(tokens.Split(','), false).Sort);

    [Fact]
    public void Round_trip_keeps_the_legacy_panel_order()
    {
        var preferences = new LibraryPreferences
        {
            Sort = LibrarySort.LastPlayed,
            FavoritesOnly = true,
            FilterGamepads = true,
            FilterMultipleKeyboards = true,
        };

        List<string> tokens = LibraryPreferencesMapper.ToLegacy(preferences);
        Assert.Equal(new[] { "Sort_4", "Show_Type1", "Show_Type3" }, tokens);

        LibraryPreferences back = LibraryPreferencesMapper.FromLegacy(tokens, true);
        Assert.Equal(LibrarySort.LastPlayed, back.Sort);
        Assert.True(back.FavoritesOnly && back.FilterGamepads && back.FilterMultipleKeyboards);
        Assert.False(back.FilterKeyboard);
    }

    [Fact]
    public void Unknown_tokens_are_ignored() =>
        Assert.Equal(LibrarySort.Name, LibraryPreferencesMapper.FromLegacy(new[] { "Update", " ", "Show_Type9" }, false).Sort);
}

public class GameInputRulesTests
{
    [Fact]
    public void Sdl2_shows_both_gamepad_kinds()
    {
        GameInputs inputs = GameInputRules.ToInputs(new HandlerInputFlags { Sdl2Enabled = true });
        Assert.True(inputs.XInput);
        Assert.True(inputs.DInput);
    }

    [Fact]
    public void Rerouted_xinput_counts_as_dinput()
    {
        GameInputs inputs = GameInputRules.ToInputs(new HandlerInputFlags { XInputEnabled = true, XInputReroute = true });
        Assert.False(inputs.XInput);
        Assert.True(inputs.DInput);
    }

    [Fact]
    public void Keyboards()
    {
        GameInputs inputs = GameInputRules.ToInputs(new HandlerInputFlags { SupportsKeyboard = true, SupportsMultipleKeyboardsAndMice = true });
        Assert.True(inputs.Keyboard && inputs.MultipleKeyboards);
        Assert.False(inputs.XInput || inputs.DInput);
        Assert.False(GameInputRules.SupportsGamepads(new HandlerInputFlags { SupportsKeyboard = true }));
        Assert.True(GameInputRules.SupportsGamepads(new HandlerInputFlags { ProtoDinputDeviceHook = true }));
    }
}

public class InstalledGamesScannerTests
{
    [Fact]
    public void Vdf_nested_blocks_escapes_and_comments()
    {
        const string text = "// comment\n\"libraryfolders\"\n{\n\t\"0\"\n\t{\n\t\t\"path\"\t\t\"C:\\\\Program Files (x86)\\\\Steam\"\n\t\t\"apps\" { \"228980\" \"1\" }\n\t}\n\t\"1\" { \"path\" \"D:\\\\SteamLibrary\" }\n}";

        Vdf.Node root = Vdf.Parse(text);

        Assert.Equal(@"C:\Program Files (x86)\Steam", root["libraryfolders"]!["0"]!.Get("path"));
        Assert.Equal("1", root["LIBRARYFOLDERS"]!["0"]!["apps"]!.Get("228980"));
    }

    [Fact]
    public void Steam_libraries_new_and_old_formats()
    {
        const string modern = "\"libraryfolders\" { \"0\" { \"path\" \"C:\\\\Steam\" } \"1\" { \"path\" \"D:\\\\SteamLibrary\" } }";
        const string legacy = "\"LibraryFolders\" { \"TimeNextStatsReport\" \"123\" \"ContentStatsID\" \"-1\" \"1\" \"E:\\\\Games\\\\Steam\" }";

        Assert.Equal(new[] { @"C:\Steam", @"D:\SteamLibrary" }, InstalledGamesScanner.SteamLibraries(@"C:\Steam", modern));
        Assert.Equal(new[] { @"C:\Steam", @"E:\Games\Steam" }, InstalledGamesScanner.SteamLibraries(@"C:\Steam", legacy));
        Assert.Equal(new[] { @"C:\Steam" }, InstalledGamesScanner.SteamLibraries(@"C:\Steam", null));
    }

    [Fact]
    public void Steam_manifest()
    {
        const string acf = "\"AppState\"\n{\n\t\"appid\"\t\t\"1245620\"\n\t\"Universe\"\t\t\"1\"\n\t\"name\"\t\t\"ELDEN RING\"\n\t\"installdir\"\t\t\"ELDEN RING\"\n}";

        InstalledGame game = InstalledGamesScanner.ParseSteamManifest(acf, @"D:\SteamLibrary\steamapps")!;

        Assert.Equal(GameStore.Steam, game.Store);
        Assert.Equal("1245620", game.StoreId);
        Assert.Equal("ELDEN RING", game.Name);
        Assert.Equal(@"D:\SteamLibrary\steamapps\common\ELDEN RING", game.InstallFolder);
        Assert.Null(InstalledGamesScanner.ParseSteamManifest("\"AppState\" { \"appid\" \"228980\" \"installdir\" \"Steamworks Shared\" }", @"C:\x"));
    }

    [Fact]
    public void Epic_manifest()
    {
        const string json = "{\"FormatVersion\":0,\"bIsIncompleteInstall\":false,\"AppName\":\"Fortnite\",\"DisplayName\":\"Fortnite\",\"InstallLocation\":\"C:/Program Files/Epic Games/Fortnite\"}";

        InstalledGame game = InstalledGamesScanner.ParseEpicManifest(json)!;

        Assert.Equal(GameStore.Epic, game.Store);
        Assert.Equal("Fortnite", game.StoreId);
        Assert.Equal(@"C:\Program Files\Epic Games\Fortnite", game.InstallFolder);
        Assert.Null(InstalledGamesScanner.ParseEpicManifest("{\"AppName\":\"X\",\"InstallLocation\":\"C:/X\",\"bIsIncompleteInstall\":true}"));
    }

    [Fact]
    public void Finds_the_shallowest_executable_and_skips_redistributables()
    {
        string root = Path.Combine(Path.GetTempPath(), "nc-find-" + Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "_CommonRedist"));
            File.WriteAllText(Path.Combine(root, "_CommonRedist", "Game.exe"), "");
            Directory.CreateDirectory(Path.Combine(root, "bin", "win64"));
            File.WriteAllText(Path.Combine(root, "bin", "win64", "Game.exe"), "");
            Directory.CreateDirectory(Path.Combine(root, "a", "b", "c", "d", "e", "f", "g"));
            File.WriteAllText(Path.Combine(root, "a", "b", "c", "d", "e", "f", "g", "Deep.exe"), "");

            Assert.Equal(Path.Combine(root, "bin", "win64", "Game.exe"), ExecutableFinder.Find(root, "game.EXE", CancellationToken.None), ignoreCase: true);
            Assert.Null(ExecutableFinder.Find(root, "Deep.exe", CancellationToken.None)); // deeper than the limit
            Assert.Null(ExecutableFinder.Find(root, "Missing.exe", CancellationToken.None));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }
}
