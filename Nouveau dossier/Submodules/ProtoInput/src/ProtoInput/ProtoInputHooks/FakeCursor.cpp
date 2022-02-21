#include "FakeCursor.h"
#include <windows.h>
#include "InstallHooks.h"
#include "FakeMouseKeyboard.h"
#include "HwndSelector.h"
#include <string>

namespace Proto
{

FakeCursor FakeCursor::state{};

LRESULT WINAPI FakeCursorWndProc(HWND hWnd, UINT msg, WPARAM wParam, LPARAM lParam)
{
    switch (msg)
    {
    case WM_DESTROY:
        PostQuitMessage(0);
        return 0;
    default:
    	break;
    }

    return DefWindowProc(hWnd, msg, wParam, lParam);
}

LONG fakeCursorMinX = 0, fakeCursorMaxX = 0, fakeCursorMinY = 0, fakeCursorMaxY = 0;

BOOL CALLBACK EnumWindowsProc(HMONITOR hMonitor, HDC hdcMonitor, LPRECT lprcMonitor, LPARAM dwData)
{
    MONITORINFO info = { sizeof(info) };
    if (GetMonitorInfo(hMonitor, &info))
    {
#undef min // Thanks Windows.h
#undef max
        fakeCursorMinX = std::min(fakeCursorMinX, info.rcMonitor.left);
        fakeCursorMinY = std::min(fakeCursorMinY, info.rcMonitor.top);
        fakeCursorMaxX = std::max(fakeCursorMaxX, info.rcMonitor.right);
        fakeCursorMaxY = std::max(fakeCursorMaxY, info.rcMonitor.bottom);
    }
    return true;
}

void FakeCursor::DrawCursor()
{
	//TODO: width/height probably needs to change
    constexpr int cursorWidth = 40;
    constexpr int cursorHeight = 40;

    if (oldHadShowCursor)
    {
        RECT fill{ oldX, oldY, oldX + cursorWidth, oldY + cursorHeight };
        FillRect(hdc, &fill, transparencyBrush); // Note: window, not screen coordinates!
    }

    oldHadShowCursor = showCursor;

    POINT pos = { FakeMouseKeyboard::GetMouseState().x,FakeMouseKeyboard::GetMouseState().y };
    ClientToScreen((HWND)HwndSelector::GetSelectedHwnd(), &pos);
    ScreenToClient(pointerWindow, &pos);

	if (showCursor)
	    DrawIcon(hdc, pos.x, pos.y, hCursor);
	    // DrawIconEx(hdc, pos.x, pos.y, hCursor, 0, 0, 0, transparencyBrush, DI_NORMAL | DI_COMPAT | DI_DEFAULTSIZE);
	
    oldX = pos.x;
    oldY = pos.y;
}

DWORD WINAPI FakeCursorDrawLoopThread(LPVOID lpParameter)
{
    printf("Fake cursor draw loop thread start\n");
    Proto::AddThreadToACL(GetCurrentThreadId());
    FakeCursor::state.StartDrawLoopInternal();

    return 0;
}

void FakeCursor::StartDrawLoopInternal()
{
    int tick = 0;

	while (true)
	{
        std::unique_lock<std::mutex> lock(mutex);
		conditionvar.wait(lock);

        DrawCursor();

        //TODO: is this ok? (might eat cpu)
        Sleep(drawingEnabled ? 12 : 500);

        tick = (tick + 1) % 200;

        if (tick == 0)
            // Nucleus can put the game window above the pointer without this
            SetWindowPos(pointerWindow, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSIZE);
	}
}

void FakeCursor::StartInternal()
{
    Proto::AddThreadToACL(GetCurrentThreadId());

    const auto hInstance = GetModuleHandle(NULL);

    // Like a green screen
    transparencyBrush = (HBRUSH)CreateSolidBrush(transparencyKey);

    WNDCLASS wc = { 0 };
    wc.lpfnWndProc = FakeCursorWndProc;
    wc.hInstance = hInstance;
    wc.hbrBackground = transparencyBrush;

    std::srand(std::time(nullptr));
    const std::wstring classNameStr = std::wstring{ L"ProtoInputPointer" } + std::to_wstring(std::rand());
    const wchar_t* className = classNameStr.c_str();

    wc.lpszClassName = className;
    wc.style = CS_OWNDC | CS_NOCLOSE;

    if (!RegisterClass(&wc))
    {
        fprintf(stderr, "Failed to open fake cursor window\n");
    }
    else
    {
        pointerWindow = CreateWindowExW(WS_EX_NOACTIVATE | WS_EX_NOINHERITLAYOUT | WS_EX_NOPARENTNOTIFY |
                                                          WS_EX_TOPMOST | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW | WS_EX_LAYERED,
                                                          wc.lpszClassName, classNameStr.c_str(), 0,
                                                          0, 0, 200, 200,
                                                          nullptr, nullptr, hInstance, nullptr);

        SetWindowLongW(pointerWindow, GWL_STYLE, WS_VISIBLE | WS_DISABLED);
        SetLayeredWindowAttributes(pointerWindow, transparencyKey, 0, LWA_COLORKEY);

        // Nucleus can put the game window above the pointer without this
        SetWindowPos(pointerWindow, HWND_TOPMOST, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOREDRAW | SWP_NOSIZE);

        // ShowWindow(pointerWindow, SW_SHOWDEFAULT);
        // UpdateWindow(pointerWindow);
        EnableDisableFakeCursor(drawingEnabled);

    	// Over every screen
        EnumDisplayMonitors(nullptr, nullptr, &EnumWindowsProc, 0);
        MoveWindow(pointerWindow, fakeCursorMinX, fakeCursorMinY, fakeCursorMaxX - fakeCursorMinX, fakeCursorMaxY - fakeCursorMinY, TRUE);

        hdc = GetDC(pointerWindow);

        //TODO: configurable cursor
        hCursor = LoadCursorW(NULL, IDC_ARROW);

        const auto threadHandle = CreateThread(nullptr, 0,
                              (LPTHREAD_START_ROUTINE)FakeCursorDrawLoopThread, GetModuleHandle(0), 0, 0);

        if (threadHandle != nullptr)
            CloseHandle(threadHandle);

    	// Want to avoid doing anything in the message loop that might cause it to not respond, as the entire screen will say not responding...
        MSG msg;
        ZeroMemory(&msg, sizeof(msg));
        while (msg.message != WM_QUIT)
        {
        	if (GetMessageW(&msg, pointerWindow, 0U, 0U))// Blocks
        	{
                TranslateMessage(&msg);
                DispatchMessage(&msg);
                continue;
            }
        }
    }
}

DWORD WINAPI FakeCursorThreadStart(LPVOID lpParameter)
{
    printf("Fake Cursor thread start\n");
    FakeCursor::state.StartInternal();
    return 0;
}


void FakeCursor::EnableDisableFakeCursor(bool enable)
{
    state.drawingEnabled = enable;
	
    ShowWindow(state.pointerWindow, enable ? SW_SHOWDEFAULT : SW_HIDE);
    UpdateWindow(state.pointerWindow);
}

void FakeCursor::Initialise()
{
	const auto threadHandle = CreateThread(nullptr, 0,
                 (LPTHREAD_START_ROUTINE)FakeCursorThreadStart, GetModuleHandle(0), 0, 0);

    if (threadHandle != nullptr)
        CloseHandle(threadHandle);
}

}
