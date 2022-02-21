#include "stdafx.h"
#include "Common.h"
#include "Logger.h"
#include "Utils.h"

#include "Globals.h"

#include "Config.h"

#include "ControllerManager.h"
#include "InputHookManager.h"
#include "XInputModuleManager.h"

#include <hidsdi.h>
#include <atlstr.h>  
#include <dinputd.h>

HANDLE tmpHandle;

//typedef NTSTATUS(WINAPI* HidP_GetButtonCapsProc)(_In_ HIDP_REPORT_TYPE, _Out_ PHIDP_BUTTON_CAPS, _Inout_ PUSHORT, _In_ PHIDP_PREPARSED_DATA);
//typedef NTSTATUS(WINAPI* HidP_GetUsagesProc)(_In_    HIDP_REPORT_TYPE, _In_    USAGE, _In_    USHORT, _Out_   PUSAGE, _Inout_ PULONG, _In_    PHIDP_PREPARSED_DATA, _Out_   PCHAR, _In_    ULONG);
//typedef NTSTATUS(WINAPI* HidP_GetUsageValueProc)(_In_  HIDP_REPORT_TYPE, _In_  USAGE, _In_  USHORT, _In_  USAGE, _Out_ PULONG, _In_  PHIDP_PREPARSED_DATA, _In_  PCHAR, _In_  ULONG);
//typedef NTSTATUS(WINAPI* HidP_GetDataProc)(_In_    HIDP_REPORT_TYPE, _Out_   PHIDP_DATA, _Inout_ PULONG, _In_    PHIDP_PREPARSED_DATA, _In_    PCHAR, _In_    ULONG);
//typedef void (WINAPI* HidD_GetHidGuidProc)(_Out_ LPGUID HidGuid);
//typedef HANDLE(WINAPI* CreateFileProc)(_In_     LPCTSTR, _In_     DWORD, _In_     DWORD, _In_opt_ LPSECURITY_ATTRIBUTES, _In_     DWORD, _In_     DWORD, _In_opt_ HANDLE);
//typedef int(WINAPI* CHid_GetDeviceStateProc)(int*, void*);
//typedef int(WINAPI* CDIDev_CreateDeviceProc)(int*, int, void*, void*);
//typedef signed int(__thiscall* GetDeviceInfoProc)(void*, LPDIDEVICEINSTANCE pdidi);
//LPDIENUMDEVICEOBJECTSCALLBACK
typedef signed int(__stdcall* CDIObj_EnumObjectsWProc)(int, LPDIENUMDEVICEOBJECTSCALLBACK, int, unsigned int);
typedef signed int(__stdcall* CDIObj_CreateDeviceWProc)(int, REFGUID, LPDIRECTINPUTDEVICE, LPUNKNOWN);
typedef int(_stdcall* CDIDev_SetCooperativeLevelProc)(LPDIRECTINPUTDEVICE, HWND, int);
typedef signed int(__fastcall* JoyReg_GetConfigProc)(unsigned int, UINT, LPDIJOYCONFIG, DWORD);
typedef HRESULT(__thiscall* CDIDev_GetDeviceStateProc)(void*, IDirectInputDevice2W*, int, int);
typedef HRESULT(__thiscall* CDIDev_PollProc)(void*);
typedef HRESULT(__stdcall* CDIDev_SetDataFormatProc)(int*, int);
typedef HRESULT(__stdcall* DirectInputCreateWProc)(HINSTANCE, DWORD, LPDIRECTINPUTW*, LPUNKNOWN);

//HidP_GetButtonCapsProc TrueHidP_GetButtonCaps = nullptr;
//HidP_GetUsagesProc TrueHidP_GetUsages = nullptr;
//HidP_GetUsageValueProc TrueHidP_GetUsageValue = nullptr;
//HidP_GetDataProc TrueHidP_GetData = nullptr;
//HidD_GetHidGuidProc TrueHidD_GetHidGuidProc = nullptr;
//CreateFileProc TrueCreateFileProc = nullptr;
//CHid_GetDeviceStateProc TrueCHid_GetDeviceState = nullptr;
//CDIDev_CreateDeviceProc TrueCDIDev_CreateDeviceW = nullptr;
//GetDeviceInfoProc TrueGetDeviceInfo = nullptr;
CDIObj_EnumObjectsWProc TrueCDIObj_EnumObjectsW = nullptr;
CDIObj_CreateDeviceWProc TrueCDIObj_CreateDeviceW = nullptr;
CDIDev_GetDeviceStateProc TrueCDIDev_GetDeviceState = nullptr;
JoyReg_GetConfigProc TrueJoyReg_GetConfig = nullptr;
CDIDev_SetCooperativeLevelProc TrueCDIDev_SetCooperativeLevel = nullptr;
CDIDev_SetDataFormatProc TrueCDIDev_SetDataFormat = nullptr;
CDIDev_PollProc TrueCDIDev_Poll = nullptr;
DirectInputCreateWProc TrueDirectInputCreateW = nullptr;

// dinput.dll hooks
HRESULT HookDirectInputCreateW(HINSTANCE hInst, DWORD dwVersion, LPDIRECTINPUTW* ppDI, LPUNKNOWN punkOuter)
{
	return DIERR_INVALIDPARAM;
}

