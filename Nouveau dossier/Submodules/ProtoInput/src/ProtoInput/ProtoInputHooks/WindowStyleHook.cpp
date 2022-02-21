#include "WindowStyleHook.h"

namespace Proto
{

inline void FilterStyle(LONG& style)
{
	style &= ~(WS_BORDER | WS_SYSMENU | WS_DLGFRAME);
}

inline void FilterStylePtr(LONG_PTR& style)
{
	style &= ~(WS_BORDER | WS_SYSMENU | WS_DLGFRAME);
}

LONG WINAPI Hook_SetWindowLongA(
	HWND hWnd,
	int  nIndex,
	LONG dwNewLong
)
{
	if (nIndex == GWL_STYLE)
	{
		FilterStyle(dwNewLong);
	}
	return SetWindowLongA(hWnd, nIndex, dwNewLong);
}

LONG WINAPI Hook_SetWindowLongW(
	HWND hWnd,
	int  nIndex,
	LONG dwNewLong
)
{
	if (nIndex == GWL_STYLE)
	{
		FilterStyle(dwNewLong);
	}
	return SetWindowLongW(hWnd, nIndex, dwNewLong);
}

LONG_PTR WINAPI Hook_SetWindowLongPtrA(
	HWND hWnd,
	int  nIndex,
	LONG_PTR dwNewLong
)
{
	if (nIndex == GWL_STYLE)
	{
		FilterStylePtr(dwNewLong);
	}
	return SetWindowLongPtrA(hWnd, nIndex, dwNewLong);
}

LONG_PTR WINAPI Hook_SetWindowLongPtrW(
	HWND hWnd,
	int  nIndex,
	LONG_PTR dwNewLong
)
{
	if (nIndex == GWL_STYLE)
	{
		FilterStylePtr(dwNewLong);
	}
	return SetWindowLongPtrW(hWnd, nIndex, dwNewLong);
}

void WindowStyleHook::InstallImpl()
{
	hookInfoA = std::get<1>(InstallNamedHook(L"user32", "SetWindowLongA", Hook_SetWindowLongA));
	hookInfoW = std::get<1>(InstallNamedHook(L"user32", "SetWindowLongW", Hook_SetWindowLongW));
	hookInfoPtrA = std::get<1>(InstallNamedHook(L"user32", "SetWindowLongPtrA", Hook_SetWindowLongPtrA));
	hookInfoPtrW = std::get<1>(InstallNamedHook(L"user32", "SetWindowLongPtrW", Hook_SetWindowLongPtrW));
}

void WindowStyleHook::UninstallImpl()
{
	UninstallHook(&hookInfoA);
	UninstallHook(&hookInfoW);
	UninstallHook(&hookInfoPtrA);
	UninstallHook(&hookInfoPtrW);
}

}
