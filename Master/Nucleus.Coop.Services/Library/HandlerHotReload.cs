using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// Handler hot reload (Nucleus 2.5): a script edited, added or deleted in <c>handlers\</c> is picked up without a
/// restart. <see cref="HandlersWatcher"/> reports file events; the scripts are parsed on a worker
/// (<see cref="GameManager.ScanHandlerFiles"/>) and swapped in on the UI thread (<see cref="GameManager.ApplyHandlerScan"/>).
/// Nothing is applied while a session runs: changes wait for its end. Reactions to <see cref="GameManager.HandlersChanged"/>
/// (any source, the library "reload" included): broken scripts become handler notices (once per error), games whose
/// script was deleted leave the library, the UI gets <c>handlers.changed</c> / <c>library.changed</c>, and the setup is
/// told through <see cref="LibraryHooks.HandlersReplaced"/>.
/// </summary>
internal sealed class HandlerHotReload : IDisposable
{
    private const int SessionRetryMs = 5000;

    private readonly NucleusCore core;
    private readonly GameManager manager;
    private readonly SemaphoreSlim scanning = new SemaphoreSlim(1, 1);
    private readonly Timer retry;
    private HandlersWatcher? watcher;
    private bool disposed;
    private bool applyingFolderChanges; // UI thread only

    public HandlerHotReload(NucleusCore core, GameManager manager)
    {
        this.core = core;
        this.manager = manager;
        retry = new Timer(_ => _ = RunAsync(), null, Timeout.Infinite, Timeout.Infinite);
        manager.HandlersChanged += OnHandlersChanged;
    }

    /// <summary>Starts watching the handlers folder.</summary>
    public void Start()
    {
        try
        {
            watcher = new HandlersWatcher(manager.GetJsScriptsPath());
            watcher.Changed += () => _ = RunAsync();
            ServiceLog.Info("handlers: watching " + manager.GetJsScriptsPath());
        }
        catch (Exception e)
        {
            ServiceLog.Warn("handlers: the folder can't be watched, scripts reload on restart only", e);
        }
    }

    private static bool SessionBusy => NucleusCore.SessionRunning || GenericGameHandler.IsRunning;

    /// <summary>Scans the handlers folder and applies the changes on the UI thread (now, or after the session).</summary>
    public async Task RunAsync()
    {
        if (disposed)
        {
            return;
        }

        await scanning.WaitAsync().ConfigureAwait(false);

        try
        {
            if (SessionBusy)
            {
                retry.Change(SessionRetryMs, Timeout.Infinite);
                return;
            }

            HandlerScan scan = await Task.Run(() => manager.ScanHandlerFiles()).ConfigureAwait(false);

            if (scan.IsEmpty)
            {
                return;
            }

            await core.Ui.InvokeAsync(() =>
            {
                if (SessionBusy)
                {
                    retry.Change(SessionRetryMs, Timeout.Infinite);
                    return;
                }

                // HandlersChanged is raised inside ApplyHandlerScan, on this thread: the UI is told these changes came from the folder.
                applyingFolderChanges = true;
                try
                {
                    manager.ApplyHandlerScan(scan);
                }
                finally
                {
                    applyingFolderChanges = false;
                }
            }).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            ServiceLog.Warn("handlers: hot reload failed", e);
        }
        finally
        {
            scanning.Release();
        }
    }

    // UI thread (ApplyHandlerScan runs there).
    private void OnHandlersChanged(HandlerChanges changes)
    {
        foreach (HandlerInfo invalid in changes.Invalid)
        {
            ReportInvalid(invalid);
        }

        List<string> removedGames = new List<string>();

        foreach (UserGameInfo orphan in changes.Orphaned.ToList())
        {
            if (LibraryService.RemoveFromLibrary(core, orphan.GameGuid))
            {
                removedGames.Add(orphan.GameGuid);
            }
        }

        if (changes.Rebound.Count > 0 && removedGames.Count == 0)
        {
            _ = core.UserProfile.SaveAsync();//script paths of the rebound entries
        }

        ServiceLog.Info($"handlers: {changes.Added.Count} added, {changes.Updated.Count} updated, {changes.Fixed.Count} fixed, {changes.Invalid.Count} with errors, {changes.Removed.Count} removed" +
                        (removedGames.Count > 0 ? $"; removed from the library: {string.Join(", ", removedGames)}" : ""));

        if (changes.GameIds.Count == 0)
        {
            // A script that broke, or a broken one deleted, changes no game but the list of handlers with errors.
            if (changes.Any)
            {
                core.Events.Emit(new HandlersChanged());
            }

            return;
        }

        List<ReloadedHandler> reloaded = applyingFolderChanges ? Describe(changes, id => manager.Games.ContainsKey(id)) : new List<ReloadedHandler>();
        core.Events.Emit(new HandlersChanged { GameIds = changes.GameIds.ToList(), Reloaded = reloaded.Count > 0 ? reloaded : null });
        core.Notifier.GamesChanged(changes.GameIds);
        LibraryHooks.RaiseHandlersReplaced(changes.GameIds.ToList());

        CheckUpdatesInBackground(changes.Added.Concat(changes.Updated).Concat(changes.Fixed).Select(h => h.GUID).Where(id => !string.IsNullOrEmpty(id)).ToList());
    }

    /// <summary>
    /// The handlers the UI names in a short message: added, edited and fixed scripts, and deleted ones whose game is gone
    /// (<paramref name="stillLoaded"/>: another file may declare the same GUID). Broken scripts are notices instead.
    /// </summary>
    internal static List<ReloadedHandler> Describe(HandlerChanges changes, Func<string, bool> stillLoaded)
    {
        var reloaded = new List<ReloadedHandler>();

        void Add(IEnumerable<HandlerInfo> infos, HandlerReloadKind kind)
        {
            foreach (HandlerInfo info in infos)
            {
                string? id = info.GUID;
                if (string.IsNullOrEmpty(id) || reloaded.Any(r => string.Equals(r.GameId, id, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                if (kind == HandlerReloadKind.Removed && stillLoaded(id!))
                {
                    continue;
                }

                reloaded.Add(new ReloadedHandler { GameId = id!, GameName = info.GameName ?? info.FileName ?? id!, Kind = kind });
            }
        }

        Add(changes.Fixed, HandlerReloadKind.Fixed);
        Add(changes.Updated, HandlerReloadKind.Updated);
        Add(changes.Added, HandlerReloadKind.Added);
        Add(changes.Removed, HandlerReloadKind.Removed);
        return reloaded;
    }

    private static void ReportInvalid(HandlerInfo invalid)
    {
        string title = I18n.T("engine.handlers.scriptError.title");
        string message = invalid.GenericGameInfo != null
            ? I18n.T("host.handlers.reload.keptPrevious", new { file = invalid.FileName, details = invalid.Error })
            : invalid.Error ?? I18n.T("engine.handlers.invalid.unreadable");

        //Not on the UI thread: the native fallback would block it.
        Task.Run(() => UserPrompts.Notice(title, message, false, PromptOrigin.Handler, PromptIcon.Error));
    }

    private void CheckUpdatesInBackground(List<string> gameIds)
    {
        if (gameIds.Count == 0)
        {
            return;
        }

        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets(gameIds);

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
                ServiceLog.Warn("update check after a handler reload failed", e);
            }
        });
    }

    public void Dispose()
    {
        disposed = true;
        manager.HandlersChanged -= OnHandlersChanged;
        watcher?.Dispose();
        retry.Dispose();
    }
}
