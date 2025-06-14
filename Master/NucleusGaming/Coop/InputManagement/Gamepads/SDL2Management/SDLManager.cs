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

namespace Gamepads
{
    unsafe public static class SDLManager
    {
        private static bool logDeviceInfo = false;

        public static bool SDL_INITIALIZED { get; private set; }
        public static List<SDL_GameController> SDL2DevicesList = new List<SDL_GameController>();
        public static int JoystickCount => SDL2.NumJoysticks();

        /// <summary>
        /// Use to filter out VGMaster virtual x360 controllers
        /// </summary>
        public const ushort Virtual_VendorID = 202;

        #region INITIAL SETUP

        public static void InitSDL(SynchronizationContext syncContext)
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

            Start_SDL_Eventsloop(syncContext);
        }

        private static void Start_SDL_Eventsloop(SynchronizationContext syncContext)
        {
            Thread sdl_Loop = new Thread(() => SDL_Loop(syncContext));
            sdl_Loop.IsBackground = true;
            SDL_INITIALIZED = true;
            sdl_Loop.Start();
        }

        private static void Refresh_SDL_CallBack(SynchronizationContext syncContext)
        {
            // Post the method callback to DevicesFunctions thread(main thread)
            syncContext.Post(_ =>
            {
                DevicesFunctions.RefreshSDL(syncContext);

            }, null);
        }

        public static void Refresh(SynchronizationContext syncContext)
        {
            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                SDL_GameController controller = SDL2DevicesList[i];
                controller = IntPtr.Zero;
            }

            SDL2DevicesList.Clear();

            SDL2.Quit();
            SDL_INITIALIZED = false;
            Thread.Sleep(1000);

            SDL2.InitSubSystem(InitFlags.Video);
            SDL2.InitSubSystem(InitFlags.GameController);
            
            InitSDL(syncContext);
        }

        private static void InitGameControllers()
        {
            int deviceCount = SDL2.NumJoysticks();
            string deviceVar = deviceCount > 1 ? "Devices" : "Device";

            for (int i = 0; i < deviceCount; i++)
            {
                SDL_GameController controller = SDL2.GameControllerOpen(i);
                SDL2DevicesList.Add(controller);        
                
                if(logDeviceInfo)
                {
                    LogDeviceInfo(controller);
                }

                LoadControllerMapping(controller);           
            }
        }
    
        #endregion

        #region SDL EVENTS LOOP

        private static void SDL_Loop(SynchronizationContext syncContext)
        {
            
            Event _event;

            while (SDL2.PollEvent(out _event) != -1 && SDL_INITIALIZED)
            {
                if (!SDL_INITIALIZED)
                {
                    Console.WriteLine("Exit SDL");
                    return;
                }

                if (_event.Type == EventType.ControllerDeviceAdded)
                {
                    if (_event.CDevice.Which > SDL2DevicesList.Count - 1 && !SDL2.JoystickGetDeviceVendor(_event.CDevice.Which).ToString().StartsWith(Virtual_VendorID.ToString()))
                    {
                        Refresh_SDL_CallBack(syncContext);
                        return;
                    }
                }

                if (_event.Type == EventType.ControllerDeviceRemoved)
                {
                    RemoveSDLDevice();
                }

                SDL2.GameControllerUpdate();

                SDL2.SDL_Delay(16);
            }
        }

        #endregion

        #region ADD/REMOVE SDL DEVICES FUNCTIONS

        private static void AddNewSDLDevice()
        {
            int lastEntry = SDL2DevicesList.Count;

            SDL_GameController controller = SDL2.GameControllerOpen(lastEntry);

            if (controller != IntPtr.Zero)
            {
                if(!SDL2DevicesList.Contains(controller))
                {
                    SDL2DevicesList.Add(controller);
                    //LogDeviceInfo(controller);
                    //SortDeviceByInstanceId();
                }             
            }    
        }

        private static void RemoveSDLDevice()
        {
            List<SDL_GameController> toRemove = new List<SDL_GameController>();

            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                SDL_GameController isConnected = SDL2DevicesList[i];

                if (SDL.SDL2.GameControllerGetAttached(isConnected) == false)
                {
                    toRemove.Add(isConnected);
                }
            }

            foreach (SDL_GameController disconnected in toRemove)
            {
                if (SDL2DevicesList.Contains(disconnected)) 
                {
                    SDL2DevicesList.Remove(disconnected);
                }              
            }
        }

        private static void SortDeviceByInstanceId()
        {
            Dictionary<int, SDL_GameController> tempList = new Dictionary<int, SDL_GameController>();


            for (int i = 0; i < SDL2DevicesList.Count; i++)
            {
                SDL_GameController controller = SDL2DevicesList[i];
                SDL_ControllerUtils.SDL_DeviceInfo info = SDL_ControllerUtils.GetSDL_DeviceInfo(controller);
                tempList.Add(info.JOYSTICKID, controller);
            }

            var test = tempList.OrderBy(k => k.Key);

            SDL2DevicesList.Clear();

            foreach (var kvp in test)
            {
                SDL2DevicesList.Add(kvp.Value);
                Console.WriteLine($"Clé : {kvp.Key}, Valeur : {kvp.Value}");
            }
            SDL.SDL2.GameControllerUpdate();
        }

        public static bool IsConnected(SDL_GameController controller)
        {
            if (controller == IntPtr.Zero || !SDL.SDL2.GameControllerGetAttached(controller))
            {
                RemoveSDLDevice();
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

            if(queryGuide)
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

                SDL_DeviceInfo info = SDL_ControllerUtils.GetSDL_DeviceInfo(player.SDL2Joystick);

                if (QueryControllerStateAny(player.SDL2Joystick))
                {
                    if (GameProfile.UseXinputIndex && !player.Vibrate)
                    {
                        SendRumble(player.SDL2Joystick);
                        player.Vibrate = true;
                        return true;
                    }

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

    }
}