//NTSTATUS __stdcall HookHidP_GetButtonCaps(_In_    HIDP_REPORT_TYPE     ReportType,
//	_Out_   PHIDP_BUTTON_CAPS    ButtonCaps,
//	_Inout_ PUSHORT              ButtonCapsLength,
//	_In_    PHIDP_PREPARSED_DATA PreparsedData)
//{
//	PrintLog("HidP_GetButtonCaps");
//	return TrueHidP_GetButtonCaps(ReportType, ButtonCaps, ButtonCapsLength, PreparsedData);
//}
//
//NTSTATUS __stdcall HookHidP_GetUsages(_In_    HIDP_REPORT_TYPE     ReportType,
//	_In_    USAGE                UsagePage,
//	_In_    USHORT               LinkCollection,
//	_Out_   PUSAGE               UsageList,
//	_Inout_ PULONG               UsageLength,
//	_In_    PHIDP_PREPARSED_DATA PreparsedData,
//	_Out_   PCHAR                Report,
//	_In_    ULONG                ReportLength)
//{
//	PrintLog("HidP_GetUsages");
//	return TrueHidP_GetUsages(ReportType, UsagePage, LinkCollection, UsageList, UsageLength, PreparsedData, Report, ReportLength);
//}
//
//NTSTATUS __stdcall HookHidP_GetUsageValue(_In_  HIDP_REPORT_TYPE     ReportType,
//	_In_  USAGE                UsagePage,
//	_In_  USHORT               LinkCollection,
//	_In_  USAGE                Usage,
//	_Out_ PULONG               UsageValue,
//	_In_  PHIDP_PREPARSED_DATA PreparsedData,
//	_In_  PCHAR                Report,
//	_In_  ULONG                ReportLength)
//{
//	PrintLog("HidP_GetUsageValue");
//	return TrueHidP_GetUsageValue(ReportType, UsagePage, LinkCollection, Usage, UsageValue, PreparsedData, Report, ReportLength);
//}
//
//NTSTATUS __stdcall HookHidP_GetData(_In_ HIDP_REPORT_TYPE ReportType,
//	_Out_   PHIDP_DATA           DataList,
//	_Inout_ PULONG               DataLength,
//	_In_    PHIDP_PREPARSED_DATA PreparsedData,
//	_In_    PCHAR                Report,
//	_In_    ULONG                ReportLength)
//{
//	//PrintLog("HidP_GetData");
//	return TrueHidP_GetData(ReportType, DataList, DataLength, PreparsedData, Report, ReportLength);
//}
//
//HANDLE WINAPI HookCreateFile(_In_  LPCTSTR lpFileName,
//	_In_     DWORD                 dwDesiredAccess,
//	_In_     DWORD                 dwShareMode,
//	_In_opt_ LPSECURITY_ATTRIBUTES lpSecurityAttributes,
//	_In_     DWORD                 dwCreationDisposition,
//	_In_     DWORD                 dwFlagsAndAttributes,
//	_In_opt_ HANDLE                hTemplateFile)
//{
//	//PrintLog("CreateFile " + CStringA(lpFileName));
//	return TrueCreateFileProc(lpFileName, dwDesiredAccess, dwShareMode, lpSecurityAttributes, dwCreationDisposition, dwFlagsAndAttributes, hTemplateFile);
//}
//
//int HookCHid_GetDeviceState(int* a, void* dst)
//{
//	PrintLog(("HookCHid_GetDeviceState A:" + std::to_string((DWORD)a)).c_str());
//	return TrueCHid_GetDeviceState(a, dst);
//}
typedef HWND(WINAPI* GetForegroundProc)(void);
typedef bool(WINAPI* SetForegroundProc)(HWND);
typedef HWND(WINAPI* GetHwndProc)(void);
typedef HWND(WINAPI* SetActiveWindowProc)(HWND);
typedef BOOL(WINAPI* SetCursorPosProc)(int, int);
typedef BOOL(WINAPI* GetCursorPosProc)(LPPOINT);
typedef HWND(WINAPI* SetWindowPosProc)(HWND, HWND, int, int, int, int, UINT);
typedef long(WINAPI* GetWindowLongWProc)(HWND, int);
typedef LONG(WINAPI* SetWindowLongWProc)(HWND, int, LONG);
typedef BOOL(WINAPI* ClipCursorProc)(RECT*);

WNDPROC TrueWndProc = nullptr;
HWND GameHWND = nullptr;
GetForegroundProc TrueGetForegroundWindow = nullptr;
SetForegroundProc TrueSetForegroundWindow = nullptr;
GetHwndProc TrueGetActiveWindow = nullptr;
SetActiveWindowProc TrueSetActiveWindow = nullptr;
SetWindowPosProc TrueSetWindowPos = nullptr;
SetCursorPosProc TrueSetCursorPos = nullptr;
GetCursorPosProc TrueGetCursorPos = nullptr;
GetHwndProc TrueGetFocus = nullptr;
GetWindowLongWProc TrueGetWindowLongW = nullptr;
SetWindowLongWProc TrueSetWindowLongW = nullptr;
ClipCursorProc TrueClipCursor = nullptr;

extern "C" long WINAPI HookGetWindowLongW(HWND wnd, int nIndex)
{
	if (wnd == GameHWND)
	{
		return 0;
	}
	long result = TrueGetWindowLongW(wnd, nIndex);
	return result;
}

extern "C" long WINAPI HookSetWindowLongW(HWND wnd, int nIndex, LONG dwNewLong)
{
	return TrueSetWindowLongW(wnd, nIndex, dwNewLong);
}

int SetX = 0;
int SetY = 0;

extern "C" BOOL WINAPI HookSetCursorPos(int x, int y)
{
	SetX = x;
	SetY = y;
	return 1;
}

extern "C" BOOL WINAPI HookGetCursorPos(LPPOINT pt)
{
	pt->x = SetX;
	pt->y = SetY;
	return 1;
}

extern "C" BOOL WINAPI HookClipCursor(RECT* lpRect)
{
	return true;
}

extern "C" HWND WINAPI HookGetForegroundWindow()
{
	return GameHWND;
}

extern "C" bool WINAPI HookSetForegroundWindow(HWND hWnd)
{
	return true;
}

extern "C" HWND WINAPI HookGetActiveWindow()
{
	return GameHWND;
}

extern "C" HWND WINAPI HookGetFocus()
{
	return GameHWND;
}

extern "C" HWND WINAPI HookSetActiveWindow(HWND hWnd)
{
	return NULL;
}

extern "C" bool WINAPI HookSetWindowPos(_In_ HWND hWnd, _In_opt_  HWND insertAfter, _In_ int x, _In_ int y, _In_ int cx, _In_ int cy, _In_ UINT uflags)
{
	return TrueSetWindowPos(hWnd, insertAfter, x, y, cx, cy, uflags);
}

struct EnumWindowsCallbackArgs {
	EnumWindowsCallbackArgs(DWORD p) : pid(p) { }
	const DWORD pid;
	std::vector<HWND> handles;
};

static BOOL CALLBACK EnumWindowsCallback(HWND hnd, LPARAM lParam)
{
	EnumWindowsCallbackArgs *args = (EnumWindowsCallbackArgs *)lParam;

	DWORD windowPID;
	(void)::GetWindowThreadProcessId(hnd, &windowPID);
	if (windowPID == args->pid) {
		args->handles.push_back(hnd);
	}

	return TRUE;
}

std::vector<HWND> getToplevelWindows()
{
	EnumWindowsCallbackArgs args(::GetCurrentProcessId());
	if (::EnumWindows(&EnumWindowsCallback, (LPARAM)&args) == FALSE) {
		// XXX Log error here
		return std::vector<HWND>();
	}
	return args.handles;
}

LPDIENUMDEVICEOBJECTSCALLBACK tempCallback;

extern "C" bool __stdcall HookCDIObj_EnumObjectsWCallback(DIDEVICEOBJECTINSTANCEW* instance, void* function)
{
	return DIENUM_CONTINUE;
}

