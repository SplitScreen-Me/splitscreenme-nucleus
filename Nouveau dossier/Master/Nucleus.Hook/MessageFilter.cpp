#include "pch.h"
#include "Logging.h"
#include "FakeMouse.h"
#include "InstallHooks.h"
#include "Globals.h"
#include <windowsx.h>
#include "Piping.h"
#include "ReRegisterRawInput.h"
#include <mutex>

bool filterMessageCalledAtLeastOnce = false;

//WNDPROC originalWndProc = nullptr;

BOOL filterMessage(const LPMSG lpMsg)
{
	const auto msg = lpMsg->message;
	const auto wParam = lpMsg->wParam;
	const auto lParam = lpMsg->lParam;

#define ALLOW return 1;
	//Massive performance benefits for returning a successful WM_NULL compared to causing an error in the application. (see Deep Rock Galactic mouse movement)
#define BLOCK memset(lpMsg, 0, sizeof(MSG)); return 1;
			
	if (!filterMessageCalledAtLeastOnce)
	{
		filterMessageCalledAtLeastOnce = true;
		DEBUGLOG("First filter message call.\n");
	}
	
	if (!ReRegisterRawInput::hasReRegisteredRawInput && Globals::options.reRegisterRawInput && (((msg & WM_INPUT) == WM_INPUT) || (msg == WM_INPUT)))
	{
		ReRegisterRawInput::hasReRegisteredRawInput = true;

		DEBUGLOG("Detected raw input window from filterMessage");
		
		//We found the window that this process using for raw input. Now re-register raw input for it.
		ReRegisterRawInput::reRegisterRawInput(lpMsg->hwnd);
	}
	
	//Filter raw input
	if (msg == WM_INPUT && Globals::options.filterRawInput)
	{
		lpMsg->wParam = RIM_INPUT;//While in foreground

		
		const auto sorh = sizeof(RAWINPUTHEADER);
		UINT dwSize = sorh;
		static RAWINPUT raw[sorh];
		
		if (//0 == GetRawInputData(reinterpret_cast<HRAWINPUT>(lpMsg->lParam), RID_HEADER, nullptr, &dwSize, sorh) &&
//			dwSize == sorh &&
			0 != GetRawInputData(reinterpret_cast<HRAWINPUT>(lpMsg->lParam), RID_HEADER, raw, &dwSize, sorh)
			)
		{
			if (raw->header.dwType == RIM_TYPEMOUSE)
			{
				if(raw->header.hDevice == FakeMouse::allowedMouseHandle)
				{
					ALLOW;
				}
				
				BLOCK;
			}
			
			if (raw->header.dwType == RIM_TYPEKEYBOARD)
			{
				if (raw->header.hDevice == FakeMouse::allowedKeyboardHandle)
				{
					ALLOW;
				}

				BLOCK;
			}
		}
		else
		{
			//Probably a broken message.
			BLOCK;
		}
	}

	//Legacy input filter
	if (Globals::options.legacyInput)
	{
		if (msg == WM_MOUSEMOVE)
		{
			if ((static_cast<int>(wParam) & 0b10000000) > 0) //Signature for message sent from USS (C#)
			{
				if (FakeMouse::useAbsoluteCursorPos == false)
				{
					if (Globals::options.updateAbsoluteFlagInMouseMessage)
					{
						const int x = GET_X_LPARAM(lParam);
						const int y = GET_Y_LPARAM(lParam);

						//TODO: this is inside USS signature, is it actually used??
						if (!(x == 0 && y == 0) && !(x == FakeMouse::lastX && y == FakeMouse::lastY))
							// - Minecraft (GLFW/LWJGL) will create a WM_MOUSEMOVE message with (0,0) AND another with (lastX, lastY) 
							//whenever a mouse button is clicked, WITHOUT calling SetCursorPos
							// - This would cause absoluteCursorPos to be turned on when it shouldn't.
						{
							FakeMouse::updateAbsoluteCursorCheck();
						}

						if (x != 0)
							FakeMouse::lastX = x;

						if (y != 0)
							FakeMouse::lastY = y;

						lpMsg->lParam = MAKELPARAM(FakeMouse::getAndUpdateFakeX(), FakeMouse::getAndUpdateFakeY());
						ALLOW;
					}
					BLOCK;
				}
				//pMsg->lParam = MAKELPARAM(absoluteX, absoluteY);
				ALLOW;
			}
			BLOCK;
		}
	}

	if(msg == WM_KILLFOCUS && Globals::options.preventWindowDeactivation)
	{
		BLOCK;
	}
	
	//USS signature is 1 << 7 or 0b10000000 for WM_MOUSEMOVE(0x0200). If this is detected, allow event to pass
	if (Globals::options.filterMouseMessages)
	{
		FakeMouse::InternalGetCursorPosition(&(lpMsg->pt));
		
		if (msg == WM_KILLFOCUS || /*msg == WM_ACTIVATE && wParam == 0 ||*/ msg == WM_CAPTURECHANGED /*|| msg == WM_ACTIVATE && static_cast<int>(lParam) == reinterpret_cast<int>(hWnd)*/)
		{
			BLOCK;
		}
		
		if (msg == WM_MOUSEMOVE && (static_cast<int>(wParam) & 0b10000000) > 0)
		{
			if (!Globals::options.legacyInput)
			{
				lpMsg->lParam = MAKELPARAM(*(Piping::memBuf), *(Piping::memBuf + 1));
			}
			ALLOW;
		}

		// || Msg == 0x00FF
		if ((msg >= WM_XBUTTONDOWN && msg <= WM_XBUTTONDBLCLK) || msg == WM_MOUSEMOVE || msg
			== WM_MOUSEHOVER || msg == WM_MOUSELEAVE || msg == WM_MOUSEWHEEL || msg == WM_SETCURSOR || msg ==
			WM_NCMOUSELEAVE || msg == WM_KILLFOCUS || msg == WM_CAPTURECHANGED) //Other mouse events. 
		{
			BLOCK;
		}
		
		if (msg == WM_MOUSEACTIVATE)
		{
			lpMsg->wParam = reinterpret_cast<WPARAM>(Globals::hWnd);
			lpMsg->lParam = 1;
			ALLOW;
		}

		if (msg == WM_ACTIVATE)
		{
			lpMsg->lParam = 1;
			lpMsg->wParam = 0;
			ALLOW;
		}

		if (msg == WM_NCACTIVATE)
		{
			BLOCK;
			//lpMsg->wParam = TRUE;
			//lpMsg->lParam = NULL;
			//ALLOW;
		}

		if (msg == WM_ACTIVATEAPP)
		{
			lpMsg->wParam = TRUE;
			lpMsg->lParam = NULL;
			ALLOW;
		}
	}

	ALLOW;

#undef ALLOW
#undef BLOCK
}

