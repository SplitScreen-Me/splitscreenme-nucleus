// ProtoInputHost.cpp : This file contains the 'main' function. Program execution begins and ends there.
//

#include <iostream>
#include <Windows.h>
#include <BlackBone/Process/Process.h>
#include <BlackBone/Patterns/PatternSearch.h>
#include <BlackBone/Process/RPC/RemoteFunction.hpp>
#include <BlackBone/Syscalls/Syscall.h>
#include <BlackBone/LocalHook/LocalHook.hpp>

#include "protoloader.h"
#include "protoinpututil.h"
#include <filesystem>
#include "Gui.h"


bool CheckBuildTimings(const std::wstring& folderpath)
{
	const auto loaderPath32 = std::filesystem::path{ folderpath }.append("ProtoInputLoader32.dll");
	const auto loaderPath64 = std::filesystem::path{ folderpath }.append("ProtoInputLoader64.dll");

	const auto hookPath32 = std::filesystem::path{ folderpath }.append("ProtoInputHooks32.dll");
	const auto hookPath64 = std::filesystem::path{ folderpath }.append("ProtoInputHooks64.dll");

	long long secLoader;
	long long secHooks;

	try
	{
		secLoader = abs(std::chrono::duration_cast<std::chrono::seconds>(last_write_time(loaderPath32) - last_write_time(loaderPath64))).count();
		secHooks = abs(std::chrono::duration_cast<std::chrono::seconds>(last_write_time(hookPath32) - last_write_time(hookPath64))).count();
	}
	catch(...)
	{
		MessageBoxW(NULL, L"Hooks32/64 and/or Loader32/64 are missing", L"Error", MB_OKCANCEL);
		return true;
	}
		
	constexpr int maximumDurationSec = 60;

	if (secLoader > maximumDurationSec || secHooks > maximumDurationSec)
	{
		return IDCANCEL == MessageBoxW(NULL, L"Hooks32/64 or Loader32/64 are built at very different times\nMake they have both been compiled", L"Warning", MB_OKCANCEL);
	}

	return false;
}

void ShowGui()
{
	ProtoHost::ShowGuiImpl();
}

int main();
int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
	return main();
}

int main()
{
	// (Uncomment if you need a quick and dirty console output)
	// AllocConsole();
	// FILE* f = new FILE();
	// freopen_s(&f, "CONOUT$", "w", stdout);
	// freopen_s(&f, "CONOUT$", "w", stderr);
	
	wchar_t pathchars[MAX_PATH];
	GetModuleFileNameW(NULL, pathchars, MAX_PATH);
	std::wstring folderpath = pathchars;
	size_t pos = folderpath.find_last_of(L"\\");
	if (pos != std::string::npos)
		folderpath = folderpath.substr(0, pos + 1);
		
	if (CheckBuildTimings(folderpath))
		return 0;

	constexpr bool gui = true;
	constexpr bool runtime = false;
	constexpr bool hookSelf = false;

	if (gui)
	{
		ShowGui();
		return 0;
	}

	// These can be useful if you're debugging and want to quickly launch instances
	else if (runtime)
	{
		if (hookSelf)
			const auto instanceHandle = RemoteLoadLibraryInjectRuntime(GetCurrentProcessId(), folderpath.c_str());
		else 
		{
			auto pids = blackbone::Process::EnumByName(L"notepad.exe");
			for (const auto& pid : pids)
			{
				// const auto instanceHandle = EasyHookInjectRuntime(pid, folderpath.c_str());
			}
		}
	}
	else
	{
		for (int i = 1; i <= 2; i++)
		{
			auto path = LR"(C:\WINDOWS\system32\notepad.exe)";
			unsigned long pid;

			ProtoInstanceHandle instanceHandle = EasyHookInjectStartup(
				path, L"", 0, folderpath.c_str(), &pid);

			SetupState(instanceHandle, i);

			InstallHook(instanceHandle, RegisterRawInputHookID);
			InstallHook(instanceHandle, GetRawInputDataHookID);
			InstallHook(instanceHandle, MessageFilterHookID);
			InstallHook(instanceHandle, GetCursorPosHookID);
			InstallHook(instanceHandle, SetCursorPosHookID);
			InstallHook(instanceHandle, GetKeyStateHookID);
			InstallHook(instanceHandle, GetAsyncKeyStateHookID);
			InstallHook(instanceHandle, GetKeyboardStateHookID);
			InstallHook(instanceHandle, CursorVisibilityStateHookID);
			InstallHook(instanceHandle, ClipCursorHookID);
			InstallHook(instanceHandle, FocusHooksHookID);
			InstallHook(instanceHandle, RenameHandlesHookID);

			EnableMessageFilter(instanceHandle, RawInputFilterID);
			EnableMessageFilter(instanceHandle, MouseMoveFilterID);
			EnableMessageFilter(instanceHandle, MouseActivateFilterID);
			EnableMessageFilter(instanceHandle, WindowActivateFilterID);
			EnableMessageFilter(instanceHandle, WindowActivateAppFilterID);
			EnableMessageFilter(instanceHandle, MouseWheelFilterID);
			EnableMessageFilter(instanceHandle, MouseButtonFilterID);

			SetupMessagesToSend(instanceHandle);

			StartFocusMessageLoop(instanceHandle);

			WakeUpProcess(instanceHandle);
		}
	}

	// Don't want to end the pipe immediately
	MessageBoxW(NULL, L"Close to exit ProtoInputHost", L"", MB_OK);
}
