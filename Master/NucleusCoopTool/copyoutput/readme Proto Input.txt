Proto Input

This page serves as a guide to how Proto Input works, and how to use it to setup a Nucleus Co-op handler.

-What is Proto Input?

Proto Input is the set of libraries used by Nucleus Co-op to inject games with hooks for input (mainly multiple keyboards/mice) as well as window focus faking, etc. Proto Input is a massive improvement over the original hooks inside Nucleus Co-op and Universal Split Screen. It's highly recommended to update any handlers using the old mouse/keyboard hooks to Proto Input as input will work much more reliably.

-How it works 

The main code for Proto Input is inside ProtoInputHooks32/64.dll. This needs to be injected into the game process. The main API, inside ProtoInputLoader32/64.dll, contains two methods of injection. First is EasyHook startup injection, which creates the game process and immediately injects into it. This is necessary for games that call a function at startup which needs to be hooked (e.g. creating a mutex). The other method is runtime injection, which injects into an existing process. The API (in ProtoInputLoader) is then called from a "host" process, e.g. Nucleus or ProtoInputHost. In principle, the hooks can be injected from any dll injector.

Before the dll is injected, ProtoInputLoader will open a named pipe (a tunnel used for communication) with a name related to the injected process ID, e.g. ProtoInput1234. Once the dll is injected, it will connect to the pipe. All data sent to the dll is done through this pipe. At this point, the dll will not have injected any hooks. Commands to inject hooks (or set data, etc) are then done by calling the API in ProtoInputLoader. For example, in Nucleus, it would now inject all the hooks set in the handler.

-How to use Proto Input

Proto Input is standalone from Nucleus Co-op, so it's usually fastest to setup the options for a game first using the GUI, then make it into a Nucleus Co-op handler.

-Proto Input Host

Run ProtoInputHost.exe

There are 3 methods to select a process to inject:

    First is (2), for startup injection: browse the game executable. This will then add an instance to the instances list (1).
    Second is (3): focus onto the running game's window, e.g. by alt+tab or clicking on it, then go back into the GUI. Focused process should display the name of the process, then click the button to add it to the instance list.
    Third is (4): searching through all running processes

You can add as many instances as you need, although for testing 1 is usually enough. Each instance can have different input devices attached. Set the input devices in the (5) panel for selected instance.

Once you have the instances setup, you can change the settings. Set the injection method in (6). For runtime injection, EasyHook Inject will work for most games. If it doesn't, try Remote Load Library. Some games that block injection may work with Stealth Inject. Startup injection (EasyHook Create and Inject) can work when runtime injection doesn't, and vice versa, so be sure to test all options.

The hooks settings in (7) will be sent to the injected dll immediately after injection. If you are only testing, you can leave these as default, or all off. You can then change the hooks from within the game later. If you want to save your settings as a preset, type a name into the textbox in (9), e.g. profile.json, then Save. You can then load it whenever.

In (8), The "Lock input with End key" means the "real" mouse and keyboard will be locked when you press End, so they won't interfere with the "fake" input. "Lock input also suspends explorer.exe" means when input is locked, the explorer.exe process will have some threads suspended. This prevents you from pressing alt+tab accidentally, opening the start menu, moving the taskbar, etc. It's highly recommended you have this on. "Freeze game input when input isn't locked" means that Proto Input will only send "fake" input when the "real" input is locked. This is to prevent you from controlling the real and fake mouse cursor at the same time.

When you have finished with settings, you can start injection with (8). If injection works, you might see a quick flash as a command window is opened then hidden. If you see an error, try changing the injection method.

-Proto Input GUI

Once the hooks dll is injected, you can open the hooks GUI. Press right ctrl + right alt + 1 to open the GUI for the first instance (and 2 for the 2nd, etc). The GUI lets you enable/disable hooks at runtime, change device settings, and everything else related to Proto Input. You should tweak the settings in the GUI until you get input working smoothly in-game.

After opening the GUI, the first thing to do is set "Freeze fake input while GUI opened" to disabled in (4). This options prevents Proto Input from sending "fake" input when the GUI is opened, however it's mostly redundant if you already have the "Freeze game input when input isn't locked" from Proto Input Host.

In the hooks panel (1), you can see what hooks are enabled (green) and disabled (red). The selected hook panel (2) gives you a description of the hook, and sometimes a details panel to change settings related to the hook or view some output information.

