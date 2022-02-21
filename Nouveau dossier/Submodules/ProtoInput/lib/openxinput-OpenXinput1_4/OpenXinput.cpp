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

#include "OpenXinputInternal.h"

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN10)
 // XInputEnable is deprecated since Windows 10, disable the warning if needed to build Xinput.
#pragma warning(disable : 4995)
#endif

/////////////////////////////////////////////////////
// Declarations
/////////////////////////////////////////////////////

struct DeviceInfo_t
{
    DWORD status;
    HANDLE hDevice;
    HANDLE hGuideWait;
    BYTE dwUserIndex;
    LPWSTR lpDevicePath;
    DWORD dwDevicePathSize;
    LPWSTR field_18;
    SIZE_T field_1C;
    WORD wType;
    WORD field_22;
    XINPUT_STATE DeviceState;
    XINPUT_VIBRATION DeviceVibration;
    WORD field_38;
    WORD field_3A;
    WORD field_3C;
    WORD field_3E;
    WORD field_40;
    WORD field_42;
    WORD field_44;
    WORD field_46;
    WORD field_48;
    WORD field_4A;
    WORD field_4C;
    WORD vendorId;
    WORD productId;
    BYTE inputId;
    BYTE bTriggers;
    WORD wButtons;
    WORD LeftStickVirtualKey;
    WORD RightStickVirtualKey;
    WORD field_5A;
    WORD field_5C;
    WORD field_5E;
};
// All theses structs are passed to the XUSB driver, they __NEED__ to be unaligned.
#pragma pack(push, 1)
struct InSetLEDBuffer_t
{
    BYTE deviceIndex;
    BYTE state;
    BYTE pad1;
    BYTE pad2;
    BYTE unk0;
};

struct InVibrationBuffer_t
{
    BYTE deviceIndex;
    BYTE pad1;
    BYTE leftMotorSpeed;
    BYTE rightMotorSpeed;
    BYTE unk0;
};

struct InGetAudioDeviceInformation_t
{
    WORD wType;
    BYTE DeviceIndex;
};

struct OutGetAudioDeviceInformation_t
{
    WORD unk0;
    WORD vendorId;
    WORD productId;
    BYTE inputId;
};

struct InGetLEDBuffer_t
{
    WORD wType;
    BYTE DeviceIndex;
};

struct OutGetLEDBuffer_t
{
    BYTE unk0;
    BYTE unk1;
    BYTE LEDState;
};

struct OutDeviceInfos_t
{
    WORD wType;
    BYTE deviceIndex;
    BYTE unk1;
    WORD unk2;
    WORD unk3;
    WORD vendorId;
    WORD productId;
};

struct InPowerOffBuffer_t
{
    WORD wType;
    BYTE DeviceIndex;
};

struct InWaitForGuideButtonBuffer_t
{
    WORD wType;
    BYTE DeviceIndex;
};

struct OutWaitForGuideButtonBuffer_t
{
    BYTE field_0;
    BYTE field_1;
    BYTE status;
    BYTE field_3;
    BYTE field_4;
    DWORD field_5;
    BYTE field_9;
    BYTE field_A;
    WORD field_B;
    BYTE field_D;
    BYTE field_E;
    WORD field_F;
    WORD field_11;
    WORD field_13;
    WORD field_15;
    BYTE field_17;
    BYTE field_18;
    BYTE field_19;
    BYTE field_1A;
    BYTE field_1B;
    BYTE field_1C;
};

struct InGamepadState0100
{
    BYTE DeviceIndex;
};

struct GamepadState0100
{
    BYTE  status;
    BYTE  unk0;
    BYTE  inputId;
    DWORD dwPacketNumber;
    BYTE  unk2;
    WORD  wButtons;
    BYTE  bLeftTrigger;
    BYTE  bRightTrigger;
    SHORT sThumbLX;
    SHORT sThumbLY;
    SHORT sThumbRX;
    SHORT sThumbRY;
};

struct InGamepadState0101
{
    WORD wType;
    BYTE DeviceIndex;
};

struct GamepadState0101
{
    BYTE  unk0;
    BYTE  inputId;
    BYTE  status;
    BYTE  unk2;
    BYTE  unk3;
    DWORD dwPacketNumber;
    BYTE  unk4;
    BYTE  unk5;
    WORD  wButtons;
    BYTE  bLeftTrigger;
    BYTE  bRightTrigger;
    SHORT sThumbLX;
    SHORT sThumbLY;
    SHORT sThumbRX;
    SHORT sThumbRY;
    BYTE  unk6;
    BYTE  unk7;
    BYTE  unk8;
    BYTE  unk9;
    BYTE  unk10;
    BYTE  unk11;
};

struct InGamepadCapabilities0101
{
    WORD wType;
    BYTE DeviceIndex;
};

struct GamepadCapabilities0101
{
    BYTE  unk0;
    BYTE  unk1;
    BYTE  Type;
    BYTE  SubType;
    WORD  wButtons;
    BYTE  bLeftTrigger;
    BYTE  bRightTrigger;
    SHORT sThumbLX;
    SHORT sThumbLY;
    SHORT sThumbRX;
    SHORT sThumbRY;
    BYTE  unk16;
    BYTE  unk17;
    BYTE  unk18;
    BYTE  unk19;
    BYTE  unk20;
    BYTE  unk21;
    BYTE  bLeftMotorSpeed;
    BYTE  bRightMotorSpeed;
};

struct InGamepadBatteryInformation0102
{
    WORD wType;
    BYTE DeviceIndex;
    BYTE DeviceType;
};

struct GamepadBatteryInformation0102
{
    WORD wType;
    BYTE BatteryType;
    BYTE BatteryLevel;
};

#pragma pack(pop)

struct GetStateApiParam_t
{
    XINPUT_STATE* pState;
};

struct SetStateApiParam_t
{
    XINPUT_VIBRATION* pVibration;
};

struct DSoundCallbackParam_t
{
    PCWSTR DevicePath;
    GUID* HeadphoneGuid;
    GUID* MicrophoneGuid;
};

struct GetAudioDeviceGuidsApiParam_t
{
    GUID* pHeadphoneGuid;
    GUID* pMicrophoneGuid;
};

struct GetBatteryInformationApiParam_t
{
    LPBYTE DeviceType;
    XINPUT_BATTERY_INFORMATION* pBatteryInformation;
};

struct GetCapabilitiesApiParam_t
{
    XINPUT_CAPABILITIES* pCapabilities;
};

struct GetKeystrokeApiParam_t
{
    LPDWORD lpReserved;
    XINPUT_KEYSTROKE* pKeystroke;
    LPDWORD lpLastError;
};

struct WaitGuideButtonApiParam_t
{
    LPHANDLE lphEvent;
    XINPUT_LISTEN_STATE* pListenState;
};

struct WaitForGuideButtonHelperApiParam_t
{
    HANDLE hGuideWait;
    HANDLE hGuideEvent;
    BYTE dwUserIndex;
    XINPUT_LISTEN_STATE* pListenState;
};

constexpr DWORD SET_USER_LED_ON_CREATE = (1 << 0);
constexpr DWORD DISABLE_USER_LED_ON_DESTROY = (1 << 1);
constexpr DWORD DISABLE_VIBRATION_ON_DESTROY = (1 << 2);

/////////////////////////////////////////////////////
// Anonymous namespace
/////////////////////////////////////////////////////
namespace
{

static PVOID g_pDetailBuffer;
static DWORD g_dwDetailBufferSize;

static decltype(DeviceIoControl)* g_pfnDeviceIoControl;
static decltype(SetupDiGetClassDevsW)* g_pfnGetClassDevs;
static decltype(SetupDiEnumDeviceInterfaces)* g_pfnEnumDeviceInterfaces;
static decltype(SetupDiGetDeviceInterfaceDetailW)* g_pfnGetDeviceInterfaceDetail;
static decltype(SetupDiDestroyDeviceInfoList)* g_pfnDestroyDeviceInfoList;

static BOOL g_IsInitialized = FALSE;
static BOOL g_IsCommunicationEnabled = FALSE;

static RTL_CRITICAL_SECTION g_csGlobalLock;
static DWORD g_dwSettings = SET_USER_LED_ON_CREATE | DISABLE_USER_LED_ON_DESTROY | DISABLE_VIBRATION_ON_DESTROY;
static DWORD g_dwDeviceListSize = 0;
static DeviceInfo_t** g_pDeviceList;

static DWORD g_dwLogVerbosity = 0;

DWORD WINAPI XInputReturnCodeFromHRESULT(HRESULT hr);

HRESULT WINAPI GrowList(DWORD newSize);

void WINAPI CopyGamepadStateToDeviceInfo(DeviceInfo_t* pDevice, GamepadState0100* pGamepadState);
void WINAPI CopyGamepadStateToDeviceInfo(DeviceInfo_t* pDevice, GamepadState0101* pGamepadState);
void WINAPI TranslateCapabilities(GamepadCapabilities0101* pGamepadCapabilities, XINPUT_CAPABILITIES* pCapabilities);

HRESULT WINAPI SendReceiveIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpInBuffer, DWORD nInBufferSize, LPVOID lpOutBuffer, DWORD nOutBufferSize, LPOVERLAPPED lpOverlapped);
HRESULT WINAPI SendIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpInBuffer, DWORD nInBufferSize);
HRESULT WINAPI ReceiveIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpOutBuffer, DWORD nOufBufferSize);

HRESULT WINAPI EnumerateXInputDevices();
HRESULT WINAPI EnumerateDevicesOnDeviceInterface(HANDLE hDevice, LPCWSTR lpDevicePath);
HRESULT WINAPI ProcessEnumeratedDevice(DeviceInfo_t* pDevice);

HRESULT WINAPI GetDeviceList(HDEVINFO* phDevInfo);
HRESULT WINAPI GetDeviceInterfaceData(HDEVINFO DeviceInfoSet, DWORD MemberIndex, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
HRESULT WINAPI GetDeviceInterfaceDetail(HDEVINFO hDeviceInfo, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, PSP_DEVICE_INTERFACE_DETAIL_DATA_W* ppDeviceInterfaceDetailData);

DWORD WINAPI WaitForGuideButtonHelper(void* pParam);

BOOL CALLBACK DirectSoundEnumerateCallback(PDSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA lpData, LPVOID lpUser);

}

