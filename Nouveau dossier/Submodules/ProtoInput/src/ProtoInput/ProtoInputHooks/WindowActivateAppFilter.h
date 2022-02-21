#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"

namespace Proto
{

class WindowActivateAppFilter : public Proto::MessageFilterBase<ProtoMessageFilterIDs::WindowActivateAppFilterID, WM_ACTIVATEAPP>
{
public:
	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		*wparam = TRUE;
		*lparam = 0;
		return true;
	}

	static const char* Name() { return "Window Activate App"; }
	static const char* Description()
	{
		return
			"WM_ACTIVATEAPP is sent whenever a window is activated or deactivated. "
			"This filter makes the message always seem that the game is being focused. ";
	}
};

}
