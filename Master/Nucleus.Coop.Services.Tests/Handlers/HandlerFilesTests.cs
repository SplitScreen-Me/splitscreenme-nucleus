using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Gaming;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Handlers;

public sealed class HandlerFilesTests : IDisposable
{
    // A small real hub package (Mirage: Arcane Warfare, v6): handler.js + "Mirage Arcane Warfare/configs/*". Downloaded
    // from the API v3 with count=false, so the test is never counted as a hub download, and checked against its SHA-256.
    private const string SampleHandlerId = "ye9NKnb7eBFREecZc";
    private const int SampleVersion = 6;

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
        string archive = Path.Combine(root, "download", $"handler-{SampleHandlerId}-v{SampleVersion}.nc");
        Directory.CreateDirectory(Path.GetDirectoryName(archive)!);
        using (var hub = new HubClient(HubSite.DefaultUrl))
        {
            string? versions = null;
            try
            {
                versions = hub.GetStringAsync(hub.HandlerUrl(SampleHandlerId) + "/versions", hub.ApiTimeout, CancellationToken.None).GetAwaiter().GetResult();
            }
            catch (HubUnavailableException)
            {
                return; // offline: nothing to test here
            }

            Assert.NotNull(versions); // the sample handler must stay listed
            string sha256 = (string)Newtonsoft.Json.Linq.JObject.Parse(versions!)["items"]!.First(v => (int)v["version"]! == SampleVersion)["sha256"]!;
            hub.DownloadToFileAsync(hub.PackageUrl(SampleHandlerId, SampleVersion, countDownload: false), archive, sha256, null, CancellationToken.None).GetAwaiter().GetResult();
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
