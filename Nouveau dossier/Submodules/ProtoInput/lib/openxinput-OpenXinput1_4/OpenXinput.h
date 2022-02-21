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

#pragma once

#include <Windows.h>

#define OPENXINPUT_NO_XINPUT_HEADER

#if !defined(OPENXINPUT_NO_XINPUT_HEADER)
#include <Xinput.h>
#else

#pragma region Application Family
#if WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_APP)
 // This part has been copied from Xinput.h to remove the dependency on Xinput.h that might update in the future.

 //
 // Device types available in XINPUT_CAPABILITIES
 //
#define XINPUT_DEVTYPE_GAMEPAD          0x01

//
// Device subtypes available in XINPUT_CAPABILITIES
//

#define XINPUT_DEVSUBTYPE_GAMEPAD           0x01

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

#define XINPUT_DEVSUBTYPE_UNKNOWN           0x00
#define XINPUT_DEVSUBTYPE_WHEEL             0x02
#define XINPUT_DEVSUBTYPE_ARCADE_STICK      0x03
#define XINPUT_DEVSUBTYPE_FLIGHT_STICK      0x04
#define XINPUT_DEVSUBTYPE_DANCE_PAD         0x05
#define XINPUT_DEVSUBTYPE_GUITAR            0x06
#define XINPUT_DEVSUBTYPE_GUITAR_ALTERNATE  0x07
#define XINPUT_DEVSUBTYPE_DRUM_KIT          0x08
#define XINPUT_DEVSUBTYPE_GUITAR_BASS       0x0B
#define XINPUT_DEVSUBTYPE_ARCADE_PAD        0x13

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

//
// Flags for XINPUT_CAPABILITIES
//

#define XINPUT_CAPS_VOICE_SUPPORTED     0x0004

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

#define XINPUT_CAPS_FFB_SUPPORTED       0x0001
#define XINPUT_CAPS_WIRELESS            0x0002
#define XINPUT_CAPS_PMD_SUPPORTED       0x0008
#define XINPUT_CAPS_NO_NAVIGATION       0x0010

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

//
// Constants for gamepad buttons
//
#define XINPUT_GAMEPAD_DPAD_UP          0x0001
#define XINPUT_GAMEPAD_DPAD_DOWN        0x0002
#define XINPUT_GAMEPAD_DPAD_LEFT        0x0004
#define XINPUT_GAMEPAD_DPAD_RIGHT       0x0008
#define XINPUT_GAMEPAD_START            0x0010
#define XINPUT_GAMEPAD_BACK             0x0020
#define XINPUT_GAMEPAD_LEFT_THUMB       0x0040
#define XINPUT_GAMEPAD_RIGHT_THUMB      0x0080
#define XINPUT_GAMEPAD_LEFT_SHOULDER    0x0100
#define XINPUT_GAMEPAD_RIGHT_SHOULDER   0x0200
#define XINPUT_GAMEPAD_A                0x1000
#define XINPUT_GAMEPAD_B                0x2000
#define XINPUT_GAMEPAD_X                0x4000
#define XINPUT_GAMEPAD_Y                0x8000


//
// Gamepad thresholds
//
#define XINPUT_GAMEPAD_LEFT_THUMB_DEADZONE  7849
#define XINPUT_GAMEPAD_RIGHT_THUMB_DEADZONE 8689
#define XINPUT_GAMEPAD_TRIGGER_THRESHOLD    30

//
// Flags to pass to XInputGetCapabilities
//
#define XINPUT_FLAG_GAMEPAD             0x00000001

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

//
// Devices that support batteries
//
#define BATTERY_DEVTYPE_GAMEPAD         0x00
#define BATTERY_DEVTYPE_HEADSET         0x01

