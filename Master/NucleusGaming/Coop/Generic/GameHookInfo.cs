namespace Nucleus.Gaming
{
    /// <summary>
    /// Holds information about our custom x360ce/xinput hook dll
    /// </summary>
    public class GameHookInfo
    {
        /// <summary>
        /// If the game should be run using our custom version of x360ce for gamepad control.
        /// Enabled by default as the majority of our games need it
        /// </summary>
        public bool CustomDllEnabled = true;

        /// <summary>
        /// If the game supports direct input joysticks
        /// </summary>
        public bool DInputEnabled;

        /// <summary>
        /// If we should completely remove support for DirectInput input from the game
        /// </summary>
        public bool DInputForceDisable;

        /// <summary>
        /// If the game supports xinput joysticks
        /// </summary>
        public bool XInputEnabled;


        /// <summary>
        /// If the game supports sdl2 joysticks(not a hook)
        /// </summary>
        public bool SDL2Enabled;

        public string[] XInputNames = new string[]
        {
            "xinput1_3.dll"
        };
        public string[] XInputCopies = new string[]
        {
            ""
        };

        /// <summary>
        /// If xinput is enabled, if rerouting should be enabled (basically is we'll reroute directinput back to xinput,
        /// so we can track more than 4 gamepads on xinput at once)
        /// </summary>
        public bool XInputReroute = false;

        /// <summary>
        /// If our custom dll should set the window size and position locally, instead of the handler
        /// (inconsistent with some window sizes, look at Borderlands2.js for an example usage)
        /// </summary>
        [Dynamic]
        public bool SetWindowSize;

        /// <summary>
        /// If our custom DLL should hook into the game's window and fake Window's events
        /// so we never leave focus. Depends on the ForceFocusWindowName variable
        /// (used for games that don't work when out of focus. See Borderlands.js)
        /// </summary>
        [Dynamic]
        public bool ForceFocus;

        /// <summary>
        /// If force focus is enabled, this is the window we are attaching ourselves to
        /// and the window we are going to keep on top.
        /// This is used in a very specific case even out
        /// </summary>
        [Dynamic]
        public string ForceFocusWindowName = "";

        public bool BlockMouseEvents = true;
        public bool BlockKeyboardEvents = true;
        public bool BlockInputEvents = true;

        public bool FixResolution = false;
        public bool FixPosition = false;

        public bool UseAlpha8CustomDll = false;

        public int WindowX = 0;
        public int WindowY = 0;
        public int ResWidth = 0;
        public int ResHeight = 0;

        public bool EnableMKBInput = false;
    }
}
