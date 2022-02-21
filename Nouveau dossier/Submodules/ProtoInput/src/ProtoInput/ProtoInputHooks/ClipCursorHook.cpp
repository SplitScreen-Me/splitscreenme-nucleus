#include "ClipCursorHook.h"
#include <imgui.h>
#include "FakeMouseKeyboard.h"
#include "HwndSelector.h"

namespace Proto
{

bool ClipCursorHook::useFakeCursorClip = true;

BOOL WINAPI Hook_ClipCursor(const RECT* lpRect)
{
	if (ClipCursorHook::GetUseFakeCursorClip())
	{
		if (lpRect == nullptr)
			FakeMouseKeyboard::RemoveClipCursor();
		else
		{
			const auto hwnd = (HWND)HwndSelector::GetSelectedHwnd();
			RECT clip = *lpRect;
			ScreenToClient(hwnd, (POINT*)&clip);
			ScreenToClient(hwnd, (POINT*)&clip + 1);
			FakeMouseKeyboard::SetClipCursor(clip.left, clip.top, clip.right, clip.bottom);
		}
	}
	
	return TRUE;
}

BOOL WINAPI Hook_GetClipCursor(LPRECT lpRect)
{
	const auto& fakeMouseState = FakeMouseKeyboard::GetMouseState();
	
	if (ClipCursorHook::GetUseFakeCursorClip() && fakeMouseState.hasClipCursor && lpRect != nullptr)
	{
		lpRect->left = fakeMouseState.clipClientLeft;
		lpRect->right = fakeMouseState.clipClientRight;
		lpRect->top = fakeMouseState.clipClientTop;
		lpRect->bottom = fakeMouseState.clipClientBottom;
		
		const auto hwnd = (HWND)HwndSelector::GetSelectedHwnd();
		ClientToScreen(hwnd, (POINT*)lpRect);
		ClientToScreen(hwnd, (POINT*)lpRect + 1);
				
		return TRUE;
	}
	else
	{
		return GetClipCursor(lpRect);
	}
}

void ClipCursorHook::ShowGuiStatus()
{
	{
		bool _useFakeCursorClip = useFakeCursorClip;
		if (ImGui::Checkbox("Use fake cursor clip", &_useFakeCursorClip))
		{
			SetUseFakeCursorClip(_useFakeCursorClip);
		}
	}

	if (useFakeCursorClip)
	{
		const auto& fakeMouseState = FakeMouseKeyboard::GetMouseState();

		if (fakeMouseState.hasClipCursor)
		{
			ImGui::Text("Fake clip rect: (%d, %d) to (%d, %d)",
						fakeMouseState.clipClientLeft, fakeMouseState.clipClientTop, fakeMouseState.clipClientRight, fakeMouseState.clipClientBottom);
		}
		else
			ImGui::Text("No fake clip rect set");
	}
}

void ClipCursorHook::InstallImpl()
{
	hookInfoClipCursor = std::get<1>(InstallNamedHook(L"user32", "ClipCursor", Hook_ClipCursor));
	hookInfoGetClipCursor = std::get<1>(InstallNamedHook(L"user32", "GetClipCursor", Hook_GetClipCursor));
}

void ClipCursorHook::UninstallImpl()
{
	UninstallHook(&hookInfoClipCursor);
	UninstallHook(&hookInfoGetClipCursor);
}

void ClipCursorHook::SetUseFakeCursorClip(bool enable)
{
	useFakeCursorClip = enable;
	
	if (!enable)
		FakeMouseKeyboard::RemoveClipCursor();
}

}
