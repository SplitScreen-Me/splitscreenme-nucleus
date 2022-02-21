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

#include <Windows.h>
#include "../OpenXinput.h"

#include <thread>
#include <iostream>

//#pragma comment(lib, "Xinput1_4.lib")
#pragma comment(lib, "Xinput.lib")

#ifndef XINPUT_GAMEPAD_GUIDE
#define XINPUT_GAMEPAD_GUIDE 0x0400
#endif

enum XInputDeviceConsoleBufferIndex
{
    DEVICE_STATUS = 0                 ,
    DEVICE_BATTERY_HEADER             ,
    DEVICE_BATTERY_TYPE               ,
    DEVICE_BATTERY_LEVEL              ,
    DEVICE_CAPABILITY_HEADER          ,
    DEVICE_CAPABILITY_TYPE            ,
    DEVICE_CAPABILITY_SUBTYPE         ,
    DEVICE_CAPABILITY_FLAGS           ,
    DEVICE_CAPABILITY_PRODUCTID       ,
    DEVICE_CAPABILITY_VENDORID        ,
    DEVICE_STATE_HEADER               ,
    DEVICE_STATE_BUTTON_DPAD_UP       ,
    DEVICE_STATE_BUTTON_DPAD_DOWN     ,
    DEVICE_STATE_BUTTON_DPAD_LEFT     ,
    DEVICE_STATE_BUTTON_DPAD_RIGHT    ,
    DEVICE_STATE_BUTTON_START         ,
    DEVICE_STATE_BUTTON_BACK          ,
    DEVICE_STATE_BUTTON_LEFT_THUMB    ,
    DEVICE_STATE_BUTTON_RIGHT_THUMB   ,
    DEVICE_STATE_BUTTON_LEFT_SHOULDER ,
    DEVICE_STATE_BUTTON_RIGHT_SHOULDER,
    DEVICE_STATE_BUTTON_GUIDE         ,
    DEVICE_STATE_BUTTON_A             ,
    DEVICE_STATE_BUTTON_B             ,
    DEVICE_STATE_BUTTON_X             ,
    DEVICE_STATE_BUTTON_Y             ,
    DEVICE_STATE_LEFTTRIGGER          ,
    DEVICE_STATE_RIGHTTRIGGER         ,
    DEVICE_STATE_LEFTTHUMBX           ,
    DEVICE_STATE_LEFTTHUMBY           ,
    DEVICE_STATE_RIGHTTHUMBX          ,
    DEVICE_STATE_RIGHTTHUMBY          ,
    DEVICE_MAX_CONSOLE_LINES
};

#define MAX_DEVICE_LINE_WIDTH 35

struct XInputDevice_t
{
    bool connected;
    DWORD deviceIndex;
    char consoleBuffer[DEVICE_MAX_CONSOLE_LINES][MAX_DEVICE_LINE_WIDTH+1];
    XINPUT_CAPABILITIES_EX capabilities;
    XINPUT_BATTERY_INFORMATION battery;
    XINPUT_STATE oldState;
    XINPUT_STATE state;
};

static DWORD XinputMaxControllerCount = XUSER_MAX_COUNT;

// conio library
#define COLOR_BLACK 0
#define COLOR_BLUE 1
#define COLOR_GREEN 2
#define COLOR_CYAN 3
#define COLOR_RED 4
#define COLOR_PURPLE 5
#define COLOR_YELLOW 6
#define COLOR_GRAY 7
#define COLOR_DARKGRAY 8
#define COLOR_LIGHTBLUE 9
#define COLOR_LIGHTGREEN 10
#define COLOR_LIGHTCYAN 11
#define COLOR_LIGHTRED 12
#define COLOR_LIGHTPURPLE 13
#define COLOR_LIGHTYELLOW 14
#define COLOR_WHITE 15

void GetConsoleInfos(HANDLE hConsole, CONSOLE_SCREEN_BUFFER_INFOEX* pInfos)
{
    pInfos->cbSize = sizeof(CONSOLE_SCREEN_BUFFER_INFOEX);
    GetConsoleScreenBufferInfoEx(hConsole, pInfos);
}

void gotoxy(const int x, const int y)
{
    CONSOLE_SCREEN_BUFFER_INFOEX Infos;
    HANDLE hConsole;
    
    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleInfos(hConsole , &Infos);

    if (x >= 0 && y >= 0 && x <= Infos.dwSize.X && y <= Infos.dwSize.Y)
    {
        COORD Coords;
        Coords.X = x;
        Coords.Y = y;
        SetConsoleCursorPosition(hConsole, Coords);
    }
}

