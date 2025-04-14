using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Windows;
using System;

namespace Nucleus.Gaming.Tools
{
    public static class TaskbarState
    {
        private static GenericGameInfo genericGameInfo = GenericGameHandler.Instance?.CurrentGameInfo;

        private static bool userTaskbarAutoHide;
        private static  ProtoInputOptions protoInputOpt;
        private static ProtoInput protoInput;

        public static Action Hide => ToggleHide;
        public static Action Show => ToggleShow;

        private static void ToggleHide()
        {
            if(genericGameInfo == null)
            {
                return;
            }

            if(protoInput == null)
            {
                protoInput = ProtoInput.protoInput;
                protoInputOpt = genericGameInfo.ProtoInput;
                userTaskbarAutoHide = protoInput.GetTaskbarAutohide();
                GenericGameHandler.Instance.Ended += OnEndedCallback;
            }
            
            if (genericGameInfo.HideTaskbar && !GameProfile.UseSplitDiv && !userTaskbarAutoHide)
            {
                User32Util.HideTaskbar();
            }
            else if (protoInputOpt.AutoHideTaskbar || GameProfile.UseSplitDiv)
            {
                if (userTaskbarAutoHide)
                {
                    protoInputOpt.AutoHideTaskbar = false; // If already hidden don't change it, and dont set it unhidden after.
                }
                else
                {
                   protoInput?.SetTaskbarAutohide(true);
                }
            }
        }

        private static void ToggleShow()
        {
            if (genericGameInfo == null)
            {
                return;
            }

            if (genericGameInfo.HideTaskbar && !GameProfile.UseSplitDiv && !userTaskbarAutoHide)
            {
                User32Util.ShowTaskBar();
            }

            //protoInput.SetTaskbarAutohide(false) can't be called from here else the app get stuck for some reason.
            //so it's called only once handler has ended.
        }

        private static void OnEndedCallback()
        {
            ToggleShow();

            if ((protoInputOpt.AutoHideTaskbar || GameProfile.UseSplitDiv) && !userTaskbarAutoHide)
            {
                protoInput.SetTaskbarAutohide(false);
            }
        
            protoInput = null;
        }
    }
}
