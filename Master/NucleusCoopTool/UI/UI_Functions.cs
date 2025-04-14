using Nucleus.Coop.Controls;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nucleus.Gaming.Cache;
using System.Diagnostics;
using Nucleus.Gaming.Windows.Interop;
using Nucleus.Coop.Forms;
using System.Linq;
using Nucleus.Coop.Tools;
using System.IO;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;

namespace Nucleus.Coop.UI
{
    public static class UI_Functions
    {       
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public static void Logo_Click(object sender, EventArgs e) => Process.Start(Links.NC_Repo);

        public static void DonationButton_Click(object sender, EventArgs e) => Process.Start(Links.NC_Patreon);

        public static void CloseButton_Click(object sender, EventArgs e) => MainWindowFunc.FadeOut();
        public static void CloseButton_MouseEnter(object sender, EventArgs e) => UI_Interface.CloseButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        public static void CloseButton_MouseLeave(object sender, EventArgs e) => UI_Interface.CloseButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");

        public static void MinimizeButton_Click(object sender, EventArgs e) => UI_Interface.MainForm.WindowState = FormWindowState.Minimized;
        public static void MinimizeButton_MouseLeave(object sender, EventArgs e) => UI_Interface.MinimizeButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_minimize.png");
        public static void MinimizeButton_MouseEnter(object sender, EventArgs e) => UI_Interface.MinimizeButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_minimize_mousehover.png");

        public static void MaximizeButton_Click(object sender, EventArgs e)
        {
            UI_Interface.MainForm.WindowState = UI_Interface.MainForm.WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            UI_Interface.MaximizeButton.BackgroundImage = UI_Interface.MainForm.WindowState == FormWindowState.Maximized ? ImageCache.GetImage(Globals.ThemeFolder + "title_windowed.png") : ImageCache.GetImage(Globals.ThemeFolder + "title_maximize.png");
        }

        public static void MaximizeButton_MouseEnter(object sender, EventArgs e) => 
            UI_Interface.MaximizeButton.BackgroundImage = UI_Interface.MaximizeButton.BackgroundImage = UI_Interface.MainForm.WindowState == FormWindowState.Maximized ? 
            ImageCache.GetImage(Globals.ThemeFolder + "title_windowed_mousehover.png") : ImageCache.GetImage(Globals.ThemeFolder + "title_maximize_mousehover.png");

        public static void MaximizeButton_MouseLeave(object sender, EventArgs e) => 
            UI_Interface.MaximizeButton.BackgroundImage = UI_Interface.MainForm.WindowState == FormWindowState.Maximized ? 
            ImageCache.GetImage(Globals.ThemeFolder + "title_windowed.png") : ImageCache.GetImage(Globals.ThemeFolder + "title_maximize.png");

        public static void VirtualMouseToggle_MouseClick(object sender, MouseEventArgs e)
        {
            PictureBox navPb = sender as PictureBox;

            if (e.Button == MouseButtons.Left)
            {
                App_GamePadNavigation.Enabled = !App_GamePadNavigation.Enabled;
                GamepadNavigation.UpdateUINavSettings();
                navPb.Invalidate();
            }
            else if (e.Button == MouseButtons.Right)
            {
                UI_Interface.Xinput_S_Setup.Visible = !UI_Interface.Xinput_S_Setup.Visible;
            }
        }

