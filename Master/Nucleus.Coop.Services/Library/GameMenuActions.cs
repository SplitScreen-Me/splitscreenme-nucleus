using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Bridge;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Handlers;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Library;

/// <summary>
/// The per-game menu: builds its context from the game (visibility rules of <c>GameOptionMenuFunc</c>) and runs its
/// items. Confirmations are the UI's job (<see cref="GameMenuItem.Confirm"/>); folders open in File Explorer.
/// </summary>
public sealed class GameMenuActions
{
    private static readonly string[] IconExtensions = { ".png", ".jpg", ".jpeg", ".jfif", ".gif", ".bmp", ".tif", ".tiff", ".ico", ".exe" };

    private readonly NucleusCore core;
    private readonly Action<string> removeGame;

    /// <summary>Creates the menu. <paramref name="removeGame"/> runs "Remove Game From List".</summary>
    public GameMenuActions(NucleusCore core, Action<string> removeGame)
    {
        this.core = core;
        this.removeGame = removeGame;
    }

    /// <summary>Backups of player files (<c>BackupDatas</c>): <c>%USERPROFILE%\NucleusCoop\_Game Files Backup_\&lt;GUID&gt;</c>.</summary>
    public static string BackupRoot(string gameId) => Path.Combine(Globals.UserEnvironmentRoot, "NucleusCoop", "_Game Files Backup_", gameId);

    /// <summary>Snapshot of what the menu depends on. Call on the UI thread.</summary>
    public static GameMenuContext CreateContext(UserGameInfo entry)
    {
        GenericGameInfo game = entry.Game;
        SavePaths paths = ResolveSavePaths(game.UserProfileConfigPath, game.UserProfileSavePath, game.DocumentsConfigPath, game.DocumentsSavePath);
        GameMetaInfo meta = game.MetaInfo;
        string contentFolder = Path.Combine(GameManager.Instance.GetAppContentPath(), game.GUID);

        GameMenuContext context = new GameMenuContext
        {
            GameId = game.GUID,
            GameName = game.GameName ?? game.GUID,
            HasNotes = !string.IsNullOrEmpty(game.Description),
            ContentFolder = contentFolder,
            ContentFolderExists = Directory.Exists(contentFolder),
            UserConfigFolders = Folders(SafeList(() => game.Profiles_ConfigPath), paths.UserConfig, documents: false),
            UserSaveFolders = Folders(SafeList(() => game.Profiles_SavePath), paths.UserSave, documents: false),
            DocumentsConfigFolders = Folders(SafeList(() => game.Profiles_Documents_ConfigPath), paths.DocumentsConfig, documents: true),
            DocumentsSaveFolders = Folders(SafeList(() => game.Profiles_Documents_SavePath), paths.DocumentsSave, documents: true),
            BackupNicknames = SafeList(() => game.Players_BackupPath).Select(p => Path.GetFileName(p.TrimEnd('\\'))).Where(n => n.Length > 0).ToList(),
            Inputs = GameDtoFactory.Flags(game),
            UseDevReorder = game.UseDevReorder,
            CreateSingleDeviceFile = game.CreateSingleDeviceFile,
            PlayersPerInstance = game.PlayersPerInstance,
            KeepSymLinkOnExit = game.KeepSymLinkOnExit,
            SteamId = game.SteamID,
            UseGoldberg = game.UseGoldberg,
            UseGoldbergNoOGSteamDlls = game.UseGoldbergNoOGSteamDlls,
            NeedsSteamEmulation = game.NeedsSteamEmulation,
            UseApiIndex = meta.UseApiIndex,
            UseApiIndexForGuests = meta.UseApiIndexForGuests,
            ProfileAssignGamepadByButtonPress = meta.ProfileAssignGamepadByButonPress,
            KeepSymLink = meta.KeepSymLink,
            DisableProfiles = meta.DisableProfiles,
            CheckUpdate = meta.CheckUpdate,
            SteamLanguage = meta.SteamLanguage ?? GameMenuBuilder.AppSettingLanguage,
            ProfilesDisabledGlobally = App_Misc.DisableGameProfiles,
            CoverExists = File.Exists(GameAssetFiles.CoverPath(game.GUID)),
            ScreenshotsExist = Directory.Exists(GameAssetFiles.ScreenshotsFolder(game.GUID)),
        };

        // Where each deletion goes, for the confirmation texts (only for the items the menu shows).
        RecycleBins bins = new RecycleBins();
        if (context.ContentFolderExists)
        {
            context.ContentFolderRecycleBin = bins.Has(contentFolder);
        }

        foreach (MenuFolderTarget folder in context.UserConfigFolders.Concat(context.UserSaveFolders).Concat(context.DocumentsConfigFolders).Concat(context.DocumentsSaveFolders))
        {
            folder.RecycleBin = bins.Has(folder.Path);
        }

        if (context.BackupNicknames.Count > 0)
        {
            context.BackupsRecycleBin = bins.Has(BackupRoot(game.GUID));
        }

        return context;
    }

