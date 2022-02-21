#pragma once
#include <windows.h>

namespace ProtoHost
{

extern HWND protoHostHwnd;

void RenderImgui();

int ShowGuiImpl();

void OnInputLockChange(bool locked);

}