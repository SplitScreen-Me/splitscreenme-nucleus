#region copyright
// This file is part of Dual Monitor Tools which is a set of tools to assist
// users with multiple monitor setups.
// Copyright (C) 2009-2015 Gerald Evans
// 
// Dual Monitor Tools is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Nucleus.Gaming.Coop.Generic.Cursor
{
    /// <summary>
    /// Definitions extracted from winuser.h
    /// </summary>
    internal static class NativeMethods
    {
        public const int ERROR_SUCCESS = 0;

        // flags for SystemParametersInfo(uiAction)
        public static uint SPI_SETDESKWALLPAPER = 20;

        // flags for SystemParametersInfo(fWinIni)
        public static uint SPIF_UPDATEINIFILE = 0x0001;
        public static uint SPIF_SENDWININICHANGE = 0x0002;

        // Flags for ShowWindow() and WINDOWPLACEMENT.showCmd
        public const int SW_HIDE = 0;
        public const int SW_SHOWNORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_SHOWNOACTIVATE = 4;
        public const int SW_SHOW = 5;
        public const int SW_MINIMIZE = 6;
        public const int SW_SHOWMINNOACTIVE = 7;
        public const int SW_SHOWNA = 8;
        public const int SW_RESTORE = 9;
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;

        // Global/Local Hooks
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        // Flags for Add/Check/EnableMenuItem() 
        public const int MF_STRING = 0x00000000;
        public const int MF_SEPARATOR = 0x00000800;

        // Commands for GetWindow()
        public const uint GW_HWNDNEXT = 2;

        // Indexes for GetWindowLong()
        public const int GWL_STYLE = -16;
        public const int GWL_EXSTYLE = -20;

        // Flags for GetWindowLong( , GWL_STYLE)
        public const int WS_MAXIMIZEBOX = 0x00010000;
        public const int WS_THICKFRAME = 0x00040000;
        public const int WS_MAXIMIZE = 0x01000000;
        public const int WS_MINIMIZE = 0x20000000;

        // Flags for GetWindowLong( , GWL_EXSTYLE)
        public const int WS_EX_TOPMOST = 0x00000008;
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int WS_EX_TOOLWINDOW = 0x00000080;
        public const int WS_EX_APPWINDOW = 0x00040000;

        // WINDOWPLACEMENT.Flags
        public const int WPF_SETMINPOSITION = 0x0001;
        public const int WPF_RESTORETOMAXIMIZED = 0x0002;

        // MSLLHOOKSTRUCT.dwExtraInfo
        public const uint MOUSEEVENTF_FROMTOUCH = 0xFF515700;

        // Modifier keys
        public const int MOD_ALT = 0x0001;
        public const int MOD_CONTROL = 0x0002;
        public const int MOD_SHIFT = 0x0004;
        public const int MOD_WIN = 0x0008;

        // flags for STARTUPINFO.dwFlags
        public const int STARTF_USESHOWWINDOW = 0x00000001;
        public const int STARTF_USESIZE = 0x00000002;
        public const int STARTF_USEPOSITION = 0x00000004;

        // flags for CreateProcess().dwCreationFlags
        public const uint CREATE_SUSPENDED = 0x00000004;
        public const uint NORMAL_PRIORITY_CLASS = 0x00000020;

        // flags for MSLLHOOKSTRUCT.mouseData for X buttons
        public const uint XBUTTON1 = 0x0001;
        public const uint XBUTTON2 = 0x0002;

        // flags for GetDeviceCaps
        public const int BITSPIXEL = 12;
        public const int PLANES = 14;

        // size of device name in MonitorInfoEx.DeviceName
        private const int CCHDEVICENAME = 32;

        // flags for MONITORINFOEX.dwFlags
        public const int MONITORINFOF_PRIMARY = 1;

        // Windows messages
        public const int WM_CLOSE = 0x0010;
        public const int WM_COPYDATA = 0x004A;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;
        public const int WM_SYSCOMMAND = 0x0112;
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_XBUTTONDOWN = 0x020B;
        public const int WM_XBUTTONUP = 0x020C;
        public const int WM_HOTKEY = 0x0312;

        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint EVENT_SYSTEM_FOREGROUND = 3;

        [Flags]
        public enum ASSOCF
        {
            ASSOCF_INIT_NOREMAPCLSID = 0x1,
            ASSOCF_INIT_BYEXENAME = 0x2,
            ASSOCF_OPEN_BYEXENAME = 0x2,
            ASSOCF_INIT_DEFAULTTOSTAR = 0x4,
            ASSOCF_INIT_DEFAULTTOFOLDER = 0x8,
            ASSOCF_NOUSERSETTINGS = 0x10,
            ASSOCF_NOTRUNCATE = 0x20,
            ASSOCF_VERIFY = 0x40,
            ASSOCF_REMAPRUNDLL = 0x80,
            ASSOCF_NOFIXUPS = 0x100,
            ASSOCF_IGNOREBASECLASS = 0x200,
            ASSOCF_IGNOREUNKNOWN = 0x400
        }

        public enum ASSOCSTR
        {
            ASSOCSTR_COMMAND = 1,
            ASSOCSTR_EXECUTABLE,
            ASSOCSTR_FRIENDLYDOCNAME,
            ASSOCSTR_FRIENDLYAPPNAME,
            ASSOCSTR_NOOPEN,
            ASSOCSTR_SHELLNEWVALUE,
            ASSOCSTR_DDECOMMAND,
            ASSOCSTR_DDEIFEXEC,
            ASSOCSTR_DDEAPPLICATION,
            ASSOCSTR_DDETOPIC,
            ASSOCSTR_INFOTIP,
            ASSOCSTR_QUICKTIP,
            ASSOCSTR_TILEINFO,
            ASSOCSTR_CONTENTTYPE,
            ASSOCSTR_DEFAULTICON,
            ASSOCSTR_SHELLEXTENSION,
            ASSOCSTR_DROPTARGET,
            ASSOCSTR_DELEGATEEXECUTE,
            ASSOCSTR_MAX
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            public IntPtr lpData;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct MONITORINFOEX
        {
            public int cbSize;
            public RECT rcMonitor;
            public RECT rcWork;
            public uint dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CCHDEVICENAME)]
            public string szDevice;

            public MONITORINFOEX(int dummy)
            {
                cbSize = 40 + 2 * CCHDEVICENAME;
                rcMonitor = new RECT();
                rcWork = new RECT();
                dwFlags = 0;
                szDevice = string.Empty;
            }
        }


        public struct MSLLHOOKSTRUCT
        {
            public POINT pt;
            public uint mouseData;
            public uint flags;
            public uint time;
            public uint dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct PHYSICAL_MONITOR
        {
            public IntPtr hPhysicalMonitor;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
            public string szPhysicalMonitorDescription;
        }

        public struct POINT
        {
            public int x;
            public int y;
        }

        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public uint dwProcessId;
            public uint dwThreadId;
        }

        public struct RECT
        {
            public int left;
            public int top;
            public int right;
            public int bottom;

            // ctor to simplify creation
            public RECT(int left, int top, int right, int bottom)
            {
                this.left = left;
                this.top = top;
                this.right = right;
                this.bottom = bottom;
            }
        }

        public struct STARTUPINFO
        {
            public uint cb;
            public string lpReserved;
            public string lpDesktop;
            public string lpTitle;
            public uint dwX;
            public uint dwY;
            public uint dwXSize;
            public uint dwYSize;
            public uint dwXCountChars;
            public uint dwYCountChars;
            public uint dwFillAttribute;
            public uint dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        public struct WINDOWPLACEMENT
        {
            public uint length;
            public uint flags;
            public uint showCmd;
            public POINT ptMinPosition;
            public POINT ptMaxPosition;
            public RECT rcNormalPosition;
        }

        //public class KBDLLHOOKSTRUCT
        //{
        //    public uint vkCode;
        //    public uint scanCode;
        //    public uint flags;
        //    public uint time;
        //    public uint dwExtraInfo;
        //}

        // delegate used by EnumDisplayMonitors
        public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

        // deleagte used by EnumWindows()
        public delegate bool EnumWindowsProc(IntPtr Wnd, uint lParam);

        // delegate used by SetWindowsHookEx()
        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        // delegate used by SetWinEventHook
        public delegate void WinEventProc(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime);

        [DllImport("user32.dll")]
        public static extern int AppendMenu(int hMenu, int uFlags, int uIDNewItem, string lpNewItem);

        [DllImport("Shlwapi.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint AssocQueryString(ASSOCF flags, ASSOCSTR str, string pszAssoc, string pszExtra,
           [Out] StringBuilder pszOut, [In][Out] ref uint pcchOut);

        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(int pid);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        //[DllImport("user32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool ClipCursor(ref RECT lpRect);

        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateDC(string lpszDriver, string lpszDevice,
           string lpszOutput, IntPtr lpInitData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CreateProcess(string lpApplicationName, string lpCommandLine,
        IntPtr lpProcessAttributes, IntPtr lpThreadAttributes,
        bool bInheritHandles, uint dwCreationFlags, IntPtr lpEnvironment,
        string lpCurrentDirectory, ref STARTUPINFO lpStartupInfo, out PROCESS_INFORMATION lpProcessInformation);

        [DllImport("gdi32.dll", EntryPoint = "DeleteDC")]
        public static extern bool DeleteDC([In] IntPtr hdc);

        [DllImport("dxva2.dll", EntryPoint = "DestroyPhysicalMonitors", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyPhysicalMonitors(
            uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip,
           EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, uint lParam);

        [DllImport("user32.dll")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("kernel32")]
        public static extern bool FreeConsole();

        [DllImport("user32.dll")]
        public static extern short GetAsyncKeyState(int vKey);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern IntPtr GetDesktopWindow();

        [DllImport("gdi32.dll")]
        public static extern int GetDeviceCaps(IntPtr hdc, int nIndex);

        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("kernel32.dll")]     // winbase.h
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("dxva2.dll")]
        public static extern bool GetMonitorBrightness(IntPtr hMonitor,
            out uint pdwMinimumBrightness,
            out uint pdwCurrentBrightness,
            out uint pdwMaximumBrightness);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

        [DllImport("dxva2.dll", EntryPoint = "GetNumberOfPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

        [DllImport("user32.dll")]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("dxva2.dll", EntryPoint = "GetPhysicalMonitorsFromHMONITOR")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetPhysicalMonitorsFromHMONITOR(
            IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

        [DllImport("user32.dll")]
        public static extern IntPtr GetShellWindow();

        [DllImport("user32.dll")]
        public static extern int GetSystemMenu(IntPtr hwnd, int bRevert);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hwnd, out NativeMethods.RECT lpRect);

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("kernel32.dll")]
        public static extern uint ResumeThread(IntPtr hThread);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

        [DllImport("user32.dll")]
        public static extern IntPtr SetForegroundWindow(IntPtr hWnd);

        [DllImport("kernel32.dll")]     // winbase.h
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetProcessWorkingSetSize(IntPtr hProcess, uint dwMinimumWorkingSetSize, uint dwMaximumWorkingSetSize);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        public static extern int SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        [DllImport("user32.dll")]
        public static extern IntPtr WindowFromPoint(NativeMethods.POINT point);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, int idProcess, int idThread, uint dwflags);

        [DllImport("user32.dll")]
        public static extern int UnhookWinEvent(IntPtr hWinEventHook);

        [DllImport("User32.dll")]
        public static extern bool ShowWindow(IntPtr handle, int nCmdShow);
    }
}
