using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.Windows.Interop;
using Nucleus.Interop.User32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Media;
using WindowScrape.Constants;
using WindowScrape.Static;
using WindowScrape.Types;

namespace Nucleus.Gaming.Tools.GlobalWindowMethods
{
    public static class GlobalWindowMethods
    {
        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT Rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);


        public static bool CutsceneOn = false;
        public static bool TopMostToggle = true;
        public static bool ResetingWindows { get; set; }
        private static bool canSwitchLayout = true;

        public static void ResetBools()
        {
            ResetingWindows = false;
            TopMostToggle = true;
            CutsceneOn = false;
            canSwitchLayout = true;
        }

        public static void ResetWindows(ProcessData processData, int x, int y, int w, int h, int i)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (processData.HWnd == null)
            {
                handlerInstance.profile.DevicesList[i - 1].ProcessData.HWnd = new HwndObject(handlerInstance.profile.DevicesList[i - 1].ProcessData.Process.NucleusGetMainWindowHandle());
                processData.HWnd = handlerInstance.profile.DevicesList[i - 1].ProcessData.HWnd;
                Thread.Sleep(1000);
            }

            handlerInstance.Log("Attempting to reposition, resize and strip borders for instance " + (i - 1) + $" - {processData.Process.ProcessName} (pid {processData.Process.Id})");

