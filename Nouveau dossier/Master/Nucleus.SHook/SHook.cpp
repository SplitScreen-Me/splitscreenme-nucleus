#include "pch.h"
#include "easyhook.h"
#include "framework.h"
//#include "string"
#include "windows.h"
//#include <sstream>
//#include <ios>
#include <fstream>
//#include <atlbase.h>
#include <random>
#include <string>
#include <map>
#include <iostream>
#include <locale>
#include <codecvt>
#include <time.h>
//#include <stdio.h>
//#include <Xinput.h>
using namespace std;

//#pragma comment(lib, "XInput.lib")

//typedef DWORD(WINAPI* XInputGetStateExProc)(DWORD dwUserIndex, XINPUT_STATE* pState);

//#ifndef UNICODE  
//typedef std::string String;
//#else
//typedef std::wstring String;
//#endif

HWND hWnd = 0;
//WNDPROC g_OldWndProc;

bool IsDebug = false;

std::ofstream outfile;
std::wstring nucleusFolder;
std::wstring logFile = L"\\debug-log-startup-hook.txt";
std::wstring rawHid;
std::string playerNick;

int width;
int height;
int posx;
int posy;

//std::wstring rawHid;

std::mt19937 randomGenerator;

//Key: search term. Value: the assigned name that is replaced for every name that matched the search term. (value is empty if needs generating)
std::map <std::wstring, std::wstring> searchTermsToAssignedNames;

typedef enum _EVENT_TYPE {
	NotificationEvent,
	SynchronizationEvent
} EVENT_TYPE, * PEVENT_TYPE;

