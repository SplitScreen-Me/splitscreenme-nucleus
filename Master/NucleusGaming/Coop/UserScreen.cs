using Nucleus.Gaming.Cache;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop
{
    public class UserScreen
    {
        private RectangleF uiBounds;
        private RectangleF swapTypeRect;

        private Rectangle monitorBounds;
        public Rectangle MonitorBounds => monitorBounds;

        private Dictionary<Rectangle, RectangleF> subScreensBounds = new Dictionary<Rectangle, RectangleF>();
        public Dictionary<Rectangle, RectangleF> SubScreensBounds => subScreensBounds;

        public int Index;
        public int priority;
        public int DisplayIndex;
        private int playerOnScreen = 0;
        public int ManualTypeDefautScale = 1;

        public Bitmap SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "fullscreen.png");

        public int PlayerOnScreen
        {
            get => playerOnScreen;
            set => playerOnScreen = value;
        }

        public RectangleF SwapTypeBounds
        {
            get => swapTypeRect;
            set => swapTypeRect = value;
        }

        public RectangleF UIBounds
        {
            get => uiBounds;
            set
            {
                uiBounds = value;
                Type = UserScreenType.FullScreen;
            }
        }

        private UserScreenType type;
        public UserScreenType Type
        {
            get => type;
            set
            {
                type = value;
                UpdateSubBounds(value);
            }       
        }

        private void UpdateSubBounds(UserScreenType type)
        {
            subScreensBounds = new Dictionary<Rectangle, RectangleF>();

            UserScreenType screenType = type;
            Rectangle bounds = MonitorBounds;
            RectangleF ebounds = UIBounds;

            int index = 0;
            int horLines = 0;
            int verLines = 0;
            int maxPlayers = 0;

            bool Regular(int width, int height)
            {
                if (index == maxPlayers)
                {
                    return false;
                }

                int y = index % height;
                int x = (index - y) / height;

                int halfw = bounds.Width / height;
                int halfh = bounds.Height / width;

                Rectangle subMonitorBounds = new Rectangle(bounds.X + (halfw * y), bounds.Y + (halfh * x), halfw, halfh);
                float ey = (float)index % (float)height;
                float ex = ((float)index - ey) / (float)height;

                float halfwe = ebounds.Width / (float)height;
                float halfhe = ebounds.Height / (float)width;
                RectangleF editorBounds = new RectangleF(ebounds.X + (halfwe * ey), ebounds.Y + (halfhe * ex), halfwe, halfhe);

                subScreensBounds.Add(subMonitorBounds, editorBounds);
                return true;
            }

            switch (type)
            {
                case UserScreenType.FullScreen:

                    subScreensBounds.Add(bounds, ebounds);

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "fullscreen.png");

                    break;

                case UserScreenType.DualHorizontal:

                    horLines = 2;
                    verLines = 1;
                    maxPlayers = 2;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "2horizontal.png");

                    break;

                case UserScreenType.DualVertical:

                    horLines = 1;
                    verLines = 2;
                    maxPlayers = 2;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "2vertical.png");

                    break;

                case UserScreenType.FourPlayers:

                    horLines = 2;
                    verLines = 2;
                    maxPlayers = 4;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "4players.png");

                    break;

                case UserScreenType.SixPlayers:

                    horLines = 2;
                    verLines = 3;
                    maxPlayers = 6;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "6players.png");

                    break;

                case UserScreenType.EightPlayers:

                    horLines = 2;
                    verLines = 4;
                    maxPlayers = 8;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "8players.png");

                    break;

                case UserScreenType.SixteenPlayers:

                    horLines = 4;
                    verLines = 4;
                    maxPlayers = 16;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "16players.png");

                    break;

                case UserScreenType.Custom:

                    horLines = GameProfile.CustomLayout_Hor + 1;
                    verLines = GameProfile.CustomLayout_Ver + 1;
                    maxPlayers = horLines * verLines;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "customLayout.png");

                    break;

                case UserScreenType.Manual:

                    int max = 60;//set grid density, could be an option?
                    int[] divs = new int[max];
                    int last = 0;
                    int destBoundsScaleFactor = 1;

                    float divW;
                    float divH;

                    float width = (float)bounds.Width;
                    float height = (float)bounds.Height;

                    for (float i = 2; i < max; i++)
                    {
                        divW = width / i;
                        divH = height / i;

                        if ((divW % 1) == 0 && (divH % 1) == 0)
                        {
                            divs[(int)i] = (int)i;
                            last = (int)i;
                            if (i < 5)
                            {
                                destBoundsScaleFactor = (int)i;
                            }
                        }
                    }

                    horLines = (int)divs[last];
                    verLines = (int)divs[last];
                    maxPlayers = horLines * verLines;

                    ManualTypeDefautScale = destBoundsScaleFactor;

                    SwapTypeImage = ImageCache.GetImage(Globals.ThemeFolder + "manualLayout.png");

                    break;
            }

            while (Regular(horLines, verLines))
            {
                ++index;
            }
        }

        public UserScreen(Rectangle display)
        {
            monitorBounds = display;
            type = UserScreenType.FullScreen;
        }

        public int GetPlayerCount()
        {
            switch (type)
            {
                case UserScreenType.DualHorizontal:
                case UserScreenType.DualVertical:
                    return 2;
                case UserScreenType.FourPlayers:
                    return 4;
                case UserScreenType.SixPlayers:
                    return 6;
                case UserScreenType.EightPlayers:
                    return 8;
                case UserScreenType.SixteenPlayers:
                    return 16;
                case UserScreenType.Custom:
                    return GameProfile.CustomLayout_Max;
                default:
                    return -1;
            }
        }

        public bool IsFullscreen()
        {
            return type == UserScreenType.FullScreen;
        }

        public bool IsDualHorizontal()
        {
            return type == UserScreenType.DualHorizontal;
        }

        public bool IsDualVertical()
        {
            return type == UserScreenType.DualVertical;
        }

        public bool IsFourPlayers()
        {
            return type == UserScreenType.FourPlayers;
        }
    }
}
