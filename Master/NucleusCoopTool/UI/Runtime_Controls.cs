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

        public static void Insert_SearchFieldControls()
        {
            float scale = (float)User32Util.GetDpiForWindow(UI_Interface.MainForm.Handle) / (float)100;
            int size = (int)((float)15 * scale);
            int offset = (int)((float)12 * scale);

            UI_Interface.SearchTextBox = new SearchTextBox();
            UI_Interface.SearchTextBox.Location = new Point(4 ,4);
            UI_Interface.SearchTextBox.Size = new Size((UI_Interface.GameListContainer.Width - (size *2)) - offset, UI_Interface.GameList.Controls[0].Height / 2);
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.SearchTextBox);

            UI_Interface.SortGamesButton = new SortGamesButton(new Size(size, size), new Point(UI_Interface.SearchTextBox.Right + (int)( 3 * scale), (UI_Interface.SearchTextBox.Top + UI_Interface.SearchTextBox.Height / 2) - size / 2));
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.SortGamesButton);

            UI_Interface.ToggleFavoriteButton = new ToggleFavoriteButton(new Size(size, size), new Point(UI_Interface.SortGamesButton.Right + (int)(3 * scale), UI_Interface.SortGamesButton.Top));
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.ToggleFavoriteButton);
        }

        public static void Insert_HubButton()
        {
            UI_Interface.HubButton = new HubButton(UI_Interface.GameListContainer.Width/*(UI_Interface.GameListContainer.Width - 5) - (UI_Interface.GameList.Controls[0].Height/2)*/, UI_Interface.GameList.Controls[0].Height);
            UI_Interface.GameListContainer.Controls.Add(UI_Interface.HubButton);
            UI_Interface.GameList.Height -= UI_Interface.HubButton.Height + 7;
            UI_Interface.HubButton.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            UI_Interface.HubButton.Location = new Point(0, UI_Interface.SearchTextBox.Bottom);
            UI_Interface.GameList.Top = UI_Interface.HubButton.Bottom;
        }
    }
}
