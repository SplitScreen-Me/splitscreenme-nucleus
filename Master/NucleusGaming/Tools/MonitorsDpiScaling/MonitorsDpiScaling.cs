using Microsoft.Win32;
using Nucleus.Gaming.Util;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nucleus.Gaming.Tools.MonitorsDpiScaling
{
    public static class MonitorsDpiScaling
    {
        public enum DMDO
        {
            DEFAULT = 0,
            D90 = 1,
            D180 = 2,
            D270 = 3
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct DEVMODE
        {
            public const int DM_PELSWIDTH = 0x80000;
            public const int DM_PELSHEIGHT = 0x100000;
            private const int CCHDEVICENAME = 32;
            private const int CCHFORMNAME = 32;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string dmDeviceName;
            public short dmSpecVersion;
            public short dmDriverVersion;
            public short dmSize;
            public short dmDriverExtra;
            public int dmFields;

            public int dmPositionX;
            public int dmPositionY;
            public DMDO dmDisplayOrientation;
            public int dmDisplayFixedOutput;

            public short dmColor;
            public short dmDuplex;
            public short dmYResolution;
            public short dmTTOption;
            public short dmCollate;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHFORMNAME)]
            public string dmFormName;
            public short dmLogPixels;
            public int dmBitsPerPel;
            public int dmPelsWidth;
            public int dmPelsHeight;
            public int dmDisplayFlags;
            public int dmDisplayFrequency;
            public int dmICMMethod;
            public int dmICMIntent;
            public int dmMediaType;
            public int dmDitherType;
            public int dmReserved1;
            public int dmReserved2;
            public int dmPanningWidth;
            public int dmPanningHeight;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int ChangeDisplaySettings([In] ref DEVMODE lpDevMode, int dwFlags);

        enum DISP_CHANGE : int
        {
            Successful = 0,
            Restart = 1,
            Failed = -1,
            BadMode = -2,
            NotUpdated = -3,
            BadFlags = -4,
            BadParam = -5,
            BadDualView = -6
        }

        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, ref DEVMODE lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern DISP_CHANGE ChangeDisplaySettingsEx(string lpszDeviceName, IntPtr lpDevMode, IntPtr hwnd, uint dwflags, IntPtr lParam);

        [Flags()]
        public enum ChangeDisplaySettingsFlags : uint
        {
            CDS_NONE = 0,
            CDS_UPDATEREGISTRY = 0x00000001,
            CDS_TEST = 0x00000002,
            CDS_FULLSCREEN = 0x00000004,
            CDS_GLOBAL = 0x00000008,
            CDS_SET_PRIMARY = 0x00000010,
            CDS_VIDEOPARAMETERS = 0x00000020,
            CDS_ENABLE_UNSAFE_MODES = 0x00000100,
            CDS_DISABLE_UNSAFE_MODES = 0x00000200,
            CDS_RESET = 0x40000000,
            CDS_RESET_EX = 0x20000000,
            CDS_NORESET = 0x10000000
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);

        private static DISP_CHANGE SetResolution(int w, int h, string deviceName)
        {

            DEVMODE dm = new DEVMODE();

            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmPelsWidth = w;
            dm.dmPelsHeight = h;
            dm.dmFields = DEVMODE.DM_PELSWIDTH | DEVMODE.DM_PELSHEIGHT;
            DISP_CHANGE result = ChangeDisplaySettingsEx(deviceName, ref dm, IntPtr.Zero, (uint)(ChangeDisplaySettingsFlags.CDS_UPDATEREGISTRY | ChangeDisplaySettingsFlags.CDS_RESET), IntPtr.Zero);
            ChangeDisplaySettingsEx(null, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero);

            return result;
        }

        private static string EnumerateSupportedModes(Display displayDevice)
        {
            DEVMODE dm = new DEVMODE();
            dm.dmSize = (short)Marshal.SizeOf(typeof(DEVMODE));
            dm.dmDeviceName = displayDevice.DeviceName;

            int modeIndex = 0; // 0 = The first mode

            string size = "NULL";

            while (EnumDisplaySettings(null, modeIndex, ref dm) == true) // Mode found
            {
                if ((dm.dmPelsWidth > 0 && dm.dmPelsHeight > 0) && (dm.dmPelsWidth != displayDevice.Bounds.Width || dm.dmPelsHeight != displayDevice.Bounds.Height))
                {
                    size = dm.dmPelsWidth + "x" + dm.dmPelsHeight;
                    break;
                }
                else
                {
                    modeIndex++; // The next mode
                }
            }

            return size;
        }

        public static List<Display> screensChanged = new List<Display>();

        public static void SetupMonitors()
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Checking if any monitors to be used by Nucleus are using DPI scaling other than 100%");
            RegistryKey perMonKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop\\PerMonitorSettings", true);
            if (perMonKey != null)
            {
                //Log("TEMP: PerMonitorSettings exist");
                foreach (var v in perMonKey.GetSubKeyNames())
                {
                    //Log("TEMP: Looping through GetSubKeyNames");
                    foreach (Display screen in handlerInstance.screensInUse)
                    {
                        //Log("TEMP: Looping through screensInUse");
                        if (v.ToString().StartsWith(screen.MonitorID))
                        {
                            //Log("TEMP: Found matching monitorID");
                            RegistryKey monitorKey = perMonKey.OpenSubKey(v, true);

                            Point dpi = new Point();
                            bool result = User32Util.GetDPIForMonitor(screen, ref dpi);
                            int calc = dpi.X / 24; // 96 (100%) / 24 = 4
                            int diff = calc - 4;

                    
                            UInt32 currentVal = unchecked((UInt32)((Int32)monitorKey.GetValue("DpiValue")));

                            int newVal = unchecked((int)(currentVal - (UInt32)diff));

                            if (diff != 0)
                            {
                                if (!File.Exists(Path.Combine(Globals.NucleusInstallRoot, $@"utils\backup\{v.ToString()}.reg")))
                                {
                                    handlerInstance.Log($"Backing up monitor settings for {screen.MonitorID}");
                                    RegistryUtil.ExportRegistry($@"HKEY_CURRENT_USER\Control Panel\Desktop\PerMonitorSettings\{v.ToString()}", Path.Combine(Globals.NucleusInstallRoot , $@"utils\backup\{v.ToString()}.reg"));
                                }

                                handlerInstance.Log($"Setting DpiValue for {screen.MonitorID} from {currentVal} to {newVal}");
                                monitorKey.SetValue("DpiValue", newVal, RegistryValueKind.DWord);

                                string modeOutput = EnumerateSupportedModes(screen);

                                if (modeOutput != "NULL")
                                {
                                    int width = Convert.ToInt32(modeOutput.Substring(0, modeOutput.IndexOf("x")));
                                    int height = Convert.ToInt32(modeOutput.Substring(modeOutput.IndexOf("x") + 1));
                                    Thread.Sleep(1000);
                                    SetResolution(width, height, screen.DeviceName);
                                }
                                else
                                {
                                    Thread.Sleep(1000);
                                    SetResolution(800, 600, screen.DeviceName);
                                }

                                Thread.Sleep(1000);
                                SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);

                                screensChanged.Add(screen);
                            }
                        }
                    }
                }
            }
            else
            {
                handlerInstance.Log("PerMonitorSettings does not exist");
                RegistryKey userCpDesktopKey = Registry.CurrentUser.OpenSubKey("Control Panel\\Desktop", true);
                if (userCpDesktopKey != null)
                {
                    //Log("TEMP: Using Control Panel\\Desktop\\ LogPixels and Win8DpiScaling");
                    string origPix = userCpDesktopKey.GetValue("LogPixels", string.Empty).ToString();
                    string origScale = userCpDesktopKey.GetValue("Win8DpiScaling", string.Empty).ToString();

                    if ((!string.IsNullOrEmpty(origPix) && origPix != "96") || (!string.IsNullOrEmpty(origScale) && origScale != "0") || (string.IsNullOrEmpty(origPix) && string.IsNullOrEmpty(origScale)))
                    {
                        handlerInstance.Log($"Setting Windows DPI Scaling to 100% for the duration of Nucleus session - Original LogPixels:{origPix}, DpiScaling:{origScale}");
                        RegistryUtil.ExportRegistry(@"HKEY_CURRENT_USER\Control Panel\Desktop", Path.Combine(Globals.NucleusInstallRoot, @"utils\backup\User Control Panel Desktop.reg"));

                        userCpDesktopKey.SetValue("LogPixels", 96, RegistryValueKind.DWord);
                        userCpDesktopKey.SetValue("Win8DpiScaling", 0, RegistryValueKind.DWord);

                        foreach (Display screen in handlerInstance.screensInUse)
                        {
                            string modeOutput = EnumerateSupportedModes(screen);
                            if (modeOutput != "NULL")
                            {
                                int width = Convert.ToInt32(modeOutput.Substring(0, modeOutput.IndexOf("x")));
                                int height = Convert.ToInt32(modeOutput.Substring(modeOutput.IndexOf("x") + 1));
                                Thread.Sleep(1000);
                                SetResolution(width, height, screen.DeviceName);
                            }
                            else
                            {
                                Thread.Sleep(1000);
                                SetResolution(800, 600, screen.DeviceName);
                            }
                            Thread.Sleep(1000);
                            SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);

                            screensChanged.Add(screen);
                        }
                    }
                }
            }
        }

        public static void ResetMonitorsSettings()
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (screensChanged.Count > 0)
            {
                foreach (Display screen in screensChanged)
                {
                    handlerInstance.Log($"Resetting resolution for {screen.MonitorID} to revert Dpi settings");
                    SetResolution(800, 600, screen.DeviceName);
                    SetResolution(screen.Bounds.Width, screen.Bounds.Height, screen.DeviceName);
                }
            }
        }
    }
}
