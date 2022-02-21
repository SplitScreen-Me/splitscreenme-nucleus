#include "SetWindowPosHook.h"
#include <imgui.h>

namespace Proto
{

int SetWindowPosHook::width = 0;
int SetWindowPosHook::height = 0;
int SetWindowPosHook::posx = 0;
int SetWindowPosHook::posy = 0;

BOOL WINAPI Hook_SetWindowPos(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags)
{
	return SetWindowPos(hWnd, hWndInsertAfter, SetWindowPosHook::posx, SetWindowPosHook::posy, SetWindowPosHook::width, SetWindowPosHook::height, uFlags);
}

void SetWindowPosHook::ShowGuiStatus()
{
	int pos[2] = { posx, posy };
	ImGui::SliderInt2("Position", &pos[0], -5000, 5000);
	posx = pos[0];
	posy = pos[1];
	
	int size[2] = { width, height };
	ImGui::SliderInt2("Size", &size[0], 0, 5000);
	width = size[0];
	height = size[1];
}

void SetWindowPosHook::InstallImpl()
{
	hookInfo = std::get<1>(InstallNamedHook(L"user32", "SetWindowPos", Hook_SetWindowPos));
}

void SetWindowPosHook::UninstallImpl()
{
	UninstallHook(&hookInfo);
}

}
