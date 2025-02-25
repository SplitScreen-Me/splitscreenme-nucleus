using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.UI;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public partial class SortGamesButton : PictureBox
    {
        public SortGamesButton(Size size, Point location)
        {
            Image = ImageCache.GetImage(Globals.ThemeFolder + "buttons_dropdown.png");

            SizeMode = PictureBoxSizeMode.StretchImage;
            BackColor = Color.Transparent;
            Cursor = Theme_Settings.Hand_Cursor;
            Size = new Size(size.Width, size.Height);
            Location = location;
            CustomToolTips.SetToolTip(this, "Game list sorting options.", this.GetType().ToString(), new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            Click += OpenSortOptions;
            
        }

        private void OpenSortOptions(object sender , object e)
        {           
            UI_Interface.SortOptionsPanel.Location = new Point(UI_Interface.GameListContainer.Right, UI_Interface.GameListContainer.Top);
            UI_Interface.SortOptionsPanel.Visible = !UI_Interface.SortOptionsPanel.Visible;
            UI_Interface.SortOptionsPanel.BringToFront();
        }
    }
}
