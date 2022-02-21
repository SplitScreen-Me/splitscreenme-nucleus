#include "GetCursorPosHook.h"
#include "FakeMouseKeyboard.h"
#include "HwndSelector.h"

namespace Proto
{

BOOL WINAPI Hook_GetCursorPos(LPPOINT lpPoint)
{	
	if (lpPoint)
	{
		const auto& state = FakeMouseKeyboard::GetMouseState();
		lpPoint->x = state.x;
		lpPoint->y = state.y;
		ClientToScreen((HWND)HwndSelector::GetSelectedHwnd(), lpPoint);
	}
	
	return true;
}

void GetCursorPosHook::ShowGuiStatus()
{
	
}

void GetCursorPosHook::InstallImpl()
{
	hookInfo = std::get<1>(InstallNamedHook(L"user32", "GetCursorPos", Hook_GetCursorPos));
}

void GetCursorPosHook::UninstallImpl()
{
	UninstallHook(&hookInfo);
}

}