typedef NTSTATUS(NTAPI* t_NtCreateMutant)(PHANDLE MutantHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, BOOLEAN InitialOwner);
typedef NTSTATUS(NTAPI* t_NtOpenMutant)(PHANDLE MutantHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

typedef NTSTATUS(NTAPI* t_NtCreateEvent)(PHANDLE EventHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, EVENT_TYPE EventType, BOOLEAN InitialState);
typedef NTSTATUS(NTAPI* t_NtOpenEvent)(PHANDLE EventHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

typedef NTSTATUS(NTAPI* t_NtCreateSemaphore)(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, ULONG InitialCount, ULONG MaximumCount);
typedef NTSTATUS(NTAPI* t_NtOpenSemaphore)(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

static t_NtCreateMutant NtCreateMutant;
static t_NtOpenMutant NtOpenMutant;

static t_NtCreateEvent NtCreateEvent;
static t_NtOpenEvent NtOpenEvent;

static t_NtCreateSemaphore NtCreateSemaphore;
static t_NtOpenSemaphore NtOpenSemaphore;

std::string ws2s(const std::wstring& wstr)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.to_bytes(wstr);
}

std::wstring s2ws(const std::string& str)
{
	using convert_typeX = std::codecvt_utf8<wchar_t>;
	std::wstring_convert<convert_typeX, wchar_t> converterX;

	return converterX.from_bytes(str);
}

inline std::string date_string()
{
	tm tinfo;
	time_t rawtime;
	std::time(&rawtime);
	localtime_s(&tinfo, &rawtime);
	char buffer[21];
	strftime(buffer, 21, "%Y-%m-%d %H:%M:%S", &tinfo);
	return "[" + std::string(buffer) + "]";
}

inline UNICODE_STRING stdWStringToUnicodeString(const std::wstring& str) {
	UNICODE_STRING unicodeString;
	DWORD len = 0;

	len = str.length();
	LPWSTR cstr = new WCHAR[len + 1];
	memcpy(cstr, str.c_str(), (len + 1) * sizeof(WCHAR));
	unicodeString.Buffer = cstr;
	unicodeString.Length = (USHORT)(len * sizeof(WCHAR));
	unicodeString.MaximumLength = (USHORT)((len + 1) * sizeof(WCHAR));
	return unicodeString;
}

void updateName(PUNICODE_STRING inputName)
{
	if (!(inputName->Length > 0 && inputName->Length <= inputName->MaximumLength)) return;

	for (std::map<std::wstring, std::wstring>::value_type& pair : searchTermsToAssignedNames)
	{
		if (wcsstr(inputName->Buffer, pair.first.c_str()) != nullptr)
		{
			if (pair.second.empty())
			{
				const auto rand = std::to_wstring(randomGenerator());

				const std::wstring oldName = inputName->Buffer;
				const auto newName = oldName + rand;
				if (IsDebug)
				{
					outfile.open(nucleusFolder + logFile, std::ios_base::app);
					outfile << date_string() << "SHOOK: Mutex function called, renaming mutex " << ws2s(oldName) << " to " << ws2s(newName) << "\n";
					outfile.close();
				}
				pair.second = newName;
			}

			*inputName = stdWStringToUnicodeString(pair.second);
		}
	}
}

inline void updateNameObject(POBJECT_ATTRIBUTES ObjectAttributes)
{
	if (ObjectAttributes != NULL && ObjectAttributes->ObjectName != NULL)
	{
		updateName(ObjectAttributes->ObjectName);
	}
}

//LONG_PTR CALLBACK WndProc_Hook(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam);
//
//LONG_PTR CALLBACK WndProc_Hook(HWND hwnd, UINT uMsg, WPARAM wParam, LPARAM lParam)
//{
//	//if (IsDebug)
//	//{
//	//	outfile.open(nucleusFolder + logFile, std::ios_base::app);
//	//	outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//	//	outfile.close();
//	//}
//	switch (uMsg)
//	{
//	//case WM_INPUT:
//	//{
//	//	break;
//	//}
//	//case WM_KILLFOCUS:
//	//{
//	//	SetFocus(hWnd);
//	//	return -1;
//	//}
//	//case WM_NCACTIVATE:
//	//{
//	//	return 1;
//	//}
//	case WM_ACTIVATEAPP:
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//		return 0;
//	}
//	case WM_ACTIVATE:
//	{
//		//if (wParam == WA_INACTIVE)
//		//{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//			return 0;
//		//}
//	}
//	case WM_NCHITTEST:
//		// System is checking if the mouse is inside the game screen, but we dont want
//		// that call to ever be recognized by the game
//		// ??? needed for borderlands to detect mouse
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//		return HTNOWHERE;
//
//	case WM_MOUSEHOVER:
//			//PrintLog(("No MKB Input blocked: " + int_to_hex(message) + " " + std::to_string(gameSetWindowLongW)).c_str());
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//			return 0;
//
//	case WM_SETCURSOR:
//
//			// return true to stop Windows from trying to
//			// keep sending WM_SETCURSOR to the process
//			// (prevents crashes in all Borderlands with early clicks, not sure other games)
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//			return true;
//		break;
//	case WM_MOUSEMOVE:
//		// clip everytime, TODO: benchmark this
//			//PrintLog(("No MKB Input blocked: " + int_to_hex(message) + " " + std::to_string(gameSetWindowLongW)).c_str());
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//			return 0;
//
//	case WM_SYSKEYUP:
//			//PrintLog(("No MKB Input keyboard blocked: " + int_to_hex(message) + " " + std::to_string(gameSetWindowLongW)).c_str());
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "SHOOK: uMsg: " << uMsg << " wParam: " << wParam << " lParam: " << lParam << "\n";
//			outfile.close();
//		}
//			return 0;
//	default:
//		DefWindowProc(hwnd, uMsg, wParam, lParam);
//	}
//	return 1;
//}

bool compareChar(char& c1, char& c2)
{
	if (c1 == c2)
		return true;
	else if (std::toupper(c1) == std::toupper(c2))
		return true;
	return false;
}
//
/*
 * Case Insensitive String Comparision
 */
bool caseInSensStringCompare(std::string& str1, std::string& str2)
{
	return ((str1.size() == str2.size()) &&
		std::equal(str1.begin(), str1.end(), str2.begin(), &compareChar));
}

//BOOL FilterMessage(LPMSG lpMsg)
//{
//	UINT Msg = lpMsg->message;
//	WPARAM _wParam = lpMsg->wParam;
//	LPARAM _lParam = lpMsg->lParam;
//
//#define ALLOW return 1;
//#define BLOCK memset(lpMsg, 0, sizeof(MSG)); return -1;
//
//	//Filter raw input
//	if (Msg == WM_KILLFOCUS)
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "killfocus called\n";
//			outfile.close();
//		}
//		BLOCK;
//	}
//	else if (Msg == WM_MOUSEACTIVATE)
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "mouseactivate called\n";
//			outfile.close();
//		}
//	}
//	else if (Msg == WM_SETFOCUS)
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "setfocus called\n";
//			outfile.close();
//		}
//	}
//	else if (Msg == WM_CAPTURECHANGED)
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "capturechanged called\n";
//			outfile.close();
//		}
//	}
//	else if (Msg == WM_INPUT)
//	{
//		UINT cbSize;
//		GetRawInputData((HRAWINPUT)_lParam, RID_INPUT, 0, &cbSize, sizeof(RAWINPUTHEADER));
//		LPBYTE lpbBuffer = new BYTE[cbSize];
//		GetRawInputData((HRAWINPUT)_lParam, RID_INPUT, lpbBuffer, &cbSize, sizeof(RAWINPUTHEADER));
//		RAWINPUT* raw = (RAWINPUT*)lpbBuffer;
//
//		RID_DEVICE_INFO rdi;
//		rdi.cbSize = sizeof(RID_DEVICE_INFO);
//
//		cbSize = rdi.cbSize;
//		if (GetRawInputDeviceInfo(raw->header.hDevice, RIDI_DEVICEINFO, &rdi, &cbSize) < 0)
//		{
//			// Error in reading information
//		}
//
//		UINT size = 256;
//		TCHAR tBuffer[256] = { 0 };
//
//		if (GetRawInputDeviceInfo(raw->header.hDevice, RIDI_DEVICENAME, tBuffer, &size) < 0)
//		{
//			// Error in reading device name
//			if (IsDebug)
//			{
//				outfile.open(nucleusFolder + logFile, std::ios_base::app);
//				outfile << date_string() << "error reading device name\n";
//				outfile.close();
//			}
//		}
//
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "Device name: " << tBuffer << "\n";
//			outfile.close();
//		}
//
//		if (raw->header.dwType == RIM_TYPEHID)
//		{
//			//if (rdi.hid.usUsage == 4 || rdi.hid.usUsage == 5)
//			//{
//
//
//				//std::string str1 = ws2s(rawHid);
//				//std::string str2 = tBuffer;
//
//				//bool result = caseInSensStringCompare(str1, str2);
//
//				//if (result)
//				//{
//				//	ALLOW;
//				//}
//				//else
//				//{
//				//	BLOCK;
//				//}
//
//			//}
//		}
//	}
//
//	return 1;
//
//#undef ALLOW
//#undef BLOCK
//}
//
//BOOL WINAPI GetMessageA_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
//{
//	BOOL ret = GetMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
//
//	return ret == -1 ? -1 : FilterMessage(lpMsg);
//}
//
//BOOL WINAPI GetMessageW_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax)
//{
//	BOOL ret = GetMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax);
//
//	return ret == -1 ? -1 : FilterMessage(lpMsg);
//}
//
//BOOL WINAPI PeekMessageA_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
//{
//	BOOL ret = PeekMessageA(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
//
//	return ret == FALSE ? FALSE : FilterMessage(lpMsg);
//}
//
//BOOL WINAPI PeekMessageW_Hook(LPMSG lpMsg, HWND hWnd, UINT wMsgFilterMin, UINT wMsgFilterMax, UINT wRemoveMsg)
//{
//	BOOL ret = PeekMessageW(lpMsg, hWnd, wMsgFilterMin, wMsgFilterMax, wRemoveMsg);
//
//	return ret == FALSE ? FALSE : FilterMessage(lpMsg);
//}

UINT WINAPI GetRawInputDeviceList_Hook(PRAWINPUTDEVICELIST pRawInputDeviceList, PUINT puiNumDevices, UINT cbSize)
{
	*puiNumDevices = 0;

	//#define MAX_CONTROLLERS 4
	//
	//	HMODULE xinput_lib = LoadLibrary("xinput1_3.dll");
	//
	//	XInputGetStateExProc XInputGetStateEx;
	//	int XInputGetStateExOrdinal = 100;
	//	XInputGetStateEx = (XInputGetStateExProc)GetProcAddress(xinput_lib, (LPCTSTR)XInputGetStateExOrdinal);
	//
	//	for (DWORD i = 0; i < MAX_CONTROLLERS; i++)
	//	{
	//		XINPUT_STATE state;
	//		ZeroMemory(&state, sizeof(XINPUT_STATE));
	//		DWORD dwResult = XInputGetState(i, &state);
	//		ZeroMemory(&state, sizeof(XINPUT_STATE));
	//		DWORD dwResultEx = XInputGetStateEx(i, &state);
	//	}

	return 0; //GetRawInputDeviceList(pRawInputDeviceList, puiNumDevices, cbSize);
}

UINT WINAPI GetRegisteredRawInputDevices_Hook(PRAWINPUTDEVICE pRawInputDevices, PUINT puiNumDevices, UINT cbSize)
{
	*puiNumDevices = 0;

	return 0; // GetRegisteredRawInputDevices(pRawInputDevices, puiNumDevices, cbSize);
}

//BOOL WINAPI RegisterRawInputDevices_Hook(PCRAWINPUTDEVICE pRawInputDevices, UINT uiNumDevices, UINT cbSize)
//{
//
//	RAWINPUTDEVICE rawInputDevice[1];
//	rawInputDevice[0].usUsagePage = 1;
//	rawInputDevice[0].usUsage = 5;
//	rawInputDevice[0].dwFlags = RIDEV_INPUTSINK;
//	rawInputDevice[0].hwndTarget = hWnd;
//
//	BOOL result = RegisterRawInputDevices(rawInputDevice, 1, sizeof(rawInputDevice[0]));
//
//	if (!result)
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "block raw RegisterRawInputDevices Error: " << GetLastError() << "\n";
//			outfile.close();
//		}
//	}
//	else
//	{
//		if (IsDebug)
//		{
//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
//			outfile << date_string() << "injection entry registered\n";
//			outfile.close();
//		}
//	}
//
//	if (IsDebug)
//	{
//		outfile.open(nucleusFolder + logFile, std::ios_base::app);
//		outfile << date_string() << "RegisterRawInputDevices called " << "usage: " << pRawInputDevices->usUsage << " usagepage: " << pRawInputDevices->usUsagePage << " dwflags: " << pRawInputDevices->dwFlags << " hwndtarget: " << pRawInputDevices->hwndTarget <<  endl;
//		outfile.close();
//	}
//
//	return RegisterRawInputDevices(pRawInputDevices, uiNumDevices, cbSize);
//}

BOOL WINAPI SetWindowPos_Hook(HWND hWnd, HWND hWndInsertAfter, int X, int Y, int cx, int cy, UINT uFlags)
{
	//if ((X == posx && Y == posy && cx == width && cy == height) || (X == 0 && Y == 0 && cx == 0 && cy == 0))
	//{
	//	return true;
	//}
	//else
	//{
	//	return false;
	//}

	bool result = SetWindowPos(hWnd, hWndInsertAfter, posx, posy, width, height, uFlags);

	return result;
}

BOOL WINAPI RegisterUserApiHook_Hook(HINSTANCE hInstance, FARPROC CallbackFunc)
{
	if (IsDebug)
	{
		outfile.open(nucleusFolder + logFile, std::ios_base::app);
		outfile << date_string() << "SHOOK: REGISTERUSERAPIHOOK \n";
		outfile.close();
	}

	return false;
}

FARPROC WINAPI GetProcAddress_Hook(HMODULE hModule, LPCSTR lpProcName)
{
	if (((DWORD)lpProcName >> 16) == 0) //Check if Ordinal
	{
		//do stuff to hook your wanted method in case you think it is being requested by ordinal

		if (LOWORD(lpProcName) == 0x958) { //SetWindowPos Ordinal Win10x64

			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: ORDINAL\n";
				outfile.close();
			}

		}

	}
	else
	{
		if (strcmp(lpProcName, "SetWindowPos") == 0) {
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: " << lpProcName << " " << hModule << "\n";
				outfile.close();

				char path[MAX_PATH];
				HMODULE hm = NULL;

				if (GetModuleHandleEx(GET_MODULE_HANDLE_EX_FLAG_FROM_ADDRESS |
					GET_MODULE_HANDLE_EX_FLAG_UNCHANGED_REFCOUNT,
					lpProcName, &hm) == 0)
				{
					int ret = GetLastError();
					fprintf(stderr, "GetModuleHandle failed, error = %d\n", ret);
					// Return or however you want to handle an error.
				}
				if (GetModuleFileName(hm, path, sizeof(path)) == 0)
				{
					int ret = GetLastError();
					fprintf(stderr, "GetModuleFileName failed, error = %d\n", ret);
					// Return or however you want to handle an error.
				}

				// The path variable should now contain the full filepath for this DLL.

				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: path " << path << "\n";
				outfile.close();

				return NULL;
			}

		}
	}

	return GetProcAddress(hModule, lpProcName);
}

