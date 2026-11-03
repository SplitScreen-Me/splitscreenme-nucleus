using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace Nucleus.Gaming.Coop.Search
{
    /// <summary>Progress of a <see cref="GameExecutableSearch"/>.</summary>
    public sealed class GameSearchProgress
    {
        /// <summary>0 to 100.</summary>
        public int Percent { get; set; }

        /// <summary>Folder being scanned (a game folder of a search root), null when done.</summary>
        public string CurrentFolder { get; set; }

        /// <summary>Executables matched so far.</summary>
        public int Found { get; set; }
    }

    /// <summary>An executable a handler accepts (name, binaries folder and <c>Game.ExecutableContext</c>).</summary>
    public sealed class GameSearchResult
    {
        public GameSearchResult(GenericGameInfo game, string executablePath)
        {
            Game = game;
            ExecutablePath = executablePath;
            InstallDirectory = Path.GetDirectoryName(executablePath);
        }

        /// <summary>The handler.</summary>
        public GenericGameInfo Game { get; }

        /// <summary>Full path of the executable.</summary>
        public string ExecutablePath { get; }

        /// <summary>Folder of the executable (what 2.5 used as the file picker's start folder).</summary>
        public string InstallDirectory { get; }
    }

    /// <summary>
    /// Nucleus 2.5 automatic game search, without its WinForms windows: walks the user's game folders and returns the
    /// executables that handlers accept. Each search root's sub-folders are the units of progress; inside, every
    /// <c>*.exe</c> is checked by file name (<c>Game.ExecutableNames</c>, else <c>Game.ExecutableName</c>), then
    /// <c>Game.BinariesFolder</c> (the executable's folder must end with it) and <c>Game.ExecutableContext</c> (negated
    /// entries included). Links and junctions are not followed; paths over 260 characters are skipped. Runs on the
    /// caller's thread: call it from a worker.
    /// </summary>
    public static class GameExecutableSearch
    {
        public static List<GameSearchResult> Search(IEnumerable<GenericGameInfo> handlers, IEnumerable<string> roots,
            IProgress<GameSearchProgress> progress = null, CancellationToken cancellation = default(CancellationToken), bool stopAtFirst = false)
        {
            List<GameSearchResult> results = new List<GameSearchResult>();
            Dictionary<string, List<GenericGameInfo>> byExe = IndexByExecutable(handlers);
            List<string> rootList = (roots ?? Enumerable.Empty<string>()).Where(r => !string.IsNullOrWhiteSpace(r)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            if (byExe.Count == 0 || rootList.Count == 0)
            {
                progress?.Report(new GameSearchProgress { Percent = 100 });
                return results;
            }

            HashSet<string> seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (int r = 0; r < rootList.Count; r++)
            {
                cancellation.ThrowIfCancellationRequested();

                string root = rootList[r];

                if (!Directory.Exists(root))
                {
                    continue;
                }

                //The root's own files, then each game folder.
                List<string> units = new List<string> { null };
                units.AddRange(SafeDirectories(root));

                for (int u = 0; u < units.Count; u++)
                {
                    cancellation.ThrowIfCancellationRequested();

                    string unit = units[u];
                    int percent = (int)((r + (u / (double)units.Count)) / rootList.Count * 100);
                    progress?.Report(new GameSearchProgress { Percent = percent, CurrentFolder = unit ?? root, Found = results.Count });

                    IEnumerable<string> exes = unit == null ? SafeFiles(root) : EnumerateExecutables(unit, cancellation);

                    foreach (string exe in exes)
                    {
                        cancellation.ThrowIfCancellationRequested();

                        if (!seen.Add(exe) || !byExe.TryGetValue(Path.GetFileName(exe), out List<GenericGameInfo> candidates))
                        {
                            continue;
                        }

                        foreach (GenericGameInfo game in candidates)
                        {
                            if (!Accepts(game, exe))
                            {
                                continue;
                            }

                            results.Add(new GameSearchResult(game, exe));

                            if (stopAtFirst)
                            {
                                progress?.Report(new GameSearchProgress { Percent = 100, Found = results.Count });
                                return results;
                            }
                        }
                    }
                }
            }

            progress?.Report(new GameSearchProgress { Percent = 100, Found = results.Count });
            return results;
        }

        /// <summary>Whether the handler accepts this executable: binaries folder and context (the name is checked by the caller).</summary>
        public static bool Accepts(GenericGameInfo game, string exePath)
        {
            string exeDir = Path.GetDirectoryName(exePath) ?? string.Empty;

            if (!string.IsNullOrEmpty(game.BinariesFolder))
            {
                string binaries = game.BinariesFolder.Trim('\\', '/');

                if (binaries.Length > 0 && !exeDir.TrimEnd('\\', '/').EndsWith(binaries, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return ExecutableContext.Check(game.ExecutableContext, exeDir).Matches;
        }

        /// <summary>Executable file name (case-insensitive) → handlers declaring it.</summary>
        public static Dictionary<string, List<GenericGameInfo>> IndexByExecutable(IEnumerable<GenericGameInfo> handlers)
        {
            Dictionary<string, List<GenericGameInfo>> byExe = new Dictionary<string, List<GenericGameInfo>>(StringComparer.OrdinalIgnoreCase);

            foreach (GenericGameInfo game in handlers ?? Enumerable.Empty<GenericGameInfo>())
            {
                if (game == null)
                {
                    continue;
                }

                IEnumerable<string> names = game.ExecutableNames != null && game.ExecutableNames.Length > 0 ? game.ExecutableNames : new[] { game.ExecutableName };

                foreach (string name in names)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    string fileName = Path.GetFileName(name.Trim());

                    if (!byExe.TryGetValue(fileName, out List<GenericGameInfo> list))
                    {
                        list = new List<GenericGameInfo>();
                        byExe[fileName] = list;
                    }

                    if (!list.Contains(game))
                    {
                        list.Add(game);
                    }
                }
            }

            return byExe;
        }

        private static IEnumerable<string> EnumerateExecutables(string folder, CancellationToken cancellation)
        {
            Stack<string> pending = new Stack<string>();
            pending.Push(folder);

            while (pending.Count > 0)
            {
                cancellation.ThrowIfCancellationRequested();

                string current = pending.Pop();

                if (current.Length > 260)
                {
                    continue;
                }

                foreach (string file in SafeFiles(current))
                {
                    yield return file;
                }

                foreach (string sub in SafeDirectories(current))
                {
                    pending.Push(sub);
                }
            }
        }

        private static IEnumerable<string> SafeFiles(string folder)
        {
            try
            {
                return Directory.GetFiles(folder, "*.exe");
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }

        private static IEnumerable<string> SafeDirectories(string folder)
        {
            try
            {
                return new DirectoryInfo(folder).GetDirectories()
                    .Where(d => (d.Attributes & FileAttributes.ReparsePoint) == 0)
                    .Select(d => d.FullName)
                    .ToArray();
            }
            catch
            {
                return Enumerable.Empty<string>();
            }
        }
    }
}
