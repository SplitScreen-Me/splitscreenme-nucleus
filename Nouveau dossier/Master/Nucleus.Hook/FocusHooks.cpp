#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"
#include "Globals.h"

HWND WINAPI GetForegroundWindow_Hook()
{
	return Globals::hWnd;
}

HWND WINAPI WindowFromPoint_Hook(POINT Point)
{
	return Globals::hWnd;
}

HWND WINAPI GetActiveWindow_Hook()
{
	return Globals::hWnd;
}

BOOL WINAPI IsWindowEnabled_Hook(HWND hWnd)
{
	return TRUE;
}

HWND WINAPI GetFocus_Hook()
{
	return Globals::hWnd;
}

HWND WINAPI GetCapture_Hook()
{
	return Globals::hWnd;
}

HWND WINAPI SetCapture_Hook(HWND inputHwnd)
{
	return inputHwnd;
	//return NULL;
}

BOOL WINAPI ReleaseCapture_Hook()
{
	return TRUE;
}

HWND WINAPI SetActiveWindow_Hook(
	HWND input
)
{
	return input;
}

HWND WINAPI SetFocus_Hook(
	HWND input
)
{
	return input;
}

BOOL WINAPI SetForegroundWindow_Hook(
	HWND hWnd
)
{
	return true;
}

void installFocusHooks()
{
	DEBUGLOG("Injecting Focus hooks\n");
	installHook("user32", "GetForegroundWindow", GetForegroundWindow_Hook);
	installHook("user32", "WindowFromPoint", WindowFromPoint_Hook);
	installHook("user32", "GetActiveWindow", GetActiveWindow_Hook);
	installHook("user32", "IsWindowEnabled", IsWindowEnabled_Hook);
	installHook("user32", "GetFocus", GetFocus_Hook);
	installHook("user32", "GetCapture", GetCapture_Hook);
	installHook("user32", "SetCapture", SetCapture_Hook);
	installHook("user32", "ReleaseCapture", ReleaseCapture_Hook);
	installHook("user32", "SetActiveWindow", SetActiveWindow_Hook);
	installHook("user32", "SetFocus", SetFocus_Hook);

	installHook("user32", "SetForegroundWindow", SetForegroundWindow_Hook);
}