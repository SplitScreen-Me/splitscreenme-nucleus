using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SDL
{
    public static partial class SDL2
    {
        private const string LibraryName = "SDL2.dll";

#if NETCOREAPP3_0
        private static IntPtr _handle;

        static SDL()
        {
            NativeLibrary.SetDllImportResolver(typeof(SDL).Assembly, ResolveDllImport);
        }

        private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            if (_handle == IntPtr.Zero)
            {
                foreach (var path in GetSearchPaths(assembly))
                {
                    if (path != null && NativeLibrary.TryLoad(path, out _handle))
                    {
#if DEBUG
                        Console.WriteLine($"Loading SDL2 from: {path}");
#endif
                        return _handle;
                    }
                }
            }

            return _handle;
        }

        private static IEnumerable<string> GetSearchPaths(Assembly assembly)
        {
            // Try loading from environment variable, if set
            yield return Environment.GetEnvironmentVariable("SHARPSDL_SDL2");

            var libName = GetNativeLibraryName();

            // Try loading from runtimes/<rid>/native/<lib-name>
            yield return Path.Combine(
                Path.GetDirectoryName(assembly.Location),
                "runtimes",
                GetRuntimeIdentifier(),
                "native",
                libName);

            // Finally, just try the name of the library
            yield return libName;
        }

        private static string GetRuntimeIdentifier()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return "win-x64";
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                {
                    return "win-x86";
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (RuntimeInformation.ProcessArchitecture == Architecture.X64)
                {
                    return "linux-x64";
                }

                if (RuntimeInformation.ProcessArchitecture == Architecture.X86)
                {
                    return "linux-x86";
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "osx-x64";
            }

            throw new PlatformNotSupportedException();
        }

        private static string GetNativeLibraryName()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "SDL2.dll";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "libSDL2-2.0.so.0";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                return "libSDL2.dylib";
            }

            throw new PlatformNotSupportedException();
        }
#endif
    }
}
