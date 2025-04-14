using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
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
        }

        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;
        public static PlayerInfo SelectedPlayer;

        public static UserScreen[] Screens;

        private static PointF draggingOffset;
        public static Point MousePos => mousePos;
        private static int draggingIndex = -1;
        public static int DraggingScreen = -1;

        public static Rectangle TotalBounds;
        private static RectangleF sizer;
        private static RectangleF sizerBtnLeft;
        private static RectangleF sizerBtnRight;
        private static RectangleF sizerBtnTop;
        private static RectangleF sizerBtnBottom;
        private static RectangleF sizerBtnCenter;

        private  static RectangleF destEditBounds;
        public static RectangleF DestEditBounds => destEditBounds;
        private static Rectangle destMonitorBounds;

        public static bool Dragging = false;
        public static bool ShowSwapTypeTip = true;

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


        internal static RectangleF GetDefaultBounds(int index)
        {
            RectangleF playersArea = DevicesFunctions.playersArea;
            float playerSize = DevicesFunctions.playerSize;
            float lineWidth = index * playerSize;
            float line = (float)Math.Round(((lineWidth + playerSize) / (double)playersArea.Width) - 0.5);
            int perLine = (int)Math.Round((playersArea.Width / (double)playerSize) - 0.5);

            float x = playersArea.X + (index * playerSize) - (perLine * playerSize * line);
            float y = playersArea.Y + (playerSize * line);

            return new RectangleF(x, y, playerSize, playerSize);
        }

        internal static void UpdateScreens()
        {
            if (Screens == null)
            {
                Screens = ScreensUtil.AllScreens();
                TotalBounds = RectangleUtil.Union(Screens);
            }
            else
            {
                for (int i = 0; i < Screens.Length; i++)
                {
                    Screens[i].PlayerOnScreen = 0;
                }

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
                        player.EditBounds = GetDefaultBounds(draggingIndex);
                        player.ScreenIndex = -1;
                        player.DisplayIndex = -1;
                    }
                }
            }

            RectangleF ScreensArea;

            float ScreensAreaScale;

            if (Screens.Length > 1)
            {
                ScreensArea = new RectangleF(5.0F, (float)parent.Height / 2.3f, (float)parent.Width - 20.0F, (float)parent.Height / 2.1f);
            }
            else
            {
                ScreensArea = new RectangleF(10.0F, 35.0F + (float)parent?.Height * 0.2f, (float)parent.Width - 20.0F, (float)parent?.Height * 0.5f);
            }

            ScreensAreaScale = ScreensArea.Width / (float)TotalBounds.Width;

            if ((float)TotalBounds.Height * ScreensAreaScale > ScreensArea.Height)
            {
                ScreensAreaScale = (float)ScreensArea.Height / (float)TotalBounds.Height;
            }

            RectangleF scaledBounds = RectangleUtil.Scale(TotalBounds, ScreensAreaScale);
            scaledBounds.X = ScreensArea.X;
            scaledBounds.Y = ScreensArea.Y;

            int minY = 0;
            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];
                screen.priority = screen.MonitorBounds.X + screen.MonitorBounds.Y;
                Rectangle bounds = RectangleUtil.Scale(screen.MonitorBounds, ScreensAreaScale);

                RectangleF uiBounds = new RectangleF((float)bounds.X, (float)bounds.Y + scaledBounds.Y, (float)bounds.Width, (float)bounds.Height);

                screen.UIBounds = uiBounds;

                minY = Math.Min(minY, (int)uiBounds.X);
            }

            ///remove negative monitors
            minY = -minY;
            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen screen = Screens[i];

                RectangleF uiBounds = screen.UIBounds;

                uiBounds.X += (float)minY + scaledBounds.X;
                screen.UIBounds = uiBounds;
                screen.SwapTypeBounds = new RectangleF(uiBounds.X, uiBounds.Y, uiBounds.Width * 0.1f, uiBounds.Width * 0.1f);

                screen.Index = i;
            }

            float instHeight = ScreensArea.Width / 1.77f;
          
        }

        public static RectangleF ActiveSizer => activeSizer;
        private static RectangleF activeSizer;

        public static RectangleF GetActiveSizer(MouseEventArgs e)
        {
            if (sizerBtnLeft.Contains(e.Location)) return sizerBtnLeft;
            if (sizerBtnRight.Contains(e.Location)) return sizerBtnRight;
            if (sizerBtnTop.Contains(e.Location)) return sizerBtnTop;
            if (sizerBtnBottom.Contains(e.Location)) return sizerBtnBottom;

            return RectangleF.Empty;
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
                        //Cursor.Position = Parent.PointToScreen(new Point((int)(Parent.Location.X + r.X + r.Width / 2), (int)(Parent.Location.Y + r.Y + r.Height / 2)));
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

                    if (playerInbounds != null)
                    {
                        pib = playerInbounds.EditBounds;
                    }

                    if (r.Contains(e.Location) || pib.Contains(e.Location))
                    {
                        if (p.ScreenIndex != -1)
                        {
                            UserScreen screen = Screens[p.ScreenIndex];

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

                            Rectangle bounds = p.MonitorBounds;
                            if ((int)screen.Type >= 3)
                            {
                                ///check if the size is 1/4th of screen
                                if (bounds.Width == halfWidth && bounds.Height == halfHeight)
                                {
                                    bool hasLeftRightSpace = true;
                                    bool hasTopBottomSpace = true;

                                    if (players.Where(pl => (pl != p && pl.ScreenIndex != -1 && pl.ScreenIndex == p.ScreenIndex) && (pl.EditBounds.Left == r.Right || (pl.EditBounds.Right == r.Left && r.Right == screen.UIBounds.Right)) && (pl.EditBounds.Y == r.Y)).Count() > 0)
                                    {
                                        hasLeftRightSpace = false;
                                    }

                                    if (players.Where(pl => (pl != p && pl.ScreenIndex != -1 && pl.ScreenIndex == p.ScreenIndex) &&
                                         (
                                          (pl.EditBounds.Top == r.Bottom || (pl.EditBounds.Top == r.Bottom && r.Bottom == screen.UIBounds.Bottom))//has a player bellow or is already at max bottom
                                          ||
                                          (pl.EditBounds.Bottom == r.Top || (pl.EditBounds.Bottom == r.Top && pl.EditBounds.Top == screen.UIBounds.Top))//has a player above or is already at max top
                                         ) && pl.EditBounds.X == r.X || pl.MonitorBounds.Width == Screens[p.ScreenIndex].MonitorBounds.Width).Count() > 0)
                                    {
                                        hasTopBottomSpace = false;
                                    }

                                    ///check if we have something left/right or top/bottom
                                    if (hasLeftRightSpace)
                                    {
                                        RectangleF edit = r;

                                        if (edit.X > screen.UIBounds.X + edit.Width)
                                        {
                                            bounds.X -= bounds.Width;
                                            edit.X -= edit.Width;
                                        }

                                        bounds.Width *= verLines;
                                        edit.Width *= verLines;

                                        while (players.Where(pl => (pl != p) && (pl.ScreenIndex != -1) && bounds.IntersectsWith(pl.MonitorBounds) && pl.EditBounds != r).Count() > 0 ||
                                            edit.Right > screen.UIBounds.Right)
                                        {
                                            bounds.Width -= p.MonitorBounds.Width;
                                            edit.Width -= p.EditBounds.Width;
                                        }

                                        if (edit != r)
                                        {
                                            p.EditBounds = edit;
                                            p.MonitorBounds = bounds;

                                            if (playerInbounds != null)
                                            {
                                                playerInbounds.MonitorBounds = p.MonitorBounds;
                                                playerInbounds.EditBounds = p.EditBounds;
                                            }

                                            parent.Invalidate(false);
                                            break;
                                        }

                                    }

                                    if (hasTopBottomSpace)
                                    {
                                        RectangleF edit = r;

                                        if (edit.Y > screen.UIBounds.Y + edit.Height)
                                        {
                                            bounds.Y -= bounds.Height;
                                            edit.Y -= edit.Height;
                                        }

                                        bounds.Height *= horLines;
                                        edit.Height *= horLines;

                                        while (players.Where(pl => (pl != p) && (pl.ScreenIndex != -1) && bounds.IntersectsWith(pl.MonitorBounds) && pl.EditBounds != r).Count() > 0 ||
                                            edit.Bottom > screen.UIBounds.Bottom)
                                        {

                                            bounds.Height -= p.MonitorBounds.Height;
                                            edit.Height -= r.Height;
                                        }

                                        p.EditBounds = edit;
                                        p.MonitorBounds = bounds;

                                        if (playerInbounds != null)
                                        {
                                            playerInbounds.MonitorBounds = p.MonitorBounds;
                                            playerInbounds.EditBounds = p.EditBounds;
                                        }

                                        parent.Invalidate(false);
                                        break;
                                    }
                                }
                                else
                                {
                                    bounds.Width = screen.MonitorBounds.Width / verLines;
                                    bounds.Height = screen.MonitorBounds.Height / horLines;
                                    p.MonitorBounds = bounds;

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
            player.EditBounds = destEditBounds;

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

        private static Point mousePos;

        internal static void OnMouseMove(MouseEventArgs e)
        {
            mousePos = e.Location;

            if (Dragging)
            {
                PlayerInfo player = profile.DevicesList[draggingIndex];

                if (!player.IsInputUsed)
                {
                    return;
                }

                player.MonitorBounds = Rectangle.Empty;

                UserScreen screen = Screens.Where(scr => scr.UIBounds.Contains(e.Location)).FirstOrDefault();

                if (screen != null)
                {
                    RectangleF s = screen.UIBounds;

                    DraggingScreen = screen.Index;

                    if (!GetFreeSpace(player))
                    {
                        DraggingScreen = -1;
                    }
                    else
                    {
                        player.EditBounds = destEditBounds;
                        parent.Invalidate(false);
                        return;
                    }
                }
                else
                {
                    DraggingScreen = -1;
                    destEditBounds = RectangleF.Empty;
                }

                RectangleF p = new RectangleF(mousePos.X - (player.SourceEditBounds.Width / 2), mousePos.Y - (player.SourceEditBounds.Height / 2), player.SourceEditBounds.Width, player.SourceEditBounds.Height);
                player.EditBounds = p;

                parent.Invalidate(false);
            }
            else
            {
                SelectedPlayer = profile.DevicesList.Where(pl => pl.ScreenIndex != -1 && pl.EditBounds.Contains(e.Location)).FirstOrDefault();

                if (SelectedPlayer != null)
                {
                    int maxPlayers = Screens[SelectedPlayer.ScreenIndex].SubScreensBounds.Count();

                    if (maxPlayers >= 4)
                    {
                        if (!GameProfile.Loaded)
                        {
                            sizer = SelectedPlayer.EditBounds;
                            UpdatetSizersBounds();
                            SetCursor(e.Location);

                            if (activeSizer != RectangleF.Empty)
                            {
                                EditPlayerBounds(e);
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
            destEditBounds = screen.SubScreensBounds.Where(b => b.Value.Contains(mousePos)).FirstOrDefault().Value;

            if (screen.Type == UserScreenType.Manual)
            {
                destMonitorBounds = new Rectangle(destMonitorBounds.X, destMonitorBounds.Y, screen.MonitorBounds.Width / screen.ManualTypeDefautScale, screen.MonitorBounds.Height / screen.ManualTypeDefautScale);
                destEditBounds = new RectangleF(destEditBounds.X, destEditBounds.Y, screen.UIBounds.Width / (float)screen.ManualTypeDefautScale, screen.UIBounds.Height / (float)screen.ManualTypeDefautScale);

                if (destEditBounds.Bottom > s.Bottom || destEditBounds.Right > s.Right)
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
                    destEditBounds = translatedBounds.Item2;

                    return true;
                }

                if (playersInDiv.Count() > 0)
                {
                    destMonitorBounds = playersInDiv.First().MonitorBounds;
                    destEditBounds = playersInDiv.First().EditBounds;
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

                if (p.ScreenIndex != -1)
                {
                    UserScreen screen = Screens[p.ScreenIndex];

                    bool isManual = screen.Type == UserScreenType.Manual;

                    Rectangle pmb = p.MonitorBounds;
                    RectangleF peb = p.EditBounds;
                    RectangleF pmbToCompare = pmb;

                    Size mSubScreen = new Size(screen.SubScreensBounds.ElementAt(0).Key.Width, screen.SubScreensBounds.ElementAt(0).Key.Height);
                    SizeF eSubScreen = new SizeF(screen.SubScreensBounds.ElementAt(0).Value.Width, screen.SubScreensBounds.ElementAt(0).Value.Height);

                    PlayerInfo playerInbounds = players.Where(pl => (pl != p) && pl.MonitorBounds == pmb).FirstOrDefault();

                    float offset = 2;//(isManual || screen.SubScreensBounds.Count() > 10) ? 2 : 5;//Can be very sensitive with few sub Screens

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
                                    if (pmb.Width < mSubScreen.Width)
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
                                    if (pmb.Width < mSubScreen.Width)
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
                                    if (pmb.Height < mSubScreen.Height)
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
                                    if (pmb.Height < mSubScreen.Height)
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

                    parent.Invalidate(false);
                }
            }
        }


        internal static void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (Dragging || GameProfile.Loaded)
            {
                return;
            }

            if (e.Button == MouseButtons.Left)
            {
                //first count how many devices we have
                bool changed = false;

                for (int i = 0; i < Screens.Length; i++)
                {
                    UserScreen screen = Screens[i];

                    if (screen.Type == UserScreenType.Manual)
                    {
                        return;
                    }

                    if (screen.UIBounds.Contains(e.Location) && !screen.SwapTypeBounds.Contains(e.Location))
                    {
                        List<PlayerInfo> players = profile.DevicesList;

                        // add all possible players!
                        for (int p = 0; p < players.Count; p++)
                        {
                            PlayerInfo player = players[p];

                            for (int b = 0; b < screen.SubScreensBounds.Count; b++)
                            {
                                destMonitorBounds = screen.SubScreensBounds.ElementAt(b).Key;
                                destEditBounds = screen.SubScreensBounds.ElementAt(b).Value;

                                if (GetFreeSpace(player))
                                {
                                    if (player.ScreenIndex == -1)
                                    {
                                        changed = true;

                                        AddPlayer(player, i);
                                    }
                                }
                            }
                        }
                    }
                }

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

            for (int i = 0; i < Screens.Length; i++)
            {
                UserScreen s = Screens[i];
                s.PlayerOnScreen = 0;
            }
        }
    }
}
