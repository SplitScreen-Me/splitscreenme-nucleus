using Nucleus.Gaming.Windows.Interop;
using Nucleus.Interop.User32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static Nucleus.Gaming.Windows.Interop.User32Interop;

namespace Nucleus.Gaming.Windows
{
    public static class User32Util
    {
        [Flags()]
        public enum DisplayDeviceStateFlags : int
        {
            /// <summary>The device is part of the desktop.</summary>
            AttachedToDesktop = 0x1,
            MultiDriver = 0x2,
            /// <summary>The device is part of the desktop.</summary>
            PrimaryDevice = 0x4,
            /// <summary>Represents a pseudo device used to mirror application drawing for remoting or other purposes.</summary>
            MirroringDriver = 0x8,
            /// <summary>The device is VGA compatible.</summary>
            VGACompatible = 0x10,
            /// <summary>The device is removable; it cannot be the primary display.</summary>
            Removable = 0x20,
            /// <summary>The device has more display modes than its output devices support.</summary>
            ModesPruned = 0x8000000,
            Remote = 0x4000000,
            Disconnect = 0x2000000
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DISPLAY_DEVICE
        {
            [MarshalAs(UnmanagedType.U4)]
            public int cb;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
            public string DeviceName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceString;
            [MarshalAs(UnmanagedType.U4)]
            public DisplayDeviceStateFlags StateFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceID;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string DeviceKey;
        }
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool EnumDisplayDevices(string lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);


        public enum DeviceCap
        {
            VERTRES = 10,
            DESKTOPVERTRES = 117,

            // http://pinvoke.net/default.aspx/gdi32/GetDeviceCaps.html
        }

        public static float GetDPIScalingFactor()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            IntPtr desktop = g.GetHdc();
            int LogicalScreenHeight = Gdi32Interop.GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
            int PhysicalScreenHeight = Gdi32Interop.GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);

            float ScreenScalingFactor = PhysicalScreenHeight / (float)LogicalScreenHeight;

            return ScreenScalingFactor;
        }

        public static uint GetDpiForWindow(Form form)
        {
            return User32Interop.GetDpiForWindow(form.Handle);
        }
        public static uint GetDpiForWindow(IntPtr handle)
        {
            return User32Interop.GetDpiForWindow(handle);
        }

        public static bool GetDPIForMonitor(Display display, ref Point dpi)
        {
            Version os = WindowsVersionInfo.Version;

            if (os.Major > 6 || os.Major == 6 && os.Minor >= 3)
            {
                User32Interop.GetDpiForMonitor(display.Handle, User32Interop.DpiType.Effective /*MonitorDpiType.RawDPI*/, out uint dpiX, out uint dpiY);

                dpi = new Point((int)dpiX, (int)dpiY);
                return true;
            }
            else
            {
                int mDpi = (int)(GetDPIScalingFactor() * 96);
                dpi = new Point(mDpi, mDpi);
                return true;
            }
        }


        public static int SetProcessDpiAwareness(ProcessDPIAwareness value)
        {
            //Environment.OSVersion.Version; // reports wrong values
            Version os = WindowsVersionInfo.Version;

            // Windows 8.1 or newer
            if (os.Major > 6 || os.Major == 6 && os.Minor >= 3)
            {
                // We need this, else Windows will fake
                // all the data about monitors inside the application
                return ShcoreInterop.SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
            }
            return 0;
        }

