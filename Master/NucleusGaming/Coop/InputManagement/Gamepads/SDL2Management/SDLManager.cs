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
using Nucleus.Gaming;

namespace Gamepads
{
    unsafe public static class SDLManager
    {
        private static bool logDeviceInfo = false;

        public static bool SDL_INITIALIZED { get; private set; }
        public static List<SDL_GameController> SDL2DevicesList = new List<SDL_GameController>();

        /// <summary>
        /// Guards SDL: init and refresh, <see cref="SDL2DevicesList"/> and the controller handles. The event loop holds it
        /// while it pumps events, a refresh for its whole length (more than a second): the setup's device ticks take it with
        /// TryEnter and skip SDL while it is busy.
        /// </summary>
        public static readonly object Sync = new object();

        //Controllers opened since the last init. A refresh quits SDL, which frees every controller: an older handle must
        //never reach SDL again (reading a freed controller is undefined, closing it twice can end the process).
        private static readonly HashSet<IntPtr> liveHandles = new HashSet<IntPtr>();

        //Each init starts one event loop; a refresh bumps the generation so the previous loop exits.
        private static int loopGeneration;

        /// <summary>
        /// Use to filter out VGMaster virtual x360 controllers
        /// </summary>
        public const ushort Virtual_VendorID = 202;

        #region INITIAL SETUP

        public static void InitSDL(SynchronizationContext syncContext)
        {
            lock (Sync)
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
        }

        private static void Start_SDL_Eventsloop(SynchronizationContext syncContext)
        {
            int generation = ++loopGeneration;
            Thread sdl_Loop = new Thread(() => SDL_Loop(syncContext, generation));
            sdl_Loop.IsBackground = true;
            sdl_Loop.Name = "Nucleus SDL events";
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
            lock (Sync)
            {
                SDL2DevicesList.Clear();
                liveHandles.Clear();
                loopGeneration++;//the running loop exits

                SDL2.Quit();//closes and frees every controller
                SDL_INITIALIZED = false;
                Thread.Sleep(1000);

                SDL2.InitSubSystem(InitFlags.Video);
                SDL2.InitSubSystem(InitFlags.GameController);

                InitSDL(syncContext);
            }
        }

        private static void InitGameControllers()
        {
            int deviceCount = SDL2.NumJoysticks();
            string deviceVar = deviceCount > 1 ? "Devices" : "Device";

            for (int i = 0; i < deviceCount; i++)
            {
                //NULL for a joystick SDL has no game controller mapping for: kept so list indexes stay device indexes.
                SDL_GameController controller = SDL2.GameControllerOpen(i);
                SDL2DevicesList.Add(controller);

                if (controller == IntPtr.Zero)
                {
                    continue;
                }

                liveHandles.Add(controller);

                if(logDeviceInfo)
                {
                    LogDeviceInfo(controller);
                }

                LoadControllerMapping(controller);
            }
        }

        #endregion

        #region SDL EVENTS LOOP

        private static void SDL_Loop(SynchronizationContext syncContext, int generation)
        {
            while (true)
            {
                //SDL calls happen under the lock: a refresh quits SDL, which must not run while this thread pumps events.
                lock (Sync)
                {
                    if (!SDL_INITIALIZED || generation != loopGeneration)
                    {
                        return;
                    }

                    //PollEvent leaves the event untouched when the queue is empty: only handle the events it returns
                    //(the last one used to be handled again every 16 ms).
                    while (SDL2.PollEvent(out Event _event) == 1)
                    {
                        if (_event.Type == EventType.ControllerDeviceAdded)
                        {
                            if (_event.CDevice.Which > SDL2DevicesList.Count - 1 && !SDL2.JoystickGetDeviceVendor(_event.CDevice.Which).ToString().StartsWith(Virtual_VendorID.ToString()))
                            {
                                Refresh_SDL_CallBack(syncContext);
                                return;
                            }
                        }
                        else if (_event.Type == EventType.ControllerDeviceRemoved)
                        {
                            RemoveSDLDevice();
                        }
                    }

                    SDL2.GameControllerUpdate();
                }

                Thread.Sleep(16);
            }
        }

        #endregion

        #region ADD/REMOVE SDL DEVICES FUNCTIONS

        //Call with Sync held.
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

            for (int i = 0; i < toRemove.Count; i++)
            {
                SDL_GameController disconnected = toRemove[i];

                if (SDL2DevicesList.Contains(disconnected))
                {
                    SDL2DevicesList.Remove(disconnected);
                }
            }
        }


        /// <summary>
        /// Whether a controller is still plugged in. False for a handle from before the last refresh (SDL freed it). True
        /// while SDL is busy refreshing on another thread: it can't tell yet, and the next tick will.
        /// </summary>
        public static bool IsConnected(SDL_GameController controller)
        {
            if (!Monitor.TryEnter(Sync))
            {
                return true;
            }

            try
            {
                if (!IsLive(controller) || !SDL.SDL2.GameControllerGetAttached(controller))
                {
                    RemoveSDLDevice();
                    return false;
                }

                return true;
            }
            finally
            {
                Monitor.Exit(Sync);
            }
        }

        /// <summary>Whether a handle was opened since the last init (call with Sync held).</summary>
        private static bool IsLive(SDL_GameController controller)
        {
            return controller != IntPtr.Zero && liveHandles.Contains(controller);
        }

        /// <summary>
        /// Closes a controller of a removed setup device and forgets it. A handle from before the last refresh is left alone
        /// (SDL already freed it); so is one SDL is busy with, rather than block the caller.
        /// </summary>
        public static void Close(SDL_GameController controller)
        {
            if (!Monitor.TryEnter(Sync))
            {
                return;
            }

            try
            {
                SDL2DevicesList.Remove(controller);

                if (IsLive(controller))
                {
                    liveHandles.Remove(controller);
                    SDL2.GameControllerClose(controller);
                }
            }
            finally
            {
                Monitor.Exit(Sync);
            }
        }

