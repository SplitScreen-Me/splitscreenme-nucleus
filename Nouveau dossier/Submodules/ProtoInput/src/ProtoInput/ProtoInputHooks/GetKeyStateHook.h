#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class GetKeyStateHook final : public Hook
{
private:
	HookInfo hookInfo{};

public:
	const char* GetHookName() const override { return "Get Key State"; }
	const char* GetHookDescription() const override
	{
		return
		"Hooks the GetKeyState function to return our 'fake' keyboard state. "
		"You should almost always have this enabled even if it appears to have no effect: "
		"when using multiple keyboards at the same time, they may interfere without this hook. ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
