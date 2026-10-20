using System;

namespace Nucleus.Gaming.Coop.ProtoInput
{
    public class ProtoInputOptions
    {
        public bool InjectStartup;
        public bool InjectRuntime_RemoteLoadMethod;
        public bool InjectRuntime_EasyHookMethod;
        public bool InjectRuntime_EasyHookStealthMethod;

        public bool TranslateXinputtoMKB;
        //XtranslateCFG all bools
        public bool TranslateXSwapSticks;
        public bool TranslateXScanner;
        public bool TranslateXShouldersNextPoint;
        public bool TranslateXAstatic;
        public bool TranslateXBstatic;
        public bool TranslateXXstatic;
        public bool TranslateXYstatic;
        public bool TranslateXAPointClick;
        public bool TranslateXBPointClick;
        public bool TranslateXXPointClick;
        public bool TranslateXYPointClick;
        public bool TranslateXAPointMove;
        public bool TranslateXBPointMove;
        public bool TranslateXXPointMove;
        public bool TranslateXYPointMove;

        //XtranslateKeys all ints
        public int TranslateXButtonA;
        public int TranslateXButtonB;
        public int TranslateXButtonX;
        public int TranslateXButtonY;
        public int TranslateXButtonRS;
        public int TranslateXButtonLS;

        public int TranslateXButtonRight;
        public int TranslateXButtonLeft;
        public int TranslateXButtonUp;
        public int TranslateXButtonDown;

        public int TranslateXStickR;
        public int TranslateXStickL;

        public int TranslateXStickRight;
        public int TranslateXStickLeft;
        public int TranslateXStickUp;
        public int TranslateXStickDown;

        public int TranslateXButtonOption;
        public int TranslateXButtonStart;
        public int TranslateXSensitivity;
        public int TranslateXAccelration;
        public int TranslateXDeadzone;


        public bool RegisterRawInputHook;
        public bool GetRawInputDataHook;
        public bool MessageFilterHook;
        public bool GetCursorPosHook;
        public bool SetCursorPosHook;
        public bool GetKeyStateHook;
        public bool GetAsyncKeyStateHook;
        public bool GetKeyboardStateHook;
        public bool CursorVisibilityHook;
        public bool ClipCursorHook;
        public bool ClipCursorHookCreatesFakeClip;
        public bool FocusHooks;
        public bool RenameHandlesHook;
        public bool XinputHook;
        public bool DinputDeviceHook;
        public SetWindowPosHook SetWindowPosHook;
        
        public bool BlockRawInputHook;
        public bool FindWindowHook;
        public bool CreateSingleHIDHook;
        public bool SetWindowStyleHook;
        public bool GetCursorInfoHook;
        public bool SetWindowsHookHook;
        public bool GetRawInputBufferHook;
        public MoveWindowHook MoveWindowHook;
        public bool AdjustWindowRectHook;
        public SetRemoveBorderHook SetRemoveBorderHook;
        public int ScaleFromX;
        public int ScaleFromY;

        public bool RemoveBorders;
        public bool SetPosition;

        public bool RawInputFilter;
        public bool MouseMoveFilter;
        public bool MouseActivateFilter;
        public bool WindowActivateFilter;
        public bool WindowActvateAppFilter;
        public bool MouseWheelFilter;
        public bool MouseButtonFilter;
        public bool KeyboardButtonFilter;

        public uint[] BlockedMessages;

        public string[] RenameHandles;
        public string[] RenameNamedPipes;

        public bool SendMouseWheelMessages;
        public bool SendMouseButtonMessages;
        public bool SendMouseMovementMessages;
        public bool SendKeyboardButtonMessages;
        public bool SendMouseDblClkMessages;

        public bool EnableFocusMessageLoop;
        public int FocusLoopIntervalMilliseconds = 5;
        public bool FocusLoop_WM_ACTIVATE;
        public bool FocusLoop_WM_ACTIVATEAPP;
        public bool FocusLoop_WM_NCACTIVATE;
        public bool FocusLoop_WM_SETFOCUS;
        public bool FocusLoop_WM_MOUSEACTIVATE;

        public DrawFakeCursor DrawFakeCursor;
        public bool SetCursorPosHookAlsoMessage;
        public bool AllowFakeCursorOutOfBounds;
        public bool ExtendFakeCursorBounds;
        public bool EnableToggleFakeCursorVisibilityShortcut;
        public uint ToggleFakeCursorVisibilityShortcutVkey = 0x24; // VK_HOME
        public bool DontShowCursorWhenImageUpdated;
        public PutMouseInsideWindow PutMouseInsideWindow;

        public bool UseDinputRedirection;

        public bool UseOpenXinput;

        public bool TranslateMKBtoXinput;

        public bool ReRegisterInput;
        public bool PointerInMouse;
        public bool MessageSubWindows;

        public bool MultipleProtoControllers;

        public bool DinputHookAlsoHooksGetDeviceState;

        public bool FreezeExternalInputWhenInputNotLocked;

        public Action OnInputLocked;
        public Action OnInputUnlocked;

        public bool AutoHideTaskbar;
    }
}
