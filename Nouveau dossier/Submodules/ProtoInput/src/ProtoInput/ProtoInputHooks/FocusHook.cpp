#include "FocusHook.h"
#include <imgui.h>
#include "HwndSelector.h"

namespace Proto
{

inline HWND GetHwnd()
{	
	if (HwndSelector::GetSelectedHwnd() == 0)
		HwndSelector::UpdateMainHwnd();

	return (HWND)HwndSelector::GetSelectedHwnd();
}

HWND WINAPI Hook_GetForegroundWindow()
{
	return GetHwnd();
}

HWND WINAPI Hook_WindowFromPoint(POINT Point)
{
	return GetHwnd();
}

HWND WINAPI Hook_GetActiveWindow()
{
	return GetHwnd();
}

BOOL WINAPI Hook_IsWindowEnabled(HWND hWnd)
{
	return TRUE;
}

HWND WINAPI Hook_GetFocus()
{
	return GetHwnd();
}

HWND WINAPI Hook_GetCapture()
{
	return GetHwnd();
}

HWND WINAPI Hook_SetCapture(HWND inputHwnd)
{
	return inputHwnd;
}

BOOL WINAPI Hook_ReleaseCapture()
{
	return TRUE;
}

HWND WINAPI Hook_SetActiveWindow(HWND input)
{
	return input;
}

HWND WINAPI Hook_SetFocus(HWND input)
{
	return input;
}

BOOL WINAPI Hook_SetForegroundWindow(HWND hWnd)
{
	return true;
}

void FocusHook::ShowGuiStatus()
{	
	if (IsInstalled() && needReinstalling)
	{
		ImGui::PushID(1234);
		ImGui::PushStyleColor(ImGuiCol_Text,
							  (ImVec4)ImColor::HSV(35.0f / 255.0f, 0.9f, 0.9f)
		);
		ImGui::TextWrapped("Disable and enable the hook for changes to take effect");
		ImGui::PopStyleColor(1);
		ImGui::PopID();
	}

	ImGui::TextWrapped("Window directing fake focus to: %d (0x%X)", HwndSelector::GetSelectedHwnd(), HwndSelector::GetSelectedHwnd());

	if (ImGui::Button("Find new main window"))
		HwndSelector::UpdateMainHwnd();
	
	HookCheckbox("GetForegroundWindow", &enabledHookGetForegroundWindow);
	HookCheckbox("WindowFromPoint", &enabledHookWindowFromPoint);
	HookCheckbox("GetActiveWindow", &enabledHookGetActiveWindow);
	HookCheckbox("IsWindowEnabled", &enabledHookIsWindowEnabled);
	HookCheckbox("GetFocus", &enabledHookGetFocus);
	HookCheckbox("GetCapture", &enabledHookGetCapture);
	HookCheckbox("SetCapture", &enabledHookSetCapture);
	HookCheckbox("SetActiveWindow", &enabledHookSetActiveWindow);
	HookCheckbox("ReleaseCapture", &enabledHookReleaseCapture);
	HookCheckbox("SetFocus", &enabledHookSetFocus);
	HookCheckbox("SetForegroundWindow", &enabledHookSetForegroundWindow);
}

void FocusHook::InstallImpl()
{
	needReinstalling = false;
	
	if (enabledHookGetForegroundWindow) hookInfoGetForegroundWindow = std::get<1>(InstallNamedHook(L"user32", "GetForegroundWindow", Hook_GetForegroundWindow));
	if (enabledHookWindowFromPoint) hookInfoWindowFromPoint = std::get<1>(InstallNamedHook(L"user32", "WindowFromPoint", Hook_WindowFromPoint));
	if (enabledHookGetActiveWindow) hookInfoGetActiveWindow = std::get<1>(InstallNamedHook(L"user32", "GetActiveWindow", Hook_GetActiveWindow));
	if (enabledHookIsWindowEnabled) hookInfoIsWindowEnabled = std::get<1>(InstallNamedHook(L"user32", "IsWindowEnabled", Hook_IsWindowEnabled));
	if (enabledHookGetFocus) hookInfoGetFocus = std::get<1>(InstallNamedHook(L"user32", "GetFocus", Hook_GetFocus));
	if (enabledHookGetCapture) hookInfoGetCapture = std::get<1>(InstallNamedHook(L"user32", "GetCapture", Hook_GetCapture));
	if (enabledHookSetCapture) hookInfoSetCapture = std::get<1>(InstallNamedHook(L"user32", "SetCapture", Hook_SetCapture));
	if (enabledHookReleaseCapture) hookInfoReleaseCapture = std::get<1>(InstallNamedHook(L"user32", "ReleaseCapture", Hook_ReleaseCapture));
	if (enabledHookSetActiveWindow) hookInfoSetActiveWindow = std::get<1>(InstallNamedHook(L"user32", "SetActiveWindow", Hook_SetActiveWindow));
	if (enabledHookSetFocus) hookInfoSetFocus = std::get<1>(InstallNamedHook(L"user32", "SetFocus", Hook_SetFocus));
	if (enabledHookSetForegroundWindow) hookInfoSetForegroundWindow = std::get<1>(InstallNamedHook(L"user32", "SetForegroundWindow", Hook_SetForegroundWindow));
}

void FocusHook::UninstallImpl()
{
	UninstallHook(&hookInfoGetForegroundWindow);
	UninstallHook(&hookInfoWindowFromPoint	  );
	UninstallHook(&hookInfoGetActiveWindow	  );
	UninstallHook(&hookInfoIsWindowEnabled	  );
	UninstallHook(&hookInfoGetFocus 		  );
	UninstallHook(&hookInfoGetCapture 		  );
	UninstallHook(&hookInfoSetCapture 		  );
	UninstallHook(&hookInfoReleaseCapture 	  );
	UninstallHook(&hookInfoSetActiveWindow 	  );
	UninstallHook(&hookInfoSetFocus			  );
	UninstallHook(&hookInfoSetForegroundWindow);
}

}
