using Nucleus.Coop.Controls;
using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class UI_Actions
    {
        public static Action On_GameChange;
        public static Action Goto_Home;
        public static Action Handler_Start;
        public static Action Handler_End;
        public static Action ProfileEnabled_Change;

        public static void InitUIActions()
        {
            On_GameChange += ChangeGame;
            On_GameChange += SetPlayTime;           
            DevicesFunctions.OnAssignedDeviceDisconnect += DeviceDisconnected_CallBack;
            ProfileEnabled_Change += SetProfileState;
        }

        private static void DeviceDisconnected_CallBack(string data)
        {
            UIThread.Post(() =>
            {
                if (GameProfile.Loaded)
                {
                    Core_Interface.GoToStep(0);
                    UI_Interface.ProfilesList.Update_Reload();
                }
                else
                {
                    Core_Interface.GoToStep(0);
                }
            });

            string[] dataArr = data.Split('|');
            Globals.MainOSD.Show(3000, $@"*@{OSDHighlightColors.Info}|/!\ Device Connection Lost or Updated For@* *@{OSDHighlightColors.Regular}|{dataArr[0]}@* *@{OSDHighlightColors.Info}|({dataArr[1]}) /!\@*");
        }

        private static void ChangeGame()
        {
            if (!CheckGameRequirements.MatchRequirements(Core_Interface.Current_UserGameInfo))
            {
                UI_Functions.RefreshUI(true);
                UI_Interface.MainForm.Invalidate(false);
                return;
            }

            var currentGame = Core_Interface.Current_GenericGameInfo;

            bool keepSymlink = currentGame.KeepSymLinkOnExit || currentGame.MetaInfo.KeepSymLink;

            if(!keepSymlink)
            {
                CleanGameContent.CleanContentFolder(currentGame, false);
            }
          
            UI_Interface.SaveProfileSwitch.RadioChecked = currentGame.MetaInfo.SaveProfile;

            UI_Interface.CoverFrame.BackColor = Color.Transparent;

            UI_Interface.ProfileButtonsPanel.Visible = false;
            UI_Interface.BigLogo.Visible = false;
            UI_Interface.ProfileSettings.Visible = false;
            UI_Interface.HandlerNotesZoom.Visible = false;
            UI_Interface.GotoNext.Visible = false;
            UI_Interface.GotoPrev.Visible = false;
            UI_Interface.IconsContainer.Visible = false;

            if (!UI_Graphics.IsNotesTitleScrollTimerRunning)
            {
                if (UI_Graphics.NotesTitleScrollTimer == null)
                {
                    UI_Graphics.NotesTitleScrollTimer?.Dispose();
                    UI_Graphics.NotesTitleScrollTimer = new System.Windows.Forms.Timer();
                    UI_Graphics.NotesTitleScrollTimer.Interval = 16; //millisecond                   
                    UI_Graphics.NotesTitleScrollTimer.Tick += UI_Graphics.NotesTitleScrollTimer_Tick;
                }

                UI_Graphics.NotesTitleScrollTimer.Start();
                UI_Graphics.IsNotesTitleScrollTimerRunning = true;
            }           

            foreach (Control icon in UI_Interface.IconsContainer.Controls)
            {
                icon.Dispose();
            }

            UI_Interface.IconsContainer.Controls.Clear();
            UI_Interface.IconsContainer.Controls.AddRange(InputIcons.SetInputsIcons(currentGame));
            UI_Interface.IconsContainer.Visible = true;

            UI_Interface.PlayButton.Visible = false;
            
            UI_Interface.SetupPanel.Visible = true;

            GameProfile newProfile = new GameProfile();
            newProfile.InitializeDefault();

            Core_Interface.StepsList = new List<UserInputControl> { UI_Interface.SetupScreen, Core_Interface.OptionsControl };

            Core_Interface.JsControl?.Dispose();

            Core_Interface.JsControl = new JSUserInputControl();
            Core_Interface.JsControl.OnCanPlayUpdated += Core_Interface.StepCanPlay;

            for (int i = 0; i < currentGame.CustomSteps.Count; i++)
            {
                Core_Interface.StepsList.Add(Core_Interface.JsControl);
            }

            if (!Core_Interface.DisableGameProfiles && !currentGame.MetaInfo.DisableProfiles)
            {
                UI_Interface.ProfileSettingsButton.Visible = true;

                ProfilesList.Instance.Update_ProfilesList();

                if(ProfilesList.UIAction_OnUnload_Click == null)
                {
                    ProfilesList.UIAction_OnUnload_Click -= UI_Functions.ResetPlayButton;
                    ProfilesList.UIAction_OnShowPreview -= UI_Functions.ShowProfilePreview;
                    ProfilesList.UIAction_OnUnload_Click += UI_Functions.ResetPlayButton;
                    ProfilesList.UIAction_OnShowPreview += UI_Functions.ShowProfilePreview;
                }

                bool showList = GameProfile.profilesPathList.Count > 0;

                if (!currentGame.MetaInfo.FirstLaunch)
                {
                    UI_Interface.ProfilesList.Visible = showList;
                }

                CustomToolTips.SetToolTip(UI_Interface.ProfileListButton, $"{currentGame.GameName} profiles list.", "profilesList_btn", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });   

                UI_Interface.ProfileListButton.Visible = showList;
                UI_Functions.SetGameProfileButtonLoc();
                UI_Functions.SetCoverLocation(true);
             
                UI_Interface.ProfileButtonsPanel.Visible = true;

                ProfilesList.Instance.Locked = false;
            }
            else
            {
                UI_Interface.ProfilesList.Visible = false;
                UI_Interface.ProfileButtonsPanel.Visible = false;
                UI_Functions.SetCoverLocation(false);
            }

            if (currentGame.Description?.Length > 0)
            {
                UI_Interface.HandlerNotes.Visible = false;
                UI_Interface.HandlerNotes.Text = currentGame.Description;
                UI_Interface.HandlerNotes.Visible = true;
                UI_Interface.HandlerNotesContainer.Visible = true;

                if (currentGame.MetaInfo.FirstLaunch && !UI_Interface.DisableForcedNote)
                {
                    UI_Functions.ExpandHandlerNotesButton_Click(UI_Interface.ExpandHandlerNotesButton, null);
                }
            }
            else
            {
                UI_Interface.HandlerNotesContainer.Visible = false;
                UI_Interface.HandlerNotes.Text = "";
                UI_Interface.HandlerNotesZoom.Visible = false;
            }

            Core_Interface.HandlerContent?.Dispose();

            // content manager is shared within the same game
            Core_Interface.HandlerContent = new ContentManager(currentGame);
           
            UI_Interface.GotoNext.Visible = currentGame.Options?.Count > 0;
            UI_Interface.GotoPrev.Visible = currentGame.Options?.Count > 0;
            UI_Interface.InfoPanel.Visible = true;

            Core_Interface.GoToStep(0);
           
            GameAssets.LoadGameAssets();              

            UI_Interface.SetupScreen.Focus();
        }

        private static void SetPlayTime()
        {
            UI_Interface.PlayTimePanel.Visible = false;
            UI_Interface.PlayTimePanel.Playtime = Core_Interface.Current_GameMetaInfo.TotalPlayTime;
            UI_Interface.PlayTimePanel.LastPlayed = Core_Interface.Current_GameMetaInfo.LastPlayedAt;
            UI_Interface.PlayTimePanel.Visible = true;
        }

        public static void SetProfileState()
        {
            if (Core_Interface.CurrentMenuUserGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.DisableProfiles)
            {
                Core_Interface.CurrentMenuUserGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.DisableProfiles = false;
                UI_Interface.GameOptionMenu.Items["disableProfilesMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "locked.png");

                if (Core_Interface.CurrentMenuUserGameInfo == Core_Interface.Current_UserGameInfo)
                {
                    GameProfile.Instance.InitializeDefault();
                    ProfilesList.Instance.Update_ProfilesList();

                    bool showList = GameProfile.profilesPathList.Count > 0;

                    if (!Core_Interface.CurrentMenuUserGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.FirstLaunch)
                    {
                        UI_Interface.ProfilesList.Visible = showList;
                    }

                    UI_Interface.ProfileListButton.Visible = showList;
                    UI_Interface.ProfileSettingsButton.Location = showList ? (Point)UI_Interface.ProfileSettingsButton.Tag : UI_Interface.ProfileListButton.Location;
                    UI_Interface.SaveProfileSwitch.Location = new Point(UI_Interface.ProfileSettingsButton.Right + 5, UI_Interface.SaveProfileSwitch.Location.Y);
                    UI_Interface.ProfileSettingsButton.Visible = true;

                    CustomToolTips.SetToolTip(UI_Interface.ProfileSettingsButton, $"{Core_Interface.Current_GenericGameInfo.GameName} profiles list.", "profilesList_btn", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

                    if (Core_Interface.StepsList != null)
                    {
                        Core_Interface.GoToStep(0);
                    }

                    UI_Functions.SetCoverLocation(true);
                    UI_Interface.InfoPanel.Refresh();

                    UI_Interface.ProfileButtonsPanel.Visible = true;
                }
            }
            else
            {
                Core_Interface.CurrentMenuUserGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.DisableProfiles = true;
                UI_Interface.GameOptionMenu.Items["disableProfilesMenuItem"].Image = ImageCache.GetImage(Globals.ThemeFolder + "unlocked.png");

                if (Core_Interface.CurrentMenuUserGameInfo == Core_Interface.Current_UserGameInfo)
                {
                    GameProfile.Instance.InitializeDefault();
                    UI_Interface.ProfilesList.Visible = false;
                    UI_Interface.ProfileListButton.Visible = false;
                    UI_Interface.ProfileSettingsButton.Visible = false;
                    UI_Interface.ProfileButtonsPanel.Visible = false;
                    UI_Functions.SetCoverLocation(false);
                    UI_Interface.InfoPanel.Refresh();
                }

                if (Core_Interface.StepsList != null)
                {
                    Core_Interface.GoToStep(0);
                }

                UI_Interface.ProfileButtonsPanel.Visible = false;
            }
        }

    }
}
