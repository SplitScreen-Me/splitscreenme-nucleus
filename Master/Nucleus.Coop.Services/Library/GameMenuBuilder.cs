using System;
using System.Collections.Generic;
using System.Linq;
using Nucleus.Bridge.Contracts;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Coop.Services.Library;

/// <summary>A folder offered by a dynamic menu item (profile config/save paths).</summary>
public sealed class MenuFolderTarget
{
    /// <summary>Label (legacy wording: "Nucleus: &lt;nickname&gt;" or the folder name).</summary>
    public string Label { get; set; } = "";

    /// <summary>Full path of the folder the item opens or deletes.</summary>
    public string Path { get; set; } = "";
}

/// <summary>Everything the per-game menu depends on, captured from the handler, metaInfo, settings and disk.</summary>
public sealed class GameMenuContext
{
    /// <summary>Game id.</summary>
    public string GameId { get; set; } = "";

    /// <summary>Game name.</summary>
    public string GameName { get; set; } = "";

    /// <summary>The handler has notes (<c>Game.Description</c>).</summary>
    public bool HasNotes { get; set; }

    /// <summary><c>content\&lt;GUID&gt;</c> exists.</summary>
    public bool ContentFolderExists { get; set; }

    /// <summary><c>content\&lt;GUID&gt;</c>.</summary>
    public string ContentFolder { get; set; } = "";

    /// <summary>Existing <c>&lt;profile root&gt;\&lt;UserProfileConfigPath&gt;</c> folders.</summary>
    public List<MenuFolderTarget> UserConfigFolders { get; set; } = new List<MenuFolderTarget>();

    /// <summary>Existing <c>&lt;profile root&gt;\&lt;UserProfileSavePath&gt;</c> folders.</summary>
    public List<MenuFolderTarget> UserSaveFolders { get; set; } = new List<MenuFolderTarget>();

    /// <summary>Existing <c>&lt;documents root&gt;\&lt;DocumentsConfigPath&gt;</c> folders.</summary>
    public List<MenuFolderTarget> DocumentsConfigFolders { get; set; } = new List<MenuFolderTarget>();

    /// <summary>Existing <c>&lt;documents root&gt;\&lt;DocumentsSavePath&gt;</c> folders.</summary>
    public List<MenuFolderTarget> DocumentsSaveFolders { get; set; } = new List<MenuFolderTarget>();

    /// <summary>Nicknames that have a backup folder (<c>_Game Files Backup_\&lt;GUID&gt;\&lt;nickname&gt;</c>).</summary>
    public List<string> BackupNicknames { get; set; } = new List<string>();

    /// <summary>Input hooks of the handler.</summary>
    public HandlerInputFlags Inputs { get; set; } = new HandlerInputFlags();

    /// <summary><c>Game.UseDevReorder</c>.</summary>
    public bool UseDevReorder { get; set; }

    /// <summary><c>Game.CreateSingleDeviceFile</c>.</summary>
    public bool CreateSingleDeviceFile { get; set; }

    /// <summary><c>Game.PlayersPerInstance</c>.</summary>
    public int PlayersPerInstance { get; set; }

    /// <summary><c>Game.KeepSymLinkOnExit</c>: the handler always keeps the instances folder.</summary>
    public bool KeepSymLinkOnExit { get; set; }

    /// <summary><c>Game.SteamID</c>.</summary>
    public string? SteamId { get; set; }

    /// <summary><c>Game.UseGoldberg</c>.</summary>
    public bool UseGoldberg { get; set; }

    /// <summary><c>Game.UseGoldbergNoOGSteamDlls</c>.</summary>
    public bool UseGoldbergNoOGSteamDlls { get; set; }

    /// <summary><c>Game.NeedsSteamEmulation</c>.</summary>
    public bool NeedsSteamEmulation { get; set; }

    /// <summary>metaInfo <c>UseApiIndex</c>.</summary>
    public bool UseApiIndex { get; set; }

    /// <summary>metaInfo <c>UseApiIndexForGuests</c>.</summary>
    public bool UseApiIndexForGuests { get; set; }

    /// <summary>metaInfo <c>ProfileAssignGamepadByButonPress</c>.</summary>
    public bool ProfileAssignGamepadByButtonPress { get; set; }

