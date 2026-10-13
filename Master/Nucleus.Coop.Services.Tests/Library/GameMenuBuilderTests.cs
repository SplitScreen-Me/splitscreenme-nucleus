using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Library;

public class GameMenuBuilderTests
{
    private static GameMenuContext XInputGame() => new GameMenuContext
    {
        GameId = "game-1",
        GameName = "My Game",
        HasNotes = true,
        CheckUpdate = true,
        Inputs = new HandlerInputFlags { XInputEnabled = true },
    };

    private static IEnumerable<GameMenuItem> Flatten(IEnumerable<GameMenuItem> items) =>
        items.SelectMany(i => new[] { i }.Concat(Flatten(i.Children)));

    private static GameMenuItem? Item(GameMenu menu, string id) => Flatten(menu.Items).FirstOrDefault(i => i.Id == id);

    [Fact]
    public void Static_items_follow_the_legacy_order()
    {
        GameMenu menu = GameMenuBuilder.Build(XInputGame());
        string[] ids = menu.Items.Where(i => i.Kind != GameMenuItemKind.Separator).Select(i => i.Id).ToArray();

        Assert.Equal(new[]
        {
            "notes", "details", "openHandler", "openExeFolder", "useApiIndex", "useGamepadButtonPress",
            "keepInstancesFolder", "disableProfiles", "disableHandlerUpdate", "changeIcon", "remove",
        }, ids);
    }

    [Fact]
    public void Separators_are_never_leading_trailing_or_doubled()
    {
        GameMenu menu = GameMenuBuilder.Build(new GameMenuContext { GameId = "g", GameName = "G", CheckUpdate = true, ProfilesDisabledGlobally = true });

        Assert.NotEqual(GameMenuItemKind.Separator, menu.Items.First().Kind);
        Assert.NotEqual(GameMenuItemKind.Separator, menu.Items.Last().Kind);
        for (int i = 1; i < menu.Items.Count; i++)
        {
            Assert.False(menu.Items[i].Kind == GameMenuItemKind.Separator && menu.Items[i - 1].Kind == GameMenuItemKind.Separator);
        }

        Assert.Equal(menu.Items.Count, menu.Items.Select(i => i.Id).Distinct().Count());
    }

    [Theory]
    [InlineData(true, false, false, false, false, false, false, true)]   // XInput hook
    [InlineData(true, true, false, false, false, false, false, false)]   // XInput rerouted to DInput
    [InlineData(false, false, true, false, false, false, false, true)]   // ProtoInput XInput hook
    [InlineData(false, false, false, true, false, false, false, true)]   // SDL2
    [InlineData(true, false, false, false, true, false, false, false)]   // DInput hook
    [InlineData(true, false, false, false, false, true, false, false)]   // UseDevReorder
    [InlineData(true, false, false, false, false, false, true, false)]   // CreateSingleDeviceFile
    [InlineData(false, false, false, false, false, false, false, false)] // keyboard only
    public void Assign_by_index_needs_xinput_style_hooks(bool xinput, bool reroute, bool protoX, bool sdl2, bool dinput, bool devReorder, bool singleDeviceFile, bool expected)
    {
        var context = XInputGame();
        context.Inputs = new HandlerInputFlags { XInputEnabled = xinput, XInputReroute = reroute, ProtoXinputHook = protoX, Sdl2Enabled = sdl2, DInputEnabled = dinput };
        context.UseDevReorder = devReorder;
        context.CreateSingleDeviceFile = singleDeviceFile;

        Assert.Equal(expected, GameMenuBuilder.ShowUseApiIndex(context));
        Assert.Equal(expected, Item(GameMenuBuilder.Build(context), "useApiIndex") != null);
    }

