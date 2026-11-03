using System;
using System.IO;
using System.Text;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Tests.Engine;

/// <summary>Real handlers parsed by Jint for the engine tests, with metaInfo.json kept in a temporary folder.</summary>
internal static class EngineTestHandlers
{
    private static readonly object Sync = new object();
    private static string? metaFolder;

    /// <summary>Points GameMetaInfo at a temporary folder (once per test run: the setting is process-wide).</summary>
    public static void UseTemporaryMetaInfo()
    {
        lock (Sync)
        {
            if (metaFolder == null)
            {
                metaFolder = Path.Combine(Path.GetTempPath(), "nucleus-engine-tests-meta-" + Guid.NewGuid().ToString("N"));
                Directory.CreateDirectory(metaFolder);
                GameMetaInfo.StorageFolder = metaFolder;
            }
        }
    }

    /// <summary>Script text of a minimal handler.</summary>
    public static string Script(string guid, string gameName, string executable, string? extra = null) =>
        $"Game.GameName = \"{gameName}\";\nGame.GUID = \"{guid}\";\nGame.ExecutableName = \"{executable}\";\n{extra ?? ""}\n";

    /// <summary>Parses a script without a file (no error prompt).</summary>
    public static GenericGameInfo Parse(string script, string fileName = "test.js")
    {
        UseTemporaryMetaInfo();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(script));
        return new GenericGameInfo(fileName, Path.Combine(Path.GetTempPath(), "nucleus-no-folder"), stream, new[] { false, false }, false);
    }

    /// <summary>A unique GUID for a test (metaInfo values are shared per GUID in the process).</summary>
    public static string NewGuid(string prefix) => prefix + "-" + Guid.NewGuid().ToString("N");

    /// <summary>Writes a script file and moves its write time forward, so a scan sees the change even within the same tick.</summary>
    public static void WriteScript(string path, string script, int secondsLater = 0)
    {
        File.WriteAllText(path, script);
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddSeconds(secondsLater));
    }
}
