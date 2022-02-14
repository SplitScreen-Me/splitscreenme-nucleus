//********************************************************************************************
//Author: Sergey Stoyan, CliverSoft Co.
//        stoyan@cliversoft.com
//        sergey.stoyan@gmail.com
//        http://www.cliversoft.com
//        07 September 2006
//Copyright: (C) 2006, Sergey Stoyan
//********************************************************************************************
using System;
using System.Collections;
//using System.Messaging;
using System.Text;

namespace CliverSoft
{
    /// <summary>
    /// contains collected window info
    /// </summary>
	public struct Window
    {
        public string path;
        public string text;
        public string internal_text;
        public string class_name;
        public int level;
        public int parent_id;
    }


    public class EnumWindows
    {
        public ArrayList Windows = new ArrayList();

        /// <summary>
        /// enum all windows including child windows
        /// </summary>
        /// <param name="print_to_log"></param>
		public EnumWindows(bool print_to_log)
        {
            //Print2Log = print_to_log;
            Windows.Clear();
            Win32.Functions.EnumWindows(new Win32.Functions.EnumProc(enum_window_call_back), 0);
            //			Message.Inform("Completed");
        }

        /// <summary>
        /// invoke building child window tree for give parent window
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="lValue"></param>
        /// <returns></returns>
        private bool enum_window_call_back(IntPtr hwnd, int lValue)
        {
            Window w = new Window
            {
                level = 0,
                parent_id = -1
            };

            StringBuilder s = new StringBuilder(255); ;

            Win32.Functions.GetClassName(hwnd, s, 255);
            w.class_name = s.ToString();
            Win32.Functions.GetWindowText(hwnd, s, 255);
            w.text = s.ToString();
            Win32.Functions.InternalGetWindowText(hwnd, s, 255);
            w.internal_text = s.ToString();
            w.path = "[" + w.text + "]";

            Windows.Add(w);

            //Message.Write2Log(w.level + " " + w.path + " - " + w.class_name);

            Win32.Functions.EnumChildWindows(hwnd, new Win32.Functions.EnumProc(enum_child_window_call_back), Windows.Count - 1);

            return true;
        }

        /// <summary>
        /// build child window tree recurcively
        /// </summary>
        /// <param name="hwnd"></param>
        /// <param name="parent_id"></param>
        /// <returns></returns>
        private bool enum_child_window_call_back(IntPtr hwnd, int parent_id)
        {
            Window w = new Window
            {
                level = ((Window)Windows[parent_id]).level + 1,
                parent_id = parent_id
            };

            StringBuilder s = new StringBuilder(255);

            Win32.Functions.GetClassName(hwnd, s, 255);
            w.class_name = s.ToString();
            Win32.Functions.GetWindowText(hwnd, s, 255);
            w.text = s.ToString();
            Win32.Functions.InternalGetWindowText(hwnd, s, 255);
            w.internal_text = s.ToString();
            w.path = ((Window)Windows[parent_id]).path + "[" + w.text + "]";

            Windows.Add(w);

            //Message.Write2Log(w.level + " " + w.path + " - " + w.class_name);

            Win32.Functions.EnumChildWindows(hwnd, new Win32.Functions.EnumProc(enum_child_window_call_back), Windows.Count - 1);

            return true;
        }
    }
}
