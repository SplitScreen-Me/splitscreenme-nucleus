namespace Nucleus.Gaming
{
    public enum SetWindowPosHook
    {
        True = 1,
        DontResize,
        DontReposition
    }

   

    public enum MoveWindowHook
    {
        True = 1,
        DontResize,
        DontReposition
    }

    public enum DrawFakeCursor
    {
        True = 1,
        Fix
    }

    public enum PutMouseInsideWindow
    {
        True = 1,
        IgnoreTopLeft,
        IgnoreBottomRight
    }

    public enum SetRemoveBorderHook
    {
        True = 1,
        DontWait
    }

    public enum Key
    {

        Back = 8,
        Tab = 9,

        Clear = 12,
        Return = 13,

        Shift = 16,
        Control = 17,
        Menu = 18,
        Pause = 19,
        Capital = 20,

        Escape = 27,

        Space = 32,
        Prior = 33,
        Next = 34,
        End = 35,
        Home = 36,
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        Select = 41,
        Print = 42,
        Execute = 43,
        Snapshot = 44,
        Insert = 45,
        Delete = 46,
        Help = 47,

        N0 = 48,
        N1 = 49,
        N2 = 50,
        N3 = 51,
        N4 = 52,
        N5 = 53,
        N6 = 54,
        N7 = 55,
        N8 = 56,
        N9 = 57,

        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,

        LWin = 91,
        RWin = 92,
        Apps = 93,

        Sleep = 95,

        NumPad0 = 96,
        NumPad1 = 97,
        NumPad2 = 98,
        NumPad3 = 99,
        NumPad4 = 100,
        NumPad5 = 101,
        NumPad6 = 102,
        NumPad7 = 103,
        NumPad8 = 104,
        NumPad9 = 105,

        Multiply = 106,
        Add = 107,
        Separator = 108,
        Subtract = 109,
        Decimal = 110,
        Divide = 111,

        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123,
        F13 = 124,
        F14 = 125,
        F15 = 126,
        F16 = 127,
        F17 = 128,
        F18 = 129,
        F19 = 130,
        F20 = 131,
        F21 = 132,
        F22 = 133,
        F23 = 134,
        F24 = 135,

        NumLock = 144,
        Scroll = 145,

        LShift = 160,
        RShift = 161,
        LControl = 162,
        RControl = 163,
        LMenu = 164,
        RMenu = 165,
    }
}