        /// <summary>
        /// Loops through all connected monitors and caches their display information
        /// into an array
        /// </summary>
        /// <returns>Display array</returns>
        public static Display[] GetDisplays()
        {
            List<Display> displays = new List<Display>();
            MonitorEnumProc callback = (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, int d) =>
            {
                MonitorInfoEx info = new MonitorInfoEx();
                info.Size = Marshal.SizeOf(info);
                if (User32Interop.GetMonitorInfo(hMonitor, ref info))
                {
                    Rectangle r = lprcMonitor.ToRectangle();

                    DISPLAY_DEVICE displayDevice = new DISPLAY_DEVICE();
                    displayDevice.cb = Marshal.SizeOf(displayDevice);

                    const int EDD_GET_DEVICE_INTERFACE_NAME = 0x1;

                    uint deviceIndex = 0;
                    string deviceID = string.Empty;
                    string deviceString = string.Empty;
                    string monitorID = string.Empty;
                    int displayIndex = -1;
                    int monitorIndex = -1;

                    while (EnumDisplayDevices(null, deviceIndex, ref displayDevice, 0))
                    {
                        if (info.DeviceName == displayDevice.DeviceName)
                        {
                            DISPLAY_DEVICE monDisplayDevice = new DISPLAY_DEVICE();
                            monDisplayDevice.cb = Marshal.SizeOf(monDisplayDevice);
                            uint monitorNum = 0;
                            EnumDisplayDevices(displayDevice.DeviceName, monitorNum, ref monDisplayDevice, EDD_GET_DEVICE_INTERFACE_NAME);
                            try
                            {
                                deviceID = monDisplayDevice.DeviceID;
                                deviceString = monDisplayDevice.DeviceString;
                                monitorID = deviceID.Substring(deviceID.IndexOf("DISPLAY#") + 8, 7);
                                int.TryParse(info.DeviceName.Substring(info.DeviceName.IndexOf("DISPLAY") + 7),
                                    out displayIndex);
                                int.TryParse(
                                    monDisplayDevice.DeviceName.Substring(
                                        monDisplayDevice.DeviceName.IndexOf("Monitor") + 7), out monitorIndex);

                                monitorNum++;
                                break;
                            }
                            catch (Exception)
                            {

                            }
                        }
                        deviceIndex++;
                    }

                    Display display = new Display(hMonitor, r, info.DeviceName, true, deviceID, deviceString, monitorID, displayIndex, monitorIndex);
                    displays.Add(display);
                }
                return true;
            };

            //deviceIndex = 0;
            //while()
            //{
            //    LogManager.Log(displayDevice.DeviceID);
            //    deviceIndex++;
            //}

            bool result = User32Interop.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, 0);

            if (result)
            {
                return displays.ToArray();
            }
            return null;
        }

        public static void HideBorder(IntPtr handle)
        {
            uint lStyle = User32Interop.GetWindowLong(handle, User32_WS.GWL_STYLE);
            lStyle &= ~(User32_WS.WS_CAPTION | User32_WS.WS_BORDER | User32_WS.WS_DLGFRAME | User32_WS.WS_SIZEBOX | User32_WS.WS_THICKFRAME);
            User32Interop.SetWindowLong(handle, User32_WS.GWL_STYLE, lStyle);
        }
        public static void HideTaskbar()
        {
            IntPtr hwnd = User32Interop.FindWindow("Shell_TrayWnd", "");
            User32Interop.ShowWindow(hwnd, WindowShowStyle.Hide);

            IntPtr hwndOrb = User32Interop.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
            User32Interop.ShowWindow(hwndOrb, WindowShowStyle.Hide);
        }
        public static void MinimizeEverything()
        {
            IntPtr lHwnd = User32Interop.FindWindow("Shell_TrayWnd", null);
            User32Interop.SendMessage(lHwnd, User32_WS.WM_COMMAND, (IntPtr)User32_WS.MIN_ALL, IntPtr.Zero);
        }
        public static void ShowTaskBar()
        {
            IntPtr hwnd = User32Interop.FindWindow("Shell_TrayWnd", "");
            User32Interop.ShowWindow(hwnd, WindowShowStyle.Show);

            IntPtr hwndOrb = User32Interop.FindWindowEx(IntPtr.Zero, IntPtr.Zero, (IntPtr)0xC017, null);
            User32Interop.ShowWindow(hwndOrb, WindowShowStyle.Show);
        }
    }
}
