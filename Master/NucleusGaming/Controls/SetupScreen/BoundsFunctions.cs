using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    public static class BoundsFunctions
    {
        //This should not rely on UI at all in the future(SetupScreenControl)
        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;
            profile = _profile;
            screensWatcherTimer = new System.Threading.Timer(ScreensWatcher_Tick, null, 0, 3000);
        }

        private static System.Threading.Timer screensWatcherTimer;
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;
        public static PlayerInfo SelectedPlayer;

        public static UserScreen[] Screens;

        private static PointF draggingOffset;
        public static Point MousePos => mousePos;
        private static int draggingIndex = -1;
        public static int DraggingScreen = -1;

        internal static RectangleF PlayersArea;
        public static Rectangle TotalBounds;
        private static RectangleF sizer;
        private static RectangleF sizerBtnLeft;
        private static RectangleF sizerBtnRight;
        private static RectangleF sizerBtnTop;
        private static RectangleF sizerBtnBottom;
        private static RectangleF sizerBtnCenter;

        private  static RectangleF destEditBounds;
        public static RectangleF DestEditBounds

        {
            get => destEditBounds;
            private set
            {
                if (destEditBounds != RectangleF.Empty)
                {
                    parent?.Invalidate(new Rectangle((int)destEditBounds.X - 5, (int)destEditBounds.Y - 5, (int)destEditBounds.Width + 10, (int)destEditBounds.Height + 10), false);//invalidate prev bounds
                }

                destEditBounds = value;

                if (destEditBounds != RectangleF.Empty)
                {
                    parent?.Invalidate(new Rectangle((int)destEditBounds.X - 5, (int)destEditBounds.Y - 5, (int)destEditBounds.Width + 10, (int)destEditBounds.Height + 10), false);//invalidate new bounds
                }
            }
        }

        private static Rectangle destMonitorBounds;

        public static RectangleF ActiveSizer => activeSizer;
        private static RectangleF activeSizer;

        public static bool Dragging = false;
        public static bool ShowSwapTypeTip = true;
        private static Point mousePos;
        private static bool canAddGuest;

        private static PlayerInfo instanceHost;
        public static PlayerInfo ShowGuestRemovelText;
        
        public static List<object> PlayerCounters = new List<object>();

        internal static string PlayerBoundsInfoText(PlayerInfo selectedPlayer)
        {
            int width = selectedPlayer.MonitorBounds.Width;
            int height = selectedPlayer.MonitorBounds.Height;

            int fwidth = selectedPlayer.MonitorBounds.Width;
            int fheight = selectedPlayer.MonitorBounds.Height;

            float ratio = (float)width / height;

            while (fwidth != 0 && fheight != 0)
            {
                if (fwidth > fheight)
                    fwidth %= fheight;
                else
                    fheight %= fwidth;
            }

            float val1 = (fwidth != 0 ? (width / fwidth) : (width / fheight));
            float val2 = (fwidth != 0 ? ((width / fwidth) / ratio) : ((width / fheight) / ratio));

            var spb = selectedPlayer.MonitorBounds;
            return $"Resolution: {spb.Width} X {spb.Height}  Aspect Ratio: {val1} : {val2}  Top: {spb.Top}  Bottom: {spb.Bottom}  Left: {spb.Left}  Right: {spb.Right}";
        }

        public static void ScreensWatcher_Tick(object state)
        {
            if (Screens != null)
            {
                Rectangle newBounds = RectangleUtil.Union(ScreensUtil.AllScreens());
                var sysScreens = ScreensUtil.AllScreens();

                bool requiresUpdate = Screens?.Length != sysScreens.Length;

                //check screen bounds changed even a little bit to get proper datas
                if (!requiresUpdate)
                {
                    for (int i = 0; i < Screens.Length; i++)
                    {
                        UserScreen scr = Screens[i];

                        if(scr.MonitorBounds != sysScreens[i].MonitorBounds)
                        {
                            requiresUpdate = true;
                        }
                    }
                }

                if (requiresUpdate || newBounds != TotalBounds)
                {
                    UpdateScreens();

                    for (int i = 0; i < Screens.Length; i++)
                    {
                        UserScreen screen = Screens[i];
                        screen.Type = 0;
                        screen.PlayerOnScreen = 0;
                    }

                    GameProfile.Instance?.Reset();
                    DevicesFunctions.UpdateDevices();

                    parent.Invalidate(false);
                }
            }
            else
            {
                UpdateScreens();
            }
        }

        internal static float playerSize;

        internal static void SetPlayerArea()
        {
            List<PlayerInfo> playerData = GameProfile.Instance?.DevicesList;

            if (playerData != null)
            {           
                float playersWidth = parent.Width * 0.70f;

                float playerCount = playerData.Count;
                float playerWidth = playersWidth * 0.9f;
                float playerHeight = parent.Height * 0.2f;

                PlayersArea = new RectangleF(10, 0, playersWidth, playerHeight);

                float playersAreaScale = PlayersArea.Width * PlayersArea.Height;
                float maxArea = playersAreaScale / playerCount;
                playerSize = (float)(Math.Sqrt(maxArea) - 0.5F);///force the round down
                                                                ///see if the size can fit it or we need to make some further adjustments
                float horizontal = (playersWidth / playerSize) - 0.5F;
                float vertical = (int)Math.Round((playerHeight / playerSize) - 0.5);
                float total = vertical * horizontal;

                if (total < playerCount)
                {
                    float newVertical = vertical + 1;
                    Draw.PlayerCustomFont = new Font(Draw.PlayerCustomFont.FontFamily, Draw.PlayerCustomFont.Size * 0.8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                    playerSize = (int)Math.Round(((playerHeight / 1.2f) / newVertical));
                }

                for (int i = 0; i < playerData.Count; i++)
                {
                    PlayerInfo info = playerData[i];

                    if (info.ScreenIndex == -1)
                    {
                        info.EditBounds = GetDefaultBounds(i);
                        info.SourceEditBounds = info.EditBounds;
                        info.DisplayIndex = -1;
                    }
                }
            }
            parent.Invalidate();
        }
      
        internal static RectangleF GetDefaultBounds(int index)
        {
            float lineWidth = index * playerSize;
            float line = (float)Math.Round(((lineWidth + playerSize) / (double)PlayersArea.Width) - 0.5);
            int perLine = (int)Math.Round((PlayersArea.Width / (double)playerSize) - 0.5);

            float x = PlayersArea.X + (index * playerSize) - (perLine * playerSize * line);
            float y = PlayersArea.Y + (playerSize * line);

            return new RectangleF(x, y, playerSize, playerSize);
        }

        private static RectangleF ScaleAndTranslate(Rectangle original, Rectangle bounding, Rectangle targetArea, float scale)
        {
            float newX = (original.X - bounding.X) * scale;
            float newY = (original.Y - bounding.Y) * scale;
            float newWidth = original.Width * scale;
            float newHeight = original.Height * scale;

            float offsetX = targetArea.X + (targetArea.Width - bounding.Width * scale) / 2f;
            float offsetY = targetArea.Y + (targetArea.Height - bounding.Height * scale) / 2f;

            return new RectangleF(newX + offsetX, newY + offsetY, newWidth, newHeight);
        }

        private static RectangleF SetScreenArea(UserScreen screen)
        {
            Rectangle freeSpace = new Rectangle(0, (int)(parent.Top + PlayersArea.Height) - 5, parent.Width - 5, (int)(parent.Height - PlayersArea.Height) - 5);

            if(Screens.Length == 1)
            {
                freeSpace = new Rectangle(10, (int)(parent.Top + PlayersArea.Height) - 30, (int)(parent.Width / 1.38f), (int)(parent.Height - PlayersArea.Height) - 10);
            }

            float scaleX = (float)freeSpace.Width / TotalBounds.Width;
            float scaleY = (float)freeSpace.Height / TotalBounds.Height;
            float scale = Math.Min(scaleX, scaleY); // Maintain aspect ratio

            return ScaleAndTranslate(screen.MonitorBounds, TotalBounds, freeSpace, scale);
        }

        internal static void UpdateScreens()
        {
            if (PlayersArea == RectangleF.Empty)
            {
                return;
            }

            if (Screens == null)
            {
                Screens = ScreensUtil.AllScreens();
                TotalBounds = RectangleUtil.Union(Screens);
            }
            else
            {
                //for (int i = 0; i < Screens.Length; i++)
                //{
                //    Screens[i].PlayerOnScreen = 0;
                //}

                UserScreen[] newScreens = ScreensUtil.AllScreens();
                Rectangle newBounds = RectangleUtil.Union(newScreens);

                if (newBounds.Equals(TotalBounds))
                {
                    return;
                }

                ///Screens got updated, need to reflect in our window
                Screens = newScreens;
                TotalBounds = newBounds;

                ///remove all players Screens
                List<PlayerInfo> playerData = profile?.DevicesList;

                if (playerData != null)
                {
                    for (int i = 0; i < playerData.Count; i++)
                    {
                        PlayerInfo player = playerData[i];

                        ClearPlayerGuests(player);
                        player.EditBounds = GetDefaultBounds(draggingIndex);
                        player.ScreenIndex = -1;
                        player.DisplayIndex = -1;
                    }
                }
            }

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];
                screen.UIBounds = SetScreenArea(screen);
                screen.SwapTypeBounds = new RectangleF(screen.UIBounds.X, screen.UIBounds.Y, screen.UIBounds.Width * 0.1f, screen.UIBounds.Width * 0.1f);
                screen.Index = i;
            }

           parent.Invalidate();
        }

        internal static RectangleF ScreensArea;

        internal static void UpdateUIBounds()
        {
            if (Screens != null)
            {
                SetPlayerArea();

                for (int i = 0; i < Screens.Length; i++)
                {
                    UserScreen screen = Screens[i];

                    RectangleF prevSb = screen.UIBounds;
                    screen.UIBounds = SetScreenArea(screen);
                    screen.SwapTypeBounds = new RectangleF(screen.UIBounds.X, screen.UIBounds.Y, screen.UIBounds.Width * 0.1f, screen.UIBounds.Width * 0.1f);

                    if (profile != null && profile.DevicesList != null)
                    {
                        foreach (PlayerInfo player in profile?.DevicesList)
                        {
                            if (player.ScreenIndex != -1 && player.ScreenIndex == i)
                            {
                                var datas = TranslateBounds(player.EditBounds, prevSb, screen.UIBounds);
                                player.EditBounds = datas;
                            }
                            else if (player.ScreenIndex == -1)
                            {
                                int playerIndex = profile.DevicesList.FindIndex(p => p == player);
                                player.EditBounds = GetDefaultBounds(playerIndex);
                            }
                        }

                        for(int j = 0; j< GameProfile.GhostBounds.Count;j++)
                        {
                            GameProfile.GhostBounds[j] = (GameProfile.GhostBounds[j].Item1,TranslateBounds(GameProfile.GhostBounds[j].Item2, prevSb, screen.UIBounds));
                        }
                    }
                }

                TotalBounds = RectangleUtil.Union(Screens);           
            }

          parent.Invalidate();
        }      

        internal static RectangleF TranslateBounds(RectangleF currentBounds, RectangleF prevSb, RectangleF newSb)
        {
            RectangleF ogEditBounds = currentBounds;

            Vector2 ogScruiLoc = new Vector2(prevSb.X, prevSb.Y);
            Vector2 ogpEb = new Vector2(ogEditBounds.X, ogEditBounds.Y);
            Vector2 ogOnUIScrLoc = Vector2.Subtract(ogpEb, ogScruiLoc);

            float ratioEW = (float)newSb.Width / (float)prevSb.Width;
            float ratioEH = (float)newSb.Height / (float)prevSb.Height;

            return new RectangleF(newSb.X + (ogOnUIScrLoc.X * ratioEW), newSb.Y + (ogOnUIScrLoc.Y * ratioEH), ogEditBounds.Width * ratioEW, ogEditBounds.Height * ratioEH);
        }

        public static RectangleF GetActiveSizer(MouseEventArgs e)
        {
            if (sizerBtnLeft.Contains(e.Location)) return sizerBtnLeft;
            if (sizerBtnRight.Contains(e.Location)) return sizerBtnRight;
            if (sizerBtnTop.Contains(e.Location)) return sizerBtnTop;
            if (sizerBtnBottom.Contains(e.Location)) return sizerBtnBottom;

            return RectangleF.Empty;
        }

        public static Rectangle BufferedSurface = Rectangle.Empty;

        public static void SetBufferSurface(RectangleF surface, float offset)
        {
            BufferedSurface = new Rectangle((int)(surface.X - (surface.Width / offset)), (int)(surface.Y - (surface.Height / offset)), (int)((surface.Width) + (surface.Width / offset) * 2), (int)((surface.Height) + (surface.Height / offset) * 2));
        }

        internal static void OnMouseDown(MouseEventArgs e)
        {
            List<PlayerInfo> players = profile.DevicesList;

            if (Dragging)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                activeSizer = GetActiveSizer(e);

                if (activeSizer != RectangleF.Empty && Screens.All(s => !s.SwapTypeBounds.Contains(mousePos)))
                {
                    Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                    return;
                }
                else
                {
                    Cursor.Current = Theme_Settings.Hand_Cursor; 
                }

                for (int i = 0; i < Screens.Length; i++)
                {
                    UserScreen screen = Screens[i];

                    if (screen.SwapTypeBounds.Contains(e.Location))
                    {
                        if (GameProfile.Loaded)
                        {
                            return;
                        }

                        if (screen.Type == UserScreenType.Manual)
                        {
                            screen.Type = 0;
                        }
                        else
                        {
                            screen.Type++;
                        }

                        ///invalidate all players inside screen
                        for (int j = 0; j < players.Count; j++)
                        {
                            ///return to default position
                            PlayerInfo p = players[j];

                            if (p.ScreenIndex == i)
                            {
                                RemovePlayer(p, i);
                            }
                        }
                       
                        ShowSwapTypeTip = false;
                        DevicesFunctions.UpdateDevices();
                        parent.Invalidate(false);
                        return;
                    }
                }

                for (int i = 0; i < players.Count; i++)
                {
                    PlayerInfo p = players[i];

                    RectangleF r = p.EditBounds;

                    if (r.Contains(e.Location))
                    {
                        if (!p.IsInputUsed)
                        {
                            return;
                        }

                        if (p.ScreenIndex != -1)
                        {
                            RemovePlayer(p, p.ScreenIndex);
                        }

                        Dragging = true;
                        draggingIndex = i;

                        draggingOffset = new PointF(r.X - e.X, r.Y - e.Y);

                        RectangleF newBounds = GetDefaultBounds(draggingIndex);

                        profile.DevicesList[draggingIndex].EditBounds = newBounds;

                        if (draggingOffset.X < -newBounds.Width ||
                            draggingOffset.Y < -newBounds.Height)
                        {
                            draggingOffset = new Point(0, 0);
                        }
     
                        break;
                    }
                }
            }
            else if (e.Button == MouseButtons.Right || e.Button == MouseButtons.Middle)
            {
                for (int i = 0; i < Screens.Length; i++)
                {
                    UserScreen screen = Screens[i];

                    PlayerInfo player = profile.DevicesList.Where(pl => pl.EditBounds.Contains(e.Location)).FirstOrDefault();

                    if (player != null)
                    {
                        var guest = player.InstanceGuests.Where(gst => gst.GuestBounds.Contains(e.Location)).FirstOrDefault();

                        if (guest != null)
                        {
                            ShowGuestRemovelText = null;
                            guest.GuestBounds = RectangleF.Empty;
                            guest.EditBounds = guest.SourceEditBounds;
                            player.InstanceGuests.Remove(guest);
                            return;
                        }
                    }

                    if (screen.SwapTypeBounds.Contains(e.Location))
                    {
                        if (GameProfile.Loaded)
                        {
                            return;
                        }
                       
                        if (screen.Type == UserScreenType.FullScreen)
                        {
                            screen.Type = UserScreenType.Manual;
                        }
                        else
                        {
                            screen.Type--;
                        }

                        ///invalidate all players inside screen
                        for (int j = 0; j < players.Count; j++)
                        {
                            ///return to default position
                            PlayerInfo p = players[j];

                            if (p.ScreenIndex == i)
                            {
                                RemovePlayer(p, p.ScreenIndex);
                            }
                        }

                        parent.Invalidate(false);
                        return;
                    }
                }

                if (GameProfile.Loaded)
                {
                    return;
                }

                ///if over a player on a screen, expand player bounds
                for (int i = 0; i < players.Count; i++)
                {
                    PlayerInfo p = players[i];

                    RectangleF r = p.EditBounds;
                    RectangleF pib = Rectangle.Empty;

                    PlayerInfo playerInbounds = players.Where(pl => pl != p && pl.EditBounds == r).FirstOrDefault();

                    var guest = p.InstanceGuests.Where(gst => gst.GuestBounds.Contains(e.Location)).FirstOrDefault();

                    if (guest != null)
                    {
                        ShowGuestRemovelText = null;
                        guest.GuestBounds = RectangleF.Empty;
                        guest.EditBounds = guest.SourceEditBounds;
                        p.InstanceGuests.Remove(guest);
                        break;
                    }

                    if (playerInbounds != null)
                    {
                        pib = playerInbounds.EditBounds;
                    }

                    if (r.Contains(e.Location) || pib.Contains(e.Location))
                    {
                        if (p.ScreenIndex != -1)
                        {
                            UserScreen screen = Screens[p.ScreenIndex];

                            if(screen.Type == UserScreenType.Manual)
                            {
                                return;
                            }

                            #region Reset the player bounds if the manual method has been used
                            var match = screen.SubScreensBounds.FirstOrDefault(sb => p.MonitorBounds == sb.Key);

                            if (!match.Equals(default(KeyValuePair<Rectangle, RectangleF>)))
                            {
                                //Exact match found, do nothing
                            }
                            else
                            {
                                var intersecting = screen.SubScreensBounds.FirstOrDefault(sb => p.MonitorBounds.IntersectsWith(sb.Key));

                                if (!intersecting.Equals(default(KeyValuePair<Rectangle, RectangleF>)))
                                {
                                    if (players.Any(pl => pl != p && pl != playerInbounds && pl.MonitorBounds.IntersectsWith(intersecting.Key)))
                                    {
                                        return;
                                    }

                                    p.MonitorBounds = intersecting.Key;
                                    p.EditBounds = intersecting.Value;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = intersecting.Key;
                                        playerInbounds.EditBounds = intersecting.Value;
                                    }
                                }

                                //If only one device on screen, prevent it to fill the screen(useless)
                                if (screen.PlayerOnScreen == 1 || ((p.IsRawKeyboard || p.IsRawMouse) && screen.PlayerOnScreen == 0))
                                {
                                    return;
                                }
                            }
                            #endregion

                            int verLines = 2;
                            int horLines = 2;

                            switch (screen.Type)
                            {
                                case UserScreenType.FourPlayers:
                                    {
                                        verLines = 2;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.SixPlayers:
                                    {
                                        verLines = 3;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.EightPlayers:
                                    {
                                        verLines = 4;
                                        horLines = 2;
                                    }
                                    break;
                                case UserScreenType.SixteenPlayers:
                                    {
                                        verLines = 4;
                                        horLines = 4;
                                    }
                                    break;
                                case UserScreenType.Custom:
                                    {
                                        horLines = GameProfile.CustomLayout_Hor + 1;
                                        verLines = GameProfile.CustomLayout_Ver + 1;
                                    }
                                    break;
                                case UserScreenType.Manual:
                                    return;
                            }

                            int halfWidth = screen.MonitorBounds.Width / verLines;
                            int halfHeight = screen.MonitorBounds.Height / horLines;
                            
                            float halfUIWidth = screen.UIBounds.Width / verLines;
                            float halfUIHeight = screen.UIBounds.Height / horLines;

                            int margin = 10;//is a margin for uneven division (2X3)

                            Rectangle bounds = p.MonitorBounds;
                            if ((int)screen.Type >= 3 &&(int)screen.Type != 8)
                            {
                                var filteredlistHor = players.Where(pl => pl != p && pl != playerInbounds && pl.ScreenIndex != -1 && pl.ScreenIndex == p.ScreenIndex && pl.EditBounds.Y == p.EditBounds.Y).ToList();
                                var filteredlistVer = players.Where(pl => pl != p && pl != playerInbounds && pl.ScreenIndex != -1 && pl.ScreenIndex == p.ScreenIndex && pl.EditBounds.X == p.EditBounds.X).ToList();
                                var regularlist = players.Where(pl => pl != p && pl != playerInbounds && pl.ScreenIndex != -1 && pl.ScreenIndex == p.ScreenIndex).ToList();
                                
                                ///check if the size is 1/4th of screen
                                if (bounds.Width == halfWidth && bounds.Height == halfHeight)
                                {
                                    bool hasLeftSpace = true;
                                    bool hasRightSpace = true;

                                    bool hasTopSpace = true;
                                    bool hasBottomSpace = true;

                                    #region Check right space
                                    if (filteredlistHor.Any(pl => (pl.MonitorBounds.Left == p.MonitorBounds.Right || p.MonitorBounds.Right + margin >= screen.MonitorBounds.Right)))
                                    {
                                        hasRightSpace = false;
                                    }

                                    if (hasRightSpace)
                                    {
                                        RectangleF edit = p.EditBounds;
                                        if (filteredlistHor.Count > 0)
                                        {
                                            while (!filteredlistHor.Any(pl => pl.MonitorBounds.Left == bounds.Right))
                                            {
                                                if (bounds.Right + margin >= screen.MonitorBounds.Right)
                                                {
                                                    break;
                                                }

                                                bounds.Width += halfWidth;
                                                edit.Width += halfUIWidth;                                                
                                            }
                                        }
                                        else
                                        {
                                            while (!(bounds.Right + margin >= screen.MonitorBounds.Right))//10 is a margin for uneven division (2X3)
                                            {
                                                if (bounds.Right >= screen.MonitorBounds.Right || regularlist.Any(pl => pl.MonitorBounds.Left == bounds.Right && pl.MonitorBounds.Y == bounds.Y))
                                                {
                                                    break;
                                                }

                                                bounds.Width += halfWidth;
                                                edit.Width += halfUIWidth;

                                                if (bounds.Right >= screen.MonitorBounds.Right || regularlist.Any(pl => pl.MonitorBounds.Left == bounds.Right))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        bool isFull = false;

                                        if (bounds.Width + margin >= screen.MonitorBounds.Width && bounds.Height + margin >= screen.MonitorBounds.Height)
                                        {
                                            isFull = true;
                                        }

                                        if (!regularlist.Any(pl => pl.MonitorBounds.IntersectsWith(bounds)) && (bounds.Width > 0 && bounds.Height > 0) && !isFull)
                                        {
                                            p.EditBounds = edit;
                                            p.MonitorBounds = bounds;

                                            if (playerInbounds != null)
                                            {
                                                playerInbounds.MonitorBounds = p.MonitorBounds;
                                                playerInbounds.EditBounds = p.EditBounds;
                                            }

                                            parent.Invalidate(false);
                                        }
                                    }
                                    #endregion

                                    bounds = p.MonitorBounds;

                                    #region Check left space
                                    if (filteredlistHor.Any(pl => pl.MonitorBounds.Right == p.MonitorBounds.Left) || p.MonitorBounds.Left - margin <= screen.MonitorBounds.Left)
                                    {
                                        hasLeftSpace = false;
                                    }

                                    if (hasLeftSpace)
                                    {
                                        RectangleF edit = p.EditBounds;

                                        if (filteredlistHor.Count > 0)
                                        {
                                            while (!filteredlistHor.Any(pl => pl.MonitorBounds.Right == bounds.Left || bounds.Left == screen.MonitorBounds.Left))
                                            {
                                                if (bounds.Left - margin <= screen.MonitorBounds.Left + margin)
                                                {
                                                    break;
                                                }

                                                bounds.Width += halfWidth;
                                                edit.Width += halfUIWidth;
                                                bounds.X -= halfWidth;
                                                edit.X -= halfUIWidth;
                                            }
                                        }
                                        else
                                        {
                                            while (!(bounds.Left - margin <= screen.MonitorBounds.Left))
                                            {
                                                if (bounds.Left  <= screen.MonitorBounds.Left  || regularlist.Any(pl => pl.MonitorBounds.Right == bounds.Left && pl.MonitorBounds.Y == bounds.Y))
                                                {
                                                    break;
                                                }

                                                bounds.Width += halfWidth;
                                                edit.Width += halfUIWidth;
                                                bounds.X -= halfWidth;
                                                edit.X -= halfUIWidth;

                                                if (bounds.Left <= screen.MonitorBounds.Left || regularlist.Any(pl => pl.MonitorBounds.Right == bounds.Left))
                                                { 
                                                    break;
                                                }
                                            }
                                        }

                                        bool isFull = false;

                                        if (bounds.Width + margin >= screen.MonitorBounds.Width && bounds.Height + margin >= screen.MonitorBounds.Height)
                                        {
                                            isFull = true;
                                        }

                                        if (!regularlist.Any(pl => pl.MonitorBounds.IntersectsWith(bounds)) && (bounds.Width > 0 && bounds.Height > 0) && !isFull)
                                        {
                                            p.EditBounds = edit;
                                            p.MonitorBounds = bounds;

                                            if (playerInbounds != null)
                                            {
                                                playerInbounds.MonitorBounds = p.MonitorBounds;
                                                playerInbounds.EditBounds = p.EditBounds;
                                            }

                                            parent.Invalidate(false);
                                        }
                                    }
                                    #endregion

                                    bounds = p.MonitorBounds;

                                    #region Check top space
                                    if (filteredlistVer.Any(pl => (pl.MonitorBounds.Bottom == p.MonitorBounds.Top ) || p.MonitorBounds.Top <= screen.MonitorBounds.Top))
                                    {
                                        hasTopSpace = false;
                                    }

                                    if (hasTopSpace)
                                    {
                                        RectangleF edit = p.EditBounds;
                                        if (filteredlistVer.Count > 0)
                                        {

                                            while (!filteredlistVer.Any(pl => pl.MonitorBounds.Bottom == bounds.Top))
                                            {
                                                if (bounds.Top == screen.MonitorBounds.Top)
                                                {
                                                    break;
                                                }

                                                bounds.Height += halfHeight;
                                                edit.Height += halfUIHeight;
                                                bounds.Y -= halfHeight;
                                                edit.Y -= halfUIHeight;
                                            }
                                        }
                                        else
                                        {
                                            while (!(bounds.Top == screen.MonitorBounds.Top))
                                            {
                                                if (bounds.Top == screen.MonitorBounds.Top || regularlist.Any(pl => pl.MonitorBounds.Bottom == bounds.Top && pl.MonitorBounds.X == bounds.X))
                                                {
                                                    break;
                                                }

                                                bounds.Y -= halfHeight;
                                                bounds.Height += halfHeight;

                                                edit.Y -= halfUIHeight;
                                                edit.Height += halfUIHeight;

                                                if (bounds.Top == screen.MonitorBounds.Top || regularlist.Any(pl => pl.MonitorBounds.Bottom == bounds.Top))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        bool isFull = false;

                                        if (bounds.Width + margin >= screen.MonitorBounds.Width && bounds.Height + margin >= screen.MonitorBounds.Height)
                                        {
                                            isFull = true;
                                        }

                                        if (!regularlist.Any(pl => pl.MonitorBounds.IntersectsWith(bounds)) && (bounds.Width > 0 && bounds.Height > 0) && !isFull)
                                        {
                                            p.EditBounds = edit;
                                            p.MonitorBounds = bounds;

                                            if (playerInbounds != null)
                                            {
                                                playerInbounds.MonitorBounds = p.MonitorBounds;
                                                playerInbounds.EditBounds = p.EditBounds;
                                            }

                                            parent.Invalidate(false);
                                        }
                                    }
                                    #endregion

                                    bounds = p.MonitorBounds;

                                    #region Check bottom space

                                    if (filteredlistVer.Any(pl => (pl.MonitorBounds.Top == p.MonitorBounds.Bottom) || p.MonitorBounds.Bottom >= screen.MonitorBounds.Bottom))
                                    {
                                        hasBottomSpace = false;
                                    }

                                    if (hasBottomSpace)
                                    {
                                        RectangleF edit = p.EditBounds;
                                        if (filteredlistVer.Count > 0)
                                        {
                                            while (!filteredlistVer.Any(pl => pl.MonitorBounds.Top == bounds.Bottom))
                                            {
                                                if (bounds.Bottom == screen.MonitorBounds.Bottom)
                                                {
                                                    break;
                                                }

                                                bounds.Height += halfHeight;
                                                edit.Height += halfUIHeight;                                                
                                            }
                                        }
                                        else
                                        {
                                            while (!(bounds.Bottom == screen.MonitorBounds.Bottom))
                                            {
                                                if (bounds.Bottom >= screen.MonitorBounds.Bottom || regularlist.Any(pl => pl.MonitorBounds.Top == bounds.Bottom && pl.MonitorBounds.X == bounds.X))
                                                {
                                                    break;
                                                }

                                                bounds.Height += halfHeight;
                                                edit.Height += halfUIHeight;

                                                if (bounds.Bottom >= screen.MonitorBounds.Bottom || regularlist.Any(pl => pl.MonitorBounds.Top == bounds.Bottom))
                                                {
                                                    break;
                                                }
                                            }
                                        }

                                        bool isFull = false;

                                        if (bounds.Width + margin >= screen.MonitorBounds.Width && bounds.Height + margin >= screen.MonitorBounds.Height)
                                        {
                                            isFull = true;
                                        }

                                        if (!regularlist.Any(pl => pl.MonitorBounds.IntersectsWith(bounds)) && (bounds.Width > 0 && bounds.Height > 0) && !isFull)
                                        {
                                            p.EditBounds = edit;
                                            p.MonitorBounds = bounds;

                                            if (playerInbounds != null)
                                            {
                                                playerInbounds.MonitorBounds = p.MonitorBounds;
                                                playerInbounds.EditBounds = p.EditBounds;
                                            }

                                            parent.Invalidate(false);
                                        }
                                    }
                                    #endregion

                                    bounds = p.MonitorBounds;
                                    break;
                                }
                                else
                                {
                                    bounds.Width = screen.MonitorBounds.Width / verLines;
                                    bounds.Height = screen.MonitorBounds.Height / horLines;
                                    p.MonitorBounds = bounds;

                                    //anyway
                                    if (p.MonitorBounds.Width <= 0 || p.MonitorBounds.Height <= 0)
                                    {
                                        RemovePlayer(p, p.ScreenIndex);
                                        break;
                                    }

                                    RectangleF edit = p.EditBounds;
                                    edit.Width = screen.UIBounds.Width / verLines;
                                    edit.Height = screen.UIBounds.Height / horLines;
                                    p.EditBounds = edit;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = p.MonitorBounds;
                                        playerInbounds.EditBounds = p.EditBounds;
                                    }

                                    parent.Invalidate(false);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void AddPlayer(PlayerInfo player, int screenIndex)
        {
            if ((player.IsRawKeyboard || player.IsRawMouse) && !(player.IsRawKeyboard && player.IsRawMouse))
            {
                if (GameProfile.AssignedDevices.Any(pl => pl.MonitorBounds == destMonitorBounds))
                {
                    GameProfile.DevicesToMerge.Add(player);
                }
                else
                {
                    GameProfile.AssignedDevices.Add(player);
                }

                if (profile.DevicesList.Where(pp => pp.MonitorBounds == destMonitorBounds).Count() == 1)
                {
                    Screens[screenIndex].PlayerOnScreen++;
                    GameProfile.TotalAssignedPlayers++;
                }
            }
            else
            {
                GameProfile.AssignedDevices.Add(player);
                Screens[screenIndex].PlayerOnScreen++;
                GameProfile.TotalAssignedPlayers++;
            }

            player.IsInputUsed = true;
            player.Owner = Screens[screenIndex];
            player.ScreenIndex = screenIndex;

            player.MonitorBounds = destMonitorBounds;
            player.EditBounds = DestEditBounds;

            player.ScreenPriority = Screens[screenIndex].priority;
            player.DisplayIndex = Screens[screenIndex].DisplayIndex;
        }

        internal static void RemovePlayer(PlayerInfo player, int screenIndex)
        {
            int playerIndex = profile.DevicesList.FindIndex(p => p == player);

            if ((player.IsRawKeyboard || player.IsRawMouse) && !(player.IsRawKeyboard && player.IsRawMouse))
            {
                if (GameProfile.AssignedDevices.Contains(player))
                {
                    GameProfile.AssignedDevices.Remove(player);

                    PlayerInfo secondInBounds = GameProfile.DevicesToMerge.Where(pl => pl.EditBounds == player.EditBounds && pl != player && pl.ScreenIndex != -1).FirstOrDefault();

                    if(secondInBounds != null)
                    {
                        GameProfile.AssignedDevices.Add(secondInBounds);
                        GameProfile.DevicesToMerge.Remove(secondInBounds);  
                    }
                }
                else if (GameProfile.DevicesToMerge.Contains(player))
                {
                    GameProfile.DevicesToMerge.Remove(player);
                }

                if (profile.DevicesList.Where(pp => pp.MonitorBounds == player.MonitorBounds).Count() == 2)
                {
                    Screens[screenIndex].PlayerOnScreen--;
                    GameProfile.TotalAssignedPlayers--;
                }
            }
            else
            {
                GameProfile.AssignedDevices.Remove(player);
                Screens[screenIndex].PlayerOnScreen--;
                GameProfile.TotalAssignedPlayers--;

                ClearPlayerGuests(player);

                parent.Invalidate(false);
            }

            if (player.ScreenIndex == screenIndex)
            {
                player.EditBounds = GetDefaultBounds(playerIndex);
                player.Owner = null;
                player.ScreenIndex = -1;
                player.MonitorBounds = Rectangle.Empty;
                player.DisplayIndex = -1;             
            }
        }

        private static void ClearPlayerGuests(PlayerInfo player)
        {
            foreach (PlayerInfo guest in player.InstanceGuests)
            {
                guest.GuestBounds = RectangleF.Empty;
                guest.EditBounds = guest.SourceEditBounds;
            }

            player.InstanceGuests.Clear();
        }

        internal static void OnMouseUp(MouseEventArgs e)
        {
            if (activeSizer == RectangleF.Empty)
            {
                parent.Cursor = Theme_Settings.Hand_Cursor;
            }

            if (e.Button == MouseButtons.Left)
            {
                if (Dragging)
                {
                    Dragging = false;

                    PlayerInfo p = profile.DevicesList[draggingIndex];

                    if (DraggingScreen != -1)
                    {
                        //for multiple player per instance
                        if(canAddGuest && instanceHost != null)
                        {                          
                            instanceHost.InstanceGuests.Add(p);
                            canAddGuest = false;
                            instanceHost = null;
                            parent.Invalidate(false);
                            DraggingScreen = -1;
                            return;
                        }

                        AddPlayer(p, DraggingScreen);
                        DraggingScreen = -1;
                    }
                    else
                    {
                        for (int i = 0; i < Screens.Length; i++)
                        {
                            if (p.ScreenIndex == i)
                            {
                                // return to default position
                                p.Owner = null;
                                p.EditBounds = GetDefaultBounds(draggingIndex);
                                p.MonitorBounds = Rectangle.Empty;
                                p.ScreenPriority = -1;
                                p.ScreenIndex = -1;
                            }
                        }
                    }

                    DevicesFunctions.UpdateDevices();
                    parent.Invalidate(false);
                }

                activeSizer = RectangleF.Empty;
            }

            UpdatetSizersBounds();
        }

        public static bool CanFillGap(Point mousePos,PlayerInfo player, UserScreen screen)
        {
            Rectangle container = screen.MonitorBounds;

            var toUnion = profile.DevicesList.Where(pl => pl != player && container.IntersectsWith(pl.MonitorBounds)).ToList();

            List<Rectangle> placedMB = new List<Rectangle>();
            List<RectangleF> placedEB = new List<RectangleF>();

            for (int j = 0; j < toUnion.Count; j++)
            {
                placedMB.Add(toUnion[j].MonitorBounds);
                placedEB.Add(toUnion[j].EditBounds);
            }

            List<Rectangle> freeSpacesMB = new List<Rectangle> { container };
            List<RectangleF> freeSpacesEB = new List<RectangleF> { screen.UIBounds};

            for (int i = 0; i < placedMB.Count; i++)
            {
                Rectangle pcmb = placedMB[i];

                if(i > placedEB.Count -1)
                {
                    break;
                }

                RectangleF pced = placedEB[i];

                List<Rectangle> newFreeSpacesMB = new List<Rectangle>();
                List<RectangleF> newFreeSpacesEB = new List<RectangleF> ();

                for(int j = 0; j < freeSpacesMB.Count;j++)
                {
                    Rectangle f = freeSpacesMB[j];

                    if(j > freeSpacesEB.Count - 1)
                    {
                        break;
                    }

                    RectangleF eb = freeSpacesEB[j];

                    if (eb.Contains(mousePos))
                    {
                        if (f.IntersectsWith(pcmb))
                        {
                            newFreeSpacesMB.AddRange(RectangleUtil.SubtractSurface(f, pcmb));
                            newFreeSpacesEB.AddRange(RectangleUtil.SubtractSurfaceF(eb, pced));
                        }
                        else
                        {
                            newFreeSpacesMB.Add(f);
                            newFreeSpacesEB.Add(eb);
                        }
                    }
                }

                freeSpacesMB = newFreeSpacesMB;
                freeSpacesEB = newFreeSpacesEB;
            }

            int newWidth = 5;
            int newHeight = 5;

            for(int i = 0; i < freeSpacesMB.Count;i++)
            {
                Rectangle free = freeSpacesMB[i];
                if(i > freeSpacesEB.Count - 1)
                {
                    break;
                }

                RectangleF freemb = freeSpacesEB[i];

                if (freemb.Contains(mousePos))
                {
                    if (newWidth <= free.Width && newHeight <= free.Height)
                    {
                        DestEditBounds = freemb;
                        destMonitorBounds = free;
                        player.EditBounds = freemb;
                        
                        //Console.WriteLine($"Can fit at: {free.X},{free.Y} in free area {free.Width}x{free.Height}"); parent.Invalidate();
                        return true;
                    }
                }
            }
           
            return false;
        }

        internal static void OnMouseMove(MouseEventArgs e)
        {
            mousePos = e.Location;

            if (Dragging)
            {
                canAddGuest = false;
                instanceHost = null;

                PlayerInfo player = profile.DevicesList[draggingIndex];

                if (!player.IsInputUsed)
                {
                    return;
                }

                player.MonitorBounds = Rectangle.Empty;

                SetBufferSurface(player.EditBounds, 2.5F);

                UserScreen screen = Screens.Where(scr => scr.UIBounds.Contains(e.Location)).FirstOrDefault();

                if (screen != null)
                {
                    RectangleF s = screen.UIBounds;

                    DraggingScreen = screen.Index;

                    #region UI support for multiple controller per instance 

                    bool hasFreeSpace = GetFreeSpace(player);

                    if (!hasFreeSpace)
                    {   //try to fill any gaps on the screen
                        if (CanFillGap(e.Location, player, screen))
                        {
                            bool skip = false;

                            if (GameProfile.Loaded && !GameProfile.GhostBounds.Any(gb => gb.Item1 == destMonitorBounds))
                            {
                                skip = true;
                            }

                            if (!skip)
                            {
                                parent.Invalidate();
                                return;
                            }
                        }

                        //Gamepads only
                        if (player.IsController && userGameInfo.Game.PlayersPerInstance > 0)
                        {
                            var playersInDiv = GameProfile.AssignedDevices.Where(pl => (pl != player) && pl.EditBounds.Contains(e.Location) && pl.IsController).ToList();

                            if (playersInDiv.Count == 1)
                            {
                                PlayerInfo instanceOwner = playersInDiv[0];

                                if (instanceOwner.MonitorBounds.IntersectsWith(destMonitorBounds))
                                {
                                    if (instanceOwner.IsController)
                                    {
                                        var instanceOwnerGuests = instanceOwner.InstanceGuests.Where(ig => ig.GamepadId != -1).ToList();
                                        if (!instanceOwner.InstanceGuests.Contains(player) &&
                                            !GameProfile.AssignedDevices.Any(pl => pl.InstanceGuests.Contains(player)) &&
                                            instanceOwnerGuests.Count < userGameInfo.Game.PlayersPerInstance - 1 /*&&*/
                                            /*!( GameProfile.AssignedDevices.Count == userGameInfo.Game.MaxPlayers)*/)
                                        {
                                            canAddGuest = true;
                                            instanceHost = instanceOwner;
                                            DestEditBounds = instanceOwner.EditBounds;
                                        }
                                    }
                                }
                            }
                        }

                        //multiple kbm
                        if ((player.IsRawKeyboard || player.IsRawMouse))
                        {
                            var playersInDiv = profile.DevicesList.Where(pl => pl != player && pl.EditBounds.Contains(e.Location)).ToList();

                            if (playersInDiv.Count == 1)
                            {
                                PlayerInfo instanceOwner = playersInDiv[0];

                                if ((instanceOwner.IsRawKeyboard && player.IsRawMouse) || (instanceOwner.IsRawMouse && player.IsRawKeyboard))
                                {
                                    DestEditBounds = instanceOwner.EditBounds;
                                    destMonitorBounds = instanceOwner.MonitorBounds;
                                    AddPlayer(player, instanceOwner.ScreenIndex);
                                    parent.Invalidate();
                                    return;
                                }
                            }
                        }
                        #endregion
                    }

                    if (player.EditBounds.IntersectsWith(s) && DestEditBounds == RectangleF.Empty)
                    {
                        SetBufferSurface(s, 20.0F);
                    }
                    else
                    {
                        SetBufferSurface(player.EditBounds, 2.5F);
                    }
                   
                    if (!canAddGuest)
                    {
                        if (!GetFreeSpace(player))
                        {
                            DraggingScreen = -1;
                        }
                        else
                        {
                            player.EditBounds = DestEditBounds;
                            parent.Invalidate();
                            return;
                        }
                    }       
                }
                else
                {
                    DraggingScreen = -1;
                    DestEditBounds = RectangleF.Empty;
                }

                RectangleF p = new RectangleF(mousePos.X - (player.SourceEditBounds.Width / 2), mousePos.Y - (player.SourceEditBounds.Height / 2), player.SourceEditBounds.Width, player.SourceEditBounds.Height);
                player.EditBounds = p;
              
                parent.Invalidate(BufferedSurface);
            }
            else
            {
                BufferedSurface = Rectangle.Empty;

                SelectedPlayer = profile.DevicesList.Where(pl => pl.ScreenIndex != -1 && pl.EditBounds.Contains(e.Location)).FirstOrDefault();

                if (SelectedPlayer != null)
                {
                    int maxPlayers = Screens[SelectedPlayer.ScreenIndex].SubScreensBounds.Count();

                    var guest = SelectedPlayer.InstanceGuests.Where(gst => gst.GuestBounds.Contains(e.Location)).FirstOrDefault();

                    if(guest != null)
                    {
                        ShowGuestRemovelText = guest;
                        parent.Cursor = Theme_Settings.Hand_Cursor;
                    }
                    else 
                    {
                        ShowGuestRemovelText = null;
                    }

                    if (maxPlayers >= 4)
                    {
                        if (!GameProfile.Loaded)
                        {
                            sizer = SelectedPlayer.EditBounds;
                            UpdatetSizersBounds();
                      
                            if (activeSizer != RectangleF.Empty)
                            {
                                SetCursor(e.Location);
                                EditPlayerBounds(e);
                            }
                            else if (guest == null)
                            {
                                SetCursor(e.Location);
                            }
                        }
                        else
                        {
                            sizer = RectangleF.Empty;
                        }

                        return;
                    }
                    else
                    {
                        sizer = RectangleF.Empty;
                        return;
                    }
                }
                else
                {
                    sizer = RectangleF.Empty;
                    ShowGuestRemovelText = null;
                }

                bool isInSwapBounds = IsCursorInSwapBounds();

                if (isInSwapBounds)
                {
                    parent.Cursor = Theme_Settings.Hand_Cursor;
                }
                else if (parent.Cursor != Theme_Settings.Default_Cursor && !isInSwapBounds)
                {
                    parent.Cursor = Theme_Settings.Default_Cursor;
                }
            }
        }

        internal static bool IsCursorInSwapBounds()
        {
            var cursorInSwapType = Screens?.Where(scr => scr.SwapTypeBounds.Contains(MousePos)).FirstOrDefault();
            
            if (cursorInSwapType != null)
            {
                return true;
            }

            return false;
        }

        internal static void SetCursor(Point cursorLoc)
        {
            if (IsCursorInSwapBounds() || sizerBtnCenter.Contains(MousePos))
            {
                parent.Cursor = Theme_Settings.Hand_Cursor;
                return;
            }

            if (sizerBtnLeft.Contains(cursorLoc) || sizerBtnRight.Contains(cursorLoc))
            {
                parent.Cursor = Cursors.SizeWE;
            }
            else if (sizerBtnTop.Contains(cursorLoc) || sizerBtnBottom.Contains(cursorLoc))
            {
                parent.Cursor = Cursors.SizeNS;
            }
            else 
            {
                parent.Cursor = Theme_Settings.Default_Cursor;
            }
        }

        internal static bool GetFreeSpace(PlayerInfo player)
        {
            if (DraggingScreen == -1)
            {
                return false;
            }

            UserScreen screen = Screens[DraggingScreen];
            RectangleF s = screen.UIBounds;

            destMonitorBounds = screen.SubScreensBounds.Where(b => b.Value.Contains(mousePos)).FirstOrDefault().Key;
            DestEditBounds = screen.SubScreensBounds.Where(b => b.Value.Contains(mousePos)).FirstOrDefault().Value;
         
            if (screen.Type == UserScreenType.Manual)
            {
                destMonitorBounds = new Rectangle(destMonitorBounds.X, destMonitorBounds.Y, screen.MonitorBounds.Width / screen.ManualTypeDefautScale, screen.MonitorBounds.Height / screen.ManualTypeDefautScale);
                DestEditBounds = new RectangleF(DestEditBounds.X, DestEditBounds.Y, screen.UIBounds.Width / (float)screen.ManualTypeDefautScale, screen.UIBounds.Height / (float)screen.ManualTypeDefautScale);

                if (DestEditBounds.Bottom > s.Bottom || DestEditBounds.Right > s.Right)
                {
                    return false;
                }
            }

            var playersInDiv = profile.DevicesList.Where(pl => (pl != player) && pl.MonitorBounds.IntersectsWith(destMonitorBounds)).ToList();


            if (player.IsRawMouse && !(player.IsRawMouse && player.IsRawKeyboard) ? playersInDiv.All(x => x.IsRawKeyboard && !x.IsRawMouse) :
                player.IsRawKeyboard && !(player.IsRawMouse && player.IsRawKeyboard) ? playersInDiv.All(x => x.IsRawMouse && !x.IsRawKeyboard) :
                !playersInDiv.Any())
            {
                
                if (GameProfile.Loaded)
                {
                    if (GameProfile.ProfilePlayersList.Count == 0)
                    {
                        return false;
                    }

                    //Check if this bounds are the bounds of a profile player. 
                    PlayerInfo profilePlayer = GameProfile.ProfilePlayersList.Where(ppl => GameProfile.TranslateBounds(ppl, GameProfile.FindScreenOrAlternative(ppl).Item1).Item1.IntersectsWith(destMonitorBounds)).FirstOrDefault();

                    if (GameProfile.TotalAssignedPlayers == GameProfile.TotalProfilePlayers || profilePlayer == null)
                    {
                        return false;
                    }

                    var translatedBounds = GameProfile.TranslateBounds(profilePlayer, GameProfile.FindScreenOrAlternative(profilePlayer).Item1);

                    destMonitorBounds = translatedBounds.Item1;
                    DestEditBounds = translatedBounds.Item2;

                    return true;
                }

                if (playersInDiv.Count() > 0)
                {
                    destMonitorBounds = playersInDiv.First().MonitorBounds;
                    DestEditBounds = playersInDiv.First().EditBounds;
                }

                return true;
            }

            return false;
        }

        internal static void UpdatetSizersBounds()
        {
            sizerBtnLeft = new RectangleF(sizer.Left, sizer.Top + (sizer.Height / 3), sizer.Width / 5, sizer.Height / 3);
            sizerBtnRight = new RectangleF(sizer.Right - sizer.Width / 5, sizer.Top + (sizer.Height / 3), sizer.Width / 5, sizer.Height / 3);
            sizerBtnTop = new RectangleF(sizer.Left + (sizer.Width / 3), sizer.Top, (sizer.Width / 3), (sizer.Height / 5));
            sizerBtnBottom = new RectangleF(sizer.Left + (sizer.Width / 3), sizer.Bottom - (sizer.Height / 5), (sizer.Width / 3), (sizer.Height / 5));
            sizerBtnCenter = new RectangleF(sizerBtnLeft.Right, sizerBtnTop.Bottom, sizerBtnRight.Left - sizerBtnLeft.Right, sizerBtnBottom.Top - sizerBtnTop.Bottom);
        }

        internal static void EditPlayerBounds(MouseEventArgs e)
        {
            if (SelectedPlayer != null && !GameProfile.Loaded)
            {
                List<PlayerInfo> players = profile.DevicesList;

                PlayerInfo p = SelectedPlayer;

                ShowGuestRemovelText = null;
                
                if (p.ScreenIndex != -1)
                {
                    UserScreen screen = Screens[p.ScreenIndex];

                    bool isManual = screen.Type == UserScreenType.Manual;

                    Rectangle pmb = p.MonitorBounds;
                    RectangleF peb = p.EditBounds;
                    RectangleF pmbToCompare = pmb;

                    int sensitivity = isManual ? 1 : 20;
                    int maxMinFactor = 1;//so they can't be very small

                    switch (screen.Type)
                    {
                        case UserScreenType.FourPlayers:
                            maxMinFactor = 6;
                            break;

                        case UserScreenType.SixPlayers:
                            maxMinFactor = 8;
                            break;

                        case UserScreenType.EightPlayers:
                            maxMinFactor = 10;
                            break;

                        case UserScreenType.SixteenPlayers:
                        case UserScreenType.Custom:
                            maxMinFactor = 12;
                            break;
                    }

                    Size mSubScreen = new Size(screen.SubScreensBounds.ElementAt(0).Key.Width/ sensitivity, screen.SubScreensBounds.ElementAt(0).Key.Height/ sensitivity);
                    SizeF eSubScreen = new SizeF(screen.SubScreensBounds.ElementAt(0).Value.Width/sensitivity, screen.SubScreensBounds.ElementAt(0).Value.Height/ sensitivity);

                    PlayerInfo playerInbounds = players.Where(pl => (pl != p) && pl.MonitorBounds == pmb).FirstOrDefault();

                    float offset = 2;
                    
                    if (activeSizer == sizerBtnLeft)
                    {
                        if (e.Location.X <= (activeSizer.Left + (activeSizer.Width / 2)) - offset)
                        {
                            if (pmb.Left != screen.MonitorBounds.Left && players.Where(pl => (pl != p) && pl.MonitorBounds.Right == pmb.Left && pl.MonitorBounds.Y == pmb.Y && pl != playerInbounds).Count() == 0)
                            {
                                PlayerInfo other = players.Where(pl => (pl != p && pl.ScreenIndex != -1) && pl.MonitorBounds != p.MonitorBounds && pl.MonitorBounds.Right < pmb.Left && pl.MonitorBounds.Y == pmb.Y && pl != playerInbounds).FirstOrDefault();

                                pmb.Width += mSubScreen.Width;
                                peb.Width += eSubScreen.Width;

                                pmb.Location = new Point(pmb.X - mSubScreen.Width, pmb.Y);
                                peb.Location = new PointF(peb.X - eSubScreen.Width, peb.Y);

                                int mboundsLimit = other == null ? screen.MonitorBounds.X : other.MonitorBounds.Right;
                                float eboundsLimit = other == null ? screen.UIBounds.X : other.EditBounds.Right;

                                if (pmb.Left >= mboundsLimit && players.Where(pl => (pl != p) && pl.MonitorBounds.IntersectsWith(pmb) && pl != playerInbounds).Count() == 0)
                                {
                                    if ((screen.MonitorBounds.Right - pmb.Right) < mSubScreen.Width && Math.Abs(mboundsLimit - pmb.Left) < mSubScreen.Width && other == null)
                                    {
                                        pmb.Width = screen.MonitorBounds.Width;
                                        pmb.X = screen.MonitorBounds.X;
                                    }

                                    p.MonitorBounds = pmb;
                                    p.EditBounds = peb;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = pmb;
                                        playerInbounds.EditBounds = peb;
                                    }

                                    sizer = p.EditBounds;
                                    UpdatetSizersBounds();
                                    activeSizer = sizerBtnLeft;
                                    Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                                }
                            }

                        }
                        else if (e.Location.X > (activeSizer.Right - (activeSizer.Width / 2)) + offset)
                        {
                            if (pmb.Width >= mSubScreen.Width)
                            {
                                pmb.Width -= mSubScreen.Width;
                                peb.Width -= eSubScreen.Width;

                                if (isManual)
                                {
                                    if (pmb.Width < screen.MonitorBounds.Width / screen.ManualTypeDefautScale)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (pmb.Width < mSubScreen.Width * maxMinFactor)
                                    {
                                        return;
                                    }
                                }

                                pmb.Location = new Point(pmb.X + mSubScreen.Width, pmb.Y);
                                peb.Location = new PointF(peb.X + eSubScreen.Width, peb.Y);

                                p.MonitorBounds = pmb;
                                p.EditBounds = peb;

                                if (playerInbounds != null)
                                {
                                    playerInbounds.MonitorBounds = pmb;
                                    playerInbounds.EditBounds = peb;
                                }

                                sizer = p.EditBounds;
                                UpdatetSizersBounds();
                                activeSizer = sizerBtnLeft;
                                Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                            }
                        }
                    }
                    else if (activeSizer == sizerBtnRight)
                    {
                        if (e.Location.X > (activeSizer.Right - (activeSizer.Width / 2)) + offset)
                        {
                            if (pmb.Right != screen.MonitorBounds.Right && players.Where(pl => (pl != p) && pl.MonitorBounds.Left == pmb.Right && pl.MonitorBounds.Y == pmb.Y && pl != playerInbounds).Count() == 0)
                            {
                                PlayerInfo other = players.Where(pl => (pl != p && pl.ScreenIndex != -1) && pl.MonitorBounds != p.MonitorBounds && pl.MonitorBounds.Left > pmb.Right && pl.MonitorBounds.Y == pmb.Y && pl != playerInbounds).FirstOrDefault();

                                pmb.Width += mSubScreen.Width;
                                peb.Width += eSubScreen.Width;

                                int mboundsLimit = other == null ? screen.MonitorBounds.Right : other.MonitorBounds.Left;
                                float eboundsLimit = other == null ? screen.UIBounds.Right : other.EditBounds.Left;

                                if (pmb.Right <= mboundsLimit && players.Where(pl => (pl != p) && pl.MonitorBounds.IntersectsWith(pmb) && pl != playerInbounds).Count() == 0)
                                {
                                    if (pmb.Left == screen.MonitorBounds.Left && Math.Abs(mboundsLimit - pmb.Right) < mSubScreen.Width && other == null)
                                    {
                                        pmb.Width = screen.MonitorBounds.Width;
                                        pmb.X = screen.MonitorBounds.X;
                                    }

                                    p.MonitorBounds = pmb;
                                    p.EditBounds = peb;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = pmb;
                                        playerInbounds.EditBounds = peb;
                                    }

                                    sizer = p.EditBounds;
                                    UpdatetSizersBounds();
                                    activeSizer = sizerBtnRight;
                                    Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                                }
                            }
                        }
                        else if (e.Location.X <= (activeSizer.Left + (activeSizer.Width / 2)) - offset)
                        {
                            if (pmb.Width > mSubScreen.Width)
                            {
                                pmb.Width -= mSubScreen.Width;
                                peb.Width -= eSubScreen.Width;

                                if (isManual)
                                {
                                    if (pmb.Width < screen.MonitorBounds.Width / screen.ManualTypeDefautScale)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (pmb.Width < mSubScreen.Width * maxMinFactor)
                                    {
                                        return;
                                    }
                                }

                                p.MonitorBounds = pmb;
                                p.EditBounds = peb;

                                if (playerInbounds != null)
                                {
                                    playerInbounds.MonitorBounds = pmb;
                                    playerInbounds.EditBounds = peb;
                                }

                                sizer = p.EditBounds;
                                UpdatetSizersBounds();
                                activeSizer = sizerBtnRight;
                                Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                            }
                        }
                    }
                    else if (activeSizer == sizerBtnTop)
                    {
                        if (e.Location.Y <= (activeSizer.Top + (activeSizer.Height / 2)) - offset)
                        {
                            if (pmb.Top != screen.MonitorBounds.Top && players.Where(pl => (pl != p) && pl.MonitorBounds.Bottom == pmb.Top && pl.MonitorBounds.X == pmb.X && pl != playerInbounds).Count() == 0)
                            {
                                PlayerInfo other = players.Where(pl => (pl != p && pl.ScreenIndex != -1) && pl.MonitorBounds != p.MonitorBounds && pl.MonitorBounds.Bottom > pmb.Top && pl.MonitorBounds.X == pmb.X && pl.MonitorBounds.Top != pmb.Bottom && pl != playerInbounds).FirstOrDefault();

                                pmb.Height += mSubScreen.Height;
                                peb.Height += eSubScreen.Height;

                                pmb.Location = new Point(pmb.X, pmb.Y - mSubScreen.Height);
                                peb.Location = new PointF(peb.X, peb.Y - eSubScreen.Height);

                                int mboundsLimit = other == null ? screen.MonitorBounds.Top : other.MonitorBounds.Bottom;
                                float eboundsLimit = other == null ? screen.UIBounds.Top : other.EditBounds.Bottom;

                                if (Math.Abs(p.MonitorBounds.Top) != Math.Abs(mboundsLimit) && players.Where(pl => (pl != p) && pl.MonitorBounds.IntersectsWith(pmb) && pl != playerInbounds).Count() == 0)
                                {
                                    if ((Math.Abs(screen.MonitorBounds.Height) - Math.Abs(pmb.Height)) < mSubScreen.Height && Math.Abs(screen.MonitorBounds.Top - pmb.Top) < mSubScreen.Height && other == null)
                                    {
                                        pmb.Height = screen.MonitorBounds.Height;
                                        pmb.Y = screen.MonitorBounds.Y;
                                    }

                                    p.MonitorBounds = pmb;
                                    p.EditBounds = peb;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = pmb;
                                        playerInbounds.EditBounds = peb;
                                    }
                                }

                                sizer = p.EditBounds;
                                UpdatetSizersBounds();
                                activeSizer = sizerBtnTop;
                                Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                            }
                        }
                        else if (e.Location.Y >= (activeSizer.Top + (activeSizer.Height / 2)) + offset)
                        {
                            if (pmb.Height > mSubScreen.Height)
                            {
                                pmb.Height -= mSubScreen.Height;
                                peb.Height -= eSubScreen.Height;

                                if (isManual)
                                {
                                    if (pmb.Height < screen.MonitorBounds.Height / screen.ManualTypeDefautScale)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (pmb.Height < mSubScreen.Height * maxMinFactor)
                                    {
                                        return;
                                    }
                                }

                                pmb.Location = new Point(pmb.X, pmb.Y + mSubScreen.Height);
                                peb.Location = new PointF(peb.X, peb.Y + eSubScreen.Height);

                                p.MonitorBounds = pmb;
                                p.EditBounds = peb;

                                if (playerInbounds != null)
                                {
                                    playerInbounds.MonitorBounds = pmb;
                                    playerInbounds.EditBounds = peb;
                                }

                                sizer = p.EditBounds;
                                UpdatetSizersBounds();
                                activeSizer = sizerBtnTop;
                                Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                            }
                        }
                    }
                    else if (activeSizer == sizerBtnBottom)
                    {
                        if (e.Location.Y >= (activeSizer.Top + (activeSizer.Height / 2)) + offset)
                        {
                            if (pmb.Bottom <= screen.MonitorBounds.Bottom && players.Where(pl => (pl != p) && pl.MonitorBounds.Top == pmb.Bottom && pl.MonitorBounds.X == pmb.X && pl != playerInbounds).Count() == 0)
                            {
                                PlayerInfo other = players.Where(pl => (pl != p && pl.ScreenIndex != -1) && pl.MonitorBounds != p.MonitorBounds && pl.MonitorBounds.Top > pmb.Bottom && pl.MonitorBounds.X == pmb.X && pl.MonitorBounds.Bottom != pmb.Bottom && pl != playerInbounds).FirstOrDefault();

                                pmb.Height += mSubScreen.Height;
                                peb.Height += eSubScreen.Height;

                                int mboundsLimit = other == null ? screen.MonitorBounds.Bottom : other.MonitorBounds.Top;
                                float eboundsLimit = other == null ? screen.UIBounds.Bottom : other.EditBounds.Top;

                                if (pmb.Bottom <= mboundsLimit && players.Where(pl => (pl != p) && pl.MonitorBounds.IntersectsWith(pmb) && pl != playerInbounds).Count() == 0)
                                {
                                    if (pmb.Top == screen.MonitorBounds.Y && Math.Abs(mboundsLimit - pmb.Bottom) < mSubScreen.Height && other == null)
                                    {
                                        pmb.Height = screen.MonitorBounds.Height;
                                        pmb.Y = screen.MonitorBounds.Y;
                                    }

                                    p.MonitorBounds = pmb;
                                    p.EditBounds = peb;

                                    if (playerInbounds != null)
                                    {
                                        playerInbounds.MonitorBounds = pmb;
                                        playerInbounds.EditBounds = peb;
                                    }

                                    sizer = p.EditBounds;
                                    UpdatetSizersBounds();
                                    activeSizer = sizerBtnBottom;
                                    Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                                }
                            }
                        }
                        else if (e.Location.Y <= (activeSizer.Top + (activeSizer.Height / 2)) - offset)
                        {
                            if (pmb.Height > mSubScreen.Height)
                            {
                                pmb.Height -= mSubScreen.Height;
                                peb.Height -= eSubScreen.Height;

                                if (isManual)
                                {
                                    if (pmb.Height < screen.MonitorBounds.Height / screen.ManualTypeDefautScale)
                                    {
                                        return;
                                    }
                                }
                                else
                                {
                                    if (pmb.Height < mSubScreen.Height * maxMinFactor)
                                    {
                                        return;
                                    }
                                }

                                p.MonitorBounds = pmb;
                                p.EditBounds = peb;

                                if (playerInbounds != null)
                                {
                                    playerInbounds.MonitorBounds = pmb;
                                    playerInbounds.EditBounds = peb;
                                }

                                sizer = p.EditBounds;
                                UpdatetSizersBounds();
                                activeSizer = sizerBtnBottom;
                                Cursor.Position = parent.PointToScreen(new Point((int)(parent.Location.X + activeSizer.X + activeSizer.Width / 2), (int)(parent.Location.Y + activeSizer.Y + activeSizer.Height / 2)));
                            }
                        }
                    }

                    if(p?.MonitorBounds.Width <= 0 || p?.MonitorBounds.Height <= 0)
                    {
                        RemovePlayer(p, p.ScreenIndex);                      
                    }

                    SetBufferSurface(p.EditBounds,3.0F);

                    parent.Invalidate(BufferedSurface);
                }
            }
        }

        internal static void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (Dragging || GameProfile.Loaded)
            {
                return;
            }

            if (e.Button == MouseButtons.Left && GameProfile.UseXinputIndex)
            {
                bool changed = false;

                for (int i = 0; i < Screens.Length; i++)
                {
                    UserScreen screen = Screens[i];
                    DraggingScreen = i;

                    if (screen.Type == UserScreenType.Manual)
                    {
                        return;
                    }

                    List<PlayerInfo> players = profile.DevicesList;

                    if (Screens.Any(s => s.UIBounds.Contains( e.Location)) && 
                       !Screens.Any(s => s.SwapTypeBounds.Contains(e.Location)) && 
                       !players.Any(pl => pl.EditBounds.Contains(e.Location)))
                    {
                       
                        for (int b = 0; b < screen.SubScreensBounds.Count; b++)
                        {
                            DestEditBounds = screen.SubScreensBounds.ElementAt(b).Value;
                            destMonitorBounds = screen.SubScreensBounds.ElementAt(b).Key;
                            // add all possible players!
                            for (int p = 0; p < players.Count; p++)
                            {
                                PlayerInfo player = players[p];

                                if(GameProfile.AssignedDevices.Contains(player) && GameProfile.DevicesToMerge.Contains(player))
                                {
                                    continue;
                                }

                                if (player.ScreenIndex == -1 && 
                                    (players.All(pl => (pl.MonitorBounds != destMonitorBounds && !pl.MonitorBounds.IntersectsWith(destMonitorBounds) )
                                    || (pl.MonitorBounds == destMonitorBounds) &&  (pl.IsRawMouse && player.IsRawKeyboard || pl.IsRawKeyboard && player.IsRawMouse))))
                                {
                                    player.IsInputUsed = true;
                                    changed = true;

                                    AddPlayer(player, i);
                                    p++;
                                }
                            }
                        }
                    }
                }

                DraggingScreen = -1;

                if (changed)
                {
                    DevicesFunctions.UpdateDevices();
                }
            }
        }

        public static void RefreshScreens()
        {
            Screens = null;
            UpdateScreens();

            for (int i = 0; i < Screens?.Length; i++)
            {
                UserScreen s = Screens[i];
                s.PlayerOnScreen = 0;
            }
        }
    }
}
