#region copyright
// This file is part of Dual Monitor Tools which is a set of tools to assist
// users with multiple monitor setups.
// Copyright (C) 2010-2015  Gerald Evans
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.Generic.Cursor
{
    /// <summary>
    /// Module for handling mouse/cursor related features
    /// </summary>
    internal class CursorModule
    {
        private NativeMethods.HookProc llMouseProc;
        private NativeMethods.WinEventProc winEventProc;
        private IntPtr llMouseHook = IntPtr.Zero;
        private IntPtr winEventHook = IntPtr.Zero;
        private IntPtr processHandle = IntPtr.Zero;
        private HashSet<IntPtr> _otherGames = new HashSet<IntPtr>();
        private int _minForce = int.MaxValue;
        private Process _process;
        private Rectangle _rectangle;

        // Barriers which constrain the cursor movement
        private CursorBarrierLower _leftBarrier = new CursorBarrierLower(false, 0, 0);
        private CursorBarrierUpper _rightBarrier = new CursorBarrierUpper(false, 0, 0);
        private CursorBarrierLower _topBarrier = new CursorBarrierLower(false, 0, 0);
        private CursorBarrierUpper _bottomBarrier = new CursorBarrierUpper(false, 0, 0);


        public CursorModule()
        {
            llMouseProc = llMouseHookCallback;
            winEventProc = EventCallback;

            StartListeningForWindowChanges();
        }

        public void Setup(Process p, Rectangle rectangle)
        {
            _process = p;
            _rectangle = rectangle;
            ReBuildBarriers(_rectangle);
            processHandle = p.MainWindowHandle;

            _process.Exited += (sender, args) =>
            {
                Stop();
            };
        }

        public void AddOtherGameHandle(IntPtr gameHandle)
        {
            _otherGames.Add(gameHandle);
        }

        public void Stop()
        {
            StopListeningForWindowChanges();
            UnLockCursor();
        }

        // The cursor should be locked (possibly just sticky) to the screen it is currently on.
        public void LockCursorToScreen()
        {
            if (llMouseHook == IntPtr.Zero)
            {
                using (Process curProcess = Process.GetCurrentProcess())
                {
                    using (ProcessModule curModule = curProcess.MainModule)
                    {
                        IntPtr hModule = NativeMethods.GetModuleHandle(curModule.ModuleName);
                        llMouseHook = NativeMethods.SetWindowsHookEx(NativeMethods.WH_MOUSE_LL, llMouseProc, hModule, 0);
                    }
                }
            }
        }

        // rebuild the barriers to restrict movement of the cursor
        // to the screen that it is currently on.
        // This can be called by the low level mouse hook callback,
        // so needs to be reasonably efficient.
        //
        // Note: no locking is currently employed so need to be carefull of the order in which things are done
        private void ReBuildBarriers(Rectangle r)
        {
            _leftBarrier.ChangeBarrier(true, r.Left + 1, _minForce);
            _rightBarrier.ChangeBarrier(true, r.Right - 1, _minForce);
            _topBarrier.ChangeBarrier(true, r.Top + 1, _minForce);
            _bottomBarrier.ChangeBarrier(true, r.Bottom - 1, _minForce);
        }

        // The cursor's movement should not be hindered by screen edges
        public void UnLockCursor()
        {
            // make sure the low level mouse hook is unhooked
            if (llMouseHook != IntPtr.Zero)
            {
                // unhook our callback to make sure there is no performance degredation
                NativeMethods.UnhookWindowsHookEx(llMouseHook);
                llMouseHook = IntPtr.Zero;
            }
        }

        // This is the low level Mouse hook callback
        // Processing in here should be efficient as possible
        // as it can be called very frequently.
        public int llMouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                // lParam is a pointer to a MSLLHOOKSTRUCT, 
                NativeMethods.MSLLHOOKSTRUCT msllHookStruct =
                    (NativeMethods.MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(NativeMethods.MSLLHOOKSTRUCT));
                int originalX = msllHookStruct.pt.x;
                int originalY = msllHookStruct.pt.y;
                int x = originalX;
                int y = originalY;

                // check if the cursor has moved from one screen to another
                // and if so add the required amount of stickiness to the cursor
                // restraining it to the current screen if necessary

                bool brokenThrough = _leftBarrier.BrokenThrough(ref x);
                if (_rightBarrier.BrokenThrough(ref x))
                {
                    brokenThrough = true;
                }

                if (_topBarrier.BrokenThrough(ref y))
                {
                    brokenThrough = true;
                }

                if (_bottomBarrier.BrokenThrough(ref y))
                {
                    brokenThrough = true;
                }

                if (brokenThrough)
                {
                    //ReBuildBarriers(new Point(x, y));
                    //????
                    //UnLockCursor();
                }

                if (x != originalX || y != originalY)
                {
                    // override the position that Windows wants to place the cursor
                    System.Windows.Forms.Cursor.Position = new Point(x, y);
                    return 1;
                }
            }

            return NativeMethods.CallNextHookEx(llMouseHook, nCode, wParam, lParam);
        }

        public void StartListeningForWindowChanges()
        {
            //setting the window hook
            if (winEventHook == IntPtr.Zero)
            {
                winEventHook = NativeMethods.SetWinEventHook(NativeMethods.EVENT_SYSTEM_FOREGROUND, NativeMethods.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, winEventProc, 0, 0, NativeMethods.WINEVENT_OUTOFCONTEXT);
            }
        }

        public void StopListeningForWindowChanges()
        {
            if (winEventHook != IntPtr.Zero)
            {
                NativeMethods.UnhookWinEvent(winEventHook);
                winEventHook = IntPtr.Zero;
            }
        }

        private void EventCallback(IntPtr hWinEventHook, uint iEvent, IntPtr hWnd, int idObject, int idChild, int dwEventThread, int dwmsEventTime)
        {
            // handle active window changed!
            if (processHandle == hWnd)
            {
                //Debug.WriteLine("Lock cursor to main game screen"); 
                LockCursorToScreen();
            }
            else if (_otherGames.Contains(hWnd))
            {
                //Debug.WriteLine("Other game screen, focus to main");
                SetActiveWindow();
            }
            else
            {
                //Debug.WriteLine("Not game window is active");
                UnLockCursor();
            }
        }

        public void SetActiveWindow()
        {
            NativeMethods.ShowWindow(processHandle, NativeMethods.SW_RESTORE | NativeMethods.SW_SHOW);
            NativeMethods.SetForegroundWindow(processHandle);
        }
    }
}
