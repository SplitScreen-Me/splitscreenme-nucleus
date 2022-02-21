#include "FindWindowHook.h"

namespace Proto
{

HWND WINAPI Hook_FindWindowA(LPCSTR lpClassName, LPCSTR lpWindowName)
{
	return NULL;
}

HWND WINAPI Hook_FindWindowW(LPCWSTR lpClassName, LPCWSTR lpWindowName)
{
	return NULL;
}

HWND WINAPI Hook_FindWindowExA(HWND hWndParent, HWND hWndChildAfter, LPCSTR lpszClass, LPCSTR lpszWindow)
{
	return NULL;
}

HWND WINAPI Hook_FindWindowExW(HWND hWndParent, HWND hWndChildAfter, LPCWSTR lpszClass, LPCWSTR lpszWindow)
{
	return NULL;
}

BOOL WINAPI Hook_EnumWindows(WNDENUMPROC lpEnumFunc, LPARAM lParam)
{
	return TRUE;
}

void FindWindowHook::InstallImpl()
{
	hookInfoFindWindowA = std::get<1>(InstallNamedHook(L"user32", "FindWindowA", Hook_FindWindowA));
	hookInfoFindWindowW = std::get<1>(InstallNamedHook(L"user32", "FindWindowW", Hook_FindWindowW));
	hookInfoFindWindowExA = std::get<1>(InstallNamedHook(L"user32", "FindWindowExA", Hook_FindWindowExA));
	hookInfoFindWindowExW = std::get<1>(InstallNamedHook(L"user32", "FindWindowExW", Hook_FindWindowExW));
	hookInfoEnumWindows = std::get<1>(InstallNamedHook(L"user32", "EnumWindows", Hook_EnumWindows));
}

void FindWindowHook::UninstallImpl()
{
	UninstallHook(&hookInfoFindWindowA);
	UninstallHook(&hookInfoFindWindowW);
	UninstallHook(&hookInfoFindWindowExA);
	UninstallHook(&hookInfoFindWindowExW);
	UninstallHook(&hookInfoEnumWindows);
}

}