/////////////////////////////////////////////////////
// Protocol namespace
/////////////////////////////////////////////////////
namespace Protocol
{

DEFINE_HIDDEN_GUID(XUSB_INTERFACE_CLASS_GUID, 0xEC87F1E3, 0xC13B, 0x4100, 0xB5, 0xF7, 0x8B, 0x84, 0xD5, 0x42, 0x60, 0xCB);

constexpr DWORD IOCTL_XINPUT_BASE = 0x8000;

//                                                                16bits         12bits       3bits            3bits
static DWORD IOCTL_XINPUT_GET_INFORMATION = CTL_CODE(IOCTL_XINPUT_BASE, 0x800, METHOD_BUFFERED, FILE_READ_ACCESS);                     // 0x80006000
static DWORD IOCTL_XINPUT_GET_CAPABILITIES = CTL_CODE(IOCTL_XINPUT_BASE, 0x801, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS); // 0x8000E004
static DWORD IOCTL_XINPUT_GET_LED_STATE = CTL_CODE(IOCTL_XINPUT_BASE, 0x802, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS); // 0x8000E008
static DWORD IOCTL_XINPUT_GET_GAMEPAD_STATE = CTL_CODE(IOCTL_XINPUT_BASE, 0x803, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS); // 0x8000E00C
static DWORD IOCTL_XINPUT_SET_GAMEPAD_STATE = CTL_CODE(IOCTL_XINPUT_BASE, 0x804, METHOD_BUFFERED, FILE_WRITE_ACCESS);                    // 0x8000A010
static DWORD IOCTL_XINPUT_WAIT_FOR_GUIDE_BUTTON = CTL_CODE(IOCTL_XINPUT_BASE, 0x805, METHOD_BUFFERED, FILE_WRITE_ACCESS);                    // 0x8000A014
static DWORD IOCTL_XINPUT_GET_BATTERY_INFORMATION = CTL_CODE(IOCTL_XINPUT_BASE, 0x806, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS); // 0x8000E018
static DWORD IOCTL_XINPUT_POWER_DOWN_DEVICE = CTL_CODE(IOCTL_XINPUT_BASE, 0x807, METHOD_BUFFERED, FILE_WRITE_ACCESS);                    // 0x8000A01C
static DWORD IOCTL_XINPUT_GET_AUDIO_INFORMATION = CTL_CODE(IOCTL_XINPUT_BASE, 0x808, METHOD_BUFFERED, FILE_READ_ACCESS | FILE_WRITE_ACCESS); // 0x8000E020

namespace LEDState
{

static BYTE XINPUT_LED_OFF = 0;
static BYTE XINPUT_LED_BLINK = 1;
static BYTE XINPUT_LED_1_SWITCH_BLINK = 2;
static BYTE XINPUT_LED_2_SWITCH_BLINK = 3;
static BYTE XINPUT_LED_3_SWITCH_BLINK = 4;
static BYTE XINPUT_LED_4_SWITCH_BLINK = 5;
static BYTE XINPUT_LED_1 = 6;
static BYTE XINPUT_LED_2 = 7;
static BYTE XINPUT_LED_3 = 8;
static BYTE XINPUT_LED_4 = 9;
static BYTE XINPUT_LED_CYCLE = 10;
static BYTE XINPUT_LED_FAST_BLINK = 11;
static BYTE XINPUT_LED_SLOW_BLINK = 12;
static BYTE XINPUT_LED_FLIPFLOP = 13;
static BYTE XINPUT_LED_ALLBLINK = 14;

static BYTE XINPUT_PORT_TO_LED_MAP[] =
{
    XINPUT_LED_1,
    XINPUT_LED_2,
    XINPUT_LED_3,
    XINPUT_LED_4,
};
#define MAX_XINPUT_PORT_TO_LED_MAP (sizeof(Protocol::LEDState::XINPUT_PORT_TO_LED_MAP)/sizeof(*Protocol::LEDState::XINPUT_PORT_TO_LED_MAP))

static BYTE XINPUT_LED_TO_PORT_MAP[] =
{
    0xFF,//XINPUT_LED_OFF
    0xFF,//XINPUT_LED_BLINK
    0x00,//XINPUT_LED_1_SWITCH_BLINK
    0x01,//XINPUT_LED_2_SWITCH_BLINK
    0x02,//XINPUT_LED_3_SWITCH_BLINK
    0x03,//XINPUT_LED_4_SWITCH_BLINK
    0x00,//XINPUT_LED_1
    0x01,//XINPUT_LED_2
    0x02,//XINPUT_LED_3
    0x03,//XINPUT_LED_4
    0xFF,//XINPUT_LED_CYCLE
    0xFF,//XINPUT_LED_FAST_BLINK
    0xFF,//XINPUT_LED_SLOW_BLINK
    0xFF,//XINPUT_LED_FLIPFLOP
    0xFF,//XINPUT_LED_ALLBLINK
    0x00,//UNUSED
};

}//namespace LEDState

}//namespace Protocol

/////////////////////////////////////////////////////
// DeviceEnum namespace
/////////////////////////////////////////////////////
namespace DeviceEnum
{

HRESULT WINAPI Initialize();
HRESULT WINAPI Close();

HRESULT WINAPI GetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t** ppDevice, bool rescan);

}

