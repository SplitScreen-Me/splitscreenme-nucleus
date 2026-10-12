using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Reads the play time and last session values of metaInfo.json, which the legacy app wrote loosely: total seconds as
/// a string, and <c>DateTime.Now.ToString()</c> in whatever culture Windows used at that time.
/// </summary>
public static class PlaytimeParser
{
    private static readonly Regex Formatted = new Regex(@"^\s*(\d+)\s*h\s*:\s*(\d+)\s*m\s*:\s*(\d+)\s*s\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly string[] FallbackCultures =
    {
        "en-US", "en-GB", "fr-FR", "de-DE", "es-ES", "it-IT", "pt-BR", "ru-RU", "pl-PL", "nl-NL", "sv-SE", "tr-TR", "ja-JP", "zh-CN", "ko-KR",
    };

    /// <summary>
    /// Total seconds from the stored value: digits ("1234", also "1234.0"), or the displayed format
    /// "HHh:MMm:SSs". Null, "...", negative or garbage give 0.
    /// </summary>
    public static long ParseSeconds(string? stored)
    {
        if (string.IsNullOrWhiteSpace(stored))
        {
            return 0;
        }

        string value = stored!.Trim();
        if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out long seconds))
        {
            return Math.Max(0, seconds);
        }

        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double fractional) && !double.IsNaN(fractional) && !double.IsInfinity(fractional))
        {
            return fractional <= 0 ? 0 : (long)Math.Min(fractional, long.MaxValue / 2);
        }

        Match match = Formatted.Match(value);
        if (match.Success
            && long.TryParse(match.Groups[1].Value, out long h)
            && long.TryParse(match.Groups[2].Value, out long m)
            && long.TryParse(match.Groups[3].Value, out long s))
        {
            return (h * 3600) + (m * 60) + s;
        }

        return 0;
    }

    /// <summary>
    /// Last session as UTC from the stored culture-formatted local time. Tries the current culture first (most
    /// likely the one that wrote it), then the invariant culture, ISO 8601 and common cultures. Null when unknown.
    /// </summary>
    public static DateTime? ParseLastPlayed(string? stored, CultureInfo? current = null)
    {
        if (string.IsNullOrWhiteSpace(stored) || stored!.Trim() == "...")
        {
            return null;
        }

        string value = stored.Trim();
        const DateTimeStyles styles = DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeLocal;

        if (DateTime.TryParseExact(value, "o", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime iso))
        {
            return ToUtc(iso);
        }

        CultureInfo first = current ?? CultureInfo.CurrentCulture;
        if (DateTime.TryParse(value, first, styles, out DateTime parsed))
        {
            return ToUtc(parsed);
        }

        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, styles, out parsed))
        {
            return ToUtc(parsed);
        }

        foreach (string name in FallbackCultures)
        {
            try
            {
                if (DateTime.TryParse(value, CultureInfo.GetCultureInfo(name), styles, out parsed))
                {
                    return ToUtc(parsed);
                }
            }
            catch (CultureNotFoundException)
            {
            }
        }

        return null;
    }

    private static DateTime ToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Local).ToUniversalTime(),
        _ => value.ToUniversalTime(),
    };
}