void clrscr()
{
    DWORD NumberOfCharWritten = 0;
    COORD Coordinates;
    
    CONSOLE_SCREEN_BUFFER_INFOEX Infos;
    HANDLE hConsole;

    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleInfos(hConsole, &Infos);

    Coordinates.X = 0;
    
    for (Coordinates.Y = 0; Coordinates.Y < Infos.dwSize.Y; Coordinates.Y++)
    {
        FillConsoleOutputCharacterA(hConsole, ' ', Infos.dwSize.X, Coordinates, &NumberOfCharWritten);
        FillConsoleOutputAttribute(hConsole, Infos.wAttributes, Infos.dwSize.X, Coordinates, &NumberOfCharWritten);
    }
    gotoxy(1, 1);
}

BOOL setcursortype(unsigned int Type)
{
    HANDLE HConsole;
    CONSOLE_CURSOR_INFO CursorInfo;

    HConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    
    switch (Type)
    {
        case 0:// Hide
            CursorInfo.dwSize = 0x01;
            CursorInfo.bVisible = 0;
            break;

        case 1:// Default
            CursorInfo.dwSize = 0x19;
            CursorInfo.bVisible = 1;
            break;

        case 2:// Big
            CursorInfo.dwSize = 0x64;
            CursorInfo.bVisible = 1;
            break;
    }
    
    return SetConsoleCursorInfo(HConsole, &CursorInfo);
}

WORD getcolor()
{
    CONSOLE_SCREEN_BUFFER_INFOEX Infos;
    HANDLE hConsole;

    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleInfos(hConsole, &Infos);

    return Infos.wAttributes & 0xFF;
}

void setcolor(WORD color)
{
    HANDLE hConsole;
    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    SetConsoleTextAttribute(hConsole, color);
}

void textcolor(const WORD color)
{
    CONSOLE_SCREEN_BUFFER_INFOEX Infos;
    HANDLE hConsole;

    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleInfos(hConsole, &Infos);

    SetConsoleTextAttribute(hConsole, (color & 0x0F) | (Infos.wAttributes & 0xF0));
}

void textbackground(const WORD color)
{
    CONSOLE_SCREEN_BUFFER_INFOEX Infos;
    HANDLE hConsole;

    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);
    GetConsoleInfos(hConsole, &Infos);

    SetConsoleTextAttribute(hConsole, ((color & 0x0F) << 4) | (Infos.wAttributes & 0x0F));
}

BOOL textmode(const int x, const int y)
{
    HANDLE hConsole;
    SMALL_RECT ConsoleWindow;
    COORD coords = { x, y };

    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

    ConsoleWindow.Top = 0; ConsoleWindow.Left = 0;
    ConsoleWindow.Bottom = coords.Y - 1;
    ConsoleWindow.Right = coords.X - 1;

    SetConsoleScreenBufferSize(hConsole, coords);
    Sleep(40); // This sleep is needed
    SetConsoleWindowInfo(hConsole, 1, &ConsoleWindow);
    Sleep(40); // This sleep is needed
    return SetConsoleScreenBufferSize(hConsole, coords);
}
//

const char* XInputDevTypeToStr(BYTE Type)
{
    switch (Type)
    {
        case XINPUT_DEVTYPE_GAMEPAD: return "GAMEPAD";
    }
    return "UNKNOWN";
}

const char* XInputDevSubTypeToStr(BYTE SubType)
{
    switch (SubType)
    {
        case XINPUT_DEVSUBTYPE_GAMEPAD         : return "GAMEPAD";
        case XINPUT_DEVSUBTYPE_WHEEL           : return "WHEEL";
        case XINPUT_DEVSUBTYPE_ARCADE_STICK    : return "ARCADE_STICK";
        case XINPUT_DEVSUBTYPE_FLIGHT_STICK    : return "FLIGHT_STICK";
        case XINPUT_DEVSUBTYPE_DANCE_PAD       : return "DANCE_PAD";
        case XINPUT_DEVSUBTYPE_GUITAR          : return "GUITAR";
        case XINPUT_DEVSUBTYPE_GUITAR_ALTERNATE: return "GUITAR_ALTERNATE";
        case XINPUT_DEVSUBTYPE_DRUM_KIT        : return "DRUM_KIT";
        case XINPUT_DEVSUBTYPE_GUITAR_BASS     : return "GUITAR_BASS";
        case XINPUT_DEVSUBTYPE_ARCADE_PAD      : return "ARCADE_PAD";
    }

    return "UNKNOWN";
}