    /// <summary>
    /// <see cref="FileUtil.HasRecycleBin"/> asked once per drive (it queries the shell): whether a deletion there goes
    /// to the recycle bin (<see cref="UserDataDeletion"/>) or is permanent.
    /// </summary>
    internal sealed class RecycleBins
    {
        private readonly Func<string, bool> query;
        private readonly Dictionary<string, bool> byRoot = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        public RecycleBins(Func<string, bool>? query = null)
        {
            this.query = query ?? FileUtil.HasRecycleBin;
        }

        public bool Has(string path)
        {
            string root;
            try
            {
                root = Path.GetPathRoot(Path.GetFullPath(path)) ?? "";
            }
            catch (Exception)
            {
                return false;
            }

            if (!byRoot.TryGetValue(root, out bool has))
            {
                has = root.Length > 0 && query(root);
                byRoot[root] = has;
            }

            return has;
        }
    }

    /// <summary>Runs a menu item. Call on the UI thread.</summary>
    public async Task RunAsync(UserGameInfo entry, string itemId, string? value)
    {
        GenericGameInfo game = entry.Game;
        GameMetaInfo meta = game.MetaInfo;
        string gameId = game.GUID;
        GameMenuContext context = CreateContext(entry);

        // Only items the menu offers right now (also checks dynamic targets: real paths and nicknames in the ids).
        if (!Contains(GameMenuBuilder.Build(context).Items, itemId))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.itemUnavailable"), new { reason = "itemUnavailable", itemId });
        }

