using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
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
        private static SolidBrush guestBrush;
        private static SolidBrush guestBackBrush;
        private static SolidBrush guestFlashBrush;
        private static SolidBrush guestIndexBrush;
        private static SolidBrush sizerBrush;
        private static SolidBrush screenBackBrush;

        private static SolidBrush guestRemovalBack;
        private static Pen positionPlayerScreenPen;
        private static Pen positionScreenPen;
        private static Pen ghostBoundsPen;
        private static Pen destEditBoundsPen;

        private static ImageAttributes flashImageAttributes;
        private static ImageAttributes ExtraGamepadflashImage;

        private static Font playerTextFont;
        public static Font PlayerCustomFont;
        
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;

        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;

            PlayerCustomFont = new Font("Franklin Gothic", 12.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
            playerTextFont = new Font(Theme_Settings.CustomFont, 9.0f, FontStyle.Regular, GraphicsUnit.Point, 0);
     
            positionScreenPen = new Pen(Theme_Settings.SetupScreenUIScreenColor, 2);
            positionPlayerScreenPen = new Pen(Theme_Settings.SetupScreenPlayerScreenColor, 2);
            guestIndexBrush = new SolidBrush(Color.FromArgb(200,255,255,255));

            tagBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            guestBrush = new SolidBrush(Color.DarkGreen);
            guestRemovalBack = new SolidBrush(Color.FromArgb(200, 0, 0, 0));

            guestBackBrush = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
            guestFlashBrush = new SolidBrush(Color.FromArgb(255, 0, 180, 20));
            screenBackBrush = new SolidBrush(Color.FromArgb(60, 0, 0, 0));
            sizerBrush = new SolidBrush(Color.FromArgb(30, 0, 0, 0));

            ghostBoundsPen = new Pen(Color.Red,2);
            destEditBoundsPen = new Pen(Color.FromArgb(255, 15, 220, 15),2);

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

            {
                ColorMatrix colorMatrix = new ColorMatrix(new[]
                {
                    new float[] {0.7f, 0, 0, 0, 0},
                    new float[] {0, 1.4f, 0, 0, 0}, 
                    new float[] {0, 0, 0.6f, 0, 0},
                    new float[] {0, 0, 0, 1, 0}, 
                    new float[] {0.2f, 0.5f, 0.2f, 0, 1}
                });

                ExtraGamepadflashImage = new ImageAttributes();
                ExtraGamepadflashImage.SetColorMatrix(colorMatrix);
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
                    //we keep a margin
                    Dictionary<RectangleF, RectangleF> bounds = new Dictionary<RectangleF, RectangleF>();

                    foreach (KeyValuePair<Rectangle, RectangleF>  sub in s.SubScreensBounds)
                    {
                        RectangleF rec = new RectangleF(sub.Value.X + 2, sub.Value.Y +2,sub.Value.Width -5,sub.Value.Height -5);
                        bounds.Add(rec,sub.Value);
                    }

                    var boundsToDraw = bounds.Where(sb => !GameProfile.Instance.DevicesList.Any(pl => pl.EditBounds.IntersectsWith(sb.Key)) && !GameProfile.GhostBounds.Any(pl => pl.Item2.IntersectsWith(sb.Key)))/*.ToList()*/;

                    if (boundsToDraw.Count() > 0)
                    {
                        foreach(KeyValuePair<RectangleF, RectangleF> sub in boundsToDraw)
                        {
                            g.DrawRectangles(positionScreenPen, new RectangleF[] { sub.Value });
                        }
                           
                    }
                }

                if (s.SwapTypeBounds != null && s.SwapTypeBounds != RectangleF.Empty)
                {
                    RectangleF minimizedSwapType;

                    //resize the SwapTypeBound rectangle dynamically
                    var interstcWithSwapTypeBound = GameProfile.Instance.DevicesList.Where(dv => dv.EditBounds.IntersectsWith(s.SwapTypeBounds)).ToArray();

                    if ((interstcWithSwapTypeBound.Length > 0 && !s.SwapTypeBounds.Contains(BoundsFunctions.MousePos) || BoundsFunctions.Dragging))
                    {
                        minimizedSwapType = new RectangleF(s.SwapTypeBounds.X, s.SwapTypeBounds.Y, s.SwapTypeBounds.Width / 2, s.SwapTypeBounds.Height / 2);
                    }
                    else
                    {
                        minimizedSwapType = s.SwapTypeBounds;
                    }

                    if (useLayoutSelectionBorder)
                    {
                        g.DrawRectangles(positionScreenPen, new RectangleF[] { minimizedSwapType });
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

                    //show a swap type screen tip on each UI screen if its a new game
                    if (parent.UserGameInfo.Game.MetaInfo.FirstLaunch &&
                        s.Type == UserScreenType.FullScreen &&
                        interstcWithSwapTypeBound.Length == 0 &&
                        BoundsFunctions.ShowSwapTypeTip)
                    {
                        g.DrawImage(clickCursor, new RectangleF((minimizedSwapType.Left + (minimizedSwapType.Width / 2)) + (minimizedSwapType.Width / 8.2f), (minimizedSwapType.Top + (minimizedSwapType.Height / 2)) - (minimizedSwapType.Width / 8.2f), (minimizedSwapType.Width * 8) / 2.5f, (minimizedSwapType.Width) / 2.5f));
                    }
                }
            }
        }

        public static void UIDevices(Graphics g, PlayerInfo player)
        {           
            //Don't draw this player, will draw with its guests instead
            if (GameProfile.Instance.DevicesList.Any(pl => pl.InstanceGuests.Contains(player)))
            {
                return;
            }

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

                        #region instance guests drawing
                        if (userGameInfo.Game.PlayersPerInstance > 0 && player.ScreenIndex != -1)
                        {
                            DrawGuests(g, player, fontToScale);
                        }
                        #endregion
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

                        #region instance guests drawing
                        if (userGameInfo.Game.PlayersPerInstance > 0 && player.ScreenIndex != -1)
                        {
                            DrawGuests(g, player, fontToScale);
                        }
                        #endregion
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
                            if (player.IsInputUsed)
                                g.DrawImage(player.Image, gamepadRect);
                        }
                       
                        if (controllerIdentification && player.IsInputUsed)
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
                                try
                                {
                                    Font virtualfontToScale = new Font("Franklin Gothic", 12.75F, FontStyle.Regular, GraphicsUnit.Pixel);
                                    string str = "virtual";

                                    SizeF size = g.MeasureString(str, virtualfontToScale);
                                    Rectangle loc = RectangleUtil.Center(new Rectangle(0,0,(int)size.Width + 5,(int)size.Height + 5), new Rectangle((int)s.X, (int)s.Y, (int)s.Width, (int)s.Height));
                                    loc.Y = (int)(gamepadRect.Top + 2);

                                    RectangleF gradientBrushbounds = new RectangleF(loc.X, loc.Y, size.Width, size.Height);
                                    RectangleF bounds = new RectangleF(loc.X, loc.Y, size.Width, size.Height);

                                    Color vcolor = Color.FromArgb(150, 0, 0, 0);
                                    Color vcolor2 = Color.FromArgb(255, 0, 0, 0);

                                    using (LinearGradientBrush lgb = new LinearGradientBrush(gradientBrushbounds, vcolor2, vcolor, 90f))
                                    {
                                        ColorBlend topcblend = new ColorBlend(4);
                                        topcblend.Colors = new Color[3] { vcolor, vcolor2, vcolor };
                                        topcblend.Positions = new float[3] { 0f, 0.8f, 1f };

                                        lgb.InterpolationColors = topcblend;

                                        g.FillRectangle(lgb, bounds);
                                        g.DrawString(str, virtualfontToScale, Brushes.YellowGreen, loc);

                                        virtualfontToScale.Dispose();
                                    }
                                }
                                catch
                                {

                                }
                            }
                        }

                        break;
                    }
            }

            g.Clip.Dispose();
            fontToScale.Dispose();
        }

        private static void DrawGuests(Graphics g, PlayerInfo player, Font fontToScale)
        {
            RectangleF hostUIBounds = player.EditBounds;
            float guestsAreaHeight = (hostUIBounds.Height / 3) * 2.2f;

            RectangleF guestsArea = RectangleUtil.ScaleAndCenter(player.Image.Size, new Rectangle((int)hostUIBounds.X + 2, (int)(hostUIBounds.Bottom - guestsAreaHeight) - 2, (int)hostUIBounds.Width - 2, (int)guestsAreaHeight));

            guestsArea.Location = new Point((int)(hostUIBounds.Right - guestsArea.Width), (int)hostUIBounds.Top + 2);

            int guestsPerRow = 4;
            float guestUIHeight = (guestsArea.Width / guestsPerRow) / (guestsArea.Width / guestsArea.Height);
            var playerInstanceGuests = player.InstanceGuests.Where(ig => ig.GamepadId != -1).ToList();

            for (int ppi = 0; ppi < userGameInfo.Game.PlayersPerInstance; ppi++)
            {
                PlayerInfo guest = null;

                if (ppi < playerInstanceGuests.Count)
                {
                    guest = playerInstanceGuests[ppi];
                }

                int row = ppi / guestsPerRow;
                int col = ppi % guestsPerRow;

                float x = guestsArea.Left + col * (guestsArea.Width / guestsPerRow);
                float y = guestsArea.Top + row * guestUIHeight;

                Rectangle sub = new Rectangle((int)x, (int)y, (int)(guestsArea.Width / guestsPerRow), (int)guestUIHeight);
                SolidBrush idBrush = guestBrush;

                g.SmoothingMode = SmoothingMode.AntiAlias;

                if (guest != null)
                {
                    if (guest.ShouldFlash)
                    {
                        idBrush = guestFlashBrush;
                    }

                    g.FillRectangles(guestBackBrush, new RectangleF[] { sub });

                    DrawGamepadShape(g, sub, idBrush);
                }
                else
                {
                    if (ppi == playerInstanceGuests.Count)
                    {
                        int freeSlots = ((userGameInfo.Game.PlayersPerInstance - 1) - playerInstanceGuests.Count);

                        if (freeSlots > 0)
                        {
                            g.FillRectangles(guestBackBrush, new RectangleF[] { sub });
                            DrawGamepadShape(g, sub, null);
                            float height = sub.Height;

                            fontToScale = new Font(PlayerCustomFont.FontFamily, height / 1.2f, FontStyle.Regular, GraphicsUnit.Pixel);
                            string gamepadIndex = "+" + freeSlots.ToString();
                            SizeF size = g.MeasureString(gamepadIndex, fontToScale);
                            PointF loc = new PointF(sub.X + ((sub.Width / 2) - (size.Width / 2)) + 2, (sub.Y + ((sub.Height / 2) - (size.Height / 2))) + 2);


                            g.DrawString(gamepadIndex, fontToScale, guestIndexBrush, loc);
                            if (player.WaitGuests)
                            {
                                int awaitCount = player.CurrentMaxGuests - player.InstanceGuests.Count();
                                string infoText = "Await " + (awaitCount).ToString() + (awaitCount > 1 ? " Guests..." : " Guest...");
                                fontToScale = new Font(PlayerCustomFont.FontFamily, height / 2.2f, FontStyle.Regular, GraphicsUnit.Pixel);
                                size = g.MeasureString(infoText, fontToScale);
                                loc = new PointF(sub.Left, sub.Bottom);
                                g.FillRectangles(guestBackBrush, new RectangleF[] { new RectangleF(loc.X, loc.Y, size.Width, size.Height) });

                                g.DrawString(infoText, fontToScale, Brushes.Orange, loc);
                            }
                        }
                    }

                }

                if (guest != null)
                {
                    if (controllerIdentification)
                    {
                        float height = sub.Height;
                        fontToScale = new Font(PlayerCustomFont.FontFamily, height, FontStyle.Regular, GraphicsUnit.Pixel);
                        string gamepadIndex = (guest.GamepadId + 1).ToString();
                        SizeF size = g.MeasureString(gamepadIndex, fontToScale);
                        PointF loc = new PointF(sub.X + ((sub.Width / 2) - (size.Width / 2)) + 2, (sub.Y + ((sub.Height / 2) - (size.Height / 2))) + 2);

                        g.DrawString(gamepadIndex, fontToScale, guestIndexBrush, loc);
                    }

                    guest.GuestBounds = sub;
                }
            }
        }
        
        private static void DrawGamepadShape(Graphics g,RectangleF sub,SolidBrush pollingBrush)
        {
            //Draw gamepad shape (quit primitive)
            GraphicsState state = g.Save();

            Brush brush = pollingBrush != null ? pollingBrush : Brushes.DarkGreen;

            Pen outline = Pens.GreenYellow;
            
            //left
            float langle = -70f; 
            float lcenterX = sub.X + (sub.Width / 3.9F);
            float lcenterY = sub.Y + (sub.Height / 2);
            float lwidth = sub.Width / 2;
            float lheight = sub.Height / 2.5F;
            g.TranslateTransform(lcenterX, lcenterY);
            g.RotateTransform(langle);
            g.FillEllipse(brush, -lwidth / 2, -lheight / 2, lwidth, lheight);
            g.Restore(state);
            
            //right
            float rangle = 70f;
            float rcenterX = sub.Right - (sub.Width / 3.9F);
            float rcenterY = sub.Y + (sub.Height / 2);
            float rwidth = sub.Width / 2;
            float rheight = sub.Height / 2.5F;
            state = g.Save();
            g.TranslateTransform(rcenterX, rcenterY);
            g.RotateTransform(rangle);
            g.FillEllipse(brush, -rwidth / 2, -rheight / 2, rwidth, rheight);
            g.Restore(state);

            //middle
            float mangle = 0f; 
            float mcenterX = sub.X + (sub.Width / 2);
            float mcenterY = sub.Y + (sub.Height / 2.5F);
            float mwidth = sub.Width / 3.9F;
            float mheight = sub.Height / 2.5F;
            state = g.Save();
            g.TranslateTransform(mcenterX, mcenterY);
            g.RotateTransform(mangle);
            g.FillRectangle(brush, -mwidth / 2, -mheight / 2, mwidth, mheight);
            g.Restore(state);    
        }

        public static void DrawGuestRemovalText(Graphics g, PlayerInfo guest)
        {
            RectangleF s = guest.GuestBounds;

            g.Clip = new Region(new RectangleF(s.X, s.Y, s.Width + 1, s.Height + 1));

            string tag = $"Right click to remove ({guest.GamepadId + 1})";

            SizeF tagSize = g.MeasureString(tag, playerTextFont);
            Point tagLocation = new Point((int)s.Left, (int)s.Bottom);
            RectangleF tagBack = new RectangleF(tagLocation.X, tagLocation.Y, tagSize.Width, tagSize.Height);

            while(tagBack.Right >= parent?.Right)
            {
                float adjustX = tagBack.X -= 1.0F;
                tagBack.Location = new PointF(adjustX, tagBack.Y);
            }

            g.Clip = new Region(tagBack);

            using (GraphicsPath backGp = FormGraphicsUtil.MakeRoundedRect(tagBack, 10, 10, true, true, true, true))
            {
                g.FillPath(guestRemovalBack, backGp);
            }

            g.DrawString(tag, playerTextFont, Brushes.Orange, tagBack.X, tagBack.Y);
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
            g.DrawString(playerBoundsInfo, playerTextFont, Brushes.GreenYellow, location.X - 3, location.Y - 6 );
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
            g.DrawRectangle(positionPlayerScreenPen, tagBorder);
            g.DrawString(tag, playerTextFont, Brushes.White, tagLocation.X, tagLocation.Y);
            g.Clip.Dispose();
        }
    }
}
