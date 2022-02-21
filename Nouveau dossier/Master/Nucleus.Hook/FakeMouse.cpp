#include "pch.h"
#include "Logging.h"
#include "FakeMouse.h"
#include "Globals.h"
#include "Piping.h"

namespace FakeMouse
{
	int fakeX, fakeY; //Delta X/Y

	//int absoluteX;
	//int absoluteY;

	int useAbsoluteCursorPosCounter = 0; // 0/1/2/3/... : FALSE, requiredAbsCount : TRUE
	const int REQUIRED_ABS_COUNT = 40;
	int originX, originY;

	HANDLE allowedMouseHandle = HANDLE(-1); //We will allow raw input from this mouse handle.
	HANDLE allowedKeyboardHandle = HANDLE(-1);

	bool useAbsoluteCursorPos = true;

	int lastX, lastY;

	bool sentVisibility = true;//The visibility last sent to the C# side

	int getAndUpdateFakeX()
	{
		const int deltaX = *(Piping::memBuf + 2);
		if (deltaX  != 0) *(Piping::memBuf + 2) = 0;

		fakeX += deltaX;
		return fakeX;
	}

	int getAndUpdateFakeY()
	{
		const int deltaY = *(Piping::memBuf + 3);
		if (deltaY != 0) *(Piping::memBuf + 3) = 0;

		fakeY += deltaY;
		return fakeY;
	}
	
	void updateAbsoluteCursorCheck()
	{
		if (Globals::options.legacyInput && !useAbsoluteCursorPos && ++useAbsoluteCursorPosCounter == REQUIRED_ABS_COUNT)
		{
			//We assume we're in a menu and need absolute cursor pos
			useAbsoluteCursorPos = true;
		}
	}

	void setCursorVisibility(const bool show)
	{
		if (show != sentVisibility)
		{
			BYTE buffer[9] = { 0x06, 0, 0, 0, (show ? 1 : 0), 0, 0, 0, 0 };

			DWORD bytesRead = 0;

			const BOOL result = WriteFile(
				Piping::hPipeWrite,
				buffer,
				9 * sizeof(BYTE),
				&bytesRead,
				nullptr
			);

			if (result == FALSE)
			{
				DEBUGLOG("SetCursorVisibility fail, error = " << GetLastError() << "\n")
			}

			sentVisibility = show;
		}
	}

	void InternalGetCursorPosition(LPPOINT lpPoint)
	{
		if (!Globals::options.legacyInput || useAbsoluteCursorPos)
		{
			//Absolute mouse position (always do this if legacy input is off)
			lpPoint->x = *(Piping::memBuf);
			lpPoint->y = *(Piping::memBuf + 1);
		}
		else
		{
			//Delta mouse position
			lpPoint->x = getAndUpdateFakeX();
			lpPoint->y = getAndUpdateFakeY();
		}

		ClientToScreen(Globals::hWnd, lpPoint);
	}
}