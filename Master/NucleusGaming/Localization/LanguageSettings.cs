using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nucleus.Gaming.Localization
{
    /// <summary>Where the chosen language is kept.</summary>
    public interface ILanguageStore
    {
        /// <summary>The saved locale, or an empty string when the user has not chosen yet.</summary>
        string Load();

        /// <summary>Saves the chosen locale.</summary>
        void Save(string locale);
    }

    /// <summary>Settings.ini <c>[Misc] AppLanguage</c>. Empty (the shipped default) means "not chosen yet".</summary>
    public sealed class IniLanguageStore : ILanguageStore
    {
        /// <summary>Settings.ini section.</summary>
        public const string Section = "Misc";

        /// <summary>
        /// Settings.ini key. Not to be confused with <c>[Misc] SteamLang</c> / <c>EpicLang</c>, the languages the
        /// emulators report to games.
        /// </summary>
        public const string Key = "AppLanguage";

        private readonly IniFile ini;

        public IniLanguageStore(IniFile ini = null)
        {
            this.ini = ini;
        }

        private IniFile File => ini ?? Globals.ini;

        public string Load() => File.IniReadValue(Section, Key)?.Trim() ?? "";

        public void Save(string locale) => File.IniWriteValue(Section, Key, locale ?? "");
    }

    /// <summary>
    /// The language the user chose: saved in Settings.ini, asked once at first launch (the web UI shows a picker while
    /// <see cref="IsChosen"/> is false), changed in Settings &gt; General. Until it is chosen, the app uses the language
    /// that matches Windows (<see cref="Detected"/>).
    /// </summary>
    public static class LanguageSettings
    {
        /// <summary>Storage of the choice: Settings.ini. The methods take another store for tests.</summary>
        public static ILanguageStore Store { get; set; } = new IniLanguageStore();

        /// <summary>The saved locale, or null when none is saved.</summary>
        public static string Saved => SavedIn(Store);

        /// <summary>The locale saved in <paramref name="store"/> (default <see cref="Store"/>), or null.</summary>
        public static string SavedIn(ILanguageStore store)
        {
            try
            {
                string value = (store ?? Store).Load();
                return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
            }
            catch
            {
                return null;
            }
        }

        /// <summary>Whether the user chose a language that is still available.</summary>
        public static bool IsChosen(Translator translator, ILanguageStore store = null) =>
            SavedIn(store) is string saved && translator.Resolve(saved) != null;

        /// <summary>The available language that matches the Windows display language (English when none does).</summary>
        public static string Detected(Translator translator) =>
            WindowsMatch(translator) ?? LanguageDetector.Match(Enumerable.Empty<string>(), Locales(translator));

        /// <summary>
        /// The available language that matches the Windows display language (its tag, a parent, or another region of
        /// the language: see <see cref="LanguageDetector"/>), or null when none does (Italian Windows without it.json).
        /// </summary>
        public static string WindowsMatch(Translator translator) =>
            LanguageDetector.Match(CultureInfo.CurrentUICulture, Locales(translator), fallback: null);

        /// <summary>The saved language when it is available, else <see cref="Detected"/>.</summary>
        public static string StartupLocale(Translator translator, ILanguageStore store = null) =>
            (SavedIn(store) is string saved ? translator.Resolve(saved) : null) ?? Detected(translator);

        /// <summary>
        /// Saves <paramref name="locale"/> as the user's choice and switches to it (raising
        /// <see cref="Translator.LanguageChanged"/>). Returns false, saving nothing, for a language that is not available.
        /// </summary>
        public static bool Choose(string locale, Translator translator = null, ILanguageStore store = null)
        {
            Translator target = translator ?? I18n.Current;
            string resolved = target.Resolve(locale);
            if (resolved == null)
            {
                return false;
            }

            (store ?? Store).Save(resolved);
            return target.SetLanguage(resolved);
        }

        private static IEnumerable<string> Locales(Translator translator) => translator.Available.Select(l => l.Locale);
    }
}
