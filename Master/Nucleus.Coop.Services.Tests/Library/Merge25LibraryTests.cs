using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Library;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Library;

/// <summary>Library parts of the Nucleus 2.5 merge: recycle bin, documents paths, hub status checks.</summary>
public sealed class Merge25LibraryTests
{
    [Theory]
    [InlineData(GameMenuBuilder.Ids.DeleteContentFolder)]
    [InlineData(GameMenuBuilder.Ids.DeleteUserConfig)]
    [InlineData(GameMenuBuilder.Ids.DeleteUserSave)]
    [InlineData(GameMenuBuilder.Ids.DeleteDocumentsConfig)]
    [InlineData(GameMenuBuilder.Ids.DeleteDocumentsSave)]
    [InlineData(GameMenuBuilder.Ids.DeleteBackup)]
    public void User_deletions_go_to_the_recycle_bin(string itemId)
    {
        Assert.Equal(DeleteMode.RecycleBin, UserDataDeletion.ForMenuItem(itemId));
    }

    [Theory]
    [InlineData(GameMenuBuilder.Ids.Remove)]
    [InlineData(GameMenuBuilder.Ids.OpenContentFolder)]
    [InlineData("unknown")]
    public void Other_items_are_not_recycle_bin_deletions(string itemId)
    {
        Assert.Equal(DeleteMode.Permanent, UserDataDeletion.ForMenuItem(itemId));
    }

    [Fact]
    public void Without_a_recycle_bin_a_confirmed_deletion_is_permanent()
    {
        Assert.Equal(DeleteMode.RecycleBin, UserDataDeletion.Effective(DeleteMode.RecycleBin, driveHasRecycleBin: true));
        Assert.Equal(DeleteMode.Permanent, UserDataDeletion.Effective(DeleteMode.RecycleBin, driveHasRecycleBin: false));
        Assert.Equal(DeleteMode.Permanent, UserDataDeletion.Effective(DeleteMode.Permanent, driveHasRecycleBin: true));
    }

    private static GameMenuContext DeletableGame(bool recycleBin)
    {
        var context = new GameMenuContext
        {
            GameId = "game-1",
            GameName = "My Game",
            ContentFolderExists = true,
            ContentFolder = @"C:\Nucleus\content\game-1",
            ContentFolderRecycleBin = recycleBin,
            BackupNicknames = { "Player 2" },
            BackupsRecycleBin = recycleBin,
        };
        context.UserSaveFolders.Add(new MenuFolderTarget { Label = "Nucleus: Bob", Path = @"C:\Users\me\NucleusCoop\Bob\Saves", RecycleBin = recycleBin });
        return context;
    }

    private static GameMenuItem? Find(IEnumerable<GameMenuItem> items, string id) =>
        items.Select(i => i.Id == id ? i : Find(i.Children, id)).FirstOrDefault(i => i != null);

    [Fact]
    public void Deletion_confirmations_say_the_items_go_to_the_recycle_bin()
    {
        GameMenu menu = GameMenuBuilder.Build(DeletableGame(recycleBin: true));

        string content = Find(menu.Items, GameMenuBuilder.Ids.DeleteContentFolder)!.Confirm!;
        Assert.Contains(@"C:\Nucleus\content\game-1", content);
        Assert.Contains("My Game", content);
        Assert.Contains("Recycle Bin", content);
        Assert.DoesNotContain("for good", content);

        string save = Find(menu.Items, GameMenuBuilder.Ids.DeleteUserSave + @":C:\Users\me\NucleusCoop\Bob\Saves")!.Confirm!;
        Assert.Contains(@"C:\Users\me\NucleusCoop\Bob\Saves", save);
        Assert.Contains("goes to the Recycle Bin", save);

        string backup = Find(menu.Items, GameMenuBuilder.Ids.DeleteBackup + ":Player 2")!.Confirm!;
        Assert.Contains("Player 2", backup);
        Assert.Contains("goes to the Recycle Bin", backup);
    }

    [Fact]
    public void Without_a_recycle_bin_the_confirmations_say_the_deletion_is_permanent()
    {
        GameMenu menu = GameMenuBuilder.Build(DeletableGame(recycleBin: false));

        foreach (string id in new[] { GameMenuBuilder.Ids.DeleteContentFolder, GameMenuBuilder.Ids.DeleteUserSave + @":C:\Users\me\NucleusCoop\Bob\Saves", GameMenuBuilder.Ids.DeleteBackup + ":Player 2" })
        {
            string confirm = Find(menu.Items, id)!.Confirm!;
            Assert.Contains("for good", confirm);
            Assert.Contains("no Recycle Bin", confirm);
        }

        Assert.Contains("My Game", Find(menu.Items, GameMenuBuilder.Ids.DeleteContentFolder)!.Confirm);
    }

    [Fact]
    public void The_recycle_bin_is_asked_once_per_drive()
    {
        var asked = new List<string>();
        var bins = new GameMenuActions.RecycleBins(root =>
        {
            asked.Add(root);
            return !root.StartsWith(@"\\", StringComparison.Ordinal);
        });

        Assert.True(bins.Has(@"C:\Nucleus\content\game-1"));
        Assert.True(bins.Has(@"c:\Users\me\Saves"));
        Assert.True(bins.Has(@"D:\Backups"));
        Assert.False(bins.Has(@"\\nas\share\Saves"));
        Assert.False(bins.Has(@"\\nas\share\Other"));

        Assert.Equal(new[] { @"C:\", @"D:\", @"\\nas\share" }, asked.ToArray());
    }

    [Fact]
    public void Documents_prefixed_user_paths_resolve_without_touching_the_handler()
    {
        GameMenuActions.SavePaths paths = GameMenuActions.ResolveSavePaths(@"Documents\My Games\Cfg", @"AppData\Saves", null, @"Docs\Save");

        Assert.Null(paths.UserConfig);
        Assert.Equal(@"My Games\Cfg", paths.DocumentsConfig);
        Assert.Equal(@"AppData\Saves", paths.UserSave);
        Assert.Equal(@"Docs\Save", paths.DocumentsSave);

        GameMenuActions.SavePaths both = GameMenuActions.ResolveSavePaths(null, @"documents\Saves", @"Cfg", null);
        Assert.Null(both.UserSave);
        Assert.Equal("Saves", both.DocumentsSave);
        Assert.Equal("Cfg", both.DocumentsConfig);
    }

    [Fact]
    public void Hub_is_checked_every_30_s_offline_and_every_5_min_online()
    {
        DateTime now = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);

        Assert.False(HubStatus.ProbeDue(online: false, now.AddSeconds(-10), now));
        Assert.True(HubStatus.ProbeDue(online: false, now.AddSeconds(-31), now));
        Assert.False(HubStatus.ProbeDue(online: true, now.AddMinutes(-4), now));
        Assert.True(HubStatus.ProbeDue(online: true, now.AddMinutes(-6), now));
        Assert.True(HubStatus.ProbeDue(online: true, DateTime.MinValue, now));
    }
}
