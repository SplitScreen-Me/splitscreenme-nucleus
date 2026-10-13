using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Nucleus.Gaming.Localization
{
    /// <summary>Arguments of <see cref="Translator.LanguageChanged"/>.</summary>
    public sealed class LanguageChangedEventArgs : EventArgs
    {
        public LanguageChangedEventArgs(string previous, string locale)
        {
            Previous = previous;
            Locale = locale;
        }

        /// <summary>Language before the change.</summary>
        public string Previous { get; }

        /// <summary>Language now in use.</summary>
        public string Locale { get; }
    }

    /// <summary>
    /// The strings of the app in one language, with English as the per-key fallback. Thread-safe: reads use an
    /// immutable snapshot, <see cref="SetLanguage"/> swaps it. Most code uses the <see cref="I18n"/> facade; tests and
    /// tools create their own instances.
    /// </summary>
    public sealed class Translator
    {
        /// <summary>English: the source language, complete by definition.</summary>
        public const string DefaultLocale = "en";

        private readonly string folder;
        private readonly Action<string> log;
        private readonly Func<string> builtInEnglish;
        private readonly object gate = new object();
        private readonly ConcurrentDictionary<string, byte> reported = new ConcurrentDictionary<string, byte>(StringComparer.Ordinal);
        private Dictionary<string, LanguageFile> files = new Dictionary<string, LanguageFile>(StringComparer.OrdinalIgnoreCase);
        private IReadOnlyDictionary<string, string> englishStrings;
        private LanguageFile builtInFile;
        private bool builtInRead;
        private string scanned;
        private volatile State state;

        /// <param name="folder">Folder of the language files (<c>i18n</c> next to NucleusCoop.exe).</param>
        /// <param name="locale">Language to use; English when it is not available.</param>
        /// <param name="log">Receives problems (unreadable files, missing keys); each missing key is reported once.</param>
        /// <param name="builtInEnglish">
        /// English file to use when <c>en.json</c> is missing or broken, and for the keys <c>en.json</c> lacks (an older or
        /// edited copy).
        /// </param>
        public Translator(string folder, string locale = DefaultLocale, Action<string> log = null, Func<string> builtInEnglish = null)
        {
            this.folder = folder ?? "";
            this.log = log ?? (_ => { });
            this.builtInEnglish = builtInEnglish;
            lock (gate)
            {
                Rescan();
                state = Build(DefaultLocale);
                if (!IsEnglish(locale))
                {
                    Apply(locale);
                }
            }
        }

        /// <summary>Raised after the language changed, on the thread that changed it.</summary>
        public event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        /// <summary>Folder of the language files.</summary>
        public string Folder => folder;

        /// <summary>Language in use, spelled as its file name (<c>pt-BR</c>).</summary>
        public string Locale => state.Info.Locale;

        /// <summary>Metadata of the language in use.</summary>
        public LanguageInfo Language => state.Info;

        /// <summary>Culture of the language in use, for numbers and dates (invariant when .NET doesn't know it).</summary>
        public CultureInfo Culture => state.Culture;

        /// <summary>
        /// Languages available: every readable file of the folder (read again when files were added, removed or edited),
        /// English always included, English first, then by English name.
        /// </summary>
        public IReadOnlyList<LanguageInfo> Available
        {
            get
            {
                lock (gate)
                {
                    RescanIfChanged();
                    return files.Values.Select(f => f.Info).OrderBy(i => IsEnglish(i.Locale) ? 0 : 1).ThenBy(i => i.EnglishName, StringComparer.OrdinalIgnoreCase).ToList();
                }
            }
        }

        /// <summary>Share of the English strings (plural forms counted once) that <paramref name="locale"/> translates, 0 to 1.</summary>
        public double Completion(string locale)
        {
            lock (gate)
            {
                if (!files.TryGetValue(locale ?? "", out LanguageFile file))
                {
                    return 0;
                }

                HashSet<string> english = LanguageFile.BaseKeys(state.English.Keys);
                if (IsEnglish(file.Info.Locale) || english.Count == 0)
                {
                    return 1;
                }

                HashSet<string> translated = LanguageFile.BaseKeys(file.Strings.Keys);
                return english.Count(translated.Contains) / (double)english.Count;
            }
        }

        /// <summary>
        /// Switches to <paramref name="locale"/> (matched without case) after reading the folder again, so edited files
        /// are picked up. Returns false, and keeps the current language, when the language is not available (no file,
        /// or a broken one). Raises <see cref="LanguageChanged"/> when the language in use changes.
        /// </summary>
        public bool SetLanguage(string locale)
        {
            string previous, now;
            bool ok;
            lock (gate)
            {
                previous = Locale;
                RescanIfChanged();
                ok = Apply(locale);
                now = Locale;
            }

            if (!string.Equals(previous, now, StringComparison.Ordinal))
            {
                reported.Clear();
                LanguageChanged?.Invoke(this, new LanguageChangedEventArgs(previous, now));
            }

            return ok;
        }

        /// <summary>The available locale <paramref name="locale"/> stands for (case-insensitive), or null.</summary>
        public string Resolve(string locale)
        {
            lock (gate)
            {
                RescanIfChanged();
                return locale != null && files.TryGetValue(locale.Trim(), out LanguageFile file) ? file.Info.Locale : null;
            }
        }

        /// <summary>
        /// The strings a language file defines, by dotted key (plural forms as <c>key_one</c>...), without the English
        /// fallback; empty for an unknown language.
        /// </summary>
        public IReadOnlyDictionary<string, string> StringsOf(string locale)
        {
            State current = state;
            if (string.Equals(locale, current.Info.Locale, StringComparison.OrdinalIgnoreCase))
            {
                return current.Strings;
            }

            lock (gate)
            {
                if (IsEnglish(locale))
                {
                    return englishStrings;
                }

                return files.TryGetValue(locale ?? "", out LanguageFile file) ? file.Strings : new ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
            }
        }

        /// <summary>Whether <paramref name="key"/> exists in the language in use or in English (plural forms included).</summary>
        public bool Has(string key)
        {
            State current = state;
            return Candidates(key, null, current.Info.Locale).Any(k => current.Strings.ContainsKey(k) || current.English.ContainsKey(k));
        }

        /// <summary>
        /// The string <paramref name="key"/> in the language in use, else in English, with its placeholders replaced
        /// by <paramref name="args"/> (an anonymous object or a dictionary). A numeric <c>count</c> argument selects the
        /// plural form: <c>key_zero</c> for 0 when it exists, then <c>key_&lt;category&gt;</c>, <c>key_other</c>, <c>key</c>.
        /// A key missing in English too returns the key itself, logged once.
        /// </summary>
        public string T(string key, object args = null) => Translate(key, args, plain: false);

        /// <summary>
        /// Like <see cref="T"/>, without the tags meant for the web UI (<c>&lt;br/&gt;</c> becomes a line break): for
        /// native windows and plain text. The tags are removed from the string before the placeholders are replaced, so
        /// values keep their text (<c>List&lt;String&gt;</c> in an error message stays as it is).
        /// </summary>
        public string TPlain(string key, object args = null) => Translate(key, args, plain: true);

        private string Translate(string key, object args, bool plain)
        {
            if (string.IsNullOrEmpty(key))
            {
                return "";
            }

            State current = state;
            IReadOnlyDictionary<string, object> arguments = MessageFormat.Arguments(args);
            string template = Find(current.Strings, Candidates(key, arguments, current.Info.Locale));
            if (template == null && !ReferenceEquals(current.Strings, current.English))
            {
                template = Find(current.English, Candidates(key, arguments, DefaultLocale));
            }

            if (template == null)
            {
                if (reported.TryAdd(key, 0))
                {
                    log($"missing string \"{key}\" (not in {current.Info.Locale} nor in English)");
                }

                return key;
            }

            return MessageFormat.Format(plain ? MessageFormat.StripTags(template) : template, arguments, current.Culture);
        }

        private static string Find(IReadOnlyDictionary<string, string> strings, IEnumerable<string> keys)
        {
            foreach (string key in keys)
            {
                if (strings.TryGetValue(key, out string value))
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>Keys to try for <paramref name="key"/>, most specific first.</summary>
        internal static IEnumerable<string> Candidates(string key, IReadOnlyDictionary<string, object> args, string locale)
        {
            if (args != null && args.TryGetValue("count", out object count) && PluralRules.IsNumber(count))
            {
                if (IsZero(count))
                {
                    yield return key + "_zero";
                }

                yield return key + "_" + PluralRules.Select(locale, count);
                yield return key + "_other";
                yield return key;
            }
            else
            {
                yield return key;
                yield return key + "_other";
            }
        }

        private static bool IsZero(object number)
        {
            switch (number)
            {
                case double d: return d == 0;
                case float f: return f == 0;
                default: return Convert.ToDecimal(number, CultureInfo.InvariantCulture) == 0;
            }
        }

        // Reads the folder again when its files changed: names, sizes, dates (under the gate).
        private void RescanIfChanged()
        {
            string signature = FolderSignature();
            if (signature != scanned)
            {
                Rescan();
            }
        }

        private string FolderSignature()
        {
            try
            {
                return Directory.Exists(folder)
                    ? string.Join("|", new DirectoryInfo(folder).GetFiles("*.json").OrderBy(f => f.Name, StringComparer.OrdinalIgnoreCase).Select(f => $"{f.Name}:{f.Length}:{f.LastWriteTimeUtc.Ticks}"))
                    : "";
            }
            catch (Exception)
            {
                return scanned;
            }
        }

        // Reads every language file of the folder (under the gate).
        private void Rescan()
        {
            scanned = FolderSignature();
            var found = new Dictionary<string, LanguageFile>(StringComparer.OrdinalIgnoreCase);
            if (Directory.Exists(folder))
            {
                foreach (string path in SafeFiles(folder))
                {
                    try
                    {
                        LanguageFile file = LanguageFile.Load(path);
                        foreach (string problem in file.Problems)
                        {
                            log(Path.GetFileName(path) + ": " + problem);
                        }

                        found[file.Info.Locale] = file;
                    }
                    catch (LanguageFileException e)
                    {
                        log(e.Message);
                    }
                }
            }

            if (!found.TryGetValue(DefaultLocale, out LanguageFile english))
            {
                english = BuiltInEnglishFile();
                if (english == null)
                {
                    log($"no English strings in {folder}: keys are shown instead of texts");
                    english = LanguageFile.Parse("{\"_meta\":{\"locale\":\"en\",\"englishName\":\"English\",\"nativeName\":\"English\"}}", DefaultLocale);
                }

                found[DefaultLocale] = english;
            }

            files = found;
            englishStrings = CompleteEnglish(english);
        }

        // The English of the folder, completed by the built-in English for the keys it lacks: an en.json kept from an
        // older version, or edited by hand, must not show keys where the app has a text.
        private IReadOnlyDictionary<string, string> CompleteEnglish(LanguageFile english)
        {
            LanguageFile builtIn = BuiltInEnglishFile();
            if (builtIn == null || ReferenceEquals(builtIn, english))
            {
                return english.Strings;
            }

            HashSet<string> defined = LanguageFile.BaseKeys(english.Strings.Keys);
            var complete = english.Strings.ToDictionary(p => p.Key, p => p.Value, StringComparer.Ordinal);
            HashSet<string> added = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, string> pair in builtIn.Strings)
            {
                string baseKey = PluralRules.BaseKey(pair.Key);
                if (!defined.Contains(baseKey))
                {
                    complete[pair.Key] = pair.Value;
                    added.Add(baseKey);
                }
            }

            if (added.Count == 0)
            {
                return english.Strings;
            }

            log($"{Path.GetFileName(english.Info.Path ?? "en.json")} lacks {added.Count} strings of this version: using the built-in English for them (first: {added.First()})");
            return new ReadOnlyDictionary<string, string>(complete);
        }

        private IEnumerable<string> SafeFiles(string directory)
        {
            try
            {
                return Directory.GetFiles(directory, "*.json");
            }
            catch (Exception e)
            {
                log($"{directory} can't be read: {e.Message}");
                return new string[0];
            }
        }

        // The built-in English, read once (under the gate); null when there is none or it can't be read.
        private LanguageFile BuiltInEnglishFile()
        {
            if (!builtInRead)
            {
                builtInRead = true;
                builtInFile = ReadBuiltInEnglish();
            }

            return builtInFile;
        }

        private LanguageFile ReadBuiltInEnglish()
        {
            if (builtInEnglish == null)
            {
                return null;
            }

            string json = null;
            try
            {
                json = builtInEnglish();
            }
            catch (Exception e)
            {
                log("the built-in English strings can't be read: " + e.Message);
            }

            if (json != null)
            {
                try
                {
                    return LanguageFile.Parse(json, DefaultLocale);
                }
                catch (LanguageFileException e)
                {
                    log(e.Message);
                }
            }

            return null;
        }

        // Switches the snapshot to an available language (under the gate).
        private bool Apply(string locale)
        {
            if (string.IsNullOrWhiteSpace(locale) || !files.ContainsKey(locale.Trim()))
            {
                log($"language \"{locale}\" is not available: keeping {state.Info.Locale}");
                return false;
            }

            state = Build(locale.Trim());
            return true;
        }

        private State Build(string locale)
        {
            LanguageFile file = files[locale];
            return new State
            {
                Info = file.Info,
                Culture = CultureFor(file.Info.Locale),
                English = englishStrings,
                Strings = IsEnglish(file.Info.Locale) ? englishStrings : file.Strings,
            };
        }

        private static CultureInfo CultureFor(string locale)
        {
            for (string tag = locale; !string.IsNullOrEmpty(tag); tag = LanguageDetector.Parent(tag))
            {
                try
                {
                    return CultureInfo.GetCultureInfo(tag);
                }
                catch (CultureNotFoundException)
                {
                }
            }

            return CultureInfo.InvariantCulture;
        }

        private static bool IsEnglish(string locale) => string.Equals(locale, DefaultLocale, StringComparison.OrdinalIgnoreCase);

        private sealed class State
        {
            public LanguageInfo Info;
            public CultureInfo Culture;
            public IReadOnlyDictionary<string, string> Strings;
            public IReadOnlyDictionary<string, string> English;
        }
    }
}