        (string action, string? target) = GameMenuBuilder.ParseId(itemId);
        switch (action)
        {
            case GameMenuBuilder.Ids.Notes:
            case GameMenuBuilder.Ids.Details:
                return; // shown by the UI from library.get

            case GameMenuBuilder.Ids.OpenHandler:
                OpenInTextEditor(Path.Combine(GameManager.Instance.GetJsScriptsPath(), game.JsFileName ?? ""));
                return;

            case GameMenuBuilder.Ids.OpenExeFolder:
                string? exeFolder = string.IsNullOrEmpty(entry.ExePath) ? null : Path.GetDirectoryName(entry.ExePath);
                if (exeFolder == null || !Directory.Exists(exeFolder))
                {
                    throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.exeFolderMissing"));
                }

                OpenFolder(exeFolder);
                return;

            case GameMenuBuilder.Ids.OpenContentFolder:
                OpenFolder(context.ContentFolder);
                return;

            case GameMenuBuilder.Ids.DeleteContentFolder:
                RefuseDuringSession();
                //Nucleus 2.5: what the user deletes goes to the recycle bin.
                bool deleted = await CleanGameContent.CleanContentFolderAsync(game, reportLockedFiles: false,
                    sendToRecycleBin: UserDataDeletion.ForMenuItem(GameMenuBuilder.Ids.DeleteContentFolder) == DeleteMode.RecycleBin);
                core.Notifier.GamesChanged(gameId);
                if (!deleted)
                {
                    throw new BridgeException(BridgeErrorKind.Conflict,
                        I18n.T("host.library.errors.contentFilesLocked"),
                        new { reason = "filesLocked", path = game.Content_Folder });
                }

                return;

            case GameMenuBuilder.Ids.OpenUserConfig:
            case GameMenuBuilder.Ids.OpenUserSave:
            case GameMenuBuilder.Ids.OpenDocumentsConfig:
            case GameMenuBuilder.Ids.OpenDocumentsSave:
                OpenFolder(target!);
                return;

            case GameMenuBuilder.Ids.DeleteUserConfig:
            case GameMenuBuilder.Ids.DeleteUserSave:
            case GameMenuBuilder.Ids.DeleteDocumentsConfig:
            case GameMenuBuilder.Ids.DeleteDocumentsSave:
                RefuseDuringSession();
                await DeleteFolderAsync(target!);
                core.Notifier.GamesChanged(gameId);
                return;

            case GameMenuBuilder.Ids.OpenBackup:
                OpenFolder(Path.Combine(BackupRoot(gameId), target!));
                return;

            case GameMenuBuilder.Ids.DeleteBackup:
                RefuseDuringSession();
                await DeleteFolderAsync(Path.Combine(BackupRoot(gameId), target!));
                core.Notifier.GamesChanged(gameId);
                return;

            case GameMenuBuilder.Ids.UseApiIndex:
                meta.UseApiIndex = !meta.UseApiIndex;
                if (meta.UseApiIndex)
                {
                    meta.ProfileAssignGamepadByButonPress = false;
                }

                AfterSetupToggle(gameId, meta, updateXinputIndex: true);
                return;

            case GameMenuBuilder.Ids.UseGamepadButtonPress:
                meta.ProfileAssignGamepadByButonPress = !meta.ProfileAssignGamepadByButonPress;
                if (meta.ProfileAssignGamepadByButonPress)
                {
                    meta.UseApiIndex = false;
                }

                AfterSetupToggle(gameId, meta, updateXinputIndex: true);
                return;

            case GameMenuBuilder.Ids.UseApiIndexForGuests:
                meta.UseApiIndexForGuests = !meta.UseApiIndexForGuests;
                AfterSetupToggle(gameId, meta, updateXinputIndex: false);
                return;

            case GameMenuBuilder.Ids.KeepInstancesFolder:
                meta.KeepSymLink = !meta.KeepSymLink;
                core.Notifier.GamesChanged(gameId);
                return;

            case GameMenuBuilder.Ids.DisableProfiles:
                meta.DisableProfiles = !meta.DisableProfiles;
                AfterSetupToggle(gameId, meta, updateXinputIndex: false);
                return;

            case GameMenuBuilder.Ids.DisableHandlerUpdate:
                await ToggleHandlerUpdateAsync(game);
                return;

            case GameMenuBuilder.Ids.ChangeIcon:
                ChangeIcon(entry, value);
                return;

            case GameMenuBuilder.Ids.OpenCoverFolder:
                SelectInExplorer(GameAssetFiles.CoverPath(gameId));
                return;

            case GameMenuBuilder.Ids.OpenScreenshotsFolder:
                OpenFolder(GameAssetFiles.ScreenshotsFolder(gameId));
                return;

            case GameMenuBuilder.Ids.SteamLanguage:
                if (value == null || !GameMenuBuilder.SteamLanguages.Contains(value))
                {
                    throw new BridgeException(BridgeErrorKind.InvalidParams, I18n.T("host.library.errors.invalidSteamLanguage"), new { reason = "invalidValue" });
                }

                meta.SteamLanguage = value;
                core.Notifier.GamesChanged(gameId);
                return;

            case GameMenuBuilder.Ids.Remove:
                removeGame(gameId);
                return;

            default:
                throw new BridgeException(BridgeErrorKind.NotFound, $"Unknown menu item '{itemId}'.", new { reason = "itemUnavailable", itemId });
        }
    }

    /// <summary>The save and config paths of a handler, with the legacy <c>documents\</c> forms resolved.</summary>
    internal sealed class SavePaths
    {
        public string? UserConfig { get; set; }
        public string? UserSave { get; set; }
        public string? DocumentsConfig { get; set; }
        public string? DocumentsSave { get; set; }
    }

    /// <summary>
    /// Legacy rule: a <c>Game.UserProfile*Path</c> starting with <c>documents\</c> is a Documents path. The menu used to
    /// apply it by changing the handler (<c>UserProfile*Path</c> set to null, Nucleus 2.5 fix), which broke the launch of
    /// the same handler object; it is now computed without touching the handler. The launch converts these paths itself.
    /// </summary>
    internal static SavePaths ResolveSavePaths(string? userConfig, string? userSave, string? documentsConfig, string? documentsSave)
    {
        var paths = new SavePaths { UserConfig = userConfig, UserSave = userSave, DocumentsConfig = documentsConfig, DocumentsSave = documentsSave };

        if (userConfig?.Length > 0 && userConfig.StartsWith(@"documents\", StringComparison.OrdinalIgnoreCase))
        {
            paths.DocumentsConfig = userConfig.Substring(10);
            paths.UserConfig = null;
        }

        if (userSave?.Length > 0 && userSave.StartsWith(@"documents\", StringComparison.OrdinalIgnoreCase))
        {
            paths.DocumentsSave = userSave.Substring(10);
            paths.UserSave = null;
        }

        return paths;
    }

    /// <summary>Opens a file in the configured text editor (<c>[Dev] TextEditorPath</c>), else Notepad++, else Notepad (<c>OpenHandler.OpenRawHandler</c>).</summary>
    internal static void OpenInTextEditor(string path)
    {
        if (!File.Exists(path))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.handlerFileMissing"), new { path });
        }

        string editor = App_Misc.TextEditorPath;
        try
        {
            if (!string.IsNullOrEmpty(editor) && editor != "Default" && File.Exists(editor))
            {
                Process.Start(editor, "\"" + path + "\"");
            }
            else
            {
                Process.Start("notepad++.exe", "\"" + path + "\"");
            }
        }
        catch (Exception)
        {
            Process.Start("notepad.exe", "\"" + path + "\"");
        }
    }

    internal static void OpenFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.folderGone"), new { path = folder });
        }

        Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true });
    }

    internal static void SelectInExplorer(string file)
    {
        if (!File.Exists(file))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.library.errors.fileGone"), new { path = file });
        }

        Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + file + "\"") { UseShellExecute = true });
    }

    private static bool Contains(IEnumerable<GameMenuItem> items, string id) =>
        items.Any(i => string.Equals(i.Id, id, StringComparison.Ordinal) || Contains(i.Children, id));

    private static void RefuseDuringSession()
    {
        if (NucleusCore.SessionRunning)
        {
            throw new BridgeException(BridgeErrorKind.Conflict, I18n.T("host.library.errors.sessionRunning"), new { reason = "sessionRunning" });
        }
    }

    // User data deleted from the menu: to the recycle bin (Nucleus 2.5).
    private static async Task DeleteFolderAsync(string folder)
    {
        try
        {
            await UserDataDeletion.DeleteAsync(folder, DeleteMode.RecycleBin);
        }
        catch (Exception e) when (e is IOException || e is UnauthorizedAccessException)
        {
            throw new BridgeException(BridgeErrorKind.Conflict, I18n.T("host.library.errors.deleteFailed", new { folder, error = e.Message }), new { reason = "deleteFailed", path = folder });
        }
    }

    private static List<string> SafeList(Func<List<string>> read)
    {
        try
        {
            return read() ?? new List<string>();
        }
        catch (Exception)
        {
            return new List<string>();
        }
    }

    // Each existing <root>\<suffix> folder, with the legacy labels; a label used twice gets its folder appended.
    private static List<MenuFolderTarget> Folders(List<string> roots, string? suffix, bool documents)
    {
        var targets = new List<MenuFolderTarget>();
        if (string.IsNullOrEmpty(suffix))
        {
            return targets;
        }

        foreach (string root in roots.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            try
            {
                string trimmed = root.TrimEnd('\\');
                string path = Path.Combine(trimmed, suffix);
                if (!Directory.Exists(path))
                {
                    continue;
                }

                string label;
                if (documents)
                {
                    DirectoryInfo? parent = Directory.GetParent(trimmed);
                    string name = parent?.Name ?? trimmed;
                    label = parent?.Parent?.Name == "NucleusCoop" ? I18n.T("host.library.gameMenu.nucleusFolder", new { name }) : name;
                }
                else
                {
                    string name = Path.GetFileName(trimmed);
                    label = Directory.GetParent(trimmed)?.Name == "NucleusCoop" ? I18n.T("host.library.gameMenu.nucleusFolder", new { name }) : name;
                }

                targets.Add(new MenuFolderTarget { Label = label, Path = path });
            }
            catch (Exception)
            {
                // Legacy showed an inert "/!\ OneDrive /!\" item here; the folder is simply left out.
            }
        }

        foreach (IGrouping<string, MenuFolderTarget> duplicates in targets.GroupBy(t => t.Label).Where(g => g.Count() > 1))
        {
            foreach (MenuFolderTarget target in duplicates)
            {
                target.Label += $" ({target.Path})";
            }
        }

        return targets;
    }

    private void AfterSetupToggle(string gameId, GameMetaInfo meta, bool updateXinputIndex)
    {
        core.Notifier.GamesChanged(gameId);

        if (string.Equals(GameProfile.GameInfo?.GameGuid, gameId, StringComparison.OrdinalIgnoreCase))
        {
            if (updateXinputIndex)
            {
                try
                {
                    // Legacy: resets the setup screen through GameProfile.OnUseXinputIndexChanged.
                    GameProfile.UseXinputIndex = meta.UseApiIndex;
                }
                catch (Exception e)
                {
                    ServiceLog.Warn("GameProfile.UseXinputIndex could not be updated", e);
                }
            }

            LibraryHooks.RaiseSetupInvalidated(gameId);
        }
    }

    private async Task ToggleHandlerUpdateAsync(GenericGameInfo game)
    {
        string gameId = game.GUID;
        if (game.MetaInfo.CheckUpdate)
        {
            game.MetaInfo.CheckUpdate = false;
            game.UpdateAvailable = false;
            core.Notifier.GamesChanged(gameId);
            return;
        }

        game.MetaInfo.CheckUpdate = true;
        core.Notifier.GamesChanged(gameId);

        List<HandlerUpdateChecker.Target> targets = HandlerUpdateChecker.LibraryTargets(new[] { gameId });
        _ = Task.Run(async () =>
        {
            try
            {
                if (await core.HubStatus.EnsureOnlineAsync(CancellationToken.None).ConfigureAwait(false))
                {
                    await core.Updates.CheckAsync(targets, throwWhenOffline: false, CancellationToken.None).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn($"update check of {gameId} failed", e);
            }
        });

        await Task.CompletedTask;
    }

    private void ChangeIcon(UserGameInfo entry, string? value)
    {
        string? path = value?.Trim().Trim('"');
        if (string.IsNullOrEmpty(path))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, "Pick an image or an executable first (dialogs.pickFile), then pass its path as the value.", new { reason = "valueRequired" });
        }

        if (!IconExtensions.Contains(Path.GetExtension(path), StringComparer.OrdinalIgnoreCase))
        {
            throw new BridgeException(BridgeErrorKind.InvalidParams, I18n.T("host.library.errors.unsupportedIcon"), new { reason = "unsupportedFile" });
        }

        if (!File.Exists(path))
        {
            throw new BridgeException(BridgeErrorKind.NotFound, I18n.T("host.errors.fileNotFound"), new { path });
        }

        entry.Game.MetaInfo.IconPath = path;
        entry.Icon = null; // legacy in-memory icon of the WinForms list
        core.Icons.Refresh(entry.Game.GUID, entry.ExePath, path);
        core.Notifier.GamesChanged(entry.Game.GUID);
    }
}