const char* XInputBatteryTypeToStr(BYTE BatteryType)
{
    switch (BatteryType)
    {
        case BATTERY_TYPE_DISCONNECTED: return "DISCONNECTED";
        case BATTERY_TYPE_WIRED       : return "WIRED";
        case BATTERY_TYPE_ALKALINE    : return "ALKALINE";
        case BATTERY_TYPE_NIMH        : return "NI/MH";
        case BATTERY_TYPE_UNKNOWN     : return "UNKNOWN";
    }

    return "UNKNOWN";
}

const char* XInputBatteryLevelToStr(BYTE BatteryLevel)
{
    switch (BatteryLevel)
    {
        case BATTERY_LEVEL_EMPTY : return "EMPTY";
        case BATTERY_LEVEL_LOW   : return "LOW";
        case BATTERY_LEVEL_MEDIUM: return "MEDIUM";
        case BATTERY_LEVEL_FULL  : return "FULL";
    }
    return "UNKNOWN";
}

void print_state(XINPUT_STATE const& state)
{
    std::cout << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP)        << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN)      << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT)      << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT)     << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_START)          << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_BACK)           << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB)     << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_THUMB)    << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER)  << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_GUIDE)          << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_A)              << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_B)              << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_X)              << ' '
              << (bool)(state.Gamepad.wButtons & XINPUT_GAMEPAD_Y)              << ' ';
    std::cout << std::endl;
}

const char* print_keystroke_flags(WORD flags)
{
    switch (flags)
    {
        case XINPUT_KEYSTROKE_KEYDOWN                            : return "KEYDOWN";
        case XINPUT_KEYSTROKE_KEYUP                              : return "KEYUP";
        case XINPUT_KEYSTROKE_REPEAT                             : return "KEYREPEAT";
        case (XINPUT_KEYSTROKE_KEYDOWN | XINPUT_KEYSTROKE_KEYUP) : return "KEYDOWN|KEYUP";
        case (XINPUT_KEYSTROKE_KEYDOWN | XINPUT_KEYSTROKE_REPEAT): return "KEYDOWN|KEYREPEAT";
        case (XINPUT_KEYSTROKE_KEYUP | XINPUT_KEYSTROKE_REPEAT)  : return "KEYUP|KEYREPEAT";
        case (XINPUT_KEYSTROKE_KEYDOWN | XINPUT_KEYSTROKE_KEYUP | XINPUT_KEYSTROKE_REPEAT)  : return "KEYDOWN|KEYUP|KEYREPEAT";
    }
    
    return "NONE";
}

void print_keystroke(XINPUT_KEYSTROKE const& ks)
{
    switch (ks.VirtualKey)
    {
        case VK_PAD_A               : std::cout << "VK_PAD_A         : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_B               : std::cout << "VK_PAD_B         : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_X               : std::cout << "VK_PAD_X         : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_Y               : std::cout << "VK_PAD_Y         : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RSHOULDER       : std::cout << "VK_PAD_R1        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LSHOULDER       : std::cout << "VK_PAD_L1        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTRIGGER        : std::cout << "VK_PAD_L2        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTRIGGER        : std::cout << "VK_PAD_R2        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_DPAD_UP         : std::cout << "VK_PAD_UP        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_DPAD_DOWN       : std::cout << "VK_PAD_DOWN      : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_DPAD_LEFT       : std::cout << "VK_PAD_LEFT      : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_DPAD_RIGHT      : std::cout << "VK_PAD_RIGHT     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_START           : std::cout << "VK_PAD_START     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_BACK            : std::cout << "VK_PAD_BACK      : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_PRESS    : std::cout << "VK_PAD_LT        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_PRESS    : std::cout << "VK_PAD_RT        : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_UP       : std::cout << "VK_PAD_LUP       : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_DOWN     : std::cout << "VK_PAD_LDOWN     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_RIGHT    : std::cout << "VK_PAD_LRIGHT    : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_LEFT     : std::cout << "VK_PAD_LLEFT     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_UPLEFT   : std::cout << "VK_PAD_LUPLEFT   : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_UPRIGHT  : std::cout << "VK_PAD_LUPRIGHT  : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_DOWNRIGHT: std::cout << "VK_PAD_LDOWNRIGHT: " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_LTHUMB_DOWNLEFT : std::cout << "VK_PAD_LDOWNLEFT : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_UP       : std::cout << "VK_PAD_RUP       : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_DOWN     : std::cout << "VK_PAD_RDOWN     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_RIGHT    : std::cout << "VK_PAD_RRIGHT    : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_LEFT     : std::cout << "VK_PAD_RLEFT     : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_UPLEFT   : std::cout << "VK_PAD_RUPLEFT   : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_UPRIGHT  : std::cout << "VK_PAD_RUPRIGHT  : " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_DOWNRIGHT: std::cout << "VK_PAD_RDOWNRIGHT: " << print_keystroke_flags(ks.Flags); break;
        case VK_PAD_RTHUMB_DOWNLEFT : std::cout << "VK_PAD_RDOWNLEFT : " << print_keystroke_flags(ks.Flags); break;
    }
    std::cout << std::endl;
}