bool hasEnding(std::string const& fullString, std::string const& ending)
{
	if (fullString.length() >= ending.length()) {
		return (0 == fullString.compare(fullString.length() - ending.length(), ending.length(), ending));
	}
	else {
		return false;
	}
}

bool replace(std::string& str, const std::string& from, const std::string& to) {
	size_t start_pos = str.find(from);
	if (start_pos == std::string::npos)
		return false;
	str.replace(start_pos, from.length(), to);
	return true;
}

NTSTATUS NTAPI NtCreateMutant_Hook(OUT PHANDLE MutantHandle, IN ULONG DesiredAccess, IN POBJECT_ATTRIBUTES ObjectAttributes OPTIONAL, IN BOOLEAN InitialOwner)
{
	updateNameObject(ObjectAttributes);
	return NtCreateMutant(MutantHandle, DesiredAccess, ObjectAttributes, InitialOwner);
}

NTSTATUS NTAPI NtOpenMutant_Hook(PHANDLE MutantHandle, ULONG DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenMutant(MutantHandle, DesiredAccess, ObjectAttributes);
}

NTSTATUS NTAPI NtCreateEvent_Hook(PHANDLE EventHandle, DWORD DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, EVENT_TYPE EventType, BOOLEAN InitialState)
{
	updateNameObject(ObjectAttributes);
	return NtCreateEvent(EventHandle, DesiredAccess, ObjectAttributes, EventType, InitialState);
}