//
// Flags for battery status level
//
#define BATTERY_TYPE_DISCONNECTED       0x00    // This device is not connected
#define BATTERY_TYPE_WIRED              0x01    // Wired device, no battery
#define BATTERY_TYPE_ALKALINE           0x02    // Alkaline battery source
#define BATTERY_TYPE_NIMH               0x03    // Nickel Metal Hydride battery source
#define BATTERY_TYPE_UNKNOWN            0xFF    // Cannot determine the battery type

// These are only valid for wireless, connected devices, with known battery types
// The amount of use time remaining depends on the type of device.
#define BATTERY_LEVEL_EMPTY             0x00
#define BATTERY_LEVEL_LOW               0x01
#define BATTERY_LEVEL_MEDIUM            0x02
#define BATTERY_LEVEL_FULL              0x03

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

// User index definitions

#define XUSER_INDEX_ANY                 0x000000FF

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

//
// Codes returned for the gamepad keystroke
//

#define VK_PAD_A                        0x5800
#define VK_PAD_B                        0x5801
#define VK_PAD_X                        0x5802
#define VK_PAD_Y                        0x5803
#define VK_PAD_RSHOULDER                0x5804
#define VK_PAD_LSHOULDER                0x5805
#define VK_PAD_LTRIGGER                 0x5806
#define VK_PAD_RTRIGGER                 0x5807

#define VK_PAD_DPAD_UP                  0x5810
#define VK_PAD_DPAD_DOWN                0x5811
#define VK_PAD_DPAD_LEFT                0x5812
#define VK_PAD_DPAD_RIGHT               0x5813
#define VK_PAD_START                    0x5814
#define VK_PAD_BACK                     0x5815
#define VK_PAD_LTHUMB_PRESS             0x5816
#define VK_PAD_RTHUMB_PRESS             0x5817

#define VK_PAD_LTHUMB_UP                0x5820
#define VK_PAD_LTHUMB_DOWN              0x5821
#define VK_PAD_LTHUMB_RIGHT             0x5822
#define VK_PAD_LTHUMB_LEFT              0x5823
#define VK_PAD_LTHUMB_UPLEFT            0x5824
#define VK_PAD_LTHUMB_UPRIGHT           0x5825
#define VK_PAD_LTHUMB_DOWNRIGHT         0x5826
#define VK_PAD_LTHUMB_DOWNLEFT          0x5827

#define VK_PAD_RTHUMB_UP                0x5830
#define VK_PAD_RTHUMB_DOWN              0x5831
#define VK_PAD_RTHUMB_RIGHT             0x5832
#define VK_PAD_RTHUMB_LEFT              0x5833
#define VK_PAD_RTHUMB_UPLEFT            0x5834
#define VK_PAD_RTHUMB_UPRIGHT           0x5835
#define VK_PAD_RTHUMB_DOWNRIGHT         0x5836
#define VK_PAD_RTHUMB_DOWNLEFT          0x5837

//
// Flags used in XINPUT_KEYSTROKE
//
#define XINPUT_KEYSTROKE_KEYDOWN        0x0001
#define XINPUT_KEYSTROKE_KEYUP          0x0002
#define XINPUT_KEYSTROKE_REPEAT         0x0004

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

//
// Structures used by XInput APIs
//
typedef struct _XINPUT_GAMEPAD
{
    WORD                                wButtons;
    BYTE                                bLeftTrigger;
    BYTE                                bRightTrigger;
    SHORT                               sThumbLX;
    SHORT                               sThumbLY;
    SHORT                               sThumbRX;
    SHORT                               sThumbRY;
} XINPUT_GAMEPAD, * PXINPUT_GAMEPAD;

typedef struct _XINPUT_STATE
{
    DWORD                               dwPacketNumber;
    XINPUT_GAMEPAD                      Gamepad;
} XINPUT_STATE, * PXINPUT_STATE;

typedef struct _XINPUT_VIBRATION
{
    WORD                                wLeftMotorSpeed;
    WORD                                wRightMotorSpeed;
} XINPUT_VIBRATION, * PXINPUT_VIBRATION;

