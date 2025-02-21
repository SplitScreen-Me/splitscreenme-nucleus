using System;
using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Version
    {
        public const int SizeInBytes = 3;
        public byte Major;
        public byte Minor;
        public byte Patch;
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_GetVersion", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetVersion(Version* version);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRevision", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetRevision();

        public static string GetRevisionString()
        {
            return GetString(GetRevision());
        }

        [DllImport(LibraryName, EntryPoint = "SDL_GetRevisionNumber", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRevisionNumber();
    }
}
