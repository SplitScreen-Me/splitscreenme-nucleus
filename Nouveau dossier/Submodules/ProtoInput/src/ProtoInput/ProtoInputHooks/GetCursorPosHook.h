#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class GetCursorPosHook final : public Hook
{
private:
	HookInfo hookInfo{};

public:
	const char* GetHookName() const override { return "Get Cursor Position"; }
	const char* GetHookDescription() const override { return "Hooks the GetCursorPos function to return our 'fake' position"; }
	bool HasGuiStatus() const override { return false; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
