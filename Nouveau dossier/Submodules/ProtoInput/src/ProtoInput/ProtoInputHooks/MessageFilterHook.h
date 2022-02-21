#pragma once
#include "Hook.h"
#include "HookManager.h"
#include "protoloader.h"

namespace Proto
{

class MessageFilterHook final : public Hook
{
private:
	HookInfo hookInfoGetMessageW{};
	HookInfo hookInfoGetMessageA{};
	HookInfo hookInfoPeekMessageA{};
	HookInfo hookInfoPeekMessageW{};

public:
	const char* GetHookName() const override { return "Message Filter"; }
	const char* GetHookDescription() const override
	{
		return
			"Installs hooks in GetMessage and PeekMessage to filter messages. "
			"See the message filter for more details. This hook is requried for it to work.";
	}

	bool HasGuiStatus() const override { return false; }
	
	static void FilterGui();

	static void SetFilterEnabled(ProtoMessageFilterIDs filterID, bool enabled);
	
	void InstallImpl() override;
	void UninstallImpl() override;

	static bool IsKeyboardButtonFilterEnabled();
};

}
