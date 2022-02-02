using System;

namespace Nucleus.Gaming.Coop.InputManagement.Enums
{
    [Flags]
    public enum MessageTypes : ushort
    {
        WM_SETFOCUS = 0x0007,
        WM_ACTIVATE = 0x0006,
        WM_ACTIVATEAPP = 0x001C,
        WM_NCACTIVATE = 0x0086,
        WM_CAPTURECHANGED = 0x0215,
        WM_MOUSEACTIVATE = 0x0021,
        WM_KEYDOWN = 0x0100,
        WM_KEYUP = 0x0101,
        WM_INPUT = 0x00FF
    }
}
