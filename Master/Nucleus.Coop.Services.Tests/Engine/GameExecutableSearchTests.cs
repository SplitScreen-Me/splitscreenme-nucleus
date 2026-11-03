using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Nucleus.Coop.Services.Library;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.Search;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>Automatic game executable search (Nucleus 2.5), without its WinForms windows.</summary>
public sealed class GameExecutableSearchTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "nucleus-search-" + Guid.NewGuid().ToString("N"));

    public GameExecutableSearchTests()
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
        EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid(name), name, exe, extra));

    [Fact]
    public void Finds_executables_by_name_in_game_folders_case_insensitively()
    {
        string found = Exe(@"Games\Alpha\bin\Alpha.EXE");
        Exe(@"Games\Beta\beta.exe");
        GenericGameInfo alpha = Handler("Alpha", "alpha.exe");

        List<GameSearchResult> results = GameExecutableSearch.Search(new[] { alpha }, new[] { Path.Combine(root, "Games") });

        GameSearchResult result = Assert.Single(results);
        Assert.Same(alpha, result.Game);
        Assert.Equal(found, result.ExecutablePath, ignoreCase: true);
        Assert.Equal(Path.GetDirectoryName(found), result.InstallDirectory, ignoreCase: true);
    }

    [Fact]
    public void Executable_names_binaries_folder_and_context_are_checked()
    {
        Exe(@"Games\Gamma\Gamma.exe");
        string right = Exe(@"Games\Gamma2\x64\Gamma.exe");
        Exe(@"Games\Delta\delta.exe");
        Directory.CreateDirectory(Path.Combine(root, @"Games\Delta\packs"));
        string deltaOk = Exe(@"Games\Delta Clean\delta.exe");
        string named = Exe(@"Games\Eps\eps_vk.exe");

        GenericGameInfo gamma = Handler("Gamma", "Gamma.exe", "Game.BinariesFolder = \"x64\";");
        GenericGameInfo delta = Handler("Delta", "delta.exe", "Game.ExecutableContext = [\"!packs\"];");
        GenericGameInfo eps = Handler("Eps", "eps.exe", "Game.ExecutableNames = [\"eps.exe\", \"eps_vk.exe\"];");

        List<GameSearchResult> results = GameExecutableSearch.Search(new[] { gamma, delta, eps }, new[] { Path.Combine(root, "Games") });

        Assert.Equal(new[] { right }, results.Where(r => r.Game == gamma).Select(r => r.ExecutablePath));
        Assert.Equal(new[] { deltaOk }, results.Where(r => r.Game == delta).Select(r => r.ExecutablePath));
        Assert.Equal(new[] { named }, results.Where(r => r.Game == eps).Select(r => r.ExecutablePath));
    }

    [Fact]
    public void Stop_at_first_progress_and_cancellation()
    {
        Exe(@"Lib\One\zeta.exe");
        Exe(@"Lib\Two\zeta.exe");
        GenericGameInfo zeta = Handler("Zeta", "zeta.exe");
        var reports = new List<GameSearchProgress>();
        var progress = new SyncProgress(reports.Add);

        Assert.Single(GameExecutableSearch.Search(new[] { zeta }, new[] { Path.Combine(root, "Lib") }, progress, stopAtFirst: true));
        Assert.Equal(100, reports.Last().Percent);
        Assert.Equal(2, GameExecutableSearch.Search(new[] { zeta }, new[] { Path.Combine(root, "Lib"), Path.Combine(root, "missing") }).Count);

        using var cancelled = new CancellationTokenSource();
        cancelled.Cancel();
        Assert.Throws<OperationCanceledException>(() => GameExecutableSearch.Search(new[] { zeta }, new[] { Path.Combine(root, "Lib") }, null, cancelled.Token));
    }

    [Fact]
    public void Candidates_mark_what_is_already_in_the_library()
    {
        string first = Exe(@"C\Eta\eta.exe");
        string second = Exe(@"C\Eta2\eta.exe");
        GenericGameInfo eta = Handler("Eta", "eta.exe");
        List<GameSearchResult> results = GameExecutableSearch.Search(new[] { eta }, new[] { Path.Combine(root, "C") });

        List<AutoSearchCandidate> none = GameAutoSearch.ToCandidates(results, Array.Empty<(string, string)>());
        List<AutoSearchCandidate> byPath = GameAutoSearch.ToCandidates(results.Concat(results), new[] { ("other", first) });

        Assert.Equal(2, none.Count);
        Assert.All(none, c => Assert.False(c.InLibrary));
        Assert.Equal(2, byPath.Count);//duplicates merged
        Assert.True(byPath.Single(c => string.Equals(c.ExecutablePath, first, StringComparison.OrdinalIgnoreCase)).InLibrary);
        Assert.False(byPath.Single(c => string.Equals(c.ExecutablePath, second, StringComparison.OrdinalIgnoreCase)).InLibrary);
        Assert.All(GameAutoSearch.ToCandidates(results, new[] { (eta.GUID, @"D:\elsewhere\eta.exe") }), c => Assert.True(c.InLibrary));
    }

    [Fact]
    public void Search_folders_are_stored_in_the_2_5_format()
    {
        string file = Path.Combine(root, "userGameDirectories.json");
        var folders = new GameSearchDirectories(file);
        string games = Directory.CreateDirectory(Path.Combine(root, "SteamLibrary")).FullName;

        Assert.True(folders.Add(games + "\\"));
        Assert.False(folders.Add(games.ToUpperInvariant()));
        Assert.True(folders.Add(Path.Combine(root, "Unplugged")));

        var reread = new GameSearchDirectories(file);
        Assert.Equal(2, reread.All.Count);
        Assert.Equal(new[] { games }, reread.Existing);
        Assert.Contains("\"Path\"", File.ReadAllText(file));

        Assert.True(reread.Remove(Path.Combine(root, "Unplugged")));
        Assert.False(reread.Remove(Path.Combine(root, "Unplugged")));
        Assert.Single(new GameSearchDirectories(file).All);
    }

    [Fact]
    public void Missing_or_broken_folders_file_is_empty()
    {
        Assert.Empty(new GameSearchDirectories(Path.Combine(root, "none.json")).All);

        string broken = Path.Combine(root, "broken.json");
        File.WriteAllText(broken, "{ not json");
        Assert.Empty(new GameSearchDirectories(broken).All);
    }

    private sealed class SyncProgress : IProgress<GameSearchProgress>
    {
        private readonly Action<GameSearchProgress> report;

        public SyncProgress(Action<GameSearchProgress> report) => this.report = report;

        public void Report(GameSearchProgress value) => report(value);
    }
}
