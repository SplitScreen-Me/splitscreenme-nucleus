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
using System.Text.RegularExpressions;
using Nucleus.Gaming.UI;
using System.Threading.Tasks;
using static Nucleus.Gaming.Http_Client.Http_Client;
using Nucleus.Gaming.Coop.Generic;
using System.Drawing.Drawing2D;


namespace Nucleus.Coop.UI
{
    public static class UI_Functions
    {
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        public static void Logo_Click(object sender, EventArgs e) => Process.Start(Links.NC_Repo);

        public static void DonationButton_Click(object sender, EventArgs e)
        {
            Button patreonMenuButton = sender as Button;

            if (!UI_Interface.PatreonMenu.Visible)
            {
                Point ptLowerLeft = new Point(0, patreonMenuButton.Height);
                ptLowerLeft = patreonMenuButton.PointToScreen(ptLowerLeft);
                UI_Interface.PatreonMenu.Show(ptLowerLeft);
                UI_Interface.PatreonMenu.BringToFront();
            }
        }


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

        public static void DisableGameSelection()
        {
            UI_Interface.GameList.SelectedChanged -= GameList_SelectedChanged;
        }

        public static void EnableGameSelection()
        {
            UI_Interface.GameList.SelectedChanged -= GameList_SelectedChanged;
            UI_Interface.GameList.SelectedChanged += GameList_SelectedChanged;
        }

        public static void GameList_SelectedChanged(object arg1, Control arg2)
        {
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

            UI_Interface.ProfilesList.Visible = UI_Interface.ProfilesList.Visible ? false : true;
        }

        public static void ExpandHandlerNotesButton_Click(object sender, EventArgs e)
        {
            if (!UI_Interface.HandlerNotesZoom.Visible)
            {
                UI_Interface.HandlerNotesZoom.Warning.Visible = Core_Interface.Current_GenericGameInfo.MetaInfo.FirstLaunch;

                UI_Interface.HandlerNotesZoom.TextBox.Rtf = UI_Interface.HandlerNotes.Rtf;
                UI_Interface.HandlerNotesZoom.TextBox.Font = UI_Interface.HandlerNotesZoom.DefaultNotesFont;
                UI_Interface.HandlerNotesZoom.Visible = true;
                UI_Interface.HandlerNotesZoom.BringToFront();
            }
            else
            {
                UI_Interface.HandlerNotesZoom.Visible = false;
            }
        }

        public static void ManageGamesButton_Click(object sender, EventArgs e)
        {
            Button mainMenuButton = sender as Button;

            if (!UI_Interface.ManageGamesMenu.Visible)
            {
                Point ptLowerLeft = new Point(0, mainMenuButton.Height);
                ptLowerLeft = mainMenuButton.PointToScreen(ptLowerLeft);
                UI_Interface.ManageGamesMenu.Show(ptLowerLeft);
                UI_Interface.ManageGamesMenu.BringToFront();
            }
        }

        public static void HandlerNotes_TextChanged(object sender, EventArgs e)
        {
            if (UI_Interface.HandlerNotesZoom == null)
            {
                return;
            }

            if (Core_Interface.Current_GameMetaInfo.FirstLaunch)
            {
                UI_Interface.HandlerNotesZoom.Warning.Visible = true;
                UI_Interface.HandlerNotesZoom.Warning.Text = "⚠ Important! Launch the game out of Nucleus before launching the handler for the first time. ⚠";
            }
            else
            {
                UI_Interface.HandlerNotesZoom.Warning.Visible = false;
            }

            UI_Interface.HandlerNotesZoom.TextBox.Rtf = UI_Interface.HandlerNotes.Rtf;
            UI_Interface.HandlerNotesZoom.TextBox.Font = UI_Interface.HandlerNotesZoom.DefaultNotesFont;
        }

        public static async void Button_UpdateAvailable_Click(object sender, EventArgs e)
        {
            PictureBox pictureBox = sender as PictureBox;
            GameControl gameControl = pictureBox.Parent as GameControl;

            UserGameInfo userGameInfo = gameControl.UserGameInfo;

            Handler handler = await HubCache.SearchByIdOnline(userGameInfo.HandlerInfo.GenericGameInfo.HandlerId);

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

                    gameControl.UserGameInfo = Core_Interface.GameManager.User.Games.Find(g => g.GameGuid == gameControl.UserGameInfo.GameGuid);

                    if (UI_Interface.CurrentGameListControl?.UserGameInfo.HandlerInfo.GenericGameInfo.GUID == gameControl.UserGameInfo.GameGuid)
                    {
                        gameControl.RadioSelected();
                        GameList_SelectedChanged(gameControl, null);
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

        public static void ProfileButtonsPanel_StateChanged(object sender, EventArgs e)
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
                UI_Interface.InputsTextLabel.SetText(new (string, Color, Font)[] { });
                Core_Interface.Current_GenericGameInfo = null;
                UI_Interface.ProfileButtonsPanel.Visible = false;
                UI_Interface.GotoNext.Enabled = false;
                UI_Interface.GotoPrev.Enabled = false;
                UI_Interface.InfoPanel.Visible = false;
                UI_Interface.SetupPanel.Visible = false;
                UI_Interface.BigLogo.Visible = UI_Interface.WebView == null;
                UI_Graphics.NotesTitleScrollTimer?.Dispose();
                UI_Graphics.IsNotesTitleScrollTimerRunning = false;
                UI_Graphics.BackgroundImg = UI_Graphics.DefaultBackground;
                UI_Interface.HomeScreen.Invalidate();
            }

            if (UI_Interface.ProfilesList.Visible)
            {
                UI_Interface.ProfilesList.Visible = false;
            }

            DevicesFunctions.DisposeGamePadTimer();

            Core_Interface.RefreshGamesList();
            UI_Interface.WindowPanel.Focus();
            UI_Interface.BigLogo.Refresh();

            UI_Interface.MainForm.Invalidate(false);
        }