Opening the console window (3) lets you see the console output of Proto Input, as well as any standard output of the game. A useful trick is closing the console window to immediately terminate the game process.

You can set the keyboard and mouse devices in (5). This sets the devices that Proto Input will process to then simulate the "fake" input. Note that you can select more than 1 device of each type.

Some options can be configured in (3), (4) and (5): see below.

Message filters can be configured in the Message filter tab. Message "modifiers" will change the data sent in a Windows message, or selectively block messages, e.g. by only allowing messages that are simulated by Proto Input. Additionally you can set blocks on any messages, e.g. on WM_KILLFOCUS to prevent the game losing focus.

-Handler development with Nucleus Co-op

In your handler, you should always have the lines:

// This can be true or false, whether or not you want to allow raw keyboards/mice to be selected in the Nucleus GUI. Doesn't actually affect the hooks being injected or not.Game.SupportsMultipleKeyboardsAndMice = true;
// These options are deprecated, and must explicitly be set to false (since some default to true)Game.HookSetCursorPos = false;Game.HookGetCursorPos = false;Game.HookGetKeyState = false;Game.HookGetAsyncKeyState = false;Game.HookGetKeyboardState = false;Game.HookFilterRawInput = false;Game.HookFilterMouseMessages = false;Game.HookUseLegacyInput = false;Game.HookDontUpdateLegacyInMouseMsg = false;Game.HookMouseVisibility = false;Game.SendNormalMouseInput = false;Game.SendNormalKeyboardInput = false;Game.SendScrollWheel = false;Game.ForwardRawKeyboardInput = false;Game.ForwardRawMouseInput = false;Game.HookReRegisterRawInput = false;Game.HookReRegisterRawInputMouse = false;Game.HookReRegisterRawInputKeyboard = false;Game.DrawFakeMouseCursor = false;
// Set which injection method you want to useGame.ProtoInput.InjectStartup = false;Game.ProtoInput.InjectRuntime_RemoteLoadMethod = false;Game.ProtoInput.InjectRuntime_EasyHookMethod = true;Game.ProtoInput.InjectRuntime_EasyHookStealthMethod = false;
// Automatically locks input once the instances are set upGame.LockInputAtStart = false;
// This should almost always be on (see above)Game.LockInputSuspendsExplorer = true;
// This is identical to the setting in ProtoInputHost. You should leave this on so the "fake" input is only sent when the "real" input isn'tGame.ProtoInput.FreezeExternalInputWhenInputNotLocked = true;
// Sets the virtual keycode for locking input (End key by default)Game.LockInputToggleKey = 0x23;

-Hooks

You should use the Proto GUI to enable/disable hooks at runtime until you get input working. Once you have, you should add the corresponding lines to your handler. You should also do the same for messages filters and the other settings.
Hook	Description
Register Raw Input	Game.ProtoInput.RegisterRawInputHook = true;

This hook will unsubscribe the game from Raw Input (if it has already subscribed to it), and then subscribes Proto Input to receive raw input instead. Raw input received is forwarded back to the game if it uses raw input. Proto Input receives all keyboard/mouse input from this hook so it always needs to be enabled.
Get Raw Input Data	Game.ProtoInput.GetRawInputDataHook = true;

When forwarding the input back to the game, you need to create a handle to the input data. However these handles can only be created by Windows. To get around this, Proto Input will send a dummy WM_INPUT message to the game (from the Register Raw Input hook). The game then calls GetRawInputData using the dummy handle. This hook intercepts the call and passes in the saved data. This hook is required to make forwarding raw input work, so it should always be enabled.
Message Filter	Game.ProtoInput.MessageFilterHook = true;

Hooks the GetMessage and PeekMessage functions to intercept every windows message for every window owned by the process. (Note this method is superior to changing WndProc since it works for all windows without knowing what they are, and is less fragile). This hook is used to make the message filters and blocks work. This should always be enabled.
Get Cursor Position	Game.ProtoInput.GetCursorPosHook = true;

The GetCursorPos is one way of obtaining the cursor position, so this hook redirects it to the "fake" cursor position. Should always be enabled.
Set Cursor Position	Game.ProtoInput.SetCursorPosHook = true;

