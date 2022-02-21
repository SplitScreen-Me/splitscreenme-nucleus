#pragma once
#include <vector>
#include <windows.h>
#include <bitset>
#include <hidusage.h>

namespace Proto
{

struct RawInputState
{
	std::vector<void*> selectedKeyboardHandles{};
	std::vector<void*> deselectedKeyboardHandles{};
	std::vector<void*> selectedMouseHandles{};
	std::vector<void*> deselectedMouseHandles{};
	
	std::vector<void*> keyboardHandles{};
	std::vector<void*> mouseHandles{};
		
	bool sendMouseWheelMessages = false;
	bool sendMouseButtonMessages = false;
	bool sendMouseMoveMessages = false;
	bool sendKeyboardPressMessages = false;

	// 'Freeze' input means don't send any input to the game, so it doesn't interfere when setting things up
	bool externalFreezeInput = false;
	bool freezeInput = false;
	bool freezeInputWhileGuiOpened = true;
	bool guiOpened = false; // This is just a copy of the variable
};

const size_t RawInputBufferSize = 1024;

class RawInput
{
private:
	static std::bitset<9> usages;
	static std::vector<HWND> forwardingWindows;

	static const std::vector<USAGE> usageTypesOfInterest;

	static void ProcessMouseInput(const RAWMOUSE& data, HANDLE deviceHandle);
	static void ProcessKeyboardInput(const RAWKEYBOARD& data, HANDLE deviceHandle);

public:
	static RawInputState rawInputState;
	static HWND rawInputHwnd;
	static bool forwardRawInput;

	// Passes input from all devices to the game. Proto Input doesn't process anything
	static bool rawInputBypass;
	
	static std::vector<RAWINPUT> rawinputs;
	static RAWINPUT inputBuffer[RawInputBufferSize];

	static bool lockInputToggleEnabled;
	
	static void RefreshDevices();

	static void AddSelectedMouseHandle(unsigned int handle);
	static void AddSelectedKeyboardHandle(unsigned int handle);
	
	static void AddWindowToForward(HWND hwnd);
	static void SetUsageBitField(std::bitset<9> _usages);
	static std::bitset<9> GetUsageBitField();

	static void ProcessRawInput(HRAWINPUT rawInputHandle, bool inForeground, const MSG& msg);
		
	static void InitialiseRawInput();

	static void UnregisterGameFromRawInput();
	static void RegisterProtoForRawInput();
};

}
