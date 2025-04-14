using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nucleus.Gaming.UI;
using Nucleus.Coop.UI;

namespace Nucleus.Coop.Controls
{
    public partial class HubButton : DoubleBufferPanel
    {
        private MainForm mainForm => UI_Interface.MainForm;
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

        public HubButton(int width, int height)
        {
            InitializeComponent();

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

            MouseDoubleClick += RefreshHandlers;
            Update(mainForm.Connected);
        }

        private void RefreshHandlers(object sender, MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Right)
            {
                Core_Interface.RefreshHandlers();
                Globals.MainOSD.Show(1000, "Game Handlers Refreshed");
            }
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

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

