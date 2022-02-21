using Nucleus.Gaming.Coop.InputManagement.Enums;
using Nucleus.Gaming.Coop.InputManagement.Logging;
using Nucleus.Gaming.Coop.InputManagement.Structs;
using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.InputManagement
{
    internal class RawInputWindow
    {
        //https://stackoverflow.com/a/30992796/11516572

        /*const uint WS_OVERLAPPEDWINDOW = 0xcf0000;
		const uint WS_VISIBLE = 0x10000000;
		const uint CS_USEDEFAULT = 0x80000000;
		const uint CS_DBLCLKS = 8;
		const uint CS_VREDRAW = 1;
		const uint CS_HREDRAW = 2;
		const uint COLOR_WINDOW = 5;
		const uint COLOR_BACKGROUND = 1;
		const uint IDC_CROSS = 32515;
		const uint WM_LBUTTONUP = 0x0202;
		const uint WM_LBUTTONDBLCLK = 0x0203;*/

        private const uint WM_DESTROY = 2;
        private const uint WM_PAINT = 0x0f;

        private readonly IntPtr HWND_MESSAGE = (IntPtr)(-3);

        public IntPtr hWnd { get; private set; }

        private WinApi.WndProc wndProc;

        public RawInputWindow()
        {
            ushort regResult = RegisterClass(out WNDCLASSEX wndClass);
            hWnd = WinApi.CreateWindowEx(
                0,
                regResult,
                "NCRawInputWindow",
                0,
                0, 0,
                0, 0,
                HWND_MESSAGE,
                IntPtr.Zero,
                IntPtr.Zero,
                IntPtr.Zero);

            Logger.WriteLine(hWnd == IntPtr.Zero ? $"Error in CreateWindowEx = 0x{Marshal.GetLastWin32Error()}" : "Successfully create raw input window");

            WinApi.ShowWindow(hWnd, 1);
            WinApi.UpdateWindow(hWnd);
        }

        private ushort RegisterClass(out WNDCLASSEX wndClass)
        {
            wndProc = WndProc;

            wndClass = new WNDCLASSEX
            {
                cbSize = Marshal.SizeOf(typeof(WNDCLASSEX)),
                hInstance = Marshal.GetHINSTANCE(GetType().Module),
                lpszClassName = "NCRawInputClass",
                lpfnWndProc = Marshal.GetFunctionPointerForDelegate(wndProc)
            };

            ushort regResult = WinApi.RegisterClassEx(ref wndClass);

            Logger.WriteLine($"RegisterClass result = 0x{regResult:x}");

            return regResult;
        }

        public void StartMessageLoop(RawInputProcessor rawInputProcessor)
        {
            int bRet;
            int sqErr = 0;

            //hWnd zero for all windows (the mouse pointers are in this loop!)
            while ((bRet = WinApi.GetMessage(out MSG msg, IntPtr.Zero, 0, 0)) != 0)
            {
                if (bRet == -1)
                {
                    if (sqErr++ > 10)
                    {
                        return;
                    }
                }
                else if (msg.message == 0x00FF)
                {
                    //Raw input
                    sqErr = 0;
                    rawInputProcessor.Process(msg.lParam);
                }
                else if (msg.message == 0x0400)
                {
                    //End split screen message.
                    Logger.WriteLine($"RawInputWindow received split screen end");
                    foreach (Window window in RawInputManager.windows)
                    {
                        window.End();
                    }
                }
                else if (msg.message == 0x0400 + 1)
                {
                    //Create cursors
                    Logger.WriteLine($"RawInputWindow received create cursors message");

                    bool internalInputUpdate = msg.wParam == (IntPtr)1;
                    bool drawCursorForControllers = msg.lParam == (IntPtr)1;

                    foreach (Window window in RawInputManager.windows)
                    {
                        //Cursor needs to be created on the MainForm message loop so it can be accessed in the loop.
                        bool kbm = window.KeyboardAttached != (IntPtr)(-1) || window.MouseAttached != (IntPtr)(-1);
                        if (kbm || drawCursorForControllers)
                        {
                            window.CreateCursor(!kbm && internalInputUpdate);
                        }
                    }
                }
                else
                {
                    sqErr = 0;
                    WinApi.TranslateMessage(ref msg);
                    WinApi.DispatchMessage(ref msg);
                }
            }
        }

        private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
        {
            /*switch (msg)
			{
				case WM_PAINT:
					break;

				case WM_DESTROY:
					//DestroyWindow(hWnd);
					break;
			}*/

            return (IntPtr)(1);
            //return WinApi.DefWindowProc(hWnd, msg, wParam, lParam);
        }
    }
}
