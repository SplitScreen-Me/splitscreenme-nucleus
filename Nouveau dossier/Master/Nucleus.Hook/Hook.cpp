#include "pch.h"
#include "easyhook.h"
#include "framework.h"
#include "windows.h"
#include <locale>
#include <iomanip>
#include "Logging.h"
#include "InstallHooks.h"
#include "Globals.h"
#include "Piping.h"
#include "FakeMouse.h"
#include "ReRegisterRawInput.h"
#include "Controller.h"

#ifdef _DEBUG
bool IsDebug = false;
#else
bool IsDebug = true;
#endif

//Globals.h
namespace Globals
{
	Options options;
	HWND hWnd = nullptr;
}

// Structure used to communicate data from and to enumeration procedure
struct EnumData {
	DWORD dwProcessId;
	HWND hWnd;
};

// Application-defined callback for EnumWindows
BOOL CALLBACK EnumProc(HWND hWnd, LPARAM lParam) {
	// Retrieve storage location for communication data
	EnumData& ed = *reinterpret_cast<EnumData*>(lParam);
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
	if (!EnumWindows(EnumProc, reinterpret_cast<LPARAM>(&ed)) &&
		(GetLastError() == ERROR_SUCCESS)) {
		return ed.hWnd;
	}
	return NULL;
}

// Helper method for convenience
HWND FindWindowFromProcess(HANDLE hProcess) {
	return FindWindowFromProcessId(GetProcessId(hProcess));
}

NTSTATUS installHookEx(const LPCSTR moduleHandle, const LPCSTR proc, void* callBack, const bool isOrdinal)
{
	// Perform hooking
	HOOK_TRACE_INFO hHook = { NULL }; // keep track of our hook

	// Install the hook
	const auto result = LhInstallHook(
		static_cast<void*>(GetProcAddress(GetModuleHandle(moduleHandle), proc)),
		callBack,
		NULL,
		&hHook);

	// Don't treat proc as an actual pointer to a string if it used as an ordinal.
	const LPCSTR name = isOrdinal ? "ORDINAL" : proc;
	
	if (FAILED(result))
	{
		DEBUGLOG("Error installing " << name << " hook, error msg: " << RtlGetLastErrorString() << "\n")
	}
	else
	{
		// If the threadId in the ACL is set to 0,
		// then internally EasyHook uses GetCurrentThreadId()
		ULONG ACLEntries[1] = { 0 };

		// Disable the hook for the provided threadIds, enable for all others
		const auto ACLres = LhSetExclusiveACL(ACLEntries, 1, &hHook);

		if (FAILED(ACLres))
		{
			DEBUGLOG("Error setting ACL for " << name << " hook, ACLres = " << ACLres << ", error msg: " << RtlGetLastErrorString() << "\n");
		}
		else
		{
			DEBUGLOG("Successfully installed " << name << " hook, in module: " << moduleHandle << ", result: " << result << "\n");
		}
	}

	return result;
}

NTSTATUS installHook(const LPCSTR moduleHandle, const LPCSTR proc, void* callBack)
{
	return installHookEx(moduleHandle, proc, callBack, false);
}

// EasyHook will be looking for this export to support DLL injection. If not found then 
// DLL injection will fail.
extern "C" void __declspec(dllexport) __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo);