NTSTATUS NTAPI NtOpenEvent_Hook(PHANDLE EventHandle, DWORD DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenEvent(EventHandle, DesiredAccess, ObjectAttributes);
}

NTSTATUS NTAPI NtCreateSemaphore_Hook(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, ULONG InitialCount, ULONG MaximumCounts)
{
	updateNameObject(ObjectAttributes);
	return NtCreateSemaphore(SemaphoreHandle, DesiredAccess, ObjectAttributes, InitialCount, MaximumCounts);
}

NTSTATUS NTAPI NtOpenSemaphore_Hook(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenSemaphore(SemaphoreHandle, DesiredAccess, ObjectAttributes);
}

HWND WINAPI FindWindow_Hook(LPCSTR lpClassName, LPCSTR lpWindowName)
{
	return NULL;
}

HWND WINAPI FindWindowEx_Hook(LPCSTR lpClassName, LPCSTR lpWindowName)
{
	return NULL;
}

BOOL WINAPI EnumWindows_Hook(WNDENUMPROC lpEnumFunc, LPARAM lParam)
{
	return TRUE;
}

BOOL ReadFile_Hook(
	HANDLE       hFile,
	LPVOID       lpBuffer,
	DWORD        nNumberOfBytesToRead,
	LPDWORD      lpNumberOfBytesRead,
	LPOVERLAPPED lpOverlapped
)
{
	return ReadFile(
		hFile,
		lpBuffer,
		nNumberOfBytesToRead,
		lpNumberOfBytesRead,
		lpOverlapped
	);
}

BOOL ReadFileEx_Hook(
	HANDLE                          hFile,
	LPVOID                          lpBuffer,
	DWORD                           nNumberOfBytesToRead,
	LPOVERLAPPED                    lpOverlapped,
	LPOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine
)
{
	return ReadFileEx(
		hFile,
		lpBuffer,
		nNumberOfBytesToRead,
		lpOverlapped,
		lpCompletionRoutine
	);
}

BOOL WriteFile_Hook(
	HANDLE       hFile,
	LPCVOID      lpBuffer,
	DWORD        nNumberOfBytesToWrite,
	LPDWORD      lpNumberOfBytesWritten,
	LPOVERLAPPED lpOverlapped
)
{
	return WriteFile(
		hFile,
		lpBuffer,
		nNumberOfBytesToWrite,
		lpNumberOfBytesWritten,
		lpOverlapped
	);
}

BOOL WriteFileEx_Hook(
	HANDLE                          hFile,
	LPCVOID                         lpBuffer,
	DWORD                           nNumberOfBytesToWrite,
	LPOVERLAPPED                    lpOverlapped,
	LPOVERLAPPED_COMPLETION_ROUTINE lpCompletionRoutine
)
{
	return WriteFileEx(
		hFile,
		lpBuffer,
		nNumberOfBytesToWrite,
		lpOverlapped,
		lpCompletionRoutine
	);
}

