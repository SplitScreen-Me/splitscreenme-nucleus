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
            Logger.WriteLine($"Attempting to RegisterRawInputDevices for window handle {windowHandle}");

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
        }

        public static IEnumerable<(RID_DEVICE_INFO deviceInfo, IntPtr deviceHandle)> GetDeviceList()
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
                    if (device.dwType == 0)
                    {
                        //Mouse
                        Logger.WriteLine($"Found mouse. Mouse ID = {device.mouse.dwId}, number of buttons = {device.mouse.dwNumberOfButtons}, sample rate = {device.mouse.dwSampleRate}, has horizontal wheel = {device.mouse.dwSampleRate}");
                    }
                    else if (device.dwType == 1)
                    {
                        //Keyboard
                        Logger.WriteLine($"Found keyboard. Keyboard type = {device.keyboard.dwType}, keyboard subtype = {device.keyboard.dwSubType}, scan code mode = {device.keyboard.dwKeyboardMode}, number of keys = {device.keyboard.dwNumberOfKeysTotal}");
                    }
                    yield return (device, rid.hDevice);
                }

                Marshal.FreeHGlobal(pRawInputDeviceList);
            }
        }

        public static IEnumerable<PlayerInfo> GetDeviceInputInfos()
        {
            //TODO: Add device handle zero mouse & keyboard w/ special icon

            int i = 100;

            foreach ((RID_DEVICE_INFO deviceInfo, IntPtr deviceHandle) device in GetDeviceList().Where(x => x.deviceInfo.dwType <= 1))
            {
                PlayerInfo player = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = device.deviceInfo.dwType == 0,
                    IsRawKeyboard = device.deviceInfo.dwType == 1,
                    HIDDeviceID = "T" + device.deviceInfo.dwType + "PID" + device.deviceInfo.hid.dwProductId + "VID" + device.deviceInfo.hid.dwVendorId + "VN" + device.deviceInfo.hid.dwVersionNumber
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
                yield return player;
            }

            // Zero device handle mouse
            {
                PlayerInfo playerMouseZero = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = true,
                    IsRawKeyboard = false,
                    HIDDeviceID = "MouseHandleZero"
                };
                playerMouseZero.RawMouseDeviceHandle = IntPtr.Zero;
                playerMouseZero.RawKeyboardDeviceHandle = (IntPtr)(-1);
                playerMouseZero.IsKeyboardPlayer = true;
                yield return playerMouseZero;
            }


            // Zero device handle keyboard
            {
                PlayerInfo playerKeyboardZero = new PlayerInfo
                {
                    GamepadId = i++,
                    IsRawMouse = false,
                    IsRawKeyboard = true,
                    HIDDeviceID = "KeyboardHandleZero"
                };
                playerKeyboardZero.RawKeyboardDeviceHandle = IntPtr.Zero;
                playerKeyboardZero.RawMouseDeviceHandle = (IntPtr)(-1);
                playerKeyboardZero.IsKeyboardPlayer = true;
                yield return playerKeyboardZero;
            }
        }
    }
}
