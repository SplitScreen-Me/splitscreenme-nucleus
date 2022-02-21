#pragma once
#include <windows.h>

namespace Proto
{

class FocusMessageLoop
{
	static HANDLE loopThread;

public:
	static bool running;

	static int sleepMilliseconds;

	struct ToSend
	{
		bool wm_activate = true;
		bool wm_activateapp = true;
		bool wm_ncactivate = true;
		bool wm_setfocus = true;
		bool wm_mouseactivate = true;
	};

	static ToSend messagesToSend;
	
	static void SetupThread();
	static void StartMessageLoop();
	static void PauseMessageLoop();
	static void Cleanup();
};

}