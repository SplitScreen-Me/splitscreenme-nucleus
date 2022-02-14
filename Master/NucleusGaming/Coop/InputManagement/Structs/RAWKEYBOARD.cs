using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.InputManagement.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct RAWKEYBOARD
    {
        public ushort MakeCode;
        public ushort Flags;
        public ushort Reserved;

        /// <summary>
        /// https://docs.microsoft.com/en-gb/windows/desktop/inputdev/virtual-key-codes
        /// </summary>
        public ushort VKey;

        public uint Message;
        public ulong ExtraInformation;
    }
}
