#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{


class CursorVisibilityHook final : public Hook
{
private:
	HookInfo hookInfoShowCursor{};
	HookInfo hookInfoSetCursor{};
	HookInfo hookInfoSetSystemCursor{};

public:
	const char* GetHookName() const override { return "Cursor Visibility"; }
	const char* GetHookDescription() const override
	{
		return "Whenever the game calls for the cursor to be shown/hidden, the 'fake' cursor is shown/hidden instead. "
		"Also detects when the game tries to set a custom cursor image, so the fake cursor can copy it. ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;

	static bool ShowCursorWhenImageUpdated;
};

}