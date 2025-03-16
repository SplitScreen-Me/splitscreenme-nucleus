using Newtonsoft.Json.Linq;
using Nucleus.Gaming;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal static class InputsText
    {
        internal static Color defaultForeColor;
        internal static Color notEnoughPlayers = Color.FromArgb(255, 245, 4, 68);

        static InputsText()
        {
            string[] rgb_font = Globals.ThemeConfigFile.IniReadValue("Colors", "Font").Split(',');
            defaultForeColor = Color.FromArgb(int.Parse(rgb_font[0]), int.Parse(rgb_font[1]), int.Parse(rgb_font[2]));
        }

        public static (string, Color) GetInputText(bool profileDisabled)
        {
            Color color = defaultForeColor;

            string msg = string.Empty;

            if(GameProfile.Instance.DevicesList.Count == 0)
            {
                msg = "Waiting For Compatible Devices...";
                color = notEnoughPlayers;
            }
            else if (GameProfile.Loaded)
            {
                if (GameProfile.TotalAssignedPlayers > GameProfile.TotalProfilePlayers)
                {
                    msg = $"There Is Too Much Players!";
                    color = notEnoughPlayers;
                }
                else if ((GameProfile.TotalProfilePlayers - GameProfile.TotalAssignedPlayers) > 0)
                {
                    string st = GameProfile.GamepadCount > 1 ? "Controllers" : "Controller";
                    string sc = GameProfile.AllScreens.Count() > 1 ? "Screens" : "Screen";
                    msg = $"{GameProfile.GamepadCount} {st}, {GameProfile.KeyboardCount} K&M And {GameProfile.AllScreens.Count()} {sc}, Were Used Last Time.";
                    color = notEnoughPlayers;
                }
                else if (GameProfile.TotalProfilePlayers == GameProfile.TotalAssignedPlayers)
                {
                    msg = $"Profile Ready!";
                }
            }
            else
            {
                string stepTxt = GameProfile.Game.Options.Count > 0 ? "Click \"NEXT\" When Ready." : "Click \"PLAY\" When Ready.";
                string screenText = GameProfile.Instance.Screens.Count > 1 ? "On The Desired Screens" : "On The Screen";

                if (GameProfile.Game.SupportsMultipleKeyboardsAndMice)
                {
                    msg = $"Press A Key\\Button On Each Device And Drop Them {screenText}. {stepTxt}";

                }
                else if (!GameProfile.Game.SupportsMultipleKeyboardsAndMice && !GameProfile.Game.SupportsKeyboard)
                {
                    if (DevicesFunctions.UseGamepadApiIndex || profileDisabled)
                    {
                        msg = $"Drop The Gamepads {screenText}. {stepTxt}";
                    }
                    else
                    {
                        msg = $"Press A Button On Each Gamepad And Drop Them {screenText}. {stepTxt}";
                    }
                }
                else
                {
                    if (DevicesFunctions.UseGamepadApiIndex || profileDisabled)
                    {
                        msg = $"Drop The Gamepads Or Keyboard\\Mouse {screenText}. {stepTxt}";
                    }
                    else
                    {
                        msg = $"Press A Button On Each Gamepad And Drop The Devices {screenText}. {stepTxt}";
                    }
                }
            }

            return (msg, color);
        }
    }
}