void test_keystroke()
{
    XINPUT_KEYSTROKE state = {};

    int run = 3;

    std::cout << "Press BACK + START to exit." << std::endl;
    while (run)
    {
        if (XInputGetKeystroke(XUSER_INDEX_ANY, 0, &state) == ERROR_SUCCESS)
        {
            switch(state.VirtualKey)
            {
                case VK_PAD_BACK:
                    if (state.Flags & XINPUT_KEYSTROKE_KEYDOWN)
                        run &= ~1;
                    else
                        run |= 1;
                    break;

                case VK_PAD_START:
                    if (state.Flags & XINPUT_KEYSTROKE_KEYDOWN)
                        run &= ~2;
                    else
                        run |= 2;
                    break;
            }
            
            print_keystroke(state);
        }
    }
}

void BuildDeviceConsoleOutput(XInputDevice_t& device)
{
    // Status
    sprintf_s(device.consoleBuffer[DEVICE_STATUS]                     , "     Device %u %s !", device.deviceIndex, device.connected ? "connected" : "disconnected");
    // Battery
    sprintf_s(device.consoleBuffer[DEVICE_BATTERY_HEADER]             , "Device Battery Informations:");
    sprintf_s(device.consoleBuffer[DEVICE_BATTERY_TYPE]               , "  - Type     : %s", XInputBatteryTypeToStr(device.battery.BatteryType));
    sprintf_s(device.consoleBuffer[DEVICE_BATTERY_LEVEL]              , "  - Level    : %s", XInputBatteryLevelToStr(device.battery.BatteryLevel));
    // Capabilities
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_HEADER]          , "Device Capabilities:");
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_TYPE]            , "  - Type     : %s", XInputDevTypeToStr(device.capabilities.Capabilities.Type));
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_SUBTYPE]         , "  - SubType  : %s", XInputDevSubTypeToStr(device.capabilities.Capabilities.SubType));
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_FLAGS]           , "  - Flags    : 0x%04hx", device.capabilities.Capabilities.Flags);
    // Capabilities Ex
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_PRODUCTID]       , "  - ProductId: 0x%04hx", device.capabilities.ProductId);
    sprintf_s(device.consoleBuffer[DEVICE_CAPABILITY_VENDORID]        , "  - VendorId : 0x%04hx", device.capabilities.VendorId);
    // State
    sprintf_s(device.consoleBuffer[DEVICE_STATE_HEADER]               , "Device State:");
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_DPAD_UP]       , "  - UP       : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_UP) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_DPAD_DOWN]     , "  - DOWN     : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_DOWN) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_DPAD_LEFT]     , "  - LEFT     : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_LEFT) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_DPAD_RIGHT]    , "  - RIGHT    : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_DPAD_RIGHT) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_START]         , "  - START    : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_START) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_BACK]          , "  - BACK     : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_BACK) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_LEFT_THUMB]    , "  - LTHUMB   : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_THUMB) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_RIGHT_THUMB]   , "  - RTHUMB   : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_THUMB) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_LEFT_SHOULDER] , "  - LSHOULDER: %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_LEFT_SHOULDER) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_RIGHT_SHOULDER], "  - RSHOULDER: %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_RIGHT_SHOULDER) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_GUIDE]         , "  - GUIDE    : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_GUIDE) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_A]             , "  - A        : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_A) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_B]             , "  - B        : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_B) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_X]             , "  - X        : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_X) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_BUTTON_Y]             , "  - Y        : %u", (device.state.Gamepad.wButtons & XINPUT_GAMEPAD_Y) != 0);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_LEFTTRIGGER]          , "  - LTrigger : %hhu", device.state.Gamepad.bLeftTrigger);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_RIGHTTRIGGER]         , "  - RTrigger : %hhu", device.state.Gamepad.bRightTrigger);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_LEFTTHUMBX]           , "  - Left X   : %hd", device.state.Gamepad.sThumbLX);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_LEFTTHUMBY]           , "  - Left Y   : %hd", device.state.Gamepad.sThumbLY);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_RIGHTTHUMBX]          , "  - Right X  : %hd", device.state.Gamepad.sThumbRX);
    sprintf_s(device.consoleBuffer[DEVICE_STATE_RIGHTTHUMBY]          , "  - Right Y  : %hd", device.state.Gamepad.sThumbRY);
}

