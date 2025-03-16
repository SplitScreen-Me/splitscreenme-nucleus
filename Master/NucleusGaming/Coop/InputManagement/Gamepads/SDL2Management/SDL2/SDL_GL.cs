using System;
using System.Runtime.InteropServices;

namespace SDL
{
    [StructLayout(LayoutKind.Sequential)]
    public struct GLContext
    {
        private readonly IntPtr ptr;

        public GLContext(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(GLContext context)
        {
            return context.ptr;
        }

        public static implicit operator GLContext(IntPtr ptr)
        {
            return new GLContext(ptr);
        }
    }

    public enum GLAttr
    {
        RedSize,
        GreenSize,
        BlueSize,
        AlphaSize,
        BufferSize,
        DoubleBuffer,
        DepthSize,
        StencilSize,
        AccumRedSize,
        AccumGreenSize,
        AccumBlueSize,
        AccumAlphaSize,
        Stereo,
        MultiSampleBuffers,
        MultiSampleSamples,
        AcceleratedVisual,
        RetainedBacking,
        ContextMajorVersion,
        ContextMinorVersion,
        ContextEGL,
        ContextFlags,
        ContextProfileMask,
        ShareWithCurrentContext,
        FramebufferSRGBCapable,
        ContextReleaseBehaviour,
        ContextResetNotification,
        ContextNoError
    }

    public static unsafe partial class SDL2
    {
        [DllImport(LibraryName, EntryPoint = "SDL_GL_BindTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_BindTexture(Texture texture, float* texw, float* texh);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_CreateContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLContext GL_CreateContext(Window window);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_DeleteContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_DeleteContext(GLContext context);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_ExtensionSupported", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern bool GL_ExtensionSupported(string extension);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetAttribute", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_GetAttribute(GLAttr attr, int* value);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetCurrentContext", CallingConvention = CallingConvention.Cdecl)]
        public static extern GLContext GL_GetCurrentContext();

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetCurrentWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern Window GL_GetCurrentWindow();

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetDrawableSize", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_GetDrawableSize(Window window, int* width, int* height);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetProcAddress", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern IntPtr GL_GetProcAddress(string proc);

        public static T GL_GetProcDelegate<T>(string proc) where T : class
        {
            return Marshal.GetDelegateForFunctionPointer<T>(GL_GetProcAddress(proc));
        }

        [DllImport(LibraryName, EntryPoint = "SDL_GL_GetSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_GetSwapInterval();

        [DllImport(LibraryName, EntryPoint = "SDL_GL_LoadLibrary", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GL_LoadLibrary(string path);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_MakeCurrent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_MakeCurrent(Window window, GLContext context);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_ResetAttributes", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_ResetAttributes();

        [DllImport(LibraryName, EntryPoint = "SDL_GL_SetAttribute", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_SetAttribute(GLAttr attr, int value);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_SetSwapInterval", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_SetSwapInterval(int interval);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_SwapWindow", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_SwapWindow(Window window);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_UnbindTexture", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GL_UnbindTexture(Texture texture);

        [DllImport(LibraryName, EntryPoint = "SDL_GL_UnloadLibrary", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GL_UnloadLibrary();
    }
}
