using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace Nucleus.Coop.Host.Services
{
    /// <summary>Handshake, app information, external links and UI logging.</summary>
    internal sealed class AppService : IAppService
    {
        private const int MaxLogMessageLength = 4000;

        private const int ERROR_CANCELLED = 1223;

        private readonly HostEnvironment environment;
        private readonly string contractHash;
        private readonly Func<string> webViewVersion;
        private readonly Action quit;
        private readonly Lazy<bool> isAdmin = new Lazy<bool>(IsElevated);

        /// <param name="environment">Host paths and build flavour.</param>
        /// <param name="contractHash">Hash of the contract the bridge serves (<see cref="BridgeHost.ContractHash"/>).</param>
        /// <param name="webViewVersion">Returns the version of the WebView2 runtime in use.</param>
        /// <param name="quit">Closes the host window, which quits the app (safe to call from a WebView2 event).</param>
        public AppService(HostEnvironment environment, string contractHash, Func<string> webViewVersion, Action quit)
        {
            this.environment = environment;
            this.contractHash = contractHash;
            this.webViewVersion = webViewVersion;
            this.quit = quit;
        }

        public HelloResponse Hello()
        {
            return new HelloResponse
            {
                ProtocolVersion = BridgeHost.ProtocolVersion,
                ContractHash = contractHash,
                AppVersion = Globals.Version,
                Debug = environment.IsDebug,
            };
        }

        public AppInfo GetInfo()
        {
            return new AppInfo
            {
                AppVersion = Globals.Version,
                InstallPath = environment.InstallFolder,
                HandlersPath = environment.HandlersFolder,
                HubUrl = HubSite.Url,
                HubOnline = Nucleus.Gaming.Coop.Generic.Hub.Connected, // live: kept current by the services (app.hubStatusChanged)
                WebViewVersion = webViewVersion() ?? "",
                IsAdmin = isAdmin.Value,
                StartupGameId = HostStartup.GameId,
            };
        }

        /// <summary>Port of CheckGameRequirements.RestartAsAdmin: relaunch elevated with the same arguments, then quit.</summary>
        public void RestartAsAdmin()
        {
            IGameHandler handler = GameProfile.I_GameHandler;
            if (handler != null && !handler.HasEnded)
            {
                throw new BridgeException(BridgeErrorKind.Conflict, "Stop the running session before restarting Nucleus Co-op.");
            }

            ProcessStartInfo startInfo = new ProcessStartInfo(Application.ExecutablePath)
            {
                UseShellExecute = true,
                Verb = "runas",
                WorkingDirectory = environment.InstallFolder,
                Arguments = string.Join(" ", HostStartup.ForwardedArgs.Select(QuoteArgument)),
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception ex) when (ex.NativeErrorCode == ERROR_CANCELLED)
            {
                throw new BridgeException(BridgeErrorKind.Unavailable, "Administrator rights were not granted.", new { reason = "declined" });
            }
            catch (Exception ex)
            {
                HostLog.Error("Restarting as administrator failed.", ex);
                throw new BridgeException(BridgeErrorKind.Unavailable, "Nucleus Co-op could not be restarted as administrator.");
            }

            // The new instance waits a second before its single-instance check (StartChecks.IsAlreadyRunning).
            HostLog.Info("Restarting as administrator.");
            quit();
        }

        /// <summary>Quotes one argument for the Windows command line (CommandLineToArgvW rules).</summary>
        internal static string QuoteArgument(string argument)
        {
            if (string.IsNullOrEmpty(argument))
            {
                return "\"\"";
            }

            if (argument.IndexOfAny(new[] { ' ', '\t', '"' }) < 0)
            {
                return argument;
            }

            StringBuilder builder = new StringBuilder("\"");
            int backslashes = 0;

            foreach (char c in argument)
            {
                if (c == '\\')
                {
                    backslashes++;
                    continue;
                }

                builder.Append('\\', c == '"' ? backslashes * 2 + 1 : backslashes);
                backslashes = 0;
                builder.Append(c);
            }

            builder.Append('\\', backslashes * 2).Append('"');
            return builder.ToString();
        }

        public void OpenExternal(OpenExternalRequest request)
        {
            Uri uri = ExternalLauncher.ParseWebUrl(request?.Url);
            if (uri == null)
            {
                throw new BridgeException(BridgeErrorKind.InvalidParams, "Only absolute http and https URLs can be opened.", new { url = request?.Url });
            }

            if (!ExternalLauncher.TryOpenUrl(uri.AbsoluteUri))
            {
                throw new BridgeException(BridgeErrorKind.Unavailable, "The default browser could not be started.");
            }
        }

        public void OpenPath(OpenPathRequest request)
        {
            string path = ExternalLauncher.NormalizeLocalPath(request?.Path);
            if (path == null)
            {
                throw new BridgeException(BridgeErrorKind.InvalidParams, "The path must be an absolute local path.", new { path = request?.Path });
            }

            if (!File.Exists(path) && !Directory.Exists(path))
            {
                throw new BridgeException(BridgeErrorKind.NotFound, "This file or folder does not exist.", new { path });
            }

            try
            {
                ExternalLauncher.OpenInExplorer(path);
            }
            catch (Exception ex)
            {
                HostLog.Warning($"Could not open {path} in File Explorer: {ex.Message}");
                throw new BridgeException(BridgeErrorKind.Unavailable, "File Explorer could not be started.", new { path });
            }
        }

        public void Log(LogRequest request)
        {
            if (request == null)
            {
                return;
            }

            // Debug lines from the page only reach the log in Debug builds or when debug logging is on.
            if (request.Level == LogLevel.Debug && !environment.IsDebug && !App_Misc.DebugLog)
            {
                return;
            }

            string message = request.Message ?? "";
            if (message.Length > MaxLogMessageLength)
            {
                message = message.Substring(0, MaxLogMessageLength) + "…";
            }

            string source = string.IsNullOrWhiteSpace(request.Source) ? "page" : "page/" + request.Source.Trim();
            HostLog.Write(ToLogLevel(request.Level), $"[{source}] {message}");
        }

        private static string ToLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.Debug: return "DEBUG";
                case LogLevel.Warning: return "WARN";
                case LogLevel.Error: return "ERROR";
                default: return "INFO";
            }
        }

        private static bool IsElevated()
        {
            try
            {
                using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
                {
                    return new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
