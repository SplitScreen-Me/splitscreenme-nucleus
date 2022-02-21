#include <windows.h>
#include "RegisterRawInputHook.h"
#include <cassert>
#include <hidusage.h>
#include <bitset>
#include "RawInput.h"
#include <imgui.h>
#include "HwndSelector.h"

namespace Proto
{

bool RegisterRawInputHook::logCallsToRegisterRawInput = false;
RegisterRawInputHook* registerRawInputHookPtr = nullptr;

BOOL WINAPI Hook_RegisterRawInputDevices(PCRAWINPUTDEVICE pRawInputDevices, UINT uiNumDevices, UINT cbSize)
{
	if (RegisterRawInputHook::logCallsToRegisterRawInput)
		printf("Detected a call to RegisterRawInputDevices\n");
	
	HWND targetHWND = NULL;
	
	// Do a copy since it might already have some values set
	// (some games will use multiple calls to RegisterRawInputDevices instead of passing an array)
	std::bitset<9> usagesToForward = RawInput::GetUsageBitField(); 

	for (size_t i = 0; i < uiNumDevices; i++)
	{
		//TODO: hwndTarget can be NULL if raw input follows the keyboard focus.
		const auto targetI = pRawInputDevices[i].hwndTarget;
		if (targetI != RawInput::rawInputHwnd)
		{
			if (targetHWND != NULL && targetHWND != targetI && RegisterRawInputHook::logCallsToRegisterRawInput)
				printf("WARNING: The game appears to have multiple hwnds subscribed to raw input. We are only using one\n");

			if (targetI != NULL)
				targetHWND = targetI;

			const auto usage = pRawInputDevices[i].usUsage;
			
			if (RegisterRawInputHook::logCallsToRegisterRawInput)
				printf("Detected raw input usage 0x%X\n", usage);
			
			if (usage < HID_USAGE_GENERIC_POINTER || usage > HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER)
				fprintf(stderr, "Raw input usUsage out of range\n");
			else
			{
				usagesToForward[usage] = true;
			}
		}
	}

	if (targetHWND == nullptr)
	{
		if (RegisterRawInputHook::logCallsToRegisterRawInput)
			printf("Couldn't find a hwnd subscribed to raw input (The call was probably to unsubscribe). Going to add the main window just in case it was trying to follow keyboard focus. \n");

		registerRawInputHookPtr->AddWindowToForward((HWND)HwndSelector::GetSelectedHwnd(), usagesToForward);
	}
	else
	{
		if (RegisterRawInputHook::logCallsToRegisterRawInput)
			printf("Found raw input hwnd: 0x%X\n", targetHWND);
		
		registerRawInputHookPtr->AddWindowToForward(targetHWND, usagesToForward);
	}

	return TRUE;
}

void RegisterRawInputHook::FindAlreadySubscribedWindows()
{
	UINT numDevices = 0;
	const auto size = sizeof(RAWINPUTDEVICE);
	GetRegisteredRawInputDevices(nullptr, &numDevices, size); // Get the number of devices

	RAWINPUTDEVICE* devices = new RAWINPUTDEVICE[numDevices];
	auto ret = GetRegisteredRawInputDevices(devices, &numDevices, size); // Get the devices

	if (numDevices == 0 || ret <= 0)
	{
		printf("Couldn't find a window that's already subscribed to raw input. numDevices = %d, ret = 0x%X, GetLastError = 0x%X\n",
			   numDevices, ret, GetLastError());
	}
	else
	{
		// https://docs.microsoft.com/en-us/windows-hardware/drivers/hid/hid-usages#usage-id
		//	Usage ID	Usage Name 					hidusage.h constant
		//	0x01		Pointer 					HID_USAGE_GENERIC_POINTER
		//	0x02		Mouse 						HID_USAGE_GENERIC_MOUSE
		//	0x04		Joystick 					HID_USAGE_GENERIC_JOYSTICK
		//	0x05		Game Pad 					HID_USAGE_GENERIC_GAMEPAD
		//	0x06		Keyboard 					HID_USAGE_GENERIC_KEYBOARD
		//	0x07		Keypad 						HID_USAGE_GENERIC_KEYPAD
		//	0x08		Multi - axis Controller 	HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER
		HWND targetHWND = NULL;
		std::bitset<9> usagesToForward{};

		for (size_t i = 0; i < numDevices; i++)
		{
			//TODO: hwndTarget can be NULL if raw input follows the keyboard focus.
			const auto targetI = devices[i].hwndTarget;
			if (targetI != NULL && targetI != RawInput::rawInputHwnd)
			{
				if (targetHWND != NULL && targetHWND != targetI)
					printf("WARNING: The game appears to have multiple hwnds subscribed to raw input. We are only using one\n");

				targetHWND = targetI;

				const auto usage = devices[i].usUsage;

				if (usage < HID_USAGE_GENERIC_POINTER || usage > HID_USAGE_GENERIC_MULTI_AXIS_CONTROLLER)
					fprintf(stderr, "Raw input usUsage out of range\n");
				else
				{
					usagesToForward[usage] = true;
				}
			}
		}

		if (targetHWND == nullptr)
			printf("Couldn't find a hwnd subscribed to raw input\n");
		else
		{
			printf("Found raw input hwnd: 0x%X\n", targetHWND);
			AddWindowToForward(targetHWND, usagesToForward);
		}
	}
}

void RegisterRawInputHook::AddWindowToForward(HWND hwnd, std::bitset<9> usages)
{
	if (updateDeviceTypeOnGameUpdate)
	{
		RawInput::SetUsageBitField(usages);
	}

	RawInput::AddWindowToForward(hwnd);
}

void RegisterRawInputHook::ShowGuiStatus()
{
	ImGui::Checkbox("Forward raw input", &RawInput::forwardRawInput);
	ImGui::Checkbox("Log calls to registering raw input", &logCallsToRegisterRawInput);
		
	ImGui::Checkbox("Raw input bypass", &RawInput::rawInputBypass);
	if (ImGui::IsItemHovered())
	{
		ImGui::BeginTooltip();
		ImGui::PushTextWrapPos(ImGui::GetFontSize() * 35.0f);
		ImGui::TextUnformatted("Forwards Raw Input to the game without processing it. ");
		ImGui::PopTextWrapPos();
		ImGui::EndTooltip();
	}
}

void RegisterRawInputHook::InstallImpl()
{
	if (!installedAtLeastOnce)
	{
		installedAtLeastOnce = true;
		std::bitset<9> usages{};
		// usages[HID_USAGE_GENERIC_MOUSE] = true;
		// usages[HID_USAGE_GENERIC_KEYBOARD] = true;
		RawInput::SetUsageBitField(usages);
	}

	auto [status, _hookInfo] = InstallNamedHook(L"user32", "RegisterRawInputDevices", Hook_RegisterRawInputDevices);
	this->hookInfo = _hookInfo;
		
	FindAlreadySubscribedWindows();

	RawInput::UnregisterGameFromRawInput();
	RawInput::RegisterProtoForRawInput();
}

void RegisterRawInputHook::UninstallImpl()
{
	UninstallHook(&hookInfo);
}

RegisterRawInputHook::RegisterRawInputHook()
{
	registerRawInputHookPtr = this;
}

}
