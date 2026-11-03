using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.Coop.Search
{
    /// <summary>
    /// The folders where the user keeps games (Nucleus 2.5 automatic executable search), stored in
    /// <c>%USERPROFILE%\NucleusCoop\userGameDirectories.json</c> as <c>{ "Path": [ ... ] }</c>, the 2.5 file and format.
    /// Thread-safe. Folders that no longer exist are kept in the file but left out of <see cref="Existing"/>.
    /// </summary>
    public sealed class GameSearchDirectories
    {
        private readonly object sync = new object();
        private readonly string filePath;
        private List<string> paths;

        /// <summary>The 2.5 location of the file.</summary>
        public static string DefaultFilePath => Path.Combine(Globals.UserEnvironmentRoot, "NucleusCoop", "userGameDirectories.json");

        public GameSearchDirectories() : this(DefaultFilePath)
        {
        }

        public GameSearchDirectories(string filePath)
        {
            this.filePath = filePath;
        }

        /// <summary>Path of the JSON file.</summary>
        public string FilePath => filePath;

        /// <summary>Every saved folder, sorted (case-insensitive).</summary>
        public IReadOnlyList<string> All
        {
            get
            {
                lock (sync)
                {
                    return Load().ToList();
                }
            }
        }

        /// <summary>Saved folders that exist now.</summary>
        public IReadOnlyList<string> Existing => All.Where(Directory.Exists).ToList();

        /// <summary>Adds a folder (full path; a duplicate, compared case-insensitively, is ignored). Returns false when it was already there.</summary>
        public bool Add(string folder)
        {
            string normalized = Normalize(folder);

            lock (sync)
            {
                List<string> list = Load();

                if (list.Any(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)))
                {
                    return false;
                }

                list.Add(normalized);
                Save(list);
                return true;
            }
        }

        /// <summary>Removes a folder. Returns false when it wasn't saved.</summary>
        public bool Remove(string folder)
        {
            string normalized = Normalize(folder);

            lock (sync)
            {
                List<string> list = Load();
                int removed = list.RemoveAll(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase));

                if (removed == 0)
                {
                    return false;
                }

                Save(list);
                return true;
            }
        }

        /// <summary>Replaces the whole list.</summary>
        public void SetAll(IEnumerable<string> folders)
        {
            lock (sync)
            {
                List<string> list = new List<string>();

                foreach (string folder in folders ?? Enumerable.Empty<string>())
                {
                    string normalized = Normalize(folder);

                    if (!list.Any(p => string.Equals(p, normalized, StringComparison.OrdinalIgnoreCase)))
                    {
                        list.Add(normalized);
                    }
                }

                Save(list);
            }
        }

        private static string Normalize(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder))
            {
                throw new ArgumentException("Empty folder", nameof(folder));
            }

            string full = Path.GetFullPath(folder.Trim());
            string root = Path.GetPathRoot(full);

            //Keep "C:\" as is, drop the trailing separator of other folders.
            return full.Length > (root?.Length ?? 0) ? full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) : full;
        }

        private List<string> Load()
        {
            if (paths != null)
            {
                return paths;
            }

            paths = new List<string>();

            try
            {
                if (File.Exists(filePath))
                {
                    JObject json = JsonConvert.DeserializeObject(File.ReadAllText(filePath)) as JObject;

                    if (json?["Path"] is JArray array)
                    {
                        foreach (JToken token in array)
                        {
                            string value = token?.ToString();

                            if (!string.IsNullOrWhiteSpace(value) && !paths.Any(p => string.Equals(p, value, StringComparison.OrdinalIgnoreCase)))
                            {
                                paths.Add(value);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                LogManager.Log("Game search folders: could not read " + filePath + ": " + e.Message);
            }

            paths.Sort(StringComparer.OrdinalIgnoreCase);
            return paths;
        }

        private void Save(List<string> list)
        {
            list.Sort(StringComparer.OrdinalIgnoreCase);
            paths = list;

            string folder = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(folder))
            {
                Directory.CreateDirectory(folder);
            }

            JObject json = new JObject(new JProperty("Path", new JArray(list)));
            string temp = filePath + ".tmp";
            File.WriteAllText(temp, JsonConvert.SerializeObject(json, Formatting.Indented));

            if (File.Exists(filePath))
            {
                File.Replace(temp, filePath, null);
            }
            else
            {
                File.Move(temp, filePath);
            }
        }
    }
}
