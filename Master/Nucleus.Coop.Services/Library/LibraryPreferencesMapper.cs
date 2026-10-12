using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Converts the list sorting and filtering between the contract and the legacy storage, so both UIs agree:
/// <c>Settings.ini [Misc] GameSortingOpt</c> holds the names of the checked boxes of the legacy sort panel
/// (<c>Sort_1</c> descending, <c>Sort_2</c> max players, <c>Sort_3</c> play time, <c>Sort_4</c> last played,
/// <c>Show_Type1</c> gamepads, <c>Show_Type2</c> one keyboard and mouse, <c>Show_Type3</c> several), and
/// <c>[Dev] ShowFavoriteOnly</c> the favourites filter.
/// </summary>
public static class LibraryPreferencesMapper
{
    /// <summary>Token of "Sort Descending".</summary>
    public const string SortDescending = "Sort_1";

    /// <summary>Token of "Sort By Max Supported Players".</summary>
    public const string SortMaxPlayers = "Sort_2";

    /// <summary>Token of "Sort By Play Time".</summary>
    public const string SortPlaytime = "Sort_3";

    /// <summary>Token of "Sort By Last Played".</summary>
    public const string SortLastPlayed = "Sort_4";

    /// <summary>Token of "Supports Gamepads".</summary>
    public const string ShowGamepads = "Show_Type1";

    /// <summary>Token of "Supports Single Keyboard and Mouse".</summary>
    public const string ShowKeyboard = "Show_Type2";

    /// <summary>Token of "Supports Multiple Keyboards And Mice".</summary>
    public const string ShowMultipleKeyboards = "Show_Type3";

    /// <summary>Reads the legacy tokens. When several sorts are checked, the legacy precedence applies: play time, last played, max players, descending.</summary>
    public static LibraryPreferences FromLegacy(IEnumerable<string>? tokens, bool favoritesOnly)
    {
        var set = new HashSet<string>((tokens ?? Enumerable.Empty<string>()).Select(t => t?.Trim() ?? "").Where(t => t.Length > 0), StringComparer.OrdinalIgnoreCase);

        LibrarySort sort = set.Contains(SortPlaytime) ? LibrarySort.Playtime
            : set.Contains(SortLastPlayed) ? LibrarySort.LastPlayed
            : set.Contains(SortMaxPlayers) ? LibrarySort.MaxPlayers
            : set.Contains(SortDescending) ? LibrarySort.NameDescending
            : LibrarySort.Name;

        return new LibraryPreferences
        {
            Sort = sort,
            FavoritesOnly = favoritesOnly,
            FilterGamepads = set.Contains(ShowGamepads),
            FilterKeyboard = set.Contains(ShowKeyboard),
            FilterMultipleKeyboards = set.Contains(ShowMultipleKeyboards),
        };
    }

    /// <summary>Legacy tokens for preferences, in the order of the legacy panel.</summary>
    public static List<string> ToLegacy(LibraryPreferences preferences)
    {
        var tokens = new List<string>();
        switch (preferences.Sort)
        {
            case LibrarySort.NameDescending:
                tokens.Add(SortDescending);
                break;
            case LibrarySort.MaxPlayers:
                tokens.Add(SortMaxPlayers);
                break;
            case LibrarySort.Playtime:
                tokens.Add(SortPlaytime);
                break;
            case LibrarySort.LastPlayed:
                tokens.Add(SortLastPlayed);
                break;
        }

        if (preferences.FilterGamepads)
        {
            tokens.Add(ShowGamepads);
        }

        if (preferences.FilterKeyboard)
        {
            tokens.Add(ShowKeyboard);
        }

        if (preferences.FilterMultipleKeyboards)
        {
            tokens.Add(ShowMultipleKeyboards);
        }

        return tokens;
    }
}
