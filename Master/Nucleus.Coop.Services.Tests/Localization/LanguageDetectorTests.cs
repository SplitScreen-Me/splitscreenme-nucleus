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

    private static readonly string[] BothChinese = { "en", "zh-Hans", "zh-Hant" };

    // Windows cultures: .NET Framework chains zh-TW → zh-CHT → zh-Hant → zh, zh-CN → zh-CHS → zh-Hans → zh.
    [Theory]
    [InlineData("zh-TW", "zh-Hant")]
    [InlineData("zh-HK", "zh-Hant")]
    [InlineData("zh-MO", "zh-Hant")]
    [InlineData("zh-CN", "zh-Hans")]
    [InlineData("zh-SG", "zh-Hans")]
    public void Reads_the_Chinese_script_of_a_Windows_culture(string culture, string expected) =>
        Assert.Equal(expected, LanguageDetector.Match(CultureInfo.GetCultureInfo(culture), BothChinese));

    // Browser and Windows tags often name the region only.
    [Theory]
    [InlineData("zh-TW", "zh-Hant")]
    [InlineData("zh-HK", "zh-Hant")]
    [InlineData("zh-CN", "zh-Hans")]
    [InlineData("zh", "zh-Hans")]
    [InlineData("zh-CHT", "zh-Hant")]
    [InlineData("zh-CHS", "zh-Hans")]
    [InlineData("zh-Hant-TW", "zh-Hant")]
    public void Infers_the_Chinese_script_from_the_region(string preferred, string expected) =>
        Assert.Equal(expected, LanguageDetector.Match(new[] { preferred }, BothChinese));

    [Theory]
    [InlineData("zh-HK", "zh-TW")]
    [InlineData("zh-Hant", "zh-TW")]
    [InlineData("zh-SG", "zh-CN")]
    public void Prefers_a_region_written_in_the_same_script(string preferred, string expected) =>
        Assert.Equal(expected, LanguageDetector.Match(new[] { preferred }, new[] { "en", "zh-CN", "zh-TW" }));

    [Fact]
    public void Tries_all_the_parents_of_a_culture_before_another_region() =>
        Assert.Equal("zh-Hant", LanguageDetector.Match(CultureInfo.GetCultureInfo("zh-TW"), new[] { "en", "zh-CN", "zh-Hant" }));

    [Fact]
    public void Falls_back_to_English_even_when_nothing_is_available() =>
        Assert.Equal("en", LanguageDetector.Match(new[] { "fr" }, new string[0]));
}
