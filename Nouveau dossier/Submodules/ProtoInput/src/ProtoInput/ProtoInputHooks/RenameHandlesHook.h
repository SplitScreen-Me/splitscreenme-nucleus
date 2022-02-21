#pragma once
#include "Hook.h"
#include "InstallHooks.h"
#include <map>

namespace Proto
{

class RenameHandlesHook final : public Hook
{
private:
	HookInfo hookInfoNtCreateMutant{};
	HookInfo hookInfoNtOpenMutant{};

	HookInfo hookInfoNtCreateEvent{};
	HookInfo hookInfoNtOpenEvent{};

	HookInfo hookInfoNtCreateSemaphore{};
	HookInfo hookInfoNtOpenSemaphore{};
	
	HookInfo hookInfoNtCreateNamedPipeFile{};

public:
	const char* GetHookName() const override { return "Rename Handles"; }
	const char* GetHookDescription() const override {
		return
			"Hooks NT functions that open Mutants, Events, Semaphores and Named Pipes to change what name is used. "
			"Renaming the objects prevents another instance from detecting them, which can prevent multiple instances opening. ";
	}
	bool HasGuiStatus() const override { return false; }
	void InstallImpl() override;
	void UninstallImpl() override;

	static void AddHandleToRenameList(const std::wstring& name); // Mutex, Event, Semaphore
	static void AddHandleToNamedPipeRenameList(const std::wstring& name);
};

}
