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
            // Library, handlers, settings. Runs first: loads GameManager (every handler + the library), which the
            // other services use through GameManager.Instance.
            Startup.NucleusCore core = Startup.NucleusCore.Start(shell.UiContext, bridge.Events);
            var handlersService = new Handlers.HandlersService(core);
            Handlers.IHandlerUpdater handlerUpdater = handlersService; // for the setup's "handler up to date" check before a launch
            bridge.Register<Bridge.Contracts.ILibraryService>(new Library.LibraryService(core));
            bridge.Register<Bridge.Contracts.IHandlersService>(handlersService);
            bridge.Register<Bridge.Contracts.ISettingsService>(new Settings.SettingsService(core));

            // Setup, profiles and session: one shared headless setup state (device timers post to the UI thread).
            var setupSession = new Setup.SetupSession(bridge.Events, shell) { HandlerUpdater = handlerUpdater };
            bridge.Register<Bridge.Contracts.ISetupService>(new Setup.SetupService(setupSession));
            bridge.Register<Bridge.Contracts.IProfilesService>(new Profiles.ProfilesService(setupSession));
            bridge.Register<Bridge.Contracts.ISessionService>(new Session.SessionService(setupSession, bridge.Events, shell));
        }
    }
}
