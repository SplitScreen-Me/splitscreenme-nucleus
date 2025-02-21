using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System.Collections.Generic;
using Nucleus.Gaming.Generic.Step;
using System.Windows.Forms;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.App.Settings;
using System.IO;
using Nucleus.Gaming.Cache;
using System.Drawing;
using System;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Platform.PCSpecs;
using System.Threading;
using Nucleus.Coop.Controls;
using Nucleus.Coop.Forms;
using Nucleus.Coop.Tools;

namespace Nucleus.Coop.UI
{
    public static class Core_Interface
    {
        public static GameManager GameManager { get; set; }
        public static IGameHandler I_GameHandler;

        public static UserInputControl CurrentStep;
        public static int CurrentStepIndex { get; set; }

        public static Dictionary<UserGameInfo, GameControl> GameControlsInfo;
        private static UserGameInfo current_UserGameInfo;

        public static JSUserInputControl JsControl;

        public static UserGameInfo CurrentMenuUserGameInfo;

        public static List<UserInputControl> StepsList;

        public static ContentManager HandlerContent;

        public static UserGameInfo Current_UserGameInfo
        {
            get => current_UserGameInfo;
            set
            {
                current_UserGameInfo = value;
                Current_GenericGameInfo = current_UserGameInfo.Game;
                Current_GameMetaInfo = Current_GenericGameInfo.MetaInfo;
                UI_Actions.On_GameChange?.Invoke();
            }
        }

        public static GenericGameInfo Current_GenericGameInfo { get; set; }
        public static GameMetaInfo Current_GameMetaInfo { get; set; }

        private static bool disableGameProfiles = App_Misc.DisableGameProfiles;
        public static bool DisableGameProfiles
        {
            get => disableGameProfiles;
            set
            {
                if (disableGameProfiles != value)
                {
                    disableGameProfiles = value;
                    UI_Functions.RefreshUI(true);///A changer surement
                }
            }
        }

        public static void RefreshGames(object sender,object e)
        {
            UI_Interface.GameList.Visible = false;///smoother transition

            lock (GameControlsInfo)
            {
                foreach (KeyValuePair<UserGameInfo, GameControl> con in GameControlsInfo)
                {
                    foreach (Control ch in con.Value.Controls)
                    {
                        ch.Font.Dispose();
                        ch.Dispose();
                    }

                    con.Value?.Dispose();
                }

                UI_Interface.GameList.Controls.Clear();
                GameControlsInfo.Clear();

                List<UserGameInfo> games = GameManager.User.Games;

                if (e is List<UserGameInfo> gameList)
                {
                    games = gameList;
                }

                if (games.Count == 0)
                {
                    UI_Interface.NoGamesPresent = true;
                    GameControl con = new GameControl(null, null, false)
                    {
                        Width = UI_Interface.GameListContainer.Width,
                        Text = "No games",
                        Font = UI_Interface.MainForm.Font,
                    };

                    if (UI_Interface.SearchTextBox != null)
                    {
                        UI_Interface.SearchTextBox.Visible = false;
                    }

                    UI_Interface.GameList.Controls.Add(con);
                }
                else
                {
                    for (int i = 0; i < games.Count; i++)
                    {
                        UserGameInfo game = games[i];

                        if (sender is FlatTextBox search)
                        {
                            if (search.Text == "")
                            {
                                SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
                                break;
                            }
                            else if (!game.Game.GameName.ToLower().StartsWith(search.Text.ToLower()))
                            {
                                continue;
                            }
                        }

                        if (game.Game == null && games.Count == 1)
                        {
                            UI_Interface.NoGamesPresent = true;
                            GameControl con = new GameControl(null, null, false)
                            {
                                Width = UI_Interface.GameListContainer.Width,
                                Text = "No games",
                                Font = UI_Interface.MainForm.Font,
                            };

                            UI_Interface.GameList.Controls.Add(con);

                            break;
                        }

                        NewUserGame(game);
                    }
                }

                if (UI_Interface.AddGameButton == null)
                {
                    float scale = (float)User32Util.GetDpiForWindow(UI_Interface.MainForm.Handle) / (float)100;
                    int size = (int)((float)15 * scale);
                    int offset = (int)((float)3 * scale);

                    UI_Interface.SearchTextBox = new SearchTextBox();
                    UI_Interface.SearchTextBox.Location = new Point(0, 3);
                    UI_Interface.SearchTextBox.Size = new Size((UI_Interface.GameListContainer.Width - (size * 3)) + offset, UI_Interface.GameList.Controls[0].Height /2);
                    UI_Interface.GameListContainer.Controls.Add(UI_Interface.SearchTextBox);

                    UI_Interface.SortGamesButton = new SortGamesButton(new Size(size , size),new Point(UI_Interface.SearchTextBox.Right, (UI_Interface.SearchTextBox.Top + UI_Interface.SearchTextBox.Height / 2) - size / 2));
                    UI_Interface.GameListContainer.Controls.Add(UI_Interface.SortGamesButton);

                    UI_Interface.ToggleFavoriteButton = new ToggleFavoriteButton(new Size(size, size), new Point(UI_Interface.GameListContainer.Right - (size + 5), UI_Interface.SortGamesButton.Top));
                    UI_Interface.GameListContainer.Controls.Add(UI_Interface.ToggleFavoriteButton);

                    UI_Interface.AddGameButton = new AddGameButton(UI_Interface.GameListContainer.Width, UI_Interface.GameList.Controls[0].Height);        
                }

                UI_Interface.GameList.Visible = true;
            }

            if (UI_Interface.CurrentGameListControl != null)
            {
                UI_Interface.GameList.ScrollControlIntoView(UI_Interface.CurrentGameListControl);
            }

            GameManager.SaveUserProfile();
        }

