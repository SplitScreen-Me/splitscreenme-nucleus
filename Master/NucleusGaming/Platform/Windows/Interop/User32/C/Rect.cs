using System.Drawing;

namespace Nucleus.Interop.User32
{
    public struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public Rectangle ToRectangle()
        {
            return new Rectangle(Left, Top, Right - Left, Bottom - Top);
        }
    }
}
