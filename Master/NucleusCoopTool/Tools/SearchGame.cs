using Nucleus.Coop.Forms;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Http_Client;
using Nucleus.Gaming.Tools.Steam;
using Nucleus.Gaming.Tools.UserDriveInfo;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nucleus.Coop.Tools.GameAssets;
using static Nucleus.Gaming.Forms.SearchProgress;

namespace Nucleus.Coop.Tools
{
    internal class SearchGame
    {       
        public static async Task<bool> Search(string exeName, HandlerInfo handlerInfo)
        {
            try
            {
                bool iniValueExists = !string.IsNullOrEmpty(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));

                string result = null;//eventually better to add a default path, but what could it be?

                if (handlerInfo != null)
                {                 
                    if (!iniValueExists)
                    {
                        DialogResult userChoice = MessageBox.Show(
                            "Do you want to enable Nucleus’s automatic game executable search feature?\n" +
                            "If enabled, you will be asked to select the directories where your games are installed\n" +
                            "(for example, “C:\\Program Files(x86)”).\n" +
                            "Nucleus will then search those locations for you.\n" +
                             "Note: Avoid selecting the root of a drive when possible\n" +
                             "(for example, “C:\\”),\n" +
                             "as this may unnecessarily slow down the search process in most cases.", "Enable Automatic Game Executables Search?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (userChoice == DialogResult.Yes)
                        {
                            Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=True" });

                            if(GameManager.Instance.GamesSearcheDirectories.Count == 0)
                            {
                                GamePathForm gpf = new GamePathForm();
                                gpf.ShowDialog();
                            }
                            else
                            {
                                SearchProgressForm searchform = new SearchProgressForm();
                                var paths = await searchform.StartSearchAsync(handlerInfo, GameManager.Instance.GamesSearcheDirectories);
                                result = paths?.InstallDirectory;
                            }
                        }
                        else
                        {
                            Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=False" });
                            if (!string.IsNullOrEmpty(handlerInfo.GenericGameInfo.SteamID))
                            {
                                result = SteamFunctions.TryGetSteamGameInstallPath(handlerInfo);
                            }
                        }
                    }
                    else
                    {
                        if (bool.Parse(Globals.ini.IniReadValue("Misc", "AutoSearchGame")))
                        {
                            if (GameManager.Instance.GamesSearcheDirectories.Count == 0)
                            {
                                GamePathForm gpf = new GamePathForm();
                                gpf.ShowDialog();
                            }

                            SearchProgressForm searchform = new SearchProgressForm();
                            var paths = await searchform.StartSearchAsync(handlerInfo, GameManager.Instance.GamesSearcheDirectories);
                            result = paths?.InstallDirectory;
                        }
                        else if (!string.IsNullOrEmpty(handlerInfo.GenericGameInfo.SteamID))
                        {
                            result = SteamFunctions.TryGetSteamGameInstallPath(handlerInfo);
                        }
                    }
                }
            
                using (System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog())
                {
                    open.InitialDirectory = result; 
                    open.CheckFileExists = true;
                    open.DefaultExt = "exe";

                    if (string.IsNullOrEmpty(exeName))
                    {
                        open.Title = "Select a game executable to add to Nucleus";
                        open.Filter = "Game Executable Files|*.exe";
                    }
                    else
                    {                    
                        if(exeName.Contains(","))
                        {
                            exeName = Regex.Replace(exeName, @"[\\\""]", " ")
                           .Replace(",", " or ");

                            open.Title = $"Select {exeName}";
                            open.Filter = "Valid Executables|*.exe";
                        }
                        else
                        {
                            open.Title = $"Select {exeName}";
                            open.Filter = $"Valid Executables |{exeName}";
                        }    
                    }                   

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        string path = open.FileName;

                        if (UserDriveInfo.IsExFat(path, false))
                        {
                            return false;
                        }

                        (List<HandlerInfo>, string) handlerInfos;

                        if (handlerInfo != null)
                        {
                            handlerInfos = GameManager.Instance.GetGameByJsFilePath(handlerInfo.FileFullPath,path);
                        }
                        else
                        {
                            handlerInfos = GameManager.Instance.GetGames(path);
                        }
                      

                        if(handlerInfos.Item1.Count == 0)
                        {
                            MessageBox.Show(string.Format(handlerInfos.Item2, Path.GetFileName(path)), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return false;
                        }

                        if (handlerInfos.Item1.Count > 1)
                        {
                            GameList list = new GameList(handlerInfos.Item1);

                            if (list.ShowDialog() == DialogResult.OK)
                            {
                                if (list.Selected != null)
                                {
                                    Console.WriteLine($"Selected handler info: {list.Selected.GameName} - {list.Selected.GUID}");
                                    return await AddGame(list.Selected,path, handlerInfos.Item2);
                                   
                                }
                                else
                                {
                                    MessageBox.Show("No handler file selected", "Aborted", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            return await AddGame(handlerInfos.Item1[0], path, handlerInfos.Item2);                   
                        }
                    }
                }
            }
            catch (Exception)
            {

                return false;
            }

            return false;
        }

        private static async Task<bool> AddGame(HandlerInfo _handlerInfo,string path,string reason)
        {
            if (_handlerInfo != null)
            {
                UserGameInfo userGameInfo = GameManager.Instance.TryAddGame(path, _handlerInfo);

                if (userGameInfo == null) { return false; }

                Globals.MainOSD.Show(2000, $"*@{OSDHighlightColors.Info}|{userGameInfo?.HandlerInfo.GameName}@* *@{OSDHighlightColors.Regular}|has been installed successfully@*");

                Core_Interface.AddGameToList(userGameInfo);

                if (AssetsDownloader.ShouldPromptUserForAssestsDownload(userGameInfo))
                {
                    if (!string.IsNullOrEmpty(_handlerInfo.GenericGameInfo.HandlerId))
                    {
                        if (Http_Client.HubConnected)
                        {
                            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to download game cover and screenshots?", "Download game assets?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (dialogResult == DialogResult.Yes)
                            {
                                await new GameAssets.AssetsDownloader().DownloadGameAssets(userGameInfo, null);

                                UIThread.Post(() =>
                                {
                                    if (Core_Interface.Current_UserGameInfo != null && UI_Interface.MainForm.SetupPanel.Visible)
                                    {
                                        GameAssets.LoadGameAssets();
                                    }

                                    UI_Interface.MainForm.WindowPanel.Select();
                                });
                            }
                        }
                    }
                }

                return true;
            }
            else
            {
                MessageBox.Show(string.Format(reason, Path.GetFileName(path)), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

        }
    }
}
