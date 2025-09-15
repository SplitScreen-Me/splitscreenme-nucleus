using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System.Collections.Generic;
using Nucleus.Gaming.Generic.Step;
using System.Windows.Forms;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.App.Settings;
using System.IO;
using System;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Platform.PCSpecs;
using System.Threading;
using Nucleus.Coop.Tools;
using System.Linq;
using System.Drawing;
using Nucleus.Coop.Forms;

namespace Nucleus.Coop.UI
{
    public static class Core_Interface
    {
        public static SynchronizationContext SyncContext;

        public static GameManager GameManager { get; set; }

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

                if (value != null)
                {
                    Current_GenericGameInfo = current_UserGameInfo.Game;
                    Current_GameMetaInfo = Current_GenericGameInfo.MetaInfo;

                    GameProfile.GameInfo = current_UserGameInfo;
                    GameProfile.Game = Current_GenericGameInfo;

                    UI_Actions.On_GameChange?.Invoke();
                }
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
                    UI_Functions.RefreshUI(true);
                }
            }
        }

        private static string prevSearchText = "";

        private static bool ProcessSearch(object obj)
        {
            if (obj is FlatTextBox search0)
            {
                if(search0.Text == "start htool")
                {
                    TriggerSearchTool();
                    return false;
                }

                if (search0.Text == search0.Hint) { return false; }
                if (search0.Text.StartsWith(" ")) { search0.Text = ""; return false; }
                if (prevSearchText == search0.Hint && search0.Text == "") { return false; }
                if (prevSearchText == "" && search0.Text == "") { return false; }
                if (search0.Text == search0.Hint && prevSearchText == "") { return false; }
                if (prevSearchText != "" && search0.Text == search0.Hint) { SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions); return false; }
                prevSearchText = search0.Text;
                return true;
            }

            //not the search control
            return true;
        }

        public static void RefreshGames(object sender,object e)
        {       
            if(!ProcessSearch(sender))//avoid to update the list when it's not necessary
            {
                return;
            }

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

                    UI_Interface.GameList.Controls.Add(con);
                }
                else
                {

                    for (int i = 0; i < games.Count; i++)
                    {
                        UserGameInfo game = games[i];

                        if (sender is FlatTextBox search)
                        {
                            List<string> splittedName = game.Game.GameName.ToLower().Split(' ').ToList();
                            List<string> splittedSearch = search.Text.ToLower().Split(' ').ToList();

                            bool found = (game.Game.GameName.ToLower().Contains(search.Text.ToLower()) && search.Text.Length > 1) || 
                                          game.Game.GameName.ToLower().StartsWith(search.Text.ToLower());

                            if (!found)
                            {
                                found = splittedName.Any(s => s.StartsWith(search.Text.ToLower())) && search.Text.Length > 1;

                                if (!found)
                                {
                                    found = splittedName.Any(s => s.StartsWith(search.Text.ToLower()));
                                }
                            }

                            if (!found)
                            {
                                continue;
                            }
                            else if (search.Text == "" || search.Text == search.Hint)
                            {
                                SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
                                break;
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

                if (UI_Interface.HubButton == null)
                {          
                    //Add search field controls first because the hub button location is relative to their location
                    Runtime_Controls.Insert_SearchFieldControls();
                    Runtime_Controls.Insert_HubButton();
                }

                UI_Interface.GameList.Visible = true;
            }

            if (GameManager.User.Games.Count >= 2)
            {
                if (UI_Interface.SearchTextBox != null)
                {
                    UI_Interface.SearchTextBox.Visible = true;
                }
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
            //Avoid winforms to focus buttons randomly
            UI_Interface.SetupPanel.Focus();

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

                    UI_Interface.GotoNext.Visible = CurrentStep.CanProceed && step != StepsList.Count - 1;

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

        public static void PlayClicked()
        {         
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

            if ((string)UI_Interface.PlayButton.Tag == "S T O P")
            {
                UI_Functions.RefreshUI(true);//Sort the game in case the last played sorting filter is enabled
                I_GameHandlerEndFunc("Stop button clicked", true);
                GameProfile.Instance.Reset();
               
                return;
            }

            CurrentStep?.Ended();

            UI_Interface.GotoPrev.Enabled = false;

            GameProfile.InitializeHandlerStartup();

            GameProfile.I_GameHandler.Ended += Handler_Ended;
            
            if (UI_Interface.ProfileSettings.Visible)
            {
                UI_Interface.ProfileSettings.Visible = false;
            }

            UI_Interface.MainForm.WindowState = FormWindowState.Minimized;

            UI_Interface.CurrentGameListControl = null;
            UI_Functions.RefreshUI(true);
            UI_Functions.DisableGameSelection();

            UI_Interface.PlayButton.Tag = "S T O P";
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

            if (UI_Interface.MainForm.Opacity == 1.0)//If resizing the mainForm window skip that
            {
                UI_Interface.ProfileButtonsPanel.Enabled = UI_Interface.SetupPanel.Visible && CurrentStepIndex == 0;                            
            }

            if (!canProceed)
            {
                UI_Interface.GotoPrev.Enabled = false;

                if ((string)UI_Interface.PlayButton.Tag == "START" || UI_Interface.GotoNext.Enabled)
                {
                    UI_Interface.PlayButton.Visible = false;
                }

                UI_Interface.GotoNext.Enabled = false;

                return;
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
                UI_Interface.GotoNext.Enabled = false;
            }
            else
            {
                UI_Interface.GotoNext.Enabled = true;             
            }
        }

        public static void I_GameHandlerEndFunc(string msg, bool stopButton)
        {
            try
            {
               if(GameProfile.I_GameHandler != null)
               {
                    Log($"{msg}, calling Handler End function");
                    GameProfile.I_GameHandler.End(stopButton);
               }
            }
            catch { }
        }

        public static void Handler_Ended()
        {
            Log("Handler ended method called");

            UI_Interface.MainForm.Invoke((MethodInvoker)delegate ()
            {     
                if (!UI_Interface.IsFormClosing)
                {
                    GameProfile.I_GameHandler = null;
                    UI_Interface.CurrentGameListControl = null;

                    UI_Interface.MainForm.WindowState = FormWindowState.Normal;

                    UI_Interface.WindowPanel.Focus();
                    UI_Interface.PlayButton.Tag = "START";
                    UI_Interface.PlayButton.Visible = false;

                    UI_Interface.MainForm.BringToFront();
                
                    SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);//refresh game list controls again so we have fresh datas for all
                    UI_Functions.EnableGameSelection();
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
                Settings.Ctrlr_Shorcuts.Text = "Windows 8™ and up only";
                Settings.Ctrlr_Shorcuts.Enabled = false;
                if(UI_Interface.ToggleVirtualMouse != null)
                {
                    UI_Interface.ToggleVirtualMouse.Visible = false;
                }

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

        public static void RefreshHandlers()
        {
            GameManager = new GameManager();
       
            SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);

            UI_Functions.RefreshUI(true);
        }

        public static void TriggerSearchTool()
        {
            HandlerScanner handlerScanner = new HandlerScanner();
            handlerScanner.Initialize();
        }
    }
}