extern "C" signed int __stdcall HookCDIObj_EnumObjectsW(int a1, LPDIENUMDEVICEOBJECTSCALLBACK callback, int a3, unsigned int a4)
{
	tempCallback = callback;
	int retValue = TrueCDIObj_EnumObjectsW(a1, (LPDIENUMDEVICEOBJECTSCALLBACK)HookCDIObj_EnumObjectsWCallback, a3, a4);
	tempCallback = nullptr;

	return retValue;
}

LPDIRECTINPUTDEVICE DevicePtr;

extern "C" signed int __stdcall HookCDIObj_CreateDeviceW_Disabled(int a1, REFGUID rguid, LPDIRECTINPUTDEVICE device, LPUNKNOWN pUnkOuter)
{
	return DIERR_OUTOFMEMORY;
}

extern "C" signed int __stdcall HookCDIObj_CreateDeviceW(int a1, REFGUID rguid, LPDIRECTINPUTDEVICE device, LPUNKNOWN pUnkOuter)
{
	GUID iguid = rguid;
	if (iguid == Globals::dInputPlayerGuid && GameHWND != NULL)
	{
		int result = TrueCDIObj_CreateDeviceW(a1, rguid, device, pUnkOuter);
		DevicePtr = device;

		//IDirectInputDevice2W* test = (IDirectInputDevice2W*)device;
		//Sleep(100);
		//HRESULT res = device->SetCooperativeLevel(GameHWND, DISCL_BACKGROUND | DISCL_EXCLUSIVE);
		//TrueCDIDev_SetCooperativeLevel(device, GameHWND, DISCL_BACKGROUND | DISCL_EXCLUSIVE);

		return result;
	}
	else
	{
		// noooope we dont allow you to access this gamepad
		return DIERR_OUTOFMEMORY;
	}
}

extern "C" signed int __fastcall HookJoyReg_GetConfig_Disabled(unsigned int a1, UINT uiJoy, LPDIJOYCONFIG pjc, DWORD dwFlags)
{
	return DIERR_NOMOREITEMS;
}

extern "C" signed int __fastcall HookJoyReg_GetConfig(unsigned int a1, UINT uiJoy, LPDIJOYCONFIG pjc, DWORD dwFlags)
{
	return DIERR_NOMOREITEMS;

	if (a1 == 0)
	{
		// How this works:
		// On DirectInput devices, there's a freakin hidden property that is the primary gamepad.
		// That gamepad is always in the 0 ID.
		// So when the application requests our gamepad ID, we only show the first
		for (int i = 0;; i++)
		{
			signed int result = TrueJoyReg_GetConfig(i, uiJoy, pjc, dwFlags);
			if (result == DIERR_NOMOREITEMS)
			{
				break;
			}

			if (pjc->guidInstance == Globals::dInputPlayerGuid)
			{
				return result;
			}
		}
	}
	else
	{
		return DIERR_NOMOREITEMS;
	}
}

bool SetGamepad = false;

HRESULT __fastcall HookCDIDev_GetDeviceState(void* This, void* notUsed, IDirectInputDevice2W* pDevice, int cbData, int lpvData)
{
	if (!SetGamepad)
	{
		DevicePtr = pDevice;

		// before setting, change the cooperative level
		pDevice->Unacquire();
		HRESULT result = pDevice->SetCooperativeLevel(GameHWND, DISCL_BACKGROUND | DISCL_EXCLUSIVE);

		SetGamepad = true;
	}

	DIDEVICEINSTANCEW instance;
	instance.dwSize = sizeof(DIDEVICEINSTANCEW);
	pDevice->GetDeviceInfo(&instance);

	GUID iguid = instance.guidInstance;
	if (iguid == Globals::dInputPlayerGuid)
	{
		return TrueCDIDev_GetDeviceState(This, pDevice, cbData, lpvData);
	}
	else
	{
		return -1;
	}
}

HRESULT __fastcall HookCDIDev_Poll(LPDIRECTINPUTDEVICE8 device, void* notUsed)
{
	return device->Acquire();
}


int __stdcall HookCDIDev_SetCooperativeLevel(LPDIRECTINPUTDEVICE a1, HWND hWnd, int a3)
{
	return TrueCDIDev_SetCooperativeLevel(a1, hWnd, a3);
}

HRESULT __fastcall HookCDIDev_SetDataFormat(int* a1, int a2, int a3, int a4, int a5, int a6, int a7, int a8, int a9)
{
	LPDIRECTINPUTDEVICE dev = (LPDIRECTINPUTDEVICE)a6;
	LPCDIDATAFORMAT data4 = (LPCDIDATAFORMAT)a4;
	return TrueCDIDev_SetDataFormat((int*)a6, a4);
}

