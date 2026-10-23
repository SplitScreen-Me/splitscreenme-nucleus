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
                string logPath = Path.Combine(Application.StartupPath, "debug-log.txt");
                if (App_Misc.TextEditorPath != "Default" && File.Exists(App_Misc.TextEditorPath))
                {
                    Process.Start($"{App_Misc.TextEditorPath}", $"\"{logPath}\"");
                }
                else
                {
                    Process.Start(Application.StartupPath);
                    Process.Start("notepad.exe", $"\"{logPath}\"");
                }
            }
            catch (Exception)
            {
            }
        }
    }
}
