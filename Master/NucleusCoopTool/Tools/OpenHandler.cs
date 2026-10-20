using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using System;
using System.Diagnostics;
using System.IO;

namespace Nucleus.Coop.Tools
{
    internal class OpenHandler
    {
        public static void OpenRawHandler(UserGameInfo currentGameInfo)
        {
            GameManager gameManager = GameManager.Instance;

            if (File.Exists(currentGameInfo.FileFullPath))
            {
                try
                {
                    if (App_Misc.TextEditorPath != "Default")
                    {
                        ProcessStartInfo editor = new ProcessStartInfo
                        {
                            FileName = App_Misc.TextEditorPath,
                            Arguments = $"\"{currentGameInfo.FileFullPath}\"",
                            UseShellExecute = true
                        };

                        Process.Start(editor);
                    }
                    else
                    {
                        Process.Start("notepad++.exe",  currentGameInfo.FileFullPath );
                    }

                }
                catch (Exception)
                {
                    Process.Start("notepad.exe", currentGameInfo.FileFullPath);
                }
            }
        }
    }
}
