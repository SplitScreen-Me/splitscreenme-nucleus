#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class GetKeyboardStateHook final : public Hook
{
private:
	HookInfo hookInfo{};

public:
	const char* GetHookName() const override { return "Get Keyboard State"; }
	const char* GetHookDescription() const override
	{
		return
			"Hooks the GetKeyboardState function to return our entire 'fake' keyboard state. "
			"You should almost always have this enabled even if it appears to have no effect: "
			"when using multiple keyboards at the same time, they may interfere without this hook. ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
