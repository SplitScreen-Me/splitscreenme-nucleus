#include "pch.h"
#include "InstallHooks.h"
#include "Logging.h"

BOOL WINAPI GetCursorInfo_Hook(PCURSORINFO pci)
{
	bool result = GetCursorInfo(pci);
    if (result == true)
    {
        POINT nypt;
        GetCursorPos(&nypt);
        pci->ptScreenPos.x = nypt.x;
        pci->ptScreenPos.y = nypt.y;
        return true;
    }
    else return false;
}

void installGetCursorInfoHook()
{

	DEBUGLOG("Injecting GetCursorInfo hook\n");
	installHook("user32", "GetCursorInfo", GetCursorInfo_Hook);
}