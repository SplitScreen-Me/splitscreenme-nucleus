#include "include/protoloader.h"

#include <iostream>
#include <Windows.h>
#include <BlackBone/Process/Process.h>
#include <BlackBone/Patterns/PatternSearch.h>
#include "TrackedInstances.h"

#include "pipeinclude.h"

#include "easyhook.h"
#include <filesystem>

#include "Loader.h"

extern "C" NTSTATUS __declspec(dllexport) __stdcall HookCompleteInjection(void* InInfo)
{
	return HookCompleteInjectionImpl(InInfo);
}

// extern "C" void __declspec(dllexport) __cdecl WakeupProcess()
// {
// 	RhWakeUpProcess();
// }

bool Isx64(unsigned long pid)
{
	// blackbone::Process proc;
	// proc.Attach(pid);

	SYSTEM_INFO systemInfo;
	GetNativeSystemInfo(&systemInfo);

	// const auto& barrier = proc.barrier();

	// const bool b1 = barrier.targetWow64;

	auto hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid);

	BOOL isWow64;
	IsWow64Process(hProcess, &isWow64);
	CloseHandle(hProcess);
	
	bool is32 = isWow64 || 
		(systemInfo.wProcessorArchitecture == PROCESSOR_ARCHITECTURE_INTEL); // x86;
	
	return !is32;
}

extern "C" __declspec(dllexport) void StartFocusMessageLoop(ProtoInstanceHandle instanceHandle, int milliseconds,
															bool wm_activate, bool wm_activateapp, bool wm_ncactivate, bool wm_setfocus, bool wm_mouseactivate)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageStartFocusMessageLoop message
		{
			milliseconds,
			wm_activate,
			wm_activateapp,
			wm_ncactivate,
			wm_setfocus,
			wm_mouseactivate
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::StartFocusMessageLoop, &message);
	}
}

extern "C" __declspec(dllexport) void StopFocusMessageLoop(ProtoInstanceHandle instanceHandle)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageStopFocusMessageLoop message
		{

		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::StopFocusMessageLoop, &message);
	}
}

void SetDrawFakeCursor(ProtoInstanceHandle instanceHandle, bool enable)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetDrawFakeCursor message
		{
			enable
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetDrawFakeCursor, &message);
	}
}

extern "C" __declspec(dllexport) void SetExternalFreezeFakeInput(ProtoInstanceHandle instanceHandle, bool enableFreeze)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetExternalFreezeFakeInput message
		{
			enableFreeze
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetExternalFreezeFakeInput, &message);
	}
}

void AddSelectedInputHandleImpl(ProtoInstanceHandle instanceHandle, unsigned int handle, bool mouse)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMesasgeAddSelectedMouseOrKeyboard message
		{
			mouse ? handle : -1,
			mouse ? -1 : handle
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::AddSelectedMouseOrKeyboard, &message);
	}
}

extern "C" __declspec(dllexport) void AddSelectedMouseHandle(ProtoInstanceHandle instanceHandle, unsigned int mouseHandle)
{
	AddSelectedInputHandleImpl(instanceHandle, mouseHandle, true);
}

extern "C" __declspec(dllexport) void AddSelectedKeyboardHandle(ProtoInstanceHandle instanceHandle, unsigned int keyboardHandle)
{
	AddSelectedInputHandleImpl(instanceHandle, keyboardHandle, false);
}

extern "C" __declspec(dllexport) void SetControllerIndex(ProtoInstanceHandle instanceHandle, unsigned int controllerIndex, unsigned int controllerIndex2, unsigned int controllerIndex3, unsigned int controllerIndex4)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetControllerIndex message
		{
			controllerIndex,
			controllerIndex2,
			controllerIndex3,
			controllerIndex4
			
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetControllerIndex, &message);
	}
}

extern "C" __declspec(dllexport) void SetUseDinputRedirection(ProtoInstanceHandle instanceHandle, bool useRedirection)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageUseDinput message
		{
			useRedirection
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetUseDinput, &message);
	}
}

void SetUseOpenXinput(ProtoInstanceHandle instanceHandle, bool useOpenXinput)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetUseOpenXinput message
		{
			useOpenXinput
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetUseOpenXinput, &message);
	}
}

extern "C" __declspec(dllexport) void SetupState(ProtoInstanceHandle instanceHandle, int instanceIndex)
{
	if (instanceIndex < 1)
	{
		fprintf(stderr, "Instance index out of range (must start at 1)\n");
		return;
	}

	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetupState message
		{
			instanceIndex
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetupState, &message);
	}
}

void AddHandleToRenameImpl(ProtoInstanceHandle instanceHandle, const wchar_t* name, bool namedPipe)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);


		ProtoPipe::PipeMessageAddHandleToRename message{};
		message.isNamedPipe = namedPipe;

		if (wcslen(name) > sizeof(message.buff) / sizeof(wchar_t) - 2)
		{
			fprintf(stderr, "Handle name \"%ws\" is too long and will not be captured\n", name);
			return;
		}

		memset(message.buff, 0, sizeof(message.buff));
		wcscpy_s(message.buff, name);

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::AddHandleToRename, &message);
	}
}

