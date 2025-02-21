using Nucleus.Gaming.Cache;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Nucleus.Coop.UI;

namespace Nucleus.Coop.Tools
{
    internal class ChangeGameIcon
    {
        public static void ChangeIcon(UserGameInfo userGameInfo)
        {
            MainForm mainForm = UI_Interface.MainForm;

            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "All Images Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe" +
                            "|PNG Portable Network Graphics (*.png)|*.png" +
                            "|JPEG File Interchange Format (*.jpg *.jpeg *jfif)|*.jpg;*.jpeg;*.jfif" +
                            "|BMP Windows Bitmap (*.bmp)|*.bmp" +
                            "|TIF Tagged Imaged File Format (*.tif *.tiff)|*.tif;*.tiff" +
                            "|Icon (*.ico)|*.ico" +
                            "|Executable (*.exe)|*.exe";

                dlg.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons");

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    //string prevImage = userGameInfo.Game.MetaInfo.IconPath;
                    userGameInfo.Game.MetaInfo.IconPath = dlg.FileName;

                    lock (Core_Interface.GameControlsInfo)
                    {
                        if (Core_Interface.GameControlsInfo.ContainsKey(userGameInfo))
                        {
                            GameControl control = Core_Interface.GameControlsInfo[userGameInfo];

                            if (userGameInfo.Game.MetaInfo.IconPath.EndsWith(".exe"))
                            { 
                                Icon icon = Shell32.GetIcon(userGameInfo.Game.MetaInfo.IconPath, false);
                                userGameInfo.Icon = icon.ToBitmap();
                                control.Image = userGameInfo.Icon;
                                icon.Dispose();
                            }
                            else
                            {
                                userGameInfo.Icon = ImageCache.GetImage(userGameInfo.Game.MetaInfo.IconPath);
                                control.Image = userGameInfo.Icon;
                            }
                        }
                    }

                   //ImageCache.DeleteImageFromCache(prevImage);
                }
            }

        }
    }
}
