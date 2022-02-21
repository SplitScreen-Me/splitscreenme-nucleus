#pragma once
#include "pch.h"

namespace ReRegisterRawInput
{
	extern volatile bool hasReRegisteredRawInput;

	void reRegisterRawInput(HWND hwnd);

	void SetupReRegisterRawInput();
}