    /// <summary>metaInfo <c>KeepSymLink</c>.</summary>
    public bool KeepSymLink { get; set; }

    /// <summary>metaInfo <c>DisableProfiles</c>.</summary>
    public bool DisableProfiles { get; set; }

    /// <summary>metaInfo <c>CheckUpdate</c>.</summary>
    public bool CheckUpdate { get; set; }

    /// <summary>metaInfo <c>SteamLanguage</c>.</summary>
    public string SteamLanguage { get; set; } = GameMenuBuilder.AppSettingLanguage;

    /// <summary>Settings.ini <c>[Misc] DisableGameProfiles</c>.</summary>
    public bool ProfilesDisabledGlobally { get; set; }

    /// <summary><c>gui\covers\&lt;GUID&gt;.jpeg</c> exists.</summary>
    public bool CoverExists { get; set; }

    /// <summary><c>gui\screenshots\&lt;GUID&gt;\</c> exists.</summary>
    public bool ScreenshotsExist { get; set; }
}

/// <summary>
/// Builds the per-game menu from a <see cref="GameMenuContext"/>: the items of the legacy game context menu
/// (<c>MainForm.Designer</c> gameContextMenuStrip, rules of <c>GameOptionMenuFunc.GameOptionMenu_Opening</c>), in the
/// same order. Pure: no disk or global access, so the visibility rules are unit tested.
/// </summary>
public static class GameMenuBuilder
{
    /// <summary>Per-game Steam language that means "use the app setting".</summary>
    public const string AppSettingLanguage = "App Setting";

    /// <summary>Choices of the per-game Steam language (legacy combo box, sorted).</summary>
    public static readonly IReadOnlyList<string> SteamLanguages = new[]
    {
        AppSettingLanguage, "Arabic", "Brazilian", "Bulgarian", "Czech", "Danish", "Dutch", "English", "Finnish", "French", "German", "Greek",
        "Hungarian", "Italian", "Japanese", "Koreana", "Latam", "Norwegian", "Polish", "Portuguese", "Romanian", "Russian", "Schinese",
        "Spanish", "Swedish", "Tchinese", "Thai", "Turkish", "Ukrainian",
    };

    /// <summary>Item ids.</summary>
    public static class Ids
    {
#pragma warning disable CS1591 // ids are self-explanatory
        public const string Notes = "notes";
        public const string Details = "details";
        public const string OpenHandler = "openHandler";
        public const string OpenExeFolder = "openExeFolder";
        public const string OpenContentFolder = "openContentFolder";
        public const string DeleteContentFolder = "deleteContentFolder";
        public const string OpenUserConfig = "openUserConfig";
        public const string DeleteUserConfig = "deleteUserConfig";
        public const string OpenUserSave = "openUserSave";
        public const string DeleteUserSave = "deleteUserSave";
        public const string OpenDocumentsConfig = "openDocumentsConfig";
        public const string DeleteDocumentsConfig = "deleteDocumentsConfig";
        public const string OpenDocumentsSave = "openDocumentsSave";
        public const string DeleteDocumentsSave = "deleteDocumentsSave";
        public const string OpenBackup = "openBackup";
        public const string DeleteBackup = "deleteBackup";
        public const string UseApiIndex = "useApiIndex";
        public const string UseGamepadButtonPress = "useGamepadButtonPress";
        public const string UseApiIndexForGuests = "useApiIndexForGuests";
        public const string KeepInstancesFolder = "keepInstancesFolder";
        public const string DisableProfiles = "disableProfiles";
        public const string DisableHandlerUpdate = "disableHandlerUpdate";
        public const string ChangeIcon = "changeIcon";
        public const string GameAssets = "gameAssets";
        public const string OpenCoverFolder = "openCoverFolder";
        public const string OpenScreenshotsFolder = "openScreenshotsFolder";
        public const string SteamLanguage = "steamLanguage";
        public const string Remove = "remove";
#pragma warning restore CS1591
    }

    /// <summary>Gamepad hooks that work with "assign gamepads by index" (XInput-style API indexes).</summary>
    public static bool SupportsApiIndex(GameMenuContext c)
    {
        HandlerInputFlags f = c.Inputs;
        return ((f.XInputEnabled && !f.XInputReroute && !f.ProtoDinputDeviceHook) || f.ProtoXinputHook || f.Sdl2Enabled)
            && !c.UseDevReorder && !c.CreateSingleDeviceFile && !f.DInputEnabled;
    }