        private static void NewUserGame(UserGameInfo game)
        {
            if (game.Game == null || !game.IsGamePresent())
            {
                return;
            }

            if (UI_Interface.NoGamesPresent)
            {
                UI_Interface.NoGamesPresent = false;
                SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
                return;
            }

            bool favorite = game.Game.MetaInfo.Favorite;

            GameControl con = new GameControl(game.Game, game, favorite)
            {
                Width = UI_Interface.GameListContainer.Width,
            };

            con.Click += Generic_Functions.WebviewDisposed;
            con.HandlerUpdate.Click += UI_Functions.Button_UpdateAvailable_Click;
            con.GameOptions.Click += UI_Functions.GameOptionsButton_Click;

            if (UI_Interface.ShowFavoriteOnly)
            {
                if (favorite || con.GameInfo == Current_GenericGameInfo)
                {
                    if (con.GameInfo == Current_GenericGameInfo)
                    {
                        con.RadioSelected();
                        UI_Interface.CurrentGameListControl = con;
                    }

                    GameControlsInfo.Add(game, con);
                    UI_Interface.GameList.Controls.Add(con);
                    ThreadPool.QueueUserWorkItem(GetGameIcon.GetIcon, game);
                }
            }
            else
            {
                if (con.GameInfo == Current_GenericGameInfo)
                {
                    con.RadioSelected();
                    UI_Interface.CurrentGameListControl = con;
                }

                GameControlsInfo.Add(game, con);
                UI_Interface.GameList.Controls.Add(con);

                ThreadPool.QueueUserWorkItem(GetGameIcon.GetIcon, game);
            }
        }

        public static void KillCurrentStep()
        {
            foreach (Control c in UI_Interface.SetupPanel.Controls)
            {
                UI_Interface.SetupPanel.Controls.Remove(c);
            }
        }

        private static PlayerOptionsControl optionsControl;
        public static PlayerOptionsControl OptionsControl
        {
            get => optionsControl;
            set
            {
                optionsControl = value;
                optionsControl.OnCanPlayUpdated += StepCanPlay;
            }
        }

        public static void GotoPrevStep()
        {
            CurrentStepIndex--;

            if (CurrentStepIndex < 0)
            {
                CurrentStepIndex = 0;
                return;
            }

            GameProfile.Ready = false;

            GoToStep(CurrentStepIndex);
        }

        public static void GotoNextStep()
        {
            if (UI_Interface.ProfileSettings.Visible) { UI_Interface.ProfileSettings.BringToFront(); return; }
            if (UI_Interface.Settings.Visible) { UI_Interface.Settings.BringToFront(); return; }

            GoToStep(CurrentStepIndex + 1);
        }

