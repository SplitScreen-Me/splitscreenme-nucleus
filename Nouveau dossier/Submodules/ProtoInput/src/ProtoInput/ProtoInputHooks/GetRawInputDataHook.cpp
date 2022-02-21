#include <windows.h>
#include "GetRawInputDataHook.h"
#include "RegisterRawInputHook.h"
#include <cassert>
#include "RawInput.h"

namespace Proto
{

UINT WINAPI Hook_GetRawInputData(
	HRAWINPUT hRawInput,
	UINT      uiCommand,
	LPVOID    pData,
	PUINT     pcbSize,
	UINT      cbSizeHeader
)
{
	unsigned int h = (unsigned int)hRawInput; // Only care about first 4 bytes.
	bool hasSignature = (h & 0xFF000000) == 0xAB000000;
	if (!hasSignature)
	{
		// printf("GetRawInputData doesn't have signature: h = 0x%X\n", h);
		return GetRawInputData(hRawInput, uiCommand, pData, pcbSize, cbSizeHeader);
	}
	else
	{
		// printf("$");

		auto index = h & 0x00FFFFFF;

		if (pData == NULL)
		{
			*pcbSize = uiCommand == RID_INPUT ? sizeof(RAWINPUT) : sizeof(RAWINPUTHEADER);
			return 0;
		}
		else
		{
			if (uiCommand == RID_INPUT)
			{
				RAWINPUT* ptr = (RAWINPUT*)pData;
				*ptr = RawInput::inputBuffer[index];
				return *pcbSize;
			}
			else
			{
				RAWINPUTHEADER* ptr = (RAWINPUTHEADER*)pData;
				*ptr = RawInput::inputBuffer[index].header;
				return *pcbSize;
			}
		}
	}
}

// void GetRawInputDataHook::ShowGuiStatus()
// {
// }

void GetRawInputDataHook::InstallImpl()
{
	hookInfo = std::get<1>(InstallNamedHook(L"user32", "GetRawInputData", Hook_GetRawInputData));
}

void GetRawInputDataHook::UninstallImpl()
{
	UninstallHook(&hookInfo);
}

}