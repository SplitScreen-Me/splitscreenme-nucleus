using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace Nucleus.Gaming.Forms.Overlay
{
    /// <summary>
    /// The "Bezel" look of the app (Nucleus.WebUI src/styles/index.css) for the windows drawn over the games:
    /// the launch player cards and the on-screen display toasts. Everything is frozen: usable from any UI thread.
    /// </summary>
    internal static class OverlayStyle
    {
        public static readonly Color Pane = Rgb(0x14171e);
        public static readonly Color Pane2 = Rgb(0x1b1f28);
        public static readonly Color Pane3 = Rgb(0x242935);
        public static readonly Color Line = Rgb(0x2c3240);
        public static readonly Color LineStrong = Rgb(0x3d4454);
        public static readonly Color Ink = Rgb(0xf3f4f6);
        public static readonly Color Ink2 = Rgb(0xa3abba);
        public static readonly Color Ink3 = Rgb(0x6b7383);
        public static readonly Color OnLight = Rgb(0x07080a);
        public static readonly Color Ok = Rgb(0x8fe3b8);

        /// <summary>The eight player colours of the app (--color-p1…p8), in join order.</summary>
        private static readonly Color[] PlayerColors =
        {
            Rgb(0xff6b35), //ember
            Rgb(0x3b8bff), //azure
            Rgb(0xffcf3a), //sun
            Rgb(0x3ddb86), //mint
            Rgb(0xff5db1), //pink
            Rgb(0x9b7cff), //violet
            Rgb(0x23d3d3), //teal
            Rgb(0xc4eb45), //lime
        };

        /// <summary>Colour of player <paramref name="number"/> (1-based), cycling after 8 like the app's playerClass(n).</summary>
        public static Color PlayerColor(int number) => PlayerColors[(((number - 1) % 8) + 8) % 8];

        //No font files: Windows 11 has Segoe UI Variable, older systems fall back to Segoe UI.
        public static readonly FontFamily DisplayFont = new FontFamily("Segoe UI Variable Display, Segoe UI");
        public static readonly FontFamily TextFont = new FontFamily("Segoe UI Variable Text, Segoe UI");

        private static readonly Lazy<FontFamily> iconFont = new Lazy<FontFamily>(FindIconFont);

        /// <summary>Segoe Fluent Icons (Windows 11) or Segoe MDL2 Assets (Windows 10), null when neither is installed.</summary>
        public static FontFamily IconFont => iconFont.Value;

        public const char GlyphController = '';
        public const char GlyphKeyboard = '';
        public const char GlyphMouse = '';

        /// <summary>Quick out, firm landing: the app's --ease-snap.</summary>
        public static readonly IEasingFunction Snap = Frozen(new ExponentialEase { Exponent = 6, EasingMode = EasingMode.EaseOut });
        public static readonly IEasingFunction Smooth = Frozen(new CubicEase { EasingMode = EasingMode.EaseInOut });
        public static readonly IEasingFunction Out = Frozen(new CubicEase { EasingMode = EasingMode.EaseOut });
        public static readonly IEasingFunction Pop = Frozen(new BackEase { Amplitude = 0.45, EasingMode = EasingMode.EaseOut });

        public static SolidColorBrush Brush(Color color, double opacity = 1.0)
        {
            SolidColorBrush brush = new SolidColorBrush(Color.FromArgb((byte)Math.Round(255 * opacity), color.R, color.G, color.B));
            brush.Freeze();
            return brush;
        }

        /// <summary>A lit pane: a hair lighter at the top, near-black at the bottom.</summary>
        public static Brush PaneBrush(double opacity)
        {
            LinearGradientBrush brush = new LinearGradientBrush(
                Color.FromArgb((byte)Math.Round(255 * opacity), Pane2.R, Pane2.G, Pane2.B),
                Color.FromArgb((byte)Math.Round(255 * opacity), 0x0f, 0x11, 0x16),
                90);
            brush.Freeze();
            return brush;
        }

        /// <summary>Soft shadow under a pane.</summary>
        public static Effect Shadow(double blur, double depth, double opacity)
        {
            DropShadowEffect shadow = new DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = blur,
                ShadowDepth = depth,
                Direction = 270,
                Opacity = opacity,
                RenderingBias = RenderingBias.Quality,
            };
            shadow.Freeze();
            return shadow;
        }

        public static Color Mix(Color a, Color b, double t)
        {
            return Color.FromRgb(
                (byte)Math.Round(a.R + (b.R - a.R) * t),
                (byte)Math.Round(a.G + (b.G - a.G) * t),
                (byte)Math.Round(a.B + (b.B - a.B) * t));
        }

        public static Color WithAlpha(Color color, double opacity) => Color.FromArgb((byte)Math.Round(255 * opacity), color.R, color.G, color.B);

        private static Color Rgb(int rgb) => Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);

        private static IEasingFunction Frozen(EasingFunctionBase easing)
        {
            easing.Freeze();
            return easing;
        }

        private static FontFamily FindIconFont()
        {
            foreach (string name in new[] { "Segoe Fluent Icons", "Segoe MDL2 Assets" })
            {
                try
                {
                    Typeface typeface = new Typeface(new FontFamily(name), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);

                    if (typeface.TryGetGlyphTypeface(out GlyphTypeface glyphs) && glyphs.CharacterToGlyphMap.ContainsKey(GlyphController))
                    {
                        return new FontFamily(name);
                    }
                }
                catch
                {
                }
            }

            return null;
        }
    }

    /// <summary>Win32 for the overlay windows: no activation, click-through, topmost, placed in physical pixels.</summary>
    internal static class OverlayNative
    {
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_TRANSPARENT = 0x00000020;
        private const int WS_EX_TOOLWINDOW = 0x00000080;
        private const int WS_EX_APPWINDOW = 0x00040000;
        private const int WS_EX_LAYERED = 0x00080000;
        private const int WS_EX_NOACTIVATE = 0x08000000;

        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOACTIVATE = 0x0010;
        private const uint SWP_NOOWNERZORDER = 0x0200;

        private const uint MONITOR_DEFAULTTONEAREST = 2;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MONITORINFO
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        private static extern IntPtr MonitorFromRect(ref RECT rect, uint flags);

        [DllImport("user32.dll")]
        private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

        [DllImport("shcore.dll")]
        private static extern int GetDpiForMonitor(IntPtr monitor, int dpiType, out uint dpiX, out uint dpiY);

        [DllImport("user32.dll")]
        private static extern IntPtr GetThreadDpiAwarenessContext();

        [DllImport("user32.dll")]
        private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

        /// <summary>
        /// Never activated, never hit-tested, no taskbar or Alt+Tab entry. Call before the window is shown.
        /// (WS_EX_LAYERED is already set by AllowsTransparency, it's repeated for clarity.)
        /// </summary>
        public static void MakeOverlay(IntPtr hwnd)
        {
            int style = GetWindowLong(hwnd, GWL_EXSTYLE);
            SetWindowLong(hwnd, GWL_EXSTYLE, (style | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_LAYERED | WS_EX_NOACTIVATE) & ~WS_EX_APPWINDOW);
        }

        /// <summary>Moves and sizes the window in physical pixels, topmost, without activating it.</summary>
        public static void Place(IntPtr hwnd, System.Drawing.Rectangle bounds)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, bounds.X, bounds.Y, Math.Max(1, bounds.Width), Math.Max(1, bounds.Height), SWP_NOACTIVATE | SWP_NOOWNERZORDER);
        }

        /// <summary>Back on top of the topmost game windows, without activating it.</summary>
        public static void RaiseTopmost(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER);
        }

        /// <summary>No longer topmost (an invisible toast mustn't stay above the games), without activating it.</summary>
        public static void DropTopmost(IntPtr hwnd)
        {
            SetWindowPos(hwnd, HWND_NOTOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOOWNERZORDER);
        }

        /// <summary>Work area of the monitor nearest to <paramref name="bounds"/>, in this thread's coordinates.</summary>
        public static System.Drawing.Rectangle WorkArea(System.Drawing.Rectangle bounds)
        {
            RECT rect = new RECT { Left = bounds.Left, Top = bounds.Top, Right = bounds.Right, Bottom = bounds.Bottom };
            MONITORINFO info = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };
            IntPtr monitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

            if (monitor != IntPtr.Zero && GetMonitorInfo(monitor, ref info))
            {
                return System.Drawing.Rectangle.FromLTRB(info.rcWork.Left, info.rcWork.Top, info.rcWork.Right, info.rcWork.Bottom);
            }

            return bounds;
        }

        /// <summary>
        /// Pixels per DIP for the monitor nearest to <paramref name="bounds"/>, as this thread sees it
        /// (system DPI for a system-aware thread, the monitor's own DPI for a per-monitor one). 1 when unknown.
        /// </summary>
        public static double DpiScale(System.Drawing.Rectangle bounds)
        {
            try
            {
                RECT rect = new RECT { Left = bounds.Left, Top = bounds.Top, Right = bounds.Right, Bottom = bounds.Bottom };
                IntPtr monitor = MonitorFromRect(ref rect, MONITOR_DEFAULTTONEAREST);

                if (monitor != IntPtr.Zero && GetDpiForMonitor(monitor, 0 /*MDT_EFFECTIVE_DPI*/, out uint dpiX, out _) == 0 && dpiX > 0)
                {
                    return dpiX / 96.0;
                }
            }
            catch (Exception)//Windows 7/8: no shcore.dll GetDpiForMonitor
            {
            }

            return 1.0;
        }

        /// <summary>DPI awareness context of the calling thread, IntPtr.Zero before Windows 10 1607.</summary>
        public static IntPtr CurrentDpiContext()
        {
            try
            {
                return GetThreadDpiAwarenessContext();
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        /// <summary>Gives the calling thread the DPI awareness of another one, so both share one coordinate space.</summary>
        public static void UseDpiContext(IntPtr context)
        {
            if (context == IntPtr.Zero)
            {
                return;
            }

            try
            {
                SetThreadDpiAwarenessContext(context);
            }
            catch (Exception)
            {
            }
        }
    }
}
