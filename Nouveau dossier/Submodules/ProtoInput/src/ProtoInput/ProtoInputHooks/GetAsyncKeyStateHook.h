#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class GetAsyncKeyStateHook final : public Hook
{
private:
	HookInfo hookInfo{};

public:
	const char* GetHookName() const override { return "Get Async Key State"; }
	const char* GetHookDescription() const override {
		return
		"Hooks the GetAsyncKeyState function to return our 'fake' keyboard state. "
		"You should almost always have this enabled even if it appears to have no effect: "
		"when using multiple keyboards at the same time, they may interfere without this hook. "
		"GetAsyncKeyState returns two booleans: whether the key is pressed and also whether the key was pressed since the last call to GetAsyncKeyState. "
		"Make sure to enable this if you have problems with keys being pressed/released when they shouldn't be. ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;
};

}