    /// <summary>"Assign Gamepads By Index".</summary>
    public static bool ShowUseApiIndex(GameMenuContext c) => SupportsApiIndex(c);

    /// <summary>"Assign Gamepads On Button Press (profile)": hidden when profiles are off globally or for the game.</summary>
    public static bool ShowUseGamepadButtonPress(GameMenuContext c) => !c.ProfilesDisabledGlobally && !c.DisableProfiles;

    /// <summary>
    /// "Assign Gamepads By Index For Guests Only". The legacy code computed the hook rule and then overwrote it with
    /// the second half; both halves apply here.
    /// </summary>
    public static bool ShowUseApiIndexForGuests(GameMenuContext c) =>
        SupportsApiIndex(c) && c.PlayersPerInstance > 0 && !c.UseApiIndex && !c.DisableProfiles;

    /// <summary>"Keep Instances Content Folder": hidden when the handler always keeps it.</summary>
    public static bool ShowKeepInstancesFolder(GameMenuContext c) => !c.KeepSymLinkOnExit;

    /// <summary>"Disable Profile": hidden when profiles are off globally.</summary>
    public static bool ShowDisableProfiles(GameMenuContext c) => !c.ProfilesDisabledGlobally;

    /// <summary>"Steam Language": games using a Steam emulator.</summary>
    public static bool ShowSteamLanguage(GameMenuContext c) =>
        (!string.IsNullOrEmpty(c.SteamId) && c.UseGoldberg) || c.UseGoldbergNoOGSteamDlls || c.NeedsSteamEmulation;

