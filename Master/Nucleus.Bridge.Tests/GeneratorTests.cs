using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using Nucleus.Bridge.Gen;
using Nucleus.Bridge.Model;
using Nucleus.Bridge.Tests.TestContract;
using Xunit;

namespace Nucleus.Bridge.Tests;

public class GeneratorTests
{
    /// <summary>
    /// Compares the TypeScript generated for the shelf contract with Snapshots/ShelfContract.ts.
    /// To accept a change, run the tests with UPDATE_SNAPSHOTS=1 and review the diff.
    /// </summary>
    [Fact]
    public void Matches_the_snapshot()
    {
        var actual = TypeScriptGenerator.Generate(ContractModel.Build(ShelfContract.Types, ContractModelTests.Docs));
        var path = Path.Combine(SourceDirectory(), "Snapshots", "ShelfContract.ts");

        if (Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1")
        {
            File.WriteAllText(path, actual);
        }

        var expected = File.ReadAllText(path).Replace("\r\n", "\n");
        Assert.True(expected == actual, $"The generated TypeScript differs from {path}. Run the tests with UPDATE_SNAPSHOTS=1 and review the diff.");
    }

    [Fact]
    public void Is_deterministic()
    {
        var first = TypeScriptGenerator.Generate(ContractModel.Build(ShelfContract.Types, ContractModelTests.Docs));
        var second = TypeScriptGenerator.Generate(ContractModel.Build(ShelfContract.Types.Reverse(), ContractModelTests.Docs));

        Assert.Equal(first, second);
    }

    [Fact]
    public void Uses_erasable_syntax_only()
    {
        var code = TypeScriptGenerator.Generate(ContractModel.Default);

        Assert.DoesNotContain("enum ", code);
        Assert.DoesNotContain("namespace ", code);
        Assert.DoesNotContain("\r", code);
        Assert.Contains($"export const contractHash = '{ContractModel.Default.Hash}';", code);
    }

    [Fact]
    public void Lists_undocumented_members()
    {
        var undocumented = DocCoverage.Undocumented(ContractModel.Build(new[] { typeof(IUndocumentedService) })).ToList();

        Assert.Contains("service undocumented (IUndocumentedService)", undocumented);
        Assert.Contains("method undocumented.get", undocumented);
    }

    private static string SourceDirectory([CallerFilePath] string path = "") => Path.GetDirectoryName(path)!;
}

[BridgeService("undocumented")]
public interface IUndocumentedService
{
    string Get();
}
