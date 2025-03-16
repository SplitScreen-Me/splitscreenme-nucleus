using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Gaming.App.Settings
{
    public static class App_GamePadNavigation
    {
        private static string type;
        public static string Type
        {
            get => type;
            set
            {
                type = value;
                Globals.ini.IniWriteValue("XUINav", "Type", value);
            }
        }

        private static bool enabled;
        public static bool Enabled
        {
            get => enabled;
            set
            {
                enabled = value; 
                Globals.ini.IniWriteValue("XUINav", "Enabled", value.ToString());
            }
        }

        private static int deadzone;
        public static int Deadzone
        {
            get => deadzone;
            set
            {
                deadzone = value; 
                Globals.ini.IniWriteValue("XUINav", "Deadzone", value.ToString());
            }
        }

        private static int dragDrop;
        public static int DragDrop
        {
            get => dragDrop;
            set
            {
                dragDrop = value;
                Globals.ini.IniWriteValue("XUINav", "DragDrop", value.ToString());
            }
        }

        private static int rightClick;
        public static int RightClick
        {
            get => rightClick;
            set
            {
                rightClick = value;
                Globals.ini.IniWriteValue("XUINav", "RightClick", value.ToString());
            }
        }

        private static int leftClick;
        public static int LeftClick
        {
            get => leftClick;
            set
            {
                leftClick = value; Globals.ini.IniWriteValue("XUINav", "LeftClick", value.ToString());
            }
        }

        private static int[] togglekUINavigation;
        public static int[] TogglekUINavigation
        {
            get => togglekUINavigation;
            set
            {
                togglekUINavigation = value;
                Globals.ini.IniWriteValue("XUINav", "LockUIControl", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] openOsk;
        public static int[] OpenOsk
        {
            get => openOsk;
            set
            {
                openOsk = value;
                Globals.ini.IniWriteValue("XUINav", "OpenOsk", $"{value[0]}+{value[1]}");
            }
        }

        public static bool LoadSettings()
        {
            type = Globals.ini.IniReadValue("XUINav", "Type");
            enabled = bool.Parse(Globals.ini.IniReadValue("XUINav", "Enabled"));
            deadzone = int.Parse(Globals.ini.IniReadValue("XUINav", "Deadzone"));
            dragDrop = int.Parse(Globals.ini.IniReadValue("XUINav", "DragDrop"));
            rightClick = int.Parse(Globals.ini.IniReadValue("XUINav", "RightClick"));
            leftClick = int.Parse(Globals.ini.IniReadValue("XUINav", "LeftClick"));
            togglekUINavigation = new int[]{ int.Parse(Globals.ini.IniReadValue("XUINav", "LockUIControl").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XUINav", "LockUIControl").Split('+')[1])};
            openOsk = new int[] { int.Parse(Globals.ini.IniReadValue("XUINav", "OpenOsk").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XUINav", "OpenOsk").Split('+')[1])};

            return true;
        }
    }
}
