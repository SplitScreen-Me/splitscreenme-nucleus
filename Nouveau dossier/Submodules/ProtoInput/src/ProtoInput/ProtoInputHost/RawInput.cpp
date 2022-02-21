#include "RawInput.h"
#include <windows.h>
#include <cstdio>
#include <memory>
#include <vector>
#include <hidusage.h>
#include "protoinpututil.h"
#include "Gui.h"

namespace ProtoHost
{

intptr_t lastKeypressKeyboardHandle = -1;
intptr_t lastMouseClicked = -1;

bool lockInputWithTheEndKey = true;
bool lockInputSuspendsExplorer = true;
bool freezeGameInputWhileInputNotLocked = true;

HWND rawInputHwnd;

std::vector<void*> keyboardHandles{};
std::vector<void*> mouseHandles{};

void ProcessRawInput(HRAWINPUT rawInputHandle)
{
	RAWINPUT rawinput;
	UINT cbSize;

	if (0 != GetRawInputData(rawInputHandle, RID_INPUT, nullptr, &cbSize, sizeof(RAWINPUTHEADER)))
		return;

	if (cbSize > sizeof(RAWINPUT))
		return;
	
	if (cbSize != GetRawInputData(rawInputHandle, RID_INPUT, &rawinput, &cbSize, sizeof(RAWINPUTHEADER)))
		return;
		
	if (rawinput.header.dwType == RIM_TYPEKEYBOARD &&
		rawinput.data.keyboard.Flags == RI_KEY_MAKE)
	{
		lastKeypressKeyboardHandle = (intptr_t)rawinput.header.hDevice;
	}
	else if (rawinput.header.dwType == RIM_TYPEMOUSE &&
			 rawinput.data.mouse.usButtonFlags != 0)
	{
		lastMouseClicked = (intptr_t)rawinput.header.hDevice;
	}

	if (lockInputWithTheEndKey && 
		rawinput.header.dwType == RIM_TYPEKEYBOARD && 
		rawinput.data.keyboard.VKey == VK_END && 
		rawinput.data.keyboard.Message == WM_KEYUP)
	{
		static bool locked = false;
		locked = !locked;
		
		LockInput(locked);

		if (locked)
		{
			if (lockInputSuspendsExplorer)
				SuspendExplorer();
		}
		else
			RestartExplorer();

		OnInputLockChange(locked);
	}

	// if (lockInputWithTheEndKey &&
	// 	rawinput.header.dwType == RIM_TYPEKEYBOARD &&
	// 	rawinput.data.keyboard.VKey == VK_PRIOR &&
	// 	rawinput.data.keyboard.Message == WM_KEYUP)
	// {
	// 	SuspendExplorer();
	// }
	//
	// if (lockInputWithTheEndKey &&
	// 	rawinput.header.dwType == RIM_TYPEKEYBOARD &&
	// 	rawinput.data.keyboard.VKey == VK_NEXT &&
	// 	rawinput.data.keyboard.Message == WM_KEYUP)
	// {
	// 	RestartExplorer();
	// }
}
	
LRESULT WINAPI RawInputWindowWndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
	switch (msg)
	{
		case WM_DESTROY:
		{
			printf("Raw input window posting quit message\n");
			PostQuitMessage(0);
			return 0;
		}
	}
	return DefWindowProc(hWnd, msg, wParam, lParam);
}

DWORD WINAPI RawInputWindowThread(LPVOID lpParameter)
{
	const auto hinstance = GetModuleHandle(nullptr);

	WNDCLASS wc = { 0 };
	wc.lpfnWndProc = RawInputWindowWndProc;
	wc.hInstance = hinstance;
	wc.hbrBackground = (HBRUSH)(COLOR_BACKGROUND);
	const wchar_t* className = L"PROTOHOSTRAWINPUT";
	wc.lpszClassName = className;
	wc.style = CS_OWNDC;

	if (RegisterClass(&wc))
	{
		rawInputHwnd = CreateWindowW(
			wc.lpszClassName,
			L"Proto Host: Raw Input window",
			0,
			0, 0, 300, 300,
			nullptr, nullptr,
			hinstance,
			nullptr);

		UpdateWindow(rawInputHwnd);

		//TODO: cleanup window
		// DestroyWindow(hwnd);
		// UnregisterClassW(className, wc.hInstance);
	}

	MSG msg;
	ZeroMemory(&msg, sizeof(msg));
	while (msg.message != WM_QUIT)
	{		
		if (GetMessage(&msg, rawInputHwnd, WM_INPUT, WM_INPUT))
		{
			ProcessRawInput((HRAWINPUT)msg.lParam);
			
			TranslateMessage(&msg);
			DispatchMessage(&msg);
			continue;
		}
	}

	return 0;
}

void RefreshDevices()
{
	unsigned int numDevices = 0;
	GetRawInputDeviceList(NULL, &numDevices, sizeof(RAWINPUTDEVICELIST));
	auto deviceArray = std::make_unique<RAWINPUTDEVICELIST[]>(numDevices);
	GetRawInputDeviceList(&deviceArray[0], &numDevices, sizeof(RAWINPUTDEVICELIST));

	keyboardHandles.clear();
	mouseHandles.clear();

	for (unsigned int i = 0; i < numDevices; ++i)
	{
		auto* device = &deviceArray[i];

		if (device->dwType == RIM_TYPEKEYBOARD)
		{
			// printf("Found keyboard %d\n", device->hDevice);
			keyboardHandles.push_back(device->hDevice);
		}
		else if (device->dwType == RIM_TYPEMOUSE)
		{
			// printf("Found mouse %d\n", device->hDevice);
			mouseHandles.push_back(device->hDevice);
		}
	}

	// Device handle 0
	keyboardHandles.push_back(0);
	mouseHandles.push_back(0);
}

void RegisterProtoHostForRawInput()
{
	int tryCount = 0;

	do
	{
		if (rawInputHwnd == nullptr)
		{
			Sleep(500);
		}
	}
	while (tryCount++ < 20);

	if (rawInputHwnd == nullptr)
	{
		fprintf(stderr, "Raw input window hasn't opened. Giving up registering raw input\n");
		return;
	}


	RAWINPUTDEVICE dev[2];
	
	dev[0].usUsagePage = HID_USAGE_PAGE_GENERIC;
	dev[0].usUsage = HID_USAGE_GENERIC_KEYBOARD;
	dev[0].dwFlags = RIDEV_INPUTSINK;
	dev[0].hwndTarget = rawInputHwnd;

	dev[1].usUsagePage = HID_USAGE_PAGE_GENERIC;
	dev[1].usUsage = HID_USAGE_GENERIC_MOUSE;
	dev[1].dwFlags = RIDEV_INPUTSINK;
	dev[1].hwndTarget = rawInputHwnd;


	if (RegisterRawInputDevices(&dev[0], sizeof(dev)/sizeof(RAWINPUTDEVICE), sizeof(RAWINPUTDEVICE)) == FALSE)
	{
		fprintf(stderr, "Failed to register raw input, last error = 0x%X\n", GetLastError());
	}
}

void InitialiseRawInput()
{
	RefreshDevices();

	HANDLE hThread = CreateThread(nullptr, 0,
								  (LPTHREAD_START_ROUTINE)RawInputWindowThread, GetModuleHandle(nullptr), 0, 0);
	if (hThread != nullptr)
		CloseHandle(hThread);

	RegisterProtoHostForRawInput();
}

}
