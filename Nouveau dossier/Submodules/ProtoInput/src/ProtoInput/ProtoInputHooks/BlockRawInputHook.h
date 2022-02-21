#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class BlockRawInputHook final : public Hook
{
private:
	HookInfo hookInfoGetRegisteredRawInputDevices{};
	HookInfo hookInfoGetRawInputDeviceList{};

public:
	const char* GetHookName() const override { return "Block Raw Input"; }
	const char* GetHookDescription() const override
	{
		return
			"Returns empty data from GetRegisteredRawInputDevices and GetRawInputDeviceList to prevent the game from detecting raw input devices. "
			"Works best with blocking WM_INPUT in the message blocker, and disabling the Raw Input filter (since filters take priority over filters). ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