extern "C" __declspec(dllexport) void AddHandleToRename(ProtoInstanceHandle instanceHandle, const wchar_t* name)
{
	AddHandleToRenameImpl(instanceHandle, name, false);
}

extern "C" __declspec(dllexport) void AddNamedPipeToRename(ProtoInstanceHandle instanceHandle, const wchar_t* name)
{
	AddHandleToRenameImpl(instanceHandle, name, true);
}

extern "C" __declspec(dllexport) void SetupMessagesToSend(ProtoInstanceHandle instanceHandle, bool sendMouseWheelMessages, bool sendMouseButtonMessages, bool sendMouseMoveMessages, bool sendKeyboardPressMessages)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetupMessagesToSend message
		{
			sendMouseWheelMessages,
			sendMouseButtonMessages,
			sendMouseMoveMessages,
			sendKeyboardPressMessages
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetupMessagesToSend, &message);
	}
}

extern "C" __declspec(dllexport) void WakeUpProcess(ProtoInstanceHandle instanceHandle)
{
	ProtoPipe::PipeMessageWakeUpProcess message
	{
	};

	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
		ProtoSendPipeMessage(find->second.pipeHandle, ProtoPipe::PipeMessageType::WakeUpProcess, &message);
}

extern "C" __declspec(dllexport) void UpdateMainWindowHandle(ProtoInstanceHandle instanceHandle, uint64_t hwnd)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMesasgeUpdateMainWindowHandle message
		{
			hwnd
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::UpdateMainWindowHandle, &message);
	}
}

extern "C" __declspec(dllexport) void SetDinputDeviceGUID(ProtoInstanceHandle instanceHandle,
														  unsigned long  Data1,
														  unsigned short Data2,
														  unsigned short Data3,
														  unsigned char  Data4a,
														  unsigned char  Data4b,
														  unsigned char  Data4c,
														  unsigned char  Data4d,
														  unsigned char  Data4e,
														  unsigned char  Data4f,
														  unsigned char  Data4g,
														  unsigned char  Data4h)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetDinputDeviceGuid message
		{
			GUID
			{
				Data1,
				Data2,
				Data3,
				{ Data4a, Data4b, Data4c, Data4d, Data4e, Data4f, Data4g, Data4h}
			}
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetDinputDeviceGuid, &message);
	}
}

void DinputHookAlsoHooksGetDeviceState(ProtoInstanceHandle instanceHandle, bool enable)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetDinputHookGetDeviceState message
		{
			enable
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetDinputHookGetDeviceState, &message);
	}
}

void SetSetWindowPosSettings(ProtoInstanceHandle instanceHandle, int posx, int posy, int width, int height)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetSetWindowPosSettings message
		{
			posx, posy, width, height
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetSetWindowPosSettings, &message);
	}
}

void SetCreateSingleHIDName(ProtoInstanceHandle instanceHandle, const wchar_t* name)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);


		ProtoPipe::PipeMessageSetCreateSingleHIDName message{};

		memset(message.buff, 0, sizeof(message.buff));

		if (name != nullptr)
		{
			if (wcslen(name) > sizeof(message.buff) / sizeof(wchar_t) - 2)
			{
				fprintf(stderr, "HID name \"%ws\" is too long and will not be sent\n", name);
				return;
			}
			
			wcscpy_s(message.buff, name);
		}

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetCreateSingleHIDName, &message);
	}
}

void SetCursorClipOptions(ProtoInstanceHandle instanceHandle, bool useFakeClipCursor)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetClipCursorHookOptions message
		{
			useFakeClipCursor
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetClipCursorHookOptions, &message);
	}
}

void AllowFakeCursorOutOfBounds(ProtoInstanceHandle instanceHandle, bool allowOutOfBounds, bool extendBounds)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetAllowFakeCursorOutOfBounds message
		{
			allowOutOfBounds,
			extendBounds
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetAllowFakeCursorOutOfBounds, &message);
	}
}

void SetToggleFakeCursorVisibilityShortcut(ProtoInstanceHandle instanceHandle, bool enabled, unsigned vkey)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetToggleCursorVisibilityShortcut message
		{
			enabled,
			vkey
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetToggleCursorVisibilityShortcut, &message);
	}
}

void SetRawInputBypass(ProtoInstanceHandle instanceHandle, bool enabled)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetRawInputBypass message
		{
			enabled
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetRawInputBypass, &message);
	}
}

void SetShowCursorWhenImageUpdated(ProtoInstanceHandle instanceHandle, bool enabled)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageShowCursorWhenImageUpdated message
		{
			enabled
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetShowCursorWhenImageUpdated, &message);
	}
}
