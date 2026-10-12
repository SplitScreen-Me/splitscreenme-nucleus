using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Settings;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Settings;

public class SettingsValidatorTests
{
    private static readonly List<SettingRule> Rules = new List<SettingRule>
    {
        new SettingRule { Key = "hotkeys.closeApp", Label = "Close Nucleus", Type = SettingType.Hotkey, UniqueGroup = "keyboard" },
        new SettingRule { Key = "hotkeys.stopSession", Label = "Stop session", Type = SettingType.Hotkey, UniqueGroup = "keyboard" },
        new SettingRule { Key = "gamepad.close", Label = "Close Nucleus", Type = SettingType.GamepadCombo, ComboSize = 2, UniqueGroup = "pad" },
        new SettingRule { Key = "gamepad.stop", Label = "Stop session", Type = SettingType.GamepadCombo, ComboSize = 2, UniqueGroup = "pad" },
        new SettingRule { Key = "gamepad.leftClick", Label = "Left click", Type = SettingType.GamepadCombo, ComboSize = 1 },
        new SettingRule { Key = "layout.lines", Label = "Lines", Type = SettingType.Number, Min = 0, Max = 5 },
        new SettingRule { Key = "misc.lang", Label = "Language", Type = SettingType.Choice, AllowedValues = new[] { "English", "French" } },
        new SettingRule { Key = "misc.password", Label = "Password", Type = SettingType.Text, MaxLength = 5 },
        new SettingRule { Key = "a", Label = "A", Type = SettingType.Toggle },
        new SettingRule { Key = "b", Label = "B", Type = SettingType.Toggle },
    };

    private static readonly List<ExclusiveRule> Exclusive = new List<ExclusiveRule> { new ExclusiveRule("a", new[] { "b" }, "A and B can't both be on.") };

    private static readonly Dictionary<string, string> Current = new Dictionary<string, string>
    {
        ["hotkeys.closeApp"] = "Ctrl+Q",
        ["hotkeys.stopSession"] = "Ctrl+E",
        ["gamepad.close"] = "1024+16",
        ["gamepad.stop"] = "1024+32",
        ["gamepad.leftClick"] = "9999",
        ["layout.lines"] = "1",
        ["misc.lang"] = "Klingon", // not offered anymore, still accepted as is
        ["misc.password"] = "12345",
        ["a"] = "false",
        ["b"] = "true",
    };

    private static List<ValidationError> Validate(Dictionary<string, string> changes) => SettingsValidator.Validate(changes, Current, Rules, Exclusive);

    [Fact]
    public void Valid_changes_pass() =>
        Assert.Empty(Validate(new Dictionary<string, string> { ["hotkeys.closeApp"] = "alt+x", ["layout.lines"] = "5", ["misc.lang"] = "French", ["gamepad.leftClick"] = "10000" }));

    [Fact]
    public void Unknown_key() => Assert.Equal("nope", Validate(new Dictionary<string, string> { ["nope"] = "1" }).Single().Field);

    [Theory]
    [InlineData("Ctrl+Q", "Ctrl+Q")]
    [InlineData("control + q", "Ctrl+Q")]
    [InlineData("shift+7", "Shift+7")]
    [InlineData("Win+Q", null)]
    [InlineData("Ctrl+F5", null)]
    [InlineData("Ctrl+Alt+K", null)]
    [InlineData("Q", null)]
    [InlineData("Ctrl++", null)]
    public void Hotkeys(string value, string? expected) => Assert.Equal(expected, HotkeyFormat.Normalize(value, out _));

    [Fact]
    public void Duplicate_hotkeys_are_reported_on_the_changed_field()
    {
        ValidationError error = Validate(new Dictionary<string, string> { ["hotkeys.stopSession"] = "ctrl+q" }).Single();
        Assert.Equal("hotkeys.stopSession", error.Field);
        Assert.Contains("Close Nucleus", error.Message);
    }

    [Fact]
    public void Existing_problems_between_untouched_fields_do_not_block_other_saves()
    {
        var current = new Dictionary<string, string>(Current) { ["hotkeys.stopSession"] = "Ctrl+Q" };
        Assert.Empty(SettingsValidator.Validate(new Dictionary<string, string> { ["layout.lines"] = "2" }, current, Rules, Exclusive));
    }

    [Theory]
    [InlineData("16+1024")]   // same pair in another order
    [InlineData("1024+16")]
    public void Gamepad_duplicates_compare_like_the_runtime(string value) =>
        Assert.Equal("gamepad.stop", Validate(new Dictionary<string, string> { ["gamepad.stop"] = value }).Single().Field);

