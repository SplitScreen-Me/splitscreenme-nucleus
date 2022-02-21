#pragma once
#include <string>
#include <utility>
#include <Windows.h>
namespace ProtoHost
{

class Instance
{
private:
	std::string GetInstanceName() const;

public:
	bool hasBeenInjected = false;
	
	bool runtime;
	unsigned long pid;
	std::wstring filepath;
	std::wstring exename;

	intptr_t keyboardHandle = -1;
	intptr_t mouseHandle = -1;

	unsigned int controllerIndex = 0;
	
	std::string instanceName;
		
	Instance(unsigned long _pid, std::wstring _exename) : runtime(true), pid(_pid), exename(std::move(_exename)), filepath(L""), instanceName(GetInstanceName()) {}
	Instance(std::wstring _filepath, std::wstring _exename) : runtime(false), pid(-1), exename(std::move(_exename)), filepath(std::move(_filepath)), instanceName(GetInstanceName()) {}
 	
};

inline std::string utf8_encode(const std::wstring& wstr)
{
	if (wstr.empty()) return std::string();
	int size_needed = WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), NULL, 0, NULL, NULL);
	std::string strTo(size_needed, 0);
	WideCharToMultiByte(CP_UTF8, 0, &wstr[0], (int)wstr.size(), &strTo[0], size_needed, NULL, NULL);
	return strTo;
}

inline std::wstring utf8_decode(const std::string& str)
{
	if (str.empty()) return std::wstring();
	int size_needed = MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), NULL, 0);
	std::wstring wstrTo(size_needed, 0);
	MultiByteToWideChar(CP_UTF8, 0, &str[0], (int)str.size(), &wstrTo[0], size_needed);
	return wstrTo;
}

inline std::string Instance::GetInstanceName() const
{
	auto str = runtime ?
		exename + L" (PID " + std::to_wstring(pid) + L")" :
		exename;

	return utf8_encode(str);
}

}
