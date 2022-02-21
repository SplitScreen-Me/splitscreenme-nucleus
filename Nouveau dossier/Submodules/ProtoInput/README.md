## Proto Input
Proto Input is a set of libraries that enables split screen on PC games by hooking Windows functions and redirecting input from multiple keyboards/mice/controllers. ProtoInput contains many improvements over my previous works on [Universal Split Screen](https://universalsplitscreen.github.io/ "Universal Split Screen") and [ZeroFox\'s Nucleus Coop Mod](https://github.com/zerofox5866/nucleuscoop "ZeroFox's Nucleus Coop Mod"). The project is in a modular form so it can be easily used in any project with its C function API.

Proto Input is now included as part of [Nucleus Co-Op](https://nucleus-coop.github.io/). This is the best way to use Proto Input, as it is fully scriptable.

The main highlights are:

### In-game GUI control interface
The GUI interface can be opened by pressing Right Ctrl + Right Alt + 1/2/3/4... (depending on the instance index).
The GUI lets you enable/configure hooks, change input settings, Windows message filters, etc, all without restarting the game, so you can quickly setup a profile.

![Proto Input Hooks GUI](https://raw.githubusercontent.com/Ilyaki/ProtoInput/master/img/ProtoInputHooks.png)

### Proto Input Host
Proto Input Host is a GUI tool that uses the API interface to set up input and hooks for processes. 
You can effectively think of this as a souped up Universal Split Screen.

![Proto Input Host GUI](https://raw.githubusercontent.com/Ilyaki/ProtoInput/master/img/ProtoInputHost.png)

### Smooth input
Unlike my previous works, ProtoInput performs all input redirection from within the target process using hooks. This gives much smoother input, and better compatibility with games. Additionally I\'ve completely re-written most of the hooks. You should now be able to enable almost all hooks and have input working out of the box.

### API interface
ProtoInput is modular by design so it can be used in another project. An external project can inject the ProtoInput hooks into a process by calling the functions in `protoloader.h`, e.g.
```cpp
auto path = LR"(C:\WINDOWS\system32\notepad.exe)";
unsigned long pid;

// Use startup injection to create a process and inject the hooks
ProtoInstanceHandle instanceHandle = EasyHookInjectStartup(path, L"", 0, folderpath.c_str(), &pid);

// Let the hooks know it's the 1st index (So open the GUI with Right Ctrl + Right Alt + 1)
SetupState(instanceHandle, 1);

// Install the RegisterRawInput hook
InstallHook(instanceHandle, RegisterRawInputHookID);

// Tell the hooks to send mouse move, button, etc messages
SetupMessagesToSend(instanceHandle, true, true, true, true);

// Start a loop that sends WM_ACTIVATE, WM_ACTIVATEAPP, etc every 5 milliseconds
StartFocusMessageLoop(instanceHandle, 5, true, true, true, true, true);

// The processes was created in a suspended state, so now wake it up
WakeUpProcess(instanceHandle);
```

The API interface is C-style, so you can call it from almost any language. Some additional functionality (e.g. input locking) is provided in ProtoInputUtil, separate from the hooks.

### Some minor things
- Locking input, so the \'real\' mouse/keyboard does nothing, has been greatly improved, notably with suspening explorer.exe so you no longer accidentally alt+tab or open the Start menu.
- Startup/Runtime hooking has been merged into one. The procedure for each is almost identical in the API.
- ProtoInput will automatically detect and use a custom cursor if a game has one.
- I\'ve cleaned up my old Windows message filter system, to give more control over each filter and to be able to block individual messages. Some games are particularly fussy when it comes to filtering messages, so this can be very useful.
- More injection methods have been added, and some bugs fixed, so you can now inject hooks into games that didn\'t work before.
- You can open a console window from within the hooks GUI. This lets you see the debug output without having to open a debug log.
- (More of a note than an addition) You can attach a debugger to the game in Visual Studio, then set breakpoints/etc inside the hooks when debugging.
