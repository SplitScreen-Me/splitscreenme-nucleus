using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Settings;
using Nucleus.Coop.Services.Setup;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

/// <summary>
/// Texts built by the services follow the language in use (docs/webui/i18n.md, "Texts built by the host"): they are
/// built when asked for, with English for the keys a translation lacks.
/// </summary>
[Collection(GlobalLanguageCollection.Name)]
public sealed class HostTextsTests : IDisposable
{
    private const string French = @"{
  ""_meta"": { ""locale"": ""fr"", ""englishName"": ""French"", ""nativeName"": ""Français"" },
  ""host"": {
    ""settings"": {
      ""sections"": { ""players"": { ""title"": ""Joueurs"" } },
      ""fields"": {
        ""misc"": { ""useNicknames"": { ""label"": ""Utiliser les pseudos en jeu"" } },
        ""audio"": { ""instance"": { ""label"": ""Instance n° {number}"" } }
      },
      ""colors"": { ""darkBlue"": ""Bleu foncé"" },
      ""gamepadButtons"": { ""cross"": ""Croix"" },
      ""rules"": { ""gamepadAssignment"": ""Ces deux options s'excluent."" },
      ""reminderImage"": { ""rows"": { ""closeApp"": ""Fermer Nucleus"" } },
      ""errors"": { ""maxLength_one"": ""{count} caractère au plus."", ""maxLength_other"": ""{count} caractères au plus."" }
    },
    ""setup"": {
      ""steps"": { ""players"": ""Placer les joueurs"" },
      ""devices"": { ""keyboard"": ""Clavier"" }
    }
  }
}";

    private readonly Translator previous;
    private readonly LanguageFolder folder;

    public HostTextsTests()
    {
        previous = I18n.Current;
        folder = new LanguageFolder(("en.json", I18n.BuiltInEnglish()!), ("fr.json", French));
        I18n.Use(folder.Translator());
    }

    public void Dispose()
    {
        I18n.Use(previous);
        folder.Dispose();
    }

    [Fact]
    public void SettingsTextsFollowTheLanguage()
    {
        Assert.True(I18n.Current.SetLanguage("fr"));

        Assert.Equal("Joueurs", SettingsCatalog.Sections.Single(s => s.Id == "players").Title);
        Assert.Equal("Keyboard shortcuts", SettingsCatalog.Sections.Single(s => s.Id == "hotkeys").Title); // not translated yet: English

        List<SettingDefinition> catalog = SettingsCatalog.Create();
        Assert.Equal("Utiliser les pseudos en jeu", catalog.Single(d => d.Field.Key == "misc.useNicknames").Field.Label);
        Assert.Equal("Utiliser les pseudos en jeu", catalog.Single(d => d.Field.Key == "misc.useNicknames").Rule.Label);
        Assert.Equal("Instance n° 3", catalog.Single(d => d.Field.Key == "audio.instance3").Field.Label);
        Assert.Contains(catalog.Single(d => d.Field.Key == "layout.splitDivColor").Field.Choices, c => c.Value == "Dark Blue" && c.Label == "Bleu foncé");

        Assert.Equal("Ces deux options s'excluent.", SettingsCatalog.ExclusiveRules[0].Message);
        Assert.Equal(("Fermer Nucleus", "hotkeys.closeApp"), SettingsCatalog.KeyboardReminderRows[0]);
        Assert.Equal("Alt+Tab", SettingsCatalog.GamepadReminderRows.Single(r => r.Key == "gamepad.releaseCursor").Label);

        // Button names: the descriptive ones are translated, the ones printed on the gamepads are not.
        Assert.Equal("Croix", GamepadComboFormat.Name(4096, "Playstation"));
        Assert.Equal("A", GamepadComboFormat.Name(4096, "Xbox"));

        I18n.Current.SetLanguage("en");
        Assert.Equal("Players", SettingsCatalog.Sections.Single(s => s.Id == "players").Title);
        Assert.Equal("Instance 3", SettingsCatalog.Create().Single(d => d.Field.Key == "audio.instance3").Field.Label);
    }

    [Fact]
    public void ValidationMessagesFollowTheLanguageAndItsPlurals()
    {
        var rules = new List<SettingRule> { new SettingRule { Key = "misc.text", Label = "Text", Type = SettingType.Password, MaxLength = 1 } };
        var current = new Dictionary<string, string> { ["misc.text"] = "" };

        I18n.Current.SetLanguage("fr");
        ValidationError error = SettingsValidator.Validate(new Dictionary<string, string> { ["misc.text"] = "abc" }, current, rules, new ExclusiveRule[0]).Single();
        Assert.Equal("1 caractère au plus.", error.Message);

        I18n.Current.SetLanguage("en");
        error = SettingsValidator.Validate(new Dictionary<string, string> { ["misc.text"] = "abc" }, current, rules, new ExclusiveRule[0]).Single();
        Assert.Equal("Use at most 1 character.", error.Message);
    }

    [Fact]
    public void SetupTextsFollowTheLanguage()
    {
        I18n.Current.SetLanguage("fr");
        Assert.Equal("Placer les joueurs", SetupFlowRules.PlayersTitle);
        Assert.Equal("Options", SetupFlowRules.OptionsTitle);
        Assert.Equal("Clavier (046D:C539)", DeviceIds.Name(InputType.KB, 100, null, @"\\?\HID#VID_046D&PID_C539&MI_00#7&1eb#{884b96c3}"));
        Assert.Equal("Keyboard and mouse", DeviceIds.Name(InputType.SingleKB, 99, null, null));
    }

    [Fact]
    public void GameMenuTextsArePlainEnglishByDefault()
    {
        I18n.Current.SetLanguage("en");
        GameMenu menu = GameMenuBuilder.Build(new GameMenuContext { GameId = "g", GameName = "Some Game" });
        GameMenuItem remove = menu.Items.Single(i => i.Id == GameMenuBuilder.Ids.Remove);
        Assert.Equal("Remove Game From List", remove.Label);
        Assert.Equal("Are you sure you want to remove Some Game from your library? The handler stays installed.", remove.Confirm);
    }
}

/// <summary>Tests that change the language in use (<see cref="I18n"/>, static) never run in parallel with the others.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class GlobalLanguageCollection
{
    public const string Name = "Global language";
}
