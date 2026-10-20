using Gamepads;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;
using static SDL.SDL_ControllerUtils;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    //This should not rely on UI at all in the future
    public class DevicesFunctions
    {     
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;

        private static int testDinputPlayers = -1;// 16;
        private static int testXinputPlayers = -1;// 16;
        private static int testSDL2Players = -1;
        private static int testSDL3Players = -1;

        public static System.Threading.Timer GamepadTimer;
        private static System.Threading.Timer PollingTimer;

        internal static float playerSize;
       
        internal static bool insideGamepadTick = false;
        public static bool isDisconnected;

        public static Action<string> OnAssignedDeviceDisconnect;

        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;
            profile = _profile;

            if (userGameInfo != null)
            {
                UpdateDevices();

                UIThread.Post(() =>
                {
                    DInputManager.Init();

                    if (SDLManager.SDL_INITIALIZED && !userGameInfo.HandlerInfo.GenericGameInfo.Hook.SDL2Enabled && !userGameInfo.HandlerInfo.GenericGameInfo.Hook.SDL3Enabled)
                    {
                        SDLManager.QUIT_SDL();
                    }
                    else if (!SDLManager.SDL_INITIALIZED && userGameInfo.HandlerInfo.GenericGameInfo.Hook.SDL2Enabled || userGameInfo.HandlerInfo.GenericGameInfo.Hook.SDL3Enabled)
                    {
                        SDLManager.InitSDL();
                    }
                });


                GamepadTimer?.Dispose();
                GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 150);

                PollingTimer?.Dispose();
                PollingTimer = new System.Threading.Timer(PollingTimer_Tick, null, 0, 60);
            }

            GameProfile.OnUseXinputIndexAssignMethodChanged -= UpdateUseGamepadApiIndex;
            GameProfile.OnUseXinputIndexAssignMethodChanged += UpdateUseGamepadApiIndex;
        }

        private static void UpdateUseGamepadApiIndex()
        {
            parent?.CanPlayUpdated(false, false);
            profile?.DevicesList.Clear();
            UpdateDevices();
            GamepadTimer?.Dispose();
            GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 150);

            GameProfile.Instance?.Reset();
        }

        public static void GamepadTimer_Tick(object state)
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

                GenericGameInfo g = userGameInfo.HandlerInfo.GenericGameInfo;

                if (g == null)
                {
                    insideGamepadTick = false;
                    return;
                }

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
                            continue;
                        }

                        player.DInputJoystick = joystick;
                        player.GamepadProductGuid = joystick.ProductGuid;
                        player.GamepadGuid = joystick.InstanceGuid;
                        player.GamepadName = joystick.InstanceName;
                        player.RawHID = joystick.InterfacePath;

                        int start = player.RawHID.IndexOf("hid#");

                        if (start == -1)
                        {
                            start = player.RawHID.IndexOf("usb#");//common to all emulated gamepads?
                        }

                        int end = player.RawHID.LastIndexOf("#{");

                        if (start == -1 || end == -1)
                        {
                            continue;
                        }

                        string fhid = player.RawHID.Substring(start, end - start).Replace('#', '\\').ToUpper();

                        player.HIDDeviceID = new string[] { fhid, "" };
                        player.IsInputUsed = !userGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.ProfileAssignGamepadByButonPress;

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
                        OpenXinputController c = new OpenXinputController(true, (uint)i);//

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

                            PlayerInfo player = new PlayerInfo();

                            if (!GameProfile.UseXinputIndex)
                            {
                                StringBuilder path = new StringBuilder(1024);

                                uint result = OpenXinputController.OpenXInputGetDevicePath((uint)i, path, (uint)path.Capacity);

                                if (result == 0) // ERROR_SUCCESS
                                {
                                    string x_devicePath = path.ToString();
                                    int start = x_devicePath.IndexOf("hid#");

                                    if (start == -1)
                                    {
                                        start = x_devicePath.IndexOf("usb#");//common to all emulated gamepads?
                                    }

                                    int end = x_devicePath.LastIndexOf("#{");

                                    if (start == -1 || end == -1)
                                    {
                                        continue;
                                    }

                                    x_devicePath = x_devicePath.Substring(start, end - start).Replace('#', '\\').ToUpper();

                                    string d_devicePath = string.Empty;
                                    D_Joystick match = null;

                                    for (int j = 0; j < DInputManager.D_JoysticksList.Count; j++)
                                    {
                                        D_Joystick dj = DInputManager.D_JoysticksList[j];

                                        start = dj.InterfacePath.IndexOf("hid#");

                                        if (start == -1)
                                        {
                                            start = dj.InterfacePath.IndexOf("usb#");//common to all emulated gamepads?
                                        }

                                        end = dj.InterfacePath.LastIndexOf("#{");

                                        if (start == -1 || end == -1)
                                        {
                                            continue;
                                        }

                                        string _d_devicePath = dj.InterfacePath.Substring(start, end - start).Replace('#', '\\').ToUpper();

                                        if (x_devicePath == _d_devicePath)
                                        {
                                            match = dj;
                                            d_devicePath = _d_devicePath;
                                            break;
                                        }
                                        else
                                        {
                                            //manage other emulated devices cases (uncommon device path?)
                                            start = dj.InterfacePath.ToUpper().IndexOf("VID");
                                            end = dj.InterfacePath.ToUpper().IndexOf("&IG_");

                                            if (end == -1)
                                            {
                                                end = dj.InterfacePath.ToUpper().LastIndexOf("&");
                                            }

                                            if (start == -1 || end == -1)
                                            {
                                                continue;
                                            }

                                            _d_devicePath = dj.InterfacePath.Substring(start, end - start).ToUpper();

                                            string _x_devicePath = string.Empty;

                                            start = x_devicePath.IndexOf("VID");
                                            end = x_devicePath.LastIndexOf("\\");

                                            if (start == -1 || end == -1)
                                            {
                                                continue;
                                            }

                                            _x_devicePath = x_devicePath.Substring(start, end - start).ToUpper();


                                            if (_x_devicePath == _d_devicePath)
                                            {
                                                match = dj;
                                                d_devicePath = _d_devicePath;
                                                break;
                                            }
                                        }
                                    }

                                    if (string.IsNullOrEmpty(d_devicePath) || match == null)
                                    {
                                        continue;
                                    }

                                    player.GamepadId = i;
                                    player.DInputJoystick = match;
                                    player.GamepadGuid = player.DInputJoystick.InstanceGuid;
                                    player.GamepadProductGuid = player.DInputJoystick.ProductGuid;
                                    player.GamepadName = player.DInputJoystick.InstanceName;
                                    player.RawHID = player.DInputJoystick.InterfacePath;

                                    player.HIDDeviceID = new string[] { x_devicePath, "" };
                                    player.IsInputUsed = !userGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.ProfileAssignGamepadByButonPress;
                                }
                                else
                                {
                                    //fallback to the press button method if no device hid path is available
                                    ;
                                }
                            }
                            else
                            {
                                player.GamepadId = i;
                                string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                                player.GamepadGuid = new Guid(guid);
                                player.HIDDeviceID = new string[] { "Not required", "Not required" };
                                player.IsInputUsed = true;
                            }

                            player.XInputJoystick = c;
                            player.IsXInput = true;
                            data.Add(player);
                            changed = true;
                        }
                    }
                }

                if (g.Hook.SDL2Enabled || g.Hook.SDL3Enabled)
                {
                    changed = CheckConnectedGamepads();

                    for (int i = 0; i < SDLManager.SDL2DevicesList.Count; i++)
                    {
                        SDL_GameController controller = SDLManager.SDL2DevicesList[i];

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
                            int vid = SDL2.JoystickGetDeviceVendor(i);

                            if (!vid.ToString().StartsWith("202"))
                            {
                                continue;
                            }
                        }

                        PlayerInfo player = new PlayerInfo();

                        if (!GameProfile.UseXinputIndex)
                        {
                            SDL_DeviceInfo devInfo = GetSDL_DeviceInfo(controller);

                            string s_devicePath = devInfo.HIDPATH.ToLower();

                            if (!string.IsNullOrEmpty(s_devicePath))
                            {
                                int start = s_devicePath.IndexOf("hid#");

                                if (start == -1)
                                {
                                    start = s_devicePath.IndexOf("usb#");//common to all emulated gamepads?
                                }

                                int end = s_devicePath.LastIndexOf("#{");

                                if (start == -1 || end == -1)
                                {
                                    continue;
                                }

                                s_devicePath = s_devicePath.Substring(start, end - start).Replace('#', '\\').ToUpper();

                                string d_devicePath = string.Empty;
                                D_Joystick match = null;

                                for (int j = 0; j < DInputManager.D_JoysticksList.Count; j++)
                                {
                                    D_Joystick dj = DInputManager.D_JoysticksList[j];

                                    start = dj.InterfacePath.IndexOf("hid#");

                                    if (start == -1)
                                    {
                                        start = dj.InterfacePath.IndexOf("usb#");//common to all emulated gamepads?
                                    }

                                    end = dj.InterfacePath.LastIndexOf("#{");

                                    if (start == -1 || end == -1)
                                    {
                                        continue;
                                    }

                                    string _d_devicePath = dj.InterfacePath.Substring(start, end - start).Replace('#', '\\').ToUpper();

                                    if (s_devicePath == _d_devicePath)
                                    {
                                        match = dj;
                                        s_devicePath = _d_devicePath;
                                        break;
                                    }
                                    else
                                    {
                                        //manage other emulated devices cases (uncommon device path)
                                        start = dj.InterfacePath.ToUpper().IndexOf("VID");
                                        end = dj.InterfacePath.ToUpper().IndexOf("&IG_");

                                        if (end == -1)
                                        {
                                            end = dj.InterfacePath.ToUpper().LastIndexOf("#");
                                        }

                                        if (start == -1 || end == -1)
                                        {
                                            continue;
                                        }

                                        _d_devicePath = dj.InterfacePath.Substring(start, end - start).ToUpper();

                                        string _s_devicePath = string.Empty;

                                        start = s_devicePath.IndexOf("VID");
                                        end = s_devicePath.LastIndexOf("\\");

                                        if (start == -1 || end == -1)
                                        {
                                            continue;
                                        }

                                        _s_devicePath = s_devicePath.Substring(start, end - start).ToUpper();

                                        if (_s_devicePath == _d_devicePath)
                                        {
                                            match = dj;
                                            d_devicePath = _d_devicePath;
                                            break;
                                        }
                                    }
                                }

                                if (string.IsNullOrEmpty(s_devicePath) || match == null)
                                {
                                    continue;
                                }

                                player.GamepadId = SDL2.SDL_GameControllerGetPlayerIndex(controller);
                                player.DInputJoystick = match;
                                player.GamepadGuid = player.DInputJoystick.InstanceGuid;
                                player.GamepadProductGuid = player.DInputJoystick.ProductGuid;
                                player.GamepadName = player.DInputJoystick.InstanceName;
                                player.RawHID = player.DInputJoystick.InterfacePath;
                                player.HIDDeviceID = new string[] { d_devicePath, "" };
                                player.IsInputUsed = !userGameInfo.HandlerInfo.GenericGameInfo.MetaInfo.ProfileAssignGamepadByButonPress;
                            }
                            else
                            {
                                //fallback to the press button method if no device hid path is available
                            }
                        }
                        else
                        {
                            player.GamepadId = SDL2.SDL_GameControllerGetPlayerIndex(controller);
                            string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                            player.GamepadGuid = new Guid(guid);
                            player.HIDDeviceID = new string[] { "Not required", "Not required" };
                            player.IsInputUsed = true;
                        }

                        player.SDL2Joystick = controller;

                        if (g.Hook.SDL2Enabled)
                        {
                            player.IsSDL2 = true;
                        }
                        else
                        {
                            player.IsSDL3 = true;
                        }

                        data.Add(player);
                        changed = true;
                    }
                }

                if (changed)
                {
                    UpdateDevices();
                    insideGamepadTick = false;
                    return;
                }

                insideGamepadTick = false;

                if (parent != null && parent.IsHandleCreated)
                {
                    parent.Invoke((MethodInvoker)delegate ()
                    {
                        parent.Invalidate(false);
                    });
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

                    if(player == null) { continue;}

                    if (!player.IsController){ continue; }

                    switch (player.InputType)
                    {
                        case InputType.DInput:
                            if (DInputManager.Poll(player)) 
                            { 
                                player.FlashIcon();
                            }
                            break;
                        case InputType.XInput:
                            if (XInputManager.Poll(player))
                            {
                                player.FlashIcon(); 
                            }
                            break;
                        case InputType.SDL2:
                        case InputType.SDL3:
                            if (SDLManager.Poll(player))
                            { 
                                player.FlashIcon();
                            }
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

                        OpenXinputController c = new OpenXinputController(userGameInfo.HandlerInfo.GenericGameInfo.ProtoInput.UseOpenXinput, (uint)player.GamepadId);

                        if (!c.IsConnected)
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

               Type deviceDataType = devicedata.GetType();

                if (deviceDataType == typeof(Guid))
                {
                    var instanceGuid = devicedata.ToString();
                    Guid.TryParse(instanceGuid, out Guid result);

                    if(result == Guid.Empty || player.GamepadGuid == Guid.Empty)
                    {
                        continue;
                    }

                    if (player.GamepadGuid == result)
                    {
                        assigned = true;
                        break;
                    }             
                }
                else if (deviceDataType == typeof(OpenXinputController))
                {
                    if (player.GamepadId == index)
                    {
                        OpenXinputController oxc = devicedata as OpenXinputController;

                        if (oxc == null)
                        {
                            continue;
                        }

                        var s = oxc.GetState();

                        int newmask = (int)s.Gamepad.Buttons;

                        if (player.GamepadMask != newmask)
                        {
                            player.GamepadMask = newmask;
                        }

                        assigned = true;
                        break;
                    }
                }
                else if (deviceDataType == typeof(SDL_GameController))
                {
                    SDL_GameController controller = (SDL_GameController)devicedata;
                    if (player.SDL2Joystick == (IntPtr)controller)
                    {
                        assigned = true;
                        break;
                    }
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
                if(player.ScreenIndex >= 0 && player.ScreenIndex < BoundsFunctions.Screens.Length)
                {
                    BoundsFunctions.Screens[player.ScreenIndex].PlayerOnScreen--;
                }
                
                GameProfile.TotalAssignedPlayers--;
                GameProfile.AssignedDevices.Remove(player);
                OnAssignedDeviceDisconnect?.Invoke(lostData);
            }

            var isGuestOf = GameProfile.AssignedDevices.Where(ad => ad.InstanceGuests.Contains(player)).FirstOrDefault();

            if (isGuestOf != null)
            {
                lostData = $"{isGuestOf.Nickname} guest {player.GamepadId}|{player.InputType.ToString()}";
                isGuestOf.InstanceGuests.Remove(player);
                OnAssignedDeviceDisconnect?.Invoke(lostData);
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

        internal static void UpdateDevices()
        {
            if (profile == null)
            {
                return;
            }

            GenericGameInfo g = userGameInfo.HandlerInfo.GenericGameInfo;

            if(g == null)
            {
                return;
            }

            List<PlayerInfo> playerData = profile.DevicesList;

            if (GameProfile.Loaded)
            {
                parent.canProceed = (GameProfile.TotalAssignedPlayers == GameProfile.TotalProfilePlayers);
            }
            else
            {
                parent.canProceed = playerData.Count(c => c.ScreenIndex != -1) >= 1;
            }

            if (playerData.Count == 0 || playerData.All(pl => !pl.IsRawKeyboard && !pl.IsRawMouse && !pl.IsKeyboardPlayer))
            {
                if (userGameInfo.HandlerInfo.GenericGameInfo.SupportsKeyboard)
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

                if (userGameInfo.HandlerInfo.GenericGameInfo.SupportsMultipleKeyboardsAndMice)///Raw mice/keyboards
                {
                    playerData.AddRange(RawInputManager.GetDeviceInputInfos());
                }

                if (testDinputPlayers != -1)///make fake data if needed
                {
                    for (int i = 0; i < testDinputPlayers; i++)
                    {
                        if (i >= userGameInfo.HandlerInfo.GenericGameInfo.MaxPlayers)
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
                            HIDDeviceID = new string[] { "Not required", "Not required" }
                        };

                        playerData.Add(player);
                    }
                }

                if (testXinputPlayers != -1)
                {
                    for (int i = 0; i < testXinputPlayers; i++)
                    {
                        if (i >= userGameInfo.HandlerInfo.GenericGameInfo.MaxPlayers)
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
                            HIDDeviceID = new string[] { "Not required", "Not required" }
                        };

                        playerData.Add(player);
                    }
                }

                if (testSDL2Players != -1)
                {
                    for (int i = 0; i < testSDL2Players; i++)
                    {

                        if(i >= userGameInfo.HandlerInfo.GenericGameInfo.MaxPlayers)
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
                            HIDDeviceID = new string[] { "Not required", "Not required" }
                        };

                        playerData.Add(player);
                    }
                }

                if(testSDL3Players != -1)
                {
                    for (int i = 0; i < testSDL3Players; i++)
                    {

                        if (i >= userGameInfo.HandlerInfo.GenericGameInfo.MaxPlayers)
                        {
                            break;
                        }

                        PlayerInfo player = new PlayerInfo
                        {
                            GamepadGuid = new Guid(),
                            GamepadName = "SDL3" + i,
                            IsSDL3 = true,
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

            BoundsFunctions.SetPlayerArea();

            if (parent != null && parent.IsHandleCreated)
            {
                UIThread.Post(() =>
                {
                    parent.CanPlayUpdated(parent.canProceed, false);
                    parent.Invalidate(false);
                });
            }
            
        }
    }
}
