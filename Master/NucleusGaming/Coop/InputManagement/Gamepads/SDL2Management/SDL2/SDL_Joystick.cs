using System;
using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_JoystickGUID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] data;
    }

    public enum JoystickType
    {
        Unknown,
        GameController,
        Wheel,
        ArcadeStick,
        FlightStick,
        DancePad,
        Guitar,
        DrumKit,
        ArcadePad,
        Throttle
    }

    public enum JoystickPowerLevel
    {
        Unknown = -1,
        Empty,
        Low,
        Medium,
        Full,
        Wired,
        Max
    }

    public enum Hat : byte
    {
        Centered = 0x00,
        Up = 0x01,
        Right = 0x02,
        Down = 0x04,
        Left = 0x08,

        RightUp = Right | Up,
        RightDown = Right | Down,
        LeftUp = Left | Up,
        LeftDown = Left | Down
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_Joystick
    {
        private readonly IntPtr ptr;

        public SDL_Joystick(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(SDL_Joystick joystick)
        {
            return joystick.ptr;
        }

        public static implicit operator SDL_Joystick(IntPtr ptr)
        {
            return new SDL_Joystick(ptr);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JoystickID
    {
        private readonly int id;

        public JoystickID(int id)
        {
            this.id = id;
        }

        public static implicit operator int(JoystickID joystickID)
        {
            return joystickID.id;
        }
    }

    public static unsafe partial class SDL2
    {

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern void JoystickClose(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickCurrentPowerLevel", CallingConvention = CallingConvention.Cdecl)]
        public static extern JoystickPowerLevel JoystickCurrentPowerLevel(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickFromInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_Joystick JoystickFromInstanceID(JoystickID joyid);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetAttached", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool JoystickGetAttached(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern short JoystickGetAxis(SDL_Joystick joystick, int axis);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetBall", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickGetBall(SDL_Joystick joystick, int ball, int* dx, int* dy);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte JoystickGetButton(SDL_Joystick joystick, int button);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetDeviceGUID", CallingConvention = CallingConvention.Cdecl)]
        public static extern Guid JoystickGetDeviceGUID(int deviceIndex);


        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetDeviceVendor", CallingConvention = CallingConvention.Cdecl)]
        public static extern ushort JoystickGetDeviceVendor(int deviceIndex);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetGUID", CallingConvention = CallingConvention.Cdecl)]
        public static extern Guid JoystickGetGUID(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetGUIDFromString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern Guid JoystickGetGUIDFromString(string pchGUID);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetGUIDString", CallingConvention = CallingConvention.Cdecl)]
        public static extern void JoystickGetGUIDString(Guid guid, byte* pszGUID, int cbGUID);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickGetHat", CallingConvention = CallingConvention.Cdecl)]
        public static extern Hat JoystickGetHat(SDL_Joystick joystick, int hat);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern JoystickID JoystickInstanceID(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickName", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* JoystickName(SDL_Joystick joystick);

        public static string JoystickNameString(SDL_Joystick joystick)
        {
            return GetString(JoystickName(joystick));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickNameForIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* JoystickNameForIndex(int deviceIndex);

        public static string JoystickNameForIndexString(int deviceIndex)
        {
            return GetString(JoystickNameForIndex(deviceIndex));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickNumAxes", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumAxes(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickNumBalls", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumBalls(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickNumButtons", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumButtons(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickNumHats", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickNumHats(SDL_Joystick joystick);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_Joystick JoystickOpen(int deviceIndex);

        [DllImport(LibraryName, EntryPoint = "SDL_JoystickUpdate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void JoystickUpdate();

        [DllImport(LibraryName, EntryPoint = "SDL_NumJoysticks", CallingConvention = CallingConvention.Cdecl)]
        public static extern int NumJoysticks();

        [DllImport("SDL2.dll", EntryPoint = "SDL_JoystickRumble", CallingConvention = CallingConvention.Cdecl)]
        public static extern int JoystickRumble(SDL_Joystick joystick, ushort low_frequency_rumble, ushort high_frequency_rumble, uint duration_ms);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_JoystickGetDeviceIndexFromInstanceID(int instance_id);

    }
}
