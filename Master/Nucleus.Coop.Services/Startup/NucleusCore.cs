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
    /// Loads the handlers and the library (synchronously, on the calling UI thread, as the MainForm constructor did),
    /// then probes the hub and checks handler updates in the background.
    /// </summary>
    public static NucleusCore Start(SynchronizationContext? uiContext, IBridgeEvents events)
    {
        var ui = new UiDispatcher(uiContext);

        Stopwatch clock = Stopwatch.StartNew();
        // Parses handlers\*.js (Jint), loads content\userprofile.json, registers raw input. LoadUser runs no update
        // check: StartBackgroundWork runs one and publishes its results.
        _ = new GameManager();
        clock.Stop();
        GameManager manager = GameManager.Instance;
        ServiceLog.Info($"GameManager loaded {manager.Games.Count} handler(s) and {manager.User?.Games?.Count ?? 0} library game(s) in {clock.ElapsedMilliseconds} ms");

        var core = new NucleusCore(ui, events);
        LibraryHooks.Attach(core.Notifier);
        GameMetaInfo.Saved += core.OnMetaInfoSaved;
        core.StartBackgroundWork();
        return core;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        GameMetaInfo.Saved -= OnMetaInfoSaved;
        Notifier.Dispose();
        Hub.Dispose();
    }

    // metaInfo.json saves: favourite, toggles, and the play time written every 20 s during a session and once more
    // when it ends. Any thread.
    private void OnMetaInfoSaved(string gameId) => Notifier.GamesChanged(gameId);

    private void StartBackgroundWork()
    {
        Task.Run(async () =>
        {
            try
            {
                bool online = await HubStatus.ProbeAsync(CancellationToken.None).ConfigureAwait(false);
                if (!online)
                {
                    return;
                }

                var targets = await Ui.InvokeAsync(() => HandlerUpdateChecker.LibraryTargets()).ConfigureAwait(false);
                await Updates.CheckAsync(targets, throwWhenOffline: false, CancellationToken.None).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                ServiceLog.Warn("startup hub check failed", e);
            }
        });
    }
}