    [Fact]
    public void Button_press_assignment_is_hidden_when_profiles_are_off()
    {
        var context = XInputGame();
        Assert.True(GameMenuBuilder.ShowUseGamepadButtonPress(context));

        context.DisableProfiles = true;
        Assert.False(GameMenuBuilder.ShowUseGamepadButtonPress(context));

        context.DisableProfiles = false;
        context.ProfilesDisabledGlobally = true;
        Assert.False(GameMenuBuilder.ShowUseGamepadButtonPress(context));
        Assert.Null(Item(GameMenuBuilder.Build(context), "disableProfiles"));
    }

    [Fact]
    public void Guests_by_index_needs_guests_and_index_mode_off()
    {
        var context = XInputGame();
        Assert.False(GameMenuBuilder.ShowUseApiIndexForGuests(context)); // no guests

        context.PlayersPerInstance = 2;
        Assert.True(GameMenuBuilder.ShowUseApiIndexForGuests(context));

        context.UseApiIndex = true;
        Assert.False(GameMenuBuilder.ShowUseApiIndexForGuests(context));

        context.UseApiIndex = false;
        context.DisableProfiles = true;
        Assert.False(GameMenuBuilder.ShowUseApiIndexForGuests(context));

        context.DisableProfiles = false;
        context.Inputs = new HandlerInputFlags { DInputEnabled = true };
        Assert.False(GameMenuBuilder.ShowUseApiIndexForGuests(context));
    }

    [Fact]
    public void Toggles_reflect_metainfo()
    {
        var context = XInputGame();
        context.UseApiIndex = true;
        context.KeepSymLink = true;
        context.CheckUpdate = false;
        GameMenu menu = GameMenuBuilder.Build(context);

        Assert.True(Item(menu, "useApiIndex")!.Checked);
        Assert.True(Item(menu, "keepInstancesFolder")!.Checked);
        Assert.True(Item(menu, "disableHandlerUpdate")!.Checked);
        Assert.False(Item(menu, "disableProfiles")!.Checked);
    }

    [Fact]
    public void Keep_instances_is_hidden_when_the_handler_keeps_them()
    {
        var context = XInputGame();
        context.KeepSymLinkOnExit = true;
        Assert.Null(Item(GameMenuBuilder.Build(context), "keepInstancesFolder"));
    }

    [Theory]
    [InlineData("12345", true, false, false, true)]
    [InlineData("", true, false, false, false)]
    [InlineData(null, false, true, false, true)]
    [InlineData(null, false, false, true, true)]
    [InlineData("12345", false, false, false, false)]
    public void Steam_language_for_emulated_steam(string? steamId, bool goldberg, bool goldbergNoOg, bool needsEmulation, bool expected)
    {
        var context = XInputGame();
        context.SteamId = steamId;
        context.UseGoldberg = goldberg;
        context.UseGoldbergNoOGSteamDlls = goldbergNoOg;
        context.NeedsSteamEmulation = needsEmulation;
        context.SteamLanguage = "French";

        GameMenuItem? item = Item(GameMenuBuilder.Build(context), "steamLanguage");
        Assert.Equal(expected, item != null);
        if (item != null)
        {
            Assert.Equal(GameMenuItemKind.Choice, item.Kind);
            Assert.Equal("French", item.Value);
            Assert.Equal("App Setting", item.Choices[0]);
            // One label per value: the stored "App Setting" is labelled in the language in use, Steam languages keep their names.
            Assert.Equal(item.Choices.Count, item.ChoiceLabels.Count);
            Assert.Equal("App Setting", item.ChoiceLabels[0]);
            Assert.Equal("French", item.ChoiceLabels[item.Choices.IndexOf("French")]);
        }
    }

    [Fact]
    public void Content_folder_items_need_the_folder_and_deletion_is_confirmed()
    {
        Assert.Null(Item(GameMenuBuilder.Build(XInputGame()), "deleteContentFolder"));

        var context = XInputGame();
        context.ContentFolderExists = true;
        context.ContentFolder = @"C:\Nucleus\content\game-1";
        GameMenuItem delete = Item(GameMenuBuilder.Build(context), "deleteContentFolder")!;

        Assert.NotNull(Item(GameMenuBuilder.Build(context), "openContentFolder"));
        Assert.True(delete.Danger);
        Assert.Contains(@"C:\Nucleus\content\game-1", delete.Confirm);
    }