LRESULT CALLBACK HookWndProc(HWND hWnd, UINT message, WPARAM wParam, LPARAM lParam)
{
	if (!Globals::hasSetWindow && Globals::hasHooked)
	{
		Globals::hasSetWindow = true;

		DWORD width = Globals::resWidth;
		DWORD height = Globals::resHeight;
		if (width > 32 && height > 32)
		{
			SetWindowPos(GameHWND, 0, 0, 0, width, height, SWP_NOZORDER | SWP_NOMOVE);
			PrintLog(("Set window size to: " + std::to_string(width) + "x" + std::to_string(height)).c_str());
		}

		DWORD x = Globals::windowX;
		DWORD y = Globals::windowY;

		if (x != 0 || y != 0)
		{
			SetWindowPos(GameHWND, 0, x, y, 0, 0, SWP_NOZORDER | SWP_NOSIZE);
			PrintLog(("Set window position to: " + std::to_string(x) + ":" + std::to_string(y)).c_str());
		}
	}

	switch (message)
	{
		//PrintLog(("Event: " + std::to_string(message)).c_str());
		//return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);

	case WM_INPUT: // raw mouse input?
		//DefWindowProc(hWnd, message, 0, lParam);
		if (Globals::blockInputEvents)
		{
			return 0;
		}
		else
		{
			return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);
		}

	case WM_NCACTIVATE:
		// Sent to a window when its nonclient area needs to be changed to indicate an active or inactive state.
		// Faked: wParam is always true, so the window always looks active
		CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);
		return 0;

	case WM_NCHITTEST:
		// System is checking if the mouse is inside the game screen, but we dont want
		// that call to ever be recognized by the game
		if (Globals::blockMouseEvents)
		{
			return 0;

	case WM_IME_SETCONTEXT:
		return 0;
		return CallWindowProc(TrueWndProc, hWnd, message, wParam, 0);
	case WM_IME_NOTIFY:
		return 0;// return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);

	case WM_ACTIVATE:
		// msdn: Sent to both the window being activated and the window being deactivated
		return 0;
		return CallWindowProc(TrueWndProc, hWnd, message, 1, NULL);

	case WM_SETFOCUS:
	case WM_WINDOWPOSCHANGING:
	case WM_WINDOWPOSCHANGED:
	case WM_KILLFOCUS:
	case 144:
	case 5374:
	case WM_CAPTURECHANGED:
	case WM_ACTIVATEAPP:
		//PrintLog(("Event blocked: " + std::to_string(message)).c_str());
		return 0;

		// Mouse and shit
	case WM_SETCURSOR:
	case WM_LBUTTONDOWN:
	case WM_LBUTTONUP:
	case WM_NCMOUSEMOVE:
	case WM_NCMOUSELEAVE:
	case WM_VSCROLL:
	case WM_VSCROLLCLIPBOARD:
	case WM_HSCROLL:
	case WM_NCMOUSEHOVER:
	case WM_MOUSEMOVE:
	case WM_NCLBUTTONDOWN:
	case WM_KEYDOWN:
	case WM_KEYUP:
	case WM_WTSSESSION_CHANGE:
	case WM_XBUTTONDBLCLK:
	case WM_XBUTTONDOWN:
	case WM_XBUTTONUP:
	case WM_LBUTTONDBLCLK:
	case WM_MBUTTONDBLCLK:
	case WM_MBUTTONDOWN:
	case WM_MBUTTONUP:
	case WM_MOUSEACTIVATE:
	case WM_MOUSEHOVER:
	case WM_MOUSELAST:
	case WM_MOUSELEAVE:
	case WM_MOUSEWHEEL:
	case WM_RBUTTONDBLCLK:
	case WM_RBUTTONDOWN:
	case WM_RBUTTONUP:
	case WM_NCLBUTTONDBLCLK:
	case WM_NCLBUTTONUP:
	case WM_NCMBUTTONDBLCLK:
	case WM_NCMBUTTONDOWN:
	case WM_NCMBUTTONUP:
	case WM_NCRBUTTONDBLCLK:
	case WM_NCRBUTTONDOWN:
	case WM_NCRBUTTONUP:
	case WM_NCXBUTTONDBLCLK:
	case WM_NCXBUTTONDOWN:
	case WM_NCXBUTTONUP:
	case WM_CHARTOITEM:
		if (Globals::blockMouseEvents)
		{
			return 0;
		}
		else
		{
			//#ifdef _DEBUG
			PrintLog(("Event: " + std::to_string(message)).c_str());
			//#endif
			return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);
		}

		// keyboard
	case WM_CHAR:
	case WM_IME_KEYDOWN:
	case WM_IME_KEYUP:
	case WM_SYSKEYDOWN:
	case WM_SYSKEYUP:
		if (Globals::blockKeyboardEvents)
		{
			return 0;
		}
		else
		{
			//ifdef _DEBUG
			PrintLog(("Event: " + std::to_string(message)).c_str());
			//#endif
			return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);
		}

	default:
	case WM_SIZE:
	case WM_SIZING:
	case WM_GETICON:
	case WM_SYSCOMMAND:
	case WM_GETMINMAXINFO:
	case WM_ENTERSIZEMOVE:
	case WM_MOVING:
	case WM_MOVE:
	case WM_EXITSIZEMOVE:
	case WM_CLOSE:
	case WM_DESTROY:
	case WM_NCDESTROY:
	case WM_CANCELMODE:
	case WM_ENABLE:
	case WM_AFXFIRST:
	case WM_AFXLAST:
	case WM_APP:
	case WM_APPCOMMAND:
	case WM_ASKCBFORMATNAME:
	case WM_CANCELJOURNAL:
	case WM_CHANGECBCHAIN:
	case WM_CHANGEUISTATE:
	case WM_CLEAR:
	case WM_COMMAND:
	case WM_COMMNOTIFY:
	case WM_COMPACTING:
	case WM_COMPAREITEM:
	case WM_CONTEXTMENU:
	case WM_COPY:
	case WM_COPYDATA:
	case WM_CTLCOLORBTN:
	case WM_CTLCOLORDLG:
	case WM_CTLCOLOREDIT:
	case WM_CTLCOLORLISTBOX:
	case WM_CTLCOLORMSGBOX:
	case WM_CTLCOLORSCROLLBAR:
	case WM_CTLCOLORSTATIC:
	case WM_CUT:
	case WM_DELETEITEM:
	case WM_DESTROYCLIPBOARD:
	case WM_DEVICECHANGE:
	case WM_DEVMODECHANGE:
	case WM_DISPLAYCHANGE:
	case WM_DRAWCLIPBOARD:
	case WM_DRAWITEM:
	case WM_DROPFILES:
	case WM_ENDSESSION:
	case WM_ENTERIDLE:
	case WM_ENTERMENULOOP:
	case WM_ERASEBKGND:
	case WM_EXITMENULOOP:
	case WM_FONTCHANGE:
	case WM_GETDLGCODE:
	case WM_GETFONT:
	case WM_GETHOTKEY:
	case WM_GETOBJECT:
	case WM_GETTEXT:
	case WM_GETTEXTLENGTH:
	case WM_HANDHELDFIRST:
	case WM_HANDHELDLAST:
	case WM_HELP:
	case WM_HOTKEY:
	case WM_HSCROLLCLIPBOARD:
	case WM_ICONERASEBKGND:
	case WM_IME_CHAR:
	case WM_IME_COMPOSITION:
	case WM_IME_COMPOSITIONFULL:
	case WM_IME_CONTROL:
	case WM_IME_ENDCOMPOSITION:

	case WM_IME_REQUEST:
	case WM_IME_SELECT:
	case WM_IME_STARTCOMPOSITION:
	case WM_INITDIALOG:
	case WM_INITMENU:
	case WM_INITMENUPOPUP:
	case WM_INPUTLANGCHANGE:
	case WM_INPUTLANGCHANGEREQUEST:
	case WM_KEYLAST:
	case WM_MDIACTIVATE:
	case WM_MDICASCADE:
	case WM_MDICREATE:
	case WM_MDIDESTROY:
	case WM_MDIGETACTIVE:
	case WM_MDIICONARRANGE:
	case WM_MDIMAXIMIZE:
	case WM_MDINEXT:
	case WM_MDIREFRESHMENU:
	case WM_MDIRESTORE:
	case WM_MDISETMENU:
	case WM_MDITILE:
	case WM_MEASUREITEM:
	case WM_MENUCHAR:
	case WM_MENUCOMMAND:
	case WM_MENUDRAG:
	case WM_MENUGETOBJECT:
	case WM_MENURBUTTONUP:
	case WM_MENUSELECT:
	case WM_NCCALCSIZE:
	case WM_NCCREATE:
	case WM_NCPAINT:
	case WM_NEXTDLGCTL:
	case WM_NEXTMENU:
	case WM_NOTIFY:
	case WM_NOTIFYFORMAT:
	case WM_NULL:
	case WM_PAINT:
	case WM_PAINTCLIPBOARD:
	case WM_PAINTICON:
	case WM_PALETTECHANGED:
	case WM_PALETTEISCHANGING:
	case WM_PARENTNOTIFY:
	case WM_PASTE:
	case WM_PENWINFIRST:
	case WM_PENWINLAST:
	case WM_POWER:
	case WM_POWERBROADCAST:
	case WM_PRINT:
	case WM_PRINTCLIENT:
	case WM_QUERYDRAGICON:
	case WM_QUERYENDSESSION:
	case WM_QUERYNEWPALETTE:
	case WM_QUERYOPEN:
	case WM_QUERYUISTATE:
	case WM_QUEUESYNC:
	case WM_RENDERALLFORMATS:
	case WM_RENDERFORMAT:
	case WM_SETFONT:
	case WM_SETHOTKEY:
	case WM_SETICON:
	case WM_SETREDRAW:
	case WM_SETTEXT:
	case WM_SETTINGCHANGE:
	case WM_SHOWWINDOW:
	case WM_SIZECLIPBOARD:
	case WM_SPOOLERSTATUS:
	case WM_STYLECHANGED:
	case WM_STYLECHANGING:
	case WM_SYNCPAINT:
	case WM_SYSCHAR:
	case WM_SYSCOLORCHANGE:
	case WM_SYSDEADCHAR:
	case WM_TABLET_FIRST:
	case WM_TABLET_LAST:
	case WM_TCARD:
	case WM_THEMECHANGED:
	case WM_TIMECHANGE:
	case WM_TIMER:
	case WM_UNDO:
	case WM_UNINITMENUPOPUP:
	case WM_UPDATEUISTATE:
	case WM_USER:
	case WM_USERCHANGED:
	case WM_VKEYTOITEM:
	case WM_DEADCHAR:
	case WM_CREATE:
	case WM_CHILDACTIVATE:
	case WM_QUIT:
	{
		//#ifdef _DEBUG
		PrintLog(("Event: " + std::to_string(message)).c_str());
		//#endif
		return CallWindowProc(TrueWndProc, hWnd, message, wParam, lParam);
	}

		}
	}
}