        public static void EnablePlay(bool enabled)
        {
            UI_Interface.PlayButton.Visible = enabled;
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
            UIThread.Post(() =>
            {
                UI_Interface.HandlerNotesZoom.Warning.Visible = false;
                UI_Interface.HandlerNotesZoom.Notes.Text = previewText;
                UI_Interface.HandlerNotesZoom.Visible = true;
                UI_Interface.HandlerNotesZoom.BringToFront();
            });
        }

        public static void OnClickHideProfileList(object sender, MouseEventArgs e)
        {
            if (UI_Interface.ProfilesList != null)
            {
                UI_Interface.ProfilesList.Visible = false;
            }
        }

        public static void ExtractHandlerDragDrop(object sender, DragEventArgs e)
        {
            DisposeDragAndDropModal(modalControls[0], null);

            try
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                foreach (string file in files)
                {
                    if (File.Exists(file))
                    {
                        var info = new FileInfo(file);

                        if (info.Extension == ".js" && info.Name != "handler.js")
                        {
                            var handlersDir = GameManager.Instance.GetJsScriptsPath();

                            if (info.Name == "handler.js")
                            {
                                MessageBox.Show("\"handler.js\" is not supported file name.", "File name not supported", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }

                            if (file == Path.Combine(handlersDir, info.Name))
                            {
                                MessageBox.Show("Use the search executable function to add this handler.", "The file already exists", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                continue;
                            }

                            if (File.Exists(Path.Combine(handlersDir, info.Name)))
                            {
                                DialogResult ovdialogResult = MessageBox.Show("A handler with the name " + (info.Name) + " already exists. Do you wish to overwrite it?", "Handler already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                                if (ovdialogResult != DialogResult.Yes)
                                {
                                    return;
                                }
                            }

                            if (File.Exists(Path.Combine(handlersDir, info.Name)))
                            {
                                //File.Delete(Path.Combine(handlersDir, info.Name));
                                FileUtil.SendToRecycleBin(Path.Combine(handlersDir, info.Name));
                            }

                            File.Copy(file, Path.Combine(handlersDir, info.Name));

                            Regex pattern = new Regex("[\\/:*?\"<>|]");
                            string exeName = null;
                            int found = 0;

                            foreach (string line in File.ReadAllLines(Path.Combine(handlersDir, info.Name)))
                            {
                                if (line.ToLower().StartsWith("game.executablename"))
                                {
                                    int start = line.IndexOf("\"");
                                    int end = line.LastIndexOf("\"");
                                    exeName = line.Substring(start + 1, (end - start) - 1);
                                    found++;
                                }
                                else if (line.ToLower().StartsWith("game.gamename"))
                                {
                                    int start = line.IndexOf("\"");
                                    int end = line.LastIndexOf("\"");
                                    found++;
                                }

                                if (found == 2)
                                {
                                    break;
                                }
                            }

                            if (found == 0)
                            {
                                MessageBox.Show("The file is missing important variables or is not a Nucleus handler.", "Not suppoerted file.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                                return;
                            }

                            var handlerName = info.Name.Replace(".js", "");

                            bool update = GameManager.Instance.AddNewGenericGameInfo(handlerName, false);

                            if (update)
                            {
                                return;
                            }

                            HandlerInfo handlerInfo = Core_Interface.GameManager.HandlerInfos.Where(aggi => aggi.FileName == info.Name).FirstOrDefault();
                            _ = SearchGame.Search(exeName, handlerInfo);

                            return;
                        }

                        if (info.Extension != ".zip" && info.Extension != ".nc")
                        {
                            MessageBox.Show("The file is in an unsupported format or is not a valid Nucleus handler archive(.zip, .nc) or file (.js).", "Not a valid handler archive or file", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                            return;
                        }

                        var prompt = new DownloadPrompt(null, file, false);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "An error occured.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
        }

        private static Control[] modalControls = new Control[2];
        private static string modalFileText;

        public static void ShowDragAndDropModal(object sender, DragEventArgs e)
        {
            if (modalControls[0] == null || modalControls[1] == null)
            {
                DragAndDropModal modal = new DragAndDropModal();
                modal.Dock = DockStyle.Fill;
                modal.BackColor = Color.FromArgb(100, 0, 0, 0);
                modal.BackgroundImageLayout = ImageLayout.Center;

                Label modalText = modal.Controls[0] as Label;
                Control parent = sender as Control;

                string modalTextPrefix = string.Empty;
                string textNoLineBreaks = string.Empty;

                if (modalFileText.StartsWith("\n"))
                {
                    textNoLineBreaks = modalFileText.Substring(1).Trim();
                }
                else
                {
                    textNoLineBreaks = modalFileText;
                }

                if (modalFileText.EndsWith(".zip") || modalFileText.EndsWith(".nc") || modalFileText.EndsWith(".zip\n") || modalFileText.EndsWith(".nc\n"))
                {
                    modalTextPrefix = "Extract && Install";
                }
                else if (modalFileText.EndsWith(".js") || modalFileText.EndsWith(".js\n"))
                {
                    if (!modalFileText.EndsWith(".js\n"))
                    {
                        if (Core_Interface.GameManager.User.Games.Any(hi => hi.HandlerInfo.FileName == textNoLineBreaks))
                        {
                            modalTextPrefix = "Update";
                        }
                        else
                        {
                            modalTextPrefix = "Install";
                        }
                    }
                    else
                    {
                        modalTextPrefix = "Install/Update";
                    }
                }
                else
                {
                    modalTextPrefix = "Not a valid file";
                }

                modalText.Text = $"{modalTextPrefix} {modalFileText}";

                modalControls[0] = parent;
                modalControls[1] = modal;

                parent.Controls.Add(modal);
                modal.BringToFront();

                modalText.TextAlign = ContentAlignment.MiddleCenter;
                modalText.Location = new Point(parent.Width / 2 - modalText.Width / 2, (parent.Height / 2 - modalText.Height / 2) /*+ 105*/);
                modalText.BackColor = Color.Transparent;
                modalText.ForeColor = Color.AntiqueWhite;
            }
        }

        public static void DisposeDragAndDropModal(object sender, EventArgs e)
        {
            if (modalControls[0] != null && modalControls[1] != null)
            {
                modalControls[0].Controls.Remove(modalControls[1]);
                modalControls[1].Dispose();
            }

            modalControls[0].Invalidate();
            UI_Interface.BigLogo.Refresh();
            modalControls[0] = null;
            modalControls[1] = null;
        }

        public static void ExtractHandlerDragEnter(object sender, DragEventArgs e)
        {
            modalFileText = string.Empty;

            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                for (int i = 0; i < files.Length; i++)
                {
                    string fileName = files[i];

                    if (files.Length == 1)
                    {
                        modalFileText += "\n" + fileName.Split('\\').Last();
                    }
                    else
                    {
                        if (i == 0)
                        {
                            modalFileText += "\n" + fileName.Split('\\').Last() + "\n";
                        }
                        else
                        {
                            modalFileText += fileName.Split('\\').Last() + "\n";
                        }
                    }
                }
            }
        }

        public static void NewsFeed_Resize(object sender, EventArgs e)
        {
            NewsFeed newsFeed = sender as NewsFeed;

            newsFeed.Location = new Point(newsFeed.Location.X, UI_Interface.ManageGamesMenuButton.Location.Y + (UI_Interface.ManageGamesMenuButton.Height / 2 - newsFeed.Height / 2));
        }

        public static void HandlerNoteTitle_Resize(object sender, EventArgs e)
        {
            Label title = sender as Label;
            title.Location = new Point(title.Location.X, UI_Interface.ExpandHandlerNotesButton.Location.Y + (UI_Interface.ExpandHandlerNotesButton.Height / 2 - title.Height / 2));
        }

        private static System.Threading.Timer endMessageTimer;
        private static int endMessageStepCount = 0;
        private static string[] endMessageStrings = new string[] {"🕛","🕐","🕑","🕒","🕓",
            "🕔","🕕","🕖","🕗","🕘",
            "🕙","🕚" };

        public static void ShowEndingMessage()
        {
            if(endMessageTimer == null)
            {
                endMessageTimer?.Dispose();
                endMessageTimer = new System.Threading.Timer(EndMessageTimer_Tick, null, 0, 800);
            }
        }

        public static void HideEndingMessage()
        {
            endMessageStepCount = 0;
            endMessageTimer?.Dispose();
            endMessageTimer = null;
        }

        private static void EndMessageTimer_Tick(object obj)
        {           
            Globals.MainOSD.Show(1000, $"{endMessageStrings[endMessageStepCount]} Processing session end operations, please wait {endMessageStrings[endMessageStepCount]}");
            endMessageStepCount++;

            if (endMessageStepCount == 11)
            {
                endMessageStepCount = 0;
            }
        }

        public static void TriggerSearchTool()
        {
            HandlerScanner handlerScanner = new HandlerScanner();
            _ = handlerScanner.Initialize();
        }

    }
}
