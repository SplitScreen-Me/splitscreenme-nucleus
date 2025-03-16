using System;
using System.Runtime.InteropServices;

namespace SDL
{
    public enum SystemCursor
    {
        Arrow,
        IBeam,
        Wait,
        Crosshair,
        WaitArrow,
        SizeNWSE,
        SizeNESW,
        SizeWE,
        SizeNS,
        SizeALL,
        NO,
        Hand
    }

    public enum MouseWheelDirection : uint
    {
        Normal,
        Flipped
    }

    public enum Button : uint
    {
        Left = 1,
        Middle = 2,
        Right = 3,
        X1 = 4,
        X2 = 5,
    }

    [Flags]
    public enum ButtonMask : uint
    {
        Left = 1 << ((int)Button.Left - 1),
        Middle = 1 << ((int)Button.Middle - 1),
        Right = 1 << ((int)Button.Right - 1),
        X1 = 1 << ((int)Button.X1 - 1),
        X2 = 1 << ((int)Button.X2 - 1)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Cursor
    {
        private readonly IntPtr ptr;

        public Cursor(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(Cursor cursor)
        {
            return cursor.ptr;
        }

        public static implicit operator Cursor(IntPtr ptr)
        {
            return new Cursor(ptr);
        }
    }

    public static unsafe partial class SDL2
    {
        public static uint MOUSE(uint x)
        {
            return (uint)(1 << ((int)x - 1));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_CaptureMouse", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CaptureMouse(bool enabled);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern Cursor CreateCursor(byte* data, byte* mask, int w, int h, int hotX, int hotY);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateSystemCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern Cursor CreateSystemCursor(SystemCursor id);

        [DllImport(LibraryName, EntryPoint = "SDL_FreeCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeCursor(Cursor cursor);

        [DllImport(LibraryName, EntryPoint = "SDL_GetCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern Cursor GetCursor();

        [DllImport(LibraryName, EntryPoint = "SDL_GetDefaultCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern Cursor GetDefaultCursor();

        [DllImport(LibraryName, EntryPoint = "SDL_GetGlobalMouseState", CallingConvention = CallingConvention.Cdecl)]
        public static extern ButtonMask GetGlobalMouseState(int* x, int* y);

        [DllImport(LibraryName, EntryPoint = "SDL_GetMouseFocus", CallingConvention = CallingConvention.Cdecl)]
        public static extern Window GetMouseFocus();

        [DllImport(LibraryName, EntryPoint = "SDL_GetMouseState", CallingConvention = CallingConvention.Cdecl)]
        public static extern ButtonMask GetMouseState(int* x, int* y);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetRelativeMouseMode();

        [DllImport(LibraryName, EntryPoint = "SDL_GetRelativeMouseState", CallingConvention = CallingConvention.Cdecl)]
        public static extern ButtonMask GetRelativeMouseState(int* x, int* y);

        [DllImport(LibraryName, EntryPoint = "SDL_SetCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetCursor(Cursor cursor);

        [DllImport(LibraryName, EntryPoint = "SDL_SetRelativeMouseMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetRelativeMouseMode(bool enabled);

        [DllImport(LibraryName, EntryPoint = "SDL_ShowCursor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ShowCursor(int toggle);

        [DllImport(LibraryName, EntryPoint = "SDL_WarpMouseGlobal", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WarpMouseGlobal(int x, int y);

        [DllImport(LibraryName, EntryPoint = "SDL_WarpMouseInWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void WarpMouseInWindow(Window window, int x, int y);
    }
}