        public static void GoToStep(int step)
        {
            UI_Interface.GotoPrev.Enabled = step > 0;

            if (step >= StepsList.Count)
            {
                return;
            }

            if (step >= 2)
            {
                // Custom steps
                List<CustomStep> customSteps = Current_GenericGameInfo.CustomSteps;
                int customStepIndex = step - 2;
                CustomStep customStep = customSteps[0];

                customStep.UpdateRequired?.Invoke();

                if (customStep.Required)
                {
                    JsControl.CustomStep = customStep;
                    JsControl.Content = HandlerContent;
                }
                else
                {
                    Generic_Functions.EnablePlay();
                    return;
                }
            }

            KillCurrentStep();

            if (GameProfile.Ready)
            {
                if (Current_GenericGameInfo.CustomSteps.Count > 0)
                {
                    JsControl.CustomStep = Current_GenericGameInfo.CustomSteps[0];
                    JsControl.Content = HandlerContent;

                    CurrentStepIndex = StepsList.Count - 1;
                    CurrentStep = StepsList[StepsList.Count - 1];
                    CurrentStep.Size = UI_Interface.SetupPanel.Size;
                    CurrentStep.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                    CurrentStep = StepsList[StepsList.Count - 1];
                    CurrentStep.Initialize(Current_UserGameInfo, GameProfile.Instance);

                    UI_Interface.SetupPanel.Controls.Add(CurrentStep);

                    UI_Interface.GotoNext.Enabled = CurrentStep.CanProceed && step != StepsList.Count - 1;

                    if (GameProfile.AutoPlay)
                    {
                        Generic_Functions.EnablePlay();
                    }

                    return;
                }
            }

            CurrentStepIndex = step;
            CurrentStep = StepsList[step];
            CurrentStep.Size = UI_Interface.SetupPanel.Size;
            CurrentStep.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
            CurrentStep.Initialize(Current_UserGameInfo, GameProfile.Instance);

            UI_Interface.SetupPanel.Controls.Add(CurrentStep);

            UI_Interface.GotoNext.Enabled = CurrentStep.CanProceed && step != StepsList.Count - 1;
        }

        public static void PlayClicked(object sender)
        {
            Button playButton = sender as Button;

            if (UI_Interface.ProfileSettings.Visible)
            {
                UI_Interface.ProfileSettings.BringToFront();
                return;
            }

            if (UI_Interface.Settings.Visible)
            {
                UI_Interface.Settings.BringToFront();
                return;
            }

            if ((string)playButton.Tag == "S T O P")
            {
                I_GameHandlerEndFunc("Stop button clicked", true);
                GameProfile.Instance.Reset();
                DevicesFunctions.GamepadTimer = new System.Threading.Timer(DevicesFunctions.GamepadTimer_Tick, null, 0, 500);
                return;
            }

            DevicesFunctions.DisposeGamePadTimer();

            CurrentStep?.Ended();

            playButton.Tag = "S T O P";

            UI_Interface.GotoPrev.Enabled = false;

            //reload the handler here so it can be edited/updated until play button get clicked
            GameManager.Instance.AddScript(Path.GetFileNameWithoutExtension(Current_GenericGameInfo.JsFileName), new bool[] { false, Current_GenericGameInfo.UpdateAvailable });

            Current_GenericGameInfo = GameManager.Instance.GetGame(Current_UserGameInfo.ExePath);
            Current_UserGameInfo.InitializeDefault(Current_GenericGameInfo, Current_UserGameInfo.ExePath);

            I_GameHandler = GameManager.Instance.MakeHandler(Current_GenericGameInfo);
            I_GameHandler.Initialize(Current_UserGameInfo, GameProfile.CleanClone(GameProfile.Instance), I_GameHandler);
            I_GameHandler.Ended += Handler_Ended;

            if (UI_Interface.ProfileSettings.Visible)
            {
                UI_Interface.ProfileSettings.Visible = false;
            }

            HotkeysRegistration.RegHotkeys(UI_Interface.MainForm.Handle);

            if (Current_UserGameInfo.Game.CustomHotkeys != null)
            {
                HotkeysRegistration.RegCustomHotkeys(Current_GenericGameInfo);
            }

            UI_Interface.MainForm.WindowState = FormWindowState.Minimized;

            UI_Interface.CurrentGameListControl = null;
            UI_Functions.RefreshUI(true);
        }

