using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Localization;

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
                UserPrompts.Ask(new PromptRequest
                {
                    Kind = PromptKind.Message,
                    Icon = PromptIcon.Error,
                    Title = I18n.T("engine.settingsLoad.title", new { error = ex.Message, stackTrace = ex.StackTrace }),
                    Message = I18n.T("engine.settingsLoad.message"),
                    DesktopOnly = true
                });
                return false;
            }

            return true;
        }
    }
}
