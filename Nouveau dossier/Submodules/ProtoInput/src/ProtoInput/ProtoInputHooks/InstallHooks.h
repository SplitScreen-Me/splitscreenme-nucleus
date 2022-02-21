#pragma once
#include <easyhook.h>
#include <vector>

namespace Proto
{

struct HookInfo
{
	bool hasHook = false;
	HOOK_TRACE_INFO info;
};

extern std::vector<unsigned long> ACLThreads;

void AddThreadToACL(unsigned long threadID);

std::tuple<NTSTATUS, HookInfo> InstallHook(void* address, void* callback);
std::tuple<NTSTATUS, HookInfo> InstallNamedHook(const LPCWSTR moduleHandle, const LPCSTR proc, void* callBack, const bool isOrdinal = false);
void UninstallHook(HookInfo* hookInfo);

}