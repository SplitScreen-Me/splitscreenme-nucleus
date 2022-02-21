#include "Loader.h"

void InstallUninstallHook(ProtoInstanceHandle instanceHandle, ProtoHookIDs hookID, bool install)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetupHook message
		{
			hookID,
			install
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetupHook, &message);
	}
}

extern "C" __declspec(dllexport) void InstallHook(ProtoInstanceHandle instanceHandle, ProtoHookIDs hookID)
{
	InstallUninstallHook(instanceHandle, hookID, true);
}

extern "C" __declspec(dllexport) void UninstallHook(ProtoInstanceHandle instanceHandle, ProtoHookIDs hookID)
{
	InstallUninstallHook(instanceHandle, hookID, false);
}