using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Keysym
    {
        public Scancode Scancode;
        public Keycode Sym;
        public KeyModifier Mod;
        public uint Unused;
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_GetKeyFromName", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern Keycode GetKeyFromName(string name);

        [DllImport(LibraryName, EntryPoint = "SDL_GetKeyFromScancode", CallingConvention = CallingConvention.Cdecl)]
        public static extern Keycode GetKeyFromScancode(Scancode scancode);

        [DllImport(LibraryName, EntryPoint = "SDL_GetKeyName", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetKeyName(Keycode key);

        public static string GetKeyNameString(Keycode key)
        {
            return GetString(GetKeyName(key));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_GetKeyboardFocus", CallingConvention = CallingConvention.Cdecl)]
        public static extern Window GetKeyboardFocus();

        [DllImport(LibraryName, EntryPoint = "SDL_GetKeyboardState", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetKeyboardState(int* numKeys);

        [DllImport(LibraryName, EntryPoint = "SDL_GetModState", CallingConvention = CallingConvention.Cdecl)]
        public static extern KeyModifier GetModState();

        [DllImport(LibraryName, EntryPoint = "SDL_GetScancodeFromKey", CallingConvention = CallingConvention.Cdecl)]
        public static extern Scancode GetScancodeFromKey(Keycode key);

        [DllImport(LibraryName, EntryPoint = "SDL_GetScancodeFromName", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern Scancode GetScancodeFromName(string name);

        [DllImport(LibraryName, EntryPoint = "SDL_GetScancodeName", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetScancodeName(Scancode scancode);

        public static string GetScancodeNameString(Scancode scancode)
        {
            return GetString(GetScancodeName(scancode));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_HasScreenKeyboardSupport", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HasScreenKeyboardSupport();

        [DllImport(LibraryName, EntryPoint = "SDL_IsScreenKeyboardShown", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsScreenKeyboardShown(Window window);

        [DllImport(LibraryName, EntryPoint = "SDL_IsTextInputActive", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsTextInputActive();

        [DllImport(LibraryName, EntryPoint = "SDL_SetModState", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetModState(KeyModifier modstate);

        [DllImport(LibraryName, EntryPoint = "SDL_SetTextInputRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetTextInputRect(Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_StartTextInput", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StartTextInput();

        [DllImport(LibraryName, EntryPoint = "SDL_StopTextInput", CallingConvention = CallingConvention.Cdecl)]
        public static extern void StopTextInput();
    }
}
