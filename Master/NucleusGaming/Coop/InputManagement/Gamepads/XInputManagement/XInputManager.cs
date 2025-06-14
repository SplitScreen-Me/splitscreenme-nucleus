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

        public static bool Poll(PlayerInfo player)
        {
            if (player.IsFake || !player.IsXInput)
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

                if (player.XInputJoystick.GetState().Gamepad.Buttons != 0)
                {
                    if (GameProfile.UseXinputIndex)
                    {
                        Vibrate(player);
                        return true;
                    }

                    if (player.DInputJoystick == null)
                    {
                        int pressedCount = 0;

                        D_Joystick joystick = null;

                        for (int i = 0; i < DInputManager.D_JoysticksList.Count; i++)
                        {                           
                            if (DInputManager.D_JoysticksList[i].State.Buttons.Any(b => b == true))
                            {
                                joystick = DInputManager.D_JoysticksList[i];
                                ++pressedCount;
                            }
                        }

                        if (pressedCount > 1)
                        {
                            return false;
                        }

                        if (joystick != null)
                        {
                            player.DInputJoystick = joystick;
                            player.GamepadGuid = player.DInputJoystick.InstanceGuid;
                            player.GamepadProductGuid = player.DInputJoystick.ProductGuid;
                            player.GamepadName = player.DInputJoystick.InstanceName;

                            player.RawHID = player.DInputJoystick.InterfacePath;
                            int start = player.RawHID.IndexOf("hid#");
                            int end = player.RawHID.LastIndexOf("#{");
                            string fhid = player.RawHID.Substring(start, end - start).Replace('#', '\\').ToUpper();
                 
                            player.HIDDeviceID = new string[] { fhid, "" };
                            player.IsInputUsed = true;
                            Vibrate(player);
                        }
                    }

                    if (player.DInputJoystick != null)
                    {
                        player.IsInputUsed = true;
                        return true;
                    }

                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
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

        //public OpenXinputController XinputController;
        //public State State => GetState();
        //public Guid InstanceGuid;
        //public Guid ProductGuid;
        //public string HIDPath;
        //public string InstanceName;
        //public int UserIndex;

        //private State GetState()
        //{
        //    if(!XinputController.IsConnected)
        //    {
        //        return new State();
        //    }

        //    State state = XinputController.GetState();

        //    return state;
        //}
        //private static bool init;
        //public static bool Init
        //{
        //    get
        //    {
        //        if (!init)
        //        {
        //            init = true;
        //            Thread watchDevicesListThread = new Thread(WatchDevicesList);
        //            watchDevicesListThread.Start();
        //        }

        //        return init;
        //    }
        //}


        //public static List<X_Joystick> X_JoysticksList => x_joysticksList;

        //private static List<X_Joystick> x_joysticksList = new List<X_Joystick>();

        //private static void WatchDevicesList()
        //{
        //   InitJoystickList();

        //    while (true)
        //    {
        //        for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
        //        {
        //            OpenXinputController device = new OpenXinputController(true, i);

        //            //if (device.IsConnected && !x_joysticksList.Any(dev => dev.XinputController.userIndex == i))
        //            //{
        //            //    X_Joystick x_Joystick = new X_Joystick();
        //            //    x_Joystick.UserIndex = i;
        //            //    x_Joystick.XinputController = device;
        //            //    x_joysticksList.Add(x_Joystick);
        //            //}
        //        }

        //        x_joysticksList.RemoveAll(dev => !dev.XinputController.IsConnected);
        //        x_joysticksList = x_joysticksList.OrderBy(dev => dev.UserIndex).ToList();
        //        Thread.Sleep(100);
        //    }
        //}

        //private static void InitJoystickList()
        //{
        //    for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
        //    {
        //        OpenXinputController device = new OpenXinputController(true, i);

        //        //if (device.IsConnected && !x_joysticksList.Any(dev => dev.XinputController.userIndex == i))
        //        //{
        //        //    X_Joystick x_Joystick = new X_Joystick();
        //        //    x_Joystick.UserIndex = i;
        //        //    x_Joystick.XinputController = device;
        //        //    x_joysticksList.Add(x_Joystick);
        //        //}
        //    }
        //}
    }
}
