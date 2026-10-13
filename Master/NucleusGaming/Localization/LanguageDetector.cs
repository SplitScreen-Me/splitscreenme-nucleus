using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Nucleus.Gaming.Localization
{
    /// <summary>
    /// Picks the language that matches the Windows display language among the available ones. The web UI's mock does
    /// the same with <c>navigator.languages</c> (<c>matchLocale</c> in src/i18n/core.ts).
    /// </summary>
    public static class LanguageDetector
    {
        /// <summary>
        /// The best available locale for <paramref name="preferred"/> (Windows display language first, as
        /// <c>pt-BR</c> or <c>zh-Hant-TW</c>): an exact match, else a parent tag (<c>zh-Hant</c>, then <c>zh</c>), else
        /// another region of the same language (<c>pt-BR</c> for <c>pt-PT</c>), else the next preferred tag, else
        /// <paramref name="fallback"/>. Case doesn't matter; the answer is spelled as in <paramref name="available"/>.
        /// </summary>
        public static string Match(IEnumerable<string> preferred, IEnumerable<string> available, string fallback = Translator.DefaultLocale)
        {
            List<string> locales = available?.Where(l => !string.IsNullOrWhiteSpace(l)).ToList() ?? new List<string>();
            foreach (string tag in preferred ?? Enumerable.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                string wanted = tag.Trim().Replace('_', '-');
                for (string candidate = wanted; candidate.Length > 0; candidate = Parent(candidate))
                {
                    string exact = locales.FirstOrDefault(l => string.Equals(l, candidate, StringComparison.OrdinalIgnoreCase));
                    if (exact != null)
                    {
                        return exact;
                    }
                }

                string language = LanguageOf(wanted);
                string sibling = locales
                    .Where(l => string.Equals(LanguageOf(l), language, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(l => l.Length)
                    .ThenBy(l => l, StringComparer.OrdinalIgnoreCase)
                    .FirstOrDefault();
                if (sibling != null)
                {
                    return sibling;
                }
            }

            return locales.FirstOrDefault(l => string.Equals(l, fallback, StringComparison.OrdinalIgnoreCase)) ?? fallback;
        }

        /// <summary>The best available locale for a Windows culture (its name, then its parents).</summary>
        public static string Match(CultureInfo culture, IEnumerable<string> available, string fallback = Translator.DefaultLocale)
        {
            var tags = new List<string>();
            for (CultureInfo c = culture; c != null && !string.IsNullOrEmpty(c.Name); c = c.Parent)
            {
                tags.Add(c.Name);
            }

            return Match(tags, available, fallback);
        }

        /// <summary><c>pt-BR</c> → <c>pt</c>, <c>zh-Hant-TW</c> → <c>zh-Hant</c>, <c>en</c> → empty.</summary>
        public static string Parent(string tag)
        {
            int dash = tag.LastIndexOf('-');
            return dash > 0 ? tag.Substring(0, dash) : "";
        }

        /// <summary>Language subtag: <c>pt-BR</c> → <c>pt</c>.</summary>
        public static string LanguageOf(string tag)
        {
            int dash = tag.IndexOf('-');
            return dash > 0 ? tag.Substring(0, dash) : tag;
        }
    }
}
