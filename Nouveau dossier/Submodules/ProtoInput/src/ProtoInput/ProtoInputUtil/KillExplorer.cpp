#include "include/protoinpututil.h"
#include <BlackBone/Process/Process.h>

std::vector<HANDLE> suspendedThreads{ };

// We are only interested in suspending two threads:
// One is responsible for the taskbar, start menu and probably other stuff
// One is responsible for alt+tab, volume menu (and also probably other stuff)
// One is responsible for giving the right-click menu on the desktop
// Both of these threads own a large number of windows, so we can use that to tell
//   them apart from the other threads that we don't want to suspend
// We're also probably going to suspend a few other threads that own windows

std::unordered_map<DWORD, int> threadWindowCount{};
DWORD explorerProcessID;

constexpr int minimumNumberOfWindows = 4;

BOOL CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam)
{
	DWORD processID;
	DWORD threadID = GetWindowThreadProcessId(hwnd, &processID);

	if (processID == explorerProcessID)
	{
		if (auto s = threadWindowCount.find(threadID); s != threadWindowCount.end())
			s->second++;
		else
			threadWindowCount.insert({ threadID, 1 });
	}
	
	return TRUE;
}

inline int GetNumberOfWindowsOwnedByThread(DWORD threadID)
{
	if (auto s = threadWindowCount.find(threadID); s != threadWindowCount.end())
		return s->second;

	return 0;
}

void UpdateNumberOfThreadsOwnedByWindows(DWORD processID)
{
	explorerProcessID = processID;
	threadWindowCount = std::unordered_map<DWORD, int>{};
	EnumWindows(&EnumWindowsProc, 0);
}

extern "C" __declspec(dllexport) void SuspendExplorer()
{
	auto pids = blackbone::Process::EnumByName(L"explorer.exe");
	for (const auto pid : pids)
	{
		// int numSuspended = 0;
		
		HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, pid);
		if (h != INVALID_HANDLE_VALUE) {
			UpdateNumberOfThreadsOwnedByWindows(pid);
			THREADENTRY32 te;
			te.dwSize = sizeof(te);
			if (Thread32First(h, &te)) {
				do {
					if (te.th32OwnerProcessID == pid &&
						te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID) &&
						GetNumberOfWindowsOwnedByThread(te.th32ThreadID) >= minimumNumberOfWindows)
					{
						if (auto hThread = OpenThread(THREAD_SUSPEND_RESUME, FALSE, te.th32ThreadID); hThread != NULL)
						{
							// numSuspended++;
							SuspendThread(hThread);

							suspendedThreads.push_back(hThread);
						}
					}
					te.dwSize = sizeof(te);
				}
				while (Thread32Next(h, &te));
			}
			CloseHandle(h);
		}

		// auto str = std::string{ "Num suspended = " } + std::to_string(numSuspended);
		// MessageBoxA(0, str.c_str(), "", 0);
	}

	
}

extern "C" __declspec(dllexport) void RestartExplorer()
{	
	for (const auto h : suspendedThreads)
	{
		ResumeThread(h);
		CloseHandle(h);
	}

	suspendedThreads.clear();


	// Just in case its a reboot of the program that's trying to clean up a crashed exit
	auto pids = blackbone::Process::EnumByName(L"explorer.exe");
	for (const auto pid : pids)
	{
		HANDLE h = CreateToolhelp32Snapshot(TH32CS_SNAPTHREAD, pid);
		if (h != INVALID_HANDLE_VALUE) {
			UpdateNumberOfThreadsOwnedByWindows(pid);
			THREADENTRY32 te;
			te.dwSize = sizeof(te);
			if (Thread32First(h, &te)) {
				do {
					if (te.th32OwnerProcessID == pid &&
						te.dwSize >= FIELD_OFFSET(THREADENTRY32, th32OwnerProcessID) + sizeof(te.th32OwnerProcessID) &&
						GetNumberOfWindowsOwnedByThread(te.th32ThreadID) >= minimumNumberOfWindows)
					{
						if (auto hThread = OpenThread(THREAD_RESUME, FALSE, te.th32ThreadID); hThread != NULL)
						{
							ResumeThread(hThread);
							CloseHandle(hThread);
						}
					}
					te.dwSize = sizeof(te);
				}
				while (Thread32Next(h, &te));
			}
			CloseHandle(h);
		}

	}
}