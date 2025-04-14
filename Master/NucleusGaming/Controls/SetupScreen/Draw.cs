using Gamepads;
using Jint.Parser.Ast;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    internal class Draw
    {
        private static string theme = Globals.ThemeFolder;

        private static Bitmap screenimg;
        private static Bitmap clickCursor;

        private static bool controllerIdentification = Theme_Settings.ControllerIdentification;

        private static bool useSetupScreenBorder = Theme_Settings.UseSetupScreenBorder;
        private static bool useLayoutSelectionBorder = Theme_Settings.UseLayoutSelectionBorder;
        private static bool useSetupScreenImage = Theme_Settings.UseSetupScreenImage;

        private static SolidBrush tagBrush;
        private static SolidBrush sizerBrush;
        private static SolidBrush screenBackBrush;

        private static Pen positionPlayerScreenPen;
        private static Pen positionScreenPen;
        private static Pen ghostBoundsPen;
        private static Pen destEditBoundsPen;

        private static ImageAttributes flashImageAttributes;

        private static Font playerTextFont;
        public static Font PlayerCustomFont;
        
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;

        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;

            PlayerCustomFont = new Font("Vermin Vibes 2 Soft", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            playerTextFont = new Font(Theme_Settings.CustomFont, 9.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
     
            positionScreenPen = new Pen(Theme_Settings.SetupScreenUIScreenColor, 1);
            positionPlayerScreenPen = new Pen(Theme_Settings.SetupScreenPlayerScreenColor, 1);

            tagBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            screenBackBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
            sizerBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));
            ghostBoundsPen = new Pen(Color.Red);
            destEditBoundsPen = new Pen(Color.FromArgb(255, 15, 220, 15));

            clickCursor = ImageCache.GetImage(theme + "click.png");
            screenimg = ImageCache.GetImage(theme + "screen.png");

            ///Flash image attributes
            {
                ColorMatrix colorMatrix = new ColorMatrix(new[]
                {
                    new float[] {1, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 0, 0, 0, 0},
                    new float[] {0, 1, 0, 1, 0},
                    new float[] {0.4f, 0, 0, 0, 1}
                });

                flashImageAttributes = new ImageAttributes();
                flashImageAttributes.SetColorMatrix(colorMatrix);
            }
        }


        public static void UIScreens(Graphics g)
        {
            var screens = BoundsFunctions.Screens;

            for (int i = 0; i < screens?.Length; i++)
            {
                UserScreen s = screens[i];

                if (s == null)
                {
                    continue;
                }

                //if a player is expanded don't draw the screen subBounds intersecting with its bounds
                if (s.Type != UserScreenType.Manual && s.Type != UserScreenType.FullScreen)
                {
                    var boundsToDraw = s.SubScreensBounds?.Values.Where(sb => !GameProfile.Instance.DevicesList.Any(pl => pl.EditBounds.IntersectsWith(sb)) && !GameProfile.GhostBounds.Any(pl => pl.Item2.IntersectsWith(sb))).ToList();

                    if (boundsToDraw.Count > 0)
                    {
                        g.DrawRectangles(positionScreenPen, boundsToDraw.ToArray());
                    }
                }

                if (s.SwapTypeBounds != null && s.SwapTypeBounds != RectangleF.Empty)
                {
                    RectangleF minimizedSwapType = s.SwapTypeBounds;

                    //resize the SwapTypeBound rectangle dynamically
                    var interstcWithSwapTypeBound = GameProfile.Instance.DevicesList.Where(dv => dv.EditBounds.IntersectsWith(minimizedSwapType)).ToArray();

                    if (interstcWithSwapTypeBound.Length > 0 && !s.SwapTypeBounds.Contains(BoundsFunctions.MousePos))
                    {
                        minimizedSwapType = new RectangleF(s.SwapTypeBounds.X, s.SwapTypeBounds.Y, s.SwapTypeBounds.Width / 2, s.SwapTypeBounds.Height / 2);
                    }

                    if (useLayoutSelectionBorder)
                    {
                        g.DrawRectangles(positionScreenPen, new RectangleF[] {minimizedSwapType });
                    }       

                    if (useSetupScreenImage)
                    {
                        g.DrawImage(screenimg, s.UIBounds);
                    }

                    g.DrawImage(s.SwapTypeImage, minimizedSwapType);
                         
                    if (useSetupScreenBorder)
                    {
                        g.DrawRectangles(positionScreenPen, new RectangleF[] { s.UIBounds });
                    }

                    //show a sawp type screen type on each UI screen if its anew game
                    if (parent.UserGameInfo.Game.MetaInfo.FirstLaunch &&
                        s.Type == UserScreenType.FullScreen &&
                        interstcWithSwapTypeBound.Length == 0 && 
                        BoundsFunctions.ShowSwapTypeTip)
                    {
                        g.DrawImage(clickCursor, new RectangleF((minimizedSwapType.Left + (minimizedSwapType.Width / 2)) + (minimizedSwapType.Width / 8.2f), (minimizedSwapType.Top + (minimizedSwapType.Height / 2)) - (minimizedSwapType.Width / 8.2f), (minimizedSwapType.Width * 8 ) / 2.5f, (minimizedSwapType.Width) / 2.5f));
                    }
                }
            }
        }


        public static void UIDevices(Graphics g, PlayerInfo player)
        {
            if (player.EditBounds == RectangleF.Empty)
            {
                return;
            }

            RectangleF s = player.EditBounds;

            g.Clip = new Region(new RectangleF(s.X, s.Y, s.Width + 2, s.Height + 1));

            Rectangle gamepadRect = RectangleUtil.ScaleAndCenter(player.Image.Size, new Rectangle((int)s.X, (int)s.Y, (int)s.Width, (int)s.Height));

            float height = player.EditBounds.Height / 2.8f > 0 ? (player.EditBounds.Height / 2.8f) : 1;

            Font fontToScale = new Font(PlayerCustomFont.FontFamily, height, FontStyle.Regular, GraphicsUnit.Pixel);

            if (player.ScreenIndex != -1 && player.MonitorBounds != Rectangle.Empty)
            {
                g.DrawRectangle(positionPlayerScreenPen, new Rectangle((int)s.X + 1, (int)s.Y + 1, (int)s.Width, (int)s.Height));
            }

            switch (player.InputType)
            {
                case InputType.XInput:
                    {
                        string str = (player.GamepadId + 1).ToString();

                        SizeF size = g.MeasureString(str, fontToScale);
                        PointF loc = RectangleUtil.Center(size, s);
                        loc.Y -= gamepadRect.Height * 0.10f;

                        if (player.ShouldFlash)
                        {
                            g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                        }
                        else
                        {
                            if (player.IsInputUsed)
                                g.DrawImage(player.Image, gamepadRect);
                        }

                        if (controllerIdentification && player.IsInputUsed)
                        {
                            g.DrawString(str, fontToScale, Brushes.White, loc);
                        }
                        break;
                    }
                case InputType.SDL2:
                    {
                        string str = (player.GamepadId + 1).ToString();

                        SizeF size = g.MeasureString(str, fontToScale);
                        PointF loc = RectangleUtil.Center(size, s);
                        loc.Y -= gamepadRect.Height * 0.10f;

                        if (player.ShouldFlash)
                        {
                            g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                        }
                        else
                        {
                            if (player.IsInputUsed)
                                g.DrawImage(player.Image, gamepadRect);
                        }

                        if (controllerIdentification && player.IsInputUsed)
                        {
                            g.DrawString(str, fontToScale, Brushes.White, loc);
                        }
                        break;
                    }
                case InputType.DInput:
                    {
                        string str = (player.GamepadId + 1).ToString();
                        SizeF size = g.MeasureString(str, fontToScale);
                        PointF loc = RectangleUtil.Center(size, gamepadRect);
                        loc.Y -= gamepadRect.Height * 0.12f;

                        if (player.ShouldFlash)
                        {
                            g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                        }
                        else
                        {
                            g.DrawImage(player.Image, gamepadRect);
                        }

                        if (controllerIdentification)
                        {
                            g.DrawString(str, fontToScale, Brushes.White, loc);
                        }
                        break;
                    }
                case InputType.SingleKB:
                    {
                        if (player.ShouldFlash)
                        {
                            player.IsInputUsed = true;
                            g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                        }
                        else if (player.IsInputUsed)
                        {
                            g.DrawImage(player.Image, gamepadRect);

                        }
                        break;
                    }
                case InputType.KB:
                case InputType.Mouse:
                case InputType.KBM:
                    {
                        if (player.RawMouseDeviceHandle != IntPtr.Zero && player.RawKeyboardDeviceHandle != IntPtr.Zero)
                        {
                            if (player.ShouldFlash)
                            {
                                player.IsInputUsed = true;
                                g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                            }
                            else if (player.IsInputUsed)
                            {
                                g.DrawImage(player.Image, gamepadRect);
                            }
                        }
                        else
                        {
                            if (player.ShouldFlash)
                            {
                                player.IsInputUsed = true;
                                g.DrawImage(player.Image, gamepadRect, 0, 0, player.Image.Width, player.Image.Height, GraphicsUnit.Pixel, flashImageAttributes);
                            }
                            else if (player.IsInputUsed)
                            {
                                g.DrawImage(player.Image, gamepadRect);
                            }

                            if (player.IsInputUsed)
                            {
                                float virtualheight = player.EditBounds.Height / 4f > 0 ? (player.EditBounds.Height / 4f) : 1;
                                Font virtualfontToScale = new Font("Franklin Gothic", virtualheight, FontStyle.Regular, GraphicsUnit.Pixel);
                                string str = "virtual";

                                SizeF size = g.MeasureString(str, virtualfontToScale);
                                PointF loc = RectangleUtil.Center(size, s);
                                loc.Y -= gamepadRect.Height * 0.10f;

                                RectangleF gradientBrushbounds = new RectangleF(loc.X, loc.Y, size.Width, size.Height);
                                RectangleF bounds = new RectangleF(loc.X, loc.Y, size.Width, size.Height);

                                Color vcolor = Color.FromArgb(150, 0, 0, 0);
                                Color vcolor2 = Color.FromArgb(255, 0, 0, 0);

                                LinearGradientBrush lgb =
                                new LinearGradientBrush(gradientBrushbounds, vcolor2, vcolor, 90f);

                                ColorBlend topcblend = new ColorBlend(4);
                                topcblend.Colors = new Color[3] { vcolor, vcolor2, vcolor };
                                topcblend.Positions = new float[3] { 0f, 0.8f, 1f };

                                lgb.InterpolationColors = topcblend;

                                g.FillRectangle(lgb, bounds);
                                g.DrawString(str, virtualfontToScale, Brushes.YellowGreen, loc);

                                virtualfontToScale.Dispose();
                                lgb.Dispose();
                            }
                        }

                        break;
                    }
            }

            g.Clip.Dispose();
            fontToScale.Dispose();
        }

        public static void GhostBounds(Graphics g)
        {
            g.ResetClip();

            //Draw all the profile players bounds until they get filled
            for (int b = 0; b < GameProfile.GhostBounds.Count(); b++)
            {
                var ghostBounds = GameProfile.GhostBounds[b];

                Rectangle ghostMBounds = ghostBounds.Item1;
                RectangleF ghostEBounds = ghostBounds.Item2;

                if (GameProfile.Instance.DevicesList.All(p => p.MonitorBounds != ghostMBounds))
                {
                    string ghostTag = $"P{b + 1}: {GameProfile.ProfilePlayersList[b].Nickname}";

                    SizeF ghostTagSize = g.MeasureString(ghostTag, playerTextFont);
                    Point ghostTagLocation = new Point(((int)ghostEBounds.Left + (int)ghostEBounds.Width / 2) - ((int)ghostTagSize.Width / 2), (int)(ghostEBounds.Bottom + 1 - ghostTagSize.Height));
                    RectangleF ghostTagBack = new RectangleF(ghostTagLocation.X, ghostTagLocation.Y, ghostTagSize.Width, ghostTagSize.Height);
                    Rectangle ghostTagBorder = new Rectangle(ghostTagLocation.X, ghostTagLocation.Y, (int)ghostTagSize.Width, (int)ghostTagSize.Height);

                    g.FillRectangle(Brushes.DarkSlateGray, ghostTagBack);
                    g.DrawRectangles(ghostBoundsPen, new RectangleF[] { ghostEBounds });
                    g.DrawRectangle(ghostBoundsPen, ghostTagBorder);
                    g.DrawString(ghostTag, playerTextFont, Brushes.Orange, ghostTagLocation.X, ghostTagLocation.Y);
                }
            }
        }

        public static void SelectedPlayerBounds(Graphics g)
        {
            g.FillRectangle(sizerBrush, BoundsFunctions.SelectedPlayer.EditBounds);
        }

        public static void DestinationBounds(Graphics g)
        {
            g.DrawRectangles(destEditBoundsPen, new RectangleF[] { BoundsFunctions.DestEditBounds });
        }

        public static void PlayerBoundsInfo(Graphics g)
        {
            string playerBoundsInfo = BoundsFunctions.PlayerBoundsInfoText(BoundsFunctions.SelectedPlayer);
            SizeF boundsRect = g.MeasureString(playerBoundsInfo, playerTextFont);
            Point location = new Point(((parent.Width / 2) - (int)boundsRect.Width / 2) , ((parent.Bottom - (int)boundsRect.Height)) );  
            g.DrawString(playerBoundsInfo, playerTextFont, Brushes.White, location.X - 3, location.Y - 6 );
            
            //g.FillRectangles(sizerBrush, new RectangleF[] { BoundsFunctions.sizerBtnLeft });
            //g.FillRectangles(sizerBrush, new RectangleF[] { BoundsFunctions.sizerBtnRight });
            //g.FillRectangles(sizerBrush, new RectangleF[] { BoundsFunctions.sizerBtnTop });
            //g.FillRectangles(sizerBrush, new RectangleF[] { BoundsFunctions.sizerBtnBottom });
        }

        public static void PlayerTag(Graphics g, PlayerInfo player)
        {
            RectangleF s = player.EditBounds;

            g.Clip = new Region(new RectangleF(s.X, s.Y, s.Width + 1, s.Height + 1));
            int playerIndex = GameProfile.AssignedDevices.FindIndex(pl => pl == player);

            string tag = $"P{playerIndex + 1}: {player.Nickname}";

            SizeF tagSize = g.MeasureString(tag, playerTextFont);
            Point tagLocation = new Point(((int)s.Left + (int)s.Width / 2) - ((int)tagSize.Width / 2), (int)(s.Bottom + 1 - tagSize.Height));
            RectangleF tagBack = new RectangleF(tagLocation.X, tagLocation.Y, tagSize.Width, tagSize.Height);
            Rectangle tagBorder = new Rectangle(tagLocation.X, tagLocation.Y, (int)tagSize.Width, (int)tagSize.Height);

            g.Clip = new Region(tagBack);

            g.FillRectangle(tagBrush, tagBack);
            g.DrawRectangle(positionScreenPen, tagBorder);
            g.DrawString(tag, playerTextFont, Brushes.White, tagLocation.X, tagLocation.Y);
            g.Clip.Dispose();
        }
    }
}
