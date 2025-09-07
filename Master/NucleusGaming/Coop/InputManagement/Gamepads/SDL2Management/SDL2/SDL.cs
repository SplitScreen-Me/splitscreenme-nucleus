using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SDL
{
    [Flags]
    public enum InitFlags : uint
    {
        None = 0,
        Timer = 0x00000001u,
        Audio = 0x00000010u,
        Video = 0x00000020u,
        Joystick = 0x00000200u,
        Haptic = 0x00001000u,
        GameController = 0x00002000u,
        Events = 0x00004000u,
        NoParachute = 0x00100000u,
        Everything = Timer | Audio | Video | Events
            | Joystick | Haptic | GameController
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GUID
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        public byte[] data;
    }

    public static unsafe partial class SDL2
    {
        public enum SDL_bool
        {
            SDL_FALSE = 0,
            SDL_TRUE = 1
        }
        
        [DllImport(LibraryName, EntryPoint = "SDL_Init", CallingConvention = CallingConvention.Cdecl)]
        public static extern int Init(InitFlags flags);

        [DllImport(LibraryName, EntryPoint = "SDL_InitSubSystem", CallingConvention = CallingConvention.Cdecl)]
        public static extern int InitSubSystem(InitFlags flags);

        [DllImport(LibraryName, EntryPoint = "SDL_QuitSubSystem", CallingConvention = CallingConvention.Cdecl)]
        public static extern void QuitSubSystem(InitFlags flags);

        [DllImport(LibraryName, EntryPoint = "SDL_WasInit", CallingConvention = CallingConvention.Cdecl)]
        public static extern InitFlags WasInit(InitFlags flags);

        [DllImport(LibraryName, EntryPoint = "SDL_Quit", CallingConvention = CallingConvention.Cdecl)]
        public static extern void Quit();

        [DllImport(LibraryName, EntryPoint = "SDL_Delay", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_Delay(UInt32 ms);

        [DllImport(LibraryName, EntryPoint = "SDL_SetHint", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_bool SDL_SetHint(string hintName,string hintValue);
      
        unsafe public static string GetString(byte* ptr)
        {
            int count = 0;
            while (*(ptr + count) != 0)
            {
                count += 1;
            }

            return Encoding.UTF8.GetString(ptr, count);
        }
    }
}