HANDLE CreateFileW_Hook(
	LPCWSTR               lpFileName,
	DWORD                 dwDesiredAccess,
	DWORD                 dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes,
	DWORD                 dwCreationDisposition,
	DWORD                 dwFlagsAndAttributes,
	HANDLE                hTemplateFile
)
{
	wstring fileNameWstring(lpFileName);
	string fileNameStr(fileNameWstring.begin(), fileNameWstring.end());

	wstring rawHidWstring(rawHid);
	string rawHidStr(rawHidWstring.begin(), rawHidWstring.end());

	if (fileNameStr.find("HID") != std::string::npos)
	{
		if (!caseInSensStringCompare(fileNameStr, rawHidStr))
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: CREATEFILE - BLOCKING " << fileNameStr << "\n";
				outfile.close();
			}
			return INVALID_HANDLE_VALUE;
		}
	}
	//else if (fileNameStr.find("LocalLow") != std::string::npos)
	//{
	//	if (replace(fileNameStr, "LocalLow", "NucleusCoop\\" + playerNick + "\\LocalLow"))
	//	{
	//		if (IsDebug)
	//		{
	//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//			outfile << date_string() << "SHOOK: CREATEFILE - BEFORE " << fileNameStr << "\n";
	//			outfile.close();
	//		}
	//		lpFileName = s2ws(fileNameStr).c_str();
	//		if (IsDebug)
	//		{
	//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//			outfile << date_string() << "SHOOK: CREATEFILE - REPLACING " << fileNameStr << "\n";
	//			outfile.close();
	//		}
	//	}
	//}

	return CreateFileW(
		lpFileName,
		dwDesiredAccess,
		dwShareMode,
		lpSecurityAttributes,
		dwCreationDisposition,
		dwFlagsAndAttributes,
		hTemplateFile
	);
}

HANDLE CreateFileA_Hook(
	LPCSTR				  lpFileName,
	DWORD                 dwDesiredAccess,
	DWORD                 dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes,
	DWORD                 dwCreationDisposition,
	DWORD                 dwFlagsAndAttributes,
	HANDLE                hTemplateFile
)
{
	string fileNameStr(lpFileName);

	wstring rawHidWstring(rawHid);
	string rawHidStr(rawHidWstring.begin(), rawHidWstring.end());

	if (fileNameStr.find("HID") != std::string::npos)
	{
		if (!caseInSensStringCompare(fileNameStr, rawHidStr))
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: CREATEFILE - BLOCKING " << fileNameStr << "\n";
				outfile.close();
			}
			return INVALID_HANDLE_VALUE;
		}
	}
	//else if (fileNameStr.find("LocalLow") != std::string::npos)
	//{
	//	if (replace(fileNameStr, "LocalLow", "NucleusCoop\\" + playerNick + "\\LocalLow"))
	//	{
	//		if (IsDebug)
	//		{
	//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//			outfile << date_string() << "SHOOK: CREATEFILE - BEFORE " << fileNameStr << "\n";
	//			outfile.close();
	//		}
	//		lpFileName = fileNameStr.c_str();
	//		if (IsDebug)
	//		{
	//			outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//			outfile << date_string() << "SHOOK: CREATEFILE - REPLACING " << fileNameStr << "\n";
	//			outfile.close();
	//		}
	//	}
	//}

	return CreateFileA(
		lpFileName,
		dwDesiredAccess,
		dwShareMode,
		lpSecurityAttributes,
		dwCreationDisposition,
		dwFlagsAndAttributes,
		hTemplateFile
	);
}


// Structure used to communicate data from and to enumeration procedure
struct EnumData {
	DWORD dwProcessId;
	HWND hWnd;
};

// Application-defined callback for EnumWindows
BOOL CALLBACK EnumProc(HWND hWnd, LPARAM lParam) {
	// Retrieve storage location for communication data
	EnumData& ed = *(EnumData*)lParam;
	DWORD dwProcessId = 0x0;
	// Query process ID for hWnd
	GetWindowThreadProcessId(hWnd, &dwProcessId);
	// Apply filter - if you want to implement additional restrictions,
	// this is the place to do so.
	if (ed.dwProcessId == dwProcessId) {
		// Found a window matching the process ID
		ed.hWnd = hWnd;
		// Report success
		SetLastError(ERROR_SUCCESS);
		// Stop enumeration
		return FALSE;
	}
	// Continue enumeration
	return TRUE;
}

// Main entry
HWND FindWindowFromProcessId(DWORD dwProcessId) {
	EnumData ed = { dwProcessId };
	if (!EnumWindows(EnumProc, (LPARAM)&ed) &&
		(GetLastError() == ERROR_SUCCESS)) {
		return ed.hWnd;
	}
	return NULL;
}

// Helper method for convenience
HWND FindWindowFromProcess(HANDLE hProcess) {
	return FindWindowFromProcessId(GetProcessId(hProcess));
}

string ExePath() {
	char buffer[MAX_PATH];
	GetModuleFileName(NULL, buffer, MAX_PATH);
	string::size_type pos = string(buffer).find_last_of("\\/");
	return string(buffer).substr(0, pos);
}

