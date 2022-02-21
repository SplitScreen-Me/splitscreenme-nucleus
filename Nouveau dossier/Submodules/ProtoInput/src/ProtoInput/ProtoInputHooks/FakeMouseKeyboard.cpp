#include <windows.h>
#include "FakeMouseKeyboard.h"
#include "HwndSelector.h"

namespace Proto
{

FakeMouseState FakeMouseKeyboard::mouseState{};
FakeKeyboardState FakeMouseKeyboard::keyboardState{};

void FakeMouseKeyboard::AddMouseDelta(int dx, int dy)
{
	mouseState.x += dx;
	mouseState.y += dy;

	if (!mouseState.ignoreMouseBounds)
	{
		int min = mouseState.extendMouseBounds ? -100 : -1;
		if (mouseState.x < min)
			mouseState.x = min;
		if (mouseState.y < min)
			mouseState.y = min;

		if (int max = mouseState.extendMouseBounds ? HwndSelector::windowWidth + 100 : HwndSelector::windowWidth; mouseState.x > max)
			mouseState.x = max;

		if (int max = mouseState.extendMouseBounds ? HwndSelector::windowHeight + 100 : HwndSelector::windowHeight; mouseState.y > max)
			mouseState.y = max;
		
		if (mouseState.hasClipCursor)
		{
			if (mouseState.x < mouseState.clipClientLeft)
				mouseState.x = mouseState.clipClientLeft;
			if (mouseState.y < mouseState.clipClientTop)
				mouseState.y = mouseState.clipClientTop;

			if (mouseState.x > mouseState.clipClientRight)
				mouseState.x = mouseState.clipClientRight;

			if (mouseState.y > mouseState.clipClientBottom)
				mouseState.y = mouseState.clipClientBottom;
		}
	}
}

void FakeMouseKeyboard::SetMousePos(int x, int y)
{
	mouseState.x = x;
	mouseState.y = y;

	if (!mouseState.ignoreMouseBounds)
	{
		int min = mouseState.extendMouseBounds ? -100 : -1;
		if (mouseState.x < min)
			mouseState.x = min;
		if (mouseState.y < min)
			mouseState.y = min;

		if (int max = mouseState.extendMouseBounds ? HwndSelector::windowWidth + 100 : HwndSelector::windowWidth; mouseState.x > max)
			mouseState.x = max;

		if (int max = mouseState.extendMouseBounds ? HwndSelector::windowHeight + 100 : HwndSelector::windowHeight; mouseState.y > max)
			mouseState.y = max;

		if (mouseState.hasClipCursor)
		{
			if (mouseState.x < mouseState.clipClientLeft)
				mouseState.x = mouseState.clipClientLeft;
			if (mouseState.y < mouseState.clipClientTop)
				mouseState.y = mouseState.clipClientTop;

			if (mouseState.x > mouseState.clipClientRight)
				mouseState.x = mouseState.clipClientRight;

			if (mouseState.y > mouseState.clipClientBottom)
				mouseState.y = mouseState.clipClientBottom;
		}
	}
}

void FakeMouseKeyboard::SetClipCursor(int clientLeft, int clientTop, int clientRight, int clientBottom)
{
	mouseState.hasClipCursor = true;
	mouseState.clipClientLeft = clientLeft;
	mouseState.clipClientTop = clientTop;
	mouseState.clipClientRight = clientRight;
	mouseState.clipClientBottom = clientBottom;
}

void FakeMouseKeyboard::RemoveClipCursor()
{
	mouseState.hasClipCursor = false;
}

void FakeMouseKeyboard::SetIgnoreMouseBounds(bool ignore)
{
	mouseState.ignoreMouseBounds = ignore;
}

void FakeMouseKeyboard::SetExtendMouseBounds(bool extend)
{
	mouseState.extendMouseBounds = extend;
}

void FakeMouseKeyboard::ReceivedKeyPressOrRelease(int vkey, bool pressed)
{
	static_assert(keyboardState.keysState.size() == keyboardState.asyncKeysState.size());
	
	if (vkey >= 0 && vkey < keyboardState.keysState.size())
	{
		keyboardState.keysState[vkey] = pressed;

		if (pressed)
			keyboardState.asyncKeysState[vkey] = true;
	}
}

void FakeMouseKeyboard::ClearAsyncKeyState(int vkey)
{
	if (vkey >= 0 && vkey < keyboardState.asyncKeysState.size())
	{
		keyboardState.asyncKeysState[vkey] = false;
	}
}

bool FakeMouseKeyboard::IsKeyStatePressed(int vkey)
{
	if (vkey >= 0 && vkey < keyboardState.keysState.size())
	{
		if (vkey == VK_SHIFT)
			return keyboardState.keysState[VK_SHIFT] || keyboardState.keysState[VK_LSHIFT] || keyboardState.keysState[VK_RSHIFT];
		if (vkey == VK_CONTROL)
			return keyboardState.keysState[VK_CONTROL] || keyboardState.keysState[VK_LCONTROL] || keyboardState.keysState[VK_RCONTROL];
		if (vkey == VK_MENU)
			return keyboardState.keysState[VK_MENU] || keyboardState.keysState[VK_LMENU] || keyboardState.keysState[VK_RMENU];
		if (vkey == VK_LSHIFT)
			return keyboardState.keysState[VK_SHIFT] || keyboardState.keysState[VK_LSHIFT];
		if (vkey == VK_LCONTROL)
			return keyboardState.keysState[VK_CONTROL] || keyboardState.keysState[VK_LCONTROL];
		
		return keyboardState.keysState[vkey];
	}
	
	return false;
}

bool FakeMouseKeyboard::IsAsyncKeyStatePressed(int vkey)
{
	if (vkey >= 0 && vkey < keyboardState.asyncKeysState.size())
	{
		if (vkey == VK_SHIFT)
			return keyboardState.asyncKeysState[VK_SHIFT] || keyboardState.asyncKeysState[VK_LSHIFT] || keyboardState.asyncKeysState[VK_RSHIFT];
		if (vkey == VK_CONTROL)
			return keyboardState.asyncKeysState[VK_CONTROL] || keyboardState.asyncKeysState[VK_LCONTROL] || keyboardState.asyncKeysState[VK_RCONTROL];
		if (vkey == VK_MENU)
			return keyboardState.asyncKeysState[VK_MENU] || keyboardState.asyncKeysState[VK_LMENU] || keyboardState.asyncKeysState[VK_RMENU];
		if (vkey == VK_LSHIFT)
			return keyboardState.asyncKeysState[VK_SHIFT] || keyboardState.asyncKeysState[VK_LSHIFT];
		if (vkey == VK_LCONTROL)
			return keyboardState.asyncKeysState[VK_CONTROL] || keyboardState.asyncKeysState[VK_LCONTROL];
		
		return keyboardState.asyncKeysState[vkey];
	}

	return false;
}

}
