using System;
using System.Runtime.InteropServices;

namespace SDL
{
    public enum LogPriority : int
    {
        Verbose = 1,
        Debug,
        Info,
        Warn,
        Error,
        Critical
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_LogSetAllPriority", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void LogSetAllPriority(LogPriority priority);

        [DllImport(LibraryName, EntryPoint = "SDL_Log", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true, SetLastError = true)]
        public static extern void Log(string fmt);
    }
}