/////////////////////////////////////////////////////
// DeviceInfo namespace
/////////////////////////////////////////////////////
namespace DeviceInfo
{

struct XINPUT_AUDIO_INFORMATION
{
    WORD vendorId;
    WORD productId;
    BYTE inputId;
};

namespace Enabled
{

HRESULT WINAPI GetState(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI SetVibration(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);

}

namespace Disabled
{

HRESULT WINAPI GetState(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI SetVibration(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);

}

DeviceInfo_t* WINAPI Create(HANDLE hDevice, LPCWSTR lpDevicePath);
void WINAPI Destroy(DeviceInfo_t* pDevice);
void WINAPI Recycle(DeviceInfo_t* pDevice);

BOOL WINAPI IsDeviceInactive(DeviceInfo_t* pDevice);

HRESULT WINAPI GetKeystroke(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI PowerOffController(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI CancelGuideButtonWait(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI GetAudioDeviceGuids(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI GetBatteryInformation(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI GetCapabilities(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
HRESULT WINAPI WaitForGuideButton(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);

void WINAPI OnEnableSettingChanged(BOOL bEnabled);


static HRESULT(WINAPI* g_pfnGetStateDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
static HRESULT(WINAPI* g_pfnSetVibrationDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved);
static HRESULT(WINAPI* g_pfnGetKeystrokeDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = GetKeystroke;
static HRESULT(WINAPI* g_pfnPowerOffController)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = PowerOffController;
static HRESULT(WINAPI* g_pfnCancelGuideButtonWait)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = CancelGuideButtonWait;
static HRESULT(WINAPI* g_pfnGetAudioDeviceGuidsDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = GetAudioDeviceGuids;
static HRESULT(WINAPI* g_pfnGetBatteryInformationDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = GetBatteryInformation;
static HRESULT(WINAPI* g_pfnGetCapabilitiesDispatcher)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = GetCapabilities;
static HRESULT(WINAPI* g_pfnWaitForGuideButton)(DeviceInfo_t* pDevice, void* pParams, DWORD reserved) = WaitForGuideButton;

}

/////////////////////////////////////////////////////
// DeviceList namespace
/////////////////////////////////////////////////////
namespace DeviceList
{

constexpr size_t InitialDeviceListSize = XUSER_MAX_COUNT * 2;

HRESULT WINAPI Initialize();
HRESULT WINAPI Close();

HRESULT WINAPI SetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t* pDevice);
HRESULT WINAPI GetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t** ppDevice);
HRESULT WINAPI IsDevicePresent(DeviceInfo_t* pDevice);
BOOL WINAPI IsDeviceOnPort(DWORD dwUserIndex);

}

/////////////////////////////////////////////////////
// DriverComm namespace
/////////////////////////////////////////////////////
namespace DriverComm
{

HRESULT WINAPI Initialize();
HRESULT WINAPI Close();
BOOL WINAPI CheckForDriverHook(DWORD driverHook, LPVOID hookFunction);

HRESULT WINAPI SendLEDState(DeviceInfo_t* pDevice, BYTE ledState);
HRESULT WINAPI SendDeviceVibration(DeviceInfo_t* pDevice);
HRESULT WINAPI GetDeviceInfoFromInterface(HANDLE hDevice, OutDeviceInfos_t* pDeviceInfos);
HRESULT WINAPI GetLatestDeviceInfo(DeviceInfo_t* pDevice);
HRESULT WINAPI GetCapabilities(DeviceInfo_t* pDevice, XINPUT_CAPABILITIES* pCapabilities);
HRESULT WINAPI GetBatteryInformation(DeviceInfo_t* pDevice, BYTE DeviceType, XINPUT_BATTERY_INFORMATION* pBatteryInformation);
HRESULT WINAPI GetAudioDeviceInformation(DeviceInfo_t* pDevice, DeviceInfo::XINPUT_AUDIO_INFORMATION* pAudioInformation);
HRESULT WINAPI GetLEDState(DeviceInfo_t* pDevice, BYTE* ledState);
HRESULT WINAPI PowerOffController(DeviceInfo_t* pDevice);
HRESULT WINAPI WaitForGuideButton(HANDLE hDevice, DWORD dwUserIndex, XINPUT_LISTEN_STATE* pListenState);
HRESULT WINAPI CancelGuideButtonWait(DeviceInfo_t* pDevice);

namespace SetupDiWrapper
{

HDEVINFO WINAPI GetClassDevs(const GUID* ClassGuid, PCWSTR Enumerator, HWND hwndParent, DWORD Flags);
BOOL WINAPI EnumDeviceInterfaces(HDEVINFO DeviceInfoSet, PSP_DEVINFO_DATA DeviceInfoData, const GUID* InterfaceClassGuid, DWORD MemberIndex, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData);
BOOL WINAPI GetDeviceInterfaceDetail(HDEVINFO DeviceInfoSet, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, PSP_DEVICE_INTERFACE_DETAIL_DATA_W DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, PDWORD RequiredSize, PSP_DEVINFO_DATA DeviceInfoData);
BOOL WINAPI DestroyDeviceInfoList(HDEVINFO DeviceInfoSet);

}

}

/////////////////////////////////////////////////////
// Utilities namespace
/////////////////////////////////////////////////////
namespace Utilities
{

HRESULT WINAPI Initialize();
HRESULT WINAPI Close();

LPVOID WINAPI MemAlloc(DWORD size);
void WINAPI MemFree(LPVOID lpMem);

HRESULT WINAPI SafeCopyToUntrustedBuffer(void* pDst, const void* pSrc, DWORD size);
HRESULT WINAPI SafeCopyFromUntrustedBuffer(void* pDst, const void* pSrc, DWORD size);

BOOL WINAPI IsSettingSet(DWORD setting);
BOOL WINAPI CheckForSettings(DWORD setting, LPVOID lpParam);

}

/////////////////////////////////////////////////////
// XInputCore namespace
/////////////////////////////////////////////////////
namespace XInputCore
{

BOOL WINAPI Initialize();
BOOL WINAPI Close();

HRESULT WINAPI Enter();
HRESULT WINAPI Leave();
HRESULT WINAPI ProcessAPIRequest(DWORD dwUserIndex, HRESULT(*pfn_ApiAction)(DeviceInfo_t*, void*, DWORD), void* pApiParam, DWORD reserved);

void WINAPI EnableCommunications(BOOL bEnabled);

}

/////////////////////////////////////////////////////
// QuickDriverEnum namespace
/////////////////////////////////////////////////////
class QuickDriverEnum
{
    HDEVINFO hDevInfo;
    DWORD  MemberIndex;
    PSP_DEVICE_INTERFACE_DETAIL_DATA_W DeviceInterfaceDetailData;
    DWORD  DeviceInterfaceDetailDataSize;

public:
    QuickDriverEnum()
    {
        hDevInfo = (HDEVINFO)INVALID_HANDLE_VALUE;
        MemberIndex = 0;
        DeviceInterfaceDetailData = nullptr;
        DeviceInterfaceDetailDataSize = 0;
    }

    ~QuickDriverEnum()
    {
        if (hDevInfo != (HDEVINFO)INVALID_HANDLE_VALUE)
        {
            DriverComm::SetupDiWrapper::DestroyDeviceInfoList(hDevInfo);
            hDevInfo = (HDEVINFO)INVALID_HANDLE_VALUE;
            MemberIndex = 0;
        }
        if (DeviceInterfaceDetailData != nullptr)
        {
            Utilities::MemFree(DeviceInterfaceDetailData);
            DeviceInterfaceDetailData = nullptr;
            DeviceInterfaceDetailDataSize = 0;
        }
    }

    bool Restart()
    {
        if (hDevInfo != (HDEVINFO)INVALID_HANDLE_VALUE)
        {
            DriverComm::SetupDiWrapper::DestroyDeviceInfoList(hDevInfo);
            hDevInfo = (HDEVINFO)INVALID_HANDLE_VALUE;
        }
        MemberIndex = 0;
        if (DeviceInterfaceDetailData == nullptr)
        {
            DeviceInterfaceDetailDataSize = (sizeof(DWORD) + MAX_PATH * sizeof(WCHAR));
            DeviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA_W)Utilities::MemAlloc(DeviceInterfaceDetailDataSize);

            if (DeviceInterfaceDetailData == nullptr)
            {
                DeviceInterfaceDetailDataSize = 0;
                return false;
            }
        }

        hDevInfo = DriverComm::SetupDiWrapper::GetClassDevs(&Protocol::XUSB_INTERFACE_CLASS_GUID, nullptr, nullptr, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
        return hDevInfo != (HDEVINFO)INVALID_HANDLE_VALUE;
    }

    bool GetNext(LPHANDLE phDevice)
    {
        SP_DEVICE_INTERFACE_DATA DeviceInterfaceData;
        DWORD RequiredSize;

        if (phDevice == nullptr)
            return false;

        *phDevice = INVALID_HANDLE_VALUE;
        if (hDevInfo == INVALID_HANDLE_VALUE || DeviceInterfaceDetailData == nullptr)
            return false;

        ZeroMemory(&DeviceInterfaceData, sizeof(SP_DEVICE_INTERFACE_DATA));
        DeviceInterfaceData.cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);

        if (DriverComm::SetupDiWrapper::EnumDeviceInterfaces(hDevInfo, nullptr, &Protocol::XUSB_INTERFACE_CLASS_GUID, MemberIndex, &DeviceInterfaceData) == FALSE)
            return false;

        ++MemberIndex;

        DeviceInterfaceDetailData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_W);
        RequiredSize = 0;
        if (DriverComm::SetupDiWrapper::GetDeviceInterfaceDetail(hDevInfo, &DeviceInterfaceData, DeviceInterfaceDetailData, DeviceInterfaceDetailDataSize, &RequiredSize, nullptr) == TRUE)
        {

            *phDevice = CreateFileW(DeviceInterfaceDetailData->DevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
            return true;
        }

        if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
            return false;

        Utilities::MemFree(DeviceInterfaceDetailData);
        DeviceInterfaceDetailDataSize = RequiredSize;
        DeviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA_W)Utilities::MemAlloc(DeviceInterfaceDetailDataSize);
        if (DeviceInterfaceDetailData == nullptr)
        {
            DeviceInterfaceDetailDataSize = 0;
            return false;
        }

        DeviceInterfaceDetailData->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_W);
        if (DriverComm::SetupDiWrapper::GetDeviceInterfaceDetail(hDevInfo, &DeviceInterfaceData, DeviceInterfaceDetailData, DeviceInterfaceDetailDataSize, nullptr, nullptr) == FALSE)
            return false;

        *phDevice = CreateFileW(DeviceInterfaceDetailData->DevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
        return true;
    }

    bool MinFillFromInterface(HANDLE hDevice, DeviceInfo_t* pDevice)
    {
        OutDeviceInfos_t outBuff;

        if (hDevice == INVALID_HANDLE_VALUE || pDevice == nullptr)
            return false;

        if (DriverComm::GetDeviceInfoFromInterface(hDevice, &outBuff) < 0 || (outBuff.unk2 & 0x80))
            return false;

        ZeroMemory(pDevice, sizeof(DeviceInfo_t));
        pDevice->hDevice = hDevice;
        pDevice->status = DEVICE_STATUS_ACTIVE;
        pDevice->hGuideWait = INVALID_HANDLE_VALUE;
        pDevice->wType = outBuff.wType;
        pDevice->vendorId = outBuff.vendorId;
        pDevice->productId = outBuff.productId;
        return true;
    }
};

/////////////////////////////////////////////////////
// Controller namespace
/////////////////////////////////////////////////////
DWORD WINAPI Controller_GetUserKeystroke(DeviceInfo_t* pDevice, BYTE unk1, DWORD reserved, XINPUT_KEYSTROKE* pKeystroke);
DWORD WINAPI Controller_CalculateKeyFromThumbPos(SHORT base, SHORT X, SHORT Y);

/////////////////////////////////////////////////////
// Implementations
/////////////////////////////////////////////////////

/////////////////////////////////////////////////////
// Anonymous namespace
/////////////////////////////////////////////////////
namespace
{

DWORD WINAPI XInputReturnCodeFromHRESULT(HRESULT hr)
{
    if (hr >= 0)
    {
        if (hr == 1)
            return ERROR_DEVICE_NOT_CONNECTED;

        return ERROR_SUCCESS;
    }

    if (hr == E_OUTOFMEMORY)
        return ERROR_OUTOFMEMORY;

    if (hr == E_INVALIDARG)
        return ERROR_BAD_ARGUMENTS;

    return hr;
}

HRESULT WINAPI GrowList(DWORD newSize)
{
    DeviceInfo_t** ppDevice;

    ppDevice = (DeviceInfo_t**)Utilities::MemAlloc(newSize * sizeof(DeviceInfo_t*));
    if (!ppDevice)
        return E_OUTOFMEMORY;

    CopyMemory(ppDevice, g_pDeviceList, g_dwDeviceListSize * sizeof(DeviceInfo_t*));

    Utilities::MemFree(g_pDeviceList);
    g_pDeviceList = ppDevice;
    g_dwDeviceListSize = newSize;
    return S_OK;
}

void WINAPI CopyGamepadStateToDeviceInfo(DeviceInfo_t* pDevice, GamepadState0100* pGamepadState)
{
    pDevice->DeviceState.dwPacketNumber = pGamepadState->dwPacketNumber;
    pDevice->DeviceState.Gamepad.bLeftTrigger = pGamepadState->bLeftTrigger;
    pDevice->DeviceState.Gamepad.bRightTrigger = pGamepadState->bRightTrigger;
    pDevice->DeviceState.Gamepad.sThumbLX = pGamepadState->sThumbLX;
    pDevice->DeviceState.Gamepad.sThumbLY = pGamepadState->sThumbLY;
    pDevice->DeviceState.Gamepad.sThumbRX = pGamepadState->sThumbRX;
    pDevice->DeviceState.Gamepad.sThumbRY = pGamepadState->sThumbRY;
    pDevice->DeviceState.Gamepad.wButtons = pGamepadState->wButtons;
    if (pGamepadState->status == 1)
        pDevice->status |= DEVICE_STATUS_ACTIVE;
    else
        pDevice->status &= ~DEVICE_STATUS_ACTIVE;

    pDevice->inputId = pGamepadState->inputId;
}

void WINAPI CopyGamepadStateToDeviceInfo(DeviceInfo_t* pDevice, GamepadState0101* pGamepadState)
{
    pDevice->DeviceState.dwPacketNumber = pGamepadState->dwPacketNumber;
    pDevice->DeviceState.Gamepad.bLeftTrigger = pGamepadState->bLeftTrigger;
    pDevice->DeviceState.Gamepad.bRightTrigger = pGamepadState->bRightTrigger;
    pDevice->DeviceState.Gamepad.sThumbLX = pGamepadState->sThumbLX;
    pDevice->DeviceState.Gamepad.sThumbLY = pGamepadState->sThumbLY;
    pDevice->DeviceState.Gamepad.sThumbRX = pGamepadState->sThumbRX;
    pDevice->DeviceState.Gamepad.sThumbRY = pGamepadState->sThumbRY;
    pDevice->DeviceState.Gamepad.wButtons = pGamepadState->wButtons;
    pDevice->DeviceState.Gamepad.wButtons &= XINPUT_BUTTON_MASK;

    if (pGamepadState->status == 1)
        pDevice->status |= DEVICE_STATUS_ACTIVE;
    else
        pDevice->status &= ~DEVICE_STATUS_ACTIVE;

    pDevice->inputId = pGamepadState->inputId;
}

void WINAPI TranslateCapabilities(GamepadCapabilities0101* pGamepadCapabilities, XINPUT_CAPABILITIES* pCapabilities)
{
    pCapabilities->Type = pGamepadCapabilities->Type;
    pCapabilities->SubType = pGamepadCapabilities->SubType;
    pCapabilities->Flags = XINPUT_CAPS_VOICE_SUPPORTED;
    pCapabilities->Gamepad.wButtons = pGamepadCapabilities->wButtons;
    pCapabilities->Gamepad.bLeftTrigger = pGamepadCapabilities->bLeftTrigger;
    pCapabilities->Gamepad.bRightTrigger = pGamepadCapabilities->bRightTrigger;
    pCapabilities->Gamepad.sThumbLX = pGamepadCapabilities->sThumbLX;
    pCapabilities->Gamepad.sThumbLY = pGamepadCapabilities->sThumbLY;
    pCapabilities->Gamepad.sThumbRX = pGamepadCapabilities->sThumbRX;
    pCapabilities->Gamepad.sThumbRY = pGamepadCapabilities->sThumbRY;
    pCapabilities->Vibration.wLeftMotorSpeed = pGamepadCapabilities->bLeftMotorSpeed;
    pCapabilities->Vibration.wRightMotorSpeed = pGamepadCapabilities->bRightMotorSpeed;
    pCapabilities->Gamepad.wButtons &= XINPUT_BUTTON_MASK;
}

HRESULT WINAPI SendReceiveIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpInBuffer, DWORD nInBufferSize, LPVOID lpOutBuffer, DWORD nOutBufferSize, LPOVERLAPPED lpOverlapped)
{
    DWORD BytesReturned = 0;
    BOOL bRes = FALSE;

    if (g_pfnDeviceIoControl)
        bRes = g_pfnDeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, &BytesReturned, lpOverlapped);
    else
        bRes = DeviceIoControl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, lpOutBuffer, nOutBufferSize, &BytesReturned, lpOverlapped);

    if (bRes == TRUE)
        return S_OK;

    if (GetLastError() == ERROR_IO_PENDING && lpOverlapped)
        return E_PENDING;

    return E_FAIL;
}

HRESULT WINAPI SendIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpInBuffer, DWORD nInBufferSize)
{
    return SendReceiveIoctl(hDevice, dwIoControlCode, lpInBuffer, nInBufferSize, nullptr, 0, nullptr);
}

HRESULT WINAPI ReceiveIoctl(HANDLE hDevice, DWORD dwIoControlCode, LPVOID lpOutBuffer, DWORD nOufBufferSize)
{
    return SendReceiveIoctl(hDevice, dwIoControlCode, nullptr, 0, lpOutBuffer, nOufBufferSize, nullptr);
}

HRESULT WINAPI EnumerateXInputDevices()
{
    HRESULT hr;
    HDEVINFO hDeviceInfoSet;
    DWORD dwMemberIndex = 0;
    SP_DEVICE_INTERFACE_DATA DeviceInterfaceData;
    PSP_DEVICE_INTERFACE_DETAIL_DATA_W pDeviceInterfaceDetailData;
    HANDLE hDevice;

    if ((hr = GetDeviceList(&hDeviceInfoSet)) < 0)
        return hr;

    while (GetDeviceInterfaceData(hDeviceInfoSet, dwMemberIndex++, &DeviceInterfaceData) >= 0)
    {
        if ((hr = GetDeviceInterfaceDetail(hDeviceInfoSet, &DeviceInterfaceData, &pDeviceInterfaceDetailData)) < 0)
            continue;

        hDevice = CreateFileW(pDeviceInterfaceDetailData->DevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, 0);
        if (hDevice != INVALID_HANDLE_VALUE)
        {
            hr = EnumerateDevicesOnDeviceInterface(hDevice, pDeviceInterfaceDetailData->DevicePath);
            if (hr < 0)
                hr = S_OK;

            CloseHandle(hDevice);
            hDevice = INVALID_HANDLE_VALUE;
        }
    }

    if (hDeviceInfoSet != (HDEVINFO)INVALID_HANDLE_VALUE)
    {
        DriverComm::SetupDiWrapper::DestroyDeviceInfoList(hDeviceInfoSet);
        hDeviceInfoSet = (HDEVINFO)INVALID_HANDLE_VALUE;
    }

    return hr;
}

HRESULT WINAPI EnumerateDevicesOnDeviceInterface(HANDLE hDevice, LPCWSTR lpDevicePath)
{
    HRESULT hr;
    DeviceInfo_t* pDevice = nullptr;
    OutDeviceInfos_t deviceInfos = {};

    hr = DriverComm::GetDeviceInfoFromInterface(hDevice, &deviceInfos);
    if (hr < 0 || (deviceInfos.unk3 & 0x80))
        return hr;

    for (int i = 0; i < deviceInfos.deviceIndex; ++i)
    {
        if (pDevice != nullptr)
        {
            DeviceInfo::Recycle(pDevice);
        }
        else
        {
            pDevice = DeviceInfo::Create(hDevice, lpDevicePath);
            if (pDevice == nullptr)
                return E_OUTOFMEMORY;
        }

        pDevice->dwUserIndex = i;
        pDevice->productId = deviceInfos.productId;
        pDevice->vendorId = deviceInfos.vendorId;
        pDevice->wType = deviceInfos.wType;
        hr = DriverComm::GetLatestDeviceInfo(pDevice);
        if (hr >= 0)
        {
            hr = ProcessEnumeratedDevice(pDevice);

            if (hr == S_OK)
            {
                pDevice = nullptr;
            }
        }
    }

    if (pDevice)
    {
        DeviceInfo::Destroy(pDevice);
        pDevice = nullptr;
    }

    return hr;
}

HRESULT WINAPI ProcessEnumeratedDevice(DeviceInfo_t* pDevice)
{
    DWORD dwUserIndex = 255;
    HRESULT hr;
    BYTE LEDState;

    if (DeviceInfo::IsDeviceInactive(pDevice) == TRUE)
        return 1;

    hr = DeviceList::IsDevicePresent(pDevice);
    if (hr < 0)
        return hr;

    if (hr == 0)
        return 1;

    LEDState = 0;
    if (DriverComm::GetLEDState(pDevice, &LEDState) >= 0 && LEDState < 15)
    {
        dwUserIndex = Protocol::LEDState::XINPUT_LED_TO_PORT_MAP[LEDState];
        if (dwUserIndex != 255)
        {
            hr = DeviceList::IsDeviceOnPort(dwUserIndex);
            if (hr < 0)
                return hr;

            if (hr != TRUE)
            {
                dwUserIndex = 255;
                DriverComm::SendLEDState(pDevice, Protocol::LEDState::XINPUT_LED_FLIPFLOP);
            }
        }
    }

    for (int i = 0; i < XUSER_MAX_COUNT && dwUserIndex == 255; ++i)
    {
        hr = DeviceList::IsDeviceOnPort(i);
        if (hr < 0)
            return hr;
        if (hr == TRUE)
            dwUserIndex = i;
    }

    if (dwUserIndex >= XUSER_MAX_COUNT)
        return 1;

    hr = DeviceList::SetDeviceOnPort(dwUserIndex, pDevice);
    if (hr < 0)
        return hr;

    if (Utilities::IsSettingSet(SET_USER_LED_ON_CREATE))
        DriverComm::SendLEDState(pDevice, Protocol::LEDState::XINPUT_PORT_TO_LED_MAP[dwUserIndex % MAX_XINPUT_PORT_TO_LED_MAP]);

    return S_OK;
}

HRESULT WINAPI GetDeviceList(HDEVINFO* phDevInfo)
{
    HRESULT hr;

    *phDevInfo = DriverComm::SetupDiWrapper::GetClassDevs(&Protocol::XUSB_INTERFACE_CLASS_GUID, nullptr, nullptr, DIGCF_DEVICEINTERFACE | DIGCF_PRESENT);
    if (*phDevInfo == (HDEVINFO)INVALID_HANDLE_VALUE)
        hr = E_FAIL;
    else
        hr = S_OK;

    return hr;
}

HRESULT WINAPI GetDeviceInterfaceData(HDEVINFO DeviceInfoSet, DWORD MemberIndex, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData)
{
    HRESULT hr;

    ZeroMemory(DeviceInterfaceData, sizeof(SP_DEVICE_INTERFACE_DATA));
    DeviceInterfaceData->cbSize = sizeof(SP_DEVICE_INTERFACE_DATA);
    if (DriverComm::SetupDiWrapper::EnumDeviceInterfaces(DeviceInfoSet, nullptr, &Protocol::XUSB_INTERFACE_CLASS_GUID, MemberIndex, DeviceInterfaceData) == TRUE)
        hr = S_OK;
    else
        hr = E_FAIL;

    return hr;
}

HRESULT WINAPI GetDeviceInterfaceDetail(HDEVINFO hDeviceInfo, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, PSP_DEVICE_INTERFACE_DETAIL_DATA_W* ppDeviceInterfaceDetailData)
{
    DWORD uBytes = g_dwDetailBufferSize;
    *ppDeviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA_W)g_pDetailBuffer;
    if (*ppDeviceInterfaceDetailData)
    {
        ZeroMemory(*ppDeviceInterfaceDetailData, g_dwDetailBufferSize);
        (*ppDeviceInterfaceDetailData)->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_W);
    }
    if (DriverComm::SetupDiWrapper::GetDeviceInterfaceDetail(hDeviceInfo, DeviceInterfaceData, *ppDeviceInterfaceDetailData, g_dwDetailBufferSize, &uBytes, nullptr) == TRUE)
    {
        return S_OK;
    }

    *ppDeviceInterfaceDetailData = nullptr;
    if (GetLastError() != ERROR_INSUFFICIENT_BUFFER)
    {
        return E_FAIL;
    }

    if (g_pDetailBuffer)
    {
        Utilities::MemFree(g_pDetailBuffer);
        g_pDetailBuffer = 0;
        g_dwDetailBufferSize = 0;
    }

    g_pDetailBuffer = Utilities::MemAlloc(uBytes);
    if (g_pDetailBuffer == nullptr)
        return E_OUTOFMEMORY;

    g_dwDetailBufferSize = uBytes;
    *ppDeviceInterfaceDetailData = (PSP_DEVICE_INTERFACE_DETAIL_DATA_W)g_pDetailBuffer;

    (*ppDeviceInterfaceDetailData)->cbSize = sizeof(SP_DEVICE_INTERFACE_DETAIL_DATA_W);
    if (DriverComm::SetupDiWrapper::GetDeviceInterfaceDetail(hDeviceInfo, DeviceInterfaceData, *ppDeviceInterfaceDetailData, g_dwDetailBufferSize, nullptr, nullptr) == FALSE)
    {
        return E_FAIL;
    }

    return S_OK;
}

DWORD WINAPI WaitForGuideButtonHelper(void* pParam)
{
    WaitForGuideButtonHelperApiParam_t* pApiParam = (WaitForGuideButtonHelperApiParam_t*)pParam;

    if (DriverComm::WaitForGuideButton(pApiParam->hGuideWait, pApiParam->dwUserIndex, pApiParam->pListenState) >= 0
        && pApiParam->hGuideEvent)
    {
        SetEvent(pApiParam->hGuideEvent);
    }

    delete pApiParam;
    return 0;
}

BOOL CALLBACK DirectSoundEnumerateCallback(PDSPROPERTY_DIRECTSOUNDDEVICE_DESCRIPTION_W_DATA lpData, LPVOID lpUser)
{
    DSoundCallbackParam_t* pParam = (DSoundCallbackParam_t*)lpUser;
    size_t devicePathLen;

    if (lpUser == nullptr)
        return FALSE; // Stop enumeration.

    if (lpData->Type != DIRECTSOUNDDEVICE_TYPE_WDM)
        return TRUE; // Skip this device and continue enumeration.

    devicePathLen = wcslen(pParam->DevicePath);
    if (wcslen(lpData->Interface) < devicePathLen)
        return TRUE; // Skip this device and continue enumeration.

    if (CopyMemory(lpData->Interface, pParam->DevicePath, devicePathLen * sizeof(WCHAR)) != 0)
        return TRUE; // Skip this device and continue enumeration.

    if (lpData->DataFlow == DIRECTSOUNDDEVICE_DATAFLOW_RENDER)
    {
        *pParam->HeadphoneGuid = lpData->DeviceId;
    }
    else if (lpData->DataFlow == DIRECTSOUNDDEVICE_DATAFLOW_CAPTURE)
    {
        *pParam->MicrophoneGuid = lpData->DeviceId;
    }

    return TRUE; // Continue enumeration.
}

class QuickModule
{
    HMODULE hModule;

public:
    QuickModule(LPCWSTR lpLibFileName)
    {
        hModule = LoadLibraryW(lpLibFileName);
    }

