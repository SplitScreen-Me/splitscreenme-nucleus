using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Windows;
using System.Drawing;

namespace Nucleus.Gaming
{
    public static class ScreensUtil
    {

        public static UserScreen[] AllScreens()
        {
            Display[] all = User32Util.GetDisplays();
            UserScreen[] rects = new UserScreen[all.Length];


            for (int i = 0; i < rects.Length; i++)
            {
                rects[i] = new UserScreen(all[i].Bounds);
                rects[i].DisplayIndex = all[i].DisplayIndex;
            }

            return rects;
        }


        public static Display[] AllScreensParams()
        {
            Display[] all = User32Util.GetDisplays();

            return all;
        }

    }
}
