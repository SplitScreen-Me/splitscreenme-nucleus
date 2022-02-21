#pragma once
#include <cstdint>
#include <windows.h>

namespace ProtoHost
{

extern HWND rawInputHwnd;

extern intptr_t lastKeypressKeyboardHandle;
extern intptr_t lastMouseClicked;

extern bool lockInputWithTheEndKey;
extern bool lockInputSuspendsExplorer;
extern bool freezeGameInputWhileInputNotLocked;

void InitialiseRawInput();

}
