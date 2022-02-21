#include "include/protoloader.h"
#include "Loader.h"
#include <iostream>
#include <Windows.h>
#include <BlackBone/Process/Process.h>
#include <BlackBone/Patterns/PatternSearch.h>
#include <filesystem>
#include "BlackBone/Process/RPC/RemoteFunction.hpp"


extern "C" __declspec(dllexport) ProtoInstanceHandle RemoteLoadLibraryInjectRuntime(unsigned long pid, const wchar_t* dllFolderPath)
{
	bool is64 = Isx64(pid);
	std::cout << "Target is " << (is64 ? "64-bit" : "32-bit") << std::endl;

	blackbone::Process proc;
	proc.Attach(pid);

	auto dllpath = std::wstring(dllFolderPath);
	dllpath += is64 ? L"ProtoInputHooks64.dll" : L"ProtoInputHooks32.dll";
	std::wcout << L"Using dll \"" << dllpath << L"\"" << std::endl;

	// Blackbone LoadLibrary injection method
	if (auto pLoadLibrary =
		blackbone::MakeRemoteFunction<decltype(&LoadLibraryW)>(proc, L"kernel32.dll", "LoadLibraryW"))
	{
		auto callRes = pLoadLibrary(dllpath.c_str());
		if (!callRes.success())
		{
			std::cerr << "Remote call failed" << std::endl;
			return 0;
		}
		else if (callRes.result() == NULL)
		{
			std::cerr << "Failed to load DLL" << ", GetLastError = 0x" << std::hex << GetLastError() << std::dec << std::endl;
			return 0;
		}
		else
		{
			std::cout << "LoadLibrary remote call success\n" << std::endl;
			return CreateInstanceHandle(pid);
		}
	}
	else
		printf("MakeRemoteFunction failed to get LoadLibraryW\n");

	proc.Detach();

	return 0;
}
