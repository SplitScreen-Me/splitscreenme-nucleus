using System;
using System.Runtime.InteropServices;

namespace Nucleus.Gaming.Coop.ProtoInput
{
    public class ProtoInput
    {
        public static readonly ProtoInput protoInput = new ProtoInput();

        // These are for convenience in the scripts
        public class ExposedValues
        {
            public uint RegisterRawInputHookID = (uint)ProtoHookIDs.RegisterRawInputHookID;
            public uint GetRawInputDataHookID = (uint)ProtoHookIDs.GetRawInputDataHookID;
            public uint MessageFilterHookID = (uint)ProtoHookIDs.MessageFilterHookID;
            public uint GetCursorPosHookID = (uint)ProtoHookIDs.GetCursorPosHookID;
            public uint SetCursorPosHookID = (uint)ProtoHookIDs.SetCursorPosHookID;
            public uint GetKeyStateHookID = (uint)ProtoHookIDs.GetKeyStateHookID;
            public uint GetAsyncKeyStateHookID = (uint)ProtoHookIDs.GetAsyncKeyStateHookID;
            public uint GetKeyboardStateHookID = (uint)ProtoHookIDs.GetKeyboardStateHookID;
            public uint CursorVisibilityStateHookID = (uint)ProtoHookIDs.CursorVisibilityStateHookID;
            public uint ClipCursorHookID = (uint)ProtoHookIDs.ClipCursorHookID;
            public uint FocusHooksHookID = (uint)ProtoHookIDs.FocusHooksHookID;
            public uint RenameHandlesHookID = (uint)ProtoHookIDs.RenameHandlesHookID;
            public uint XinputHookID = (uint)ProtoHookIDs.XinputHookID;
            public uint DinputOrderHookID = (uint)ProtoHookIDs.DinputOrderHookID;
            public uint SetWindowPosHookID = (uint)ProtoHookIDs.SetWindowPosHookID;
            public uint BlockRawInputHookID = (uint)ProtoHookIDs.BlockRawInputHookID;
            public uint FindWindowHookID = (uint)ProtoHookIDs.FindWindowHookID;
            public uint CreateSingleHIDHookID = (uint)ProtoHookIDs.CreateSingleHIDHookID;
            public uint WindowStyleHookID = (uint)ProtoHookIDs.WindowStyleHookID;

            public uint RawInputFilterID = (uint)ProtoMessageFilterIDs.RawInputFilterID;
            public uint MouseMoveFilterID = (uint)ProtoMessageFilterIDs.MouseMoveFilterID;
            public uint MouseActivateFilterID = (uint)ProtoMessageFilterIDs.MouseActivateFilterID;
            public uint WindowActivateFilterID = (uint)ProtoMessageFilterIDs.WindowActivateFilterID;
            public uint WindowActivateAppFilterID = (uint)ProtoMessageFilterIDs.WindowActivateAppFilterID;
            public uint MouseWheelFilterID = (uint)ProtoMessageFilterIDs.MouseWheelFilterID;
            public uint MouseButtonFilterID = (uint)ProtoMessageFilterIDs.MouseButtonFilterID;
            public uint KeyboardButtonFilterID = (uint)ProtoMessageFilterIDs.KeyboardButtonFilterID;
        }

        public readonly ExposedValues Values = new ExposedValues();

        public enum ProtoHookIDs : uint
        {
            RegisterRawInputHookID = 0,
            GetRawInputDataHookID,
            MessageFilterHookID,
            GetCursorPosHookID,
            SetCursorPosHookID,
            GetKeyStateHookID,
            GetAsyncKeyStateHookID,
            GetKeyboardStateHookID,
            CursorVisibilityStateHookID,
            ClipCursorHookID,
            FocusHooksHookID,
            RenameHandlesHookID,
            XinputHookID,
            DinputOrderHookID,
            SetWindowPosHookID,
            BlockRawInputHookID,
            FindWindowHookID,
            CreateSingleHIDHookID,
            WindowStyleHookID
        };

        public enum ProtoMessageFilterIDs : uint
        {
            RawInputFilterID = 0,
            MouseMoveFilterID,
            MouseActivateFilterID,
            WindowActivateFilterID,
            WindowActivateAppFilterID,
            MouseWheelFilterID,
            MouseButtonFilterID,
            KeyboardButtonFilterID
        };

