using Gamepads;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using SDL;
using SharpDX.DirectInput;
using SharpDX.XInput;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Markup;
using static Nucleus.Gaming.Coop.OpenXinputController.NativeOpenXinput;
using static SDL.SDL_ControllerUtils;

namespace Nucleus.Gaming.Controls.SetupScreen
{
    public class DevicesFunctions
    {
        private static SynchronizationContext _syncContext;
        public static void Initialize(SetupScreenControl _parent, UserGameInfo _userGameInfo, GameProfile _profile)
        {
            parent = _parent;
            userGameInfo = _userGameInfo;
            profile = _profile;
            useGamepadApiIndex = App_Misc.UseXinputIndex;

            _syncContext = SynchronizationContext.Current ?? new SynchronizationContext();

            ClearDInputDevicesList();
            SDLDevices.InitSDL(_syncContext);

            GamepadTimer = new System.Threading.Timer(GamepadTimer_Tick, null, 0, 500);         
        }


        /// <summary>
        /// We need this if a new controller is plugged after sdl initialization
        /// else Nucleus sdl gamepad indexes will not match with the custom sdl one loaded by the game.
        /// </summary>
        /// <param name="RefreshSDL"></param>
        public static void RefreshSDL(SynchronizationContext syncContext)
        {
            SDLDevices.Refresh(syncContext);       
        }

        internal static List<Joystick> JoyStickList = new List<Joystick>();
        private static SetupScreenControl parent;
        private static UserGameInfo userGameInfo;
        private static GameProfile profile;

        private static int testDinputPlayers = -1;// 16;
        private static int testXinputPlayers = -1;// 16;

        public static System.Threading.Timer GamepadTimer;
        internal static System.Threading.Timer vibrationTimer;

        internal static float playerSize;
        internal static RectangleF playersArea;

        public static bool polling = false;
        internal static bool insideGamepadTick = false;
        public static bool isDisconnected;
        private static bool vgmDevicesOnly = false;

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

        internal static void ClearDInputDevicesList()
        {
            try
            {
                foreach (Joystick joystick in JoyStickList)
                {
                    if (!joystick.IsDisposed)
                    {
                        joystick.Dispose();
                    }
                }

                JoyStickList.Clear();
            }
            catch
            {
                JoyStickList.Clear();
            }
        }
       
