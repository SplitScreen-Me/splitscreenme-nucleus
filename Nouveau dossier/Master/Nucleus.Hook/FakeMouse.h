#pragma once

namespace FakeMouse
{
	extern int fakeX, fakeY; //Delta X/Y

	//extern int absoluteX, absoluteY;

	extern int useAbsoluteCursorPosCounter; // 0/1/2/3/... : FALSE, requiredAbsCount : TRUE
	extern const int REQUIRED_ABS_COUNT;
	extern int originX, originY;
	extern int lastX, lastY;

	extern HANDLE allowedMouseHandle; //We will allow raw input from this mouse handle.
	extern HANDLE allowedKeyboardHandle;

	extern bool useAbsoluteCursorPos;

	void updateAbsoluteCursorCheck();

	void setCursorVisibility(bool show);

	int getAndUpdateFakeX();
	int getAndUpdateFakeY();

	void InternalGetCursorPosition(LPPOINT lpPoint);
}