LRESULT CALLBACK WndProc_Hook(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
{
	//if (uMsg == WM_KILLFOCUS && options.preventWindowDeactivation)
	//{
	//	return -1;
	//}

	////There is no point using WndProc filter for this as it only hooks one window
	///*if (hasReRegisteredRawInput && uMsg == WM_INPUT)
	//{
	//	hasReRegisteredRawInput = false;

	//	//We found the window that this process using for raw input. Now re-register raw input for it.
	//	reRegisterRawInput(hwnd);
	//}*/
	//
	//return CallWindowProc(originalWndProc, hwnd, uMsg, wParam, lParam);

	switch (uMsg)
	{
	case WM_KILLFOCUS:
	{
		//SetFocus(hWnd);
		return -1;
	}
	default:
		DefWindowProc(hwnd, uMsg, wParam, lParam);
	}
	return 1;
}


BOOL WINAPI GetMessageA_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
{
	const auto ret = GetMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);

	return ret == -1 ? -1 : filterMessage(lpMsg);
}

BOOL WINAPI GetMessageW_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
{
	const auto ret = GetMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);

	return ret == -1 ? -1 : filterMessage(lpMsg);
}

BOOL WINAPI PeekMessageA_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
{
	const auto ret = PeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);

	return ret == FALSE ? FALSE : ((1+filterMessage(lpMsg))/2);//TODO: filterMessage returns -1 but PeekMessage expects FALSE(0): is this okay?
}

BOOL WINAPI PeekMessageW_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
{
	const auto ret = PeekMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
	
	return ret == FALSE ? FALSE : ((1 + filterMessage(lpMsg)) / 2);
}

UINT WINAPI GetRawInputData_Hook(
	HRAWINPUT hRawInput,
	UINT      uiCommand,
	RAWINPUT* pData,
	PUINT     pcbSize,
	UINT      cbSizeHeader)
{
	auto ret = GetRawInputData(hRawInput, uiCommand, pData, pcbSize, cbSizeHeader);
	
	if (pData != NULL && ret != 0)
	{
		pData->header.wParam = RIM_INPUT; // Sent in foreground
	}

	return ret;
}



void installMessageFilterHooks()
{
	if (Globals::options.filterRawInput)
	{
		installHook(TEXT("user32"), "GetRawInputData", GetRawInputData_Hook);
	}
	
	DEBUGLOG("Injecting message filter hooks\n");
	installHook(TEXT("user32"), "GetMessageA", GetMessageA_Hook);
	installHook(TEXT("user32"), "GetMessageW", GetMessageW_Hook);

	installHook(TEXT("user32"), "PeekMessageA", PeekMessageA_Hook);
	installHook(TEXT("user32"), "PeekMessageW", PeekMessageW_Hook);
	
	//TODO: filterMessage doesn't work for PreventWindowDeactivation?
	//TODO: This method seems to cause CSGO/Minecraft to not respond? "Hook Injection Complete" is never printed to the log. API Monitor shows calls being repeated infinitely?
	//Using original method instead:
	if (Globals::options.preventWindowDeactivation)
	{
		DEBUGLOG("Injecting WndProc message filter\n");
		//originalWndProc = reinterpret_cast<WNDPROC>(GetWindowLongPtr(hWnd, GWLP_WNDPROC));
		
		SetWindowLongPtr(Globals::hWnd, GWLP_WNDPROC, (LONG_PTR)WndProc_Hook);
	}
}
