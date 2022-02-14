using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Windows;
using System;
using System.Drawing;

namespace Nucleus.Gaming
{
    public static class ScreensUtil
    {
        public static UserScreen[] GetSetup_Triple4kHorizontal()
        {
            return new UserScreen[]
            {
                new UserScreen(new Rectangle(0, 0, 3840, 2160)),
                new UserScreen(new Rectangle(3840, 0, 3840, 2160)),
                new UserScreen(new Rectangle(7680, 0, 3840, 2160))
            };
        }

        public static UserScreen[] GetSetup_Triple4kVertical()
        {
            return new UserScreen[]
            {
                new UserScreen(new Rectangle(0, 0, 2160, 3840)),
                new UserScreen(new Rectangle(2160, 0, 2160, 3840)),
                new UserScreen(new Rectangle(4320, 0, 2160, 3840))
            };
        }

        public static UserScreen[] GetSetup_Four1080pHorizontal()
        {
            return new UserScreen[]
            {
                new UserScreen(new Rectangle(-1920, 0, 1920, 1080)),
                new UserScreen(new Rectangle(0, 0, 1920, 1080)),
                new UserScreen(new Rectangle(1920, 0, 1920, 1080)),
                new UserScreen(new Rectangle(3840, 0, 1920, 1080))
            };
        }

        public static UserScreen[] AllScreens()
        {
            Display[] all = User32Util.GetDisplays();
            UserScreen[] rects = new UserScreen[all.Length];

            for (int i = 0; i < rects.Length; i++)
            {
                rects[i] = new UserScreen(all[i].Bounds);
            }

            return rects;
        }

        
        public static Display[] AllScreensParams()
        {
            Display[] all = User32Util.GetDisplays();
           
            return all;
        }

        public static Rectangle[] AllScreensRec()
        {
            //return GetSetup_Triple4kHorizontal();
            Display[] all = User32Util.GetDisplays();
            Rectangle[] rects = new Rectangle[all.Length];

            for (int i = 0; i < all.Length; i++)
            {
                rects[i] = all[i].Bounds;
            }

            return rects;
        }
    }
}
