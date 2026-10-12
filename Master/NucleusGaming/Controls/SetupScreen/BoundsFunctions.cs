using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    public static class BoundsFunctions
    {
        /// <summary>
        /// Initializes the screens for the setup service: the layout math runs on a virtual surface of
        /// <see cref="SurfaceSize"/> pixels (EditBounds/UIBounds stay consistent for profiles and keyboard/mouse pairing).
        /// Call on the thread that owns the setup state, after the profile is initialized; <see cref="DevicesFunctions.Dispatch"/>
        /// must be set.
        /// </summary>
        public static void Initialize(UserGameInfo _userGameInfo, GameProfile _profile)
        {
            if (DevicesFunctions.Dispatch == null)
            {
                throw new InvalidOperationException("DevicesFunctions.Dispatch must be set before the setup screens are initialized.");
            }

            userGameInfo = _userGameInfo;
            profile = _profile;
            IsInitialized = true;

            //Runs each time a setup is opened: never leak the previous watcher.
            screensWatcherTimer?.Dispose();
            screensWatcherTimer = new System.Threading.Timer(ScreensWatcher_Tick, null, 0, 3000);

            SetPlayerArea();
            RefreshScreens();
        }

        /// <summary>Stops the screens watcher (setup closed).</summary>
        public static void StopScreensWatcher()
        {
            screensWatcherTimer?.Dispose();
            screensWatcherTimer = null;
        }

        /// <summary>True once the screens were initialized by <see cref="Initialize"/>.</summary>
        public static bool IsInitialized { get; private set; }

        /// <summary>Size of the virtual setup surface the layout math runs on.</summary>
        public static Size SurfaceSize = new Size(1280, 720);

        private static System.Threading.Timer screensWatcherTimer;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;

        public static UserScreen[] Screens;

        internal static RectangleF PlayersArea;
        public static Rectangle TotalBounds;

        private static RectangleF DestEditBounds { get; set; }

        private static Rectangle destMonitorBounds;

        private static int screensTickQueued;

        public static void ScreensWatcher_Tick(object state)
        {
            //During a session the launch uses the setup objects (the handler's player list is GameProfile.AssignedDevices):
            //a reset here would empty it.
            if (GameProfile.I_GameHandler != null && !GameProfile.I_GameHandler.HasEnded)
            {
                return;
            }

            Action<Action> dispatch = DevicesFunctions.Dispatch;

            //The screens and the device list are changed on the dispatcher only. One check queued at most.
            if (dispatch != null && System.Threading.Interlocked.Exchange(ref screensTickQueued, 1) == 0)
            {
                dispatch(() =>
                {
                    System.Threading.Interlocked.Exchange(ref screensTickQueued, 0);

                    if (screensWatcherTimer != null && (GameProfile.I_GameHandler == null || GameProfile.I_GameHandler.HasEnded))
                    {
                        ScreensWatcherCheck();
                    }
                });
            }
        }

        /// <summary>Raised after the monitors changed and the setup was reset (on the dispatcher).</summary>
        public static event Action ScreensChanged;

        private static void ScreensWatcherCheck()
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

                    ScreensChanged?.Invoke();
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
                float playersWidth = SurfaceSize.Width * 0.70f;

                float playerCount = playerData.Count;
                float playerWidth = playersWidth * 0.9f;
                float playerHeight = SurfaceSize.Height * 0.2f;

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
            Rectangle freeSpace = new Rectangle(0, (int)PlayersArea.Height - 5, SurfaceSize.Width - 5, (int)(SurfaceSize.Height - PlayersArea.Height) - 5);

            if(Screens.Length == 1)
            {
                freeSpace = new Rectangle(10, (int)PlayersArea.Height - 30, (int)(SurfaceSize.Width / 1.38f), (int)(SurfaceSize.Height - PlayersArea.Height) - 10);
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
                        player.EditBounds = GetDefaultBounds(i);
                        player.ScreenIndex = -1;
                        player.DisplayIndex = -1;
                    }
                }
            }

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];
                screen.UIBounds = SetScreenArea(screen);
                screen.Index = i;
            }
        }

        /// <summary>
        /// Expands a placed player to the free neighbour cells, or shrinks it back to one cell.
        /// </summary>
        private static void ToggleExpandCore(PlayerInfo p, PlayerInfo playerInbounds, List<PlayerInfo> players)
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
                            }
                        }
                        #endregion

                        bounds = p.MonitorBounds;
                        return;
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
                            return;
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
                    }
                }
            }
        }

        internal static void AddPlayer(PlayerInfo player, int screenIndex)
        {
            //Idempotent: a device already in the lists only gets its new placement below.
            bool alreadyAdded = GameProfile.AssignedDevices.Contains(player) || GameProfile.DevicesToMerge.Contains(player);

            if (alreadyAdded)
            {
                //only update the placement below
            }
            else if ((player.IsRawKeyboard || player.IsRawMouse) && !(player.IsRawKeyboard && player.IsRawMouse))
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
                guest.EditBounds = guest.SourceEditBounds;
            }

            player.InstanceGuests.Clear();
        }

        private static bool CanFillGap(Point point, PlayerInfo player, UserScreen screen)
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

                    if (eb.Contains(point))
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

                if (freemb.Contains(point))
                {
                    if (newWidth <= free.Width && newHeight <= free.Height)
                    {
                        DestEditBounds = freemb;
                        destMonitorBounds = free;
                        player.EditBounds = freemb;
                        
                        return true;
                    }
                }
            }
           
            return false;
        }

        /// <summary>Takes the slot of <paramref name="screen"/> under <paramref name="point"/> as the destination and checks it.</summary>
        private static bool GetFreeSpace(PlayerInfo player, UserScreen screen, Point point)
        {
            destMonitorBounds = screen.SubScreensBounds.Where(b => b.Value.Contains(point)).FirstOrDefault().Key;
            DestEditBounds = screen.SubScreensBounds.Where(b => b.Value.Contains(point)).FirstOrDefault().Value;

            return CheckFreeSpace(player, screen);
        }

        /// <summary>
        /// Checks the destination already set in destMonitorBounds/DestEditBounds (the slot under the cursor, or a slot
        /// picked by index): free, or holding only the complementary raw keyboard/mouse; in profile mode it must be a
        /// slot the profile expects. May adjust the destination (manual layout size, profile bounds, partner bounds).
        /// </summary>
        private static bool CheckFreeSpace(PlayerInfo player, UserScreen screen)
        {
            RectangleF s = screen.UIBounds;

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

        /// <summary>Edge of a placed player, for <see cref="Resize"/>.</summary>
        public enum ResizeEdge { Left, Right, Top, Bottom }

        /// <summary>
        /// Moves one edge of a placed player by one step: a twentieth of a cell (one grid cell in the manual layout).
        /// Used by <see cref="Resize"/>. Returns false when the
        /// step is refused because the player would get smaller than the minimum size.
        /// </summary>
        private static bool ResizeStep(PlayerInfo p, ResizeEdge edge, bool grow)
        {
            List<PlayerInfo> players = profile.DevicesList;

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

            if (edge == ResizeEdge.Left && grow)
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
                    }
                }

            }
            else if (edge == ResizeEdge.Left)
            {
                if (pmb.Width >= mSubScreen.Width)
                {
                    pmb.Width -= mSubScreen.Width;
                    peb.Width -= eSubScreen.Width;

                    if (isManual)
                    {
                        if (pmb.Width < screen.MonitorBounds.Width / screen.ManualTypeDefautScale)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (pmb.Width < mSubScreen.Width * maxMinFactor)
                        {
                            return false;
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
                }
            }
            else if (edge == ResizeEdge.Right && grow)
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
                    }
                }
            }
            else if (edge == ResizeEdge.Right)
            {
                if (pmb.Width > mSubScreen.Width)
                {
                    pmb.Width -= mSubScreen.Width;
                    peb.Width -= eSubScreen.Width;

                    if (isManual)
                    {
                        if (pmb.Width < screen.MonitorBounds.Width / screen.ManualTypeDefautScale)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (pmb.Width < mSubScreen.Width * maxMinFactor)
                        {
                            return false;
                        }
                    }

                    p.MonitorBounds = pmb;
                    p.EditBounds = peb;

                    if (playerInbounds != null)
                    {
                        playerInbounds.MonitorBounds = pmb;
                        playerInbounds.EditBounds = peb;
                    }
                }
            }
            else if (edge == ResizeEdge.Top && grow)
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
                }
            }
            else if (edge == ResizeEdge.Top)
            {
                if (pmb.Height > mSubScreen.Height)
                {
                    pmb.Height -= mSubScreen.Height;
                    peb.Height -= eSubScreen.Height;

                    if (isManual)
                    {
                        if (pmb.Height < screen.MonitorBounds.Height / screen.ManualTypeDefautScale)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (pmb.Height < mSubScreen.Height * maxMinFactor)
                        {
                            return false;
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
                }
            }
            else if (edge == ResizeEdge.Bottom && grow)
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
                    }
                }
            }
            else if (edge == ResizeEdge.Bottom)
            {
                if (pmb.Height > mSubScreen.Height)
                {
                    pmb.Height -= mSubScreen.Height;
                    peb.Height -= eSubScreen.Height;

                    if (isManual)
                    {
                        if (pmb.Height < screen.MonitorBounds.Height / screen.ManualTypeDefautScale)
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (pmb.Height < mSubScreen.Height * maxMinFactor)
                        {
                            return false;
                        }
                    }

                    p.MonitorBounds = pmb;
                    p.EditBounds = peb;

                    if (playerInbounds != null)
                    {
                        playerInbounds.MonitorBounds = pmb;
                        playerInbounds.EditBounds = peb;
                    }
                }
            }

            return true;
        }

        #region Shared cores

        /// <summary>Sets the layout of a screen and sends the players of that screen back to the tray.</summary>
        private static void ApplyScreenLayout(int screenIndex, UserScreenType type)
        {
            List<PlayerInfo> players = profile.DevicesList;

            Screens[screenIndex].Type = type;

            ///invalidate all players inside screen
            for (int j = 0; j < players.Count; j++)
            {
                ///return to default position
                PlayerInfo p = players[j];

                if (p.ScreenIndex == screenIndex)
                {
                    RemovePlayer(p, screenIndex);
                }
            }
        }

        private static void RemoveGuestCore(PlayerInfo host, PlayerInfo guest)
        {
            guest.EditBounds = guest.SourceEditBounds;
            host.InstanceGuests.Remove(guest);
        }

        #endregion

        #region Commands (setup service)

        /// <summary>Refusal reasons returned by the commands below (null means done).</summary>
        public static class Refusal
        {
            public const string NoSetup = "noSetup";
            public const string InvalidScreen = "invalidScreen";
            public const string InvalidSlot = "invalidSlot";
            public const string ManualLayout = "manualLayout";
            public const string ProfileLocked = "profileLocked";
            public const string DeviceNotRevealed = "deviceNotRevealed";
            public const string SlotOccupied = "slotOccupied";
            public const string NotAssigned = "notAssigned";
            public const string NotAGamepad = "notAGamepad";
            public const string GuestsNotSupported = "guestsNotSupported";
            public const string GuestLimitReached = "guestLimitReached";
            public const string AlreadyGuest = "alreadyGuest";
            public const string NotAGuest = "notAGuest";
            public const string CannotExpand = "cannotExpand";
            public const string CannotResize = "cannotResize";
            public const string NoFreeSlot = "noFreeSlot";
            public const string NothingToPlace = "nothingToPlace";
        }

        private static bool IsReady => Screens != null && profile?.DevicesList != null;

        private static PlayerInfo HostOf(PlayerInfo guest)
        {
            return GameProfile.AssignedDevices.FirstOrDefault(pl => pl.InstanceGuests.Contains(guest));
        }

        private static bool IsPlaced(PlayerInfo p)
        {
            return p.ScreenIndex != -1 && (GameProfile.AssignedDevices.Contains(p) || GameProfile.DevicesToMerge.Contains(p));
        }

        /// <summary>Sets the layout of a screen (players of that screen go back to the tray). Same layout: nothing happens.</summary>
        public static string SetLayout(int screenIndex, UserScreenType type)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (screenIndex < 0 || screenIndex >= Screens.Length)
            {
                return Refusal.InvalidScreen;
            }

            if (GameProfile.Loaded)
            {
                return Refusal.ProfileLocked;
            }

            if (Screens[screenIndex].Type == type)
            {
                return null;
            }

            ApplyScreenLayout(screenIndex, type);
            DevicesFunctions.UpdateDevices();

            return null;
        }

        /// <summary>Places a device in a slot of a screen, like a drop on that slot. Raw keyboard + raw mouse share a slot.</summary>
        public static string AssignToSlot(PlayerInfo player, int screenIndex, int slotIndex)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (screenIndex < 0 || screenIndex >= Screens.Length)
            {
                return Refusal.InvalidScreen;
            }

            UserScreen screen = Screens[screenIndex];

            if (screen.Type == UserScreenType.Manual)
            {
                return Refusal.ManualLayout;
            }

            if (slotIndex < 0 || slotIndex >= screen.SubScreensBounds.Count)
            {
                return Refusal.InvalidSlot;
            }

            if (!player.IsInputUsed)
            {
                return Refusal.DeviceNotRevealed;
            }

            KeyValuePair<Rectangle, RectangleF> slot = screen.SubScreensBounds.ElementAt(slotIndex);

            //Another player holds the slot: the two swap places (the other player goes to the tray when this device wasn't placed).
            List<PlayerInfo> ownGroup = GroupOf(player);
            List<List<PlayerInfo>> occupants = PlacedGroups()
                .Where(g => !g.Contains(player) && g.Any(d => d.MonitorBounds.IntersectsWith(slot.Key)))
                .ToList();

            bool joinsPartner = occupants.Count == 1 && occupants[0].Count == 1 && IsLoneRaw(player) && IsLoneRaw(occupants[0][0]) &&
                                occupants[0][0].IsRawKeyboard != player.IsRawKeyboard && occupants[0][0].MonitorBounds == slot.Key;

            if (occupants.Count > 0 && !joinsPartner)
            {
                if (GameProfile.Loaded)
                {
                    return Refusal.ProfileLocked;
                }

                if (occupants.Count > 1)
                {
                    return Refusal.SlotOccupied;
                }

                SwapPlaces(ownGroup, occupants[0], screenIndex, slot);
                return null;
            }

            return PlaceAt(player, screenIndex, () =>
            {
                destMonitorBounds = slot.Key;
                DestEditBounds = slot.Value;

                if (CheckFreeSpace(player, screen))
                {
                    return null;
                }

                return GameProfile.Loaded ? Refusal.ProfileLocked : Refusal.SlotOccupied;
            });
        }

        /// <summary>
        /// Places a device at a point of a screen (x, y from 0 to 1), like a drop at that point: the slot under the point,
        /// or the free area around it (manual layout, gaps left by expanded players).
        /// </summary>
        public static string AssignAt(PlayerInfo player, int screenIndex, double x, double y)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (screenIndex < 0 || screenIndex >= Screens.Length)
            {
                return Refusal.InvalidScreen;
            }

            if (!player.IsInputUsed)
            {
                return Refusal.DeviceNotRevealed;
            }

            UserScreen screen = Screens[screenIndex];
            x = Math.Max(0, Math.Min(0.999, x));
            y = Math.Max(0, Math.Min(0.999, y));

            return PlaceAt(player, screenIndex, () =>
            {
                Point point = new Point((int)(screen.UIBounds.X + x * screen.UIBounds.Width), (int)(screen.UIBounds.Y + y * screen.UIBounds.Height));

                if (GetFreeSpace(player, screen, point))
                {
                    return null;
                }

                if (CanFillGap(point, player, screen) && (!GameProfile.Loaded || GameProfile.GhostBounds.Any(gb => gb.Item1 == destMonitorBounds)))
                {
                    return null;
                }

                return GameProfile.Loaded ? Refusal.ProfileLocked : Refusal.SlotOccupied;
            });
        }

        /// <summary>
        /// Moves a device to a new place found by <paramref name="findDestination"/> (null = found, else a refusal reason).
        /// The device leaves its current place first, as when a drag starts; on refusal it gets its old place back.
        /// </summary>
        private static string PlaceAt(PlayerInfo player, int screenIndex, Func<string> findDestination)
        {
            PlayerInfo host = HostOf(player);

            if (host != null)
            {
                RemoveGuestCore(host, player);
            }

            bool wasPlaced = IsPlaced(player);
            int oldScreen = player.ScreenIndex;
            Rectangle oldMonitorBounds = player.MonitorBounds;
            RectangleF oldEditBounds = player.EditBounds;
            List<PlayerInfo> oldGuests = player.InstanceGuests.ToList();

            if (wasPlaced)
            {
                RemovePlayer(player, oldScreen);
            }

            string refusal = findDestination();

            if (refusal == null)
            {
                AddPlayer(player, screenIndex);
            }
            else if (wasPlaced)
            {
                destMonitorBounds = oldMonitorBounds;
                DestEditBounds = oldEditBounds;
                AddPlayer(player, oldScreen);
                player.InstanceGuests.AddRange(oldGuests);
            }
            else
            {
                player.EditBounds = GetDefaultBounds(profile.DevicesList.IndexOf(player));
                player.MonitorBounds = Rectangle.Empty;
            }

            DestEditBounds = RectangleF.Empty;
            DevicesFunctions.UpdateDevices();

            return refusal;
        }

        /// <summary>Sends a device back to the tray (a guest leaves its host).</summary>
        public static string Unassign(PlayerInfo player)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            PlayerInfo host = HostOf(player);

            if (host != null)
            {
                RemoveGuestCore(host, player);
                DevicesFunctions.UpdateDevices();
                return null;
            }

            if (!IsPlaced(player))
            {
                return Refusal.NotAssigned;
            }

            RemovePlayer(player, player.ScreenIndex);
            DevicesFunctions.UpdateDevices();

            return null;
        }

        /// <summary>Adds a gamepad as a guest of a placed gamepad's instance (handlers with several players per instance).</summary>
        public static string AddGuest(PlayerInfo host, PlayerInfo guest)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            int playersPerInstance = userGameInfo?.Game?.PlayersPerInstance ?? 0;

            if (playersPerInstance <= 1)
            {
                return Refusal.GuestsNotSupported;
            }

            if (!host.IsController || !guest.IsController || host == guest)
            {
                return Refusal.NotAGamepad;
            }

            if (!GameProfile.AssignedDevices.Contains(host))
            {
                return Refusal.NotAssigned;
            }

            if (!guest.IsInputUsed)
            {
                return Refusal.DeviceNotRevealed;
            }

            if (HostOf(guest) != null)
            {
                return Refusal.AlreadyGuest;
            }

            if (host.InstanceGuests.Count(ig => ig.GamepadId != -1) >= playersPerInstance - 1 ||
                (GameProfile.Loaded && host.InstanceGuests.Count >= host.CurrentMaxGuests))
            {
                return Refusal.GuestLimitReached;
            }

            if (IsPlaced(guest))
            {
                RemovePlayer(guest, guest.ScreenIndex);
            }

            host.InstanceGuests.Add(guest);
            DevicesFunctions.UpdateDevices();

            return null;
        }

        /// <summary>Removes a guest gamepad from its host instance.</summary>
        public static string RemoveGuest(PlayerInfo guest)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            PlayerInfo host = HostOf(guest);

            if (host == null)
            {
                return Refusal.NotAGuest;
            }

            RemoveGuestCore(host, guest);
            DevicesFunctions.UpdateDevices();

            return null;
        }

        /// <summary>Expands a placed player to the free neighbour cells, or shrinks it back to one cell (right click).</summary>
        public static string ToggleExpand(PlayerInfo player)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (GameProfile.Loaded)
            {
                return Refusal.ProfileLocked;
            }

            if (!IsPlaced(player))
            {
                return Refusal.NotAssigned;
            }

            List<PlayerInfo> players = profile.DevicesList;
            PlayerInfo playerInbounds = players.Where(pl => pl != player && pl.EditBounds == player.EditBounds).FirstOrDefault();
            Rectangle before = player.MonitorBounds;

            ToggleExpandCore(player, playerInbounds, players);
            DevicesFunctions.UpdateDevices();

            return player.ScreenIndex == -1 || player.MonitorBounds != before ? null : Refusal.CannotExpand;
        }

        /// <summary>Moves one edge of a placed player by <paramref name="steps"/> steps (positive grows), like the sizer handles.</summary>
        public static string Resize(PlayerInfo player, ResizeEdge edge, int steps)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (GameProfile.Loaded)
            {
                return Refusal.ProfileLocked;
            }

            if (!IsPlaced(player))
            {
                return Refusal.NotAssigned;
            }

            if (Screens[player.ScreenIndex].SubScreensBounds.Count < 4)//no sizer handles below 4 slots
            {
                return Refusal.CannotResize;
            }

            Rectangle before = player.MonitorBounds;

            for (int i = 0; i < Math.Abs(steps); i++)
            {
                Rectangle stepBefore = player.MonitorBounds;

                if (!ResizeStep(player, edge, steps > 0) || player.MonitorBounds == stepBefore)
                {
                    break;
                }

                if (player.MonitorBounds.Width <= 0 || player.MonitorBounds.Height <= 0)
                {
                    RemovePlayer(player, player.ScreenIndex);
                    break;
                }
            }

            DevicesFunctions.UpdateDevices();

            return player.MonitorBounds != before || player.ScreenIndex == -1 ? null : Refusal.CannotResize;
        }

        /// <summary>
        /// Places every revealed, unplaced device in the free slots, in device order (manual layouts are skipped; a raw
        /// keyboard and a raw mouse can share a slot). It doesn't need API indexes and
        /// only places revealed devices.
        /// </summary>
        public static string AutoFill()
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (GameProfile.Loaded)
            {
                return Refusal.ProfileLocked;
            }

            List<PlayerInfo> players = profile.DevicesList;

            bool IsCandidate(PlayerInfo pl) => pl.IsInputUsed && pl.ScreenIndex == -1 && HostOf(pl) == null;

            if (!players.Any(IsCandidate))
            {
                return Refusal.NothingToPlace;
            }

            int placed = 0;

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];

                if (screen.Type == UserScreenType.Manual)
                {
                    continue;
                }

                for (int b = 0; b < screen.SubScreensBounds.Count; b++)
                {
                    KeyValuePair<Rectangle, RectangleF> slot = screen.SubScreensBounds.ElementAt(b);

                    for (int j = 0; j < players.Count; j++)
                    {
                        PlayerInfo player = players[j];

                        if (!IsCandidate(player))
                        {
                            continue;
                        }

                        List<PlayerInfo> occupants = players.Where(pl => pl != player && pl.MonitorBounds.IntersectsWith(slot.Key)).ToList();

                        bool free = occupants.Count == 0;
                        bool joinsPartner = occupants.Count == 1 && occupants[0].MonitorBounds == slot.Key && IsLoneRaw(player) && IsLoneRaw(occupants[0]) &&
                                            occupants[0].IsRawKeyboard != player.IsRawKeyboard;

                        if (!free && !joinsPartner)
                        {
                            continue;
                        }

                        destMonitorBounds = slot.Key;
                        DestEditBounds = slot.Value;
                        AddPlayer(player, i);
                        placed++;

                        if (joinsPartner || !IsLoneRaw(player))
                        {
                            break;//slot full
                        }
                    }
                }
            }

            DestEditBounds = RectangleF.Empty;
            DevicesFunctions.UpdateDevices();

            return placed > 0 ? null : Refusal.NoFreeSlot;
        }

        /// <summary>A raw keyboard or a raw mouse not merged with its partner (they share a slot to make one player).</summary>
        private static bool IsLoneRaw(PlayerInfo pl) => (pl.IsRawKeyboard || pl.IsRawMouse) && !(pl.IsRawKeyboard && pl.IsRawMouse);

        /// <summary>The placed devices that make one player: a device, or a raw keyboard and raw mouse sharing their bounds.</summary>
        private static List<PlayerInfo> GroupOf(PlayerInfo device)
        {
            if (!IsPlaced(device) || !IsLoneRaw(device))
            {
                return new List<PlayerInfo> { device };
            }

            return GameProfile.AssignedDevices.Concat(GameProfile.DevicesToMerge)
                   .Where(d => d == device || (IsLoneRaw(d) && d.ScreenIndex == device.ScreenIndex && d.MonitorBounds == device.MonitorBounds))
                   .Distinct()
                   .OrderBy(d => GameProfile.AssignedDevices.Contains(d) ? 0 : 1)
                   .ToList();
        }

        /// <summary>Every placed player as its group of devices, in launch order.</summary>
        private static List<List<PlayerInfo>> PlacedGroups()
        {
            List<List<PlayerInfo>> groups = new List<List<PlayerInfo>>();

            foreach (PlayerInfo device in GameProfile.AssignedDevices.Concat(GameProfile.DevicesToMerge).ToList())
            {
                if (device.ScreenIndex == -1 || groups.Any(g => g.Contains(device)))
                {
                    continue;
                }

                groups.Add(GroupOf(device));
            }

            return groups;
        }

        private sealed class GroupPlacement
        {
            public List<PlayerInfo> Devices;
            public int Screen;
            public Rectangle MonitorBounds;
            public RectangleF EditBounds;
            public List<PlayerInfo> Guests;
        }

        /// <summary>Takes a placed group off its screen, remembering where it was and its guests.</summary>
        private static GroupPlacement TakeOff(List<PlayerInfo> group)
        {
            PlayerInfo first = group[0];
            GroupPlacement placement = new GroupPlacement
            {
                Devices = group,
                Screen = first.ScreenIndex,
                MonitorBounds = first.MonitorBounds,
                EditBounds = first.EditBounds,
                Guests = group.SelectMany(d => d.InstanceGuests).ToList(),
            };

            //secondary devices first so the primary keeps its rank in the launch order list when re-added
            for (int i = group.Count - 1; i >= 0; i--)
            {
                if (IsPlaced(group[i]))
                {
                    RemovePlayer(group[i], group[i].ScreenIndex);
                }
            }

            return placement;
        }

        /// <summary>Puts a group (primary first) at the given place, with its guests.</summary>
        private static void PutOn(List<PlayerInfo> group, int screenIndex, Rectangle monitorBounds, RectangleF editBounds, List<PlayerInfo> guests)
        {
            foreach (PlayerInfo device in group)
            {
                destMonitorBounds = monitorBounds;
                DestEditBounds = editBounds;
                AddPlayer(device, screenIndex);
            }

            if (guests != null && guests.Count > 0)
            {
                group[0].InstanceGuests.AddRange(guests.Where(g => HostOf(g) == null));
            }

            DestEditBounds = RectangleF.Empty;
        }

        /// <summary>The device's player takes the slot; the player holding it takes the device's old place (or goes to the tray).</summary>
        private static void SwapPlaces(List<PlayerInfo> moving, List<PlayerInfo> holder, int screenIndex, KeyValuePair<Rectangle, RectangleF> slot)
        {
            PlayerInfo movingHost = HostOf(moving[0]);

            if (movingHost != null)
            {
                RemoveGuestCore(movingHost, moving[0]);
            }

            bool movingWasPlaced = IsPlaced(moving[0]);

            //Players keep their numbers (launch order): a device coming from the tray takes the number of the player it replaces.
            int movingRank = GameProfile.AssignedDevices.IndexOf(moving[0]);
            int holderRank = GameProfile.AssignedDevices.IndexOf(holder[0]);

            GroupPlacement movingFrom = movingWasPlaced ? TakeOff(moving) : null;
            GroupPlacement holderFrom = TakeOff(holder);

            PutOn(moving, screenIndex, slot.Key, slot.Value, movingFrom?.Guests);

            if (movingFrom != null)
            {
                PutOn(holder, movingFrom.Screen, movingFrom.MonitorBounds, movingFrom.EditBounds, holderFrom.Guests);
                RestoreRanks(new[] { (moving[0], movingRank), (holder[0], holderRank) });
            }
            else
            {
                RestoreRanks(new[] { (moving[0], holderRank) });
            }

            DevicesFunctions.UpdateDevices();
        }

        /// <summary>Moves players back to their former positions in the launch order.</summary>
        private static void RestoreRanks(IEnumerable<(PlayerInfo device, int rank)> ranks)
        {
            foreach ((PlayerInfo device, int rank) in ranks.Where(r => r.rank >= 0).OrderBy(r => r.rank))
            {
                if (GameProfile.AssignedDevices.Remove(device))
                {
                    GameProfile.AssignedDevices.Insert(Math.Min(rank, GameProfile.AssignedDevices.Count), device);
                }
            }
        }

        /// <summary>Layouts tried, in order, when press-to-join needs more slots.</summary>
        private static readonly UserScreenType[] JoinLayouts =
        {
            UserScreenType.FullScreen, UserScreenType.DualHorizontal, UserScreenType.FourPlayers,
            UserScreenType.SixPlayers, UserScreenType.EightPlayers, UserScreenType.SixteenPlayers
        };

        /// <summary>The next layout with more slots, or null when the layout can't grow (custom, manual, sixteen players).</summary>
        private static UserScreenType? GrowLayout(UserScreenType type)
        {
            if (type == UserScreenType.DualVertical)
            {
                return UserScreenType.FourPlayers;
            }

            int index = Array.IndexOf(JoinLayouts, type);
            return index >= 0 && index < JoinLayouts.Length - 1 ? JoinLayouts[index + 1] : (UserScreenType?)null;
        }

        /// <summary>
        /// Press-to-join: places a revealed device's player in the first free slot (screens in order; a raw keyboard or
        /// mouse joins a lone complementary device first). When every slot is taken, the first screen whose layout can grow
        /// switches to the next layout with more slots and its players are placed again in order. In profile mode the
        /// device goes to the next slot the profile expects. Returns <see cref="Refusal"/> "full" when no slot can be made.
        /// </summary>
        public static string Join(PlayerInfo player)
        {
            if (!IsReady)
            {
                return Refusal.NoSetup;
            }

            if (!player.IsInputUsed)
            {
                return Refusal.DeviceNotRevealed;
            }

            if (IsPlaced(player) || HostOf(player) != null)
            {
                return null;//already in
            }

            if (GameProfile.Loaded)
            {
                return JoinProfileSlot(player);
            }

            //a raw keyboard/mouse completes a lone complementary device first
            if (IsLoneRaw(player))
            {
                List<PlayerInfo> partner = PlacedGroups().FirstOrDefault(g => g.Count == 1 && IsLoneRaw(g[0]) && g[0].IsRawKeyboard != player.IsRawKeyboard);

                if (partner != null)
                {
                    PutOn(new List<PlayerInfo> { player }, partner[0].ScreenIndex, partner[0].MonitorBounds, partner[0].EditBounds, null);
                    DevicesFunctions.UpdateDevices();
                    return null;
                }
            }

            if (TryPlaceInFreeSlot(player))
            {
                DevicesFunctions.UpdateDevices();
                return null;
            }

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreenType? bigger = GrowLayout(Screens[i].Type);

                if (bigger == null)
                {
                    continue;
                }

                //the players of that screen, in launch order, are placed again in the new layout (keeping their numbers)
                List<List<PlayerInfo>> groups = PlacedGroups().Where(g => g[0].ScreenIndex == i).ToList();
                List<(PlayerInfo, int)> ranks = groups.Select(g => (g[0], GameProfile.AssignedDevices.IndexOf(g[0]))).ToList();
                List<GroupPlacement> onScreen = groups.Select(TakeOff).ToList();

                Screens[i].Type = bigger.Value;

                for (int g = 0; g < onScreen.Count && g < Screens[i].SubScreensBounds.Count; g++)
                {
                    KeyValuePair<Rectangle, RectangleF> slot = Screens[i].SubScreensBounds.ElementAt(g);
                    PutOn(onScreen[g].Devices, i, slot.Key, slot.Value, onScreen[g].Guests);
                }

                RestoreRanks(ranks);

                if (TryPlaceInFreeSlot(player))
                {
                    DevicesFunctions.UpdateDevices();
                    return null;
                }
            }

            DevicesFunctions.UpdateDevices();
            return "full";
        }

        private static bool TryPlaceInFreeSlot(PlayerInfo player)
        {
            List<Rectangle> taken = PlacedGroups().Select(g => g[0].MonitorBounds).ToList();

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];

                if (screen.Type == UserScreenType.Manual)
                {
                    continue;
                }

                foreach (KeyValuePair<Rectangle, RectangleF> slot in screen.SubScreensBounds)
                {
                    if (taken.Any(t => t.IntersectsWith(slot.Key)))
                    {
                        continue;
                    }

                    PutOn(new List<PlayerInfo> { player }, i, slot.Key, slot.Value, null);
                    return true;
                }
            }

            return false;
        }

        /// <summary>Profile mode: the slot of the next profile player still waiting for a device.</summary>
        private static string JoinProfileSlot(PlayerInfo player)
        {
            for (int i = 0; i < GameProfile.ProfilePlayersList.Count && i < GameProfile.GhostBounds.Count; i++)
            {
                (Rectangle monitor, RectangleF edit) = GameProfile.GhostBounds[i];

                if (PlacedGroups().Any(g => g[0].MonitorBounds.IntersectsWith(monitor)))
                {
                    continue;
                }

                int screenIndex = GameProfile.FindScreenOrAlternative(GameProfile.ProfilePlayersList[i]).Item2;
                UserScreen screen = Screens[screenIndex];

                string refusal = PlaceAt(player, screenIndex, () =>
                {
                    destMonitorBounds = monitor;
                    DestEditBounds = edit;
                    return CheckFreeSpace(player, screen) ? null : Refusal.ProfileLocked;
                });

                return refusal == null ? null : "full";
            }

            return "full";
        }

        #endregion

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
