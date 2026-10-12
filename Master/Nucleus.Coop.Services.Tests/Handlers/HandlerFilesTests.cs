using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Threading;
using Nucleus.Coop.Services.Handlers;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Handlers;

public sealed class HandlerFilesTests : IDisposable
{
    // A small real hub package (Mirage: Arcane Warfare, v6): handler.js + "Mirage Arcane Warfare/configs/*".
    // Downloaded WITHOUT ?download=true so the test does not count as a hub download.
    private const string SamplePackageUrl = "https://hub.splitscreen.me/cdn/storage/packages/y36G52ABCs7pjJd54/original/handler-ye9NKnb7eBFREecZc-v6.nc";

    private readonly string root = Path.Combine(Path.GetTempPath(), "nc-handler-tests-" + Guid.NewGuid().ToString("N"));

    public HandlerFilesTests()
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

    [Fact]
    public void Real_hub_package_extracts_and_installs_like_the_legacy_installer()
    {
        string archive = Path.Combine(root, "download", "handler-ye9NKnb7eBFREecZc-v6.nc");
        Directory.CreateDirectory(Path.GetDirectoryName(archive)!);
        try
        {
            using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            File.WriteAllBytes(archive, http.GetByteArrayAsync(SamplePackageUrl).GetAwaiter().GetResult());
        }
        catch (Exception e) when (e is HttpRequestException || e is System.Threading.Tasks.TaskCanceledException)
        {
            return; // offline: nothing to test here
        }

        string handlers = Path.Combine(root, "handlers");
        Directory.CreateDirectory(handlers);
        int lastPercent = -1;
        var progress = new SyncProgress(p => lastPercent = p);

        ExtractedHandler extracted = HandlerFiles.Extract(archive, Path.Combine(handlers, ".install-test"), progress, CancellationToken.None);

        Assert.Equal(100, lastPercent);
        Assert.False(string.IsNullOrEmpty(extracted.Script.GameName));
        Assert.False(string.IsNullOrEmpty(extracted.Script.ExecutableName));
        Assert.Equal(HandlerScript.ToTitle(extracted.Script.GameName), extracted.Title);
        Assert.Single(extracted.AssetFolders);

        InstalledHandlerFiles installed = HandlerFiles.Install(extracted, handlers);

        Assert.True(File.Exists(Path.Combine(handlers, extracted.Title + ".js")));
        Assert.True(Directory.GetFiles(Path.Combine(handlers, extracted.Title), "*", SearchOption.AllDirectories).Length >= 2);
        Assert.True(File.Exists(Path.Combine(handlers, extracted.Title, "configs", "DefaultGame.ini")));
        Assert.Equal(extracted.Title, installed.Title);
        Assert.True(File.Exists(archive)); // the source file is never deleted
    }

    [Fact]
    public void Archive_without_directory_entries_keeps_its_assets()
    {
        string archive = MakeZip("plain.zip",
            ("handler.js", "Game.GameName = \"Plain: Game\";\nGame.ExecutableName = \"plain.exe\";\n"),
            ("Plain Game/data/file.txt", "x"));
        string handlers = Path.Combine(root, "handlers");

        ExtractedHandler extracted = HandlerFiles.Extract(archive, Path.Combine(root, "work"), null, CancellationToken.None);
        HandlerFiles.Install(extracted, handlers);

        Assert.Equal("Plain Game", extracted.Title);
        Assert.Equal("plain.exe", extracted.Script.ExecutableName);
        Assert.True(File.Exists(Path.Combine(handlers, "Plain Game.js")));
        Assert.True(File.Exists(Path.Combine(handlers, "Plain Game", "data", "file.txt")));
    }

    [Fact]
    public void Reinstall_replaces_the_previous_files()
    {
        string handlers = Path.Combine(root, "handlers");
        Directory.CreateDirectory(Path.Combine(handlers, "Game"));
        File.WriteAllText(Path.Combine(handlers, "Game", "old.txt"), "old");
        File.WriteAllText(Path.Combine(handlers, "Game.js"), "old");
        string archive = MakeZip("game.nc", ("handler.js", "Game.GameName = \"Game\";"), ("Game/new.txt", "new"));

        HandlerFiles.Install(HandlerFiles.Extract(archive, Path.Combine(root, "work"), null, CancellationToken.None), handlers);

        Assert.False(File.Exists(Path.Combine(handlers, "Game", "old.txt")));
        Assert.True(File.Exists(Path.Combine(handlers, "Game", "new.txt")));
        Assert.Contains("GameName", File.ReadAllText(Path.Combine(handlers, "Game.js")));
    }

    [Fact]
    public void Entries_outside_the_work_folder_are_refused()
    {
        string archive = MakeZip("evil.zip", ("handler.js", "Game.GameName = \"Evil\";"), ("../escaped.txt", "x"));

        Assert.Throws<InvalidHandlerPackageException>(() => HandlerFiles.Extract(archive, Path.Combine(root, "work"), null, CancellationToken.None));
        Assert.False(File.Exists(Path.Combine(root, "escaped.txt")));
    }

    [Fact]
    public void Archives_without_a_handler_are_refused()
    {
        string archive = MakeZip("nothing.zip", ("readme.txt", "hello"));
        Assert.Throws<InvalidHandlerPackageException>(() => HandlerFiles.Extract(archive, Path.Combine(root, "work"), null, CancellationToken.None));

        string notZip = Path.Combine(root, "fake.nc");
        File.WriteAllText(notZip, "not a zip");
        Assert.Throws<InvalidHandlerPackageException>(() => HandlerFiles.Extract(notZip, Path.Combine(root, "work2"), null, CancellationToken.None));
    }

