using System;
using System.Runtime.InteropServices;

namespace SDL
{

    public enum SDL_GameControllerType
    {
        SDL_CONTROLLER_TYPE_UNKNOWN = 0,
        SDL_CONTROLLER_TYPE_XBOX360,
        SDL_CONTROLLER_TYPE_XBOXONE,
        SDL_CONTROLLER_TYPE_PS3,
        SDL_CONTROLLER_TYPE_PS4,
        SDL_CONTROLLER_TYPE_NINTENDO_SWITCH_PRO,
        SDL_CONTROLLER_TYPE_VIRTUAL,
        SDL_CONTROLLER_TYPE_PS5,
        SDL_CONTROLLER_TYPE_AMAZON_LUNA,
        SDL_CONTROLLER_TYPE_GOOGLE_STADIA,
        SDL_CONTROLLER_TYPE_NVIDIA_SHIELD,
        SDL_CONTROLLER_TYPE_NINTENDO_SWITCH_JOYCON_LEFT,
        SDL_CONTROLLER_TYPE_NINTENDO_SWITCH_JOYCON_RIGHT,
        SDL_CONTROLLER_TYPE_NINTENDO_SWITCH_JOYCON_PAIR,
        SDL_CONTROLLER_TYPE_MAX
    }
    
    public enum GameControllerBindType
    {
        None = 0,
        Button,
        Axis,
        Hat
    }

    public enum GameControllerAxis
    {
        SDL_CONTROLLER_AXIS_INVALID = -1,
        SDL_CONTROLLER_AXIS_LEFTX,
        SDL_CONTROLLER_AXIS_LEFTY,
        SDL_CONTROLLER_AXIS_RIGHTX,
        SDL_CONTROLLER_AXIS_RIGHTY,
        SDL_CONTROLLER_AXIS_TRIGGERLEFT,
        SDL_CONTROLLER_AXIS_TRIGGERRIGHT,
        SDL_CONTROLLER_AXIS_MAX
    }