    [Theory]
    [InlineData("1024", 2, false)]
    [InlineData("1024+1024", 2, false)]
    [InlineData("9999+10000", 2, false)]
    [InlineData("1024+3", 2, false)]
    [InlineData("1024+9999", 2, true)]
    [InlineData("512", 1, true)]
    [InlineData("512+1", 1, false)]
    public void Gamepad_combos(string value, int size, bool valid) => Assert.Equal(valid, GamepadComboFormat.Parse(value, size, out _) != null);

    [Theory]
    [InlineData("6")]
    [InlineData("-1")]
    [InlineData("2.5")]
    [InlineData("two")]
    public void Numbers_out_of_range_or_not_whole(string value) => Assert.Single(Validate(new Dictionary<string, string> { ["layout.lines"] = value }));

    [Fact]
    public void Choices_accept_listed_values_and_the_current_one()
    {
        Assert.Empty(Validate(new Dictionary<string, string> { ["misc.lang"] = "Klingon" }));
        Assert.Single(Validate(new Dictionary<string, string> { ["misc.lang"] = "German" }));
    }

    [Fact]
    public void Text_length_and_line_breaks()
    {
        Assert.Single(Validate(new Dictionary<string, string> { ["misc.password"] = "123456" }));
        Assert.Single(Validate(new Dictionary<string, string> { ["misc.password"] = "a\nb" }));
        Assert.Empty(Validate(new Dictionary<string, string> { ["misc.password"] = "" }));
    }

    [Fact]
    public void Exclusive_toggles()
    {
        Assert.Equal("a", Validate(new Dictionary<string, string> { ["a"] = "true" }).Single().Field);
        Assert.Empty(Validate(new Dictionary<string, string> { ["a"] = "true", ["b"] = "false" }));
        Assert.Single(Validate(new Dictionary<string, string> { ["a"] = "yes" }));
    }

    [Fact]
    public void Normalized_forms()
    {
        SettingRule toggle = Rules.First(r => r.Type == SettingType.Toggle);
        Assert.Equal("true", SettingsValidator.Normalize(toggle, "True", null, out _));
        Assert.Equal("16+1024", SettingsValidator.Normalize(Rules.First(r => r.Key == "gamepad.close"), " 16 + 1024 ", null, out _));
    }
}

public class SettingsCatalogTests
{
    private static readonly List<SettingDefinition> Catalog = SettingsCatalog.Create();

    [Fact]
    public void Keys_are_unique_and_every_section_has_fields()
    {
        Assert.Equal(Catalog.Count, Catalog.Select(d => d.Field.Key).Distinct().Count());
        foreach (SettingSectionDefinition section in SettingsCatalog.Sections)
        {
            Assert.Contains(Catalog, d => d.Section == section.Id);
        }

        Assert.All(Catalog, d => Assert.Contains(SettingsCatalog.Sections, s => s.Id == d.Section));
        Assert.All(Catalog, d => Assert.Equal(d.Field.Key, d.Rule.Key));
    }

    [Fact]
    public void Defaults_are_valid_values()
    {
        foreach (SettingDefinition definition in Catalog.Where(d => d.DynamicChoices == null))
        {
            string? normalized = SettingsValidator.Normalize(definition.Rule, definition.Field.Default, null, out string? error);
            Assert.True(normalized != null, $"{definition.Field.Key}: {error}");
        }
    }

    [Fact]
    public void Default_shortcuts_do_not_collide()
    {
        Dictionary<string, string> defaults = Catalog.ToDictionary(d => d.Field.Key, d => d.Field.Default ?? "");
        // Saving every default at once must pass (duplicates and exclusive rules included).
        var changes = Catalog.Where(d => d.DynamicChoices == null).ToDictionary(d => d.Field.Key, d => d.Field.Default ?? "");
        var current = defaults.ToDictionary(p => p.Key, p => p.Key == "layout.mergerResolution" ? "1920X1080" : p.Value);

        Assert.Empty(SettingsValidator.Validate(changes, current, Catalog.Select(d => d.Rule).ToList(), SettingsCatalog.ExclusiveRules));
    }

    [Fact]
    public void Reminder_rows_point_at_catalog_keys()
    {
        Assert.All(SettingsCatalog.KeyboardReminderRows, r => Assert.Contains(Catalog, d => d.Field.Key == r.Key));
        Assert.All(SettingsCatalog.GamepadReminderRows, r => Assert.Contains(Catalog, d => d.Field.Key == r.Key && d.Field.Type == SettingType.GamepadCombo));
    }