    ~QuickModule()
    {
        if (hModule != nullptr)
        {
            FreeLibrary(hModule);
            hModule = nullptr;
        }
    }

    bool IsValid()
    {
        return hModule != nullptr;
    }

    void* GetFunctionPointer(LPCSTR lpProcName)
    {
        return GetProcAddress(hModule, lpProcName);
    }
};

}

/////////////////////////////////////////////////////
// DeviceEnum namespace
/////////////////////////////////////////////////////
namespace DeviceEnum
{

HRESULT WINAPI Initialize()
{
    g_pDetailBuffer = nullptr;
    g_dwDetailBufferSize = 0;
    return S_OK;
}

HRESULT WINAPI Close()
{
    if (g_pDetailBuffer)
    {
        ((HLOCAL)g_pDetailBuffer);
        g_pDetailBuffer = nullptr;
    }

    g_dwDetailBufferSize = 0;
    return S_OK;
}

HRESULT WINAPI GetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t** ppDevice, bool rescan)
{
    HRESULT hr;

    hr = DeviceList::GetDeviceOnPort(dwUserIndex, ppDevice);
    while (*ppDevice == nullptr && rescan)
    {
        rescan = false;
        if ((hr = EnumerateXInputDevices()) < 0)
            break;

        if ((hr = DeviceList::GetDeviceOnPort(dwUserIndex, ppDevice)) < 0)
            break;
    }

    return hr;
}

}

/////////////////////////////////////////////////////
// DeviceInfo namespace
/////////////////////////////////////////////////////
namespace DeviceInfo
{

namespace Enabled
{

HRESULT WINAPI GetState(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    HRESULT hr;
    GetStateApiParam_t* pApiParam = (GetStateApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    if ((hr = DriverComm::GetLatestDeviceInfo(pDevice)) < 0)
        return hr;

    return Utilities::SafeCopyToUntrustedBuffer(pApiParam->pState, &pDevice->DeviceState, sizeof(XINPUT_STATE));
}

HRESULT WINAPI SetVibration(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    HRESULT hr;
    SetStateApiParam_t* pApiParam = (SetStateApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    if (pApiParam->pVibration != nullptr)
    {
        if ((hr = Utilities::SafeCopyFromUntrustedBuffer(&pDevice->DeviceVibration, pApiParam->pVibration, sizeof(XINPUT_VIBRATION))) < 0)
            return hr;
    }
    return DriverComm::SendDeviceVibration(pDevice);
}

}

namespace Disabled
{

HRESULT WINAPI GetState(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    static XINPUT_STATE DisabledXINPUT_STATE = {};
    GetStateApiParam_t* pApiParam = (GetStateApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    if (Utilities::SafeCopyToUntrustedBuffer(pApiParam->pState, &DisabledXINPUT_STATE, sizeof(XINPUT_STATE)) >= 0)
        pApiParam->pState->dwPacketNumber = pDevice->DeviceState.dwPacketNumber + 1;

    return S_OK;
}

HRESULT WINAPI SetVibration(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    DeviceInfo_t tmpDevice;
    SetStateApiParam_t* pApiParam = (SetStateApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    if (pApiParam->pVibration != nullptr)
    {
        return Utilities::SafeCopyToUntrustedBuffer(&pDevice->DeviceVibration, pApiParam->pVibration, sizeof(XINPUT_VIBRATION));
    }
    CopyMemory(&tmpDevice, pDevice, sizeof(DeviceInfo_t));
    tmpDevice.DeviceVibration.wLeftMotorSpeed = 0;
    tmpDevice.DeviceVibration.wRightMotorSpeed = 0;
    return DriverComm::SendDeviceVibration(&tmpDevice);
}

}

DeviceInfo_t* WINAPI Create(HANDLE hDevice, LPCWSTR lpDevicePath)
{
    HANDLE hSourceProcessHandle;
    DeviceInfo_t* pDevice;

    pDevice = (DeviceInfo_t*)Utilities::MemAlloc(sizeof(DeviceInfo_t));
    if (pDevice)
    {
        // Shouldn't be needed
        ZeroMemory(pDevice, sizeof(DeviceInfo_t));
        pDevice->hDevice = INVALID_HANDLE_VALUE;
        pDevice->hGuideWait = INVALID_HANDLE_VALUE;
        hSourceProcessHandle = GetCurrentProcess();
        if (DuplicateHandle(hSourceProcessHandle, hDevice, hSourceProcessHandle, &pDevice->hDevice, 0, FALSE, DUPLICATE_SAME_ACCESS))
        {
            pDevice->dwDevicePathSize = wcslen(lpDevicePath) + 1;
            pDevice->lpDevicePath = (LPWSTR)Utilities::MemAlloc(pDevice->dwDevicePathSize * sizeof(WCHAR));
            if (pDevice->lpDevicePath)
            {
                if (StringCchCopyW(pDevice->lpDevicePath, pDevice->dwDevicePathSize, lpDevicePath) >= 0)
                    return pDevice;
            }
        }
    }

    if (pDevice)
    {
        if (pDevice->lpDevicePath)
            Utilities::MemFree(pDevice->lpDevicePath);

        if (pDevice->hDevice != INVALID_HANDLE_VALUE)
            CloseHandle(pDevice->hDevice);

        Utilities::MemFree(pDevice);
    }

    return nullptr;
}

void WINAPI Destroy(DeviceInfo_t* pDevice)
{
    if (pDevice == nullptr)
        return;

    if (pDevice->hGuideWait != INVALID_HANDLE_VALUE)
        CloseHandle(pDevice->hGuideWait);

    if (pDevice->hDevice != INVALID_HANDLE_VALUE)
        CloseHandle(pDevice->hDevice);

    if (pDevice->lpDevicePath)
        Utilities::MemFree(pDevice->lpDevicePath);

    Utilities::MemFree(pDevice);
}

void WINAPI Recycle(DeviceInfo_t* pDevice)
{
    if (pDevice == nullptr)
        return;

    HANDLE hDevice = pDevice->hDevice;
    LPWSTR lpDevicePath = pDevice->lpDevicePath;
    ZeroMemory(pDevice, sizeof(DeviceInfo_t));
    pDevice->hDevice = hDevice;
    pDevice->hGuideWait = INVALID_HANDLE_VALUE;
    pDevice->lpDevicePath = lpDevicePath;
}

BOOL WINAPI IsDeviceInactive(DeviceInfo_t* pDevice)
{
    return (pDevice->status & DEVICE_STATUS_ACTIVE) == 0;
}

HRESULT WINAPI GetKeystroke(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    GetKeystrokeApiParam_t* pApiParam = (GetKeystrokeApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    *pApiParam->lpLastError = Controller_GetUserKeystroke(pDevice, 0, *pApiParam->lpReserved, pApiParam->pKeystroke);

    return S_OK;
}

HRESULT WINAPI PowerOffController(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    return DriverComm::PowerOffController(pDevice);
}

HRESULT WINAPI CancelGuideButtonWait(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    return DriverComm::CancelGuideButtonWait(pDevice);
}

HRESULT WINAPI GetAudioDeviceGuids(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    GetAudioDeviceGuidsApiParam_t* pApiParam = (GetAudioDeviceGuidsApiParam_t*)pParams;
    HRESULT hr;
    DWORD vendorId;
    DWORD productId;
    DWORD inputId;

    IClassFactory* pClassFactory;
    IKsPropertySet* pIKsPropertySet;

    WCHAR devicePath[64];

    DSoundCallbackParam_t CallbackData;
    DSPROPERTY_DIRECTSOUNDDEVICE_ENUMERATE_W_DATA EnumerateData;

    XINPUT_AUDIO_INFORMATION AudioInformation;

    ZeroMemory(pApiParam->pHeadphoneGuid, sizeof(GUID));
    ZeroMemory(pApiParam->pMicrophoneGuid, sizeof(GUID));

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    vendorId = pDevice->vendorId;
    productId = pDevice->productId;
    inputId = pDevice->inputId;
    if (pDevice->wType >= 0x0102)
    {
        if (DriverComm::GetAudioDeviceInformation(pDevice, &AudioInformation) < 0)
            return E_FAIL;

        vendorId = AudioInformation.vendorId;
        productId = AudioInformation.productId;
        inputId = AudioInformation.inputId;

        if (pDevice->inputId == 0xFF)
            return E_FAIL;
    }
    else
    {
        if ((hr = DriverComm::GetLatestDeviceInfo(pDevice)) < 0)
            return hr;

        if (IsDeviceInactive(pDevice))
            return E_FAIL;

        inputId = pDevice->inputId;
        if (pDevice->inputId == 0xFF)
            return S_OK;
    }

    QuickModule quickModule(L"dsound.dll");
    if (!quickModule.IsValid())
        return E_FAIL;

    DllGetClassObject_t* pfn_DllGetClassObject = (DllGetClassObject_t*)quickModule.GetFunctionPointer("DllGetClassObject");
    if (pfn_DllGetClassObject == nullptr)
        return E_FAIL;

    if (StringCchPrintfW(devicePath, sizeof(devicePath) / sizeof(*devicePath), L"\\\\?\\USB#VID_%04X&PID_%04X&IA_%02X", vendorId, productId, inputId) < 0)
        return E_FAIL;

    if ((hr = pfn_DllGetClassObject(&XINPUT_CLSID_DirectSoundPrivate, &XINPUT_IID_IClassFactory, (LPVOID*)&pClassFactory)) < 0)
        return hr;

    pIKsPropertySet = nullptr;
    hr = pClassFactory->CreateInstance(pClassFactory, XINPUT_IID_IKsPropertySet, (LPVOID*)&pIKsPropertySet);
    pClassFactory->Release();
    pClassFactory = nullptr;

    if (hr < 0)
        return hr;

    CallbackData.DevicePath = devicePath;
    CallbackData.HeadphoneGuid = pApiParam->pHeadphoneGuid;
    CallbackData.MicrophoneGuid = pApiParam->pMicrophoneGuid;

    EnumerateData.Callback = &DirectSoundEnumerateCallback;
    EnumerateData.Context = &CallbackData;
    hr = pIKsPropertySet->Get(XINPUT_DSPROPSETID_DirectSoundDevice, 8, nullptr, 0, (LPVOID)&EnumerateData, sizeof(DSPROPERTY_DIRECTSOUNDDEVICE_ENUMERATE_W_DATA), nullptr);

    pIKsPropertySet->Release();
    pIKsPropertySet = nullptr;

    return hr;
}

HRESULT WINAPI GetBatteryInformation(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    GetBatteryInformationApiParam_t* pApiParam = (GetBatteryInformationApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    return DriverComm::GetBatteryInformation(pDevice, *pApiParam->DeviceType, pApiParam->pBatteryInformation);
}

HRESULT WINAPI GetCapabilities(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    GetCapabilitiesApiParam_t* pApiParam = (GetCapabilitiesApiParam_t*)pParams;

    if (IsDeviceInactive(pDevice))
        return E_FAIL;

    return DriverComm::GetCapabilities(pDevice, pApiParam->pCapabilities);
}

HRESULT WINAPI WaitForGuideButton(DeviceInfo_t* pDevice, void* pParams, DWORD reserved)
{
    HRESULT hr;
    WaitGuideButtonApiParam_t* pApiParam = (WaitGuideButtonApiParam_t*)pParams;
    WaitForGuideButtonHelperApiParam_t* pThreadParameter;

    if (IsDeviceInactive(pDevice))
    {
        pApiParam->pListenState->Status = ERROR_DEVICE_NOT_CONNECTED;
        return E_FAIL;
    }

    if (pDevice->hGuideWait == INVALID_HANDLE_VALUE &&
        (pDevice->hGuideWait = CreateFileW(pDevice->lpDevicePath, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE, nullptr, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED, 0)) == INVALID_HANDLE_VALUE)
    {
        return E_FAIL;
    }

    pThreadParameter = new WaitForGuideButtonHelperApiParam_t;
    if (pThreadParameter == nullptr)
    {
        pApiParam->pListenState->Status = ERROR_OUTOFMEMORY;
        return E_OUTOFMEMORY;
    }

    pThreadParameter->hGuideWait = pDevice->hGuideWait;
    pThreadParameter->hGuideEvent = *pApiParam->lphEvent;
    pThreadParameter->dwUserIndex = pDevice->dwUserIndex;
    pThreadParameter->pListenState = pApiParam->pListenState;

    if (*pApiParam->lphEvent != nullptr)
    {// Async wait
        if (CreateThread(nullptr, 0, WaitForGuideButtonHelper, pThreadParameter, 0, nullptr) == nullptr)
        {
            delete pThreadParameter;
            pApiParam->pListenState->Status = GetLastError();
            hr = E_FAIL;
        }
        else
        {
            hr = S_OK;
        }
    }
    else
    {// Sync wait, free the library lock
        hr = XInputCore::Leave();
        if (hr < 0)
        {
            delete pThreadParameter;
        }
        else
        {
            WaitForGuideButtonHelper(pThreadParameter);
            hr = 1;
        }
    }

    return hr;
}

void WINAPI OnEnableSettingChanged(BOOL bEnabled)
{
    if (bEnabled)
    {
        g_pfnGetStateDispatcher = Enabled::GetState;
        g_pfnSetVibrationDispatcher = Enabled::SetVibration;
    }
    else
    {
        g_pfnGetStateDispatcher = Disabled::GetState;
        g_pfnSetVibrationDispatcher = Disabled::SetVibration;
    }
}

}

/////////////////////////////////////////////////////
// DeviceList namespace
/////////////////////////////////////////////////////
namespace DeviceList
{

HRESULT WINAPI Initialize()
{
    g_dwDeviceListSize = 0;
    g_pDeviceList = (DeviceInfo_t**)Utilities::MemAlloc(InitialDeviceListSize * sizeof(DeviceInfo_t*));
    if (!g_pDeviceList)
        return E_FAIL;

    g_dwDeviceListSize = InitialDeviceListSize;
    return S_OK;
}

HRESULT WINAPI Close()
{
    if (g_dwDeviceListSize == 0 || g_pDeviceList == nullptr)
        return E_FAIL;

    for (DWORD i = 0; i < g_dwDeviceListSize; ++i)
    {
        DeviceInfo_t** ppDevice = &g_pDeviceList[i];
        if (*ppDevice == nullptr)
            continue;

        if (Utilities::IsSettingSet(DISABLE_USER_LED_ON_DESTROY) &&
            DriverComm::SendLEDState(*ppDevice, Protocol::LEDState::XINPUT_LED_OFF) < 0 &&
            // && dword_410C24 & 4 &&
            g_dwLogVerbosity >= 2
            )
        {
            // Log
        }
        if (Utilities::IsSettingSet(DISABLE_VIBRATION_ON_DESTROY))
        {
            (*ppDevice)->DeviceVibration = {};
            if (DriverComm::SendDeviceVibration(*ppDevice) < 0 &&
                //dword_410C24 & 4 &&
                g_dwLogVerbosity >= 2
                )
            {
                // Log
            }
        }

        DeviceInfo::Destroy(*ppDevice);
        *ppDevice = nullptr;
    }

    Utilities::MemFree(g_pDeviceList);
    g_pDeviceList = nullptr;
    g_dwDeviceListSize = 0;
    return S_OK;
}

HRESULT WINAPI SetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t* pDevice)
{
    HRESULT hr;

    if (dwUserIndex >= g_dwDeviceListSize)
    {
        hr = GrowList(dwUserIndex + 1);
        if (hr < 0)
            return hr;
    }
    if (pDevice && g_pDeviceList[dwUserIndex])
        return E_FAIL;

    g_pDeviceList[dwUserIndex] = pDevice;
    return S_OK;
}

HRESULT WINAPI RemoveDeviceFromPort(DWORD dwUserIndex)
{
    if (dwUserIndex >= g_dwDeviceListSize)
        return S_OK;

    if (g_pDeviceList[dwUserIndex] != nullptr)
    {
        DeviceInfo::Destroy(g_pDeviceList[dwUserIndex]);
        g_pDeviceList[dwUserIndex] = nullptr;
    }

    return S_OK;
}

HRESULT WINAPI GetDeviceOnPort(DWORD dwUserIndex, DeviceInfo_t** ppDevice)
{
    *ppDevice = (dwUserIndex < g_dwDeviceListSize ? g_pDeviceList[dwUserIndex] : nullptr);
    return S_OK;
}

HRESULT WINAPI IsDevicePresent(DeviceInfo_t* pDevice)
{
    for (DWORD i = 0; i < g_dwDeviceListSize; ++i)
    {
        if (g_pDeviceList[i] != nullptr &&
            pDevice->dwUserIndex == g_pDeviceList[i]->dwUserIndex &&
            g_pDeviceList[i]->lpDevicePath != nullptr &&
            pDevice->lpDevicePath != nullptr &&
            wcscmp(g_pDeviceList[i]->lpDevicePath, pDevice->lpDevicePath) == 0)
        {
            return 0;
        }
    }

    return 1;
}

BOOL WINAPI IsDeviceOnPort(DWORD dwUserIndex)
{
    if (dwUserIndex < g_dwDeviceListSize)
        return g_pDeviceList[dwUserIndex] == nullptr;

    return TRUE;
}

}

/////////////////////////////////////////////////////
// DriverComm namespace
/////////////////////////////////////////////////////
namespace DriverComm
{

HRESULT WINAPI Initialize()
{
    g_pfnDeviceIoControl = nullptr;
    g_pfnGetClassDevs = nullptr;
    g_pfnEnumDeviceInterfaces = nullptr;
    g_pfnGetDeviceInterfaceDetail = nullptr;
    g_pfnDestroyDeviceInfoList = nullptr;
    return S_OK;
}

HRESULT WINAPI Close()
{
    return Initialize();
}

BOOL WINAPI CheckForDriverHook(DWORD driverHook, LPVOID hookFunction)
{
    switch (driverHook)
    {
    case 0xBAAD0000:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        break;

    case 0xBAAD0001:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnDeviceIoControl = (decltype(g_pfnDeviceIoControl))hookFunction;
        break;

    case 0xBAAD0002:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnDeviceIoControl = nullptr;
        break;

    case 0xBAAD0003:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnGetClassDevs = (decltype(g_pfnGetClassDevs))hookFunction;
        break;

    case 0xBAAD0004:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnGetClassDevs = nullptr;
        break;

    case 0xBAAD0005:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnEnumDeviceInterfaces = (decltype(g_pfnEnumDeviceInterfaces))hookFunction;
        break;

    case 0xBAAD0006:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnEnumDeviceInterfaces = nullptr;
        break;

    case 0xBAAD0007:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnGetDeviceInterfaceDetail = (decltype(g_pfnGetDeviceInterfaceDetail))hookFunction;
        break;

    case 0xBAAD0008:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnGetDeviceInterfaceDetail = nullptr;
        break;

    case 0xBAAD0009:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnDestroyDeviceInfoList = (decltype(g_pfnDestroyDeviceInfoList))hookFunction;
        break;

    case 0xBAAD000A:
        //if(dword_410C24 & 8 && g_dwLogVerbosity >= 4)
            // Log
        g_pfnDestroyDeviceInfoList = nullptr;
        break;

    }

    return TRUE;
}

HRESULT WINAPI SendLEDState(DeviceInfo_t* pDevice, BYTE ledState)
{
    InSetLEDBuffer_t inBuffer = {
        pDevice->dwUserIndex,
        ledState,
        0,
        0,
        1
    };

    return SendIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_SET_GAMEPAD_STATE, &inBuffer, sizeof(InSetLEDBuffer_t));
}

HRESULT WINAPI SendDeviceVibration(DeviceInfo_t* pDevice)
{
    InVibrationBuffer_t inBuffer = {
        pDevice->dwUserIndex,
        0,
        (BYTE)(pDevice->DeviceVibration.wLeftMotorSpeed / 256),
        (BYTE)(pDevice->DeviceVibration.wRightMotorSpeed / 256),
        2
    };

    return SendIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_SET_GAMEPAD_STATE, &inBuffer, sizeof(InVibrationBuffer_t));
}

HRESULT WINAPI GetDeviceInfoFromInterface(HANDLE hDevice, OutDeviceInfos_t* pDeviceInfos)
{
    HRESULT hr;

    hr = ReceiveIoctl(hDevice, Protocol::IOCTL_XINPUT_GET_INFORMATION, pDeviceInfos, sizeof(OutDeviceInfos_t));
    if (hr >= 0)
        hr = S_OK;

    return hr;
}

HRESULT WINAPI GetLatestDeviceInfo(DeviceInfo_t* pDevice)
{
    HRESULT hr;

    union
    {
        InGamepadState0100 in0100;
        InGamepadState0101 in0101;
    } inBuffer = {};

    DWORD inSize;

    union
    {
        GamepadState0100 out0100;
        GamepadState0101 out0101;
    } outBuffer = {};

    DWORD outSize;

    if (pDevice->wType == 0x0100)
    {
        inBuffer.in0100.DeviceIndex = pDevice->dwUserIndex;
        inSize = sizeof(inBuffer.in0100);

        outSize = sizeof(outBuffer.out0100);
    }
    else
    {
        inBuffer.in0101.wType = 0x0101;
        inBuffer.in0101.DeviceIndex = pDevice->dwUserIndex;
        inSize = sizeof(inBuffer.in0101);

        outSize = sizeof(outBuffer.out0101);
    }

    hr = SendReceiveIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_GET_GAMEPAD_STATE, &inBuffer, inSize, &outBuffer, outSize, nullptr);
    if (hr < 0)
        return hr;

    if (pDevice->wType == 0x0100)
    {
        CopyGamepadStateToDeviceInfo(pDevice, &outBuffer.out0100);
    }
    else
    {
        CopyGamepadStateToDeviceInfo(pDevice, &outBuffer.out0101);
    }

    return S_OK;
}

HRESULT WINAPI GetCapabilities(DeviceInfo_t* pDevice, XINPUT_CAPABILITIES* pCapabilities)
{
    static XINPUT_CAPABILITIES s_GamepadCapabilities = {
        XINPUT_DEVTYPE_GAMEPAD            , // Type
        XINPUT_DEVSUBTYPE_GAMEPAD         , // SubType
        XINPUT_CAPS_VOICE_SUPPORTED       , // Flags
        {// Gamepad
          XINPUT_BUTTON_MASK_WITHOUT_GUIDE, // Gamepad.wButtons
          1                               , // Gamepad.bLeftTrigger
          1                               , // Gamepad.bRightTrigger
          1                               , // Gamepad.sThumbLX
          1                               , // Gamepad.sThumbLY
          1                               , // Gamepad.sThumbRX
          1                               , // Gamepad.sThumbRY
        },
        {// Vibration
          1                               , // Vibration.wLeftMotorSpeed
          1                               , // Vibration.wRightMotorSpeed
        }
    };
    HRESULT hr;
    InGamepadCapabilities0101 InBuffer;
    GamepadCapabilities0101 OutBuffer;

    if (pDevice->wType == 0x0100)
        return Utilities::SafeCopyToUntrustedBuffer(pCapabilities, &s_GamepadCapabilities, sizeof(XINPUT_CAPABILITIES));

    InBuffer.wType = 0x0101;
    InBuffer.DeviceIndex = pDevice->dwUserIndex;
    ZeroMemory(&OutBuffer, sizeof(GamepadCapabilities0101));

    hr = SendReceiveIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_GET_CAPABILITIES, &InBuffer, sizeof(InGamepadCapabilities0101), &OutBuffer, sizeof(GamepadCapabilities0101), nullptr);
    if (hr >= 0)
        TranslateCapabilities(&OutBuffer, pCapabilities);

    return hr;
}

HRESULT WINAPI GetBatteryInformation(DeviceInfo_t* pDevice, BYTE DeviceType, XINPUT_BATTERY_INFORMATION* pBatteryInformation)
{
    static const GUID null_guid = {};

    HRESULT hr;
    InGamepadBatteryInformation0102 InBuffer;
    GamepadBatteryInformation0102 OutBuffer;
    XINPUT_BATTERY_INFORMATION BatteryInformation;
    GetAudioDeviceGuidsApiParam_t apiParam;
    GUID HeadphoneGuid;
    GUID MicrophoneGuid;

    if (pDevice->wType >= 0x102u)
    {
        InBuffer.wType = 0x102;
        InBuffer.DeviceIndex = pDevice->dwUserIndex;
        InBuffer.DeviceType = DeviceType;

        ZeroMemory(&OutBuffer, sizeof(GamepadBatteryInformation0102));

        hr = SendReceiveIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_GET_BATTERY_INFORMATION, &InBuffer, sizeof(InGamepadBatteryInformation0102), &OutBuffer, sizeof(GamepadBatteryInformation0102), nullptr);
        if (hr >= 0)
        {
            hr = Utilities::SafeCopyToUntrustedBuffer(pBatteryInformation, &OutBuffer.BatteryType, sizeof(XINPUT_BATTERY_INFORMATION));
        }
    }
    else
    {
        BatteryInformation.BatteryType = BATTERY_TYPE_WIRED;
        BatteryInformation.BatteryLevel = BATTERY_LEVEL_FULL;

        if (DeviceType == XINPUT_DEVTYPE_GAMEPAD)
        {
            apiParam.pHeadphoneGuid = &HeadphoneGuid;
            apiParam.pMicrophoneGuid = &MicrophoneGuid;
            if (DeviceInfo::g_pfnGetAudioDeviceGuidsDispatcher(pDevice, &apiParam, 2) < 0 || IsEqualGUID(HeadphoneGuid, null_guid) != 0)
            {
                BatteryInformation.BatteryType = BATTERY_TYPE_DISCONNECTED;
                BatteryInformation.BatteryLevel = BATTERY_LEVEL_EMPTY;
            }
        }
        hr = Utilities::SafeCopyToUntrustedBuffer(pBatteryInformation, &BatteryInformation, sizeof(XINPUT_BATTERY_INFORMATION));
    }
    return hr;
}

HRESULT WINAPI GetAudioDeviceInformation(DeviceInfo_t* pDevice, DeviceInfo::XINPUT_AUDIO_INFORMATION* pAudioInformation)
{
    HRESULT hr;
    InGetAudioDeviceInformation_t InBuffer;
    OutGetAudioDeviceInformation_t OutBuffer;

    if (pDevice->wType < 0x0102)
        return E_FAIL;

    InBuffer.wType = 0x102;
    InBuffer.DeviceIndex = pDevice->dwUserIndex;
    hr = SendReceiveIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_GET_AUDIO_INFORMATION, &InBuffer, sizeof(InGetAudioDeviceInformation_t), &OutBuffer, sizeof(OutGetAudioDeviceInformation_t), nullptr);
    if (hr >= 0)
    {
        pAudioInformation->vendorId = OutBuffer.vendorId;
        pAudioInformation->productId = OutBuffer.productId;
        pAudioInformation->inputId = OutBuffer.inputId;
    }
    return hr;
}

HRESULT WINAPI GetLEDState(DeviceInfo_t* pDevice, BYTE* ledState)
{
    InGetLEDBuffer_t inBuffer;
    OutGetLEDBuffer_t outBuffer;
    HRESULT hr;

    *ledState = Protocol::LEDState::XINPUT_LED_OFF;
    if (pDevice->wType < 0x0101)
        return S_OK;

    inBuffer.wType = 0x0101;
    inBuffer.DeviceIndex = pDevice->dwUserIndex;
    hr = SendReceiveIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_GET_LED_STATE, &inBuffer, sizeof(InGetLEDBuffer_t), &outBuffer, sizeof(OutGetLEDBuffer_t), nullptr);
    if (hr >= 0)
    {
        *ledState = outBuffer.LEDState;
    }