            try
            {
                if (!handlerInstance.CurrentGameInfo.DontRemoveBorders)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE);
                    if (handlerInstance.CurrentGameInfo.WindowStyleValues?.Length > 0)
                    {
                        handlerInstance.Log("Using user custom window style");
                        foreach (string val in handlerInstance.CurrentGameInfo.WindowStyleValues)
                        {
                            if (val.StartsWith("~"))
                            {
                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                            }
                            else
                            {
                                lStyle |= Convert.ToUInt32(val, 16);
                            }
                        }
                    }
                    else
                    {
                        lStyle &= ~User32_WS.WS_CAPTION;
                        lStyle &= ~User32_WS.WS_THICKFRAME;
                        lStyle &= ~User32_WS.WS_MINIMIZE;
                        lStyle &= ~User32_WS.WS_MAXIMIZE;
                        lStyle &= ~User32_WS.WS_SYSMENU;

                        lStyle &= ~User32_WS.WS_DLGFRAME;
                        lStyle &= ~User32_WS.WS_BORDER;
                    }
                    int resultCode = User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);

                    lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);

                    if (handlerInstance.CurrentGameInfo.ExtWindowStyleValues?.Length > 0)
                    {
                        handlerInstance.Log("Using user custom extended window style");
                        foreach (string val in handlerInstance.CurrentGameInfo.ExtWindowStyleValues)
                        {
                            if (val.StartsWith("~"))
                            {
                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                            }
                            else
                            {
                                lStyle |= Convert.ToUInt32(val, 16);
                            }
                        }
                    }
                    else
                    {
                        lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                        lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                        lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                    }


                    int resultCode2 = User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);
                    User32Interop.SetWindowPos(processData.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));

                }

                if (!handlerInstance.CurrentGameInfo.DontReposition)
                {
                    processData.HWnd.Location = new Point(x, y);
                }

                if (!handlerInstance.CurrentGameInfo.DontResize)
                {
                    processData.HWnd.Size = new Size(w, h);
                }
            }
            catch (Exception ex)
            {
                handlerInstance.Log("ERROR - Exception in ResetWindows for instance " + (i - 1) + ". " + ex.Message);
            }

            try
            {
                if ((processData.HWnd.Location != new Point(x, y) && !handlerInstance.CurrentGameInfo.DontReposition) || (processData.HWnd.Size != new Size(w, h) && !handlerInstance.CurrentGameInfo.DontResize))
                {
                    handlerInstance.Log("ERROR - ResetWindows was unsuccessful for instance " + (i - 1));
                }
            }
            catch (Exception e)
            {
                handlerInstance.Log("ERROR - Exception in ResetWindows for instance " + (i - 1) + ", error = " + e);
            }
        }

        public static void ChangeGameWindow(Process proc, List<PlayerInfo> players, int playerIndex)
        {
            var handlerInstance = GenericGameHandler.Instance;

            var hwnd = WaitForProcWindowHandleNotZero(proc);

            Point loc = new Point(players[playerIndex].MonitorBounds.X, players[playerIndex].MonitorBounds.Y);
            Size size = new Size(players[playerIndex].MonitorBounds.Width, players[playerIndex].MonitorBounds.Height);

            if (!handlerInstance.CurrentGameInfo.DontRemoveBorders)
            {
                handlerInstance.Log($"Removing game window border for process {proc.ProcessName} (pid {proc.Id})");

                uint lStyle = User32Interop.GetWindowLong(hwnd, User32_WS.GWL_STYLE);
                if (handlerInstance.CurrentGameInfo.WindowStyleValues?.Length > 0)
                {
                    handlerInstance.Log("Using user custom window style");

                    foreach (string val in handlerInstance.CurrentGameInfo.WindowStyleValues)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                }
                else
                {
                    lStyle &= ~User32_WS.WS_CAPTION;
                    lStyle &= ~User32_WS.WS_THICKFRAME;
                    lStyle &= ~User32_WS.WS_MINIMIZE;
                    lStyle &= ~User32_WS.WS_MAXIMIZE;
                    lStyle &= ~User32_WS.WS_SYSMENU;

                    lStyle &= ~User32_WS.WS_DLGFRAME;
                    lStyle &= ~User32_WS.WS_BORDER;
                }
                int resultCode = User32Interop.SetWindowLong(hwnd, User32_WS.GWL_STYLE, lStyle);

                lStyle = User32Interop.GetWindowLong(hwnd, User32_WS.GWL_EXSTYLE);
                if (handlerInstance.CurrentGameInfo.ExtWindowStyleValues?.Length > 0)
                {
                    handlerInstance.Log("Using user custom extended window style");

                    foreach (string val in handlerInstance.CurrentGameInfo.ExtWindowStyleValues)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                }
                else
                {
                    lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                    lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                    lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                }

                int resultCode2 = User32Interop.SetWindowLong(hwnd, User32_WS.GWL_EXSTYLE, lStyle);
                User32Interop.SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
            }

            if (handlerInstance.CurrentGameInfo.KeepAspectRatio || handlerInstance.CurrentGameInfo.KeepMonitorAspectRatio)
            {
                if (handlerInstance.CurrentGameInfo.KeepMonitorAspectRatio)
                {
                    handlerInstance.origWidth = handlerInstance.owner.MonitorBounds.Width;
                    handlerInstance.origHeight = handlerInstance.owner.MonitorBounds.Height;
                }
                else
                {
                    if (GetWindowRect(hwnd, out RECT Rect))
                    {
                        handlerInstance.origWidth = Rect.Right - Rect.Left;
                        handlerInstance.origHeight = Rect.Bottom - Rect.Top;
                    }
                }

                double newWidth = handlerInstance.playerBoundsWidth;
                double newHeight = handlerInstance.playerBoundsHeight;

                if (newWidth < handlerInstance.origWidth)
                {
                    if (handlerInstance.origHeight > 0 && handlerInstance.origWidth > 0)
                    {
                        handlerInstance.origRatio = (double)handlerInstance.origWidth / handlerInstance.origHeight;

                        newHeight = (newWidth / handlerInstance.origRatio);

                        if (newHeight > handlerInstance.playerBoundsWidth)
                        {
                            newHeight = handlerInstance.playerBoundsWidth;
                        }
                    }
                }
                else
                {
                    if (handlerInstance.origHeight > 0 && handlerInstance.origWidth > 0)
                    {
                        handlerInstance.origRatio = (double)handlerInstance.origWidth / handlerInstance.origHeight;

                        newWidth = (newHeight * handlerInstance.origRatio);

                        if (newWidth > handlerInstance.playerBoundsHeight)
                        {
                            newWidth = handlerInstance.playerBoundsHeight;
                        }
                    }
                }
                size.Width = (int)newWidth;
                size.Height = (int)newHeight;

                if (newWidth < handlerInstance.origWidth)
                {
                    int yOffset = Convert.ToInt32(loc.Y + ((handlerInstance.playerBoundsHeight - newHeight) / 2));
                    loc.Y = yOffset;
                }
                if (newHeight < handlerInstance.origHeight)
                {
                    int xOffset = Convert.ToInt32(loc.X + ((handlerInstance.playerBoundsWidth - newWidth) / 2));
                    loc.X = xOffset;
                }
            }

            if (!handlerInstance.CurrentGameInfo.DontResize)
            {
                handlerInstance.Log(string.Format("Resizing this game window and keeping aspect ratio. Values: width:{0}, height:{1}, aspectratio:{2}, origwidth:{3}, origheight:{4}, plyrboundwidth:{5}, plyrboundheight:{6}", size.Width, size.Height, handlerInstance.origRatio, handlerInstance.origWidth, handlerInstance.origHeight, handlerInstance.playerBoundsWidth, handlerInstance.playerBoundsHeight));
                WindowScrape.Static.HwndInterface.SetHwndSize(hwnd, size.Width, size.Height);
            }

            if (!handlerInstance.CurrentGameInfo.DontReposition)
            {
                handlerInstance.Log(string.Format("Repositioning this game window to coords x:{0},y:{1}", loc.X, loc.Y));
                WindowScrape.Static.HwndInterface.SetHwndPos(hwnd, loc.X, loc.Y);
            }

            if (!handlerInstance.CurrentGameInfo.NotTopMost)
            {
                handlerInstance.Log("Setting this game window to top most");
                WindowScrape.Static.HwndInterface.MakeTopMost(hwnd);
            }
        }

        public static void WindowStyleChanges(ProcessData processData)
        {
            var handlerInstance = GenericGameHandler.Instance;

            try
            {
                handlerInstance.Log("WindowStyleChanges called");
                if (handlerInstance.CurrentGameInfo.WindowStyleEndChanges?.Length > 0)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE);
                    handlerInstance.Log("Using user custom window style");
                    foreach (string val in handlerInstance.CurrentGameInfo.WindowStyleEndChanges)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                    User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);
                }

                if (handlerInstance.CurrentGameInfo.ExtWindowStyleEndChanges?.Length > 0)
                {
                    uint lStyle = User32Interop.GetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);
                    handlerInstance.Log("Using user custom extended window style");
                    foreach (string val in handlerInstance.CurrentGameInfo.ExtWindowStyleEndChanges)
                    {
                        if (val.StartsWith("~"))
                        {
                            lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                        }
                        else
                        {
                            lStyle |= Convert.ToUInt32(val, 16);
                        }
                    }
                    User32Interop.SetWindowLong(processData.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);
                }

                User32Interop.SetWindowPos(processData.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
            }
            catch (Exception ex)
            {
                handlerInstance.Log("ERROR - Exception in WindowStyleChanges for process with id" + processData.Process.Id + ". " + ex.Message);
            }
        }

        public static Window CreateRawInputWindow(Process proc, PlayerInfo player)//probably not the best class where to place this :/
        {
            var handlerInstance = GenericGameHandler.Instance;
            handlerInstance.Log("Creating raw input window");

            var hWnd = WaitForProcWindowHandleNotZero(proc);

            var mouseHdev = player.IsRawKeyboard ? player.RawMouseDeviceHandle : (IntPtr)(-1);
            var keyboardHdev = player.IsRawMouse ? player.RawKeyboardDeviceHandle : (IntPtr)(-1);

            var window = new Window(hWnd)
            {
                CursorVisibility = player.IsRawMouse && handlerInstance.CurrentGameInfo.HideCursor && handlerInstance.CurrentGameInfo.DrawFakeMouseCursor,
                KeyboardAttached = keyboardHdev,
                MouseAttached = mouseHdev
            };

            window.CreateHookPipe(handlerInstance.CurrentGameInfo);

            player.RawInputWindow = window;
            RawInputManager.windows.Add(window);

            return window;
        }

        public static IntPtr WaitForProcWindowHandleNotZero(Process proc)
        {
            var handlerInstance = GenericGameHandler.Instance;
            try
            {
                if ((int)proc.NucleusGetMainWindowHandle() == 0)
                {
                    for (int times = 0; times < 200; times++)
                    {
                        Thread.Sleep(500);
                        if ((int)proc.NucleusGetMainWindowHandle() > 0)
                        {
                            break;
                        }

                        if (times == 199 && (int)proc.NucleusGetMainWindowHandle() == 0)
                        {
                            if (!proc.HasExited)
                                handlerInstance.Log(string.Format("ERROR - WaitForProcWindowHandleNotZero could not find main window handle for {0} (pid {1})", proc.ProcessName, proc.Id));
                        }
                    }
                }

                return proc.NucleusGetMainWindowHandle();
            }
            catch
            {
                handlerInstance.Log("ERROR - WaitForProcWindowHandleNotZero encountered an exception");
                return (IntPtr)(-1);
            }
        }
        /// https://stackoverflow.com/questions/20938934/controlling-applications-volume-by-process-id/25584074#25584074
        public class VolumeMixer//Need to be moved to the "audio" class
        {
            public static float? GetApplicationVolume(int pid)
            {
                ISimpleAudioVolume volume = GetVolumeObject(pid);
                if (volume == null)
                    return null;

                float level;
                volume.GetMasterVolume(out level);
                Marshal.ReleaseComObject(volume);
                return level * 100;
            }

            public static bool? GetApplicationMute(int pid)
            {
                ISimpleAudioVolume volume = GetVolumeObject(pid);
                if (volume == null)
                    return null;

                bool mute;
                volume.GetMute(out mute);
                Marshal.ReleaseComObject(volume);
                return mute;
            }

            public static void SetApplicationVolume(int pid, float level)
            {
                ISimpleAudioVolume volume = GetVolumeObject(pid);
                if (volume == null)
                    return;

                Guid guid = Guid.Empty;
                volume.SetMasterVolume(level / 100, ref guid);
                Marshal.ReleaseComObject(volume);
            }

            public static void SetApplicationMute(int pid, bool mute)
            {
                ISimpleAudioVolume volume = GetVolumeObject(pid);
                if (volume == null)
                    return;

                Guid guid = Guid.Empty;
                volume.SetMute(mute, ref guid);
                Marshal.ReleaseComObject(volume);
            }

            private static ISimpleAudioVolume GetVolumeObject(int pid)
            {
                // get the speakers (1st render + multimedia) device
                IMMDeviceEnumerator deviceEnumerator = (IMMDeviceEnumerator)(new MMDeviceEnumerator());
                IMMDevice speakers;
                deviceEnumerator.GetDefaultAudioEndpoint(EDataFlow.eRender, ERole.eMultimedia, out speakers);

                // activate the session manager. we need the enumerator
                Guid IID_IAudioSessionManager2 = typeof(IAudioSessionManager2).GUID;
                object o;
                speakers.Activate(ref IID_IAudioSessionManager2, 0, IntPtr.Zero, out o);
                IAudioSessionManager2 mgr = (IAudioSessionManager2)o;

                // enumerate sessions for on this device
                IAudioSessionEnumerator sessionEnumerator;
                mgr.GetSessionEnumerator(out sessionEnumerator);
                int count;
                sessionEnumerator.GetCount(out count);

                // search for an audio session with the required name
                // NOTE: we could also use the process id instead of the app name (with IAudioSessionControl2)
                ISimpleAudioVolume volumeControl = null;
                for (int i = 0; i < count; i++)
                {
                    IAudioSessionControl2 ctl;
                    sessionEnumerator.GetSession(i, out ctl);
                    int cpid;
                    ctl.GetProcessId(out cpid);

                    if (cpid == pid)
                    {
                        volumeControl = ctl as ISimpleAudioVolume;
                        break;
                    }
                    Marshal.ReleaseComObject(ctl);
                }
                Marshal.ReleaseComObject(sessionEnumerator);
                Marshal.ReleaseComObject(mgr);
                Marshal.ReleaseComObject(speakers);
                Marshal.ReleaseComObject(deviceEnumerator);
                return volumeControl;
            }
        }

        [ComImport]
        [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
        internal class MMDeviceEnumerator
        {
        }

        internal enum EDataFlow
        {
            eRender,
            eCapture,
            eAll,
            EDataFlow_enum_count
        }

        internal enum ERole
        {
            eConsole,
            eMultimedia,
            eCommunications,
            ERole_enum_count
        }

        [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDeviceEnumerator
        {
            int NotImpl1();

            [PreserveSig]
            int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

            // the rest is not implemented
        }

        [Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IMMDevice
        {
            [PreserveSig]
            int Activate(ref Guid iid, int dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

            // the rest is not implemented
        }

        [Guid("77AA99A0-1BD6-484F-8BC7-2C654C9A9B6F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionManager2
        {
            int NotImpl1();
            int NotImpl2();

            [PreserveSig]
            int GetSessionEnumerator(out IAudioSessionEnumerator SessionEnum);

            // the rest is not implemented
        }

        [Guid("E2F5BB11-0570-40CA-ACDD-3AA01277DEE8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionEnumerator
        {
            [PreserveSig]
            int GetCount(out int SessionCount);

            [PreserveSig]
            int GetSession(int SessionCount, out IAudioSessionControl2 Session);
        }

        [Guid("87CE5498-68D6-44E5-9215-6DA47EF883D8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ISimpleAudioVolume
        {
            [PreserveSig]
            int SetMasterVolume(float fLevel, ref Guid EventContext);

            [PreserveSig]
            int GetMasterVolume(out float pfLevel);

            [PreserveSig]
            int SetMute(bool bMute, ref Guid EventContext);

            [PreserveSig]
            int GetMute(out bool pbMute);
        }

        [Guid("bfb7ff88-7239-4fc9-8fa2-07c950be9c6d"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface IAudioSessionControl2
        {
            // IAudioSessionControl
            [PreserveSig]
            int NotImpl0();

            [PreserveSig]
            int GetDisplayName([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetDisplayName([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetIconPath([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int SetIconPath([MarshalAs(UnmanagedType.LPWStr)] string Value, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int GetGroupingParam(out Guid pRetVal);

            [PreserveSig]
            int SetGroupingParam([MarshalAs(UnmanagedType.LPStruct)] Guid Override, [MarshalAs(UnmanagedType.LPStruct)] Guid EventContext);

            [PreserveSig]
            int NotImpl1();

            [PreserveSig]
            int NotImpl2();

            // IAudioSessionControl2
            [PreserveSig]
            int GetSessionIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetSessionInstanceIdentifier([MarshalAs(UnmanagedType.LPWStr)] out string pRetVal);

            [PreserveSig]
            int GetProcessId(out int pRetVal);

            [PreserveSig]
            int IsSystemSoundsSession();

            [PreserveSig]
            int SetDuckingPreference(bool optOut);
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);   

        public static void SwitchLayout()
        {
            var profile = GenericGameHandler.Instance.profile;

            if (profile == null || !GameProfile.Saved || !canSwitchLayout || ResetingWindows)
            {
                Globals.MainOSD.Show(1600, $"Can't Be Used Now");
                return;
            }

            List<PlayerInfo> players = profile.DevicesList;
            bool adjust = GameProfile.UseSplitDiv;

            var screens = ScreensUtil.AllScreens();
            //PlayerInfo prev = null;

            //int shift = -2;
            int reset = 0;//set to 1  reset debugging purpose

            if (reset == 1)
            {
                foreach (PlayerInfo pl in players)
                {
                    pl.OtherLayout = null;
                    pl.CurrentLayout = 0;
                    pl.MonitorBounds = new Rectangle(pl.ProcessData.HWnd.Location, pl.ProcessData.HWnd.Size);
                }
                return;
            }

            for (int i = 0; i < players.Count; i++)
            {
                PlayerInfo p = players[i];

                ProcessData data = p.ProcessData;

                if (data == null)
                {
                    continue;
                }

                if (p.OtherLayout == null)
                {
                    p.OtherLayout = new List<Rectangle>();
                }

                // data.HWnd.TopMost = false;//debug

                Rectangle ownerBounds = WindowsMerger.Instance == null ? new Rectangle(p.Owner.MonitorBounds.X, p.Owner.MonitorBounds.Y, p.Owner.MonitorBounds.Width, p.Owner.MonitorBounds.Height)
                                                                       : WindowsMerger.Instance.WindowBounds;

                Rectangle localizeFirstOfScr = new Rectangle(ownerBounds.Location.X, ownerBounds.Location.Y, 20, 20);
                Rectangle playerWindow = new Rectangle(data.HWnd.Location.X, data.HWnd.Location.Y, data.HWnd.Size.Width, data.HWnd.Size.Height);
                bool fisrtOfScr = localizeFirstOfScr.IntersectsWith(playerWindow);

                if (data.HWnd.Location.X <= -32000)//if minimized or in cutscenes mode
                {
                    return;
                }

                if (p.Owner.Type == UserScreenType.DualHorizontal)
                {
                    Globals.MainOSD.Show(1600, $"Switching Layouts");

                    foreach (UserScreen screen in screens)
                    {                        
                        if (screen.DisplayIndex == p.Owner.DisplayIndex)
                        {
                            Rectangle screenBounds = WindowsMerger.Instance == null ? new Rectangle(screen.MonitorBounds.X, screen.MonitorBounds.Y, screen.MonitorBounds.Width, screen.MonitorBounds.Height)
                                                                       : WindowsMerger.Instance.WindowBounds;
                            if (fisrtOfScr)
                            {
                                Rectangle Win = new Rectangle(adjust ? ownerBounds.Location.X + 1 : ownerBounds.Location.X,
                                                              adjust ? ownerBounds.Location.Y + 1 : p.Owner.MonitorBounds.Location.Y,
                                                              adjust ? (ownerBounds.Width / 2) - 3 : ownerBounds.Width / 2,
                                                              adjust ? ownerBounds.Height - 2 : ownerBounds.Height);
                                data.HWnd.Size = Win.Size;
                                data.HWnd.Location = Win.Location;
                                p.MonitorBounds = Win;

                                continue;
                            }
                            else
                            {
                                Rectangle Win = new Rectangle(adjust ? (screenBounds.X + (screenBounds.Width / 2)) + 1 : screenBounds.X + screenBounds.Width / 2,
                                                              adjust ? screenBounds.Y + 1 : screenBounds.Y,
                                                              adjust ? (screenBounds.Width / 2) - 4 : screenBounds.Width / 2,
                                                              adjust ? screenBounds.Height - 2 : screenBounds.Height);
                                data.HWnd.Size = Win.Size;
                                data.HWnd.Location = Win.Location;
                                p.MonitorBounds = Win;
                                // break;
                            }

                            p.Owner.Type = UserScreenType.DualVertical;
                        }
                    }                
                }
                else if (p.Owner.Type == UserScreenType.DualVertical)
                {
                    Globals.MainOSD.Show(1600, $"Switching Layouts");
                    foreach (UserScreen screen in ScreensUtil.AllScreens())
                    {
                        //Console.WriteLine(p.Owner.MonitorBounds + " " + screen.MonitorBounds);
                        if (screen.DisplayIndex == p.Owner.DisplayIndex)
                        {
                            Rectangle screenBounds = WindowsMerger.Instance == null ? new Rectangle(screen.MonitorBounds.X, screen.MonitorBounds.Y, screen.MonitorBounds.Width, screen.MonitorBounds.Height)
                                                                       : WindowsMerger.Instance.WindowBounds;

                            if (fisrtOfScr)
                            {
                                Rectangle Win = new Rectangle(adjust ? ownerBounds.X + 2 : ownerBounds.X,
                                                               adjust ? ownerBounds.Y + 2 : ownerBounds.Y,
                                                               adjust ? ownerBounds.Width - 4 : ownerBounds.Width,
                                                               adjust ? (ownerBounds.Height / 2) - 2 : ownerBounds.Height / 2);
                                data.HWnd.Size = Win.Size;
                                data.HWnd.Location = Win.Location;
                                p.MonitorBounds = Win;

                                continue;
                            }
                            else
                            {

                                Rectangle Win = new Rectangle(adjust ? screenBounds.X + 2 : screenBounds.X,
                                                              adjust ? (screenBounds.Y + (screenBounds.Height / 2)) + 2 : screenBounds.Y + screenBounds.Height / 2,
                                                              adjust ? screenBounds.Width - 4 : screenBounds.Width,
                                                              adjust ? (screenBounds.Height / 2) - 4 : screenBounds.Height / 2);

                                data.HWnd.Size = Win.Size;
                                data.HWnd.Location = Win.Location;
                                p.MonitorBounds = Win;
                            }

                            p.Owner.Type = UserScreenType.DualHorizontal;                       
                        }
                    }
             
                }

                //else if (p.Owner.Type == UserScreenType.FourPlayers && !switchingLayout)
                //{

                //    foreach (UserScreen screen in ScreensUtil.AllScreens())
                //    {
                //        if (screen.DisplayIndex == p.Owner.DisplayIndex)
                //        {

                //            p.OtherLayout.Add(p.MonitorBounds);
                //            p.CurrentLayout++;//Skip Original default bounds on first swap

                //            if (fisrtOfScr)//Player 0
                //            {
                //                //All Vertical
                //                Rectangle avail1 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                 adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                 adjust ? (screen.MonitorBounds.Width / p.Owner.PlayerOnScreen) - 3 : screen.MonitorBounds.Width / p.Owner.PlayerOnScreen,
                //                                                 adjust ? p.Owner.display.Height - 4 : p.Owner.display.Height);
                //                p.OtherLayout.Add(avail1);

                //                //All Horizontal
                //                Rectangle avail2 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                 adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                 adjust ? screen.MonitorBounds.Width - 4 : screen.MonitorBounds.Width,
                //                                                 adjust ? (screen.MonitorBounds.Height / p.Owner.PlayerOnScreen) - 3 : screen.MonitorBounds.Height / p.Owner.PlayerOnScreen);
                //                p.OtherLayout.Add(avail2);

                //                //Four player Layout but with only 3 players
                //                if (p.Owner.PlayerOnScreen == 3)
                //                {

                //                    // Top player full Width
                //                    Rectangle avail3 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                     adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                     adjust ? screen.MonitorBounds.Width - 4 : screen.MonitorBounds.Width,
                //                                                     adjust ? (screen.MonitorBounds.Height / 2) - 2 : screen.MonitorBounds.Height / 2);
                //                    p.OtherLayout.Add(avail3);

                //                    //Full height vertical left
                //                    Rectangle avail4 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                     adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                     adjust ? (screen.MonitorBounds.Width / 2) - 2 : screen.MonitorBounds.Width / 2,
                //                                                     adjust ? screen.MonitorBounds.Height - 3 : screen.MonitorBounds.Height);
                //                    p.OtherLayout.Add(avail4);


                //                    //Top left
                //                    Rectangle avail5 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                     adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                     adjust ? (screen.MonitorBounds.Width / 2) - 2 : screen.MonitorBounds.Width / 2,
                //                                                     adjust ? (screen.MonitorBounds.Height / 2) - 2 : screen.MonitorBounds.Height / 2);
                //                    p.OtherLayout.Add(avail5);

                //                    //Top left
                //                    Rectangle avail6 = avail5;

                //                    p.OtherLayout.Add(avail6);

                //                }

                //                shift += 3;

                //                prev = p;

                //                continue;
                //            }
                //            else//All players above 0
                //            {
                //                //All Vertical
                //                Rectangle avail1 = new Rectangle(adjust ? prev.OtherLayout[1].Right + 3 : prev.OtherLayout[1].Right,
                //                                                 adjust ? screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y,
                //                                                 adjust ? prev.OtherLayout[1].Width : prev.OtherLayout[1].Width,
                //                                                 adjust ? prev.OtherLayout[1].Height : prev.OtherLayout[1].Height);
                //                p.OtherLayout.Add(avail1);

                //                //All Horizontal
                //                Rectangle avail2 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                 adjust ? prev.OtherLayout[2].Bottom + 2 : prev.OtherLayout[2].Bottom,
                //                                                 adjust ? prev.OtherLayout[2].Width : prev.OtherLayout[2].Width,
                //                                                 adjust ? prev.OtherLayout[2].Height : prev.OtherLayout[2].Height);
                //                p.OtherLayout.Add(avail2);

                //                if (i >= 1 && prev.DisplayIndex == p.DisplayIndex)
                //                {
                //                    prev = p;
                //                }

                //                //Four player Layout but with only 3 players//
                //                //Top player full Width
                //                if (p.Owner.PlayerOnScreen == 3)
                //                {

                //                    //if (i == 1 || i == 4)// 
                //                    if (i == shift)
                //                    {
                //                        //Bottom left
                //                        Rectangle avail3 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                         adjust ? (screen.MonitorBounds.Y + (screen.MonitorBounds.Height / 2)) + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Height / 2,
                //                                                         adjust ? (screen.MonitorBounds.Width / 2) - 3 : screen.MonitorBounds.Width / 2,
                //                                                         adjust ? (screen.MonitorBounds.Height / 2) - 4 : screen.MonitorBounds.Height / 2);
                //                        p.OtherLayout.Add(avail3);

                //                        //Top right
                //                        Rectangle avail4 = new Rectangle(adjust ? (screen.MonitorBounds.X + (screen.MonitorBounds.Width / 2)) + 2 : screen.MonitorBounds.X + screen.MonitorBounds.Width / 2,
                //                                                         adjust ? screen.MonitorBounds.Y + screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Y,
                //                                                         adjust ? (screen.MonitorBounds.Width / 2) - 4 : screen.MonitorBounds.Width / 2,
                //                                                         adjust ? (screen.MonitorBounds.Height / 2) - 2 : screen.MonitorBounds.Height / 2);
                //                        p.OtherLayout.Add(avail4);

                //                        //Top right
                //                        Rectangle avail5 = avail4;

                //                        p.OtherLayout.Add(avail5);

                //                        //Right full height
                //                        Rectangle avail6 = new Rectangle(adjust ? (screen.MonitorBounds.X + (screen.MonitorBounds.Width / 2)) + 2 : screen.MonitorBounds.X + screen.MonitorBounds.Width / 2,
                //                                                         adjust ? screen.MonitorBounds.Y + screen.MonitorBounds.Y + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Y,
                //                                                         adjust ? (screen.MonitorBounds.Width / 2) - 3 : screen.MonitorBounds.Width / 2,
                //                                                         adjust ? screen.MonitorBounds.Height - 3 : screen.MonitorBounds.Height);
                //                        p.OtherLayout.Add(avail6);
                //                    }
                //                    else if (i == shift + 1)//f(i == 2 || i == 5)
                //                    {
                //                        //Bottom right
                //                        Rectangle avail3 = new Rectangle(adjust ? screen.MonitorBounds.X + (screen.MonitorBounds.Width / 2) + 2 : screen.MonitorBounds.X + screen.MonitorBounds.Width / 2,
                //                                                         adjust ? (screen.MonitorBounds.Y + (screen.MonitorBounds.Height / 2)) + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Height / 2,
                //                                                         adjust ? (screen.MonitorBounds.Width / 2) - 4 : screen.MonitorBounds.Width / 2,
                //                                                         adjust ? (screen.MonitorBounds.Height / 2) - 4 : screen.MonitorBounds.Height / 2);
                //                        p.OtherLayout.Add(avail3);

                //                        //Bottom right
                //                        Rectangle avail4 = avail3;

                //                        p.OtherLayout.Add(avail4);

                //                        //Bottom full width
                //                        Rectangle avail5 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                         adjust ? (screen.MonitorBounds.Y + (screen.MonitorBounds.Height / 2)) + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Height / 2,
                //                                                         adjust ? screen.MonitorBounds.Width - 4 : screen.MonitorBounds.Width,
                //                                                         adjust ? (screen.MonitorBounds.Height / 2) - 3 : screen.MonitorBounds.Height / 2);
                //                        p.OtherLayout.Add(avail5);

                //                        //Bottom left
                //                        Rectangle avail6 = new Rectangle(adjust ? screen.MonitorBounds.X + 2 : screen.MonitorBounds.X,
                //                                                         adjust ? (screen.MonitorBounds.Y + (screen.MonitorBounds.Height / 2)) + 2 : screen.MonitorBounds.Y + screen.MonitorBounds.Height / 2,
                //                                                         adjust ? (screen.MonitorBounds.Width / 2) - 2 : screen.MonitorBounds.Width / 2,
                //                                                         adjust ? (screen.MonitorBounds.Height / 2) - 3 : screen.MonitorBounds.Height / 2);
                //                        p.OtherLayout.Add(avail6);

                //                    }
                //                }
                //            }

                //            break;
                //        }

                //    }

                //    if (i == players.Count - 1)
                //    {
                //        switchingLayout = true;
                //    }
                //}

            }

            //for (int i = 0; i < players.Count; i++)
            //{
            //    PlayerInfo p = players[i];
            //    ProcessData data = p.ProcessData;

            //    //Swaping 4p layout here 
            //    if (p.Owner.Type == UserScreenType.FourPlayers)
            //    {
            //        if (p.OtherLayout.Count == 0)
            //        {
            //            Console.WriteLine("No other Layout found");
            //            return;
            //        }

            //        data.HWnd.Size = p.OtherLayout[p.CurrentLayout].Size;
            //        data.HWnd.Location = p.OtherLayout[p.CurrentLayout].Location;
            //        p.MonitorBounds = new Rectangle(data.HWnd.Location, data.HWnd.Size);
            //        //Console.WriteLine(i +": "+p.OtherLayout.Count);
            //        if (p.CurrentLayout == p.OtherLayout.Count() - 1)
            //        {
            //            p.CurrentLayout = 0;
            //        }
            //        else
            //        {
            //            p.CurrentLayout++;
            //        }
            //    }
            //}

           
        }

        #region Cutscene mode function
        public static void ToggleCutScenesMode()
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.profile == null || !GameProfile.Saved || ResetingWindows)
            {
                Globals.MainOSD.Show(1600, $"Can't Be Used Now");
                return;
            }

            List<PlayerInfo> players = handlerInstance.profile.DevicesList;

            List<UserScreen> screens = handlerInstance.profile.Screens;

            foreach (UserScreen screen in screens)
            {
                PlayerInfo p = handlerInstance.profile.DevicesList.Where(pl => pl.DefaultMonitorBounds.IntersectsWith(screen.SubScreensBounds.ElementAt(0).Key)).First();
                ProcessData data = p.ProcessData;//datas of first player (of each screen) 

                if (data == null)
                {
                    Globals.MainOSD.Show(1600, $"Can't Be Used Now");
                    return;
                }

                Rectangle ownerBounds = WindowsMerger.Instance == null ? new Rectangle(p.Owner.MonitorBounds.X, p.Owner.MonitorBounds.Y, p.Owner.MonitorBounds.Width, p.Owner.MonitorBounds.Height)
                                                                        : WindowsMerger.Instance.WindowBounds;
                Point hiddenWindowsLoc = WindowsMerger.Instance == null ? new Point(-32000, -32000) : new Point(ownerBounds.Right, 1);

                if (!CutsceneOn)
                {
                    //First player on this screen
                    if (!GameProfile.Cts_KeepAspectRatio && !GameProfile.Cts_MuteAudioOnly)//will set player 0 window fullscreened.
                    {
                        Rectangle setfullScreen = ownerBounds;
                        data.HWnd.Size = setfullScreen.Size;
                        data.HWnd.Location = new Point(setfullScreen.X, setfullScreen.Y);
                    }
                    else if (!GameProfile.Cts_MuteAudioOnly)//will keep player window size and center it on screen.
                    {
                        Rectangle centeredOnly = RectangleUtil.Center(p.MonitorBounds, ownerBounds);
                        data.HWnd.Size = centeredOnly.Size;
                        data.HWnd.Location = centeredOnly.Location;
                    }


                    float? hostPlayerAudioVolume = VolumeMixer.GetApplicationVolume(p.ProcessData.Process.Id);
                    p.ProcessData.AudioVolume = hostPlayerAudioVolume == null ? 100 : hostPlayerAudioVolume.Value;
                    
                    if(WindowsMerger.Instance != null)
                    {
                        HwndInterface.MakeTop(data.HWnd.NativePtr);
                    }
                   
                    //All other players on this screen
                    foreach (PlayerInfo player in players.Where(player => player.ProcessData != null && player != p && player.ScreenIndex == p.ScreenIndex))
                    {
                        if (!GameProfile.Cts_MuteAudioOnly)
                        {
                            Rectangle hiddenWindow = new Rectangle(p.MonitorBounds.X, p.MonitorBounds.Y, p.MonitorBounds.Width, p.MonitorBounds.Height);
                            player.ProcessData.HWnd.Location = hiddenWindowsLoc;
                            player.ProcessData.HWnd.Size = hiddenWindow.Size;                
                        }

                        float? otherPlayerAudioVolume = VolumeMixer.GetApplicationVolume(player.ProcessData.Process.Id);
                        player.ProcessData.AudioVolume = otherPlayerAudioVolume == null ? 100 : otherPlayerAudioVolume.Value;

                        VolumeMixer.SetApplicationVolume(player.ProcessData.Process.Id, 0.0f);
                    }

                    canSwitchLayout = false;
                    Globals.MainOSD.Show(1600, "Cutscenes Mode On");
                }
                else//Reset all player windows and unmute
                {
                    //First player on this screen
                    if (!GameProfile.Cts_MuteAudioOnly)
                    {
                        Rectangle resizeReposition = new Rectangle(p.MonitorBounds.X, p.MonitorBounds.Y, p.MonitorBounds.Width, p.MonitorBounds.Height);
                        data.HWnd.Location = resizeReposition.Location;
                        data.HWnd.Size = resizeReposition.Size;
                    }

                    VolumeMixer.SetApplicationVolume(p.ProcessData.Process.Id, (float)p.ProcessData.AudioVolume);

                    //All other players on this screen
                    foreach (PlayerInfo player in players.Where(player => player.ProcessData != null && player != p && player.ScreenIndex == p.ScreenIndex))
                    {
                        if (!GameProfile.Cts_MuteAudioOnly)//Reset all other player window
                        {
                            Rectangle reposition = new Rectangle(player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height);

                            player.ProcessData.HWnd.Location = reposition.Location;
                            player.ProcessData.HWnd.Size = reposition.Size;
                        }

                        VolumeMixer.SetApplicationVolume(player.ProcessData.Process.Id, (float)player.ProcessData.AudioVolume);
                    }

                    canSwitchLayout = true;

                    Globals.MainOSD.Show(1600, "Cutscenes Mode Off");

                    if (handlerInstance.CurrentGameInfo.SetForegroundWindowElsewhere || GameProfile.Cts_Unfocus)
                    {
                        ChangeForegroundWindow();
                    }
                }
            }

            if (!CutsceneOn)
            {
                CutsceneOn = true;
            }
            else
            {
                CutsceneOn = false;
            }
        }
        #endregion

        public static void UpdateAndRefreshGameWindows(double delayMS, bool refresh)
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance.profile == null)
            {
                return;
            }

            handlerInstance.exited = 0;

            List<PlayerInfo> players = handlerInstance.profile.DevicesList;

            handlerInstance.Timer += delayMS;

            bool updatedHwnd = false;

            if (handlerInstance.Timer > handlerInstance.HWndInterval)
            {
                updatedHwnd = true;
                handlerInstance.Timer = 0;
            }

            Application.DoEvents();
 
            for (int i = 0; i < players.Count; i++)
            {             
                PlayerInfo p = players[i];
                ProcessData data = p.ProcessData;

                if (data == null)
                {
                    continue;
                }
                
                if (ResetingWindows && data.Finished)
                {
                    Globals.MainOSD.Show(100000, "Resetting game windows. Please wait...");
                    data.HWNDRetry = false;
                    data.Setted = false;
                    data.Finished = false;
                    data.Status = 0;
                }

                if (data.Finished)
                {
                    if (data.Process.HasExited)
                    {
                        handlerInstance.exited++;
                    }

                    continue;
                }
            
                if (p.SteamEmu)
                {
                    List<int> children = ProcessUtil.GetChildrenProcesses(data.Process);

                    // catch the game process, that was spawned from Smart Steam Emu
                    if (children.Count > 0)
                    {
                        for (int j = 0; j < children.Count; j++)
                        {
                            int id = children[j];
                            Process child = Process.GetProcessById(id);
                            try
                            {
                                if (child.ProcessName.Contains("conhost"))
                                {
                                    continue;
                                }
                            }
                            catch
                            {
                                continue;
                            }

                            data.AssignProcess(child);
                            p.SteamEmu = child.ProcessName.Contains("SmartSteamLoader") || child.ProcessName.Contains("cmd");
                        }
                    }
                }
                else
                {
                    if (updatedHwnd)
                    {
                        if (data.Setted)
                        {
                            if (data.Process.HasExited)
                            {
                                handlerInstance.exited++;
                                continue;
                            }

                            if (!handlerInstance.CurrentGameInfo.PromptBetweenInstances)
                            {
                                if (!handlerInstance.CurrentGameInfo.NotTopMost)
                                {
                                    if (!handlerInstance.CurrentGameInfo.SetTopMostAtEnd)
                                    {
                                        handlerInstance.Log("(Update) Setting game window to top most");
                                        data.HWnd.TopMost = true;
                                    }
                                }
                            }

                            if (data.Status == 2)
                            {
                                if (!handlerInstance.CurrentGameInfo.DontRemoveBorders)
                                {
                                    handlerInstance.Log("(Update) Removing game window border for pid " + data.Process.Id);

                                    uint lStyle = User32Interop.GetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_STYLE);

                                    if (handlerInstance.CurrentGameInfo.WindowStyleValues?.Length > 0)
                                    {
                                        handlerInstance.Log("(Update) Using user custom window style");
                                        foreach (string val in handlerInstance.CurrentGameInfo.WindowStyleValues)
                                        {
                                            if (val.StartsWith("~"))
                                            {
                                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                                            }
                                            else
                                            {
                                                lStyle |= Convert.ToUInt32(val, 16);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lStyle &= ~User32_WS.WS_CAPTION;
                                        lStyle &= ~User32_WS.WS_THICKFRAME;
                                        lStyle &= ~User32_WS.WS_MINIMIZE;
                                        lStyle &= ~User32_WS.WS_MAXIMIZE;
                                        lStyle &= ~User32_WS.WS_SYSMENU;

                                        lStyle &= ~User32_WS.WS_DLGFRAME;
                                        lStyle &= ~User32_WS.WS_BORDER;
                                    }

                                    int resultCode = User32Interop.SetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_STYLE, lStyle);

                                    lStyle = User32Interop.GetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_EXSTYLE);

                                    if (handlerInstance.CurrentGameInfo.ExtWindowStyleValues?.Length > 0)
                                    {
                                        handlerInstance.Log("(Update) Using user custom extended window style");

                                        foreach (string val in handlerInstance.CurrentGameInfo.ExtWindowStyleValues)
                                        {
                                            if (val.StartsWith("~"))
                                            {
                                                lStyle &= ~Convert.ToUInt32(val.Substring(1), 16);
                                            }
                                            else
                                            {
                                                lStyle |= Convert.ToUInt32(val, 16);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        lStyle &= ~User32_WS.WS_EX_DLGMODALFRAME;
                                        lStyle &= ~User32_WS.WS_EX_CLIENTEDGE;
                                        lStyle &= ~User32_WS.WS_EX_STATICEDGE;
                                    }

                                    int resultCode2 = User32Interop.SetWindowLong(data.HWnd.NativePtr, User32_WS.GWL_EXSTYLE, lStyle);

                                    User32Interop.SetWindowPos(data.HWnd.NativePtr, IntPtr.Zero, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_FRAMECHANGED | PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER | PositioningFlags.SWP_NOOWNERZORDER));
                                }

                                if (handlerInstance.CurrentGameInfo.EnableWindows)
                                {
                                    EnableWindow(data.HWnd.NativePtr, true);
                                }

                                //Minimise and un-minimise the window. Fixes black borders in Minecraft, but causing stretching issues in games like Minecraft.
                                if (handlerInstance.CurrentGameInfo.RefreshWindowAfterStart)
                                {
                                    handlerInstance.Log("Refreshing game window");
                                    ShowWindow(data.HWnd.NativePtr, 6);
                                    ShowWindow(data.HWnd.NativePtr, 9);

                                    if (handlerInstance.CurrentGameInfo.SetForegroundWindowElsewhere)
                                    {
                                        ChangeForegroundWindow();
                                    }
                                }

                                if (handlerInstance.CurrentGameInfo.WindowStyleEndChanges?.Length > 0 || handlerInstance.CurrentGameInfo.ExtWindowStyleEndChanges?.Length > 0)
                                {
                                    Thread.Sleep(1000);
                                    WindowStyleChanges(data);
                                }

                               
                                Debug.WriteLine("State 2");
                                handlerInstance.Log("Update State 2");

                                if (i == (players.Count - 1))
                                {
                                    if (handlerInstance.CurrentGameInfo.LockMouse)
                                    {
                                        handlerInstance._cursorModule.SetActiveWindow();
                                    }

                                    if (ResetingWindows)
                                    {
                                        if (handlerInstance.CurrentGameInfo.SetForegroundWindowElsewhere)
                                        {
                                            ChangeForegroundWindow();
                                        }

                                        Globals.MainOSD.Show(2000, "Game Windows Resetted");
                                        ResetingWindows = false;
                                    }                
                                }

                                data.Finished = true;
                            }
                            else if (data.Status == 1)
                            {
                                if (!handlerInstance.CurrentGameInfo.KeepAspectRatio && !handlerInstance.CurrentGameInfo.KeepMonitorAspectRatio && !handlerInstance.dllRepos && !handlerInstance.CurrentGameInfo.DontResize)
                                {
                                    if (data.Position.X != p.MonitorBounds.X || data.Position.Y != p.MonitorBounds.Y)
                                    {
                                        handlerInstance.Log("(Update) Data position X or Y does not match player bounds for pid " + data.Process.Id + ", using player bound variables");
                                        handlerInstance.Log(string.Format("(Update) Repostioning game window for pid {0} to coords x:{1},y:{2}", data.Process.Id, p.MonitorBounds.X, p.MonitorBounds.Y));
                                        data.HWnd.Location = new Point(p.MonitorBounds.X, p.MonitorBounds.Y);
                                    }
                                    else
                                    {
                                        handlerInstance.Log(string.Format("(Update) Repostioning game window for pid {0} to coords x:{1},y:{2}", data.Process.Id, data.Position.X, data.Position.Y));
                                        data.HWnd.Location = data.Position;
                                    }
                                }

                                data.Status++;
                                handlerInstance.Log("Update State 1");

                                if (handlerInstance.CurrentGameInfo.LockMouse)
                                {
                                    if (p.IsKeyboardPlayer && !p.IsRawKeyboard)
                                    {
                                        handlerInstance._cursorModule.Setup(data.Process, p.MonitorBounds);
                                    }
                                    else
                                    {
                                        handlerInstance._cursorModule.AddOtherGameHandle(data.Process.NucleusGetMainWindowHandle());
                                    }
                                }                           
                            }
                            else if (data.Status == 0)
                            {
                                if (ResetingWindows)
                                {
                                    //no proof of it having an effect.
                                    if (!handlerInstance.CurrentGameInfo.IgnoreWindowBorderCheck)
                                    {
                                        RemoveBorder();
                                    }
                                }

                                if (!handlerInstance.dllResize && !handlerInstance.CurrentGameInfo.DontResize)
                                {
                                    if (handlerInstance.CurrentGameInfo.KeepAspectRatio || handlerInstance.CurrentGameInfo.KeepMonitorAspectRatio)
                                    {
                                        if (handlerInstance.CurrentGameInfo.KeepMonitorAspectRatio)
                                        {
                                            handlerInstance.origWidth = handlerInstance.owner.MonitorBounds.Width;
                                            handlerInstance.origHeight = handlerInstance.owner.MonitorBounds.Height;
                                        }
                                        else
                                        {
                                            if (GetWindowRect(data.Process.NucleusGetMainWindowHandle(), out RECT Rect))
                                            {
                                                handlerInstance.origWidth = Rect.Right - Rect.Left;
                                                handlerInstance.origHeight = Rect.Bottom - Rect.Top;
                                            }
                                        }

                                        double newWidth = handlerInstance.playerBoundsWidth;
                                        double newHeight = handlerInstance.playerBoundsHeight;

                                        if (newWidth < handlerInstance.origWidth)
                                        {
                                            if (handlerInstance.origHeight > 0 && handlerInstance.origWidth > 0)
                                            {
                                                handlerInstance.origRatio = (double)handlerInstance.origWidth / handlerInstance.origHeight;

                                                newHeight = (newWidth / handlerInstance.origRatio);

                                                if (newHeight > handlerInstance.playerBoundsHeight)
                                                {
                                                    newHeight = handlerInstance.playerBoundsHeight;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (handlerInstance.origHeight > 0 && handlerInstance.origWidth > 0)
                                            {
                                                handlerInstance.origRatio = (double)handlerInstance.origWidth / handlerInstance.origHeight;

                                                newWidth = (newHeight * handlerInstance.origRatio);

                                                if (newWidth > handlerInstance.playerBoundsWidth)
                                                {
                                                    newWidth = handlerInstance.playerBoundsWidth;
                                                }
                                            }
                                        }

                                        handlerInstance.Log(string.Format("(Update) Resizing game window for pid {0} and keeping aspect ratio. Values: width:{1}, height:{2}, aspectratio:{3}, origwidth:{4}, origheight:{5}, plyrboundwidth:{6}, plyrboundheight:{7}", data.Process.Id, (int)newWidth, (int)newHeight, (Math.Truncate(handlerInstance.origRatio * 100) / 100), handlerInstance.origWidth, handlerInstance.origHeight, handlerInstance.playerBoundsWidth, handlerInstance.playerBoundsHeight));
                                        data.HWnd.Size = new Size(Convert.ToInt32(newWidth), Convert.ToInt32(newHeight));

                                        //x horizontal , y vertical
                                        if (newWidth < handlerInstance.origWidth)
                                        {
                                            int yOffset = Convert.ToInt32(data.Position.Y + ((handlerInstance.playerBoundsHeight - newHeight) / 2));
                                            data.Position.Y = yOffset;
                                        }
                                        if (newHeight < handlerInstance.origHeight)
                                        {
                                            int xOffset = Convert.ToInt32(data.Position.X + ((handlerInstance.playerBoundsWidth - newWidth) / 2));
                                            data.Position.X = xOffset;
                                        }

                                        handlerInstance.Log(string.Format("(Update) Resizing game window (for horizontal centering), coords x:{1},y:{2}", data.Process.Id, data.Position.X, data.Position.Y));
                                        data.HWnd.Location = data.Position;
                                    }
                                    else
                                    {
                                        if (data.Size.Width == 0 || data.Size.Height == 0)
                                        {
                                            handlerInstance.Log("(Update) Data size width or height is showing as 0 for pid " + data.Process.Id + ", using player bound variables");
                                            data.Size.Width = handlerInstance.playerBoundsWidth;
                                            data.Size.Height = handlerInstance.playerBoundsHeight;

                                            if (handlerInstance.playerBoundsWidth == 0 || handlerInstance.playerBoundsHeight == 0)
                                            {
                                                handlerInstance.Log("(Update) Play bounds width or height is showing as 0 for pid " + data.Process.Id + ", using monitor bound variables");
                                                data.Size.Width = p.MonitorBounds.Width;
                                                data.Size.Height = p.MonitorBounds.Height;
                                            }
                                        }

                                        handlerInstance.Log(string.Format("(Update) Resizing game window for pid {0} to the following width:{1}, height:{2}", data.Process.Id, data.Size.Width, data.Size.Height));
                                        data.HWnd.Size = data.Size;
                                    }
                                }

                                data.Status++;
                                handlerInstance.Log("Update State 0");
                            }
                        }
                        else
                        {
                            data.Process.Refresh();

                            if (data.Process.HasExited)
                            {
                                if (p.GotLauncher)
                                {
                                    if (p.GotGame)
                                    {
                                        handlerInstance.exited++;
                                    }
                                    else
                                    {
                                        List<int> children = ProcessUtil.GetChildrenProcesses(data.Process);
                                        if (children.Count > 0)
                                        {
                                            for (int j = 0; j < children.Count; j++)
                                            {
                                                int id = children[j];
                                                Process pro = Process.GetProcessById(id);

                                                if (!handlerInstance.attached.Contains(pro))
                                                {
                                                    handlerInstance.attached.Add(pro);
                                                    handlerInstance.attachedIds.Add(pro.Id);
                                                    p.ProcessID = pro.Id;
                                                    data.HWnd = null;
                                                    p.GotGame = true;
                                                    data.AssignProcess(pro);
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Steam showing a launcher, need to find our game process
                                    string launcher = handlerInstance.CurrentGameInfo.LauncherExe;
                                    if (!string.IsNullOrEmpty(launcher))
                                    {
                                        if (launcher.ToLower().EndsWith(".exe"))
                                        {
                                            launcher = launcher.Remove(launcher.Length - 4, 4);
                                        }

                                        Process[] procs = Process.GetProcessesByName(launcher);
                                        for (int j = 0; j < procs.Length; j++)
                                        {
                                            Process pro = procs[j];

                                            if (!handlerInstance.attachedIds.Contains(pro.Id))
                                            {
                                                handlerInstance.attached.Add(pro);
                                                handlerInstance.attachedIds.Add(pro.Id);
                                                p.ProcessID = pro.Id;
                                                data.AssignProcess(pro);
                                                p.GotLauncher = true;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (ResetingWindows)
                                {
                                    if (WindowsMerger.Instance != null)
                                    {
                                        if (WindowsMerger.Instance.RefreshChildWindows(p) != IntPtr.Zero)
                                        {
                                            data.Setted = true;
                                            continue;
                                        }
                                    }
                                }

                                if (data.HWNDRetry || data.HWnd == null || data.HWnd.NativePtr != data.Process.NucleusGetMainWindowHandle())
                                {
                                    handlerInstance.Log("Update data process has not exited");
                                    data.HWnd = new HwndObject(data.Process.NucleusGetMainWindowHandle());
                                    Point pos = data.HWnd.Location;

                                    if (string.IsNullOrEmpty(data.HWnd.Title) ||
                                        pos.X == -32000 ||
                                        data.HWnd.Title.ToLower() == handlerInstance.CurrentGameInfo.LauncherTitle?.ToLower())
                                    {
                                        data.HWNDRetry = true;

                                    }
                                    else if (!string.IsNullOrEmpty(handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName) &&
                                        // TODO: this Levenshtein distance is being used to help us around Call of Duty Black Ops, as it uses a ® icon in the title bar
                                        //       there must be a better way
                                        StringUtil.ComputeLevenshteinDistance(data.HWnd.Title, handlerInstance.CurrentGameInfo.Hook.ForceFocusWindowName) > 2 && !handlerInstance.CurrentGameInfo.HasDynamicWindowTitle)
                                    {
                                        data.HWNDRetry = true;
                                    }
                                    else
                                    {
                                        data.Setted = true;
                                    }
                                }
                                else
                                {
                                    handlerInstance.Log("Assigning window handle");
                                    data.HWnd = new HwndObject(data.Process.NucleusGetMainWindowHandle());
                                    data.Setted = true;
                                }
                            }
                        }
                    }
                }
            }

            if (handlerInstance.exited == players.Count)
            {
                if (!handlerInstance.hasEnded)
                {
                    handlerInstance.hasEnded = true;
                    handlerInstance.Log("Update method calling Handler End function");
                    handlerInstance.End(false);
                }
            }
        }

        public static void SetWindowText(Process proc, string windowTitle)
        {
            SetWindowText(proc.NucleusGetMainWindowHandle(), windowTitle);
        }

        public static void ShowHideWindows()
        {
            var handlerInstance = GenericGameHandler.Instance;

            if (handlerInstance == null)
            {
                return;
            }

            if (LockInputRuntime.IsLocked)
            {
                Globals.MainOSD.Show(1600, $"Unlock Inputs First (Press {App_Hotkeys.LockInputs} key)");
                return;
            }

            if (handlerInstance.profile.DevicesList.All(pl => pl.ProcessData == null))
            {
                Globals.MainOSD.Show(1600, $"Can't Be Used Now");
                return;//return until atleast 1 player processdata is set 
            }

            int windowsFound = 0;

            if (TopMostToggle)
            {
                try
                {                  
                    for (int i = 0; i < handlerInstance.profile.DevicesList.Count; i++)
                    {
                        if (handlerInstance.profile.DevicesList[i].ProcessData != null && !handlerInstance.profile.DevicesList[i].IsMinimized)
                        {
                            if (handlerInstance.profile.DevicesList[i].ProcessData.HWnd != null)
                            {
                                IntPtr hWnd = handlerInstance.profile.DevicesList[i].ProcessData.HWnd.NativePtr;
                                if (hWnd != IntPtr.Zero)
                                {
                                    User32Interop.SetWindowPos(hWnd, new IntPtr(-2), 0, 0, 0, 0,
                                    (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                                    ShowWindow(hWnd, ShowWindowEnum.Minimize);
                                    handlerInstance.profile.DevicesList[i].IsMinimized = true;
                                    windowsFound++;
                                }
                            }
                        }
                    }

                    if (handlerInstance.profile.DevicesList.Any(pl => pl.IsMinimized))
                    {
                        foreach (WPFDiv back in GenericGameHandler.Instance.splitForms)
                        {
                            back.Dispatcher.Invoke(new Action(() =>
                            {
                                back.WindowState = System.Windows.WindowState.Minimized;
                            }));
                        }

                        if (WindowsMerger.Instance != null)
                        {
                            User32Interop.SetWindowPos(WindowsMerger.Instance.Handle, IntPtr.Zero, 0, 0, 0, 0,
                                        (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                            ShowWindow(WindowsMerger.Instance.Handle, ShowWindowEnum.Minimize);
                        }
                    }
                }
                catch
                { }

                if (windowsFound > 0)
                {
                    TaskbarState.Show.Invoke();
                    Globals.MainOSD.Show(1600, $"Game Windows Minimized");                 
                    TopMostToggle = false;
                }
            }
            else if (!TopMostToggle)
            {
                if(!handlerInstance.profile.DevicesList.Any(p => p.IsMinimized))
                {
                    return;
                }

                TaskbarState.Hide.Invoke();

                for (int i = 0; i < handlerInstance.profile.DevicesList.Count; i++)
                {
                    if (handlerInstance.profile.DevicesList[i].ProcessData != null && handlerInstance.profile.DevicesList[i].IsMinimized)
                    {
                        if (handlerInstance.profile.DevicesList[i].ProcessData.HWnd != null)
                        {
                            IntPtr hWnd = handlerInstance.profile.DevicesList[i].ProcessData.HWnd.NativePtr;
                            if (hWnd != IntPtr.Zero)
                            {
                                ShowWindow(hWnd, ShowWindowEnum.Restore);

                                User32Interop.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0,
                                    (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                                handlerInstance.profile.DevicesList[i].IsMinimized = false;
                                windowsFound++;
                            }
                        }
                    }
                }

                if (handlerInstance.profile.DevicesList.Any(pl => !pl.IsMinimized))
                {
                    foreach (WPFDiv back in GenericGameHandler.Instance.splitForms)
                    {
                        back.Dispatcher.Invoke(new Action(() =>
                        {
                            back.WindowState = System.Windows.WindowState.Maximized;
                        }));
                    }

                    if (WindowsMerger.Instance != null)
                    {
                        ShowWindow(WindowsMerger.Instance.Handle, ShowWindowEnum.Restore);
                        User32Interop.SetWindowPos(WindowsMerger.Instance.Handle, IntPtr.Zero, 0, 0, 0, 0,
                            (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                    }
                }

                if (windowsFound > 0)
                {
                    Globals.MainOSD.Show(1600, $"Game Windows Restored");
                    TopMostToggle = true;
                }
            }
        }

        public static void RemoveBorder()
        {
            var handlerInstance = GenericGameHandler.Instance;

            foreach (PlayerInfo plyr in handlerInstance.profile.DevicesList)
            {
                Thread.Sleep(1000);

                if (plyr.ProcessData == null)
                {
                    continue;
                }

                Process plyrProc = plyr.ProcessData.Process;

                if (!handlerInstance.CurrentGameInfo.DontRemoveBorders)
                {
                    const int flip = 0x00C00000 | 0x00080000 | 0x00040000; //WS_BORDER | WS_SYSMENU

                    var x = (int)User32Interop.GetWindowLong(plyrProc.NucleusGetMainWindowHandle(), User32_WS.GWL_STYLE);
                    if ((x & flip) > 0)//has a border
                    {
                        x &= (~flip);
                        ResetWindows(plyr.ProcessData, plyr.ProcessData.Position.X, plyr.ProcessData.Position.Y, plyr.ProcessData.Size.Width, plyr.ProcessData.Size.Height, plyr.PlayerID + 1);
                    }
                }

                if (handlerInstance.CurrentGameInfo.WindowStyleEndChanges?.Length > 0 || handlerInstance.CurrentGameInfo.ExtWindowStyleEndChanges?.Length > 0)
                {
                    Thread.Sleep(1000);
                    WindowStyleChanges(plyr.ProcessData);
                }

                if (handlerInstance.CurrentGameInfo.EnableWindows)
                {
                    EnableWindow(plyr.ProcessData.HWnd.NativePtr, true);
                }
            }
        }

        public static void ChangeForegroundWindow()
        {
            IntPtr windowToFocus = IntPtr.Zero;

            if (WindowsMerger.Instance != null)
            {
                windowToFocus = User32Interop.FindWindow(null, "WindowsMerger");            
                User32Interop.SetForegroundWindow(windowToFocus);
                return;
            }

            foreach (WPFDiv back in GenericGameHandler.Instance.splitForms)
            {
                back.Dispatcher.Invoke(new Action(() =>
                {
                    windowToFocus = User32Interop.FindWindow(null, back.Title);
                }));

                if (windowToFocus != IntPtr.Zero)
                {
                    User32Interop.SetForegroundWindow(windowToFocus);
                    return;
                }
            }
          
            if (windowToFocus == IntPtr.Zero)
            {
                windowToFocus = Globals.MainWindowHandle;
            }

            if (windowToFocus != IntPtr.Zero)
            {
                User32Interop.SetForegroundWindow(windowToFocus);
            }
        }
    }
}
