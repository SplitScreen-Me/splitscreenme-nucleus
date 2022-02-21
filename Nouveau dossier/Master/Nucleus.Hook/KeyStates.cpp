#include "pch.h"
#include "Logging.h"

namespace KeyStates
{
	//(unused from USS)
	//UINT16 vkey_state; //Stores the mouse keys (5 of them) and the WASD keys. (1=on, 0=off)

	const int VKEYS_STATE_SIZE = 256 / 8;//256 vkeys, 8 bits per byte
	BYTE* vkeysState = new BYTE[VKEYS_STATE_SIZE]();

	void setVkeyState(const int vkey, const bool down)
	{
		if (vkey >= 0xFF) return;

		const auto x = (vkey / 8);
		const BYTE shift = (1 << (vkey % 8));
		if (down)
			vkeysState[x] |= shift;
		else
			vkeysState[x] &= (~shift);

		if (vkey == VK_LSHIFT || vkey == VK_RSHIFT) setVkeyState(VK_SHIFT, down);
		else if (vkey == VK_LMENU || vkey == VK_RMENU) setVkeyState(VK_MENU, down);
		else if (vkey == VK_LCONTROL || vkey == VK_RCONTROL) setVkeyState(VK_CONTROL, down);
	}

	bool isVkeyDown(const int vkey)
	{
		if (vkey >= 0xFF) return false;

		const BYTE p = vkeysState[vkey / 8];
		const bool ret = (p & (1 << (vkey % 8))) != 0;

		if (!ret)
		{
			if (vkey == VK_LSHIFT || vkey == VK_RSHIFT) return isVkeyDown(VK_SHIFT);
			if (vkey == VK_LMENU || vkey == VK_RMENU) return isVkeyDown(VK_MENU); //alt
			if (vkey == VK_LCONTROL || vkey == VK_RCONTROL) return isVkeyDown(VK_CONTROL);
		}

		return ret;
	}
}