    [Fact]
    public void Loose_script_is_copied_not_moved()
    {
        string script = Path.Combine(root, "My Handler.js");
        File.WriteAllText(script, "Game.GameName = \"Something Else\";\nGame.GUID = \"abc\";");
        string handlers = Path.Combine(root, "handlers");

        InstalledHandlerFiles installed = HandlerFiles.InstallScript(script, handlers);

        Assert.Equal("My Handler", installed.Title);
        Assert.Equal("abc", installed.Script.Guid);
        Assert.True(File.Exists(script));
        Assert.True(File.Exists(Path.Combine(handlers, "My Handler.js")));
    }

    private string MakeZip(string name, params (string Path, string Content)[] entries)
    {
        string path = Path.Combine(root, name);
        using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            foreach ((string entryPath, string content) in entries)
            {
                using var writer = new StreamWriter(zip.CreateEntry(entryPath).Open());
                writer.Write(content);
            }
        }

        return path;
    }

    private sealed class SyncProgress : IProgress<int>
    {
        private readonly Action<int> report;

        public SyncProgress(Action<int> report) => this.report = report;

        public void Report(int value) => report(value);
    }
}

public class HandlerScriptTests
{
    [Fact]
    public void Reads_assignments_at_the_start_of_lines()
    {
        HandlerScriptInfo info = HandlerScript.Parse(new[]
        {
            "Hub.Handler.Version = 3;",
            "   Game.ExecutableNames = [\"a.exe\", \"b.exe\"];", // must not be read as ExecutableName
            "  game.executablename = 'Game-Win64.exe'; // comment \"x\"",
            "Game.GUID = \"ENDLESS Space 2\";",
            "Game.GameName = \"Tom \\\"Quoted\\\" Game\";",
            "Game.GameName = \"Second assignment ignored\";",
        });

        Assert.Equal("Game-Win64.exe", info.ExecutableName);
        Assert.Equal("ENDLESS Space 2", info.Guid);
        Assert.Equal("Tom \"Quoted\" Game", info.GameName);
        Assert.True(info.IsHandler);
    }

    [Fact]
    public void Not_a_handler() => Assert.False(HandlerScript.Parse(new[] { "var x = 1;", "// Game.GameName = \"no\"" }).IsHandler);

    [Theory]
    [InlineData("Mirage: Arcane Warfare", "Mirage Arcane Warfare")]
    [InlineData("What? <Game> \"2\" | A/B\\C*", "What Game 2  ABC")]
    [InlineData("  Trailing dots...  ", "Trailing dots")]
    [InlineData(":?*", null)]
    [InlineData(null, null)]
    public void Titles_are_safe_file_names(string? gameName, string? expected) => Assert.Equal(expected, HandlerScript.ToTitle(gameName));
}

public class HubClientParsingTests
{
    [Fact]
    public void Handler_record()
    {
        HubHandlerInfo info = HubClient.ParseHandler("{\"Handlers\":[{\"_id\":\"fcaL3FKqLTH62jLoM\",\"gameName\":\"ENDLESS Space 2\",\"gameCover\":\"co1x7j\",\"currentVersion\":3,\"currentPackage\":\"pzS4FWr3kQXckLKC5\"}]}")!;

        Assert.Equal("fcaL3FKqLTH62jLoM", info.Id);
        Assert.Equal(3, info.CurrentVersion);
        Assert.Equal("pzS4FWr3kQXckLKC5", info.CurrentPackage);
        Assert.Equal("co1x7j", info.GameCover);
        Assert.Null(HubClient.ParseHandler("{}"));
        Assert.Null(HubClient.ParseHandler("{\"Handlers\":[]}"));
    }

    [Fact]
    public void Package_urls_count_downloads_only_when_asked()
    {
        var info = new HubHandlerInfo { Id = "fcaL3FKqLTH62jLoM", CurrentPackage = "pzS4FWr3kQXckLKC5", CurrentVersion = 3 };

        Assert.Equal("https://hub.splitscreen.me/cdn/storage/packages/pzS4FWr3kQXckLKC5/original/handler-fcaL3FKqLTH62jLoM-v3.nc?download=true", HubClient.PackageUrl(info, countDownload: true));
        Assert.EndsWith("-v3.nc", HubClient.PackageUrl(info, countDownload: false));
    }

    [Fact]
    public void Screenshots()
    {
        var urls = HubClient.ParseScreenshots("{\"screenshots\":[{\"id\":1,\"url\":\"//images.igdb.com/igdb/image/upload/t_thumb/scu5kw.jpg\"},{\"id\":2}]}");

        Assert.Equal(new[] { "//images.igdb.com/igdb/image/upload/t_thumb/scu5kw.jpg" }, urls.ToArray());
        Assert.Equal("https://images.igdb.com/igdb/image/upload/t_screenshot_big/scu5kw.jpg", HubClient.ScreenshotUrl(urls[0], "t_screenshot_big"));
        Assert.Empty(HubClient.ParseScreenshots("{}"));
    }

    [Theory]
    [InlineData("fcaL3FKqLTH62jLoM", true)]
    [InlineData("", false)]
    [InlineData(null, false)]
    [InlineData("../api", false)]
    [InlineData("abc?x=1", false)]
    public void Hub_ids(string? id, bool valid) => Assert.Equal(valid, HubClient.IsValidHubId(id));
}
