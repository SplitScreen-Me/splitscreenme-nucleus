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

        static InputsText()
        {
            string[] rgb_font = Globals.ThemeConfigFile.IniReadValue("Colors", "Font").Split(',');
            defaultForeColor = Color.FromArgb(int.Parse(rgb_font[0]), int.Parse(rgb_font[1]), int.Parse(rgb_font[2]));
        }

        public static (string, Color)[] GetInputText()
        {
            var msg = new (string, Color)[] { };
      
            if (GameProfile.Instance.DevicesList.Count == 0)
            {
                msg = new (string, Color)[] { ("Awaiting handler compatible devices...", notEnoughPlayers) };
                if (UI_Graphics.BlinkInputTimer == null)
                UI_Graphics.StartBlinkInputIcons();
                
            }
            else if (GameProfile.Loaded)
            {
                if (GameProfile.TotalAssignedPlayers > GameProfile.TotalProfilePlayers)
                {
                    //msg = $"There are too many players! {GameProfile.TotalAssignedPlayers}";
                    msg = new (string, Color)[] { ($"There are too many players! {GameProfile.TotalAssignedPlayers}", notEnoughPlayers) };
                    //color = notEnoughPlayers;
                }
                else if ((GameProfile.TotalProfilePlayers - GameProfile.TotalAssignedPlayers) > 0)
                {
                    if (GameProfile.AwaitedProfilePlayer != null)
                    {
                        var inputType = GameProfile.AwaitedProfilePlayer.IsKeyboardPlayer || GameProfile.AwaitedProfilePlayer.IsRawKeyboard || GameProfile.AwaitedProfilePlayer.IsRawMouse ? ("k&&m.", "Press a key/button on") : ("gamepad.", GameProfile.UseXinputIndex ? "Connect the" : "Press a button on");
                        msg = new (string, Color)[] { (inputType.Item2, defaultForeColor), ($"{GameProfile.AwaitedProfilePlayer.Nickname}'s", Color.DodgerBlue), ($"{inputType.Item1}", defaultForeColor) };
                    }
                }
                else if (GameProfile.AssignedDevices.Any(p => p.InstanceGuests.Count < p.CurrentMaxGuests))
                {
                    if (GameProfile.AwaitedProfilePlayer != null)
                    {
                        msg = new (string, Color)[] { ($"Press a button", Color.YellowGreen),($"on each gamepad to add as guest for", defaultForeColor) , ($"{GameProfile.AwaitedProfilePlayer.Nickname}.", Color.DodgerBlue) };
                    }
                }
                else if (GameProfile.TotalProfilePlayers == GameProfile.TotalAssignedPlayers)
                {
                    msg = new (string, Color)[] { ($"Profile ready!", defaultForeColor) };
                }
            }
            else
            {
                Color stepTextColor = GetImageMainColor.ParseColor(ImageCache.GetImage(Globals.ThemeFolder + "play.png"));
                string stepTxt = GameProfile.Game.Options.Count > 0 ? "NEXT" : "▶PLAY";
                string screenText = GameProfile.Instance.Screens.Count > 1 ? "on the desired screens" : "on the screen";

                if (GameProfile.Game.SupportsMultipleKeyboardsAndMice)
                {
                    msg = new (string, Color)[] { ($"Press a key or button", Color.YellowGreen),($"on each device and drop them {screenText}.", defaultForeColor), ("Click", defaultForeColor), (stepTxt, stepTextColor), ("when ready.", defaultForeColor) };

                }
                else if (!GameProfile.Game.SupportsMultipleKeyboardsAndMice && !GameProfile.Game.SupportsKeyboard)
                {
                    if (GameProfile.UseXinputIndex)
                    {
                        msg = new (string, Color)[] { ($"Drop the gamepads {screenText}.", defaultForeColor), ("Click", defaultForeColor), (stepTxt, stepTextColor), ("when ready.", defaultForeColor) };
                    }
                    else
                    {
                        msg = new (string, Color)[] { ($"Press a button", Color.YellowGreen),($"on each gamepad and drop them {screenText}.", defaultForeColor), ("Click", defaultForeColor), (stepTxt, stepTextColor), ("when ready.", defaultForeColor) };
                    }
                }
                else
                {
                    if (GameProfile.UseXinputIndex)
                    {
                        msg = new (string, Color)[] { ($"Drop the gamepads or keyboard\\mouse {screenText}.", defaultForeColor), ("Click", defaultForeColor), (stepTxt, stepTextColor), ("when ready.", defaultForeColor) };
                    }
                    else
                    {
                        msg = new (string, Color)[] { ($"Press a button", Color.YellowGreen),($"on each gamepad and drop the devices {screenText}.", defaultForeColor), ("Click", defaultForeColor), (stepTxt, stepTextColor), ("when ready.", defaultForeColor) };
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
