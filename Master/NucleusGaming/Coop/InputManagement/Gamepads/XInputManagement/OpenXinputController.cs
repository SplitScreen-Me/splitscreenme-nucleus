// More or less a copy of the Controller class from SharpDX, except without the limit on 4 controllers 
// since we are going to be using OpenXinput

using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;
using SharpDX.XInput;
using System;
using System.Runtime.InteropServices;
using System.Text;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;

namespace Nucleus.Gaming.Coop
{
    public class OpenXinputController
    {
        public static class NativeOpenXinput
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct CapabilitiesEx
            {
                public Capabilities Capabilities;  // Nested structure
                public ushort VendorId;       // WORD -> ushort (16-bit)
                public ushort ProductId;      // WORD -> ushort (16-bit)
                public ushort ProductVersion; // WORD -> ushort (16-bit)
                public ushort unk1;           // WORD -> ushort (16-bit)
                public uint unk2;             // DWORD -> uint (32-bit)
            }

            public static unsafe int XInputSetState(int dwUserIndex, Vibration vibrationRef)
            {
                return XInputSetState_(dwUserIndex, &vibrationRef);
            }

            [DllImport("openxinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputSetState")]
            private static extern unsafe int XInputSetState_(int arg0, void* arg1);

            [DllImport("openxinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputGetState")]
            public static extern int XInputGetState(int dwUserIndex, out State stateRef);

            [DllImport("openxinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputGetCapabilities")]
            public static extern int XInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities capabilitiesRef);

        }

        public static class NativeXinput
        {
            public static unsafe int XInputSetState(int dwUserIndex, Vibration vibrationRef)
            {
                return XInputSetState_(dwUserIndex, &vibrationRef);
            }

            [DllImport("xinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputSetState")]
            private static extern unsafe int XInputSetState_(int arg0, void* arg1);

            [DllImport("xinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputGetState")]
            public static extern int XInputGetState(int dwUserIndex, out State stateRef);

            [DllImport("xinput1_3.dll", CallingConvention = CallingConvention.StdCall, EntryPoint = "XInputGetCapabilities")]
            public static extern int XInputGetCapabilities(int dwUserIndex, DeviceQueryType dwFlags, out Capabilities capabilitiesRef);
        }

        private readonly int userIndex;
        public bool openXinput;

        public OpenXinputController(bool openXinput, int userIndex = 255)
        {
            this.userIndex = userIndex;
            this.openXinput = openXinput;
        }


        /// <summary>
        /// Device interface path of the pad in an OpenXinput slot. Only in the custom openxinput1_3.dll shipped since Nucleus
        /// 2.5: use <see cref="TryGetDevicePath"/>, which survives an older DLL.
        /// </summary>
        [DllImport("openxinput1_3.dll", CallingConvention = CallingConvention.Winapi, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern uint OpenXInputGetDevicePath(uint dwUserIndex, StringBuilder devicePath, uint cchDevicePath);

        //-1 not tried yet, 0 the loaded openxinput1_3.dll has no OpenXInputGetDevicePath, 1 it has.
        private static int devicePathSupport = -1;

        /// <summary>Whether the loaded openxinput1_3.dll can give the device path of a slot (false until a call succeeded).</summary>
        public static bool DevicePathSupported => devicePathSupport == 1;

        /// <summary>
        /// The device interface path of the pad in OpenXinput slot <paramref name="userIndex"/> (for example
        /// "\\?\USB#VID_045E&amp;PID_028E#…#{…}"), which lets the setup bind an XInput pad to its DirectInput device without
        /// a button press. False when the slot is empty, or always when openxinput1_3.dll is an older build without the
        /// export (logged once).
        /// </summary>
        public static bool TryGetDevicePath(int userIndex, out string path)
        {
            path = null;

            if (devicePathSupport == 0 || userIndex < 0)
            {
                return false;
            }

            try
            {
                StringBuilder buffer = new StringBuilder(1024);
                uint result = OpenXInputGetDevicePath((uint)userIndex, buffer, (uint)buffer.Capacity);
                devicePathSupport = 1;

                if (result != 0)//ERROR_SUCCESS
                {
                    return false;
                }

                path = buffer.ToString();
                return !string.IsNullOrEmpty(path);
            }
            catch (EntryPointNotFoundException)
            {
                devicePathSupport = 0;
                LogManager.Log("Gamepad: openxinput1_3.dll has no OpenXInputGetDevicePath (older build): XInput pads are identified by a button press.");
                return false;
            }
            catch (DllNotFoundException)
            {
                devicePathSupport = 0;
                return false;
            }
        }

        [DllImport("openxinput1_3.dll", EntryPoint = "#108", CallingConvention = CallingConvention.StdCall)]
        public static extern uint XInputGetCapabilitiesEx(
        uint dwReserved,   // Must be 1
        uint dwUserIndex,  // Controller index
        uint dwFlags,      // Device query flags
        out CapabilitiesEx pCapabilitiesEx // Receives capabilities
        );

        public Capabilities GetCapabilities(DeviceQueryType deviceQueryType)
        {
            ErrorCodeHelper.ToResult(
                openXinput ? NativeOpenXinput.XInputGetCapabilities(userIndex, deviceQueryType, out Capabilities temp) :
                             NativeXinput.XInputGetCapabilities(userIndex, deviceQueryType, out temp)
                ).CheckError();
            return temp;
        }

        public bool GetCapabilities(DeviceQueryType deviceQueryType, out Capabilities capabilities)
        {
            return (openXinput ?
                       NativeOpenXinput.XInputGetCapabilities(userIndex, deviceQueryType, out capabilities) :
                       NativeXinput.XInputGetCapabilities(userIndex, deviceQueryType, out capabilities)) == 0;
        }

        public State GetState()
        {
            ErrorCodeHelper.ToResult(
                openXinput ? NativeOpenXinput.XInputGetState(userIndex, out State temp) : NativeXinput.XInputGetState(userIndex, out temp)).CheckError();
            return temp;
        }

        public bool GetState(out State state)
        {
            return (openXinput ?
                       NativeOpenXinput.XInputGetState(userIndex, out state) :
                       NativeXinput.XInputGetState(userIndex, out state)) == 0;
        }

        public Result SetVibration(Vibration vibration)
        {
            Result result = ErrorCodeHelper.ToResult(openXinput ?
                NativeOpenXinput.XInputSetState(userIndex, vibration) : NativeXinput.XInputSetState(userIndex, vibration));
            result.CheckError();
            return result;
        }

        public bool IsConnected => (openXinput ? NativeOpenXinput.XInputGetState(userIndex, out State temp) : NativeXinput.XInputGetState(userIndex, out temp)) == 0;
    }

}
