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
            string jsPath = Path.Combine(gameManager.GetJsScriptsPath(), currentGameInfo.Game.JsFileName);

            if (File.Exists(jsPath))
            {
                try
                {
                    if (App_Misc.TextEditorPath != "Default")
                    {
                        Process.Start(App_Misc.TextEditorPath, "\"" + jsPath + "\"");
                    }
                    else
                    {
                        Process.Start("notepad++.exe", "\"" + jsPath + "\"");
                    }

                }
                catch (Exception)
                {
                    Process.Start("notepad.exe", jsPath);
                }
            }
        }
    }
}
