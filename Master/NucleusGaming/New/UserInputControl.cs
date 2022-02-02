using Nucleus.Gaming.Coop;
using System;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class UserInputControl : UserControl
    {
        protected GameProfile profile;
        protected UserGameInfo game;

        public virtual bool CanProceed => throw new NotImplementedException();
        public virtual bool CanPlay => throw new NotImplementedException();

        public virtual string Title => throw new NotImplementedException();

        public GameProfile Profile => profile;

        public event Action<UserControl, bool, bool> OnCanPlayUpdated;

        protected virtual void RemoveFlicker()
        {
                SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.UserPaint |
                ControlStyles.DoubleBuffer| ControlStyles.OptimizedDoubleBuffer| ControlStyles.FixedHeight| ControlStyles.FixedWidth | ControlStyles.ResizeRedraw,
                true);
        }

        public virtual void Initialize(UserGameInfo game, GameProfile profile)
        {
            this.profile = profile;
            this.game = game;
        }

        public virtual void Ended()
        {

        }

        protected virtual void CanPlayUpdated(bool canPlay, bool autoProceed)
        {
            if (OnCanPlayUpdated != null)
            {
                OnCanPlayUpdated(this, canPlay, autoProceed);
            }
        }
   
    }
}
