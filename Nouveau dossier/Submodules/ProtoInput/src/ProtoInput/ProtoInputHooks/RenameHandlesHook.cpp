#include "RenameHandlesHook.h"
#include <windows.h>
#include <string>
#include <random>
#include <imgui.h>

namespace Proto
{

//Key: search term. Value: the assigned name that is replaced for every name that matched the search term. (value is empty if needs generating)
std::map<std::wstring, std::wstring> originalsToNewNames{};
std::vector<std::wstring> searchTerms{};
std::vector<std::wstring> searchTermsNamedPipe{};

typedef enum _EVENT_TYPE
{
	NotificationEvent,
	SynchronizationEvent
} EVENT_TYPE, * PEVENT_TYPE;

typedef NTSTATUS(NTAPI* t_NtCreateMutant)(PHANDLE MutantHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, BOOLEAN InitialOwner);
typedef NTSTATUS(NTAPI* t_NtOpenMutant)(PHANDLE MutantHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

typedef NTSTATUS(NTAPI* t_NtCreateEvent)(PHANDLE EventHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, EVENT_TYPE EventType, BOOLEAN InitialState);
typedef NTSTATUS(NTAPI* t_NtOpenEvent)(PHANDLE EventHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

typedef NTSTATUS(NTAPI* t_NtCreateSemaphore)(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, ULONG InitialCount, ULONG MaximumCount);
typedef NTSTATUS(NTAPI* t_NtOpenSemaphore)(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes);

typedef NTSTATUS(NTAPI* t_NtCreateNamedPipeFile)(OUT PHANDLE             NamedPipeFileHandle,
												 IN ACCESS_MASK          DesiredAccess,
												 IN POBJECT_ATTRIBUTES   ObjectAttributes,
												 OUT PIO_STATUS_BLOCK    IoStatusBlock,
												 IN ULONG                ShareAccess,
												 IN ULONG                CreateDisposition,
												 IN ULONG                CreateOptions,
												 IN BOOLEAN              WriteModeMessage,
												 IN BOOLEAN              ReadModeMessage,
												 IN BOOLEAN              NonBlocking,
												 IN ULONG                MaxInstances,
												 IN ULONG                InBufferSize,
												 IN ULONG                OutBufferSize,
												 IN PLARGE_INTEGER       DefaultTimeOut);

t_NtCreateMutant NtCreateMutant;
t_NtOpenMutant NtOpenMutant;

t_NtCreateEvent NtCreateEvent;
t_NtOpenEvent NtOpenEvent;

t_NtCreateSemaphore NtCreateSemaphore;
t_NtOpenSemaphore NtOpenSemaphore;

t_NtCreateNamedPipeFile NtCreateNamedPipeFile;

//TODO: NtCreateNamedPipeFile http://undocumented.ntinternals.net/

UNICODE_STRING stdWStringToUnicodeString(const std::wstring& str)
{
	UNICODE_STRING unicodeString;
	DWORD len = 0;

	len = str.length();
	LPWSTR cstr = new WCHAR[len + 1];
	memcpy(cstr, str.c_str(), (len + 1) * sizeof(WCHAR));
	unicodeString.Buffer = cstr;
	unicodeString.Length = (USHORT)(len * sizeof(WCHAR));
	unicodeString.MaximumLength = (USHORT)((len + 1) * sizeof(WCHAR));
	return unicodeString;
}

void updateName(PUNICODE_STRING inputName, bool namedPipe = false)
{
	if (!(inputName->Length > 0 && inputName->Length <= inputName->MaximumLength)) return;

	// printf("updateName %ws\n", inputName->Buffer);

	if (!namedPipe) // Don't try and use the map for named pipes (they are created with a different name than they're accessed with)
	{
		for (const auto& pair : originalsToNewNames)
		{
			// if (wcsstr(inputName->Buffer, pair.first.c_str()) != nullptr)
			if (wcscmp(inputName->Buffer, pair.first.c_str()) == 0) // We have mapped this to something else
			{
				printf("Using map of handle \"%ws\" to \"%ws\"\n", inputName->Buffer, pair.second.c_str());
				*inputName = stdWStringToUnicodeString(pair.second);
				return;
			}
		}
	}

	// Unable to find a mapping, now check if we have a matching search term
	for (const auto& searchTerm : (namedPipe ? searchTermsNamedPipe : searchTerms))
	{
		if (wcsstr(inputName->Buffer, searchTerm.c_str()) != nullptr) // Only care if a substring matches (not the whole string)
		{
			// Match so generate a name
			static std::random_device rd{};
			static std::mt19937 randomGenerator{ rd() };

			const std::wstring oldName = inputName->Buffer;
			const auto newName = oldName + std::to_wstring(randomGenerator());


			if (!namedPipe) // Again, don't use the map for named pipes
			{
				printf("Generating map of handle \"%ws\" to \"%ws\"\n", oldName.c_str(), newName.c_str());
				originalsToNewNames.insert({ oldName, newName });
			}
			else
				printf("Renamed named pipe \"%ws\" to \"%ws\"\n", oldName.c_str(), newName.c_str());
			
			*inputName = stdWStringToUnicodeString(newName);
			return;
		}
	}
}

inline void updateNameObject(POBJECT_ATTRIBUTES ObjectAttributes, bool namedPipe = false)
{
	if (ObjectAttributes != NULL && ObjectAttributes->ObjectName != NULL)
	{
		updateName(ObjectAttributes->ObjectName, namedPipe);
	}
}

NTSTATUS NTAPI Hook_NtCreateMutant(OUT PHANDLE MutantHandle, IN ULONG DesiredAccess, IN POBJECT_ATTRIBUTES ObjectAttributes OPTIONAL, IN BOOLEAN InitialOwner)
{
	updateNameObject(ObjectAttributes);
	return NtCreateMutant(MutantHandle, DesiredAccess, ObjectAttributes, InitialOwner);
}

NTSTATUS NTAPI Hook_NtOpenMutant(PHANDLE MutantHandle, ULONG DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenMutant(MutantHandle, DesiredAccess, ObjectAttributes);
}

NTSTATUS NTAPI Hook_NtCreateEvent(PHANDLE EventHandle, DWORD DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, EVENT_TYPE EventType, BOOLEAN InitialState)
{
	updateNameObject(ObjectAttributes);
	return NtCreateEvent(EventHandle, DesiredAccess, ObjectAttributes, EventType, InitialState);
}

NTSTATUS NTAPI Hook_NtOpenEvent(PHANDLE EventHandle, DWORD DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenEvent(EventHandle, DesiredAccess, ObjectAttributes);
}

NTSTATUS NTAPI Hook_NtCreateSemaphore(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes, ULONG InitialCount, ULONG MaximumCounts)
{	
	updateNameObject(ObjectAttributes);
	return NtCreateSemaphore(SemaphoreHandle, DesiredAccess, ObjectAttributes, InitialCount, MaximumCounts);
}

NTSTATUS NTAPI Hook_NtOpenSemaphore(PHANDLE SemaphoreHandle, ACCESS_MASK DesiredAccess, POBJECT_ATTRIBUTES ObjectAttributes)
{
	updateNameObject(ObjectAttributes);
	return NtOpenSemaphore(SemaphoreHandle, DesiredAccess, ObjectAttributes);
}

NTSTATUS NTAPI Hook_NtCreateNamedPipeFile(
	OUT PHANDLE             NamedPipeFileHandle,
	IN ACCESS_MASK          DesiredAccess,
	IN POBJECT_ATTRIBUTES   ObjectAttributes,
	OUT PIO_STATUS_BLOCK    IoStatusBlock,
	IN ULONG                ShareAccess,
	IN ULONG                CreateDisposition,
	IN ULONG                CreateOptions,
	IN BOOLEAN              WriteModeMessage,
	IN BOOLEAN              ReadModeMessage,
	IN BOOLEAN              NonBlocking,
	IN ULONG                MaxInstances,
	IN ULONG                InBufferSize,
	IN ULONG                OutBufferSize,
	IN PLARGE_INTEGER       DefaultTimeOut)
{
	updateNameObject(ObjectAttributes, true);
	
	return NtCreateNamedPipeFile(
		NamedPipeFileHandle,
		DesiredAccess,
		ObjectAttributes,
		IoStatusBlock,
		ShareAccess,
		CreateDisposition,
		CreateOptions,
		WriteModeMessage,
		ReadModeMessage,
		NonBlocking,
		MaxInstances,
		InBufferSize,
		OutBufferSize,
		DefaultTimeOut
	);
}

void RenameHandlesHook::InstallImpl()
{
	if (static bool firstTime = true; firstTime)
	{
		firstTime = false;

		const auto ntHandle = GetModuleHandleW(L"ntdll.dll");

		NtCreateMutant = (t_NtCreateMutant)GetProcAddress(ntHandle, "NtCreateMutant");
		NtOpenMutant = (t_NtOpenMutant)GetProcAddress(ntHandle, "NtOpenMutant");

		NtCreateEvent = (t_NtCreateEvent)GetProcAddress(ntHandle, "NtCreateEvent");
		NtOpenEvent = (t_NtOpenEvent)GetProcAddress(ntHandle, "NtOpenEvent");

		NtCreateSemaphore = (t_NtCreateSemaphore)GetProcAddress(ntHandle, "NtCreateSemaphore");
		NtOpenSemaphore = (t_NtOpenSemaphore)GetProcAddress(ntHandle, "NtOpenSemaphore");
		
		NtCreateNamedPipeFile = (t_NtCreateNamedPipeFile)GetProcAddress(ntHandle, "NtCreateNamedPipeFile");		
	}

	hookInfoNtCreateMutant = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtCreateMutant", Hook_NtCreateMutant));
	hookInfoNtOpenMutant = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtOpenMutant", Hook_NtOpenMutant));
	
	hookInfoNtCreateEvent = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtCreateEvent", Hook_NtCreateEvent));
	hookInfoNtOpenEvent = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtOpenEvent", Hook_NtOpenEvent));
	
	hookInfoNtCreateSemaphore = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtCreateSemaphore", Hook_NtCreateSemaphore));
	hookInfoNtOpenSemaphore = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtOpenSemaphore", Hook_NtOpenSemaphore));
	
	hookInfoNtCreateNamedPipeFile = std::get<1>(InstallNamedHook(L"ntdll.dll", "NtCreateNamedPipeFile", Hook_NtCreateNamedPipeFile));
}

void RenameHandlesHook::UninstallImpl()
{
	UninstallHook(&hookInfoNtCreateMutant);
	UninstallHook(&hookInfoNtOpenMutant);
	UninstallHook(&hookInfoNtCreateEvent);
	UninstallHook(&hookInfoNtOpenEvent);
	UninstallHook(&hookInfoNtCreateSemaphore);
	UninstallHook(&hookInfoNtOpenSemaphore);
	UninstallHook(&hookInfoNtCreateNamedPipeFile);
}

void RenameHandlesHook::AddHandleToRenameList(const std::wstring& name)
{
	searchTerms.push_back(name);
}

void RenameHandlesHook::AddHandleToNamedPipeRenameList(const std::wstring& name)
{
	searchTermsNamedPipe.push_back(name);
}

}