        public static void ExtractHandlerButton_Click(object sender, EventArgs e)=> ExtractHandler.Extract();
        public static void ExtractHandlerButton_MouseEnter(object sender, EventArgs e) => UI_Interface.ExtractHandlerButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "extract_nc_mousehover.png");
        public static void ExtractHandlerButton_MouseLeave(object sender, EventArgs e) => UI_Interface.ExtractHandlerButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "extract_nc.png");

        public static void SearchGameButton_Click(object sender, EventArgs e) => SearchGame.Search(null, null);
        public static void SearchGameButton_MouseEnter(object sender, EventArgs e) => UI_Interface.SearchGameButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "search_game_mousehover.png");
        public static void SearchGameButton_MouseLeave(object sender, EventArgs e) => UI_Interface.SearchGameButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "search_game.png");

        public static void OpenLogButton_Click(object sender, EventArgs e)
        {
            if (File.Exists(Path.Combine(Application.StartupPath, "debug-log.txt")))
            {
                OpenDebugLog.OpenDebugLogFile();
                return;
            }

            Globals.MainOSD.Show(2000, "No Available Log");
        }

        public static void OpenLogButton_MouseEnter(object sender, EventArgs e) => UI_Interface.OpenLogButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "log_mousehover.png");
        public static void OpenLogButton_MouseLeave(object sender, EventArgs e) => UI_Interface.OpenLogButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "log.png");

        public static void GameList_SelectedChanged(object arg1, Control arg2)
        {
            if (GenericGameHandler.Instance != null)
            {
                if (!GenericGameHandler.Instance.HasEnded)
                {
                    var selected = (GameControl)arg1;
                    selected.RadioUnselected();
                    selected.Invalidate();
                    return;
                }
            }

            if ((GameControl)arg1 == null)
            {
                return;
            }

            UI_Interface.CurrentGameListControl = (GameControl)arg1;

            Core_Interface.Current_UserGameInfo = UI_Interface.CurrentGameListControl.UserGameInfo;
        }

        public static void WindowPanelMouseDown(object sender, MouseEventArgs e)
        {
            DoubleBufferPanel windowPanel = (DoubleBufferPanel)sender;

            Generic_Functions.ClickAnyControl(windowPanel, null);

            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, UI_Interface.MainForm.Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }

            if (UI_Interface.Settings.Visible)
            {
                UI_Interface.Settings.BringToFront();
            }

            if (UI_Interface.ProfileSettings.Visible)
            {
                UI_Interface.ProfileSettings.BringToFront();
            }
        }

        public static void SaveProfileSwitch_Click(object sender, EventArgs e)
        {
            CustomSwitch radio = (CustomSwitch)sender;
            Core_Interface.Current_GameMetaInfo.SaveProfile = radio.RadioChecked;
        }

        public static void DwldAssetsButton_Click(object sender, EventArgs e)
        {
            if (Core_Interface.GameManager.User.Games.Count == 0)
            {
                Globals.MainOSD.Show(1600, $"Add Game(s) to Your List");
                return;
            }

            AssetsDownloader.DownloadAllGamesAssets(UI_Interface.CurrentGameListControl);
        }

        public static void DwldAssetsButton_MouseEnter(object sender, EventArgs e) => UI_Interface.DwldAssetsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_download_assets_mousehover.png");

        public static void DwldAssetsButton_MouseLeave(object sender, EventArgs e) => UI_Interface.DwldAssetsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_download_assets.png");

        public static void TutorialButton_MouseEnter(object sender, EventArgs e) =>
            UI_Interface.TutorialButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "instruction_opened.png");

        public static void TutorialButton_MouseLeave(object sender, EventArgs e) => 
            UI_Interface.TutorialButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "instruction_closed.png");

        public static void GotoPrev_Click(object sender, EventArgs e) => Core_Interface.GotoPrevStep();

        public static void GotoNext_Click(object sender, EventArgs e) => Core_Interface.GotoNextStep();

        public static void GotoPrevEnabledChanged(object sender, EventArgs e)
        {
            Button stepBtn = (Button)sender;        
        }

        public static void GotoNextEnabledChanged(object sender, EventArgs e)
        {
            Button stepBtn = (Button)sender;
        }

        public static void Play_Click(object sender, EventArgs e) => Core_Interface.PlayClicked();
        
        public static void ProfileSettingsButton_Click(object sender, EventArgs e)
        {
            if (UI_Interface.Settings.Visible)
            {
                return;
            }

            if (!UI_Interface.ProfileSettings.Visible)
            {
                ProfilesList.Instance.Locked = true;
                ProfileSettings.UpdateProfileSettingsUiValues();
                UI_Interface.ProfileSettings.SetVisible();
                UI_Interface.ProfileSettings.BringToFront();
            }
            else
            {
                UI_Interface.ProfileSettings.BringToFront();
            }
        }

        public static void SettingsButton_MouseEnter(object sender, EventArgs e)
        { 
            if (UI_Interface.ProfileSettings.Visible) 
            { 
                return; 
            } 
            
            UI_Interface.SettingsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_settings_mousehover.png"); 
        }

        public static void SettingsButton_MouseLeave(object sender, EventArgs e) => UI_Interface.SettingsButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_settings.png");

        public static void TutorialButton_Click(object sender, EventArgs e)
        {
            if (UI_Interface.Tutorial == null)
            {
                UI_Interface.Tutorial = new Tutorial();
                UI_Interface.MainForm.Controls.Add(UI_Interface.Tutorial);
                Generic_Functions.SizeAndScaleTuto();
                UI_Interface.Tutorial.Click += Generic_Functions.ClickAnyControl;

                UI_Interface.Tutorial.BringToFront();
            }
            else
            {
                Generic_Functions.DisposeTutorial();
            }

            UI_Interface.MainForm.Update();
        }

        public static void ProfileListButton_Click(object sender, EventArgs e)
        {
            if (UI_Interface.Settings.Visible)
            {
                return;
            }

            if (GameProfile.profilesPathList.Count == 0)
            {
                UI_Interface.ProfilesList.Visible = false;
                return;
            }

            UI_Interface.ProfilesList.Visible = !UI_Interface.ProfilesList.Visible;
        }

        public static void SettingsButton_Click(object sender, EventArgs e)
        {
            if (GenericGameHandler.Instance != null)
            {
                if (!GenericGameHandler.Instance.HasEnded)
                {
                    return;
                }
            }

            if (UI_Interface.ProfileSettings.Visible || UI_Interface.Settings == null)
            {
                return;
            }

            if (!UI_Interface.Settings.Visible)
            {
                UI_Interface.Settings.SetVisible();
                UI_Interface.Settings.BringToFront();
            }
            else
            {
                UI_Interface.Settings.BringToFront();
            }
        }

        public static void ExpandHandlerNotesButton_Click(object sender, EventArgs e)
        {
            if (!UI_Interface.HandlerNotesZoom.Visible)
            {               
                UI_Interface.HandlerNotesZoom.Visible = true;
                UI_Interface.HandlerNotesZoom.BringToFront();
            }
            else
            {
                UI_Interface.HandlerNotesZoom.Visible = false;
            }
        }

        public static void HandlerNotes_TextChanged(object sender, EventArgs e)
        {
            if( UI_Interface.HandlerNotesZoom == null)
            {
                return;
            }

            if (Core_Interface.Current_GameMetaInfo.FirstLaunch)
            {
                UI_Interface.HandlerNotesZoom.Warning.Visible = true;
                UI_Interface.HandlerNotesZoom.Warning.Text = "⚠ Important! Launch the game out of Nucleus before launching the handler for the first time. ⚠";
                UI_Interface.HandlerNotesZoom.Notes.Text = UI_Interface.HandlerNotes.Text;
                UI_Interface.HandlerNotesZoom.Notes.Rtf = UI_Interface.HandlerNotes.Rtf;
                UI_Interface.HandlerNotesZoom.Notes.Font = UI_Interface.HandlerNotesZoom.DefaultNotesFont;//Upate the font after setting rtf
            }
            else
            {
                UI_Interface.HandlerNotesZoom.Notes.Text = UI_Interface.HandlerNotes.Text;
                UI_Interface.HandlerNotesZoom.Notes.Rtf = UI_Interface.HandlerNotes.Rtf;
                UI_Interface.HandlerNotesZoom.Notes.Font = UI_Interface.HandlerNotesZoom.DefaultNotesFont;//Upate the font after setting rtf
                UI_Interface.HandlerNotesZoom.Warning.Visible = false;
            }
        }

        public static void Button_UpdateAvailable_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            GameControl gameControl = pictureBox.Parent as GameControl;

            GenericGameInfo gameInfo = gameControl.GameInfo;

            Handler handler = HubCache.SearchById(gameInfo.HandlerId);

            if (handler == null)
            {
                pictureBox.Visible = false;
                MessageBox.Show("Error fetching update information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Are sure you want to update this handler?", "", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    DownloadPrompt downloadPrompt = new DownloadPrompt(handler, null, true);
                    downloadPrompt.ShowDialog();

                    gameControl.UserGameInfo = Core_Interface.GameManager.User.Games.Where(g => g.GameGuid == gameControl.GameInfo.GUID).FirstOrDefault();
                    gameControl.GameInfo = gameControl.UserGameInfo.Game;

                    if (UI_Interface.CurrentGameListControl?.GameInfo.GUID == gameControl.GameInfo.GUID)
                    {
                        gameControl.RadioSelected();
                        UI_Functions.GameList_SelectedChanged(gameControl, null);
                        UI_Interface.GameList.ScrollControlIntoView(gameControl);
                    }
                }
            }
        }

        public static void GameOptionsButton_Click(object sender, EventArgs e)
        {
            PictureBox btnSender = (PictureBox)sender;
            Point ptLowerLeft = new Point(0, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            UI_Interface.GameOptionMenu.Show(ptLowerLeft);
        }

        public static void SocialMenuButton_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(0, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            UI_Interface.SocialMenu.Show(ptLowerLeft);
        }

        public static void HandlerNotes_LinkClicked(object sender, LinkClickedEventArgs e) => Process.Start(e.LinkText);


        public static void ProfileListButton_VisibleChanged(object sender, EventArgs e)
        {
            SetGameProfileButtonLoc();
        }

        public static void SetGameProfileButtonLoc()
        {
            UI_Interface.ProfileSettingsButton.Location = UI_Interface.ProfileListButton.Visible ? (Point)UI_Interface.ProfileSettingsButton.Tag : UI_Interface.ProfileListButton.Location;           
            UI_Interface.SaveProfileSwitch.Location = new Point(UI_Interface.ProfileSettingsButton.Right + 5, UI_Interface.SaveProfileSwitch.Location.Y);
        }

        public static void SetCoverLocation(bool profileEnabled)
        {
            UI_Interface.Cover.Visible = false;

            if (profileEnabled)
            {
                UI_Interface.Cover.Location = UI_Interface.DefCoverLoc;
                UI_Interface.Cover.Visible = true;
                return;
            }

            UI_Interface.Cover.BringToFront();
            UI_Interface.Cover.Location = new Point(UI_Interface.Cover.Location.X, UI_Interface.ProfileButtonsPanel.Bottom - 5);
            UI_Interface.Cover.Visible = true;
        }

        public static void ProfileButtonsPanel_StateChanged(object sender,EventArgs e)
        {
            SetProfileButtonPanelLockPbState();
        }

        public static void SetProfileButtonPanelLockPbState()
        {
            UI_Interface.ProfileButtonPanelLockPb.Text = UI_Interface.ProfileButtonsPanel.Enabled ? "" : "🔒";
            UI_Interface.ProfileButtonPanelLockPb.Location = new Point(UI_Interface.ProfileButtonsPanel.Right - UI_Interface.ProfileButtonPanelLockPb.Width, UI_Interface.SaveProfileSwitch.Top + ((UI_Interface.SaveProfileSwitch.Height / 2) - (UI_Interface.ProfileButtonPanelLockPb.Height / 2)) - 2);
        }

        public static void RefreshUI(bool refreshAll)
        {
            if (refreshAll)
            {
                UI_Interface.InputsTextLabel.Text = "";
                Core_Interface.Current_GenericGameInfo = null;
                UI_Interface.ProfileButtonsPanel.Visible = false;
                UI_Interface.GotoNext.Enabled = false;
                UI_Interface.GotoPrev.Enabled = false;
                UI_Interface.InfoPanel.Visible = false;
                UI_Interface.SetupPanel.Visible = false;
                UI_Interface.BigLogo.Visible = UI_Interface.WebView == null;
                UI_Graphics.RainbowTimer?.Dispose();
                UI_Graphics.RainbowTimerRunning = false;
                UI_Graphics.BackgroundImg = UI_Graphics.DefaultBackground;
                UI_Interface.HomeScreen.Invalidate();
            }

            DevicesFunctions.DisposeGamePadTimer();
            SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);

            UI_Interface.WindowPanel.Focus();
            UI_Interface.BigLogo.Refresh();

            if(UI_Interface.SearchTextBox != null)
            {
                UI_Interface.SearchTextBox.Visible = Core_Interface.GameManager?.User.Games.Count >= 2;
            }         
        }

        public static void ResetPlayButton()
        {
            UI_Interface.PlayButton?.Invoke((MethodInvoker)delegate ()
            {
               UI_Interface.PlayButton.Tag = "START";
               UI_Interface.PlayButton.Visible = false;
            });
        }

        public static void ShowProfilePreview(string previewText)
        {
            UI_Interface.HandlerNotesZoom?.Invoke((MethodInvoker)delegate ()
            {
                UI_Interface.HandlerNotesZoom.Notes.Text = previewText;
                UI_Interface.HandlerNotesZoom.Visible = true;
                UI_Interface.HandlerNotesZoom.BringToFront();
            });
        }

        public static void OnClickHideProfileList(object sender,MouseEventArgs e)
        {
            if(UI_Interface.ProfilesList != null)
            {
                UI_Interface.ProfilesList.Visible = false;
            }
        }
    }
}