SetCursorPos sets the cursor position. This hook causes it to set the "fake" cursor instead. This is important in some games that will set the mouse cursor to the centre of the screen each frame, then calculate the difference in movement each frame. This hook also prevents moving the "real" cursor, which can be problematic. Should always be enabled.
Get Key State	Game.ProtoInput.GetKeyStateHook = true;

Hooks GetKeyState to return the "fake" keyboard state. You should almost always have this enabled even if it appears to have no effect: when using multiple keyboards at the game time, they may interfere without this hook.
Get Async Key State	Game.ProtoInput.GetAsyncKeyStateHook = true;

Similar to Get Key State. Async also tells the caller whether the key was pressed since the last call, so this can resolve issues where keys are pressed or release when they shouldn't. Should almost always be enabled.
Get Keyboard State	Game.ProtoInput.GetKeyboardStateHook = true;

Basically the same as Get Key State, except it returns the state of the entire keyboard. Should almost always be enabled.
Cursor Visibility	Game.ProtoInput.CursorVisibilityHook = true;
Game.ProtoInput.DontShowCursorWhenImageUpdated = false;

Monitors when the game tries to hide the cursor (e.g. when you close the menu in a first person game), and sets the visibility of the fake cursor instead. Should be almost always be enabled, except sometimes during debugging when you want to see where the fake mouse cursor is at all times. This hook also monitors calls to set the cursor image, so Proto Input will change the fake cursor to match the game's cursor image. Some games (notably Unity engine games) need DontShowCursorWhenImageUpdated enabled or the cursors will show when they shouldn't.
Clip Cursor	Game.ProtoInput.ClipCursorHook = true;
Game.ProtoInput.ClipCursorHookCreatesFakeClip = true;

Prevents the game from setting a bounding region about the mouse cursor. Some games will repeatedly clip the cursor when they receive the fake focus, so this should almost always be enabled. Fake clip means that if the game tries to clip (i.e. bound) the real cursor, it will bound the fake cursor instead.
Focus Hook	Game.ProtoInput.FocusHooks = true;

Hooks many functions (see the Details tab of the hook) related to the game checking if it's in the foreground or focused. Should always be enabled. Proto Input will repeatedly scan to find the main window, in case it changes.
Rename Handles	Game.ProtoInput.RenameHandles = [ "some mutex", "some other mutex" ];
Game.ProtoInput.RenameNamedPipes = [ "some named pipe", "some other named pipe" ];

Some games will use handles (of mutants, events or semaphores) or named pipes to prevent opening multiple instances. New instances will detect the handle opened by the previous instance, then refuse to open. Proto Input gets around this by renaming the handles to something random (e.g. "mutex" to "mutex2733894") so the new instances can't detect it. Since the handles are usually opened at startup, you will usually need startup injection. To get the names of these handles, use Process Explorer to browse the handles, then use some guesswork.
Xinput	Game.ProtoInput.XinputHook = true;
Game.ProtoInput.UseOpenXinput = false;
Game.ProtoInput.UseDinputRedirection = false;
Game.Hook.DInputEnabled = false;
Game.Hook.DInputForceDisable = false;
Game.Hook.XInputEnabled = false;
Game.Hook.XInputReroute = false;
Game.Hook.CustomDllEnabled = false;

Xinput is used by most games for controller input. This hook changes the controller indexes so each instance can receive input from a different device. The controller index can be set from Nucleus or Proto Input Host, or it can be edited at runtime in the details tab. Xinput by default has a maximum of 4 controllers. This restriction can be removed if you enable use of the Openxinput library. However, this still will only let you use standard xinput controllers (no cheap PS1 controller adapters). If you want to use any device, enable the Dinput to Xinput redirection, which takes input from Direct Input (which supports any controller) and translates it to Xinput in the hook. Usually Openxinput is the best choice since it will work with vibration and using both triggers simultaneously.
Direct Input	Game.ProtoInput.DinputDeviceHook = true;
Game.ProtoInput.DinputHookAlsoHooksGetDeviceState = false;
Game.Hook.DInputEnabled = false;
Game.Hook.DInputForceDisable = false;
Game.Hook.XInputEnabled = false;
Game.Hook.XInputReroute = false;
Game.Hook.CustomDllEnabled = false;

