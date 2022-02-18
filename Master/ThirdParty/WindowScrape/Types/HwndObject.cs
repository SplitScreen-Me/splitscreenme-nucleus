using System;
using System.Collections.Generic;
using System.Drawing;
using WindowScrape.Constants;
using WindowScrape.Static;
using Point = System.Drawing.Point;

namespace WindowScrape.Types
{
    /// <summary>
    /// Allows the searching, navigation, and manipulation of Hwnd objects.
    /// </summary>
    public class HwndObject
    {
        /// <summary>
        /// The windows handle to this object.
        /// </summary>
        public IntPtr NativePtr { get; private set; }

        /// <summary>
        /// The registered class name (if any) of this object.
        /// </summary>
        public string ClassName => HwndInterface.GetHwndClassName(NativePtr);

        /// <summary>
        /// The title of this object - Setting this will only effect window title-bar text.
        /// </summary>
        public string Title
        {
            get => HwndInterface.GetHwndTitle(NativePtr);
            set => HwndInterface.SetHwndTitle(NativePtr, value);
        }

        /// <summary>
        /// The text of this item - setting this will only effect controls and only with appropriate access/privacy
        /// </summary>
        public string Text
        {
            get => HwndInterface.GetHwndText(NativePtr);
            set => HwndInterface.SetHwndText(NativePtr, value);
        }

        /// <summary>
        /// The location of this Hwnd Object.
        /// </summary>
        public Point Location
        {
            get => HwndInterface.GetHwndPos(NativePtr);
            set
            {
                if (TopMost)
                {
                    HwndInterface.SetHwndPosTopMost(NativePtr, value.X, value.Y);
                }
                else
                {
                    HwndInterface.SetHwndPos(NativePtr, value.X, value.Y);
                }
            }
        }

        private bool isTopMost;

        public bool TopMost
        {
            get => isTopMost;
            set
            {
                isTopMost = value;
                if (value)
                {
                    HwndInterface.MakeTopMost(NativePtr);
                }
                else
                {
                    HwndInterface.MakeNoTopMost(NativePtr);
                }
            }
        }

        /// <summary>
        /// The size of this Hwnd Object.
        /// </summary>
        public Size Size
        {
            get => HwndInterface.GetHwndSize(NativePtr);
            set
            {
                if (TopMost)
                {
                    HwndInterface.SetHwndSizeTopMost(NativePtr, value.Width, value.Height);
                }
                else
                {
                    HwndInterface.SetHwndSize(NativePtr, value.Width, value.Height);
                }
            }
        }

        /// <summary>
        /// Retrieves all top-level Hwnd Objects.
        /// </summary>
        /// <returns></returns>
        public static List<HwndObject> GetWindows()
        {
            List<HwndObject> result = new List<HwndObject>();
            foreach (IntPtr hwnd in HwndInterface.EnumHwnds())
            {
                result.Add(new HwndObject(hwnd));
            }

            return result;
        }

        /// <summary>
        /// Gets the first top-level HwndObject with the given title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static HwndObject GetWindowByTitle(string title)
        {
            return new HwndObject(HwndInterface.GetHwndFromTitle(title));
        }

        /// <summary>
        /// Initialized a new HwndObject.
        /// </summary>
        /// <param name="hwnd"></param>
        public HwndObject(IntPtr hwnd)
        {
            NativePtr = hwnd;
        }

        /// <summary>
        /// Sends a message to this Hwnd Object
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        public void SendMessage(WM msg, uint param1, string param2)
        {
            HwndInterface.SendMessage(NativePtr, msg, param1, param2);
        }

        /// <summary>
        /// Sends a message to this Hwnd Object
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="param1"></param>
        /// <param name="param2"></param>
        public void SendMessage(WM msg, uint param1, uint param2)
        {
            HwndInterface.SendMessage(NativePtr, msg, param1, param2);
        }

        /// <summary>
        /// Returns a string result from a message.
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="param"></param>
        /// <returns></returns>
        public string GetMessageString(WM msg, uint param)
        {
            return HwndInterface.GetMessageString(NativePtr, msg, param);
        }
        /// <summary>
        /// Returns an integer result from a message.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        public int GetMessageInt(WM msg)
        {
            return HwndInterface.GetMessageInt(NativePtr, msg);
        }

        /// <summary>
        /// Simulates a user-click on this object.
        /// </summary>
        public void Click()
        {
            HwndInterface.ClickHwnd(NativePtr);
        }

        /// <summary>
        /// Seeks a parent for this Hwnd Object (if any).
        /// </summary>
        /// <returns></returns>
        public HwndObject GetParent()
        {
            return new HwndObject(HwndInterface.GetHwndParent(NativePtr));
        }

        /// <summary>
        /// Seeks all children of this Hwnd Object.
        /// </summary>
        /// <returns></returns>
        public List<HwndObject> GetChildren()
        {
            List<HwndObject> result = new List<HwndObject>();
            foreach (IntPtr hwnd in HwndInterface.EnumChildren(NativePtr))
            {
                result.Add(new HwndObject(hwnd));
            }

            return result;
        }

        /// <summary>
        /// Retrieves a child Hwnd Object by its class and title.
        /// </summary>
        /// <param name="cls"></param>
        /// <param name="title"></param>
        /// <returns></returns>
        public HwndObject GetChild(string cls, string title)
        {
            IntPtr hwnd = HwndInterface.GetHwndChild(NativePtr, cls, title);
            return new HwndObject(hwnd);
        }

        public bool MoveWindow(Point location, Size size)
        {
            return HwndInterface.MoveWindow(NativePtr, location.X, location.Y, size.Width, size.Height);
        }

        public override string ToString()
        {
            Point pt = Location;
            Size sz = Size;
            string result =
                string.Format(
                    "({0}) {1},{2}:{3}x{4} \"{5}\"",
                    NativePtr,
                    pt.X, pt.Y,
                    sz.Width, sz.Height,
                    Title);
            return result;
        }

        #region Operators
        //public static bool operator ==(HwndObject a, HwndObject b)
        //{
        //    return a == null || b == null || (a.Hwnd == b.Hwnd);
        //}

        //public static bool operator !=(HwndObject a, HwndObject b)
        //{
        //    return !(a == b);
        //}

        //public bool Equals(HwndObject obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    return obj.Hwnd.Equals(Hwnd);
        //}

        //public override bool Equals(object obj)
        //{
        //    if (ReferenceEquals(null, obj)) return false;
        //    if (ReferenceEquals(this, obj)) return true;
        //    if (obj.GetType() != typeof(HwndObject)) return false;
        //    return Equals((HwndObject)obj);
        //}

        //public override int GetHashCode()
        //{
        //    return Hwnd.GetHashCode();
        //}
        #endregion
    }
}
