#pragma once

// Note: when adding any functions, make sure to add the pragma comments in protoinpututilpragma.h

// Returns the thread handle of the loop thread
extern "C" __declspec(dllexport) unsigned int LockInput(bool lock);

// What this does and why:
// explorer.exe is responsible for the desktop window, the taskbar, start menu etc.
// If the explorer.exe process is killed, all this vanishes
// (however this also appears to break the low-level mouse/keyboard hooks and raw input for some reason when exporer.exe is restarted)
// Instead, these functions will suspend all the threads of explorer.exe so it doesn't do anything.
// This now means you can't accidentally click the start menu, or open the start menu with the windows key, alt+tab, win+tab, right click the desktop, etc...
// 
// Another benifit of suspending the threads is that when explorer.exe is running, it appears to override the alt+tab menu with a fancier Windows 10 one
// (when explorer.exe is killed it uses an older style one).
// However when it's suspended, the new alt+tab menu does nothing so it doesn't use the fallback
//
// You MUST call RestartExplorer after you finish or the desktop will be broken and the only way out is to kill explorer.exe and restart it with task manager (if it opens)
extern "C" __declspec(dllexport) void SuspendExplorer();
extern "C" __declspec(dllexport) void RestartExplorer();

extern "C" __declspec(dllexport) void SetTaskbarVisibility(bool autoHide, bool alwaysOnTop);
extern "C" __declspec(dllexport) void GetTaskbarVisibility(bool* autoHide, bool* alwaysOnTop);