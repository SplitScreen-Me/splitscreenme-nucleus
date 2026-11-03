using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>
    /// Installed handlers: install and update from the hub (package checked against the SHA-256 of the API v3), install from a file, screenshots.
    /// The catalogue, handler pages and comments are read by the UI directly from the public hub API v3.
    /// </summary>
    [BridgeService("handlers")]
    public interface IHandlersService
    {
        /// <summary>Handlers installed on this PC.</summary>
        List<InstalledHandler> ListInstalled();

        /// <summary>
        /// Downloads a handler from the hub and installs it (replacing an older version). Progress comes as <see cref="HandlerProgress"/>.
        /// Throws <c>conflict</c> (reason <c>checksumMismatch</c>) when the package does not match the SHA-256 published by the hub.
        /// </summary>
        Task<InstallResult> Install(InstallHandlerRequest request, CancellationToken cancellationToken);

        /// <summary>Updates the handler of a library game to the hub's current version.</summary>
        Task<InstallResult> Update(GameRequest request, CancellationToken cancellationToken);

        /// <summary>Installs a handler from a .nc, .zip or .js file. Throws <c>conflict</c> (reason <c>alreadyInstalled</c>) when it exists and <see cref="InstallFromFileRequest.Overwrite"/> is false.</summary>
        Task<InstallResult> InstallFromFile(InstallFromFileRequest request);

        /// <summary>Checks the hub for handler updates of every library game. Results arrive through <c>library.changed</c>.</summary>
        Task CheckUpdates();

        /// <summary>Screenshot URLs (IGDB) of a hub handler's game, from the handler detail of the hub API.</summary>
        Task<List<string>> Screenshots(HubHandlerRequest request);

        /// <summary>Handler scripts of the handlers folder that can't be used as they are now (script error, no game name or GUID), by file name.</summary>
        List<HandlerFileError> ListErrors();
    }

    /// <summary>A handler script that can't be used as it is now.</summary>
    public sealed class HandlerFileError
    {
        /// <summary>Script file name, for example "Elden Ring.js".</summary>
        public string File { get; set; } = "";

        /// <summary>Game name of the version in use, when an earlier version of the file loaded.</summary>
        public string? GameName { get; set; }

        /// <summary>Game id of the version in use, when an earlier version of the file loaded.</summary>
        public string? GameId { get; set; }

        /// <summary>
        /// The previous version of the script is still in use (it loaded before the file was broken), so its game keeps working
        /// until the file is fixed. False when the file never loaded.
        /// </summary>
        public bool PreviousVersionInUse { get; set; }

        /// <summary>What is wrong, as the script engine or Nucleus reports it (not translated when it comes from the script engine).</summary>
        public string Error { get; set; } = "";
    }

    /// <summary>How a handler changed when its script file was reloaded.</summary>
    public enum HandlerReloadKind
    {
        /// <summary>A new script was added to the handlers folder.</summary>
        Added,
        /// <summary>The script was edited.</summary>
        Updated,
        /// <summary>A broken script was fixed.</summary>
        Fixed,
        /// <summary>The script was deleted; its game left the library.</summary>
        Removed,
    }

    /// <summary>A handler reloaded from its script file.</summary>
    public sealed class ReloadedHandler
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Game name.</summary>
        public string GameName { get; set; } = "";

        /// <summary>What happened.</summary>
        public HandlerReloadKind Kind { get; set; }
    }

    /// <summary>Identifies a handler on the hub.</summary>
    public sealed class HubHandlerRequest
    {
        /// <summary>Hub handler id (the <c>id</c> of the hub API, the legacy 17-character id).</summary>
        public string HandlerId { get; set; } = "";
    }

    /// <summary>A handler installed on this PC.</summary>
    public sealed class InstalledHandler
    {
        /// <summary>Game id (handler GUID).</summary>
        public string GameId { get; set; } = "";

        /// <summary>Game name.</summary>
        public string GameName { get; set; } = "";

        /// <summary>Hub handler id, when it comes from the hub.</summary>
        public string? HandlerId { get; set; }

        /// <summary>Installed version, when known.</summary>
        public int? Version { get; set; }

        /// <summary>Whether a game uses it in the library.</summary>
        public bool InLibrary { get; set; }
    }

    /// <summary>Parameters of <see cref="IHandlersService.Install"/>.</summary>
    public sealed class InstallHandlerRequest
    {
        /// <summary>Hub handler id.</summary>
        public string HandlerId { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="IHandlersService.InstallFromFile"/>.</summary>
    public sealed class InstallFromFileRequest
    {
        /// <summary>Path of the .nc, .zip or .js file.</summary>
        public string Path { get; set; } = "";

        /// <summary>Replace an installed handler for the same game.</summary>
        public bool Overwrite { get; set; }
    }

    /// <summary>Result of an install or update.</summary>
    public sealed class InstallResult
    {
        /// <summary>Game id of the installed handler.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Game name.</summary>
        public string GameName { get; set; } = "";

        /// <summary>Executable the handler expects, for example "eldenring.exe".</summary>
        public string? ExecutableName { get; set; }

        /// <summary>Every executable name the handler accepts (for the picker filter).</summary>
        public List<string> ExecutableNames { get; set; } = new List<string>();

        /// <summary>Whether the game is already in the library (then nothing else to do).</summary>
        public bool InLibrary { get; set; }

        /// <summary>Install folder found automatically (Steam), to start the executable picker in.</summary>
        public string? SuggestedFolder { get; set; }
    }

    /// <summary>Stage of a handler install.</summary>
    public enum HandlerInstallStage
    {
        /// <summary>Downloading the package.</summary>
        Downloading,
        /// <summary>Extracting files.</summary>
        Extracting,
        /// <summary>Installed.</summary>
        Done,
        /// <summary>Failed (see <see cref="HandlerProgress.Error"/>).</summary>
        Failed,
    }

    /// <summary>Progress of an install or update.</summary>
    [BridgeEvent("handlers.progress")]
    public sealed class HandlerProgress
    {
        /// <summary>Hub handler id, or the file path for installs from a file.</summary>
        public string Source { get; set; } = "";

        /// <summary>Library game being updated, for updates.</summary>
        public string? GameId { get; set; }

        /// <summary>Stage.</summary>
        public HandlerInstallStage Stage { get; set; }

        /// <summary>Percentage of the stage, 0 to 100, when known.</summary>
        public int? Percent { get; set; }

        /// <summary>Error message when failed.</summary>
        public string? Error { get; set; }
    }

    /// <summary>Installed handlers changed.</summary>
    [BridgeEvent("handlers.changed", Invalidates = new[] { "handlers.listInstalled", "handlers.listErrors", "library.list", "library.get" })]
    public sealed class HandlersChanged
    {
        /// <summary>Game ids of the handlers that changed.</summary>
        public List<string> GameIds { get; set; } = new List<string>();

        /// <summary>
        /// Handlers picked up from the handlers folder by themselves (a script added, edited, fixed or deleted while Nucleus runs), for a
        /// short message. Absent for installs, updates and the library reload, which say so themselves, and for broken scripts, which
        /// are notices.
        /// </summary>
        public List<ReloadedHandler>? Reloaded { get; set; }
    }
}
