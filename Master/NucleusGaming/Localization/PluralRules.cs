using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace Nucleus.Gaming.Localization
{
    /// <summary>CLDR plural operands of a number (https://unicode.org/reports/tr35/tr35-numbers.html#Operands).</summary>
    public struct PluralOperands
    {
        /// <summary>Absolute value.</summary>
        public decimal N;

        /// <summary>Integer digits of <see cref="N"/>.</summary>
        public long I;

        /// <summary>Number of visible fraction digits, with trailing zeros.</summary>
        public int V;

        /// <summary>Visible fraction digits, with trailing zeros, as an integer.</summary>
        public long F;

        /// <summary>Visible fraction digits, without trailing zeros, as an integer.</summary>
        public long T;

        public static PluralOperands From(decimal number)
        {
            decimal n = Math.Abs(number);
            int scale = (decimal.GetBits(n)[3] >> 16) & 0xFF;
            decimal integer = decimal.Truncate(n);
            long i = integer > long.MaxValue ? long.MaxValue : (long)integer;
            long f = 0;
            if (scale > 0 && scale <= 18)
            {
                f = (long)((n - integer) * Pow10(scale));
            }

            long t = f;
            while (t != 0 && t % 10 == 0)
            {
                t /= 10;
            }

            return new PluralOperands { N = n, I = i, V = scale, F = f, T = t };
        }

        private static decimal Pow10(int exponent)
        {
            decimal value = 1;
            for (int k = 0; k < exponent; k++)
            {
                value *= 10;
            }

            return value;
        }
    }

    /// <summary>
    /// Cardinal plural categories (CLDR): which form of a string a count takes, the same answer as
    /// <c>Intl.PluralRules</c> in the web UI. A string with plural forms has one key per category,
    /// <c>key_one</c>, <c>key_other</c>... (<c>_other</c> is required). Languages without rules here use the English rule;
    /// <see cref="Register"/> adds one.
    /// </summary>
    public static class PluralRules
    {
        /// <summary>The CLDR categories, in the order used by key suffixes.</summary>
        public static readonly string[] Categories = { "zero", "one", "two", "few", "many", "other" };

        private static readonly ConcurrentDictionary<string, Func<PluralOperands, string>> rules =
            new ConcurrentDictionary<string, Func<PluralOperands, string>>(StringComparer.OrdinalIgnoreCase);

        static PluralRules()
        {
            // One / other with "i = 1 and v = 0": English and most Germanic languages.
            foreach (string language in new[] { "en", "de", "nl", "sv", "fi", "et", "gl", "ur", "sw" })
            {
                Register(language, EnglishRule);
            }

            // "n = 1".
            foreach (string language in new[] { "tr", "hu", "el", "bg", "nb", "no", "nn", "ka", "az", "kk" })
            {
                Register(language, o => o.N == 1 ? "one" : "other");
            }

            // No plural forms.
            foreach (string language in new[] { "ja", "zh", "ko", "th", "vi", "id", "ms", "lo", "my", "km", "yue" })
            {
                Register(language, o => "other");
            }

            Register("fr", o => o.I == 0 || o.I == 1 ? "one" : MillionsMany(o));
            Register("pt", o => o.I == 0 || o.I == 1 ? "one" : MillionsMany(o));
            Register("pt-PT", o => o.I == 1 && o.V == 0 ? "one" : MillionsMany(o));
            Register("es", o => o.N == 1 ? "one" : MillionsMany(o));
            Register("it", o => o.I == 1 && o.V == 0 ? "one" : MillionsMany(o));
            Register("ca", o => o.I == 1 && o.V == 0 ? "one" : MillionsMany(o));
            Register("da", o => o.N == 1 || (o.T != 0 && (o.I == 0 || o.I == 1)) ? "one" : "other");

            Func<PluralOperands, string> eastSlavic = o =>
            {
                if (o.V != 0)
                {
                    return "other";
                }

                long mod10 = o.I % 10, mod100 = o.I % 100;
                if (mod10 == 1 && mod100 != 11) return "one";
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return "few";
                return "many";
            };
            Register("ru", eastSlavic);
            Register("uk", eastSlavic);
            Register("be", eastSlavic);

            Register("pl", o =>
            {
                if (o.I == 1 && o.V == 0) return "one";
                if (o.V != 0) return "other";
                long mod10 = o.I % 10, mod100 = o.I % 100;
                if (mod10 >= 2 && mod10 <= 4 && (mod100 < 12 || mod100 > 14)) return "few";
                return "many";
            });

            Func<PluralOperands, string> czech = o =>
            {
                if (o.V != 0) return "many";
                if (o.I == 1) return "one";
                return o.I >= 2 && o.I <= 4 ? "few" : "other";
            };
            Register("cs", czech);
            Register("sk", czech);

            Register("ar", o =>
            {
                if (o.N == 0) return "zero";
                if (o.N == 1) return "one";
                if (o.N == 2) return "two";
                if (o.N != decimal.Truncate(o.N)) return "other";
                long mod100 = o.I % 100;
                if (mod100 >= 3 && mod100 <= 10) return "few";
                if (mod100 >= 11) return "many";
                return "other";
            });
        }

        /// <summary>Adds or replaces the rule of a language (<c>fr</c>) or of a locale (<c>pt-PT</c>, which wins over <c>pt</c>).</summary>
        public static void Register(string localeOrLanguage, Func<PluralOperands, string> rule)
        {
            rules[localeOrLanguage] = rule ?? throw new ArgumentNullException(nameof(rule));
        }

        /// <summary>Whether <paramref name="locale"/> (or its language) has its own rule, rather than the English one.</summary>
        public static bool HasRule(string locale) => Find(locale) != null;

        /// <summary>Category of <paramref name="number"/> in <paramref name="locale"/>: zero, one, two, few, many or other.</summary>
        public static string Select(string locale, decimal number)
        {
            Func<PluralOperands, string> rule = Find(locale) ?? EnglishRule;
            return rule(PluralOperands.From(number));
        }

        /// <summary>Category of <paramref name="number"/> in <paramref name="locale"/>.</summary>
        public static string Select(string locale, double number)
        {
            if (double.IsNaN(number) || double.IsInfinity(number) || Math.Abs(number) > 7.9e27)
            {
                return "other";
            }

            // "R" keeps the shortest exact digits (1.5 stays 1.5, 1.0 becomes 1), like Intl.PluralRules does with numbers.
            return Select(locale, decimal.Parse(number.ToString("R", CultureInfo.InvariantCulture), NumberStyles.Float, CultureInfo.InvariantCulture));
        }

        /// <summary>Category of a count of any numeric type; anything else is "other".</summary>
        public static string Select(string locale, object count)
        {
            switch (count)
            {
                case int v: return Select(locale, (decimal)v);
                case long v: return Select(locale, (decimal)v);
                case short v: return Select(locale, (decimal)v);
                case byte v: return Select(locale, (decimal)v);
                case uint v: return Select(locale, (decimal)v);
                case ulong v: return Select(locale, (decimal)v);
                case decimal v: return Select(locale, v);
                case double v: return Select(locale, v);
                case float v: return Select(locale, (double)v);
                default: return "other";
            }
        }

        /// <summary>Whether a count argument is numeric (and so selects a plural form).</summary>
        public static bool IsNumber(object value) =>
            value is int || value is long || value is short || value is byte || value is uint || value is ulong || value is decimal || value is double || value is float;

        /// <summary><c>library.gameCount_one</c> → <c>library.gameCount</c>; other keys are returned as they are.</summary>
        public static string BaseKey(string key)
        {
            int at = key.LastIndexOf('_');
            if (at <= 0)
            {
                return key;
            }

            string suffix = key.Substring(at + 1);
            return Array.IndexOf(Categories, suffix) >= 0 ? key.Substring(0, at) : key;
        }

        private static Func<PluralOperands, string> Find(string locale)
        {
            if (string.IsNullOrEmpty(locale))
            {
                return null;
            }

            if (rules.TryGetValue(locale, out Func<PluralOperands, string> rule))
            {
                return rule;
            }

            int dash = locale.IndexOf('-');
            return dash > 0 && rules.TryGetValue(locale.Substring(0, dash), out rule) ? rule : null;
        }

        private static string EnglishRule(PluralOperands o) => o.I == 1 && o.V == 0 ? "one" : "other";

        // French, Spanish, Italian, Portuguese, Catalan: "many" for exact millions (1 000 000 de...), "other" otherwise.
        private static string MillionsMany(PluralOperands o) => o.I != 0 && o.I % 1000000 == 0 && o.V == 0 ? "many" : "other";
    }
}
