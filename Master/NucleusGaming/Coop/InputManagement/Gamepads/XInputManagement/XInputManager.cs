using Nucleus.Gaming.App.Settings;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public static class XInputManager
    {
        private static System.Threading.Timer vibrationTimer;

        /// <summary>Polls a pad of the current setup (<see cref="GameProfile.Game"/>, <see cref="GameProfile.Instance"/>).</summary>
        public static bool Poll(PlayerInfo player)
        {
            return Poll(player, GameProfile.Game, GameProfile.Instance?.DevicesList);
        }

        /// <summary>
        /// Polls an XInput pad of the setup. True when it reports a button press (the setup flashes it). A press on a pad not
        /// identified yet identifies it (<see cref="GamepadIdentity.Identify"/>); when that fails the pad stays hidden, flashes,
        /// and keeps <see cref="PlayerInfo.IdentificationFailedUtc"/> so the setup can say why.
        /// </summary>
        public static bool Poll(PlayerInfo player, GenericGameInfo game, IEnumerable<PlayerInfo> devices)
        {
            if (player.IsFake || !player.IsXInput || player.XInputJoystick == null)
            {
                return false;
            }

            try
            {
                if (!player.XInputJoystick.IsConnected)
                {
                    return false;
                }

                if (App_Misc.VGMOnly)
                {
                    CapabilitiesEx cap;
                    var _cap = OpenXinputController.XInputGetCapabilitiesEx((uint)1, (uint)player.GamepadId, 1, out cap);

                    if (!cap.VendorId.ToString().StartsWith("202"))
                    {
                        return false;
                    }
                }

                if (player.XInputJoystick.GetState().Gamepad.Buttons == 0)
                {
                    return false;
                }

                if (GameProfile.UseXinputIndex)
                {
                    Vibrate(player);
                    return true;
                }

                if (player.DInputJoystick != null)
                {
                    player.IsInputUsed = true;
                    return true;
                }

                if (player.IsInputUsed)
                {
                    return true;//identified by its slot earlier
                }

                List<DInputCandidate> candidates = new List<DInputCandidate>();

                foreach (D_Joystick joystick in DInputManager.D_JoysticksList)
                {
                    JoystickState state = joystick.State;
                    candidates.Add(new DInputCandidate(joystick, state != null && GamepadIdentity.IsPressed(state.Buttons, state.PointOfViewControllers)));
                }

                bool firstFailure = player.IdentificationFailedUtc == DateTime.MinValue;
                IdentifyResult result = GamepadIdentity.Identify(player, devices, candidates, GamepadIdentity.NeedsDInputIdentity(game), DateTime.UtcNow);

                switch (result)
                {
                    case IdentifyResult.Matched:
                        Vibrate(player);
                        break;
                    case IdentifyResult.SlotFallback:
                        LogManager.Log($"Gamepad: XInput slot {player.GamepadId + 1} identified by its slot ({candidates.Count(c => c.Pressed)} DirectInput devices pressed).");
                        Vibrate(player);
                        break;
                    default:
                        if (firstFailure)
                        {
                            LogManager.Log($"Gamepad: XInput slot {player.GamepadId + 1} pressed but not identified: {candidates.Count(c => c.Pressed)} of {candidates.Count} DirectInput devices pressed, and the handler needs the DirectInput identity.");
                        }
                        break;
                }

                //Also true when the pad stays hidden: it flashes, so the press was seen, and the setup explains why.
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        internal static void Vibrate(PlayerInfo player)
        {
            if (!player.Vibrate)
            {
                Vibration vibration = new Vibration
                {
                    RightMotorSpeed = (ushort)65535,///make it full strenght because controllers can have different sensitivity
                    LeftMotorSpeed = (ushort)65535///make it full strenght because controllers can have different sensitivity
                };

                player.XInputJoystick.SetVibration(vibration);
                vibrationTimer = new System.Threading.Timer(Vibration_Tick, player, 90, 0);
                player.Vibrate = true;
            }
        }


        internal static void Vibration_Tick(object state)
        {
            PlayerInfo player = (PlayerInfo)state;

            if (player.XInputJoystick.IsConnected)
            {
                Vibration vibration = new Vibration
                {
                    RightMotorSpeed = (ushort)0,
                    LeftMotorSpeed = (ushort)0
                };

                player.XInputJoystick.SetVibration(vibration);
            }

            vibrationTimer.Dispose();
        }
    }
}
