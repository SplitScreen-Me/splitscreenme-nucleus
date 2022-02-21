/*  x360ce - XBOX360 Controller Emulator
 *
 *  https://code.google.com/p/x360ce/
 *
 *  Copyright (C) 2002-2010 Racer_S
 *  Copyright (C) 2010-2014 Robert Krawczyk
 *
 *  x360ce is free software: you can redistribute it and/or modify it under the terms
 *  of the GNU Lesser General Public License as published by the Free Software Foundation,
 *  either version 3 of the License, or any later version.
 *
 *  x360ce is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
 *  without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
 *  PURPOSE.  See the GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License along with x360ce.
 *  If not, see <http://www.gnu.org/licenses/>.
 */

#include "stdafx.h"
#include "Common.h"

#include "InputHook.h"
#include "Config.h"

#include "ForceFeedbackBase.h"
#include "ForceFeedback.h"
#include "ControllerBase.h"
#include "Controller.h"
#include "ControllerCombiner.h"

#include "XInputModuleManager.h"
#include "Globals.h"

extern "C" DWORD WINAPI XInputGetState(DWORD dwUserIndex, XINPUT_STATE* pState)
{
	if (Globals::enableMKBInput)
	{
		if (pState)
		{
			ZeroMemory(pState, sizeof(XINPUT_STATE));
		}
		return ERROR_SUCCESS;
	}

	if (Globals::xInputEnabled)
	{
		if (Globals::xInputReRouteEnabled)
		{
			// if rerouting is enabled we'll use x360ce to reroute directinput back to xinput
			ControllerBase* pController;
			if (!pState)
			{
				return ERROR_BAD_ARGUMENTS;
			}

			u32 initFlag = ControllerManager::Get().DeviceInitialize(0, &pController);
			if (initFlag != ERROR_SUCCESS)
			{
				ZeroMemory(pState, sizeof(XINPUT_STATE));
				return ERROR_SUCCESS;
			}

			u32 result = pController->GetState(pState);
			if (result != ERROR_SUCCESS)
			{
				ZeroMemory(pState, sizeof(XINPUT_STATE));
				return ERROR_SUCCESS;
			}

			if ((pState->Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0)
			{
				int test = -1;
				return result;
			}

			return result;
		}
		else if (dwUserIndex == 0)
		{
			DWORD playerOverride = Globals::xInputPlayerId;
			return XInputModuleManager::Get().XInputGetState(playerOverride, pState);
		}
	}
	else
	{
		return XInputModuleManager::Get().XInputGetState(dwUserIndex, pState);
	}

	if (pState)
	{
		ZeroMemory(pState, sizeof(XINPUT_STATE));
	}
	return ERROR_DEVICE_NOT_CONNECTED;
}

extern "C" DWORD WINAPI XInputSetState(DWORD dwUserIndex, XINPUT_VIBRATION* pVibration)
{
	ControllerBase* pController;
	if (!pVibration)
		return ERROR_BAD_ARGUMENTS;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->SetState(pVibration);
}

extern "C" DWORD WINAPI XInputGetCapabilities(DWORD dwUserIndex, DWORD dwFlags, XINPUT_CAPABILITIES* pCapabilities)
{
	if (Globals::xInputEnabled || Globals::enableMKBInput)
	{
		DWORD playerOverride = Globals::xInputPlayerId;

		if (playerOverride >= 4)
		{
			playerOverride = 0;// show the capabilities of the first gamepad
		}

		if (dwUserIndex == 0)
		{
			if (Globals::xInputReRouteEnabled)
			{
				ControllerBase* pController;
				DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
				if (initFlag != ERROR_SUCCESS)
				{
					// this works for Left 4 Dead 2
					XInputModuleManager::Get().XInputGetCapabilities(0, dwFlags, pCapabilities);
					return ERROR_SUCCESS;
				}

				DWORD result = XInputModuleManager::Get().XInputGetCapabilities(0, dwFlags, pCapabilities);
				//DWORD result = pController->GetCapabilities(dwFlags, pCapabilities);
				if (result != ERROR_SUCCESS)
				{
					return ERROR_SUCCESS;
				}
				return result;
			}
			else
			{
				return XInputModuleManager::Get().XInputGetCapabilities(playerOverride, dwFlags, pCapabilities);
			}
		}
	}
	return ERROR_DEVICE_NOT_CONNECTED;

	// Validate
	if (!pCapabilities || dwFlags != 0 && dwFlags != XINPUT_FLAG_GAMEPAD)
	{
		return ERROR_BAD_ARGUMENTS;
	}

	// Get controller
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);

	// If problem initializing controller, bail
	if (initFlag != ERROR_SUCCESS)
	{
		return initFlag;
	}

	return pController->GetCapabilities(dwFlags, pCapabilities);
}

