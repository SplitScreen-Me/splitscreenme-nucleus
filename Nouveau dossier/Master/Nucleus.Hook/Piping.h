#pragma once
#include "pch.h"
#include <string>

namespace Piping
{
	extern std::wstring writePipeName;
	extern std::wstring readPipeName;
	extern std::wstring sharedMemName;

	extern HANDLE hPipeRead;
	extern HANDLE hPipeWrite;

	extern int* memBuf;

	void startPipeListen();
	void startSharedMem();
}