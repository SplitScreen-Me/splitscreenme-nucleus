using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Nucleus.Gaming.Coop.ProtoInput
{
    public static class GetMainWindow
    {
        private static IntPtr bestHandle;
        private static int processIdOfInterest;

        private static object dummyObject = new object();

        public delegate bool EnumThreadWindowsCallback(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool EnumWindows(EnumThreadWindowsCallback callback, IntPtr extraData);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int GetWindowThreadProcessId(HandleRef handle, out int processId);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindow(HandleRef hWnd, int uCmd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool IsWindowVisible(HandleRef hWnd);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        private static string GetWindowName(IntPtr hwnd)
        {
            const int nChars = 256;
            StringBuilder buff = new StringBuilder(nChars);

            if (GetWindowText(hwnd, buff, nChars) > 0)
            {
                return buff.ToString();
            }

            return null;
        }

        private static bool IsMainWindow(IntPtr handle)
        {
            return !(GetWindow(new HandleRef(dummyObject, handle), 4) != (IntPtr)0)
                          &&
                          IsWindowVisible(new HandleRef(dummyObject, handle))
                          &&
                          !(GetWindowName(handle)?.Contains("ProtoInput") ?? false)
                          &&
                          !(GetWindowName(handle)?.Contains("Proto Input") ?? false);
        }

        private static bool EnumWindowsCallback(IntPtr handle, IntPtr extraParameter)
        {
            GetWindowThreadProcessId(new HandleRef(dummyObject, handle), out int processId);
            if (processIdOfInterest != processId || !IsMainWindow(handle))
            {
                return true;
            }

            bestHandle = handle;
            return false;
        }

        public static IntPtr NucleusGetMainWindowHandle(this Process p)
        {
            bestHandle = (IntPtr)0;
            processIdOfInterest = p.Id;

            EnumThreadWindowsCallback callback = new EnumThreadWindowsCallback(EnumWindowsCallback);

            EnumWindows(callback, IntPtr.Zero);

            GC.KeepAlive(callback);

            return bestHandle;
        }
    }
}
