using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

public class TranslatorTests
{
    [Fact]
    public void Looks_up_nested_keys_in_the_language_in_use()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator("fr");

        Assert.Equal("fr", t.Locale);
        Assert.Equal("D'accord", t.T("common.ok"));
        Assert.Equal("Bonjour Léa", t.T("library.hello", new { name = "Léa" }));
    }

    [Fact]
    public void Falls_back_to_English_per_key()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator("fr");

        Assert.Equal("Cancel", t.T("common.cancel"));
        Assert.Equal("Library", t.T("library.title"));
        Assert.Empty(folder.Log.Where(l => l.Contains("missing")));
    }

    [Fact]
    public void Returns_the_key_of_a_string_missing_in_English_and_logs_it_once()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator("fr");

        Assert.Equal("library.nope", t.T("library.nope"));
        Assert.Equal("library.nope", t.T("library.nope"));
        Assert.Single(folder.Log, l => l.Contains("library.nope"));
    }

    [Fact]
    public void Picks_plural_forms_by_count()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator en = folder.Translator("en");
        Translator fr = folder.Translator("fr");

        Assert.Equal("No games", en.T("library.gameCount", new { count = 0 }));
        Assert.Equal("1 game", en.T("library.gameCount", new { count = 1 }));
        Assert.Equal("3 games", en.T("library.gameCount", new { count = 3 }));
        Assert.Equal("1.5 games", en.T("library.gameCount", new { count = 1.5 }));

        // French has no _zero form: 0 takes "one" in French, like 1.
        Assert.Equal("0 jeu", fr.T("library.gameCount", new { count = 0 }));
        Assert.Equal("1 jeu", fr.T("library.gameCount", new { count = 1 }));
        Assert.Equal("2 jeux", fr.T("library.gameCount", new { count = 2 }));
    }

    [Fact]
    public void Formats_numbers_for_the_language_and_keeps_unknown_placeholders()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator en = folder.Translator("en");
        Translator fr = folder.Translator("fr");

        Assert.Equal("1,234 games", en.T("library.gameCount", new { count = 1234 }));
        Assert.Equal(1234.ToString("#,0", CultureInfo.GetCultureInfo("fr")) + " jeux", fr.T("library.gameCount", new { count = 1234 }));
        Assert.Equal("Hello {name}", en.T("library.hello"));
        Assert.Equal("Hello 2026", en.T("library.hello", new Dictionary<string, object> { ["name"] = "2026" }));
    }

    [Fact]
    public void Leaves_tags_to_the_UI_and_strips_them_for_native_windows()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();

        Assert.Equal("Press <kbd>End</kbd> to lock", t.T("library.lockHint", new { key = "End" }));
        Assert.Equal("Press End to lock", MessageFormat.StripTags(t.T("library.lockHint", new { key = "End" })));
        Assert.Equal("a\nb", MessageFormat.StripTags("a<br/>b"));
        Assert.Equal("Press End to lock", t.TPlain("library.lockHint", new { key = "End" }));
    }

    [Fact]
    public void Plain_text_keeps_tag_like_values()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();

        Assert.Equal("Press List<String> to lock", t.TPlain("library.lockHint", new { key = "List<String>" }));
        Assert.Equal("Press <b>x</b> to lock", t.TPlain("library.lockHint", new { key = "<b>x</b>" }));
        Assert.Equal("missing.key", t.TPlain("missing.key"));
    }

    [Fact]
    public void Completes_an_older_en_json_with_the_built_in_English()
    {
        const string olderEnglish = @"{
  ""_meta"": { ""locale"": ""en"", ""englishName"": ""English"", ""nativeName"": ""English"" },
  ""common"": { ""ok"": ""Okay"" },
  ""library"": { ""gameCount"": ""{count} game(s)"" }
}";
        using var folder = new LanguageFolder(("en.json", olderEnglish), ("fr.json", LanguageFolder.French));
        Translator t = folder.Translator("en", () => LanguageFolder.English);

        Assert.Equal("Okay", t.T("common.ok"));                                // the folder's English wins
        Assert.Equal("Cancel", t.T("common.cancel"));                          // missing there: built in
        Assert.Equal("2 game(s)", t.T("library.gameCount", new { count = 2 })); // its plural forms are not mixed with the built-in ones
        Assert.Equal("1 game(s)", t.T("library.gameCount", new { count = 1 }));
        Assert.Equal("Cancel", t.StringsOf("en")["common.cancel"]);
        Assert.Contains(folder.Log, l => l.Contains("en.json lacks"));

        Assert.True(t.SetLanguage("fr"));
        Assert.Equal("Cancel", t.T("common.cancel"));                          // French falls back to the completed English
        Assert.True(t.Has("library.title"));
    }

    [Fact]
    public void Lists_the_files_of_the_folder_with_their_metadata()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        folder.Write("broken.json", "{ not json");
        Translator t = folder.Translator();

        Assert.Equal(new[] { "en", "fr" }, t.Available.Select(l => l.Locale));
        LanguageInfo french = t.Available.Single(l => l.Locale == "fr");
        Assert.Equal("French", french.EnglishName);
        Assert.Equal("Français", french.NativeName);
        Assert.Contains(folder.Log, l => l.Contains("broken.json"));
    }

    [Fact]
    public void Measures_how_much_of_English_a_language_translates()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();

        // English: common.ok, common.cancel, library.title, library.gameCount, library.hello, library.lockHint.
        Assert.Equal(1, t.Completion("en"));
        Assert.Equal(3 / 6.0, t.Completion("fr"), 3);
    }

    [Fact]
    public void Uses_English_when_the_language_file_is_missing_or_broken()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        folder.Write("de.json", "[1, 2]");

        Translator missing = folder.Translator("it");
        Translator broken = folder.Translator("de");

        Assert.Equal("en", missing.Locale);
        Assert.Equal("OK", missing.T("common.ok"));
        Assert.Equal("en", broken.Locale);
        Assert.Contains(folder.Log, l => l.Contains("de.json"));
    }

    [Fact]
    public void Uses_the_built_in_English_when_en_json_is_missing()
    {
        using var folder = new LanguageFolder(("fr.json", LanguageFolder.French));
        Translator t = folder.Translator("fr", () => LanguageFolder.English);

        Assert.Equal("Cancel", t.T("common.cancel"));
        Assert.Contains(t.Available, l => l.Locale == "en" && l.Path == null);
    }

    [Fact]
    public void The_real_built_in_English_is_the_app_file()
    {
        string? json = I18n.BuiltInEnglish();

        Assert.NotNull(json);
        LanguageFile file = LanguageFile.Parse(json!, "en");
        Assert.Equal("English", file.Info.EnglishName);
        Assert.Equal("OK", file.Strings["common.ok"]);
        Assert.Empty(file.Problems);
    }

    [Fact]
    public void Switches_language_and_raises_one_event_per_change()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();
        var changes = new List<(string, string)>();
        t.LanguageChanged += (_, e) => changes.Add((e.Previous, e.Locale));

        Assert.True(t.SetLanguage("FR"));
        Assert.True(t.SetLanguage("fr"));
        Assert.False(t.SetLanguage("xx"));

        Assert.Equal("fr", t.Locale);
        Assert.Equal(new[] { ("en", "fr") }, changes);
        Assert.Equal("D'accord", t.T("common.ok"));
    }

    [Fact]
    public void Reads_files_again_when_switching()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();
        folder.Write("it.json", @"{ ""_meta"": { ""locale"": ""it"", ""englishName"": ""Italian"", ""nativeName"": ""Italiano"" }, ""common"": { ""ok"": ""Va bene"" } }");

        Assert.True(t.SetLanguage("it"));
        Assert.Equal("Va bene", t.T("common.ok"));
    }

    [Fact]
    public void Lookups_are_safe_while_the_language_changes()
    {
        using var folder = LanguageFolder.WithEnglishAndFrench();
        Translator t = folder.Translator();

        Parallel.For(0, 200, i =>
        {
            if (i % 20 == 0)
            {
                t.SetLanguage(i % 40 == 0 ? "fr" : "en");
            }

            string ok = t.T("common.ok");
            Assert.True(ok == "OK" || ok == "D'accord", ok);
            Assert.Equal("Cancel", t.T("common.cancel"));
        });
    }

    [Fact]
    public void Flattens_nested_objects_and_skips_what_is_not_a_string()
    {
        LanguageFile file = LanguageFile.Parse(@"{ ""_meta"": { ""locale"": ""xx"" }, ""a"": { ""b"": { ""c"": ""deep"" }, ""n"": 3, ""_hint"": ""note"" } }", "en");

        Assert.Equal(new[] { "a.b.c" }, file.Strings.Keys);
        Assert.Equal(2, file.Problems.Count); // a.n is a number; _meta.locale differs from the file name
        Assert.Equal("en", file.Info.Locale);
    }

    [Theory]
    [InlineData("library.gameCount_one", "library.gameCount")]
    [InlineData("library.gameCount_other", "library.gameCount")]
    [InlineData("library.title", "library.title")]
    [InlineData("library.some_thing", "library.some_thing")]
    public void Base_keys_drop_plural_suffixes(string key, string expected) => Assert.Equal(expected, PluralRules.BaseKey(key));
}
