using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using SDL;

namespace Nucleus.Gaming.Coop
{
    public class PlayerInfo
    {
        public List<Rectangle> OtherLayout;
        private RectangleF sourceEditBounds;
        private RectangleF editBounds;
        private Rectangle monitorBounds;
        private Rectangle defaultMonitorBounds;

        //Only to use for "profile players"
        public Rectangle OwnerDisplay;
        public RectangleF OwnerUIBounds;
        public int OwnerType;
        public int CurrentMaxGuests = 0;

        public bool WaitGuests => InstanceGuests.Count < CurrentMaxGuests;
        //

        public InputType InputType;
        private object tag;

        /// <summary>
        /// Stable identity of the device this player was created from, for the headless setup services
        /// (for example "dinput:{guid}", "xinput:2", "rawkb:{hid name}", "kb:single"). Set where the device is detected; null for profile templates.
        /// </summary>
        public string DeviceKey;

        public string IdealProcessor = "*";
        public string Affinity = "";
        public string PriorityClass = "Normal";
        public string GamepadName;
        public string[] HIDDeviceID;
        public string Nickname;
        public string InstanceId;
        public string RawHID;
        public string SID;
        public string Adapter;
        public string UserProfile;
        public List<PlayerInfo> InstanceGuests = new List<PlayerInfo>();
        public List<Guid> GuestsGuid = new List<Guid>();

        public bool SteamEmu;
        public bool GotLauncher;
        public bool GotGame;
        public bool Polling;
        private bool isRawMouse;
        public bool IsRawMouse
        {
            get => isRawMouse;
            set
            {
                isRawMouse = value;
                if (value && !IsRawKeyboard)
                {
                    InputType = InputType.Mouse;
                }
                else if (isRawMouse && isRawKeyboard)//merged profile k&b player
                {
                    InputType = InputType.KBM;
                }
            }
        }

        private bool isRawKeyboard;
        public bool IsRawKeyboard
        {
            get => isRawKeyboard;
            set
            {
                isRawKeyboard = value;
                if(value && !isRawMouse)
                {
                    InputType = InputType.KB;
                }
                else if (isRawMouse && isRawKeyboard)//merged profile k&b player
                {
                    InputType = InputType.KBM;
                }
            }
        }

        private bool isKeyboardPlayer;
        public bool IsKeyboardPlayer
        {
            get => isKeyboardPlayer;
            set
            {
                isKeyboardPlayer = value;
                if(value && !isRawMouse && !isRawKeyboard)
                {
                    InputType = InputType.SingleKB;
                }               
            }
        }

        private bool isXinput;
        public bool IsXInput
        {
            get => isXinput;                        
            set
            {
                isXinput = value;
                IsController = value;

                if(value)
                {
                    InputType = InputType.XInput;
                }    
            }
        }

        private bool isSDL2;
        public bool IsSDL2
        {
            get => isSDL2;
            set
            {
                isSDL2 = value;
                IsController = value;

                if (value)
                {
                    InputType = InputType.SDL2;
                }
            }
        }

        private bool isDInput;
        public bool IsDInput
        {
            get => isDInput;
            set
            {
                isDInput = value;
                IsController = value;

                if (value)
                {
                    InputType = InputType.DInput;
                }
            }
        }

        public bool IsFake;
              
        public bool IsInputUsed;

        /// <summary>Last time a button press on this pad could not identify it (no unique DirectInput match); MinValue otherwise.</summary>
        public DateTime IdentificationFailedUtc = DateTime.MinValue;
        public bool IsController;//Good to do not have to loop both Xinput & DInput
        public bool Vibrate;

        public Guid GamepadProductGuid;
        public Guid GamepadGuid;

        public Display Display;
        public UserScreen Owner;
        
        public D_Joystick DInputJoystick;
        public OpenXinputController XInputJoystick;
        public SDL_GameController SDL2Joystick;
        
        private ProcessData processData;

        public Window RawInputWindow { get; set; }

        /// <summary>
        /// "IsMinimzed" used to sync all players window state 
        /// if the window is not minimized by Nucleus and some other cases.
        /// </summary>
        public bool IsMinimized;
        public long SteamID;

        public uint ProtoInputInstanceHandle = 0;