Some (few) games will use Direct Input instead of Xinput to get controller input. The Direct Input API involves creating an "interface" to receive input from a controller. Typically a game will create this interface at startup for the first or all controllers. This hook will redirect the interface to the GUID of the selected controller. Note that the controller GUID can only be set automatically through Nucleus (not at runtime).
If this method doesn't work, you can also try with DinputHookAlsoHooksGetDeviceState. This will additionally hook the GetDeviceState function, which is called whenever the game polls input. This method is highly un-recommended since there's no guarantee the game expects the data to be formatted in the same way that Proto Input does, which can lead to completely mesesed up input.
Set Window Position	Game.ProtoInput.SetWindowPosHook = true;

Prevents the game from calling SetWindowPos to resposition and resize the game window, instead forcing it to the resolution set by Nucleus. This will usually require using startup hooks.
Block Raw Input	Game.ProtoInput.BlockRawInputHook = true;

Blocks calls to GetRegisteredRawInputDevices and GetRawInputDeviceList to prevent the game from detecting raw input devices. You will usually need to use startup injection with this hook. If you enable this hook, you should disable the raw input filter, and add a block to WM_INPUT, like so: Game.ProtoInput.BlockedMessages = [ 0x00FF ];
Find Window	Game.ProtoInput.FindWindowHook = true;

Blocks calls to FindWindow and EnumWindow so the game cannot detect any windows. This is required for opening multiple instances of some games. You will usually need to use startup injection with this hook.
Create Single HID	Game.ProtoInput.CreateSingleHIDHook = true;

Hooks CreateFile so the game can only open the HID for the selected device (in Nucleus). This will usually require using startup injection.
Set Window Style	Game.ProtoInput.SetWindowStyleHook = true;

Hooks SetWindowLong and SetWindowLongPtr to prevent the game from re-adding a title bar after Nucleus has stripped it.

-Message filters & Filter	Description

Raw Input	Game.ProtoInput.RawInputFilter = true;

Filters the WM_INPUT messages (which send raw input data) to only allow the forwarded messages from the Register Raw Input hook. This will allow only input from the selected device(s). Also modifies the message to tell the game that input was received while the game was in the foreground. Should always be enabled.
Mouse Move	Game.ProtoInput.MouseMoveFilter = true;

One method of getting the mouse position/state is through the WM_MOUSEMOVE message, so this modifies the data to return the "fake" mouse state. **This filter is not recommended as it can be very CPU intensive (with multiple mice polling at hundreds of Hz), however it is required for input to work in some games.** Test with it enabled and disabled to check if you require it.
Mouse Activate, Window Activate, Window Activate App	Game.ProtoInput.MouseActivateFilter = true;
Game.ProtoInput.WindowActivateFilter = true;
Game.ProtoInput.WindowActvateAppFilter = true;

WM_MOUSEACTIVATE, WM_ACTIVATE and WM_ACTIVATEAPP all tell a window when it's activated or deactivated. These filters modify the messages to make the window think it's in the foreground and activated. Should always be enabled.
Mouse Wheel	Game.ProtoInput.MouseWheelFilter = true;

Filters the WM_MOUSEWHEEL messages to only pass "fake" input. This fixes the issue of the foreground window receiving two scrolls for each scroll. Should always be enabled.
Mouse Buttons	Game.ProtoInput.MouseButtonFilter = true;

Filters the mouse buttons messages to only pass "fake" input. This prevents any interaction from the "real" mouse cursor with our input. Should always be enabled.
Keyboard Buttons	Game.ProtoInput.KeyboardButtonFilter = true;

Same as the Mouse Buttons filter, for keyboard buttons. Should always be enabled.

-Message blocks

In the Block tab, you can select individual windows messages to block from the game. Usually a good one to block is WM_KILLFOCUS (0x0008). You can search for the hexadecimal code on MSDN, e.g. for WM_KILLFOCUS. Add the blocks to a handler like so:

// Add multiple blocks by separating with commasGame.ProtoInput.BlockedMessages = [ 0x0008 ]; // Blocks WM_KILLFOCUS

-Freeze input

Freezing input prevents Proto Input from sending any "fake" input to the game, it should not be confused with Locking input, which locks the "real" mouse and keyboard.

In the GUI settings, you can enable Freeze fake input at any time to freeze input. External freeze fake input is controlled by the host, either Nucleus or ProtoInputHost. The only way it's turned in is by "Freeze input when input not locked", from before. Freeze fake input while GUI opened freezes input when the Proto GUI is open. You usually don't want this, as it means you can't test input with the GUI open. It's usually unnecessary as input will usually be frozen from when the input is unlocked, so there won't be any interference anyway.

