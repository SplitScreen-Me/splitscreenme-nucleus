using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public class ToggleFavoriteButton : PictureBox
    {
        private Bitmap favorite_Unselected;
        private Bitmap favorite_Selected;

        public ToggleFavoriteButton(Size size, Point location)
        {
            favorite_Unselected = ImageCache.GetImage(Globals.ThemeFolder + "favorite_unselected.png");
            favorite_Selected = ImageCache.GetImage(Globals.ThemeFolder + "favorite_selected.png");

            SizeMode = PictureBoxSizeMode.StretchImage;
            BackColor = Color.Transparent;
            Cursor = Theme_Settings.Hand_Cursor;
            Size = new Size(size.Width, size.Height);
            Location = location;
            Image = UI_Interface.ShowFavoriteOnly ? favorite_Selected : favorite_Unselected;
            CustomToolTips.SetToolTip(this, "Show favorite games only.", this.GetType().ToString(), new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            Click += FavoriteOnly_Click;
            Click += Generic_Functions.ClickAnyControl;

            MouseEnter += FavoriteOnly_MouseEnter;
            MouseLeave += FavoriteOnly_MouseLeave;
        }


        private void FavoriteOnly_Click(object sender, EventArgs e)
        {
            if (GameManager.Instance.User.Games.All(g => g.Game.MetaInfo.Favorite == false) && !UI_Interface.ShowFavoriteOnly) { return; }

            bool selected = Image.Equals(favorite_Selected);

            Image = selected ? favorite_Unselected : favorite_Selected;
            UI_Interface.ShowFavoriteOnly = !selected;

            App_Misc.ShowFavoriteOnly = UI_Interface.ShowFavoriteOnly;
            SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
            UI_Interface.MainForm.Invalidate(false);
        }

        private void FavoriteOnly_MouseEnter(object sender, EventArgs e)
        {
            Control con = sender as Control;
            con.Size = new Size(con.Width += 3, con.Height += 3);
            con.Location = new Point(con.Location.X - 1, con.Location.Y - 1);
        }

        private void FavoriteOnly_MouseLeave(object sender, EventArgs e)
        {
            Control con = sender as Control;
            con.Size = new Size(con.Width -= 3, con.Height -= 3);
            con.Location = new Point(con.Location.X + 1, con.Location.Y + 1);
        }
    }
}