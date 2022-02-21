using Nucleus.Gaming.Coop;
using System.Drawing;

namespace Nucleus.Gaming
{
    public static class RectangleUtil
    {
        public static Rectangle Float(float x, float y, float width, float height)
        {
            return new Rectangle((int)x, (int)y, (int)width, (int)height);
        }

        public static Rectangle Union(params UserScreen[] rects)
        {
            Rectangle r = new Rectangle();
            for (int i = 0; i < rects.Length; i++)
            {
                r = Rectangle.Union(r, rects[i].MonitorBounds);
            }
            return r;
        }

        public static Rectangle Union(params Rectangle[] rects)
        {
            Rectangle r = new Rectangle();
            for (int i = 0; i < rects.Length; i++)
            {
                r = Rectangle.Union(r, rects[i]);
            }
            return r;
        }

        public static Rectangle ScaleAndCenter(Size srcSize, Rectangle parent)
        {
            float width = srcSize.Width;
            float height = srcSize.Height; 
                                                                   
            float pwidth = parent.Width;
            float pheight = parent.Height;

            float pratio = pwidth / pheight;
            float ratio = width / height;

            if (pratio > ratio)
            {
                height = pheight;
                width = pheight * ratio;
            }
            else
            {
                width = pwidth;
                height = pwidth * (1 / ratio);
            }

            return new Rectangle(
                (int)((parent.Width / 2.0f) - (width / 2.0f)) + parent.X,
                (int)((parent.Height / 2.0f) - (height / 2.0f)) + parent.Y,
                (int)width,
                (int)height);
        }

        public static Rectangle Center(Rectangle rect, Rectangle parent)
        {
            float rectWidth = rect.Width / 2.0f;
            float rectHeight = rect.Height / 2.0f;

            float parentWidth = parent.Width / 2.0f;
            float parentHeight = parent.Height / 2.0f;

            return new Rectangle(
                (int)(parentWidth - rectWidth) + parent.X,
                (int)(parentHeight - rectHeight) + parent.Y,
                rect.Width,
                rect.Height);
        }

        public static PointF Center(SizeF rect, Rectangle parent)
        {
            float rectWidth = rect.Width / 2.0f;
            float rectHeight = rect.Height / 2.0f;

            float parentWidth = parent.Width / 2.0f;
            float parentHeight = parent.Height / 2.0f;

            return new PointF(
                (parentWidth - rectWidth) + parent.X,
                (parentHeight - rectHeight) + parent.Y);
        }

        /// <summary>
        /// Returns a value between 0 and 1 telling how much the first rectangle is inside the parent
        /// </summary>
        /// <param name="first"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static float PcInside(Rectangle first, Rectangle parent)
        {
            float pc = 0;

            if (parent.Contains(first))
            {
                return 1;
            }
            else if (parent.IntersectsWith(first))
            {
                Rectangle intersection = Rectangle.Intersect(first, parent);
                float peri = first.Width * first.Height;
                float nperi = intersection.Width * intersection.Height;

                return nperi / peri;

            }
            return pc;
        }

        /// <summary>
        /// Scales all the Rectangle parameters by the desired value
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static Rectangle Scale(Rectangle rect, float value)
        {
            return new Rectangle(
                (int)(rect.X * value),
                (int)(rect.Y * value),
                (int)(rect.Width * value),
                (int)(rect.Height * value));
        }
    }
}
