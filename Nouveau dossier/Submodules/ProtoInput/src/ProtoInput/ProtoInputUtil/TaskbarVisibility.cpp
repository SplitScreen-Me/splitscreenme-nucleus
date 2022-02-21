#include "include/protoinpututil.h"
#include <windows.h>

#define _INC_SHELLAPI
#include "shellapi.h"

extern "C" __declspec(dllexport) void SetTaskbarVisibility(bool autoHide, bool alwaysOnTop)
{
	APPBARDATA message{};
	message.cbSize = sizeof(APPBARDATA);
	message.lParam = (autoHide ? ABS_AUTOHIDE : 0) | (alwaysOnTop ? ABS_ALWAYSONTOP : 0);

	SHAppBarMessage(ABM_SETSTATE, &message);
}


extern "C" __declspec(dllexport) void GetTaskbarVisibility(bool* autoHide, bool* alwaysOnTop)
{
	APPBARDATA message{};
	message.cbSize = sizeof(APPBARDATA);

	auto ret = SHAppBarMessage(ABM_GETSTATE, &message);
	
	*autoHide = (ret & ABS_AUTOHIDE) != 0;
	*alwaysOnTop = (ret & ABS_ALWAYSONTOP) != 0;
}