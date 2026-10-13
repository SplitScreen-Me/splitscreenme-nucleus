using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.Localization
{
    /// <summary>Metadata of a language file: its <c>_meta</c> block.</summary>
    public sealed class LanguageInfo
    {
        public LanguageInfo(string locale, string englishName, string nativeName, bool rightToLeft, int keyCount, string path)
        {
            Locale = locale;
            EnglishName = string.IsNullOrWhiteSpace(englishName) ? locale : englishName;
            NativeName = string.IsNullOrWhiteSpace(nativeName) ? EnglishName : nativeName;
            RightToLeft = rightToLeft;
            KeyCount = keyCount;
            Path = path;
        }

        /// <summary>BCP 47 tag, the file name without <c>.json</c>: <c>en</c>, <c>fr</c>, <c>pt-BR</c>.</summary>
        public string Locale { get; }

        /// <summary>Name in English, for example "French".</summary>
        public string EnglishName { get; }

        /// <summary>Name in the language itself, for example "Français".</summary>
        public string NativeName { get; }

        /// <summary>Written right to left (<c>"direction": "rtl"</c>).</summary>
        public bool RightToLeft { get; }

        /// <summary>Number of strings in the file.</summary>
        public int KeyCount { get; }

        /// <summary>File it was read from; null for the English copy built into Nucleus.Gaming.</summary>
        public string Path { get; }

        public override string ToString() => $"{Locale} ({EnglishName})";
    }

    /// <summary>A language file was unreadable or malformed.</summary>
    public sealed class LanguageFileException : Exception
    {
        public LanguageFileException(string message, Exception inner = null) : base(message, inner) { }
    }

    /// <summary>
    /// A parsed language file (<c>i18n\&lt;locale&gt;.json</c>): JSON objects nested by namespace, strings as leaves, flattened
    /// to dotted keys (<c>{"common": {"ok": "OK"}}</c> gives <c>common.ok</c>). Properties whose name starts with
    /// <c>_</c> are not strings to translate: <c>_meta</c> (locale, englishName, nativeName, direction) and notes.
    /// </summary>
    public sealed class LanguageFile
    {
        private LanguageFile(LanguageInfo info, IReadOnlyDictionary<string, string> strings, IReadOnlyList<string> problems)
        {
            Info = info;
            Strings = strings;
            Problems = problems;
        }

        public LanguageInfo Info { get; }

        /// <summary>Strings by dotted key, plural forms included (<c>library.gameCount_one</c>).</summary>
        public IReadOnlyDictionary<string, string> Strings { get; }

        /// <summary>What was skipped while reading (values that are not strings, a locale that differs from the file name).</summary>
        public IReadOnlyList<string> Problems { get; }

        /// <summary>Reads <paramref name="path"/>; its locale is the file name without the extension.</summary>
        /// <exception cref="LanguageFileException">The file can't be read or is not a JSON object.</exception>
        public static LanguageFile Load(string path)
        {
            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (Exception e)
            {
                throw new LanguageFileException($"{path} can't be read: {e.Message}", e);
            }

            return Parse(json, System.IO.Path.GetFileNameWithoutExtension(path), path);
        }

        /// <summary>Parses a language file. <paramref name="locale"/> wins over the locale written in <c>_meta</c>.</summary>
        /// <exception cref="LanguageFileException">Not a JSON object.</exception>
        public static LanguageFile Parse(string json, string locale, string path = null)
        {
            string source = path ?? locale + ".json";
            JObject root;
            try
            {
                using (var reader = new JsonTextReader(new StringReader(json ?? "")) { DateParseHandling = DateParseHandling.None })
                {
                    root = JToken.ReadFrom(reader) as JObject;
                }
            }
            catch (JsonException e)
            {
                throw new LanguageFileException($"{source} is not valid JSON: {e.Message}", e);
            }

            if (root == null)
            {
                throw new LanguageFileException($"{source} must contain a JSON object.");
            }

            var problems = new List<string>();
            var strings = new Dictionary<string, string>(StringComparer.Ordinal);
            Flatten(root, "", strings, problems);

            var meta = root["_meta"] as JObject;
            string metaLocale = (string)meta?["locale"];
            if (!string.IsNullOrEmpty(metaLocale) && !string.Equals(metaLocale, locale, StringComparison.OrdinalIgnoreCase))
            {
                problems.Add($"_meta.locale is \"{metaLocale}\" but the file is named {locale}.json: the file name wins.");
            }

            var info = new LanguageInfo(
                locale,
                (string)meta?["englishName"],
                (string)meta?["nativeName"],
                string.Equals((string)meta?["direction"], "rtl", StringComparison.OrdinalIgnoreCase),
                strings.Count,
                path);
            return new LanguageFile(info, new ReadOnlyDictionary<string, string>(strings), problems);
        }

        private static void Flatten(JObject node, string prefix, Dictionary<string, string> into, List<string> problems)
        {
            foreach (JProperty property in node.Properties())
            {
                if (property.Name.StartsWith("_", StringComparison.Ordinal))
                {
                    continue;
                }

                string key = prefix + property.Name;
                switch (property.Value.Type)
                {
                    case JTokenType.Object:
                        Flatten((JObject)property.Value, key + ".", into, problems);
                        break;
                    case JTokenType.String:
                        into[key] = (string)property.Value;
                        break;
                    default:
                        problems.Add($"{key} is a {property.Value.Type.ToString().ToLowerInvariant()}, not a string: skipped.");
                        break;
                }
            }
        }

        /// <summary>Base keys (plural suffixes removed) of a set of keys.</summary>
        internal static HashSet<string> BaseKeys(IEnumerable<string> keys) =>
            new HashSet<string>(keys.Select(PluralRules.BaseKey), StringComparer.Ordinal);
    }
}
