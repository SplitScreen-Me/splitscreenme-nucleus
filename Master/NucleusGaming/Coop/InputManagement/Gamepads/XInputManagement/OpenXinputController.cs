// More or less a copy of the Controller class from SharpDX, except without the limit on 4 controllers 
// since we are going to be using OpenXinput

using SharpDX;
using SharpDX.Mathematics.Interop;
using SharpDX.Win32;
using SharpDX.XInput;
using System.Runtime.InteropServices;
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
