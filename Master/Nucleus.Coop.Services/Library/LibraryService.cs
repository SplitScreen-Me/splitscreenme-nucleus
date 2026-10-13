using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// <c>library.*</c>: the user's games. Runs on the UI thread (bridge dispatch); disk-heavy and network work runs on
/// workers and hands results back to the UI thread before touching GameManager state.
/// </summary>
public sealed class LibraryService : ILibraryService
{
    private readonly NucleusCore core;
    private readonly GameDtoFactory dto;
    private readonly GameMenuActions menu;
    private readonly SemaphoreSlim artworkQueue = new SemaphoreSlim(1, 1);

    /// <summary>Creates the service.</summary>
    public LibraryService(NucleusCore core)
    {
        this.core = core;
        dto = new GameDtoFactory(core.Icons, core.Updates.LatestVersion);
        menu = new GameMenuActions(core, RemoveGame);
    }

    private static GameManager Manager => GameManager.Instance;

    /// <inheritdoc />
    public List<GameSummary> List() =>
        LibraryEntries().Select(dto.Summary).ToList();

    /// <inheritdoc />
    public GameDetails Get(GameRequest request) => dto.Details(Find(request?.GameId));

    /// <inheritdoc />
    public void SetFavorite(SetFavoriteRequest request)
    {
        UserGameInfo entry = Find(request?.GameId);
        if (entry.Game.MetaInfo.Favorite != request!.Favorite)
        {
            entry.Game.MetaInfo.Favorite = request.Favorite;
        }

        core.Notifier.GamesChanged(entry.Game.GUID);
    }

    /// <inheritdoc />
    public GameMenu GetMenu(GameRequest request) => GameMenuBuilder.Build(GameMenuActions.CreateContext(Find(request?.GameId)));

