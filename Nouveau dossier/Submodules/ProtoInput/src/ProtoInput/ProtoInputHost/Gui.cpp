#include "Gui.h"
#include <imgui.h>
#include <algorithm>
#include <imgui_internal.h>
#include <string>
#include <vector>
#include "Instance.h"
#include <filesystem>
#include <BlackBone/Process/Process.h>
#include <iostream>
#include "RawInput.h"
#include "protoloader.h"
#include "Profiles.h"
#include "RawInput.h"
#include "MessageList.h"

namespace ProtoHost
{

enum class RuntimeInjectionType : int
{
    REMOTE_LOAD_LIBRARY_INJECTION = 0,
    EASYHOOK_INJECTION,
    EASYHOOK_STEALTH_INJECTION
};

enum class StartupInjectionType : int
{
    EASYHOOK_CREATE_AND_INJECT
};

struct ProcInfo
{
    unsigned long pid;
    std::wstring name;
};

std::wstring dllFolderPath{};

std::vector<Instance> instances{};
int selectedIndex = -1;
bool hasAlreadyAddedFocusedPid = false;
unsigned long focusedPid = -1;
std::wstring processSearchString{};
std::vector<ProcInfo> processList{};

Profile currentProfile{};

RuntimeInjectionType selectedRuntimeInjectionType = RuntimeInjectionType::REMOTE_LOAD_LIBRARY_INJECTION;
StartupInjectionType selectedStartupInjectionType = StartupInjectionType::EASYHOOK_CREATE_AND_INJECT;

std::vector<ProtoInstanceHandle> trackedInstanceHandles{};

static bool isInputCurrentlyLocked = false;
void OnInputLockChange(bool locked)
{
    isInputCurrentlyLocked = locked;

    bool freeze = !isInputCurrentlyLocked && freezeGameInputWhileInputNotLocked;

    for (const auto& instanceHandle : trackedInstanceHandles)
        SetExternalFreezeFakeInput(instanceHandle, freeze);
}

bool Launch()
{
    bool noReinjection = true;
    for (const auto& instance : instances)
    {
        if (instance.hasBeenInjected)
            noReinjection = false;
    }

    if (!noReinjection)
    {
        return false;
    }

    int index = 0;
    for (auto& instance : instances)
    {
        index++; // Yes, this starts at 1

        if (instance.runtime) instance.hasBeenInjected = true;

        ProtoInstanceHandle instanceHandle = 0;
        unsigned long pid = -1;

        if (instance.runtime)
        {
            pid = instance.pid;

            if (selectedRuntimeInjectionType == RuntimeInjectionType::EASYHOOK_INJECTION)
                instanceHandle = EasyHookInjectRuntime(instance.pid, dllFolderPath.c_str());
            else if (selectedRuntimeInjectionType == RuntimeInjectionType::EASYHOOK_STEALTH_INJECTION)
                instanceHandle = EasyHookStealthInjectRuntime(instance.pid, dllFolderPath.c_str());
            else if (selectedRuntimeInjectionType == RuntimeInjectionType::REMOTE_LOAD_LIBRARY_INJECTION)
                instanceHandle = RemoteLoadLibraryInjectRuntime(instance.pid, dllFolderPath.c_str());
            else
                printf("Unknown runtime injection type");
        }
        else
        {
            if (selectedStartupInjectionType == StartupInjectionType::EASYHOOK_CREATE_AND_INJECT)
                instanceHandle = EasyHookInjectStartup(instance.filepath.c_str(), L"", 0, dllFolderPath.c_str(), &pid);
            else
                printf("Unknown startup injection type");
        }

        trackedInstanceHandles.push_back(instanceHandle);

        SetupState(instanceHandle, index);

        // Bit lazy but it works 
        auto hookEnabled = [](unsigned int id)
        {
            for (const auto& hook : currentProfile.hooks)
            {
                if (hook.enabled && hook.id == id)
                    return true;
            }

            return false;
        };

        auto filterEnabled = [](unsigned int id)
        {
            for (const auto& filter : currentProfile.messageFilters)
            {
                if (filter.enabled && filter.id == id)
                    return true;
            }

            return false;
        };

        if (hookEnabled(RegisterRawInputHookID))        InstallHook(instanceHandle, RegisterRawInputHookID);
        if (hookEnabled(GetRawInputDataHookID))         InstallHook(instanceHandle, GetRawInputDataHookID);
        if (hookEnabled(MessageFilterHookID))           InstallHook(instanceHandle, MessageFilterHookID);
        if (hookEnabled(GetCursorPosHookID))            InstallHook(instanceHandle, GetCursorPosHookID);
        if (hookEnabled(SetCursorPosHookID))            InstallHook(instanceHandle, SetCursorPosHookID);
        if (hookEnabled(GetKeyStateHookID))             InstallHook(instanceHandle, GetKeyStateHookID);
        if (hookEnabled(GetAsyncKeyStateHookID))        InstallHook(instanceHandle, GetAsyncKeyStateHookID);
        if (hookEnabled(GetKeyboardStateHookID))        InstallHook(instanceHandle, GetKeyboardStateHookID);

        SetShowCursorWhenImageUpdated(instanceHandle, currentProfile.showCursorWhenImageUpdated);
        if (hookEnabled(CursorVisibilityStateHookID))   InstallHook(instanceHandle, CursorVisibilityStateHookID);

        SetCursorClipOptions(instanceHandle, currentProfile.useFakeClipCursor);
        if (hookEnabled(ClipCursorHookID))              InstallHook(instanceHandle, ClipCursorHookID);
    	
        if (hookEnabled(FocusHooksHookID))              InstallHook(instanceHandle, FocusHooksHookID);
        if (hookEnabled(RenameHandlesHookID))           InstallHook(instanceHandle, RenameHandlesHookID);

        if (hookEnabled(BlockRawInputHookID))           InstallHook(instanceHandle, BlockRawInputHookID);
    	
        SetUseOpenXinput(instanceHandle, currentProfile.useOpenXinput);
        SetUseDinputRedirection(instanceHandle, currentProfile.dinputToXinputRedirection);
        if (hookEnabled(XinputHookID))                  InstallHook(instanceHandle, XinputHookID);
        
        if (hookEnabled(DinputOrderHookID))             InstallHook(instanceHandle, DinputOrderHookID);
    	
        if (filterEnabled(RawInputFilterID))            EnableMessageFilter(instanceHandle, RawInputFilterID);
        if (filterEnabled(MouseMoveFilterID))           EnableMessageFilter(instanceHandle, MouseMoveFilterID);
        if (filterEnabled(MouseActivateFilterID))       EnableMessageFilter(instanceHandle, MouseActivateFilterID);
        if (filterEnabled(WindowActivateFilterID))      EnableMessageFilter(instanceHandle, WindowActivateFilterID);
        if (filterEnabled(WindowActivateAppFilterID))   EnableMessageFilter(instanceHandle, WindowActivateAppFilterID);
        if (filterEnabled(MouseWheelFilterID))          EnableMessageFilter(instanceHandle, MouseWheelFilterID);
        if (filterEnabled(MouseButtonFilterID))         EnableMessageFilter(instanceHandle, MouseButtonFilterID);
        if (filterEnabled(KeyboardButtonFilterID))      EnableMessageFilter(instanceHandle, KeyboardButtonFilterID);


        for (const auto msg : currentProfile.blockedMessages)
        {
            EnableMessageBlock(instanceHandle, msg);
        }

        SetupMessagesToSend(instanceHandle,
                            currentProfile.sendMouseWheelMessages,
                            currentProfile.sendMouseButtonMessages,
                            currentProfile.sendMouseMovementMessages,
                            currentProfile.sendKeyboardButtonMessages);

        if (currentProfile.focusMessageLoop)
            StartFocusMessageLoop(instanceHandle,
                                  5,
                                  currentProfile.focusLoopSendWM_ACTIVATE,
                                  currentProfile.focusLoopSendWM_ACTIVATEAPP,
                                  currentProfile.focusLoopSendWM_NCACTIVATE,
                                  currentProfile.focusLoopSendWM_SETFOCUS,
                                  currentProfile.focusLoopSendWM_MOUSEACTIVATE);

        SetDrawFakeCursor(instanceHandle, currentProfile.drawFakeMouseCursor);
    	
        AllowFakeCursorOutOfBounds(instanceHandle, currentProfile.allowMouseOutOfBounds, currentProfile.extendMouseBounds);

        SetToggleFakeCursorVisibilityShortcut(instanceHandle, currentProfile.toggleFakeCursorVisibilityShortcut, VK_HOME);
    	
        for (const auto& renameHandle : currentProfile.renameHandles)
            AddHandleToRename(instanceHandle, utf8_decode(renameHandle).c_str());

        for (const auto& renameNamedPipeHandle : currentProfile.renameNamedPipeHandles)
            AddNamedPipeToRename(instanceHandle, utf8_decode(renameNamedPipeHandle).c_str());

        if (instance.mouseHandle != -1)
            AddSelectedMouseHandle(instanceHandle, instance.mouseHandle);

        if (instance.keyboardHandle != -1)
            AddSelectedKeyboardHandle(instanceHandle, instance.keyboardHandle);

        SetControllerIndex(instanceHandle, instance.controllerIndex);

        SetExternalFreezeFakeInput(instanceHandle, !isInputCurrentlyLocked && freezeGameInputWhileInputNotLocked);

        if (!instance.runtime)
            WakeUpProcess(instanceHandle);

    }

    return true;
}

inline void PushDisabled()
{
    ImGui::PushItemFlag(ImGuiItemFlags_Disabled, true);
    ImGui::PushStyleVar(ImGuiStyleVar_Alpha, ImGui::GetStyle().Alpha * 0.5f);
}

inline void PopDisabled()
{
    ImGui::PopItemFlag();
    ImGui::PopStyleVar();
}

void RefreshPids()
{
    processList.clear();

    auto pids = blackbone::Process::EnumByName(L"");
    std::sort(pids.begin(), pids.end());

    for (const auto& pid : pids)
    {
        const auto ph = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, pid);

        constexpr size_t len = 260;
        wchar_t processNameBuffer[len]{};
        DWORD buffSize = len;
        QueryFullProcessImageNameW(ph, 0, processNameBuffer, &buffSize);

        CloseHandle(ph);

        if (buffSize != len)
        {
            auto processName = std::filesystem::path(processNameBuffer).filename().wstring();

            if (!processName.empty())
                processList.push_back({ pid, std::move(processName) });
        }
    }
}

