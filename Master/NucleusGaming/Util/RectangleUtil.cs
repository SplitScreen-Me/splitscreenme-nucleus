using Nucleus.Gaming.Coop;
using System.Collections.Generic;
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

        public static RectangleF Union(params RectangleF[] rects)
        {
            RectangleF r = new RectangleF();
            for (int i = 0; i < rects.Length; i++)
            {
                r = RectangleF.Union(r, rects[i]);
            }
            return r;
        }

        public static RectangleF ScaleAndCenter(SizeF srcSize, Rectangle parent)
        {
            float width = srcSize.Width;
            float height = srcSize.Height;

            float pwidth = (float)parent.Width;
            float pheight = (float)parent.Height;

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
                height = pwidth * ratio;
            }

            return new RectangleF(
             ((float)parent.Width / 2.0f) - (width / 2.0f) + (float)parent.X,
            ((float)parent.Height / 2.0f) - (height / 2.0f) + (float)parent.Y,
              width,
             height);
        }

        public static RectangleF ScaleAndCenter(SizeF srcSize, RectangleF parent)
        {
            float width = srcSize.Width;
            float height = srcSize.Height;

            float pwidth = (float)parent.Width;
            float pheight = (float)parent.Height;

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
                height = pwidth * ratio;
            }

            return new RectangleF(
             ((float)parent.Width / 2.0f) - (width / 2.0f) + (float)parent.X,
            ((float)parent.Height / 2.0f) - (height / 2.0f) + (float)parent.Y,
              width,
             height);
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

        public static PointF Center(SizeF rect, RectangleF parent)
        {
            float rectWidth = rect.Width / 2.0f;
            float rectHeight = rect.Height / 2.0f;

            float parentWidth = parent.Width / 2.0f;
            float parentHeight = parent.Height / 2.0f;

            return new PointF(
                (parentWidth - rectWidth) + parent.X,
                (parentHeight - rectHeight) + parent.Y);
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
        public static float PcInside(RectangleF first, RectangleF parent)
        {
            float pc = 0;

            if (parent.Contains(first))
            {
                return 1;
            }
            else if (parent.IntersectsWith(first))
            {
                RectangleF intersection = RectangleF.Intersect(first, parent);
                float peri = first.Width * first.Height;
                float nperi = intersection.Width * intersection.Height;

                return nperi / peri;

            }
            return pc;
        }


        public  static List<RectangleF> SubtractSurfaceF(RectangleF a, RectangleF b)
        {
            List<RectangleF> result = new List<RectangleF>();

            RectangleF intersect = RectangleF.Intersect(a, b);
            if (intersect.IsEmpty) return new List<RectangleF> { a };

            // Top
            if (intersect.Top > a.Top)
                result.Add(new RectangleF(a.Left, a.Top, a.Width, intersect.Top - a.Top));

            // Bottom
            if (intersect.Bottom < a.Bottom)
                result.Add(new RectangleF(a.Left, intersect.Bottom, a.Width, a.Bottom - intersect.Bottom));

            // Left
            if (intersect.Left > a.Left)
                result.Add(new RectangleF(a.Left, intersect.Top, intersect.Left - a.Left, intersect.Height));

            // Right
            if (intersect.Right < a.Right)
                result.Add(new RectangleF(intersect.Right, intersect.Top, a.Right - intersect.Right, intersect.Height));

            return result;
        }


        public static List<Rectangle> SubtractSurface(Rectangle a, Rectangle b)
        {
            List<Rectangle> result = new List<Rectangle>();

            Rectangle intersect = Rectangle.Intersect(a, b);
            if (intersect.IsEmpty) return new List<Rectangle> { a };

            // Top
            if (intersect.Top > a.Top)
                result.Add(new Rectangle(a.Left, a.Top, a.Width, intersect.Top - a.Top));

            // Bottom
            if (intersect.Bottom < a.Bottom)
                result.Add(new Rectangle(a.Left, intersect.Bottom, a.Width, a.Bottom - intersect.Bottom));

            // Left
            if (intersect.Left > a.Left)
                result.Add(new Rectangle(a.Left, intersect.Top, intersect.Left - a.Left, intersect.Height));

            // Right
            if (intersect.Right < a.Right)
                result.Add(new Rectangle(intersect.Right, intersect.Top, a.Right - intersect.Right, intersect.Height));

            return result;
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

        public static RectangleF Scale(RectangleF rect, float value)
        {
            return new RectangleF(
                ((float)rect.X * value),
                ((float)rect.Y * value),
                ((float)rect.Width * value),
                ((float)rect.Height * value));
        }
    }
}
