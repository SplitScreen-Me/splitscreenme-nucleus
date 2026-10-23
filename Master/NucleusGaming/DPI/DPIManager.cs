using Nucleus.Gaming.DPI;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public static class DPIManager
    {
        public static float Scale = 1.0f;
        private static List<IDynamicSized> components = new List<IDynamicSized>();

        public static Font Font;

        [DllImport("user32.dll")]
        public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDC(IntPtr hwnd);

        [DllImport("gdi32.dll")]
        private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        private static float GetDpi()
        {
            IntPtr desktopWnd = IntPtr.Zero;
            IntPtr dc = GetDC(desktopWnd);
            float dpi = 100f;
            const int LOGPIXELSX = 88;
            try
            {
                dpi = GetDeviceCaps(dc, LOGPIXELSX);
            }
            finally
            {
                ReleaseDC(desktopWnd, dc);
            }

            return dpi; /// 96f;
        }

        public static void PreInitialize()
        {
            Scale = User32Util.GetDPIScalingFactor();
        }

        private static void UpdateForm(Form form)
        {
            uint val = Convert.ToUInt32(GetDpi());
            float newScale = val / 96.0f;

            float dif = Math.Abs(newScale - Scale);

            if (dif > 0.001f)
            {
                Scale = newScale;
                UIThread.Post(() =>
                {
                    UpdateAll();
                });
            }
        }

        public static void AddForm(Form form)
        {
            Version os = WindowsVersionInfo.Version;

            if (os.Major > 6 || os.Major == 6 && os.Minor >= 3)
            {
                // if we are on Windows 8.1 or higher we can have
                // custom DPI by window
                form.LocationChanged += AppForm_LocationChanged;
            }

            UpdateForm(form);
        }

        private static void AppForm_LocationChanged(object sender, EventArgs e)
        {
            Form form = (Form)sender;
            UpdateForm(form);
        }

        public static float getScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
           
            int LogicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCapEnum.DeviceCap.VERTRES);
            int PhysicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCapEnum.DeviceCap.DESKTOPVERTRES);

            g.ReleaseHdc(desktop);

            float ScreenScalingFactor = (float)PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor; // 1.25 = 125%
        }

        public static void ForceUpdate()
        {
            UpdateAll();
        }

        private static void UpdateAll()
        {
            for (int i = 0; i < components.Count; i++)
            {
                IDynamicSized comp = components[i];

                comp.UpdateSize(Scale);
            }
        }

        public static int Adjust(float value, float scale)
        {
            return (int)(value / scale);
        }

        public static void Register(IDynamicSized component)
        {
            components.Add(component);
        }

        public static void Update(IDynamicSized component)
        {
            component.UpdateSize(Scale);
        }

        public static void Unregister(IDynamicSized component)
        {
            components.Remove(component);
        }
    }
}
