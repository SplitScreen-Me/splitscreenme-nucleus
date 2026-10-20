using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public class GaussianBlur
    {
        private readonly int[] _alpha;
        private readonly int[] _red;
        private readonly int[] _green;
        private readonly int[] _blue;

        private readonly int _width;
        private readonly int _height;

        private readonly ParallelOptions _pOptions = new ParallelOptions { MaxDegreeOfParallelism = 16 };
        public Color topColor;
        public Color bottomColor;

        public GaussianBlur(Bitmap bmp)
        {
            Bitmap image = new Bitmap(bmp, new Size(1280,720));

            var rct = new Rectangle(0, 0, image.Width, image.Height);
            var source = new int[rct.Width * rct.Height];
            var bits = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(bits.Scan0, source, 0, source.Length);
            image.UnlockBits(bits);

            _width = image.Width;
            _height = image.Height;

            _alpha = new int[_width * _height];
            _red = new int[_width * _height];
            _green = new int[_width * _height];
            _blue = new int[_width * _height];

            // Process top and bottom colors
            //ProcessColors(source);

            Parallel.For(0, source.Length, _pOptions, i =>
            {
                _alpha[i] = (int)((source[i] & 0xff000000) >> 24);
                _red[i] = (source[i] & 0xff0000) >> 16;
                _green[i] = (source[i] & 0x00ff00) >> 8;
                _blue[i] = (source[i] & 0x0000ff);
            });
        }

        private void ProcessColors(int[] source)
        {
            int topRedTotal = 0, topGreenTotal = 0, topBlueTotal = 0, topCount = 0;
            int bottomRedTotal = 0, bottomGreenTotal = 0, bottomBlueTotal = 0, bottomCount = 0;

            int topEnd = _width * 10;
            int bottomStart = source.Length - _width * 10;

            for (int i = 0; i < topEnd; i++)
            {
                topRedTotal += (source[i] & 0xff0000) >> 16;
                topGreenTotal += (source[i] & 0x00ff00) >> 8;
                topBlueTotal += (source[i] & 0x0000ff);
                topCount++;
            }

            for (int i = bottomStart; i < source.Length; i++)
            {
                bottomRedTotal += (source[i] & 0xff0000) >> 16;
                bottomGreenTotal += (source[i] & 0x00ff00) >> 8;
                bottomBlueTotal += (source[i] & 0x0000ff);
                bottomCount++;
            }

            topColor = Color.FromArgb(
                Math.Min(255, topRedTotal / topCount),
                Math.Min(255, topGreenTotal / topCount),
                Math.Min(255, topBlueTotal / topCount));

            bottomColor = Color.FromArgb(
                Math.Min(255, bottomRedTotal / bottomCount),
                Math.Min(255, bottomGreenTotal / bottomCount),
                Math.Min(255, bottomBlueTotal / bottomCount));
        }

        public Bitmap Process(int radial)
        {
            var newAlpha = new int[_width * _height];
            var newRed = new int[_width * _height];
            var newGreen = new int[_width * _height];
            var newBlue = new int[_width * _height];
            var dest = new int[_width * _height];

            // Apply Gaussian blur in parallel
            Parallel.Invoke(
                () => gaussBlur_4(_alpha, newAlpha, radial),
                () => gaussBlur_4(_red, newRed, radial),
                () => gaussBlur_4(_green, newGreen, radial),
                () => gaussBlur_4(_blue, newBlue, radial)
            );

            //ApplyVerticalColorBlend(newRed, newGreen, newBlue);
            //ApplyCinematicFade(newAlpha, newRed, newGreen, newBlue);

            Parallel.For(0, dest.Length, _pOptions, i =>
            {
                dest[i] = (Clamp(newAlpha[i], 0, 255) << 24) |
                          (Clamp(newRed[i], 0, 255) << 16) |
                          (Clamp(newGreen[i], 0, 255) << 8) |
                          Clamp(newBlue[i], 0, 255);
            });

            var image = new Bitmap(_width, _height);
            var rct = new Rectangle(0, 0, image.Width, image.Height);
            var bits2 = image.LockBits(rct, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            Marshal.Copy(dest, 0, bits2.Scan0, dest.Length);
            image.UnlockBits(bits2);

            return image;
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        private void gaussBlur_4(int[] source, int[] dest, int r)
        {
            var bxs = boxesForGauss(r, 3);
            boxBlur_4(source, dest, _width, _height, (bxs[0] - 1) / 2);
            boxBlur_4(dest, source, _width, _height, (bxs[1] - 1) / 2);
            boxBlur_4(source, dest, _width, _height, (bxs[2] - 1) / 2);
        }

        private int[] boxesForGauss(int sigma, int n)
        {
            var wIdeal = Math.Sqrt((12 * sigma * sigma / n) + 1);
            var wl = (int)Math.Floor(wIdeal);
            if (wl % 2 == 0) wl--;
            var wu = wl + 2;

            var mIdeal = (double)(12 * sigma * sigma - n * wl * wl - 4 * n * wl - 3 * n) / (-4 * wl - 4);
            var m = Math.Round(mIdeal);

            var sizes = new int[n];
            for (var i = 0; i < n; i++) sizes[i] = (i < m) ? wl : wu;
            return sizes;
        }

        private void boxBlur_4(int[] source, int[] dest, int w, int h, int r)
        {
            Array.Copy(source, dest, source.Length);  // One copy instead of two
            boxBlurH_4(dest, source, w, h, r);
            boxBlurT_4(source, dest, w, h, r);
        }

        private void boxBlurH_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = 1.0 / (r + r + 1);
            Parallel.For(0, h, _pOptions, i =>
            {
                var ti = i * w;
                var li = ti;
                var ri = ti + r;
                var fv = source[ti];
                var lv = source[ti + w - 1];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri++] - fv;
                    dest[ti++] = (int)(val * iar);
                }
                for (var j = r + 1; j < w - r; j++)
                {
                    val += source[ri++] - source[li++];
                    dest[ti++] = (int)(val * iar);
                }
                for (var j = w - r; j < w; j++)
                {
                    val += lv - source[li++];
                    dest[ti++] = (int)(val * iar);
                }
            });
        }

        private void boxBlurT_4(int[] source, int[] dest, int w, int h, int r)
        {
            var iar = 1.0 / (r + r + 1);
            Parallel.For(0, w, _pOptions, i =>
            {
                var ti = i;
                var li = ti;
                var ri = ti + r * w;
                var fv = source[ti];
                var lv = source[ti + w * (h - 1)];
                var val = (r + 1) * fv;
                for (var j = 0; j < r; j++) val += source[ti + j * w];
                for (var j = 0; j <= r; j++)
                {
                    val += source[ri] - fv;
                    dest[ti] = (int)(val * iar);
                    ri += w;
                    ti += w;
                }
                for (var j = r + 1; j < h - r; j++)
                {
                    val += source[ri] - source[li];
                    dest[ti] = (int)(val * iar);
                    li += w;
                    ri += w;
                    ti += w;
                }
                for (var j = h - r; j < h; j++)
                {
                    val += lv - source[li];
                    dest[ti] = (int)(val * iar);
                    li += w;
                    ti += w;
                }
            });
        }

        private void ApplyCinematicFade(int[] alpha, int[] red, int[] green, int[] blue)
        {
            double centerX = _width / 2.0;
            double centerY = _height / 2.0;

            double radiusX = centerX;
            double radiusY = centerY;

            Parallel.For(0, _height, _pOptions, y =>
            {
                for (int x = 0; x < _width; x++)
                {
                    int index = y * _width + x;

                    double dx = (x - centerX) / radiusX;
                    double dy = (y - centerY) / radiusY;

                    double dist = Math.Sqrt(dx * dx + dy * dy);

                    // Elliptical fade
                    double fade = 1.1 - dist;
                    if (fade < 0) fade = 0;

                    // Smoothstep (AAA fade curve)
                    fade = fade * fade * (3 - 2 * fade);

                    // Apply transparency
                    alpha[index] = (int)(alpha[index] * fade);

                    // Vignette darkening near edges
                    double vignette = 0.7 + (fade * 0.3);

                    red[index] = Clamp((int)(red[index] * vignette), 0, 255);
                    green[index] = Clamp((int)(green[index] * vignette), 0, 255);
                    blue[index] = Clamp((int)(blue[index] * vignette), 0, 255);
                }
            });
        }

        private void ApplyVerticalColorBlend(int[] red, int[] green, int[] blue)
        {
            int fadeHeight = _height / 3;

            Parallel.For(0, _height, _pOptions, y =>
            {
                double blend = 0;

                if (y < fadeHeight)
                {
                    blend = 1.0 - (double)y / fadeHeight;
                }
                else if (y > _height - fadeHeight)
                {
                    blend = 1.0 - (double)(_height - y) / fadeHeight;
                }

                blend = Math.Max(0, Math.Min(1, blend));

            });
        }

    }


}