void SelectFirstIndex()
{
    if (instances.empty())
        selectedIndex = -1;
    else
    {
#undef min
        selectedIndex = std::min(selectedIndex, (int)instances.size() - 1);
    }

    hasAlreadyAddedFocusedPid = false;
    for (const auto& instance : instances)
    {
        if (instance.runtime && instance.pid == focusedPid)
        {
            hasAlreadyAddedFocusedPid = true;
            break;
        }
    }
}

void InstancesWindow()
{	
    // Current instances
    {
        ImGui::PushStyleVar(ImGuiStyleVar_ChildRounding, 5.0f);
        ImGui::BeginChild("ChildR", ImVec2(0, 260), true, ImGuiWindowFlags_None);

        if (ImGui::BeginMenuBar())
        {
            if (ImGui::BeginMenu("Current instances"))
            {
                ImGui::EndMenu();
            }

            ImGui::EndMenuBar();
        }

        int i = 0;
        for (const auto& instance : instances)
        {
            auto text = std::to_string(i + 1) + std::string(". ") + instance.instanceName;
            if (ImGui::Selectable(text.c_str(), i == selectedIndex))
                selectedIndex = i;

            i++;
        }

        ImGui::EndChild();
        ImGui::PopStyleVar();
    }

    {
        const bool disabled = selectedIndex == -1;
        if (disabled) PushDisabled();
        if (ImGui::Button("Removed selected"))
        {
            instances.erase(instances.begin() + selectedIndex);
            SelectFirstIndex();
        }
        if (disabled) PopDisabled();
    }

    // Clear instances button
    {
        if (instances.empty()) PushDisabled();

        if (ImGui::Button("Clear all instances"))
            ImGui::OpenPopup("Clear all instances?");

        if (instances.empty()) PopDisabled();

        // Always center this window when appearing
        ImVec2 center(ImGui::GetIO().DisplaySize.x * 0.5f, ImGui::GetIO().DisplaySize.y * 0.5f);
        ImGui::SetNextWindowPos(center, ImGuiCond_Appearing, ImVec2(0.5f, 0.5f));

        if (ImGui::BeginPopupModal("Clear all instances?", NULL, ImGuiWindowFlags_AlwaysAutoResize))
        {
            ImGui::Text("This  cannot be undone!\n\n");
            ImGui::Separator();

            if (ImGui::Button("OK", ImVec2(120, 0)))
            {
                instances.clear();
                SelectFirstIndex();

                ImGui::CloseCurrentPopup();
            }
            ImGui::SetItemDefaultFocus();
            ImGui::SameLine();
            if (ImGui::Button("Cancel", ImVec2(120, 0))) { ImGui::CloseCurrentPopup(); }
            ImGui::EndPopup();
        }
    }

    ImGui::Separator();

    // Browse executable to add
    {
        static bool selectedAnything = false;

        static std::filesystem::path filepath{};

        if (!selectedAnything) PushDisabled();
        ImGui::TextWrapped("Selected executable: %ws", (selectedAnything ? filepath.c_str() : L"None"));
        if (!selectedAnything) PopDisabled();

        if (ImGui::Button("Browse..."))
        {
            selectedAnything = !selectedAnything;

            wchar_t szFile[260];

            OPENFILENAMEW ofn{};
            ofn.lStructSize = sizeof(ofn);
            ofn.hwndOwner = protoHostHwnd;
            ofn.lpstrFile = szFile;
            // Set lpstrFile[0] to '\0' so that GetOpenFileName does not 
            // use the contents of szFile to initialize itself.
            ofn.lpstrFile[0] = '\0';
            ofn.nMaxFile = sizeof(szFile);
            ofn.lpstrFilter = L"Executable\0*.EXE\0";
            ofn.nFilterIndex = 1;
            ofn.lpstrFileTitle = NULL;
            ofn.nMaxFileTitle = 0;
            ofn.lpstrInitialDir = NULL;
            ofn.Flags = OFN_PATHMUSTEXIST | OFN_FILEMUSTEXIST;

            if (GetOpenFileNameW(&ofn) == TRUE)
            {
                filepath = std::filesystem::path(ofn.lpstrFile);
                selectedAnything = true;

                instances.push_back({ filepath.c_str(), filepath.filename().c_str() });
            }

        }

        ImGui::SameLine();
        if (!selectedAnything) PushDisabled();

        if (ImGui::Button("Add executable"))
        {
            instances.push_back({ filepath.c_str(), filepath.filename().c_str() });
        }

        if (!selectedAnything) PopDisabled();
    }

    ImGui::Separator();

    // Add focused process
    {
        static HWND foregroundWindow = nullptr;
        static wchar_t windowNameBuffer[260];
        static std::wstring processName;

        if (const auto h = GetForegroundWindow(); h != nullptr && h != protoHostHwnd && h != rawInputHwnd)
        {
            wchar_t tempBuff[260];
            GetWindowTextW(h, tempBuff, sizeof(tempBuff) / sizeof(wchar_t));


            if (wcsstr(tempBuff, L"Cortana") == nullptr &&
                wcsstr(tempBuff, L"Task Switching") == nullptr &&
                wcsstr(tempBuff, L"Program Manager") == nullptr)
            {

                DWORD tmpPid;
                GetWindowThreadProcessId(h, &tmpPid);

                const auto ph = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, FALSE, tmpPid);

                wchar_t processNameBuffer[260];
                DWORD buffSize = sizeof(processNameBuffer) / sizeof(wchar_t);
                QueryFullProcessImageNameW(ph, 0, processNameBuffer, &buffSize);

                auto tmpProcessName = std::filesystem::path(processNameBuffer).filename().wstring();

                if (tmpProcessName.find(L"explorer.exe") == std::string::npos &&
                    tmpProcessName.find(L"SearchApp.exe") == std::string::npos)
                {
                    processName = std::move(tmpProcessName);
                    wcscpy(&windowNameBuffer[0], &tempBuff[0]);

                    foregroundWindow = h;
                    focusedPid = tmpPid;

                    hasAlreadyAddedFocusedPid = false;
                    for (const auto& instance : instances)
                    {
                        if (instance.runtime && instance.pid == focusedPid)
                        {
                            hasAlreadyAddedFocusedPid = true;
                            break;
                        }
                    }
                }

                CloseHandle(ph);
            }
        }

        if (focusedPid != -1)
        {
            if (wcslen(windowNameBuffer) != 0)
                ImGui::TextWrapped(R"(Focused process: "%ws" (PID %d, Window "%ws"))", processName.c_str(), focusedPid, &windowNameBuffer[0]);
            else
                ImGui::TextWrapped(R"(Focused process: "%ws" (PID %d))", processName.c_str(), focusedPid);

            const bool copyhasAlreadyAddedPid = hasAlreadyAddedFocusedPid; // Copy so don't Pop without a Push
            if (copyhasAlreadyAddedPid) PushDisabled();
            if (ImGui::Button("Add focused window"))
            {
                bool clear = true;
                for (const auto& instance : instances)
                {
                    if (instance.runtime && instance.pid == focusedPid)
                    {
                        clear = false;
                        break;
                    }
                }

                if (clear)
                    instances.push_back({ focusedPid, processName });

                hasAlreadyAddedFocusedPid = true;
            }
            if (copyhasAlreadyAddedPid) PopDisabled();
        }
        else
        {
            PushDisabled();
            ImGui::TextWrapped("Focused process: None");
            ImGui::Button("Add focused window");
            PopDisabled();
        }
    }

    ImGui::Separator();

    // Running process list
    {
        ImGui::TextWrapped("Running processes");

        constexpr size_t searchBuffLength = 128;
        static char searchBuf[searchBuffLength] = "";

        static std::wstring search{};

        if (ImGui::InputText("Search", searchBuf, searchBuffLength, 0))
        {
            search = utf8_decode(std::string{ searchBuf });
        }

        ImGui::PushStyleVar(ImGuiStyleVar_ChildRounding, 5.0f);

        if (ImGui::Button("Refresh"))
        {
            RefreshPids();
        }

        static int selectedIndex = -1;
#undef min
        selectedIndex = std::min(selectedIndex, (int)processList.size() - 1);

        static bool selectedProcessVisible = false;

        if (!selectedProcessVisible) PushDisabled();
        if (ImGui::Button("Add process"))
        {
            if (selectedIndex < 0 || selectedIndex >= processList.size())
                std::cerr << "Selected index out of range\n" << std::flush;
            else
            {
                const auto& proc = processList[selectedIndex];

                bool alreadyHavePid = false;
                for (const auto& instance : instances)
                {
                    if (instance.runtime && instance.pid == proc.pid)
                    {
                        alreadyHavePid = true;
                        break;
                    }
                }

                if (!alreadyHavePid)
                {
                    instances.push_back({ proc.pid, proc.name });
                }
            }
        }
        if (!selectedProcessVisible) PopDisabled();

        ImGui::BeginChild("Running process list", ImVec2(0, 0), ImGuiWindowFlags_AlwaysAutoResize);

        const bool noSearchTerm = search.empty();
        selectedProcessVisible = false;
        int i = 0;
        for (auto& proc : processList)
        {
            if (noSearchTerm || proc.name.find(search) != std::string::npos)
            {
                const bool currentIsSelected = selectedIndex == i;

                std::string message = utf8_encode(L"PID " + std::to_wstring(proc.pid) + L": " + proc.name);

                if (ImGui::Selectable(message.c_str(), currentIsSelected))
                {
                    selectedProcessVisible = true;
                    selectedIndex = i;
                }
                else if (currentIsSelected)
                    selectedProcessVisible = true;
            }
            i++;
        }

        ImGui::EndChild();
        ImGui::PopStyleVar();
    }
}

