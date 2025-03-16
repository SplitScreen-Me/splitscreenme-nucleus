using Nucleus.Gaming.Coop.InputManagement.Enums;
using Nucleus.Gaming.Coop.InputManagement.Logging;
using Nucleus.Gaming.Coop.InputManagement.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nucleus.Gaming.Coop.InputManagement
{
    public static class RawInputManager
    {
        public static readonly List<Window> windows = new List<Window>();

        private static RawInputWindow rawInputWindow;

        public static void RegisterRawInput(RawInputProcessor rawInputProcessor)
        {
            Thread windowThread = new Thread(WindowThread)
            {
                Priority = ThreadPriority.AboveNormal,
                IsBackground = true
            };

            windowThread.Start(rawInputProcessor);
        }

        public static void EndSplitScreen()
        {
            WinApi.PostMessageA(rawInputWindow.hWnd, 0x0400, IntPtr.Zero, IntPtr.Zero);
        }

        public static void CreateCursorsOnWindowThread(bool internalInputUpdate, bool cursorForControllers)
        {
            WinApi.PostMessageA(rawInputWindow.hWnd, 0x0400 + 1,
                internalInputUpdate ? (IntPtr)1 : IntPtr.Zero,
                cursorForControllers ? (IntPtr)1 : IntPtr.Zero);
        }

        private static void WindowThread(object rawInputProcessor)
        {
            rawInputWindow = new RawInputWindow();
            IntPtr hWnd = rawInputWindow.hWnd;
            RegisterRawInputInternal(hWnd);
            rawInputWindow.StartMessageLoop((RawInputProcessor)rawInputProcessor);
        }

        private static void RegisterRawInputInternal(IntPtr windowHandle)
        {
            //GetDeviceList();
            //Logger.WriteLine($"Attempting to RegisterRawInputDevices for window handle {windowHandle}");

            //https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hidclass-hardware-ids-for-top-level-collections
            RAWINPUTDEVICE[] rid = new RAWINPUTDEVICE[2];

            //Keyboard
            rid[0].usUsagePage = 0x01;
            rid[0].usUsage = 0x06;
            rid[0].dwFlags = (uint)RawInputDevice_dwFlags.RIDEV_INPUTSINK;
            rid[0].hwndTarget = windowHandle;

            //Mouse
            rid[1].usUsagePage = 0x01;
            rid[1].usUsage = 0x02;
            rid[1].dwFlags = (uint)RawInputDevice_dwFlags.RIDEV_INPUTSINK;
            rid[1].hwndTarget = windowHandle;

            ////Gamepad
            //rid[2].usUsagePage = 0x01;
            //rid[2].usUsage = 0x05;
            //rid[2].dwFlags = (uint)RawInputDevice_dwFlags.RIDEV_INPUTSINK;
            //rid[2].hwndTarget = windowHandle;

            bool success = WinApi.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[0]));
            Logger.WriteLine($"Succeeded RegisterRawInputDevices Keyboard = {success}");

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.WriteLine($"Error code = {error}");
            }

            success = WinApi.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[1]));
            Logger.WriteLine($"Succeeded RegisterRawInputDevices Mouse = {success}");

            if (!success)
            {
                int error = Marshal.GetLastWin32Error();
                Logger.WriteLine($"Error code = {error}");
            }

            //success = WinApi.RegisterRawInputDevices(rid, (uint)rid.Length, (uint)Marshal.SizeOf(rid[2]));
            ////Logger.WriteLine($"Succeeded RegisterRawInputDevices Gamepad = {success}");

            //if (!success)
            //{
            //    int error = Marshal.GetLastWin32Error();
            //    Logger.WriteLine($"Error code = {error}");
            //}
        }

        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern uint GetRawInputDeviceInfo(
          [In] IntPtr hDevice,
          [In] RawInputDeviceInformationCommand uiCommand,
          [In, Out] IntPtr pData,
          [In, Out] ref uint pcbSize);

        public enum RawInputDeviceInformationCommand : int
        {
            /// <summary>
            /// pData points to a string that contains the device name. For this uiCommand only, the value in pcbSize is the character count (not the byte count).
            /// </summary>
            RIDI_DEVICENAME = 0x20000007,
            /// <summary>
            /// pData points to an RID_DEVICE_INFO structure.
            /// </summary>
            RIDI_DEVICEINFO = 0x2000000b,
            /// <summary>
            /// pData points to the previously parsed data.
            /// </summary>
            RIDI_PREPARSEDDATA = 0x20000005
        }


        public static IEnumerable<(RID_DEVICE_INFO deviceInfo, IntPtr deviceHandle, string deviceName)> GetDeviceList()
        {
            uint numDevices = 0;
            int cbSize = Marshal.SizeOf(typeof(RAWINPUTDEVICELIST));

            if (WinApi.GetRawInputDeviceList(IntPtr.Zero, ref numDevices, (uint)cbSize) == 0)//Return value isn't zero if there is an error
            {
                IntPtr pRawInputDeviceList = Marshal.AllocHGlobal((int)(cbSize * numDevices));
                WinApi.GetRawInputDeviceList(pRawInputDeviceList, ref numDevices, (uint)cbSize);

                for (int i = 0; i < numDevices; i++)
                {
                    RAWINPUTDEVICELIST rid = (RAWINPUTDEVICELIST)Marshal.PtrToStructure(new IntPtr(pRawInputDeviceList.ToInt32() + (cbSize * i)), typeof(RAWINPUTDEVICELIST));

                    uint pcbSize = 0;
                    WinApi.GetRawInputDeviceInfo(rid.hDevice, 0x2000000b, IntPtr.Zero, ref pcbSize);//Get the size required in memory
                    IntPtr pData = Marshal.AllocHGlobal((int)pcbSize);
                    WinApi.GetRawInputDeviceInfo(rid.hDevice, 0x2000000b, pData, ref pcbSize);
                    RID_DEVICE_INFO device = (RID_DEVICE_INFO)Marshal.PtrToStructure(pData, typeof(RID_DEVICE_INFO));

                    string name = "";

                    if (rid.dwType <= 1)
                    {
                        RawInputDeviceName deviceName = new RawInputDeviceName();
                        name = deviceName.GetDeviceName(pData, rid.hDevice);
                        deviceName.Dispose();
                    }

                    Marshal.FreeHGlobal(pData);//Possibly fix a very random Heap Corruption exception(???)

                    yield return (device, rid.hDevice, name);
                }

                Marshal.FreeHGlobal(pRawInputDeviceList);
            }
        }

        public static List<PlayerInfo> GetDeviceInputInfos()
        {
            int i = 100;

            List<PlayerInfo> rawDevices = new List<PlayerInfo>();

            foreach ((RID_DEVICE_INFO deviceInfo, IntPtr deviceHandle, string deviceName) device in GetDeviceList().Where(x => x.deviceInfo.dwType <= 1))
            {
                PlayerInfo player = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = device.deviceInfo.dwType == 0,
                    IsRawKeyboard = device.deviceInfo.dwType == 1,
                    HIDDeviceID = new string[] { device.deviceName, "" }
                };

                if (player.IsRawMouse)
                {
                    player.RawMouseDeviceHandle = device.deviceHandle;
                }

                if (player.IsRawKeyboard)
                {
                    player.RawKeyboardDeviceHandle = device.deviceHandle;

                };

                player.IsKeyboardPlayer = true;

                rawDevices.Add(player);
            }

            // Zero device handle mouse
            {
                PlayerInfo playerMouseZero = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = true,
                    IsRawKeyboard = false,
                    HIDDeviceID = new string[] { "MouseHandleZero", "" }
                };

                playerMouseZero.RawMouseDeviceHandle = IntPtr.Zero;
                playerMouseZero.RawKeyboardDeviceHandle = (IntPtr)(-1);
                playerMouseZero.IsKeyboardPlayer = true;

                rawDevices.Add(playerMouseZero);
            }

            // Zero device handle keyboard
            {
                PlayerInfo playerKeyboardZero = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = false,
                    IsRawKeyboard = true,
                    HIDDeviceID = new string[] { "KeyboardHandleZero", "" }
                };

                playerKeyboardZero.RawKeyboardDeviceHandle = IntPtr.Zero;
                playerKeyboardZero.RawMouseDeviceHandle = (IntPtr)(-1);
                playerKeyboardZero.IsKeyboardPlayer = true;

                rawDevices.Add(playerKeyboardZero);
            }

            return rawDevices.OrderBy(dv => dv.IsRawKeyboard).ToList();
        }
    }
}
