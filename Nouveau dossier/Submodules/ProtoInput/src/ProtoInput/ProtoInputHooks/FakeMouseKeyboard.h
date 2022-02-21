#pragma once
#include <bitset>

namespace Proto
{

struct FakeMouseState
{
	int x, y;
	
	int clipClientLeft;
	int clipClientTop;
	int clipClientRight;
	int clipClientBottom;
	bool hasClipCursor = false;

	bool ignoreMouseBounds = false;
	bool extendMouseBounds = false;
};

struct FakeKeyboardState
{
	// GetAyncKeyState will tell the caller
	// 1. Whether the key is pressed
	// 2. Whether the key has been pressed during the time since the last call to GetAsyncKeyState
	//    (The key doesn't have to currently be pressed for this to be true)
	//
	// To emulate this, take two buffers:
	// keysState records the physical state of the keyboard. It is changed whenever a key is pressed or released
	// asyncKeysState records whether the keys were pressed since the last call.
	//		The buffer is only set to true when a key is pressed (releasing a key does nothing)
	//		The buffer is then wiped at the end of the GetAsyncKeyState hook

	std::bitset<256> keysState{};
	std::bitset<256> asyncKeysState{};	
};

class FakeMouseKeyboard
{
	static FakeMouseState mouseState;
	static FakeKeyboardState keyboardState;
	
public:
	static const FakeMouseState& GetMouseState() { return mouseState; }
	static void AddMouseDelta(int dx, int dy);
	static void SetMousePos(int x, int y);
	
	static void SetClipCursor(int clientLeft, int clientTop, int clientRight, int clientBottom);
	static void RemoveClipCursor();

	static void SetIgnoreMouseBounds(bool ignore);
	static void SetExtendMouseBounds(bool extend);
	
	static void ReceivedKeyPressOrRelease(int vkey, bool pressed);
	static void ClearAsyncKeyState(int vkey);
	static bool IsKeyStatePressed(int vkey);
	static bool IsAsyncKeyStatePressed(int vkey);

	static unsigned int GetMouseMkFlags()
	{
#define PROTO_MMFKEY(x, y) ((FakeMouseKeyboard::IsKeyStatePressed(x)) ? (y) : (0))

		return	PROTO_MMFKEY(VK_CONTROL, MK_CONTROL) |
				PROTO_MMFKEY(VK_SHIFT, MK_SHIFT) |
				PROTO_MMFKEY(VK_LBUTTON, MK_LBUTTON) |
				PROTO_MMFKEY(VK_MBUTTON, MK_MBUTTON) |
				PROTO_MMFKEY(VK_RBUTTON, MK_RBUTTON) |
				PROTO_MMFKEY(VK_XBUTTON1, MK_XBUTTON1) |
				PROTO_MMFKEY(VK_XBUTTON2, MK_XBUTTON2);

#undef PROTO_MMFKEY

	}
};

}