    [Fact]
    public void Field_metadata()
    {
        SettingField Field(string key) => Catalog.Single(d => d.Field.Key == key).Field;

        Assert.Equal(SettingType.Password, Field("misc.nucleusAccountPassword").Type);
        Assert.Equal(127, Field("misc.nucleusAccountPassword").MaxLength);
        Assert.Equal(2, Field("gamepad.close").ComboButtons);
        Assert.Equal(1, Field("gamepad.leftClick").ComboButtons);
        Assert.Equal("audio.mode", Field("audio.instance3").EnabledWhen!.Key);
        Assert.Equal(new[] { "false" }, Field("players.assignGamepadsByIndex").VisibleWhen!.Values.ToArray());
        Assert.All(Catalog.Select(d => d.Field).Where(f => f.VisibleWhen != null || f.EnabledWhen != null),
            f => Assert.Contains(Catalog, d => d.Field.Key == (f.VisibleWhen ?? f.EnabledWhen)!.Key));
    }

    [Fact]
    public void Schema_summary()
    {
        var counts = Catalog.GroupBy(d => d.Section).ToDictionary(g => g.Key, g => g.Count());
        Assert.Equal(10, counts["general"]);
        Assert.Equal(2, counts["players"]);
        Assert.Equal(10, counts["hotkeys"]);
        Assert.Equal(17, counts["gamepad"]);
        Assert.Equal(9, counts["audio"]);
        Assert.Equal(11, counts["layout"]);
    }
}

public class PlayerIdentitiesRulesTests
{
    private static List<PlayerIdentity> Current() => Enumerable.Range(1, 32).Select(n => new PlayerIdentity { Number = n }).ToList();

    [Fact]
    public void Empty_values_mean_defaults()
    {
        var (errors, result) = PlayerIdentitiesRules.Validate(new[] { new PlayerIdentity { Number = 3, Nickname = " ", SteamId = "" } }, Current());

        Assert.Empty(errors);
        Assert.Equal(32, result.Count);
        Assert.Equal("Player3", result[2].Nickname);
        Assert.Equal("", result[2].SteamId);
    }

    [Fact]
    public void Nickname_rules()
    {
        var (errors, _) = PlayerIdentitiesRules.Validate(new[]
        {
            new PlayerIdentity { Number = 1, Nickname = "TooLongName" },
            new PlayerIdentity { Number = 2, Nickname = "a/b" },
            new PlayerIdentity { Number = 3, Nickname = "Bob" },
            new PlayerIdentity { Number = 4, Nickname = "bob" },
        }, Current());

        Assert.Equal(new[] { 0, 1, 3 }, errors.Where(e => e.Field == "nickname").Select(e => e.Index!.Value).OrderBy(i => i).ToArray());
        Assert.Contains("player 3", errors.Single(e => e.Index == 3).Message);
    }

    [Fact]
    public void Steam_id_rules()
    {
        var (errors, _) = PlayerIdentitiesRules.Validate(new[]
        {
            new PlayerIdentity { Number = 1, SteamId = "1234" },
            new PlayerIdentity { Number = 2, SteamId = "76561199075562883" },
            new PlayerIdentity { Number = 3, SteamId = "76561199075562883" },
            new PlayerIdentity { Number = 5, SteamId = PlayerIdentitiesRules.DefaultSteamId(6) }, // collides with player 6's default
            new PlayerIdentity { Number = 7, SteamId = "" },
            new PlayerIdentity { Number = 8, SteamId = "" }, // two empty ids are not duplicates
        }, Current());

        Assert.Equal(new[] { 0, 2, 3 }, errors.Select(e => e.Index!.Value).OrderBy(i => i).ToArray());
        Assert.All(errors, e => Assert.Equal("steamId", e.Field));
    }

    [Fact]
    public void Numbers_must_be_valid_and_unique()
    {
        var (errors, _) = PlayerIdentitiesRules.Validate(new[]
        {
            new PlayerIdentity { Number = 0 },
            new PlayerIdentity { Number = 33 },
            new PlayerIdentity { Number = 2 },
            new PlayerIdentity { Number = 2 },
        }, Current());

        Assert.Equal(new[] { 0, 1, 3 }, errors.Select(e => e.Index!.Value).ToArray());
    }

    [Fact]
    public void Defaults_match_the_identity_cache() =>
        Assert.Equal("76561199023125438", PlayerIdentitiesRules.DefaultSteamId(1));
}
