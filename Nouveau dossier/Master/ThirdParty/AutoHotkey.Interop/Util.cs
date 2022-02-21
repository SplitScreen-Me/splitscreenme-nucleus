using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AutoHotkey.Interop
{
    internal static class Util
    {
        public static string FindEmbededResourceName(Assembly assembly, string path)
        {
            path = Regex.Replace(path, @"[/\\]", ".");

            if (!path.StartsWith("."))
            {
                path = "." + path;
            }

            string[] names = assembly.GetManifestResourceNames();

            foreach (string name in names)
            {
                if (name.EndsWith(path))
                {
                    return name;
                }
            }

            return null;
        }

        public static void ExtractEmbededResourceToFile(Assembly assembly, string embededResourcePath, string targetFileName)
        {
            //ensure directory exists
            string dir = Path.GetDirectoryName(targetFileName);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            using (Stream readStream = assembly.GetManifestResourceStream(embededResourcePath))
            using (FileStream writeStream = File.Open(targetFileName, FileMode.Create))
            {
                readStream.CopyTo(writeStream);
                readStream.Flush();
            }
        }

        public static bool Is64Bit()
        {
            return IntPtr.Size == 8;
        }
        public static bool Is32Bit()
        {
            return IntPtr.Size == 4;
        }



        public static void EnsureAutoHotkeyLoaded()
        {
            if (dllHandle.IsValueCreated)
            {
                return;
            }

            SafeLibraryHandle handle = dllHandle.Value;
        }

        private static Lazy<SafeLibraryHandle> dllHandle = new Lazy<SafeLibraryHandle>(
            () => Util.LoadAutoHotKeyDll());
        private static SafeLibraryHandle LoadAutoHotKeyDll()
        {
            //Locate and Load 32bit or 64bit version of AutoHotkey.dll
            string tempFolderPath = Path.Combine(Path.GetTempPath(), "AutoHotkey.Interop");
            string path32 = @"x86\AutoHotkey.dll";
            string path64 = @"x64\AutoHotkey.dll";

            Func<string, SafeLibraryHandle> loadDllFromFileOrResource = new Func<string, SafeLibraryHandle>(relativePath =>
            {
                if (File.Exists(relativePath))
                {
                    return SafeLibraryHandle.LoadLibrary(relativePath);
                }
                else
                {
                    Assembly assembly = typeof(AutoHotkeyEngine).Assembly;
                    string resource = Util.FindEmbededResourceName(assembly, relativePath);

                    if (resource != null)
                    {
                        string target = Path.Combine(tempFolderPath, relativePath);
                        Util.ExtractEmbededResourceToFile(assembly, resource, target);
                        return SafeLibraryHandle.LoadLibrary(target);
                    }

                    return null;
                }
            });


            if (Util.Is32Bit())
            {
                return loadDllFromFileOrResource(path32);
            }
            else if (Util.Is64Bit())
            {
                return loadDllFromFileOrResource(path64);
            }
            else
            {
                return null;
            }
        }
    }
}
