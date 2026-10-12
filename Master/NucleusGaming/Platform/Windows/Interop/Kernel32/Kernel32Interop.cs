using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Platform.Windows.Interop
{
    internal static class Kernel32Interop
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern bool CreateHardLink(string lpFileName, string lpExistingFileName, IntPtr lpSecurityAttributes);
    }
}