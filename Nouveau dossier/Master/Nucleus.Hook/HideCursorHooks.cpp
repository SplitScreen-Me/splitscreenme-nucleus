#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"
#include "Globals.h"
#include "FakeMouse.h"

int WINAPI ShowCursor_Hook(BOOL bShow)
{
	if (Globals::options.hideCursor)
	{
		return ShowCursor(FALSE);
	}

	FakeMouse::setCursorVisibility(bShow == TRUE);
	if (bShow == FALSE) ShowCursor(FALSE);
	return bShow == TRUE ? 0 : -1;
}

HCURSOR WINAPI SetCursor_Hook(HCURSOR hCursor)
{
	if (Globals::options.hideCursor)
	{
		return SetCursor(nullptr);
	}

	FakeMouse::setCursorVisibility(hCursor != nullptr);
	if (hCursor == nullptr) SetCursor(nullptr);
	return hCursor;
}

void installHideCursorHooks()
{
	DEBUGLOG("Injecting ShowCursor and SetCursor hooks\n");
	installHook("user32", "ShowCursor", ShowCursor_Hook);
	installHook("user32", "SetCursor", SetCursor_Hook);
}