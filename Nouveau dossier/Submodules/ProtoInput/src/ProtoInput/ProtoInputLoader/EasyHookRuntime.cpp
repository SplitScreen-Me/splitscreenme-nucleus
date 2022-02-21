#include "protoloader.h"
#include "Loader.h"
#include <string>
#include "TrackedInstances.h"
#include <iostream>
#include <Windows.h>
#include <BlackBone/Process/Process.h>
#include <BlackBone/Patterns/PatternSearch.h>
#include "easyhook.h"
#include <filesystem>

ProtoInstanceHandle EasyHookInjectImpl(unsigned long pid, const wchar_t* dllFolderPath, bool stealth)
{
	const bool is64 = Isx64(pid);
	std::cout << "Target is " << (is64 ? "64-bit" : "32-bit") << std::endl;

	auto processPath = std::wstring(dllFolderPath) + (is64 ? L"ProtoInputIJ64.exe" : L"ProtoInputIJ32.exe");

	STARTUPINFOW startupInfo = { 0 };
	startupInfo.cb = sizeof(startupInfo);
	PROCESS_INFORMATION processInfo = { 0 };

	std::wstring commandLineString = std::to_wstring(pid) + (stealth ? L" 1" : L" 0");

	BOOL success = CreateProcessW(
		processPath.c_str(),
		const_cast<LPWSTR>(commandLineString.c_str()),
		NULL,
		NULL,
		FALSE,
		0,
		NULL,
		NULL,
		&startupInfo,
		&processInfo);

	if (success)
	{
		ProtoInstanceHandle ret = 0;


		WaitForSingleObject(processInfo.hProcess, INFINITE);

		DWORD exitCode;
		GetExitCodeProcess(processInfo.hProcess, &exitCode);

		switch (exitCode)
		{
		case STATUS_INVALID_PARAMETER_4:
		case STATUS_INVALID_PARAMETER_5:
		{
			printf("Error: The given library could not be found\n");
			break;
		}
		case STATUS_WOW_ASSERTION:
		{
			printf("Error: Tried to inject 32-bit hooks into 64-bit or vice versa\n");
			break;
		}
		case STATUS_NOT_FOUND:
		{
			printf("Could not find target process %d\n", pid);
			break;
		}
		case STATUS_ACCESS_DENIED:
		{
			printf("Target process could not be access properly or remote thread creation failed\n");
			break;
		}
		case 0:
		{
			printf("Successful EasyHook return value\n");
			break;
		}
		default:
		{
			printf("EasyHook NTSTATUS error: 0x%X\n", exitCode);
			break;
		}
		}

		if (exitCode == 0)
		{
			ret = CreateInstanceHandle(pid);
		}

		CloseHandle(processInfo.hThread);
		CloseHandle(processInfo.hProcess);

		return ret;
	}
	else
	{
		printf("Failed to create injector sub process\n");
		return 0;
	}
}

extern "C" __declspec(dllexport) ProtoInstanceHandle EasyHookInjectRuntime(unsigned long pid, const wchar_t* dllFolderPath)
{
	return EasyHookInjectImpl(pid, dllFolderPath, false);
}

extern "C" __declspec(dllexport) ProtoInstanceHandle EasyHookStealthInjectRuntime(unsigned long pid, const wchar_t* dllFolderPath)
{
	return EasyHookInjectImpl(pid, dllFolderPath, true);
}