    /// <inheritdoc />
    public Task RunMenuAction(RunMenuActionRequest request)
    {
        if (string.IsNullOrEmpty(request?.ItemId))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "itemId is required.");
        }

        return menu.RunAsync(Find(request!.GameId), request.ItemId, request.Value);
    }

    /// <inheritdoc />
    public async Task<AddGameResult> AddGame(AddGameRequest request)
    {
        string path = CheckExecutable(request?.ExecutablePath);

        // Handlers whose Game.ExecutableNames / ExecutableName match the file name and whose ExecutableContext files exist.
        List<GenericGameInfo> candidates = Manager.GetGames(path).GroupBy(g => g.GUID).Select(g => g.First()).ToList();
        GenericGameInfo game;

        if (!string.IsNullOrEmpty(request!.HandlerGameId))
        {
            game = candidates.FirstOrDefault(c => string.Equals(c.GUID, request.HandlerGameId, StringComparison.OrdinalIgnoreCase))
                ?? throw HandlerMismatch(request.HandlerGameId!, path);
        }
        else if (candidates.Count == 0)
        {
            throw NoHandler(path);
        }
        else if (candidates.Count > 1)
        {
            return new AddGameResult
            {
                Candidates = candidates.OrderBy(c => c.GameName).Select(c => new HandlerCandidate { GameId = c.GUID, Name = c.GameName ?? c.GUID }).ToList(),
            };
        }
        else
        {
            game = candidates[0];
        }

        UserGameInfo entry = AddToLibrary(game, path);
        await core.UserProfile.SaveAsync();
        ServiceLog.Info($"added {game.GameName} ({game.GUID}): {path}");
        core.Notifier.GamesChanged(game.GUID);

        CheckUpdateInBackground(game.GUID);
        DownloadArtworkInBackground(game.GUID);

        return new AddGameResult { Game = dto.Summary(entry) };
    }

    /// <summary>
    /// Explains why no handler accepts an executable: either no handler expects that file name, or one does but the
    /// files it expects next to the executable (Game.ExecutableContext) are missing, which usually means a wrong copy of the exe.
    /// </summary>
    private static BridgeException NoHandler(string path)
    {
        string fileName = Path.GetFileName(path);
        string folder = Path.GetDirectoryName(path) ?? "";
        bool NameMatches(GenericGameInfo g) =>
            (g.ExecutableNames?.Any(n => string.Equals(n, fileName, StringComparison.OrdinalIgnoreCase)) ?? false)
            || string.Equals(g.ExecutableName, fileName, StringComparison.OrdinalIgnoreCase);

        GenericGameInfo? byName = Manager.Games.Values.FirstOrDefault(NameMatches);
        if (byName != null)
        {
            List<string> missing = (byName.ExecutableContext ?? new string[0])
                .Where(c => !File.Exists(Path.Combine(folder, c)) && !Directory.Exists(Path.Combine(folder, c)))
                .ToList();
            return new BridgeException(BridgeErrorKind.NotFound,
                I18n.T("host.library.errors.contextMissing", new { game = byName.GameName, files = string.Join(", ", missing), executable = fileName }),
                new { reason = "contextMissing", executable = fileName, gameId = byName.GUID, missing });
        }

        return new BridgeException(BridgeErrorKind.NotFound,
            I18n.T("host.library.errors.noHandler", new { executable = fileName }),
            new { reason = "noHandler", executable = fileName });
    }

    /// <inheritdoc />
    public void SetExecutable(SetExecutableRequest request)
    {
        UserGameInfo entry = Find(request?.GameId);
        string path = CheckExecutable(request!.ExecutablePath);
        GenericGameInfo game = entry.Game;

        if (!Manager.GetGames(path).Any(c => string.Equals(c.GUID, game.GUID, StringComparison.OrdinalIgnoreCase)))
        {
            throw HandlerMismatch(game.GUID, path);
        }

        UserGameInfo? other = Manager.User.Games.FirstOrDefault(g => g != entry && string.Equals(g.ExePath, path, StringComparison.OrdinalIgnoreCase));
        if (other != null)
        {
            throw new BridgeException(BridgeErrorKind.Conflict, I18n.T("host.library.errors.alreadyInLibrary", new { game = other.Game?.GameName ?? other.GameGuid }), new { reason = "alreadyInLibrary", gameId = other.GameGuid });
        }

        entry.ExePath = path; // metaInfo, profiles and assets are keyed by the game id: nothing else moves
        entry.Icon = null;
        if (game.ExecutableNames != null)
        {
            game.ExecutableName = Path.GetFileName(path);
        }

        _ = core.UserProfile.SaveAsync();
        ServiceLog.Info($"{game.GameName} ({game.GUID}) now points at {path}");
        core.Notifier.GamesChanged(game.GUID);
    }

    /// <inheritdoc />
    public void Remove(GameRequest request) => RemoveGame(Find(request?.GameId).Game.GUID);

    /// <inheritdoc />
    public Task<List<InstalledGame>> ScanInstalledGames() => Task.Run(InstalledGamesScanner.Scan);

    /// <inheritdoc />
    public Task<string?> FindExecutable(FindExecutableRequest request)
    {
        string folder = request?.Folder?.Trim().Trim('"') ?? "";
        string name = request?.ExecutableName?.Trim() ?? "";
        if (folder.Length == 0 || name.Length == 0)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "folder and executableName are required.");
        }

        if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || name.IndexOfAny(new[] { '*', '?' }) >= 0)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "executableName must be a file name, for example \"game.exe\".");
        }

        if (!Directory.Exists(folder))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.errors.folderNotFound"), new { path = folder });
        }

        return Task.Run(() => ExecutableFinder.Find(folder, name, CancellationToken.None));
    }

    /// <inheritdoc />
    public async Task DownloadArtwork(DownloadArtworkRequest request, CancellationToken cancellationToken)
    {
        List<(string GameId, string HandlerId)> targets;
        if (!string.IsNullOrEmpty(request?.GameId))
        {
            GenericGameInfo game = Find(request!.GameId).Game;
            if (!HubClient.IsValidHubId(game.Hub?.Handler?.Id))
            {
                throw new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.library.errors.noHubHandler"), new { reason = "noHubHandler" });
            }

            targets = new List<(string, string)> { (game.GUID, game.Hub!.Handler.Id) };
        }
        else
        {
            targets = ArtworkTargets();
            if (targets.Count == 0)
            {
                return;
            }
        }

        if (!await core.HubStatus.EnsureOnlineAsync(cancellationToken))
        {
            throw Offline();
        }

        await DownloadArtworkCoreAsync(targets, throwWhenAllFail: true, cancellationToken);
    }

    /// <inheritdoc />
    public async Task Reload()
    {
        if (NucleusCore.SessionRunning)
        {
            throw new BridgeException(BridgeErrorKind.Conflict, I18n.T("host.library.errors.sessionRunningReload"), new { reason = "sessionRunning" });
        }

        await ReloadHandlersAsync(core);
    }

    /// <inheritdoc />
    public LibraryPreferences GetPreferences() => LibraryPreferencesMapper.FromLegacy(App_Misc.GamesSorting, App_Misc.ShowFavoriteOnly);

    /// <inheritdoc />
    public void SetPreferences(LibraryPreferences preferences)
    {
        if (preferences == null)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "preferences are required.");
        }

        App_Misc.GamesSorting = LibraryPreferencesMapper.ToLegacy(preferences);
        if (App_Misc.ShowFavoriteOnly != preferences.FavoritesOnly)
        {
            App_Misc.ShowFavoriteOnly = preferences.FavoritesOnly;
        }
    }

    /// <summary>
    /// Parses every handler again on a worker (legacy <c>RefreshHandlers</c> built a whole new GameManager, which also
    /// registered raw input once more), swaps them in on the UI thread and rebinds the library entries.
    /// </summary>
    internal static async Task ReloadHandlersAsync(NucleusCore core)
    {
        string folder = Manager.GetJsScriptsPath();
        List<GenericGameInfo> parsed = await Task.Run(() => ParseHandlers(folder));

        GameManager manager = Manager;
        manager.Games.Clear();
        manager.GameInfos.Clear();
        foreach (GenericGameInfo info in parsed)
        {
            manager.Games[info.GUID] = info; // a duplicate GUID replaces the earlier one, as in GameManager.Initialize
            manager.GameInfos[info.GUID] = info;
        }

        // UserGameInfo caches its GenericGameInfo: give every entry a fresh object (same id and path).
        var rebuilt = new List<UserGameInfo>();
        var dropped = new List<string>();
        foreach (UserGameInfo old in manager.User.Games)
        {
            if (!manager.Games.TryGetValue(old.GameGuid, out GenericGameInfo game))
            {
                dropped.Add(old.GameGuid);
                continue;
            }

            if (game.ExecutableNames != null && !string.IsNullOrEmpty(old.ExePath))
            {
                game.ExecutableName = Path.GetFileName(old.ExePath);
            }

            rebuilt.Add(new UserGameInfo { GameGuid = old.GameGuid, ExePath = old.ExePath });
        }

        manager.User.Games = rebuilt;
        await core.UserProfile.SaveAsync();
        ServiceLog.Info($"reloaded {parsed.Count} handler(s); removed from the library (handler missing): {(dropped.Count == 0 ? "none" : string.Join(", ", dropped))}");

        core.Notifier.Everything();
        core.Events.Emit(new HandlersChanged { GameIds = manager.Games.Keys.ToList() });
        LibraryHooks.RaiseHandlersReplaced(new List<string>());

        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets();
        _ = Task.Run(async () =>
        {
            try
            {
                if (await core.HubStatus.EnsureOnlineAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    await core.Updates.CheckAsync(targets, throwWhenOffline: false, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn("update check after reload failed", e);
            }
        });
    }

    private static List<GenericGameInfo> ParseHandlers(string folder)
    {
        var result = new List<GenericGameInfo>();
        if (!Directory.Exists(folder))
        {
            return result;
        }

        foreach (FileInfo file in new DirectoryInfo(folder).GetFiles("*.js"))
        {
            try
            {
                using Stream stream = file.OpenRead();
                string assets = Path.Combine(file.Directory!.FullName, Path.GetFileNameWithoutExtension(file.Name));
                var info = new GenericGameInfo(file.Name, assets, stream, new[] { false, false });
                if (!string.IsNullOrEmpty(info.GUID))
                {
                    result.Add(info);
                }
            }
            catch (Exception e)
            {
                // Script errors are reported by GenericGameInfo (a notice in the web UI).
                ServiceLog.Warn($"handler {file.Name} could not be loaded", e);
            }
        }

        return result;
    }

    private static IEnumerable<UserGameInfo> LibraryEntries() =>
        Manager.User.Games.Where(g => g?.Game != null).GroupBy(g => g.GameGuid, StringComparer.OrdinalIgnoreCase).Select(g => g.First());

    private static UserGameInfo Find(string? gameId)
    {
        if (string.IsNullOrEmpty(gameId))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "gameId is required.");
        }

        return LibraryEntries().FirstOrDefault(g => string.Equals(g.GameGuid, gameId, StringComparison.OrdinalIgnoreCase))
            ?? throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.errors.gameNotFound"), new { gameId });
    }

    private static string CheckExecutable(string? rawPath)
    {
        string path = rawPath?.Trim().Trim('"') ?? "";
        if (path.Length == 0)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "executablePath is required.");
        }

        try
        {
            path = Path.GetFullPath(path);
        }
        catch (Exception)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "executablePath is not a valid path.");
        }

        if (!File.Exists(path))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.executableNotFound"), new { reason = "fileNotFound", path });
        }

        if (DriveFormat.IsExFat(path))
        {
            throw new BridgeException(BridgeErrorKind.Conflict,
                I18n.T("host.library.errors.exFat"),
                new { reason = "exFat" });
        }

        return path;
    }

    private static BridgeException HandlerMismatch(string handlerGameId, string path)
    {
        if (!Manager.Games.TryGetValue(handlerGameId, out GenericGameInfo handler))
        {
            return new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.handlerNotFound"), new { reason = "handlerNotFound", gameId = handlerGameId });
        }

        string expected = handler.ExecutableNames?.Length > 0 ? string.Join(", ", handler.ExecutableNames) : handler.ExecutableName;
        return new BridgeException(BridgeErrorKind.NotFound,
            I18n.T("host.library.errors.handlerMismatch", new { executable = Path.GetFileName(path), game = handler.GameName, expected }),
            new { reason = "handlerMismatch", expected });
    }

    private BridgeException Offline() =>
        new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.hub.errors.offline", new { hub = core.Hub.DisplayName }), new { reason = "offline" });

    // GameManager.TryAddGame without its quirks: the path keeps its case (InitializeDefault lowercases every 'I'),
    // and a game already in the library moves to the new executable instead of being listed twice.
    private static UserGameInfo AddToLibrary(GenericGameInfo game, string path)
    {
        List<UserGameInfo> games = Manager.User.Games;
        games.RemoveAll(g => string.Equals(g.ExePath, path, StringComparison.OrdinalIgnoreCase)
                          || string.Equals(g.GameGuid, game.GUID, StringComparison.OrdinalIgnoreCase));

        game.MetaInfo.FirstLaunch = true;
        if (game.ExecutableNames != null)
        {
            game.ExecutableName = Path.GetFileName(path);
        }

        var entry = new UserGameInfo();
        entry.InitializeDefault(game, path);
        entry.ExePath = path;
        games.Add(entry);
        return entry;
    }

    private void RemoveGame(string gameId)
    {
        if (NucleusCore.SessionRunning && string.Equals(GameProfile.Game?.GUID, gameId, StringComparison.OrdinalIgnoreCase))
        {
            throw new BridgeException(BridgeErrorKind.Conflict, I18n.T("host.library.errors.sessionRunning"), new { reason = "sessionRunning" });
        }

        int removed = Manager.User.Games.RemoveAll(g => string.Equals(g.GameGuid, gameId, StringComparison.OrdinalIgnoreCase));
        if (removed == 0)
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.errors.gameNotFound"), new { gameId });
        }

        _ = core.UserProfile.SaveAsync();
        GameAssetFiles.Delete(gameId);
        core.Icons.Delete(gameId);
        ServiceLog.Info($"removed {gameId} from the library");
        core.Notifier.GamesChanged(gameId);
    }

    private static List<(string GameId, string HandlerId)> ArtworkTargets() =>
        LibraryEntries()
            .Where(e => HubClient.IsValidHubId(e.Game.Hub?.Handler?.Id))
            .Select(e => (e.Game.GUID, e.Game.Hub.Handler.Id))
            .ToList();

    private async Task DownloadArtworkCoreAsync(List<(string GameId, string HandlerId)> targets, bool throwWhenAllFail, CancellationToken cancellationToken)
    {
        await artworkQueue.WaitAsync(cancellationToken);
        try
        {
            int failures = 0;
            Exception? lastError = null;
            for (int i = 0; i < targets.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                (string gameId, string handlerId) = targets[i];
                core.Events.Emit(new ArtworkProgress { GameId = gameId, Done = i, Total = targets.Count });
                try
                {
                    if (await core.Artwork.DownloadAsync(gameId, handlerId, cancellationToken).ConfigureAwait(false))
                    {
                        core.Notifier.GamesChanged(gameId);
                    }
                }
                catch (Exception e) when (!(e is OperationCanceledException && cancellationToken.IsCancellationRequested))
                {
                    failures++;
                    lastError = e;
                    ServiceLog.Warn($"artwork of {gameId} could not be downloaded", e);
                }
            }

            core.Events.Emit(new ArtworkProgress { GameId = targets[targets.Count - 1].GameId, Done = targets.Count, Total = targets.Count });

            if (throwWhenAllFail && failures == targets.Count && lastError != null)
            {
                core.HubStatus.Report(!(lastError is HubUnavailableException));
                throw new BridgeException(BridgeErrorKind.Unavailable, I18n.T("host.library.errors.artworkFailed", new { error = lastError.Message }), new { reason = "downloadFailed" });
            }
        }
        finally
        {
            artworkQueue.Release();
        }
    }

    private void DownloadArtworkInBackground(string gameId)
    {
        List<(string GameId, string HandlerId)> targets = ArtworkTargets().Where(t => t.GameId == gameId).ToList();
        if (targets.Count == 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                if (await core.HubStatus.EnsureOnlineAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    await DownloadArtworkCoreAsync(targets, throwWhenAllFail: false, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn($"artwork download of {gameId} failed", e);
            }
        });
    }

    private void CheckUpdateInBackground(string gameId)
    {
        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets(new[] { gameId });
        if (targets.Count == 0)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                if (await core.HubStatus.EnsureOnlineAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    await core.Updates.CheckAsync(targets, throwWhenOffline: false, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn($"update check of {gameId} failed", e);
            }
        });
    }
}

/// <summary>exFAT check of <c>UserDriveInfo.IsExFat</c>, without its message boxes.</summary>
public static class DriveFormat
{
    /// <summary>Whether the path is on an exFAT volume (no symbolic links). False when unknown (network paths…).</summary>
    public static bool IsExFat(string path)
    {
        try
        {
            string? root = Path.GetPathRoot(Path.GetFullPath(path));
            if (string.IsNullOrEmpty(root) || root!.StartsWith(@"\\", StringComparison.Ordinal))
            {
                return false;
            }

            return string.Equals(new DriveInfo(root).DriveFormat, "exFAT", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception)
        {
            return false;
        }
    }
}
