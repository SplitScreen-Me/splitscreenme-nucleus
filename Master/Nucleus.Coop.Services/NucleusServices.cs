using System;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;

namespace Nucleus.Coop.Services
{
    /// <summary>Registers the app services on the bridge. Called once by the host's composition root, on the UI thread.</summary>
    public static class NucleusServices
    {
        /// <summary>Registers library, handlers, settings, setup, profiles and session services.</summary>
        /// <param name="bridge">Bridge to register the services on.</param>
        /// <param name="shell">Host window.</param>
        public static void Register(BridgeHost bridge, IHostShell shell)
        {
            // Languages first: the page needs its strings before the library has loaded.
            bridge.Register<Bridge.Contracts.II18nService>(new Localization.I18nService(bridge.Events));

            // Out-of-game engine messages (handler errors at startup included) go to the web UI from now on.
            var notices = new Notices.NoticeBoard(bridge.Events);
            Nucleus.Gaming.Forms.UserPrompts.Handler = new Notices.WebUserPrompts(notices, () => shell.PageState, Nucleus.Gaming.Forms.UserPrompts.Native);
            bridge.Register<Bridge.Contracts.INoticesService>(notices);

            // Library, handlers, settings, setup, profiles and session all use GameManager (every handler + the
            // library). It loads in the background while the window and the page start; the services are created on
            // the UI thread once it is ready, and calls made meanwhile wait for them without blocking the UI thread.
            var ui = new Startup.UiDispatcher(shell.UiContext);
            Task<AppServices> services = Startup.NucleusCore.StartAsync(ui, bridge.Events)
                .ContinueWith(t => ui.InvokeAsync(() => AppServices.Create(t.GetAwaiter().GetResult(), bridge, shell, notices)),
                    CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default)
                .Unwrap();

            bridge.RegisterDeferred(Select<Bridge.Contracts.ILibraryService>(services, s => s.Library));
            bridge.RegisterDeferred(Select<Bridge.Contracts.IHandlersService>(services, s => s.Handlers));
            bridge.RegisterDeferred(Select<Bridge.Contracts.ISettingsService>(services, s => s.Settings));
            bridge.RegisterDeferred(Select<Bridge.Contracts.ISetupService>(services, s => s.Setup));
            bridge.RegisterDeferred(Select<Bridge.Contracts.IProfilesService>(services, s => s.Profiles));
            bridge.RegisterDeferred(Select<Bridge.Contracts.ISessionService>(services, s => s.Session));
        }

        private static Task<T> Select<T>(Task<AppServices> services, Func<AppServices, T> pick) =>
            services.ContinueWith(t => pick(t.GetAwaiter().GetResult()), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

        /// <summary>The services that need the library, created together on the UI thread once it has loaded.</summary>
        private sealed class AppServices
        {
            public Bridge.Contracts.ILibraryService Library = null!;
            public Bridge.Contracts.IHandlersService Handlers = null!;
            public Bridge.Contracts.ISettingsService Settings = null!;
            public Bridge.Contracts.ISetupService Setup = null!;
            public Bridge.Contracts.IProfilesService Profiles = null!;
            public Bridge.Contracts.ISessionService Session = null!;

            public static AppServices Create(Startup.NucleusCore core, BridgeHost bridge, IHostShell shell, Notices.NoticeBoard notices)
            {
                var handlersService = new Handlers.HandlersService(core);
                Handlers.IHandlerUpdater handlerUpdater = handlersService; // for the setup's "handler up to date" check before a launch

                // Setup, profiles and session: one shared headless setup state (device timers post to the UI thread).
                var setupSession = new Setup.SetupSession(bridge.Events, shell) { HandlerUpdater = handlerUpdater };

                return new AppServices
                {
                    Library = new Library.LibraryService(core),
                    Handlers = handlersService,
                    Settings = new Settings.SettingsService(core),
                    Setup = new Setup.SetupService(setupSession),
                    Profiles = new Profiles.ProfilesService(setupSession),
                    Session = new Session.SessionService(setupSession, bridge.Events, shell, notices),
                };
            }
        }
    }
}
