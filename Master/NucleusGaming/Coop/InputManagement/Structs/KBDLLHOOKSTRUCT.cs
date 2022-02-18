using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.InputManagement.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class KBDLLHOOKSTRUCT
    {
        public int vkCode;
        public int scanCode;
        public int flags;
        public uint time;
        public UIntPtr dwExtraInfo;
    }
}
