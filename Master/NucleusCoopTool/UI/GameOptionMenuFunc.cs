using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class GameOptionMenuFunc
    {
        private static GameControl menuCurrentControl;
        private static  UserGameInfo menuCurrentGameInfo;

        public static void GameOptionMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ContextMenuStrip gameOptionMenu = sender as ContextMenuStrip;

            Control selectedControl = Generic_Functions.FindControlAtCursor(UI_Interface.MainForm);

            if (selectedControl == null)
            {
                return;
            }

            if (selectedControl is Label || selectedControl is PictureBox)
            {
                selectedControl = selectedControl.Parent;
            }

            foreach (Control c in selectedControl?.Controls)
            {
                if (c is Label)
                {
                    if (c.Text == "No games")
                    {                    
                        for (int i = 0; i < gameOptionMenu.Items.Count; i++)
                        {
                            gameOptionMenu.Items[i].Visible = false;
                        }
                        return;
                    }
                }
            }

            if (selectedControl is GameControl || selectedControl is Button)
            {
                bool isButton = !(selectedControl is GameControl);

                menuCurrentControl = isButton ? UI_Interface.CurrentGameListControl : (GameControl)selectedControl;
                menuCurrentGameInfo = menuCurrentControl.UserGameInfo;
                Core_Interface.CurrentMenuUserGameInfo = menuCurrentGameInfo;

                gameOptionMenu.Items["gameNameMenuItem"].Visible = !isButton;
                gameOptionMenu.Items["detailsMenuItem"].Visible = !isButton;
                gameOptionMenu.Items["notesMenuItem"].Visible = !isButton;
                gameOptionMenu.Items["menuSeparator2"].Visible = false;

                gameOptionMenu.Items["openUserProfConfigMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteUserProfConfigMenuItem"].Visible = false;
                gameOptionMenu.Items["openUserProfSaveMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteUserProfSaveMenuItem"].Visible = false;
                gameOptionMenu.Items["openDocumentConfMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteDocumentConfMenuItem"].Visible = false;
                gameOptionMenu.Items["openDocumentSaveMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteDocumentSaveMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteContentFolderMenuItem"].Visible = false;
                gameOptionMenu.Items["openBackupFolderMenuItem"].Visible = false;
                gameOptionMenu.Items["deleteBackupFolderMenuItem"].Visible = false;

                gameOptionMenu.Items["gameNameMenuItem"].ForeColor = Color.DodgerBlue;
                gameOptionMenu.Items["gameNameMenuItem"].ImageAlign = ContentAlignment.MiddleCenter;
                gameOptionMenu.Items["gameNameMenuItem"].ImageScaling = ToolStripItemImageScaling.SizeToFit;
                gameOptionMenu.Items["gameNameMenuItem"].Image = menuCurrentGameInfo.Icon;
                

                //Set always visible items click event manually
                gameOptionMenu.Items["removeGameMenuItem"].Click -= RemoveGameMenuItem_Click;
                gameOptionMenu.Items["removeGameMenuItem"].Click += RemoveGameMenuItem_Click;

                gameOptionMenu.Items["openHandlerMenuItem"].Click -= OpenHandlerMenuItem_Click;
                gameOptionMenu.Items["openHandlerMenuItem"].Click += OpenHandlerMenuItem_Click;

                gameOptionMenu.Items["openContentFolderMenuItem"].Click -= OpenContentFolderMenuItem_Click;
                gameOptionMenu.Items["openContentFolderMenuItem"].Click += OpenContentFolderMenuItem_Click;

                gameOptionMenu.Items["deleteContentFolderMenuItem"].Click -= DeleteContentFolderMenuItem_Click;
                gameOptionMenu.Items["deleteContentFolderMenuItem"].Click += DeleteContentFolderMenuItem_Click;

                gameOptionMenu.Items["changeIconMenuItem"].Click -= ChangeIconMenuItem_Click;
                gameOptionMenu.Items["changeIconMenuItem"].Click += ChangeIconMenuItem_Click;

                gameOptionMenu.Items["notesMenuItem"].Click -= NotesMenuItem_Click;
                gameOptionMenu.Items["notesMenuItem"].Click += NotesMenuItem_Click;

                gameOptionMenu.Items["openOrigExePathMenuItem"].Click -= OpenOrigExePathMenuItem_Click;
                gameOptionMenu.Items["openOrigExePathMenuItem"].Click += OpenOrigExePathMenuItem_Click; 
                //

                if (string.IsNullOrEmpty(menuCurrentGameInfo?.GameGuid) || menuCurrentGameInfo == null)
                {                  
                    for (int i = 0; i < gameOptionMenu.Items.Count; i++)
                    {
                        gameOptionMenu.Items[i].Visible = false;
                    }
                }
                else
                {
                    gameOptionMenu.Items["gameNameMenuItem"].Text = menuCurrentGameInfo.Game.GameName;

                    bool userConfigPathExists = false;
                    bool userSavePathExists = false;
                    bool docConfigPathExists = false;
                    bool docSavePathExists = false;
                    bool backupFolderExist = false;
                    //bool userConfigPathConverted = false;
                    if (menuCurrentGameInfo.Game.UserProfileConfigPath?.Length > 0 && menuCurrentGameInfo.Game.UserProfileConfigPath.ToLower().StartsWith(@"documents\"))
                    {
                        menuCurrentGameInfo.Game.DocumentsConfigPath = menuCurrentGameInfo.Game.UserProfileConfigPath.Substring(10);
                        menuCurrentGameInfo.Game.UserProfileConfigPath = null;
                        menuCurrentGameInfo.Game.DocumentsConfigPathNoCopy = menuCurrentGameInfo.Game.UserProfileConfigPathNoCopy;
                        menuCurrentGameInfo.Game.ForceDocumentsConfigCopy = menuCurrentGameInfo.Game.ForceUserProfileConfigCopy;
                        //userConfigPathConverted = true;
                    }

                    //bool userSavePathConverted = false;
                    if (menuCurrentGameInfo.Game.UserProfileSavePath?.Length > 0 && menuCurrentGameInfo.Game.UserProfileSavePath.ToLower().StartsWith(@"documents\"))
                    {
                        menuCurrentGameInfo.Game.DocumentsSavePath = menuCurrentGameInfo.Game.UserProfileSavePath.Substring(10);
                        menuCurrentGameInfo.Game.UserProfileSavePath = null;
                        menuCurrentGameInfo.Game.DocumentsSavePathNoCopy = menuCurrentGameInfo.Game.UserProfileSavePathNoCopy;
                        menuCurrentGameInfo.Game.ForceDocumentsSaveCopy = menuCurrentGameInfo.Game.ForceUserProfileSaveCopy;
                        //userSavePathConverted = true;
                    }

                    for (int i = 1; i < gameOptionMenu.Items.Count; i++)
                    {
                        gameOptionMenu.Items[i].Visible = true;

                        if (string.IsNullOrEmpty(menuCurrentGameInfo.Game.UserProfileConfigPath) && string.IsNullOrEmpty(menuCurrentGameInfo.Game.UserProfileSavePath) && string.IsNullOrEmpty(menuCurrentGameInfo.Game.DocumentsConfigPath) && string.IsNullOrEmpty(menuCurrentGameInfo.Game.DocumentsSavePath))
                        {
                            if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["menuSeparator2"]))
                            {
                                gameOptionMenu.Items["menuSeparator2"].Visible = false;
                            }
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["useAPIIndexMenuItem"]))
                        {
                            gameOptionMenu.Items[i].Visible = menuCurrentGameInfo.Game.MetaInfo.DisableProfiles ? false :
                                ((menuCurrentGameInfo.Game.Hook.XInputEnabled && !menuCurrentGameInfo.Game.Hook.XInputReroute && !menuCurrentGameInfo.Game.ProtoInput.DinputDeviceHook) ||
                                (menuCurrentGameInfo.Game.ProtoInput.XinputHook || menuCurrentGameInfo.Game.Hook.SDL2Enabled)) &&
                                (!menuCurrentGameInfo.Game.UseDevReorder && !menuCurrentGameInfo.Game.CreateSingleDeviceFile) &&
                                !menuCurrentGameInfo.Game.Hook.DInputEnabled;

                            if (menuCurrentGameInfo.Game.MetaInfo.UseApiIndex)
                            {
                                gameOptionMenu.Items[i].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                            }
                            else
                            {
                                gameOptionMenu.Items[i].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                            }

                            gameOptionMenu.Items[i].Click -= UseAPIIndexMenuItem_Click;
                            gameOptionMenu.Items[i].Click += UseAPIIndexMenuItem_Click;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["useGamepadAPIIndexForGuestsMenuItem"]))
                        {
                            gameOptionMenu.Items[i].Visible = menuCurrentGameInfo.Game.MetaInfo.DisableProfiles ? false :
                                ((menuCurrentGameInfo.Game.Hook.XInputEnabled && !menuCurrentGameInfo.Game.Hook.XInputReroute && !menuCurrentGameInfo.Game.ProtoInput.DinputDeviceHook) ||
                                (menuCurrentGameInfo.Game.ProtoInput.XinputHook || menuCurrentGameInfo.Game.Hook.SDL2Enabled)) &&
                                (!menuCurrentGameInfo.Game.UseDevReorder && !menuCurrentGameInfo.Game.CreateSingleDeviceFile) &&
                                menuCurrentGameInfo.Game.PlayersPerInstance > 0 &&
                                !menuCurrentGameInfo.Game.Hook.DInputEnabled;

                            if (menuCurrentGameInfo.Game.MetaInfo.UseApiIndexForGuests)
                            {
                                gameOptionMenu.Items[i].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                            }
                            else
                            {
                                gameOptionMenu.Items[i].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                            }

                            gameOptionMenu.Items[i].Click -= UseApiIndexForGuestsMenuItem_Click;
                            gameOptionMenu.Items[i].Click += UseApiIndexForGuestsMenuItem_Click;
                           
                            //gameOptionMenu.Items[i].Visible = !menuCurrentGameInfo.Game.MetaInfo.UseApiIndex &&
                            //    !menuCurrentGameInfo.Game.MetaInfo.DisableProfiles;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["openUserProfConfigMenuItem"]))
                        {
                            (gameOptionMenu.Items["openUserProfConfigMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameOptionMenu.Items["deleteUserProfConfigMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();

                            var configsPath = menuCurrentGameInfo.Game.Profiles_ConfigPath;

                            if (configsPath.Count > 0)
                            {
                                if (!userConfigPathExists) { userConfigPathExists = true; }

                                foreach (string profileConfigPath in configsPath)
                                {
                                    string nucPrefix = "";
                                    try
                                    {
                                        if (Directory.GetParent(profileConfigPath).Name == "NucleusCoop")
                                        {
                                            nucPrefix = "Nucleus: ";
                                        }
                                    }
                                    catch
                                    {
                                        if(Globals.IsOneDriveEnabled)
                                        {
                                            (gameOptionMenu.Items["openUserProfConfigMenuItem"] as ToolStripMenuItem).DropDownItems.Add("/!\\ OneDrive /!\\", null, null);
                                            continue;
                                        }

                                        continue;
                                    }

                                    (gameOptionMenu.Items["openUserProfConfigMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profileConfigPath.TrimEnd('\\')), null, UserProfileOpenSubmenuItem_Click);
                                    (gameOptionMenu.Items["deleteUserProfConfigMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profileConfigPath.TrimEnd('\\')), null, UserProfileDeleteSubmenuItem_Click);
                                }
                            }
                        }

                        if (!userConfigPathExists)
                        {
                            gameOptionMenu.Items["openUserProfConfigMenuItem"].Visible = false;
                            gameOptionMenu.Items["deleteUserProfConfigMenuItem"].Visible = false;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["openUserProfSaveMenuItem"]))
                        {
                            (gameOptionMenu.Items["openUserProfSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameOptionMenu.Items["deleteUserProfSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();

                            var savesPath = menuCurrentGameInfo.Game.Profiles_SavePath;

                            if (savesPath.Count > 0)
                            {
                                if (!userSavePathExists) { userSavePathExists = true; }

                                foreach (string profileSavePath in savesPath)
                                {
                                    string nucPrefix = "";
                                    try
                                    {
                                        if (Directory.GetParent(profileSavePath).Name == "NucleusCoop")
                                        {
                                            nucPrefix = "Nucleus: ";
                                        }
                                    }
                                    catch
                                    {
                                        if (Globals.IsOneDriveEnabled)
                                        {
                                            (gameOptionMenu.Items["openUserProfSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add("/!\\ OneDrive /!\\", null, null);
                                            continue;
                                        }

                                        continue;
                                    }

                                    (gameOptionMenu.Items["openUserProfSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profileSavePath.TrimEnd('\\')), null, UserProfileOpenSubmenuItem_Click);
                                    (gameOptionMenu.Items["deleteUserProfSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profileSavePath.TrimEnd('\\')), null, UserProfileDeleteSubmenuItem_Click);
                                }
                            }
                        }

                        if (!userSavePathExists)
                        {
                            gameOptionMenu.Items["openUserProfSaveMenuItem"].Visible = false;
                            gameOptionMenu.Items["deleteUserProfSaveMenuItem"].Visible = false;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["openDocumentConfMenuItem"]))
                        {
                            (gameOptionMenu.Items["openDocumentConfMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameOptionMenu.Items["deleteDocumentConfMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();

                            var profileDocConfigPath = menuCurrentGameInfo.Game.Profiles_Documents_ConfigPath;

                            if (profileDocConfigPath.Count > 0)
                            {
                                if (!docConfigPathExists) { docConfigPathExists = true; }

                                foreach (string profilePath in profileDocConfigPath)
                                {
                                    string nucPrefix = "";
                                    try
                                    {
                                        if (Directory.GetParent(Directory.GetParent(profilePath).ToString()).Name == "NucleusCoop")
                                        {
                                            nucPrefix = "Nucleus: ";
                                        }
                                    }
                                    catch
                                    {
                                        if (Globals.IsOneDriveEnabled)
                                        {
                                            (gameOptionMenu.Items["openDocumentConfMenuItem"] as ToolStripMenuItem).DropDownItems.Add("/!\\ OneDrive /!\\", null, null);
                                            continue;
                                        }

                                        continue;
                                    }

                                    (gameOptionMenu.Items["openDocumentConfMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, DocOpenSubmenuItem_Click);
                                    (gameOptionMenu.Items["deleteDocumentConfMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, DocDeleteSubmenuItem_Click);
                                }
                            }
                        }

                        if (!docConfigPathExists)
                        {
                            gameOptionMenu.Items["openDocumentConfMenuItem"].Visible = false;
                            gameOptionMenu.Items["deleteDocumentConfMenuItem"].Visible = false;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["openDocumentSaveMenuItem"]))
                        {
                            (gameOptionMenu.Items["openDocumentSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameOptionMenu.Items["deleteDocumentSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();

                            var profileDocSavePath = menuCurrentGameInfo.Game.Profiles_Documents_SavePath;

                            if (profileDocSavePath.Count > 0)
                            {
                                if (!docSavePathExists) { docSavePathExists = true; }

                                foreach (string profilePath in profileDocSavePath)
                                {
                                    string nucPrefix = "";

                                    try
                                    {
                                        if (Directory.GetParent(Directory.GetParent(profilePath).ToString()).Name == "NucleusCoop")
                                        {
                                            nucPrefix = "Nucleus: ";
                                        }
                                    }
                                    catch
                                    {
                                        if (Globals.IsOneDriveEnabled)
                                        {
                                            (gameOptionMenu.Items["openDocumentSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add("/!\\ OneDrive /!\\", null, null);
                                            continue;
                                        }

                                        continue;
                                    }

                                    (gameOptionMenu.Items["openDocumentSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, DocOpenSubmenuItem_Click);
                                    (gameOptionMenu.Items["deleteDocumentSaveMenuItem"] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, DocDeleteSubmenuItem_Click);
                                }
                            }

                            if (!userConfigPathExists && !userSavePathExists && !docConfigPathExists && !docSavePathExists)
                            {
                                gameOptionMenu.Items["menuSeparator2"].Visible = false;
                            }
                        }

                        if (!docSavePathExists)
                        {
                            gameOptionMenu.Items["openDocumentSaveMenuItem"].Visible = false;
                            gameOptionMenu.Items["deleteDocumentSaveMenuItem"].Visible = false;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["openBackupFolderMenuItem"]) /*&& !Globals.IsOneDriveEnabled*/)
                        {
                            (gameOptionMenu.Items["openBackupFolderMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameOptionMenu.Items["deleteBackupFolderMenuItem"] as ToolStripMenuItem).DropDownItems.Clear();

                            var playersBackupPath = menuCurrentGameInfo.Game.Players_BackupPath;

                            if (playersBackupPath.Count > 0)
                            {
                                foreach (string playerBackup in playersBackupPath)
                                {
                                    string path = playerBackup;
                                    string playerName = playerBackup.Split('\\').Last();
                                    
                                    (gameOptionMenu.Items["openBackupFolderMenuItem"] as ToolStripMenuItem).DropDownItems.Add(playerName, null, OpenBackupFolderSubmenuItem_Click);                                 
                                    (gameOptionMenu.Items["deleteBackupFolderMenuItem"] as ToolStripMenuItem).DropDownItems.Add(playerName, null, DeleteBackupFolderSubmenuItem_Click);
                                }

                                backupFolderExist = true;
                            }
                        }

                        if (!backupFolderExist)
                        {
                            gameOptionMenu.Items["openBackupFolderMenuItem"].Visible = false;
                            gameOptionMenu.Items["deleteBackupFolderMenuItem"].Visible = false;
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["notesMenuItem"]) && menuCurrentGameInfo.Game.Description == null)
                        {
                            gameOptionMenu.Items["notesMenuItem"].Visible = false;
                            if (isButton)
                            {
                                gameOptionMenu.Items["detailsMenuItem"].Visible = false;
                                i++;
                            }
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["keepInstancesFolderMenuItem"]))
                        {
                            if (!menuCurrentGameInfo.Game.KeepSymLinkOnExit)
                            {
                                if (menuCurrentGameInfo.Game.MetaInfo.KeepSymLink)
                                {
                                    gameOptionMenu.Items["keepInstancesFolderMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                                }
                                else
                                {
                                    gameOptionMenu.Items["keepInstancesFolderMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                                }

                                gameOptionMenu.Items["keepInstancesFolderMenuItem"].Click -= KeepInstancesFolderMenuItem_Click;
                                gameOptionMenu.Items["keepInstancesFolderMenuItem"].Click += KeepInstancesFolderMenuItem_Click;
                            }
                            else
                            {
                                gameOptionMenu.Items["keepInstancesFolderMenuItem"].Click -= KeepInstancesFolderMenuItem_Click;
                                gameOptionMenu.Items["keepInstancesFolderMenuItem"].Visible = false;
                            }
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["disableProfilesMenuItem"]))
                        {
                            if (!Core_Interface.DisableGameProfiles)
                            {
                                if (menuCurrentGameInfo.Game.MetaInfo.DisableProfiles)
                                {
                                    gameOptionMenu.Items["disableProfilesMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                                }
                                else
                                {
                                    gameOptionMenu.Items["disableProfilesMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                                }

                                gameOptionMenu.Items["disableProfilesMenuItem"].Click -= DisableProfilesMenuItem_Click;
                                gameOptionMenu.Items["disableProfilesMenuItem"].Click += DisableProfilesMenuItem_Click;
                            }
                            else
                            {
                                gameOptionMenu.Items["disableProfilesMenuItem"].Visible = false;
                            }
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["gameAssetsMenuItem"]))
                        {
                            ToolStripMenuItem gameAssetsMenuItem = gameOptionMenu.Items["gameAssetsMenuItem"] as ToolStripMenuItem;
                            if (gameAssetsMenuItem.DropDownItems.Count > 0)
                            {
                                int visibleCount = 0;

                                for (int d = 0; d < gameAssetsMenuItem.DropDownItems.Count; d++)
                                {
                                    ToolStripItem subItem = gameAssetsMenuItem.DropDownItems[d];

                                    if (subItem.Name == "screenshotsMenuItem")
                                    {
                                        bool showItem = Directory.Exists(Path.Combine(Application.StartupPath, $@"gui\screenshots\{menuCurrentControl.UserGameInfo.GameGuid}"));
                                        subItem.Visible = showItem;
                                        if (showItem)
                                            visibleCount++;
                                        subItem.Click -= ScreenshotsMenuItem_Click;
                                        subItem.Click += ScreenshotsMenuItem_Click;
                                    }

                                    if (subItem.Name == "coverMenuItem")
                                    {
                                        bool showItem = File.Exists(Path.Combine(Application.StartupPath, $@"gui\covers\{menuCurrentControl.UserGameInfo.GameGuid}.jpeg"));
                                        subItem.Visible = showItem;
                                        if (showItem)
                                            visibleCount++;
                                        subItem.Click -= CoverMenuItem_Click;
                                        subItem.Click += CoverMenuItem_Click;
                                    }
                                }

                                gameOptionMenu.Items[i].Visible = visibleCount > 0;
                            }
                        }

                        if (i == gameOptionMenu.Items.IndexOf(gameOptionMenu.Items["disableHandlerUpdateMenuItem"]))
                        {
                            gameOptionMenu.Items["disableHandlerUpdateMenuItem"].ImageAlign = ContentAlignment.MiddleCenter;
                            gameOptionMenu.Items["disableHandlerUpdateMenuItem"].ImageScaling = ToolStripItemImageScaling.SizeToFit;

                            if (menuCurrentGameInfo.Game.MetaInfo.CheckUpdate)
                            {
                                gameOptionMenu.Items["disableHandlerUpdateMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                            }
                            else
                            {
                                gameOptionMenu.Items["disableHandlerUpdateMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                            }

                            gameOptionMenu.Items["disableHandlerUpdateMenuItem"].Click -= DisableHandlerUpdateMenuItem_Click;
                            gameOptionMenu.Items["disableHandlerUpdateMenuItem"].Click += DisableHandlerUpdateMenuItem_Click;

                        }

                        gameOptionMenu.Items["gameNameMenuItem"].Visible = (!isButton) || !UI_Interface.SetupPanel.Visible;
                        
                        gameOptionMenu.Items["detailsMenuItem"].Visible = (!isButton && Core_Interface.Current_UserGameInfo != menuCurrentGameInfo) || !UI_Interface.SetupPanel.Visible;
                        gameOptionMenu.Items["detailsMenuItem"].Click -= DetailsToolStripMenuItem_Click;
                        gameOptionMenu.Items["detailsMenuItem"].Click += DetailsToolStripMenuItem_Click;

                        gameOptionMenu.Items["notesMenuItem"].Visible = (!isButton && Core_Interface.Current_UserGameInfo != menuCurrentGameInfo) || !UI_Interface.SetupPanel.Visible;
                        gameOptionMenu.Items["menuSeparator1"].Visible = (!isButton && Core_Interface.Current_UserGameInfo != menuCurrentGameInfo) || !UI_Interface.SetupPanel.Visible;
                    }

                    foreach (ToolStripMenuItem menuItem in gameOptionMenu.Items.OfType<ToolStripMenuItem>())
                    {
                        if (menuItem != gameOptionMenu.Items["disableProfilesMenuItem"] && menuItem != gameOptionMenu.Items["useAPIIndexMenuItem"] && 
                            menuItem != gameOptionMenu.Items["keepInstancesFolderMenuItem"] && menuItem != gameOptionMenu.Items["gameNameMenuItem"] && 
                            menuItem != gameOptionMenu.Items["disableHandlerUpdateMenuItem"] && menuItem != gameOptionMenu.Items["useGamepadAPIIndexForGuestsMenuItem"])
                        {
                            menuItem.DisplayStyle = ToolStripItemDisplayStyle.Text;
                        }

                        if (menuItem.DropDownItems.Count > 0)
                        {
                            menuItem.DropDown.BackgroundImage = gameOptionMenu.BackgroundImage;

                            for (int d = 0; d < menuItem.DropDownItems.Count; d++)
                            {
                                menuItem.DropDownItems[d].BackColor = menuItem.DropDownItems[d] is ToolStripComboBox ? Color.Black : Color.Transparent;
                                menuItem.DropDownItems[d].ForeColor = gameOptionMenu.ForeColor;

                                if (menuItem.Name == "steamLanguage")
                                {
                                    if (menuItem.DropDownItems[d] is ToolStripComboBox steamLang)
                                    {
                                        menuItem.Visible = false;
                                        if ((menuCurrentGameInfo.Game.SteamID != null && menuCurrentGameInfo.Game.SteamID != "") &&
                                            menuCurrentGameInfo.Game.UseGoldberg ||
                                            menuCurrentGameInfo.Game.UseGoldbergNoOGSteamDlls ||
                                            menuCurrentGameInfo.Game.NeedsSteamEmulation)
                                        {
                                            steamLang.ComboBox.Text = menuCurrentGameInfo.Game.MetaInfo.SteamLanguage;
                                            steamLang.TextChanged -= SteamLangCb_TextChanged;
                                            steamLang.TextChanged += SteamLangCb_TextChanged;
                                            menuItem.Visible = true;
                                        }
                                    }
                                }
                            }

                            ((ToolStripDropDownMenu)menuItem.DropDown).ShowImageMargin = false;
                        }
                    }
                }

                bool contentExists = Directory.Exists(menuCurrentGameInfo.Game.Content_Folder);
                gameOptionMenu.Items["openContentFolderMenuItem"].Visible = contentExists;
                gameOptionMenu.Items["deleteContentFolderMenuItem"].Visible = contentExists;
            }
            else
            {
                for (int i = 0; i < gameOptionMenu.Items.Count; i++)
                {
                    gameOptionMenu.Items[i].Visible = false;
                }
            }
        }

        private static void GameOptionMenuFunc_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        public static void GameOptionMenu_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip gameOptionMenu = sender as ContextMenuStrip;
            FormGraphicsUtil.CreateRoundedControlRegion(gameOptionMenu, 2, 2, gameOptionMenu.Width - 1, gameOptionMenu.Height, 20, 20);
        }

        public static void GameOptionMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (UI_Interface.CurrentGameListControl != null)
                menuCurrentControl = UI_Interface.CurrentGameListControl;
        }

        public static void DetailsToolStripMenuItem_Click(object sender, EventArgs e) => GetGameDetails.GetDetails(menuCurrentGameInfo);

        public static void RemoveGameMenuItem_Click(object sender, EventArgs e) => RemoveGame.Remove(menuCurrentGameInfo, false);

        public static void OpenBackupFolderSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string backupsPath = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\_Game Files Backup_\{menuCurrentGameInfo.Game.GUID}";

            string path = $@"{backupsPath}\{item.Text}";

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        public static void DeleteBackupFolderSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            string backupsPath = $@"{Globals.UserEnvironmentRoot}\NucleusCoop\_Game Files Backup_\{menuCurrentGameInfo.Game.GUID}";

            string path = $@"{backupsPath}\{item.Text}";

            DialogResult dialogResult = MessageBox.Show($"Do you really want to delete \"{menuCurrentControl.GameInfo.GUID}\" {item.Text}'s backup folder?", $"Delete {item.Text}'s backup folder.", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }
            }
        }

        public static void UserProfileOpenSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = menuCurrentGameInfo.Game.UserProfileConfigPath;
            }
            else
            {
                pathSuffix = menuCurrentGameInfo.Game.UserProfileSavePath;
            }

            string path;

            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\", pathSuffix);
            }
            else
            {
                path = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), pathSuffix);
            }

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        public static void UserProfileDeleteSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = menuCurrentGameInfo.Game.UserProfileConfigPath;
            }
            else
            {
                pathSuffix = menuCurrentGameInfo.Game.UserProfileSavePath;
            }

            string path;
            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Globals.UserEnvironmentRoot}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\", pathSuffix);
            }
            else
            {
                path = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), pathSuffix);
            }

            if (Directory.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                }
            }
        }

        public static void DocOpenSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = menuCurrentGameInfo.Game.DocumentsConfigPath;
            }
            else
            {
                pathSuffix = menuCurrentGameInfo.Game.DocumentsSavePath;
            }

            string path;

            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\Documents", pathSuffix);
            }
            else
            {
                path = Path.Combine(Globals.UserDocumentsRoot, pathSuffix);
            }

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        public static void DocDeleteSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = menuCurrentGameInfo.Game.DocumentsConfigPath;
            }
            else
            {
                pathSuffix = menuCurrentGameInfo.Game.DocumentsSavePath;
            }

            string path;

            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Path.GetDirectoryName(Globals.UserDocumentsRoot)}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\Documents", pathSuffix);
            }
            else
            {
                path = Path.Combine(Globals.UserDocumentsRoot, pathSuffix);
            }

            if (Directory.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                }
            }
        }

        public static void OpenHandlerMenuItem_Click(object sender, EventArgs e)
       => OpenHandler.OpenRawHandler(menuCurrentGameInfo);

        public static void OpenContentFolderMenuItem_Click(object sender, EventArgs e)
        => OpenGameContentFolder.OpenContentFolder(menuCurrentGameInfo);

        public static void DeleteContentFolderMenuItem_Click(object sender, EventArgs e)
        {
            CleanGameContent.CleanContentFolder(menuCurrentGameInfo.Game, true);
        }

        public static void ChangeIconMenuItem_Click(object sender, EventArgs e)
        => ChangeGameIcon.ChangeIcon(menuCurrentGameInfo);

        public static void OpenOrigExePathMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(menuCurrentGameInfo.ExePath);
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Unable to open original executable path for this game.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void NotesMenuItem_Click(object sender, EventArgs e) => NucleusMessageBox.Show("Handler Author's Notes", menuCurrentGameInfo.Game.Description, true);

        public static void DisableProfilesMenuItem_Click(object sender, EventArgs e)
        {
            UI_Actions.ProfileEnabled_Change.Invoke();
        }

        public static void KeepInstancesFolderMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem instancesFolderMenuItem = sender as ToolStripMenuItem;

            if (menuCurrentGameInfo.Game.MetaInfo.KeepSymLink)
            {
                menuCurrentGameInfo.Game.MetaInfo.KeepSymLink = false;
                instancesFolderMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
            }
            else
            {
                menuCurrentGameInfo.Game.MetaInfo.KeepSymLink = true;
                instancesFolderMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
            }
        }

        public static void UseAPIIndexMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem useAPIIndexMenuItem = sender as ToolStripMenuItem;

            if (menuCurrentGameInfo.Game.MetaInfo.UseApiIndex)
            {
                menuCurrentGameInfo.Game.MetaInfo.UseApiIndex = false;
                menuCurrentGameInfo.Game.MetaInfo.UseApiIndexForGuests = false;
                useAPIIndexMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
            }
            else
            {
                menuCurrentGameInfo.Game.MetaInfo.UseApiIndex = true;
                useAPIIndexMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
            }

            if (Core_Interface.CurrentMenuUserGameInfo == Core_Interface.Current_UserGameInfo)
            {
                if (GameProfile.Loaded)
                {
                    GameProfile.Instance?.Reset();
                }

                if (Core_Interface.StepsList != null)
                {
                    Core_Interface.GoToStep(0);
                }

                GameProfile.UseXinputIndex = menuCurrentGameInfo.Game.MetaInfo.UseApiIndex;
            }
        }

        public static void UseApiIndexForGuestsMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem useGamepadAPIIndexForGuestsMenuItem = sender as ToolStripMenuItem;

            if (menuCurrentGameInfo.Game.MetaInfo.UseApiIndexForGuests)
            {
                menuCurrentGameInfo.Game.MetaInfo.UseApiIndexForGuests = false;
                useGamepadAPIIndexForGuestsMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
            }
            else
            {
                menuCurrentGameInfo.Game.MetaInfo.UseApiIndexForGuests = true;
                useGamepadAPIIndexForGuestsMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
            }

            if (Core_Interface.CurrentMenuUserGameInfo == Core_Interface.Current_UserGameInfo)
            {
                if (GameProfile.Loaded)
                {
                    GameProfile.Instance?.Reset();
                }

                if (Core_Interface.StepsList != null)
                {
                    Core_Interface.GoToStep(0);
                }
                //GameProfile.UseXinputIndex = menuCurrentGameInfo.Game.MetaInfo.UseApiIndex;
            }
        }

        public static void CoverMenuItem_Click(object sender, EventArgs e)
        {
            string coverPath = Path.Combine(Application.StartupPath, $@"gui\covers");

            if (File.Exists($@"{coverPath}\{menuCurrentGameInfo.GameGuid}.jpeg"))
            {
                Process.Start(coverPath);
            }
        }

        public static void ScreenshotsMenuItem_Click(object sender, EventArgs e)
        {
            string screenshotsPath = Path.Combine(Application.StartupPath, $@"gui\screenshots\{menuCurrentGameInfo.GameGuid}");
            if (Directory.Exists(screenshotsPath))
            {
                Process.Start(screenshotsPath);
            }
        }

        public static void DisableHandlerUpdateMenuItem_Click(object sender, EventArgs e)
        {
            UI_Interface.MainForm.Invoke((MethodInvoker)delegate ()
            {
                System.Threading.Tasks.Task.Run(() =>
                {
                    bool update = false;

                    ToolStripMenuItem disableHandlerUpdateMenuItem = sender as ToolStripMenuItem;

                    if (menuCurrentGameInfo.Game.MetaInfo.CheckUpdate)
                    {
                        menuCurrentGameInfo.Game.MetaInfo.CheckUpdate = update;
                        menuCurrentGameInfo.Game.UpdateAvailable = update;
                        disableHandlerUpdateMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");
                    }
                    else
                    {
                        menuCurrentGameInfo.Game.MetaInfo.CheckUpdate = true;
                        update = menuCurrentGameInfo.Game.Hub.CheckUpdateAvailable();
                        menuCurrentGameInfo.Game.UpdateAvailable = update;
                        disableHandlerUpdateMenuItem.Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");
                    }
                });
            });
        }

        private static void SteamLangCb_TextChanged(object sender, EventArgs e)
        {
            ToolStripComboBox cb = sender as ToolStripComboBox;
            menuCurrentGameInfo.Game.MetaInfo.SteamLanguage = cb.Text;
        }       
    }
}
