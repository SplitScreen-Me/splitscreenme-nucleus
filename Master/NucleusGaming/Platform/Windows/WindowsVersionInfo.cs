using Microsoft.Win32;
using System;

// http://www.codeproject.com/Articles/73000/Getting-Operating-System-Version-Info-Even-for-Win
//https://en.wikipedia.org/wiki/List_of_Microsoft_Windows_versions

//Thanks to Member 7861383, Scott Vickery for the Windows 8.1 update and workaround.
//I have moved it to the beginning of the Name property, though...

//Thakts to Brisingr Aerowing for help with the Windows 10 adapatation

namespace Nucleus.Gaming.Windows
{
    /// <summary>
    /// Provides detailed information about the host operating system.
    /// </summary>
    public static class WindowsVersionInfo
    {
        #region VERSION
        #region BUILD
        /// <summary>
        /// Gets the build version number of the operating system running on this computer.
        /// </summary>
        public static int BuildVersion => int.Parse(RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentBuildNumber", "0"));
        #endregion BUILD

        #region FULL
        #region VERSION
        /// <summary>
        /// Gets the full version of the operating system running on this computer.
        /// </summary>
        public static Version Version => new Version(MajorVersion, MinorVersion, BuildVersion, RevisionVersion);
        #endregion VERSION
        #endregion FULL

        #region MAJOR
        /// <summary>
        /// Gets the major version number of the operating system running on this computer.
        /// </summary>
        public static int MajorVersion
        {
            get
            {
                if (IsWindows10())
                {
                    return 10;
                }
                string exactVersion = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "");
                if (!string.IsNullOrEmpty(exactVersion))
                {
                    string[] splitVersion = exactVersion.Split('.');
                    return int.Parse(splitVersion[0]);
                }
                return Environment.OSVersion.Version.Major;
            }
        }
        #endregion MAJOR

        #region MINOR
        /// <summary>
        /// Gets the minor version number of the operating system running on this computer.
        /// </summary>
        public static int MinorVersion
        {
            get
            {
                if (IsWindows10())
                {
                    return 0;
                }
                string exactVersion = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CurrentVersion", "");
                if (!string.IsNullOrEmpty(exactVersion))
                {
                    string[] splitVersion = exactVersion.Split('.');
                    return int.Parse(splitVersion[1]);
                }
                return Environment.OSVersion.Version.Minor;
            }
        }
        #endregion MINOR

        #region REVISION
        /// <summary>
        /// Gets the revision version number of the operating system running on this computer.
        /// </summary>
        public static int RevisionVersion
        {
            get
            {
                if (IsWindows10())
                {
                    return 0;
                }
                return Environment.OSVersion.Version.Revision;
            }
        }
        #endregion REVISION
        #endregion VERSION

        #region Windows 10 Detection

        private static bool IsWindows10()
        {
            string productName = RegistryRead(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName", "");
            if (productName.StartsWith("Windows 10", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Registry Methods

        private static string RegistryRead(string RegistryPath, string Field, string DefaultValue)
        {
            string rtn = "";
            string backSlash = "";
            string newRegistryPath = "";

            try
            {
                RegistryKey OurKey = null;
                string[] split_result = RegistryPath.Split('\\');

                if (split_result.Length > 0)
                {
                    split_result[0] = split_result[0].ToUpper();        // Make the first entry uppercase...

                    if (split_result[0] == "HKEY_CLASSES_ROOT")
                    {
                        OurKey = Registry.ClassesRoot;
                    }
                    else if (split_result[0] == "HKEY_CURRENT_USER")
                    {
                        OurKey = Registry.CurrentUser;
                    }
                    else if (split_result[0] == "HKEY_LOCAL_MACHINE")
                    {
                        OurKey = Registry.LocalMachine;
                    }
                    else if (split_result[0] == "HKEY_USERS")
                    {
                        OurKey = Registry.Users;
                    }
                    else if (split_result[0] == "HKEY_CURRENT_CONFIG")
                    {
                        OurKey = Registry.CurrentConfig;
                    }

                    if (OurKey != null)
                    {
                        for (int i = 1; i < split_result.Length; i++)
                        {
                            newRegistryPath += backSlash + split_result[i];
                            backSlash = "\\";
                        }

                        if (newRegistryPath != "")
                        {
                            //rtn = (string)Registry.GetValue(RegistryPath, "CurrentVersion", DefaultValue);

                            OurKey = OurKey.OpenSubKey(newRegistryPath);
                            rtn = (string)OurKey.GetValue(Field, DefaultValue);
                            OurKey.Close();
                        }
                    }
                }
            }
            catch { }

            return rtn;
        }

        #endregion Registry Methods
    }
}
