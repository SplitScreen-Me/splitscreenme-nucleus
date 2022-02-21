#include "CursorVisibilityHook.h"
#include "FakeCursor.h"
#include <imgui.h>

namespace Proto
{

bool CursorVisibilityHook::ShowCursorWhenImageUpdated = false;

int WINAPI Hook_ShowCursor(BOOL bShow)
{
	FakeCursor::SetCursorVisibility(bShow == TRUE);
	if (bShow == FALSE) ShowCursor(FALSE);
	return bShow == TRUE ? 0 : -1;
}

HCURSOR WINAPI Hook_SetCursor(HCURSOR hCursor)
{
	if (CursorVisibilityHook::ShowCursorWhenImageUpdated)
	{
		// This is the original hook implementation. Required for cursors to show at all on Minecraft, for example
		
		FakeCursor::SetCursorVisibility(hCursor != nullptr);
	}
	else
	{
		// This is what the API documentation would suggest is the correct thing to do
		// Required for Unity engine games
		
		if (hCursor == nullptr)
			FakeCursor::SetCursorVisibility(false);
	}
	
	if (hCursor != nullptr)
		FakeCursor::SetCursorHandle(hCursor); // Custom cursor image
	
	return hCursor;
}

BOOL WINAPI Hook_SetSystemCursor(HCURSOR hCursor, DWORD id)
{
	if (id == 32512) // OCR_NORMAL: Standard arrow
	{
		FakeCursor::SetCursorVisibility(hCursor != nullptr);

		if (hCursor != nullptr)
			FakeCursor::SetCursorHandle(hCursor); // Custom cursor image
	}

	return TRUE;
}

void CursorVisibilityHook::ShowGuiStatus()
{
	ImGui::Text(FakeCursor::GetCursorVisibility() ? "Fake cursor is visible: true" : "Fake cursor is visible: false");

	ImGui::Separator();
	
	ImGui::Text("This option is required for the cursor to show up on some games");
	ImGui::Checkbox("Show cursor when image updated", &ShowCursorWhenImageUpdated);
}

void CursorVisibilityHook::InstallImpl()
{
	hookInfoShowCursor = std::get<1>(InstallNamedHook(L"user32", "ShowCursor", Hook_ShowCursor));
	hookInfoSetCursor = std::get<1>(InstallNamedHook(L"user32", "SetCursor", Hook_SetCursor));
	hookInfoSetSystemCursor = std::get<1>(InstallNamedHook(L"user32", "SetSystemCursor", Hook_SetSystemCursor));
}

void CursorVisibilityHook::UninstallImpl()
{
	UninstallHook(&hookInfoShowCursor);
	UninstallHook(&hookInfoSetCursor);
	UninstallHook(&hookInfoSetSystemCursor);
}

}