-Draw mouse cursor

Enable this for games that don't have an in-game cursor

Game.ProtoInput.DrawFakeCursor = false;

If you want to allow the fake cursor to not be bound by the window boundaries, enable this

Game.ProtoInput.AllowFakeCursorOutOfBounds = true;

This can be useful as a workaround in some games that do not correctly reset the mouse cursor position to the window centre.

Some games may detect mouse input with an offset, in which case some regions of the window would be inaccessible since Proto Input bounds the fake cursor to the window by default.

Game.ProtoInput.ExtendFakeCursorBounds = true;

You can get around this problem with this option: it will allow the fake cursor to go slightly beyond the window boundaries, so all regions should be accessible.

-Send messages

These settings will cause the Register Raw Input hook to send WM_MOUSEMOVE, etc messages when raw input is detected from the selected devices. These options should always be enabled

Game.ProtoInput.SendMouseWheelMessages = true;Game.ProtoInput.SendMouseButtonMessages = true;Game.ProtoInput.SendMouseMovementMessages = true;Game.ProtoInput.SendKeyboardButtonMessages = true;

-Focus message loop

Sometimes the Focus hook is not enough to trick the game into thinking it has focus, so the focus message loop will periodically send these messages that activate focus. For some games, some of these messages will actually break the focus, so you should test with each message. Usually it's best to enable WM_ACTIVATE, WM_SETFOCUS and WM_MOUSEACTIVATE, and leave the others disabled. You can also configure the interval between messages. Usually it's best to leave it as about 5ms.

Game.ProtoInput.EnableFocusMessageLoop = true;Game.ProtoInput.FocusLoopIntervalMilliseconds = 5;Game.ProtoInput.FocusLoop_WM_ACTIVATE = true;Game.ProtoInput.FocusLoop_WM_SETFOCUS = true;Game.ProtoInput.FocusLoop_WM_MOUSEACTIVATE = true;Game.ProtoInput.FocusLoop_WM_ACTIVATEAPP = false;Game.ProtoInput.FocusLoop_WM_NCACTIVATE = false;

-Multiple controller indices

Some games have a built-in split screen, typically with a maximum of 2 or 4 players. You can combine this with Proto Input to open multiple instance, each with multiple players. This is done in Nucleus by setting the individual controller index mappings within a handler. By default, Nucleus will tell Proto Input to use the controller index Player.GamepadId (i.e. the number labelled on the controller UI icon). To use multiple controller indices, set Game.ProtoInput.MultipleProtoControllers = true;. You then need to set Player.ProtoController1/2/3/4 for each player. A controller index of 0 means no controller, 1 means controller 1, etc. (Note that Player.GamepadId starts at zero, so if Player.GamepadId is 0 it actually means controller 1). A good idea is, for example with 3 players, to set controllers 1/4/7/10 to the first instance, 2/5/8/11 to the second instance and 3/6/9/12 to the third. Here is a snippet that does that:

