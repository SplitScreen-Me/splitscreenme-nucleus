using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Controls;
using System;
using System.IO;

namespace Nucleus.Gaming
{
    public static class Globals
    {
        public const string Version = "3.0.0-beta-3";

        public static readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        public static string GameProfilesFolder => Path.Combine(InstallPaths.Root, $"game profiles");
       
        public static readonly int NucleusMaxPlayers = 32;

        /// <summary>Folder of NucleusCoop.exe, in every process (see <see cref="InstallPaths.Root"/>).</summary>
        public static string NucleusInstallRoot => InstallPaths.Root;

        //Used so any launcher can add its own "main window" handle without worring about catching the window manually.
        public static IntPtr MainWindowHandle;

        public static string UserEnvironmentRoot => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserDocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public static bool IsOneDriveEnabled = UserDocumentsRoot.Contains("OneDrive");

        public static WPF_OSD MainOSD;
    }
}
