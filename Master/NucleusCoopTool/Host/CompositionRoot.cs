using Microsoft.Web.WebView2.Core;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Host.Services;
using Nucleus.Coop.Services;
using System;
using System.Threading;

namespace Nucleus.Coop.Host
{
    /// <summary>
    /// Builds the bridge for the web UI: creates the <see cref="BridgeHost"/> on the UI thread, registers the host
    /// services and the app services, and attaches a <see cref="WebView2Transport"/> to each CoreWebView2 the window creates.
    /// </summary>
    internal sealed class CompositionRoot : IDisposable
    {
        private readonly HostForm form;
        private readonly HostEnvironment environment;
        private WebView2Transport transport;

        /// <summary>Must run on the UI thread, after the host form is constructed.</summary>
        public CompositionRoot(HostForm form, HostEnvironment environment)
        {
            this.form = form;
            this.environment = environment;

            SynchronizationContext uiContext = SynchronizationContext.Current;
            if (uiContext == null)
            {
                throw new InvalidOperationException("The composition root must be created on the UI thread.");
            }

            // Events are raised on the UI thread; WebView2 delivers requests there too, so services run on the UI thread.
            Bridge = new BridgeHost(uiContext, LogBridgeEntry);

            // Host services: they drive the window itself.
            Bridge.Register<IAppService>(new AppService(environment, Bridge.ContractHash, () => form.WebViewVersion, form.CloseWindow));
            Bridge.Register<IWindowService>(new WindowService(form, Bridge.Events));
            Bridge.Register<IDialogsService>(new DialogsService(form));

            RegisterAppServices(Bridge, new HostShell(form, uiContext));

            form.CoreWebView2Ready += OnCoreWebView2Ready;
            form.CoreWebView2Closing += OnCoreWebView2Closing;
        }

        public BridgeHost Bridge { get; }

        /// <summary>
        /// EXTENSION POINT for Master\Nucleus.Coop.Services: library, handlers, settings, setup, profiles and session
        /// services register themselves in <see cref="NucleusServices.Register"/>. Runs once, on the UI thread, before
        /// the first page load.
        /// </summary>
        private static void RegisterAppServices(BridgeHost bridge, IHostShell shell)
        {
            NucleusServices.Register(bridge, shell);
        }

        private void OnCoreWebView2Ready(object sender, CoreWebView2 core)
        {
            DetachTransport();

            // https://ui.nucleus.example, or the Vite dev server origin in Debug when webui\dev-server.txt is used.
            transport = new WebView2Transport(core, Bridge, environment.StartOrigin);
        }

        private void OnCoreWebView2Closing(object sender, CoreWebView2 core)
        {
            DetachTransport();
        }

        private void DetachTransport()
        {
            WebView2Transport old = transport;
            transport = null;
            old?.Dispose();
        }

        private static void LogBridgeEntry(BridgeLogEntry entry)
        {
            switch (entry.Level)
            {
                case BridgeLogLevel.Debug:
                    // Every call is logged at Debug level: only with debug logging on.
                    if (Nucleus.Gaming.App.Settings.App_Misc.DebugLog)
                    {
                        HostLog.Write("DEBUG", "[bridge] " + entry);
                    }
                    break;
                case BridgeLogLevel.Info:
                    HostLog.Write("INFO", "[bridge] " + entry);
                    break;
                case BridgeLogLevel.Warning:
                    HostLog.Write("WARN", "[bridge] " + entry);
                    break;
                default:
                    HostLog.Write("ERROR", "[bridge] " + entry);
                    break;
            }
        }

        public void Dispose()
        {
            form.CoreWebView2Ready -= OnCoreWebView2Ready;
            form.CoreWebView2Closing -= OnCoreWebView2Closing;
            DetachTransport();
        }
    }
}
