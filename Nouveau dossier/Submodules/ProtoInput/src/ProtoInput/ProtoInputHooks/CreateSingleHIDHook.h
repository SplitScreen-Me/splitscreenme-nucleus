#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class CreateSingleHIDHook final : public Hook
{
private:
	HookInfo hookInfoCreateFileA{};
	HookInfo hookInfoCreateFileW{};

public:
	const char* GetHookName() const override { return "Create Single HID"; }
	const char* GetHookDescription() const override
	{
		return
			"Hooks CreateFile to only permit opening the allowed HID. ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;

	// Only need to call one of these, it will get converted automatically
	static void SetRawHidA(const char* name);
	static void SetRawHidW(const wchar_t* name);
};

}
