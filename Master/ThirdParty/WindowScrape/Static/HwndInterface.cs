using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using WindowScrape.Constants;
using WindowScrape.Types;

namespace WindowScrape.Static
{
    public static class HwndInterface
    {
        #region Windows
        public static List<IntPtr> EnumHwnds()
        {
            IntPtr parent = IntPtr.Zero;
            return EnumChildren(parent);
        }

        public static IntPtr GetHwnd(string windowText, string className)
        {
            return (IntPtr)FindWindow(className, windowText);
        }

        public static IntPtr GetHwndFromTitle(string windowText)
        {
            return (IntPtr)FindWindow(null, windowText);
        }

        public static IntPtr GetHwndFromClass(string className)
        {
            return (IntPtr)FindWindow(className, null);
        }

        public static bool ActivateWindow(IntPtr hwnd)
        {
            return SetForegroundWindow(hwnd);
        }

        public static bool MinimizeWindow(IntPtr hwnd)
        {
            return CloseWindow(hwnd);
        }
        #endregion

        #region Hwnd Attributes
        public static string GetHwndClassName(IntPtr hwnd)
        {
            StringBuilder result = new StringBuilder(256);
            GetClassName(hwnd, result, result.MaxCapacity);
            return result.ToString();
        }
        public static int GetHwndTitleLength(IntPtr hwnd)
        {
            return GetWindowTextLength(hwnd);
        }
        public static string GetHwndTitle(IntPtr hwnd)
        {
            int length = GetHwndTitleLength(hwnd);
            StringBuilder result = new StringBuilder(length + 1);
            GetWindowText(hwnd, result, result.Capacity);
            return result.ToString();
        }
        public static bool SetHwndTitle(IntPtr hwnd, string text)
        {
            return SetWindowText(hwnd, text);
        }
        public static string GetHwndText(IntPtr hwnd)
        {
            int len = (int)SendMessage(hwnd, (UInt32)WM.GETTEXTLENGTH, 0, 0) + 1;
            StringBuilder sb = new StringBuilder(len);
            SendMessage(hwnd, (UInt32)WM.GETTEXT, (uint)len, sb);
            return sb.ToString();
        }
        public static void SetHwndText(IntPtr hwnd, string text)
        {
            SendMessage(hwnd, (UInt32)WM.SETTEXT, 0, text);
        }
        public static bool SetHwndPos(IntPtr hwnd, int x, int y)
        {
            return SetWindowPos(hwnd, IntPtr.Zero, x, y, 0, 0, (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOZORDER));
        }
        public static bool SetHwndPosTopMost(IntPtr hwnd, int x, int y)
        {
            return SetWindowPos(hwnd, new IntPtr(-1), x, y, 0, 0, (uint)PositioningFlags.SWP_NOSIZE);
        }
        public static Point GetHwndPos(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            Point result = new Point(rect.Left, rect.Top);
            return result;
        }
        public static bool SetHwndSize(IntPtr hwnd, int w, int h)
        {
            return SetWindowPos(hwnd, IntPtr.Zero, 0, 0, w, h, (uint)(PositioningFlags.SWP_NOMOVE | PositioningFlags.SWP_NOZORDER));
        }
        public static bool SetHwndSizeTopMost(IntPtr hWnd, int w, int h)
        {
            return SetWindowPos(hWnd, new IntPtr(-1), 0, 0, w, h, (uint)PositioningFlags.SWP_NOMOVE);
        }

        public static bool MakeTopMost(IntPtr hWnd)
        {
            return SetWindowPos(hWnd, HWND_TOPMOST, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
        }

        public static bool MakeNoTopMost(IntPtr hWnd)
        {
            return SetWindowPos(hWnd, HWND_NOTOPMOST, 0, 0, 0, 0, (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
        }

        public static Size GetHwndSize(IntPtr hwnd)
        {
            RECT rect = new RECT();
            GetWindowRect(hwnd, out rect);
            Size result = new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            return result;
        }

        public static bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h)
        {
            return MoveWindow(hWnd, x, y, w, h, true);
        }

        #endregion

        #region Hwnd Functions
        public static List<IntPtr> EnumChildren(IntPtr hwnd)
        {
            IntPtr child = IntPtr.Zero;
            List<IntPtr> results = new List<IntPtr>();
            do
            {
                child = FindWindowEx(hwnd, child, null, null);
                if (child != IntPtr.Zero)
                {
                    results.Add(child);
                }
            } while (child != IntPtr.Zero);
            return results;
        }
        public static IntPtr GetHwndChild(IntPtr hwnd, string clsName, string ctrlText)
        {
            return FindWindowEx(hwnd, IntPtr.Zero, clsName, ctrlText);
        }
        public static IntPtr GetHwndParent(IntPtr hwnd)
        {
            return GetParent(hwnd);
        }
        public static int SendMessage(IntPtr hwnd, WM msg, uint param1, uint param2)
        {
            return (int)SendMessage(hwnd, (uint)msg, param1, param2);
        }
        public static int SendMessage(IntPtr hwnd, WM msg, uint param1, string param2)
        {
            return (int)SendMessage(hwnd, (uint)msg, param1, param2);
        }
        public static string GetMessageString(IntPtr hwnd, WM msg, uint param)
        {
            StringBuilder sb = new StringBuilder(65536);
            SendMessage(hwnd, (uint)msg, param, sb);
            return sb.ToString();
        }
        public static int GetMessageInt(IntPtr hwnd, WM msg)
        {
            return (int)SendMessage(hwnd, (uint)msg, 0, 0);
        }
        public static void ClickHwnd(IntPtr hwnd)
        {
            SendMessage(hwnd, (uint)WM.BN_CLICKED, IntPtr.Zero, IntPtr.Zero);
        }
        public static Point GetTitleBarSize(IntPtr hwnd)
        {
            GetClientRect(hwnd, out RECT rcClient);
            GetWindowRect(hwnd, out RECT rcWind);

            Point ptDiff = new Point
            {
                X = (rcWind.Right - rcWind.Left) - rcClient.Right,
                Y = (rcWind.Bottom - rcWind.Top) - rcClient.Bottom
            };
            return ptDiff;
        }

        #endregion

        #region lib

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);
        private static readonly IntPtr HWND_TOP = new IntPtr(0);
        private static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int x, int y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        private static extern int FindWindow(string lpClassName, string lpWindowName);

        // Standard interface
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        // Sending messages by string
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, uint wParam, string lParam);

        // Retrieving string data
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, uint wParam, StringBuilder lParam);

        // Retrieving numeric data
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        private static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, uint wParam, uint lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        /// <summary>
        /// Changes an attribute of the specified window. The function also sets the 32-bit (long) value at the specified offset into the extra window memory.
        /// </summary>
        /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs..</param>
        /// <param name="nIndex">The zero-based offset to the value to be set. Valid values are in the range zero through the number of bytes of extra window memory, minus the size of an integer. To set any other value, specify one of the following values: GWL_EXSTYLE, GWL_HINSTANCE, GWL_ID, GWL_STYLE, GWL_USERDATA, GWL_WNDPROC </param>
        /// <param name="dwNewLong">The replacement value.</param>
        /// <returns>If the function succeeds, the return value is the previous value of the specified 32-bit integer. 
        /// If the function fails, the return value is zero. To get extended error information, call GetLastError. </returns>
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("USER32.DLL")]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);


        [DllImport("user32.dll")]
        private static extern bool CloseWindow(IntPtr hWnd);

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        private static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion

    }
}
