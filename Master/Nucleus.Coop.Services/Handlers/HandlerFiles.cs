using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>A handler package extracted to a work folder, ready to be installed.</summary>
public sealed class ExtractedHandler
{
    /// <summary>The handler script inside the work folder.</summary>
    public string ScriptPath { get; set; } = "";

    /// <summary>Top-level folders of the archive (handler assets), as paths inside the work folder.</summary>
    public List<string> AssetFolders { get; set; } = new List<string>();

    /// <summary>What the script declares.</summary>
    public HandlerScriptInfo Script { get; set; } = new HandlerScriptInfo();

    /// <summary>File name (without extension) the handler is installed under: <c>handlers\&lt;Title&gt;.js</c> and <c>handlers\&lt;Title&gt;\</c>.</summary>
    public string Title { get; set; } = "";
}

/// <summary>Where an installed handler's files went.</summary>
public sealed class InstalledHandlerFiles
{
    /// <summary>Name of the script without extension (what <c>GameManager.AddScript</c> takes).</summary>
    public string Title { get; set; } = "";

    /// <summary>Full path of the installed script.</summary>
    public string ScriptPath { get; set; } = "";

    /// <summary>What the script declares.</summary>
    public HandlerScriptInfo Script { get; set; } = new HandlerScriptInfo();
}

/// <summary>The archive is not a valid handler package.</summary>
public sealed class InvalidHandlerPackageException : Exception
{
    /// <summary>Creates the exception.</summary>
    public InvalidHandlerPackageException(string message, Exception? inner = null)
        : base(message, inner)
    {
    }
}

/// <summary>
/// File operations of handler installs, extracted from <c>DownloadPrompt.ExtractHandler</c> (and the drag &amp; drop
/// code of <c>UI_Functions</c>): extract a .nc/.zip, find <c>handler.js</c>, read the game name, then move the script
/// to <c>handlers\&lt;GameName&gt;.js</c> and the archive's top-level folders into <c>handlers\&lt;GameName&gt;\</c>.
/// Differences with the legacy code: entries that would land outside the work folder are refused, top-level folders
/// are found even when the archive has no directory entries, the source file is never deleted or moved, and the
/// progress is a real percentage.
/// </summary>
public static class HandlerFiles
{
    /// <summary>Extracts a handler archive into an empty work folder.</summary>
    /// <param name="archivePath">The .nc or .zip file.</param>
    /// <param name="workFolder">Folder to extract into (created; should be empty).</param>
    /// <param name="progress">Extraction percentage.</param>
    /// <param name="cancellationToken">Stops between entries.</param>
    public static ExtractedHandler Extract(string archivePath, string workFolder, IProgress<int>? progress, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(workFolder);
        string workRoot = Path.GetFullPath(workFolder).TrimEnd('\\') + "\\";
        var topFolders = new List<string>();

        try
        {
            using ZipArchive zip = ZipFile.OpenRead(archivePath);
            List<ZipArchiveEntry> entries = zip.Entries.ToList();
            int total = Math.Max(1, entries.Count);
            int done = 0;
            int lastPercent = -1;

            foreach (ZipArchiveEntry entry in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string name = entry.FullName.Replace('\\', '/');
                string[] segments = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length == 0)
                {
                    continue;
                }

                if (segments.Any(s => s == ".." || s.Contains(':')) || name.StartsWith("/", StringComparison.Ordinal))
                {
                    throw new InvalidHandlerPackageException($"The archive contains an unsafe path: {entry.FullName}");
                }

                bool isDirectory = name.EndsWith("/", StringComparison.Ordinal);
                if (segments.Length > 1 || isDirectory)
                {
                    string top = segments[0];
                    if (!topFolders.Contains(top, StringComparer.OrdinalIgnoreCase))
                    {
                        topFolders.Add(top);
                    }
                }

                string target = Path.GetFullPath(Path.Combine(workFolder, Path.Combine(segments)));
                if (!target.StartsWith(workRoot, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidHandlerPackageException($"The archive contains an unsafe path: {entry.FullName}");
                }

                if (isDirectory)
                {
                    Directory.CreateDirectory(target);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    entry.ExtractToFile(target, overwrite: true);
                }

                done++;
                int percent = done * 100 / total;
                if (percent != lastPercent)
                {
                    lastPercent = percent;
                    progress?.Report(percent);
                }
            }
        }
        catch (InvalidDataException e)
        {
            throw new InvalidHandlerPackageException("The file is not a valid handler archive (.nc or .zip).", e);
        }

        string script = FindScript(workFolder, archivePath)
            ?? throw new InvalidHandlerPackageException("The archive has no handler.js: make sure it is a Nucleus handler package.");

        HandlerScriptInfo info = HandlerScript.ParseFile(script);
        if (!info.IsHandler)
        {
            throw new InvalidHandlerPackageException("The archive's script does not declare Game.GameName or Game.ExecutableName: it is not a Nucleus handler.");
        }

        string title = HandlerScript.ToTitle(info.GameName)
            ?? HandlerScript.ToTitle(Path.GetFileNameWithoutExtension(archivePath))
            ?? throw new InvalidHandlerPackageException("The handler has no usable game name.");

        return new ExtractedHandler
        {
            ScriptPath = script,
            AssetFolders = topFolders.Select(f => Path.Combine(workFolder, f)).Where(Directory.Exists).ToList(),
            Script = info,
            Title = title,
        };
    }