    return hr;
}

HRESULT WINAPI PowerOffController(DeviceInfo_t* pDevice)
{
    InPowerOffBuffer_t inBuff;

    if (pDevice->wType < 0x0102)
        return E_FAIL;

    inBuff.wType = 0x0102;
    inBuff.DeviceIndex = pDevice->dwUserIndex;
    return SendIoctl(pDevice->hDevice, Protocol::IOCTL_XINPUT_POWER_DOWN_DEVICE, &inBuff, sizeof(InPowerOffBuffer_t));
}

HRESULT WINAPI WaitForGuideButton(HANDLE hDevice, DWORD dwUserIndex, XINPUT_LISTEN_STATE* pListenState)
{
    HRESULT hr;
    HANDLE hEvent;
    OVERLAPPED overlapped;
    DWORD receivedBytes;
    InWaitForGuideButtonBuffer_t inBuffer;
    OutWaitForGuideButtonBuffer_t outBuffer;

    hEvent = CreateEvent(nullptr, TRUE, FALSE, nullptr);
    if (hEvent == nullptr)
        return E_OUTOFMEMORY;

    overlapped.Internal = 0;
    overlapped.InternalHigh = 0;
    overlapped.Offset = 0;
    overlapped.OffsetHigh = 0;
    overlapped.hEvent = hEvent;

    inBuffer.wType = 0x0102;
    inBuffer.DeviceIndex = (BYTE)dwUserIndex;

    ZeroMemory(&outBuffer, sizeof(OutWaitForGuideButtonBuffer_t));

    hr = SendReceiveIoctl(hDevice, Protocol::IOCTL_XINPUT_WAIT_FOR_GUIDE_BUTTON, &inBuffer, sizeof(InWaitForGuideButtonBuffer_t), &outBuffer, sizeof(OutWaitForGuideButtonBuffer_t), &overlapped);
    if (hr < 0 && hr != E_PENDING)
    {
        CloseHandle(hEvent);
        pListenState->Status = GetLastError();
        return hr;
    }

    receivedBytes = 0;
    pListenState->Status = ERROR_IO_PENDING;
    if (GetOverlappedResult(hDevice, &overlapped, &receivedBytes, TRUE) != FALSE)
    {
        if (receivedBytes == sizeof(OutWaitForGuideButtonBuffer_t))
        {
            if (outBuffer.status == 1)
            {
                pListenState->Status = 0;
                pListenState->unk1 = outBuffer.field_5;
                pListenState->unk2 = outBuffer.field_B;
                pListenState->unk3 = outBuffer.field_D;
                pListenState->unk4 = outBuffer.field_E;
                pListenState->unk5 = outBuffer.field_F;
                pListenState->unk6 = outBuffer.field_11;
                pListenState->unk7 = outBuffer.field_13;
                pListenState->unk8 = outBuffer.field_15;
            }
            else
            {
                pListenState->Status = ERROR_DEVICE_NOT_CONNECTED;
            }
        }
        else
        {
            pListenState->Status = ERROR_CANCELLED;
        }
    }
    else if (GetLastError() == ERROR_OPERATION_ABORTED)
    {
        pListenState->Status = ERROR_CANCELLED;
    }
    else
    {
        pListenState->Status = GetLastError();
    }

    CloseHandle(hEvent);
    return S_OK;
}

