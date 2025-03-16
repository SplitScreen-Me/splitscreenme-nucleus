//using SharpDX.DirectInput;
//using SharpDX.XInput;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;

//namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
//{
//    public class XInputDevices
//    {
//        private static System.Threading.Timer vibrationTimer;
//        private static bool vgmDevicesOnly = false;

//        public static bool PollXInputGamepad(PlayerInfo player, bool useGamepadApiIndex)
//        {
//            if (player.IsFake || !player.IsXInput)
//            {
//                return false;
//            }

//            try
//            {
//                if (!player.XInputJoystick.IsConnected)
//                {
//                    return false;
//                }

//                if (vgmDevicesOnly)
//                {
//                    CapabilitiesEx cap;
//                    var _cap = OpenXinputController.XInputGetCapabilitiesEx((uint)1, (uint)player.GamepadId, 1, out cap);

//                    if (!cap.VendorId.ToString().StartsWith("202"))
//                    {
//                        return false;
//                    }

//                    //JoyStickList = JoyStickList.Where(j => j.Properties.VendorId.ToString().StartsWith("202")).ToList();
//                }

//                if (player.XInputJoystick.GetState().Gamepad.Buttons != 0)
//                {
//                    if (useGamepadApiIndex)
//                    {
//                        Vibrate(player);
//                        return true;
//                    }

//                    if (player.DInputJoystick == null)
//                    {
//                        int pressedCount = 0;

//                        D_Joystick joystick = null;

//                        for (int i = 0; i < DInput.D_JoysticksList.Count; i++)
//                        {
//                            joystick = DInput.D_JoysticksList[i];

//                            if (joystick.State.Buttons.Any(b => b == true))
//                            {
//                                player.DInputJoystick = joystick;
//                                ++pressedCount;
//                            }
//                        }

//                        if (pressedCount != 1)
//                        {
//                            return false;
//                        }

//                        if (player.DInputJoystick != null)
//                        {
//                            player.GamepadGuid = player.DInputJoystick.InstanceGuid;
//                            player.GamepadProductGuid = player.DInputJoystick.ProductGuid;
//                            player.GamepadName = player.DInputJoystick.InstanceName;

//                            string hid = player.DInputJoystick.InterfacePath;
//                            int start = hid.IndexOf("hid#");
//                            int end = hid.LastIndexOf("#{");
//                            string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();

//                            player.RawHID = hid;
//                            player.HIDDeviceID = new string[] { fhid, "" };
//                            player.IsInputUsed = true;
//                            Vibrate(player);
//                        }
//                    }

//                    if (player.DInputJoystick != null)
//                    {
//                        player.IsInputUsed = true;
//                        return true;
//                    }

//                    return false;
//                }
//            }
//            catch (Exception)
//            {
//                return false;
//            }

//            return false;
//        }

//        internal static void Vibrate(PlayerInfo player)
//        {
//            if (!player.Vibrate)
//            {
//                Vibration vibration = new Vibration
//                {
//                    RightMotorSpeed = (ushort)65535,///make it full strenght because controllers can have different sensitivity
//                    LeftMotorSpeed = (ushort)65535///make it full strenght because controllers can have different sensitivity
//                };

//                player.XInputJoystick.SetVibration(vibration);
//                vibrationTimer = new System.Threading.Timer(Vibration_Tick, player, 90, 0);
//                player.Vibrate = true;
//            }
//        }


//        internal static void Vibration_Tick(object state)
//        {
//            PlayerInfo player = (PlayerInfo)state;

//            if (player.XInputJoystick.IsConnected)
//            {
//                Vibration vibration = new Vibration
//                {
//                    RightMotorSpeed = (ushort)0,
//                    LeftMotorSpeed = (ushort)0
//                };

//                player.XInputJoystick.SetVibration(vibration);
//            }

//            vibrationTimer.Dispose();
//        }

//        //public OpenXinputController XinputController;
//        //public State State => GetState();
//        //public Guid InstanceGuid;
//        //public Guid ProductGuid;
//        //public string HIDPath;
//        //public string InstanceName;
//        //public int UserIndex;

//        //private State GetState()
//        //{
//        //    if(!XinputController.IsConnected)
//        //    {
//        //        return new State();
//        //    }

//        //    State state = XinputController.GetState();

//        //    return state;
//        //}
//    }
//}