        public static void StepCanPlay(UserControl obj, bool canProceed, bool autoProceed)
        {
            if (canProceed || autoProceed)
            {
                UI_Interface.SaveProfileSwitch.Visible = !GameProfile.Loaded && !Current_GameMetaInfo.DisableProfiles && !DisableGameProfiles;
            }
            else
            {
                UI_Interface.SaveProfileSwitch.Visible = false;
            }

            UI_Interface.SaveProfileSwitch.Location = GameProfile.profilesPathList.Count > 0 ? new Point(UI_Interface.ProfileListButton.Right + 5, UI_Interface.SaveProfileSwitch.Location.Y) : new Point(UI_Interface.ProfileSettingsButton.Right + 5, UI_Interface.SaveProfileSwitch.Location.Y);

            if (UI_Interface.GotoPrev.Enabled)
            {
                UI_Interface.GotoPrev.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_left_mousehover.png");
            }
            else
            {
                UI_Interface.GotoPrev.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_left.png");
            }

            if (UI_Interface.MainForm.Opacity == 1.0)//If resizing the mainForm window skip that
            {
                UI_Interface.ProfileButtonsPanel.Visible = UI_Interface.SetupPanel.Visible && CurrentStepIndex == 0;
            }

            if (!canProceed)
            {
                UI_Interface.GotoPrev.Enabled = false;

                if ((string)UI_Interface.PlayButton.Tag == "START" || UI_Interface.GotoNext.Enabled)
                {
                    UI_Interface.PlayButton.Visible = false;
                }

                UI_Interface.GotoNext.Enabled = false;
                UI_Interface.GotoNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_right.png");
                UI_Interface.GotoPrev.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_left.png");
                return;
            }
            else
            {
                UI_Interface.GotoNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_right.png");
            }

            if (Current_GenericGameInfo?.Options?.Count == 0)
            {
                Generic_Functions.EnablePlay();
                return;
            }

            if (CurrentStepIndex + 1 > StepsList.Count - 1)
            {
                Generic_Functions.EnablePlay();
                return;
            }
            else
            {
                if ((string)UI_Interface.PlayButton.Tag == "START")
                {
                    UI_Interface.PlayButton.Visible = false;
                }
                else
                {
                    UI_Interface.PlayButton.Visible = true;
                }
            }

            if (autoProceed)
            {
                GoToStep(CurrentStepIndex + 1);
            }
            else
            {
                UI_Interface.GotoNext.Enabled = true;
                UI_Interface.GotoNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "arrow_right_mousehover.png");
            }
        }

        public static void I_GameHandlerEndFunc(string msg, bool stopButton)
        {
            try
            {
                if (I_GameHandler != null)
                {
                    Log($"{msg}, calling Handler End function");
                    I_GameHandler.End(stopButton);
                }
            }
            catch { }
        }

        public static void Handler_Ended()
        {
            Log("Handler ended method called");

            User32Util.ShowTaskBar();

            UI_Interface.MainForm.Invoke((MethodInvoker)delegate ()
            {
                if (!UI_Interface.IsFormClosing)
                {
                    I_GameHandler = null;
                    UI_Interface.CurrentGameListControl = null;

                    UI_Interface.MainForm.WindowState = FormWindowState.Normal;

                    UI_Interface.WindowPanel.Focus();
                    UI_Interface.PlayButton.Tag = "START";
                    UI_Interface.PlayButton.Visible = false;
                    HotkeysRegistration.UnRegHotkeys();

                    UI_Interface.MainForm.BringToFront();
                }
            });
        }

        public static bool InitializeGamepadThreads()
        {   //Enable only for Windows versions with default support for xinput1.4.dll ,
            //might be fixable by placing the dll at the root of our exe but not for now.
            string windowsVersion = MachineSpecs.GetPCspecs();
            if (!windowsVersion.Contains("Windows 7") &&
                !windowsVersion.Contains("Windows Vista"))
            {
                GamepadShortcuts.GamepadShortcutsThread = new Thread(GamepadShortcuts.GamepadShortcutsUpdate);
                GamepadShortcuts.GamepadShortcutsThread.Start();
                GamepadShortcuts.UpdateShortcutsValue();

                GamepadNavigation.GamepadNavigationThread = new Thread(GamepadNavigation.GamepadNavigationUpdate);
                GamepadNavigation.GamepadNavigationThread.Start();
                GamepadNavigation.UpdateUINavSettings();
                return true;
            }
            else
            {
                Settings._ctrlr_shorcuts.Text = "Windows 8™ and up only";
                Settings._ctrlr_shorcuts.Enabled = false;
                return false;
            }
        }

        public static void Log(string logMessage)
        {
            if (App_Misc.DebugLog)
            {
                using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]MAIN: {logMessage}");
                    writer.Close();
                }
            }
        }
    }
}