unsigned int ComputeLevenshteinDistance(const std::wstring& s1, const std::wstring& s2)
{
	const std::size_t len1 = s1.size(), len2 = s2.size();
	std::vector<std::vector<unsigned int>> d(len1 + 1, std::vector<unsigned int>(len2 + 1));

	d[0][0] = 0;
	for (unsigned int i = 1; i <= len1; ++i) d[i][0] = i;
	for (unsigned int i = 1; i <= len2; ++i) d[0][i] = i;

	for (unsigned int i = 1; i <= len1; ++i)
		for (unsigned int j = 1; j <= len2; ++j)
			// note that std::min({arg1, arg2, arg3}) works only in C++11,
			// for C++98 use std::min(std::min(arg1, arg2), arg3)
			d[i][j] = std::min({ d[i - 1][j] + 1, d[i][j - 1] + 1, d[i - 1][j - 1] + (s1[i - 1] == s2[j - 1] ? 0 : 1) });
	return d[len1][len2];
}

void HandleForceFocus()
{
	if (Globals::forceFocus)
	{
		std::wstring* wndName = Globals::forceFocusWindowName;
		if (wndName == nullptr)
		{
			return;
		}

		WCHAR* str = new WCHAR[50];
		memset(str, 0, sizeof(WCHAR) * 50);

		while (GameHWND == nullptr)
		{
			Sleep(2000);
			std::vector<HWND> windows = getToplevelWindows();
			size_t size = windows.size();

			for (int i = 0; i < size; i++)
			{
				HWND wnd = windows[i];
				if (wnd == nullptr)// || Globals::hasHooked)
				{
					continue;
				}

				GetWindowText(wnd, str, 50);

				std::wstring stStr = std::wstring(str);
				if (ComputeLevenshteinDistance(stStr, *wndName) < 2) // good enough
				{
					GameHWND = wnd;
					TrueWndProc = (WNDPROC)SetWindowLongPtr(GameHWND, GWL_WNDPROC, (LONG_PTR)&HookWndProc);
					if (TrueWndProc == nullptr)
					{
						PrintLog("!!!COULD NOT SET WINDOW PROCEDURE!!!");
					}
					else
					{
						PrintLog("SetWindowLongPtr", (long)TrueWndProc);
						PrintLog("Hooked to Game Window %u", (long)TrueWndProc);
					}
					HMODULE mod = LoadLibrary(L"user32");

					/*void* getWindowLongWPtr = GetProcAddress(mod, "GetWindowLongW");
					IH_CreateHook(getWindowLongWPtr, HookGetWindowLongW, reinterpret_cast<LPVOID*>(&TrueGetWindowLongW));
					IH_EnableHook(getWindowLongWPtr);*/

					void* getForegroundPtr = GetProcAddress(mod, "GetForegroundWindow");
					IH_CreateHook(getForegroundPtr, HookGetForegroundWindow, reinterpret_cast<LPVOID*>(&TrueGetForegroundWindow));
					IH_EnableHook(getForegroundPtr);

					void* setForegroundPtr = GetProcAddress(mod, "SetForegroundWindow");
					IH_CreateHook(setForegroundPtr, HookSetForegroundWindow, reinterpret_cast<LPVOID*>(&TrueSetForegroundWindow));
					IH_EnableHook(setForegroundPtr);

					void* setActivePtr = GetProcAddress(mod, "SetActiveWindow");
					IH_CreateHook(setActivePtr, HookSetActiveWindow, reinterpret_cast<LPVOID*>(&TrueSetActiveWindow));
					IH_EnableHook(setActivePtr);

					/*void* setWindowLongWPtr = GetProcAddress(mod, "SetWindowLongW");
					IH_CreateHook(setWindowLongWPtr, HookSetWindowLongW, reinterpret_cast<LPVOID*>(&TrueSetWindowLongW));
					IH_EnableHook(setWindowLongWPtr);*/

					void* getFocusPtr = GetProcAddress(mod, "GetFocus");
					IH_CreateHook(getFocusPtr, HookGetFocus, reinterpret_cast<LPVOID*>(&TrueGetFocus));
					IH_EnableHook(getFocusPtr);

					void* getActivePtr = GetProcAddress(mod, "GetActiveWindow");
					IH_CreateHook(getActivePtr, HookGetActiveWindow, reinterpret_cast<LPVOID*>(&TrueGetActiveWindow));
					IH_EnableHook(getActivePtr);

					if (!Globals::enableMKBInput)
					{
						void* setCursorPosPtr = GetProcAddress(mod, "SetCursorPos");
						IH_CreateHook(setCursorPosPtr, HookSetCursorPos, reinterpret_cast<LPVOID*>(&TrueSetCursorPos));
						IH_EnableHook(setCursorPosPtr);

						void* getCursorPosPtr = GetProcAddress(mod, "GetCursorPos");
						IH_CreateHook(getCursorPosPtr, HookGetCursorPos, reinterpret_cast<LPVOID*>(&TrueGetCursorPos));
						IH_EnableHook(getCursorPosPtr);

						void* clipCursorPtr = GetProcAddress(mod, "ClipCursor");
						IH_CreateHook(clipCursorPtr, HookClipCursor, reinterpret_cast<LPVOID*>(&TrueClipCursor));
						IH_EnableHook(clipCursorPtr);
					}

					PrintLog("Hooked to game window");
					Globals::hasHooked = true;
					//break;
				}
			}
		}

		delete[] str;
	}
}

