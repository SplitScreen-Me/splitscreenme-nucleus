using System.Runtime.InteropServices;

namespace SDL
{
    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_GetClipboardText", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetClipboardText();

        public static string GetClipboardTextString()
        {
            return GetString(GetClipboardText());
        }

        [DllImport(LibraryName, EntryPoint = "SDL_HasClipboardText", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HasClipboardText();

        [DllImport(LibraryName, EntryPoint = "SDL_SetClipboardText", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int SetClipboardText(string text);
    }
}
