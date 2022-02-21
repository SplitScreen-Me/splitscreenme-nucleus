#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"

namespace Proto
{

class KeyboardButtonFilter : public MessageFilterBase<ProtoMessageFilterIDs::KeyboardButtonFilterID, WM_KEYDOWN, WM_KEYUP>
{
public:
	static constexpr unsigned int signature = 0x10000000;

	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		if ((*wparam & signature) != 0)
		{
			*wparam = (*wparam) & (~signature);
			return true;
		}
		else return false;
	}

	static const char* Name() { return "Keyboard Buttons"; }
	static const char* Description()
	{
		return
			"Filters the keyboard button press/release messages to only pass synthesized messages by Proto Input. "
			"This prevents the 'real' keyboard from interfering with the 'fake' synthesized input. ";
	}
};

}
