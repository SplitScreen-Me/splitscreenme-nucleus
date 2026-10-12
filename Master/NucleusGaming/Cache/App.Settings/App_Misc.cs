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

        // Fast launch: ends launch waits early when the awaited state is observable. On by default (missing key = on).
        private static bool fastLaunch;
        public static bool FastLaunch
        {
            get => fastLaunch;
            set
            {
                fastLaunch = value;
                Globals.ini.IniWriteValue("Misc", "FastLaunch", value.ToString());
            }
        }

        // Hidden [Misc] FastLaunchLevel: 1 = only Nucleus's own sleeps, 2 (default) = also the handler pauses and window setup cadence.
        private static int fastLaunchLevel = 2;
        public static int FastLaunchLevel => fastLaunchLevel;

        // Web UI host window: "x,y,width,height,maximized" in physical pixels, empty for the default placement.
        private static string hostWindowPlacement;
        public static string HostWindowPlacement
        {
            get => hostWindowPlacement;
            set
            {
                hostWindowPlacement = value;
                Globals.ini.IniWriteValue("Misc", "HostWindowPlacement", value);
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

        private static bool profileAssignGamepadByButonPress;
        public static bool ProfileAssignGamepadByButonPress
        {
            get => profileAssignGamepadByButonPress;
            set
            {
                profileAssignGamepadByButonPress = value;
                Globals.ini.IniWriteValue("Dev", "ProfileAssignGamepadByButonPress", value.ToString());
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
            hostWindowPlacement = Globals.ini.IniReadValue("Misc", "HostWindowPlacement");
            //Tolerant on purpose: Settings.ini files older than the key must keep loading (missing = on, the default).
            fastLaunch = Nucleus.Gaming.Coop.Generic.LaunchWait.ParseFastLaunch(Globals.ini.IniReadValue("Misc", "FastLaunch"));
            fastLaunchLevel = Nucleus.Gaming.Coop.Generic.LaunchWait.ParseLevel(Globals.ini.IniReadValue("Misc", "FastLaunchLevel"));

            showFavoriteOnly = bool.Parse(Globals.ini.IniReadValue("Dev", "ShowFavoriteOnly"));
            disablePathCheck = bool.Parse(Globals.ini.IniReadValue("Dev", "DisablePathCheck"));
            textEditorPath = Globals.ini.IniReadValue("Dev", "TextEditorPath");

            useXinputIndex = bool.Parse(Globals.ini.IniReadValue("Dev", "UseXinputIndex"));
            profileAssignGamepadByButonPress = bool.Parse(Globals.ini.IniReadValue("Dev", "ProfileAssignGamepadByButonPress"));

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