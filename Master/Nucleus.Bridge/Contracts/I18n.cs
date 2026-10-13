using System.Collections.Generic;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// Languages of the app (docs/webui/i18n.md). The language files are <c>i18n\&lt;locale&gt;.json</c> next to
    /// NucleusCoop.exe; the UI carries the English strings itself and falls back to them per key. Texts the host
    /// builds (settings labels, notices, setup hints) come in the language in use: after
    /// <see cref="LanguageChanged"/>, the UI fetches them again.
    /// </summary>
    [BridgeService("i18n")]
    public interface II18nService
    {
        /// <summary>The available languages, the one in use, the one matching Windows, and whether the user has chosen one.</summary>
        LanguageState GetState();

        /// <summary>
        /// The strings a language file defines, by dotted key (<c>common.ok</c>, plural forms as <c>key_one</c>...), without
        /// the English fallback. <c>notFound</c> for a language that is not available.
        /// </summary>
        LanguageStrings GetStrings(GetLanguageStringsRequest request);

        /// <summary>
        /// Saves the user's choice in Settings.ini and switches to it; emits <see cref="LanguageChanged"/> when the
        /// language in use changes. <c>invalidParams</c> for a language that is not available.
        /// </summary>
        void SetLanguage(SetLanguageRequest request);
    }

    /// <summary>A language the app can use: a file of the i18n folder.</summary>
    public sealed class LanguageSummary
    {
        /// <summary>BCP 47 tag, the file name: <c>en</c>, <c>fr</c>, <c>pt-BR</c>.</summary>
        public string Locale { get; set; } = "";

        /// <summary>Name in English, for example "French".</summary>
        public string EnglishName { get; set; } = "";

        /// <summary>Name in the language itself, for example "Français".</summary>
        public string NativeName { get; set; } = "";

        /// <summary>Written right to left.</summary>
        public bool RightToLeft { get; set; }

        /// <summary>Share of the English strings it translates, 0 to 100 (100 for English).</summary>
        public int Completion { get; set; }
    }

    /// <summary>Answer to <see cref="II18nService.GetState"/>.</summary>
    public sealed class LanguageState
    {
        /// <summary>Language in use. Until the user chooses, the one matching Windows.</summary>
        public string Current { get; set; } = "";

        /// <summary>The available language that matches the Windows display language (English when none does).</summary>
        public string Detected { get; set; } = "";

        /// <summary>Whether the user has chosen a language. False on the first launch: the UI asks.</summary>
        public bool Chosen { get; set; }

        /// <summary>Available languages, English first, then by English name.</summary>
        public List<LanguageSummary> Languages { get; set; } = new List<LanguageSummary>();
    }

    /// <summary>Parameters of <see cref="II18nService.GetStrings"/>.</summary>
    public sealed class GetLanguageStringsRequest
    {
        /// <summary>Language, as listed in <see cref="LanguageState.Languages"/>.</summary>
        public string Locale { get; set; } = "";
    }

    /// <summary>Answer to <see cref="II18nService.GetStrings"/>.</summary>
    public sealed class LanguageStrings
    {
        /// <summary>Language of the strings.</summary>
        public string Locale { get; set; } = "";

        /// <summary>Strings by dotted key; keys the file lacks are absent (the UI uses English for them).</summary>
        public Dictionary<string, string> Strings { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>Parameters of <see cref="II18nService.SetLanguage"/>.</summary>
    public sealed class SetLanguageRequest
    {
        /// <summary>Language to use, as listed in <see cref="LanguageState.Languages"/> (case doesn't matter).</summary>
        public string Locale { get; set; } = "";
    }

    /// <summary>
    /// The language in use changed (Settings, first-launch picker). The UI loads the new strings and refetches every
    /// bridge query, since host texts are built in the language in use.
    /// </summary>
    [BridgeEvent("i18n.languageChanged", Invalidates = new[] { "i18n.getState", "i18n.getStrings" })]
    public sealed class LanguageChanged
    {
        /// <summary>Language now in use.</summary>
        public string Locale { get; set; } = "";
    }
}
