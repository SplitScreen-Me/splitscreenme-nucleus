using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// <c>Game.ExecutableContext</c>: files or folders, relative to the executable's folder, that tell a game's install
    /// apart from another game shipping an executable with the same name. An entry starting with <c>!</c> is negated: the
    /// handler does not match when that file or folder exists (Nucleus 2.5, e.g. <c>["!packs"]</c>).
    /// </summary>
    public static class ExecutableContext
    {
        /// <summary>Checks the context of a handler against an executable's folder. Null or empty context: always matches.</summary>
        public static ContextCheck Check(string[] context, string exeDirectory)
        {
            ContextCheck result = new ContextCheck();

            if (context == null)
            {
                return result;
            }

            foreach (string raw in context)
            {
                if (string.IsNullOrWhiteSpace(raw))
                {
                    continue;
                }

                string entry = raw.Trim();
                bool negated = entry.StartsWith("!", StringComparison.Ordinal);
                string relative = negated ? entry.TrimStart('!') : entry;

                if (relative.Length == 0)
                {
                    continue;
                }

                bool exists = Exists(exeDirectory, relative);

                if (negated && exists)
                {
                    result.present.Add(relative);
                }
                else if (!negated && !exists)
                {
                    result.missing.Add(relative);
                }
            }

            return result;
        }

        private static bool Exists(string exeDirectory, string relative)
        {
            try
            {
                string path = Path.Combine(exeDirectory ?? string.Empty, relative);
                return File.Exists(path) || Directory.Exists(path);
            }
            catch (ArgumentException)
            {
                //Illegal characters in a handler value: treat as absent.
                return false;
            }
        }
    }

    /// <summary>Result of <see cref="ExecutableContext.Check"/>.</summary>
    public sealed class ContextCheck
    {
        internal readonly List<string> missing = new List<string>();
        internal readonly List<string> present = new List<string>();

        /// <summary>Required entries (relative paths) that don't exist next to the executable.</summary>
        public IReadOnlyList<string> Missing => missing;

        /// <summary>Negated entries (relative paths, without the <c>!</c>) that exist next to the executable.</summary>
        public IReadOnlyList<string> Forbidden => present;

        /// <summary>The handler matches this install.</summary>
        public bool Matches => missing.Count == 0 && present.Count == 0;
    }

    /// <summary>A handler whose executable name matched but whose <c>Game.ExecutableContext</c> did not.</summary>
    public sealed class ContextMismatch
    {
        public ContextMismatch(GenericGameInfo game, ContextCheck check)
        {
            Game = game;
            Missing = check.Missing.ToList();
            Forbidden = check.Forbidden.ToList();
        }

        /// <summary>The handler.</summary>
        public GenericGameInfo Game { get; }

        /// <summary>Required files or folders that are missing (relative to the executable's folder).</summary>
        public IReadOnlyList<string> Missing { get; }

        /// <summary>Negated entries found next to the executable (relative paths, without the <c>!</c>).</summary>
        public IReadOnlyList<string> Forbidden { get; }
    }

    /// <summary>Handlers matching an executable (<see cref="GameManager.MatchExecutable"/>).</summary>
    public sealed class ExecutableMatch
    {
        internal readonly List<GenericGameInfo> matches = new List<GenericGameInfo>();
        internal readonly List<ContextMismatch> rejected = new List<ContextMismatch>();

        /// <summary>Handlers that accept the executable (name and context).</summary>
        public IReadOnlyList<GenericGameInfo> Matches => matches;

        /// <summary>Handlers for this executable name whose context did not match, with what is missing or in the way.</summary>
        public IReadOnlyList<ContextMismatch> Rejected => rejected;

        /// <summary>No handler declares this executable name.</summary>
        public bool NoHandlerForName => matches.Count == 0 && rejected.Count == 0;

        /// <summary>
        /// Handlers declaring this executable (<c>Game.ExecutableNames</c>, else <c>Game.ExecutableName</c>, compared
        /// case-insensitively), split by <c>Game.ExecutableContext</c> against the executable's folder.
        /// </summary>
        public static ExecutableMatch Evaluate(IEnumerable<GenericGameInfo> handlers, string exePath)
        {
            ExecutableMatch match = new ExecutableMatch();

            if (string.IsNullOrEmpty(exePath) || handlers == null)
            {
                return match;
            }

            string fileName = Path.GetFileName(exePath);
            string dir = Path.GetDirectoryName(exePath);
            List<GenericGameInfo> all = handlers.Where(h => h != null).ToList();

            //a workaround so we can choose different game executables.
            List<GenericGameInfo> possibilities = all
                .Where(gen => gen.ExecutableNames != null && gen.ExecutableNames.Any(exe => string.Equals(exe, fileName, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (possibilities.Count == 0)
            {
                possibilities = all.Where(c => string.Equals(c.ExecutableName, fileName, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            foreach (GenericGameInfo game in possibilities)
            {
                ContextCheck check = ExecutableContext.Check(game.ExecutableContext, dir);

                if (check.Matches)
                {
                    match.matches.Add(game);
                }
                else
                {
                    match.rejected.Add(new ContextMismatch(game, check));
                }
            }

            return match;
        }
    }
}
