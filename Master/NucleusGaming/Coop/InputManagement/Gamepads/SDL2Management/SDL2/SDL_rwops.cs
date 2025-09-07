using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SDL
{
    public static class SDL_Utf8EncodeHeap
    {
        internal static int Utf8Size(string str)
        {
            if (str == null)
            {
                return 0;
            }
            return (str.Length * 4) + 1;
        }

        internal static unsafe byte* Utf8EncodeHeap(string str)
        {
            if (str == null)
            {
                return (byte*)0;
            }

            int bufferSize = Utf8Size(str);
            byte* buffer = (byte*)Marshal.AllocHGlobal(bufferSize);
            fixed (char* strPtr = str)
            {
                Encoding.UTF8.GetBytes(strPtr, str.Length + 1, buffer, bufferSize);
            }
            return buffer;
        }
    }

    public static class SDL_rwops
    {
        #region SDL_rwops.h

        public const int RW_SEEK_SET = 0;
        public const int RW_SEEK_CUR = 1;
        public const int RW_SEEK_END = 2;

        public const UInt32 SDL_RWOPS_UNKNOWN = 0; /* Unknown stream type */
        public const UInt32 SDL_RWOPS_WINFILE = 1; /* Win32 file */
        public const UInt32 SDL_RWOPS_STDFILE = 2; /* Stdio file */
        public const UInt32 SDL_RWOPS_JNIFILE = 3; /* Android asset */
        public const UInt32 SDL_RWOPS_MEMORY = 4; /* Memory stream */
        public const UInt32 SDL_RWOPS_MEMORY_RO = 5; /* Read-Only memory stream */

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long SDLRWopsSizeCallback(IntPtr context);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate long SDLRWopsSeekCallback(
            IntPtr context,
            long offset,
            int whence
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr SDLRWopsReadCallback(
            IntPtr context,
            IntPtr ptr,
            IntPtr size,
            IntPtr maxnum
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr SDLRWopsWriteCallback(
            IntPtr context,
            IntPtr ptr,
            IntPtr size,
            IntPtr num
        );

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int SDLRWopsCloseCallback(
            IntPtr context
        );

        [StructLayout(LayoutKind.Sequential)]
        public struct SDL_RWops
        {
            public IntPtr size;
            public IntPtr seek;
            public IntPtr read;
            public IntPtr write;
            public IntPtr close;

            public UInt32 type;

            /* NOTE: This isn't the full structure since
			 * the native SDL_RWops contains a hidden union full of
			 * internal information and platform-specific stuff depending
			 * on what conditions the native library was built with
			 */
        }


    }

    public static unsafe partial class SDL2
    {
        /* IntPtr refers to an SDL_RWops* */
        [DllImport("SDL2.dll", EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe IntPtr INTERNAL_SDL_RWFromFile(byte* file, byte* mode);

        public static unsafe IntPtr SDL_RWFromFile(string file, string mode)
        {
            byte* utf8File = SDL_Utf8EncodeHeap.Utf8EncodeHeap(file);
            byte* utf8Mode = SDL_Utf8EncodeHeap.Utf8EncodeHeap(mode);
            IntPtr rwOps = INTERNAL_SDL_RWFromFile(
                utf8File,
                utf8Mode
            );
            Marshal.FreeHGlobal((IntPtr)utf8Mode);
            Marshal.FreeHGlobal((IntPtr)utf8File);
            return rwOps;
        }

        /* IntPtr refers to an SDL_RWops* */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_AllocRW();

        /* area refers to an SDL_RWops* */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_FreeRW(IntPtr area);

        /* fp refers to a void* */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromFP(IntPtr fp, SDL.SDL2.SDL_bool autoclose);

        /* mem refers to a void*, IntPtr to an SDL_RWops* */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromMem(IntPtr mem, int size);

        /* mem refers to a const void*, IntPtr to an SDL_RWops* */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr SDL_RWFromConstMem(IntPtr mem, int size);

        /* context refers to an SDL_RWops*.
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWsize(IntPtr context);

        /* context refers to an SDL_RWops*.
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWseek(
            IntPtr context,
            long offset,
            int whence
        );

        /* context refers to an SDL_RWops*.
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWtell(IntPtr context);

        /* context refers to an SDL_RWops*, ptr refers to a void*.
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWread(
            IntPtr context,
            IntPtr ptr,
            IntPtr size,
            IntPtr maxnum
        );

        /* context refers to an SDL_RWops*, ptr refers to a const void*.
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWwrite(
            IntPtr context,
            IntPtr ptr,
            IntPtr size,
            IntPtr maxnum
        );

        /* Read endian functions */

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte SDL_ReadU8(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 SDL_ReadLE16(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt16 SDL_ReadBE16(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 SDL_ReadLE32(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt32 SDL_ReadBE32(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 SDL_ReadLE64(IntPtr src);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern UInt64 SDL_ReadBE64(IntPtr src);

        /* Write endian functions */

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteU8(IntPtr dst, byte value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteLE16(IntPtr dst, UInt16 value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteBE16(IntPtr dst, UInt16 value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteLE32(IntPtr dst, UInt32 value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteBE32(IntPtr dst, UInt32 value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteLE64(IntPtr dst, UInt64 value);

        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint SDL_WriteBE64(IntPtr dst, UInt64 value);

        /* context refers to an SDL_RWops*
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern long SDL_RWclose(IntPtr context);

        /* datasize refers to a size_t*
         * IntPtr refers to a void*
         * Only available in SDL 2.0.10 or higher.
         */
        [DllImport("SDL2.dll", EntryPoint = "SDL_LoadFile", CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe IntPtr INTERNAL_SDL_LoadFile(byte* file, out IntPtr datasize);
        public static unsafe IntPtr SDL_LoadFile(string file, out IntPtr datasize)
        {
            byte* utf8File = SDL.SDL_Utf8EncodeHeap.Utf8EncodeHeap(file);
            IntPtr result = INTERNAL_SDL_LoadFile(utf8File, out datasize);
            Marshal.FreeHGlobal((IntPtr)utf8File);
            return result;
        }

        #endregion
    }
}

