using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using System.Collections.Generic;
using System.Drawing;

using System.Windows.Forms;

namespace Nucleus.Coop
{
    public static class InputIcons
    {
        private static Size iconsSize;

        public static Control[] SetInputsIcons(GenericGameInfo game)
        {
            MainForm mainForm = UI_Interface.MainForm;

            List<PictureBox> icons = new List<PictureBox>();

            if (game.Hook.SDL2Enabled)
            {
                Bitmap bmp1 = ImageCache.GetImage(Globals.ThemeFolder + "xinput_icon.png");
                float ratio1 = (float)bmp1.Width / (float)bmp1.Height;
                Size size1 = new Size((int)(mainForm.Icons_Container.Height * ratio1), mainForm.Icons_Container.Height);

                PictureBox icon1 = new PictureBox
                {
                    Name = "icon1",
                    Size = size1,
                    Image = bmp1,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                };

                CustomToolTips.SetToolTip(icon1, "Supports xinput gamepads (e.g. X360).", "icon1", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon1);

                Bitmap bmp2 = ImageCache.GetImage(Globals.ThemeFolder + "dinput_icon.png");
                float ratio2 = (float)bmp2.Width / (float)bmp2.Height;
                Size size2 = new Size((int)(mainForm.Icons_Container.Height * ratio2), mainForm.Icons_Container.Height);

                PictureBox icon2 = new PictureBox
                {
                    Name = "icon2",
                    Size = size2,
                    Image = bmp2,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                };

                CustomToolTips.SetToolTip(icon2, "Supports dinput gamepads (e.g. Ps3).", "icon2", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon2);
            }

            if ((game.Hook.XInputEnabled && !game.Hook.XInputReroute && !game.ProtoInput.DinputDeviceHook) || game.ProtoInput.XinputHook)
            {
                Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "xinput_icon.png");
                float ratio = (float)bmp.Width / (float)bmp.Height;
                Size size = new Size((int)(mainForm.Icons_Container.Height * ratio), mainForm.Icons_Container.Height);

                PictureBox icon = new PictureBox
                {
                    Name = "icon1",
                    Size = size,
                    Image = bmp,             
                    SizeMode = PictureBoxSizeMode.StretchImage,                   
                };

                CustomToolTips.SetToolTip(icon, "Supports xinput gamepads (e.g. X360).", "icon1", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon);
            }

            if ((game.Hook.DInputEnabled || game.Hook.XInputReroute || game.ProtoInput.DinputDeviceHook) && (game.Hook.XInputEnabled || game.ProtoInput.XinputHook))
            {
                Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "dinput_icon.png");
                float ratio = (float)bmp.Width / (float)bmp.Height;
                Size size = new Size((int)(mainForm.Icons_Container.Height * ratio), mainForm.Icons_Container.Height);

                PictureBox icon = new PictureBox
                {
                    Name = "icon2",
                    Size = size,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = bmp
                };


                CustomToolTips.SetToolTip(icon, "Supports dinput gamepads (e.g. Ps3).", "icon2", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon);
            }
            else if ((game.Hook.DInputEnabled || game.Hook.XInputReroute || game.ProtoInput.DinputDeviceHook) && (!game.Hook.XInputEnabled || !game.ProtoInput.XinputHook))
            {
                Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "dinput_icon.png");
                float ratio = (float)bmp.Width / (float)bmp.Height;
                Size size = new Size((int)(mainForm.Icons_Container.Height * ratio), mainForm.Icons_Container.Height);

                PictureBox icon = new PictureBox
                {
                    Name = "icon3",
                    Size = size,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = bmp
                };

                CustomToolTips.SetToolTip(icon, "Supports dinput gamepads (e.g. Ps3).", "icon3", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon);
            }

            if (game.SupportsKeyboard)
            {
                Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "keyboard_icon.png");
                float ratio = (float)bmp.Width / (float)bmp.Height;
                Size size = new Size((int)(mainForm.Icons_Container.Height * ratio), mainForm.Icons_Container.Height);

                PictureBox icon = new PictureBox
                {
                    Name = "icon4",
                    Size = size,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = bmp
                };

                CustomToolTips.SetToolTip(icon, @"Supports 1 keyboard\mouse.", "icon4", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(icon);
            }

            if (game.SupportsMultipleKeyboardsAndMice) //Raw mice/keyboards
            {
                Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "keyboard_icon.png");
                float ratio = (float)bmp.Width / (float)bmp.Height;
                Size size = new Size((int)(mainForm.Icons_Container.Height * ratio), mainForm.Icons_Container.Height);

                PictureBox iconKB1 = new PictureBox
                {
                    Name = "icon5",
                    Size = size,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = bmp
                };

                PictureBox iconKB2 = new PictureBox
                {
                    Name = "icon6",
                    Size = size,
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Image = bmp
                };


                CustomToolTips.SetToolTip(iconKB1, @"Supports multiple keyboards/mice.", "iconKB1", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(iconKB1);

                CustomToolTips.SetToolTip(iconKB2, @"Supports multiple keyboards/mice.", "iconKB2", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                icons.Add(iconKB2);
            }

            //if(game.PlayersPerInstance > 0)
            //{
            //    Bitmap bmp = ImageCache.GetImage(Globals.ThemeFolder + "2horizontal.png");
            //    float ratio = (float)bmp.Width / (float)bmp.Height;
            //    Size size = new Size((int)(mainForm.Icons_Container.Height  + 5), mainForm.Icons_Container.Height);

            //    PictureBox iconNativeSP = new PictureBox
            //    {
            //        Name = "icon7",
            //        Size = size,
            //        SizeMode = PictureBoxSizeMode.StretchImage,
            //        Image = bmp
            //    };

            //    CustomToolTips.SetToolTip(iconNativeSP, @"Supports native splitscreen", "icon7", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            //    icons.Add(iconNativeSP);
            //}

            return icons.ToArray();
        }

    }
}
