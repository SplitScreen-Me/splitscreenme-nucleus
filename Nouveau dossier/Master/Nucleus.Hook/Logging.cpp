#include "pch.h"
#include <fstream>
#include <locale>
#include <ctime>
#include "Logging.h"

namespace Logging
{
	std::ofstream loggingOutfile;
	std::wstring nucleusFolder;

	std::string Logging::dateString()
	{
		tm tinfo;
		time_t rawtime;
		std::time(&rawtime);
		localtime_s(&tinfo, &rawtime);
		char buffer[21];
		strftime(buffer, 21, "%Y-%m-%d %H:%M:%S", &tinfo);
		return "[" + std::string(buffer) + "]";
	}
}