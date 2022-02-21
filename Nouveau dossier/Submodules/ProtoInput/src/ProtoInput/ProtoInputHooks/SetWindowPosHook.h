#pragma once
#include "Hook.h"
#include "InstallHooks.h"

namespace Proto
{

class SetWindowPosHook final : public Hook
{
private:
	HookInfo hookInfo{};
		
public:
	static int width;
	static int height;
	static int posx;
	static int posy;
	
	const char* GetHookName() const override { return "Set Window Position"; }
	const char* GetHookDescription() const override
	{
		return
			"When the game tries to reposition/resize its game window, this hook forces it to a fixed position and size. ";
	}
	bool HasGuiStatus() const override { return true; }
	void ShowGuiStatus() override;
	void InstallImpl() override;
	void UninstallImpl() override;
};

}
