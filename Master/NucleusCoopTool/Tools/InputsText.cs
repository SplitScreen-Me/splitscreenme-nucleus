using Newtonsoft.Json.Linq;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;

namespace Nucleus.Coop.Tools
{
    internal static class InputsText
    {
        internal static Color defaultForeColor;
        internal static Color notEnoughPlayers = Color.FromArgb(255, 245, 4, 68);
        private static Font bigFont;

        static InputsText()
        {
            string[] rgb_font = Globals.ThemeConfigFile.IniReadValue("Colors", "Font").Split(',');
            defaultForeColor = Color.FromArgb(int.Parse(rgb_font[0]), int.Parse(rgb_font[1]), int.Parse(rgb_font[2]));
            bigFont = new Font(UI_Interface.InputsTextLabel.Container.Font.FontFamily, 10f, FontStyle.Bold);
        }

        public static (string, Color, Font)[] GetInputText()
        {
            var msg = new (string, Color,Font)[] { };

            if (GameProfile.Instance.DevicesList?.Count == 0)
            {
                msg = new (string, Color, Font)[] { ("Awaiting handler compatible devices...", notEnoughPlayers,null) };
                if (UI_Graphics.BlinkInputTimer == null)
                UI_Graphics.StartBlinkInputIcons();
                
            }
            else if (GameProfile.Loaded)
            {
                if (GameProfile.TotalAssignedPlayers > GameProfile.TotalProfilePlayers)
                {
                    msg = new (string, Color,Font)[] { ($"There are too many players! {GameProfile.TotalAssignedPlayers}", notEnoughPlayers,null) };
                }
                else if ((GameProfile.TotalProfilePlayers - GameProfile.TotalAssignedPlayers) > 0)
                {
                    if (GameProfile.AwaitedProfilePlayer != null)
                    {
                        var profilePlayerFound = GameProfile.ProfilePlayersList.Find(ppf => ppf.IsController && ppf.GamepadGuid == GameProfile.AwaitedProfilePlayer.GamepadGuid);

                        var inputType = GameProfile.AwaitedProfilePlayer.IsKeyboardPlayer || GameProfile.AwaitedProfilePlayer.IsRawKeyboard || GameProfile.AwaitedProfilePlayer.IsRawMouse ? 
                            ("k&&m.", "Press a key/button on") : 
                            ("gamepad.", GameProfile.UseXinputIndex || (profilePlayerFound == null && !GameProfile.Current_GenericGamInfo.MetaInfo.ProfileAssignGamepadByButonPress) ? "Connect or re-assign " : "Press a button on");

                        msg = new (string, Color,Font)[] { (inputType.Item2, defaultForeColor,null), ($"{GameProfile.AwaitedProfilePlayer.Nickname}'s", Color.DodgerBlue, null), ($"{inputType.Item1}", defaultForeColor, null) };
                    }
                }
                else if (GameProfile.AssignedDevices.Any(p => p.InstanceGuests.Count < p.CurrentMaxGuests))
                {
                    if (GameProfile.AwaitedProfilePlayer != null)
                    {
                        msg = new (string, Color,Font)[] { ($"Press a button", Color.YellowGreen, bigFont),($"on each gamepad to add as guest for", defaultForeColor, null) , ($"{GameProfile.AwaitedProfilePlayer.Nickname}.", Color.DodgerBlue, null) };
                    }
                }
                else if (GameProfile.TotalProfilePlayers == GameProfile.TotalAssignedPlayers)
                {
                    msg = new (string, Color, Font)[] { ($"Profile ready!", defaultForeColor, null) };
                }
            }
            else
            {
                Color stepTextColor = GetImageMainColor.ParseColor(ImageCache.GetImage(Globals.ThemeFolder + "play.png"));
                string stepTxt = GameProfile.Current_GenericGamInfo.Options.Count > 0 ? "NEXT" : "▶PLAY";
                string screenText = GameProfile.Instance.Screens.Count > 1 ? "on the desired screens" : "on the screen";

                if (GameProfile.Current_GenericGamInfo.SupportsMultipleKeyboardsAndMice)
                {
                    msg = new (string, Color, Font)[] { ($"Press a key or button", Color.YellowGreen, bigFont),($"on each device and drop them {screenText}.", defaultForeColor, null), ("Click", defaultForeColor, null), (stepTxt, stepTextColor, bigFont), ("when ready.", defaultForeColor, null) };

                }
                else if (!GameProfile.Current_GenericGamInfo.SupportsMultipleKeyboardsAndMice && !GameProfile.Current_GenericGamInfo.SupportsKeyboard)
                {
                    if (!GameProfile.Current_GenericGamInfo.MetaInfo.ProfileAssignGamepadByButonPress)
                    {
                        msg = new (string, Color, Font)[] { ($"Drop the gamepads {screenText}.", defaultForeColor, null), ("Click", defaultForeColor, null), (stepTxt, stepTextColor, bigFont), ("when ready.", defaultForeColor, null) };
                    }
                    else //if()
                    {
                        msg = new (string, Color, Font)[] { ($"Press a button", Color.YellowGreen, bigFont),($"on each gamepad and drop them {screenText}.", defaultForeColor, null), ("Click", defaultForeColor, null), (stepTxt, stepTextColor, bigFont), ("when ready.", defaultForeColor, null) };
                    }
                }
                else
                {
                    if (!GameProfile.Current_GenericGamInfo.MetaInfo.ProfileAssignGamepadByButonPress)
                    {
                        msg = new (string, Color, Font)[] { ($"Drop the gamepads or keyboard\\mouse {screenText}.", defaultForeColor, null), ("Click", defaultForeColor, null), (stepTxt, stepTextColor, bigFont), ("when ready.", defaultForeColor, null) };
                    }
                    else
                    {
                        msg = new (string, Color, Font)[] { ($"Press a button", Color.YellowGreen, bigFont),($"on each gamepad and drop the devices {screenText}.", defaultForeColor, null), ("Click", defaultForeColor, null), (stepTxt, stepTextColor, bigFont), ("when ready.", defaultForeColor, null) };
                    }
                }
            }

            if (GameProfile.Instance.DevicesList.Count > 0 && UI_Graphics.BlinkInputTimer != null)
            {
                UI_Graphics.StopBlinkInputIcons();
            }

            return msg;
        }
    }
}
