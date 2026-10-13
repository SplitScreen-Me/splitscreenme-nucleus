using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nucleus.Bridge.Contracts
{
    /// <summary>The user's games: list, details, favourites, the per-game menu, adding and removing games, artwork.</summary>
    [BridgeService("library")]
    public interface ILibraryService
    {
        /// <summary>Every game in the library, in no particular order (the UI sorts and filters).</summary>
        List<GameSummary> List();

        /// <summary>Everything the game page shows.</summary>
        GameDetails Get(GameRequest request);

        /// <summary>Marks or unmarks a game as favourite.</summary>
        void SetFavorite(SetFavoriteRequest request);

        /// <summary>The per-game menu, with the items that apply to this game right now.</summary>
        GameMenu GetMenu(GameRequest request);

        /// <summary>Runs a menu item. Items with <see cref="GameMenuItem.Confirm"/> must be confirmed by the user first.</summary>
        Task RunMenuAction(RunMenuActionRequest request);

        /// <summary>Adds a game from its executable. When several handlers match, returns them and adds nothing: call again with <see cref="AddGameRequest.HandlerGameId"/>.</summary>
        Task<AddGameResult> AddGame(AddGameRequest request);

        /// <summary>Points a game at a new executable (when the game was moved). The executable must match the handler.</summary>
        void SetExecutable(SetExecutableRequest request);

        /// <summary>Removes a game from the library (the handler stays installed).</summary>
        void Remove(GameRequest request);

        /// <summary>Games installed through Steam and Epic on this PC. The UI matches them with the hub catalogue to offer ready-made handlers.</summary>
        Task<List<InstalledGame>> ScanInstalledGames();

        /// <summary>Looks for an executable by name under a folder (for example a Steam install folder). Returns null when not found.</summary>
        Task<string?> FindExecutable(FindExecutableRequest request);

        /// <summary>Downloads cover and screenshots of one game, or of every game when <see cref="DownloadArtworkRequest.GameId"/> is null. Progress comes as <see cref="ArtworkProgress"/>.</summary>
        Task DownloadArtwork(DownloadArtworkRequest request, System.Threading.CancellationToken cancellationToken);

        /// <summary>Reloads every handler from disk.</summary>
        Task Reload();

        /// <summary>Saved sorting and filtering of the game list.</summary>
        LibraryPreferences GetPreferences();

        /// <summary>Saves sorting and filtering of the game list.</summary>
        void SetPreferences(LibraryPreferences preferences);
    }

    /// <summary>Inputs a handler supports.</summary>
    public sealed class GameInputs
    {
        /// <summary>XInput gamepads.</summary>
        public bool XInput { get; set; }

        /// <summary>DirectInput gamepads.</summary>
        public bool DInput { get; set; }

        /// <summary>One keyboard and mouse player.</summary>
        public bool Keyboard { get; set; }

        /// <summary>Several keyboards and mice.</summary>
        public bool MultipleKeyboards { get; set; }
    }

    /// <summary>A game in the library, as listed.</summary>
    public sealed class GameSummary
    {
        /// <summary>Game id (handler GUID).</summary>
        public string Id { get; set; } = "";

        /// <summary>Game name.</summary>
        public string Name { get; set; } = "";

        /// <summary>Hub handler id, when the handler comes from the hub.</summary>
        public string? HandlerId { get; set; }

        /// <summary>Installed handler version, when known.</summary>
        public int? HandlerVersion { get; set; }

        /// <summary>Maximum number of players.</summary>
        public int MaxPlayers { get; set; }

        /// <summary>Supported inputs.</summary>
        public GameInputs Inputs { get; set; } = new GameInputs();

        /// <summary>Favourite.</summary>
        public bool Favorite { get; set; }

        /// <summary>A newer handler version is on the hub (and update checks are on for this game).</summary>
        public bool UpdateAvailable { get; set; }

        /// <summary>Version on the hub, when an update check has run.</summary>
        public int? LatestHandlerVersion { get; set; }

        /// <summary>Wide background picture (first downloaded screenshot), when available.</summary>
        public string? HeroUrl { get; set; }

        /// <summary>Icon URL (exe icon or custom icon, served by the assets host).</summary>
        public string? IconUrl { get; set; }

        /// <summary>Cover URL, when downloaded.</summary>
        public string? CoverUrl { get; set; }

        /// <summary>Total play time in seconds.</summary>
        public long PlaytimeSeconds { get; set; }

        /// <summary>Last session (UTC).</summary>
        public DateTime? LastPlayedAt { get; set; }

        /// <summary>The game has never been launched from Nucleus: the UI shows the handler notes first.</summary>
        public bool FirstLaunch { get; set; }

        /// <summary>False when the executable is missing.</summary>
        public bool ExecutableFound { get; set; }
    }

    /// <summary>Everything the game page shows.</summary>
    public sealed class GameDetails
    {
        /// <summary>Summary fields.</summary>
        public GameSummary Summary { get; set; } = new GameSummary();

        /// <summary>Handler notes, raw. Markup: <c>*text*</c> highlight, <c>*text|r,g,b*</c> coloured, <c>[label]{url}</c> link; plain URLs are links.</summary>
        public string? Notes { get; set; }

        /// <summary>Screenshot URLs, when downloaded.</summary>
        public List<string> ScreenshotUrls { get; set; } = new List<string>();

        /// <summary>Path of the game executable.</summary>
        public string ExecutablePath { get; set; } = "";

        /// <summary>Executable architecture: "x64", "x86" or "unknown".</summary>
        public string Architecture { get; set; } = "unknown";

        /// <summary>Handler file name.</summary>
        public string HandlerFile { get; set; } = "";

        /// <summary>Handler maintainer name, when the handler declares it.</summary>
        public string? Maintainer { get; set; }

        /// <summary>Steam app id, when known.</summary>
        public string? SteamId { get; set; }

        /// <summary>Folder holding the game instances.</summary>
        public string ContentFolder { get; set; } = "";

        /// <summary>Whether profiles are enabled for this game (globally and per game).</summary>
        public bool ProfilesEnabled { get; set; }

        /// <summary>Number of saved profiles.</summary>
        public int ProfileCount { get; set; }

        /// <summary>Whether the handler has options or custom steps.</summary>
        public bool HasOptions { get; set; }

        /// <summary>Whether the notes should open by themselves (first launch, unless disabled in settings).</summary>
        public bool ShowNotesFirst { get; set; }
    }

    /// <summary>Parameters of <see cref="ILibraryService.SetFavorite"/>.</summary>
    public sealed class SetFavoriteRequest
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>New value.</summary>
        public bool Favorite { get; set; }
    }

    /// <summary>Kind of menu item.</summary>
    public enum GameMenuItemKind
    {
        /// <summary>Runs an action.</summary>
        Action,
        /// <summary>On/off setting (<see cref="GameMenuItem.Checked"/>).</summary>
        Toggle,
        /// <summary>Pick one value in <see cref="GameMenuItem.Choices"/>.</summary>
        Choice,
        /// <summary>Opens <see cref="GameMenuItem.Children"/>.</summary>
        Submenu,
        /// <summary>Visual separator.</summary>
        Separator,
    }

    /// <summary>An item of the per-game menu.</summary>
    public sealed class GameMenuItem
    {
        /// <summary>Id passed to <see cref="ILibraryService.RunMenuAction"/>. Stable for static items, for example <c>openHandler</c>, <c>deleteContentFolder</c>, <c>keepInstancesFolder</c>; dynamic items carry their target, for example <c>openBackup:Player 2</c>.</summary>
        public string Id { get; set; } = "";

        /// <summary>Kind.</summary>
        public GameMenuItemKind Kind { get; set; }

        /// <summary>Label.</summary>
        public string Label { get; set; } = "";

        /// <summary>Longer explanation (tooltip).</summary>
        public string? Description { get; set; }

        /// <summary>State of a toggle.</summary>
        public bool? Checked { get; set; }

        /// <summary>Values of a choice item.</summary>
        public List<string> Choices { get; set; } = new List<string>();

        /// <summary>Labels of <see cref="Choices"/>, in the same order, in the language in use. Empty: the values are shown as they are.</summary>
        public List<string> ChoiceLabels { get; set; } = new List<string>();

        /// <summary>Current value of a choice item.</summary>
        public string? Value { get; set; }

        /// <summary>Destructive action: shown in red.</summary>
        public bool Danger { get; set; }

        /// <summary>When set, the question the user must confirm before running the item.</summary>
        public string? Confirm { get; set; }

        /// <summary>Items of a submenu.</summary>
        public List<GameMenuItem> Children { get; set; } = new List<GameMenuItem>();

        /// <summary>When set, the UI does something before (or instead of) calling <see cref="ILibraryService.RunMenuAction"/>.</summary>
        public GameMenuClientAction? ClientAction { get; set; }

        /// <summary>File types for <see cref="GameMenuClientAction.PickFile"/>.</summary>
        public List<FileFilter> FileFilters { get; set; } = new List<FileFilter>();
    }

    /// <summary>What the UI does itself for a menu item.</summary>
    public enum GameMenuClientAction
    {
        /// <summary>Shows the handler notes (from <see cref="GameDetails.Notes"/>); no host call.</summary>
        ShowNotes,
        /// <summary>Shows the game details (from <see cref="GameDetails"/>); no host call.</summary>
        ShowDetails,
        /// <summary>Opens the file picker, then calls <see cref="ILibraryService.RunMenuAction"/> with the picked path as the value.</summary>
        PickFile,
    }

    /// <summary>The per-game menu.</summary>
    public sealed class GameMenu
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Items, in order.</summary>
        public List<GameMenuItem> Items { get; set; } = new List<GameMenuItem>();
    }

    /// <summary>Parameters of <see cref="ILibraryService.RunMenuAction"/>.</summary>
    public sealed class RunMenuActionRequest
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Item id.</summary>
        public string ItemId { get; set; } = "";

        /// <summary>New value, for choice items.</summary>
        public string? Value { get; set; }
    }

    /// <summary>Parameters of <see cref="ILibraryService.AddGame"/>.</summary>
    public sealed class AddGameRequest
    {
        /// <summary>Path of the game executable (from <c>dialogs.pickFile</c>).</summary>
        public string ExecutablePath { get; set; } = "";

        /// <summary>Handler to use when several match the executable.</summary>
        public string? HandlerGameId { get; set; }
    }

    /// <summary>A handler that can run an executable.</summary>
    public sealed class HandlerCandidate
    {
        /// <summary>Game id of the handler.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Game name.</summary>
        public string Name { get; set; } = "";
    }

    /// <summary>Result of <see cref="ILibraryService.AddGame"/>.</summary>
    public sealed class AddGameResult
    {
        /// <summary>The added game, when it was added.</summary>
        public GameSummary? Game { get; set; }

        /// <summary>Handlers to choose from when several match. Empty when the game was added.</summary>
        public List<HandlerCandidate> Candidates { get; set; } = new List<HandlerCandidate>();
    }

    /// <summary>Parameters of <see cref="ILibraryService.SetExecutable"/>.</summary>
    public sealed class SetExecutableRequest
    {
        /// <summary>Game id.</summary>
        public string GameId { get; set; } = "";

        /// <summary>New executable path.</summary>
        public string ExecutablePath { get; set; } = "";
    }

    /// <summary>Store a game was installed with.</summary>
    public enum GameStore
    {
        /// <summary>Steam.</summary>
        Steam,
        /// <summary>Epic Games Store.</summary>
        Epic,
    }

    /// <summary>A game installed through a store.</summary>
    public sealed class InstalledGame
    {
        /// <summary>Store.</summary>
        public GameStore Store { get; set; }

        /// <summary>Store id (Steam app id, Epic app name).</summary>
        public string StoreId { get; set; } = "";

        /// <summary>Name as the store shows it.</summary>
        public string Name { get; set; } = "";

        /// <summary>Install folder.</summary>
        public string InstallFolder { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="ILibraryService.FindExecutable"/>.</summary>
    public sealed class FindExecutableRequest
    {
        /// <summary>Folder to search, recursively.</summary>
        public string Folder { get; set; } = "";

        /// <summary>File name, for example "eldenring.exe".</summary>
        public string ExecutableName { get; set; } = "";
    }

    /// <summary>Parameters of <see cref="ILibraryService.DownloadArtwork"/>.</summary>
    public sealed class DownloadArtworkRequest
    {
        /// <summary>Game id, or null for every game.</summary>
        public string? GameId { get; set; }
    }

    /// <summary>How the game list is sorted.</summary>
    public enum LibrarySort
    {
        /// <summary>A to Z.</summary>
        Name,
        /// <summary>Z to A.</summary>
        NameDescending,
        /// <summary>Most players first.</summary>
        MaxPlayers,
        /// <summary>Most played first.</summary>
        Playtime,
        /// <summary>Most recent first.</summary>
        LastPlayed,
    }

    /// <summary>Saved sorting and filtering of the game list.</summary>
    public sealed class LibraryPreferences
    {
        /// <summary>Sort order.</summary>
        public LibrarySort Sort { get; set; }

        /// <summary>Show favourites only.</summary>
        public bool FavoritesOnly { get; set; }

        /// <summary>Show games that support gamepads.</summary>
        public bool FilterGamepads { get; set; }

        /// <summary>Show games that support one keyboard and mouse.</summary>
        public bool FilterKeyboard { get; set; }

        /// <summary>Show games that support several keyboards and mice.</summary>
        public bool FilterMultipleKeyboards { get; set; }
    }

    /// <summary>Games were added, removed, reloaded or changed (favourite, update found, play time, artwork).</summary>
    [BridgeEvent("library.changed", Invalidates = new[] { "library.list", "library.get", "library.getMenu", "handlers.listInstalled" })]
    public sealed class LibraryChanged
    {
        /// <summary>Changed games. Empty means "anything may have changed".</summary>
        public List<string> GameIds { get; set; } = new List<string>();
    }

    /// <summary>Progress of <see cref="ILibraryService.DownloadArtwork"/>.</summary>
    [BridgeEvent("library.artworkProgress")]
    public sealed class ArtworkProgress
    {
        /// <summary>Game being processed.</summary>
        public string GameId { get; set; } = "";

        /// <summary>Games done so far.</summary>
        public int Done { get; set; }

        /// <summary>Games to process.</summary>
        public int Total { get; set; }
    }
}