var answers = ["No", "Yes"];Game.AddOption("Use native split screen?", "", "NativeSplitScreen", answers);
Game.ProtoInput.XinputHook = true;Game.ProtoInput.UseOpenXinput = true;
Game.Play = function(){  if (Context.PlayerID == 0) // So the code only runs once  {    if (Context.Options["NativeSplitScreen"] == "No")    {      // Always explicitly set this option, in case the user stops split screen and changes the option      Game.ProtoInput.MultipleProtoControllers = false;    }    else    {      Game.ProtoInput.MultipleProtoControllers = true;
      // The number of controller icons dragged onto the screens      var numPlayers = 0;
      for (var i = 0; i < PlayerList.Count; i++)      {        var player = PlayerList[i];
        if (player.IsXInput && player.ScreenIndex !== -1)        {          numPlayers++;        }
        // These might not be reset between runs, so let's reset them here        // 0 means no controller, 1 means controller 1, etc        player.ProtoController1 = 0;        player.ProtoController2 = 0;        player.ProtoController3 = 0;        player.ProtoController4 = 0;      }
      var controllerCounter = 1;      while (controllerCounter < numPlayers * 4)      {        for (var i = 0; i < PlayerList.Count; i++)        {          var player = PlayerList[i];
          if (player.IsXInput && player.ScreenIndex !== -1)          {            if (player.ProtoController1 == 0)            {              player.ProtoController1 = controllerCounter;              controllerCounter++;            }            else if (player.ProtoController2 == 0)            {              player.ProtoController2 = controllerCounter;              controllerCounter++;            }            else if (player.ProtoController3 == 0)            {              player.ProtoController3 = controllerCounter;              controllerCounter++;            }            else if (player.ProtoController4 == 0)            {              player.ProtoController4 = controllerCounter;              controllerCounter++;            }            else            {              // This instance has all its controllers set already            }          }        }      }    }  }}

-Advanced scripting

The Proto Input API inside ProtoInputLoader is called by Nucleus when you set an option like Game.ProtoInput.DrawFakeCursor = false;, for example. The "normal" way to write a handler is to write all the options at the top of your handler, then the corresponding hooks/etc will be installed once Proto Input is injected.

There are some drawbacks to immediately installing hooks though: the game will no longer respond to the "real" cursor, which can be problematic when setting up the instances, especially if you have some instances that use controllers. We can get around this by directly calling the API from within the Nucleus handler. A slick technique is to install hooks/etc when the "real" input is locked, and uninstall the hooks/etc when the input is unlocked. This will mean the game always responds to either real input or fake input.

You can view the Proto Input API in the file nucleuscoop/Master/NucleusGaming/Coop/ProtoInput/protoinput.cs. (Ignore the APIs in the 32/64 vesrions, look to the bottom of the file). For example, we can call:

public void InstallHook(uint instanceHandle, ProtoHookIDs hookID)

in a Nucleus handler as:

ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetCursorPosHookID);

To do this for each player, we can do this:

for (var i = 0; i < PlayerList.Count; i++){    var player = PlayerList[i];    ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetCursorPosHookID);}

This can be made as complicated as necessary, e.g. by checking if the player is using a keyboard/mouse or a controller.

You can run code on the input lock/unlock callbacks like so:

Game.ProtoInput.OnInputLocked = function(){    // blah}
Game.ProtoInput.OnInputUnlocked = function(){    // blah}

One important function call is ProtoInput.SetRawInputBypass(player.ProtoInputInstanceHandle, true or false);. This will let any raw input be forwarded to the game, regardless if the input device is selected or not. Note you must still have the RegisterRawInputDevices and GetRawInputData hook enabled. This function is important in games that use raw input in the menus, for example, so then you can use 'real' input when the input is unlocked.

For a full example, this will default to not injecting any hooks, then install almost everything when input is locked, and uninstall it if input is locked again. Copy-pasting this example should usually be enough for most handlers.

Game.SupportsMultipleKeyboardsAndMice = true;
// Deprecated optionsGame.HookSetCursorPos = false;Game.HookGetCursorPos = false;Game.HookGetKeyState = false;Game.HookGetAsyncKeyState = false;Game.HookGetKeyboardState = false;Game.HookFilterRawInput = false;Game.HookFilterMouseMessages = false;Game.HookUseLegacyInput = false;Game.HookDontUpdateLegacyInMouseMsg = false;Game.HookMouseVisibility = false;Game.SendNormalMouseInput = false;Game.SendNormalKeyboardInput = false;Game.SendScrollWheel = false;Game.ForwardRawKeyboardInput = false;Game.ForwardRawMouseInput = false;Game.DrawFakeMouseCursor = false;
Game.LockInputAtStart = false;Game.LockInputSuspendsExplorer = true;Game.ProtoInput.FreezeExternalInputWhenInputNotLocked = true;Game.LockInputToggleKey = 0x23;
Game.ProtoInput.InjectStartup = false;Game.ProtoInput.InjectRuntime_RemoteLoadMethod = false;Game.ProtoInput.InjectRuntime_EasyHookMethod = true;Game.ProtoInput.InjectRuntime_EasyHookStealthMethod = false;
// This hook should always be on, regardless of input lockGame.ProtoInput.RegisterRawInputHook = true;
// This can always be enabled, since we are going to allow/disallow raw input with the bypass functionGame.ProtoInput.GetRawInputDataHook = true;
// We will be installing/uninstalling the filters manually so there's no need toGame.ProtoInput.MessageFilterHook = true;Game.ProtoInput.ClipCursorHook = true;Game.ProtoInput.FocusHooks = true;
// These aren't problematic to leave on, since the external "freeze fake input input" when real input ISN'T locked will prevent these from interferingGame.ProtoInput.SendMouseWheelMessages = true;Game.ProtoInput.SendMouseButtonMessages = true;Game.ProtoInput.SendMouseMovementMessages = true;Game.ProtoInput.SendKeyboardButtonMessages = true;
// Handled on input lock/unlock, don't enable this by defaultGame.ProtoInput.EnableFocusMessageLoop = false;//Game.ProtoInput.FocusLoopIntervalMilliseconds = 5;//Game.ProtoInput.FocusLoop_WM_ACTIVATE = true;//Game.ProtoInput.FocusLoop_WM_ACTIVATEAPP = true;//Game.ProtoInput.FocusLoop_WM_NCACTIVATE = true;//Game.ProtoInput.FocusLoop_WM_SETFOCUS = true;//Game.ProtoInput.FocusLoop_WM_MOUSEACTIVATE = true;
Game.ProtoInput.DrawFakeCursor = false; // This is set in the input locking
Game.ProtoInput.BlockedMessages = [ 0x0008 ]; // Blocks WM_KILLFOCUS
Game.ProtoInput.RenameHandlesHook = false;Game.ProtoInput.RenameHandles = [];Game.ProtoInput.RenameNamedPipes = [];
// We can leave these on and not worry about input lock. // It doesn't matter if controller input still works since it won't get in the wayGame.ProtoInput.XinputHook = true;Game.ProtoInput.UseOpenXinput = true;Game.ProtoInput.UseDinputRedirection = false;
// The old Nucleus options. Don't use these with the Xinput hookGame.Hook.DInputEnabled = false;Game.Hook.DInputForceDisable = false;Game.Hook.XInputEnabled = false;Game.Hook.XInputReroute = false;Game.Hook.CustomDllEnabled = false;

Game.ProtoInput.AutoHideTaskbar = true;
Game.ProtoInput.OnInputLocked = function(){    for (var i = 0; i < PlayerList.Count; i++)    {        var player = PlayerList[i];
        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetCursorPosHookID);        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.SetCursorPosHookID);        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetKeyStateHookID);        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetAsyncKeyStateHookID);        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetKeyboardStateHookID);        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.CursorVisibilityStateHookID);
        ProtoInput.InstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.FocusHooksHookID);
        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.RawInputFilterID);
        // Avoid the mouse move filter unless absolutely necessary as it can massively affect performance if the game gets primary input from mouse move moessages        //ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseMoveFilterID);
        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseActivateFilterID);        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.WindowActivateFilterID);        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.WindowActivateAppFilterID);        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseWheelFilterID);        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseButtonFilterID);        ProtoInput.EnableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.KeyboardButtonFilterID);
        ProtoInput.SetDrawFakeCursor(player.ProtoInputInstanceHandle, false);
        ProtoInput.StartFocusMessageLoop(player.ProtoInputInstanceHandle, 5, true, true, true, true, true);
        // Disable the bypass: let the input be processed by Proto Input        ProtoInput.SetRawInputBypass(player.ProtoInputInstanceHandle, false);    }}
Game.ProtoInput.OnInputUnlocked = function(){    for (var i = 0; i < PlayerList.Count; i++)    {        var player = PlayerList[i];
        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetCursorPosHookID);        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.SetCursorPosHookID);        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetKeyStateHookID);        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetAsyncKeyStateHookID);        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.GetKeyboardStateHookID);        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.CursorVisibilityStateHookID);
        // Intentionally disable focus so all the instances don't respond to input at the same time        ProtoInput.UninstallHook(player.ProtoInputInstanceHandle, ProtoInput.Values.FocusHooksHookID);
        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.RawInputFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseMoveFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseActivateFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.WindowActivateFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.WindowActivateAppFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseWheelFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.MouseButtonFilterID);        ProtoInput.DisableMessageFilter(player.ProtoInputInstanceHandle, ProtoInput.Values.KeyboardButtonFilterID);
        ProtoInput.SetDrawFakeCursor(player.ProtoInputInstanceHandle, false);
        // Intentionally disable focus so all the instances don't respond to input at the same time        ProtoInput.StopFocusMessageLoop(player.ProtoInputInstanceHandle);
        // Enable the bypass: allow any raw input to pass        ProtoInput.SetRawInputBypass(player.ProtoInputInstanceHandle, true);    }}