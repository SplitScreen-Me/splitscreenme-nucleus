namespace Nucleus.Gaming.App.Settings
{
    public static class App_GamePadShortcuts
    {
        private static int[] close;
        public static int[] Close
        {
            get => close;
            set
            {
                close = value; Globals.ini.IniWriteValue("XShortcuts", "Close", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] stop;
        public static int[] StopSession
        {
            get => stop;
            set
            {
                stop = value; Globals.ini.IniWriteValue("XShortcuts", "Stop", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] topMost;
        public static int[] TopMost
        {
            get => topMost;
            set
            {
                topMost = value; Globals.ini.IniWriteValue("XShortcuts", "TopMost", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] setFocus;
        public static int[] SetFocus
        {
            get => setFocus;
            set
            {
                setFocus = value; Globals.ini.IniWriteValue("XShortcuts", "SetFocus", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] resetWindows;
        public static int[] ResetWindows
        {
            get => resetWindows;
            set
            {
                resetWindows = value;
                Globals.ini.IniWriteValue("XShortcuts", "ResetWindows", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] cutscenes;
        public static int[] CutscenesMode
        {
            get => cutscenes;
            set
            {
                cutscenes = value;
                Globals.ini.IniWriteValue("XShortcuts", "Cutscenes", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] _switch;
        public static int[] SwitchLayout
        {
            get => _switch;
            set
            {
                _switch = value; Globals.ini.IniWriteValue("XShortcuts", "Switch", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] lockInputs;
        public static int[] LockInputs
        {
            get => lockInputs;
            set
            {
                lockInputs = value;
                Globals.ini.IniWriteValue("XShortcuts", "LockInputs", $"{value[0]}+{value[1]}");
            }
        }

        private static int[] releaseCursor;
        public static int[] ReleaseCursor
        {
            get => releaseCursor;
            set
            {
                releaseCursor = value;
                Globals.ini.IniWriteValue("XShortcuts", "ReleaseCursor", $"{value[0]}+{value[1]}");
            }
        }

        public static bool LoadSettings()
        {
            close = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "Close").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "Close").Split('+')[1]) };
            stop = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "Stop").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "Stop").Split('+')[1]) };
            topMost = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "TopMost").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "TopMost").Split('+')[1]) };
            setFocus = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "SetFocus").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "SetFocus").Split('+')[1]) };
            resetWindows = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "ResetWindows").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "ResetWindows").Split('+')[1]) };
            cutscenes = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "Cutscenes").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "Cutscenes").Split('+')[1]) };
            _switch = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "Switch").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "Switch").Split('+')[1]) };       
            lockInputs = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "LockInputs").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "LockInputs").Split('+')[1]) };
            releaseCursor = new int[] { int.Parse(Globals.ini.IniReadValue("XShortcuts", "ReleaseCursor").Split('+')[0]), int.Parse(Globals.ini.IniReadValue("XShortcuts", "ReleaseCursor").Split('+')[1]) };

            return true;
        }
    }
}