    /// <summary>Path a handler with this title is installed at.</summary>
    public static string ScriptPathFor(string handlersFolder, string title) => Path.Combine(handlersFolder, title + ".js");

    /// <summary>
    /// Moves an extracted handler into the handlers folder, replacing <c>&lt;Title&gt;.js</c> and <c>&lt;Title&gt;\</c>.
    /// The caller checks for conflicts first.
    /// </summary>
    public static InstalledHandlerFiles Install(ExtractedHandler extracted, string handlersFolder)
    {
        Directory.CreateDirectory(handlersFolder);
        string scriptTarget = ScriptPathFor(handlersFolder, extracted.Title);
        string assetsTarget = Path.Combine(handlersFolder, extracted.Title);

        DeleteHandlerFiles(handlersFolder, extracted.Title);

        File.Move(extracted.ScriptPath, scriptTarget);

        if (extracted.AssetFolders.Count > 0)
        {
            Directory.CreateDirectory(assetsTarget);
            foreach (string folder in extracted.AssetFolders)
            {
                MoveContents(folder, assetsTarget);
            }
        }

        return new InstalledHandlerFiles { Title = extracted.Title, ScriptPath = scriptTarget, Script = extracted.Script };
    }

    /// <summary>Copies a loose handler script into the handlers folder under its own file name (as drag &amp; drop did, without moving the user's file).</summary>
    public static InstalledHandlerFiles InstallScript(string scriptPath, string handlersFolder)
    {
        Directory.CreateDirectory(handlersFolder);
        string title = Path.GetFileNameWithoutExtension(scriptPath);
        string target = ScriptPathFor(handlersFolder, title);
        HandlerScriptInfo info = HandlerScript.ParseFile(scriptPath);

        if (!string.Equals(Path.GetFullPath(scriptPath), Path.GetFullPath(target), StringComparison.OrdinalIgnoreCase))
        {
            File.Copy(scriptPath, target, overwrite: true);
        }

        return new InstalledHandlerFiles { Title = title, ScriptPath = target, Script = info };
    }

    /// <summary>Deletes <c>&lt;title&gt;.js</c> and the <c>&lt;title&gt;\</c> folder of an installed handler.</summary>
    public static void DeleteHandlerFiles(string handlersFolder, string title)
    {
        string folder = Path.Combine(handlersFolder, title);
        if (Directory.Exists(folder))
        {
            Directory.Delete(folder, true);
        }

        string script = ScriptPathFor(handlersFolder, title);
        if (File.Exists(script))
        {
            File.Delete(script);
        }
    }

    /// <summary>Deletes a work folder, ignoring failures.</summary>
    public static void TryDeleteFolder(string folder)
    {
        try
        {
            if (Directory.Exists(folder))
            {
                Directory.Delete(folder, true);
            }
        }
        catch (Exception)
        {
        }
    }

    private static string? FindScript(string workFolder, string archivePath)
    {
        string handlerJs = Path.Combine(workFolder, "handler.js");
        if (File.Exists(handlerJs))
        {
            return handlerJs;
        }

        // Archives made by hand (drag & drop): <archive name>.js, or the only script at the root.
        string named = Path.Combine(workFolder, Path.GetFileNameWithoutExtension(archivePath) + ".js");
        if (File.Exists(named))
        {
            return named;
        }

        string[] scripts = Directory.GetFiles(workFolder, "*.js", SearchOption.TopDirectoryOnly);
        return scripts.Length == 1 ? scripts[0] : null;
    }

    private static void MoveContents(string source, string target)
    {
        foreach (string directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, directory.Substring(source.Length + 1)));
        }

        foreach (string file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            string destination = Path.Combine(target, file.Substring(source.Length + 1));
            if (File.Exists(destination))
            {
                File.Delete(destination);
            }

            File.Move(file, destination);
        }
    }
}