HRESULT WINAPI CancelGuideButtonWait(DeviceInfo_t* pDevice)
{
    if (pDevice->hGuideWait != INVALID_HANDLE_VALUE)
    {
        CloseHandle(pDevice->hGuideWait);
        pDevice->hGuideWait = INVALID_HANDLE_VALUE;
    }

    return S_OK;
}

namespace SetupDiWrapper
{

HDEVINFO WINAPI GetClassDevs(const GUID* ClassGuid, PCWSTR Enumerator, HWND hwndParent, DWORD Flags)
{
    if (g_pfnGetClassDevs)
        return g_pfnGetClassDevs(ClassGuid, Enumerator, hwndParent, Flags);

    return SetupDiGetClassDevsW(ClassGuid, Enumerator, hwndParent, Flags);
}

BOOL WINAPI EnumDeviceInterfaces(HDEVINFO DeviceInfoSet, PSP_DEVINFO_DATA DeviceInfoData, const GUID* InterfaceClassGuid, DWORD MemberIndex, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData)
{
    if (g_pfnEnumDeviceInterfaces)
        return g_pfnEnumDeviceInterfaces(DeviceInfoSet, DeviceInfoData, InterfaceClassGuid, MemberIndex, DeviceInterfaceData);

    return SetupDiEnumDeviceInterfaces(DeviceInfoSet, DeviceInfoData, InterfaceClassGuid, MemberIndex, DeviceInterfaceData);
}

BOOL WINAPI GetDeviceInterfaceDetail(HDEVINFO DeviceInfoSet, PSP_DEVICE_INTERFACE_DATA DeviceInterfaceData, PSP_DEVICE_INTERFACE_DETAIL_DATA_W DeviceInterfaceDetailData, DWORD DeviceInterfaceDetailDataSize, PDWORD RequiredSize, PSP_DEVINFO_DATA DeviceInfoData)
{
    if (g_pfnGetDeviceInterfaceDetail)
        return g_pfnGetDeviceInterfaceDetail(DeviceInfoSet, DeviceInterfaceData, DeviceInterfaceDetailData, DeviceInterfaceDetailDataSize, RequiredSize, DeviceInfoData);

    return SetupDiGetDeviceInterfaceDetail(DeviceInfoSet, DeviceInterfaceData, DeviceInterfaceDetailData, DeviceInterfaceDetailDataSize, RequiredSize, DeviceInfoData);;
}

BOOL WINAPI DestroyDeviceInfoList(HDEVINFO DeviceInfoSet)
{
    if (g_pfnDestroyDeviceInfoList)
        return g_pfnDestroyDeviceInfoList(DeviceInfoSet);

    return SetupDiDestroyDeviceInfoList(DeviceInfoSet);
}

}//namespace SetupDiWrapper

}

