#pragma once
#include <unordered_map>
#include "include/protoloader.h"
#include <windows.h>

namespace Proto
{

class ProtoInstance
{
public:
	HANDLE pipeHandle = nullptr;
	bool clientConnected = false;
	unsigned long pid = 0;
};

extern std::unordered_map<ProtoInstanceHandle, ProtoInstance> instances;
extern ProtoInstanceHandle instanceCounter;

}