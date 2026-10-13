using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Localization;

/// <summary>
/// <c>i18n.*</c>: the languages of the i18n folder, the user's choice (Settings.ini <c>[Misc] AppLanguage</c>) and the
/// strings of each language. Any change of language (this service, Settings &gt; General, code) emits
/// <c>i18n.languageChanged</c>.
/// </summary>
public sealed class I18nService : II18nService, IDisposable
{
    private readonly IBridgeEvents events;
    private readonly Translator translator;
    private readonly ILanguageStore? store;

    /// <summary>Creates the service on a translator (default: <see cref="I18n.Current"/>) and a store (default: Settings.ini).</summary>
    public I18nService(IBridgeEvents events, Translator? translator = null, ILanguageStore? store = null)
    {
        this.events = events;
        this.translator = translator ?? I18n.Current;
        this.store = store;
        this.translator.LanguageChanged += OnLanguageChanged;
    }

    /// <inheritdoc />
    public LanguageState GetState()
    {
        string? windows = LanguageSettings.WindowsMatch(translator);
        return new LanguageState
        {
            Current = translator.Locale,
            Detected = windows ?? LanguageSettings.Detected(translator),
            DetectedMatchesWindows = windows != null,
            Chosen = LanguageSettings.IsChosen(translator, store),
            Languages = translator.Available.Select(Summary).ToList(),
        };
    }

    /// <inheritdoc />
    public LanguageStrings GetStrings(GetLanguageStringsRequest request)
    {
        string locale = translator.Resolve(request?.Locale)
            ?? throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.i18n.errors.notAvailable"), new { locale = request?.Locale });
        return new LanguageStrings
        {
            Locale = locale,
            Strings = new Dictionary<string, string>(translator.StringsOf(locale).ToDictionary(p => p.Key, p => p.Value), StringComparer.Ordinal),
        };
    }

    /// <inheritdoc />
    public void SetLanguage(SetLanguageRequest request)
    {
        if (!LanguageSettings.Choose(request?.Locale ?? "", translator, store))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, I18n.T("host.i18n.errors.notAvailable"), new { locale = request?.Locale });
        }
    }

    /// <summary>The available languages as setting choices: "Français (French)", by native name.</summary>
    public static List<SettingChoice> Choices(Translator translator) =>
        translator.Available
            .Select(l => new SettingChoice { Value = l.Locale, Label = Label(l) })
            .OrderBy(c => c.Label, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    /// <summary>"Français (French)", or just "English" when both names are the same.</summary>
    public static string Label(LanguageInfo language) =>
        string.Equals(language.NativeName, language.EnglishName, StringComparison.OrdinalIgnoreCase)
            ? language.NativeName
            : $"{language.NativeName} ({language.EnglishName})";

    /// <inheritdoc />
    public void Dispose() => translator.LanguageChanged -= OnLanguageChanged;

    private LanguageSummary Summary(LanguageInfo language) => new LanguageSummary
    {
        Locale = language.Locale,
        EnglishName = language.EnglishName,
        NativeName = language.NativeName,
        RightToLeft = language.RightToLeft,
        Completion = (int)Math.Floor(translator.Completion(language.Locale) * 100),
    };

    private void OnLanguageChanged(object sender, LanguageChangedEventArgs e) => events.Emit(new LanguageChanged { Locale = e.Locale });
}