DWORD WINAPI HookThread()
{
	if (Globals::dInputEnabled || Globals::dInputForceDisable)
	{
		HMODULE dinputModule = LoadLibrary(L"dinput");

		DWORD cDIDev_GetDeviceStateOffset = 0;
		DWORD cDIObj_CreateDeviceWOffset = 0;
		DWORD joyReg_GetConfigOffset = 0;
		if (Globals::dInputLibrary == 1)
		{
			PrintLog("DInput ID1");

			// windows 10 creators update
			cDIDev_GetDeviceStateOffset = -19504;
			cDIObj_CreateDeviceWOffset = 800;
			joyReg_GetConfigOffset = 43434;
		}
		else if (Globals::dInputLibrary == 2)
		{
			PrintLog("DInput ID2");

			// random windows 10 installation (from a friend's PC)
			cDIDev_GetDeviceStateOffset = -20032;
			cDIObj_CreateDeviceWOffset = 832;
			joyReg_GetConfigOffset = 44031;
		}
		else
		{
			PrintLog("DInput Unkwnown");
		}

		void* directInputCreateAPtr = GetProcAddress(dinputModule, "DirectInputCreateA");
		void* directInputCreateWPtr = GetProcAddress(dinputModule, "DirectInputCreateW");

		if (Globals::dInputForceDisable)
		{
			PrintLog("DInput Forced Disable");
			IH_CreateHook(directInputCreateWPtr, HookDirectInputCreateW, reinterpret_cast<LPVOID*>(&TrueDirectInputCreateW));
			IH_EnableHook(directInputCreateWPtr);

			if (Globals::dInputLibrary != 0)
			{
				// hook to remove all possible calls to DirectInput
				void* cDIObj_CreateDeviceWPtr = (void*)((DWORD)directInputCreateAPtr + cDIObj_CreateDeviceWOffset);
				void* joyReg_GetConfigPtr = (void*)((DWORD)directInputCreateAPtr + joyReg_GetConfigOffset);

				IH_CreateHook(cDIObj_CreateDeviceWPtr, HookCDIObj_CreateDeviceW_Disabled, reinterpret_cast<LPVOID*>(&TrueCDIObj_CreateDeviceW));
				IH_EnableHook(cDIObj_CreateDeviceWPtr);

				IH_CreateHook(joyReg_GetConfigPtr, HookJoyReg_GetConfig_Disabled, reinterpret_cast<LPVOID*>(&TrueJoyReg_GetConfig));
				IH_EnableHook(joyReg_GetConfigPtr);
			}
		}
		else if (Globals::dInputEnabled && Globals::dInputLibrary != 0)
		{

#ifdef NOT_COMPILE
			PrintLog("DInput Hook Enabled");

			//void* cDIDev_GetDeviceStatePtr = (void*)((DWORD)directInputCreateAPtr - 20032);// other version of Windows 10??
			void* cDIDev_GetDeviceStatePtr = (void*)((DWORD)directInputCreateAPtr + cDIDev_GetDeviceStateOffset);
			void* cDIObj_CreateDeviceWPtr = (void*)((DWORD)directInputCreateAPtr + cDIObj_CreateDeviceWOffset);
			void* joyReg_GetConfigPtr = (void*)((DWORD)directInputCreateAPtr + joyReg_GetConfigOffset);
			void* cDIDev_SetCooperativeLevelPtr = (void*)((DWORD)directInputCreateAPtr - 25616);
			void* cDIDev_SetDataFormatPtr = (void*)((DWORD)directInputCreateAPtr - 19936);
			// void* cDIDev_PollPtr = (void*)((DWORD)directInputCreateAPtr - 18720);
			//void* cDIDev_AcquirePtr = (void*)((DWORD)directInputCreateAPtr - 27696);

			//IH_CreateHook(cDIDev_AcquirePtr, HookCDIDev_Poll, reinterpret_cast<LPVOID*>(&TrueCDIDev_Poll));
			//IH_EnableHook(cDIDev_AcquirePtr);

			//IH_CreateHook(cDIDev_GetDeviceStatePtr, HookCDIDev_GetDeviceState, reinterpret_cast<LPVOID*>(&TrueCDIDev_GetDeviceState));
			//IH_EnableHook(cDIDev_GetDeviceStatePtr);

			//IH_CreateHook(cDIDev_SetDataFormatPtr, HookCDIDev_SetDataFormat, reinterpret_cast<LPVOID*>(&TrueCDIDev_SetDataFormat));
			//IH_EnableHook(cDIDev_SetDataFormatPtr);

			//IH_CreateHook(cDIDev_SetCooperativeLevelPtr, HookCDIDev_SetCooperativeLevel, reinterpret_cast<LPVOID*>(&TrueCDIDev_SetCooperativeLevel));
			//IH_EnableHook(cDIDev_SetCooperativeLevelPtr);

			// hooking only these 2 work for L4D2 so we are only need them right now
			IH_CreateHook(cDIObj_CreateDeviceWPtr, HookCDIObj_CreateDeviceW, reinterpret_cast<LPVOID*>(&TrueCDIObj_CreateDeviceW));
			IH_EnableHook(cDIObj_CreateDeviceWPtr);

			IH_CreateHook(joyReg_GetConfigPtr, HookJoyReg_GetConfig, reinterpret_cast<LPVOID*>(&TrueJoyReg_GetConfig));
			IH_EnableHook(joyReg_GetConfigPtr);
#endif

			/*HMODULE dinput8Module = LoadLibrary(L"dinput8");

			void* directInput8CreatePtr = GetProcAddress(dinput8Module, "DirectInput8Create");
			void* cDIObj_CreateDeviceW = (void*)((DWORD)directInput8CreatePtr + 58560);

			IH_CreateHook(cDIObj_CreateDeviceW, HookCDIObj_EnumObjectsW, reinterpret_cast<LPVOID*>(&TrueJoyReg_GetConfig));
			IH_EnableHook(cDIObj_CreateDeviceW);*/
		}
	}

	HandleForceFocus();

	//IH_CreateHook(CDIDev_GetDeviceInfoWPtr, HookCDIDev_GetDeviceState, reinterpret_cast<LPVOID*>(&TrueCDIDev_GetDeviceState));
	//IH_EnableHook(CDIDev_GetDeviceInfoWPtr);

	/*HMODULE kernelModule = LoadLibrary("kernel32");
	void* createFilePtr = GetProcAddress(kernelModule, "CreateFileA");

	IH_CreateHook(createFilePtr, HookCreateFile, reinterpret_cast<LPVOID*>(&TrueCreateFileProc));
	IH_EnableHook(createFilePtr);*/

	/*HMODULE hidModule = LoadLibrary("hid");
	void* hidP_GetButtonCapsPtr = GetProcAddress(hidModule, "HidP_GetButtonCaps");
	void* hidP_GetUsagesPtr = GetProcAddress(hidModule, "HidP_GetUsages");
	void* hidP_GetUsageValuePtr = GetProcAddress(hidModule, "HidP_GetUsageValue");
	void* hidP_GetDataPtr = GetProcAddress(hidModule, "HidP_GetData");
	void* hidP_GetHidGuidPtr = GetProcAddress(hidModule, "HidD_GetHidGuid");

	IH_CreateHook(hidP_GetButtonCapsPtr, HookHidP_GetButtonCaps, reinterpret_cast<LPVOID*>(&TrueHidP_GetButtonCaps));
	IH_EnableHook(hidP_GetButtonCapsPtr);

	IH_CreateHook(hidP_GetUsagesPtr, HookHidP_GetUsages, reinterpret_cast<LPVOID*>(&TrueHidP_GetUsages));
	IH_EnableHook(hidP_GetUsagesPtr);

	IH_CreateHook(hidP_GetUsageValuePtr, HookHidP_GetUsageValue, reinterpret_cast<LPVOID*>(&TrueHidP_GetUsageValue));
	IH_EnableHook(hidP_GetUsageValuePtr);

	IH_CreateHook(hidP_GetDataPtr, HookHidP_GetData, reinterpret_cast<LPVOID*>(&TrueHidP_GetData));
	IH_EnableHook(hidP_GetDataPtr);*/

	//while (!hidModule)
	//{
	//	hidModule = GetModuleHandle("hid.dll");
	//	Sleep(100);
	//}
	//PrintLog("Found hid.dll: %x !", hidModule);

	//while (!dwDirectInput8Create)
	//{
	//	dwDirectInput8Create = GetProcAddress(hModDInput8, "DirectInput8Create");
	//	Sleep(100);
	//}
	//PrintLog("Found DirectInput8Create: %x !", dwDirectInput8Create);

	////dwGetDeviceState = (DWORD)((DWORD)dwDirectInput8Create - GetDeviceStateOffset);
	//dwGetDeviceState = (DWORD)((DWORD)dwDirectInput8Create + GetDeviceStateOffset);
	//PrintLog("GetDeviceState: %x !", dwGetDeviceState);

	////Sleep(15000);
	////HMODULE dinput = LoadLibrary("dinput8");
	////void* getDeviceStatePtr = GetProcAddress(dinput, "GetDeviceState");
	////PrintLog(("ResY: " + std::to_string((int)getDeviceStatePtr)).c_str());
	//void* getDeviceStatePtr = (void*)dwGetDeviceState;
	////TrueGetDeviceState = (GetDeviceStateProc)getDeviceStatePtr;
	//
	////BeginRedirect(HookGetDeviceState, getDeviceStatePtr);
	////BeginRedirect(HookEnumDevices, GetProcAddress(hModDInput8, ""));

	////IH_CreateHook(getDeviceStatePtr, HookGetDeviceState, reinterpret_cast<LPVOID*>(&TrueGetDeviceState));
	//IH_CreateHook(getDeviceStatePtr, hkGetDeviceState, reinterpret_cast<LPVOID*>(&pGetDeviceState));
	//IH_EnableHook(getDeviceStatePtr);

	return 0;
}

