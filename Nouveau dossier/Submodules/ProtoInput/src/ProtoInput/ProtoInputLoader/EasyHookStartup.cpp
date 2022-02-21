#include "Loader.h"
#include <easyhook.h>
#include <ntstatus.h>

EASYHOOK_NT_EXPORT RtlCreateSuspendedProcess(
	WCHAR* InEXEPath,
	WCHAR* InCommandLine,
	ULONG InCustomFlags,
	ULONG* OutProcessId,
	ULONG* OutThreadId,
	PVOID lpEnvironment);

extern "C" __declspec(dllexport) ProtoInstanceHandle EasyHookInjectStartup(
	const wchar_t* exePath,
	const wchar_t* commandLine,
	unsigned long processCreationFlags,
	const wchar_t* dllFolderPath,
	unsigned long* outPid,
	void* environment)
{
	// const auto dllpath = std::wstring(dllFolderPath);
	// const auto dllpath32 = dllpath + L"ProtoInputHooks32.dll";
	// const auto dllpath64 = dllpath + L"ProtoInputHooks64.dll";

	// printf("Dll path 32 = \"%ws\", 64 = \"%ws\"\n", dllpath32.c_str(), dllpath64.c_str());
	//
	// unsigned long pid;
	//
	// auto res = RhCreateAndInject(const_cast<WCHAR*>(exePath), const_cast<WCHAR*>(commandLine),
	// 							 processCreationFlags, 0,
	// 							 const_cast<WCHAR*>(dllpath32.c_str()), 
	// 							 const_cast<WCHAR*>(dllpath64.c_str()),
	// 							 nullptr, 0, &pid);		
	// if (FAILED(res))
	// {
	// 	std::cerr << "Failed CreateAndInject, NTSTATUS = 0x" << std::hex << res << std::dec << std::endl;
	// 	
	// 	if (outPid)
	// 		*outPid = 0;
	// }
	// else
	// {
	// 	if (outPid)
	// 		*outPid = pid;
	// 	
	// 	return CreateInstanceHandle(pid);
	// }

	if (environment != nullptr)
	{
		auto env = (const wchar_t* )environment;
		
		printf("Environment:\n");

		while (!(*env == L"\0"[0] && *(env+1) != L"\0"[1]))
		{
			wprintf(env);
			printf("\n");
			env += wcslen(env)+1;
		}
	}
	
	ULONG pid;
	ULONG tid;

	const auto createProcRes = RtlCreateSuspendedProcess(
		const_cast<WCHAR*>(exePath), 
		const_cast<WCHAR*>(commandLine), 
		processCreationFlags, 
		&pid, 
		&tid,
		environment);

	if(FAILED(createProcRes))
	{
		std::cerr << "Failed RtlCreateSuspendedProcess, NTSTATUS = 0x" << std::hex << createProcRes << std::dec << std::endl;
		
		if (outPid)
			*outPid = 0;
	}
	else
	{
		if (outPid)
			*outPid = pid;

		return EasyHookInjectRuntime(pid, dllFolderPath);
	}

	return 0;
}
