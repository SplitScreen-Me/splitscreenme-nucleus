#define WIN32_LEAN_AND_MEAN             // Exclude rarely-used stuff from Windows headers
#include <windows.h>
#include <cstdio>
#include <shellapi.h>
#include <ntstatus.h>
#include <string>
#include <easyhook.h>
#include "protoinputinejctorproxy.h"

// Reason for this project (ProtoInputInjector):
// EasyHook can only inject into a process if its the same 32/64 bit, so we need to launch a subprocess
// The exit code of the process gives the error code of the EasyHook injection
// We need a separate dll (ProtoInputInjectorProxy) since EasyHook will load the 'EasyHook' dll
// (which turns out to be the proxy dll since we are statically linking)

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR pCmdLine, int nCmdShow)
{
	// (Uncomment if you need a quick and dirty console output)
	// AllocConsole();
	// FILE* f = new FILE();
	// freopen_s(&f, "CONOUT$", "w", stdout);
	// freopen_s(&f, "CONOUT$", "w", stderr);

	int nArgs;
	LPWSTR* szArglist = CommandLineToArgvW(GetCommandLineW(), &nArgs);
	
	if (szArglist == nullptr)
	{
		MessageBoxW(NULL, L"CommandLineToArgvW failed", L"ProtoInputInjector Error", MB_OK);
		LocalFree(szArglist);
		return -1;
	}
	else if (nArgs != 2)
	{		
		MessageBoxW(NULL, L"Incorrect number of arguments", L"ProtoInputInjector Error", MB_OK);
		LocalFree(szArglist);
		return -1;
	}
	else
	{
		const std::wstring pidString{ szArglist[0] };
		const std::wstring stealthString{ szArglist[1] };
		LocalFree(szArglist);
		
		unsigned int pid;
		bool stealthInject;

		try
		{
			pid = std::stoi(pidString);
			stealthInject = std::stoi(stealthString) == 1;
		}
		catch(...)
		{
			MessageBoxW(NULL, L"Failed to parse arguments", L"ProtoInputInjector Error", MB_OK);
			return -1;
		}
		
		const auto res = ProtoInputEasyHookRuntimeInjectInternal(pid, stealthInject);
		
		return res;
	}


	return 0;
}