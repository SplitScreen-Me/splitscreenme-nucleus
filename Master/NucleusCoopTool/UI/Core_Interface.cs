using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System.Collections.Generic;
using Nucleus.Gaming.Generic.Step;
using System.Windows.Forms;
using Nucleus.Gaming.App.Settings;
using System.IO;
using System;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Platform.PCSpecs;
using System.Threading;
using Nucleus.Coop.Tools;
using System.Linq;


namespace Nucleus.Coop.UI
{
    public static class Core_Interface
    {
        public static GameManager GameManager { get; set; }

        public static UserInputControl CurrentStep;
        public static int CurrentStepIndex { get; set; }

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
                    Current_GenericGameInfo = current_UserGameInfo.HandlerInfo.GenericGameInfo;
                    Current_GameMetaInfo = Current_GenericGameInfo.MetaInfo;

                    GameProfile.Current_UserGameInfo = current_UserGameInfo;
                    GameProfile.Current_GenericGamInfo = Current_GenericGameInfo;

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
                    if(Current_UserGameInfo != null)
                    {
                        ReselectGameControl(Current_UserGameInfo.GameGuid);
                    }
                }
            }
        }

        private static string prevSearchText = "";

        private static bool ProcessSearch(FlatTextBox searchTextBox)
        {
            if (searchTextBox.Text == "start htool")
            {
                UI_Functions.TriggerSearchTool();
                return false;
            }

            if (searchTextBox.Text == searchTextBox.Hint) { return false; }
            if (searchTextBox.Text.StartsWith(" ")) { searchTextBox.Text = ""; return false; }
            if (prevSearchText == searchTextBox.Hint && searchTextBox.Text == "") { return false; }
            if (prevSearchText == "" && searchTextBox.Text == "") { return false; }
            if (searchTextBox.Text == searchTextBox.Hint && prevSearchText == "") { return false; }
            if (prevSearchText != "" && searchTextBox.Text == searchTextBox.Hint) { RefreshGamesList(); return false; }
            prevSearchText = searchTextBox.Text;
            return true;
        }

        private static void ClearGameList()
        {
            Control gameList = UI_Interface.GameList;

            while (gameList.Controls.Count > 0)
            {              
                gameList.Controls[0].Dispose();
            }
        }

        public static void RefreshGamesList()
        {
            List<UserGameInfo> userGameInfos = SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);

            UI_Interface.GameList.Visible = false;///smoother transition

            ClearGameList();

            string currentSelected = null;

            for (int i = 0; i < userGameInfos.Count; i++)
            {
                UserGameInfo userGameInfo = userGameInfos[i];

                if (userGameInfo.HandlerInfo?.GenericGameInfo == null)
                {
                    //if the js file was invalid but the game is already
                    //added to the app we give it a chance to be added automatically by the GameManager if the file get fixed.
                    userGameInfo.OnHandlerInfoRefresh += AddGameToList;
                    continue;
                }

                if(Current_UserGameInfo?.GameGuid == userGameInfo.GameGuid)
                {
                    currentSelected = userGameInfo.GameGuid;
                }

                AddGameControl(userGameInfo);
            }

            if (userGameInfos.Count == 0)
            {
                //a dummy control to get the default controls size
                //hidden from its contructor
                GameControl dummy = new GameControl(null)
                {
                    Width = UI_Interface.GameListContainer.Width,
                    Text = "No games",
                    Font = UI_Interface.MainForm.Font,
                };

                UI_Interface.GameList.Controls.Add(dummy);
            }

            if (UI_Interface.HubButton == null)
            {
                //Add search field controls first because the hub button location is relative to its location
                Runtime_Controls.Insert_SearchFieldControls();
                Runtime_Controls.Insert_HubButton();
            }

            if (currentSelected != null)
            {
                ReturnToGameControl(currentSelected);
            }

            UI_Interface.GameList.Visible = true;///smoother transition
        }

        public static void SearchGame(object sender, EventArgs e)
        {
            FlatTextBox searchTextBox = (FlatTextBox)sender;

            string userInput = searchTextBox.Text;

            if (!ProcessSearch(searchTextBox))
            {
                return;
            }

            List<UserGameInfo> userGameInfos = SortGameFunction.SortGamesTextInput(userInput);

            var gameList = UI_Interface.GameList;

            gameList.Visible = false;///smoother transition

            ClearGameList();

            string currentSelected = null;

            for (int i = 0; i < userGameInfos.Count; i++)
            {
                UserGameInfo userGameInfo = userGameInfos[i];

                if (userGameInfo.HandlerInfo?.GenericGameInfo == null)
                {
                    //if the js file is invalid but the game is already
                    //added to the app; we give it a chance to be added automatically by the GameManager if the file get fixed.
                    userGameInfo.OnHandlerInfoRefresh += AddGameToList;
                    continue;
                }

                if (Current_UserGameInfo?.GameGuid == userGameInfo.GameGuid)
                {
                    currentSelected = userGameInfo.GameGuid;
                }

                AddGameControl(userGameInfo);
            }

            if (userGameInfos.Count == 0)
            {
                //a dummy control to get the default controls size
                //hidden from its contructor
                GameControl dummy = new GameControl(null)
                {
                    Width = UI_Interface.GameListContainer.Width,
                    Text = "No games",
                    Font = UI_Interface.MainForm.Font,
                };

                gameList.Controls.Add(dummy);
            }

            if(currentSelected != null)
            {
                ReturnToGameControl(currentSelected);
            }

            UI_Interface.GameList.Visible = true;///smoother transition
        }

        public static GameControl GetGameControl(string gameGuid)
        {
            if(string.IsNullOrEmpty(gameGuid))
            {
                return null;
            }

            Control gameControl = UI_Interface.GameList.Controls.Find(gameGuid,false).FirstOrDefault();

            if (gameControl != null)
            {
                return gameControl as GameControl;
            }

            return null;
        }

        public static void ReselectGameControl(string gameGuid)
        {
            GameControl gameControl = GetGameControl(gameGuid);

            if (gameControl != null)
            {
                UI_Functions.GameList_SelectedChanged(gameControl, null);
                gameControl.RadioSelected();
                UI_Interface.CurrentGameListControl = gameControl;
                UI_Interface.GameList.ScrollControlIntoView(gameControl);
            }
        }

        public static void ReturnToGameControl(string gameGuid)
        {
            GameControl gameControl = GetGameControl(gameGuid);

            if (gameControl != null)
            {
                gameControl.RadioSelected();
                UI_Interface.CurrentGameListControl = gameControl;
                UI_Interface.GameList.ScrollControlIntoView(gameControl);
            }
        }

        public static void SelectGameControl(string gameGuid)
        {
            GameControl gameControl = GetGameControl(gameGuid);

            if (gameControl != null)
            {
               
                UI_Interface.CurrentGameListControl = gameControl;
                UI_Interface.GameList.ScrollControlIntoView(gameControl);
                //UI_Functions.GameList_SelectedChanged(gameControl, null);
                UI_Actions.On_GameChange.Invoke();
                gameControl.RadioSelected();
                gameControl.Focus();
            }
        }


        private static void UpdateGameControlGenericGameinfo(UserGameInfo userGameInfo)
        {
            GameControl gameControl = GetGameControl(userGameInfo.GameGuid);

            if (gameControl != null)
            {
                if (gameControl.UserGameInfo.HandlerInfo == null)
                {
                    if (current_UserGameInfo == userGameInfo)
                    {
                        UI_Functions.RefreshUI(true);
                    }

                    UI_Interface.GameList.Controls.Remove(gameControl);
                    return;
                }

                if (Current_UserGameInfo?.GameGuid == gameControl.Name)
                {
                    ReselectGameControl(Current_UserGameInfo.GameGuid);
                }
            }
        }

        public static void AddGameToList(UserGameInfo userGameInfo)
        {
            AddGameControl(userGameInfo);
            RefreshGamesList();
        }

        public static void RemoveGameFromList(string gameGuid)
        {
            Control gameControl = GetGameControl(gameGuid);

            if (gameControl != null)
            {
                UI_Interface.GameList.Controls.Remove(gameControl);
                gameControl.Dispose();

                if (Current_UserGameInfo?.GameGuid == gameGuid)
                {
                    Current_UserGameInfo = null;
                    UI_Functions.RefreshUI(true);
                }
            }
        }

        private static GameControl AddGameControl(UserGameInfo userGameInfo)
        {
            GameControl con = new GameControl(userGameInfo)
            {
                Width = UI_Interface.GameListContainer.Width,
            };

            con.Click += Generic_Functions.WebviewDisposed;
            con.HandlerUpdate.Click += UI_Functions.Button_UpdateAvailable_Click;
            con.GameOptions.Click += UI_Functions.GameOptionsButton_Click;
            con.OnHandlerInfoRefresh = null;
            con.OnHandlerInfoRefresh += UpdateGameControlGenericGameinfo;

            UI_Interface.GameList.Controls.Add(con);

            ThreadPool.QueueUserWorkItem(GetGameIcon.GetIcon, userGameInfo);

            return con;
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
            //Prevent winforms to focus buttons randomly
            UI_Interface.SetupPanel.Focus();

            //Hide profiles list
            if (step > 0)
            {
                UI_Functions.OnClickHideProfileList(null, null);
            }

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
                CustomStep customStep = customSteps[customStepIndex];

                customStep.UpdateRequired?.Invoke();

                if (customStep.Required)
                {
                    JsControl.CustomStep = customStep;
                    JsControl.Content = HandlerContent;
                }
                else
                {
                    UI_Functions.EnablePlay(true);
                    return;
                }
            }

            KillCurrentStep();

            if (GameProfile.Ready && GameProfile.TotalProfilePlayers == GameProfile.TotalAssignedPlayers)
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
                        UI_Functions.EnablePlay(true);
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

            GameProfile.InitializeHandlerStartup(Current_UserGameInfo);

            GameProfile.I_GameHandler.InitializeEnd += Handler_InitializeEnd;
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
                UI_Interface.ProfileButtonsPanel.Visible = UI_Interface.SetupPanel.Visible && CurrentStepIndex == 0 && !App_Misc.DisableGameProfiles;                            
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
                UI_Functions.EnablePlay(true);
                return;
            }

            if (CurrentStepIndex + 1 > StepsList.Count - 1)
            {
                UI_Functions.EnablePlay(true);
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

            UIThread.Post(() =>
            {
                if (!UI_Interface.IsFormClosing)
                {
                    UI_Functions.HideEndingMessage();
                    GameProfile.I_GameHandler = null;
                    UI_Interface.CurrentGameListControl = null;
                    Current_UserGameInfo = null;
                    UI_Interface.MainForm.WindowState = FormWindowState.Normal;

                    UI_Interface.WindowPanel.Focus();
                    UI_Interface.PlayButton.Tag = "START";
                    UI_Interface.PlayButton.Visible = false;

                    UI_Interface.MainForm.BringToFront();
                
                    RefreshGamesList();
                    
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

        private static void Handler_InitializeEnd()
        {
            UI_Functions.ShowEndingMessage();
        }
    }
}
