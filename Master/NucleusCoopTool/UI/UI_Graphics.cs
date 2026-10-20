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
        public static bool IsNotesTitleScrollTimerRunning = false;

        public static System.Windows.Forms.Timer NotesTitleScrollTimer;

        public static Bitmap DefaultBackground =          
           Theme_Settings.UseBackgroundImageEffect ?  ApplyBlur (new Bitmap(ImageCache.GetImage(Globals.ThemeFolder + "background.jpg"), new Size(1280, 720))) :
            ImageCache.GetImage(Globals.ThemeFolder + "background.jpg");

        private static Bitmap backgroundImg = DefaultBackground;
        public static Bitmap BackgroundImg
        {
            get => backgroundImg;
            set
            {
                backgroundImg = value;
            }
        }

        private static LinearGradientBrush homeScreenBrush;
        private static SolidBrush topBrush;

        public static void HomeScreen_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel homeScreen = (DoubleBufferPanel)sender;

            if (Theme_Settings.UseBackgroundImage)
            {
                if (Refreshing || backImgRect == null)
                {
                    RectangleF client = new RectangleF(homeScreen.ClientRectangle.X, UI_Interface.WindowPanel.Bottom, homeScreen.Width, homeScreen.Height - UI_Interface.WindowPanel.Bottom);
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
            }
            
            if (Theme_Settings.UseBackgroundImageEffect)
            {
                Rectangle gradientBrushbounds = new Rectangle(0, 0, homeScreen.ClientRectangle.Width, homeScreen.ClientRectangle.Height);

                if (homeScreenBrush == null || Refreshing)
                {
                    if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
                    {
                        return;
                    }
       
                    Color color1 = Color.FromArgb(255, backGradient.R, backGradient.G, backGradient.B);
                    Color color2 = Color.FromArgb(220, backGradient.R, backGradient.G, backGradient.B);
                    Color color3 = Color.FromArgb(180, backGradient.R, backGradient.G, backGradient.B);
                    Color color4 = Color.FromArgb(160, backGradient.R, backGradient.G, backGradient.B);
                    Color color5 = Color.FromArgb(120, backGradient.R, backGradient.G, backGradient.B);
                    Color color6 = Color.FromArgb(160, backGradient.R, backGradient.G, backGradient.B);
                    Color color7 = Color.FromArgb(180, backGradient.R, backGradient.G, backGradient.B);
                    Color color8 = Color.FromArgb(220, backGradient.R, backGradient.G, backGradient.B);
                    Color color9 = Color.FromArgb(255, backGradient.R, backGradient.G, backGradient.B);

                    homeScreenBrush = new LinearGradientBrush(gradientBrushbounds, color1, color5, 90f);

                    ColorBlend topcblend = new ColorBlend(9);
                    topcblend.Colors = new Color[9] { color1, color2, color3, color4, color5, color6, color7, color8, color9 };
                    topcblend.Positions = new float[9] { 0f, 0.125f, 0.250f, 0.375f, 0.500f, 0.625f, 0.750f, 0.875f, 1.0f };

                    homeScreenBrush.InterpolationColors = topcblend;
                }
                
                e.Graphics.FillRectangle(homeScreenBrush, gradientBrushbounds);
            }

            Rectangle topBanner = new Rectangle(UI_Interface.Logo.Left - 4, UI_Interface.WindowPanel.Top,
                homeScreen.Width, UI_Interface.WindowPanel.Height);

            if (topBrush == null)
            {
                topBrush = new SolidBrush(Theme_Settings.WindowPanelBackColor);
            }

            e.Graphics.FillRectangle(topBrush, topBanner);

            Refreshing = false;
        }

        private static SolidBrush borderBrush;
        public static Color GameBorderGradientTop;
        public static Color GameBorderGradientBottom;

        public static void MainFormPaintBackground(PaintEventArgs e)
        {
            Rectangle fill = new Rectangle(0, 0, UI_Interface.MainForm.Width, UI_Interface.MainForm.Height);

            if (borderBrush == null)
            {
                borderBrush = new SolidBrush(Theme_Settings.MainWindowBackColor);
            }

            e.Graphics.FillRectangle(borderBrush, fill);
        }

        public static void WindowPanel_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel windowPanel = (DoubleBufferPanel)sender;
            //UI_Interface.InputsTextLabel.Location = new Point((windowPanel.Width - UI_Interface.InfoPanel.Width) / 2 - UI_Interface.InputsTextLabel.Width / 2, (windowPanel.Bottom - UI_Interface.InputsTextLabel.Height) - 5);
        }

        public static void SetupPanel_Paint(object sender, PaintEventArgs e)
        {
            if (GameProfile.Instance == null)
            {
                UI_Interface.InputsTextLabel.SetText(new (string, Color,Font)[]{});
                return;
            }

            var inputText = Core_Interface.CurrentStepIndex == 0 ? InputsText.GetInputText() : new (string, Color,Font)[] {( Core_Interface.CurrentStep?.Title, Theme_Settings.ControlsForeColor, UI_Interface.InputsTextLabel.Container.Font) };
            UI_Interface.InputsTextLabel.Location = new Point(UI_Interface.SetupPanel.Width / 2 - UI_Interface.InputsTextLabel.Width / 2, 2/*(UI_Interface.WindowPanel.Bottom - UI_Interface.InputsTextLabel.Height) - 3*/);
            if (!UI_Interface.InputsTextLabel.Content.SequenceEqual(inputText))
            {
                UI_Interface.InputsTextLabel.SetText(inputText);
                
            }
        }

        private static Color backGradient = Theme_Settings.BackgroundGradientColor;
        private static SolidBrush sortBackBrush;
        private static LinearGradientBrush gameListContainerBrush;

        public static void GameListContainer_Paint(object sender, PaintEventArgs e)
        {
            DoubleBufferPanel gameListContainer = (DoubleBufferPanel)sender;

            //if (UI_Interface.HubButton != null && UI_Interface.SearchTextBox != null)
            //{
            //    Rectangle sortOptBack = new Rectangle(UI_Interface.SortGamesButton.Left - 20, UI_Interface.SearchTextBox.Top,
            //                                   /*  (*/gameListContainer.Right /*- UI_Interface.SortGamesButton.Left) + 10*/, UI_Interface.SearchTextBox.ClientRectangle.Height);

            //    Color backCol = UI_Interface.SearchTextBox.BackColor;
            //    //if (sortBackBrush == null)
            //    {
            //        sortBackBrush = new SolidBrush(/*Color.FromArgb(255, backCol.R, backCol.G, backCol.B)*/Color.FromArgb(255,54, 57, 58));
            //    }

            //    e.Graphics.FillRectangle(sortBackBrush, sortOptBack);
            //}

            if (backGradient.A == 0)
            {
                return;
            }

            if (gameListContainerBrush == null || Refreshing)
            {
                //Rectangle gradientBrushbounds = new Rectangle(0, 0, gameListContainer.Width / 2, gameListContainer.Height);

                //if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
                //{
                //    return;
                //}

                //Color color1 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);
                //Color color2 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                //Color color3 = Color.FromArgb(150, backGradient.R, backGradient.G, backGradient.B);
                //Color color4 = Color.FromArgb(90, backGradient.R, backGradient.G, backGradient.B);

                //gameListContainerBrush =
                //new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color1, 90f);

                //ColorBlend topcblend = new ColorBlend(6);
                //topcblend.Colors = new Color[6] { Color.Transparent, color1, color2, color3, color4, Color.Transparent };
                //topcblend.Positions = new float[6] { 0f, 0.2f, 0.4f, 0.6f, 0.8f, 1.0f };

                //gameListContainerBrush.InterpolationColors = topcblend;
            }

            //e.Graphics.FillRectangle(gameListContainerBrush, gameListContainer.ClientRectangle);

            //e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(255, 31, 34, 35)), gameListContainer.ClientRectangle);
        }

        private static LinearGradientBrush infoPanelBrush;

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
        }

        private static DrawParticles bigLogoParticles;

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
                    bigLogoParticles.Draw(sender, true, sender as Control, e, 8, 100, new int[] {255, 255, 255, 255 });
            }
        }

        private static DrawParticles playParticles;

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
                    playParticles.Draw(sender, true, UI_Interface.PlayButton, e, 4, 140, new int[] {255, 100, 255, 100 });
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

        public static Bitmap ApplyBlur(Bitmap screenshot)
        {
            if (screenshot == null)
            {
                return DefaultBackground;
            }

            GaussianBlur blur = new GaussianBlur(screenshot);

            screenshot.Dispose();

            return blur.Process(App_Misc.Blur);
        }

        private static Font virtualMouseToggle_Font;
        private static ImageAttributes virtualMouseToggle_ImageAttributes;
        private static Image virtualMouseToggle_Image;
        private static ColorMatrix virtualMouseToggle_ColorMatrix;

        public static void VirtualMouseToggle_Paint(object sender, PaintEventArgs e)
        {
            PictureBox navPb = UI_Interface.ToggleVirtualMouse;

            string text;

            if(virtualMouseToggle_ImageAttributes == null)
            {
                virtualMouseToggle_ImageAttributes = new ImageAttributes();
            }

            Rectangle bounds = navPb.ClientRectangle;

            if (App_GamePadNavigation.Enabled)
            {
                if (GamepadNavigation.ReceiveInputs.Any(b => b == true))
                {
                    virtualMouseToggle_ColorMatrix = new ColorMatrix(new[]//orange
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
                    virtualMouseToggle_ColorMatrix = new ColorMatrix(new[]//green
                    {
                     new float[] {0, 0, 0, 0, 0},  
                     new float[] {0, 1, 0, 0, 0},  
                     new float[] {0, 0, 0, 0, 0},  
                     new float[] {0, 0, 0, 1, 0},  
                     new float[] {0, 0.6f, 0, 0, 1} 
                   });
                }

                virtualMouseToggle_ImageAttributes.SetColorMatrix(virtualMouseToggle_ColorMatrix);
                text = "ON";
            }
            else
            {
                virtualMouseToggle_ColorMatrix = new ColorMatrix(new[]
                {
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0}, 
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0},  
                   new float[] {0.8f, 0.8f, 0.8f, 0, 0}, 
                   new float[] {0, 0, 0, 1, 0},          
                   new float[] {0.4f, 0.4f, 0.4f, 0, 1}
                 });

                text = "OFF";
                virtualMouseToggle_ImageAttributes.SetColorMatrix(virtualMouseToggle_ColorMatrix);
            }

            if(virtualMouseToggle_Image == null)
            {
                virtualMouseToggle_Image = ImageCache.GetImage(Globals.ThemeFolder + "xinput.png");
            }

            e.Graphics.DrawImage(virtualMouseToggle_Image, bounds, 0, 0, virtualMouseToggle_Image.Width, virtualMouseToggle_Image.Height, GraphicsUnit.Pixel, virtualMouseToggle_ImageAttributes);

            if(virtualMouseToggle_Font == null)
            {
                virtualMouseToggle_Font = new Font(UI_Interface.MainForm.Font.FontFamily, 7.0f, FontStyle.Bold);
            }

            SizeF tagSize = e.Graphics.MeasureString(text, virtualMouseToggle_Font);
            Point tagLocation = new Point(((int)bounds.Width / 2) - ((int)tagSize.Width / 2) - 2, ((int)bounds.Height / 2) - ((int)tagSize.Height / 2) - 3);

            e.Graphics.DrawString(text, virtualMouseToggle_Font, Brushes.White, tagLocation);
        }

        private static System.Threading.Timer blinkTimer;
        private static bool blinking;

        private static void Blink_Tick(object state)
        {
            Button stepBtn = (Button)state;

            if(!stepBtn.Enabled)
            {
                blinkTimer?.Dispose();
                blinkTimer = null;
                return;
            }
            
            blinking = !blinking;
            stepBtn.Invalidate();
        }

        private static GraphicsPath gotoPrevGP;
        private static Pen gotoPrevGPPen;
        private static SolidBrush gotoPrevTextBrush;
        private static SolidBrush gotoPrevBackBrush;

        public static void GotoPrevPaint(object sender, PaintEventArgs e)
        {
            Button stepBtn = (Button)sender;

            Rectangle bounds = new Rectangle(stepBtn.ClientRectangle.X, stepBtn.ClientRectangle.Y, stepBtn.ClientRectangle.Width -1, stepBtn.ClientRectangle.Height -1);

            stepBtn.Location = new Point((UI_Interface.Cover.Left - bounds.Width) -1, UI_Interface.Cover.Top);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Color backCol = Theme_Settings.InfoPanelBackColor;

            if (gotoPrevBackBrush == null)
            {
                gotoPrevTextBrush = new SolidBrush(Color.FromArgb(255, Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B));
                gotoPrevBackBrush = new SolidBrush(Color.FromArgb(90, backCol.R, backCol.G, backCol.B));
            }

            if (gotoPrevGP == null)
            {
                gotoPrevGP = FormGraphicsUtil.MakeRoundedRect(bounds, 20, 20, true, false, false, true);
            }

            if(gotoPrevGPPen == null)
            {
                gotoPrevGPPen = new Pen(Color.FromArgb(210, 50, 50, 50),2);
            }

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
                e.Graphics.FillPath(gotoPrevBackBrush, gotoPrevGP);
                e.Graphics.DrawPath(gotoPrevGPPen, gotoPrevGP);
                e.Graphics.DrawString(sb.ToString(), stepBtn.Font, gotoPrevTextBrush, ghostTagLocation.X, ghostTagLocation.Y);
            }
        }

        private static GraphicsPath gotoNextGP;
        private static SolidBrush gotoNextTextBrush;
        private static SolidBrush gotoNextBackBrush;
        private static Pen gotoNextGPPen;

        public static void GotoNextPaint(object sender, PaintEventArgs e)
        {
            Button stepBtn = (Button)sender;

            if(blinkTimer == null && stepBtn.Enabled)
            {
                blinkTimer?.Dispose();
                blinkTimer = new System.Threading.Timer(Blink_Tick, sender, 400,400);
            }

            Rectangle bounds = new Rectangle(stepBtn.ClientRectangle.X, stepBtn.ClientRectangle.Y, stepBtn.ClientRectangle.Width - 1, stepBtn.ClientRectangle.Height - 1 );
            stepBtn.Location = new Point(UI_Interface.Cover.Right, UI_Interface.Cover.Top);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.CompositingQuality = CompositingQuality.HighQuality;

            Color backCol = Theme_Settings.InfoPanelBackColor;

            if(gotoNextTextBrush == null)
            {
                gotoNextTextBrush = new SolidBrush(blinking ? Color.White : Color.FromArgb(255, Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B));
            }

            gotoNextTextBrush.Color = blinking ? Color.White : Color.FromArgb(255, Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B);
            
            if(gotoNextBackBrush == null)
            {
                gotoNextBackBrush = new SolidBrush(Color.FromArgb(90, backCol.R, backCol.G, backCol.B));
            }
           
            if(gotoNextGP == null)
            {
                gotoNextGP = FormGraphicsUtil.MakeRoundedRect(bounds, 20, 20, false, true, true, false);
            }

            if (gotoNextGPPen == null)
            {
                gotoNextGPPen = new Pen(Color.FromArgb(210, 50, 50, 50),2);
            }

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
                e.Graphics.FillPath(gotoNextBackBrush, gotoNextGP);
                e.Graphics.DrawPath(gotoNextGPPen, gotoNextGP);
                e.Graphics.DrawString(sb.ToString(), stepBtn.Font, gotoNextTextBrush, textLocation.X, textLocation.Y);
            }
        }

        private static Label handlerNoteTitleCopy;

        public static void NotesTitleScrollTimer_Tick(object state, EventArgs eventArgs)
        {
            var title = UI_Interface.HandlerNoteTitle;
            int offset = 15;

            if (handlerNoteTitleCopy == null)
            {
                title.Location = new Point(title.Location.X, UI_Interface.ExpandHandlerNotesButton.Location.Y + (UI_Interface.ExpandHandlerNotesButton.Height / 2 - (title.Height / 2)));
                title.Dock = DockStyle.None;
                title.AutoSize = true;
                handlerNoteTitleCopy = new Label();
                handlerNoteTitleCopy.AutoSize = true;
                handlerNoteTitleCopy.Location = new Point(title.Right + offset, title.Location.Y);
                handlerNoteTitleCopy.Font = new Font(title.Font.FontFamily, title.Font.Size, FontStyle.Italic | FontStyle.Bold);
                handlerNoteTitleCopy.ForeColor = Color.FromArgb(216, 107, 100);
                handlerNoteTitleCopy.Text = "Read and follow them very carefully before asking for support.";
                handlerNoteTitleCopy.BackColor = title.BackColor;
                UI_Interface.HandlerNotesContainer.Controls.Add(handlerNoteTitleCopy);
            }

            if (title.Right > 0 && title.Left != handlerNoteTitleCopy.Right + offset)
            {
                title.Location = new Point(title.Location.X - 1, title.Location.Y);
                handlerNoteTitleCopy.Location = new Point(title.Right + offset, title.Location.Y);
            }
            else if (title.Right == 0)
            {
                handlerNoteTitleCopy.Location = new Point(handlerNoteTitleCopy.Location.X - 1, title.Location.Y);
                title.Location = new Point(handlerNoteTitleCopy.Right + offset, handlerNoteTitleCopy.Location.Y);
            }
            else if (handlerNoteTitleCopy.Right == 0)
            {
                title.Location = new Point(title.Location.X - 1, title.Location.Y);
                handlerNoteTitleCopy.Location = new Point(handlerNoteTitleCopy.Right + offset, handlerNoteTitleCopy.Location.Y);
            }
            else if (title.Left == handlerNoteTitleCopy.Right + offset)
            {
                handlerNoteTitleCopy.Location = new Point(handlerNoteTitleCopy.Location.X - 1, title.Location.Y);
                title.Location = new Point(handlerNoteTitleCopy.Right + offset, handlerNoteTitleCopy.Location.Y);
            }
            else if (handlerNoteTitleCopy.Left == title.Right + offset)
            {
                handlerNoteTitleCopy.Location = new Point(handlerNoteTitleCopy.Location.X - 1, title.Location.Y);
                title.Location = new Point(handlerNoteTitleCopy.Right + offset, handlerNoteTitleCopy.Location.Y);
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
                                    tooltip.Show("Supported inputs:" + "\n" + text.Replace("Supports ", "-"), pic, new Point(pic.Parent.ClientRectangle.Left,pic.Parent.ClientRectangle.Bottom),5000);                        
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
                        tooltip.Show("Supported inputs:" + "\n" + sb.ToString(), firstPb, firstPb.Parent.ClientRectangle.Left, firstPb.Parent.ClientRectangle.Bottom, 5000);
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

        private static Pen noteTitleBorderPen;
        private static SolidBrush noteTitleBackBrush;
        private static SolidBrush noteTitleBackTextBrush;

        public static void HandlerNotesContainer_Paint(object sender, PaintEventArgs e)
        {          
            DoubleBufferPanel container = sender as DoubleBufferPanel;
            PictureBox expandBtn = UI_Interface.ExpandHandlerNotesButton;

            if (noteTitleBorderPen == null)
            {
                noteTitleBorderPen = new Pen(Theme_Settings.HandlerNoteBorderColor);
                noteTitleBackBrush = new SolidBrush(Theme_Settings.HandlerNoteTitleBackColor);
                noteTitleBackTextBrush = new SolidBrush(Theme_Settings.HandlerNoteBackColor);
            }

            Rectangle rec = new Rectangle(0, 0, container.Width - 1, expandBtn.Height);
            e.Graphics.FillRectangle(noteTitleBackBrush, rec);

            e.Graphics.FillRectangle(noteTitleBackTextBrush, new Rectangle(0, rec.Bottom, rec.Width, (container.Height - rec.Height) - 2));

            if (Theme_Settings.UseNotesBorder)
            {
                e.Graphics.DrawRectangle(noteTitleBorderPen, rec);
                e.Graphics.DrawRectangle(noteTitleBorderPen, new Rectangle(0, rec.Bottom, rec.Width, (container.Height - rec.Height) - 2));
            }

        }
    }
}
