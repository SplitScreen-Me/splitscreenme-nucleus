using System;
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
