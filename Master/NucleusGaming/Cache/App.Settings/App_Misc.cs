using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nucleus.Gaming.App.Settings
{
    public static class App_Misc
    {

        private static bool vGMOnly;
        public static bool VGMOnly
        {
            get => vGMOnly;
            set
            {
                vGMOnly = value;
                Globals.ini.IniWriteValue("Misc", "GMOnly", value.ToString());
            }
        }

        private static bool useNicksInGame;
        public static bool UseNicksInGame
        {
            get => useNicksInGame;
            set
            {
                useNicksInGame = value;
                Globals.ini.IniWriteValue("Misc", "UseNicksInGame", value.ToString());
            }
        }

        private static bool debugLog;
        public static bool DebugLog
        {
            get => debugLog;
            set
            {
                debugLog = value;
                Globals.ini.IniWriteValue("Misc", "DebugLog", value.ToString());
            }
        }

        private static string enableLogAnswer;
        public static string EnableLogAnswer
        {
            get => enableLogAnswer;
            set
            {
                enableLogAnswer = value;
                Globals.ini.IniWriteValue("Misc", "EnableLogAnswer", value);
            }
        }

        private static string steamLang;
        public static string SteamLang
        {
            get => steamLang;
            set
            {
                steamLang = value;
                Globals.ini.IniWriteValue("Misc", "SteamLang", value);
            }
        }

        private static string epicLang;
        public static string EpicLang
        {
            get => epicLang;
            set
            {
                epicLang = value;
                Globals.ini.IniWriteValue("Misc", "EpicLang", value);
            }
        }

        private static string network;
        public static string Network
        {
            get => network;
            set
            {
                network = value;
                Globals.ini.IniWriteValue("Misc", "Network", value);
            }
        }

        private static bool keepAccounts;
        public static bool KeepAccounts
        {
            get => keepAccounts;
            set
            {
                keepAccounts = value;
                Globals.ini.IniWriteValue("Misc", "KeepAccounts", value.ToString());
            }
        }

        private static bool disableForcedNote;
        public static bool DisableForcedNote
        {
            get => disableForcedNote;
            set
            {
                disableForcedNote = value;
                Globals.ini.IniWriteValue("Misc", "DisableForcedNote", value.ToString());
            }
        }

        private static string nucleusAccountPassword;
        public static string NucleusAccountPassword
        {
            get => nucleusAccountPassword;
            set
            {
                nucleusAccountPassword = value;
                Globals.ini.IniWriteValue("Misc", "NucleusAccountPassword", value);
            }
        }

        private static bool ignoreInputLockReminder;
        public static bool IgnoreInputLockReminder
        {
            get => ignoreInputLockReminder;
            set
            {
                ignoreInputLockReminder = value;
                Globals.ini.IniWriteValue("Misc", "IgnoreInputLockReminder", value.ToString());
            }
        }

        private static bool autoDesktopScaling;
        public static bool AutoDesktopScaling
        {
            get => autoDesktopScaling;
            set
            {
                autoDesktopScaling = value;
                Globals.ini.IniWriteValue("Misc", "AutoDesktopScaling", value.ToString());
            }
        }

        private static bool nucleusMultiInstances;
        public static bool NucleusMultiInstances
        {
            get => nucleusMultiInstances;
            set
            {
                nucleusMultiInstances = value;
                Globals.ini.IniWriteValue("Misc", "NucleusMultiInstances", value.ToString());
            }
        }

        private static bool disableGameProfiles;
        public static bool DisableGameProfiles
        {
            get => disableGameProfiles;
            set
            {
                disableGameProfiles = value;
                Globals.ini.IniWriteValue("Misc", "DisableGameProfiles", value.ToString());
            }
        }

        private static string windowSize;
        public static string WindowSize
        {
            get => windowSize;
            set
            {
                windowSize = value;
                Globals.ini.IniWriteValue("Misc", "WindowSize", value);
            }
        }

        private static string windowLocation;
        public static string WindowLocation
        {
            get => windowLocation;
            set
            {
                windowLocation = value;
                Globals.ini.IniWriteValue("Misc", "WindowLocation", value);
            }
        }

        private static string autoSearchLocation;
        public static string AutoSearchLocation
        {
            get => autoSearchLocation;
            set
            {
                autoSearchLocation = value;
                Globals.ini.IniWriteValue("Misc", "AutoSearchLocation", value);
            }
        }

        private static string settingsLocation;
        public static string SettingsLocation
        {
            get => settingsLocation;
            set
            {
                settingsLocation = value;
                Globals.ini.IniWriteValue("Misc", "SettingsLocation", value);
            }
        }

        private static string profileSettingsLocation;
        public static string ProfileSettingsLocation
        {
            get => profileSettingsLocation;
            set
            {
                profileSettingsLocation = value;
                Globals.ini.IniWriteValue("Misc", "ProfileSettingsLocation", value);
            }
        }

        private static string gamepadSettingsLocation;
        public static string GamepadSettingsLocation
        {
            get => gamepadSettingsLocation;
            set
            {
                gamepadSettingsLocation = value;
                Globals.ini.IniWriteValue("Misc", "GamepadSettingsLocation", value);
            }
        }

        private static string theme;
        public static string Theme
        {
            get => theme;
            set
            {
                theme = value;
                Globals.ini.IniWriteValue("Theme", "Theme", value);
            }
        }

        private static bool showFavoriteOnly;
        public static bool ShowFavoriteOnly
        {
            get => showFavoriteOnly;
            set
            {
                showFavoriteOnly = value;
                Globals.ini.IniWriteValue("Dev", "ShowFavoriteOnly", value.ToString());
            }
        }

        private static bool disablePathCheck;
        public static bool DisablePathCheck
        {
            get => disablePathCheck;
            set
            {
                disablePathCheck = value;
                Globals.ini.IniWriteValue("Dev", "DisablePathCheck", value.ToString());
            }
        }

        private static string textEditorPath;
        public static string TextEditorPath
        {
            get => textEditorPath;
            set
            {
                textEditorPath = value;
                Globals.ini.IniWriteValue("Dev", "TextEditorPath", value);
            }
        }

        private static bool useXinputIndex;
        public static bool UseXinputIndex
        {
            get => useXinputIndex;
            set
            {
                useXinputIndex = value;
                Globals.ini.IniWriteValue("Dev", "UseXinputIndex", value.ToString());
            }
        }

        private static int blur;
        public static int Blur
        {
            get => blur;
            set
            {
                blur = value;
                Globals.ini.IniWriteValue("Dev", "Blur", value.ToString());
            }
        }

        private static string osdColor;
        public static string OSDColor
        {
            get => osdColor;
            set
            {
                osdColor = value;
                Globals.ini.IniWriteValue("Dev", "OSDColor", value);
            }
        }

        public static bool LoadSettings()
        {
            useNicksInGame = bool.Parse(Globals.ini.IniReadValue("Misc", "UseNicksInGame"));
            debugLog = bool.Parse(Globals.ini.IniReadValue("Misc", "DebugLog"));
            enableLogAnswer = Globals.ini.IniReadValue("Misc", "EnableLogAnswer");
            steamLang = Globals.ini.IniReadValue("Misc", "SteamLang");
            epicLang = Globals.ini.IniReadValue("Misc", "EpicLang");
            network = Globals.ini.IniReadValue("Misc", "Network");
            keepAccounts = bool.Parse(Globals.ini.IniReadValue("Misc", "KeepAccounts"));
            disableForcedNote = bool.Parse(Globals.ini.IniReadValue("Misc", "DisableForcedNote"));
            nucleusAccountPassword = Globals.ini.IniReadValue("Misc", "NucleusAccountPassword");
            ignoreInputLockReminder = bool.Parse(Globals.ini.IniReadValue("Misc", "IgnoreInputLockReminder"));
            autoDesktopScaling = bool.Parse(Globals.ini.IniReadValue("Misc", "AutoDesktopScaling"));
            nucleusMultiInstances = bool.Parse(Globals.ini.IniReadValue("Misc", "NucleusMultiInstances"));
            disableGameProfiles = bool.Parse(Globals.ini.IniReadValue("Misc", "DisableGameProfiles"));
            windowSize = Globals.ini.IniReadValue("Misc", "WindowSize");
            windowLocation = Globals.ini.IniReadValue("Misc", "WindowLocation");
            autoSearchLocation = Globals.ini.IniReadValue("Misc", "AutoSearchLocation");
            settingsLocation = Globals.ini.IniReadValue("Misc", "SettingsLocation");
            profileSettingsLocation = Globals.ini.IniReadValue("Misc", "ProfileSettingsLocation");
            gamepadSettingsLocation = Globals.ini.IniReadValue("Misc", "GamepadSettingsLocation");

            theme = Globals.ini.IniReadValue("Theme", "Theme");

            showFavoriteOnly = bool.Parse(Globals.ini.IniReadValue("Dev", "ShowFavoriteOnly"));
            disablePathCheck = bool.Parse(Globals.ini.IniReadValue("Dev", "DisablePathCheck"));
            textEditorPath = Globals.ini.IniReadValue("Dev", "TextEditorPath");
            useXinputIndex = bool.Parse(Globals.ini.IniReadValue("Dev", "UseXinputIndex"));
            blur = int.Parse(Globals.ini.IniReadValue("Dev", "Blur"));
            osdColor = Globals.ini.IniReadValue("Dev", "OSDColor");
            gamesSorting = Globals.ini.IniReadValue("Misc", "GameSortingOpt").Split(',').ToList();

            string _vGMOnly = Globals.ini.IniReadValue("Misc", "VGMOnly");

            if(_vGMOnly != null && _vGMOnly != "")
            {
                vGMOnly = bool.Parse(_vGMOnly);
            }
            
            return true;
        }


        private static List<string> gamesSorting;
        public static List<string> GamesSorting
        {
            get => gamesSorting;
            set
            {
                gamesSorting = value;

                string result = string.Empty;

                for (int i = 0; i < value.Count; i++)
                {
                    if (i == 0)
                    {
                        result += value[i];
                    }
                    else
                    {
                        result += "," + value[i];
                    }
                }

                Globals.ini.IniWriteValue("Misc", "GameSortingOpt", result);
            }
        }

    }
}