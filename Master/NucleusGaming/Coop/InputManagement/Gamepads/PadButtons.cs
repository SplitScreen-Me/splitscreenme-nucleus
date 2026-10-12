using System;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    /// <summary>
    /// Gamepad buttons by their XInput names, as a set (the values are the XInput button flags). The setup polls the
    /// buttons held on each pad (<see cref="PlayerInfo.TrackButtons"/>) to tell which ones a press pushed: the lobby
    /// joins a pad on Start only.
    /// </summary>
    [Flags]
    public enum PadButtons
    {
        None = 0,
        DPadUp = 0x0001,
        DPadDown = 0x0002,
        DPadLeft = 0x0004,
        DPadRight = 0x0008,
        Start = 0x0010,
        Back = 0x0020,
        LeftThumb = 0x0040,
        RightThumb = 0x0080,
        LeftShoulder = 0x0100,
        RightShoulder = 0x0200,
        Guide = 0x0400,
        A = 0x1000,
        B = 0x2000,
        X = 0x4000,
        Y = 0x8000,
        /// <summary>A button Nucleus can't name: DirectInput pads number their buttons freely.</summary>
        Other = 0x10000,
    }

    /// <summary>Pure rules of <see cref="PadButtons"/>.</summary>
    public static class PadButtonRules
    {
        /// <summary>Microsoft's USB vendor id.</summary>
        public const int MicrosoftVendorId = 0x045E;

        /// <summary>
        /// The DirectInput button that is Start on a pad of this vendor (decimal id, as <c>D_Joystick.VendorId</c>
        /// stores it): 7 on Microsoft pads (A, B, X, Y, LB, RB, Back, Start), else 9, where most pads put it after
        /// four face buttons, four shoulder buttons and Select (Options on PlayStation pads, + on Switch pads).
        /// </summary>
        public static int DInputStartIndex(string vendorId)
        {
            return int.TryParse(vendorId, out int vendor) && vendor == MicrosoftVendorId ? 7 : 9;
        }

        /// <summary>The buttons of a DirectInput pad: its Start (<see cref="DInputStartIndex"/>), and Other for the rest.</summary>
        public static PadButtons FromDInput(bool[] buttons, string vendorId)
        {
            PadButtons held = PadButtons.None;

            if (buttons == null)
            {
                return held;
            }

            int start = DInputStartIndex(vendorId);

            for (int i = 0; i < buttons.Length; i++)
            {
                if (buttons[i])
                {
                    held |= i == start ? PadButtons.Start : PadButtons.Other;
                }
            }

            return held;
        }

        /// <summary>The buttons that went down between two polls.</summary>
        public static PadButtons Pressed(PadButtons before, PadButtons now) => now & ~before;
    }
}
