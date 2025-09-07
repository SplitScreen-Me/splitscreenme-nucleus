using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Color
    {
        public byte R;
        public byte G;
        public byte B;
        public byte A;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Palette
    {
        public int NumColors;
        public Color* Colors;
        public uint Version;
        public int RefCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PixelFormat
    {
        public uint Format;
        public Palette* Palette;
        public byte BitsPerPixel;
        public byte BytesPerPixel;
        public fixed byte Padding[2];
        public uint RMask;
        public uint GMask;
        public uint BMask;
        public uint AMask;
        public byte RLoss;
        public byte GLoss;
        public byte BLoss;
        public byte ALoss;
        public byte RShift;
        public byte GShift;
        public byte BShift;
        public byte AShift;
        public int RefCount;
        public PixelFormat* Next;
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_AllocFormat", CallingConvention = CallingConvention.Cdecl)]
        public static extern PixelFormat* AllocFormat(uint pixel_format);

        [DllImport(LibraryName, EntryPoint = "SDL_AllocPalette", CallingConvention = CallingConvention.Cdecl)]
        public static extern Palette* AllocPalette(int ncolors);

        [DllImport(LibraryName, EntryPoint = "SDL_CalculateGammaRamp", CallingConvention = CallingConvention.Cdecl)]
        public static extern void CalculateGammaRamp(float gamma, ushort* ramp);

        [DllImport(LibraryName, EntryPoint = "SDL_FreeFormat", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeFormat(PixelFormat* format);

        [DllImport(LibraryName, EntryPoint = "SDL_FreePalette", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreePalette(Palette* palette);

        [DllImport(LibraryName, EntryPoint = "SDL_GetPixelFormatName", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GetPixelFormatName(uint format);

        public static string GetPixelFormatNameString(uint format) => GetString(GetPixelFormatName(format));

        [DllImport(LibraryName, EntryPoint = "SDL_GetRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetRGB(uint pixel, PixelFormat* format, byte* r, byte* g, byte* b);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetRGBA(uint pixel, PixelFormat* format, byte* r, byte* g, byte* b, byte* a);

        [DllImport(LibraryName, EntryPoint = "SDL_MapRGB", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MapRGB(PixelFormat* format, byte r, byte g, byte b);

        [DllImport(LibraryName, EntryPoint = "SDL_MapRGBA", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MapRGBA(PixelFormat* format, byte r, byte g, byte b, byte a);

        [DllImport(LibraryName, EntryPoint = "SDL_MasksToPixelFormatEnum", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint MasksToPixelFormatEnum(int bpp, uint Rmask, uint Gmask, uint Bmask, uint Amask);

        [DllImport(LibraryName, EntryPoint = "SDL_PixelFormatEnumToMasks", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool PixelFormatEnumToMasks(uint format, int* bpp, uint* Rmask, uint* Gmask, uint* Bmask, uint* Amask);

        [DllImport(LibraryName, EntryPoint = "SDL_SetPaletteColors", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetPaletteColors(Palette* palette, Color* colors, int firstcolor, int ncolors);

        [DllImport(LibraryName, EntryPoint = "SDL_SetPixelFormatPalette", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetPixelFormatPalette(PixelFormat* format, Palette* palette);
    }
}
