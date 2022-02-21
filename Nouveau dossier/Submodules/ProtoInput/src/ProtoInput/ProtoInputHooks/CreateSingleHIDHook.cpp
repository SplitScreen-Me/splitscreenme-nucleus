#include "CreateSingleHIDHook.h"
#include <string>
#include <imgui.h>

namespace Proto
{

bool filterSet = false;
std::wstring rawHidW { L"None" };
std::string rawHidA { "None" };

bool compareCharA(char& c1, char& c2)
{
	return (c1 == c2) || (std::toupper(c1) == std::toupper(c2));
}

inline bool caseInSensStringCompareA(std::string& str1, std::string& str2)
{
	return ((str1.size() == str2.size()) && std::equal(str1.begin(), str1.end(), str2.begin(), &compareCharA));
}

bool compareCharW(wchar_t& c1, wchar_t& c2)
{
	return (c1 == c2) || (std::toupper(c1) == std::toupper(c2));
}

inline bool caseInSensStringCompareW(std::wstring& str1, std::wstring& str2)
{
	return ((str1.size() == str2.size()) && std::equal(str1.begin(), str1.end(), str2.begin(), &compareCharW));
}

HANDLE Hook_CreateFileW(
	LPCWSTR               lpFileName,
	DWORD                 dwDesiredAccess,
	DWORD                 dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes,
	DWORD                 dwCreationDisposition,
	DWORD                 dwFlagsAndAttributes,
	HANDLE                hTemplateFile
)
{
	if (std::wstring fileName(lpFileName);
		filterSet &&
		(fileName.find(L"HID") != std::string::npos || fileName.find(L"hid") != std::string::npos) &&
		!caseInSensStringCompareW(fileName, rawHidW))
	{
		return INVALID_HANDLE_VALUE;
	}
	
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

HANDLE Hook_CreateFileA(
	LPCSTR				  lpFileName,
	DWORD                 dwDesiredAccess,
	DWORD                 dwShareMode,
	LPSECURITY_ATTRIBUTES lpSecurityAttributes,
	DWORD                 dwCreationDisposition,
	DWORD                 dwFlagsAndAttributes,
	HANDLE                hTemplateFile
)
{
	if (std::string fileName(lpFileName);
		filterSet &&
		(fileName.find("HID") != std::string::npos || fileName.find("hid") != std::string::npos) &&
		!caseInSensStringCompareA(fileName, rawHidA))
	{
		return INVALID_HANDLE_VALUE;
	}

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

void CreateSingleHIDHook::ShowGuiStatus()
{
	ImGui::Checkbox("Filter enabled", &filterSet);

	static char buff[1024] = "";
	strcpy(buff, rawHidA.c_str());
	if (ImGui::InputText("Raw HID", &buff[0], sizeof(buff)/ sizeof(char)))
	{
		SetRawHidA(&buff[0]);
	}

	ImGui::Text("Saved Raw HID (A): %s", rawHidA.c_str());
	ImGui::Text("Saved Raw HID (W): %ws", rawHidW.c_str());
}

void CreateSingleHIDHook::InstallImpl()
{
	hookInfoCreateFileA = std::get<1>(InstallNamedHook(L"kernel32", "CreateFileA", Hook_CreateFileA));
	hookInfoCreateFileW = std::get<1>(InstallNamedHook(L"kernel32", "CreateFileW", Hook_CreateFileW));
}

void CreateSingleHIDHook::UninstallImpl()
{
	UninstallHook(&hookInfoCreateFileA);
	UninstallHook(&hookInfoCreateFileW);
}

//https://codereview.stackexchange.com/questions/419/converting-between-stdwstring-and-stdstring
std::wstring s2ws(const std::string& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, 0, 0);
	wchar_t* buf = new wchar_t[len];
	MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, buf, len);
	std::wstring r(buf);
	delete[] buf;
	return r;
}

std::string ws2s(const std::wstring& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = WideCharToMultiByte(CP_ACP, 0, s.c_str(), slength, 0, 0, 0, 0);
	char* buf = new char[len];
	WideCharToMultiByte(CP_ACP, 0, s.c_str(), slength, buf, len, 0, 0);
	std::string r(buf);
	delete[] buf;
	return r;
}

void CreateSingleHIDHook::SetRawHidA(const char* name)
{
	filterSet = true;
	rawHidA = std::string{ name };
	rawHidW = s2ws(rawHidA);
}

void CreateSingleHIDHook::SetRawHidW(const wchar_t* name)
{
	filterSet = true;
	rawHidW = std::wstring{ name };
	rawHidA = ws2s(rawHidW);
}

}
