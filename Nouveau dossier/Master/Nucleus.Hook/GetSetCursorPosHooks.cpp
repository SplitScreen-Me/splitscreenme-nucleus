#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"
#include "Globals.h"
#include "FakeMouse.h"
#include "Piping.h"
using namespace FakeMouse;

BOOL WINAPI GetCursorPos_Hook(LPPOINT lpPoint)
{
	if (lpPoint)
	{
		InternalGetCursorPosition(lpPoint);

		updateAbsoluteCursorCheck();
	}
	return true;
}

BOOL WINAPI SetCursorPos_Hook(int X, int Y)
{
	POINT p;
	p.x = X;
	p.y = Y;

	//SetCursorPos require screen coordinates (relative to 0,0 of monitor)
	ScreenToClient(Globals::hWnd, &p);

	originX = p.x;
	originY = p.y;

	if (!Globals::options.legacyInput)
	{
		*(Piping::memBuf) = p.x;
		*(Piping::memBuf + 1) = p.y;
	}
	else
	{
		fakeX = p.x;
		fakeY = p.y;
		//*(Piping::memBuf + 2) = p.x;
		//*(Piping::memBuf + 3) = p.y;

		useAbsoluteCursorPosCounter = 0;
		useAbsoluteCursorPos = false;
	}

	return TRUE;
}

void installSetCursorPosHook()
{
	DEBUGLOG("Injecting SetCursorPos hook\n");
	installHook("user32", "SetCursorPos", SetCursorPos_Hook);
}


void installGetCursorPosHook()
{
	DEBUGLOG("Injecting GetCursorPos hook\n");
	installHook("user32", "GetCursorPos", GetCursorPos_Hook);
}