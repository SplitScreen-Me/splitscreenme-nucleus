#pragma once

// OpenXinput uses the same definitions as the normal xinput so to avoid clashses,
// wrap it in a cpp file that only include the OpenXinput header

#include <Windows.h>

namespace OpenXinput
{

DWORD WINAPI ProtoOpenXinputGetState(DWORD dwUserIndex, void* pState, bool extended);

DWORD WINAPI ProtoOpenXinputSetState(DWORD dwUserIndex, void* pVibration);

DWORD WINAPI ProtoOpenXinputGetCapabilities(DWORD dwUserIndex, DWORD dwFlags, void* pCapabilities);

}