void __stdcall NativeInjectionEntryPoint(REMOTE_ENTRY_INFO* inRemoteInfo)
{
	const auto pid = GetCurrentProcessId();

	Globals::hWnd = FindWindowFromProcessId(pid);

#pragma region Get data from UserData
	BYTE* data = inRemoteInfo->UserData;
	auto p = data;

	if (reinterpret_cast<INT>(Globals::hWnd) == 0)
	{
		Globals::hWnd = reinterpret_cast<HWND>(bytesToInt(p));
	}
	p += 4;

	FakeMouse::allowedMouseHandle = reinterpret_cast<HANDLE>(bytesToInt(p));
	p += 4;

	FakeMouse::allowedKeyboardHandle = reinterpret_cast<HANDLE>(bytesToInt(p));
	p += 4;

#define NEXTBOOL *(p++) == 1
	Globals::options.preventWindowDeactivation = NEXTBOOL;
	Globals::options.setWindow = NEXTBOOL;
	IsDebug = NEXTBOOL;
	Globals::options.hideCursor = NEXTBOOL;
	Globals::options.hookFocus = NEXTBOOL;
	Globals::options.setCursorPos = NEXTBOOL;
	Globals::options.getCursorPos = NEXTBOOL;
	Globals::options.getKeyState = NEXTBOOL;
	Globals::options.getAsyncKeyState = NEXTBOOL;
	Globals::options.getKeyboardState = NEXTBOOL;
	Globals::options.filterRawInput = NEXTBOOL;
	Globals::options.filterMouseMessages = NEXTBOOL;
	Globals::options.legacyInput = NEXTBOOL;
	Globals::options.updateAbsoluteFlagInMouseMessage = NEXTBOOL;
	Globals::options.mouseVisibilitySendBack = NEXTBOOL;
	Globals::options.reRegisterRawInput = NEXTBOOL;
	Globals::options.reRegisterRawInputMouse = NEXTBOOL;
	Globals::options.reRegisterRawInputKeyboard = NEXTBOOL;
	Globals::options.HookXInput = NEXTBOOL;
	Globals::options.DinputToXinputTranslation = NEXTBOOL;
#undef NEXTBOOL

	const auto pathLength = static_cast<size_t>(bytesToInt(p)); p += 4;
	const auto writePipeNameLength = static_cast<size_t>(bytesToInt(p)); p += 4;
	const auto readPipeNameLength = static_cast<size_t>(bytesToInt(p)); p += 4;

	const int windowWidth = bytesToInt(p); p += 4;
	const int windowHeight = bytesToInt(p); p += 4;
	const int windowPosX = bytesToInt(p); p += 4;
	const int windowPosY = bytesToInt(p); p += 4;

	Controller::controllerIndex = bytesToInt(p); p += 4;
	
	//C# gives number of bytes without null termination
	const auto nucleusFolderPath = static_cast<PWSTR>(malloc(pathLength + sizeof(WCHAR)));
	memcpy(nucleusFolderPath, p, pathLength);
	p += pathLength;
	nucleusFolderPath[pathLength / sizeof(WCHAR)] = '\0';//Null-terminate the string
	Logging::nucleusFolder = nucleusFolderPath;

	Piping::writePipeName = std::wstring(reinterpret_cast<wchar_t*>(p), writePipeNameLength/2);
	p += writePipeNameLength;

	Piping::readPipeName = std::wstring(reinterpret_cast<wchar_t*>(p), readPipeNameLength/2);
	Piping::sharedMemName = Piping::readPipeName + L"_mem";
	p += readPipeNameLength;
#pragma endregion 

	//Should be before hooks start to avoid errors
	Piping::startSharedMem();

	DEBUGLOG("Starting hook injection," <<
		" hWnd: " << Globals::hWnd <<
		" WritePipeName: " << std::string(Piping::writePipeName.begin(), Piping::writePipeName.end()) <<
		" ReadPipeName: " << std::string(Piping::readPipeName.begin(), Piping::readPipeName.end()) <<
		" AllowedRawMouseHandle: " << FakeMouse::allowedMouseHandle <<
		" AllowedRawKeyboardHandle: " << FakeMouse::allowedKeyboardHandle <<
		" preventWindowDeactivation: " << Globals::options.preventWindowDeactivation <<
		" setCursorPos: " << Globals::options.setCursorPos <<
		" getCursorPos: " << Globals::options.getCursorPos <<
		" getKeyState: " << Globals::options.getKeyState <<
		" getAsyncKeyState: " << Globals::options.getAsyncKeyState <<
		" getKeyboardState: " << Globals::options.getKeyboardState <<
		" filterRawInput: " << Globals::options.filterRawInput <<
		" filterMouseMessages: " << Globals::options.filterMouseMessages <<
		" legacyInput: " << Globals::options.legacyInput <<
		" updateAbsoluteFlagInMouseMessage: " << Globals::options.updateAbsoluteFlagInMouseMessage <<
		" setWindow: " << Globals::options.setWindow <<
		" hideCursor: " << Globals::options.hideCursor <<
		" hookFocus: " << Globals::options.hookFocus <<
		" mouseVisibilitySendBack: " << Globals::options.mouseVisibilitySendBack <<
		" reRegisterRawInput: " << Globals::options.reRegisterRawInput <<
		" reRegisterRawInputMouse: " << Globals::options.reRegisterRawInputMouse <<
		" reRegisterRawInputKeyboard: " << Globals::options.reRegisterRawInputKeyboard <<
		" hookXinput: " << Globals::options.HookXInput <<
		" DinputToXinputTranslation: " << Globals::options.DinputToXinputTranslation <<
		" controllerIndex: " << Controller::controllerIndex <<
		" windowWidth: " << windowWidth <<
		" windowHeight: " << windowHeight <<
		" windowPosX: " << windowPosX <<
		" windowPosY: " << windowPosY <<
		"\n");

#pragma region Inject hooks
	if (Globals::options.setWindow)
		installSetWindowHook(windowWidth, windowHeight, windowPosX, windowPosY);

	if (Globals::options.hookFocus)
		installFocusHooks();

	if (Globals::options.hideCursor || Globals::options.mouseVisibilitySendBack)
		installHideCursorHooks();

	if (Globals::options.getCursorPos)
		installGetCursorPosHook();

	if (Globals::options.setCursorPos)
		installSetCursorPosHook();

	if (Globals::options.getAsyncKeyState)
		installGetAsyncKeyStateHook();

	if (Globals::options.getKeyState)
		installGetKeyStateHook();

	if (Globals::options.getKeyboardState)
		installGetKeyboardStateHook();

	if (Globals::options.HookXInput)
		installXInputHooks();

	if (Globals::options.reRegisterRawInput)
	{
		//Needs to be before message filter.
		ReRegisterRawInput::SetupReRegisterRawInput();
	}
	
	if (Globals::options.filterRawInput || Globals::options.filterMouseMessages || Globals::options.legacyInput || Globals::options.preventWindowDeactivation || Globals::options.reRegisterRawInput)
	{
		installMessageFilterHooks();
	}
#pragma endregion
	
	DEBUGLOG("Hook injection complete\n");

	Piping::startPipeListen();
}