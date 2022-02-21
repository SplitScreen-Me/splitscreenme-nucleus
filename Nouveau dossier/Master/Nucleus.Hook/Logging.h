#pragma once
#include <fstream>

namespace Logging
{
	extern std::ofstream loggingOutfile;
	extern std::wstring nucleusFolder;
	const std::wstring LOG_FILE = L"\\debug-log-hooks.txt";//Use different path to C# side or it can crash if writing at the same time

	std::string dateString();
}

#ifdef _DEBUG
#define DEBUGLOG(x) \
{Logging::loggingOutfile.open(Logging::nucleusFolder + Logging::LOG_FILE, std::ios_base::app); \
Logging::loggingOutfile << Logging::dateString() << " " << x; \
Logging::loggingOutfile.flush(); \
Logging::loggingOutfile.close();}
#else
#define DEBUGLOG(x) ;
#endif