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

#include <windows.h>
#include <Setupapi.h>

#pragma pack(push,1)

typedef struct _DO_DEVINFO_DATA {
    DWORD cbSize;
    GUID  InterfaceClassGuid;
    DWORD Flags;
    ULONG_PTR Reserved;
} DO_DEVINFO_DATA, * PDO_DEVINFO_DATA;

typedef struct _DO_DEVICE_INTERFACE_DETAIL_DATA {
    DWORD  cbSize;
    WCHAR  DevicePath[ANYSIZE_ARRAY];
} DO_DEVICE_INTERFACE_DETAIL_DATA, *PDO_DEVICE_INTERFACE_DETAIL_DATA;

typedef struct _DO_DEVICE_INTERFACE_DATA {
    DWORD cbSize;
    GUID  InterfaceClassGuid;
    DWORD Flags;
    ULONG_PTR Reserved;
} DO_DEVICE_INTERFACE_DATA, * PDO_DEVICE_INTERFACE_DATA;

#pragma pack(pop)

#ifdef __cplusplus
extern "C" {
#endif

HANDLE WINAPI DevObjCreateDeviceInfoList(HANDLE a, LPCWSTR UNCServerName, PVOID c, PVOID d, PVOID e);
BOOL   WINAPI DevObjGetClassDevs(HANDLE hDev, const GUID* pGuid, LPCWSTR pDeviceID, DWORD Flags, LPCWSTR str, SIZE_T len);
BOOL   WINAPI DevObjDestroyDeviceInfoList(HANDLE hDev);
BOOL   WINAPI DevObjEnumDeviceInterfaces(HANDLE hDev, PDO_DEVINFO_DATA DeviceInfoData, const GUID* InterfaceClassGuid, DWORD MemberIndex, PDO_DEVICE_INTERFACE_DATA DeviceInterfaceData);
BOOL   WINAPI DevObjGetDeviceInterfaceDetail(HANDLE DeviceInfoSet, PDO_DEVICE_INTERFACE_DATA DeviceInterfaceData, PDO_DEVICE_INTERFACE_DETAIL_DATA DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, PDWORD RequiredSize, PDO_DEVINFO_DATA DeviceInfoData);
BOOL   WINAPI DevObjEnumDeviceInfo(HANDLE DeviceInfoSet, DWORD MemberIndex, PDO_DEVINFO_DATA DeviceInfoData);
BOOL   WINAPI DevObjGetDeviceProperty(HANDLE DeviceInfoSet, PDO_DEVINFO_DATA DeviceInfoData, CONST DEVPROPKEY* PropertyKey, DEVPROPTYPE* PropertyType, PBYTE PropertyBuffer, DWORD PropertyBufferSize, PDWORD RequiredSize, DWORD Flags);

#ifdef __cplusplus
}
#endif