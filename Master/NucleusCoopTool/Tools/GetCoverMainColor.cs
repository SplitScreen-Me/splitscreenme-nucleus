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
    public static class GetCoverMainColor
    {

        private static int _width;
        private static int _height;

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

            _width = image.Width;
            _height = image.Height;
            // Process color
            return ProcessColor(source);
        }

        private static Color ProcessColor(int[] source)
        {
            int bottomRedTotal = 0, bottomGreenTotal = 0, bottomBlueTotal = 0, bottomCount = 0;

            int bottomStart = source.Length - _width * 10;

            for (int i = 0; i < source.Length; i++)
            {
                bottomRedTotal += (source[i] & 0xff0000) >> 16;
                bottomGreenTotal += (source[i] & 0x00ff00) >> 8;
                bottomBlueTotal += (source[i] & 0x0000ff);
                bottomCount++;
            }

            return Color.FromArgb(
                Math.Min(255, bottomRedTotal / bottomCount),
                Math.Min(255, bottomGreenTotal / bottomCount),
                Math.Min(255, bottomBlueTotal / bottomCount));
        }
               
        
    }
}
