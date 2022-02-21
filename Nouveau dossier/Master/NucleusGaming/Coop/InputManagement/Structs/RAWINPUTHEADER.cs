using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.InputManagement.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RAWINPUTHEADER
    {
        public uint dwType;
        public uint dwSize;
        public IntPtr hDevice;
        public IntPtr wParam;
    }
}