    public enum SDL_GameControllerButton
    {
        SDL_CONTROLLER_BUTTON_INVALID = -1,
        SDL_CONTROLLER_BUTTON_A,
        SDL_CONTROLLER_BUTTON_B,
        SDL_CONTROLLER_BUTTON_X,
        SDL_CONTROLLER_BUTTON_Y,
        SDL_CONTROLLER_BUTTON_BACK,
        SDL_CONTROLLER_BUTTON_GUIDE,
        SDL_CONTROLLER_BUTTON_START,
        SDL_CONTROLLER_BUTTON_LEFTSTICK,
        SDL_CONTROLLER_BUTTON_RIGHTSTICK,
        SDL_CONTROLLER_BUTTON_LEFTSHOULDER,
        SDL_CONTROLLER_BUTTON_RIGHTSHOULDER,
        SDL_CONTROLLER_BUTTON_DPAD_UP,
        SDL_CONTROLLER_BUTTON_DPAD_DOWN,
        SDL_CONTROLLER_BUTTON_DPAD_LEFT,
        SDL_CONTROLLER_BUTTON_DPAD_RIGHT,
        SDL_CONTROLLER_BUTTON_MISC1,    /* Xbox Series X share button, PS5 microphone button, Nintendo Switch Pro capture button, Amazon Luna microphone button */
        SDL_CONTROLLER_BUTTON_PADDLE1,  /* Xbox Elite paddle P1 (upper left, facing the back) */
        SDL_CONTROLLER_BUTTON_PADDLE2,  /* Xbox Elite paddle P3 (upper right, facing the back) */
        SDL_CONTROLLER_BUTTON_PADDLE3,  /* Xbox Elite paddle P2 (lower left, facing the back) */
        SDL_CONTROLLER_BUTTON_PADDLE4,  /* Xbox Elite paddle P4 (lower right, facing the back) */
        SDL_CONTROLLER_BUTTON_TOUCHPAD, /* PS4/PS5 touchpad button */
        SDL_CONTROLLER_BUTTON_MAX
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct GameControllerButtonBind
    {
        public GameControllerBindType BindType;
        public GameControllerButtonBindUnion Value;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct GameControllerButtonBindUnion
    {
        [FieldOffset(0)]
        public int Button;
        [FieldOffset(0)]
        public int Axis;
        [FieldOffset(0)]
        public HatStruct Hat;

        [StructLayout(LayoutKind.Sequential)]
        public struct HatStruct
        {
            public int Hat;
            public int HatMask;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SDL_GameController
    {
        private readonly IntPtr ptr;

        public SDL_GameController(IntPtr ptr)
        {
            this.ptr = ptr;
        }

        public static implicit operator IntPtr(SDL_GameController gameController)
        {
            return gameController.ptr;
        }

        public static implicit operator SDL_GameController(IntPtr ptr)
        {
            return new SDL_GameController(ptr);
        }
    }

    public static unsafe partial class SDL2
    {
        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerAddMapping", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern int GameControllerAddMapping(string mappingString);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerClose", CallingConvention = CallingConvention.Cdecl)]
        public static extern int GameControllerClose(SDL_GameController gamecontroller);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerFromInstanceID", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GameController GameControllerFromInstanceID(JoystickID joyid);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetAttached", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GameControllerGetAttached(SDL_GameController gamecontroller);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern short GameControllerGetAxis(SDL_GameController gameController, GameControllerAxis axis);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetAxisFromString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern GameControllerAxis GameControllerGetAxisFromString(string pchString);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetBindForAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern GameControllerButtonBind GameControllerGetBindForAxis(SDL_GameController gameController, GameControllerAxis axis);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetBindForButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern GameControllerButtonBind GameControllerGetBindForButton(SDL_GameController gameController, SDL_GameControllerButton buttons);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte GameControllerGetButton(SDL_GameController gameController, SDL_GameControllerButton button);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetButtonFromString", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, BestFitMapping = false, ThrowOnUnmappableChar = true)]
        public static extern SDL_GameControllerButton GameControllerGetButtonFromString(string pchString);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetJoystick", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_Joystick GameControllerGetJoystick(SDL_GameController gameController);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetStringForAxis", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerGetStringForAxis(GameControllerAxis axis);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerPath", CallingConvention = CallingConvention.Cdecl)]
        unsafe public static extern byte* SDL_GameControllerPath(SDL_GameController gameController);


        [DllImport("SDL2.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SDL_GameControllerSetPlayerIndex(IntPtr gamecontroller, int playerIndex);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetPlayerIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte SDL_GameControllerGetPlayerIndex(SDL_GameController gameController);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetType", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GameControllerType SDL_GameControllerGetType(SDL_GameController gameController);

        public static string GameControllerGetStringForAxisString(GameControllerAxis axis)
        {
            return GetString(GameControllerGetStringForAxis(axis));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetStringForButton", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerGetStringForButton(SDL_GameControllerButton button);

        public static string GameControllerGetStringForButtonString(SDL_GameControllerButton button)
        {
            return GetString(GameControllerGetStringForButton(button));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerMapping", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerMapping(SDL_GameController gameController);

        public static string GameControllerMappingString(SDL_GameController gameController)
        {
            return GetString(GameControllerMapping(gameController));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerMappingForGUID", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerMappingForGUID(Guid guid);

        public static string GameControllerMappingForGUIDString(Guid guid)
        {
            return GetString(GameControllerMappingForGUID(guid));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerGetSerial", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr GameControllerGetSerial(IntPtr gamecontroller);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerName", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerName(SDL_GameController gameController);

        public static string GameControllerNameString(SDL_GameController gameController)
        {
            return GetString(GameControllerName(gameController));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerNameForIndex", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte* GameControllerNameForIndex(int joystickIndex);

        public static string GameControllerNameForIndexString(int joystickIndex)
        {
            return GetString(GameControllerNameForIndex(joystickIndex));
        }

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerOpen", CallingConvention = CallingConvention.Cdecl)]
        public static extern SDL_GameController GameControllerOpen(int joystickIndex);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerUpdate", CallingConvention = CallingConvention.Cdecl)]
        public static extern void GameControllerUpdate();

        [DllImport("SDL2.dll", EntryPoint = "SDL_IsGameController", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool IsGameController(int joystickIndex);


        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerRumble", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GameControllerRumble(SDL_GameController gamecontroller, ushort low_frequency_rumble,
                                                                     ushort high_frequency_rumble, uint duration_ms);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerRumbleTriggers", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GameControllerRumbleTriggers(SDL_GameController gamecontroller, ushort left_rumble,
                                                                     ushort right_rumble, uint duration_ms);

        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerSetLED", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GameControllerSetLED(SDL_GameController gamecontroller,byte red, byte green, byte blue);
        
        
        [DllImport("SDL2.dll", EntryPoint = "SDL_GameControllerSendEffect", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GameControllerSendEffect(SDL_GameController gamecontroller,  void* data, int size);

        [DllImport(LibraryName, EntryPoint = "SDL_GameControllerAddMappingsFromRW", CallingConvention = CallingConvention.Cdecl)]
        public static extern int SDL_GameControllerAddMappingsFromRW(IntPtr rw,int freeRw);

        //[DllImport(LibraryName, EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr SDL_RWFromFile(string filePath,string mode);

        //[DllImport(LibraryName, EntryPoint = "SDL_RWFromFile", CallingConvention = CallingConvention.Cdecl)]
        //public static extern IntPtr SDL_RWFromFile(string filePath, string mode);

        //SDL_GameControllerAddMappingsFromRW(SDL_RWFromFile(file, "rb"), 1)
        //int SDLCALL SDL_GameControllerSendEffect(SDL_GameController* gamecontroller, const void* data, int size);
    }
}
