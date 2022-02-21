#include "FocusMessageLoop.h"
#include <cstdio>
#include "HwndSelector.h"
#include "GetKeyboardStateHook.h"

namespace Proto
{

HANDLE FocusMessageLoop::loopThread = nullptr;
bool FocusMessageLoop::running = false;

int FocusMessageLoop::sleepMilliseconds = 5;
FocusMessageLoop::ToSend FocusMessageLoop::messagesToSend{};

DWORD WINAPI LoopThread(LPVOID lpParameter)
{
	printf("Starting focus message loop thread\n");

	//TODO: need to close the thread handle when application closes
	//TODO: check how much cpu this uses

	
	AddThreadToACL(GetCurrentThreadId());

	while(true)
	{		
		const HWND hwnd = (HWND)HwndSelector::GetSelectedHwnd();
		if (hwnd != nullptr)
		{
			if (FocusMessageLoop::messagesToSend.wm_activate) PostMessageW(hwnd, WM_ACTIVATE, WA_CLICKACTIVE, 0);
			if (FocusMessageLoop::messagesToSend.wm_activateapp) PostMessageW(hwnd, WM_ACTIVATEAPP, TRUE, 0);
			if (FocusMessageLoop::messagesToSend.wm_ncactivate) PostMessageW(hwnd, WM_NCACTIVATE, TRUE, 0);
			if (FocusMessageLoop::messagesToSend.wm_setfocus) PostMessageW(hwnd, WM_SETFOCUS, 0, 0);
			if (FocusMessageLoop::messagesToSend.wm_mouseactivate) PostMessageW(hwnd, WM_MOUSEACTIVATE, HwndSelector::GetSelectedHwnd(), 1);

			Sleep(FocusMessageLoop::sleepMilliseconds);
		}
		else
		{
			Sleep(100);
		}
	}
	
	return 0;
}

void FocusMessageLoop::SetupThread()
{
	loopThread = CreateThread(nullptr, 0,(LPTHREAD_START_ROUTINE)LoopThread, 
							  GetModuleHandle(0), CREATE_SUSPENDED, 0);
		
	running = false;
}

void FocusMessageLoop::StartMessageLoop()
{
	if (loopThread == nullptr)
		SetupThread();
	
	ResumeThread(loopThread);
	running = true;
}

void FocusMessageLoop::PauseMessageLoop()
{
	SuspendThread(loopThread);
	running = false;
}

void FocusMessageLoop::Cleanup()
{
	CloseHandle(loopThread);
}

}
