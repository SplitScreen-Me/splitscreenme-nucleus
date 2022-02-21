#pragma once
#include <iostream>

namespace Proto
{

class Hook
{
private:
	bool isInstalled = false;

	virtual void InstallImpl(){};
	virtual void UninstallImpl(){};
		
public:
	virtual ~Hook() = default;
	virtual const char* GetHookName() const = 0;
	virtual const char* GetHookDescription() const = 0;
	virtual bool HasGuiStatus() const { return false; }
	virtual void ShowGuiStatus() {}
	
	void Install();
	void Uninstall();

	bool IsInstalled() const;
};


inline void Hook::Install()
{
	// std::cout << "Installing hook " << GetHookName() << std::endl;
	isInstalled = true;

	InstallImpl();
}

inline void Hook::Uninstall()
{
	// std::cout << "Uninstalling hook " << GetHookName() << std::endl;
	isInstalled = false;

	UninstallImpl();
}

inline bool Hook::IsInstalled() const
{
	return isInstalled;
}

}
