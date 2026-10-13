using System.Globalization;
using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

public class LanguageDetectorTests
{
    private static readonly string[] Available = { "en", "fr", "pt-BR", "zh-Hant", "de" };

    [Theory]
    [InlineData("fr-FR", "fr")]          // language part
    [InlineData("fr", "fr")]
    [InlineData("pt-BR", "pt-BR")]       // exact
    [InlineData("PT-br", "pt-BR")]       // case doesn't matter, the answer is spelled as available
    [InlineData("pt-PT", "pt-BR")]       // another region of the same language beats English
    [InlineData("zh-Hant-TW", "zh-Hant")] // parent tag
    [InlineData("de_AT", "de")]          // underscores
    [InlineData("ja-JP", "en")]          // nothing matches: English
    [InlineData("", "en")]
    public void Matches_the_preferred_language(string preferred, string expected) =>
        Assert.Equal(expected, LanguageDetector.Match(new[] { preferred }, Available));

    [Fact]
    public void Tries_each_preferred_language_in_order() =>
        Assert.Equal("de", LanguageDetector.Match(new[] { "ja-JP", "de-CH", "fr" }, Available));

    [Fact]
    public void Prefers_the_language_itself_over_one_of_its_regions() =>
        Assert.Equal("pt", LanguageDetector.Match(new[] { "pt-AO" }, new[] { "en", "pt-BR", "pt" }));

    [Fact]
    public void Reads_a_culture_and_its_parents()
    {
        Assert.Equal("fr", LanguageDetector.Match(CultureInfo.GetCultureInfo("fr-CA"), Available));
        Assert.Equal("pt-BR", LanguageDetector.Match(CultureInfo.GetCultureInfo("pt-BR"), Available));
        Assert.Equal("en", LanguageDetector.Match(CultureInfo.GetCultureInfo("ko-KR"), Available));
    }

    [Fact]
    public void Falls_back_to_English_even_when_nothing_is_available() =>
        Assert.Equal("en", LanguageDetector.Match(new[] { "fr" }, new string[0]));
}
