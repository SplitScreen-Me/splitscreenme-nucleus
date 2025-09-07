using Nucleus.Coop.Forms;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Tools.UserDriveInfo;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal class SearchGame
    {
        public static void Search( string exeName, GenericGameInfo genericGameInfo)
        {
            MainForm mainForm = UI_Interface.MainForm;

            try
            {
                string result = null;

                if (genericGameInfo != null)
                {
                    if (genericGameInfo.SteamID != null && genericGameInfo.SteamID != "")
                    {
                        result = GameManager.Instance.AutoSearchGameInstallPath(genericGameInfo);
                    }
                }
     
                using (System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog())
                {
                    open.InitialDirectory = result;

                    if (string.IsNullOrEmpty(exeName))
                    {
                        open.Title = "Select a game executable to add to Nucleus";
                        open.Filter = "Game Executable Files|*.exe";
                    }
                    else
                    {
                        open.Title = string.Format("Select {0} to add the game to Nucleus", exeName);
                        open.Filter = "Game Exe|" + exeName;
                    }

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        string path = open.FileName;

                        if (UserDriveInfo.IsExFat(path,false))
                        {
                            return;
                        }

                        List<GenericGameInfo> info = GameManager.Instance.GetGames(path);

                        if (info.Count > 1)
                        {
                            GameList list = new GameList(info);

                            if (list.ShowDialog() == DialogResult.OK)
                            {
                                UserGameInfo game = GameManager.Instance.TryAddGame(path, list.Selected);

                                if (game != null && list.Selected != null)
                                {
                                    if (list.Selected.HandlerId != null && list.Selected.HandlerId != "" && mainForm.Connected)
                                    {
                                        MessageBox.Show(string.Format("The game {0} has been added!", game.Game.GameName), "Nucleus - Game added");
                                        DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to download game cover and screenshots?", "Download game assets?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                        if (dialogResult == DialogResult.Yes)
                                        {
                                            AssetsDownloader.DownloadGameAssets(game, null);
                                        }
                                    }
                                }
                            }

                            UI_Functions.RefreshUI(true);
                        }
                        else if (info.Count == 1)
                        {
                            UserGameInfo game = GameManager.Instance.TryAddGame(path, info[0]);

                            if(game == null){ return; }

                            if (info[0].HandlerId != null && info[0].HandlerId != "")
                            {
                                if (mainForm.GameOptionMenu != null && mainForm.Connected)
                                {
                                    MessageBox.Show(string.Format("The game {0} has been added!", game.Game.GameName), "Nucleus - Game added");
                                    DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to download game cover and screenshots?", "Download game assets?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (dialogResult == DialogResult.Yes)
                                    {
                                        AssetsDownloader.DownloadGameAssets(game, null);
                                    }
                                }
                            }

                            UI_Functions.RefreshUI(true);
                        }
                        else
                        {
                            MessageBox.Show(string.Format("The executable '{0}' was not found in any game handler's Game.ExecutableName field. Game has not been added.", Path.GetFileName(path)), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }
    }
}