    [Fact]
    public void Dynamic_items_carry_their_real_target()
    {
        var context = XInputGame();
        context.UserConfigFolders.Add(new MenuFolderTarget { Label = "Nucleus: Bob", Path = @"C:\Users\me\NucleusCoop\Bob\AppData\Game" });
        context.DocumentsSaveFolders.Add(new MenuFolderTarget { Label = "me", Path = @"C:\Users\me\Documents\My Games\Game" });
        context.BackupNicknames.Add("Player 2");
        GameMenu menu = GameMenuBuilder.Build(context);

        GameMenuItem open = Item(menu, "openUserConfig")!;
        Assert.Equal(GameMenuItemKind.Submenu, open.Kind);
        Assert.Equal(@"openUserConfig:C:\Users\me\NucleusCoop\Bob\AppData\Game", open.Children.Single().Id);
        Assert.Equal("Nucleus: Bob", open.Children.Single().Label);

        GameMenuItem deleteSave = Item(menu, @"deleteDocumentsSave:C:\Users\me\Documents\My Games\Game")!;
        Assert.True(deleteSave.Danger);
        Assert.NotNull(deleteSave.Confirm);

        Assert.NotNull(Item(menu, "openBackup:Player 2"));
        Assert.NotNull(Item(menu, "deleteBackup:Player 2")!.Confirm);
        Assert.Null(Item(menu, "openUserSave"));
        Assert.Null(Item(menu, "openDocumentsConfig"));
    }

    [Fact]
    public void Asset_submenu_lists_existing_assets_only()
    {
        var context = XInputGame();
        Assert.Null(Item(GameMenuBuilder.Build(context), "gameAssets"));

        context.ScreenshotsExist = true;
        GameMenuItem assets = Item(GameMenuBuilder.Build(context), "gameAssets")!;
        Assert.Equal(new[] { "openScreenshotsFolder" }, assets.Children.Select(c => c.Id).ToArray());
    }

    [Fact]
    public void Remove_is_last_dangerous_and_confirmed()
    {
        GameMenu menu = GameMenuBuilder.Build(XInputGame());
        GameMenuItem remove = menu.Items.Last();

        Assert.Equal("remove", remove.Id);
        Assert.True(remove.Danger);
        Assert.Contains("My Game", remove.Confirm);
    }

    [Fact]
    public void Client_side_items_say_what_the_ui_does()
    {
        GameMenu menu = GameMenuBuilder.Build(XInputGame());

        Assert.Equal(GameMenuClientAction.ShowNotes, Item(menu, "notes")!.ClientAction);
        Assert.Equal(GameMenuClientAction.ShowDetails, Item(menu, "details")!.ClientAction);
        GameMenuItem icon = Item(menu, "changeIcon")!;
        Assert.Equal(GameMenuClientAction.PickFile, icon.ClientAction);
        Assert.Contains(icon.FileFilters, f => f.Extensions.Contains("exe") && f.Extensions.Contains("png"));
        Assert.Null(Item(menu, "remove")!.ClientAction);
    }

    [Fact]
    public void Notes_are_hidden_without_a_description()
    {
        var context = XInputGame();
        context.HasNotes = false;
        Assert.Null(Item(GameMenuBuilder.Build(context), "notes"));
        Assert.NotNull(Item(GameMenuBuilder.Build(context), "details"));
    }

    [Theory]
    [InlineData("openHandler", "openHandler", null)]
    [InlineData("openBackup:Player 2", "openBackup", "Player 2")]
    [InlineData(@"openUserConfig:C:\a:b", "openUserConfig", @"C:\a:b")]
    public void Ids_split_into_action_and_target(string id, string action, string? target)
    {
        (string parsedAction, string? parsedTarget) = GameMenuBuilder.ParseId(id);
        Assert.Equal(action, parsedAction);
        Assert.Equal(target, parsedTarget);
    }
}
