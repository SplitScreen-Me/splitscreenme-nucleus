using System;
using System.Runtime.InteropServices;

namespace SDL
{
    [Flags]
    public enum RendererFlags : uint
    {
        Software = 0x00000001,
        Accelerated = 0x00000002,
        PresentVsync = 0x00000004,
        TargetTexture = 0x00000008
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct RendererInfo
    {
        public byte* Name;
        public RendererFlags Flags;
        public uint NumTextureFormats;
        public fixed uint TextureFormats[16];
        public int MaxTextureWidth;
        public int MaxTextureHeight;
    }

    public enum TextureAccess
    {
        Static,
        Streaming,
        Target
    }

    [Flags]
    public enum TextureModulate : uint
    {
        None = 0x00000000,
        Color = 0x00000001,
        Alpha = 0x00000002
    }

    [Flags]
    public enum RendererFlip : uint
    {
        None = 0x00000000,
        Horizontal = 0x00000001,
        Vertical = 0x00000002
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Renderer
    {
        private readonly IntPtr ptr;

        public Renderer(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(Renderer renderer)
        {
            return renderer.ptr;
        }

        public static implicit operator Renderer(IntPtr ptr)
        {
            return new Renderer(ptr);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Texture
    {
        private readonly IntPtr ptr;

        public Texture(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(Texture texture)
        {
            return texture.ptr;
        }

        public static implicit operator Texture(IntPtr ptr)
        {
            return new Texture(ptr);
        }
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_CreateRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Renderer CreateRenderer(Window window, int index, RendererFlags flags);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateSoftwareRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Renderer CreateSoftwareRenderer(Surface* surface);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern Texture CreateTexture(Renderer renderer, uint format, TextureAccess access, int w, int h);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateTextureFromSurface", CallingConvention = CallingConvention.Cdecl)]
        public static extern Texture CreateTextureFromSurface(Renderer renderer, Surface* surface);

        [DllImport(LibraryName, EntryPoint = "SDL_CreateWindowAndRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern int CreateWindowAndRenderer(int width, int height, WindowFlags window_flags, Window* window, Renderer* renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_DestroyRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyRenderer(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_DestroyTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DestroyTexture(Texture texture);

        [DllImport(LibraryName, EntryPoint = "SDL_GetNumRenderDrivers", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetNumRenderDrivers();

        [DllImport(LibraryName, EntryPoint = "SDL_GetRenderDrawBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRenderDrawBlendMode(Renderer renderer, BlendMode* blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRenderDrawColor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRenderDrawColor(Renderer renderer, byte* r, byte* g, byte* b, byte* a);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRenderDriverInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRenderDriverInfo(int index, RendererInfo* info);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRenderTarget", CallingConvention = CallingConvention.Cdecl)]
        public static extern Texture GetRenderTarget(Renderer* renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRenderer", CallingConvention = CallingConvention.Cdecl)]
        public static extern Renderer* GetRenderer(Window window);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRendererInfo", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRendererInfo(Renderer renderer, RendererInfo* info);

        [DllImport(LibraryName, EntryPoint = "SDL_GetRendererOutputSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetRendererOutputSize(Renderer renderer, int* w, int* h);

        [DllImport(LibraryName, EntryPoint = "SDL_GetTextureAlphaMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextureAlphaMod(Texture texture, byte* alpha);

        [DllImport(LibraryName, EntryPoint = "SDL_GetTextureBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextureBlendMode(Texture texture, BlendMode* blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_GetTextureColorMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GetTextureColorMod(Texture texture, byte* r, byte* g, byte* b);

        [DllImport(LibraryName, EntryPoint = "SDL_LockTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int LockTexture(Texture texture, Rect* rect, void** pixels, int* pitch);

        [DllImport(LibraryName, EntryPoint = "SDL_QueryTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int QueryTexture(Texture texture, uint* format, TextureAccess* access, int* w, int* h);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderClear", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderClear(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderCopy", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderCopy(Renderer renderer, Texture texture, Rect* srcrect, Rect* dstrect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderCopyEx", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderCopyEx(Renderer renderer, Texture texture, Rect* srcrect, Rect* dstrect, double angle, Point* center, RendererFlip flip);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawLine", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawLine(Renderer renderer, int x1, int y1, int x2, int y2);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawLines", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawLines(Renderer renderer, Point* points, int count);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawPoint", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawPoint(Renderer renderer, int x, int y);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawPoints", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawPoints(Renderer renderer, Point* points, int count);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawRect(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderDrawRects", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderDrawRects(Renderer renderer, Rect* rects, int count);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderFillRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderFillRect(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderFillRects", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderFillRects(Renderer renderer, Rect* rects, int count);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderGetClipRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderGetClipRect(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderGetIntegerScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RenderGetIntegerScale(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderGetLogicalSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderGetLogicalSize(Renderer renderer, int* w, int* h);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderGetScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderGetScale(Renderer renderer, float* scaleX, float* scaleY);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderGetViewport", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderGetViewport(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderIsClipEnabled", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RenderIsClipEnabled(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderPresent", CallingConvention = CallingConvention.Cdecl)]
        public static extern void RenderPresent(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderReadPixels", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderReadPixels(Renderer renderer, Rect* rect, uint format, void* pixels, int pitch);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderSetClipRect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderSetClipRect(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderSetIntegerScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderSetIntegerScale(Renderer renderer, bool enable);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderSetLogicalSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderSetLogicalSize(Renderer renderer, int w, int h);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderSetScale", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderSetScale(Renderer renderer, float scaleX, float scaleY);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderSetViewport", CallingConvention = CallingConvention.Cdecl)]
        public static extern int RenderSetViewport(Renderer renderer, Rect* rect);

        [DllImport(LibraryName, EntryPoint = "SDL_RenderTargetSupported", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool RenderTargetSupported(Renderer renderer);

        [DllImport(LibraryName, EntryPoint = "SDL_SetRenderDrawBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetRenderDrawBlendMode(Renderer renderer, BlendMode blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_SetRenderDrawColor", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetRenderDrawColor(Renderer renderer, byte r, byte g, byte b, byte a);

        [DllImport(LibraryName, EntryPoint = "SDL_SetRenderTarget", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetRenderTarget(Renderer renderer, Texture texture);

        [DllImport(LibraryName, EntryPoint = "SDL_SetTextureAlphaMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetTextureAlphaMod(Texture texture, byte alpha);

        [DllImport(LibraryName, EntryPoint = "SDL_SetTextureBlendMode", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetTextureBlendMode(Texture texture, BlendMode blendMode);

        [DllImport(LibraryName, EntryPoint = "SDL_SetTextureColorMod", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SetTextureColorMod(Texture texture, byte r, byte g, byte b);

        [DllImport(LibraryName, EntryPoint = "SDL_UnlockTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern void UnlockTexture(Texture texture);

        [DllImport(LibraryName, EntryPoint = "SDL_UpdateTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpdateTexture(Texture texture, Rect* rect, void* pixels, int pitch);

        [DllImport(LibraryName, EntryPoint = "SDL_UpdateYUVTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int UpdateYUVTexture(Texture texture, Rect* rect, byte* Yplane, int Ypitch, byte* Uplane, int Upitch, byte* Vplane, int Vpitch);
    }
}
