#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"

int width;
int height;
int posx;
int posy;

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
	//DEBUGLOG(" width: " << width <<
	//	" height: " << height <<
	//	" posx: " << posx <<
	//	" posy: " << posy <<
	//	"\n");
	bool result = SetWindowPos(hWnd, hWndInsertAfter, posx, posy, width, height, uFlags);

	return result;
}

void installSetWindowHook(int inWidth, int inHeight, int inPosx, int inPosy)
{
	width = inWidth;
	height = inHeight;
	posx = inPosx;
	posy = inPosy;

	DEBUGLOG("Injecting SetWindow hook\n");
	installHook("user32", "SetWindowPos", SetWindowPos_Hook);
}