        internal static void Vibration_Tick(object state)
        {
            PlayerInfo player = (PlayerInfo)state;

            if (profile == null || !player.IsXInput)
            {
                return;
            }

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

        public static bool PollDInputGamepad(PlayerInfo player)
        {
            if (player.DInputJoystick == null || !player.IsDInput)
            {
                return false;
            }

            if ((bool)player.DInputJoystick?.IsDisposed)
            {
                player.DInputJoystick = null;
                return false;
            }

            try
            {
                //if (vgmDevicesOnly)
                //{
                //    if (!player.DInputJoystick.Properties.VendorId.ToString().StartsWith("202"))
                //    {
                //        return false;
                //    }
                //}

                if ((bool)player.DInputJoystick?.GetCurrentState().Buttons.Any(b => b == true))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                player.DInputJoystick.Dispose();
                player.DInputJoystick = null;
                return false;
            }

            return false;
        }

        public static bool PollXInputGamepad(PlayerInfo player)
        {
            if (polling || player.IsFake || !player.IsXInput)
            {
                return false;
            }

            try
            {
                if (!player.XInputJoystick.IsConnected)
                {
                    return false;
                }

                if (vgmDevicesOnly)
                {
                    CapabilitiesEx cap;
                    var _cap = OpenXinputController.XInputGetCapabilitiesEx((uint)1, (uint)player.GamepadId, 1, out cap);

                    if (!cap.VendorId.ToString().StartsWith("202"))
                    {
                        return false;
                    }

                    JoyStickList = JoyStickList.Where(j => j.Properties.VendorId.ToString().StartsWith("202")).ToList();
                }


                if (player.XInputJoystick.GetState().Gamepad.Buttons != 0)
                {
                    if (useGamepadApiIndex)
                    {
                        Vibrate(player);
                        return true;
                    }

                    if (player.DInputJoystick == null)
                    {
                        int pressedCount = 0;

                        Joystick joystick = null;

                        for (int i = 0; i < JoyStickList.Count; i++)
                        {
                            joystick = JoyStickList[i];

                            if (joystick.GetCurrentState().Buttons.Any(b => b == true))
                            {
                                player.DInputJoystick = joystick;
                                ++pressedCount;
                            }
                        }

                        if (pressedCount > 1)
                        {
                            player.DInputJoystick.Dispose();
                            player.DInputJoystick = null;

                            ClearDInputDevicesList();//Clear all beacause if one is broken all others could potentialy be too.

                            return false;
                        }

                        if (pressedCount == 0)//if 0 the device does not return any state so it's a "broken" one.
                        {
                            ClearDInputDevicesList();

                            return false;
                        }

                        if (player.DInputJoystick != null)
                        {
                            player.GamepadGuid = player.DInputJoystick.Information.InstanceGuid;
                            player.GamepadProductGuid = player.DInputJoystick.Information.ProductGuid;
                            player.GamepadName = player.DInputJoystick.Information.InstanceName;
                            string hid = player.DInputJoystick.Properties.InterfacePath;
                            player.RawHID = hid;
                            int start = hid.IndexOf("hid#");
                            int end = hid.LastIndexOf("#{");
                            string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
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
                //Some wireless devices can disconnect even with the receiver plugged which mess everything up           
                profile.Reset();

                ClearDInputDevicesList();

                UpdateDevices();

                Console.WriteLine("Something went wrong with one or more DInput device(s)");

                return false;
            }

            return false;
        }

        public static bool PollSDLGamepad(PlayerInfo player)
        {
            if (polling)
            {
                return false;
            }

            try
            {
                if (!SDLDevices.IsConnected(player.SDL2Joystick))
                {
                    return false;
                }

                SDL_DeviceInfo info = SDL_ControllerUtils.GetSDL_DeviceInfo(player.SDL2Joystick);

                if (vgmDevicesOnly)
                {
                    if (!info.VENDORID.StartsWith("202"))
                    {
                        return false;
                    }

                    JoyStickList = JoyStickList.Where(j => j.Properties.VendorId.ToString().StartsWith("202")).ToList();
                }


                if (SDLDevices.QueryControllerStateAny(player.SDL2Joystick))
                {
                    if (useGamepadApiIndex)
                    {
                        Vibrate(player);
                        return true;
                    }

                    //if (player.DInputJoystick == null)
                    {
                        int pressedCount = 0;

                        Joystick joystick = null;

                        for (int i = 0; i < JoyStickList.Count; i++)
                        {
                            joystick = JoyStickList[i];

                            if (joystick.GetCurrentState().Buttons.Any(b => b == true))
                            {
                                player.DInputJoystick = joystick;
                                ++pressedCount;
                            }
                        }

                        if (pressedCount > 1)
                        {
                            player.DInputJoystick.Dispose();
                            player.DInputJoystick = null;

                            ClearDInputDevicesList();//Clear all beacause if one is broken all others could potentialy be too.

                            return false;
                        }

                        if (pressedCount == 0)//if 0 the device does not return any state so it's a "broken" one.
                        {
                            ClearDInputDevicesList();

                            return false;
                        }

                        if (player.DInputJoystick != null)
                        {
                            player.GamepadGuid = player.DInputJoystick.Information.InstanceGuid;
                            player.GamepadProductGuid = player.DInputJoystick.Information.ProductGuid;
                            player.GamepadName = player.DInputJoystick.Information.InstanceName;
                            string hid = player.DInputJoystick.Properties.InterfacePath;
                            player.RawHID = hid;
                            int start = hid.IndexOf("hid#");
                            int end = hid.LastIndexOf("#{");
                            string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
                            player.HIDDeviceID = new string[] { fhid, "" };
                            player.IsInputUsed = true;

                            if(!player.Vibrate)
                            {
                                SendRumble(player.SDL2Joystick);
                                player.Vibrate = true;
                            }                  
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
                //Some wireless devices can disconnect even with the receiver plugged which mess everything up           
                profile.Reset();

                ClearDInputDevicesList();

                UpdateDevices();

                Console.WriteLine("Something went wrong with one or more DInput device(s)");

                return false;
            }

            return false;
        }


        private static void DeletePlayers(List<PlayerInfo> players)
        {
            foreach(PlayerInfo player in players)
            {
                if (player.DInputJoystick != null)
                {
                    JoyStickList.Remove(player.DInputJoystick);
                    player.DInputJoystick.Dispose();
                }

                if(player.SDL2Joystick != IntPtr.Zero)
                {
                    SDL2.GameControllerClose(player.SDL2Joystick);
                    SDLDevices.SDL2DevicesList.Remove(player.SDL2Joystick);
                }

                if (GameProfile.AssignedDevices.Contains(player))
                {
                    BoundsFunctions.Screens[player.ScreenIndex].PlayerOnScreen--;
                    GameProfile.TotalAssignedPlayers--;
                    GameProfile.AssignedDevices.Remove(player);
                }

                profile.DevicesList.Remove(player);
            }
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

                DirectInput dinput = new DirectInput();
                var playersToDelete = new List<PlayerInfo>();

                if (g.Hook.DInputEnabled || g.Hook.XInputReroute || g.ProtoInput.DinputDeviceHook)
                {
                    IList<DeviceInstance> devices = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices);

                    // first search for disconnected gamepads
                    for (int j = 0; j < data.Count; j++)
                    {
                        PlayerInfo p = data[j];

                        if (!p.IsDInput || p.IsFake)
                        {
                            continue;
                        }

                        bool foundGamepad = false;

                        for (int i = 0; i < devices.Count; i++)
                        {
                            if (devices[i].InstanceGuid == p.GamepadGuid && i == data[j].GamepadId)
                            {
                                foundGamepad = true;
                                break;
                            }
                        }

                        if (!foundGamepad)
                        {
                            playersToDelete.Add(data[j]);
                            isDisconnected = true;
                        }
                    }

                    DeletePlayers(playersToDelete);

                    for (int i = 0; i < devices.Count; i++)
                    {
                        bool already = false;

                        // see if this gamepad is already on a player
                        for (int j = 0; j < data.Count; j++)
                        {
                            PlayerInfo p = data[j];

                            if (p.GamepadGuid == devices[i].InstanceGuid)
                            {
                                already = true;
                                break;
                            }
                        }

                        if (already)
                        {
                            continue;
                        }

                        Joystick joystick = new Joystick(dinput, devices[i].InstanceGuid);

                        PlayerInfo player = new PlayerInfo();

                        if (vgmDevicesOnly)
                        {
                            if (!joystick.Properties.VendorId.ToString().StartsWith("202"))
                            {
                                continue;
                            }
                        }

                        if (joystick.Properties.InterfacePath.ToUpper().Contains("IG_") && !g.Hook.XInputReroute && g.Hook.XInputEnabled)
                        {
                            continue;
                        }

                        joystick.Acquire();
                        player.DInputJoystick = joystick;
                        player.GamepadProductGuid = devices[i].ProductGuid;
                        player.GamepadGuid = devices[i].InstanceGuid;
                        player.GamepadName = devices[i].InstanceName;
                        player.IsDInput = true;
                        player.GamepadId = i;
                        string hid = player.DInputJoystick.Properties.InterfacePath;
                        player.RawHID = hid;
                        int start = hid.IndexOf("hid#");
                        int end = hid.LastIndexOf("#{");
                        string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
                        player.HIDDeviceID = new string[] { fhid, "" };

                        player.IsInputUsed = true;

                        data.Add(player);
                        changed = true;
                    }
                }

                //Create a list of all DInput Joysticks available (Only used to grab gamepads hardware informations for XInput devices when polling)                
                IList<DeviceInstance> dInputList = dinput.GetDevices(SharpDX.DirectInput.DeviceType.Gamepad, DeviceEnumerationFlags.AttachedOnly);

                for (int x = 0; x < dInputList.Count; x++)
                {
                    DeviceInstance device = dInputList[x];

                    if (JoyStickList.All(j => j.Information.InstanceGuid != device?.InstanceGuid))
                    {
                        Joystick joy = new Joystick(dinput, device.InstanceGuid);
                        joy.Acquire();
                        JoyStickList.Add(joy);
                    }
                }

                //Using OpenXinput with more than 4 players means we can use more than 4 xinput controllers
                if ((g.Hook.XInputEnabled && !g.Hook.XInputReroute && !g.ProtoInput.DinputDeviceHook) || g.ProtoInput.XinputHook)
                {
                    for (int j = 0; j < data.Count; j++)
                    {
                        PlayerInfo p = data[j];

                        if (p.IsXInput && !p.IsFake)
                        {
                            OpenXinputController c = new OpenXinputController(g.ProtoInput.UseOpenXinput, p.GamepadId);

                            if (!c.IsConnected)
                            {
                                playersToDelete.Add(data[j]);
                                isDisconnected = true;
                            }
                        }
                    }

                    DeletePlayers(playersToDelete);

                    int numControllers = g.ProtoInput.UseOpenXinput ? 32 : 4;//TODO: Big bug here!!if g.ProtoInput.UseOpenXinput is true the ui gamepads won't appears until screen is refreshed (why?)
                    for (int i = 0; i < numControllers; i++)
                    {
                        OpenXinputController c = new OpenXinputController(g.ProtoInput.UseOpenXinput, i);

                        bool already = false;

                        if (!c.IsConnected)
                        {
                            continue;
                        }
                        else
                        {
                            ///Check if this gamepad is already assigned to a player
                            foreach (PlayerInfo p in data)
                            {
                                if (p.IsXInput && p.GamepadId == i)
                                {
                                    var s = c.GetState();

                                    int newmask = (int)s.Gamepad.Buttons;

                                    if (p.GamepadMask != newmask)
                                    {
                                        p.GamepadMask = newmask;
                                    }

                                    already = true;
                                    break;
                                }
                            }

                            if (already)
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

                            changed = true;

                            //new gamepad
                            PlayerInfo player = new PlayerInfo
                            {
                                HIDDeviceID = new string[] { "Not required", "Not required" },
                                XInputJoystick = c,
                                IsXInput = true,
                                GamepadId = i
                            };

                            if (useGamepadApiIndex /*|| parent.profileDisabled*/)
                            {
                                string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                                player.GamepadGuid = new Guid(guid);
                                player.IsInputUsed = true;
                            }

                            data.Add(player);
                        }
                    }
                }

                if (g.Hook.SDL2Enabled)
                {
                    for (int j = 0; j < data.Count; j++)
                    {
                        PlayerInfo sdlPlayer = data[j];

                        if (sdlPlayer.IsSDL2)
                        {
                            if (!SDLDevices.IsConnected(sdlPlayer.SDL2Joystick))
                            {
                                playersToDelete.Add(sdlPlayer);
                                
                                   isDisconnected = true;
                            }
                        }
                    }

                    DeletePlayers(playersToDelete);

                    for (int i = 0; i < SDLDevices.SDL2DevicesList.Count; i++)
                    {
                        bool already = false;

                        SDL_GameController controller = SDLDevices.SDL2DevicesList[i]; //SDL2.GameControllerOpen(i);//
                        ///Check if this gamepad is already assigned to a player
                        foreach (PlayerInfo p in data)
                        {
                            if (p.IsSDL2 && p.SDL2Joystick == (IntPtr)controller)
                            {
                                already = true;
                                break;
                            }
                        }

                        if (already)
                        {
                            continue;
                        }

                        changed = true;

                        if (controller == IntPtr.Zero || !SDLDevices.IsConnected(controller))
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
                        player.IsSDL2 = true;
                        player.GamepadId = SDL2.SDL_GameControllerGetPlayerIndex(player.SDL2Joystick);

                        if (useGamepadApiIndex)
                        {
                            string guid = player.GamepadId + 1 >= 10 ? $"00000000-0000-0000-0000-2000000000{player.GamepadId + 1}" : $"00000000-0000-0000-0000-20000000000{player.GamepadId + 1}";
                            player.GamepadGuid = new Guid(guid);
                            player.IsInputUsed = true;
                        }

                        data.Add(player);
                    }
                }

                if (changed)
                {
                    if (parent.InvokeRequired)
                    {
                        parent.Invoke(new MethodInvoker(UpdateDevices));
                        parent.Invoke(new MethodInvoker(parent.Refresh));
                        insideGamepadTick = false;
                        return;
                    }
                }

                dinput?.Dispose();
                insideGamepadTick = false;
            }
            catch (Exception)
            {
                insideGamepadTick = false;
                return;
            }

            try
            {
                PlayerInfo hasNullDInputJoystick = null;

                foreach (PlayerInfo player in data)
                {
                    if (player.IsDInput)
                    {
                        if (player.DInputJoystick == null)
                        {
                            hasNullDInputJoystick = player;
                            continue;
                        }
                    }
                }

                if (hasNullDInputJoystick != null)
                {
                    if (GameProfile.AssignedDevices.Contains(hasNullDInputJoystick))
                    {
                        GameProfile.AssignedDevices.Remove(hasNullDInputJoystick);
                        BoundsFunctions.Screens[hasNullDInputJoystick.ScreenIndex].PlayerOnScreen--;
                        GameProfile.TotalAssignedPlayers--;
                    }

                    data.Remove(hasNullDInputJoystick);
                }
            }
            catch
            {
            }

            if (parent.IsHandleCreated)
            {
                parent?.Invoke(new Action(() => parent?.Invalidate(false)));
            }
        }


        public static void DisposeGamePadTimer()
        {
            GamepadTimer?.Dispose();
            GamepadTimer = null;
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
                Draw.playerCustomFont = new Font(Draw.playerCustomFont.FontFamily, Draw.playerCustomFont.Size * 0.8f, FontStyle.Regular, GraphicsUnit.Point, 0);
                playerSize = (int)Math.Round(((playerHeight / 1.2f) / newVertical));
            }

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