std::wstring* KeepString(std::string str)
{
	std::wstring* uni = new std::wstring(str.length(), L' ');
	std::copy(str.begin(), str.end(), uni->begin());
	return uni;
}

std::wstring utf8toUtf16(const std::string & str)
{
	if (str.empty())
		return std::wstring();

	size_t charsNeeded = ::MultiByteToWideChar(CP_UTF8, 0,
		str.data(), (int)str.size(), NULL, 0);
	if (charsNeeded == 0)
		throw std::runtime_error("Failed converting UTF-8 string to UTF-16");

	std::vector<wchar_t> buffer(charsNeeded);
	int charsConverted = ::MultiByteToWideChar(CP_UTF8, 0,
		str.data(), (int)str.size(), &buffer[0], buffer.size());
	if (charsConverted == 0)
		throw std::runtime_error("Failed converting UTF-8 string to UTF-16");

	return std::wstring(&buffer[0], charsConverted);
}

GUID ParseGuid(char* str)
{
	GUID guid;
	unsigned long p0;
	int p1, p2, p3, p4, p5, p6, p7, p8, p9, p10;

	int err = sscanf_s(str, "%08lX-%04X-%04X-%02X%02X-%02X%02X%02X%02X%02X%02X",
		&p0, &p1, &p2, &p3, &p4, &p5, &p6, &p7, &p8, &p9, &p10);

	guid.Data1 = p0;
	guid.Data2 = p1;
	guid.Data3 = p2;
	guid.Data4[0] = p3;
	guid.Data4[1] = p4;
	guid.Data4[2] = p5;
	guid.Data4[3] = p6;
	guid.Data4[4] = p7;
	guid.Data4[5] = p8;
	guid.Data4[6] = p9;
	guid.Data4[7] = p10;

	return guid;
}

