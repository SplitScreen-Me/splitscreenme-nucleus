using Nucleus.Coop.Controls;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Windows;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class Runtime_Controls
    {
        public static HandlerNotesZoom BuildHandlerNotesZoom()
        {
            HandlerNotesZoom handlerNotesZoom = new HandlerNotesZoom
            {
                Visible = false,
                Size = UI_Interface.MainForm.Size,
                Location = new Point(0, 0),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom,
            };

            return handlerNotesZoom;
        }

        public static PictureBox BuildMainButtonsPanelButton()
        {
            PictureBox mainButtonsPanel = new PictureBox()
            {
                BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "buttons_dropdown.png"),
                Anchor = AnchorStyles.None,
                BackgroundImageLayout = ImageLayout.Stretch,
            };

            //mainButtonsPanel.MouseEnter += ShowMainButtonsPanel;

            return mainButtonsPanel;
        }

        private static System.Windows.Forms.Timer mainButtonsPanelTimer;

        //private static void ShowMainButtonsPanel(object sender, EventArgs e)
        //{
        //    UI_Interface.MainButtonsPanel.Visible = true;

        //    //if (mainButtonsPanelTimer == null)
        //    //{
        //    //    mainButtonsPanelTimer = new System.Windows.Forms.Timer();
        //    //    mainButtonsPanelTimer.Interval = (200);//millisecond
        //    //    mainButtonsPanelTimer.Tick += ShowMainButtonsPanelTick;
        //    //}

        //    //mainButtonsPanelTimer.Start();
        //}

        private static void ShowMainButtonsPanelTick(object Object, EventArgs EventArgs)
        {
            if (!UI_Interface.MainButtonsPanel.Bounds.Contains(UI_Interface.MainForm.PointToClient(Cursor.Position)))
            {
                UI_Interface.MainButtonsPanel.Visible = false;
                mainButtonsPanelTimer.Stop();
            }
        }

        public static void Insert_SearchFieldControls()
        {
            float scale = (float)User32Util.GetDpiForWindow(UI_Interface.MainForm.Handle) / (float)100;
            int size = (int)((float)15 * scale);
            int offset = (int)((float)12 * scale);

            UI_Interface.SearchTextBox = new SearchTextBox();
            UI_Interface.SearchTextBox.Location = new Point(4 ,0);
            UI_Interface.SearchTextBox.Size = new Size((UI_Interface.GameListContainer.Width - (size *2)) - offset, UI_Interface.GameList.Controls[0].Height / 2);
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.SearchTextBox);

            UI_Interface.SortGamesButton = new SortGamesButton(new Size(size, size), new Point(UI_Interface.SearchTextBox.Right, (UI_Interface.SearchTextBox.Top + UI_Interface.SearchTextBox.Height / 2) - size / 2));
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.SortGamesButton);
            UI_Interface.SearchTextBox.Toggle_Visiblility += UI_Interface.SortGamesButton.ToggleVisibility;

            UI_Interface.ToggleFavoriteButton = new ToggleFavoriteButton(new Size(size, size), new Point(UI_Interface.GameListContainer.Right - (size + 5), UI_Interface.SortGamesButton.Top));
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.ToggleFavoriteButton);
            UI_Interface.SearchTextBox.Toggle_Visiblility += UI_Interface.ToggleFavoriteButton.ToggleVisibility;
        }


        public static void Insert_HubButton()
        {
            UI_Interface.HubButton = new HubButton(UI_Interface.GameListContainer.Width, UI_Interface.GameList.Controls[0].Height);
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.HubButton);
            UI_Interface.GameList.Height -= UI_Interface.HubButton.Height;
            UI_Interface.HubButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            UI_Interface.HubButton.Location = new Point(0, UI_Interface.SearchTextBox.Bottom);

           // UI_Interface.GameListContainer.Controls.Add(UI_Interface.MainButtonsPanel);
            //UI_Interface.MainButtonsPanel.Location = new Point(UI_Interface.HubButton.Left, UI_Interface.HubButton.Bottom -10);
            //UI_Interface.MainButtonsPanel.Visible = true;
           
            //UI_Interface.WindowPanel.Controls.Remove(UI_Interface.MainButtonsPanel);
            //UI_Interface.SearchGameButton = new SearchGameButton(UI_Interface.HubButton.Width, UI_Interface.HubButton.Height);
            //UI_Interface.GameListContainer.Controls.Add(UI_Interface.SearchGameButton);
            //UI_Interface.GameList.Height -= UI_Interface.SearchGameButton.Height;
            //UI_Interface.SearchGameButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //UI_Interface.SearchGameButton.Location = new Point(UI_Interface.HubButton.Left, UI_Interface.HubButton.Bottom);
            ////
            //UI_Interface.ExtractHandlerButton = new ExtractButton(UI_Interface.HubButton.Width, UI_Interface.HubButton.Height);
            //UI_Interface.GameListContainer.Controls.Add(UI_Interface.ExtractHandlerButton);
            //UI_Interface.GameList.Height -= UI_Interface.ExtractHandlerButton.Height;
            //UI_Interface.ExtractHandlerButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            //UI_Interface.ExtractHandlerButton.Location = new Point(UI_Interface.HubButton.Left, UI_Interface.SearchGameButton.Bottom);
            //
            UI_Interface.GameList.Top = UI_Interface.HubButton.Bottom;
        }

    }
}
