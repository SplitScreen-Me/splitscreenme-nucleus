using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.InputManagement.Structs
{
    [StructLayout(LayoutKind.Explicit)]
    public struct RAWMOUSE
    {
        [FieldOffset(0)]
        public uint usFlags;

        [FieldOffset(4)]
        public ushort usButtonFlags;

        [FieldOffset(6)]
        public ushort usButtonData;

        [FieldOffset(8)]
        public uint ulRawButtons;

        [FieldOffset(12)]
        public int lLastX;

        [FieldOffset(16)]
        public int lLastY;

        [FieldOffset(20)]
        public uint ulExtraInformation;
    }
}
