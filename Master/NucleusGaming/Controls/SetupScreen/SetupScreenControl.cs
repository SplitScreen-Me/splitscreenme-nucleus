using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.Numerics;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    public class SetupScreenControl : UserInputControl, IDynamicSized
    {
        public static SetupScreenControl Instance;

        internal bool profileDisabled;
        internal bool canProceed;
        private bool scaled = false;
        public override bool CanProceed => canProceed;
        public override bool CanPlay => false;
      
        public ToolTip profileSettings_Tooltip;
        private bool ended = false;
        public override string Title => "Position Players";

        private UserGameInfo userGameInfo;
        public UserGameInfo UserGameInfo => userGameInfo;

        public SetupScreenControl()
        {
            Instance = this;
            Initialize();
            Invalidated += OnInvalidated;
        }

        private void Initialize()
        {
            Name = "setupScreen";

            Draw.Initialize(this, null, null);
            DevicesFunctions.Initialize(this, null, null);
            BoundsFunctions.Initialize(this, null, null);

            Cursor = Theme_Settings.Default_Cursor;

            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            BackColor = Color.Transparent;
          
            DPIManager.Register(this);
            DPIManager.Update(this);
            RemoveFlicker();          
        }

        public override void Initialize(UserGameInfo game, GameProfile profile)
        {
            base.Initialize(game, profile);

            userGameInfo = game;
            DevicesFunctions.Initialize(this, game, profile);
            BoundsFunctions.Initialize(this, game, profile);
            Draw.Initialize(this, game, profile);

            if (game.HandlerInfo.GenericGameInfo.UseHandlerSteamIds && game.HandlerInfo.GenericGameInfo.PlayerSteamIDs != null)
            {
                GameProfile.GenMissingIdFromPlayerSteamIDs();//just in case Game.PlayerSteamIDs is missing values or user add more players than the handler supports
            }

            profileDisabled = App_Misc.DisableGameProfiles || game.HandlerInfo.GenericGameInfo.MetaInfo.DisableProfiles;
            ended = false;
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            SuspendLayout();

            if (!scaled)
            {
                scaled = true;
            }

            ResumeLayout();
        }

        public void OnInvalidated(object sender, InvalidateEventArgs e)
        {
        }

        public static void InvalidateFlash(Rectangle iconBounds)
        {
            Instance?.Invalidate(iconBounds,false);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);     
        }

        public override void Ended()
        {
            base.Ended();         
            ended = true;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            BoundsFunctions.UpdateUIBounds();
            Invalidate(false);
        }

        protected override void OnDoubleClick(EventArgs e)
        {
            base.OnDoubleClick(e);
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);
            BoundsFunctions.OnMouseDoubleClick(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            BoundsFunctions.OnMouseDown(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            BoundsFunctions.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            BoundsFunctions.OnMouseUp(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {       
            base.OnPaint(e);

            Draw.UIScreens(e.Graphics);

            e.Graphics.ResetClip();
        
            for (int i = 0; i < profile.DevicesList.Count; i++)
            {
                PlayerInfo player = profile.DevicesList[i];
                
                if(player == BoundsFunctions.DraggedPlayer)
                {
                    //draw it after all devices so it's always drawn on top 
                    continue;
                }

                if (GameProfile.Loaded)
                {
                    if (!BoundsFunctions.Dragging)
                    {
                        GameProfile.FindProfilePlayers(player);
                    }
                }

                e.Graphics.ResetClip();

                if (!ended)
                {
                    Draw.UIDevices(e.Graphics, player);
                }

                if (GameProfile.AssignedDevices.Contains(player))
                {
                    GameProfile.UpdateProfilePlayerIdentity(player);

                    if (BoundsFunctions.ShowGuestRemovelText != null && !BoundsFunctions.Dragging && !GameProfile.Loaded)
                    {
                        Draw.DrawGuestRemovalText(e.Graphics, BoundsFunctions.ShowGuestRemovelText);
                    }

                    if (!player.EditBounds.IntersectsWith(BoundsFunctions.ActiveSizer) && player.EditBounds != player.SourceEditBounds)
                    {
                        Draw.PlayerTag(e.Graphics, player);
                    }
                }
            }

            if (GameProfile.Loaded)
            {
                Draw.GhostBounds(e.Graphics);
            }

            e.Graphics.ResetClip();

            if (BoundsFunctions.DraggedPlayer != null)
            {
                Draw.DraggedPlayer(e.Graphics);
            }

             Draw.PlayerBoundsInfo(e.Graphics);
            
            if (BoundsFunctions.Dragging && BoundsFunctions.DraggingScreen != -1)
            {
                if (BoundsFunctions.DraggedPlayer != null && !BoundsFunctions.DestEditBounds.Contains(BoundsFunctions.DraggedPlayer.DeviceImageBounds))
                    Draw.DestinationBounds(e.Graphics);
            }
        }
    }
}