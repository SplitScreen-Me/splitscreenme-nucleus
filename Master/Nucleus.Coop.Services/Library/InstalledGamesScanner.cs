using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// A tolerant reader of Valve KeyValues text files (<c>libraryfolders.vdf</c>, <c>appmanifest_*.acf</c>):
/// quoted or bare tokens, nested <c>{ }</c> blocks, <c>//</c> comments, backslash escapes. Keys are case-insensitive.
/// </summary>
public static class Vdf
{
    /// <summary>A node: a value, or children.</summary>
    public sealed class Node
    {
        /// <summary>Value of a leaf, null for a block.</summary>
        public string? Value { get; set; }

        /// <summary>Children of a block, in order (duplicate keys kept).</summary>
        public List<KeyValuePair<string, Node>> Children { get; } = new List<KeyValuePair<string, Node>>();

        /// <summary>First child with this key, or null.</summary>
        public Node? this[string key] => Children.FirstOrDefault(c => string.Equals(c.Key, key, StringComparison.OrdinalIgnoreCase)).Value;

        /// <summary>Value of the first child with this key, or null.</summary>
        public string? Get(string key) => this[key]?.Value;
    }

    /// <summary>Parses a document. Never throws: unreadable parts are skipped.</summary>
    public static Node Parse(string text)
    {
        var root = new Node();
        var stack = new Stack<Node>();
        stack.Push(root);
        string? pendingKey = null;
        int i = 0;

        while (true)
        {
            string? token = NextToken(text, ref i, out bool quoted);
            if (token == null)
            {
                break;
            }

            if (!quoted && token == "{")
            {
                var block = new Node();
                stack.Peek().Children.Add(new KeyValuePair<string, Node>(pendingKey ?? "", block));
                stack.Push(block);
                pendingKey = null;
            }
            else if (!quoted && token == "}")
            {
                if (stack.Count > 1)
                {
                    stack.Pop();
                }

                pendingKey = null;
            }
            else if (pendingKey == null)
            {
                pendingKey = token;
            }
            else
            {
                stack.Peek().Children.Add(new KeyValuePair<string, Node>(pendingKey, new Node { Value = token }));
                pendingKey = null;
            }
        }

        return root;
    }

    private static string? NextToken(string text, ref int i, out bool quoted)
    {
        quoted = false;
        while (i < text.Length)
        {
            char c = text[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
            }
            else if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                while (i < text.Length && text[i] != '\n')
                {
                    i++;
                }
            }
            else
            {
                break;
            }
        }

        if (i >= text.Length)
        {
            return null;
        }

        if (text[i] == '{' || text[i] == '}')
        {
            return text[i++].ToString();
        }

        var token = new StringBuilder();
        if (text[i] == '"')
        {
            quoted = true;
            i++;
            while (i < text.Length && text[i] != '"')
            {
                if (text[i] == '\\' && i + 1 < text.Length)
                {
                    char next = text[i + 1];
                    token.Append(next switch { 'n' => '\n', 't' => '\t', _ => next });
                    i += 2;
                    continue;
                }

                token.Append(text[i++]);
            }

            i++; // closing quote
            return token.ToString();
        }

        while (i < text.Length && !char.IsWhiteSpace(text[i]) && text[i] != '"' && text[i] != '{' && text[i] != '}')
        {
            token.Append(text[i++]);
        }

        return token.ToString();
    }
}

/// <summary>Games installed with Steam and the Epic Games launcher, read from their local manifests.</summary>
public static class InstalledGamesScanner
{
    private static readonly HashSet<string> IgnoredSteamApps = new HashSet<string>
    {
        "228980", // Steamworks Common Redistributables
    };

    /// <summary>Scans both stores. Missing stores give no entries.</summary>
    public static List<InstalledGame> Scan()
    {
        var games = new List<InstalledGame>();
        try
        {
            games.AddRange(ScanSteam(SteamPath()));
        }
        catch (Exception e)
        {
            ServiceLog.Warn("Steam library scan failed", e);
        }

        try
        {
            string manifests = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Epic", "EpicGamesLauncher", "Data", "Manifests");
            games.AddRange(ScanEpic(manifests));
        }
        catch (Exception e)
        {
            ServiceLog.Warn("Epic library scan failed", e);
        }

        return games.OrderBy(g => g.Name, StringComparer.CurrentCultureIgnoreCase).ToList();
    }

    /// <summary>Steam folder from <c>HKCU\Software\Valve\Steam\SteamPath</c>, or null.</summary>
    public static string? SteamPath()
    {
        using RegistryKey? key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam");
        string? path = key?.GetValue("SteamPath") as string;
        return string.IsNullOrWhiteSpace(path) ? null : path!.Replace('/', '\\');
    }

    /// <summary>Library folders listed in <c>steamapps\libraryfolders.vdf</c> (both file formats), the Steam folder first.</summary>
    public static List<string> SteamLibraries(string steamPath, string? libraryFoldersVdf)
    {
        var libraries = new List<string> { steamPath };
        if (libraryFoldersVdf != null)
        {
            Vdf.Node root = Vdf.Parse(libraryFoldersVdf);
            Vdf.Node? folders = root["libraryfolders"] ?? root["LibraryFolders"];
            if (folders != null)
            {
                foreach (KeyValuePair<string, Vdf.Node> entry in folders.Children)
                {
                    // New format: "0" { "path" "D:\\SteamLibrary" ... }; old format: "1" "D:\\SteamLibrary".
                    string? path = entry.Value.Value ?? entry.Value.Get("path");
                    if (!string.IsNullOrWhiteSpace(path) && int.TryParse(entry.Key, out _))
                    {
                        libraries.Add(path!.Replace('/', '\\'));
                    }
                }
            }
        }

        return libraries.Select(l => l.TrimEnd('\\')).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
    }

