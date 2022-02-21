#include "Loader.h"

void EnableDisableMessageFilter(ProtoInstanceHandle instanceHandle, ProtoMessageFilterIDs filterID, bool enable)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetupMessageFilter message
		{
			filterID,
			enable
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetupMessageFilter, &message);
	}
}

extern "C" __declspec(dllexport) void EnableMessageFilter(ProtoInstanceHandle instanceHandle, ProtoMessageFilterIDs filterID)
{
	EnableDisableMessageFilter(instanceHandle, filterID, true);
}

extern "C" __declspec(dllexport) void DisableMessageFilter(ProtoInstanceHandle instanceHandle, ProtoMessageFilterIDs filterID)
{
	EnableDisableMessageFilter(instanceHandle, filterID, false);
}

void EnableDisableMessageBlock(ProtoInstanceHandle instanceHandle, unsigned int messageID, bool block)
{
	if (const auto find = Proto::instances.find(instanceHandle); find != Proto::instances.end())
	{
		auto& instance = find->second;

		WaitClientConnect(instance);

		ProtoPipe::PipeMessageSetupMessageBlock message
		{
			messageID,
			block
		};

		ProtoSendPipeMessage(instance.pipeHandle, ProtoPipe::PipeMessageType::SetupMessageBlock, &message);
	}
}

extern "C" __declspec(dllexport) void EnableMessageBlock(ProtoInstanceHandle instanceHandle, unsigned int messageID)
{
	EnableDisableMessageBlock(instanceHandle, messageID, true);
}

extern "C" __declspec(dllexport) void DisableMessageBlock(ProtoInstanceHandle instanceHandle, unsigned int messageID)
{
	EnableDisableMessageBlock(instanceHandle, messageID, false);
}
