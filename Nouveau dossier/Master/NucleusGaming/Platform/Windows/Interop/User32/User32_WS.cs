namespace Nucleus.Interop.User32
{
    public static class User32_WS
    {
        /*GWL_ID = (-12)
        GWL_STYLE = (-16)
        GWL_EXSTYLE = (-20)*/

        public const int GWL_EXSTYLE = -20;
        public const int GWLP_HINSTANCE = -6;
        public const int GWLP_HWNDPARENT = -8;
        public const int GWL_ID = -12;
        public const int GWL_STYLE = -16;
        public const int GWL_USERDATA = -21;
        public const int GWL_WNDPROC = -4;
        public const int DWLP_USER = 0x8;
        public const int DWLP_MSGRESULT = 0x0;
        public const int DWLP_DLGPROC = 0x4;

        public const int WM_COMMAND = 0x111;
        public const int MIN_ALL = 419;
        public const int MIN_ALL_UNDO = 416;

        //laziness, don't want to search for the ones I don't use XD
        // Window Styles 
        public const uint WS_OVERLAPPED = 0;
        public const uint WS_POPUP = 0x80000000;
        public const uint WS_CHILD = 0x40000000;
        public const uint WS_MINIMIZE = 0x20000000;
        public const uint WS_VISIBLE = 0x10000000;
        public const uint WS_DISABLED = 0x8000000;
        public const uint WS_CLIPSIBLINGS = 0x4000000;
        public const uint WS_CLIPCHILDREN = 0x2000000;
        public const uint WS_MAXIMIZE = 0x1000000;
        public const uint WS_CAPTION = 0xC00000;      // WS_BORDER or WS_DLGFRAME  
        public const uint WS_BORDER = 0x800000;
        public const uint WS_DLGFRAME = 0x400000;
        public const uint WS_VSCROLL = 0x200000;
        public const uint WS_HSCROLL = 0x100000;
        public const uint WS_SYSMENU = 0x80000;
        public const uint WS_THICKFRAME = 0x40000;
        public const uint WS_GROUP = 0x20000;
        public const uint WS_TABSTOP = 0x10000;
        public const uint WS_MINIMIZEBOX = 0x20000;
        public const uint WS_MAXIMIZEBOX = 0x10000;
        public const uint WS_TILED = WS_OVERLAPPED;
        public const uint WS_ICONIC = WS_MINIMIZE;
        public const uint WS_SIZEBOX = WS_THICKFRAME;

        // Extended Window Styles 
        public const uint WS_EX_DLGMODALFRAME = 0x0001;
        public const uint WS_EX_NOPARENTNOTIFY = 0x0004;
        public const uint WS_EX_TOPMOST = 0x0008;
        public const uint WS_EX_ACCEPTFILES = 0x0010;
        public const uint WS_EX_TRANSPARENT = 0x0020;
        public const uint WS_EX_MDICHILD = 0x0040;
        public const uint WS_EX_TOOLWINDOW = 0x0080;
        public const uint WS_EX_WINDOWEDGE = 0x0100;
        public const uint WS_EX_CLIENTEDGE = 0x0200;
        public const uint WS_EX_CONTEXTHELP = 0x0400;
        public const uint WS_EX_RIGHT = 0x1000;
        public const uint WS_EX_LEFT = 0x0000;
        public const uint WS_EX_RTLREADING = 0x2000;
        public const uint WS_EX_LTRREADING = 0x0000;
        public const uint WS_EX_LEFTSCROLLBAR = 0x4000;
        public const uint WS_EX_RIGHTSCROLLBAR = 0x0000;
        public const uint WS_EX_CONTROLPARENT = 0x10000;
        public const uint WS_EX_STATICEDGE = 0x20000;
        public const uint WS_EX_APPWINDOW = 0x40000;
        public const uint WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE);
        public const uint WS_EX_PALETTEWINDOW = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST);
        public const uint WS_EX_LAYERED = 0x00080000;
        public const uint WS_EX_NOINHERITLAYOUT = 0x00100000; // Disable inheritence of mirroring by children
        public const uint WS_EX_LAYOUTRTL = 0x00400000; // Right to left mirroring
        public const uint WS_EX_COMPOSITED = 0x02000000;
        public const uint WS_EX_NOACTIVATE = 0x08000000;
    }
}
