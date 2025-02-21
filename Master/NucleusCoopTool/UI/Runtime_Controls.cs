using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
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

            mainButtonsPanel.MouseEnter += ShowMainButtonsPanel;

            return mainButtonsPanel;
        }

        private static System.Windows.Forms.Timer mainButtonsPanelTimer;

        private static void ShowMainButtonsPanel(object sender, EventArgs e)
        {
            UI_Interface.MainButtonsPanel.Visible = true;

            if (mainButtonsPanelTimer == null)
            {
                mainButtonsPanelTimer = new System.Windows.Forms.Timer();
                mainButtonsPanelTimer.Interval = (200);//millisecond
                mainButtonsPanelTimer.Tick += ShowMainButtonsPanelTick;
            }

            mainButtonsPanelTimer.Start();
        }

        private static void ShowMainButtonsPanelTick(object Object, EventArgs EventArgs)
        {
            if (!UI_Interface.MainButtonsPanel.Bounds.Contains(UI_Interface.MainForm.PointToClient(Cursor.Position)))
            {
                UI_Interface.MainButtonsPanel.Visible = false;
                mainButtonsPanelTimer.Stop();
            }
        }

    }
}
