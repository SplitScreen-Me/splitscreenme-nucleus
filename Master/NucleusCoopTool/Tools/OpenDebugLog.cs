using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal class OpenDebugLog
    {
        public static void OpenDebugLogFile()
        {
            try
            {
                if (App_Misc.TextEditorPath != "Default" && File.Exists(App_Misc.TextEditorPath))
                {
                    Process.Start($"{App_Misc.TextEditorPath}", Path.Combine(Application.StartupPath, "debug-log.txt"));
                }
                else
                {
                    Process.Start(Application.StartupPath);
                    Process.Start("notepad.exe", Path.Combine(Application.StartupPath, "debug-log.txt"));
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
