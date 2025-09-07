using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
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

            if (borderBrush == null)
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
        }

        public static void SetupPanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameProfile.Instance == null)
            {
                UI_Interface.InputsTextLabel.SetText(new (string, Color)[]{});
                return;
            }

            var inputText = Core_Interface.CurrentStepIndex == 0 ? InputsText.GetInputText() : new (string, Color)[] {( Core_Interface.CurrentStep?.Title, Theme_Settings.ControlsForeColor)};
            
           if (!UI_Interface.InputsTextLabel.Content.SequenceEqual(inputText))
           {
                UI_Interface.InputsTextLabel.SetText(inputText);
           }
        }

        public static void GameListContainer_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel gameListContainer = (DoubleBufferPanel)sender;

            if (UI_Interface.HubButton != null && UI_Interface.SearchTextBox != null)
            {
                if (UI_Interface.SearchTextBox.Visible)
                {
                    Rectangle sortOptBack = new Rectangle(UI_Interface.SortGamesButton.Left - 10, UI_Interface.SearchTextBox.Top,
                                                     (gameListContainer.Right - UI_Interface.SortGamesButton.Left) + 10, UI_Interface.SearchTextBox.Height);

                    Color backCol = UI_Interface.SearchTextBox.BackColor;
                    SolidBrush sortBrush = new SolidBrush(Color.FromArgb(255, backCol.R, backCol.G, backCol.B));
                    e.Graphics.FillRectangle(sortBrush, sortOptBack);
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

        public static void VirtualMouseToggle_Paint(object sender, PaintEventArgs e)
        {
            PictureBox navPb = UI_Interface.ToggleVirtualMouse;

            string text;
            ImageAttributes gpImageAttributes = new ImageAttributes();

            Rectangle bounds = navPb.ClientRectangle;
            ColorMatrix colorMatrix;

            if (App_GamePadNavigation.Enabled)
            {
                if (GamepadNavigation.ReceiveInputs.Any(b => b == true))
                {
                    colorMatrix = new ColorMatrix(new[]//orange
                    {
                       new float[] {1, 0, 0, 0, 0},  
                       new float[] {0, 1, 0, 0, 0},  
                       new float[] {0, 0, 0, 0, 0},  
                       new float[] {0, 0, 0, 1, 0},  
                       new float[] {1f, 1f, 0, 0, 1} 
                     });
                }
                else
                {
                    colorMatrix = new ColorMatrix(new[]//green
                    {
                     new float[] {0, 0, 0, 0, 0},  
                     new float[] {0, 1, 0, 0, 0},  
                     new float[] {0, 0, 0, 0, 0},  
                     new float[] {0, 0, 0, 1, 0},  
                     new float[] {0, 0.6f, 0, 0, 1} 
                   });
                }
                gpImageAttributes = new ImageAttributes();
                gpImageAttributes.SetColorMatrix(colorMatrix);

                text = "ON";
            }
            else
            {
                colorMatrix = new ColorMatrix(new[]
                {
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0}, 
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0},  
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0}, 
                   new float[] {0, 0, 0, 1, 0},          
                   new float[] {0.4f, 0.4f, 0.4f, 0, 1}
                 });

                text = "OFF";

                gpImageAttributes = new ImageAttributes();
                gpImageAttributes.SetColorMatrix(colorMatrix);
            }

            Image gpImg = ImageCache.GetImage(Globals.ThemeFolder + "xinput.png");

            e.Graphics.DrawImage(gpImg, bounds, 0, 0, gpImg.Width, gpImg.Height, GraphicsUnit.Pixel, gpImageAttributes);

            Font font = new Font(UI_Interface.MainForm.Font.FontFamily, 7.0f, FontStyle.Bold);

            SizeF tagSize = e.Graphics.MeasureString(text, font);
            Point tagLocation = new Point(((int)bounds.Width / 2) - ((int)tagSize.Width / 2) - 2, ((int)bounds.Height / 2) - ((int)tagSize.Height / 2) - 3);

            e.Graphics.DrawString(text, font, Brushes.White, tagLocation);

            gpImageAttributes.Dispose();

            font.Dispose();
        }

        private static System.Threading.Timer blink;
        private static bool blinking;

        private static void Blink_Tick(object state)
        {
            Button stepBtn = (Button)state;

            if(!stepBtn.Enabled)
            {
                blink?.Dispose();
                blink = null;
                return;
            }
            
            blinking = !blinking;
            stepBtn.Invalidate();
        }

        public static void GotoPrevPaint(object sender, PaintEventArgs e)
        {
            Button stepBtn = (Button)sender;

            Rectangle bounds = new Rectangle(stepBtn.ClientRectangle.X, stepBtn.ClientRectangle.Y, stepBtn.ClientRectangle.Width -1, stepBtn.ClientRectangle.Height -1);

            stepBtn.Location = new Point((UI_Interface.Cover.Left - bounds.Width) -1, UI_Interface.Cover.Top);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Color backCol = Theme_Settings.InfoPanelBackColor;

            SolidBrush textBrush = new SolidBrush(Color.FromArgb(255,Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B));
            SolidBrush sortBrush = new SolidBrush(Color.FromArgb(90, backCol.R, backCol.G, backCol.B));
            GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(bounds, 20, 20, true, false, false, true);
            Pen pathPen = new Pen(Color.FromArgb(210, 50, 50, 50));

            StringBuilder sb = new StringBuilder();

            string text = (string)stepBtn.Tag;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (i == 0) { sb.Append(c); } else { sb.Append('\n');  sb.Append(c);}
            }

            SizeF textSize = e.Graphics.MeasureString(sb.ToString(), stepBtn.Font);
            Point ghostTagLocation = new Point(((int)bounds.Width / 2) - ((int)textSize.Width / 2), ((int)bounds.Height / 2) - ((int)textSize.Height / 2));

            if (stepBtn.Enabled)
            {
                e.Graphics.FillPath(sortBrush, backGp);
                e.Graphics.DrawPath(pathPen, backGp);
                e.Graphics.DrawString(sb.ToString(), stepBtn.Font, textBrush, ghostTagLocation.X, ghostTagLocation.Y);
            }

            sortBrush.Dispose();
            textBrush.Dispose();
            pathPen.Dispose();
            backGp.Dispose();
        }

        public static void GotoNextPaint(object sender, PaintEventArgs e)
        {
            Button stepBtn = (Button)sender;

            if(blink == null && stepBtn.Enabled)
            {
                blink = new System.Threading.Timer(Blink_Tick, sender, 400,400);
            }

            Rectangle bounds = new Rectangle(stepBtn.ClientRectangle.X, stepBtn.ClientRectangle.Y, stepBtn.ClientRectangle.Width - 1, stepBtn.ClientRectangle.Height - 1 );
            stepBtn.Location = new Point(UI_Interface.Cover.Right, UI_Interface.Cover.Top);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Color backCol = Theme_Settings.InfoPanelBackColor;

            SolidBrush  textBrush = blinking ? new SolidBrush(Color.White) : new SolidBrush(Color.FromArgb(255, Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B));
 
            SolidBrush sortBrush = new SolidBrush(Color.FromArgb(90, backCol.R, backCol.G, backCol.B));
            GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(bounds, 20, 20, false, true, true, false);
            Pen pathPen = new Pen(Color.FromArgb(210, 50, 50, 50));
            
            StringBuilder sb = new StringBuilder();

            string text = (string)stepBtn.Tag;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (i == 0) { sb.Append(c); } else { sb.Append('\n'); sb.Append(c); }
            }
  
            SizeF textSize = e.Graphics.MeasureString(sb.ToString(), stepBtn.Font);
            Point textLocation = new Point(((int)bounds.Width / 2) - ((int)textSize.Width / 2), ((int)bounds.Height / 2) - ((int)textSize.Height / 2));

            if (stepBtn.Enabled)
            {
                e.Graphics.FillPath(sortBrush, backGp);
                e.Graphics.DrawPath(pathPen, backGp);
                e.Graphics.DrawString(sb.ToString(), stepBtn.Font, textBrush, textLocation.X, textLocation.Y);
            }
 
            sortBrush.Dispose();
            textBrush.Dispose();
            pathPen.Dispose();
            backGp.Dispose();
        }

        private static int r = 0;
        private static int b = 0;
        private static bool loop = false;

        public static void RainbowTimerTick(object state, EventArgs eventArgs)
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

        public static Timer BlinkInputTimer => blinkInputTimer;
        private static Timer blinkInputTimer;
        private static int blinkInputStep = 0;
        private static bool blinkInputShowTooltip;
        private static ImageAttributes blinkImageAttributes;

        public static void StartBlinkInputIcons()
        {   
            if (blinkInputTimer == null)
            {
               if(blinkImageAttributes == null)
               {
                    ColorMatrix colorMatrix = new ColorMatrix(new[]{
                    new float[] {1, 0, 0, 0, 0},  
                    new float[] {0, 0, 0, 0, 0},  
                    new float[] {0, 0, 0, 0, 0},  
                    new float[] {0, 0, 0, 1, 0},  
                    new float[] {0, 0, 0, 0, 1} 
                    });

                    blinkImageAttributes = new ImageAttributes();
                    blinkImageAttributes.SetColorMatrix(colorMatrix);
                }                   
             
                blinkInputTimer = new Timer();
                blinkInputTimer.Interval = 500; // Blink every 500 milliseconds
                blinkInputTimer.Tick += blinkInputTimer_Tick;
                blinkInputTimer.Start();

                UI_Actions.On_GameChange -= StopBlinkInputIcons;
                UI_Actions.On_GameChange += StopBlinkInputIcons;
            }   
        }

        private static void blinkInputTimer_Tick(object obj, EventArgs e)
        {
            var ic = UI_Interface.IconsContainer;

            if (ic != null)
            {
                bool combineToolTipsText = ic.Controls.Count >= 2;
                StringBuilder sb = new StringBuilder();

                foreach (PictureBox pic in ic.Controls)
                {                   
                    if (blinkInputStep == 1)
                    {
                        pic.Invalidate();
                    }
                    else
                    {
                        var g = pic.CreateGraphics();
                        g.DrawImage(pic.Image, pic.ClientRectangle, 0, 0, pic.Image.Width, pic.Image.Height, GraphicsUnit.Pixel, blinkImageAttributes);
                        g.Dispose();

                        var tooltip = CustomToolTips.GetControlToolTip(pic.Name);

                        if (!combineToolTipsText)
                        {
                            if (tooltip != null && !blinkInputShowTooltip)
                            {
                                var text = tooltip.GetToolTip(pic);

                                if (text != "")
                                {
                                    tooltip.Show("Supported inputs:" + "\n" + text.Replace("Supports ", "-") /*+ "\n" + " "*/, pic, new Point(pic.Parent.ClientRectangle.Left,pic.Parent.ClientRectangle.Bottom),5000);                        
                                }
                            }
                        }
                        else
                        {
                            var text = tooltip.GetToolTip(pic);
                            if (text != "")
                            {
                                sb.Append(text.Replace("Supports ","-") + "\n");
                            }              
                        }
                    }            
                }

                if (sb.ToString() != "" && !blinkInputShowTooltip)
                {
                    var firstPb = ic.Controls[0];
                    var tooltip = CustomToolTips.GetControlToolTip(firstPb.Name);

                    if (tooltip != null && !blinkInputShowTooltip)
                    {
                        tooltip.Show("Supported inputs:" + "\n" + sb.ToString() /*+ "\n" + " "*/, firstPb, firstPb.Parent.ClientRectangle.Left, firstPb.Parent.ClientRectangle.Bottom, 5000);
                    }
                }

                blinkInputStep = blinkInputStep == 0 ? 1 : 0;
                blinkInputShowTooltip = true;
            }
        }

        public static void StopBlinkInputIcons()
        {
            blinkInputTimer?.Stop();
            blinkInputTimer?.Dispose();
            blinkInputTimer = null;
            blinkInputStep = 0;
            blinkInputShowTooltip = false;

            var ic = UI_Interface.IconsContainer;

            if (ic != null)
            {
                foreach (PictureBox pic in ic.Controls)
                {
                    pic.Invalidate();
                }
            }
        }
    }
}
