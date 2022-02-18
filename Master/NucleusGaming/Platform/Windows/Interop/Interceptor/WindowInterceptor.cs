//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft Co.
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//        07 September 2006
//Copyright: (C) 2006, Sergey Stoyan
//********************************************************************************************
using System;
using System.Runtime.InteropServices;
using Win32;

namespace CliverSoft
{
    /// <summary>
    /// Intercept creation of window and get its HWND
    /// </summary>
    public class WindowInterceptor
    {
        private IntPtr hook_id = IntPtr.Zero;
        private Functions.HookProc hookProcedure;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private int errorCode;

        /// <summary>
        /// Start dialog box interception for the specified owner window
        /// </summary>
        /// <param name="owner_window">owner window, if it is IntPtr.Zero then any windows will be intercepted</param>
        /// <param name="process_window">custom delegate to process intercepted window. It should be a fast code in order to have no message stack overflow.</param>
        public WindowInterceptor(IntPtr hMod, uint threadID)
        {
            hookProcedure = new Functions.HookProc(HookProcedure);

            //hook_id = Win32.Functions.SetWindowsHookEx(Win32.HookType.WH_CALLWNDPROCRET, cbf, IntPtr.Zero, Win32.Functions.GetCurrentThreadId());   
            hook_id = Win32.Functions.SetWindowsHookEx(Win32.HookType.WH_CALLWNDPROCRET, hookProcedure, hMod, threadID);

            errorCode = Marshal.GetLastWin32Error();
        }

        /// <summary>
        /// Stop intercepting. Should be called to calm unmanaged code correctly
        /// </summary>
        public void Stop()
        {
            if (hook_id != IntPtr.Zero)
            {
                Win32.Functions.UnhookWindowsHookEx(hook_id);
            }
            hook_id = IntPtr.Zero;
        }

        ~WindowInterceptor()
        {
            Stop();
        }

        private IntPtr HookProcedure(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
            {
                return Win32.Functions.CallNextHookEx(hook_id, nCode, wParam, lParam);
            }

            Win32.CWPRETSTRUCT msg = (Win32.CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(Win32.CWPRETSTRUCT));

            //filter out create window events only
            //if (msg.message == (uint)Win32.Messages.WM_SHOWWINDOW)
            //{
            //    int h = Win32.Functions.GetWindow(msg.hwnd, Win32.Functions.GW_OWNER);
            //    //check if owner is that is specified
            //    if (owner_window == IntPtr.Zero || owner_window == new IntPtr(h))
            //    {
            //        if (process_window != null)
            //            process_window(msg.hwnd);
            //    }
            //}

            if (msg.message == (uint)Win32.Messages.WM_KILLFOCUS)
            {
                Console.WriteLine("Killed focus");
                //Win32.Functions.SendMessage(hook_id, (uint)Win32.Messages.WM_SETFOCUS, 0, 0);
            }
            else if (msg.message == (uint)Win32.Messages.WM_SETFOCUS)
            {
                Console.WriteLine("Gained focus");
            }

            return Win32.Functions.CallNextHookEx(hook_id, nCode, wParam, lParam);
        }
    }
}
