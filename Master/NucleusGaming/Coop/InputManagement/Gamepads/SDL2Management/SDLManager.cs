using SDL;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using static SDL.SDL_ControllerUtils;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.App.Settings;
using SharpDX.DirectInput;
using System.ComponentModel;

namespace Gamepads
{
    unsafe public static class SDLManager
    {
        public class SDLDevice_State
        {/// <summary>
         ///button[1] = DPAD_UP;
         ///button[2] = DPAD_DOWN;
         ///button[3] = DPAD_LEFT;
         ///button[4] = DPAD_RIGHT;
         ///button[5] = A;
         ///button[6] = B;
         ///button[7] = X;
         ///button[8] = Y;
         ///button[9] = LEFTSHOULDER;
         ///button[10] = RIGHTSHOULDER;
         ///Sticks Click
         ///button[11] = LEFTSTICK;
         ///button[12] = RIGHTSTICK;
         ///Back/Start/Guide
         ///button[13] = BACK;
         ///button[14] = START;
         ///button[15] = GUIDE;
         /// </summary>
         /// <param name="GameController"></param>
         /// <returns></returns>
            public byte[] Buttons = new byte[16];
            public long RawState;
        }

        private static bool logDeviceInfo = false;

        public static bool SDL_INITIALIZED { get; private set; }

        public static List<SDL_GameController> SDL2DevicesList = new List<SDL_GameController>();
        public static int JoystickCount => SDL2.NumJoysticks();

        /// <summary>
        /// Use to filter out VGMaster virtual x360 controllers
        /// </summary>
        public const ushort Virtual_VendorID = 202;


        public static void QUIT_SDL()
        {
            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                SDL2.GameControllerClose(SDL2DevicesList[i]);
                SDL2DevicesList[i] = IntPtr.Zero;
            }

            SDL2DevicesList.Clear();

            SDL2.Quit();
            SDL_INITIALIZED = false;
        }

        #region INITIAL SETUP

        public static void InitSDL()
        {
            if (SDL_INITIALIZED)
            {
                return;
            }

            LoadUserControllerMappings();

            ///"InitFlags.Video" Else gamecontroller add/remove events don't fire  
            //https://discourse.libsdl.org/t/sdl2-gamecontroller-events-not-firing-solved/49411/11
            SDL2.InitSubSystem(InitFlags.Video);
            SDL2.InitSubSystem(InitFlags.GameController);
            SDL2.SDL_SetHint("SDL_JOYSTICK_ALLOW_BACKGROUND_EVENTS", "1");

            InitGameControllers();

            Start_SDL_Eventsloop();
        }

        private static void Start_SDL_Eventsloop()
        {
            Thread sdl_Loop = new Thread(() => SDL_Loop());
            sdl_Loop.IsBackground = true;
            SDL_INITIALIZED = true;
            sdl_Loop.Start();
        }

        public static void Refresh()
        {
            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                SDL2.GameControllerClose(SDL2DevicesList[i]);
                SDL2DevicesList[i] = IntPtr.Zero;      
            }

            SDL2DevicesList.Clear();

            SDL2.Quit();
            SDL_INITIALIZED = false;
            Thread.Sleep(1000);

            SDL2.InitSubSystem(InitFlags.Video);
            SDL2.InitSubSystem(InitFlags.GameController);

