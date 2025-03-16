using System;
using System.Runtime.InteropServices;

namespace SDL
{
    public enum EventType : uint
    {
        FirstEvent = 0,

        /* Application events */
        Quit = 0x100,

        AppTerminating,
        AppLowMemory,
        AppWillEnterBackground,
        AppDidEnterBackground,
        AppWillEnterForeground,
        AppDidEnterForeground,

        /* Window events */
        WindowEvent = 0x200,
        SysWMEvent,

        /* Keyboard events */
        KeyDown = 0x300,
        KeyUp,
        TextEditing,
        TextInput,
        KeyMapChanged,

        /* Mouse events */
        MouseMotion = 0x400,
        MouseButtonDown,
        MouseButtonUp,
        MouseWheel,

        /* Joystick events */
        JoyAxisMotion = 0x600,
        JoyBallMotion,
        JoyHatMotion,
        JoyButtonDown,
        JoyButtonUp,
        JoyDeviceAdded,
        JoyDeviceRemoved,

        /* Game controller events */
        ControllerAxisMotion = 0x650,
        ControllerButtonDown,
        ControllerButtonUp,
        ControllerDeviceAdded,
        ControllerDeviceRemoved,
        ControllerDeviceRemapped,

        /* Touch events */
        FingerDown = 0x700,
        FingerUp,
        FingerMotion,

        /* Gesture events */
        DollarGesture = 0x800,
        DollarRecord,
        MultiGesture,

        /* Clipboard events */
        ClipboardUpdate = 0x900,

        /* Drag and drop events */
        DropFile = 0x1000,
        DropText,
        DropBegin,
        DropComplete,

        /* Audio hotplug events */
        AudioDeviceAdded = 0x1100,
        AudioDeviceRemoved,

        /* Render events */
        RenderTargetsReset = 0x2000,
        RenderDeviceReset,

        UserEvent = 0x8000,

        LastEvent = 0xFFFF
    }

    public enum ButtonState : sbyte
    {
        Released = 0,
        Pressed = 1
    }

