using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Gaming.App.Settings
{
    public static class App_Settings_Loader
    {
        public static bool InitializeSettings()
        {
            try
            {
                App_Misc.LoadSettings();
                App_Hotkeys.LoadSettings();
                App_GamePadShortcuts.LoadSettings();
                App_GamePadNavigation.LoadSettings();
                App_Layouts.LoadSettings();
                App_Audio.LoadSettings();
            }
            catch(Exception ex)
            {
                MessageBox.Show("Nucleus is unable to load its settings.\n" +
                                "Do not edit settings.ini manually.\n" +
                                "Try re-extracting Nucleus with 7-Zip.\n" +
                                "If the issue persists ask for support on our Discord server.", "Error loading Nucleus settings!" +
                                $"Error: " +
                                $"{ex.Message} \n" +
                                $"StackTrace:\n" +
                                $"{ex.StackTrace}", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly, false);
                return false;
            }

            return true;
        }
    }
}
