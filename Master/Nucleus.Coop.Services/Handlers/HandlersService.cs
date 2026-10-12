using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Library;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Handlers;

/// <summary>
/// <c>handlers.*</c>: installs handlers from the hub or a file, updates them, checks for updates. Replaces
/// <c>DownloadPrompt</c> (download + extraction in a modal form), the drag &amp; drop code of <c>UI_Functions</c> and
/// <c>UI_Functions.Button_UpdateAvailable_Click</c>. Runs on the UI thread; downloads and extraction run on workers.
/// </summary>
public sealed class HandlersService : IHandlersService, IHandlerUpdater
{
    private readonly NucleusCore core;
    private readonly SemaphoreSlim installQueue = new SemaphoreSlim(1, 1);

    /// <summary>Creates the service.</summary>
    public HandlersService(NucleusCore core)
    {
        this.core = core;
    }

    private static GameManager Manager => GameManager.Instance;

    /// <inheritdoc />
    public List<InstalledHandler> ListInstalled()
    {
        var library = new HashSet<string>(Manager.User.Games.Select(g => g.GameGuid), StringComparer.OrdinalIgnoreCase);
        return Manager.Games.Values
            .OrderBy(g => g.GameName, StringComparer.CurrentCultureIgnoreCase)
            .Select(g => new InstalledHandler
            {
                GameId = g.GUID,
                GameName = g.GameName ?? g.GUID,
                HandlerId = string.IsNullOrWhiteSpace(g.Hub?.Handler?.Id) ? null : g.Hub!.Handler.Id,
                Version = g.Hub?.Handler != null && g.Hub.Handler.Version >= 0 ? g.Hub.Handler.Version : (int?)null,
                InLibrary = library.Contains(g.GUID),
            })
            .ToList();
    }

