#include "BlockRawInputHook.h"

namespace Proto
{

UINT WINAPI Hook_GetRawInputDeviceList(PRAWINPUTDEVICELIST pRawInputDeviceList, PUINT puiNumDevices, UINT cbSize)
{
	*puiNumDevices = 0;
	return 0;
}


UINT WINAPI Hook_GetRegisteredRawInputDevices(PRAWINPUTDEVICE pRawInputDevices, PUINT puiNumDevices, UINT cbSize)
{
	*puiNumDevices = 0;
	return 0;
}

void BlockRawInputHook::InstallImpl()
{
	hookInfoGetRawInputDeviceList = std::get<1>(InstallNamedHook(L"user32", "GetRawInputDeviceList", Hook_GetRawInputDeviceList));
	hookInfoGetRegisteredRawInputDevices = std::get<1>(InstallNamedHook(L"user32", "GetRegisteredRawInputDevices", Hook_GetRegisteredRawInputDevices));
}

void BlockRawInputHook::UninstallImpl()
{
	UninstallHook(&hookInfoGetRawInputDeviceList);
	UninstallHook(&hookInfoGetRegisteredRawInputDevices);
}

}