#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"
#include "KeyStates.h"
using namespace KeyStates;

SHORT WINAPI GetAsyncKeyState_Hook(int vKey)
{
	return isVkeyDown(vKey) ? 0b1000000000000000 : 0;
}

SHORT WINAPI GetKeyState_Hook(int nVirtKey)
{
	return isVkeyDown(nVirtKey) ? 0b1000000000000000 : 0;
}

BOOL WINAPI GetKeyboardState_Hook(PBYTE lpKeyState)
{
	memset(lpKeyState, 0, 256);

	for (int vkey = 0; vkey < 256; vkey++)
	{
		lpKeyState[vkey] = isVkeyDown(vkey) ? 0b10000000 : 0;
	}

	return TRUE;
}

void installGetAsyncKeyStateHook()
{
	DEBUGLOG("Injecting GetAsyncKeyState hook\n");
	installHook(TEXT("user32"), "GetAsyncKeyState", GetAsyncKeyState_Hook);
}

void installGetKeyStateHook()
{
	DEBUGLOG("Injecting GetKeyState hook\n");
	installHook(TEXT("user32"), "GetKeyState", GetKeyState_Hook);
}

void installGetKeyboardStateHook()
{
	DEBUGLOG("Injecting GetKeyboardState hook\n");
	installHook(TEXT("user32"), "GetKeyboardState", GetKeyboardState_Hook);
}