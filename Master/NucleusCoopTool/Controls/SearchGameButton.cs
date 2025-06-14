using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nucleus.Gaming.UI;
using Nucleus.Coop.UI;
using Nucleus.Coop.Tools;

namespace Nucleus.Coop.Controls
{
    public partial class SearchGameButton : DoubleBufferPanel
    {
        private PictureBox btn_AddGamePb;
        private Label btn_AddGameLabel;

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

        public SearchGameButton(int width, int height)
        {
            InitializeComponent();

            Size = new Size(width, height);
            BackColor = Color.FromArgb(255, 0, 0, 0);
            MouseEnter += ZoomInPicture;
            MouseLeave += ZoomOutPicture;
            Cursor = Theme_Settings.Hand_Cursor;

            int baseSize = Height - 3;

            btn_AddGamePb = new PictureBox()
            {
                BackgroundImageLayout = ImageLayout.Stretch,
                Size = new Size(baseSize, baseSize),
                Location = new Point(0, 0),
                Cursor = Theme_Settings.Hand_Cursor,
            };

            btn_AddGamePb.MouseEnter += ZoomInPicture;
            btn_AddGamePb.MouseLeave += ZoomOutPicture;
            btn_AddGamePb.Click += UI_Functions.SearchGameButton_Click;

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
            btn_AddGameLabel.Click += UI_Functions.SearchGameButton_Click;

            Controls.Add(btn_AddGamePb);
            Controls.Add(btn_AddGameLabel);

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

            CustomToolTips.SetToolTip(btn_AddGameLabel, "Search and add a game to the game list (its handler must be installed).", "btnSearch", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(btn_AddGamePb, "Search and add a game to the game list (its handler must be installed).", "btnSearch", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(this, "Search and add a game to the game list (its handler must be installed).", "btnSearch", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            Click += UI_Functions.SearchGameButton_Click;

            btn_AddGameLabel.Text = "Search Games";

            btn_AddGamePb.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "search_game.png");
            btn_AddGameLabel.Location = new Point(btn_AddGamePb.Right + 7, (btn_AddGamePb.Location.Y + btn_AddGamePb.Height / 2) - (btn_AddGameLabel.Height / 2));
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