    /// <summary>Builds the menu.</summary>
    public static GameMenu Build(GameMenuContext c)
    {
        var groups = new List<List<GameMenuItem>>();

        // Information (legacy: game name title, notes, details).
        var info = new List<GameMenuItem>();
        if (c.HasNotes)
        {
            info.Add(ClientSide(Action(Ids.Notes, T("host.library.gameMenu.notes.label"), T("host.library.gameMenu.notes.description")), GameMenuClientAction.ShowNotes));
        }

        info.Add(ClientSide(Action(Ids.Details, T("host.library.gameMenu.details.label"), T("host.library.gameMenu.details.description")), GameMenuClientAction.ShowDetails));
        groups.Add(info);

        // Files.
        var files = new List<GameMenuItem>
        {
            Action(Ids.OpenHandler, T("host.library.gameMenu.openHandler.label"), T("host.library.gameMenu.openHandler.description")),
            Action(Ids.OpenExeFolder, T("host.library.gameMenu.openExeFolder.label"), T("host.library.gameMenu.openExeFolder.description")),
        };
        if (c.ContentFolderExists)
        {
            files.Add(Action(Ids.OpenContentFolder, T("host.library.gameMenu.openContentFolder.label"), T("host.library.gameMenu.openContentFolder.description")));
            GameMenuItem delete = Action(Ids.DeleteContentFolder, T("host.library.gameMenu.deleteContentFolder.label"), T("host.library.gameMenu.deleteContentFolder.description"));
            delete.Danger = true;
            delete.Confirm = T("host.library.gameMenu.deleteContentFolder.confirm", new { folder = c.ContentFolder, game = c.GameName });
            files.Add(delete);
        }

        groups.Add(files);

        // Game files of each player (dynamic submenus, legacy order).
        var paths = new List<GameMenuItem>();
        AddFolderSubmenus(paths, c.UserConfigFolders, Ids.OpenUserConfig, T("host.library.gameMenu.openUserConfig.label"), Ids.DeleteUserConfig, T("host.library.gameMenu.deleteUserConfig.label"));
        AddFolderSubmenus(paths, c.UserSaveFolders, Ids.OpenUserSave, T("host.library.gameMenu.openUserSave.label"), Ids.DeleteUserSave, T("host.library.gameMenu.deleteUserSave.label"));
        AddFolderSubmenus(paths, c.DocumentsConfigFolders, Ids.OpenDocumentsConfig, T("host.library.gameMenu.openDocumentsConfig.label"), Ids.DeleteDocumentsConfig, T("host.library.gameMenu.deleteDocumentsConfig.label"));
        AddFolderSubmenus(paths, c.DocumentsSaveFolders, Ids.OpenDocumentsSave, T("host.library.gameMenu.openDocumentsSave.label"), Ids.DeleteDocumentsSave, T("host.library.gameMenu.deleteDocumentsSave.label"));
        if (c.BackupNicknames.Count > 0)
        {
            var open = Submenu(Ids.OpenBackup, T("host.library.gameMenu.openBackup.label"), T("host.library.gameMenu.openBackup.description"));
            var delete = Submenu(Ids.DeleteBackup, T("host.library.gameMenu.deleteBackup.label"), null);
            foreach (string nickname in c.BackupNicknames)
            {
                open.Children.Add(Action(Ids.OpenBackup + ":" + nickname, nickname, null));
                GameMenuItem child = Action(Ids.DeleteBackup + ":" + nickname, nickname, null);
                child.Danger = true;
                child.Confirm = T("host.library.gameMenu.deleteBackup.confirm", new { nickname, game = c.GameName });
                delete.Children.Add(child);
            }

            paths.Add(open);
            paths.Add(delete);
        }

        groups.Add(paths);

        // Gamepad assignment.
        var gamepads = new List<GameMenuItem>();
        if (ShowUseApiIndex(c))
        {
            gamepads.Add(Toggle(Ids.UseApiIndex, T("host.library.gameMenu.useApiIndex.label"), c.UseApiIndex,
                T("host.library.gameMenu.useApiIndex.description")));
        }

        if (ShowUseGamepadButtonPress(c))
        {
            gamepads.Add(Toggle(Ids.UseGamepadButtonPress, T("host.library.gameMenu.useGamepadButtonPress.label"), c.ProfileAssignGamepadByButtonPress,
                T("host.library.gameMenu.useGamepadButtonPress.description")));
        }

        if (ShowUseApiIndexForGuests(c))
        {
            gamepads.Add(Toggle(Ids.UseApiIndexForGuests, T("host.library.gameMenu.useApiIndexForGuests.label"), c.UseApiIndexForGuests,
                T("host.library.gameMenu.useApiIndexForGuests.description")));
        }

        groups.Add(gamepads);

        // Behaviour.
        var behaviour = new List<GameMenuItem>();
        if (ShowKeepInstancesFolder(c))
        {
            behaviour.Add(Toggle(Ids.KeepInstancesFolder, T("host.library.gameMenu.keepInstancesFolder.label"), c.KeepSymLink,
                T("host.library.gameMenu.keepInstancesFolder.description")));
        }

        if (ShowDisableProfiles(c))
        {
            behaviour.Add(Toggle(Ids.DisableProfiles, T("host.library.gameMenu.disableProfiles.label"), c.DisableProfiles,
                T("host.library.gameMenu.disableProfiles.description")));
        }

        behaviour.Add(Toggle(Ids.DisableHandlerUpdate, T("host.library.gameMenu.disableHandlerUpdate.label"), !c.CheckUpdate,
            T("host.library.gameMenu.disableHandlerUpdate.description")));
        groups.Add(behaviour);

        // Appearance and emulation.
        var look = new List<GameMenuItem>
        {
            ChangeIconItem(),
        };
        var assets = Submenu(Ids.GameAssets, T("host.library.gameMenu.gameAssets.label"), T("host.library.gameMenu.gameAssets.description"));
        if (c.CoverExists)
        {
            assets.Children.Add(Action(Ids.OpenCoverFolder, T("host.library.gameMenu.openCoverFolder.label"), null));
        }

        if (c.ScreenshotsExist)
        {
            assets.Children.Add(Action(Ids.OpenScreenshotsFolder, T("host.library.gameMenu.openScreenshotsFolder.label"), null));
        }

        if (assets.Children.Count > 0)
        {
            look.Add(assets);
        }

        if (ShowSteamLanguage(c))
        {
            // The values are stored in the game's metadata. The Steam languages are what the game sees and keep their names;
            // only "App Setting" (use the global setting) gets a translated label.
            string appSetting = T("host.library.gameMenu.steamLanguage.appSetting");
            look.Add(new GameMenuItem
            {
                Id = Ids.SteamLanguage,
                Kind = GameMenuItemKind.Choice,
                Label = T("host.library.gameMenu.steamLanguage.label"),
                Description = T("host.library.gameMenu.steamLanguage.description", new { appSetting }),
                Choices = SteamLanguages.ToList(),
                ChoiceLabels = SteamLanguages.Select(v => v == AppSettingLanguage ? appSetting : v).ToList(),
                Value = SteamLanguages.Contains(c.SteamLanguage) ? c.SteamLanguage : AppSettingLanguage,
            });
        }

        groups.Add(look);

        // Removal.
        GameMenuItem remove = Action(Ids.Remove, T("host.library.gameMenu.remove.label"), T("host.library.gameMenu.remove.description"));
        remove.Danger = true;
        remove.Confirm = T("host.library.gameMenu.remove.confirm", new { game = c.GameName });
        groups.Add(new List<GameMenuItem> { remove });

        var menu = new GameMenu { GameId = c.GameId };
        int separator = 0;
        foreach (List<GameMenuItem> group in groups.Where(g => g.Count > 0))
        {
            if (menu.Items.Count > 0)
            {
                menu.Items.Add(new GameMenuItem { Id = "separator" + (++separator), Kind = GameMenuItemKind.Separator });
            }

            menu.Items.AddRange(group);
        }

        return menu;
    }

