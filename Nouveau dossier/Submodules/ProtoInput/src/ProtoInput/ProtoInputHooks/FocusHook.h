#pragma once
#include "HookManager.h"
#include <imgui.h>

namespace Proto
{

class FocusHook final : public Hook
{
private:
	HookInfo hookInfoGetForegroundWindow{};
	HookInfo hookInfoWindowFromPoint{};
	HookInfo hookInfoGetActiveWindow{};
	HookInfo hookInfoIsWindowEnabled{};
	HookInfo hookInfoGetFocus{};
	HookInfo hookInfoGetCapture{};
	HookInfo hookInfoSetCapture{};
	HookInfo hookInfoReleaseCapture{};
	HookInfo hookInfoSetActiveWindow{};
	HookInfo hookInfoSetFocus{};
	HookInfo hookInfoSetForegroundWindow{};

	bool needReinstalling = false;
	
	bool enabledHookGetForegroundWindow { true };
	bool enabledHookWindowFromPoint { true };
	bool enabledHookGetActiveWindow { true };
	bool enabledHookIsWindowEnabled { true };
	bool enabledHookGetFocus { true };
	bool enabledHookGetCapture { true };
	bool enabledHookSetCapture { true };
	bool enabledHookReleaseCapture { true };
	bool enabledHookSetActiveWindow { true };
	bool enabledHookSetFocus { true };
	bool enabledHookSetForegroundWindow { true };

private:
	void HookCheckbox(const char* label, bool* enabled)
	{
		if (ImGui::Checkbox(label, enabled))
			needReinstalling = true;
	};
	
public:	
	const char* GetHookName() const override { return "Focus"; }
	const char* GetHookDescription() const override
	{
		return
		"Fakes window focus by returning the games window as the foreground window. ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
