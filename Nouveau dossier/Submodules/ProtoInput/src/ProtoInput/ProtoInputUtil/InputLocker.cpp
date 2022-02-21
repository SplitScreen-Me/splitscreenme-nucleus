#include "framework.h"
#include "include/protoinpututil.h"
#include <cstdio>

bool isLocked = false;
bool installedHooks = false;
HANDLE threadHandle = nullptr;

LRESULT CALLBACK LowLevelMouseProc(
	_In_ int    nCode,
	_In_ WPARAM wParam,
	_In_ LPARAM lParam
)
{
	//TODO: change wparam/lparam instead of blocking
	return isLocked ? 1 : CallNextHookEx(nullptr, nCode, wParam, lParam);
}

LRESULT CALLBACK LowLevelKeyboardProc(
	_In_ int    nCode,
	_In_ WPARAM wParam,
	_In_ LPARAM lParam
)
{
	// if (!isLocked || ((KBDLLHOOKSTRUCT*)lParam)->vkCode == VK_END)
	if (!isLocked)
	{
		return CallNextHookEx(nullptr, nCode, wParam, lParam);
	}
	else
	{
		auto p = (KBDLLHOOKSTRUCT*)lParam;
		p->vkCode = 0;
		return CallNextHookEx(nullptr, nCode, wParam, lParam);
	}
}

DWORD WINAPI LoopThread(LPVOID lpParameter)
{
	while (true)
	{
		if (isLocked)
		{
			SetForegroundWindow(GetDesktopWindow());
			RECT rect{ 0,0,0,0 };
			ClipCursor(&rect);
			// ShowCursor(FALSE);
			// SetCursor(NULL);
		}
		else
		{
			ClipCursor(NULL);
		}

		//TODO: probably not the best idea
		Sleep(5);
	}

	return 0;
}

extern "C" __declspec(dllexport) unsigned int LockInput(bool lock)
{
	isLocked = lock;

	if (threadHandle == nullptr)
	{
		threadHandle = CreateThread(nullptr, 0, (LPTHREAD_START_ROUTINE)LoopThread,
									GetModuleHandle(0), CREATE_SUSPENDED, 0);

		// if (threadHandle != nullptr)
		// 	CloseHandle(threadHandle);
	}
	
	if (lock && !installedHooks)
	{
		installedHooks = true;
		SetWindowsHookExW(WH_MOUSE_LL, LowLevelMouseProc, GetModuleHandle(0), 0);
		SetWindowsHookExW(WH_KEYBOARD_LL, LowLevelKeyboardProc, GetModuleHandle(0), 0);
	}

	if (threadHandle != nullptr)
	{
		if (lock)
			ResumeThread(threadHandle);
		else
		{
			ClipCursor(NULL);
			SuspendThread(threadHandle);
		}
	}

	return (threadHandle != nullptr) ? GetThreadId(threadHandle) : 0;
}