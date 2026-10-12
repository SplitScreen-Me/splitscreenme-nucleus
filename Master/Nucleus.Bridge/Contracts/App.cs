using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>Application-level information and actions.</summary>
    [BridgeService("app")]
    public interface IAppService
    {
        /// <summary>Handshake. The UI calls it first and refuses to run when <see cref="HelloResponse.ContractHash"/> differs from its own.</summary>
        HelloResponse Hello();

        /// <summary>Versions, paths and connectivity.</summary>
        AppInfo GetInfo();

        /// <summary>Opens an http(s) URL in the default browser. Other schemes are rejected.</summary>
        void OpenExternal(OpenExternalRequest request);

        /// <summary>Opens a folder, or selects a file, in File Explorer. The path must exist.</summary>
        void OpenPath(OpenPathRequest request);

        /// <summary>Writes a line from the UI to the app log.</summary>
        void Log(LogRequest request);

        /// <summary>Restarts Nucleus as administrator (some handlers require it). The current process exits.</summary>
        void RestartAsAdmin();
    }

    /// <summary>Answer to <see cref="IAppService.Hello"/>.</summary>
    public sealed class HelloResponse
    {
        /// <summary>Version of the wire protocol (envelope, error codes). Changes only on protocol changes.</summary>
        public int ProtocolVersion { get; set; }

        /// <summary>Hash of the contract the host was built with.</summary>
        public string ContractHash { get; set; } = "";

        /// <summary>Nucleus Co-op version, for example "2.4.2".</summary>
        public string AppVersion { get; set; } = "";

        /// <summary>True in Debug builds: dev tools and diagnostics are available.</summary>
        public bool Debug { get; set; }
    }

    /// <summary>Versions, paths and connectivity.</summary>
    public sealed class AppInfo
    {
        /// <summary>Nucleus Co-op version.</summary>
        public string AppVersion { get; set; } = "";

        /// <summary>Folder that contains NucleusCoop.exe.</summary>
        public string InstallPath { get; set; } = "";

        /// <summary>Folder that contains the installed handlers.</summary>
        public string HandlersPath { get; set; } = "";

        /// <summary>Whether hub.splitscreen.me is reachable (last check; see <see cref="HubStatusChanged"/>).</summary>
        public bool HubOnline { get; set; }

        /// <summary>Version of the WebView2 runtime in use.</summary>
        public string WebViewVersion { get; set; } = "";

        /// <summary>Whether the process runs elevated.</summary>
        public bool IsAdmin { get; set; }

        /// <summary>Game to open at startup (passed on the command line, for example by a desktop shortcut).</summary>
        public string? StartupGameId { get; set; }
    }

    /// <summary>hub.splitscreen.me became reachable or unreachable.</summary>
    [BridgeEvent("app.hubStatusChanged", Invalidates = new[] { "app.getInfo" })]
    public sealed class HubStatusChanged
    {
        /// <summary>Whether the hub is reachable.</summary>
        public bool Online { get; set; }
    }

    /// <summary>Parameters of <see cref="IAppService.OpenExternal"/>.</summary>
    public sealed class OpenExternalRequest
    {
        /// <summary>Absolute http or https URL.</summary>
        public string Url { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="IAppService.OpenPath"/>.</summary>
    public sealed class OpenPathRequest
    {
        /// <summary>Absolute path of an existing folder or file.</summary>
        public string Path { get; set; } = "";
    }

    /// <summary>Severity of a log line.</summary>
    public enum LogLevel
    {
        /// <summary>Diagnostics.</summary>
        Debug,
        /// <summary>Normal operation.</summary>
        Info,
        /// <summary>Something unexpected that the app recovered from.</summary>
        Warning,
        /// <summary>A failure.</summary>
        Error,
    }

    /// <summary>Parameters of <see cref="IAppService.Log"/>.</summary>
    public sealed class LogRequest
    {
        /// <summary>Severity.</summary>
        public LogLevel Level { get; set; }

        /// <summary>Message text.</summary>
        public string Message { get; set; } = "";

        /// <summary>Where it comes from in the UI, for example a component name.</summary>
        public string? Source { get; set; }
    }
}