NTSTATUS installHook(LPCSTR moduleHandle, LPCSTR proc, void* callBack)
{
	// Perform hooking
	HOOK_TRACE_INFO hHook = { NULL }; // keep track of our hook

	// Install the hook
	NTSTATUS result = LhInstallHook(
		GetProcAddress(GetModuleHandle(moduleHandle), proc),
		callBack,
		NULL,
		&hHook);
	if (FAILED(result))
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Error installing " << proc << " hook, error msg: " << RtlGetLastErrorString() << "\n";
		}
	}
	else
	{
		// If the threadId in the ACL is set to 0,
		// then internally EasyHook uses GetCurrentThreadId()
		ULONG ACLEntries[1] = { 0 };

		// Disable the hook for the provided threadIds, enable for all others
		LhSetExclusiveACL(ACLEntries, 1, &hHook);

		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Successfully installed " << proc << " hook, in module: " << moduleHandle << ", result: " << result << "\n";
		}
	}

	outfile.close();
	return result;
}


HMODULE WINAPI LoadLibraryA_Hook(LPCSTR lpLibFileName)
{
	//HMODULE result = LoadLibraryA(lpLibFileName);

	string str(lpLibFileName);

	// convert string to back to lower case
	std::for_each(str.begin(), str.end(), [](char& c) {
		c = ::tolower(c);
		});

	//if (IsDebug)
	//{
	//	outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//	outfile << date_string() << "SHOOK: LOADLIBRARYA " << lpLibFileName << " str " << str << "\n";
	//	outfile.close();
	//}

	size_t found = str.find("xinput");

	//if (hasEnding(str, "system32\\xinput1_3.dll") || hasEnding(str, "system32\\xinput1_4.dll") || hasEnding(str, "system32\\xinput9_1_0.dll"))
	if (found != std::string::npos)
	{
		std::string path;
		//char buffer[MAX_PATH];
		//GetModuleFileName(NULL, buffer, MAX_PATH);
		path.append(ExePath());
		path.append("\\");
		path.append(str);

		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: LOADLIBRARYA HOOK " << path << "\n";
			outfile.close();
		}

		return LoadLibraryA(path.c_str());
	}

	return LoadLibraryA(lpLibFileName);
}

HMODULE WINAPI LoadLibraryExA_Hook(LPCSTR lpLibFileName, HANDLE hFile, DWORD dwFlags)
{
	//HMODULE result = LoadLibraryExA(lpLibFileName, hFile, dwFlags);

	string str(lpLibFileName);

	//if (IsDebug)
	//{
	//	outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//	outfile << date_string() << "SHOOK: LOADLIBRARYEXA " << lpLibFileName << " str " << str << "\n";
	//	outfile.close();
	//}

		// convert string to back to lower case
	//std::for_each(str.begin(), str.end(), [](char& c) {
	//	c = ::tolower(c);
	//	});

	if (hasEnding(str, "xinput1_3.dll") || hasEnding(str, "xinput1_4.dll") || hasEnding(str, "XInput9_1_0.dll"))
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: LOADLIBRARYEXA HOOK\n";
			outfile.close();
		}
		return NULL;
	}

	return LoadLibraryExA(lpLibFileName, hFile, dwFlags);
}

HMODULE WINAPI LoadLibraryW_Hook(LPCWSTR lpLibFileName)
{
	//HMODULE result = LoadLibraryW(lpLibFileName);

	wstring fileNameWstring(lpLibFileName);
	string str(fileNameWstring.begin(), fileNameWstring.end());

	//if (IsDebug)
	//{
	//	outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//	outfile << date_string() << "SHOOK: LOADLIBRARYW " << lpLibFileName << " str " << str << "\n";
	//	outfile.close();
	//}

			// convert string to back to lower case
	//std::for_each(str.begin(), str.end(), [](char& c) {
	//	c = ::tolower(c);
	//	});

	if (hasEnding(str, "xinput1_3.dll") || hasEnding(str, "xinput1_4.dll") || hasEnding(str, "XInput9_1_0.dll"))
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: LOADLIBRARYW HOOK\n";
			outfile.close();
		}
		return NULL;
	}

	return LoadLibraryW(lpLibFileName);
}

HMODULE WINAPI LoadLibraryExW_Hook(LPCWSTR lpLibFileName, HANDLE hFile, DWORD dwFlags)
{
	//HMODULE result = LoadLibraryExW(lpLibFileName, hFile, dwFlags);

	wstring fileNameWstring(lpLibFileName);
	string str(fileNameWstring.begin(), fileNameWstring.end());

	//if (IsDebug)
	//{
	//	outfile.open(nucleusFolder + logFile, std::ios_base::app);
	//	outfile << date_string() << "SHOOK: LOADLIBRARYEXW " << lpLibFileName << " str " << str << "\n";
	//	outfile.close();
	//}

			// convert string to back to lower case
	//std::for_each(str.begin(), str.end(), [](char& c) {
	//	c = ::tolower(c);
	//	});

	if (hasEnding(str, "xinput1_3.dll") || hasEnding(str, "xinput1_4.dll") || hasEnding(str, "XInput9_1_0.dll"))
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: LOADLIBRARYEXW HOOK\n";
			outfile.close();
		}
		return NULL;
	}

	return LoadLibraryExW(lpLibFileName, hFile, dwFlags);
}

