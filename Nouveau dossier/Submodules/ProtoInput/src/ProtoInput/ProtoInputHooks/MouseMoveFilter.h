#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"
#include "FakeMouseKeyboard.h"

namespace Proto
{

class MouseMoveFilter : public Proto::MessageFilterBase<ProtoMessageFilterIDs::MouseMoveFilterID, WM_MOUSEMOVE>
{
public:
	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		*wparam = FakeMouseKeyboard::GetMouseMkFlags();
				
		*lparam = MAKELPARAM(FakeMouseKeyboard::GetMouseState().x, FakeMouseKeyboard::GetMouseState().y);

		return true;
		
	}

	static const char* Name() { return "Mouse move"; }
	static const char* Description()
	{
		return
			"Modifies the WM_MOUSEMOVE messages to return our fake mouse state. "
			"The WM_MOUSEMOVE messages also contains information about the modifier (shift and ctrl) keys and the mouse buttons. "
			"This filter can be important for mouse movement to work correctly, "
			"however it can be processor intensive, so disable if not necessary. ";
	}
};

}