typedef struct _XINPUT_CAPABILITIES
{
    BYTE                                Type;
    BYTE                                SubType;
    WORD                                Flags;
    XINPUT_GAMEPAD                      Gamepad;
    XINPUT_VIBRATION                    Vibration;
} XINPUT_CAPABILITIES, * PXINPUT_CAPABILITIES;

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

typedef struct _XINPUT_BATTERY_INFORMATION
{
    BYTE BatteryType;
    BYTE BatteryLevel;
} XINPUT_BATTERY_INFORMATION, * PXINPUT_BATTERY_INFORMATION;

typedef struct _XINPUT_KEYSTROKE
{
    WORD    VirtualKey;
    WCHAR   Unicode;
    WORD    Flags;
    BYTE    UserIndex;
    BYTE    HidCode;
} XINPUT_KEYSTROKE, * PXINPUT_KEYSTROKE;

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

#endif /* WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_APP) */
#pragma endregion

//
// XInput APIs
//
#ifdef __cplusplus
extern "C" {
#endif

#pragma region Application Family
#if WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_APP)

    DWORD WINAPI OpenXInputGetState
    (
        _In_  DWORD         dwUserIndex,  // Index of the gamer associated with the device
        _Out_ XINPUT_STATE* pState        // Receives the current state
    );

    DWORD WINAPI OpenXInputSetState
    (
        _In_ DWORD             dwUserIndex,  // Index of the gamer associated with the device
        _In_ XINPUT_VIBRATION* pVibration    // The vibration information to send to the controller
    );

    DWORD WINAPI OpenXInputGetCapabilities
    (
        _In_  DWORD                dwUserIndex,   // Index of the gamer associated with the device
        _In_  DWORD                dwFlags,       // Input flags that identify the device type
        _Out_ XINPUT_CAPABILITIES* pCapabilities  // Receives the capabilities
    );

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

    void WINAPI OpenXInputEnable
    (
        _In_ BOOL enable     // [in] Indicates whether xinput is enabled or disabled. 
    );

#if(_WIN32_WINNT >= _WIN32_WINNT_WIN10)
#pragma deprecated(XInputEnable)
#endif

    DWORD WINAPI OpenXInputGetAudioDeviceIds
    (
        _In_  DWORD                             dwUserIndex,        // Index of the gamer associated with the device
        _Out_writes_opt_(*pRenderCount) LPWSTR  pRenderDeviceId,    // Windows Core Audio device ID string for render (speakers)
        _Inout_opt_ UINT* pRenderCount,       // Size of render device ID string buffer (in wide-chars)
        _Out_writes_opt_(*pCaptureCount) LPWSTR pCaptureDeviceId,   // Windows Core Audio device ID string for capture (microphone)
        _Inout_opt_ UINT* pCaptureCount       // Size of capture device ID string buffer (in wide-chars)
    );

    DWORD WINAPI OpenXInputGetBatteryInformation
    (
        _In_  DWORD                       dwUserIndex,        // Index of the gamer associated with the device
        _In_  BYTE                        devType,            // Which device on this user index
        _Out_ XINPUT_BATTERY_INFORMATION* pBatteryInformation // Contains the level and types of batteries
    );

    DWORD WINAPI OpenXInputGetKeystroke
    (
        _In_       DWORD dwUserIndex,              // Index of the gamer associated with the device
        _Reserved_ DWORD dwReserved,               // Reserved for future use
        _Out_      PXINPUT_KEYSTROKE pKeystroke    // Pointer to an XINPUT_KEYSTROKE structure that receives an input event.
    );

#endif //(_WIN32_WINNT >= _WIN32_WINNT_WIN8)

#endif /* WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_APP) */
#pragma endregion

#pragma region Desktop Family
#if WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_DESKTOP)

#if(_WIN32_WINNT < _WIN32_WINNT_WIN8)

    DWORD WINAPI XInputGetDSoundAudioDeviceGuids
    (
        _In_  DWORD     dwUserIndex,          // Index of the gamer associated with the device
        _Out_ GUID* pDSoundRenderGuid,    // DSound device ID for render (speakers)
        _Out_ GUID* pDSoundCaptureGuid    // DSound device ID for capture (microphone)
    );

#endif //(_WIN32_WINNT < _WIN32_WINNT_WIN8)

#endif /* WINAPI_FAMILY_PARTITION(WINAPI_PARTITION_DESKTOP) */
#pragma endregion

#ifdef __cplusplus
}
#endif