    /// <inheritdoc />
    public Task<InstallResult> Install(InstallHandlerRequest request, CancellationToken cancellationToken)
    {
        string handlerId = request?.HandlerId?.Trim() ?? "";
        if (!HubClient.IsValidHubId(handlerId))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "handlerId is not a valid hub handler id.");
        }

        return InstallFromHubAsync(handlerId, null, cancellationToken);
    }

    /// <inheritdoc />
    public Task<InstallResult> Update(GameRequest request, CancellationToken cancellationToken) => UpdateAsync(request?.GameId ?? "", cancellationToken);

    /// <inheritdoc />
    public async Task<InstallResult> InstallFromFile(InstallFromFileRequest request)
    {
        string path = request?.Path?.Trim().Trim('"') ?? "";
        if (path.Length == 0)
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "path is required.");
        }

        if (!File.Exists(path))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, "The file does not exist.", new { path });
        }

        string extension = Path.GetExtension(path).ToLowerInvariant();
        if (extension != ".nc" && extension != ".zip" && extension != ".js")
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "Unsupported file: use a handler archive (.nc, .zip) or a handler script (.js).", new { reason = "unsupportedFile" });
        }

        RefuseDuringSession();
        await installQueue.WaitAsync();
        string? work = null;
        try
        {
            string handlers = Manager.GetJsScriptsPath();
            InstalledHandlerFiles installed;

            if (extension == ".js")
            {
                HandlerScriptInfo info = HandlerScript.ParseFile(path);
                if (!info.IsHandler)
                {
                    throw new BridgeException(BridgeErrorKind.InvalidParams, "This file is not a Nucleus handler (no Game.GameName or Game.ExecutableName).", new { reason = "notAHandler" });
                }

                string title = Path.GetFileNameWithoutExtension(path);
                string? replaced = CheckConflict(handlers, title, info.Guid, request!.Overwrite);
                Progress(path, null, HandlerInstallStage.Extracting, null);
                installed = await Task.Run(() =>
                {
                    RemoveReplacedHandler(handlers, replaced, title);
                    return HandlerFiles.InstallScript(path, handlers);
                });
            }
            else
            {
                work = NewWorkFolder(handlers);
                Progress(path, null, HandlerInstallStage.Extracting, 0);
                var progress = new Progress<int>(p => Progress(path, null, HandlerInstallStage.Extracting, p));
                ExtractedHandler extracted = await Task.Run(() => HandlerFiles.Extract(path, work, progress, CancellationToken.None));
                string? replaced = CheckConflict(handlers, extracted.Title, extracted.Script.Guid, request!.Overwrite);
                installed = await Task.Run(() =>
                {
                    RemoveReplacedHandler(handlers, replaced, extracted.Title);
                    return HandlerFiles.Install(extracted, handlers);
                });
            }

            InstallResult result = LoadInstalled(installed, hubVersion: null);
            Progress(path, result.GameId, HandlerInstallStage.Done, 100);
            return result;
        }
        catch (InvalidHandlerPackageException e)
        {
            Progress(path, null, HandlerInstallStage.Failed, null, e.Message);
            throw new BridgeException(BridgeErrorKind.InvalidParams, e.Message, new { reason = "invalidPackage" });
        }
        catch (BridgeException e) when (e.Kind == BridgeErrorKind.Conflict)
        {
            Progress(path, null, HandlerInstallStage.Failed, null, e.Message);
            throw;
        }
        catch (Exception e)
        {
            Progress(path, null, HandlerInstallStage.Failed, null, e.Message);
            throw;
        }
        finally
        {
            if (work != null)
            {
                HandlerFiles.TryDeleteFolder(work);
            }

            installQueue.Release();
        }
    }

    /// <inheritdoc />
    public async Task CheckUpdates()
    {
        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets();
        if (targets.Count == 0)
        {
            return;
        }

        try
        {
            await core.Updates.CheckAsync(targets, throwWhenOffline: true, CancellationToken.None);
        }
        catch (HubUnavailableException e)
        {
            throw new BridgeException(BridgeErrorKind.Unavailable, "hub.splitscreen.me can't be reached: " + e.Message, new { reason = "offline" });
        }
    }

    /// <inheritdoc />
    public async Task<List<string>> Screenshots(HubHandlerRequest request)
    {
        string handlerId = request?.HandlerId?.Trim() ?? "";
        if (!HubClient.IsValidHubId(handlerId))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "handlerId is not a valid hub handler id.");
        }

        try
        {
            List<string> urls = await core.Hub.GetScreenshotUrlsAsync(handlerId, CancellationToken.None);
            core.HubStatus.Report(true);
            return urls.Select(u => HubClient.ScreenshotUrl(u, "t_screenshot_big")).ToList();
        }
        catch (HubUnavailableException e)
        {
            core.HubStatus.Report(false);
            throw new BridgeException(BridgeErrorKind.Unavailable, "hub.splitscreen.me can't be reached: " + e.Message, new { reason = "offline" });
        }
    }

    // ---- IHandlerUpdater ----

    /// <inheritdoc />
    public bool IsUpdateAvailable(string gameId) =>
        Manager.Games.TryGetValue(gameId, out GenericGameInfo game) && game.UpdateAvailable && game.MetaInfo.CheckUpdate;

    /// <inheritdoc />
    public int? LatestVersion(string gameId) => core.Updates.LatestVersion(gameId);

    /// <inheritdoc />
    public async Task<bool?> CheckAsync(string gameId, CancellationToken cancellationToken)
    {
        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets(new[] { gameId });
        if (targets.Count == 0)
        {
            return null;
        }

        try
        {
            await core.Updates.CheckAsync(targets, throwWhenOffline: true, cancellationToken);
            return IsUpdateAvailable(gameId);
        }
        catch (HubUnavailableException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public Task<InstallResult> UpdateAsync(string gameId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(gameId))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "gameId is required.");
        }

        if (!Manager.Games.TryGetValue(gameId, out GenericGameInfo game))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, "Game not found.", new { gameId });
        }

        string? handlerId = game.Hub?.Handler?.Id;
        if (!HubClient.IsValidHubId(handlerId))
        {
            throw new BridgeException(BridgeErrorKind.Conflict, "This handler does not come from the hub, so it can't be updated from there.", new { reason = "notFromHub" });
        }

        return InstallFromHubAsync(handlerId!, game.GUID, cancellationToken);
    }

    // ---- install core ----

    private async Task<InstallResult> InstallFromHubAsync(string handlerId, string? gameId, CancellationToken cancellationToken)
    {
        RefuseDuringSession();
        await installQueue.WaitAsync(cancellationToken);
        string? work = null;
        try
        {
            Progress(handlerId, gameId, HandlerInstallStage.Downloading, 0);
            HubHandlerInfo handler;
            try
            {
                handler = await core.Hub.GetHandlerAsync(handlerId, cancellationToken)
                    ?? throw new BridgeException(BridgeErrorKind.NotFound, "The hub does not know this handler.", new { handlerId });
                core.HubStatus.Report(true);
            }
            catch (HubUnavailableException e)
            {
                core.HubStatus.Report(false);
                throw new BridgeException(BridgeErrorKind.Unavailable, "hub.splitscreen.me can't be reached: " + e.Message, new { reason = "offline" });
            }

            if (handler.CurrentVersion < 0 || string.IsNullOrEmpty(handler.CurrentPackage))
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "This handler has no published package on the hub.", new { handlerId });
            }

            string handlers = Manager.GetJsScriptsPath();
            work = NewWorkFolder(handlers);
            string archive = Path.Combine(work, $"handler-{handler.Id}-v{handler.CurrentVersion}.nc");

            // ?download=true: the hub counts real installs.
            var downloadProgress = new Progress<int?>(p => Progress(handlerId, gameId, HandlerInstallStage.Downloading, p));
            try
            {
                await core.Hub.DownloadToFileAsync(HubClient.PackageUrl(handler, countDownload: true), archive, downloadProgress, cancellationToken);
            }
            catch (HubUnavailableException e)
            {
                throw new BridgeException(BridgeErrorKind.Unavailable, e.Message, new { reason = "downloadFailed" });
            }

            Progress(handlerId, gameId, HandlerInstallStage.Extracting, 0);
            var extractProgress = new Progress<int>(p => Progress(handlerId, gameId, HandlerInstallStage.Extracting, p));
            string extractFolder = Path.Combine(work, "files");
            ExtractedHandler extracted = await Task.Run(() => HandlerFiles.Extract(archive, extractFolder, extractProgress, cancellationToken), cancellationToken);

            // A hub install replaces the installed version of the same game (as the legacy update did, without asking).
            string? replaced = CheckConflict(handlers, extracted.Title, extracted.Script.Guid, overwrite: true);
            InstalledHandlerFiles installed = await Task.Run(() =>
            {
                RemoveReplacedHandler(handlers, replaced, extracted.Title);
                return HandlerFiles.Install(extracted, handlers);
            });

            InstallResult result = LoadInstalled(installed, handler.CurrentVersion);
            Progress(handlerId, gameId ?? result.GameId, HandlerInstallStage.Done, 100);
            return result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Progress(handlerId, gameId, HandlerInstallStage.Failed, null, "Cancelled.");
            throw;
        }
        catch (InvalidHandlerPackageException e)
        {
            Progress(handlerId, gameId, HandlerInstallStage.Failed, null, e.Message);
            throw new BridgeException(BridgeErrorKind.Conflict, e.Message, new { reason = "invalidPackage" });
        }
        catch (Exception e)
        {
            Progress(handlerId, gameId, HandlerInstallStage.Failed, null, e is BridgeException ? e.Message : "The handler could not be installed: " + e.Message);
            throw;
        }
        finally
        {
            if (work != null)
            {
                HandlerFiles.TryDeleteFolder(work);
            }

            installQueue.Release();
        }
    }

    /// <summary>
    /// Conflict rules of an install: an existing <c>&lt;title&gt;.js</c>, or another installed script with the same
    /// game GUID (it would load as a duplicate). Returns the file name (without extension) of a script with the same
    /// GUID that the install replaces, or null.
    /// </summary>
    private static string? CheckConflict(string handlers, string title, string? guid, bool overwrite)
    {
        GenericGameInfo? sameGame = string.IsNullOrEmpty(guid) ? null : Manager.Games.Values.FirstOrDefault(g => string.Equals(g.GUID, guid, StringComparison.OrdinalIgnoreCase));
        string? sameGameTitle = sameGame?.JsFileName == null ? null : Path.GetFileNameWithoutExtension(sameGame.JsFileName);
        bool titleExists = File.Exists(HandlerFiles.ScriptPathFor(handlers, title));

        if (!overwrite && (titleExists || sameGameTitle != null))
        {
            string name = sameGame?.GameName ?? title;
            throw new BridgeException(BridgeErrorKind.Conflict, $"A handler for {name} is already installed.", new { reason = "alreadyInstalled", gameId = sameGame?.GUID, gameName = name });
        }

        return sameGameTitle != null && !string.Equals(sameGameTitle, title, StringComparison.OrdinalIgnoreCase) ? sameGameTitle : null;
    }

    private static void RemoveReplacedHandler(string handlers, string? replacedTitle, string newTitle)
    {
        if (replacedTitle != null && !string.Equals(replacedTitle, newTitle, StringComparison.OrdinalIgnoreCase))
        {
            HandlerFiles.DeleteHandlerFiles(handlers, replacedTitle);
            ServiceLog.Info($"removed {replacedTitle}.js, replaced by {newTitle}.js (same game)");
        }
    }

    // Back on the UI thread: parse the new script (GameManager.AddScript), keep the library entry, report.
    private InstallResult LoadInstalled(InstalledHandlerFiles installed, int? hubVersion)
    {
        GameManager manager = Manager;
        Dictionary<string, string> pathsBefore = manager.User.Games
            .Where(g => !string.IsNullOrEmpty(g.GameGuid) && g.ExePath != null)
            .GroupBy(g => g.GameGuid, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().ExePath, StringComparer.OrdinalIgnoreCase);

        GenericGameInfo? info = manager.AddScript(installed.Title, new[] { false, false });
        if (info == null || string.IsNullOrEmpty(info.GUID))
        {
            throw new BridgeException(BridgeErrorKind.Conflict, "The handler was installed but could not be loaded: it has errors.", new { reason = "invalidHandler", file = installed.Title + ".js" });
        }

        // The same file may have declared another GUID before: that game's parsed handler is gone from disk.
        foreach (string stale in manager.Games.Values
                     .Where(g => !ReferenceEquals(g, info) && string.Equals(g.JsFileName, installed.Title + ".js", StringComparison.OrdinalIgnoreCase))
                     .Select(g => g.GUID).ToList())
        {
            manager.Games.Remove(stale);
            manager.GameInfos.Remove(stale);
        }

        // AddScript replaces the library entry through InitializeDefault, which lowercases every 'I' of the path.
        UserGameInfo? entry = manager.User.Games.FirstOrDefault(g => string.Equals(g.GameGuid, info.GUID, StringComparison.OrdinalIgnoreCase));
        if (entry != null)
        {
            if (pathsBefore.TryGetValue(info.GUID, out string original) && string.Equals(original, entry.ExePath, StringComparison.OrdinalIgnoreCase))
            {
                entry.ExePath = original;
            }

            _ = core.UserProfile.SaveAsync();
        }

        if (hubVersion.HasValue)
        {
            core.Updates.SetLatestVersion(info.GUID, hubVersion.Value);
        }

        ServiceLog.Info($"installed handler {installed.Title}.js ({info.GUID}, version {info.Hub?.Handler?.Version})");
        core.Events.Emit(new HandlersChanged { GameIds = new List<string> { info.GUID } });
        core.Notifier.GamesChanged(info.GUID);
        LibraryHooks.RaiseHandlersReplaced(new List<string> { info.GUID });

        return new InstallResult
        {
            GameId = info.GUID,
            GameName = info.GameName ?? installed.Title,
            ExecutableName = info.ExecutableNames?.Length > 0 ? info.ExecutableNames[0] : info.ExecutableName,
            ExecutableNames = info.ExecutableNames?.Length > 0
                ? info.ExecutableNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList()
                : string.IsNullOrWhiteSpace(info.ExecutableName) ? new List<string>() : new List<string> { info.ExecutableName },
            InLibrary = entry != null,
            SuggestedFolder = entry == null ? SuggestFolder(info) : null,
        };
    }

    private static string? SuggestFolder(GenericGameInfo info)
    {
        if (string.IsNullOrWhiteSpace(info.SteamID))
        {
            return null;
        }

        try
        {
            // HKLM ...\Uninstall\Steam App <id> InstallLocation (+ Game.BinariesFolder).
            string folder = Manager.AutoSearchGameInstallPath(info);
            return !string.IsNullOrWhiteSpace(folder) && Directory.Exists(folder) ? folder : null;
        }
        catch (Exception)
        {
            return null; // the key exists without InstallLocation: legacy NRE
        }
    }

    private static string NewWorkFolder(string handlers)
    {
        string folder = Path.Combine(handlers, ".install-" + Guid.NewGuid().ToString("N").Substring(0, 8));
        DirectoryInfo directory = Directory.CreateDirectory(folder);
        try
        {
            directory.Attributes |= FileAttributes.Hidden;
        }
        catch (Exception)
        {
        }

        return folder;
    }

    private static void RefuseDuringSession()
    {
        if (NucleusCore.SessionRunning)
        {
            throw new BridgeException(BridgeErrorKind.Conflict, "Stop the running game session before installing handlers.", new { reason = "sessionRunning" });
        }
    }

    private void Progress(string source, string? gameId, HandlerInstallStage stage, int? percent, string? error = null)
    {
        try
        {
            core.Events.Emit(new HandlerProgress { Source = source, GameId = gameId, Stage = stage, Percent = percent, Error = error });
        }
        catch (Exception e)
        {
            ServiceLog.Warn("handlers.progress could not be sent", e);
        }
    }
}