void PrintDeviceConsoleOutput(XInputDevice_t& device)
{
    HANDLE hConsole;
    hConsole = GetStdHandle(STD_OUTPUT_HANDLE);

    DWORD Written;

    COORD coord;
    coord.X = MAX_DEVICE_LINE_WIDTH * device.deviceIndex;

    WORD color = getcolor();

    for (int i = 0; i < DEVICE_MAX_CONSOLE_LINES; ++i)
    {
        coord.Y = i;
        FillConsoleOutputCharacterW(hConsole, ' ', MAX_DEVICE_LINE_WIDTH, coord, &Written);
        gotoxy(MAX_DEVICE_LINE_WIDTH * device.deviceIndex, i);
        switch (i)
        {
            case DEVICE_STATUS: textcolor(device.connected ? COLOR_GREEN : COLOR_LIGHTRED); break;
            default: textcolor(COLOR_GRAY); break;
        }
        printf(device.consoleBuffer[i]);
    }
    fflush(stdout);

    setcolor(color);
}

void OnDeviceInfoChange(XInputDevice_t& device)
{
    BuildDeviceConsoleOutput(device);
    PrintDeviceConsoleOutput(device);
    memcpy(&device.oldState.Gamepad, &device.state.Gamepad, sizeof(device.state.Gamepad));
}

void OnDeviceConnect(XInputDevice_t& device)
{
    device.connected = true;
    memset(&device.battery, 0, sizeof(device.battery));
    memset(&device.capabilities, 0, sizeof(device.capabilities));
    memset(&device.oldState, 0, sizeof(device.oldState));
    memset(&device.state, 0, sizeof(device.state));

    // When using a wireless controller, the battery infos are not ready right now.
    for (int i = 0; i < 50 && device.battery.BatteryType == BATTERY_TYPE_DISCONNECTED; ++i)
    {
        XInputGetBatteryInformation(device.deviceIndex, 0, &device.battery);
        std::this_thread::sleep_for(std::chrono::milliseconds(100));
    }

    XInputGetCapabilitiesEx(1, device.deviceIndex, XINPUT_FLAG_GAMEPAD, &device.capabilities);

    WCHAR render[300];
    UINT renderCount = 300;
    WCHAR capture[300];
    UINT captureCount = 300;

    XInputGetAudioDeviceIds(device.deviceIndex, render, &renderCount, capture, &captureCount);

    OnDeviceInfoChange(device);
}

void OnDeviceDisconnect(XInputDevice_t& device)
{
    device.connected = false;
    memset(&device.battery, 0, sizeof(device.battery));
    memset(&device.capabilities, 0, sizeof(device.capabilities));
    memset(&device.oldState, 0, sizeof(device.oldState));
    memset(&device.state, 0, sizeof(device.state));
    
    OnDeviceInfoChange(device);
}

#include <roapi.h>
#include <wrl.h>
#include <windows.gaming.input.h>
#include <cassert>

namespace WRL = Microsoft::WRL;
namespace AWF = ABI::Windows::Foundation;
namespace AWFC = ABI::Windows::Foundation::Collections;
namespace WGI = ABI::Windows::Gaming::Input;

using namespace Microsoft::WRL::Wrappers;

#pragma comment(lib, "runtimeobject.lib")

#include <list>
#include <mutex>

struct Gamepad_t
{
    WRL::ComPtr<WGI::IGamepad> gamepad;
    WGI::GamepadReading oldReading;
    WGI::GamepadReading Reading;
};

std::list<Gamepad_t> gamepads;

std::mutex gamepads_mutex;