void installFindMutexHooks(LPCWSTR targets)
{
	//Random
	std::random_device rd;
	randomGenerator = static_cast<std::mt19937>(rd());


	//Search terms
#define ADD_SEARCH_TERM(term) searchTermsToAssignedNames.insert(std::make_pair((term), L"")); std::wcout << L"Added search term: " << sub << std::endl;

	{
		std::wstring target_s(targets);
		std::wstring splitter = L"|==|";
		unsigned int startIndex = 0;
		unsigned int endIndex = 0;

		while ((endIndex = target_s.find(splitter, startIndex)) < target_s.size())
		{
			std::wstring sub = target_s.substr(startIndex, endIndex - startIndex);

			size_t found = sub.find('|');

			if (found != std::string::npos) {
				startIndex += found + 1;
			}

			sub = target_s.substr(startIndex, endIndex - startIndex);

			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Rename mutex, adding search term: " << ws2s(sub) << "\n";
				outfile.close();
			}
			ADD_SEARCH_TERM(sub);
			startIndex = endIndex + splitter.size();
		}

		if (startIndex < target_s.size())
		{
			//No splitters in string
			std::wstring sub = target_s.substr(startIndex);

			size_t found = sub.find('|');
			if (found != std::string::npos) {
				startIndex += found + 1;
			}

			sub = target_s.substr(startIndex, endIndex - startIndex);

			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Rename mutex, adding search term: " << ws2s(sub) << "\n";
				outfile.close();
			}
			ADD_SEARCH_TERM(sub);
		}
	}

#undef ADD_SEARCH_TERM


	//Ntdll functions
#define GET_NT_PROC(name, type) (type)GetProcAddress(GetModuleHandle("ntdll.dll"), name)

	NtCreateMutant = GET_NT_PROC("NtCreateMutant", t_NtCreateMutant);
	NtCreateMutant = GET_NT_PROC("NtCreateMutant", t_NtCreateMutant);
	NtOpenMutant = GET_NT_PROC("NtOpenMutant", t_NtOpenMutant);

	NtCreateEvent = GET_NT_PROC("NtCreateEvent", t_NtCreateEvent);
	NtOpenEvent = GET_NT_PROC("NtOpenEvent", t_NtOpenEvent);

	NtCreateSemaphore = GET_NT_PROC("NtCreateSemaphore", t_NtCreateSemaphore);
	NtOpenSemaphore = GET_NT_PROC("NtOpenSemaphore", t_NtOpenSemaphore);

#undef GET_NT_PROC

	//Hooks
	installHook("ntdll.dll", "NtCreateMutant", NtCreateMutant_Hook);
	installHook("ntdll.dll", "NtOpenMutant", NtOpenMutant_Hook);

	installHook("ntdll.dll", "NtCreateEvent", NtCreateEvent_Hook);
	installHook("ntdll.dll", "NtOpenEvent", NtOpenEvent_Hook);

	installHook("ntdll.dll", "NtCreateSemaphore", NtCreateSemaphore_Hook);
	installHook("ntdll.dll", "NtOpenSemaphore", NtOpenSemaphore_Hook);

	if (IsDebug)
	{
		outfile.open(nucleusFolder + logFile, std::ios_base::app);
		outfile << date_string() << "SHOOK: Hook injection complete\n";
		outfile.close();
	}
	RhWakeUpProcess();
}

// EasyHook will be looking for this export to support DLL injection. If not found then 
// DLL injection will fail.
extern "C" void __declspec(dllexport) __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO * inRemoteInfo);