void SelectedInstanceWindow()
{
    // static int lastSelectedIndex = -1;
    // const bool hasChangedInstance = lastSelectedIndex != selectedIndex;
    // lastSelectedIndex = selectedIndex;

    bool selectedAnything = selectedIndex != -1;

    if (!selectedAnything)
    {
        PushDisabled();
        ImGui::TextWrapped("Selected: None");
        PopDisabled();
        return;
    }

    auto& instance = instances[selectedIndex];
    ImGui::TextWrapped("Selected %s", instance.instanceName.c_str());
    ImGui::TextWrapped("Instance %d", selectedIndex + 1);

    ImGui::Separator();

    // Mouse
    {
        const auto mouseString = std::to_string(instance.mouseHandle);
        ImGui::TextWrapped("Selected mouse: %s", (instance.mouseHandle != -1 ? mouseString.c_str() : "None"));

        if (ImGui::Button("Click to set mouse"))
        {
            instance.mouseHandle = lastMouseClicked;
        }

        if (ImGui::Button("Unset mouse"))
            instance.mouseHandle = -1;
    }

    ImGui::Separator();

    // Keyboard
    {
        const auto keyboardString = std::to_string(instance.keyboardHandle);
        ImGui::TextWrapped("Selected keyboard: %s", (instance.keyboardHandle != -1 ? keyboardString.c_str() : "None"));

        static bool waitingKeyPress = false;

        if (waitingKeyPress)
        {
            PushDisabled();
            ImGui::Button("Press any key...");
            PopDisabled();

            if (lastKeypressKeyboardHandle != -1)
            {
                waitingKeyPress = false;
                instance.keyboardHandle = (intptr_t)lastKeypressKeyboardHandle;
            }
        }
        else if (ImGui::Button("Click to set keyboard"))
        {
            waitingKeyPress = true;
            instance.keyboardHandle = -1;
            lastKeypressKeyboardHandle = -1;
        }

        if (ImGui::Button("Unset keyboard"))
        {
            instance.keyboardHandle = -1;
            waitingKeyPress = false;
        }
    }

    ImGui::Separator();

    ImGui::TextWrapped("Controllers require the Xinput hook. "
                       "Index 0 implies no controller. "
                       "OpenXinput can be used for more than 4 xinput controllers. "
                       "Dinput translation can be used for more than 4 controllers of any type, "
                       "although the emulation isn't perfect (e.g. both triggers can't be used simultaneously)");

    ImGui::PushID(128794);
    ImGui::Spacing();
    ImGui::TextWrapped("Controller index");
    ImGui::SliderInt("", (int*)&instance.controllerIndex, 0, 16, "%d", ImGuiSliderFlags_AlwaysClamp);
    ImGui::PopID();
}

