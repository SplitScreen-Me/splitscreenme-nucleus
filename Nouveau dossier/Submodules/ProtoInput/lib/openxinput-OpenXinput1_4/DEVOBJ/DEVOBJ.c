/* Copyright (C) Nemirtingas
 * This file is part of the OpenXinput project.
 *
 * OpenXinput project is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenXinput project is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenXinput project.  If not, see <https://www.gnu.org/licenses/>
 */

#include "devobj.h"

HDEVINFO WINAPI DevObjCreateDeviceInfoList(HANDLE a, LPCWSTR UNCServerName, PVOID c, PVOID d, PVOID e)
{
    return 0xCDCDCDCD;
}

BOOL   WINAPI DevObjGetClassDevs(HANDLE h, const GUID* pGuid, LPCWSTR pDeviceID, DWORD Flags, LPCWSTR str, SIZE_T len)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DevObjDestroyDeviceInfoList(HANDLE h)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DevObjEnumDeviceInterfaces(HANDLE h, PDO_DEVINFO_DATA DeviceInfoData, const GUID* InterfaceClassGuid, DWORD MemberIndex, PDO_DEVICE_INTERFACE_DATA DeviceInterfaceData)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DevObjGetDeviceInterfaceDetail(HANDLE h, PDO_DEVICE_INTERFACE_DATA DeviceInterfaceData, PDO_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, PDWORD RequiredSize, PDO_DEVINFO_DATA DeviceInfoData)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DevObjEnumDeviceInfo(HANDLE DeviceInfoSet, DWORD MemberIndex, PDO_DEVINFO_DATA DeviceInfoData)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DevObjGetDeviceProperty(HANDLE DeviceInfoSet, PDO_DEVINFO_DATA DeviceInfoData, CONST DEVPROPKEY* PropertyKey, DEVPROPTYPE* PropertyType, PBYTE PropertyBuffer, DWORD PropertyBufferSize, PDWORD RequiredSize, DWORD Flags)
{
    return 0xCDCDCDCD;
}

BOOL WINAPI DllMain(HMODULE a, DWORD b, PVOID c)
{
    return 0xCDCDCDCD;
}