void __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo)
{
	DWORD pid = GetCurrentProcessId();
	hWnd = FindWindowFromProcessId(pid);

	BYTE* data = inRemoteInfo->UserData;
	const bool HookWindow = data[0] == 1;
	const bool RenameMutex = data[1] == 1;
	const bool SetWindow = data[2] == 1;
	IsDebug = data[3] == 1; // TODO: Is this really necessary to pass IsDebug in from C#? Better and easier to just use #ifdef Debug
	const bool BlockRaw = data[4] == 1;
	const bool CreateSingle = data[5] == 1;

	width = (data[18] << 24) + (data[19] << 16) + (data[20] << 8) + data[21];
	height = (data[22] << 24) + (data[23] << 16) + (data[24] << 8) + data[25];
	posx = (data[26] << 24) + (data[27] << 16) + (data[28] << 8) + data[29];
	posy = (data[30] << 24) + (data[31] << 16) + (data[32] << 8) + data[33];

	const size_t pathLength = (data[10] << 24) + (data[11] << 16) + (data[12] << 8) + data[13];
	auto nucleusFolderPath = static_cast<PWSTR>(malloc(pathLength + sizeof(WCHAR)));
	memcpy(nucleusFolderPath, &data[38], pathLength);
	nucleusFolderPath[pathLength / sizeof(WCHAR)] = '\0';

	nucleusFolder = nucleusFolderPath;

	const size_t targetsLength = (data[14] << 24) + (data[15] << 16) + (data[16] << 8) + data[17];
	auto targets = static_cast<PWSTR>(malloc(targetsLength + sizeof(WCHAR)));
	memcpy(targets, &data[39 + pathLength], targetsLength);
	targets[targetsLength / sizeof(WCHAR)] = '\0';

	const size_t hidLength = (data[6] << 24) + (data[7] << 16) + (data[8] << 8) + data[9];
	auto rhid = static_cast<PWSTR>(malloc(hidLength + sizeof(WCHAR)));
	memcpy(rhid, &data[40 + pathLength + targetsLength], hidLength);
	rhid[hidLength / sizeof(WCHAR)] = '\0';

	wstring rawHidWstring(rhid);
	string rawHidStr(rawHidWstring.begin(), rawHidWstring.end());

	rawHid = s2ws(rawHidStr);

	const size_t nickLength = (data[34] << 24) + (data[35] << 16) + (data[36] << 8) + data[37];
	auto nick = static_cast<PWSTR>(malloc(nickLength + sizeof(WCHAR)));
	memcpy(nick, &data[41 + pathLength + targetsLength + hidLength], nickLength);
	nick[nickLength / sizeof(WCHAR)] = '\0';

	wstring nickWstring(nick);
	string pnick(nickWstring.begin(), nickWstring.end());
	playerNick = pnick;

	//const size_t hidLength = (data[18] << 24) + (data[19] << 16) + (data[20] << 8) + data[21];
	//auto hidName = static_cast<PWSTR>(malloc(hidLength + sizeof(WCHAR)));
	//memcpy(hidName, &data[23 + pathLength], hidLength);
	//hidName[hidLength / sizeof(WCHAR)] = '\0';

	//rawHid = hidName;

	if (IsDebug)
	{
		outfile.open(nucleusFolder + logFile, std::ios_base::app);
		outfile << date_string() << "SHOOK: Starting hook injection, HookWindow: " << HookWindow << " RenameMutex: " << RenameMutex << " SetWindow: " << SetWindow << " BlockRaw: " << BlockRaw << " CreateSingle: " << CreateSingle << " RawHID: " << rawHidStr << " Width: " << width << " Height: " << height << " POSX: " << posx << " POSY: " << posy << " NICK: " << playerNick << "\n";
		outfile.close();
	}

	//installHook("kernel32", "LoadLibraryA", LoadLibraryA_Hook);
	//installHook("kernel32", "LoadLibraryExA", LoadLibraryExA_Hook);
	//installHook("kernel32", "LoadLibraryW", LoadLibraryW_Hook);
	//installHook("kernel32", "LoadLibraryExW", LoadLibraryExW_Hook);
	//installHook("user32", "RegisterUserApiHook", RegisterUserApiHook_Hook);
	//installHook("kernel32", "GetProcAddress", GetProcAddress_Hook);
	//installHook("kernel32", "ReadFile", ReadFile_Hook);
	//installHook("kernel32", "ReadFileEx", ReadFileEx_Hook);
	//installHook("kernel32", "WriteFile", WriteFile_Hook);
	//installHook("kernel32", "WriteFileEx", WriteFileEx_Hook);

	if (SetWindow)
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Injecting SetWindow hook\n";
			outfile.close();
		}
		installHook("user32", "SetWindowPos", SetWindowPos_Hook);

		if (!HookWindow && !RenameMutex && !BlockRaw)
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Hook injection complete\n";
				outfile.close();
			}
			RhWakeUpProcess();
		}
	}

	if (HookWindow)
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Injecting HookWindow hooks\n";
			outfile.close();
		}
		installHook("user32", "FindWindowA", FindWindow_Hook);
		installHook("user32", "FindWindowW", FindWindow_Hook);
		installHook("user32", "FindWindowExA", FindWindowEx_Hook);
		installHook("user32", "FindWindowExW", FindWindowEx_Hook);
		installHook("user32", "EnumWindows", EnumWindows_Hook);
		if (!RenameMutex && !BlockRaw)
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Hook injection complete\n";
				outfile.close();
			}
			RhWakeUpProcess();
		}
	}

	if (BlockRaw)
	{
		//char mbstr[256];
		//std::wcstombs(mbstr, rawHid.c_str(), 256);

		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Injecting BlockRaw hooks\n";
			outfile.close();
		}

		//installHook("user32", "GetMessageA", GetMessageA_Hook);
		//installHook("user32", "GetMessageW", GetMessageW_Hook);
		//installHook("user32", "PeekMessageA", PeekMessageA_Hook);
		//installHook("user32", "PeekMessageW", PeekMessageW_Hook);

		installHook("user32", "GetRawInputDeviceList", GetRawInputDeviceList_Hook);
		installHook("user32", "GetRegisteredRawInputDevices", GetRegisteredRawInputDevices_Hook);
		//installHook("user32", "RegisterRawInputDevices", RegisterRawInputDevices_Hook);

		/*LONG_PTR g_OldWndProc = SetWindowLongPtr(hWnd, GWLP_WNDPROC, (LONG_PTR)WndProc_Hook);*/


		if (!CreateSingle)
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Hook injection complete\n";
				outfile.close();
			}
			RhWakeUpProcess();
		}
	}

	if (CreateSingle)
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Injecting BlockRaw hooks\n";
			outfile.close();
		}

		installHook("kernel32", "CreateFileW", CreateFileW_Hook);
		installHook("kernel32", "CreateFileA", CreateFileA_Hook);

		if (!RenameMutex)
		{
			if (IsDebug)
			{
				outfile.open(nucleusFolder + logFile, std::ios_base::app);
				outfile << date_string() << "SHOOK: Hook injection complete\n";
				outfile.close();
			}
			RhWakeUpProcess();
		}
	}

	if (RenameMutex)
	{
		if (IsDebug)
		{
			outfile.open(nucleusFolder + logFile, std::ios_base::app);
			outfile << date_string() << "SHOOK: Injecting RenameMutex hooks\n";
			outfile.close();
		}
		installFindMutexHooks(targets);
	}

	return;
}