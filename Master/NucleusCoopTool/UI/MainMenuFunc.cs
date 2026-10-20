using Nucleus.Coop.Forms;
using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.UI;
using System;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class MainMenuFunc
    {

        public static void MainMenu_Opening(object sender, EventArgs e)
        {
            ContextMenuStrip mainMenu = (ContextMenuStrip)sender;

            ToolStripMenuItem autoSearchMenuItem = mainMenu.Items["autoSearchGames"] as ToolStripMenuItem;
            autoSearchMenuItem.Click -= AutoSearchMenuItem_Click;
            autoSearchMenuItem.Click += AutoSearchMenuItem_Click;

            ToolStripMenuItem manualSearchMenuItem = mainMenu.Items["manualGameSearch"] as ToolStripMenuItem;
            manualSearchMenuItem.Click -= ManualSearchMenuItem_Click;
            manualSearchMenuItem.Click += ManualSearchMenuItem_Click;

            ToolStripMenuItem downloadGameAssetsMenuItem = mainMenu.Items["downloadGameAssets"] as ToolStripMenuItem;
            downloadGameAssetsMenuItem.Click -= DownloadGameAssetsMenuItem_Click;
            downloadGameAssetsMenuItem.Click += DownloadGameAssetsMenuItem_Click;

            ToolStripMenuItem extractHandlerMenuItem = mainMenu.Items["extractHandler"] as ToolStripMenuItem;
            extractHandlerMenuItem.Click -= ExtractHandlerMenuItem_Click;
            extractHandlerMenuItem.Click += ExtractHandlerMenuItem_Click;

            ToolStripMenuItem handlerManagerMenuItem = mainMenu.Items["handlerManager"] as ToolStripMenuItem;
            handlerManagerMenuItem.Click -= HandlerManagerMenuItem_Click;
            handlerManagerMenuItem.Click += HandlerManagerMenuItem_Click;
        }

        private static void AutoSearchMenuItem_Click(object sender, EventArgs e)
        {
            if (GamePathForm.Instance != null)
            {
                GamePathForm.Instance.BringToFront();
                return;
            }

            //auto search games
            if (SearchDisksForm.Instance == null)
            {
                SearchDisksForm sDF = new SearchDisksForm();
                sDF.Show();
            }
            else
            {
                SearchDisksForm.Instance.BringToFront();
            }
        }

        public static void ManualSearchMenuItem_Click(object sender, EventArgs e)
        {
            _ = SearchGame.Search(null, null); 
        }

        public static async void DownloadGameAssetsMenuItem_Click(object sender, EventArgs e)
        {
            if (Core_Interface.GameManager.User.Games.Count == 0)
            {
                Globals.MainOSD.Show(1600, $"*@{OSDHighlightColors.Info}|Add Game(s) To Your List@*");
                return;
            }

            await new GameAssets.AssetsDownloader().DownloadAllGamesAssets(UI_Interface.CurrentGameListControl);

            UIThread.Post(() =>
            {
                if (Core_Interface.Current_UserGameInfo != null && UI_Interface.SetupPanel.Visible)
                {
                    GameAssets.LoadGameAssets();
                }

                UI_Interface.WindowPanel.Select();
            });
        }

        private static void ExtractHandlerMenuItem_Click(object sender, EventArgs e) => ExtractHandler.Extract();

        private static void HandlerManagerMenuItem_Click(object sender, EventArgs e)
        {
            //handlers manager
            if (HandlersManager.Instance == null)
            {
                HandlersManager s = new HandlersManager();
                s.Initialize();
            }
            else
            {
                HandlersManager.Instance?.Form?.BringToFront();
            }
        }
      
        public static void OnFocusLost(object sender, EventArgs e)
        {
            ContextMenuStrip mainMenu = sender as ContextMenuStrip;
            mainMenu.Close();
        } 
    }
}