    public enum EventAction
    {
        AddEvent,
        PeekEvent,
        GetEvent
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CommonEvent
    {
        public EventType Type;
        public uint Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WindowEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public WindowEventID Evt;
        private fixed byte padding[3];
        public int Data1;
        public int Data2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct KeyboardEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public sbyte State;
        public byte Repeat;
        public byte Padding2;
        public byte Padding3;
        public Keysym Keysym;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TextEditingEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public fixed byte Text[SDL2.TEXTEDITINGEVENT_TEXT_SIZE];
        public int Start;
        public int Length;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct TextInputEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public fixed byte Text[SDL2.TEXTINPUTEVENT_TEXT_SIZE];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseMotionEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public ButtonMask State;
        public int X;
        public int Y;
        public int XRel;
        public int YRel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseButtonEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public sbyte Button;
        public sbyte State;
        public byte Clicks;
        public byte Padding;
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MouseWheelEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public uint Which;
        public int X;
        public int Y;
        public MouseWheelDirection Direction;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct JoyAxisEvent
    {
        public EventType Type;
        public uint Timestamp;
        public JoystickID Which;
        public byte Axis;
        private fixed byte padding[3];
        public short Value;
        private ushort padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct JoyBallEvent
    {
        public EventType Type;
        public uint Timestamp;
        public JoystickID Which;
        public byte Ball;
        private fixed byte padding[3];
        public short XRel;
        public short YRel;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JoyHatEvent
    {
        public EventType Type;
        public uint Timestamp;
        public JoystickID Which;
        public byte Hat;
        public Hat Value;
        private ushort Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JoyButtonEvent
    {
        public EventType Type;
        public uint Timestamp;
        public JoystickID Which;
        public sbyte Button;
        public sbyte State;
        private sbyte padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JoyDeviceEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ControllerAxisEvent
    {
        public EventType Type;
        public uint Timestamp;
        public JoystickID Which;
        public GameControllerAxis Axis;
        private fixed byte padding[3];
        public short Value;
        private ushort padding4;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerButtonEvent
    {
        public uint Type;
        public uint Timestamp;
        public JoystickID Which;
        public sbyte Button;
        public sbyte State;
        private sbyte padding1;
        private sbyte padding2;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ControllerDeviceEvent
    {
        public EventType Type;
        public uint Timestamp;
        public int Which;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct AudioDeviceEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint Which;
        public byte IsCapture;
        private fixed byte padding[3];
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct TouchFingerEvent
    {
        public EventType Type;
        public uint Timestamp;
        public TouchID TouchId;
        public FingerID FingerId;
        public float X;
        public float Y;
        public float DX;
        public float DY;
        public float Pressure;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MultiGestureEvent
    {
        public EventType Type;
        public uint Timestamp;
        public TouchID TouchId;
        public float DTheta;
        public float DDist;
        public float X;
        public float Y;
        public ushort NumFingers;
        private ushort Padding;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DollarGestureEvent
    {
        public EventType Type;
        public uint Timestamp;
        public TouchID TouchId;
        public long GestureId;
        public uint NumFingers;
        public float Error;
        public float X;
        public float Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct DropEvent
    {
        public EventType Type;
        public uint Timestamp;
        public byte* File;
        public uint WindowID;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct QuitEvent
    {
        public EventType Type;
        public uint Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OSEvent
    {
        public EventType Type;
        public uint Timestamp;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct UserEvent
    {
        public EventType Type;
        public uint Timestamp;
        public uint WindowID;
        public int Code;
        public void* Data1;
        public void* Data2;
    }

    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct Event
    {
        [FieldOffset(0)]
        public EventType Type;

        [FieldOffset(0)]
        public CommonEvent Common;

        [FieldOffset(0)]
        public WindowEvent Window;

        [FieldOffset(0)]
        public KeyboardEvent Keyboard;

        [FieldOffset(0)]
        public TextEditingEvent Edit;

        [FieldOffset(0)]
        public TextInputEvent Text;

        [FieldOffset(0)]
        public MouseMotionEvent Motion;

        [FieldOffset(0)]
        public MouseButtonEvent Button;

        [FieldOffset(0)]
        public MouseWheelEvent Wheel;

        [FieldOffset(0)]
        public JoyAxisEvent JAxis;

        [FieldOffset(0)]
        public JoyBallEvent JBall;

        [FieldOffset(0)]
        public JoyHatEvent JHat;

        [FieldOffset(0)]
        public JoyButtonEvent JButton;

        [FieldOffset(0)]
        public JoyDeviceEvent JDevice;

        [FieldOffset(0)]
        public ControllerAxisEvent CAxis;

        [FieldOffset(0)]
        public ControllerButtonEvent CButton;

        [FieldOffset(0)]
        public ControllerDeviceEvent CDevice;

        [FieldOffset(0)]
        public AudioDeviceEvent ADevice;

        [FieldOffset(0)]
        public QuitEvent Quit;

        [FieldOffset(0)]
        public UserEvent User;

        [FieldOffset(0)]
        public TouchFingerEvent TFinger;

        [FieldOffset(0)]
        public MultiGestureEvent MGesture;

        [FieldOffset(0)]
        public DollarGestureEvent DGesture;

        [FieldOffset(0)]
        public DropEvent Drop;

        [FieldOffset(0)]
        private fixed byte padding[56];
    }

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public unsafe delegate int EventFilter(void* userdata, Event* evt);

    public static unsafe partial class SDL2
    {
        public const byte QUERY = unchecked((byte)-1);
        public const byte IGNORE = 0;
        public const byte DISABLE = 0;
        public const byte ENABLE = 1;

        public const int TEXTEDITINGEVENT_TEXT_SIZE = 32;
        public const int TEXTINPUTEVENT_TEXT_SIZE = 32;

        [DllImport(LibraryName, EntryPoint = "SDL_AddEventWatch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void AddEventWatch(EventFilter filter, void* userdata);

        [DllImport(LibraryName, EntryPoint = "SDL_DelEventWatch", CallingConvention = CallingConvention.Cdecl)]
        public static extern void DelEventWatch(EventFilter filter, void* userdata);

        [DllImport(LibraryName, EntryPoint = "SDL_EventState", CallingConvention = CallingConvention.Cdecl)]
        public static extern byte EventState(EventType Type, int state);

        [DllImport(LibraryName, EntryPoint = "SDL_FilterEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FilterEvents(EventFilter filter, void* userdata);

        [DllImport(LibraryName, EntryPoint = "SDL_FlushEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlushEvent(EventType Type);

        [DllImport(LibraryName, EntryPoint = "SDL_FlushEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern void FlushEvents(EventType minType, EventType maxType);

        [DllImport(LibraryName, EntryPoint = "SDL_GetEventFilter", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool GetEventFilter(IntPtr* filter, void** userdata);

        public static bool GetEventFilter(out EventFilter filter, void** userdata)
        {
            IntPtr ptr;
            bool result = GetEventFilter(&ptr, userdata);
            if (result)
                filter = Marshal.GetDelegateForFunctionPointer<EventFilter>(ptr);
            else
                filter = null;
            return result;
        }

        [DllImport(LibraryName, EntryPoint = "SDL_HasEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HasEvent(EventType Type);

        [DllImport(LibraryName, EntryPoint = "SDL_HasEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool HasEvents(EventType minType, EventType maxType);

        [DllImport(LibraryName, EntryPoint = "SDL_PeepEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PeepEvents(Event* events, int numevents, EventAction action, EventType minType, EventType maxType);
        
        [DllImport(LibraryName, EntryPoint = "SDL_PeepEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PeepEvents([Out] Event[] events, int numevents, EventAction action, EventType minType, EventType maxType);
        
        [DllImport(LibraryName, EntryPoint = "SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PollEvent(Event* evt);

        [DllImport(LibraryName, EntryPoint = "SDL_PollEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PollEvent(out Event evt);
        
        [DllImport(LibraryName, EntryPoint = "SDL_PumpEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern void PumpEvents();

        [DllImport(LibraryName, EntryPoint = "SDL_PushEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PushEvent(Event* evt);

        [DllImport(LibraryName, EntryPoint = "SDL_PushEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int PushEvent(out Event evt);
        
        [DllImport(LibraryName, EntryPoint = "SDL_RegisterEvents", CallingConvention = CallingConvention.Cdecl)]
        public static extern uint RegisterEvents(int numevents);

        [DllImport(LibraryName, EntryPoint = "SDL_SetEventFilter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEventFilter(EventFilter filter, void* userdata);
        
        [DllImport(LibraryName, EntryPoint = "SDL_SetEventFilter", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetEventFilter(EventFilter filter, IntPtr userdata);
        
        [DllImport(LibraryName, EntryPoint = "SDL_WaitEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WaitEvent(Event* evt);
        
        [DllImport(LibraryName, EntryPoint = "SDL_WaitEvent", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WaitEvent(out Event evt);
        
        [DllImport(LibraryName, EntryPoint = "SDL_WaitEventTimeout", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WaitEventTimeout(Event* evt, int timeout);
                
        [DllImport(LibraryName, EntryPoint = "SDL_WaitEventTimeout", CallingConvention = CallingConvention.Cdecl)]
        public static extern int WaitEventTimeout(out Event evt, int timeout);
    }
}
