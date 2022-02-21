#pragma once
#include <Windows.h>
#include <easyhook.h>

NTSTATUS installHook(LPCSTR moduleHandle, LPCSTR proc, void* callBack);
NTSTATUS installHookEx(LPCSTR moduleHandle, LPCSTR proc, void* callBack, bool isOrdinal);

void installSetWindowHook(int width, int height, int posx, int posy);

void installFocusHooks();

void installHideCursorHooks();

void installSetCursorPosHook();
void installGetCursorPosHook();

void installGetAsyncKeyStateHook();
void installGetKeyStateHook();
void installGetKeyboardStateHook();

void installMessageFilterHooks();

void installXInputHooks();