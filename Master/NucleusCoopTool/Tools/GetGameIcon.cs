using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nucleus.Coop.UI;

namespace Nucleus.Coop.Tools
{
    public static class GetGameIcon
    {

        public static void GetIcon(object state)
        {
            MainForm mainForm = UI_Interface.MainForm;

            UserGameInfo game = (UserGameInfo)state;
            Bitmap bmp;

            GameControl gameControl = Core_Interface.GetGameControl(game.GameGuid);

            if (gameControl == null)
            {
                return;
            }

            if (game.Icon != null)
            {
                gameControl.Image = game.Icon;
                return;
            }

            string iconPath = game.HandlerInfo.GenericGameInfo.MetaInfo.IconPath;

            try
            {
                if (File.Exists(iconPath))
                {
                    if (iconPath.EndsWith(".exe"))
                    {
                        Icon icon = Shell32.GetIcon(iconPath, false);
                        bmp = icon.ToBitmap();
                        icon.Dispose();
                    }
                    else
                    {
                        bmp = ImageCache.GetImage(iconPath);
                    }
                }
                else
                {
                    Icon icon = Shell32.GetIcon(game.ExePath, false);
                    bmp = icon.ToBitmap();
                    icon.Dispose();
                }
            }
            catch
            {
                bmp = ImageCache.GetImage(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png"));
            }

            if (bmp == null)
            {
                bmp = ImageCache.GetImage(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png"));
            }

            game.Icon = bmp;

            gameControl.Image = game.Icon;
        }
    }
}