HRESULT OnGamepadAdded(IInspectable*, WGI::IGamepad* pGamepad)
{
    std::lock_guard<std::mutex> lk(gamepads_mutex);
    for (auto it = gamepads.begin(); it != gamepads.end(); ++it)
    {
        if (it->gamepad.Get() == pGamepad)
            return S_OK;
    }

    gamepads.emplace_back(Gamepad_t{ pGamepad, {}, {} });

    return S_OK;
}

HRESULT OnGamepadRemoved(IInspectable*, WGI::IGamepad* pGamepad)
{
    std::lock_guard<std::mutex> lk(gamepads_mutex);
    for (auto it = gamepads.begin(); it != gamepads.end(); ++it)
    {
        if (it->gamepad.Get() == pGamepad)
        {
            gamepads.erase(it);
            break;
         }
    }

    return S_OK;
}

auto g_oGamepadAddedHandler = WRL::Callback<AWF::IEventHandler<WGI::Gamepad*>>(OnGamepadAdded);
auto g_oGamepadRemovedHandler = WRL::Callback<AWF::IEventHandler<WGI::Gamepad*>>(OnGamepadRemoved);

EventRegistrationToken	oOnControllerAddedToken;
EventRegistrationToken	oOnControllerRemovedToken;

void test_gaming_input()
{
    auto hr = RoInitialize(RO_INIT_MULTITHREADED);
    assert(SUCCEEDED(hr));

    WRL::ComPtr<WGI::IGamepadStatics> gamepadStatics;
    hr = RoGetActivationFactory(HStringReference(L"Windows.Gaming.Input.Gamepad").Get(), __uuidof(WGI::IGamepadStatics), &gamepadStatics);
    assert(SUCCEEDED(hr));

    oOnControllerAddedToken = { 0 };
    oOnControllerRemovedToken = { 0 };

    gamepadStatics->add_GamepadAdded(g_oGamepadAddedHandler.Get(), &oOnControllerAddedToken);
    gamepadStatics->add_GamepadRemoved(g_oGamepadRemovedHandler.Get(), &oOnControllerRemovedToken);

    while (1)
    {
        {
            std::lock_guard<std::mutex> lk(gamepads_mutex);
            for (auto& gamepad : gamepads)
            {
                
                hr = gamepad.gamepad->GetCurrentReading(&gamepad.Reading);
                if (memcmp(&gamepad.oldReading, &gamepad.Reading, sizeof(WGI::GamepadReading)) != 0)
                {
                    gamepad.oldReading = gamepad.Reading;
                }
            }
        }
    }

    gamepadStatics->remove_GamepadAdded(oOnControllerAddedToken);
    gamepadStatics->remove_GamepadRemoved(oOnControllerRemovedToken);

}

int main(int argc, char* argv[])
{
    test_gaming_input();

    HMODULE hXinput = GetModuleHandleW(L"Xinput1_4.dll");
    if (hXinput != NULL)
    {
        XInputGetMaxControllerCount_t* pfnXInputGetMaxControllerCount_t = (XInputGetMaxControllerCount_t*)GetProcAddress(hXinput, "XInputGetMaxControllerCount");
        if (pfnXInputGetMaxControllerCount_t != nullptr)
        {// We are using OpenXinput, check for the max controller count.
            XinputMaxControllerCount = pfnXInputGetMaxControllerCount_t();
        }
    }

    std::unique_ptr<XInputDevice_t[]> devices = std::make_unique<XInputDevice_t[]>(XinputMaxControllerCount);
    memset(devices.get(), 0, sizeof(XInputDevice_t) * XinputMaxControllerCount);

    setcursortype(0);
    textmode(MAX_DEVICE_LINE_WIDTH * XinputMaxControllerCount, DEVICE_MAX_CONSOLE_LINES);

    for (int i = 0; i < XinputMaxControllerCount; ++i)
    {
        devices[i].deviceIndex = i;
        BuildDeviceConsoleOutput(devices[i]);
        PrintDeviceConsoleOutput(devices[i]);
    }

    while (1)
    {
        for (int i = 0; i < XinputMaxControllerCount; ++i)
        {
            XInputDevice_t& device = devices[i];
            if (XInputGetStateEx(i, &device.state) == ERROR_SUCCESS)
            {
                if (device.connected == false)
                {
                    OnDeviceConnect(device);
                }
                else if(memcmp(&device.oldState.Gamepad, &device.state.Gamepad, sizeof(device.state.Gamepad)) != 0)
                {
                    OnDeviceInfoChange(device);
                }
            }
            else if(device.connected)
            {
                OnDeviceDisconnect(device);
            }
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(20));
    }

    return 0;
}
