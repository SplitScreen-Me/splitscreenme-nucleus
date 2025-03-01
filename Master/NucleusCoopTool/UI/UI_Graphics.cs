using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class UI_Graphics
    {
       
        private static RectangleF backImgRect;

        public static bool Refreshing;
        private static bool enableParticles = bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "EnableParticles"));
        public static bool RainbowTimerRunning = false;

        public static System.Windows.Forms.Timer RainbowTimer;

        private static Color backGradient = Theme_Settings.BackgroundGradientColor;

        public static Color GameBorderGradientTop;
        public static Color GameBorderGradientBottom;

        public static Bitmap DefaultBackground = new Bitmap(ImageCache.GetImage(Globals.ThemeFolder + "background.jpg"), new Size(1280, 720));

        private static Bitmap backgroundImg = new Bitmap(ImageCache.GetImage(Globals.ThemeFolder + "background.jpg"), new Size(1280, 720));
        public static Bitmap BackgroundImg
        {
            get => backgroundImg;
            set
            {
                backgroundImg = value;
            }
        }

        private static SolidBrush borderBrush;
        private static SolidBrush inputTextFillBrush;

        private static LinearGradientBrush homeScreenBrush;
        private static LinearGradientBrush gameListContainerBrush;
        private static LinearGradientBrush infoPanelBrush;

        private static Pen inputTextOutlinePen;
     
        private static DrawParticles playParticles;
        private static DrawParticles bigLogoParticles;
       
        public static void HomeScreen_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel homeScreen = (DoubleBufferPanel)sender;

            if (Refreshing || backImgRect == null)
            {
                RectangleF client = homeScreen.ClientRectangle;
                RectangleF img = new RectangleF(0, 0, BackgroundImg.Width, BackgroundImg.Height);

                float ratio = 1.78f;

                backImgRect = new RectangleF(0, 0, client.Width, client.Width / ratio);

                if (client.Width / client.Height < ratio)
                {
                    float multH = (float)client.Width / (float)backImgRect.Width;
                    backImgRect.Height = client.Height * multH;
                    backImgRect.Width = (backImgRect.Height * ratio) * multH;
                }

                PointF loc = RectangleUtil.Center(backImgRect.Size, client);
                backImgRect.Location = loc;
            }

            e.Graphics.DrawImage(BackgroundImg, backImgRect);

            if (backGradient.A == 0)
            {
                return;
            }

            if (homeScreenBrush == null || Refreshing)
            {
                Rectangle gradientBrushbounds = new Rectangle(0, 0, homeScreen.ClientRectangle.Width, homeScreen.ClientRectangle.Height);

                if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
                {
                    return;
                }

                Color color1 = Color.FromArgb(245, backGradient.R, backGradient.G, backGradient.B);
                Color color2 = Color.FromArgb(210, backGradient.R, backGradient.G, backGradient.B);
                Color color3 = Color.FromArgb(170, backGradient.R, backGradient.G, backGradient.B);
                Color color4 = Color.FromArgb(160, backGradient.R, backGradient.G, backGradient.B);
                Color color5 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                Color color6 = Color.FromArgb(160, backGradient.R, backGradient.G, backGradient.B);
                Color color7 = Color.FromArgb(170, backGradient.R, backGradient.G, backGradient.B);
                Color color8 = Color.FromArgb(210, backGradient.R, backGradient.G, backGradient.B);
                Color color9 = Color.FromArgb(245, backGradient.R, backGradient.G, backGradient.B);

                homeScreenBrush =
                    new LinearGradientBrush(gradientBrushbounds, color1, color5, 90f);

                ColorBlend topcblend = new ColorBlend(9);
                topcblend.Colors = new Color[9] { color1, color2, color3, color4, color5, color6, color7, color8, color9 };
                topcblend.Positions = new float[9] { 0f, 0.125f, 0.250f, 0.375f, 0.500f, 0.625f, 0.750f, 0.875f, 1.0f };

                homeScreenBrush.InterpolationColors = topcblend;
            }

            e.Graphics.FillRectangle(homeScreenBrush, homeScreen.ClientRectangle);

            Refreshing = false;
        }

        public static void MainFormPaintBackground(PaintEventArgs e)
        {
            //add sender and unpack it instead of calling the UI_Interface object
            int edgingHeight = UI_Interface.HomeScreen.Location.Y - 3;
            Rectangle topGradient = new Rectangle(0, 0, UI_Interface.MainForm.Width, edgingHeight);
            Rectangle bottomGradient = new Rectangle(0, (UI_Interface.MainForm.Height - edgingHeight) - 1, UI_Interface.MainForm.Width, edgingHeight);

            Color edgingColorTop;
            Color edgingColorBottom;

            if (!UI_Interface.WindowPanel.Enabled)
            {
                edgingColorTop = Color.Red;
                edgingColorBottom = Color.Red;
            }
            else if (UI_Interface.WebView != null)
            {
                edgingColorTop = Color.FromArgb(255, 0, 98, 190);
                edgingColorBottom = Color.FromArgb(255, 0, 98, 190);
            }
            else if (UI_Interface.SetupPanel.Visible)
            {
                edgingColorTop = GameBorderGradientTop;
                edgingColorBottom = GameBorderGradientBottom;
            }
            else
            {
                edgingColorTop = Theme_Settings.DefaultBorderGradientColor;
                edgingColorBottom = Theme_Settings.DefaultBorderGradientColor;
            }

            LinearGradientBrush topLinearGradientBrush = new LinearGradientBrush(topGradient, Color.Transparent, edgingColorTop, 0F);
            LinearGradientBrush bottomLinearGradientBrush = new LinearGradientBrush(bottomGradient, Color.Transparent, edgingColorBottom, 0F);

            ColorBlend topcblend = new ColorBlend(3);
            topcblend.Colors = new Color[3] { Color.Transparent, edgingColorTop, Color.Transparent };
            topcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            topLinearGradientBrush.InterpolationColors = topcblend;

            ColorBlend bottomcblend = new ColorBlend(3);
            bottomcblend.Colors = new Color[3] { Color.Transparent, edgingColorBottom, Color.Transparent };
            bottomcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            bottomLinearGradientBrush.InterpolationColors = bottomcblend;

            Rectangle fill = new Rectangle(0, 0, UI_Interface.MainForm.Width, UI_Interface.MainForm.Height);

            if(borderBrush == null)
            {
                borderBrush = new SolidBrush(Theme_Settings.MainWindowBackColor);
            }

            e.Graphics.FillRectangle(borderBrush, fill);
            e.Graphics.FillRectangle(topLinearGradientBrush, topGradient);
            e.Graphics.FillRectangle(bottomLinearGradientBrush, bottomGradient);

            topLinearGradientBrush.Dispose();
            bottomLinearGradientBrush.Dispose();
        }

        public static void WindowPanel_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel WindowPanel = (DoubleBufferPanel)sender;

            UI_Interface.InputsTextLabel.Location = new Point(WindowPanel.Width / 2 - UI_Interface.InputsTextLabel.Width / 2, (WindowPanel.Bottom - UI_Interface.InputsTextLabel.Height) - 5);

            if (inputTextOutlinePen == null)
            {
                inputTextOutlinePen = new Pen(Color.FromArgb(180, 100, 100, 100));
                inputTextFillBrush = new SolidBrush(Color.FromArgb(20, 20, 20, 20));
            }

            if (UI_Interface.InputsTextLabel.Text != "")
            {
                Rectangle inputTextBack = new Rectangle(UI_Interface.InputsTextLabel.Location.X - 5, UI_Interface.InputsTextLabel.Location.Y - 3, UI_Interface.InputsTextLabel.Width + 10, UI_Interface.InputsTextLabel.Height + 6);
                Rectangle inputTextBackOutline = new Rectangle(UI_Interface.InputsTextLabel.Location.X - 6, UI_Interface.InputsTextLabel.Location.Y - 4, UI_Interface.InputsTextLabel.Width + 12, UI_Interface.InputsTextLabel.Height + 8);

                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                GraphicsPath outlineGp = FormGraphicsUtil.MakeRoundedRect(inputTextBackOutline, 10, 10, true, true, true, true);
                GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(inputTextBack, 10, 10, false, false, false, true);

                e.Graphics.DrawPath(inputTextOutlinePen, outlineGp);
                e.Graphics.FillPath(inputTextFillBrush, backGp);

                outlineGp.Dispose();
                backGp.Dispose();
            }
        }

        public static void SetupPanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameProfile.Instance == null)
            {
                UI_Interface.InputsTextLabel.Text = "";
                return;
            }

            var prevText = UI_Interface.InputsTextLabel.Text;
            var inputText = Core_Interface.CurrentStepIndex == 0 ? InputsText.GetInputText(Core_Interface.DisableGameProfiles) : ("", UI_Interface.InputsTextLabel.ForeColor);

            if (prevText != inputText.Item1)
            {
                UI_Interface.InputsTextLabel.Text = inputText.Item1;
                UI_Interface.InputsTextLabel.ForeColor = inputText.Item2;
                UI_Interface.WindowPanel.Invalidate(false);
            }
        }

        public static void GameListContainer_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel gameListContainer = (DoubleBufferPanel)sender;

            if (UI_Interface.HubButton != null && UI_Interface.SearchTextBox != null)
            {
                if(UI_Interface.SearchTextBox.Visible)
                {
                    Rectangle sortOptBack = new Rectangle(UI_Interface.SortGamesButton.Left - 10, UI_Interface.SearchTextBox.Top,
                                                     (gameListContainer.Right - UI_Interface.SortGamesButton.Left) + 10, UI_Interface.SearchTextBox.Height);

                    Color backCol = UI_Interface.SearchTextBox.BackColor;

                    SolidBrush sortBrush = new SolidBrush(Color.FromArgb(255, backCol.R, backCol.G, backCol.B));

                    GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(sortOptBack, 10, 10, false, true, true, false);

                    e.Graphics.FillPath(sortBrush, backGp);
                    //e.Graphics.FillRectangle(sortBrush, sortOptBack);
                    sortBrush.Dispose();
                }
                
            }

            if (backGradient.A == 0)
            {
                return;
            }

            if (gameListContainerBrush == null || Refreshing)
            {
                Rectangle gradientBrushbounds = new Rectangle(0, 0, gameListContainer.Width / 2, gameListContainer.Height);

                if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
                {
                    return;
                }

                Color color1 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);
                Color color2 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                Color color3 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                Color color4 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);

                gameListContainerBrush =
                new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color1, 90f);

                ColorBlend topcblend = new ColorBlend(6);
                topcblend.Colors = new Color[6] { Color.Transparent, color1, color2, color3, color4, Color.Transparent };
                topcblend.Positions = new float[6] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };
  
                gameListContainerBrush.InterpolationColors = topcblend;
            }

            e.Graphics.FillRectangle(gameListContainerBrush, gameListContainer.ClientRectangle);
        }

        public static void InfoPanel_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel infoPanel = (DoubleBufferPanel)sender;

            if (backGradient.A == 0)
            {
                return;
            }

            if (infoPanelBrush == null || Refreshing)
            {
                Rectangle gradientBrushbounds = new Rectangle(0, 0, infoPanel.Width / 2, infoPanel.Height);

                if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
                {
                    return;
                }

                Color color1 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);
                Color color2 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                Color color3 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                Color color4 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);

                infoPanelBrush =
                new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color1, 90f);

                ColorBlend topcblend = new ColorBlend(6);
                topcblend.Colors = new Color[6] { Color.Transparent, color1, color2, color3, color4, Color.Transparent };
                topcblend.Positions = new float[6] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

                infoPanelBrush.InterpolationColors = topcblend;
            }

            e.Graphics.FillRectangle(infoPanelBrush, infoPanel.ClientRectangle);
        }
        
        public static void BigLogo_Paint(object sender, PaintEventArgs e)
        {
            PictureBox bigLogo = (PictureBox)sender;
            if (enableParticles)
            {
                if (bigLogoParticles == null)
                {
                    bigLogoParticles = new DrawParticles();
                }

                if (bigLogo.Visible)
                    bigLogoParticles.Draw(sender, sender as Control, e, 8, 100, new int[] { 255, 255, 255 });
            }
        }

        public static void CoverFrame_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel coverFrame = (DoubleBufferPanel)sender;

            if (enableParticles)
            {
                if (playParticles == null)
                {
                    playParticles = new DrawParticles();
                }

                if (UI_Interface.PlayButton.Visible)
                {
                    playParticles.Draw(sender, UI_Interface.PlayButton, e, 4, 140, new int[] { 100, 255, 100 });
                }
            }

            coverFrame.BackColor = UI_Interface.PlayButton.Visible ? Color.FromArgb(100, 0, 0, 0) : Color.Transparent;
        }

        public static void SetupScreen_Paint(object sender, PaintEventArgs e)
        {
            if (DevicesFunctions.isDisconnected)
            {
                DPIManager.ForceUpdate();
                DevicesFunctions.isDisconnected = false;
            }
        }

        private static int blurValue = App_Misc.Blur;

        public static Bitmap ApplyBlur(Bitmap screenshot)
        {
            if (screenshot == null)
            {
                return DefaultBackground;
            }

            GaussianBlur blur = new GaussianBlur(screenshot);

            GameBorderGradientTop = blur.topColor;
            GameBorderGradientBottom = blur.bottomColor;

            screenshot.Dispose();

            return blur.Process(blurValue);
        }

        private static int r = 0;
        private static int b = 0;
        private static bool loop = false;

        public static void RainbowTimerTick(object Object, EventArgs eventArgs)
        {
            string text = UI_Interface.HandlerNoteTitle.Text;

            if (text == "Handler Notes" || text == "Read First")
            {
                if (!loop)
                {
                    UI_Interface.HandlerNoteTitle.Text = "Handler Notes";
                    if (r < 200 && b < 200) { r += 3; b += 3; };
                    if (b >= 200 && r >= 200)
                        loop = true;

                    UI_Interface.HandlerNoteTitle.Font = new Font(UI_Interface.HandlerNoteTitle.Font.FontFamily, UI_Interface.HandlerNoteTitle.Font.Size, FontStyle.Bold);
                }
                else
                {
                    UI_Interface.HandlerNoteTitle.Text = "Read First";
                    if (r > 0 && b > 0) { r -= 3; b -= 3; }
                    if (b <= 0 && r <= 0)
                        loop = false;
                }

                UI_Interface.HandlerNoteTitle.ForeColor = Color.FromArgb(r, r, 255, b);
            }
        }
    }
}