        /// <summary>Rumbles a controller of the setup; false when it is unplugged, stale, or SDL is refreshing.</summary>
        public static bool Rumble(SDL_GameController controller, ushort low, ushort high, uint durationMs)
        {
            if (!Monitor.TryEnter(Sync))
            {
                return false;
            }

            try
            {
                return IsLive(controller) && SDL2.GameControllerGetAttached(controller) &&
                       SDL2.SDL_GameControllerRumble(controller, low, high, durationMs) == 0;
            }
            finally
            {
                Monitor.Exit(Sync);
            }
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

        /// <summary>Polls an SDL pad of the current setup (<see cref="GameProfile.Game"/>, <see cref="GameProfile.Instance"/>).</summary>
        public static bool Poll(PlayerInfo player)
        {
            return Poll(player, GameProfile.Game, GameProfile.Instance?.DevicesList);
        }

        /// <summary>
        /// Polls an SDL pad of the setup (call with <see cref="Sync"/> held). True when it reports a press: the setup flashes
        /// it, and a press on an identified pad can join. Same rules as XInput pads (<see cref="XInputManager"/>): a press on
        /// a pad not identified yet identifies it with <see cref="GamepadIdentity.Identify(PlayerInfo, IEnumerable{PlayerInfo}, IReadOnlyList{DInputCandidate}, PadHint, bool, DateTime)"/>,
        /// helped by the SDL device path and USB ids. A pad stays identified: later presses never rebind it.
        /// </summary>
        public static bool Poll(PlayerInfo player, GenericGameInfo game, IEnumerable<PlayerInfo> devices)
        {
            if (player.IsFake || !player.IsSDL2)
            {
                return false;
            }

            try
            {
                if (!IsConnected(player.SDL2Joystick) || !QueryControllerStateAny(player.SDL2Joystick))
                {
                    return false;
                }

                if (GameProfile.UseXinputIndex)
                {
                    Vibrate(player);
                    return true;//revealed with its slot GUID when it was detected
                }

                if (player.DInputJoystick != null || player.IsInputUsed)
                {
                    player.IsInputUsed = true;//identified by its path when detected (hidden until a press with "assign on button press")
                    return true;
                }

                List<DInputCandidate> candidates = new List<DInputCandidate>();

                foreach (D_Joystick joystick in DInputManager.D_JoysticksList)
                {
                    if (App_Misc.VGMOnly && !(joystick.VendorId ?? "").StartsWith("202"))
                    {
                        continue;
                    }

                    SharpDX.DirectInput.JoystickState state = joystick.State;
                    candidates.Add(new DInputCandidate(joystick, state != null && GamepadIdentity.IsPressed(state.Buttons, state.PointOfViewControllers)));
                }

                string sdlName = player.GamepadName;
                bool firstFailure = player.IdentificationFailedUtc == DateTime.MinValue;
                IdentifyResult result = GamepadIdentity.Identify(player, devices, candidates, HintOf(player.SDL2Joystick), GamepadIdentity.NeedsDInputIdentity(game), DateTime.UtcNow);

                if (!string.IsNullOrEmpty(sdlName))
                {
                    player.GamepadName = sdlName;//SDL names the model ("PS4 Controller"), DirectInput often doesn't
                }

                switch (result)
                {
                    case IdentifyResult.Matched:
                        Vibrate(player);
                        break;
                    case IdentifyResult.SlotFallback:
                        LogManager.Log($"Gamepad: SDL controller {player.GamepadId + 1} identified by its index ({candidates.Count(c => c.Pressed)} DirectInput devices pressed).");
                        Vibrate(player);
                        break;
                    default:
                        if (firstFailure)
                        {
                            LogManager.Log($"Gamepad: SDL controller {player.GamepadId + 1} pressed but not identified: {candidates.Count(c => c.Pressed)} of {candidates.Count} DirectInput devices pressed, and the handler needs the DirectInput identity.");
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

        /// <summary>What SDL knows about a controller to find its DirectInput view: path, USB ids, Xbox or not (call with Sync held).</summary>
        public static PadHint HintOf(SDL_GameController controller)
        {
            string path = PathOf(controller);
            SDL_GameControllerType type = SDL2.SDL_GameControllerGetType(controller);

            return new PadHint
            {
                Path = path,
                VendorId = SDL2.SDL_GameControllerGetVendor(controller),
                ProductId = SDL2.SDL_GameControllerGetProduct(controller),
                PreferXInputViews = GamepadIdentity.IsXInputHidPath(path) || path.StartsWith("XInput", StringComparison.OrdinalIgnoreCase) ||
                                    type == SDL_GameControllerType.SDL_CONTROLLER_TYPE_XBOX360 || type == SDL_GameControllerType.SDL_CONTROLLER_TYPE_XBOXONE,
            };
        }

        /// <summary>
        /// The device path of a controller ("" when SDL has none): HIDAPI and raw input give the HID interface path, which
        /// is also the DirectInput interface path; the XInput driver gives "XInput#n". Call with Sync held.
        /// </summary>
        public static string PathOf(SDL_GameController controller)
        {
            return controller == IntPtr.Zero ? "" : SDL2.GetString(SDL2.SDL_GameControllerPath(controller));
        }

        private static void Vibrate(PlayerInfo player)
        {
            if (!player.Vibrate)
            {
                SendRumble(player.SDL2Joystick);
                player.Vibrate = true;
            }
        }
    }
}