void OptionsMenu()
{
    if (ImGui::CollapsingHeader("Launch", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        static bool success = true;

        if (ImGui::Button("Inject instances"))
        {
            success = Launch();
        }

        if (!success)
            ImGui::TextWrapped("One or more of the selected instances has already been injected");

        ImGui::Checkbox("Lock input with the End key", &lockInputWithTheEndKey);

        if (!lockInputWithTheEndKey) PushDisabled();
        ImGui::Checkbox("Lock input also suspends explorer.exe", &lockInputSuspendsExplorer);
        ImGui::Checkbox("Freeze game input when input isn't locked", &freezeGameInputWhileInputNotLocked);
        if (!lockInputWithTheEndKey) PopDisabled();
    }

    if (ImGui::CollapsingHeader("Profiles", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        static bool needsRefreshing = true;

        static std::vector<std::string> profiles{};
        if (needsRefreshing)
        {
            profiles = Profile::GetAllProfiles();
            needsRefreshing = false;
        }

        if (ImGui::Button("Reset current profile"))
            currentProfile = Profile{};

        static int selectedIndex = 0;
        const std::string& selectedProfileToLoad = profiles.empty() ? "" : profiles[selectedIndex];

        ImGui::PushID(76127);
        if (ImGui::BeginCombo("", selectedProfileToLoad.c_str()))
        {
            for (int n = 0; n < profiles.size(); n++)
            {
                const bool is_selected = (selectedIndex == n);
                if (ImGui::Selectable(profiles[n].c_str(), is_selected))
                    selectedIndex = n;

                if (is_selected)
                    ImGui::SetItemDefaultFocus();
            }
            ImGui::EndCombo();
        }
        ImGui::PopID();

        ImGui::SameLine();

        constexpr size_t saveFileBufLength = 260;
        static char saveFileBuf[saveFileBufLength] = "";

        static std::string saveFileName{};

        if (selectedProfileToLoad.empty()) PushDisabled();
        if (ImGui::Button("Load"))
        {
            Profile::LoadFromFile(currentProfile, selectedProfileToLoad);
            strcpy(saveFileBuf, selectedProfileToLoad.c_str());
            saveFileName = saveFileBuf;
        }
        if (selectedProfileToLoad.empty()) PopDisabled();



        ImGui::PushID(76177);
        if (ImGui::InputText("", saveFileBuf, saveFileBufLength, 0))
        {
            saveFileName = std::string{ saveFileBuf };
        }
        ImGui::PopID();

        ImGui::SameLine();

        static bool needJsonExtensionWarning = false;

        if (ImGui::Button("Save"))
        {
            needJsonExtensionWarning = false;

            constexpr std::string_view suffix = ".json";

            if (!(saveFileName.size() >= suffix.size() && 0 == saveFileName.compare(saveFileName.size() - suffix.size(), suffix.size(), suffix)))
            {
                needJsonExtensionWarning = true;
            }
            else if (Profile::DoesProfileFileAlreadyExist(saveFileName))
                ImGui::OpenPopup("Overwrite existing");
            else
            {
                Profile::SaveToFile(currentProfile, saveFileName);
                needsRefreshing = true;
            }
        }

        if (needJsonExtensionWarning)
        {
            ImGui::TextWrapped("Needs .json extension");
        }

        // Always center this window when appearing
        ImVec2 center(ImGui::GetIO().DisplaySize.x * 0.5f, ImGui::GetIO().DisplaySize.y * 0.5f);
        ImGui::SetNextWindowPos(center, ImGuiCond_Appearing, ImVec2(0.5f, 0.5f));

        if (ImGui::BeginPopupModal("Overwrite existing", NULL, ImGuiWindowFlags_AlwaysAutoResize))
        {
            ImGui::Text("Overwrite existing file?\n\n");
            ImGui::Separator();

            if (ImGui::Button("OK", ImVec2(120, 0)))
            {
                Profile::SaveToFile(currentProfile, saveFileName);
                needsRefreshing = true;
                ImGui::CloseCurrentPopup();
            }
            ImGui::SetItemDefaultFocus();
            ImGui::SameLine();
            if (ImGui::Button("Cancel", ImVec2(120, 0))) { ImGui::CloseCurrentPopup(); }
            ImGui::EndPopup();
        }
    }

    if (ImGui::CollapsingHeader("Injection method", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        {
            const char* items[] = { "Remote Load Library", "EasyHook Inject", "EasyHook Stealth Inject" };
            const char* combo_label = items[(int)selectedRuntimeInjectionType];
            ImGui::TextWrapped("Runtime hooking method");

            ImGui::PushID(584582);
            if (ImGui::BeginCombo("", combo_label))
            {
                for (int n = 0; n < IM_ARRAYSIZE(items); n++)
                {
                    const bool is_selected = ((int)selectedRuntimeInjectionType == n);
                    if (ImGui::Selectable(items[n], is_selected))
                        selectedRuntimeInjectionType = (RuntimeInjectionType)n;

                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (is_selected)
                        ImGui::SetItemDefaultFocus();
                }
                ImGui::EndCombo();
            }
            ImGui::PopID();

        }

        {
            const char* items[] = { "EasyHook Create and Inject" };
            const char* combo_label = items[(int)selectedStartupInjectionType];
            ImGui::TextWrapped("Startup injection method");
            ImGui::PushID(584583);
            if (ImGui::BeginCombo("", combo_label))
            {
                for (int n = 0; n < IM_ARRAYSIZE(items); n++)
                {
                    const bool is_selected = ((int)selectedStartupInjectionType == n);
                    if (ImGui::Selectable(items[n], is_selected))
                        selectedStartupInjectionType = (StartupInjectionType)n;

                    // Set the initial focus when opening the combo (scrolling + keyboard navigation focus)
                    if (is_selected)
                        ImGui::SetItemDefaultFocus();
                }
                ImGui::EndCombo();
            }
            ImGui::PopID();
        }
    }

    if (ImGui::CollapsingHeader("Hooks", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        ImGui::TextWrapped("See the descriptions in the in-game hooks GUI for more details");

        for (auto& hook : currentProfile.hooks)
        {
            ImGui::Checkbox(hook.uiLabel.c_str(), &hook.enabled);
        }
    }

    ImGui::Checkbox("Dinput to Xinput redirection", &currentProfile.dinputToXinputRedirection);

    ImGui::Checkbox("Use OpenXinput", &currentProfile.useOpenXinput);
	
    ImGui::Checkbox("Use fake clip cursor", &currentProfile.useFakeClipCursor);
	
    ImGui::Checkbox("Show fake cursor when image updated", &currentProfile.showCursorWhenImageUpdated);

    if (ImGui::CollapsingHeader("Message Filters", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        for (auto& filter : currentProfile.messageFilters)
        {
            ImGui::Checkbox(filter.uiLabel.c_str(), &filter.enabled);
        }
    }

    if (ImGui::CollapsingHeader("Options", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        ImGui::Checkbox("Draw fake mouse cursor", &currentProfile.drawFakeMouseCursor);
    	
        ImGui::Checkbox("Allow fake cursor to go out of bounds", &currentProfile.allowMouseOutOfBounds);
        ImGui::Checkbox("Extend fake cursor boundaries", &currentProfile.extendMouseBounds);

        ImGui::Checkbox("Toggle fake cursor shortcut (Home)", &currentProfile.toggleFakeCursorVisibilityShortcut);

        ImGui::Separator();

        ImGui::Checkbox("Send mouse movement messages", &currentProfile.sendMouseMovementMessages);
        ImGui::Checkbox("Send mouse button messages", &currentProfile.sendMouseButtonMessages);
        ImGui::Checkbox("Send mouse wheel messages", &currentProfile.sendMouseWheelMessages);
        ImGui::Checkbox("Send keyboard button messages", &currentProfile.sendKeyboardButtonMessages);

    }

    if (ImGui::CollapsingHeader("Focus loop", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        ImGui::TextWrapped("This is required for any focus loop messages to be sent");
        ImGui::Checkbox("Enabled focus message loop", &currentProfile.focusMessageLoop);

        ImGui::Separator();

        ImGui::TextWrapped("Some games only work with specific messages, some games break with specific messages. Make sure to test different combinations");

        ImGui::Checkbox("Send WM_ACTIVATE", &currentProfile.focusLoopSendWM_ACTIVATE);
        ImGui::Checkbox("Send WM_NCACTIVATE", &currentProfile.focusLoopSendWM_NCACTIVATE);
        ImGui::Checkbox("Send WM_ACTIVATEAPP", &currentProfile.focusLoopSendWM_ACTIVATEAPP);
        ImGui::Checkbox("Send WM_SETFOCUS", &currentProfile.focusLoopSendWM_SETFOCUS);
        ImGui::Checkbox("Send WM_MOUSEACTIVATE", &currentProfile.focusLoopSendWM_MOUSEACTIVATE);

    }

    if (ImGui::CollapsingHeader("Handles to rename", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        ImGui::TextWrapped("These options require the Rename Handles hook");

        {
            constexpr size_t nameBufLength = 260;
            static char nameBuf[nameBufLength] = "";

            static std::string name{};

            ImGui::PushID(54322345);
            if (ImGui::Button("+"))
                currentProfile.renameHandles.emplace_back("");
            ImGui::PopID();

            ImGui::SameLine();
            ImGui::TextWrapped("Rename mutex/event/semaphore handles");

            std::vector<int> toRemove{};

            int i = 0;
            for (auto& current : currentProfile.renameHandles)
            {
                ImGui::PushID(653713 + i);

                if (ImGui::Button("-"))
                    toRemove.push_back(i);

                ImGui::SameLine();

                strcpy(nameBuf, current.c_str());
                if (ImGui::InputText("", nameBuf, nameBufLength, 0))
                {
                    current = std::string{ nameBuf };
                }
                ImGui::PopID();

                i++;
            }

            if (!toRemove.empty())
            {
                for (int i = toRemove.size() - 1; i >= 0; i--)
                {
                    currentProfile.renameHandles.erase(currentProfile.renameHandles.begin() + toRemove[i]);
                }
            }
        }

        ImGui::Separator();

        {
            constexpr size_t nameBufLength = 260;
            static char nameBuf[nameBufLength] = "";

            static std::string name{};

            ImGui::PushID(625345);
            if (ImGui::Button("+"))
                currentProfile.renameNamedPipeHandles.emplace_back("");
            ImGui::PopID();

            ImGui::SameLine();
            ImGui::TextWrapped("Rename named pipe handles");

            std::vector<int> toRemove{};

            int i = 0;
            for (auto& current : currentProfile.renameNamedPipeHandles)
            {
                ImGui::PushID(345345 + i);

                if (ImGui::Button("-"))
                    toRemove.push_back(i);

                ImGui::SameLine();

                strcpy(nameBuf, current.c_str());
                if (ImGui::InputText("", nameBuf, nameBufLength, 0))
                {
                    current = std::string{ nameBuf };
                }
                ImGui::PopID();

                i++;
            }

            if (!toRemove.empty())
            {
                for (int i = toRemove.size() - 1; i >= 0; i--)
                {
                    currentProfile.renameNamedPipeHandles.erase(currentProfile.renameNamedPipeHandles.begin() + toRemove[i]);
                }
            }
        }
    }

    if (ImGui::CollapsingHeader("Messages to block", ImGuiTreeNodeFlags_DefaultOpen | ImGuiTreeNodeFlags_Leaf))
    {
        static bool onlyShowBlocked = false;
        ImGui::Checkbox("Only show blocked", &onlyShowBlocked);

        constexpr size_t searchBuffLength = 128;
        static char searchBuf[searchBuffLength] = "";
        ImGui::InputText("Search", searchBuf, searchBuffLength, ImGuiInputTextFlags_CharsUppercase);
        const std::string search{ searchBuf };

        ImGui::BeginChild("blocked", ImVec2(0, 300), true);

        for (auto& msg : MessageList::messages)
        {
            const bool isBlocked = MessageList::IsBlocked(msg.messageID, currentProfile.blockedMessages);

            static char buff[256];
            snprintf(buff, 256, "(0x%X) %s", msg.messageID, msg.name.c_str());

            if ((!onlyShowBlocked || isBlocked)
                && msg.name.find(search) != std::string::npos
                && ImGui::Selectable(buff, isBlocked))
            {
                if (isBlocked)
                    currentProfile.blockedMessages.erase(
                        find(currentProfile.blockedMessages.begin(), currentProfile.blockedMessages.end(), msg.messageID));
                else
                    currentProfile.blockedMessages.push_back(msg.messageID);
            }

        }

        ImGui::EndChild();

        ImGui::Spacing();
    }
}

void FirstTimeSetup()
{
    InitialiseRawInput();
    RefreshPids();

    wchar_t pathchars[MAX_PATH];
    GetModuleFileNameW(NULL, pathchars, MAX_PATH);
    dllFolderPath = pathchars;
    size_t pos = dllFolderPath.find_last_of(L"\\");
    if (pos != std::string::npos)
        dllFolderPath = dllFolderPath.substr(0, pos + 1);
}

void RenderImgui()
{
    // ImGui::ShowDemoWindow();
    // return;

    if (static bool firstTimeSetup = true; firstTimeSetup)
    {
        firstTimeSetup = false;
        FirstTimeSetup();
    }

    const auto displaySize = ImGui::GetIO().DisplaySize;

    ImGui::SetNextWindowPos(ImVec2(0, 0), ImGuiCond_Always);
    ImGui::SetNextWindowSize(ImVec2(displaySize.x, displaySize.y), ImGuiCond_Always);

    if (ImGui::Begin("Main", nullptr,
                     ImGuiWindowFlags_NoTitleBar |
                     ImGuiWindowFlags_NoResize |
                     ImGuiWindowFlags_NoMove |
                     ImGuiWindowFlags_NoCollapse |
                     //ImGuiWindowFlags_NoBackground |
                     // ImGuiWindowFlags_MenuBar |
                     ImGuiWindowFlags_NoBringToFrontOnFocus
    ))
    {

        const ImVec2 mainWindowSize = ImGui::GetWindowSize();


        // Setting these other than display size doesn't work at the moment
        float originX = 0;
        float originY = 0;
        float sizeX = displaySize.x;
        float sizeY = displaySize.y;

        const auto maxX = originX + sizeX;
        const auto maxY = originY + sizeY;

        constexpr float minSize = 300.0f;



        constexpr float sizeA = 1.0f;
        constexpr float sizeB = 1.0f;
        constexpr float sizeC = 1.0f;
        constexpr float totalSize = sizeA + sizeB + sizeC;

        static ImVec2 windowPosB(originX + minSize, 0);

        ImGui::SetNextWindowSizeConstraints(ImVec2(minSize, sizeY), ImVec2(sizeX - 2 * minSize, sizeY));
        ImGui::SetNextWindowPos(ImVec2(originX, 0), ImGuiCond_Always);
        ImGui::SetNextWindowSize(ImVec2(windowPosB.x - originX, 0.0f), ImGuiCond_Always);


        ImGui::Begin("Instances", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoCollapse);
        const auto windowSizeA = ImVec2(ImGui::GetWindowSize().x, mainWindowSize.y);
        ImGui::SetWindowSize(windowSizeA);

        InstancesWindow();

        ImGui::End();

        ImGui::SetNextWindowSizeConstraints(ImVec2(minSize, sizeY), ImVec2(sizeX - 2 * minSize, sizeY));
        ImGui::SetNextWindowSize(ImVec2(sizeX * sizeB / totalSize, 0.0f), ImGuiCond_Once);
        ImGui::SetNextWindowPos(ImVec2(originX + windowSizeA.x, 0), ImGuiCond_Once);

        ImGui::Begin("Selected Instance", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoCollapse);
        auto windowSizeB = ImVec2(ImGui::GetWindowSize().x, mainWindowSize.y);
        windowPosB = ImGui::GetWindowPos();

        if ((windowPosB.x + windowSizeB.x - originX + minSize > sizeX))
            windowSizeB.x = minSize;

        if (windowPosB.x - minSize < originX)
        {
            windowSizeB.x = minSize;
            windowPosB.x = originX + minSize;
        }
        ImGui::SetWindowSize(windowSizeB);
        ImGui::SetWindowPos(windowPosB);

        SelectedInstanceWindow();

        ImGui::End();

        ImGui::SetNextWindowSizeConstraints(ImVec2(minSize, sizeY), ImVec2(sizeX - 2 * minSize, sizeY));
        ImGui::SetNextWindowPos(ImVec2(windowPosB.x + windowSizeB.x, 0), ImGuiCond_Always);
        ImGui::SetNextWindowSize(ImVec2(maxX - (windowPosB.x + windowSizeB.x), 0.0f), ImGuiCond_Always);

        ImGui::Begin("Options", nullptr, ImGuiWindowFlags_NoMove | ImGuiWindowFlags_NoResize | ImGuiWindowFlags_NoCollapse | ImGuiWindowFlags_NoTitleBar);
        ImGui::SetWindowSize(windowSizeB);

        OptionsMenu();

        ImGui::End();
    }
    ImGui::End();
}

}