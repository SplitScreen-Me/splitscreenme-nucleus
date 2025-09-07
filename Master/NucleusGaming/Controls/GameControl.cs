using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public class GameControlAlt : GameControl, IDynamicSized, IRadioControl
    {
        public GameControlAlt(GenericGameInfo game, UserGameInfo userGame, bool favorite) : base(game, userGame, favorite)
        {
            GameInfo = game;

            HandlerUpdate.Visible = false;
            GameOptions.Visible = false;
            FavoriteBox.Visible = false;
            IsUpdateAvailableTimer = null;
        }

        protected override void OnPaint(PaintEventArgs e) {}
    }

    public class GameControl : UserControl, IDynamicSized, IRadioControl
    {
        public GenericGameInfo GameInfo { get; set; }
        public UserGameInfo UserGameInfo { get; set; }
        private PictureBox picture;
        private PictureBox playerIcon;
        public PictureBox FavoriteBox;
        private Label title;
        private Label players;
        public PictureBox HandlerUpdate;
        public PictureBox GameOptions;

        private Color radioSelectedBackColor;
        private Color defaultForeColor;

        public bool favorite;
        private bool _updateAvailable;
        private bool updateAvailable
        {
            get => _updateAvailable;
            set
            {
                if (_updateAvailable != value)
                {
                    _updateAvailable = value;
                    title.ForeColor = value ? Color.FromArgb(255, 196, 145, 18) : defaultForeColor;
                    HandlerUpdate.Visible = value;
                    Invalidate();
                }
            }
        }
       
        public string TitleText { get; set; }
        public string PlayerText { get; set; }

        private Bitmap favorite_Unselected;
        private Bitmap favorite_Selected;
        public System.Windows.Forms.Timer IsUpdateAvailableTimer;

        private Size maxSize;
        private Size minSize;
        private Point maxLoc;
        private Point minLoc;

        public GameControl(GenericGameInfo game, UserGameInfo userGame, bool favorite)
        {
            try
            {
                this.favorite = favorite;
                IniFile theme = Globals.ThemeConfigFile;

                string themePath = Globals.ThemeFolder;

                defaultForeColor = Theme_Settings.ControlsForeColor;
                radioSelectedBackColor = Theme_Settings.SelectedBackColor;
                favorite_Unselected = ImageCache.GetImage(themePath + "favorite_unselected.png");
                favorite_Selected = ImageCache.GetImage(themePath + "favorite_selected.png");

                AutoScaleDimensions = new SizeF(96F, 96F);
                AutoScaleMode = AutoScaleMode.Dpi;
                AutoSize = false;
                Size = new Size(209, 42);
                Cursor = Theme_Settings.Hand_Cursor;

                GameInfo = game;
                UserGameInfo = userGame;

                picture = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                };

                playerIcon = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = ImageCache.GetImage(themePath + "players.png")
                };

                title = new Label
                {
                    AutoSize = true,
                    Font = new Font(Theme_Settings.CustomFont, Theme_Settings.FontSize - 1.75F, FontStyle.Bold, GraphicsUnit.Point, 0),  
                };

                players = new Label
                {
                    AutoSize = true,
                    Font = new Font(Theme_Settings.CustomFont, Theme_Settings.FontSize - 3.00F, FontStyle.Bold, GraphicsUnit.Point, 0)
                };

                if (game == null)
                {
                    title.Text = "No games";
                    title.Font = new Font(Theme_Settings.CustomFont, Theme_Settings.FontSize - 1.00F, FontStyle.Bold, GraphicsUnit.Point, 0);
                    Visible = false;
                }
                else
                {
                    title.Text = GameInfo.GameName;
                    if (GameInfo.MaxPlayers > 2)
                    {
                        players.Text = "2 - " + GameInfo.MaxPlayers;
                    }
                    else
                    {
                        players.Text = GameInfo.MaxPlayers.ToString();
                    }
                }

                FavoriteBox = new PictureBox
                {
                    Name = "favorite",
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                    Tag = "no_parent_click"
                };

                GameOptions = new PictureBox
                {
                    Name = "gameOptions",
                    BackColor = BackColor = Color.Transparent,
                    BackgroundImage = ImageCache.GetImage(themePath + "game_options.png"),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Tag = "no_parent_click"
                };

                HandlerUpdate = new PictureBox
                {
                    Name = "updateButton",
                    BackColor = Color.Transparent,
                    BackgroundImage = ImageCache.GetImage(themePath + "update.png"),
                    BackgroundImageLayout = ImageLayout.Stretch,
                    Visible = false,
                    Tag = "no_parent_click"
                };

                FavoriteBox.Image = favorite ? favorite_Selected : favorite_Unselected;
                FavoriteBox.Click += new EventHandler(FavoriteBox_Click);

                title.BackColor = Color.Transparent;

                TitleText = title.Text;

                PlayerText = players.Text;

                players.BackColor = Color.Transparent;
                playerIcon.BackColor = Color.Transparent;

                BackColor = Color.Transparent;

                Controls.Add(picture);
                Controls.Add(title);
                Controls.Add(players);
                Controls.Add(GameOptions);
                Controls.Add(HandlerUpdate);

                if (title.Text != "No games")
                {
                    Controls.Add(playerIcon);
                    Controls.Add(FavoriteBox);
                }

                MouseEnter += ZoomInPicture;
                MouseLeave += ZoomOutPicture;

                FavoriteBox.MouseEnter += ZoomInPicture;
                FavoriteBox.MouseLeave += ZoomOutPicture;

                GameOptions.MouseEnter += ZoomInPicture;
                GameOptions.MouseLeave += ZoomOutPicture;

                HandlerUpdate.MouseEnter += ZoomInPicture;
                HandlerUpdate.MouseLeave += ZoomOutPicture;

                this.Disposed += DisposeTimer;

                if (GameInfo != null)
                {
                    CustomToolTips.SetToolTip(FavoriteBox, "Add or remove this game from your favorites.", $"FavoriteBox_{GameInfo.GUID}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                    CustomToolTips.SetToolTip(GameOptions, "Game options menu.", $"GameOptions_{GameInfo.GUID}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                    CustomToolTips.SetToolTip(HandlerUpdate, "Update game handler.", $"HandlerUpdate{GameInfo.GUID}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                    CustomToolTips.SetToolTip(playerIcon, "Number of supported players.", $"playerIcon{GameInfo.GUID}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                   
                    //Set a different title color if a handler update is available using a timer(need to wait for the hub to return the value).
                    IsUpdateAvailableTimer = new System.Windows.Forms.Timer();
                    IsUpdateAvailableTimer.Interval = (1500); //millisecond                   
                    IsUpdateAvailableTimer.Tick += IsUpdateAvailable_Tick;
                    IsUpdateAvailableTimer.Start();
                }

                DPIManager.Register(this);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("ERROR - " + ex.Message + "\n\nSTACKTRACE: " + ex.StackTrace);
            }
        }
        ~GameControl()
        {
            DPIManager.Unregister(this);
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            SuspendLayout();

            int margin = 40;
            int border = 4;

            picture.Size = new Size((int)((margin - border) * scale), (int)((margin - border) * scale));
            picture.Location = new Point(border, border);

            title.Text = TitleText;
            players.Text = PlayerText;

            title.AutoSize = true;
            title.MaximumSize = new Size((int)(209 * scale) - picture.Width - (border * 2), 0);

            playerIcon.Size = new Size(players.Size.Height , players.Size.Height  );

            title.Location = new Point(picture.Right + border, picture.Location.Y);
            playerIcon.Location = new Point(title.Location.X + 2, title.Bottom + 3);
            players.Location = new Point(playerIcon.Right + 2, playerIcon.Bottom - players.Height);

            if (picture.Height < playerIcon.Bottom)//more than one title row
            {
                Height = playerIcon.Bottom + border;
                picture.Location = new Point(picture.Location.X, Height / 2 - picture.Height / 2);
            }
            else// one row
            {
                Height = picture.Bottom + border;//adjust the control Height
            }

            FavoriteBox.Size = new Size(playerIcon.Width - 1, playerIcon.Width - 1);

            float favoriteX = (209 * scale) - (playerIcon.Width + 5);
            float favoriteY = Height - (FavoriteBox.Height + 5);
            FavoriteBox.Location = new Point((int)favoriteX, (int)favoriteY);

            GameOptions.Size = FavoriteBox.Size;
            GameOptions.Location = new Point(FavoriteBox.Left - (GameOptions.Width + 5), FavoriteBox.Location.Y);

            HandlerUpdate.Size = FavoriteBox.Size; 
            HandlerUpdate.Location = new Point(GameOptions.Left - (HandlerUpdate.Width + 5), FavoriteBox.Location.Y);

            maxSize = new Size((FavoriteBox.Right - HandlerUpdate.Location.X) + 3, FavoriteBox.Height);
            minSize = new Size((FavoriteBox.Right - GameOptions.Location.X) + 3, FavoriteBox.Height);

            maxLoc = new Point(HandlerUpdate.Location.X,FavoriteBox.Top);
            minLoc = new Point(GameOptions.Location.X, FavoriteBox.Top);

            ResumeLayout();
        }

        private void DisposeTimer(object sender , EventArgs e)
        {
            IsUpdateAvailableTimer.Stop();
            IsUpdateAvailableTimer.Tick -= IsUpdateAvailable_Tick;
            IsUpdateAvailableTimer.Dispose();
        }

        private void IsUpdateAvailable_Tick(object Object, EventArgs eventArgs)
        {
            try
            {
                Control topLevel = TopLevelControl;

                if (topLevel == null)
                {
                    return;
                }

                if (!topLevel.IsHandleCreated)
                {
                    return;
                }

                this.Invoke((MethodInvoker)delegate ()
                {
                    updateAvailable = GameInfo.UpdateAvailable && GameInfo.MetaInfo.CheckUpdate;
                });
            }
            catch
            {
                IsUpdateAvailableTimer.Stop();
                IsUpdateAvailableTimer.Tick -= IsUpdateAvailable_Tick;
                IsUpdateAvailableTimer.Dispose();

                Console.WriteLine("GameControl was Disposed!");
            }
        }
        
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            Control c = e.Control;

            if ((string)c.Tag != "no_parent_click")
            {
                c.Click += C_Click;
            }
            
            c.MouseEnter += C_MouseEnter;
            c.MouseLeave += C_MouseLeave;

            DPIManager.Update(this);
        }

        private void FavoriteBox_Click(object sender, EventArgs e)
        {
            MouseEventArgs arg = e as MouseEventArgs;

            if (arg.Button == MouseButtons.Right)
            {
                return;
            }
            
            bool selected = FavoriteBox.Image.Equals(favorite_Selected);
            FavoriteBox.Image = selected ? favorite_Unselected : favorite_Selected;
            UserGameInfo.Game.MetaInfo.Favorite = !selected;

            GameManager.Instance.SaveUserProfile();
        }

        private void C_MouseEnter(object sender, EventArgs e)
        {
            Control c = sender as Control;

            if ((string)c.Tag != "no_parent_click")
            {
                OnMouseEnter(e);
            }
            else
            {
                c.Size = new Size(c.Width += 3, c.Height += 3);
                c.Location = new Point(c.Location.X - 1, c.Location.Y - 1);
            }
        }

        private void C_MouseLeave(object sender, EventArgs e)
        {
            Control c = sender as Control;

            if ((string)c.Tag != "no_parent_click")
            {
                OnMouseLeave(e);
            }
            else
            {
                c.Size = new Size(c.Width -= 3, c.Height -= 3);
                c.Location = new Point(c.Location.X + 1, c.Location.Y + 1);
            }
        }

        private void ZoomInPicture(object sender, EventArgs e)
        {       
            picture.Size = new Size(picture.Width += 3, picture.Height += 3);
            picture.Location = new Point(picture.Location.X - 1, picture.Location.Y - 1);
        }

        private void ZoomOutPicture(object sender, EventArgs e)
        {
            picture.Size = new Size(picture.Width -= 3, picture.Height -= 3);
            picture.Location = new Point(picture.Location.X + 1, picture.Location.Y + 1);
        }

        private void C_Click(object sender, EventArgs e)
        {          
            OnClick(e);            
        }

        public Image Image
        {
            get => picture.Image;
            set => picture.Image = value;
        }

        public override string ToString()
        {
            return Text;
        }

        private bool isSelected;

        public void RadioSelected()
        {
            isSelected = true && !GenericGameHandler.IsRunning;//lazy fix so it doesn't highlight if clicked if a handler is running
        }

        public void RadioUnselected()
        {
            isSelected = false;
        }

        public void UserOver()
        {
        }

        public void UserLeave()
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle gradientBrushbounds = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height / 3);

            if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
            {
                return;
            }

            Rectangle bounds = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height);

            Color color = isSelected ? radioSelectedBackColor : Color.Transparent;

            using (LinearGradientBrush lgb = new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color, 57f))
            {
                ColorBlend topcblend = new ColorBlend(3);
                topcblend.Colors = new Color[3] { color, color, Color.Transparent };
                topcblend.Positions = new float[3] { 0f, 0.5f, 1.0f };

                lgb.InterpolationColors = topcblend;
                lgb.SetBlendTriangularShape(.5f, 1.0f);
                e.Graphics.FillRectangle(lgb, bounds);
            }

            Size oulineRect = HandlerUpdate.Visible ? maxSize : minSize;

            int locX = HandlerUpdate.Visible ? maxLoc.X - 5 : minLoc.X - 5;
            int locY = maxLoc.Y - 3;
            int width = oulineRect.Width + 6;
            int height = oulineRect.Height + 6;

            Rectangle inputTextBack = new Rectangle(locX, locY, width, height);
            Rectangle inputTextBackOutline = new Rectangle(locX, locY, width, height);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(inputTextBack, 8, 8, true, false, false, true))
            {
                using (SolidBrush fillBrush = new SolidBrush(Color.FromArgb(50, 20, 20, 20)))
                {
                    e.Graphics.FillPath(fillBrush, backGp);
                }
            }

            using (GraphicsPath outlineGp = FormGraphicsUtil.MakeRoundedRect(inputTextBackOutline, 8, 8, true, false, false, true))
            {
                using (Pen outlinePen = new Pen(Color.FromArgb(15, 255, 255, 255)))
                {
                    e.Graphics.DrawPath(outlinePen, outlineGp);
                }
            }
        }
    }
}