VOID InitInstance()
{
#ifdef _DEBUG
	bool filelog = true;
	bool systemlog = true;
#else
	bool filelog = false;
	bool systemlog = false;
#endif

	if (systemlog)
	{
		LogSystem();
	}

	filelog = true;

	if (filelog)
	{
		SYSTEMTIME systime;
		GetLocalTime(&systime);
		std::string processName;
		ModuleFileName(&processName);

		std::string logfile = StringFormat("x360ce_%s_%02u-%02u-%02u_%08u.log", processName.c_str(), systime.wYear,
			systime.wMonth, systime.wDay, GetTickCount());

		LogFile(logfile);
	}
	// Get will initalize static InputHookManager object and we want to initialize it ASAP
	InputHookManager::Get();

	IniFile ini;
	std::string inipath("ncoop.ini");
	ini.Load(inipath);

	ini.Get("Options", "DInputEnabled", &Globals::dInputEnabled);
	ini.Get("Options", "DInputLibrary", &Globals::dInputLibrary);
	ini.Get("Options", "DInputForceDisable", &Globals::dInputForceDisable);

	ini.Get("Options", "XInputEnabled", &Globals::xInputEnabled);
	ini.Get("Options", "XInputPlayerID", &Globals::xInputPlayerId);

	ini.Get("Options", "RerouteInput", &Globals::xInputReRouteEnabled);
	ini.Get("Options", "RerouteJoystickTemplate", &Globals::xInputReRouteTemplate);

	std::string guidStr;
	ini.Get("Options", "DInputGuid", &guidStr);
	if (guidStr.length() > 0)
	{
		GUID guid = ParseGuid((char*)guidStr.c_str());
		Globals::dInputPlayerGuid = guid;

		if (Globals::xInputReRouteEnabled)
		{
			PrintLog(("XInput hook: Gamepad 0 to " + guidStr).c_str());

			std::shared_ptr<Controller> controller(new Controller(0));
			Controller* pController = controller.get();
			if (pController)
			{
				pController->isFake = true;
				pController->m_instanceid = guid;
				Config::Mapping* pMapping = &pController->m_mapping;

				PrintLog(("XInput hook: Template " + std::to_string(Globals::xInputReRouteTemplate)).c_str());

				switch (Globals::xInputReRouteTemplate)
				{
				default:
				case 0:
				{
					for (u32 i = 0; i < _countof(pMapping->Button); ++i)
					{
						pMapping->Button[i].type = Config::DIGITAL;
						pMapping->Button[i].id = i + 1;
					}

					pMapping->DpadPOV = 1;

					// left trigger
					pMapping->Trigger[0].type = Config::HAXIS;
					pMapping->Trigger[0].id = 3;
					// right trigger
					pMapping->Trigger[1].type = Config::HAXIS;
					pMapping->Trigger[1].id = -3;

					// left stick x
					pMapping->Axis[0].analogType = Config::HAXIS;
					pMapping->Axis[0].id = 1;
					// y
					pMapping->Axis[1].analogType = Config::HAXIS;
					pMapping->Axis[1].id = -2;

					// right stick x
					pMapping->Axis[2].analogType = Config::HAXIS;
					pMapping->Axis[2].id = 4;
					// y
					pMapping->Axis[3].analogType = Config::HAXIS;
					pMapping->Axis[3].id = -5;
				}
				break;
				case 2: // PS4
				{
					for (u32 i = 0; i < _countof(pMapping->Button); ++i)
					{
						pMapping->Button[i].type = Config::DIGITAL;
					}

					pMapping->Button[0].id = 2; // A  - Cross
					pMapping->Button[1].id = 3; // B  - Circle
					pMapping->Button[2].id = 1; // X  - Square
					pMapping->Button[3].id = 4; // Y  - Triangle
					pMapping->Button[4].id = 5; // LB - L1
					pMapping->Button[5].id = 6; // RB - R1
					pMapping->Button[6].id = 9; // Back
					pMapping->Button[7].id = 10; // Start
					pMapping->Button[8].id = 11; // Left Stick Button
					pMapping->Button[9].id = 12; // Right Stick Button

					pMapping->DpadPOV = 1;

					// left trigger
					pMapping->Trigger[0].type = Config::AXIS;
					pMapping->Trigger[0].id = 4;
					// right trigger
					pMapping->Trigger[1].type = Config::AXIS;
					pMapping->Trigger[1].id = 5;

					// left stick x
					pMapping->Axis[0].analogType = Config::HAXIS;
					pMapping->Axis[0].id = 1;
					// y
					pMapping->Axis[1].analogType = Config::HAXIS;
					pMapping->Axis[1].id = -2;

					// right stick x
					pMapping->Axis[2].analogType = Config::HAXIS;
					pMapping->Axis[2].id = 3;
					// y
					pMapping->Axis[3].analogType = Config::HAXIS;
					pMapping->Axis[3].id = -6;
				}
				break;
				}

				// Not a combined device. Just add like normal.
				ControllerManager::Get().GetControllers().push_back(controller);
			}
		}
	}

	ini.Get("Options", "ForceFocus", &Globals::forceFocus);
	std::string forceFocusWindowName;
	ini.Get("Options", "ForceFocusWindowName", &forceFocusWindowName);
	Globals::forceFocusWindowName = KeepString(forceFocusWindowName);

	ini.Get("Options", "WindowX", &Globals::windowX);
	ini.Get("Options", "WindowY", &Globals::windowY);

	ini.Get("Options", "ResWidth", &Globals::resWidth);
	ini.Get("Options", "ResHeight", &Globals::resHeight);

	ini.Get("Options", "BlockInputEvents", &Globals::blockInputEvents);
	ini.Get("Options", "BlockMouseEvents", &Globals::blockMouseEvents);
	ini.Get("Options", "BlockKeyboardEvents", &Globals::blockKeyboardEvents);

	ini.Get("Options", "EnableMKBInput", &Globals::enableMKBInput);

	MH_Initialize();
	tmpHandle = CreateThread(0, 0, (LPTHREAD_START_ROUTINE)&HookThread, 0, 0, 0);

	if (Globals::xInputEnabled)
	{
		// grab gamepad state
		XINPUT_STATE state;
		XInputModuleManager::Get().XInputGetState(0, &state);
	}
}

extern "C" VOID WINAPI Reset()
{
	PrintLog("Restarting");

	// Only x360ce.App will call this so InputHook is not required, disable it.
	InputHookManager::Get().GetInputHook().Shutdown();
	ControllerManager::Get().GetControllers().clear();
	ControllerManager::Get().GetConfig().ReadConfig();
}

extern "C" BOOL APIENTRY DllMain(HMODULE hModule, DWORD ul_reason_for_call, LPVOID lpReserved)
{
	UNREFERENCED_PARAMETER(lpReserved);

	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
		DisableThreadLibraryCalls(hModule);
		InitInstance();
		break;

	case DLL_PROCESS_DETACH:
		break;
	}

	return TRUE;
}
