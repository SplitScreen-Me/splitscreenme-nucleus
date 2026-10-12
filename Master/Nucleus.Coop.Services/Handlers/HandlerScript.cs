using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// What the installer reads from a handler script without running it (like <c>DownloadPrompt.ExtractHandler</c>):
/// <c>Game.ExecutableName</c>, <c>Game.GameName</c> and <c>Game.GUID</c> assignments at the start of a line.
/// </summary>
public sealed class HandlerScriptInfo
{
    /// <summary><c>Game.ExecutableName</c>, or null.</summary>
    public string? ExecutableName { get; set; }

    /// <summary><c>Game.GameName</c>, or null.</summary>
    public string? GameName { get; set; }

    /// <summary><c>Game.GUID</c>, or null.</summary>
    public string? Guid { get; set; }

    /// <summary>Whether the file looks like a Nucleus handler (declares a game name or an executable).</summary>
    public bool IsHandler => !string.IsNullOrEmpty(GameName) || !string.IsNullOrEmpty(ExecutableName);
}

/// <summary>Reads <see cref="HandlerScriptInfo"/> from handler scripts and turns game names into file names.</summary>
public static class HandlerScript
{
    // Game.X = "value"; or Game.X = 'value';  (leading spaces allowed; "Game.ExecutableNames" does not match "Game.ExecutableName")
    private static readonly Regex Assignment = new Regex(
        @"^\s*game\.(?<name>executablename|gamename|guid)\s*=\s*(?<q>[""'])(?<value>.*)\k<q>",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly char[] InvalidTitleChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    /// <summary>Parses a script file.</summary>
    public static HandlerScriptInfo ParseFile(string path) => Parse(File.ReadAllLines(path));

    /// <summary>Parses script lines. The first assignment of each property wins.</summary>
    public static HandlerScriptInfo Parse(IEnumerable<string> lines)
    {
        var info = new HandlerScriptInfo();
        foreach (string line in lines)
        {
            Match match = Assignment.Match(line);
            if (!match.Success)
            {
                continue;
            }

            string value = UnquoteValue(match.Groups["value"].Value, match.Groups["q"].Value[0]);
            switch (match.Groups["name"].Value.ToLowerInvariant())
            {
                case "executablename":
                    info.ExecutableName ??= value;
                    break;
                case "gamename":
                    info.GameName ??= value;
                    break;
                case "guid":
                    info.Guid ??= value;
                    break;
            }

            if (info.ExecutableName != null && info.GameName != null && info.Guid != null)
            {
                break;
            }
        }

        return info;
    }

    /// <summary>
    /// File name of a handler for a game name: the name without <c>\/:*?"&lt;&gt;|</c> (as the legacy installer),
    /// trimmed. Null when nothing usable is left.
    /// </summary>
    public static string? ToTitle(string? gameName)
    {
        if (gameName == null)
        {
            return null;
        }

        var chars = new List<char>(gameName.Length);
        foreach (char c in gameName)
        {
            if (Array.IndexOf(InvalidTitleChars, c) < 0 && !char.IsControl(c))
            {
                chars.Add(c);
            }
        }

        // Windows drops trailing dots and spaces from file names.
        string title = new string(chars.ToArray()).Trim().TrimEnd('.').Trim();
        return title.Length == 0 ? null : title;
    }

    private static string UnquoteValue(string raw, char quote)
    {
        // The regex is greedy up to the last quote of that kind: cut at the first unescaped closing quote instead.
        for (int i = 0; i < raw.Length; i++)
        {
            if (raw[i] == '\\')
            {
                i++;
                continue;
            }

            if (raw[i] == quote)
            {
                raw = raw.Substring(0, i);
                break;
            }
        }

        return raw.Replace("\\" + quote, quote.ToString()).Replace("\\\\", "\\");
    }
}
