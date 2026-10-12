using System;
using System.IO;
using System.Linq;
using System.Text;
using Nucleus.Bridge.Model;

namespace Nucleus.Bridge.Gen;

/// <summary>
/// <c>Nucleus.Bridge.Gen --out &lt;contract.ts&gt; [--check]</c>: generates the TypeScript contract of Nucleus.Bridge.
/// With <c>--check</c>, writes nothing and exits with 1 when the file is stale. Exit codes: 0 success, 1 stale file
/// or invalid contract, 2 bad arguments.
/// </summary>
internal static class Program
{
    private const string Usage = "Usage: Nucleus.Bridge.Gen --out <contract.ts> [--check]";

    private static int Main(string[] args)
    {
        string? output = null;
        var check = false;
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--out" && i + 1 < args.Length)
            {
                output = args[++i];
            }
            else if (args[i] == "--check")
            {
                check = true;
            }
            else
            {
                output = null;
                break;
            }
        }

        if (output == null)
        {
            Console.Error.WriteLine(Usage);
            return 2;
        }

        ContractModel model;
        try
        {
            model = LoadContract();
        }
        catch (ContractModelException e)
        {
            Console.Error.WriteLine(e.Message);
            return 1;
        }

        foreach (var name in DocCoverage.Undocumented(model))
        {
            Console.Error.WriteLine($"warning: {name} has no XML documentation.");
        }

        var code = TypeScriptGenerator.Generate(model);
        var path = Path.GetFullPath(output);
        var existing = File.Exists(path) ? File.ReadAllText(path).Replace("\r\n", "\n") : null;
        if (existing == code)
        {
            Console.WriteLine($"{path} is up to date (contract {model.Hash}).");
            return 0;
        }

        if (check)
        {
            Console.Error.WriteLine($"{path} is stale: run `npm run bridge:gen`.");
            return 1;
        }

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, code, new UTF8Encoding(false));
        Console.WriteLine($"Wrote {path}: contract {model.Hash}, {model.Services.Count} services, {model.Methods.Count()} methods, {model.Events.Count} events, {model.Types.Count} types.");
        return 0;
    }

    private static ContractModel LoadContract()
    {
        var assembly = typeof(BridgeHost).Assembly;
        var docsPath = Path.ChangeExtension(assembly.Location, ".xml");
        if (!File.Exists(docsPath))
        {
            Console.Error.WriteLine($"warning: {docsPath} not found: the generated file will have no documentation.");
            return ContractModel.Build(assembly);
        }

        return ContractModel.Build(assembly, XmlDocs.Load(docsPath));
    }
}
