#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"

namespace Proto
{

class MouseButtonFilter : public MessageFilterBase<ProtoMessageFilterIDs::MouseButtonFilterID, WM_LBUTTONDOWN, WM_XBUTTONUP>
{
public:
	static constexpr unsigned int signature = 0x10000000;
	
	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		if (message == WM_LBUTTONDOWN || message == WM_LBUTTONUP ||
			message == WM_RBUTTONDOWN || message == WM_RBUTTONUP ||
			message == WM_MBUTTONDOWN || message == WM_MBUTTONUP ||
			message == WM_XBUTTONDOWN || message == WM_MBUTTONUP)
		{
			if ((*wparam & signature) != 0)
			{
				*wparam = (*wparam) & (~signature);
				return true;
			}
			else return false;
		}

		return true;
	}

	static const char* Name() { return "Mouse Buttons"; }
	static const char* Description()
	{
		return
			"Filters the mouse button press/release messages to only pass synthesized messages by Proto Input. "
			"This prevents the 'real' mouse cursor from interfering with the 'fake' synthesized input. ";
	}
};

}
