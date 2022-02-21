#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"
#include "Globals.h"
#include "Xinput.h"
#include "dinput.h"
#include <vector>
#include "Controller.h"

const LONG DINPUT_RANGE_MAX = 32767;
const LONG DINPUT_RANGE_MIN = -32768;

IDirectInput8* pDinput;
LPDIRECTINPUTDEVICE8 dinputDevice = nullptr;
std::vector<GUID> dinputGuids;

typedef DWORD(WINAPI* t_XInputGetStateEx)(DWORD dwUserIndex, void* pState);
static t_XInputGetStateEx XInputGetStateEx = nullptr;

typedef struct _XINPUT_GAMEPAD_EX {
	WORD  wButtons;
	BYTE  bLeftTrigger;
	BYTE  bRightTrigger;
	SHORT sThumbLX;
	SHORT sThumbLY;
	SHORT sThumbRX;
	SHORT sThumbRY;
	DWORD dwUnknown;
} XINPUT_GAMEPAD_EX, * PXINPUT_GAMEPAD_EX;

DWORD packetNumber = 0;
inline DWORD WINAPI XInputGetState_Inline(DWORD dwUserIndex, XINPUT_STATE* pState, bool extended)
{
	if (Controller::controllerIndex == 0) // user wants no controller on this game
		return ERROR_DEVICE_NOT_CONNECTED;

	if (!Globals::options.DinputToXinputTranslation)
	{
		if (extended && XInputGetStateEx != nullptr)
		{
			return XInputGetStateEx(Controller::controllerIndex - 1, pState);
		}
		return XInputGetState(Controller::controllerIndex - 1, pState);
	}

	pState->dwPacketNumber = packetNumber++;
	memset(&(pState->Gamepad), 0, extended ? sizeof(XINPUT_GAMEPAD_EX) : sizeof(XINPUT_GAMEPAD));

	dinputDevice->Poll();
	DIJOYSTATE2 diState;
	dinputDevice->GetDeviceState(sizeof(DIJOYSTATE2), &diState);

#define BTN(n, f) if (diState.rgbButtons[n] != 0) pState->Gamepad.wButtons |= f
	BTN(0, XINPUT_GAMEPAD_A);
	BTN(1, XINPUT_GAMEPAD_B);
	BTN(2, XINPUT_GAMEPAD_X);
	BTN(3, XINPUT_GAMEPAD_Y);
	BTN(4, XINPUT_GAMEPAD_LEFT_SHOULDER);
	BTN(5, XINPUT_GAMEPAD_RIGHT_SHOULDER);
	BTN(6, XINPUT_GAMEPAD_BACK);
	BTN(7, XINPUT_GAMEPAD_START);
	BTN(8, XINPUT_GAMEPAD_LEFT_THUMB);
	BTN(9, XINPUT_GAMEPAD_RIGHT_THUMB);
#undef BTN

	const auto pov = diState.rgdwPOV;
	if (!(LOWORD(pov[0]) == 0xFFFF))//POV not centred
	{
		auto deg = (pov[0]) / 4500;
#define DPAD(a,b,c, f) if (deg == (a) || deg == (b) || deg == (c)) pState->Gamepad.wButtons |= f
		DPAD(7, 0, 1, XINPUT_GAMEPAD_DPAD_UP);
		DPAD(1, 2, 3, XINPUT_GAMEPAD_DPAD_RIGHT);
		DPAD(3, 4, 5, XINPUT_GAMEPAD_DPAD_DOWN);
		DPAD(5, 6, 7, XINPUT_GAMEPAD_DPAD_LEFT);
#undef DPAD
	}

#define DEADZONE(x, d) (((x) >= (d) || (x) <= (-(d))) ? (x) : 0)
	pState->Gamepad.sThumbLX = DEADZONE(diState.lX, XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE);
	pState->Gamepad.sThumbLY = -1 - DEADZONE(diState.lY, XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE);
	pState->Gamepad.sThumbRX = DEADZONE(diState.lRx, XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE);
	pState->Gamepad.sThumbRY = -1 - DEADZONE(diState.lRy, XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE);
#undef DEADZONE

#define TRIGGERDEADZONE(x) (((x) >= XINPUT_GAMEPAD_TRIGGER_THRESHOLD) ? (x) : 0)
	const auto triggerAxis = diState.lZ;
	if (triggerAxis > 0)
	{
		const auto x = 255 * ((float)triggerAxis / DINPUT_RANGE_MAX);
		pState->Gamepad.bLeftTrigger = TRIGGERDEADZONE(x);
	}
	else if (triggerAxis < 0)
	{
		const auto x = 255 * ((float)triggerAxis / DINPUT_RANGE_MIN);
		pState->Gamepad.bRightTrigger = TRIGGERDEADZONE(x);
	}
#undef TRIGGERDEADZONE

	return ERROR_SUCCESS;
}