extern "C" VOID WINAPI XInputEnable(BOOL enable)
{
	// If any controller is native XInput then use state too.
	for (auto it = ControllerManager::Get().GetControllers().begin(); it != ControllerManager::Get().GetControllers().end(); ++it)
	{
		if ((*it)->m_passthrough)
			XInputModuleManager::Get().XInputEnable(enable);
	}

	ControllerManager::Get().XInputEnable(enable);
}

extern "C" DWORD WINAPI XInputGetDSoundAudioDeviceGuids(DWORD dwUserIndex, GUID* pDSoundRenderGuid, GUID* pDSoundCaptureGuid)
{
	ControllerBase* pController;
	if (!pDSoundRenderGuid || !pDSoundCaptureGuid)
		return ERROR_BAD_ARGUMENTS;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetDSoundAudioDeviceGuids(pDSoundRenderGuid, pDSoundCaptureGuid);
}

extern "C" DWORD WINAPI XInputGetBatteryInformation(DWORD dwUserIndex, BYTE devType, XINPUT_BATTERY_INFORMATION* pBatteryInformation)
{
	ControllerBase* pController;
	if (!pBatteryInformation)
		return ERROR_BAD_ARGUMENTS;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetBatteryInformation(devType, pBatteryInformation);
}

extern "C" DWORD WINAPI XInputGetKeystroke(DWORD dwUserIndex, DWORD dwReserved, XINPUT_KEYSTROKE* pKeystroke)
{
	ControllerBase* pController;
	if (!pKeystroke)
		return ERROR_BAD_ARGUMENTS;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetKeystroke(dwReserved, pKeystroke);
}

//undocumented
extern "C" DWORD WINAPI XInputGetStateEx(DWORD dwUserIndex, XINPUT_STATE *pState)
{
	ControllerBase* pController;
	if (!pState)
		return ERROR_BAD_ARGUMENTS;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetStateEx(pState);
}

extern "C" DWORD WINAPI XInputWaitForGuideButton(DWORD dwUserIndex, DWORD dwFlag, LPVOID pVoid)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->WaitForGuideButton(dwFlag, pVoid);
}

extern "C" DWORD WINAPI XInputCancelGuideButtonWait(DWORD dwUserIndex)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->CancelGuideButtonWait();
}

extern "C" DWORD WINAPI XInputPowerOffController(DWORD dwUserIndex)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->PowerOffController();
}

extern "C" DWORD WINAPI XInputGetAudioDeviceIds(DWORD dwUserIndex, LPWSTR pRenderDeviceId, UINT* pRenderCount, LPWSTR pCaptureDeviceId, UINT* pCaptureCount)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetAudioDeviceIds(pRenderDeviceId, pRenderCount, pCaptureDeviceId, pCaptureCount);
}

extern "C" DWORD WINAPI XInputGetBaseBusInformation(DWORD dwUserIndex, struct XINPUT_BUSINFO* pBusinfo)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetBaseBusInformation(pBusinfo);
}

// XInput 1.4 uses this in XInputGetCapabilities and calls memcpy(pCapabilities, &CapabilitiesEx, 20u);
// so XINPUT_CAPABILITIES is first 20 bytes of XINPUT_CAPABILITIESEX
extern "C" DWORD WINAPI XInputGetCapabilitiesEx(DWORD unk1 /*seems that only 1 is valid*/, DWORD dwUserIndex, DWORD dwFlags, struct XINPUT_CAPABILITIESEX* pCapabilitiesEx)
{
	ControllerBase* pController;
	DWORD initFlag = ControllerManager::Get().DeviceInitialize(dwUserIndex, &pController);
	if (initFlag != ERROR_SUCCESS)
		return initFlag;

	return pController->GetCapabilitiesEx(unk1, dwFlags, pCapabilitiesEx);
}
