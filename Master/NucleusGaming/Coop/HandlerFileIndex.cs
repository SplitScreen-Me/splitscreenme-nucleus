using Nucleus.Gaming.Coop.Generic;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.Coop
{
    /// <summary>
    /// Bookkeeping of the handler scripts of a folder (Nucleus 2.5 hot reload): every file as last read, valid or not,
    /// keyed by full path. Keeps the games dictionaries of <see cref="GameManager"/> (GUID -> parsed handler) in step
    /// when files are added, edited, broken, fixed or deleted. <see cref="Scan"/> runs on any thread; the other members
    /// run on the thread that owns the library (the dictionaries and the <see cref="UserGameInfo"/> entries).
    /// </summary>
    public sealed class HandlerFileIndex
    {
        private readonly string folder;
        private readonly Dictionary<string, GenericGameInfo> games;
        private readonly Dictionary<string, GenericGameInfo> gameInfos;
        private readonly Dictionary<string, HandlerInfo> handlerFiles = new Dictionary<string, HandlerInfo>(StringComparer.OrdinalIgnoreCase);
        private readonly object handlersLock = new object();

        public HandlerFileIndex(string folder, Dictionary<string, GenericGameInfo> games, Dictionary<string, GenericGameInfo> gameInfos)
        {
            this.folder = folder;
            this.games = games;
            this.gameInfos = gameInfos;
        }

        /// <summary>Every file as last read, sorted by name.</summary>
        public List<HandlerInfo> Snapshot()
        {
            lock (handlersLock)
            {
                return handlerFiles.Values.OrderBy(h => h.FileName, StringComparer.OrdinalIgnoreCase).ToList();
            }
        }

        /// <summary>The file exists and never parsed (a library entry bound to it is kept until it is fixed).</summary>
        public bool IsBroken(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return false;
            }

            lock (handlersLock)
            {
                return handlerFiles.TryGetValue(path, out HandlerInfo info) && info.GenericGameInfo == null;
            }
        }

        /// <summary>Scripts of the handlers folder that are not handlers.</summary>
        public static bool IsIgnoredScript(string fileName)
        {
            return string.Equals(fileName, "MasterHandler.js", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>Records a file as read now (startup, AddScript).</summary>
        public void Record(HandlerInfo info)
        {
            lock (handlersLock)
            {
                handlerFiles[info.FileFullPath] = info;
            }
        }

        /// <summary>
        /// Lists the handlers folder and parses every new or changed script (any thread, it doesn't touch the library).
        /// Pass the result to <see cref="Apply"/> on the thread that owns the library. With
        /// <paramref name="all"/>, every script is parsed again (the library "reload" action).
        /// </summary>
        public HandlerScan Scan(bool all = false)
        {
            HandlerScan scan = new HandlerScan { Forced = all };
            Dictionary<string, HandlerInfo> known;

            lock (handlersLock)
            {
                known = new Dictionary<string, HandlerInfo>(handlerFiles, StringComparer.OrdinalIgnoreCase);
            }

            FileInfo[] files;

            try
            {
                files = new DirectoryInfo(folder).GetFiles("*.js");
            }
            catch (Exception e)
            {
                LogManager.Log("Handlers scan failed: " + e.Message);
                return scan;
            }

            HashSet<string> present = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (FileInfo file in files)
            {
                if (IsIgnoredScript(file.Name))
                {
                    continue;
                }

                present.Add(file.FullName);

                if (known.TryGetValue(file.FullName, out HandlerInfo previous) && previous.LastWriteTimeUtc == file.LastWriteTimeUtc && !all)
                {
                    continue;//no change
                }

                scan.parsed.Add(ParseHandlerFile(file, previous));
            }

            foreach (string path in known.Keys)
            {
                if (!present.Contains(path))
                {
                    scan.deleted.Add(path);
                }
            }

            return scan;
        }

        private static HandlerInfo ParseHandlerFile(FileInfo file, HandlerInfo previous)
        {
            DateTime lastWrite = file.LastWriteTimeUtc;

            try
            {
                bool updateAvailable = previous?.GenericGameInfo?.UpdateAvailable ?? false;
                GenericGameInfo info = GenericGameInfo.Load(file, new bool[] { false, updateAvailable }, reportScriptError: false);
                return new HandlerInfo(file.FullName, file.Name, lastWrite, info, HandlerInfo.Describe(info));
            }
            catch (IOException e)
            {
                //Still being written: keep the previous write time so the next scan tries again.
                return new HandlerInfo(file.FullName, file.Name, previous?.LastWriteTimeUtc ?? DateTime.MinValue, null, e.Message);
            }
            catch (Exception e)
            {
                return new HandlerInfo(file.FullName, file.Name, lastWrite, null, e.InnerException != null ? e.InnerException.Message + ": " + e.Message : e.Message);
            }
        }

        /// <summary>
        /// Swaps a scan in: new and fixed handlers join the games dictionary, changed ones replace their previous parse,
        /// broken edits keep the previous version, deleted files leave (another file with the same GUID takes over). Library
        /// entries are bound to the new parse (their <c>ExecutableName</c> is set again for <c>Game.ExecutableNames</c>
        /// handlers). Call it on the thread that owns the library, not during a session.
        /// </summary>
        public HandlerChanges Apply(HandlerScan scan, IEnumerable<UserGameInfo> library)
        {
            HandlerChanges changes = new HandlerChanges();

            if (scan == null || scan.IsEmpty)
            {
                return changes;
            }

            HashSet<string> touchedGuids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            lock (handlersLock)
            {
                foreach (HandlerInfo parsed in scan.parsed)
                {
                    handlerFiles.TryGetValue(parsed.FileFullPath, out HandlerInfo previous);

                    if (!scan.Forced && previous != null && previous.LastWriteTimeUtc == parsed.LastWriteTimeUtc && parsed.Error == null && previous.Error == null)
                    {
                        continue;//already applied (AddScript or another scan)
                    }

                    if (parsed.IsValid)
                    {
                        handlerFiles[parsed.FileFullPath] = parsed;

                        if (previous == null)
                        {
                            changes.added.Add(parsed);
                        }
                        else if (!previous.IsValid)
                        {
                            changes.fixedHandlers.Add(parsed);
                        }
                        else
                        {
                            changes.updated.Add(parsed);
                        }

                        if (previous?.GUID != null && !string.Equals(previous.GUID, parsed.GUID, StringComparison.OrdinalIgnoreCase))
                        {
                            touchedGuids.Add(previous.GUID);//the file now declares another game
                        }

                        //The new parse takes the GUID over, as the last loaded script did at startup.
                        games[parsed.GUID] = parsed.GenericGameInfo;
                        gameInfos[parsed.GUID] = parsed.GenericGameInfo;
                        touchedGuids.Add(parsed.GUID);
                        continue;
                    }

                    //Broken: keep the version in use, report the error once.
                    bool sameError = previous != null && !previous.IsValid && previous.Error == parsed.Error;
                    handlerFiles[parsed.FileFullPath] = new HandlerInfo(parsed.FileFullPath, parsed.FileName, parsed.LastWriteTimeUtc, previous?.GenericGameInfo, parsed.Error);

                    if (!sameError)
                    {
                        changes.invalid.Add(handlerFiles[parsed.FileFullPath]);
                    }
                }

                foreach (string path in scan.deleted)
                {
                    if (!handlerFiles.TryGetValue(path, out HandlerInfo previous) || File.Exists(path))
                    {
                        continue;
                    }

                    handlerFiles.Remove(path);
                    changes.removed.Add(previous);

                    if (previous.GUID != null)
                    {
                        touchedGuids.Add(previous.GUID);
                    }
                }

                foreach (string guid in touchedGuids)
                {
                    games.TryGetValue(guid, out GenericGameInfo current);

                    //Still provided by a file? Keep the current parse if its file is still there, else another file's.
                    HandlerInfo[] providers = handlerFiles.Values.Where(h => h.GenericGameInfo != null && string.Equals(h.GUID, guid, StringComparison.OrdinalIgnoreCase)).ToArray();
                    GenericGameInfo winner = providers.Select(h => h.GenericGameInfo).FirstOrDefault(g => ReferenceEquals(g, current))
                        ?? providers.OrderByDescending(h => h.LastWriteTimeUtc).Select(h => h.GenericGameInfo).FirstOrDefault();

                    if (winner == null)
                    {
                        games.Remove(guid);
                        gameInfos.Remove(guid);
                    }
                    else
                    {
                        games[guid] = winner;
                        gameInfos[guid] = winner;
                    }

                    changes.gameIds.Add(guid);
                }
            }

            if (library != null)
            {
                foreach (UserGameInfo entry in library)
                {
                    if (entry == null || !changes.gameIds.Contains(entry.GameGuid, StringComparer.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (games.TryGetValue(entry.GameGuid, out GenericGameInfo game))
                    {
                        //"Fixed Null ExecutableName after a handler gets refreshed when a handler uses ExecutableNames".
                        if (game.ExecutableNames != null && !string.IsNullOrEmpty(entry.ExePath))
                        {
                            game.ExecutableName = Path.GetFileName(entry.ExePath);
                        }

                        entry.Rebind(game);
                        changes.rebound.Add(entry);
                    }
                    else
                    {
                        entry.Rebind(null);

                        //Its script was deleted (a broken script keeps the game, see LoadUser).
                        if (!IsBroken(entry.FileFullPath))
                        {
                            changes.orphaned.Add(entry);
                        }
                    }
                }
            }

            foreach (HandlerInfo info in changes.Added.Concat(changes.Updated).Concat(changes.Fixed))
            {
                LogManager.Log($"Handler {(changes.Added.Contains(info) ? "added" : changes.Fixed.Contains(info) ? "fixed" : "updated")}: {info.FileName} ({info.GUID})");
            }

            foreach (HandlerInfo info in changes.Invalid)
            {
                LogManager.Log($"Handler {info.FileName} has errors: {info.Error}");
            }

            foreach (HandlerInfo info in changes.Removed)
            {
                LogManager.Log($"Handler removed: {info.FileName}");
            }

            return changes;
        }

    }
}
