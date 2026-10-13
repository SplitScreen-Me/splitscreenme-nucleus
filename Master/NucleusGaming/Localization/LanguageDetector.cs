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
        // Chinese regions written in Traditional characters; the others use Simplified ones.
        private static readonly HashSet<string> TraditionalChineseRegions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "TW", "HK", "MO" };

        /// <summary>
        /// The best available locale for <paramref name="preferred"/> (Windows display language first, as
        /// <c>pt-BR</c> or <c>zh-TW</c>): an exact match, else a parent tag (<c>zh-Hant</c>, then <c>zh</c>; Chinese
        /// tags without a script get their usual one, so <c>zh-TW</c> reaches <c>zh-Hant</c>), else another region of
        /// the same language (<c>pt-BR</c> for <c>pt-PT</c>, the same script first), else the next preferred tag, else
        /// <paramref name="fallback"/>. Case doesn't matter; the answer is spelled as in <paramref name="available"/>.
        /// </summary>
        public static string Match(IEnumerable<string> preferred, IEnumerable<string> available, string fallback = Translator.DefaultLocale) =>
            MatchGroups((preferred ?? Enumerable.Empty<string>()).Select(tag => new[] { tag }), available, fallback);

        /// <summary>
        /// The best available locale for a Windows culture. Its name and its parents (<c>zh-TW</c>, <c>zh-CHT</c>,
        /// <c>zh-Hant</c>, <c>zh</c>) are one preference: all of them are tried before another region of the language.
        /// </summary>
        public static string Match(CultureInfo culture, IEnumerable<string> available, string fallback = Translator.DefaultLocale)
        {
            var tags = new List<string>();
            for (CultureInfo c = culture; c != null && !string.IsNullOrEmpty(c.Name); c = c.Parent)
            {
                tags.Add(c.Name);
            }

            return MatchGroups(new[] { tags }, available, fallback);
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

        /// <summary>
        /// The tag with its usual script when Chinese is written without one: <c>zh-TW</c> → <c>zh-Hant-TW</c>,
        /// <c>zh-CN</c> → <c>zh-Hans-CN</c>, <c>zh</c> → <c>zh-Hans</c>, and the old .NET names <c>zh-CHT</c> →
        /// <c>zh-Hant</c>, <c>zh-CHS</c> → <c>zh-Hans</c>. Other tags are returned as they are.
        /// </summary>
        public static string WithScript(string tag)
        {
            string[] parts = (tag ?? "").Split('-');
            if (!string.Equals(parts[0], "zh", StringComparison.OrdinalIgnoreCase) || ScriptOf(tag) != null)
            {
                return tag;
            }

            if (parts.Length == 2 && string.Equals(parts[1], "CHT", StringComparison.OrdinalIgnoreCase))
            {
                return parts[0] + "-Hant";
            }

            if (parts.Length == 2 && string.Equals(parts[1], "CHS", StringComparison.OrdinalIgnoreCase))
            {
                return parts[0] + "-Hans";
            }

            string script = parts.Length > 1 && TraditionalChineseRegions.Contains(parts[1]) ? "Hant" : "Hans";
            return parts.Length == 1 ? parts[0] + "-" + script : parts[0] + "-" + script + "-" + string.Join("-", parts.Skip(1));
        }

        // Each group is one preference: its tags and their parents are tried first, then another region of the language
        // of its first tag.
        private static string MatchGroups(IEnumerable<IEnumerable<string>> groups, IEnumerable<string> available, string fallback)
        {
            List<string> locales = available?.Where(l => !string.IsNullOrWhiteSpace(l)).ToList() ?? new List<string>();
            foreach (IEnumerable<string> group in groups)
            {
                List<string> tags = (group ?? Enumerable.Empty<string>())
                    .Where(t => !string.IsNullOrWhiteSpace(t))
                    .Select(t => t.Trim().Replace('_', '-'))
                    .ToList();
                if (tags.Count == 0)
                {
                    continue;
                }

                foreach (string candidate in Candidates(tags))
                {
                    string exact = locales.FirstOrDefault(l => string.Equals(l, candidate, StringComparison.OrdinalIgnoreCase));
                    if (exact != null)
                    {
                        return exact;
                    }
                }

                string sibling = Sibling(tags[0], locales);
                if (sibling != null)
                {
                    return sibling;
                }
            }

            return locales.FirstOrDefault(l => string.Equals(l, fallback, StringComparison.OrdinalIgnoreCase)) ?? fallback;
        }

        // zh-TW → zh-TW, zh-Hant-TW, zh-Hant, zh: each tag, then its form with a script and its parents, then its own
        // parents; bare languages last.
        private static IEnumerable<string> Candidates(IEnumerable<string> tags)
        {
            var all = new List<string>();
            foreach (string tag in tags)
            {
                all.Add(tag);
                for (string c = WithScript(tag); c.Length > 0; c = Parent(c))
                {
                    all.Add(c);
                }

                for (string c = Parent(tag); c.Length > 0; c = Parent(c))
                {
                    all.Add(c);
                }
            }

            List<string> distinct = all.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            return distinct.Where(t => t.IndexOf('-') > 0).Concat(distinct.Where(t => t.IndexOf('-') <= 0));
        }

        // Another region of the same language, the same script first (zh-TW for zh-HK), then the shortest tag.
        private static string Sibling(string wanted, List<string> locales)
        {
            string language = LanguageOf(wanted);
            string script = ScriptOf(WithScript(wanted));
            return locales
                .Where(l => string.Equals(LanguageOf(l), language, StringComparison.OrdinalIgnoreCase))
                .OrderBy(l => script != null && string.Equals(ScriptOf(WithScript(l)), script, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(l => l.Length)
                .ThenBy(l => l, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        // Script subtag (four letters after the language): zh-Hant-TW → Hant; null when there is none.
        private static string ScriptOf(string tag)
        {
            string[] parts = (tag ?? "").Split('-');
            return parts.Length > 1 && parts[1].Length == 4 && parts[1].All(char.IsLetter) ? parts[1] : null;
        }
    }
}
