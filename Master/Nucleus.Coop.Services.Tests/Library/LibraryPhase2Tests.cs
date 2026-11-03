using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Settings;
using Nucleus.Coop.Services.Tests.Engine;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Search;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Library;

/// <summary>
/// Nucleus 2.5 features on the bridge: the game folder search (<c>library.searchGames</c>), the game folders, the
/// add-game context report, hot reload messages and the handlers with errors.
/// </summary>
public sealed class LibraryPhase2Tests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "nucleus-phase2-" + Guid.NewGuid().ToString("N"));
    private readonly FakeEvents events = new FakeEvents();

    public LibraryPhase2Tests()
    {
        Directory.CreateDirectory(root);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(root, true);
        }
        catch (IOException)
        {
        }
    }

    private string Exe(string relative)
    {
        string path = Path.Combine(root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
        return path;
    }

    private static GenericGameInfo Handler(string name, string exe, string? extra = null) =>
        EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid(name), name, exe, extra), name + ".js");

    private GameAutoSearch SearchIn(params string[] folders)
    {
        var directories = new GameSearchDirectories(Path.Combine(root, "userGameDirectories.json"));
        foreach (string folder in folders)
        {
            directories.Add(folder);
        }

        return new GameAutoSearch(directories);
    }

    [Fact]
    public async Task Search_maps_candidates_to_the_contract_and_ends_its_progress_at_100()
    {
        string alphaExe = Exe(@"Games\Alpha\alpha.exe");
        string betaExe = Exe(@"Games\Beta Game\bin\beta.exe");
        GenericGameInfo alpha = Handler("Alpha", "alpha.exe");
        GenericGameInfo beta = Handler("Beta", "beta.exe");
        GameAutoSearch search = SearchIn(Path.Combine(root, "Games"), Path.Combine(root, "Unplugged"));

        FolderSearchResult result = await FolderSearch.RunAsync(search, events, "s1", new[] { beta, alpha }, new[] { (beta.GUID, betaExe) }, null, CancellationToken.None);

        Assert.Equal(1, result.FoldersSearched); // the unplugged folder stays saved but is not searched
        Assert.Equal(new[] { "Alpha", "Beta" }, result.Candidates.Select(c => c.GameName));
        FolderSearchCandidate first = result.Candidates[0];
        Assert.Equal(alpha.GUID, first.GameId);
        Assert.Equal("Alpha.js", first.HandlerFile);
        Assert.Equal(alphaExe, first.ExecutablePath, ignoreCase: true);
        Assert.Equal(Path.GetDirectoryName(alphaExe), first.InstallFolder, ignoreCase: true);
        Assert.False(first.InLibrary);
        Assert.True(result.Candidates[1].InLibrary);

        List<FolderSearchProgress> progress = events.Of<FolderSearchProgress>();
        Assert.NotEmpty(progress);
        Assert.All(progress, p => Assert.Equal("s1", p.SearchId));
        Assert.Equal(100, progress.Last().Percent);
        Assert.Equal(2, progress.Last().Found);
        Assert.Null(progress.Last().CurrentFolder);
        Assert.Single(progress, p => p.Percent == 100);
    }

    [Fact]
    public async Task Search_for_one_handler_stops_at_its_first_executable_and_marks_the_library()
    {
        Exe(@"Lib\One\zeta.exe");
        Exe(@"Lib\Two\zeta.exe");
        GenericGameInfo zeta = Handler("Zeta", "zeta.exe");
        GameAutoSearch search = SearchIn(Path.Combine(root, "Lib"));

        FolderSearchResult alone = await FolderSearch.RunAsync(search, events, "one", new[] { zeta }, Array.Empty<(string, string)>(), zeta, CancellationToken.None);
        FolderSearchResult known = await FolderSearch.RunAsync(search, events, "two", new[] { zeta }, new[] { (zeta.GUID, @"X:\elsewhere\zeta.exe") }, zeta, CancellationToken.None);

        Assert.False(Assert.Single(alone.Candidates).InLibrary);
        Assert.True(Assert.Single(known.Candidates).InLibrary);
    }

    [Fact]
    public async Task Search_without_folders_or_matches_is_empty()
    {
        GenericGameInfo zeta = Handler("Zeta", "zeta.exe");

        FolderSearchResult result = await FolderSearch.RunAsync(SearchIn(), events, "none", new[] { zeta }, Array.Empty<(string, string)>(), null, CancellationToken.None);

        Assert.Empty(result.Candidates);
        Assert.Equal(0, result.FoldersSearched);
        Assert.Equal(100, events.Of<FolderSearchProgress>().Last().Percent);
    }

    [Fact]
    public async Task Cancelled_search_throws_and_never_reports_its_end()
    {
        Exe(@"Big\One\eta.exe");
        GenericGameInfo eta = Handler("Eta", "eta.exe");
        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            FolderSearch.RunAsync(SearchIn(Path.Combine(root, "Big")), events, "c", new[] { eta }, Array.Empty<(string, string)>(), null, cancelled.Token));

        Assert.DoesNotContain(events.Of<FolderSearchProgress>(), p => p.Percent == 100);
    }

    [Fact]
    public void Progress_is_throttled_unless_the_percentage_moves_on()
    {
        DateTime now = new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc);
        var progress = new FolderSearch.ThrottledProgress(events, "t", () => now);

        progress.Report(new GameSearchProgress { Percent = 10, CurrentFolder = "A" });
        progress.Report(new GameSearchProgress { Percent = 10, CurrentFolder = "B" }); // same percentage, too soon
        now = now.AddMilliseconds(FolderSearch.ThrottledProgress.IntervalMs);
        progress.Report(new GameSearchProgress { Percent = 10, CurrentFolder = "C" }); // same percentage, later
        progress.Report(new GameSearchProgress { Percent = 11, CurrentFolder = "D" }); // moved on
        progress.Report(new GameSearchProgress { Percent = 100, CurrentFolder = null }); // the engine's own end
        progress.Finish(3);
        progress.Report(new GameSearchProgress { Percent = 50, CurrentFolder = "late" });
        progress.Finish(3);

        List<FolderSearchProgress> sent = events.Of<FolderSearchProgress>();
        Assert.Equal(new[] { "A", "C", "D", null, null }, sent.Select(p => p.CurrentFolder));
        Assert.Equal(new[] { 10, 10, 11, 99, 100 }, sent.Select(p => p.Percent));
        Assert.Equal(3, sent.Last().Found);
    }

    [Fact]
    public void Context_mismatch_data_is_typed_and_camel_cased_on_the_wire()
    {
        string exe = Exe(@"Ed\game.exe");
        Directory.CreateDirectory(Path.Combine(root, "Ed", "packs"));
        GenericGameInfo noPacks = Handler("Base game", "game.exe", "Game.ExecutableContext = [\"!packs\"];");
        GenericGameInfo needsData = Handler("Expansion", "game.exe", "Game.ExecutableContext = [\"expansion.dat\"];");

        BridgeException error = LibraryService.NoHandler(exe, ExecutableMatch.Evaluate(new[] { noPacks, needsData }, exe));

        ExecutableContextMismatch data = Assert.IsType<ExecutableContextMismatch>(error.ErrorData);
        Assert.Equal("contextMissing", data.Reason);
        Assert.Equal("game.exe", data.Executable);
        Assert.Equal(2, data.Handlers.Count);
        RejectedHandler expansion = data.Handlers.Single(h => h.GameId == needsData.GUID);
        Assert.Equal("Expansion", expansion.Name);
        Assert.Equal("Expansion.js", expansion.File);
        Assert.Contains("expansion.dat", expansion.Missing);
        Assert.Empty(expansion.Excluded);
        Assert.Equal(new[] { "packs" }, data.Handlers.Single(h => h.GameId == noPacks.GUID).Excluded);
        Assert.Equal(data.Handlers[0].GameId, data.GameId);

        JObject wire = JObject.FromObject(data, JsonSerializer.Create(BridgeJson.Settings));
        Assert.Equal(new[] { "reason", "executable", "gameId", "missing", "excluded", "handlers" }, wire.Properties().Select(p => p.Name));
        Assert.Equal(new[] { "gameId", "name", "file", "missing", "excluded" }, ((JObject)wire["handlers"]![0]!).Properties().Select(p => p.Name));
    }

    [Fact]
    public void A_chosen_handler_that_refuses_the_folder_reports_only_itself()
    {
        string exe = Exe(@"Chosen\game.exe");
        GenericGameInfo needsData = Handler("Needs data", "game.exe", "Game.ExecutableContext = [\"data.pak\"];");
        GenericGameInfo other = Handler("Other", "game.exe", "Game.ExecutableContext = [\"other.pak\"];");
        GenericGameInfo elsewhere = Handler("Elsewhere", "launcher.exe");
        ExecutableMatch match = ExecutableMatch.Evaluate(new[] { needsData, other, elsewhere }, exe);

        BridgeException refused = LibraryService.RefusedBy(needsData.GUID, exe, match);
        ExecutableContextMismatch data = Assert.IsType<ExecutableContextMismatch>(refused.ErrorData);
        Assert.Equal(new[] { needsData.GUID }, data.Handlers.Select(h => h.GameId));
        Assert.Equal(new[] { "data.pak" }, data.Missing);
        Assert.Equal(2, match.Rejected.Count); // the other refusing handler is left out
    }

    [Fact]
    public void Game_folders_list_every_saved_folder_with_its_state()
    {
        string games = Directory.CreateDirectory(Path.Combine(root, "Games")).FullName;
        string unplugged = Path.Combine(root, "Unplugged");

        GameFolders folders = SettingsService.GameFoldersOf(null, new[] { unplugged, games });

        Assert.Null(folders.AutoSearch);
        Assert.Equal(new[] { unplugged, games }, folders.Folders.Select(f => f.Path));
        Assert.Equal(new[] { false, true }, folders.Folders.Select(f => f.Exists));
        Assert.True(SettingsService.GameFoldersOf(true, Array.Empty<string>()).AutoSearch);
    }

    [Fact]
    public void Folder_changes_are_described_for_a_short_message()
    {
        string folder = Directory.CreateDirectory(Path.Combine(root, "handlers")).FullName;
        var games = new Dictionary<string, GenericGameInfo>();
        var index = new HandlerFileIndex(folder, games, new Dictionary<string, GenericGameInfo>());
        var library = new List<UserGameInfo>();
        HandlerChanges Refresh() => index.Apply(index.Scan(), library);
        string alpha = EngineTestHandlers.NewGuid("alpha");
        string beta = EngineTestHandlers.NewGuid("beta");
        string alphaJs = Path.Combine(folder, "Alpha.js");
        string betaJs = Path.Combine(folder, "Beta.js");

        EngineTestHandlers.WriteScript(alphaJs, EngineTestHandlers.Script(alpha, "Alpha", "alpha.exe"));
        ReloadedHandler added = Assert.Single(HandlerHotReload.Describe(Refresh(), _ => true));
        Assert.Equal((alpha, "Alpha", HandlerReloadKind.Added), (added.GameId, added.GameName, added.Kind));

        EngineTestHandlers.WriteScript(alphaJs, EngineTestHandlers.Script(alpha, "Alpha 2", "alpha.exe"), secondsLater: 5);
        EngineTestHandlers.WriteScript(betaJs, EngineTestHandlers.Script(beta, "Beta", "beta.exe"), secondsLater: 5);
        List<ReloadedHandler> both = HandlerHotReload.Describe(Refresh(), _ => true);
        Assert.Equal(new[] { HandlerReloadKind.Updated, HandlerReloadKind.Added }, both.Select(r => r.Kind));
        Assert.Equal("Alpha 2", both[0].GameName);

        EngineTestHandlers.WriteScript(alphaJs, "Game.GameName = \"Oops\"; this is not javascript (", secondsLater: 10);
        HandlerChanges broken = Refresh();
        Assert.Empty(HandlerHotReload.Describe(broken, _ => true)); // a notice says it
        HandlerFileError error = Assert.Single(HandlersService.FileErrors(index.Snapshot()));
        Assert.Equal("Alpha.js", error.File);
        Assert.True(error.PreviousVersionInUse);
        Assert.Equal(alpha, error.GameId);
        Assert.False(string.IsNullOrEmpty(error.Error));
        // Only what is wrong: the full message says the game leaves the list, which is not true while the previous version runs.
        Assert.DoesNotContain("will not appear in the list", error.Error);

        EngineTestHandlers.WriteScript(alphaJs, EngineTestHandlers.Script(alpha, "Alpha 3", "alpha.exe"), secondsLater: 15);
        Assert.Equal(HandlerReloadKind.Fixed, Assert.Single(HandlerHotReload.Describe(Refresh(), _ => true)).Kind);
        Assert.Empty(HandlersService.FileErrors(index.Snapshot()));

        File.Delete(betaJs);
        HandlerChanges removed = Refresh();
        Assert.Equal(HandlerReloadKind.Removed, Assert.Single(HandlerHotReload.Describe(removed, _ => false)).Kind);
        Assert.Empty(HandlerHotReload.Describe(removed, _ => true)); // another script still declares the game
    }

    [Fact]
    public void A_script_that_never_loaded_is_listed_without_a_previous_version()
    {
        string folder = Directory.CreateDirectory(Path.Combine(root, "handlers2")).FullName;
        var index = new HandlerFileIndex(folder, new Dictionary<string, GenericGameInfo>(), new Dictionary<string, GenericGameInfo>());
        EngineTestHandlers.WriteScript(Path.Combine(folder, "Never.js"), "Game.GUID = ;");

        index.Apply(index.Scan(), new List<UserGameInfo>());

        HandlerFileError error = Assert.Single(HandlersService.FileErrors(index.Snapshot()));
        Assert.Equal("Never.js", error.File);
        Assert.False(error.PreviousVersionInUse);
        Assert.Null(error.GameId);
        Assert.Contains("will not appear in the list", error.Error);
    }

    private sealed class FakeEvents : IBridgeEvents
    {
        private readonly List<object> emitted = new List<object>();

        public void Emit<TEvent>(TEvent payload) where TEvent : class
        {
            lock (emitted)
            {
                emitted.Add(payload);
            }
        }

        public List<T> Of<T>()
        {
            lock (emitted)
            {
                return emitted.OfType<T>().ToList();
            }
        }
    }
}
