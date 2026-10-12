using Nucleus.Gaming;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Coop.Host
{
    /// <summary>
    /// Where the web UI comes from and how the WebView2 is configured, resolved once at startup.
    /// Debug builds may load a Vite dev server and expose a remote debugging port; Release builds never do.
    /// </summary>
    internal sealed class HostEnvironment
    {
        public const string UiHost = "ui.nucleus.example";
        public const string AssetsHost = "assets.nucleus.example";
        public const string HandlersHost = "handlers.nucleus.example";
        public const string UiOrigin = "https://" + UiHost;
        public const string StartUrl = UiOrigin + "/index.html";

        /// <summary>Environment variable holding the remote debugging port (Debug builds only).</summary>
        public const string DebugPortVariable = "NUCLEUS_WEBVIEW_DEBUG_PORT";

        /// <summary>Text file next to the built UI that holds the dev server URL (Debug builds only).</summary>
        public const string DevServerFileName = "dev-server.txt";

        private HostEnvironment()
        {
        }

        /// <summary>True in Debug builds: dev tools, context menu, reload accelerators, dev server, remote debugging.</summary>
        public bool IsDebug { get; private set; }

        public string InstallFolder { get; private set; }

        /// <summary>Built UI (<c>webui\</c>), served as https://ui.nucleus.example/.</summary>
        public string WebUiFolder { get; private set; }

        /// <summary>Local images (<c>gui\</c>), served as https://assets.nucleus.example/.</summary>
        public string AssetsFolder { get; private set; }

        /// <summary>Installed handlers (<c>handlers\</c>, as GameManager.GetJsScriptsPath), served as https://handlers.nucleus.example/ for handler-provided images.</summary>
        public string HandlersFolder { get; private set; }

        /// <summary>
        /// WebView2 user data folder of the web UI (<c>webview\ui</c>). Separate from the legacy hub view's <c>webview\cache</c>,
        /// which runs with browser extensions enabled: two environments with different options cannot share a folder.
        /// </summary>
        public string UserDataFolder { get; private set; }

        /// <summary>First page loaded in the WebView.</summary>
        public Uri StartUri { get; private set; }

        /// <summary>Origin of <see cref="StartUri"/>: the only origin allowed to talk to the bridge.</summary>
        public string StartOrigin => DevServerOrigin ?? UiOrigin;

        /// <summary>Origin of the Vite dev server when <see cref="StartUri"/> points to it, otherwise null.</summary>
        public string DevServerOrigin { get; private set; }

        /// <summary>Origins the WebView may navigate to and that may talk to the bridge.</summary>
        public IReadOnlyList<string> AppOrigins { get; private set; }

        /// <summary>Remote debugging port, or null.</summary>
        public int? RemoteDebuggingPort { get; private set; }

        public static HostEnvironment Resolve()
        {
            string install = InstallPaths.Root;

            HostEnvironment env = new HostEnvironment
            {
#if DEBUG
                IsDebug = true,
#endif
                InstallFolder = install,
                WebUiFolder = Path.Combine(install, "webui"),
                AssetsFolder = Path.Combine(install, "gui"),
                // Same folder as GameManager.Instance.GetJsScriptsPath(), which needs a GameManager instance.
                HandlersFolder = Path.Combine(install, "handlers"),
                UserDataFolder = Path.Combine(install, "webview", "ui"),
                StartUri = new Uri(StartUrl),
            };

            List<string> origins = new List<string> { UiOrigin };

            if (env.IsDebug)
            {
                Uri devServer = ReadDevServerUri(env.WebUiFolder);
                if (devServer != null)
                {
                    env.StartUri = devServer;
                    env.DevServerOrigin = GetOrigin(devServer);
                    origins.Add(env.DevServerOrigin);
                }

                env.RemoteDebuggingPort = ReadDebugPort();
            }

            env.AppOrigins = origins.AsReadOnly();
            return env;
        }

        /// <summary>Scheme, host and port of an absolute URI, for example "https://ui.nucleus.example".</summary>
        public static string GetOrigin(Uri uri)
        {
            return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
        }

        /// <summary>Whether the URI belongs to the UI (virtual host or dev server).</summary>
        public bool IsAppUri(string uri)
        {
            Uri parsed;
            if (!Uri.TryCreate(uri, UriKind.Absolute, out parsed))
            {
                return false;
            }

            if (parsed.Scheme != Uri.UriSchemeHttp && parsed.Scheme != Uri.UriSchemeHttps)
            {
                return false;
            }

            string origin = GetOrigin(parsed);
            return AppOrigins.Any(o => string.Equals(o, origin, StringComparison.OrdinalIgnoreCase));
        }

        private static Uri ReadDevServerUri(string webUiFolder)
        {
            string file = Path.Combine(webUiFolder, DevServerFileName);
            if (!File.Exists(file))
            {
                return null;
            }

            try
            {
                string line = File.ReadAllLines(file).Select(l => l.Trim()).FirstOrDefault(l => l.Length > 0);
                Uri uri;
                if (line != null && Uri.TryCreate(line, UriKind.Absolute, out uri) &&
                    (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
                {
                    HostLog.Info($"Loading the UI from the dev server {uri} ({file}).");
                    return uri;
                }

                HostLog.Warning($"Ignoring {file}: it must contain an absolute http(s) URL.");
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not read {file}: {ex.Message}");
            }

            return null;
        }

        private static int? ReadDebugPort()
        {
            string value = Environment.GetEnvironmentVariable(DebugPortVariable);
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            int port;
            if (int.TryParse(value.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out port) && port > 0 && port < 65536)
            {
                return port;
            }

            HostLog.Warning($"Ignoring {DebugPortVariable}={value}: not a TCP port.");
            return null;
        }
    }
}
