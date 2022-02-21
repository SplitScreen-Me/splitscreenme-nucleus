#pragma once
#include <cstdint>

namespace Proto
{

class HwndSelector
{
	static intptr_t selectedHwnd;
	
public:
	static int windowWidth, windowHeight;
		
	static intptr_t& GetSelectedHwnd() { return selectedHwnd; }
	static intptr_t const * GetSelectedHwndPtr() { return &selectedHwnd; }
	static void SetSelectedHwnd(intptr_t set);
	static void UpdateMainHwnd(bool logOutput = true);
	static void UpdateWindowBounds();
};

}
