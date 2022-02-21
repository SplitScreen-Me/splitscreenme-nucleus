#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class XinputHook final : public Hook
{
private:
	std::vector<HookInfo> hookInfos{};

	static bool useDinput;

public:
	static unsigned int controllerIndex;
	static unsigned int controllerIndex2;
	static unsigned int controllerIndex3;
	static unsigned int controllerIndex4;

	static bool useOpenXinput;
	
	const char* GetHookName() const override { return "Xinput"; }
	const char* GetHookDescription() const override {
		return
			"Hooks Xinput functions to redirect input to the selected controller. "
			"DirectInput (Dinput) redirecting is required to use more than 4 controllers (Xinput has a maximum of 4). ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;

	static void SetUseDinput(bool useDinput);
	static bool GetUseDinput() { return useDinput; }
};

}
