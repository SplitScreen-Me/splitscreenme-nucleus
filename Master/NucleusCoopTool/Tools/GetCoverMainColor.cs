using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    public static class GetImageMainColor
    {

        public static Color ParseColor(Bitmap image)
        {
            if(image == null)
            {
                return Color.LightGray;
            }

            var rct = new Rectangle(0, 0, image.Width, image.Height);
            var source = new int[rct.Width * rct.Height];
            var bits = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(bits.Scan0, source, 0, source.Length);
            image.UnlockBits(bits);

            return ProcessColor(source);
        }

        private static Color ProcessColor(int[] source)
        {
            int redTotal = 0, greenTotal = 0, blueTotal = 0, count = 0;

            for (int i = 0; i < source.Length; i++)
            {
                int pixel = source[i];
                int alpha = (pixel >> 24) & 0xff;

                if (alpha == 0) continue;

                redTotal += (pixel >> 16) & 0xff;
                greenTotal += (pixel >> 8) & 0xff;
                blueTotal += pixel & 0xff;
                count++;
            }

            if (count == 0)
                return Color.White;

            return Color.FromArgb(
                redTotal / count,
                greenTotal / count,
                blueTotal / count);
        }


    }
}
