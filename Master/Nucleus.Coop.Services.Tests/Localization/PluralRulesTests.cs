using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Nucleus.Gaming.Localization;
using Xunit;

namespace Nucleus.Coop.Services.Tests.Localization;

public class PluralRulesTests
{
    /// <summary>
    /// Cases of the fixture shared with the web UI tests (Nucleus.WebUI/src/bridge/__tests__/fixtures/plurals.json), where
    /// they are checked against Intl.PluralRules: the host and the UI pick the same plural form.
    /// </summary>
    public static IEnumerable<object[]> SharedCases()
    {
        JObject fixture = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Localization", "Fixtures", "plurals.json")));
        foreach (JProperty locale in fixture.Properties().Where(p => !p.Name.StartsWith("_", StringComparison.Ordinal)))
        {
            foreach (JProperty sample in ((JObject)locale.Value).Properties())
            {
                yield return new object[] { locale.Name, sample.Name, (string)sample.Value! };
            }
        }
    }

    [Theory]
    [MemberData(nameof(SharedCases))]
    public void Matches_Intl_PluralRules(string locale, string number, string expected)
    {
        decimal value = decimal.Parse(number, CultureInfo.InvariantCulture);

        Assert.Equal(expected, PluralRules.Select(locale, value));
        Assert.Equal(expected, PluralRules.Select(locale, (double)value));
    }

    [Fact]
    public void Accepts_any_numeric_count()
    {
        Assert.Equal("one", PluralRules.Select("en", (object)1L));
        Assert.Equal("other", PluralRules.Select("en", (object)2f));
        Assert.Equal("other", PluralRules.Select("en", (object)"1"));
        Assert.Equal("other", PluralRules.Select("en", double.NaN));
    }

    [Fact]
    public void Unknown_languages_use_the_English_rule_and_regions_fall_back_to_their_language()
    {
        Assert.False(PluralRules.HasRule("xx"));
        Assert.Equal("one", PluralRules.Select("xx", 1m));
        Assert.True(PluralRules.HasRule("fr-CA"));
        Assert.Equal("one", PluralRules.Select("fr-CA", 0m));
    }

    [Theory]
    [InlineData("pt-BR", "one")] // pt: i = 0..1
    [InlineData("pt-PT", "other")] // pt-PT: i = 1 and v = 0
    [InlineData("PT-pt", "other")]
    [InlineData("pt-PT-u-nu-latn", "other")]
    [InlineData("pt-AO", "one")] // ICU drops subtags from the end: pt-AO gets the pt rule, as in Intl.PluralRules
    [InlineData("pt", "one")]
    public void Regions_use_their_own_rule_else_the_one_of_their_language(string locale, string expected)
    {
        Assert.True(PluralRules.HasRule(locale));
        Assert.Equal(expected, PluralRules.Select(locale, 0m));
    }

    [Fact]
    public void Rules_can_be_added()
    {
        PluralRules.Register("x-test", o => o.I == 2 ? "two" : "other");

        Assert.Equal("two", PluralRules.Select("x-test", 2m));
    }
}