/////////////////////////////////////////////////////
// Utilities namespace
/////////////////////////////////////////////////////
namespace Utilities
{

HRESULT WINAPI Initialize()
{
    g_dwSettings = SET_USER_LED_ON_CREATE | DISABLE_USER_LED_ON_DESTROY | DISABLE_VIBRATION_ON_DESTROY;
    return S_OK;
}

HRESULT WINAPI Close()
{
    return S_OK;
}

LPVOID WINAPI MemAlloc(DWORD size)
{
    return LocalAlloc(LMEM_ZEROINIT, size);
}

void WINAPI MemFree(LPVOID lpMem)
{
    if (lpMem != nullptr)
    {
        LocalFree(lpMem);
    }
}

HRESULT WINAPI SafeCopyToUntrustedBuffer(void* pDst, const void* pSrc, DWORD size)
{
    CopyMemory(pDst, pSrc, size);
    return S_OK;
}

HRESULT WINAPI SafeCopyFromUntrustedBuffer(void* pDst, const void* pSrc, DWORD size)
{
    CopyMemory(pDst, pSrc, size);
    return S_OK;
}

BOOL WINAPI IsSettingSet(DWORD setting)
{
    return (setting & g_dwSettings) == setting;
}

BOOL WINAPI CheckForSettings(DWORD setting, LPVOID lpParam)
{
    switch (setting)
    {
    case 0xBEEF0000:
        break;

    case 0xBEEF0001:
        if (lpParam != nullptr)
            g_dwSettings |= SET_USER_LED_ON_CREATE;
        else
            g_dwSettings &= ~SET_USER_LED_ON_CREATE;
        break;

    case 0xBEEF0002:
        if (lpParam != nullptr)
            g_dwSettings |= DISABLE_USER_LED_ON_DESTROY;
        else
            g_dwSettings &= ~DISABLE_USER_LED_ON_DESTROY;
        break;

    case 0xBEEF0003:
        if (lpParam != nullptr)
            g_dwSettings |= DISABLE_VIBRATION_ON_DESTROY;
        else
            g_dwSettings &= ~DISABLE_VIBRATION_ON_DESTROY;
        break;
    }

    return TRUE;
}

}

/////////////////////////////////////////////////////
// XInputCore namespace
/////////////////////////////////////////////////////
namespace XInputCore
{

BOOL WINAPI Initialize()
{
    InitializeCriticalSection(&g_csGlobalLock);

    if (Utilities::Initialize() < 0)
        return FALSE;

    if (DeviceList::Initialize() < 0)
    {
        Utilities::Close();
        return FALSE;
    }

    if (DeviceEnum::Initialize() < 0)
    {
        DeviceList::Close();
        Utilities::Close();
        return FALSE;
    }

    if (DriverComm::Initialize() < 0)
    {
        DeviceEnum::Close();
        DeviceList::Close();
        Utilities::Close();
        return FALSE;
    }

    EnableCommunications(TRUE);
    g_IsInitialized = TRUE;

    return TRUE;
}

BOOL WINAPI Close()
{
    g_IsInitialized = FALSE;
    EnableCommunications(FALSE);

    Utilities::Close();
    DeviceList::Close();
    DeviceEnum::Close();
    DriverComm::Close();

    DeleteCriticalSection(&g_csGlobalLock);
    return TRUE;
}

HRESULT WINAPI Enter()
{
    if (!g_IsInitialized)
        return E_FAIL;

    EnterCriticalSection(&g_csGlobalLock);
    return S_OK;
}

HRESULT WINAPI Leave()
{
    if (!g_IsInitialized)
        return E_FAIL;

    LeaveCriticalSection(&g_csGlobalLock);
    return S_OK;
}

HRESULT WINAPI ProcessAPIRequest(DWORD dwUserIndex, HRESULT(WINAPI* pfn_ApiAction)(DeviceInfo_t*, void*, DWORD), void* pApiParam, DWORD reserved)
{
    HRESULT hr;
    DeviceInfo_t* pDevice = nullptr;
    BOOL retry = FALSE;

    for (DWORD i = 0; i < reserved; ++i);

    hr = Enter();
    if (hr < 0)
    {
        return hr;
    }

    do
    {
        hr = DeviceEnum::GetDeviceOnPort(dwUserIndex, &pDevice, g_IsCommunicationEnabled);
        if (hr < 0)
            break;

        if (pDevice == nullptr)
        {
            hr = 1; // NOT_CONNECTED
            break;
        }

        hr = pfn_ApiAction(pDevice, pApiParam, reserved);
        if (hr == 1)
            return S_OK;

        if (hr < 0)
        {// Error in internal API
            pDevice->status &= ~DEVICE_STATUS_ACTIVE;
        }

        if (!DeviceInfo::IsDeviceInactive(pDevice))
            break;

        pDevice = nullptr;
        hr = DeviceList::RemoveDeviceFromPort(dwUserIndex);

        if (hr < 0)
        {
            break;
        }

        if (retry == TRUE)
        {
            hr = 1; // NOT_CONNECTED
            break;
        }

        // We removed the old device, retry in case a new device has been plugged-in
        retry = TRUE;
    }
    while (retry == TRUE);

    Leave();
    return hr;
}

void WINAPI EnableCommunications(BOOL bEnabled)
{
    DeviceInfo_t* pDevice;

    if (g_IsCommunicationEnabled == bEnabled)
        return;

    g_IsCommunicationEnabled = bEnabled;
    DeviceInfo::OnEnableSettingChanged(bEnabled);

    for (DWORD dwUserIndex = 0; dwUserIndex < XUSER_MAX_COUNT; ++dwUserIndex)
    {
        pDevice = nullptr;
        if (DeviceEnum::GetDeviceOnPort(dwUserIndex, &pDevice, false) >= 0 && pDevice != nullptr)
        {
            SetStateApiParam_t apiParam;
            apiParam.pVibration = nullptr;
            DeviceInfo::g_pfnSetVibrationDispatcher(pDevice, &apiParam, 1);
        }
    }
}

}

/////////////////////////////////////////////////////
// Controller namespace
/////////////////////////////////////////////////////
DWORD WINAPI Controller_GetUserKeystroke(DeviceInfo_t* pDevice, BYTE bUserIndex, DWORD reserved, XINPUT_KEYSTROKE* pKeystroke)
{
    static DWORD s_KeyCode;
    static DWORD s_dwKeyPressStart;
    static BOOL  s_bKeyDown;

    XINPUT_STATE gamepadState;
    GetStateApiParam_t apiParam;
    DWORD virtualKey;
    int key;
    DWORD pressedTriggers;
    DWORD dwTickCount;

    apiParam.pState = &gamepadState;
    if (DeviceInfo::g_pfnGetStateDispatcher(pDevice, &apiParam, 1) < 0)
        return ERROR_EMPTY;

    gamepadState.Gamepad.wButtons &= XINPUT_BUTTON_MASK_WITHOUT_GUIDE;

    pKeystroke->UserIndex = bUserIndex;
    pKeystroke->Unicode = 0;

    key = 0;

    virtualKey = Controller_CalculateKeyFromThumbPos(VK_PAD_LTHUMB_UP, gamepadState.Gamepad.sThumbLX, gamepadState.Gamepad.sThumbLY);
    pKeystroke->VirtualKey = (WORD)virtualKey;
    if (pDevice->LeftStickVirtualKey != virtualKey)
    {
        if (pDevice->LeftStickVirtualKey != 0)
        {
            pKeystroke->VirtualKey = pDevice->LeftStickVirtualKey;
            pKeystroke->Flags = XINPUT_KEYSTROKE_KEYUP;
            s_KeyCode = 0;
            pDevice->LeftStickVirtualKey = 0;
            return ERROR_SUCCESS;
        }
        key = virtualKey;
        pDevice->LeftStickVirtualKey = (WORD)virtualKey;
    }

    virtualKey = Controller_CalculateKeyFromThumbPos(VK_PAD_RTHUMB_UP, gamepadState.Gamepad.sThumbRX, gamepadState.Gamepad.sThumbRY);

    if (virtualKey)
        pKeystroke->VirtualKey = (WORD)virtualKey;
    if (pDevice->RightStickVirtualKey != virtualKey)
    {
        if (pDevice->RightStickVirtualKey != 0)
        {
            pKeystroke->VirtualKey = pDevice->RightStickVirtualKey;
            pKeystroke->Flags = XINPUT_KEYSTROKE_KEYUP;
            s_KeyCode = 0;
            pDevice->RightStickVirtualKey = 0;
            return ERROR_SUCCESS;
        }
        key = virtualKey;
        pDevice->RightStickVirtualKey = (WORD)virtualKey;
    }

    pressedTriggers = 0;
    if (gamepadState.Gamepad.bLeftTrigger > 30u)
        pressedTriggers = 1;
    if (gamepadState.Gamepad.bRightTrigger > 30u)
        pressedTriggers = 2;

    virtualKey = VK_PAD_LTRIGGER;
    for (int i = 0; i < 2; ++i)
    {
        BYTE keyBit = 1 << i;
        if (pDevice->bTriggers & keyBit)
        {
            if (!(pressedTriggers & keyBit))
            {
                pDevice->bTriggers &= ~keyBit;
                s_KeyCode = 0;
                pKeystroke->VirtualKey = (WORD)virtualKey;
                pKeystroke->Flags = XINPUT_KEYSTROKE_KEYUP;
                return ERROR_SUCCESS;
            }
            pKeystroke->VirtualKey = (WORD)virtualKey;
        }
        else if (pressedTriggers & keyBit)
        {
            pDevice->bTriggers |= keyBit;
            key = virtualKey;
        }
        ++virtualKey;
    }

    for (int i = 0; i < (sizeof(XINPUT_GAMEPAD::wButtons) * 8); ++i)
    {
        WORD keyBit = 1 << i;
        switch (i)
        {
        case 0: virtualKey = VK_PAD_DPAD_UP; break;
        case 8: virtualKey = VK_PAD_LSHOULDER; break;
        case 9: virtualKey = VK_PAD_RSHOULDER; break;
        case 12: virtualKey = VK_PAD_A; break;
        }

        if (pDevice->wButtons & keyBit)
        {
            if (!(gamepadState.Gamepad.wButtons & keyBit))
            {
                pDevice->wButtons &= ~keyBit;
                pKeystroke->VirtualKey = (WORD)virtualKey;
                pKeystroke->Flags = XINPUT_KEYSTROKE_KEYUP;
                s_KeyCode = 0;
                return ERROR_SUCCESS;
            }
            pKeystroke->VirtualKey = (WORD)virtualKey;
        }
        else if (gamepadState.Gamepad.wButtons & keyBit)
        {
            pDevice->wButtons |= keyBit;
            key = virtualKey;
        }
        ++virtualKey;
    }

    dwTickCount = GetTickCount();
    if (key)
    {
        pKeystroke->VirtualKey = key;
        pKeystroke->Flags = XINPUT_KEYSTROKE_KEYDOWN;
        s_KeyCode = key;
        s_dwKeyPressStart = dwTickCount;
        s_bKeyDown = TRUE;
    }
    else if (pKeystroke->VirtualKey == s_KeyCode)
    {
        DWORD repeatTimeMs = (s_bKeyDown == TRUE ? 400 : 100);

        if ((dwTickCount - s_dwKeyPressStart) < repeatTimeMs)
        {
            pKeystroke->VirtualKey = 0;
        }
        else
        {
            pKeystroke->Flags = XINPUT_KEYSTROKE_KEYDOWN | XINPUT_KEYSTROKE_REPEAT;
            s_dwKeyPressStart = dwTickCount;
            s_bKeyDown = FALSE;
        }
    }
    else
    {
        pKeystroke->VirtualKey = 0;
    }
    if (!pKeystroke->VirtualKey)
        return ERROR_EMPTY;

    return ERROR_SUCCESS;
}

