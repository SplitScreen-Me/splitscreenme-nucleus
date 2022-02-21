#include "Profiles.h"
#include <iostream>
#include <fstream>
#include <Windows.h>

namespace ProtoHost
{

std::filesystem::path GetProfilesFolderPath()
{
	static std::filesystem::path folderPath;

	if (static bool init = true; init)
	{
		init = false;

		wchar_t pathchars[MAX_PATH];
		GetModuleFileNameW(NULL, pathchars, MAX_PATH);
		std::wstring exeFolder = pathchars;
		size_t pos = exeFolder.find_last_of(L"\\");
		if (pos != std::string::npos)
			exeFolder = exeFolder.substr(0, pos + 1);

		folderPath = std::filesystem::path(exeFolder);
		folderPath /= "Profiles";

		std::filesystem::create_directory(folderPath);
	}

	return folderPath;
}

inline std::filesystem::path GetFilePath(const std::string& filename)
{
	// return GetProfilesFolderPath() / (filename + ".json");
	return GetProfilesFolderPath() / (filename);
}

void Profile::SaveToFile(const Profile& data, const std::string& name)
{
	std::ofstream file;
	file.open(GetFilePath(name), std::ios::out | std::ios::trunc);

	if (file.fail())
	{
		MessageBoxA(NULL, "Failed to open file", "Error", MB_OK);
	}
	else
	{
		cereal::JSONOutputArchive oarchive(file);
		oarchive(cereal::make_nvp("data", data));
	}

	file.close();
}

void Profile::LoadFromFile(Profile& data, const std::string& name)
{
	std::ifstream file;
	file.open(GetFilePath(name), std::ios::in);

	if (file.fail())
	{
		MessageBoxA(NULL, "Failed to open file", "Error", MB_OK);
	}
	else
	{
		try
		{
			cereal::JSONInputArchive iarchive(file);
			iarchive(cereal::make_nvp("data", data));
		}
		catch (...)
		{
			// Happens when you have a config from an older version that doesn't have all the options. Still works though.
		}
	}

	file.close();
}

bool Profile::DoesProfileFileAlreadyExist(const std::string& name)
{
	return exists(GetFilePath(name));
}

std::vector<std::string> Profile::GetAllProfiles()
{
	std::vector<std::string> profiles{};

	for (const auto& entry : std::filesystem::directory_iterator(GetProfilesFolderPath()))
	{
		profiles.push_back(entry.path().filename().string());
	}

	return profiles;
}

}