    /// <summary>Installed Steam games of every library.</summary>
    public static List<InstalledGame> ScanSteam(string? steamPath)
    {
        var games = new List<InstalledGame>();
        if (steamPath == null || !Directory.Exists(steamPath))
        {
            return games;
        }

        string vdfPath = Path.Combine(steamPath, "steamapps", "libraryfolders.vdf");
        string? vdf = File.Exists(vdfPath) ? File.ReadAllText(vdfPath) : null;

        foreach (string library in SteamLibraries(steamPath, vdf))
        {
            string steamapps = Path.Combine(library, "steamapps");
            if (!Directory.Exists(steamapps))
            {
                continue;
            }

            foreach (string manifest in Directory.EnumerateFiles(steamapps, "appmanifest_*.acf"))
            {
                try
                {
                    InstalledGame? game = ParseSteamManifest(File.ReadAllText(manifest), steamapps);
                    if (game != null && Directory.Exists(game.InstallFolder) && !games.Any(g => g.StoreId == game.StoreId))
                    {
                        games.Add(game);
                    }
                }
                catch (Exception e)
                {
                    ServiceLog.Warn($"Steam manifest {manifest} could not be read", e);
                }
            }
        }

        return games;
    }

    /// <summary>Reads an <c>appmanifest_*.acf</c>: <c>AppState { appid, name, installdir }</c>.</summary>
    public static InstalledGame? ParseSteamManifest(string acf, string steamappsFolder)
    {
        Vdf.Node? state = Vdf.Parse(acf)["AppState"];
        string? appId = state?.Get("appid");
        string? name = state?.Get("name");
        string? installDir = state?.Get("installdir");
        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(installDir) || IgnoredSteamApps.Contains(appId!))
        {
            return null;
        }

        return new InstalledGame
        {
            Store = GameStore.Steam,
            StoreId = appId!,
            Name = string.IsNullOrWhiteSpace(name) ? installDir! : name!,
            InstallFolder = Path.Combine(steamappsFolder, "common", installDir!),
        };
    }

    /// <summary>Installed Epic games from the launcher's <c>*.item</c> manifests.</summary>
    public static List<InstalledGame> ScanEpic(string manifestsFolder)
    {
        var games = new List<InstalledGame>();
        if (!Directory.Exists(manifestsFolder))
        {
            return games;
        }

        foreach (string file in Directory.EnumerateFiles(manifestsFolder, "*.item"))
        {
            try
            {
                InstalledGame? game = ParseEpicManifest(File.ReadAllText(file));
                if (game != null && Directory.Exists(game.InstallFolder) && !games.Any(g => g.StoreId == game.StoreId))
                {
                    games.Add(game);
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn($"Epic manifest {file} could not be read", e);
            }
        }

        return games;
    }

    /// <summary>Reads an Epic <c>.item</c> manifest (JSON: <c>AppName</c>, <c>DisplayName</c>, <c>InstallLocation</c>).</summary>
    public static InstalledGame? ParseEpicManifest(string json)
    {
        if (JToken.Parse(json) is not JObject item)
        {
            return null;
        }

        string? appName = (string?)item["AppName"];
        string? location = (string?)item["InstallLocation"];
        bool incomplete = (bool?)item["bIsIncompleteInstall"] ?? false;
        if (string.IsNullOrWhiteSpace(appName) || string.IsNullOrWhiteSpace(location) || incomplete)
        {
            return null;
        }

        string? display = (string?)item["DisplayName"];
        return new InstalledGame
        {
            Store = GameStore.Epic,
            StoreId = appName!,
            Name = string.IsNullOrWhiteSpace(display) ? appName! : display!,
            InstallFolder = location!.Replace('/', '\\'),
        };
    }
}

/// <summary>Finds a game executable by name under a folder, shallowest match first.</summary>
public static class ExecutableFinder
{
    /// <summary>Folders deeper than this are not searched.</summary>
    public const int MaxDepth = 6;

    /// <summary>The search stops after visiting this many folders.</summary>
    public const int MaxFolders = 20000;

    private static readonly HashSet<string> SkippedFolders = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "_CommonRedist", "CommonRedist", "Redist", "Redistributables", "__Installer", "Installer", "Installers", "DirectX", "DotNetFX",
        "vcredist", "$RECYCLE.BIN", "System Volume Information", ".git", "node_modules", "ShaderCache", "shadercache", "Logs",
    };

    /// <summary>Breadth-first search, case-insensitive, without following links. Null when not found.</summary>
    public static string? Find(string folder, string executableName, CancellationToken cancellationToken)
    {
        if (!Directory.Exists(folder))
        {
            return null;
        }

        var queue = new Queue<(string Path, int Depth)>();
        queue.Enqueue((folder, 0));
        int visited = 0;

        while (queue.Count > 0 && visited < MaxFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();
            (string current, int depth) = queue.Dequeue();
            visited++;

            try
            {
                string candidate = Path.Combine(current, executableName);
                if (File.Exists(candidate))
                {
                    // Report the file name as it is on disk.
                    return Directory.EnumerateFiles(current, executableName).FirstOrDefault() ?? candidate;
                }

                if (depth >= MaxDepth)
                {
                    continue;
                }

                foreach (string child in Directory.EnumerateDirectories(current))
                {
                    var info = new DirectoryInfo(child);
                    if (SkippedFolders.Contains(info.Name) || (info.Attributes & FileAttributes.ReparsePoint) != 0)
                    {
                        continue;
                    }

                    queue.Enqueue((child, depth + 1));
                }
            }
            catch (Exception e) when (e is UnauthorizedAccessException || e is IOException || e is System.Security.SecurityException)
            {
                // Unreadable folder: skip it.
            }
        }

        return null;
    }
}