        private static class ProtoInput32
        {
            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint RemoteLoadLibraryInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookStealthInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookInjectStartup(string exePath, string commandLine, uint processCreationFlags, string dllFolderPath, out uint pid, IntPtr environment);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void InstallHook(uint instanceHandle, ProtoHookIDs hookID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void UninstallHook(uint instanceHandle, ProtoHookIDs hookID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void EnableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DisableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void EnableMessageBlock(uint instanceHandle, uint messageID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DisableMessageBlock(uint instanceHandle, uint messageID);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WakeUpProcess(uint instanceHandle);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void UpdateMainWindowHandle(uint instanceHandle, ulong hwnd);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetupState(uint instanceHandle, int instanceIndex);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetupMessagesToSend(uint instanceHandle,
                                    bool sendMouseWheelMessages,
                                    bool sendMouseButtonMessages,
                                    bool sendMouseMoveMessages,
                                    bool sendKeyboardPressMessages);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void StartFocusMessageLoop(uint instanceHandle, int milliseconds,
                bool wm_activate, bool wm_activateapp, bool wm_ncactivate, bool wm_setfocus, bool wm_mouseactivate);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void StopFocusMessageLoop(uint instanceHandle);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetDrawFakeCursor(uint instanceHandle, bool enable);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetExternalFreezeFakeInput(uint instanceHandle, bool enableFreeze);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddSelectedMouseHandle(uint instanceHandle, uint mouseHandle);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddSelectedKeyboardHandle(uint instanceHandle, uint keyboardHandle);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetControllerIndex(uint instanceHandle, uint controllerIndex, uint controllerIndex2, uint controllerIndex3, uint controllerIndex4);

            // This MUST be called before calling InstallHook on the Xinput hook
            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetUseDinputRedirection(uint instanceHandle, bool useRedirection);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetUseOpenXinput(uint instanceHandle, bool useOpenXinput);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetShowCursorWhenImageUpdated(uint instanceHandle, bool enable);

            // Both of these functions require RenameHandlesHookHookID hook
            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddHandleToRename(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddNamedPipeToRename(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetDinputDeviceGUID(uint instanceHandle,
                uint Data1,
                ushort Data2,
                ushort Data3,
                byte Data4a,
                byte Data4b,
                byte Data4c,
                byte Data4d,
                byte Data4e,
                byte Data4f,
                byte Data4g,
                byte Data4h);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DinputHookAlsoHooksGetDeviceState(uint instanceHandle, bool enable);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetSetWindowPosSettings(uint instanceHandle, int posx, int posy, int width, int height);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetCreateSingleHIDName(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetCursorClipOptions(uint instanceHandle, bool useFakeClipCursor);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AllowFakeCursorOutOfBounds(uint instanceHandle, bool allowOutOfBounds, bool extendBounds);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetToggleFakeCursorVisibilityShortcut(uint instanceHandle, bool enabled, uint vkey);

            [DllImport("ProtoInputLoader32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetRawInputBypass(uint instanceHandle, bool enabled);

            [DllImport("ProtoInputUtilDynamic32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint LockInput(bool lockInput);

            [DllImport("ProtoInputUtilDynamic32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SuspendExplorer();

            [DllImport("ProtoInputUtilDynamic32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void RestartExplorer();

            [DllImport("ProtoInputUtilDynamic32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetTaskbarVisibility(bool autoHide, bool alwaysOnTop);

            [DllImport("ProtoInputUtilDynamic32.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetTaskbarVisibility(out bool autoHide, out bool alwaysOnTop);
        }

        private static class ProtoInput64
        {
            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint RemoteLoadLibraryInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookStealthInjectRuntime(uint pid, string dllFolderPath);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint EasyHookInjectStartup(string exePath, string commandLine, uint processCreationFlags, string dllFolderPath, out uint pid, IntPtr environment);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void InstallHook(uint instanceHandle, ProtoHookIDs hookID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void UninstallHook(uint instanceHandle, ProtoHookIDs hookID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void EnableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DisableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void EnableMessageBlock(uint instanceHandle, uint messageID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DisableMessageBlock(uint instanceHandle, uint messageID);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void WakeUpProcess(uint instanceHandle);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void UpdateMainWindowHandle(uint instanceHandle, ulong hwnd);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetupState(uint instanceHandle, int instanceIndex);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetupMessagesToSend(uint instanceHandle,
                                    bool sendMouseWheelMessages,
                                    bool sendMouseButtonMessages,
                                    bool sendMouseMoveMessages,
                                    bool sendKeyboardPressMessages);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void StartFocusMessageLoop(uint instanceHandle, int milliseconds,
                bool wm_activate, bool wm_activateapp, bool wm_ncactivate, bool wm_setfocus, bool wm_mouseactivate);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void StopFocusMessageLoop(uint instanceHandle);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetDrawFakeCursor(uint instanceHandle, bool enable);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetExternalFreezeFakeInput(uint instanceHandle, bool enableFreeze);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddSelectedMouseHandle(uint instanceHandle, uint mouseHandle);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddSelectedKeyboardHandle(uint instanceHandle, uint keyboardHandle);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetControllerIndex(uint instanceHandle, uint controllerIndex, uint controllerIndex2, uint controllerIndex3, uint controllerIndex4);

            // This MUST be called before calling InstallHook on the Xinput hook
            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetUseDinputRedirection(uint instanceHandle, bool useRedirection);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetUseOpenXinput(uint instanceHandle, bool useOpenXinput);

            // Both of these functions require RenameHandlesHookHookID hook
            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddHandleToRename(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AddNamedPipeToRename(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetDinputDeviceGUID(uint instanceHandle,
                uint Data1,
                ushort Data2,
                ushort Data3,
                byte Data4a,
                byte Data4b,
                byte Data4c,
                byte Data4d,
                byte Data4e,
                byte Data4f,
                byte Data4g,
                byte Data4h);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void DinputHookAlsoHooksGetDeviceState(uint instanceHandle, bool enable);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetSetWindowPosSettings(uint instanceHandle, int posx, int posy, int width, int height);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetCreateSingleHIDName(uint instanceHandle, string name);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetCursorClipOptions(uint instanceHandle, bool useFakeClipCursor);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void AllowFakeCursorOutOfBounds(uint instanceHandle, bool allowOutOfBounds, bool extendBounds);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetToggleFakeCursorVisibilityShortcut(uint instanceHandle, bool enabled, uint vkey);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetRawInputBypass(uint instanceHandle, bool enabled);

            [DllImport("ProtoInputLoader64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetShowCursorWhenImageUpdated(uint instanceHandle, bool enable);

            [DllImport("ProtoInputUtilDynamic64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern uint LockInput(bool lockInput);

            [DllImport("ProtoInputUtilDynamic64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SuspendExplorer();

            [DllImport("ProtoInputUtilDynamic64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void RestartExplorer();

            [DllImport("ProtoInputUtilDynamic64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void SetTaskbarVisibility(bool autoHide, bool alwaysOnTop);

            [DllImport("ProtoInputUtilDynamic64.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
            public static extern void GetTaskbarVisibility(out bool autoHide, out bool alwaysOnTop);
        }

        public uint LockInput(bool lockInput)
        {
            if (IntPtr.Size == 4)
            {
                return ProtoInput32.LockInput(lockInput);
            }
            else
            {
                return ProtoInput64.LockInput(lockInput);
            }
        }

        /// <summary>
        /// What this does and why:
        /// explorer.exe is responsible for the desktop window, the taskbar, start menu etc.
        /// If the explorer.exe process is killed, all this vanishes
        /// (however this also appears to break the low-level mouse/keyboard hooks and raw input for some reason when exporer.exe is restarted)
        /// Instead, these functions will suspend all the threads of explorer.exe so it doesn't do anything.
        /// This now means you can't accidentally click the start menu, or open the start menu with the windows key, alt+tab, win+tab, right click the desktop, etc...
        /// 
        /// Another benifit of suspending the threads is that when explorer.exe is running, it appears to override the alt+tab menu with a fancier Windows 10 one
        /// (when explorer.exe is killed it uses an older style one).
        /// However when it's suspended, the new alt+tab menu does nothing so it doesn't use the fallback
        ///
        /// You MUST call RestartExplorer after you finish or the desktop will be broken and the only way out is to kill explorer.exe and restart it with task manager (if it opens)
        /// </summary>
        public void SuspendExplorer()
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SuspendExplorer();
            }
            else
            {
                ProtoInput64.SuspendExplorer();
            }
        }

        public void RestartExplorer()
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.RestartExplorer();
            }
            else
            {
                ProtoInput64.RestartExplorer();
            }
        }

        public uint RemoteLoadLibraryInjectRuntime(uint pid, string dllFolderPath)
        {
            if (IntPtr.Size == 4)
            {
                return ProtoInput32.RemoteLoadLibraryInjectRuntime(pid, dllFolderPath);
            }
            else
            {
                return ProtoInput64.RemoteLoadLibraryInjectRuntime(pid, dllFolderPath);
            }
        }

        public uint EasyHookInjectRuntime(uint pid, string dllFolderPath)
        {
            if (IntPtr.Size == 4)
            {
                return ProtoInput32.EasyHookInjectRuntime(pid, dllFolderPath);
            }
            else
            {
                return ProtoInput64.EasyHookInjectRuntime(pid, dllFolderPath);
            }
        }

        public uint EasyHookStealthInjectRuntime(uint pid, string dllFolderPath)
        {
            if (IntPtr.Size == 4)
            {
                return ProtoInput32.EasyHookStealthInjectRuntime(pid, dllFolderPath);
            }
            else
            {
                return ProtoInput64.EasyHookStealthInjectRuntime(pid, dllFolderPath);
            }
        }

        public uint EasyHookInjectStartup(string exePath, string commandLine, uint processCreationFlags, string dllFolderPath, out uint pid, IntPtr environment)
        {
            if (IntPtr.Size == 4)
            {
                return ProtoInput32.EasyHookInjectStartup(exePath, commandLine, processCreationFlags, dllFolderPath, out pid, environment);
            }
            else
            {
                return ProtoInput64.EasyHookInjectStartup(exePath, commandLine, processCreationFlags, dllFolderPath, out pid, environment);
            }
        }

        // JavaScript friendly version
        public uint EasyHookInjectStartup(string exePath, string commandLine, uint processCreationFlags, string dllFolderPath)
        {
            return EasyHookInjectStartup(exePath, commandLine, processCreationFlags, dllFolderPath, out uint pid, IntPtr.Zero);
        }

        public void InstallHook(uint instanceHandle, ProtoHookIDs hookID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.InstallHook(instanceHandle, hookID);
            }
            else
            {
                ProtoInput64.InstallHook(instanceHandle, hookID);
            }
        }

        public void UninstallHook(uint instanceHandle, ProtoHookIDs hookID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.UninstallHook(instanceHandle, hookID);
            }
            else
            {
                ProtoInput64.UninstallHook(instanceHandle, hookID);
            }
        }

        public void EnableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.EnableMessageFilter(instanceHandle, filterID);
            }
            else
            {
                ProtoInput64.EnableMessageFilter(instanceHandle, filterID);
            }
        }

        public void DisableMessageFilter(uint instanceHandle, ProtoMessageFilterIDs filterID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.DisableMessageFilter(instanceHandle, filterID);
            }
            else
            {
                ProtoInput64.DisableMessageFilter(instanceHandle, filterID);
            }
        }

        public void EnableMessageBlock(uint instanceHandle, uint messageID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.EnableMessageBlock(instanceHandle, messageID);
            }
            else
            {
                ProtoInput64.EnableMessageBlock(instanceHandle, messageID);
            }
        }

        public void DisableMessageBlock(uint instanceHandle, uint messageID)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.DisableMessageBlock(instanceHandle, messageID);
            }
            else
            {
                ProtoInput64.DisableMessageBlock(instanceHandle, messageID);
            }
        }

        public void WakeUpProcess(uint instanceHandle)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.WakeUpProcess(instanceHandle);
            }
            else
            {
                ProtoInput64.WakeUpProcess(instanceHandle);
            }
        }

        public void UpdateMainWindowHandle(uint instanceHandle, ulong hwnd)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.UpdateMainWindowHandle(instanceHandle, hwnd);
            }
            else
            {
                ProtoInput64.UpdateMainWindowHandle(instanceHandle, hwnd);
            }
        }

        public void SetupState(uint instanceHandle, int instanceIndex)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetupState(instanceHandle, instanceIndex);
            }
            else
            {
                ProtoInput64.SetupState(instanceHandle, instanceIndex);
            }
        }

        public void SetupMessagesToSend(uint instanceHandle,
                                    bool sendMouseWheelMessages,
                                    bool sendMouseButtonMessages,
                                    bool sendMouseMoveMessages,
                                    bool sendKeyboardPressMessages)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetupMessagesToSend(instanceHandle,
                                    sendMouseWheelMessages,
                                    sendMouseButtonMessages,
                                    sendMouseMoveMessages,
                                    sendKeyboardPressMessages);
            }
            else
            {
                ProtoInput64.SetupMessagesToSend(instanceHandle,
                                    sendMouseWheelMessages,
                                    sendMouseButtonMessages,
                                    sendMouseMoveMessages,
                                    sendKeyboardPressMessages);
            }
        }

        public void StartFocusMessageLoop(uint instanceHandle, int milliseconds,
                bool wm_activate, bool wm_activateapp, bool wm_ncactivate, bool wm_setfocus, bool wm_mouseactivate)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.StartFocusMessageLoop(instanceHandle, milliseconds,
                wm_activate, wm_activateapp, wm_ncactivate, wm_setfocus, wm_mouseactivate);
            }
            else
            {
                ProtoInput64.StartFocusMessageLoop(instanceHandle, milliseconds,
                wm_activate, wm_activateapp, wm_ncactivate, wm_setfocus, wm_mouseactivate);
            }
        }

        public void StopFocusMessageLoop(uint instanceHandle)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.StopFocusMessageLoop(instanceHandle);
            }
            else
            {
                ProtoInput64.StopFocusMessageLoop(instanceHandle);
            }
        }

        public void SetDrawFakeCursor(uint instanceHandle, bool enable)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetDrawFakeCursor(instanceHandle, enable);
            }
            else
            {
                ProtoInput64.SetDrawFakeCursor(instanceHandle, enable);
            }
        }

        public void SetExternalFreezeFakeInput(uint instanceHandle, bool enableFreeze)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetExternalFreezeFakeInput(instanceHandle, enableFreeze);
            }
            else
            {
                ProtoInput64.SetExternalFreezeFakeInput(instanceHandle, enableFreeze);
            }
        }

        public void AddSelectedMouseHandle(uint instanceHandle, uint mouseHandle)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.AddSelectedMouseHandle(instanceHandle, mouseHandle);
            }
            else
            {
                ProtoInput64.AddSelectedMouseHandle(instanceHandle, mouseHandle);
            }
        }

        public void AddSelectedKeyboardHandle(uint instanceHandle, uint keyboardHandle)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.AddSelectedKeyboardHandle(instanceHandle, keyboardHandle);
            }
            else
            {
                ProtoInput64.AddSelectedKeyboardHandle(instanceHandle, keyboardHandle);
            }
        }

        public void SetControllerIndex(uint instanceHandle, uint controllerIndex, uint controllerIndex2, uint controllerIndex3, uint controllerIndex4)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetControllerIndex(instanceHandle, controllerIndex, controllerIndex2, controllerIndex3, controllerIndex4);
            }
            else
            {
                ProtoInput64.SetControllerIndex(instanceHandle, controllerIndex, controllerIndex2, controllerIndex3, controllerIndex4);
            }
        }

