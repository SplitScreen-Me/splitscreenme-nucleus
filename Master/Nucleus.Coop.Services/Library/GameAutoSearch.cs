using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop.Search;

namespace Nucleus.Coop.Services.Library;

/// <summary>An executable found by <see cref="GameAutoSearch"/> for a handler.</summary>
public sealed class AutoSearchCandidate
{
    /// <summary>Game id (handler GUID).</summary>
    public string GameId { get; set; } = "";

    /// <summary>Game name declared by the handler.</summary>
    public string GameName { get; set; } = "";

    /// <summary>Handler script file name.</summary>
    public string HandlerFile { get; set; } = "";

    /// <summary>Full path of the executable (what <c>library.addGame</c> takes).</summary>
    public string ExecutablePath { get; set; } = "";

    /// <summary>Folder of the executable.</summary>
    public string InstallFolder { get; set; } = "";

    /// <summary>This executable (or this game) is already in the library.</summary>
    public bool InLibrary { get; set; }
}

/// <summary>
/// Nucleus 2.5 automatic game executable search, headless: the user's game folders (<c>userGameDirectories.json</c>),
/// the <c>[Misc] AutoSearchGame</c> choice, and a cancellable search with progress. Not on the bridge yet: phase 2 wires
/// it into the web UI (a settings section for the choice and the folders, a "search my games" action of the library,
/// and the start folder of the executable picker after a handler install, in place of the Steam-only
/// <c>suggestedFolder</c>). Thread-safe; searches run on a worker.
/// </summary>
public sealed class GameAutoSearch
{
    private readonly GameSearchDirectories folders;

    /// <summary>Uses <paramref name="folders"/>, or the 2.5 file in <c>%USERPROFILE%\NucleusCoop</c>.</summary>
    public GameAutoSearch(GameSearchDirectories? folders = null)
    {
        this.folders = folders ?? new GameSearchDirectories();
    }

    /// <summary><c>[Misc] AutoSearchGame</c>: null = never asked (2.5 asked once, when adding a game), true = search the folders, false = don't.</summary>
    public bool? Enabled
    {
        get => App_Misc.AutoSearchGame;
        set => App_Misc.AutoSearchGame = value;
    }

    /// <summary>Saved game folders, sorted. Folders missing right now are listed too (an unplugged drive).</summary>
    public IReadOnlyList<string> Folders => folders.All;

    /// <summary>Adds a game folder (must exist). False when it was already saved.</summary>
    public bool AddFolder(string folder)
    {
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException(folder);
        }

        return folders.Add(folder);
    }

    /// <summary>Removes a game folder. False when it wasn't saved.</summary>
    public bool RemoveFolder(string folder) => folders.Remove(folder);

    /// <summary>
    /// Searches the saved folders (or <paramref name="roots"/>) for executables these handlers accept (2.5 "search
    /// disks"). Pass a snapshot of the handlers taken on the UI thread (<c>GameManager.Games.Values.ToList()</c>) and the
    /// library's executables; the search runs on a worker.
    /// </summary>
    public Task<List<AutoSearchCandidate>> SearchAsync(IReadOnlyCollection<GenericGameInfo> handlers, IReadOnlyCollection<(string GameId, string ExePath)> library,
        IProgress<GameSearchProgress>? progress, CancellationToken cancellationToken, IReadOnlyList<string>? roots = null)
    {
        IReadOnlyList<string> searchRoots = roots ?? folders.Existing;
        return Task.Run(() => ToCandidates(GameExecutableSearch.Search(handlers, searchRoots, progress, cancellationToken), library), cancellationToken);
    }

    /// <summary>
    /// The first executable of one handler in the saved folders (2.5: the start folder of the file picker after a handler
    /// install, when <see cref="Enabled"/>), or null.
    /// </summary>
    public Task<AutoSearchCandidate?> FindAsync(GenericGameInfo handler, IProgress<GameSearchProgress>? progress, CancellationToken cancellationToken)
    {
        IReadOnlyList<string> searchRoots = folders.Existing;
        return Task.Run<AutoSearchCandidate?>(() =>
        {
            List<GameSearchResult> found = GameExecutableSearch.Search(new[] { handler }, searchRoots, progress, cancellationToken, stopAtFirst: true);
            return ToCandidates(found, Array.Empty<(string, string)>()).FirstOrDefault();
        }, cancellationToken);
    }

    /// <summary>Search results as candidates: one per executable and game, sorted by game name, library state marked.</summary>
    internal static List<AutoSearchCandidate> ToCandidates(IEnumerable<GameSearchResult> results, IReadOnlyCollection<(string GameId, string ExePath)> library)
    {
        var inLibraryPaths = new HashSet<string>(library.Select(l => l.ExePath).Where(p => !string.IsNullOrEmpty(p)), StringComparer.OrdinalIgnoreCase);
        var inLibraryGames = new HashSet<string>(library.Select(l => l.GameId).Where(id => !string.IsNullOrEmpty(id)), StringComparer.OrdinalIgnoreCase);

        return results
            .Where(r => r.Game?.GUID != null)
            .GroupBy(r => (r.Game.GUID.ToLowerInvariant(), r.ExecutablePath.ToLowerInvariant()))
            .Select(g => g.First())
            .Select(r => new AutoSearchCandidate
            {
                GameId = r.Game.GUID,
                GameName = r.Game.GameName ?? r.Game.GUID,
                HandlerFile = r.Game.JsFileName ?? "",
                ExecutablePath = r.ExecutablePath,
                InstallFolder = r.InstallDirectory ?? "",
                InLibrary = inLibraryPaths.Contains(r.ExecutablePath) || inLibraryGames.Contains(r.Game.GUID),
            })
            .OrderBy(c => c.GameName, StringComparer.OrdinalIgnoreCase)
            .ThenBy(c => c.ExecutablePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
