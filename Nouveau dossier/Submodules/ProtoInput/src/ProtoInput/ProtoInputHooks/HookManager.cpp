#include "HookManager.h"
#include "RegisterRawInputHook.h"
#include "GetRawInputDataHook.h"
#include "MessageFilterHook.h"
#include "FocusHook.h"
#include "GetCursorPosHook.h"
#include "SetCursorPosHook.h"
#include "GetKeyStateHook.h"
#include "GetAsyncKeyStateHook.h"
#include "GetKeyboardStateHook.h"
#include "CursorVisibilityHook.h"
#include "ClipCursorHook.h"
#include "RenameHandlesHook.h"
#include "XinputHook.h"
#include "DinputOrderHook.h"
#include "SetWindowPosHook.h"
#include "BlockRawInputHook.h"
#include "FindWindowHook.h"
#include "CreateSingleHIDHook.h"
#include "WindowStyleHook.h"

namespace Proto
{

HookManager HookManager::hookManagerInstance{};

HookManager::HookManager()
{
	// Do these in exactly the same order as in ProtoHookIDs
	AddHook<RegisterRawInputHook>(ProtoHookIDs::RegisterRawInputHookID);
	AddHook<GetRawInputDataHook>(ProtoHookIDs::GetRawInputDataHookID);
	AddHook<MessageFilterHook>(ProtoHookIDs::MessageFilterHookID);
	AddHook<GetCursorPosHook>(ProtoHookIDs::GetCursorPosHookID);
	AddHook<SetCursorPosHook>(ProtoHookIDs::SetCursorPosHookID);
	AddHook<GetKeyStateHook>(ProtoHookIDs::GetKeyStateHookID);
	AddHook<GetAsyncKeyStateHook>(ProtoHookIDs::GetAsyncKeyStateHookID);
	AddHook<GetKeyboardStateHook>(ProtoHookIDs::GetKeyboardStateHookID);
	AddHook<CursorVisibilityHook>(ProtoHookIDs::CursorVisibilityStateHookID);
	AddHook<ClipCursorHook>(ProtoHookIDs::ClipCursorHookID);
	AddHook<FocusHook>(ProtoHookIDs::FocusHooksHookID);
	AddHook<RenameHandlesHook>(ProtoHookIDs::RenameHandlesHookID);
	AddHook<XinputHook>(ProtoHookIDs::XinputHookID);
	AddHook<DinputOrderHook>(ProtoHookIDs::DinputOrderHookID);
	AddHook<SetWindowPosHook>(ProtoHookIDs::SetWindowPosHookID);
	AddHook<BlockRawInputHook>(ProtoHookIDs::BlockRawInputHookID);
	AddHook<FindWindowHook>(ProtoHookIDs::FindWindowHookID);
	AddHook<CreateSingleHIDHook>(ProtoHookIDs::CreateSingleHIDHookID);
	AddHook<WindowStyleHook>(ProtoHookIDs::WindowStyleHookID);
}

void HookManager::InstallHook(ProtoHookIDs hookID)
{
	if (hookID < 0 || hookID >= hookManagerInstance.hooks.size())
		std::cerr << "Trying to install hook ID " << hookID << " which is out of range" << std::endl;
	else
	{
		hookManagerInstance.hooks[hookID]->Install();
	}
}

void HookManager::UninstallHook(ProtoHookIDs hookID)
{
	if (hookID < 0 || hookID >= hookManagerInstance.hooks.size())
		std::cerr << "Trying to uninstall hook ID " << hookID << " which is out of range" << std::endl;
	else
	{
		hookManagerInstance.hooks[hookID]->Uninstall();
	}
}

bool HookManager::IsInstalled(ProtoHookIDs hookID)
{
	if (hookID < 0 || hookID >= hookManagerInstance.hooks.size())
	{
		std::cerr << "Trying to check hook ID " << hookID << " which is out of range" << std::endl;
		return false;
	}
	else
	{
		return hookManagerInstance.hooks[hookID]->IsInstalled();
	}
}

}
