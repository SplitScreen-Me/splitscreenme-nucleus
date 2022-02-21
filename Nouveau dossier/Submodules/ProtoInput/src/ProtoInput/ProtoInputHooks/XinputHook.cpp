#include "XinputHook.h"
#include <Xinput.h>
#include <imgui.h>
#include "dinput.h"
#include "Gui.h"
#include <string>
#include "OpenXinputWrapper.h"


namespace Proto
{

constexpr LONG DINPUT_RANGE_MAX = 32767;
constexpr LONG DINPUT_RANGE_MIN = -32768;
bool XinputHook::useDinput = false;
bool XinputHook::useOpenXinput = false;
IDirectInputDevice8W* dinputDevice = nullptr;
GUID dinputDeviceGuid{};
std::wstring dinputDeviceName{};
IDirectInput8W* dinputPtr = nullptr;
std::vector<GUID> dinputGuids{};
std::vector<std::wstring> dinputDeviceNames{};

unsigned int XinputHook::controllerIndex = 0;
unsigned int XinputHook::controllerIndex2 = 0;
unsigned int XinputHook::controllerIndex3 = 0;
unsigned int XinputHook::controllerIndex4 = 0;

typedef DWORD(WINAPI* t_XInputGetStateEx)(DWORD dwUserIndex, void* pState);
t_XInputGetStateEx XInputGetStateExPtr = nullptr;

typedef DWORD(WINAPI* t_XInputGetState)(DWORD dwUserIndex, XINPUT_STATE* pState);
t_XInputGetState XInputGetStatePtr = nullptr;

typedef DWORD(WINAPI* t_XInputSetState)(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration);
t_XInputSetState XInputSetStatePtr = nullptr;

typedef DWORD(WINAPI* t_XInputGetCapabilities)(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities);
t_XInputGetCapabilities XInputGetCapabilitiesPtr = nullptr;


unsigned int getStateCounter = 0;
unsigned int getStateExCounter = 0;

typedef struct _XINPUT_GAMEPAD_EX
{
	WORD  wButtons;
	BYTE  bLeftTrigger;
	BYTE  bRightTrigger;
	SHORT sThumbLX;
	SHORT sThumbLY;
	SHORT sThumbRX;
	SHORT sThumbRY;
	DWORD dwUnknown;
} XINPUT_GAMEPAD_EX, * PXINPUT_GAMEPAD_EX;

inline std::pair<bool, unsigned int> GetTargetControllerIndex(DWORD dwUserIndex)
{
	// XinputHook::controllerIndex 0 means user wants no controller on this game
	if (dwUserIndex == 0 || dwUserIndex == XUSER_INDEX_ANY)
		return { XinputHook::controllerIndex != 0, XinputHook::controllerIndex - 1 };
	
	if (dwUserIndex == 1)
		return { XinputHook::controllerIndex2 != 0, XinputHook::controllerIndex2 - 1 };
	
	if (dwUserIndex == 2)
		return { XinputHook::controllerIndex3 != 0, XinputHook::controllerIndex3 - 1 };
	
	if (dwUserIndex == 3)
		return { XinputHook::controllerIndex4 != 0, XinputHook::controllerIndex4 - 1 };
	
	return { false, 0 };
}

inline DWORD WINAPI XInputGetState_Inline(DWORD dwUserIndex, XINPUT_STATE* pState, bool extended)
{
	if (extended)
		getStateExCounter++;
	else
		getStateCounter++;

	auto [ connected, targetControllerIndex ] = GetTargetControllerIndex(dwUserIndex);	
	
	if (!connected)
		return ERROR_DEVICE_NOT_CONNECTED;

	if (XinputHook::useOpenXinput)
	{
		return OpenXinput::ProtoOpenXinputGetState(targetControllerIndex, pState, extended);
	}

	if (!XinputHook::GetUseDinput())
	{
		if (extended && XInputGetStateExPtr != nullptr)
		{
			return XInputGetStateExPtr(targetControllerIndex, pState);
		}
		return XInputGetStatePtr(targetControllerIndex, pState);
	}
	if (dinputDevice == nullptr)
		return ERROR_DEVICE_NOT_CONNECTED;

	static DWORD packetNumber = 0;
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
DWORD WINAPI Hook_XInputGetState(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	return XInputGetState_Inline(dwUserIndex, pState, false);
}
DWORD WINAPI Hook_XInputGetStateEx(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	return XInputGetState_Inline(dwUserIndex, pState, true);
}

DWORD WINAPI Hook_XInputSetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration)
{
	auto [connected, targetControllerIndex] = GetTargetControllerIndex(dwUserIndex);
	
	if (!connected)
		return ERROR_DEVICE_NOT_CONNECTED;

	if (XinputHook::useOpenXinput)
	{
		return OpenXinput::ProtoOpenXinputSetState(targetControllerIndex, pVibration);
	}

	if (targetControllerIndex <= 3)
		return XInputSetStatePtr(targetControllerIndex, pVibration);
	
	return ERROR_SUCCESS;
}

DWORD WINAPI Hook_XInputGetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities)
{
	auto [connected, targetControllerIndex] = GetTargetControllerIndex(dwUserIndex);

	if (!connected)
		return ERROR_DEVICE_NOT_CONNECTED;

	if (XinputHook::useOpenXinput)
	{
		return OpenXinput::ProtoOpenXinputGetCapabilities(targetControllerIndex, dwFlags, pCapabilities);
	}

	// Can have a higher index than 3 if using Dinput -> Xinput translation or OpenXinput
	if (targetControllerIndex <= 3)
		return XInputGetCapabilitiesPtr(targetControllerIndex, dwFlags, pCapabilities);
	
	return XInputGetCapabilitiesPtr(0, dwFlags, pCapabilities);
}

