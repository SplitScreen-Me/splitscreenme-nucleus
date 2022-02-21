#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"


namespace Proto
{

class WindowActivateFilter : public Proto::MessageFilterBase<ProtoMessageFilterIDs::WindowActivateFilterID, WM_ACTIVATE>
{
public:
	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		*wparam = WA_ACTIVE;
		*lparam = 0;
		return true;
	}

	static const char* Name() { return "Window Activate"; }
	static const char* Description()
	{
		return
			"WM_ACTIVATE is sent whenever a window is activated or deactivated. "
			"This filter makes the message always seem that the game is being focused. ";
	}
};

}
