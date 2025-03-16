using System;
using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct Surface
    {
        public uint Flags;
        public PixelFormat* Format;
        public int W;
        public int H;
        public int Pitch;
        public void* Pixels;
        public void* UserData;
        public int Locked;
        public void* LockData;
        public Rect ClipRect;
        public IntPtr Map;
        public int RefCount;
    }

    public static unsafe partial class SDL2
    {
        public const uint SWSURFACE = 0;
        public const uint PREALLOC = 0x00000001;
        public const uint RLEACCEL = 0x00000002;
        public const uint DONTFREE = 0x00000004;

        public static int BlitScaled(Surface* src, Rect* srcrect, Surface* dst, Rect* dstrect) => UpperBlitScaled(src, srcrect, dst, dstrect);

        public static int BlitSurface(Surface* src, Rect* srcrect, Surface* dst, Rect* dstrect) => UpperBlit(src, srcrect, dst, dstrect);

        [DllImport(LibraryName, EntryPoint = "SDL_ConvertPixels", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ConvertPixels(int width, int height, uint src_format, void* src, int src_pitch, uint dst_format, void* dst, int dst_pitch);

        [DllImport(LibraryName, EntryPoint = "SDL_ConvertSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* ConvertSurface(Surface* src, PixelFormat* fmt, uint flags);

        [DllImport(LibraryName, EntryPoint = "SDL_ConvertSurfaceFormat", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* ConvertSurfaceFormat(Surface* src, uint pixel_format, uint flags);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateRGBSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* CreateRGBSurface(uint flags, int width, int height, int depth, uint Rmask, uint Gmask, uint Bmask, uint Amask);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateRGBSurfaceFrom", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* CreateRGBSurfaceFrom(void* pixels, int width, int height, int depth, int pitch, uint Rmask, uint Gmask, uint Bmask, uint Amask);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateRGBSurfaceWithFormat", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* CreateRGBSurfaceWithFormat(uint flags, int width, int height, int depth, uint format);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateRGBSurfaceWithFormatFrom", CallingConvention = CallingConvention.Cdecl)]
        public static extern Surface* CreateRGBSurfaceWithFormatFrom(void* pixels, int width, int height, int depth, int pitch, uint format);

        [DllImport(LibraryName, EntryPoint = "SDL_FillRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FillRect(Surface* dst, Rect* rect, uint color);

        [DllImport(LibraryName, EntryPoint = "SDL_FillRects", CallingConvention = CallingConvention.Cdecl)]
        public static extern int FillRects(Surface* dst, Rect* rects, int count, uint color);

        [DllImport(LibraryName, EntryPoint = "SDL_FreeSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FreeSurface(Surface* surface);

        [DllImport(LibraryName, EntryPoint = "SDL_GetClipRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GetClipRect(Surface* surface, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_GetColorKey", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetColorKey(Surface* surface, uint* key);

        [DllImport(LibraryName, EntryPoint = "SDL_GetSurfaceAlphaMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSurfaceAlphaMod(Surface* surface, byte* alpha);

        [DllImport(LibraryName, EntryPoint = "SDL_GetSurfaceBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSurfaceBlendMode(Surface* surface, BlendMode* blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_GetSurfaceColorMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetSurfaceColorMod(Surface* surface, byte* r, byte* g, byte* b);

        [DllImport(LibraryName, EntryPoint = "SDL_LockSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LockSurface(Surface* surface);

        [DllImport(LibraryName, EntryPoint = "SDL_LowerBlit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LowerBlit(Surface* src, Rect* srcrect, Surface* dst, Rect* dstrect);

        [DllImport(LibraryName, EntryPoint = "SDL_LowerBlitScaled", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LowerBlitScaled(Surface* surface, Rect* srcrect, Surface* dst, Rect* dstrect);

        public static bool MUSTLOCK(Surface* s) => (s->Flags & 0x00000002) != 0;

        [DllImport(LibraryName, EntryPoint = "SDL_SetClipRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetClipRect(Surface* surface, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_SetColorKey", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetColorKey(Surface* surface, bool flag, uint key);

        [DllImport(LibraryName, EntryPoint = "SDL_SetSurfaceAlphaMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSurfaceAlphaMod(Surface* surface, byte alpha);

        [DllImport(LibraryName, EntryPoint = "SDL_SetSurfaceBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSurfaceBlendMode(Surface* surface, BlendMode blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_SetSurfaceColorMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSurfaceColorMod(Surface* surface, byte r, byte g, byte b);

        [DllImport(LibraryName, EntryPoint = "SDL_SetSurfacePalette", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSurfacePalette(Surface* surface, Palette* palette);

        [DllImport(LibraryName, EntryPoint = "SDL_SetSurfaceRLE", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetSurfaceRLE(Surface* surface, int flag);

        [DllImport(LibraryName, EntryPoint = "SDL_UnlockSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnlockSurface(Surface* surface);

        [DllImport(LibraryName, EntryPoint = "SDL_UpperBlit", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpperBlit(Surface* src, Rect* srcrect, Surface* dst, Rect* dstrect);

        [DllImport(LibraryName, EntryPoint = "SDL_UpperBlitScaled", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpperBlitScaled(Surface* src, Rect* srcrect, Surface* dst, Rect* dstrect);
    }
}
