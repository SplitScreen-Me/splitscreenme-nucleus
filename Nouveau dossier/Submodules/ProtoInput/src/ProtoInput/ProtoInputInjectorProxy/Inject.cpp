#include "framework.h"
#include "include/protoinputinejctorproxy.h"
#include <string>
#include <easyhook.h>

extern "C" __declspec(dllexport) int ProtoInputEasyHookRuntimeInjectInternal(unsigned int pid, bool stealthInject)
{
	wchar_t pathchars[MAX_PATH];
	GetModuleFileNameW(NULL, pathchars, MAX_PATH);
	std::wstring folderpath = pathchars;
	size_t pos = folderpath.find_last_of(L"\\");

	if (pos == std::string::npos)
	{
		MessageBoxW(NULL, L"Failed to get folder path", L"ProtoInputInjector Error", MB_OK);
		return -1;
	}

	folderpath = folderpath.substr(0, pos + 1);

	const auto path32 = folderpath + L"ProtoInputHooks32.dll";
	const auto path64 = folderpath + L"ProtoInputHooks64.dll";

	return RhInjectLibrary(pid, 0, stealthInject ? EASYHOOK_INJECT_STEALTH : EASYHOOK_INJECT_DEFAULT,
									 const_cast<wchar_t*>(path32.c_str()), const_cast<wchar_t*>(path64.c_str()),
									 nullptr, 0);
}

extern "C" NTSTATUS __declspec(dllexport) __stdcall HookCompleteInjection(void* InInfo)
{
	return HookCompleteInjectionImpl(InInfo);
}