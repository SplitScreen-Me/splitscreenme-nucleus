#include "GetKeyStateHook.h"
#include "FakeMouseKeyboard.h"

namespace Proto
{

SHORT WINAPI Hook_GetKeyState(int nVirtKey)
{
	return FakeMouseKeyboard::IsKeyStatePressed(nVirtKey) ? 0b1000000000000000 : 0;
}

void GetKeyStateHook::InstallImpl()
{
	hookInfo = std::get<1>(InstallNamedHook(L"user32", "GetKeyState", Hook_GetKeyState));
}

void GetKeyStateHook::UninstallImpl()
{
	UninstallHook(&hookInfo);
}

}