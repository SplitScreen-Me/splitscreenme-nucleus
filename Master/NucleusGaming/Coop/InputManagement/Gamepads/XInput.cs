using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public static class XInput
    {
        private static bool init;
        public static bool Init
        {
            get
            {
                if (!init)
                {
                    init = true;
                    Thread watchDevicesListThread = new Thread(WatchDevicesList);
                    watchDevicesListThread.Start();
                }

                return init;
            }
        }


        public static List<X_Joystick> X_JoysticksList => x_joysticksList;

        private static List<X_Joystick> x_joysticksList = new List<X_Joystick>();

        private static void WatchDevicesList()
        {
           InitJoystickList();

            while (true)
            {
                for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
                {
                    OpenXinputController device = new OpenXinputController(true, i);

                    //if (device.IsConnected && !x_joysticksList.Any(dev => dev.XinputController.userIndex == i))
                    //{
                    //    X_Joystick x_Joystick = new X_Joystick();
                    //    x_Joystick.UserIndex = i;
                    //    x_Joystick.XinputController = device;
                    //    x_joysticksList.Add(x_Joystick);
                    //}
                }

                x_joysticksList.RemoveAll(dev => !dev.XinputController.IsConnected);
                x_joysticksList = x_joysticksList.OrderBy(dev => dev.UserIndex).ToList();
                Thread.Sleep(100);
            }
        }

        private static void InitJoystickList()
        {
            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                OpenXinputController device = new OpenXinputController(true, i);

                //if (device.IsConnected && !x_joysticksList.Any(dev => dev.XinputController.userIndex == i))
                //{
                //    X_Joystick x_Joystick = new X_Joystick();
                //    x_Joystick.UserIndex = i;
                //    x_Joystick.XinputController = device;
                //    x_joysticksList.Add(x_Joystick);
                //}
            }
        }
    }
}
