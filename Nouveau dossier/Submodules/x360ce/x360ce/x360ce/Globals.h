#pragma once

class Globals
{
public:
	static bool dInputEnabled;
	static DWORD dInputLibrary;
	static GUID dInputPlayerGuid;
	static bool dInputForceDisable;

	static bool xInputEnabled;
	static DWORD xInputPlayerId;
	static bool xInputReRouteEnabled;
	static DWORD xInputReRouteTemplate;

	static bool blockInputEvents;
	static bool blockMouseEvents;
	static bool blockKeyboardEvents;

	static bool enableMKBInput;

	static bool forceFocus;
	static std::wstring* forceFocusWindowName;

	static DWORD windowY;
	static DWORD windowX;
	static DWORD resWidth;
	static DWORD resHeight;

	static bool hasHooked;
	static bool hasSetWindow;
};