using Gamepads;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;
using static SDL.SDL_ControllerUtils;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    public class DevicesFunctions
    {
        private static SynchronizationContext _syncContext;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;

        private static int testDinputPlayers = -1;// 16;
        private static int testXinputPlayers = -1;// 16;
        private static int testSDL2Players = -1;

        public static System.Threading.Timer GamepadTimer;
        private static System.Threading.Timer PollingTimer;

        internal static bool insideGamepadTick = false;

        /// <summary>
        /// Raised when a placed device (or a guest) is unplugged, before it leaves the lists: (device, index of its player
        /// in GameProfile.AssignedDevices, true for a guest).
        /// </summary>
        public static event Action<PlayerInfo, int, bool> AssignedDeviceLost;

        /// <summary>
        /// Posts an action to the thread that owns the setup state (the UI thread), without waiting. Required: set it
        /// before <see cref="Initialize"/>. The device timers run their whole tick through it, so the device list is only
        /// changed on that thread; SDL is initialized on a helper thread so its refresh never blocks the UI.
        /// </summary>
        public static Action<Action> Dispatch;

        /// <summary>Whether the setup step can proceed: every profile player matched, or at least one placed device.</summary>
        public static bool CanProceed { get; private set; }

        /// <summary>Raised after every device tick and every <see cref="UpdateDevices"/> (on the dispatcher thread).</summary>
        public static event Action DevicesUpdated;

        private static int gamepadTickQueued;
        private static int pollingTickQueued;

        //SDL lives on its own message-pumping thread, and the ticks skip SDL while it refreshes (SDLManager.Sync).
        private static SynchronizationContext sdlContext;

        public static void Initialize(UserGameInfo _userGameInfo, GameProfile _profile)
        {
            if (Dispatch == null)
            {
                throw new InvalidOperationException("DevicesFunctions.Dispatch must be set before the devices are initialized.");
            }

            //Initialize runs each time a setup is opened: never leak the previous timers.
            DisposeGamePadTimer();

            userGameInfo = _userGameInfo;
            profile = _profile;

            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            UpdateDevices();

            DInputManager.Init(_syncContext);

            //SDL only runs for the handlers that use SDL pads: its event thread polls every 16 ms (Nucleus 2.5: fewer threads).
            if (UsesSdl(userGameInfo?.Game))
            {
                if (!SDLManager.SDL_INITIALIZED)
                {
                    SynchronizationContext sdl = GetSdlContext();
                    sdl.Post(_ => SDLManager.InitSDL(sdl), null);
                }
            }
            else if (SDLManager.SDL_INITIALIZED && sdlContext != null)
            {
                sdlContext.Post(_ => SDLManager.Quit(), null);
            }

            GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 300);
            PollingTimer = new System.Threading.Timer(PollingTimer_Tick, null, 0, 150);

            GameProfile.OnUseXinputIndexChanged -= UpdateUseGamepadApiIndex;
            GameProfile.OnUseXinputIndexChanged += UpdateUseGamepadApiIndex;
        }

        /// <summary>Whether a handler gets its pads through SDL (Hook.SDL2Enabled, or Hook.SDL3Enabled since Nucleus 2.5).</summary>
        public static bool UsesSdl(GenericGameInfo game)
        {
            return game != null && (game.Hook.SDL2Enabled || game.Hook.SDL3Enabled);
        }

        /// <summary>A background STA thread with a message loop, where SDL is initialized and refreshed.</summary>
        private static SynchronizationContext GetSdlContext()
        {
            if (sdlContext != null)
            {
                return sdlContext;
            }

            using (ManualResetEventSlim ready = new ManualResetEventSlim())
            {
                Thread thread = new Thread(() =>
                {
                    WindowsFormsSynchronizationContext context = new WindowsFormsSynchronizationContext();
                    SynchronizationContext.SetSynchronizationContext(context);
                    sdlContext = context;
                    ready.Set();
                    Application.Run();
                })
                {
                    IsBackground = true,
                    Name = "Nucleus SDL"
                };

                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();
                ready.Wait();
            }

            return sdlContext;
        }

        private static void UpdateUseGamepadApiIndex()
        {
            //The device objects are rebuilt below: drop the assignments pointing to the old ones so nothing stale is launched.
            GameProfile.AssignedDevices.Clear();
            GameProfile.DevicesToMerge.Clear();
            GameProfile.TotalAssignedPlayers = 0;

            UserScreen[] screens = BoundsFunctions.Screens;

            for (int i = 0; i < screens?.Length; i++)
            {
                screens[i].PlayerOnScreen = 0;
            }

            profile?.DevicesList.Clear();
            UpdateDevices();
        }

        /// <summary>
        /// We need this if a new controller is plugged after sdl initialization
        /// else Nucleus sdl gamepad indexes will not match with the custom sdl one loaded by the game.
        /// </summary>
        /// <param name="RefreshSDL"></param>
        public static void RefreshSDL(SynchronizationContext syncContext)
        {
            //Runs on the SDL helper thread (syncContext is its context): the 1 s pause of the refresh doesn't block the UI.
            SDLManager.Refresh(syncContext);
        }

        /// <summary>Rescans SDL controllers on the SDL helper thread and returns at once.</summary>
        public static void RequestSdlRefresh()
        {
            //Not for a handler without SDL pads: SDL stays shut down (see Initialize).
            if (sdlContext != null && SDLManager.SDL_INITIALIZED)
            {
                SynchronizationContext sdl = sdlContext;
                sdl.Post(_ => RefreshSDL(sdl), null);
            }
        }

        /// <summary>
        /// We need this if a new controller is plugged after DirectInput initialization
        /// to ensure getting valid Joysticks.
        /// </summary>
        /// <param name="RefreshDInput"></param>
        public static void RefreshDInput(SynchronizationContext syncContext)
        {
            DInputManager.Refresh(syncContext);
        }

        public static void GamepadTimer_Tick(object state)
        {
            Action<Action> dispatch = Dispatch;

            //The tick changes the device list, so it runs on the dispatcher. One tick queued at most.
            if (dispatch != null && Interlocked.Exchange(ref gamepadTickQueued, 1) == 0)
            {
                dispatch(() =>
                {
                    Interlocked.Exchange(ref gamepadTickQueued, 0);
                    GamepadTick();
                });
            }
        }

        private static void GamepadTick()
        {
            if (insideGamepadTick || profile == null || GamepadTimer == null)
            {
                return;
            }

            List<PlayerInfo> data = profile.DevicesList;

            try
            {
                insideGamepadTick = true;

                bool changed = false;

                GenericGameInfo g = userGameInfo?.Game;

                if (g == null)//the handler was unloaded (hot reload of a deleted or broken script)
                {
                    insideGamepadTick = false;
                    return;
                }

                if (g.Hook.DInputEnabled || g.Hook.XInputReroute || g.ProtoInput.DinputDeviceHook)
                {
                    changed |= CheckConnectedGamepads();

                    List<D_Joystick> joysticks = DInputManager.D_JoysticksList;

                    for (int i = 0; i < joysticks.Count; i++)
                    {
                        D_Joystick joystick = joysticks[i];

                        if (App_Misc.VGMOnly)
                        {
                            if (!joystick.VendorId.ToString().StartsWith("202"))
                            {
                                continue;
                            }
                        }

                        //The DirectInput view of an XInput pad: the XInput branch lists that pad. Skip it, but don't dispose it:
                        //disposing marked the shared joystick disconnected, so the watcher refreshed every DirectInput device
                        //about once a second (placed DirectInput pads lost their slot) and XInput pads couldn't be matched.
                        if (GamepadIdentity.IsXInputHidPath(joystick.InterfacePath) && !g.Hook.XInputReroute && g.Hook.XInputEnabled)
                        {
                            continue;
                        }

                        if (CheckAssignedGamepads(i, joystick.InstanceGuid))
                        {
                            continue;
                        }

                        PlayerInfo player = new PlayerInfo();

                        player.DInputJoystick = joystick;
                        player.GamepadProductGuid = joystick.ProductGuid;
                        player.GamepadGuid = joystick.InstanceGuid;
                        player.GamepadName = joystick.InstanceName;
                        player.RawHID = joystick.InterfacePath;
                        player.DeviceKey = "dinput:" + joystick.InstanceGuid.ToString("D");

                        player.HIDDeviceID = new string[] { GamepadIdentity.HidIdOf(player.RawHID), "" };
                        player.IsInputUsed = !userGameInfo.Game.MetaInfo.ProfileAssignGamepadByButonPress;

                        player.GamepadId = i;
                        player.IsDInput = true;

                        data.Add(player);
                        changed = true;
                    }
                }

                //Using OpenXinput with more than 4 players means we can use more than 4 xinput controllers
                if ((g.Hook.XInputEnabled && !g.Hook.XInputReroute && !g.ProtoInput.DinputDeviceHook) || g.ProtoInput.XinputHook)
                {
                    changed |= CheckConnectedGamepads();

                    int numControllers = g.ProtoInput.UseOpenXinput ? 32 : 4;//TODO: Big bug here!!if g.ProtoInput.UseOpenXinput is true the ui gamepads won't appears until screen is refreshed (why?)
                    for (int i = 0; i < numControllers; i++)
                    {
                        OpenXinputController c = new OpenXinputController(true, i);//

                        if (!c.IsConnected)
                        {
                            continue;
                        }
                        else
                        {
                            if (App_Misc.VGMOnly)
                            {
                                CapabilitiesEx cap;
                                OpenXinputController.XInputGetCapabilitiesEx((uint)1, (uint)i, 1, out cap);

                                if (!cap.VendorId.ToString().StartsWith("202"))
                                {
                                    continue;
                                }
                            }

                            if (CheckAssignedGamepads(i, c))
                            {
                                continue;
                            }

                            //new gamepad
                            PlayerInfo player = new PlayerInfo
                            {
                                HIDDeviceID = new string[] { "Not required", "Not required" },
                                XInputJoystick = c,
                                GamepadId = i,
                                IsXInput = true,
                                DeviceKey = "xinput:" + i,
                            };

                            if (GameProfile.UseXinputIndex)
                            {
                                player.GamepadGuid = GamepadIdentity.SlotGuid(player.GamepadId);
                                player.IsInputUsed = true;
                            }
                            else
                            {
                                IdentifyXInputByPath(player, data, DInputManager.D_JoysticksList);
                            }

                            data.Add(player);
                            changed = true;
                        }
                    }

                    //DirectInput lists its devices on its own thread, sometimes after XInput: retry when its list changed.
                    if (!GameProfile.UseXinputIndex && OpenXinputController.DevicePathSupported)
                    {
                        List<D_Joystick> dinputDevices = DInputManager.D_JoysticksList;

                        if (dinputDevices.Count != xinputPathRetryDInputCount)
                        {
                            xinputPathRetryDInputCount = dinputDevices.Count;

                            foreach (PlayerInfo xinputPad in data.Where(d => d.IsXInput && !d.IsFake && !d.IsInputUsed && d.DInputJoystick == null).ToList())
                            {
                                changed |= IdentifyXInputByPath(xinputPad, data, dinputDevices);
                            }
                        }
                    }
                }

                if (UsesSdl(g))
                {
                    //Skip SDL while the helper thread refreshes it (the controller handles are being replaced).
                    if (Monitor.TryEnter(SDLManager.Sync))
                    {
                        try
                        {
                            changed |= CheckConnectedGamepads();

                            List<D_Joystick> dinputDevices = DInputManager.D_JoysticksList;

                            for (int i = 0; i < SDLManager.SDL2DevicesList.Count; i++)
                            {
                                SDL_GameController controller = SDLManager.SDL2DevicesList[i];

                                if (controller == IntPtr.Zero)
                                {
                                    continue;//a joystick without a game controller mapping: not a setup device
                                }

                                if (!SDLManager.IsConnected(controller))
                                {
                                    changed = true;//do not move
                                    continue;
                                }

                                if (CheckAssignedGamepads(i, controller))
                                {
                                    continue;
                                }

                                if (App_Misc.VGMOnly)
                                {
                                    int vid = SDL2.SDL_GameControllerGetVendor(controller);

                                    if (!vid.ToString().StartsWith("202"))
                                    {
                                        continue;
                                    }
                                }

                                PlayerInfo player = new PlayerInfo();

                                player.HIDDeviceID = new string[] { "Not required", "Not required" };
                                player.SDL2Joystick = controller;
                                player.GamepadId = SDL2.SDL_GameControllerGetPlayerIndex(player.SDL2Joystick);
                                player.GamepadName = SDL2.GameControllerNameString(controller);
                                player.DeviceKey = player.GamepadId >= 0 ? "sdl:" + player.GamepadId : "sdl:h" + ((IntPtr)controller).ToInt64().ToString("x");

                                //Same detection for both: only the DLL given to the game differs (SDL2.ini or SDL3.ini indexes).
                                if (g.Hook.SDL2Enabled)
                                {
                                    player.IsSDL2 = true;
                                }
                                else
                                {
                                    player.IsSDL3 = true;
                                }

                                if (GameProfile.UseXinputIndex)
                                {
                                    player.GamepadGuid = GamepadIdentity.SlotGuid(player.GamepadId);
                                    player.IsInputUsed = true;
                                }
                                else
                                {
                                    IdentifySdlByPath(player, data, dinputDevices);
                                }

                                data.Add(player);
                                changed = true;
                            }

                            //DirectInput lists its devices on its own thread, sometimes after SDL: retry the pads not identified yet.
                            if (!GameProfile.UseXinputIndex)
                            {
                                foreach (PlayerInfo sdlPad in data.Where(d => (d.IsSDL2 || d.IsSDL3) && !d.IsFake && !d.IsInputUsed && d.DInputJoystick == null).ToList())
                                {
                                    changed |= IdentifySdlByPath(sdlPad, data, dinputDevices);
                                }
                            }
                        }
                        finally
                        {
                            Monitor.Exit(SDLManager.Sync);
                        }
                    }
                }

                if (changed)
                {
                    //Already on the thread that owns the device list.
                    UpdateDevices();
                }

                insideGamepadTick = false;

                DevicesUpdated?.Invoke();
            }
            catch (Exception)
            {
                insideGamepadTick = false;
                return;
            }
        }

        public static void PollingTimer_Tick(object state)
        {
            Action<Action> dispatch = Dispatch;

            if (dispatch != null && Interlocked.Exchange(ref pollingTickQueued, 1) == 0)
            {
                dispatch(() =>
                {
                    Interlocked.Exchange(ref pollingTickQueued, 0);

                    if (PollingTimer != null)
                    {
                        PollingTick();
                    }
                });
            }
        }

        private static void PollingTick()
        {
            bool sdlLocked = false;

            try
            {
                for (int i = 0; i < profile?.DevicesList.Count; i++)
                {
                    PlayerInfo player = profile?.DevicesList[i];

                    if(player == null) { continue;}

                    if (!player.IsController){ continue; }

                    //A press the poll sees: which buttons it holds (the new ones raise PlayerInfo.ButtonsPressed), then the flash.
                    bool polled = true;
                    bool pressed = false;
                    PadButtons held = PadButtons.None;

                    switch (player.InputType)
                    {
                        case InputType.DInput:
                            pressed = DInputManager.Poll(player);
                            if (pressed) { held = DInputManager.ReadButtons(player); }
                            break;
                        case InputType.XInput:
                            pressed = XInputManager.Poll(player, userGameInfo?.Game, profile?.DevicesList);
                            if (pressed) { held = XInputManager.ReadButtons(player); }
                            break;
                        case InputType.SDL2:
                        case InputType.SDL3:
                            if (!sdlLocked)
                            {
                                if (!Monitor.TryEnter(SDLManager.Sync))
                                {
                                    polled = false;
                                    break;//SDL is refreshing
                                }

                                sdlLocked = true;
                            }

                            pressed = SDLManager.Poll(player, userGameInfo?.Game, profile?.DevicesList);
                            if (pressed) { held = SDLManager.ReadButtons(player); }
                            break;
                        default:
                            polled = false;
                            break;
                    }

                    if (polled)
                    {
                        player.TrackButtons(held);
                    }

                    if (pressed)
                    {
                        player.FlashIcon();
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
            finally
            {
                if (sdlLocked)
                {
                    Monitor.Exit(SDLManager.Sync);
                }
            }
        }

        /// <summary>
        /// Identifies an SDL pad by its device path, which is also its DirectInput interface path (SDL's HIDAPI and raw
        /// input drivers): exact, so no button press is needed. Like a DirectInput pad, it is shown at once, or at its first
        /// press with "assign gamepads on button press". Call with SDLManager.Sync held.
        /// </summary>
        private static bool IdentifySdlByPath(PlayerInfo player, List<PlayerInfo> devices, List<D_Joystick> dinputDevices)
        {
            string path = SDLManager.PathOf(player.SDL2Joystick);
            D_Joystick joystick = GamepadIdentity.FindByPath(player, devices, dinputDevices, path);

            if (joystick == null)
            {
                return false;
            }

            string sdlName = player.GamepadName;
            GamepadIdentity.Bind(player, joystick);

            if (!string.IsNullOrEmpty(sdlName))
            {
                player.GamepadName = sdlName;
            }

            player.IsInputUsed = !userGameInfo.Game.MetaInfo.ProfileAssignGamepadByButonPress;
            player.IdentificationFailedUtc = DateTime.MinValue;
            return true;
        }

        //DirectInput device count when unidentified XInput pads last tried their device path.
        private static int xinputPathRetryDInputCount = -1;

        /// <summary>
        /// Identifies an XInput pad by the device path the custom openxinput1_3.dll gives for its slot (Nucleus 2.5): it
        /// gets the identity of its DirectInput device at detection, so no button press is needed and a saved profile can
        /// match it by its GUID at once. Like a DirectInput pad, it is shown at once, or at its first press with "assign
        /// gamepads on button press". Two identical pads can't be told apart this way: they still need a press.
        /// </summary>
        private static bool IdentifyXInputByPath(PlayerInfo player, List<PlayerInfo> devices, List<D_Joystick> dinputDevices)
        {
            if (!OpenXinputController.TryGetDevicePath(player.GamepadId, out string path))
            {
                return false;
            }

            D_Joystick joystick = GamepadIdentity.FindByXInputPath(player, devices, dinputDevices, path);

            if (joystick == null)
            {
                return false;
            }

            GamepadIdentity.Bind(player, joystick);
            player.IsInputUsed = !userGameInfo.Game.MetaInfo.ProfileAssignGamepadByButonPress;
            player.IdentificationFailedUtc = DateTime.MinValue;
            LogManager.Log($"Gamepad: XInput slot {player.GamepadId + 1} identified by its device path ({player.HIDDeviceID[0]}).");
            return true;
        }

        private static bool CheckConnectedGamepads()
        {
            bool changed = false;

            //A copy: DeletePlayer removes from the list (iterating the list itself skipped the device after a deleted one).
            List<PlayerInfo> devices = profile.DevicesList.ToList();
            List<D_Joystick> joysticks = null;

            for (int j = 0; j < devices.Count; j++)
            {
                PlayerInfo player = devices[j];

                if (!player.IsController)
                {
                    continue;
                }

                if (player.IsFake)
                {
                    return true;
                }

                switch (player.InputType)
                {
                    case InputType.DInput:

                        bool foundGamepad = false;
                        joysticks = joysticks ?? DInputManager.D_JoysticksList;

                        for (int i = 0; i < joysticks.Count; i++)
                        {
                            if (joysticks[i].InstanceGuid == player.GamepadGuid && i == player.GamepadId)
                            {
                                foundGamepad = true;
                                break;
                            }
                        }

                        if (player.DInputJoystick.Status == JoystickStatus.Disconnected)
                        {
                            DeletePlayer(player);
                            foundGamepad = false;
                        }

                        if (!foundGamepad)
                        {
                            changed = true;
                        }

                        break;
                    case InputType.XInput:

                        if (!IsXInputConnected(player))
                        {
                            DeletePlayer(player);
                            changed = true;
                        }

                        break;
                    case InputType.SDL2:
                    case InputType.SDL3:

                        if (!SDLManager.IsConnected(player.SDL2Joystick))
                        {
                            DeletePlayer(player);
                            changed = true;
                        }

                        break;
                }
            }

            return changed;
        }

        /// <summary>
        /// Whether an XInput pad is still connected, asked through the same API that detected it (OpenXinput). The check used
        /// the system xinput1_3.dll when the handler doesn't use OpenXinput: its slots can differ from OpenXinput's (the pad was
        /// deleted and detected again every tick, so it could never be identified or placed), and without the DirectX runtime
        /// the DLL is missing (every tick threw before detecting new pads).
        /// </summary>
        private static bool IsXInputConnected(PlayerInfo player)
        {
            try
            {
                OpenXinputController controller = player.XInputJoystick ?? new OpenXinputController(true, player.GamepadId);
                return controller.IsConnected;
            }
            catch (Exception)
            {
                return true;//can't tell: keep the pad (the detection loop runs on the same API anyway)
            }
        }

        private static bool CheckAssignedGamepads(int index, object devicedata)
        {
            bool assigned = false;

            for (int j = 0; j < profile.DevicesList.Count; j++)
            {
                PlayerInfo player = profile.DevicesList[j];

                if(!player.IsController)
                {
                    continue;
                }

                if(player.IsFake)
                {
                    return true;
                }

                //Each API asks with its own device data (DirectInput: the instance GUID, XInput: the controller, SDL: the
                //handle), and the list holds the pads of every API of the handler: compare only like with like. A DirectInput
                //GUID parsed against an XInput pad, or an SDL cast of a GUID, threw and aborted the tick (Nucleus 2.5 fix).
                switch (player.InputType)
                {
                    case InputType.DInput:

                        if (devicedata is Guid instanceGuid && instanceGuid != Guid.Empty && player.GamepadGuid == instanceGuid)
                        {
                            assigned = true;
                        }

                        break;
                    case InputType.XInput:

                        if (player.GamepadId == index && devicedata is OpenXinputController oxc)
                        {
                            var s = oxc.GetState();

                            int newmask = (int)s.Gamepad.Buttons;

                            if (player.GamepadMask != newmask)
                            {
                                player.GamepadMask = newmask;
                            }

                            assigned = true;
                        }

                        break;
                    case InputType.SDL2:
                    case InputType.SDL3:

                        if (!(devicedata is SDL_GameController))
                        {
                            break;
                        }

                        SDL_GameController controller = (SDL_GameController)devicedata;
                        if (player.SDL2Joystick == (IntPtr)controller)
                        {
                            assigned = true;
                        }

                        break;
                }

                if (assigned)
                {
                    break;
                }
            }

            return assigned;
        }

        private static void DeletePlayer(PlayerInfo player)
        {
            if (player.SDL2Joystick != IntPtr.Zero)
            {
                SDLManager.Close(player.SDL2Joystick);//not a handle SDL freed in a refresh
            }

            if (GameProfile.AssignedDevices.Contains(player))
            {
                AssignedDeviceLost?.Invoke(player, GameProfile.AssignedDevices.IndexOf(player), false);

                UserScreen[] screens = BoundsFunctions.Screens;

                if (screens != null && player.ScreenIndex >= 0 && player.ScreenIndex < screens.Length)
                {
                    screens[player.ScreenIndex].PlayerOnScreen--;
                }

                GameProfile.TotalAssignedPlayers--;
                GameProfile.AssignedDevices.Remove(player);
            }

            var isGuestOf = GameProfile.AssignedDevices.Where(ad => ad.InstanceGuests.Contains(player)).FirstOrDefault();

            if (isGuestOf != null)
            {
                AssignedDeviceLost?.Invoke(player, GameProfile.AssignedDevices.IndexOf(isGuestOf), true);

                isGuestOf.InstanceGuests.Remove(player);
            }

            profile.DevicesList.Remove(player);
        }

        public static void DisposeGamePadTimer()
        {
            GamepadTimer?.Dispose();
            GamepadTimer = null;
            PollingTimer?.Dispose();
            PollingTimer = null;
        }

        /// <summary>Whether the device timers are running (between Initialize and DisposeGamePadTimer).</summary>
        public static bool IsRunning => GamepadTimer != null;

        /// <summary>Adds the keyboard devices if missing, recomputes <see cref="CanProceed"/> and the tray layout, notifies.</summary>
        public static void UpdateDevices()
        {
            if (profile == null)
            {
                return;
            }

            GenericGameInfo g = userGameInfo?.Game;

            if (g == null)//the handler was unloaded (hot reload of a deleted or broken script)
            {
                return;
            }

            List<PlayerInfo> playerData = profile.DevicesList;

            bool canProceed;

            if (GameProfile.Loaded)
            {
                canProceed = (GameProfile.TotalAssignedPlayers == GameProfile.TotalProfilePlayers);
            }
            else
            {
                canProceed = playerData.Count(c => c.ScreenIndex != -1) >= 1;
            }

            CanProceed = canProceed;

            if (playerData.Count == 0 || playerData.All(pl => !pl.IsRawKeyboard && !pl.IsRawMouse && !pl.IsKeyboardPlayer))
            {
                if (userGameInfo.Game.SupportsKeyboard)
                {
                    ///add keyboard data
                    PlayerInfo kbPlayer = new PlayerInfo
                    {
                        IsKeyboardPlayer = true,
                        GamepadId = 99,
                        IsInputUsed = true,
                        HIDDeviceID = new string[] { "Not required", "Not required" },
                        GamepadGuid = new Guid("10000000-1000-1000-1000-100000000000"),
                        DeviceKey = "kb:single",
                    };

                    playerData.Add(kbPlayer);
                }

                if (userGameInfo.Game.SupportsMultipleKeyboardsAndMice)///Raw mice/keyboards
                {
                    playerData.AddRange(RawInputManager.GetDeviceInputInfos());
                }

                if (testDinputPlayers != -1)///make fake data if needed
                {
                    for (int i = 0; i < testDinputPlayers; i++)
                    {
                        if (i >= userGameInfo.Game.MaxPlayers)
                        {
                            break;
                        }
                        ///new gamepad
                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "Player",
                            IsDInput = true,
                            IsFake = true,
                            IsInputUsed = true,
                            HIDDeviceID = new string[] { "Not required", "Not required" },
                            DeviceKey = "fake:dinput:" + i,
                        };

                        playerData.Add(player);
                    }
                }

                if (testXinputPlayers != -1)
                {
                    for (int i = 0; i < testXinputPlayers; i++)
                    {
                        if (i >= userGameInfo.Game.MaxPlayers)
                        {
                            break;
                        }

                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "XPlayer",
                            IsXInput = true,
                            IsController = true,
                            GamepadId = i,
                            IsFake = true,
                            IsInputUsed = true,
                            HIDDeviceID = new string[] { "Not required", "Not required" },
                            DeviceKey = "fake:xinput:" + i,
                        };

                        playerData.Add(player);
                    }
                }

                if (testSDL2Players != -1)
                {
                    for (int i = 0; i < testSDL2Players; i++)
                    {

                        if(i >= userGameInfo.Game.MaxPlayers)
                        {
                            break;
                        }

                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "SDL2" + i,
                            IsSDL2 = true,
                            IsController = true,
                            GamepadId = i,
                            IsFake = true,
                            IsInputUsed = true,
                            HIDDeviceID = new string[] { "Not required", "Not required" },
                            DeviceKey = "fake:sdl:" + i,
                        };

                        playerData.Add(player);
                    }
                }
            }

            BoundsFunctions.SetPlayerArea();

            DevicesUpdated?.Invoke();
        }
    }
}
