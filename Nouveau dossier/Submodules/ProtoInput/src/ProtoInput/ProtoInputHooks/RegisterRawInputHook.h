#pragma once
#include "Hook.h"
#include "HookManager.h"
#include <bitset>
#include <hidusage.h>

namespace Proto
{

class RegisterRawInputHook final : public Hook
{
private:
	//TODO: add to gui and/or pipe (or just set the bitfield from the pipe).
	bool updateDeviceTypeOnGameUpdate = true;
	
	bool installedAtLeastOnce = false;
	
	HookInfo hookInfo{};
		
	void FindAlreadySubscribedWindows();

public:
	static bool logCallsToRegisterRawInput;
	
	const char* GetHookName() const override { return "Register Raw Input"; }
	const char* GetHookDescription() const override
	{
		return
		"Disconnects the game from raw input and subscribes our window instead. "
		"We subscribe to raw input in the foreground and background so we always receive input. "
		"Forwards raw input back to the original window. "
		"Disabling this hook after it's installed may not fully uninstall it. "
		"Note that you need this hook for the GUI shortcut to work, so you should almost always have this enabled. ";
	}

	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;

	void AddWindowToForward(HWND hwnd, std::bitset<9> usages);
		
	RegisterRawInputHook();
};

}
