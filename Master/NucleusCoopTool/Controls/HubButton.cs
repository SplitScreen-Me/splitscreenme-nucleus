using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.Windows.Forms;
using Nucleus.Gaming.UI;
using Nucleus.Coop.UI;
using Nucleus.Gaming.Http_Client;
using NAudio.CoreAudioApi;
using static Nucleus.Gaming.Http_Client.Http_Client;

namespace Nucleus.Coop.Controls
{
    public partial class HubButton : DoubleBufferPanel
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

            Update(Http_Client.HubConnected ? HubResponse.Success : HubResponse.Error);
        }

        public void Update(HubResponse hubResponse)
        {          
            UIThread.Post(() =>
            {
                UpdateControls(hubResponse is HubResponse.Success);
            });
        }

        private void UpdateControls(bool connected)
        {
            if (connected)
            {
                Click -= Generic_Functions.InsertWebview;
                btn_AddGamePb.Click -= Generic_Functions.InsertWebview;
                btn_AddGameLabel.Click -= Generic_Functions.InsertWebview;

                Click += Generic_Functions.InsertWebview;
                btn_AddGamePb.Click += Generic_Functions.InsertWebview;
                btn_AddGameLabel.Click += Generic_Functions.InsertWebview;
            }
            else
            {
                Click -= Generic_Functions.InsertWebview;
                btn_AddGamePb.Click -= Generic_Functions.InsertWebview;
                btn_AddGameLabel.Click -= Generic_Functions.InsertWebview;
            }

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
                   "If the problem persist, check our FAQ to learn more.";
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            this.ResumeLayout(false);
        }
    }
}