BOOL CALLBACK DIEnumDevicesCallback(LPCDIDEVICEINSTANCE lpddi, LPVOID pvRef)
{
	// https://www.usb.org/sites/default/files/documents/hut1_12v2.pdf page 26:
	//	4 : Joystick
	//	5 : Game Pad
	bool added = false;
	if (lpddi->wUsage == 4 || lpddi->wUsage == 5)
	{
		dinputGuids.push_back(lpddi->guidInstance);
		dinputDeviceNames.push_back(lpddi->tszInstanceName);
		added = true;
	}
	printf("DirectInput device enumerate, instanceName=%ws, productName=%ws, usage=%d, usagePage=%d, added to dinputGuids list = %s\n",
		   lpddi->tszInstanceName, lpddi->tszProductName, lpddi->wUsage, lpddi->wUsagePage, (added ? "true" : "false"));
	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallback(LPCDIDEVICEOBJECTINSTANCE lpddoi, LPVOID pvRef)
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

void SelectDinputDevice()
{
	dinputDevice = nullptr;
	dinputDeviceGuid = GUID{};
	dinputDeviceName = L"None";
	if (!XinputHook::GetUseDinput())
		return;

	if (XinputHook::controllerIndex == 0)
		return;

	if (XinputHook::controllerIndex > dinputGuids.size())
	{
		fprintf(stderr, "Not enough Dinput controllers (%d) for the selected controller index (%d)\n", dinputGuids.size(), XinputHook::controllerIndex);
		MessageBoxA(NULL, "Not enough Dinput controllers for the selected controller index\n", "Error", MB_OK);
	}

	else
	{
		dinputDeviceGuid = dinputGuids[XinputHook::controllerIndex - 1];
		dinputDeviceName = dinputDeviceNames[XinputHook::controllerIndex - 1];
		printf("Selected Dinput controller GUID %lu-%u-%u...\n", dinputDeviceGuid.Data1, dinputDeviceGuid.Data2, dinputDeviceGuid.Data3);
		const auto createDeviceResult = dinputPtr->CreateDevice(dinputDeviceGuid, &dinputDevice, nullptr);
		if (createDeviceResult != DI_OK)
			fprintf(stderr, "Dinput CreateDevice failed: 0x%lX\n", createDeviceResult);
		else
		{
			dinputDevice->SetCooperativeLevel(Proto::ProtoGuiHwnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);
			dinputDevice->SetDataFormat(&c_dfDIJoystick2);
			DIDEVCAPS caps;
			caps.dwSize = sizeof(DIDEVCAPS);
			auto getCapabilitiesResult = dinputDevice->GetCapabilities(&caps);
			printf("Dinput device number of buttons %d, number of axes %d\n", caps.dwButtons, caps.dwAxes);
			dinputDevice->EnumObjects(&DIEnumDeviceObjectsCallback, dinputDevice, DIDFT_AXIS);
			const HRESULT acquireResult = dinputDevice->Acquire();
			if (acquireResult == DI_OK)
				printf("Successfully acquired Dinput device\n");
			else
				fprintf(stderr, "Failed to acquire Dinput device\n");
		}
	}
}

void PollDinputDevices()
{
	if (!XinputHook::GetUseDinput())
		return;

	const HRESULT dinputCreateResult = DirectInput8Create(GetModuleHandleW(0), DIRECTINPUT_VERSION, IID_IDirectInput8,
														  (void**)&(dinputPtr), nullptr);
	
	if (dinputCreateResult != DI_OK)
	{
		fprintf(stderr, "Failed DirectInput8Create: 0x%X\n", dinputCreateResult);
	}
	else
	{
		dinputGuids.clear();
		dinputDeviceNames.clear();
		dinputPtr->EnumDevices(DI8DEVCLASS_ALL, DIEnumDevicesCallback, nullptr, DIEDFL_ALLDEVICES);
		SelectDinputDevice();
	}
}

void XinputHook::ShowGuiStatus()
{
	ImGui::TextWrapped("Using OpenXinput will allow more than 4 xinput controllers, however it won't work with non-xinput controllers. "
					   "Dinput to Xinput redirection allows more than 4 of any controllers, although the emulation isn't perfect (e.g. both triggers can't be used simultaneously). ");
	ImGui::Separator();

	ImGui::Text("Get state calls: %d, Get state extended calls: %d", getStateCounter, getStateExCounter);
	
	{
		bool _useDinput = GetUseDinput();
		ImGui::PushID(123894);
		ImGui::Checkbox("", &_useDinput);
		ImGui::SameLine();
		ImGui::TextWrapped("Enable Dinput to Xinput redirection");
		SetUseDinput(_useDinput);
		ImGui::PopID();
	}
	{
		ImGui::PushID(123895);
		ImGui::Checkbox("", &useOpenXinput);
		ImGui::SameLine();
		ImGui::TextWrapped("Enable OpenXinput (allows more than 4 xinput controllers)");
		ImGui::PopID();
	}
	ImGui::TextWrapped("Controller index 0 implies no controller");
	if (ImGui::Button("Refresh devices"))
	{
		PollDinputDevices();
	}

	if (ImGui::SliderInt("Controller index", (int*)&controllerIndex, 0, 16, "%d", ImGuiSliderFlags_AlwaysClamp))
		SelectDinputDevice();

	ImGui::SliderInt("Controller index 2", (int*)&controllerIndex2, 0, 16, "%d", ImGuiSliderFlags_AlwaysClamp);
	ImGui::SliderInt("Controller index 3", (int*)&controllerIndex3, 0, 16, "%d", ImGuiSliderFlags_AlwaysClamp);
	ImGui::SliderInt("Controller index 4", (int*)&controllerIndex4, 0, 16, "%d", ImGuiSliderFlags_AlwaysClamp);
	
	if (dinputDevice != nullptr)
	{
		ImGui::TextWrapped("Selected Dinput device \"%ws\" (GUID %lu-%u-%u)",
						   dinputDeviceName.c_str(), dinputDeviceGuid.Data1, dinputDeviceGuid.Data2, dinputDeviceGuid.Data3);
	}
	else
	{
		ImGui::TextWrapped("No Dinput device selected/found");
	}
	if (useDinput)
	{
		if (ImGui::ListBoxHeader("Dinput devices"))
		{
			for (int i = 0; i < dinputDeviceNames.size(); ++i)
			{
				ImGui::Text("(%d) %ws", i + 1, dinputDeviceNames[i].c_str());
			}

			ImGui::ListBoxFooter();
		}
	}
}

void XinputHook::InstallImpl()
{
	if (static bool pollDevices = true; pollDevices)
	{
		pollDevices = false;
		PollDinputDevices();
	}

	// Some games (e.g. Terraria) haven't loaded the dlls when we inject hooks. So load all XInput dlls.
	const wchar_t* xinputNames[] = {
				L"xinput1_3.dll", L"xinput1_4.dll", L"xinput1_2.dll", L"xinput1_1.dll", L"xinput9_1_0.dll"
	};
	for (const auto xinputName : xinputNames)
	{
		if (LoadLibraryW(xinputName) == nullptr)
		{
			fprintf(stderr, "Not hooking %ws as failed to load dll\n", xinputName);
			continue;
		}

		if (GetModuleHandleW(xinputName) == nullptr)
		{
			fprintf(stderr, "Not hooking %ws as failed get module\n", xinputName);
			continue;
		}

		hookInfos.push_back(std::get<1>(InstallNamedHook(xinputName, "XInputGetState", Hook_XInputGetState)));
		hookInfos.push_back(std::get<1>(InstallNamedHook(xinputName, "XInputSetState", Hook_XInputSetState)));
		hookInfos.push_back(std::get<1>(InstallNamedHook(xinputName, "XInputGetCapabilities", Hook_XInputGetCapabilities)));
	}
	//XinputGetStateEx (hidden call, ordinal 100). Only present in xinput1_4.dll and xinput1_3.dll. Used by EtG and DoS2
	//DWORD as 1st param and similar structure pointer for 2nd param (with an extra DWORD at the end). Can be treated as a normal XINPUT_STATE.
	if (nullptr != LoadLibraryW(L"xinput1_4.dll"))
	{
		hookInfos.push_back(std::get<1>(InstallNamedHook(L"xinput1_4.dll", (LPCSTR)(100), Hook_XInputGetStateEx, true)));
	}
	if (nullptr != LoadLibraryW(L"xinput1_3.dll"))
	{
		hookInfos.push_back(std::get<1>(InstallNamedHook(L"xinput1_3.dll", (LPCSTR)(100), Hook_XInputGetStateEx, true)));
		
		XInputGetStateExPtr = t_XInputGetStateEx(GetProcAddress(GetModuleHandleW(L"xinput1_3.dll"), (LPCSTR)(100)));
		XInputGetStatePtr = t_XInputGetState(GetProcAddress(GetModuleHandleW(L"xinput1_3.dll"), "XInputGetState"));
		XInputSetStatePtr = t_XInputSetState(GetProcAddress(GetModuleHandleW(L"xinput1_3.dll"), "XInputSetState"));
		XInputGetCapabilitiesPtr = t_XInputGetCapabilities(GetProcAddress(GetModuleHandleW(L"xinput1_3.dll"), "XInputGetCapabilities"));

		if (XInputGetStateExPtr == nullptr)
			MessageBoxA(NULL, "XInputGetStateExPtr is null", "Error", MB_OK);

		if (XInputGetStatePtr == nullptr)
			MessageBoxA(NULL, "XInputGetStatePtr is null", "Error", MB_OK);

		if (XInputSetStatePtr == nullptr)
			MessageBoxA(NULL, "XInputSetStatePtr is null", "Error", MB_OK);

		if (XInputGetCapabilitiesPtr == nullptr)
			MessageBoxA(NULL, "XInputGetCapabilitiesPtr is null", "Error", MB_OK);
	}
}

void XinputHook::UninstallImpl()
{
	for (auto& handle : hookInfos)
	{
		UninstallHook(&handle);
	}
	hookInfos.clear();
}

void XinputHook::SetUseDinput(bool _useDinput)
{
	useDinput = _useDinput;
	if (dinputPtr == nullptr)
		PollDinputDevices();
}

}