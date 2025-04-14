using Gamepads;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Windows.Interop;
using SDL;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Markup;
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

        public static System.Threading.Timer GamepadTimer;
        private static System.Threading.Timer PollingTimer;

        internal static float playerSize;
        internal static RectangleF playersArea;

        internal static bool insideGamepadTick = false;
        public static bool isDisconnected;
        private static bool vgmDevicesOnly = false;
        public static Action<SynchronizationContext, string> OnAssignedDeviceDisconnect;

        private static bool useGamepadApiIndex;
        public static bool UseGamepadApiIndex
        {
            get => useGamepadApiIndex;
            set
            {
                useGamepadApiIndex = value;

                if (profile != null)
                {
                    profile.Reset();
                    profile.DevicesList.Clear();
                }
            }
        }

        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;
            profile = _profile;
            useGamepadApiIndex = App_Misc.UseXinputIndex;

            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            UpdateDevices();
            DInputManager.Init(_syncContext);
            SDLManager.InitSDL(_syncContext);

            GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 300);
            PollingTimer = new System.Threading.Timer(PollingTimer_Tick, null, 0, 150);
        }

        /// <summary>
        /// We need this if a new controller is plugged after sdl initialization
        /// else Nucleus sdl gamepad indexes will not match with the custom sdl one loaded by the game.
        /// </summary>
        /// <param name="RefreshSDL"></param>
        public static void RefreshSDL(SynchronizationContext syncContext)
        {
            SDLManager.Refresh(syncContext);
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
            if (insideGamepadTick || profile == null)
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

                        if (CheckAssignedGamepads(i, joystick.InstanceGuid))
                        {
                            continue;
                        }

                        PlayerInfo player = new PlayerInfo();

                        if (vgmDevicesOnly)
                        {
                            if (!joystick.VendorId.ToString().StartsWith("202"))
                            {
                                joystick.Dispose();
                                continue;
                            }
                        }

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

                        int start = player.RawHID.IndexOf("hid#");
                        int end = player.RawHID.LastIndexOf("#{");
                        string fhid = player.RawHID.Substring(start, end - start).Replace('#', '\\').ToUpper();

                        player.HIDDeviceID = new string[] { fhid, "" };

                        player.IsInputUsed = true;
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
                        OpenXinputController c = new OpenXinputController(g.ProtoInput.UseOpenXinput, i);

                        if (!c.IsConnected)
                        {
                            continue;
                        }
                        else
                        {
                            if (CheckAssignedGamepads(i, c))
                            {
                                continue;
                            }

                            if (vgmDevicesOnly)
                            {
                                CapabilitiesEx cap;
                                var _cap = OpenXinputController.XInputGetCapabilitiesEx((uint)1, (uint)i, 1, out cap);

                                if (!cap.VendorId.ToString().StartsWith("202"))
                                {
                                    continue;
                                }
                            }

                            //new gamepad
                            PlayerInfo player = new PlayerInfo
                            {
                                HIDDeviceID = new string[] { "Not required", "Not required" },
                                XInputJoystick = c,
                                GamepadId = i,
                                IsXInput = true,
                            };

                            if (useGamepadApiIndex)
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

                        if (vgmDevicesOnly)
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

                        if (useGamepadApiIndex)
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

                if (changed)
                {
                    if (parent.InvokeRequired)
                    {
                        parent.Invoke(new MethodInvoker(UpdateDevices));
                        insideGamepadTick = false;
                        return;
                    }
                }

                insideGamepadTick = false;

                if (parent.IsHandleCreated)
                {
                    parent?.Invoke(new Action(() => parent?.Invalidate(false)));
                }
            }
            catch (Exception)
            {
                insideGamepadTick = false;
                return;
            }
        }

        public static void PollingTimer_Tick(object state)
        {
            try
            {
                for (int i = 0; i < profile?.DevicesList.Count; i++)
                {
                    PlayerInfo player = profile?.DevicesList[i];

                    switch (player.InputType)
                    {
                        case InputType.DInput:
                            if (DInputManager.Poll(player)) { player.FlashIcon(); }
                            break;
                        case InputType.XInput:
                            if (XInputManager.Poll(player)) { player.FlashIcon(); }
                            break;
                        case InputType.SDL2:
                            if (SDLManager.Poll(player)) { player.FlashIcon(); }
                            break;
                    }
                }
            }
            catch (Exception)
            {
                return;
            }
        }

        private static bool CheckConnectedGamepads()
        {
            bool changed = false;

            for (int j = 0; j < profile.DevicesList.Count; j++)
            {
                PlayerInfo player = profile.DevicesList[j];

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
                BoundsFunctions.Screens[player.ScreenIndex].PlayerOnScreen--;
                GameProfile.TotalAssignedPlayers--;
                GameProfile.AssignedDevices.Remove(player);
                OnAssignedDeviceDisconnect?.Invoke(_syncContext, lostData);
            }

            profile.DevicesList.Remove(player);
        }

        public static void DisposeGamePadTimer()
        {
            GamepadTimer?.Dispose();
            GamepadTimer = null;
            PollingTimer?.Dispose();
        }

        internal static void UpdateDevices()
        {
            if (profile == null)
            {
                return;
            }

            GenericGameInfo g = userGameInfo.Game;
            List<PlayerInfo> playerData = profile.DevicesList;

            if (GameProfile.Loaded)
            {
                parent.canProceed = (GameProfile.TotalAssignedPlayers == GameProfile.TotalProfilePlayers);
            }
            else
            {
                parent.canProceed = playerData.Count(c => c.ScreenIndex != -1) >= 1;
            }

            if (playerData.Count == 0)
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
                        GamepadGuid = new Guid("10000000-1000-1000-1000-100000000000")
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
                        ///new gamepad
                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "Player",
                            IsDInput = true,
                            IsFake = true,
                            IsInputUsed = true,
                            HIDDeviceID = new string[] { "Not required", "Not required" }
                        };

                        playerData.Add(player);
                    }
                }

                if (testXinputPlayers != -1)
                {
                    for (int i = 0; i < testXinputPlayers; i++)
                    {
                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "XPlayer",
                            IsXInput = true,
                            IsController = true,
                            GamepadId = i,
                            IsFake = true,
                            IsInputUsed = true,
                            HIDDeviceID = new string[] { "Not required", "Not required" }
                        };

                        playerData.Add(player);
                    }
                }
            }

            BoundsFunctions.UpdateScreens();

            float playersWidth = parent.Width * 0.65f;

            float playerCount = playerData.Count;
            float playerWidth = playersWidth * 0.9f;
            float playerHeight = parent.Height * 0.2f;

            playersArea = new RectangleF(10, 0, playersWidth, playerHeight);

            float playersAreaScale = playersArea.Width * playersArea.Height;
            float maxArea = playersAreaScale / playerCount;
            playerSize = (float)(Math.Sqrt(maxArea) - 0.5F);///force the round down
                                                            ///see if the size can fit it or we need to make some further adjustments
            float horizontal = (playersWidth / playerSize) - 0.5F;
            float vertical = (int)Math.Round((playerHeight / playerSize) - 0.5);
            float total = vertical * horizontal;

            if (total < playerCount)
            {
                float newVertical = vertical + 1;
                Draw.PlayerCustomFont = new Font(Draw.PlayerCustomFont.FontFamily, Draw.PlayerCustomFont.Size * 0.8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                playerSize = (int)Math.Round(((playerHeight / 1.2f) / newVertical));
            }

            //var sortedList = playerData.OrderBy(p => p.InputType).ToList();

            for (int i = 0; i < playerData.Count; i++)
            {
                PlayerInfo info = playerData[i];

                if (info.ScreenIndex == -1)
                {
                    info.EditBounds = BoundsFunctions.GetDefaultBounds(i);
                    info.SourceEditBounds = info.EditBounds;
                    info.DisplayIndex = -1;
                }
            }

            parent.CanPlayUpdated(parent.canProceed, false);
            parent.Invalidate(false);
        }
    }
}
