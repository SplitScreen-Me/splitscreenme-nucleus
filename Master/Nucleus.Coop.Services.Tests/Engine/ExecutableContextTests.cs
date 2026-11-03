using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge;
using Nucleus.Coop.Services.Library;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>Game.ExecutableContext with negated entries and the report of what is missing (Nucleus 2.5).</summary>
public sealed class ExecutableContextTests : IDisposable
{
    private readonly string root = Path.Combine(Path.GetTempPath(), "nucleus-context-" + Guid.NewGuid().ToString("N"));

    public ExecutableContextTests()
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

    private string Game(string folder, string exe, params string[] entries)
    {
        string dir = Path.Combine(root, folder);
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, exe), "");

        foreach (string entry in entries)
        {
            if (entry.EndsWith("/", StringComparison.Ordinal))
            {
                Directory.CreateDirectory(Path.Combine(dir, entry.TrimEnd('/')));
            }
            else
            {
                File.WriteAllText(Path.Combine(dir, entry), "");
            }
        }

        return Path.Combine(dir, exe);
    }

    [Fact]
    public void No_context_matches()
    {
        Assert.True(ExecutableContext.Check(null, root).Matches);
        Assert.True(ExecutableContext.Check(new string[0], root).Matches);
    }

    [Fact]
    public void Required_entries_must_exist_files_or_folders()
    {
        string exe = Game("a", "game.exe", "data.pak", "levels/");
        ContextCheck check = ExecutableContext.Check(new[] { "data.pak", "levels", "missing.dll" }, Path.GetDirectoryName(exe));

        Assert.False(check.Matches);
        Assert.Equal(new[] { "missing.dll" }, check.Missing);
        Assert.Empty(check.Forbidden);
    }

    [Fact]
    public void Negated_entry_rejects_when_present()
    {
        string withPacks = Game("b", "game.exe", "packs/");
        string withoutPacks = Game("c", "game.exe");

        ContextCheck rejected = ExecutableContext.Check(new[] { "!packs" }, Path.GetDirectoryName(withPacks));
        ContextCheck accepted = ExecutableContext.Check(new[] { "!packs" }, Path.GetDirectoryName(withoutPacks));

        Assert.False(rejected.Matches);
        Assert.Equal(new[] { "packs" }, rejected.Forbidden);
        Assert.Empty(rejected.Missing);
        Assert.True(accepted.Matches);
    }

    [Fact]
    public void Mixed_required_and_negated_entries()
    {
        string exe = Game("d", "game.exe", "base.pak");
        string dir = Path.GetDirectoryName(exe)!;

        Assert.True(ExecutableContext.Check(new[] { "base.pak", "!dlc.pak" }, dir).Matches);

        File.WriteAllText(Path.Combine(dir, "dlc.pak"), "");
        ContextCheck check = ExecutableContext.Check(new[] { "base.pak", "!dlc.pak", "other.pak" }, dir);
        Assert.Equal(new[] { "other.pak" }, check.Missing);
        Assert.Equal(new[] { "dlc.pak" }, check.Forbidden);
    }

    [Fact]
    public void Blank_and_invalid_entries_are_ignored_or_absent()
    {
        string exe = Game("e", "game.exe");
        ContextCheck check = ExecutableContext.Check(new[] { "", "  ", "!", "bad|name" }, Path.GetDirectoryName(exe));

        Assert.Equal(new[] { "bad|name" }, check.Missing);
    }

    [Fact]
    public void Evaluate_splits_matches_and_rejected_handlers_with_their_report()
    {
        string exe = Game("f", "Shared.exe", "packs/");
        GenericGameInfo plain = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("plain"), "Plain", "shared.exe"));
        GenericGameInfo noPacks = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("nopacks"), "Without packs", "Shared.exe", "Game.ExecutableContext = [\"!packs\"];"));
        GenericGameInfo needsData = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("data"), "Needs data", "Shared.exe", "Game.ExecutableContext = [\"data.pak\"];"));
        GenericGameInfo other = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("other"), "Other", "other.exe"));

        ExecutableMatch match = ExecutableMatch.Evaluate(new[] { plain, noPacks, needsData, other }, exe);

        Assert.Equal(new[] { plain }, match.Matches);
        Assert.Equal(2, match.Rejected.Count);
        Assert.Equal(new[] { "packs" }, match.Rejected.Single(r => r.Game == noPacks).Forbidden);
        Assert.Equal(new[] { "data.pak" }, match.Rejected.Single(r => r.Game == needsData).Missing);
        Assert.False(match.NoHandlerForName);
    }

    [Fact]
    public void Evaluate_prefers_executable_names()
    {
        string exe = Game("g", "launcher.exe");
        GenericGameInfo names = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("names"), "Names", "game.exe", "Game.ExecutableNames = [\"launcher.exe\", \"game.exe\"];"));
        GenericGameInfo single = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("single"), "Single", "launcher.exe"));

        ExecutableMatch match = ExecutableMatch.Evaluate(new[] { names, single }, exe);

        Assert.Equal(new[] { names }, match.Matches);
        Assert.True(ExecutableMatch.Evaluate(new[] { names }, Path.Combine(root, "g", "unknown.exe")).NoHandlerForName);
    }

    [Fact]
    public void Add_game_error_lists_every_rejected_handler()
    {
        string exe = Game("h", "game.exe", "packs/");
        GenericGameInfo noPacks = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("nopacks"), "Base game", "game.exe", "Game.ExecutableContext = [\"!packs\"];"));
        GenericGameInfo needsData = EngineTestHandlers.Parse(EngineTestHandlers.Script(EngineTestHandlers.NewGuid("data"), "Expansion", "game.exe", "Game.ExecutableContext = [\"expansion.dat\"];"));

        BridgeException error = LibraryService.NoHandler(exe, ExecutableMatch.Evaluate(new[] { noPacks, needsData }, exe));

        Assert.Equal(BridgeErrorKind.NotFound, error.Kind);
        JObject data = JObject.FromObject(error.ErrorData!);
        Assert.Equal("contextMissing", (string?)data["reason"]);
        List<JToken> handlers = data["handlers"]!.ToList();
        Assert.Equal(2, handlers.Count);
        Assert.Equal(new[] { "packs" }, handlers.Single(h => (string?)h["gameId"] == noPacks.GUID)["excluded"]!.Select(t => (string)t!));
        Assert.Equal(new[] { "expansion.dat" }, handlers.Single(h => (string?)h["gameId"] == needsData.GUID)["missing"]!.Select(t => (string)t!));
    }

    [Fact]
    public void Add_game_error_without_handler_for_the_name()
    {
        string exe = Game("i", "nothing.exe");
        BridgeException error = LibraryService.NoHandler(exe, ExecutableMatch.Evaluate(Array.Empty<GenericGameInfo>(), exe));

        Assert.Equal("noHandler", (string?)JObject.FromObject(error.ErrorData!)["reason"]);
    }
}
