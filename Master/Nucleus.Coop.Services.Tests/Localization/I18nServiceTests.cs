using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Localization;
using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

public class I18nServiceTests
{
    private sealed class FakeEvents : IBridgeEvents
    {
        public List<object> Emitted { get; } = new List<object>();

        public void Emit<TEvent>(TEvent payload) where TEvent : class => Emitted.Add(payload);
    }

    [Fact]
    public void State_lists_the_languages_and_says_whether_one_was_chosen()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var store = new MemoryLanguageStore();
        var service = new I18nService(new FakeEvents(), folder.Translator(), store);

        LanguageState state = service.GetState();

        Assert.Equal("en", state.Current);
        Assert.False(state.Chosen);
        Assert.Equal(new[] { "en", "fr" }, state.Languages.Select(l => l.Locale));
        LanguageSummary french = state.Languages.Single(l => l.Locale == "fr");
        Assert.Equal("Français", french.NativeName);
        Assert.Equal(50, french.Completion);
        Assert.Equal(100, state.Languages.Single(l => l.Locale == "en").Completion);
    }

    [Theory]
    [InlineData("fr-FR", "fr", true)]
    [InlineData("fr-CA", "fr", true)]
    [InlineData("en-GB", "en", true)]
    [InlineData("it-IT", "en", false)] // no Italian file: English, but not as the Windows language
    public void State_says_whether_the_detected_language_matches_Windows(string windows, string detected, bool matches)
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var service = new I18nService(new FakeEvents(), folder.Translator(), new MemoryLanguageStore());

        LanguageState state = WithWindowsLanguage(windows, service.GetState);

        Assert.Equal(detected, state.Detected);
        Assert.Equal(matches, state.DetectedMatchesWindows);
        Assert.Equal(detected, WithWindowsLanguage(windows, () => LanguageSettings.Detected(folder.Translator())));
    }

    [Fact]
    public void Setting_the_language_saves_it_and_emits_languageChanged()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var store = new MemoryLanguageStore();
        var events = new FakeEvents();
        Translator translator = folder.Translator();
        var service = new I18nService(events, translator, store);

        service.SetLanguage(new SetLanguageRequest { Locale = "FR" });

        Assert.Equal("fr", store.Value);
        Assert.Equal("fr", translator.Locale);
        Assert.True(service.GetState().Chosen);
        Assert.Equal("fr", Assert.IsType<LanguageChanged>(Assert.Single(events.Emitted)).Locale);
    }

    [Fact]
    public void Choosing_the_language_in_use_saves_it_without_an_event()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var store = new MemoryLanguageStore();
        var events = new FakeEvents();
        var service = new I18nService(events, folder.Translator(), store);

        service.SetLanguage(new SetLanguageRequest { Locale = "en" });

        Assert.Equal("en", store.Value);
        Assert.True(service.GetState().Chosen);
        Assert.Empty(events.Emitted);
    }

    [Fact]
    public void Unknown_languages_are_refused()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var store = new MemoryLanguageStore();
        var service = new I18nService(new FakeEvents(), folder.Translator(), store);

        BridgeException set = Assert.Throws<BridgeException>(() => service.SetLanguage(new SetLanguageRequest { Locale = "xx" }));
        BridgeException get = Assert.Throws<BridgeException>(() => service.GetStrings(new GetLanguageStringsRequest { Locale = "xx" }));

        Assert.Equal(BridgeErrorKind.InvalidParams, set.Kind);
        Assert.Equal(BridgeErrorKind.NotFound, get.Kind);
        Assert.Equal("", store.Value);
    }

    [Fact]
    public void Strings_are_the_file_s_own_without_English()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var service = new I18nService(new FakeEvents(), folder.Translator(), new MemoryLanguageStore());

        LanguageStrings french = service.GetStrings(new GetLanguageStringsRequest { Locale = "fr" });

        Assert.Equal("fr", french.Locale);
        Assert.Equal("D'accord", french.Strings["common.ok"]);
        Assert.Equal("{count} jeux", french.Strings["library.gameCount_other"]);
        Assert.False(french.Strings.ContainsKey("common.cancel"));
    }

    [Fact]
    public void A_saved_language_that_is_gone_counts_as_not_chosen()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        var store = new MemoryLanguageStore { Value = "it" };
        Translator translator = folder.Translator();

        Assert.False(LanguageSettings.IsChosen(translator, store));
        Assert.Equal(LanguageSettings.Detected(translator), LanguageSettings.StartupLocale(translator, store));
    }

    // The Windows display language, as the services see it: the UI culture of the calling thread.
    private static T WithWindowsLanguage<T>(string culture, Func<T> action)
    {
        CultureInfo saved = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(culture);
        try
        {
            return action();
        }
        finally
        {
            CultureInfo.CurrentUICulture = saved;
        }
    }

    [Fact]
    public void Setting_choices_show_native_and_English_names()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();

        List<SettingChoice> choices = I18nService.Choices(folder.Translator());

        Assert.Equal(new[] { "English", "Français (French)" }, choices.Select(c => c.Label));
        Assert.Equal(new[] { "en", "fr" }, choices.Select(c => c.Value));
    }
}
