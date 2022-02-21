#include "pch.h"
#include "pch.h"
#include "Logging.h"
#include "Globals.h"
#include <string>
#include "InstallHooks.h"
#include "ReRegisterRawInput.h"

/* Each process can only have ONE window registered for input.
 * To deregister raw input, call RegisterRawInputDevices with RIDEV_REMOVE and hwnd = NULL
 * To find the window the process expects WM_INPUT messages, there are 3 methods used:
 *  (1) MessageFilter find the first message of WM_INPUT (problematic if the game window isn't focused)
 *  (2) RegisterRawInputDevices_Hook detects a call. (good if the game spams Register)
 *  (3) GetRegisteredRawInputDevices used when hooks injection starts. (good if already registered before hooks injected)
 * RegisterRawInputDevices is then called to reregister raw input with RIDEV_INPUTSINK
 * RIDEV_INPUTSINK allows the process to receive input in the background, which is the goal. */

namespace ReRegisterRawInput
{
	volatile bool hasReRegisteredRawInput = false;

	BOOL WINAPI RegisterRawInputDevices_Hook(PCRAWINPUTDEVICE pRawInputDevices, UINT uiNumDevices, UINT cbSize)
	{
		//DEBUGLOG("Blocking a RegisterRawInputDevices call\n"); (some games spam Register)

		if (!hasReRegisteredRawInput && ((pRawInputDevices->dwFlags & RIDEV_REMOVE) == 0))
		{
			hasReRegisteredRawInput = true;
			DEBUGLOG("Detected the raw input window from RegisterRawInputDevices_Hook\n");
			reRegisterRawInput(pRawInputDevices->hwndTarget);
		}
		
		return TRUE;//Pretend it succeeded
	}

	void SetupReRegisterRawInput()
	{
		//Install RegisterRawInputDevices hook to block and to monitor
		installHook("user32", "RegisterRawInputDevices", RegisterRawInputDevices_Hook);
		
		//Call GetRegisteredRawInputDevices to check if raw input devices are already registered.
		{
			UINT numDevices = 0;
			const auto size = sizeof(RAWINPUTDEVICE);
			GetRegisteredRawInputDevices(nullptr, &numDevices, size); // Get the number of devices

			RAWINPUTDEVICE* devices = new RAWINPUTDEVICE[numDevices];
			auto ret = GetRegisteredRawInputDevices(devices, &numDevices, size); // Get the devices

			if (numDevices == 0 || ret <= 0)
			{
				const auto err = GetLastError();
				DEBUGLOG("GetRegisteredRawInputDevices couldn't find a raw input hwnd, numDevices = " << numDevices << ", ret(UINT) = " << ret << ", GetLastError = " << err << "\n");
			}
			else
			{
				HWND hwndTarget = NULL;
				for (UINT i = 0; i < numDevices; i++)
				{
					if (devices[i].hwndTarget != NULL)
					{
						hwndTarget = devices[i].hwndTarget;
					}
				}

				DEBUGLOG("GetRegisteredRawInputDevices found raw input hwnd = " << hwndTarget << "\n");
				reRegisterRawInput(hwndTarget);
			}
		}
	}
	
	void reRegisterRawInput(HWND hwnd)
	{
		DEBUGLOG("Reregistering raw input for hwnd " << hwnd << "\n");
		hasReRegisteredRawInput = true;
				
		//Deregister mouse
		if (Globals::options.reRegisterRawInputMouse)
		{
			RAWINPUTDEVICE rid[1];
			rid[0].usUsagePage = 0x01;
			rid[0].usUsage = 0x02;
			rid[0].dwFlags = RIDEV_REMOVE;
			rid[0].hwndTarget = NULL;
			auto res = RegisterRawInputDevices(rid, 1, sizeof(rid[0]));
			DEBUGLOG("Deregister mouse result: " << res << "\n");
		}

		//Deregister keyboard
		if(Globals::options.reRegisterRawInputKeyboard)
		{
			RAWINPUTDEVICE rid[1];
			rid[0].usUsagePage = 0x01;
			rid[0].usUsage = 0x06;
			rid[0].dwFlags = RIDEV_REMOVE;
			rid[0].hwndTarget = NULL;
			auto res = RegisterRawInputDevices(rid, 1, sizeof(rid[0]));
			DEBUGLOG("Deregister keyboard result: " << res << "\n");
		}

		//Reregister mouse
		if(Globals::options.reRegisterRawInputMouse)
		{
			RAWINPUTDEVICE rid[1];
			rid[0].usUsagePage = 0x01;
			rid[0].usUsage = 0x02;
			rid[0].dwFlags = RIDEV_INPUTSINK;
			rid[0].hwndTarget = hwnd;
			auto res = RegisterRawInputDevices(rid, 1, sizeof(rid[0]));
			DEBUGLOG("Reregister mouse result: " << res << "\n");
		}

		//Reregister keyboard
		if(Globals::options.reRegisterRawInputKeyboard)
		{
			RAWINPUTDEVICE rid[1];
			rid[0].usUsagePage = 0x01;
			rid[0].usUsage = 0x06;
			rid[0].dwFlags = RIDEV_INPUTSINK;
			rid[0].hwndTarget = hwnd;
			auto res = RegisterRawInputDevices(rid, 1, sizeof(rid[0]));
			DEBUGLOG("Reregister keyboard result: " << res << "\n");
		}
	}
}