DWORD WINAPI XInputGetState_Hook(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	return XInputGetState_Inline(dwUserIndex, pState, false);
}

DWORD WINAPI XInputGetStateEx_Hook(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	return XInputGetState_Inline(dwUserIndex, pState, true);
}

DWORD WINAPI XInputSetState_Hook(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration)
{
	if (Controller::controllerIndex == 0)
		return ERROR_DEVICE_NOT_CONNECTED;

	if (Controller::controllerIndex <= 4)
		return XInputSetState(Controller::controllerIndex - 1, pVibration);

	return ERROR_SUCCESS;
}

DWORD WINAPI XInputGetCapabilities_Hook(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities)
{
	if (Controller::controllerIndex == 0) 
		// user wants no controller on this game (e.g. if using KBM)
		return ERROR_DEVICE_NOT_CONNECTED;

	// Can have a higher index than 4 if using Dinput -> Xinput translation
	if (Controller::controllerIndex <= 4)
		return XInputGetCapabilities(Controller::controllerIndex - 1, dwFlags, pCapabilities);

	return XInputGetCapabilities(0, dwFlags, pCapabilities);
}

#pragma region DirectInput
static BOOL CALLBACK DIEnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef)
{
	DIDEVICEINSTANCE di = *lpddi;

	bool added = false;

	// https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf page 26:
	//	4 : Joystick
	//	5 : Game Pad

	if (di.wUsage == 4 || di.wUsage == 5)
	{
		dinputGuids.push_back(di.guidInstance);
		added = true;
	}

	DEBUGLOG("DirectInput device enumerate, instanceName=" << di.tszInstanceName << 
		", productName=" << di.tszProductName <<
		", usage=" << di.wUsage << 
		", usagePage=" << di.wUsagePage << 
		", added to dinputGuids list = " << added << 
		"\n");

	return DIENUM_CONTINUE;
}

