using System;
using System.Drawing;

namespace Nucleus
{
    public class Display
    {
        public Rectangle Bounds => bounds;
        public string DeviceName => deviceName;
        public bool Primary => primary;
        public IntPtr Handle => ptr;
        public string DeviceID => deviceID;
        public string DeviceString => deviceString;
        public string MonitorID => monitorID;
        public int DisplayIndex => displayIndex;
        public int MonitorIndex => monitorIndex;

        private Rectangle bounds;
        private string deviceName;
        private string deviceID;
        private string deviceString;
        private string monitorID;
        private int displayIndex;
        private int monitorIndex;
        private bool primary;
        private IntPtr ptr;

        public Display(IntPtr pointer, Rectangle size, string device, bool isPrimary, string devID, string devStr, string monID, int disIndex, int monIndex)
        {
            ptr = pointer;
            bounds = size;
            deviceName = device;
            primary = isPrimary;
            deviceID = devID;
            deviceString = devStr;
            monitorID = monID;
            displayIndex = disIndex;
            monitorIndex = monIndex;
        }

    }
}
