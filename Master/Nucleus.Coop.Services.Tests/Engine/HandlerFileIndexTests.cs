using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>Handler hot reload bookkeeping (Nucleus 2.5): added, updated, broken, fixed and deleted scripts.</summary>
public sealed class HandlerFileIndexTests : IDisposable
{
    private readonly string folder = Path.Combine(Path.GetTempPath(), "nucleus-handlers-" + Guid.NewGuid().ToString("N"));
    private readonly Dictionary<string, GenericGameInfo> games = new Dictionary<string, GenericGameInfo>();
    private readonly Dictionary<string, GenericGameInfo> gameInfos = new Dictionary<string, GenericGameInfo>();
    private readonly List<UserGameInfo> library = new List<UserGameInfo>();
    private readonly HandlerFileIndex index;

    public HandlerFileIndexTests()
    {
        Directory.CreateDirectory(folder);
        EngineTestHandlers.UseTemporaryMetaInfo();
        index = new HandlerFileIndex(folder, games, gameInfos);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(folder, true);
        }
        catch (IOException)
        {
        }
    }

    private string PathOf(string name) => Path.Combine(folder, name + ".js");

    private HandlerChanges Refresh(bool all = false) => index.Apply(index.Scan(all), library);

    private UserGameInfo AddToLibrary(string guid, string exePath)
    {
        var entry = new UserGameInfo();
        entry.InitializeDefault(games[guid], exePath);
        library.Add(entry);
        return entry;
    }

    [Fact]
    public void New_script_is_added_and_an_unchanged_folder_reports_nothing()
    {
        string guid = EngineTestHandlers.NewGuid("added");
        EngineTestHandlers.WriteScript(PathOf("Added"), EngineTestHandlers.Script(guid, "Added game", "added.exe"));

        HandlerChanges changes = Refresh();

        Assert.Single(changes.Added);
        Assert.Equal(new[] { guid }, changes.GameIds);
        Assert.Equal("Added game", games[guid].GameName);
        Assert.Same(games[guid], gameInfos[guid]);
        Assert.Equal(PathOf("Added"), games[guid].FileFullPath, ignoreCase: true);

        Assert.True(index.Scan().IsEmpty);
        Assert.False(Refresh().Any);
    }

    [Fact]
    public void Edited_script_replaces_the_handler_and_rebinds_the_library_entry()
    {
        string guid = EngineTestHandlers.NewGuid("edited");
        EngineTestHandlers.WriteScript(PathOf("Edited"), EngineTestHandlers.Script(guid, "Version 1", "edited.exe"));
        Refresh();
        UserGameInfo entry = AddToLibrary(guid, @"C:\Games\Edited\edited.exe");
        GenericGameInfo before = games[guid];

        EngineTestHandlers.WriteScript(PathOf("Edited"), EngineTestHandlers.Script(guid, "Version 2", "edited.exe"), secondsLater: 5);
        HandlerChanges changes = Refresh();

        Assert.Single(changes.Updated);
        Assert.NotSame(before, games[guid]);
        Assert.Equal("Version 2", games[guid].GameName);
        Assert.Same(games[guid], entry.Game);
        Assert.Contains(entry, changes.Rebound);
        Assert.Empty(changes.Orphaned);
    }

    [Fact]
    public void Executable_name_is_set_again_for_executable_names_handlers()
    {
        string guid = EngineTestHandlers.NewGuid("names");
        string script = EngineTestHandlers.Script(guid, "Names", "game.exe", "Game.ExecutableNames = [\"game.exe\", \"game_dx12.exe\"];");
        EngineTestHandlers.WriteScript(PathOf("Names"), script);
        Refresh();
        AddToLibrary(guid, @"C:\Games\Names\game_dx12.exe");

        EngineTestHandlers.WriteScript(PathOf("Names"), script + "\n// edited", secondsLater: 5);
        Refresh();

        Assert.Equal("game_dx12.exe", games[guid].ExecutableName);
    }

    [Fact]
    public void Broken_edit_keeps_the_previous_version_reports_once_then_fixed()
    {
        string guid = EngineTestHandlers.NewGuid("broken");
        EngineTestHandlers.WriteScript(PathOf("Broken"), EngineTestHandlers.Script(guid, "Working", "broken.exe"));
        Refresh();
        GenericGameInfo working = games[guid];

        EngineTestHandlers.WriteScript(PathOf("Broken"), "Game.GameName = \"Oops\"; this is not javascript (", secondsLater: 5);
        HandlerChanges broken = Refresh();

        HandlerInfo invalid = Assert.Single(broken.Invalid);
        Assert.False(invalid.IsValid);
        Assert.NotNull(invalid.Error);
        Assert.Same(working, invalid.GenericGameInfo);
        Assert.Same(working, games[guid]);
        Assert.Empty(broken.GameIds);

        //Saved again with the same error: not reported twice.
        EngineTestHandlers.WriteScript(PathOf("Broken"), "Game.GameName = \"Oops\"; this is not javascript (", secondsLater: 10);
        Assert.Empty(Refresh().Invalid);

        EngineTestHandlers.WriteScript(PathOf("Broken"), EngineTestHandlers.Script(guid, "Fixed", "broken.exe"), secondsLater: 15);
        HandlerChanges fixedChanges = Refresh();

        Assert.Single(fixedChanges.Fixed);
        Assert.Equal("Fixed", games[guid].GameName);
    }

    [Fact]
    public void Script_broken_at_startup_keeps_its_game_and_brings_it_back_once_fixed()
    {
        string path = PathOf("Startup");
        string guid = EngineTestHandlers.NewGuid("startup");
        EngineTestHandlers.WriteScript(path, "this is ( broken");
        Refresh();
        Assert.True(index.IsBroken(path));

        //The library entry of that game, saved by a previous run.
        var entry = new UserGameInfo { GameGuid = guid, ExePath = @"C:\Games\Startup\startup.exe", FileFullPath = path };
        library.Add(entry);

        EngineTestHandlers.WriteScript(path, EngineTestHandlers.Script(guid, "Startup game", "startup.exe"), secondsLater: 5);
        HandlerChanges changes = Refresh();

        Assert.Single(changes.Fixed);
        Assert.Same(games[guid], entry.Game);
        Assert.False(index.IsBroken(path));
    }

    [Fact]
    public void New_broken_script_is_reported()
    {
        EngineTestHandlers.WriteScript(PathOf("NewBroken"), "Game.GUID = ;");
        HandlerChanges changes = Refresh();

        Assert.Single(changes.Invalid);
        Assert.Empty(changes.Added);
        Assert.Empty(games);
    }

    [Fact]
    public void Script_without_guid_is_invalid()
    {
        EngineTestHandlers.WriteScript(PathOf("NoGuid"), "Game.GameName = \"No guid\";");
        HandlerChanges changes = Refresh();

        Assert.Single(changes.Invalid);
        Assert.Empty(games);
    }

    [Fact]
    public void Deleted_script_removes_the_handler_and_orphans_its_game()
    {
        string guid = EngineTestHandlers.NewGuid("deleted");
        EngineTestHandlers.WriteScript(PathOf("Deleted"), EngineTestHandlers.Script(guid, "Deleted", "deleted.exe"));
        Refresh();
        UserGameInfo entry = AddToLibrary(guid, @"C:\Games\Deleted\deleted.exe");

        File.Delete(PathOf("Deleted"));
        HandlerChanges changes = Refresh();

        Assert.Single(changes.Removed);
        Assert.False(games.ContainsKey(guid));
        Assert.False(gameInfos.ContainsKey(guid));
        Assert.Equal(new[] { entry }, changes.Orphaned);
    }

    [Fact]
    public void Another_script_with_the_same_guid_takes_over_when_one_is_deleted()
    {
        string guid = EngineTestHandlers.NewGuid("dup");
        EngineTestHandlers.WriteScript(PathOf("Dup A"), EngineTestHandlers.Script(guid, "From A", "dup.exe"));
        EngineTestHandlers.WriteScript(PathOf("Dup B"), EngineTestHandlers.Script(guid, "From B", "dup.exe"), secondsLater: 5);
        Refresh();
        UserGameInfo entry = AddToLibrary(guid, @"C:\Games\Dup\dup.exe");
        string kept = games[guid].GameName == "From A" ? "Dup B" : "Dup A";
        string deleted = kept == "Dup A" ? "Dup B" : "Dup A";

        File.Delete(PathOf(deleted));
        HandlerChanges changes = Refresh();

        Assert.True(games.ContainsKey(guid));
        Assert.Equal(kept == "Dup A" ? "From A" : "From B", games[guid].GameName);
        Assert.Empty(changes.Orphaned);
        Assert.Same(games[guid], entry.Game);
    }

    [Fact]
    public void Script_that_changes_its_guid_frees_the_old_game()
    {
        string oldGuid = EngineTestHandlers.NewGuid("old");
        string newGuid = EngineTestHandlers.NewGuid("new");
        EngineTestHandlers.WriteScript(PathOf("Renamed"), EngineTestHandlers.Script(oldGuid, "Old", "renamed.exe"));
        Refresh();
        UserGameInfo entry = AddToLibrary(oldGuid, @"C:\Games\Renamed\renamed.exe");

        EngineTestHandlers.WriteScript(PathOf("Renamed"), EngineTestHandlers.Script(newGuid, "New", "renamed.exe"), secondsLater: 5);
        HandlerChanges changes = Refresh();

        Assert.False(games.ContainsKey(oldGuid));
        Assert.True(games.ContainsKey(newGuid));
        Assert.Contains(oldGuid, changes.GameIds);
        Assert.Contains(entry, changes.Orphaned);
    }

    [Fact]
    public void Full_rescan_parses_every_script_again()
    {
        string guid = EngineTestHandlers.NewGuid("full");
        EngineTestHandlers.WriteScript(PathOf("Full"), EngineTestHandlers.Script(guid, "Full", "full.exe"));
        Refresh();
        GenericGameInfo before = games[guid];

        HandlerChanges changes = Refresh(all: true);

        Assert.Single(changes.Updated);
        Assert.NotSame(before, games[guid]);
    }

    [Fact]
    public void Master_handler_script_is_ignored()
    {
        File.WriteAllText(PathOf("MasterHandler"), "this is not a handler (");
        Assert.True(index.Scan().IsEmpty);
        Assert.True(HandlerFileIndex.IsIgnoredScript("masterhandler.js"));
    }
}
