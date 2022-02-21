#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class FindWindowHook final : public Hook
{
private:
	HookInfo hookInfoFindWindowA{};
	HookInfo hookInfoFindWindowW{};
	HookInfo hookInfoFindWindowExA{};
	HookInfo hookInfoFindWindowExW{};
	HookInfo hookInfoEnumWindows{};

public:
	const char* GetHookName() const override { return "Find Window"; }
	const char* GetHookDescription() const override
	{
		return
			"Returns empty information in FindWindow and EnumWindow to prevent the game from detecting windows. ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
