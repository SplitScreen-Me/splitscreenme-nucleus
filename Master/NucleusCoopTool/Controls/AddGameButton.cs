using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.UI;
using Nucleus.Coop.UI;
using Nucleus.Coop.Tools;

namespace Nucleus.Coop.Controls
{
    public partial class AddGameButton : DoubleBufferPanel
    {
        private MainForm mainForm => UI_Interface.MainForm;
        private Panel favoriteContainer;
        private PictureBox btn_AddGamePb;
        private PictureBox favoriteOnly;
        private Label btn_AddGameLabel;

        private Bitmap favorite_Unselected;
        private Bitmap favorite_Selected;

        private bool selected;
        public bool Selected
        {
            get => selected;
            set
            {
               selected = value;
               Invalidate();            
            }
        }

        public AddGameButton(int width, int height)
        {
            InitializeComponent();

            favorite_Unselected = ImageCache.GetImage(Globals.ThemeFolder + "favorite_unselected.png");
            favorite_Selected = ImageCache.GetImage(Globals.ThemeFolder + "favorite_selected.png");

            Size = new Size(width, height);
            BackColor = Color.Transparent;
            MouseEnter += ZoomInPicture;
            MouseLeave += ZoomOutPicture;
            Cursor = Theme_Settings.Hand_Cursor;

            int baseSize = Height - 10;

            btn_AddGamePb = new PictureBox()
            {
                BackgroundImageLayout = ImageLayout.Stretch,
                Size = new Size(baseSize, baseSize),
                Location = new Point(5, Height / 2 - baseSize / 2),
                Cursor = Theme_Settings.Hand_Cursor,
            };

            btn_AddGamePb.MouseEnter += ZoomInPicture;
            btn_AddGamePb.MouseLeave += ZoomOutPicture;

            btn_AddGameLabel = new Label()
            {
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font(Theme_Settings.CustomFont, 8, FontStyle.Bold, GraphicsUnit.Point, 0),
                AutoSize = true,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                BackColor = Color.Transparent,
                Cursor = Theme_Settings.Hand_Cursor,
            };

            btn_AddGameLabel.MouseEnter += ZoomInPicture;
            btn_AddGameLabel.MouseLeave += ZoomOutPicture;

            favoriteContainer = new DoubleBufferPanel()//So it is easier to click the favorite button without opening the webview
            {
                Size = new Size(Height, Height),
                BackColor = Color.Transparent,
                Cursor = Theme_Settings.Default_Cursor,
            };

            favoriteOnly = new PictureBox
            {
                SizeMode = PictureBoxSizeMode.StretchImage,
                BackColor = Color.Transparent,
                Cursor = Theme_Settings.Hand_Cursor,
                Size = new Size(baseSize / 2, baseSize / 2),
                Image = UI_Interface.ShowFavoriteOnly ? favorite_Selected : favorite_Unselected,
            };

            favoriteOnly.Click += FavoriteOnly_Click;
            favoriteOnly.MouseEnter += FavoriteOnly_MouseEnter;
            favoriteOnly.MouseLeave += FavoriteOnly_MouseLeave;

            CustomToolTips.SetToolTip(favoriteOnly, "Show favorite game(s) only.", "favoriteOnly", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            favoriteContainer.Controls.Add(favoriteOnly);

            Controls.Add(btn_AddGamePb);
            Controls.Add(btn_AddGameLabel);
            //Controls.Add(favoriteContainer);

            Click += Generic_Functions.ClickAnyControl;

            foreach (Control control in Controls)
            {
                control.Click += Generic_Functions.ClickAnyControl;
                if (control.HasChildren)
                {
                    foreach (Control child in control.Controls)
                    {
                        child.Click += Generic_Functions.ClickAnyControl;
                    }
                }
            }

            Update(mainForm.Connected);
        }

        public void Update(bool connected)
        {          
            Click += connected ? Generic_Functions.InsertWebview : new EventHandler(RefreshNetStatus);
            btn_AddGamePb.Click += connected ? Generic_Functions.InsertWebview : new EventHandler(RefreshNetStatus);
            btn_AddGameLabel.Click += connected ? Generic_Functions.InsertWebview : new EventHandler(RefreshNetStatus);

            btn_AddGameLabel.Text = connected ? "Add New Games" : "Offline";
            btn_AddGamePb.BackgroundImage = connected ? ImageCache.GetImage(Globals.ThemeFolder + "add_game.png") : ImageCache.GetImage(Globals.ThemeFolder + "title_no_hub.png");

            CustomToolTips.SetToolTip(this, connected ? "Install new game handlers." : OfflineToolTipText(), "btn_AddGame", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(btn_AddGamePb, connected ? "Install new game handlers." : OfflineToolTipText(), "btn_AddGamePb", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(btn_AddGameLabel, connected ? "Install new game handlers." : OfflineToolTipText(), "btn_AddGameLabel", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            btn_AddGameLabel.Location = new Point(btn_AddGamePb.Right + 7, (btn_AddGamePb.Location.Y + btn_AddGamePb.Height / 2) - (btn_AddGameLabel.Height / 2));
            favoriteContainer.Location = new Point((Width - favoriteContainer.Height) - 2, 0);
            favoriteOnly.Location = new Point((favoriteContainer.Width - favoriteOnly.Width) - 2, (favoriteContainer.Height / 2) - (favoriteOnly.Height / 2));
        }

        private void ZoomInPicture(object sender, EventArgs e)
        {
            btn_AddGamePb.Size = new Size(btn_AddGamePb.Width += 3, btn_AddGamePb.Height += 3);
            btn_AddGamePb.Location = new Point(btn_AddGamePb.Location.X - 1, btn_AddGamePb.Location.Y - 1);
        }

        private void ZoomOutPicture(object sender, EventArgs e)
        {
            btn_AddGamePb.Size = new Size(btn_AddGamePb.Width -= 3, btn_AddGamePb.Height -= 3);
            btn_AddGamePb.Location = new Point(btn_AddGamePb.Location.X + 1, btn_AddGamePb.Location.Y + 1);
        }

        private string OfflineToolTipText()
        {
            return "Nucleus can't reach hub.splitscreen.me." +
                   "\nClick this button to refresh, if the " +
                   "\nproblem persist, check our FAQ to learn more.";
        }

        private void RefreshNetStatus(object sender, EventArgs e)
        {
            mainForm.Connected = StartChecks.CheckHubResponse();
        }

        private void FavoriteOnly_Click(object sender, EventArgs e)
        {
            if (GameManager.Instance.User.Games.All(g => g.Game.MetaInfo.Favorite == false) && !UI_Interface.ShowFavoriteOnly) { return; }

            bool selected = favoriteOnly.Image.Equals(favorite_Selected);

            favoriteOnly.Image = selected ? favorite_Unselected : favorite_Selected;
            UI_Interface.ShowFavoriteOnly = !selected;

            App_Misc.ShowFavoriteOnly = UI_Interface.ShowFavoriteOnly;
            SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
            mainForm.Invalidate(false);
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
        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

