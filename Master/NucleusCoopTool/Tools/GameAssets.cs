using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    public class GameAssets
    {
        public static void GetGameAssets()
        {
            var gameGUID = Core_Interface.Current_UserGameInfo?.GameGuid;

            Image coverBmp = null;

            var coversPath = Directory.GetFiles(Path.Combine(Application.StartupPath, $"gui\\covers")).Where(s => s.Contains(gameGUID) && (
                s.EndsWith(".png") ||
                s.EndsWith(".jpeg") ||
                s.EndsWith(".jpg") ||
                s.EndsWith(".bmp") ||
                s.EndsWith(".gif"))
                ).ToList();

            if (coversPath.Count > 0)
            {
                coverBmp = new Bitmap(coversPath[0]);
            }
            else
            {
                coverBmp = ImageCache.GetImage(Globals.ThemeFolder + "no_cover.png");
            }

            UI_Interface.Cover.BackgroundImage = coverBmp;

            UI_Interface.MainForm.Invoke((MethodInvoker)delegate ()
            {
                ///Apply screenshots randomly
                if (Directory.Exists(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameGUID}")))
                {
                    var imgsPath = Directory.GetFiles(Path.Combine(Application.StartupPath, $"gui\\screenshots\\{gameGUID}")).Where(s =>
                    s.EndsWith(".png") ||
                    s.EndsWith(".jpeg") ||
                    s.EndsWith(".jpg") ||
                    s.EndsWith(".bmp") ||
                    s.EndsWith(".gif")
                    ).ToList();

                    if (imgsPath.Count > 0)
                    {
                        Random rNum = new Random();
                        int RandomIndex = rNum.Next(0, imgsPath.Count);

                        Bitmap backgroundImg = UI_Graphics.ApplyBlur(new Bitmap(imgsPath[RandomIndex]));
                        UI_Graphics.BackgroundImg = backgroundImg;
                    }
                    else
                    {
                        Bitmap def = UI_Graphics.ApplyBlur(new Bitmap((Bitmap)UI_Graphics.DefaultBackground.Clone()));
                        UI_Graphics.BackgroundImg = def;
                        UI_Graphics.GameBorderGradientTop = Theme_Settings.DefaultBorderGradientColor;
                        UI_Graphics.GameBorderGradientBottom = Theme_Settings.DefaultBorderGradientColor;
                    }
                }
                else
                {
                    Bitmap def = UI_Graphics.ApplyBlur(new Bitmap((Bitmap)UI_Graphics.DefaultBackground.Clone()));
                    UI_Graphics.BackgroundImg = def;
                    UI_Graphics.GameBorderGradientTop = Theme_Settings.DefaultBorderGradientColor;
                    UI_Graphics.GameBorderGradientBottom = Theme_Settings.DefaultBorderGradientColor;
                }
            });

            UI_Interface.MainForm.Refresh();
        }
    }
}