        /// <summary>
        /// This MUST be called before calling InstallHook on the Xinput hook
        /// </summary>
        /// <param name="instanceHandle"></param>
        /// <param name="controllerIndex"></param>
        public void SetUseDinputRedirection(uint instanceHandle, bool useRedirection)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetUseDinputRedirection(instanceHandle, useRedirection);
            }
            else
            {
                ProtoInput64.SetUseDinputRedirection(instanceHandle, useRedirection);
            }
        }

        public void SetUseOpenXinput(uint instanceHandle, bool useOpenXinput)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetUseOpenXinput(instanceHandle, useOpenXinput);
            }
            else
            {
                ProtoInput64.SetUseOpenXinput(instanceHandle, useOpenXinput);
            }
        }

        public void SetShowCursorWhenImageUpdated(uint instanceHandle, bool enable)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetShowCursorWhenImageUpdated(instanceHandle, enable);
            }
            else
            {
                ProtoInput64.SetShowCursorWhenImageUpdated(instanceHandle, enable);
            }
        }

        /// <summary>
        /// Require RenameHandlesHookHookID hook
        /// </summary>
        /// <param name="instanceHandle"></param>
        /// <param name="name"></param>
        public void AddHandleToRename(uint instanceHandle, string name)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.AddHandleToRename(instanceHandle, name);
            }
            else
            {
                ProtoInput64.AddHandleToRename(instanceHandle, name);
            }
        }

        /// <summary>
        /// Require RenameHandlesHookHookID hook
        /// </summary>
        /// <param name="instanceHandle"></param>
        /// <param name="name"></param>
        public void AddNamedPipeToRename(uint instanceHandle, string name)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.AddNamedPipeToRename(instanceHandle, name);
            }
            else
            {
                ProtoInput64.AddNamedPipeToRename(instanceHandle, name);
            }
        }

        public void SetDinputDeviceGUID(uint instanceHandle,
            uint Data1,
            ushort Data2,
            ushort Data3,
            byte Data4a,
            byte Data4b,
            byte Data4c,
            byte Data4d,
            byte Data4e,
            byte Data4f,
            byte Data4g,
            byte Data4h)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetDinputDeviceGUID(instanceHandle,
                    Data1,
                    Data2,
                    Data3,
                    Data4a,
                    Data4b,
                    Data4c,
                    Data4d,
                    Data4e,
                    Data4f,
                    Data4g,
                    Data4h);
            }
            else
            {
                ProtoInput64.SetDinputDeviceGUID(instanceHandle,
                    Data1,
                    Data2,
                    Data3,
                    Data4a,
                    Data4b,
                    Data4c,
                    Data4d,
                    Data4e,
                    Data4f,
                    Data4g,
                    Data4h);
            }
        }

        public void SetDinputDeviceGUID(uint instanceHandle, Guid guid)
        {
            byte[] a = guid.ToByteArray();

            SetDinputDeviceGUID(instanceHandle,
                (uint)((a[3] << 24) | (a[2] << 16) | (a[1] << 8) | a[0]),
                (ushort)((a[5] << 8) | a[4]),
                (ushort)((a[7] << 8) | a[6]),
                a[8], a[9], a[10], a[11], a[12], a[13], a[14], a[15]);
        }

        public void SetDinputHookAlsoHooksGetDeviceState(uint instanceHandle, bool enable)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.DinputHookAlsoHooksGetDeviceState(instanceHandle, enable);
            }
            else
            {
                ProtoInput64.DinputHookAlsoHooksGetDeviceState(instanceHandle, enable);
            }
        }

        public void SetSetWindowPosSettings(uint instanceHandle, int posx, int posy, int width, int height)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetSetWindowPosSettings(instanceHandle, posx, posy, width, height);
            }
            else
            {
                ProtoInput64.SetSetWindowPosSettings(instanceHandle, posx, posy, width, height);
            }
        }

        public void SetCreateSingleHIDName(uint instanceHandle, string name)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetCreateSingleHIDName(instanceHandle, name);
            }
            else
            {
                ProtoInput64.SetCreateSingleHIDName(instanceHandle, name);
            }
        }

        public void SetCursorClipOptions(uint instanceHandle, bool useFakeClipCursor)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetCursorClipOptions(instanceHandle, useFakeClipCursor);
            }
            else
            {
                ProtoInput64.SetCursorClipOptions(instanceHandle, useFakeClipCursor);
            }
        }

        public void SetToggleFakeCursorVisibilityShortcut(uint instanceHandle, bool enabled, uint vkey)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetToggleFakeCursorVisibilityShortcut(instanceHandle, enabled, vkey);
            }
            else
            {
                ProtoInput64.SetToggleFakeCursorVisibilityShortcut(instanceHandle, enabled, vkey);
            }
        }

        public void AllowFakeCursorOutOfBounds(uint instanceHandle, bool allowOutOfBounds, bool extendBounds)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.AllowFakeCursorOutOfBounds(instanceHandle, allowOutOfBounds, extendBounds);
            }
            else
            {
                ProtoInput64.AllowFakeCursorOutOfBounds(instanceHandle, allowOutOfBounds, extendBounds);
            }
        }

        public void SetRawInputBypass(uint instanceHandle, bool enableBypass)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetRawInputBypass(instanceHandle, enableBypass);
            }
            else
            {
                ProtoInput64.SetRawInputBypass(instanceHandle, enableBypass);
            }
        }

        public bool GetTaskbarAutohide()
        {
            bool autohide;

            if (IntPtr.Size == 4)
            {
                ProtoInput32.GetTaskbarVisibility(out autohide, out bool alwaysOnTop);
            }
            else
            {
                ProtoInput64.GetTaskbarVisibility(out autohide, out bool alwaysOnTop);
            }

            return autohide;
        }

        public void SetTaskbarAutohide(bool autohide)
        {
            if (IntPtr.Size == 4)
            {
                ProtoInput32.SetTaskbarVisibility(autohide, false);
            }
            else
            {
                ProtoInput64.SetTaskbarVisibility(autohide, false);
            }
        }
    }
}
