#include "DinputOrderHook.h"
#include <dinput.h>
#include <imgui.h>
#include "Gui.h"

// Warning: this file is GENUINELY disgusting

namespace Proto
{

// NOTE: Calling DirectInputCreate on dinput.dll and getting function will give SAME address as calling DirectInput8Create on dinput8.dll
//  (dinput.dll will load dinput8.dll)

std::vector<HookInfo> DinputOrderHook::hookInfos{};

LONG dinputRangeMax = 32767;
LONG dinputRangeMin = -32768;
bool hasSetupDinputDeviceProperties8W = false;
bool hasSetupDinputDeviceProperties8A = false;
bool hasSetupDinputDeviceProperties7W = false;
bool hasSetupDinputDeviceProperties7A = false;

int createDeviceHook8WCounter = 0;
int createDeviceHook8ACounter = 0;
int createDeviceHook7WCounter = 0;
int createDeviceHook7ACounter = 0;

int getState8WCounter = 0;
int getState8ACounter = 0;
int getState7WCounter = 0;
int getState7ACounter = 0;

IDirectInput7W* pDinput7W = nullptr;
IDirectInput7A* pDinput7A = nullptr;
IDirectInput8W* pDinput8W = nullptr;
IDirectInput8A* pDinput8A = nullptr;

LPDIRECTINPUTDEVICEW pCtrlr7W = nullptr;
LPDIRECTINPUTDEVICEA pCtrlr7A = nullptr;
IDirectInputDevice8W* pCtrlr8W = nullptr;
IDirectInputDevice8A* pCtrlr8A = nullptr;
int ctrlrFormat7W = 2;
int ctrlrFormat7A = 2;
int ctrlrFormat8W = 2;
int ctrlrFormat8A = 2;

bool blockDinput = true;
GUID selectedControllerGuid{};

bool waitingForHookInstallToGetControllers = false;
bool waitingForControllersToInstall = false;

bool enableGetDeviceStateHook = false;

void InstallControllerHooks();

BOOL CALLBACK DIEnumDeviceObjectsCallback8W(LPCDIDEVICEOBJECTINSTANCEW lpddoi, LPVOID pvRef)
{
	auto did = (IDirectInputDevice8W*)pvRef;
	did->Unacquire();

	DIPROPRANGE range;
	range.lMax = dinputRangeMax;
	range.lMin = dinputRangeMin;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(did->SetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;
	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallback8A(LPCDIDEVICEOBJECTINSTANCEA lpddoi, LPVOID pvRef)
{
	auto did = (IDirectInputDevice8A*)pvRef;
	did->Unacquire();

	DIPROPRANGE range;
	range.lMax = dinputRangeMax;
	range.lMin = dinputRangeMin;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(did->SetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;
	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallback7W(const DIDEVICEOBJECTINSTANCEW* lpddoi, LPVOID pvRef)
{
	auto did = (IDirectInputDeviceW*)pvRef;
	did->Unacquire();

	DIPROPRANGE range;
	range.lMax = dinputRangeMax;
	range.lMin = dinputRangeMin;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(did->SetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;
	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallback7A(const DIDEVICEOBJECTINSTANCEA* lpddoi, LPVOID pvRef)
{
	auto did = (IDirectInputDeviceA*)pvRef;
	did->Unacquire();

	DIPROPRANGE range;
	range.lMax = dinputRangeMax;
	range.lMin = dinputRangeMin;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(did->SetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;
	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallbackCopyProperties8W(LPCDIDEVICEOBJECTINSTANCEW lpddoi, LPVOID pvRef)
{
	auto theirDevice = (IDirectInputDevice8W*)pvRef;

	DIPROPRANGE range;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(theirDevice->GetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;

	dinputRangeMax = range.lMax;
	dinputRangeMin = range.lMin;

	//seems to return min = 0 and max doubled. Correct for that here:
	if (range.lMin == 0)
	{
		dinputRangeMax = range.lMax / 2;
		dinputRangeMin = -(range.lMax / 2) - 1;
	}

	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallbackCopyProperties8A(LPCDIDEVICEOBJECTINSTANCEA lpddoi, LPVOID pvRef)
{
	auto theirDevice = (IDirectInputDevice8A*)pvRef;

	DIPROPRANGE range;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(theirDevice->GetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;

	dinputRangeMax = range.lMax;
	dinputRangeMin = range.lMin;

	//seems to return min = 0 and max doubled. Correct for that here:
	if (range.lMin == 0)
	{
		dinputRangeMax = range.lMax / 2;
		dinputRangeMin = -(range.lMax / 2) - 1;
	}

	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallbackCopyProperties7W(const DIDEVICEOBJECTINSTANCEW* lpddoi, LPVOID pvRef)
{
	auto theirDevice = (IDirectInputDeviceW*)pvRef;

	DIPROPRANGE range;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(theirDevice->GetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;

	dinputRangeMax = range.lMax;
	dinputRangeMin = range.lMin;

	//seems to return min = 0 and max doubled. Correct for that here:
	if (range.lMin == 0)
	{
		dinputRangeMax = range.lMax / 2;
		dinputRangeMin = -(range.lMax / 2) - 1;
	}

	return DIENUM_CONTINUE;
}

BOOL CALLBACK DIEnumDeviceObjectsCallbackCopyProperties7A(const DIDEVICEOBJECTINSTANCEA* lpddoi, LPVOID pvRef)
{
	auto theirDevice = (IDirectInputDeviceA*)pvRef;

	DIPROPRANGE range;
	range.diph.dwSize = sizeof(DIPROPRANGE);
	range.diph.dwHeaderSize = sizeof(DIPROPHEADER);
	range.diph.dwHow = DIPH_BYID;
	range.diph.dwObj = lpddoi->dwType;

	if (FAILED(theirDevice->GetProperty(DIPROP_RANGE, &range.diph)))
		return DIENUM_STOP;

	dinputRangeMax = range.lMax;
	dinputRangeMin = range.lMin;

	//seems to return min = 0 and max doubled. Correct for that here:
	if (range.lMin == 0)
	{
		dinputRangeMax = range.lMax / 2;
		dinputRangeMin = -(range.lMax / 2) - 1;
	}

	return DIENUM_CONTINUE;
}

void GetControllers()
{
	if (!blockDinput)
	{
		{
			pDinput8W->CreateDevice(selectedControllerGuid, &pCtrlr8W, nullptr);
			pCtrlr8W->SetCooperativeLevel(Proto::ProtoGuiHwnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);
			ctrlrFormat8W = 1;
			pCtrlr8W->SetDataFormat(&c_dfDIJoystick);
			pCtrlr8W->EnumObjects(&DIEnumDeviceObjectsCallback8W, pCtrlr8W, DIDFT_AXIS);
			pCtrlr8W->Acquire();
			
		}

		{
			pDinput8A->CreateDevice(selectedControllerGuid, &pCtrlr8A, nullptr);
			pCtrlr8A->SetCooperativeLevel(Proto::ProtoGuiHwnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);
			ctrlrFormat8A = 1;
			pCtrlr8A->SetDataFormat(&c_dfDIJoystick);
			pCtrlr8A->EnumObjects(&DIEnumDeviceObjectsCallback8A, pCtrlr8A, DIDFT_AXIS);
			pCtrlr8A->Acquire();
		}

		{
			pDinput7W->CreateDevice(selectedControllerGuid, &pCtrlr7W, nullptr);
			ctrlrFormat7W = 1;
			pCtrlr7W->SetDataFormat(&c_dfDIJoystick);
			pCtrlr7W->SetCooperativeLevel(Proto::ProtoGuiHwnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);
			pCtrlr7W->EnumObjects(&DIEnumDeviceObjectsCallback7W, pCtrlr7W, DIDFT_AXIS);
			pCtrlr7W->Acquire();
		}

		{
			pDinput7A->CreateDevice(selectedControllerGuid, &pCtrlr7A, nullptr);
			ctrlrFormat7A = 1;
			pCtrlr7A->SetDataFormat(&c_dfDIJoystick);
			pCtrlr7A->SetCooperativeLevel(Proto::ProtoGuiHwnd, DISCL_BACKGROUND | DISCL_NONEXCLUSIVE);
			pCtrlr7A->EnumObjects(&DIEnumDeviceObjectsCallback7A, pCtrlr7A, DIDFT_AXIS);
			pCtrlr7A->Acquire();
		}

		if (waitingForControllersToInstall)
		{
			waitingForControllersToInstall = false;
			InstallControllerHooks();
		}
	}
}

void DinputOrderHook::SetControllerGuid(const GUID& guid)
{
	selectedControllerGuid = guid;
	blockDinput = false;

	if (pDinput8W == nullptr)
		waitingForHookInstallToGetControllers = true;
	else
		GetControllers();
}

void DinputOrderHook::SetEnableGetDeviceStateHook(bool enable)
{
	enableGetDeviceStateHook = enable;
}

HRESULT __stdcall Hook_DinputCreateDevice8W(IDirectInput8W* pdin, const GUID & rguid, 
										   LPDIRECTINPUTDEVICE8W* lplpDirectInputDevice, LPUNKNOWN pUnkOuter)
{
	createDeviceHook8WCounter++;
		
	return blockDinput ? 
	 	DIERR_INVALIDPARAM : //pretend it failed
	 	pdin->CreateDevice(selectedControllerGuid, lplpDirectInputDevice, pUnkOuter);
}

HRESULT __stdcall Hook_DinputCreateDevice8A(IDirectInput8A* pdin, const GUID& rguid,
										   LPDIRECTINPUTDEVICE8A* lplpDirectInputDevice, LPUNKNOWN pUnkOuter)
{
	createDeviceHook8ACounter++;

	return blockDinput ?
		DIERR_INVALIDPARAM : //pretend it failed
		pdin->CreateDevice(selectedControllerGuid, lplpDirectInputDevice, pUnkOuter);
}

HRESULT __stdcall Hook_DinputCreateDevice7W(IDirectInput7W* pdin, const GUID& rguid, LPDIRECTINPUTDEVICEW* lplpDirectInputDevice, LPUNKNOWN pUnkOuter)
{
	createDeviceHook7WCounter++;

	return blockDinput ?
		DIERR_INVALIDPARAM : //pretend it failed
		pdin->CreateDevice(selectedControllerGuid, lplpDirectInputDevice, pUnkOuter);
}

HRESULT __stdcall Hook_DinputCreateDevice7A(IDirectInput7A* pdin, const GUID& rguid, LPDIRECTINPUTDEVICEA* lplpDirectInputDevice, LPUNKNOWN pUnkOuter)
{
	createDeviceHook7ACounter++;

	return blockDinput ?
		DIERR_INVALIDPARAM : //pretend it failed
		pdin->CreateDevice(selectedControllerGuid, lplpDirectInputDevice, pUnkOuter);
}

void SetupDinputDeviceProperties8W(IDirectInputDevice8W* theirDevice)
{
	//theirDevice->Unacquire();
	theirDevice->EnumObjects(&DIEnumDeviceObjectsCallbackCopyProperties8W, theirDevice, DIDFT_AXIS);
	//theirDevice->Acquire();

	pCtrlr8W->Unacquire();
	pCtrlr8W->EnumObjects(&DIEnumDeviceObjectsCallback8W, pCtrlr8W, DIDFT_AXIS);
	pCtrlr8W->Acquire();

	hasSetupDinputDeviceProperties8W = true;
}

void SetupDinputDeviceProperties8A(IDirectInputDevice8A* theirDevice)
{
	//theirDevice->Unacquire();
	theirDevice->EnumObjects(&DIEnumDeviceObjectsCallbackCopyProperties8A, theirDevice, DIDFT_AXIS);
	//theirDevice->Acquire();

	pCtrlr8A->Unacquire();
	pCtrlr8A->EnumObjects(&DIEnumDeviceObjectsCallback8A, pCtrlr8A, DIDFT_AXIS);
	pCtrlr8A->Acquire();

	hasSetupDinputDeviceProperties8A = true;
}

void SetupDinputDeviceProperties7W(IDirectInputDeviceW* theirDevice)
{
	//theirDevice->Unacquire();
	theirDevice->EnumObjects(&DIEnumDeviceObjectsCallbackCopyProperties7W, theirDevice, DIDFT_AXIS);
	//theirDevice->Acquire();

	pCtrlr7W->Unacquire();
	pCtrlr7W->EnumObjects(&DIEnumDeviceObjectsCallback7W, pCtrlr7W, DIDFT_AXIS);
	pCtrlr7W->Acquire();

	hasSetupDinputDeviceProperties7W = true;
}

void SetupDinputDeviceProperties7A(IDirectInputDeviceA* theirDevice)
{
	//theirDevice->Unacquire();
	theirDevice->EnumObjects(&DIEnumDeviceObjectsCallbackCopyProperties7A, theirDevice, DIDFT_AXIS);
	//theirDevice->Acquire();

	pCtrlr7A->Unacquire();
	pCtrlr7A->EnumObjects(&DIEnumDeviceObjectsCallback7A, pCtrlr7A, DIDFT_AXIS);
	pCtrlr7A->Acquire();

	hasSetupDinputDeviceProperties7A = true;
}

HRESULT __stdcall Hook_DinputGetDeviceState8W(IDirectInputDevice8W* pDev, DWORD cbData, LPVOID lpvData)
{
	getState8WCounter++;
	
	if (blockDinput)
	{
		memset(lpvData, 0, cbData);
		return DI_OK;
	}
	
	if (ctrlrFormat8W == 1 && cbData == sizeof(DIJOYSTATE2))
	{
		pCtrlr8W->Unacquire();
		pCtrlr8W->SetDataFormat(&c_dfDIJoystick2);
		pCtrlr8W->Acquire();
		ctrlrFormat8W = 2;
	}
	else if (ctrlrFormat8W == 2 && cbData <= sizeof(DIJOYSTATE))
	{
		pCtrlr8W->Unacquire();
		pCtrlr8W->SetDataFormat(&c_dfDIJoystick);
		pCtrlr8W->Acquire();
		ctrlrFormat8W = 1;
	}

	if (!hasSetupDinputDeviceProperties8W) 
		SetupDinputDeviceProperties8W(pDev);
	
	pCtrlr8W->Poll();

	DIJOYSTATE2 tempState;
	auto ret = pCtrlr8W->GetDeviceState(cbData <= sizeof(DIJOYSTATE) ? sizeof(DIJOYSTATE) : sizeof(DIJOYSTATE2), &tempState);
	memcpy(lpvData, &tempState, cbData);
	return ret;
	
	// return pCtrlr8W->GetDeviceState(cbData, lpvData);
}

HRESULT __stdcall Hook_DinputGetDeviceState8A(IDirectInputDevice8A* pDev, DWORD cbData, LPVOID lpvData)
{
	getState8ACounter++;

	if (blockDinput)
	{
		memset(lpvData, 0, cbData);
		return DI_OK;
	}

	if (ctrlrFormat8A == 1 && cbData == sizeof(DIJOYSTATE2))
	{
		pCtrlr8A->Unacquire();
		pCtrlr8A->SetDataFormat(&c_dfDIJoystick2);
		pCtrlr8A->Acquire();
		ctrlrFormat8A = 2;
	}
	else if (ctrlrFormat8A == 2 && cbData <= sizeof(DIJOYSTATE))
	{
		pCtrlr8A->Unacquire();
		pCtrlr8A->SetDataFormat(&c_dfDIJoystick);
		pCtrlr8A->Acquire();
		ctrlrFormat8A = 1;
	}

	if (!hasSetupDinputDeviceProperties8A)
		SetupDinputDeviceProperties8A(pDev);

	pCtrlr8A->Poll();

	DIJOYSTATE2 tempState;
	auto ret = pCtrlr8A->GetDeviceState(cbData <= sizeof(DIJOYSTATE) ? sizeof(DIJOYSTATE) : sizeof(DIJOYSTATE2), &tempState);
	memcpy(lpvData, &tempState, cbData);
	return ret;

	// return pCtrlr8A->GetDeviceState(cbData, lpvData);
}

HRESULT __stdcall Hook_DinputGetDeviceState7A(IDirectInputDeviceA* pDev, DWORD cbData, LPVOID lpvData)
{
	getState7ACounter++;

	if (blockDinput)
	{
		memset(lpvData, 0, cbData);
		return DI_OK;
	}

	if (ctrlrFormat7A == 1 && cbData == sizeof(DIJOYSTATE2))
	{
		pCtrlr7A->Unacquire();
		pCtrlr7A->SetDataFormat(&c_dfDIJoystick2);
		pCtrlr7A->Acquire();
		ctrlrFormat7A = 2;
	}
	else if (ctrlrFormat7A == 2 && cbData <= sizeof(DIJOYSTATE))
	{
		pCtrlr7A->Unacquire();
		pCtrlr7A->SetDataFormat(&c_dfDIJoystick);
		pCtrlr7A->Acquire();
		ctrlrFormat7A = 1;
	}

	if (!hasSetupDinputDeviceProperties7A) 
		SetupDinputDeviceProperties7A(pDev);
	
	//wanderlust adventure cbData is 60, but sizeof(DIJOYSTATE) is 80

	DIJOYSTATE2 tempState;
	auto ret = pCtrlr7A->GetDeviceState(cbData <= sizeof(DIJOYSTATE) ? sizeof(DIJOYSTATE) : sizeof(DIJOYSTATE2), &tempState);
	memcpy(lpvData, &tempState, cbData);
	
	// return pCtrlr7A->GetDeviceState(cbData, lpvData);
}

HRESULT __stdcall Hook_DinputGetDeviceState7W(IDirectInputDeviceW* pDev, DWORD cbData, LPVOID lpvData)
{
	getState7WCounter++;

	if (blockDinput)
	{
		memset(lpvData, 0, cbData);
		return DI_OK;
	}

	if (ctrlrFormat7W == 1 && cbData == sizeof(DIJOYSTATE2))
	{
		pCtrlr7W->Unacquire();
		pCtrlr7W->SetDataFormat(&c_dfDIJoystick2);
		pCtrlr7W->Acquire();
		ctrlrFormat7W = 2;
	}
	else if (ctrlrFormat7W == 2 && cbData <= sizeof(DIJOYSTATE))
	{
		pCtrlr7W->Unacquire();
		pCtrlr7W->SetDataFormat(&c_dfDIJoystick);
		pCtrlr7W->Acquire();
		ctrlrFormat7W = 1;
	}

	if (!hasSetupDinputDeviceProperties7W)
		SetupDinputDeviceProperties7W(pDev);

	printf("blockDinput= %d, ctrlrFormat7W = %d, cbData = %d, RangeMax=%d, RangeMin=%d\n", blockDinput, ctrlrFormat7W, cbData, dinputRangeMax, dinputRangeMin);

	
	//wanderlust adventure cbData is 60, but sizeof(DIJOYSTATE) is 80
	
	DIJOYSTATE2 tempState;
	auto ret = pCtrlr7W->GetDeviceState(cbData <= sizeof(DIJOYSTATE) ? sizeof(DIJOYSTATE) : sizeof(DIJOYSTATE2), &tempState);
	memcpy(lpvData, &tempState, cbData);

	return ret;

	// return pCtrlr7W->GetDeviceState(cbData, lpvData);
}

void DinputOrderHook::ShowGuiStatus()
{
	ImGui::Text("This checkbox needs to be set BEFORE enabling the Dinput Order hook");
	ImGui::Checkbox("Hook GetDeviceState", &enableGetDeviceStateHook);
	
	if (!blockDinput)
	{
		wchar_t* guidString;
		if (StringFromCLSID(selectedControllerGuid, &guidString) == S_OK)
			ImGui::Text("Selected GUID: %ws", guidString);
		else
			ImGui::Text("Selected GUID: ???");
	}
	else
	{
		ImGui::Text("No selected GUID");
	}

	ImGui::Separator();
	
	ImGui::Text("Counters:\n"
				"CreateDevice8W: %d\nCreateDevice8A: %d\nCreateDevice7W: %d\nCreateDevice7A: %d\n"
				"GetDeviceState8: %d\nGetDeviceState7: %d\n",
				createDeviceHook8WCounter, createDeviceHook8ACounter, createDeviceHook7WCounter, createDeviceHook7ACounter,
				getState8WCounter, getState7WCounter);

	ImGui::Separator();

	ImGui::Text("These should display the state of the ABXY buttons to test if input works:");

	if (pCtrlr8W)
	{
		pCtrlr8W->Poll();
		DIJOYSTATE state;
		pCtrlr8W->GetDeviceState(sizeof(DIJOYSTATE), &state);

		ImGui::Text("Device 8W: %d, %d, %d, %d", state.rgbButtons[0], state.rgbButtons[1], state.rgbButtons[2], state.rgbButtons[3]);
	}
	else ImGui::Text("pCtrlr8W is null");

	if (pCtrlr8A)
	{
		pCtrlr8A->Poll();
		DIJOYSTATE state;
		pCtrlr8A->GetDeviceState(sizeof(DIJOYSTATE), &state);

		ImGui::Text("Device 8A: %d, %d, %d, %d", state.rgbButtons[0], state.rgbButtons[1], state.rgbButtons[2], state.rgbButtons[3]);
	}
	else ImGui::Text("pCtrlr8A is null");

	if (pCtrlr7W)
	{
		DIJOYSTATE state;
		pCtrlr7W->GetDeviceState(sizeof(DIJOYSTATE), &state);

		ImGui::Text("Device 7W: %d, %d, %d, %d", state.rgbButtons[0], state.rgbButtons[1], state.rgbButtons[2], state.rgbButtons[3]);
	}
	else ImGui::Text("pCtrlr7W is null");

	if (pCtrlr7A)
	{
		DIJOYSTATE state;
		pCtrlr7A->GetDeviceState(sizeof(DIJOYSTATE), &state);

		ImGui::Text("Device 7A: %d, %d, %d, %d", state.rgbButtons[0], state.rgbButtons[1], state.rgbButtons[2], state.rgbButtons[3]);
	}
	else ImGui::Text("pCtrlr7A is null");
}

void InstallControllerHooks()
{
	if (!enableGetDeviceStateHook)
		return;
	
	intptr_t* vptrCtrlr8W = *(intptr_t**)pCtrlr8W;
	intptr_t* vptrCtrlr8A = *(intptr_t**)pCtrlr8A;
	intptr_t* vptrCtrlr7W = *(intptr_t**)pCtrlr7W;
	intptr_t* vptrCtrlr7A = *(intptr_t**)pCtrlr7A;

	// Turns out A/W is identical for controllers, so we'll only hook W
	printf("Get device status 8W: %lld\n", vptrCtrlr8W[9]);
	printf("(Not hooked) Get device status 8A: %lld\n", vptrCtrlr8A[9]);
	printf("Get device status 7W: %lld\n", vptrCtrlr7W[9]);
	printf("(Not hooked) Get device status 7A: %lld\n", vptrCtrlr7A[9]);

	DinputOrderHook::hookInfos.push_back(std::get<1>(InstallHook((void*)vptrCtrlr8W[9], Hook_DinputGetDeviceState8W)));
	// hookInfos.push_back(std::get<1>(InstallHook((void*)vptrCtrlr8A[9], Hook_DinputGetDeviceState8A)));
	DinputOrderHook::hookInfos.push_back(std::get<1>(InstallHook((void*)vptrCtrlr7W[9], Hook_DinputGetDeviceState7W)));
	// hookInfos.push_back(std::get<1>(InstallHook((void*)vptrCtrlr7A[9], Hook_DinputGetDeviceState7A)));
}

void DinputOrderHook::InstallImpl()
{		
	// typedef HRESULT(__stdcall* DirectInputCreate_t)(HINSTANCE hinst, DWORD dwVersion, LPDIRECTINPUTA* ppDI, LPUNKNOWN punkOuter);
	// auto DirectInputCreateA = (DirectInputCreate_t)GetProcAddress(dinputHmod, "DirectInputCreateA");
	
	const auto dinput_DirectInputCreateEx = reinterpret_cast
		<HRESULT(__stdcall *)(HINSTANCE hinst, DWORD dwVersion, REFIID riidltf, LPVOID * ppvOut, LPUNKNOWN punkOuter)>
		(GetProcAddress(LoadLibraryW(L"dinput.dll"), "DirectInputCreateEx"));

	// const auto dinput_DirectInputCreateA = reinterpret_cast
	// 	<HRESULT(__stdcall *)(HINSTANCE hinst, DWORD dwVersion, LPDIRECTINPUTA* ppDI,LPUNKNOWN punkOuter)>
	// 	(GetProcAddress(LoadLibraryW(L"dinput.dll"), "DirectInputCreateA"));

	// const auto dinput_DirectInputCreateW = reinterpret_cast
	// 	<HRESULT(__stdcall*)(HINSTANCE hinst, DWORD dwVersion, LPDIRECTINPUTW* ppDI, LPUNKNOWN punkOuter)>
	// 	(GetProcAddress(LoadLibraryW(L"dinput.dll"), "DirectInputCreateW"));
	
	// const auto dinput8_DirectInput8Create = reinterpret_cast
	// 	<HRESULT(__stdcall*)(HINSTANCE hinst, DWORD dwVersion, REFIID riidltf, LPVOID * ppvOut, LPUNKNOWN punkOuter)>
	// 	(GetProcAddress(LoadLibraryW(L"dinput8.dll"), "DirectInput8Create"));

	const auto currentMod = GetModuleHandleW(0);
		
	// Dinput 8 (dinput.dll)
	DirectInput8Create(currentMod, 0x0800, IID_IDirectInput8W, (void**)&pDinput8W, nullptr);
	DirectInput8Create(currentMod, 0x0800, IID_IDirectInput8A, (void**)&pDinput8A, nullptr);

	// Dinput 7 (dinput.dll)
	dinput_DirectInputCreateEx(currentMod, 0x0700, IID_IDirectInput7W, (void**)&pDinput7W, nullptr);
	dinput_DirectInputCreateEx(currentMod, 0x0700, IID_IDirectInput7A, (void**)&pDinput7A, nullptr);

	// Dinput 8 (dinput8.dll)
	// dinput8_DirectInput8Create(currentMod, 0x0800, IID_IDirectInput8W, (void**)&dinput8_pDinput8W, nullptr);
	// dinput8_DirectInput8Create(currentMod, 0x0800, IID_IDirectInput8A, (void**)&dinput8_pDinput8A, nullptr);

	if (waitingForHookInstallToGetControllers)
	{
		waitingForHookInstallToGetControllers = false;
		GetControllers();
	}
		
	//https://kaisar-haque.blogspot.com/2008/07/c-accessing-virtual-table.html
	intptr_t* vptrDinput8W = *(intptr_t**)pDinput8W;
	intptr_t* vptrDinput8A = *(intptr_t**)pDinput8A;
	intptr_t* vptrDinput7W = *(intptr_t**)pDinput7W;
	intptr_t* vptrDinput7A = *(intptr_t**)pDinput7A;

	printf("Create device 8W: %lld\n", vptrDinput8W[3]);
	printf("Create device 8A: %lld\n", vptrDinput8A[3]);
	printf("Create device 7W: %lld\n", vptrDinput7W[3]);
	printf("Create device 7A: %lld\n", vptrDinput7A[3]);

	hookInfos.push_back(std::get<1>(InstallHook((void*)vptrDinput8W[3], Hook_DinputCreateDevice8W)));
	hookInfos.push_back(std::get<1>(InstallHook((void*)vptrDinput8A[3], Hook_DinputCreateDevice8A)));
	hookInfos.push_back(std::get<1>(InstallHook((void*)vptrDinput7W[3], Hook_DinputCreateDevice7W)));
	hookInfos.push_back(std::get<1>(InstallHook((void*)vptrDinput7A[3], Hook_DinputCreateDevice7A)));

	if (pCtrlr8W == nullptr)
		waitingForControllersToInstall = true;
	else
	{
		InstallControllerHooks();
	}
}

void DinputOrderHook::UninstallImpl()
{
	waitingForControllersToInstall = false;
	
	for (auto& handle : hookInfos)
	{
		UninstallHook(&handle);
	}
	hookInfos.clear();
}

}