        public int CurrentLayout = 0;
        public int ScreenPriority;
        public int GamepadId = -1;
        public int GamepadMask;
        public int DisplayIndex = -1;
        private int screenIndex = -1;
        public int PlayerID = -1;
        public int ProcessID;
        // Should be set by a script, then these are sent into Proto Input.
        // Zero implies no controller, 1 means controller 1, etc
        public int ProtoController1;
        public int ProtoController2;
        public int ProtoController3;
        public int ProtoController4;

        public IntPtr RawMouseDeviceHandle = (IntPtr)(-1);
        public IntPtr RawKeyboardDeviceHandle = (IntPtr)(-1);
        public IntPtr GamepadHandle;

        /// <summary>
        /// The bounds of this player's game screen
        /// </summary>
        public Rectangle MonitorBounds
        {
            get => monitorBounds;
            set => monitorBounds = value;
        }

        public RectangleF SourceEditBounds
        {
            get => sourceEditBounds;
            set => sourceEditBounds = value;
        }

        /// <summary>
        /// A temporary rectangle to show the user
        /// where the game screen is going to be located
        /// </summary>
        public RectangleF EditBounds
        {
            get => editBounds;
            set => editBounds = value;
        }

        public Rectangle DefaultMonitorBounds
        {
            get => defaultMonitorBounds;
            set => defaultMonitorBounds = value;
        }

        /// <summary>
        /// The index of this player
        /// </summary>
        public int ScreenIndex
        {
            get => screenIndex;
            set => screenIndex = value;
        }

        /// <summary>
        /// A custom tag object for handlers to store data in
        /// </summary>
        public object Tag
        {
            get => tag;
            set => tag = value;
        }

        /// <summary>
        /// Information about the game's process, null if its not running
        /// </summary>
        public ProcessData ProcessData
        {
            get => processData;
            set => processData = value;
        }

        #region Flash
        public bool ShouldFlash;

        /// <summary>
        /// Raised when <see cref="ShouldFlash"/> turns on (true) or off (false). Raised on the thread that detected the input
        /// (polling timer, raw input thread) or on the thread pool for the "off" edge.
        /// </summary>
        public static event Action<PlayerInfo, bool> FlashChanged;

        /// <summary>Gamepad buttons held at the last setup poll (<see cref="TrackButtons"/>).</summary>
        public PadButtons HeldButtons;

        /// <summary>
        /// Raised by <see cref="TrackButtons"/> when gamepad buttons go down, with those buttons. The setup polling raises it
        /// before the flash of the same press, on the thread that polls the pads.
        /// </summary>
        public static event Action<PlayerInfo, PadButtons> ButtonsPressed;

        /// <summary>
        /// Records the buttons a poll saw held on this pad (none when the poll saw no press), and raises
        /// <see cref="ButtonsPressed"/> with the ones that went down since the previous poll.
        /// </summary>
        public void TrackButtons(PadButtons held)
        {
            PadButtons pressed = PadButtonRules.Pressed(HeldButtons, held);
            HeldButtons = held;

            if (pressed != PadButtons.None)
            {
                ButtonsPressed?.Invoke(this, pressed);
            }
        }

        private Stopwatch flashStopwatch = new Stopwatch();
        private Task flashTask = null;

        public virtual void FlashIcon()
        {
            if (ShouldFlash && flashStopwatch != null && flashStopwatch.IsRunning && flashStopwatch.ElapsedMilliseconds <= 250)
            {
                return;
            }

            //Keyboards and mice are revealed by their first input (this used to happen only while painting the setup screen).
            if (InputType == InputType.SingleKB || InputType == InputType.KB || InputType == InputType.Mouse || InputType == InputType.KBM)
            {
                IsInputUsed = true;
            }

            if (!ShouldFlash)
            {
                ShouldFlash = true;
                FlashChanged?.Invoke(this, true);
            }

            flashStopwatch.Restart();

            if (flashTask == null)
            {
                flashTask = new Task(delegate
                {
                    while (flashStopwatch.ElapsedMilliseconds <= 500)
                    {
                        Thread.Sleep(501 - (int)flashStopwatch.ElapsedMilliseconds);
                    }

                    flashTask = null;

                    ShouldFlash = false;
                    FlashChanged?.Invoke(this, false);
                });

                flashTask.Start();
            }
        }
        #endregion

    }
}
