using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Controls;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public static class Globals
    {
        public const string Version = "2.3.3";

        public static readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        //returns theme path(current theme folder)
        public static string ThemeFolder => Path.Combine(Application.StartupPath, $@"gui\theme\{App_Misc.Theme}\");

        //returns theme.ini file(current theme)
        public static IniFile ThemeConfigFile => new IniFile(Path.Combine(ThemeFolder, "theme.ini"));

        public static string GameProfilesFolder => Path.Combine(Application.StartupPath, $"game profiles");
       
        public static readonly int NucleusMaxPlayers = 32;

        public static string NucleusInstallRoot => Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

        //Used so any launcher can add its own "main window" handle without worring about catching the window manually.
        public static IntPtr MainWindowHandle;

        public static string UserEnvironmentRoot => Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        public static string UserDocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

        public static WPF_OSD MainOSD;
    }
}