#endif

#ifdef XUSER_MAX_COUNT
#undef XUSER_MAX_COUNT
#endif

#ifndef OPENXINPUT_XUSER_MAX_COUNT
#define OPENXINPUT_XUSER_MAX_COUNT 64
#endif

#if OPENXINPUT_XUSER_MAX_COUNT < 4
#define XUSER_MAX_COUNT 4
#else
#define XUSER_MAX_COUNT OPENXINPUT_XUSER_MAX_COUNT
#endif

#ifdef __cplusplus
extern "C" {
#endif

    // This is my own added exported symbol.
    // It returns the compile-time XUSER_MAX_COUNT.
    DWORD WINAPI OpenXInputGetMaxControllerCount();

#ifdef __cplusplus
}
#endif
typedef DWORD(WINAPI XInputGetMaxControllerCount_t)();


////////////////////////////////////////
// Here lies the hidden part on Xinput

typedef struct _XINPUT_LISTEN_STATE
{
    DWORD Status;
    DWORD unk1;
    WORD unk2;
    BYTE unk3;
    BYTE unk4;
    WORD unk5;
    WORD unk6;
    WORD unk7;
    WORD unk8;
} XINPUT_LISTEN_STATE, * PXINPUT_LISTEN_STATE;

struct XINPUT_GUIDE_EVENT
{
    LPHANDLE lphEvent;
    PXINPUT_LISTEN_STATE pListenState;
};

#ifndef XINPUT_GAMEPAD_GUIDE
#define XINPUT_GAMEPAD_GUIDE 0x0400
#endif

#define XINPUT_BUTTON_MASK_WITHOUT_GUIDE \
    ((XINPUT_GAMEPAD_DPAD_UP)|(XINPUT_GAMEPAD_DPAD_DOWN)|(XINPUT_GAMEPAD_DPAD_LEFT)|(XINPUT_GAMEPAD_DPAD_RIGHT)| \
    (XINPUT_GAMEPAD_START)|(XINPUT_GAMEPAD_BACK)|(XINPUT_GAMEPAD_LEFT_THUMB)|(XINPUT_GAMEPAD_RIGHT_THUMB)| \
    (XINPUT_GAMEPAD_LEFT_SHOULDER)|(XINPUT_GAMEPAD_RIGHT_SHOULDER)| \
    (XINPUT_GAMEPAD_A)|(XINPUT_GAMEPAD_B)|(XINPUT_GAMEPAD_X)|(XINPUT_GAMEPAD_Y))

#define XINPUT_BUTTON_MASK ((XINPUT_BUTTON_MASK_WITHOUT_GUIDE)|XINPUT_GAMEPAD_GUIDE)

#ifdef __cplusplus
extern "C" {
#endif

    DWORD WINAPI OpenXInputGetStateEx(_In_ DWORD dwUserIndex, _Out_ XINPUT_STATE* pState);

    // Pass INVALID_HANDLE_VALUE or NULL in hEvent for a synchronous wait
    DWORD WINAPI OpenXInputWaitForGuideButton(_In_ DWORD dwUserIndex, _In_ HANDLE hEvent, _Out_ XINPUT_LISTEN_STATE* pListenState);

    DWORD WINAPI OpenXInputCancelGuideButtonWait(_In_ DWORD dwUserIndex);

    DWORD WINAPI OpenXInputPowerOffController(_In_ DWORD dwUserIndex);

#ifdef __cplusplus
}
#endif