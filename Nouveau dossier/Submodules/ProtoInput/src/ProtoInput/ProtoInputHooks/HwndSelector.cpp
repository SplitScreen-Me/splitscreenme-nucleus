#include "Gui.h"
#include "HwndSelector.h"
#include <cstdio>
#include "FakeCursor.h"


namespace Proto
{

intptr_t HwndSelector::selectedHwnd = 0;
int HwndSelector::windowWidth, HwndSelector::windowHeight;

struct HandleData
{
    unsigned long pid;
    HWND hwnd;
};

BOOL IsMainWindow(HWND handle)
{
	// Is top level & visible & not one of ours
    return
		GetWindow(handle, GW_OWNER) == (HWND)0 && 
        IsWindowVisible(handle) &&
        handle != (HWND)Proto::ConsoleHwnd &&
        handle != Proto::ProtoGuiHwnd &&
        handle != FakeCursor::GetPointerWindow();
}

BOOL CALLBACK EnumWindowsCallback(HWND handle, LPARAM lParam)
{
    HandleData& data = *(HandleData*)lParam;
	
    DWORD pid = 0;
    GetWindowThreadProcessId(handle, &pid);
	
    if (data.pid != pid || !IsMainWindow(handle))
        return TRUE; // Keep searching
	
    data.hwnd = handle;
	
    return FALSE;
}

void HwndSelector::UpdateMainHwnd(bool logOutput)
{
    // Go through all the top level windows, select the first that's visible & belongs to the process
	
    HandleData data { GetCurrentProcessId(), nullptr };
    EnumWindows(EnumWindowsCallback, (LPARAM)&data);

    const auto hwnd = (intptr_t)data.hwnd;

    if (logOutput)
    {
        if (hwnd == 0)
            printf("Warning: UpdateMainHwnd didn't find a main window\n");
        else
            printf("UpdateMainHwnd found hwnd %d (0x%X)\n", hwnd, hwnd);
    }
		
    if (data.hwnd != nullptr)
        selectedHwnd = (intptr_t)data.hwnd;
}

void HwndSelector::UpdateWindowBounds()
{
    RECT rect;
    if (GetClientRect((HWND)selectedHwnd, &rect))
    {
        windowWidth = rect.right - rect.left;
        windowHeight = rect.bottom - rect.top;
    }
    else
        fprintf(stderr, "GetClientRect failed in update main window bounds\n");
}

void HwndSelector::SetSelectedHwnd(intptr_t set)
{
    selectedHwnd = set;
    UpdateWindowBounds();
}

}
