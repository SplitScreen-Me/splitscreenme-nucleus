#pragma once
#include <WinUser.h>
#include "MessageFilterBase.h"
#include "protoloader.h"

namespace Proto
{

class RawInputFilter : public Proto::MessageFilterBase<ProtoMessageFilterIDs::RawInputFilterID, WM_INPUT>
{
public:
	static bool Filter(unsigned int message, unsigned int* lparam, unsigned int* wparam, intptr_t hwnd)
	{
		*wparam = RIM_INPUT;
		return true;
	}

	static const char* Name() { return "Raw Input"; }
	static const char* Description()
	{
		return
		"Modifies raw input messages to pretend they are always received while the game is in the foreground. "
		"Use this if the game isn't responding to raw input when not in the foreground. "
		"This usually isn't needed since ProtoInput already stops all Windows WM_INPUT messages from reaching the application and synthesizes new ones instead. ";
	}
};

}