            InitSDL();
        }

        private static void InitGameControllers()
        {
            int deviceCount = SDL2.NumJoysticks();
            //string deviceVar = deviceCount > 1 ? "Devices" : "Device";

            for (int i = 0; i < deviceCount; i++)
            {
                SDL_GameController controller = SDL2.GameControllerOpen(i);
                SDL2DevicesList.Add(controller);

                if (logDeviceInfo)
                {
                    LogDeviceInfo(controller);
                }

                LoadControllerMapping(controller);
            }
        }

        #endregion

        #region SDL EVENTS LOOP

        private static void SDL_Loop()
        {
            while (SDL2.PollEvent(out Event _event) != -1 && SDL_INITIALIZED)
            {
                if (!SDL_INITIALIZED)
                {
                    //Console.WriteLine("Exit SDL");
                    break;
                }

                if (_event.Type == EventType.ControllerDeviceAdded)
                {
                    //requires a proper check for vgm only gampepads
                    if (_event.CDevice.Which > SDL2DevicesList.Count - 1 /*|| _event.CDevice.Which > SDL2DevicesList.Count - 1 && (App_Misc.VGMOnly && !SDL2.JoystickGetDeviceVendor(_event.CDevice.Which).ToString().StartsWith(Virtual_VendorID.ToString()))*/)
                    {
                        UIThread.Post(() =>
                        {
                            //Refresh everything, else controllers index will not match between Nucleus and the game. 
                            Refresh();
                        });

                        break;
                    }
                }

                if (_event.Type == EventType.ControllerDeviceRemoved)
                {
                    RemoveDisconnectedSDLDevices();
                }

                SDL2.GameControllerUpdate();

                SDL2.SDL_Delay(16);
            }
        }

        #endregion

        #region ADD/REMOVE SDL DEVICES FUNCTIONS
        private static void RemoveDisconnectedSDLDevices()
        {
            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                if (!SDL2.GameControllerGetAttached(SDL2DevicesList[i]))
                {
                    SDL2DevicesList.RemoveAt(i);
                    if (i - 1 >= 0)
                    {
                        i--;
                    }
                }
            }
        }

        public static bool IsConnected(SDL_GameController controller)
        {
            if (controller == IntPtr.Zero || !SDL.SDL2.GameControllerGetAttached(controller))
            {
                return false;
            }

            return true;
        }

        #endregion

        public static bool QueryControllerStateAny(SDL_GameController GameController)
        {
            int anyState = 0;

            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT);

            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y);

            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);

            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);

            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
            anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);

            bool queryGuide = false;

            if (queryGuide)
            {
                anyState += SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE);
            }

            #region Set Triggers Values
            int leftTrig = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
            anyState += leftTrig >= 200 ? 1 : 0;

            int rightTrig = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);
            anyState += rightTrig >= 200 ? 1 : 0;
            #endregion

            return anyState > 0;
        }

        public static bool Poll(PlayerInfo player)
        {
            try
            {
                if (!IsConnected(player.SDL2Joystick))
                {
                    return false;
                }

                if (QueryControllerStateAny(player.SDL2Joystick))
                {
                    if (GameProfile.UseXinputIndex)
                    {
                        if (!player.Vibrate)
                        {
                            SendRumble(player.SDL2Joystick);
                            player.Vibrate = true;
                        }

                        return true;
                    }

                    if (player.DInputJoystick == null)//only used as a fallback now if the device hid path hasn't been found beforehand
                    {
                        int pressedCount = 0;

                        D_Joystick joystick = null;

                        for (int i = 0; i < DInputManager.D_JoysticksList.Count; i++)
                        {
                            joystick = DInputManager.D_JoysticksList[i];

                            if (App_Misc.VGMOnly)
                            {
                                if (!joystick.VendorId.StartsWith("202"))
                                {
                                    continue;
                                }
                            }

                            if (joystick.State.Buttons.Any(b => b == true))
                            {
                                player.DInputJoystick = joystick;
                                ++pressedCount;
                            }
                        }

                        if (pressedCount != 1)
                        {
                            return false;
                        }

                        if (player.DInputJoystick != null)
                        {
                            player.GamepadGuid = player.DInputJoystick.InstanceGuid;
                            player.GamepadProductGuid = player.DInputJoystick.ProductGuid;
                            player.GamepadName = player.DInputJoystick.InstanceName;
                            player.RawHID = player.DInputJoystick.InterfacePath;

                            int start = player.RawHID.IndexOf("hid#");
                            int end = player.RawHID.LastIndexOf("#{");
                            string fhid = player.RawHID.Substring(start, end - start).Replace('#', '\\').ToUpper();

                            player.HIDDeviceID = new string[] { fhid, "" };
                            player.IsInputUsed = true;

                            if (!player.Vibrate)
                            {
                                SendRumble(player.SDL2Joystick);
                                player.Vibrate = true;
                            }
                        }
                    }
                    else
                    {
                        if (!player.Vibrate)
                        {
                            SendRumble(player.SDL2Joystick);
                            player.Vibrate = true;
                        }

                        player.IsInputUsed = true;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }

            return false;
        }




        
        public static SDLDevice_State ButtonsState(SDL.SDL_GameController GameController)
        {
            if (GameController == IntPtr.Zero)
            {
                return null;
            }

            byte[] button = new byte[16];
            string stateString = "";
            //D-pad
            button[0] = 0;//Set a dummy else it's always 0 anyway(bug??)
            button[1] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_UP);
            button[2] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_DOWN);
            button[3] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_LEFT);
            button[4] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_DPAD_RIGHT);


            //A,B, X, Y
            button[5] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_A);
            button[6] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_B);
            button[7] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_X);
            button[8] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_Y);

            //Shoulders
            button[9] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSHOULDER);
            button[10] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSHOULDER);

            //Sticks Click
            button[11] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_LEFTSTICK);
            button[12] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_RIGHTSTICK);

            //Back/Start/Guide
            button[13] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_BACK);
            button[14] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_START);

            button[15] = SDL.SDL2.GameControllerGetButton(GameController, SDL.SDL_GameControllerButton.SDL_CONTROLLER_BUTTON_GUIDE);

            foreach (byte b in button)
            {
                stateString += b.ToString();
            }

            SDLDevice_State state = new SDLDevice_State();
            state.Buttons = button;
            state.RawState = Convert.ToInt64(stateString); 
            return state;
        }


        /// <summary>
        /// Get the state of the triggers for a given SDL_GameController [0] left trigger, [1] right trigger
        /// </summary>
        /// <param name="GameController"></param>
        /// <returns></returns>
        public static short[] TriggersValue(SDL.SDL_GameController GameController)
        {
            short[] value = new short[2];

            value[0] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERLEFT);
            value[1] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_TRIGGERRIGHT);

            return value;
        }


        public static short[][] AxisValue(SDL.SDL_GameController GameController)
        {
            short[][] value = new short[2][];
            value[0][0] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTX);
            value[0][1] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_RIGHTY);

            value[1][0] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTX);
            value[1][1] = SDL.SDL2.GameControllerGetAxis(GameController, SDL.GameControllerAxis.SDL_CONTROLLER_AXIS_LEFTY);

            return value;
        }
    }
}
