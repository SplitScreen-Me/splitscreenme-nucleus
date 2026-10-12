using Gamepads;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;
using static SDL.SDL_ControllerUtils;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    //This should not rely on UI at all in the future
    public class DevicesFunctions
    {
        private static SynchronizationContext _syncContext;
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;

        private static int testDinputPlayers = -1;// 16;
        private static int testXinputPlayers = -1;// 16;
        private static int testSDL2Players = -1;

        public static System.Threading.Timer GamepadTimer;
        private static System.Threading.Timer PollingTimer;

        internal static float playerSize;

        internal static bool insideGamepadTick = false;
        public static bool isDisconnected;

        public static Action<SynchronizationContext, string> OnAssignedDeviceDisconnect;

        /// <summary>
        /// Raised when a placed device (or a guest) is unplugged, before it leaves the lists: (device, index of its player
        /// in GameProfile.AssignedDevices, true for a guest).
        /// </summary>
        public static event Action<PlayerInfo, int, bool> AssignedDeviceLost;

        /// <summary>
        /// Headless hosts (no WinForms setup screen): posts an action to the thread that owns the setup state (the UI
        /// thread), without waiting. When set, the device timers run their whole tick through it, so the device list is
        /// only changed on that thread, and SDL is initialized on a helper thread so its refresh never blocks the UI.
        /// Null (default) keeps the WinForms behaviour: ticks on timer threads, Control.Invoke to the setup screen.
        /// </summary>
        public static Action<Action> Dispatch;

        /// <summary>Whether the setup step can proceed: every profile player matched, or at least one placed device.</summary>
        public static bool CanProceed { get; private set; }

        /// <summary>Raised after every device tick and every <see cref="UpdateDevices"/> (on the dispatcher thread when <see cref="Dispatch"/> is set).</summary>
        public static event Action DevicesUpdated;

        private static int gamepadTickQueued;
        private static int pollingTickQueued;

        //Headless mode only: SDL lives on its own message-pumping thread, and the ticks skip SDL while it refreshes.
        private static readonly object sdlSync = new object();
        private static SynchronizationContext sdlContext;

        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            //Initialize can run several times (each time the setup screen is shown): never leak the previous timers.
            DisposeGamePadTimer();

            parent = _parent;
            userGameInfo = _userGameInfo;
            profile = _profile;

            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            UpdateDevices();

            DInputManager.Init(_syncContext);

            if (Dispatch != null)
            {
                if (!SDLManager.SDL_INITIALIZED)
                {
                    SynchronizationContext sdl = GetSdlContext();
                    sdl.Post(_ => { lock (sdlSync) { SDLManager.InitSDL(sdl); } }, null);
                }
            }
            else
            {
                SDLManager.InitSDL(_syncContext);
            }

            GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 300);
            PollingTimer = new System.Threading.Timer(PollingTimer_Tick, null, 0, 150);

            GameProfile.OnUseXinputIndexChanged -= UpdateUseGamepadApiIndex;
            GameProfile.OnUseXinputIndexChanged += UpdateUseGamepadApiIndex;
        }

        /// <summary>A background STA thread with a message loop, where SDL is initialized and refreshed in headless mode.</summary>
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
            if (Dispatch != null)
            {
                //Runs on the SDL helper thread (syncContext is its context): the 1 s pause of the refresh doesn't block the UI.
                lock (sdlSync)
                {
                    SDLManager.Refresh(syncContext);
                }

                return;
            }

            SDLManager.Refresh(syncContext);
        }

        /// <summary>
        /// Rescans SDL controllers. Headless mode: runs on the SDL helper thread and returns at once. WinForms mode: runs here.
        /// </summary>
        public static void RequestSdlRefresh()
        {
            if (Dispatch != null)
            {
                if (sdlContext != null)
                {
                    SynchronizationContext sdl = sdlContext;
                    sdl.Post(_ => RefreshSDL(sdl), null);
                }

                return;
            }

            if (SDLManager.SDL_INITIALIZED)
            {
                RefreshSDL(_syncContext);
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

            if (dispatch != null)
            {
                //Headless: the tick changes the device list, so it runs on the dispatcher. One tick queued at most.
                if (Interlocked.Exchange(ref gamepadTickQueued, 1) == 0)
                {
                    dispatch(() =>
                    {
                        Interlocked.Exchange(ref gamepadTickQueued, 0);
                        GamepadTick();
                    });
                }

                return;
            }

            GamepadTick();
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

                GenericGameInfo g = userGameInfo.Game;

                if (g.Hook.DInputEnabled || g.Hook.XInputReroute || g.ProtoInput.DinputDeviceHook)
                {
                    changed = CheckConnectedGamepads();

                    for (int i = 0; i < DInputManager.D_JoysticksList.Count; i++)
                    {
                        D_Joystick joystick = DInputManager.D_JoysticksList[i];

                        if (App_Misc.VGMOnly)
                        {
                            if (!joystick.VendorId.ToString().StartsWith("202"))
                            {
                                continue;
                            }
                        }

                        if (CheckAssignedGamepads(i, joystick.InstanceGuid))
                        {
                            continue;
                        }

                        PlayerInfo player = new PlayerInfo();

                        if (joystick.InterfacePath.ToUpper().Contains("IG_") && !g.Hook.XInputReroute && g.Hook.XInputEnabled)
                        {
                            joystick.Dispose();
                            continue;
                        }

                        player.DInputJoystick = joystick;
                        player.GamepadProductGuid = joystick.ProductGuid;
                        player.GamepadGuid = joystick.InstanceGuid;
                        player.GamepadName = joystick.InstanceName;
                        player.RawHID = joystick.InterfacePath;
                        player.DeviceKey = "dinput:" + joystick.InstanceGuid.ToString("D");

                        int start = player.RawHID.IndexOf("hid#");
                        int end = player.RawHID.LastIndexOf("#{");
                        string fhid = player.RawHID.Substring(start, end - start).Replace('#', '\\').ToUpper();

                        player.HIDDeviceID = new string[] { fhid, "" };
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
                    changed = CheckConnectedGamepads();

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
                                string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                                player.GamepadGuid = new Guid(guid);
                                player.IsInputUsed = true;
                            }

                            data.Add(player);
                            changed = true;
                        }
                    }
                }

                if (g.Hook.SDL2Enabled)
                {
                    //Headless: skip SDL while the helper thread refreshes it (the controller handles are being replaced).
                    bool sdlLocked = Dispatch != null && !Monitor.TryEnter(sdlSync);

                    if (!sdlLocked)
                    {
                        try
                        {
                            changed = CheckConnectedGamepads();

                            for (int i = 0; i < SDLManager.SDL2DevicesList.Count; i++)
                            {
                                SDL_GameController controller = SDLManager.SDL2DevicesList[i];

                                if (controller == IntPtr.Zero || !SDLManager.IsConnected(controller))
                                {
                                    changed = true;//do not move
                                    continue;
                                }

                                if (CheckAssignedGamepads(i, controller))
                                {
                                    continue;
                                }

                                SDL_DeviceInfo devInfo = GetSDL_DeviceInfo(controller);

                                if (App_Misc.VGMOnly)
                                {
                                    int vid = SDL2.JoystickGetDeviceVendor(i);

                                    if (!vid.ToString().StartsWith("202"))
                                    {
                                        continue;
                                    }
                                }

                                PlayerInfo player = new PlayerInfo();

                                player.HIDDeviceID = new string[] { "Not required", "Not required" };
                                player.SDL2Joystick = controller;
                                player.GamepadId = SDL2.SDL_GameControllerGetPlayerIndex(player.SDL2Joystick);
                                player.DeviceKey = player.GamepadId >= 0 ? "sdl:" + player.GamepadId : "sdl:h" + ((IntPtr)controller).ToInt64().ToString("x");

                                if (GameProfile.UseXinputIndex)
                                {
                                    string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                                    player.GamepadGuid = new Guid(guid);
                                    player.IsInputUsed = true;
                                }

                                player.IsSDL2 = true;
                                data.Add(player);
                                changed = true;
                            }
                        }
                        finally
                        {
                            if (Dispatch != null)
                            {
                                Monitor.Exit(sdlSync);
                            }
                        }
                    }
                }

                if (changed)
                {
                    if (Dispatch != null || parent == null)
                    {
                        //Headless: already on the thread that owns the device list.
                        UpdateDevices();
                    }
                    else if (parent.InvokeRequired)
                    {
                        parent.Invoke(new MethodInvoker(UpdateDevices));
                        insideGamepadTick = false;
                        return;
                    }
                }

                insideGamepadTick = false;

                if (parent != null && parent.IsHandleCreated)
                {
                    parent?.Invoke(new Action(() => parent?.Invalidate(false)));
                }

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

            if (dispatch != null)
            {
                if (Interlocked.Exchange(ref pollingTickQueued, 1) == 0)
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

                return;
            }

            PollingTick();
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

                    switch (player.InputType)
                    {
                        case InputType.DInput:
                            if (DInputManager.Poll(player)) { player.FlashIcon(); }
                            break;
                        case InputType.XInput:
                            if (XInputManager.Poll(player)) { player.FlashIcon(); }
                            break;
                        case InputType.SDL2:
                            if (Dispatch != null && !sdlLocked)
                            {
                                if (!Monitor.TryEnter(sdlSync))
                                {
                                    break;//SDL is refreshing
                                }

                                sdlLocked = true;
                            }

                            if (SDLManager.Poll(player)) { player.FlashIcon(); }
                            break;
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
                    Monitor.Exit(sdlSync);
                }
            }
        }

        private static bool CheckConnectedGamepads()
        {
            bool changed = false;

            for (int j = 0; j < profile.DevicesList.Count; j++)
            {
                PlayerInfo player = profile.DevicesList[j];

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

                        for (int i = 0; i < DInputManager.D_JoysticksList.Count; i++)
                        {
                            if (DInputManager.D_JoysticksList[i].InstanceGuid == player.GamepadGuid && i == player.GamepadId)
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

                        OpenXinputController c = new OpenXinputController(userGameInfo.Game.ProtoInput.UseOpenXinput, player.GamepadId);

                        if (!c.IsConnected)
                        {
                            DeletePlayer(player);
                            changed = true;
                        }

                        break;
                    case InputType.SDL2:

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

                switch (player.InputType)
                {
                    case InputType.DInput:

                        var instanceGuid = devicedata.ToString();

                        if (player.GamepadGuid == Guid.Parse(instanceGuid))
                        {
                            assigned = true;
                        }

                        break;
                    case InputType.XInput:

                        if (player.GamepadId == index)
                        {
                            OpenXinputController oxc = devicedata as OpenXinputController;
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
            string lostData = $"{player.Nickname}|{player.InputType.ToString()}";

            if (player.SDL2Joystick != IntPtr.Zero)
            {
                SDL2.GameControllerClose(player.SDL2Joystick);
                SDLManager.SDL2DevicesList.Remove(player.SDL2Joystick);
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
                OnAssignedDeviceDisconnect?.Invoke(_syncContext, lostData);
            }

            var isGuestOf = GameProfile.AssignedDevices.Where(ad => ad.InstanceGuests.Contains(player)).FirstOrDefault();

            if (isGuestOf != null)
            {
                AssignedDeviceLost?.Invoke(player, GameProfile.AssignedDevices.IndexOf(isGuestOf), true);

                lostData =$"{isGuestOf.Nickname} guest {player.GamepadId}|{player.InputType.ToString()}";
                isGuestOf.InstanceGuests.Remove(player);
                OnAssignedDeviceDisconnect?.Invoke(_syncContext, lostData);
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

            GenericGameInfo g = userGameInfo.Game;
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

            if (parent != null)
            {
                parent.canProceed = canProceed;
            }

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

            if (parent != null && parent.IsHandleCreated)
            {
                parent?.Invoke((MethodInvoker)delegate ()
                {
                    parent.CanPlayUpdated(parent.canProceed, false);
                    parent.Invalidate(false);
                });
            }

            DevicesUpdated?.Invoke();
        }
    }
}
