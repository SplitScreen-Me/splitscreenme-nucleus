#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class SetCursorPosHook final : public Hook
{
private:
	HookInfo hookInfo{};

public:
	//TODO: pipe option?
	static bool blockSettingCursorPos;
		
	const char* GetHookName() const override { return "Set Cursor Position"; }
	const char* GetHookDescription() const override
	{
		return
		"Hooks the SetCursorPos function to prevent moving the 'real' cursor and instead move our 'fake' cursor. "
		"This is useful in games that move the mouse around (e.g. in a gamepad implementation). "
		"This is also important in games that set the cursor to the window centre and record the difference each frame for first-person input. ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
