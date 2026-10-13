using System;
using System.Globalization;
using System.IO;

namespace Nucleus.Gaming.Localization
{
    /// <summary>
    /// The app's strings, in the language the user chose (docs/webui/i18n.md). Every process uses the same files:
    /// <c>i18n\&lt;locale&gt;.json</c> next to NucleusCoop.exe, English as the per-key fallback, and a copy of English built
    /// into Nucleus.Gaming when <c>en.json</c> is missing or lacks a key.
    /// <code>
    /// using static Nucleus.Gaming.Localization.I18n;
    /// string title = T("engine.startChecks.missingFile.title");
    /// string text = T("engine.startChecks.missingFile.message", new { file });
    /// string count = T("library.gameCount", new { count = games.Count });   // gameCount_one / gameCount_other
    /// </code>
    /// The process cultures (<c>CultureInfo.CurrentCulture</c>) are never changed: handlers parse numbers with them.
    /// Format numbers and dates for the user with <see cref="Culture"/>, or pass them as arguments.
    /// </summary>
    public static class I18n
    {
        /// <summary>Folder of the language files, next to NucleusCoop.exe.</summary>
        public const string FolderName = "i18n";

        private const string BuiltInEnglishResource = "Nucleus.Gaming.Localization.en.json";
        private static readonly object gate = new object();
        private static Translator current;

        /// <summary><c>&lt;install root&gt;\i18n</c>.</summary>
        public static string DefaultFolder => Path.Combine(InstallPaths.Root, FolderName);

        /// <summary>
        /// The translator in use. Created on first use from <see cref="DefaultFolder"/> and the saved language (or the
        /// Windows one) when <see cref="Initialize"/> was not called.
        /// </summary>
        public static Translator Current
        {
            get
            {
                Translator translator = current;
                if (translator != null)
                {
                    return translator;
                }

                lock (gate)
                {
                    if (current == null)
                    {
                        Use(Create(DefaultFolder));
                    }

                    return current;
                }
            }
        }

        /// <summary>Raised after the language changed (on the thread that changed it).</summary>
        public static event EventHandler<LanguageChangedEventArgs> LanguageChanged;

        /// <summary>Language in use (<c>en</c>, <c>fr</c>, <c>pt-BR</c>).</summary>
        public static string Locale => Current.Locale;

        /// <summary>Culture of the language in use, to format numbers and dates shown to the user.</summary>
        public static CultureInfo Culture => Current.Culture;

        /// <summary>
        /// The string <paramref name="key"/> in the language in use (English when it lacks it), placeholders replaced by
        /// <paramref name="args"/> (anonymous object or dictionary); a numeric <c>count</c> selects the plural form.
        /// Unknown keys return the key and are logged once.
        /// </summary>
        public static string T(string key, object args = null) => Current.T(key, args);

        /// <summary>
        /// Like <see cref="T"/>, without the tags meant for the web UI: for native windows and plain text. Only the tags of
        /// the string are removed; the values are inserted as they are.
        /// </summary>
        public static string TPlain(string key, object args = null) => Current.TPlain(key, args);

        /// <summary>Whether a key exists (in the language in use or in English).</summary>
        public static bool Has(string key) => Current.Has(key);

        /// <summary>
        /// Loads the languages of <paramref name="folder"/> (default <see cref="DefaultFolder"/>) and switches to the saved
        /// language, else the one matching Windows. Call it once, at startup, before anything shows text.
        /// </summary>
        public static Translator Initialize(string folder = null)
        {
            lock (gate)
            {
                Use(Create(folder ?? DefaultFolder));
                return current;
            }
        }

        /// <summary>Makes <paramref name="translator"/> the one in use (tests, tools).</summary>
        public static void Use(Translator translator)
        {
            if (translator == null)
            {
                throw new ArgumentNullException(nameof(translator));
            }

            lock (gate)
            {
                if (current != null)
                {
                    current.LanguageChanged -= Forward;
                }

                current = translator;
                current.LanguageChanged += Forward;
            }
        }

        /// <summary>The English file built into Nucleus.Gaming (a copy of Master\i18n\en.json), or null.</summary>
        public static string BuiltInEnglish()
        {
            using (Stream stream = typeof(I18n).Assembly.GetManifestResourceStream(BuiltInEnglishResource))
            {
                if (stream == null)
                {
                    return null;
                }

                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static Translator Create(string folder)
        {
            var translator = new Translator(folder, Translator.DefaultLocale, Log, BuiltInEnglish);
            translator.SetLanguage(LanguageSettings.StartupLocale(translator));
            return translator;
        }

        private static void Forward(object sender, LanguageChangedEventArgs e)
        {
            Log($"language changed from {e.Previous} to {e.Locale}");
            LanguageChanged?.Invoke(sender, e);
        }

        private static void Log(string message)
        {
            try
            {
                LogManager.Log("[i18n] " + message);
            }
            catch
            {
                // Logging must never break a string lookup.
            }
        }
    }
}
