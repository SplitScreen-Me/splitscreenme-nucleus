using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Nucleus.Coop.Services.Supporters;

/// <summary>
/// Reads the published supporters list (<c>tiers.txt</c>: one <c>&lt;div&gt;Name&lt;/div&gt;</c> per line) into plain names.
/// Tolerant of what a hand-edited file may hold: other tags, several names on one line, entities, blank lines.
/// </summary>
public static class SupporterNames
{
    /// <summary>Longest name kept; longer ones are cut and end with an ellipsis.</summary>
    public const int MaxNameLength = 80;

    /// <summary>Most names kept (the list had about 500 in 2026).</summary>
    public const int MaxNames = 5000;

    // Tags that end a name: block elements and line breaks. Any other tag is dropped.
    private static readonly Regex Breaks = new Regex(@"</?(?:div|p|li|ul|ol|br|tr|td|h[1-6])\b[^>]*>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    // A tag starts with a letter (or /letter, !, ?), so "<3 Bob" stays a name.
    private static readonly Regex Tags = new Regex(@"<(?:/?[A-Za-z][^>]*|![^>]*|\?[^>]*)>", RegexOptions.CultureInvariant);

    private static readonly Regex Spaces = new Regex(@"\s+", RegexOptions.CultureInvariant);

    /// <summary>
    /// The names of a list, in order: tags stripped, entities decoded, white space collapsed, control and bidi
    /// characters removed. Lines without a letter, a digit or a symbol ("? ?", a name whose characters were lost) are
    /// skipped.
    /// </summary>
    public static List<string> Parse(string? text)
    {
        var names = new List<string>();
        if (string.IsNullOrEmpty(text))
        {
            return names;
        }

        string broken = Breaks.Replace(text, "\n");
        foreach (string line in broken.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
        {
            string? name = Clean(line);
            if (name != null)
            {
                names.Add(name);
                if (names.Count == MaxNames)
                {
                    break;
                }
            }
        }

        return names;
    }

    /// <summary>One name as it is shown, or null when nothing readable is left.</summary>
    public static string? Clean(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        // Strip tags first, then decode: "&lt;b&gt;" is text someone typed and must stay text.
        string text = WebUtility.HtmlDecode(Tags.Replace(raw, " "));
        var kept = new StringBuilder(text.Length);
        foreach (char c in text)
        {
            if (IsInvisible(c))
            {
                continue;
            }

            kept.Append(char.IsControl(c) ? ' ' : c);
        }

        string name = Spaces.Replace(kept.ToString(), " ").Trim();
        if (name.Length == 0 || !name.Any(IsReadable))
        {
            return null;
        }

        if (name.Length > MaxNameLength)
        {
            int cut = MaxNameLength - 1;
            if (char.IsHighSurrogate(name[cut - 1]))
            {
                cut--;
            }

            name = name.Substring(0, cut).TrimEnd() + "…";
        }

        return name;
    }

    // Letters and digits of any script, emoji and other symbols (♥): something a person chose to be called.
    private static bool IsReadable(char c) =>
        char.IsLetterOrDigit(c) || char.IsSurrogate(c) || char.GetUnicodeCategory(c) == UnicodeCategory.OtherSymbol;

    // Invisible characters that can hide a name or reorder the text around it: zero-width space, byte order mark, bidi
    // embeddings, overrides and isolates. Joiners stay (emoji sequences, Persian).
    private static bool IsInvisible(char c) =>
        c == (char)0x200B || c == (char)0xFEFF || (c >= (char)0x202A && c <= (char)0x202E) || (c >= (char)0x2066 && c <= (char)0x2069);
}