    /// <summary>Splits an item id into its action and its target (<c>openBackup:Player 2</c> → <c>openBackup</c>, <c>Player 2</c>).</summary>
    public static (string Action, string? Target) ParseId(string itemId)
    {
        int colon = itemId.IndexOf(':');
        return colon < 0 ? (itemId, null) : (itemId.Substring(0, colon), itemId.Substring(colon + 1));
    }

    private static void AddFolderSubmenus(List<GameMenuItem> items, List<MenuFolderTarget> folders, string openId, string openLabel, string deleteId, string deleteLabel)
    {
        if (folders.Count == 0)
        {
            return;
        }

        var open = Submenu(openId, openLabel, null);
        var delete = Submenu(deleteId, deleteLabel, null);
        foreach (MenuFolderTarget folder in folders)
        {
            open.Children.Add(Action(openId + ":" + folder.Path, folder.Label, folder.Path));
            GameMenuItem child = Action(deleteId + ":" + folder.Path, folder.Label, folder.Path);
            child.Danger = true;
            child.Confirm = T("host.library.gameMenu.deleteFolderConfirm", new { folder = folder.Path });
            delete.Children.Add(child);
        }

        items.Add(open);
        items.Add(delete);
    }

    private static GameMenuItem ClientSide(GameMenuItem item, GameMenuClientAction action)
    {
        item.ClientAction = action;
        return item;
    }

    // Same file types as the legacy ChangeGameIcon dialog.
    private static GameMenuItem ChangeIconItem()
    {
        GameMenuItem item = ClientSide(Action(Ids.ChangeIcon, T("host.library.gameMenu.changeIcon.label"), T("host.library.gameMenu.changeIcon.description")), GameMenuClientAction.PickFile);
        item.FileFilters = new List<FileFilter>
        {
            new FileFilter { Name = T("host.library.gameMenu.changeIcon.filters.imagesAndExecutables"), Extensions = new List<string> { "png", "jpg", "jpeg", "jfif", "gif", "bmp", "tif", "tiff", "ico", "exe" } },
            new FileFilter { Name = T("host.library.gameMenu.changeIcon.filters.images"), Extensions = new List<string> { "png", "jpg", "jpeg", "jfif", "gif", "bmp", "tif", "tiff", "ico" } },
            new FileFilter { Name = T("host.library.gameMenu.changeIcon.filters.executables"), Extensions = new List<string> { "exe" } },
        };
        return item;
    }

    private static GameMenuItem Action(string id, string label, string? description) =>
        new GameMenuItem { Id = id, Kind = GameMenuItemKind.Action, Label = label, Description = description };

    private static GameMenuItem Toggle(string id, string label, bool value, string description) =>
        new GameMenuItem { Id = id, Kind = GameMenuItemKind.Toggle, Label = label, Checked = value, Description = description };

    private static GameMenuItem Submenu(string id, string label, string? description) =>
        new GameMenuItem { Id = id, Kind = GameMenuItemKind.Submenu, Label = label, Description = description };
}