DWORD WINAPI Controller_CalculateKeyFromThumbPos(SHORT base, SHORT X, SHORT Y)
{
    if (Y > 20000)
    {
        if (X < -20000)
            return base + 4;

        if (X > 20000)
            return base + 5;

        return base + 0;
    }

    if (Y < -20000)
    {

        if (X > 20000)
            return base + 6;

        if (X < -20000)
            return base + 7;

        return base + 1;
    }

    if (X > 20000)
        return base + 2;

    if (X < -20000)
        return base + 3;

    return 0;
}

/////////////////////////////////////////////////////
// Exported functions
/////////////////////////////////////////////////////
#ifdef __cplusplus
extern "C" {
#endif

    BOOL WINAPI OpenXinputDllMain(HINSTANCE hInstDll, DWORD fdwReason, LPVOID lpvReserved)
    {
        BOOL res = TRUE;

        switch (fdwReason)
        {
        case DLL_PROCESS_DETACH:
            res = XInputCore::Close();
            break;

        case DLL_PROCESS_ATTACH:
            res = XInputCore::Initialize();
            break;

        case DLL_THREAD_ATTACH:
        case DLL_THREAD_DETACH:
            break;

        default:
            if (DriverComm::CheckForDriverHook(fdwReason, lpvReserved) == TRUE)
            {
                Utilities::CheckForSettings(fdwReason, lpvReserved);
            }
        }

        return res;
    }

    DWORD WINAPI OpenXInputGetState(_In_ DWORD dwUserIndex, _Out_ XINPUT_STATE* pState)
    {
        GetStateApiParam_t apiParam;
        HRESULT hr;
        DWORD result;

        if (dwUserIndex > XUSER_MAX_COUNT || pState == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.pState = pState;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetStateDispatcher, &apiParam, 1);
        result = XInputReturnCodeFromHRESULT(hr);

        if (result == ERROR_SUCCESS)
            pState->Gamepad.wButtons &= XINPUT_BUTTON_MASK_WITHOUT_GUIDE;

        return result;
    }

    DWORD WINAPI OpenXInputSetState(_In_ DWORD dwUserIndex, _In_ XINPUT_VIBRATION* pVibration)
    {
        SetStateApiParam_t apiParam;
        HRESULT hr;

        if (dwUserIndex > XUSER_MAX_COUNT || pVibration == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.pVibration = pVibration;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnSetVibrationDispatcher, &apiParam, 1);
        return XInputReturnCodeFromHRESULT(hr);
    }

    DWORD WINAPI OpenXInputGetCapabilities(_In_ DWORD dwUserIndex, _In_ DWORD dwFlags, _Out_ XINPUT_CAPABILITIES* pCapabilities)
    {
        GetCapabilitiesApiParam_t apiParam;
        DWORD result;
        HRESULT hr;
        if (dwUserIndex > XUSER_MAX_COUNT || (dwFlags != 0 && dwFlags != XINPUT_CAPS_FFB_SUPPORTED) || pCapabilities == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.pCapabilities = pCapabilities;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetCapabilitiesDispatcher, &apiParam, 1);
        result = XInputReturnCodeFromHRESULT(hr);

        if (result == ERROR_SUCCESS)
            pCapabilities->Gamepad.wButtons &= XINPUT_BUTTON_MASK_WITHOUT_GUIDE;

        return result;
    }

    void WINAPI OpenXInputEnable(_In_ BOOL enable)
    {
        XInputCore::EnableCommunications(enable != FALSE);
    }

    DWORD WINAPI OpenXInputGetDSoundAudioDeviceGuids(_In_ DWORD dwUserIndex, _Out_ GUID* pDSoundRenderGuid, _Out_ GUID* pDSoundCaptureGuid)
    {
        HRESULT hr;
        DWORD result;
        bool doApiCall;
        bool run;
        HANDLE hDevice;
        DeviceInfo_t device;
        GetAudioDeviceGuidsApiParam_t apiParam;

        if (dwUserIndex > XUSER_MAX_COUNT || pDSoundRenderGuid == nullptr || pDSoundCaptureGuid == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.pHeadphoneGuid = pDSoundRenderGuid;
        apiParam.pMicrophoneGuid = pDSoundCaptureGuid;

        doApiCall = true;
        result = ERROR_DEVICE_NOT_CONNECTED;
        QuickDriverEnum quickEnum;
        run = quickEnum.Restart();
        while (run)
        {
            hDevice = INVALID_HANDLE_VALUE;
            run = quickEnum.GetNext(&hDevice);
            if (hDevice != INVALID_HANDLE_VALUE)
            {
                if (quickEnum.MinFillFromInterface(hDevice, &device) && device.wType == 0x0102)
                {
                    device.dwUserIndex = (BYTE)dwUserIndex;
                    if (DeviceInfo::g_pfnGetAudioDeviceGuidsDispatcher(&device, &apiParam, 2) >= 0)
                    {
                        doApiCall = false;
                        run = false;
                        result = ERROR_SUCCESS;
                    }
                }
                CloseHandle(hDevice);
            }
        }
        if (doApiCall == 1)
        {
            hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetAudioDeviceGuidsDispatcher, &apiParam, 2u);
            result = XInputReturnCodeFromHRESULT(hr);
        }
        return result;
    }

    DWORD WINAPI OpenXInputGetBatteryInformation(_In_ DWORD dwUserIndex, _In_ BYTE devType, _Out_ XINPUT_BATTERY_INFORMATION* pBatteryInformation)
    {
        DWORD result;
        HRESULT hr;
        bool doApiCall;
        GetBatteryInformationApiParam_t apiParam;

        if (dwUserIndex > XUSER_MAX_COUNT || pBatteryInformation == nullptr)
            return ERROR_BAD_ARGUMENTS;

        pBatteryInformation->BatteryLevel = 0;
        pBatteryInformation->BatteryType = 0;

        result = ERROR_SUCCESS;
        doApiCall = true;
        if (devType == XINPUT_DEVTYPE_GAMEPAD)
        {
            HANDLE hDevice;
            DeviceInfo_t device;
            result = ERROR_DEVICE_NOT_CONNECTED;
            QuickDriverEnum quickEnum;
            bool run = quickEnum.Restart();
            while (run)
            {
                hDevice = INVALID_HANDLE_VALUE;
                run = quickEnum.GetNext(&hDevice);
                if (hDevice != INVALID_HANDLE_VALUE)
                {
                    if (quickEnum.MinFillFromInterface(hDevice, &device) && device.wType >= 0x0102)
                    {
                        device.dwUserIndex = (BYTE)dwUserIndex;
                        if (DriverComm::GetBatteryInformation(&device, XINPUT_DEVTYPE_GAMEPAD, pBatteryInformation) >= 0)
                        {
                            doApiCall = 0;
                            run = 0;
                            result = ERROR_SUCCESS;
                        }
                    }
                    CloseHandle(hDevice);
                }
            }
        }
        if (doApiCall)
        {
            apiParam.DeviceType = &devType;
            apiParam.pBatteryInformation = pBatteryInformation;
            hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetBatteryInformationDispatcher, &apiParam, 2);
            result = XInputReturnCodeFromHRESULT(hr);
        }

        return result;
    }

    DWORD WINAPI OpenXInputGetKeystroke(_In_ DWORD dwUserIndex, _Reserved_ DWORD dwReserved, _Out_ PXINPUT_KEYSTROKE pKeystroke)
    {
        GetKeystrokeApiParam_t apiParam;
        DWORD result;
        HRESULT hr;
        if ((dwUserIndex > XUSER_MAX_COUNT && dwUserIndex != XUSER_INDEX_ANY) || pKeystroke == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.lpReserved = &dwReserved;
        apiParam.pKeystroke = pKeystroke;
        apiParam.lpLastError = &result;
        if (dwUserIndex < XUSER_MAX_COUNT)
        {
            result = ERROR_SUCCESS;
            hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetKeystrokeDispatcher, &apiParam, 3);
            if (hr >= 0)
                pKeystroke->UserIndex = (BYTE)dwUserIndex;
            else
                result = XInputReturnCodeFromHRESULT(hr);
        }
        else
        {
            result = ERROR_EMPTY;
            for (BYTE i = 0; i < XUSER_MAX_COUNT; ++i)
            {
                hr = XInputCore::ProcessAPIRequest(i, DeviceInfo::g_pfnGetKeystrokeDispatcher, &apiParam, 3);
                if (result == ERROR_SUCCESS)
                {
                    pKeystroke->UserIndex = i;
                    break;
                }
            }
            if (result != ERROR_SUCCESS)
                result = ERROR_EMPTY;
        }

        return result;
    }

    DWORD WINAPI OpenXInputGetStateEx(_In_ DWORD dwUserIndex, _Out_ XINPUT_STATE* pState)
    {
        HRESULT hr;
        GetStateApiParam_t apiParam;

        if (dwUserIndex > XUSER_MAX_COUNT || pState == nullptr)
            return ERROR_BAD_ARGUMENTS;

        apiParam.pState = pState;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnGetStateDispatcher, &apiParam, 1);
        return XInputReturnCodeFromHRESULT(hr);
    }

    DWORD WINAPI OpenXInputWaitForGuideButton(_In_ DWORD dwUserIndex, _In_ HANDLE hEvent, _Out_ XINPUT_LISTEN_STATE* pListenState)
    {
        HRESULT hr;
        WaitGuideButtonApiParam_t apiParam;

        if (dwUserIndex > XUSER_MAX_COUNT || pListenState == nullptr)
            return ERROR_BAD_ARGUMENTS;

        if (hEvent == INVALID_HANDLE_VALUE)
            hEvent = 0;

        apiParam.lphEvent = &hEvent;
        apiParam.pListenState = pListenState;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnWaitForGuideButton, &apiParam, 2);
        return XInputReturnCodeFromHRESULT(hr);
    }

    DWORD WINAPI OpenXInputCancelGuideButtonWait(_In_ DWORD dwUserIndex)
    {
        HRESULT hr;

        if (dwUserIndex > XUSER_MAX_COUNT)
            return ERROR_BAD_ARGUMENTS;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnCancelGuideButtonWait, nullptr, 0);
        return XInputReturnCodeFromHRESULT(hr);
    }

    DWORD WINAPI OpenXInputPowerOffController(_In_ DWORD dwUserIndex)
    {
        HRESULT hr;

        if (dwUserIndex > XUSER_MAX_COUNT)
            return ERROR_BAD_ARGUMENTS;

        hr = XInputCore::ProcessAPIRequest(dwUserIndex, DeviceInfo::g_pfnPowerOffController, nullptr, 0);
        return XInputReturnCodeFromHRESULT(hr);

    }

    DWORD WINAPI OpenXInputGetMaxControllerCount()
    {
        return XUSER_MAX_COUNT;
    }

#ifdef __cplusplus
}
#endif