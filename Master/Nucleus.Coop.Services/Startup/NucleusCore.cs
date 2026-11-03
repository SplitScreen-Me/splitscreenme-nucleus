using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Library;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Startup;

/// <summary>
/// App-level state shared by the library, handlers and settings services, created once at startup on the UI thread.
/// Replaces what the legacy MainForm constructor did for the library: <c>new GameManager()</c> (parses every handler
/// with Jint, loads userprofile.json), and the hub connectivity flag (<c>Hub.Connected</c>).
/// </summary>
public sealed class NucleusCore : IDisposable
{
    private NucleusCore(UiDispatcher ui, IBridgeEvents events)
    {
        Ui = ui;
        Events = events;
        Hub = new HubClient();
        ServiceLog.Info("hub: " + Hub.SiteUrl + (HubSite.IsCustom ? $" (Settings.ini [{HubSite.SettingSection}] {HubSite.SettingKey})" : ""));
        HubStatus = new HubStatus(Hub, events);
        Notifier = new LibraryNotifier(events);
        UserProfile = new UserProfileStore();
        Icons = new GameIconCache(Notifier);
        Artwork = new ArtworkDownloader(Hub);
        Updates = new HandlerUpdateChecker(Hub, HubStatus, ui, Notifier);
    }

    /// <summary>UI thread dispatcher.</summary>
    public UiDispatcher Ui { get; }

    /// <summary>Bridge events.</summary>
    public IBridgeEvents Events { get; }

    /// <summary>Hub HTTP client.</summary>
    public HubClient Hub { get; }

    /// <summary>Hub connectivity.</summary>
    public HubStatus HubStatus { get; }

    /// <summary>Coalesced <c>library.changed</c>.</summary>
    public LibraryNotifier Notifier { get; }

    /// <summary>userprofile.json writer.</summary>
    public UserProfileStore UserProfile { get; }

    /// <summary>Icons rendered for the UI.</summary>
    public GameIconCache Icons { get; }

    /// <summary>Covers and screenshots.</summary>
    public ArtworkDownloader Artwork { get; }

    /// <summary>Handler update checks.</summary>
    public HandlerUpdateChecker Updates { get; }

    /// <summary>Handler scripts reloaded when their file changes (Nucleus 2.5), set once the core is created.</summary>
    internal HandlerHotReload? HotReload { get; private set; }

    /// <summary>Automatic search of game executables in the user's game folders (Nucleus 2.5).</summary>
    public GameAutoSearch AutoSearch { get; } = new GameAutoSearch();

    /// <summary>Whether a game session is running (handlers must not be replaced meanwhile).</summary>
    public static bool SessionRunning
    {
        get
        {
            GenericGameHandler? handler = GenericGameHandler.Instance;
            return handler != null && !handler.HasEnded;
        }
    }

    /// <summary>
    /// Loads the handlers and the library on a background thread, so the window and the WebView start meanwhile,
    /// then creates the core on the UI thread and probes the hub and checks handler updates in the background.
    /// </summary>
    public static Task<NucleusCore> StartAsync(UiDispatcher ui, IBridgeEvents events)
    {
        var loaded = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        // STA: handler errors may still open a native dialog when no page is live yet (UserPrompts fallback).
        var thread = new Thread(() =>
        {
            try
            {
                Stopwatch clock = Stopwatch.StartNew();
                // Parses handlers\*.js (Jint), loads content\userprofile.json, registers raw input (on its own thread).
                // LoadUser runs no update check: StartBackgroundWork runs one and publishes its results.
                _ = new GameManager();
                clock.Stop();
                GameManager manager = GameManager.Instance;
                ServiceLog.Info($"GameManager loaded {manager.Games.Count} handler(s) and {manager.User?.Games?.Count ?? 0} library game(s) in {clock.ElapsedMilliseconds} ms (background)");
                loaded.SetResult(true);
            }
            catch (Exception e)
            {
                ServiceLog.Error("GameManager failed to load", e);
                loaded.SetException(e);
            }
        })
        {
            IsBackground = true,
            Name = "Nucleus library load",
        };

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();

        return loaded.Task.ContinueWith(t =>
        {
            t.GetAwaiter().GetResult(); // rethrows the load failure
            return ui.InvokeAsync(() =>
            {
                var core = new NucleusCore(ui, events);
                LibraryHooks.Attach(core.Notifier);
                GameMetaInfo.Saved += core.OnMetaInfoSaved;
                core.HotReload = new HandlerHotReload(core, GameManager.Instance);
                core.HotReload.Start();
                core.StartBackgroundWork();
                return core;
            });
        }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default).Unwrap();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GameMetaInfo.Saved -= OnMetaInfoSaved;
        HubStatus.Changed -= OnHubStatusChanged;
        HotReload?.Dispose();
        HubStatus.Dispose();
        Notifier.Dispose();
        Hub.Dispose();
    }

    // metaInfo.json saves: favourite, toggles, and the play time written every 20 s during a session and once more
    // when it ends. Any thread.
    private void OnMetaInfoSaved(string gameId) => Notifier.GamesChanged(gameId);

    // The hub came back: the update flags were hidden while it was offline and may be stale, check them again (Nucleus
    // 2.5 re-checked updates once the hub was back). Not during a session: the next status change, or the user, does it.
    private void OnHubStatusChanged(bool online)
    {
        if (!online || SessionRunning || GenericGameHandler.IsRunning)
        {
            return;
        }

        Task.Run(async () =>
        {
            try
            {
                var targets = await Ui.InvokeAsync(() => HandlerUpdateChecker.LibraryTargets()).ConfigureAwait(false);
                await Updates.CheckAsync(targets, throwWhenOffline: false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ServiceLog.Warn("update check after the hub came back failed", e);
            }
        });
    }

    private void StartBackgroundWork()
    {
        //Nucleus 2.5: the hub status follows the connection by itself (not during a session).
        HubStatus.Changed += OnHubStatusChanged;
        HubStatus.StartMonitoring(() => SessionRunning || GenericGameHandler.IsRunning);

        // Hub.Connected starts false: a hub that answers raises Changed(true), which checks the handler updates.
        Task.Run(async () =>
        {
            try
            {
                await HubStatus.CheckNowAsync(force: true).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ServiceLog.Warn("startup hub check failed", e);
            }
        });
    }
}