static BOOL CALLBACK DIEnumDeviceObjectsCallback(LPCDIDEVICEOBJECTINSTANCE lpddoi, LPVOID pvRef)
{
	auto did = static_cast<LPDIRECTINPUTDEVICE8>(pvRef);
	did->Unacquire();

	DIPROPRANGE range;
	range.lMax = DINPUT_RANGE_MAX;
	range.lMin = DINPUT_RANGE_MIN;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(did->SetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;
	
	return DIENUM_CONTINUE;
}

void getDInputDevice()
{
	HRESULT dinputCreateResult = DirectInput8Create(Globals::DllHandle, DIRECTINPUT_VERSION, IID_IDirectInput8,
	                                                (void**) & (pDinput), nullptr);

	if (dinputCreateResult != DI_OK)
	{
		DEBUGLOG("Fail DirectInput8Create, dinputCreateResult=" << dinputCreateResult << "\n");
		return;
	}
	
	DEBUGLOG("Succeed dDirectInput8Create\n");
	pDinput->EnumDevices(DI8DEVCLASS_ALL, DIEnumDevicesCallback, nullptr, DIEDFL_ALLDEVICES);
	//std::qsort(dinputGuids, maxDinputDevices, sizeof(GUID), compareGuids);

	if (Controller::controllerIndex > dinputGuids.size())
	{
		DEBUGLOG("Not selecting DInput controller because controllerIndex out of range\n");
		MessageBox(nullptr, "Not selecting DInput controller because controllerIndex out of range", nullptr, MB_OK);
	}
	else
	{
		const auto controllerGuid = dinputGuids[Controller::controllerIndex - 1];
		DEBUGLOG("cg8=" << controllerGuid.Data1 << "\n");
		const auto createDeviceResult = pDinput->CreateDevice(controllerGuid, &dinputDevice, nullptr);

		if (createDeviceResult != DI_OK)
		{
			DEBUGLOG("DInput create device error: " << createDeviceResult << "\n");
		}
		else
		{
			dinputDevice->SetCooperativeLevel(Globals::hWnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);

			dinputDevice->SetDataFormat(&c_dfDIJoystick2);
			//dinput_device_data_format = 2; //TODO: what's this for?

			DIDEVCAPS caps;
			caps.dwSize = sizeof(DIDEVCAPS);
			auto getCapabilitiesResult = dinputDevice->GetCapabilities(&caps);

			DEBUGLOG("DInput device number of buttons = " << caps.dwButtons << "\n");
			DEBUGLOG("DInput device number of axes = " << caps.dwAxes << "\n");

			dinputDevice->EnumObjects(&DIEnumDeviceObjectsCallback, dinputDevice, DIDFT_AXIS);

			HRESULT acquireResult = dinputDevice->Acquire();

			if (acquireResult == DI_OK)
			{
				DEBUGLOG("Successfully acquired dinput device\n");
			}
			else
			{
				DEBUGLOG("Failed to acquire dinput device\n");
			}
		}
	}
}
#pragma endregion

void installXInputHooks()
{
	DEBUGLOG("Injecting XInput hooks\n");

	if (Globals::options.DinputToXinputTranslation && Controller::controllerIndex != 0)
	{
		getDInputDevice();
	}
	
	// Some games (e.g. Terraria) haven't loaded the dlls when we inject hooks. So load all XInput dlls.
	LPCSTR xinputNames[] = {
				"xinput1_3.dll", "xinput1_4.dll", "xinput1_2.dll", "xinput1_1.dll", "xinput9_1_0.dll"
	};

	for (auto& xinputName : xinputNames)
	{
		
		if (LoadLibrary(xinputName) == nullptr)
		{
			DEBUGLOG("Not hooking " << xinputName << " because could not be loaded\n");
		}
		else
		{
			if (GetModuleHandleA(xinputName) == nullptr)
			{
				DEBUGLOG("Not hooking " << xinputName << " because not loaded (?)\n");
			}
			else
			{
				installHook(xinputName, "XInputGetState", XInputGetState_Hook);
				installHook(xinputName, "XInputSetState", XInputSetState_Hook);
				installHook(xinputName, "XInputGetCapabilities", XInputGetCapabilities_Hook);
			}
		}
	}

	//XinputGetStateEx (hidden call, ordinal 100). Only present in xinput1_4.dll and xinput1_3.dll. Used by EtG and DoS2
	//DWORD as 1st param and similar structure pointer for 2nd param (with an extra DWORD at the end). Can be treated as a normal XINPUT_STATE.
	if (NULL != LoadLibrary("xinput1_4.dll"))
	{
		installHookEx("xinput1_4.dll", (LPCSTR)(100), XInputGetStateEx_Hook, true);
	}

	if (NULL != LoadLibrary("xinput1_3.dll"))
	{
		installHookEx("xinput1_3.dll", (LPCSTR)(100), XInputGetStateEx_Hook, true);
		XInputGetStateEx = t_XInputGetStateEx(GetProcAddress(GetModuleHandle("xinput1_3.dll"), (LPCSTR)(100)));
	}
}