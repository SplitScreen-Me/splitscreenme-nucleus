using Nucleus.Gaming.Coop;

namespace Nucleus.Gaming
{
    public interface IGenericGameInfo : IGameInfo
    {
        SaveType SaveType { get; }

        string SavePath { get; }

        double HandlerInterval { get; }

        string StartArguments { get; }

        string BinariesFolder { get; }

        string WorkingFolder { get; }

        bool NeedsSteamEmulation { get; }

        string SteamID { get; }

        string[] KillMutex { get; }

        string KillMutexType { get; }

        bool KeepSymLinkOnExit { get; }

        string LauncherExe { get; }

        string LauncherTitle { get; }

        string[] FileSymlinkExclusions { get; }
        //bool SymlinkExe { get; }

        bool FakeFocus { get; }

        int KillMutexDelay { get; }

        bool HookFocus { get; }

        bool ForceWindowTitle { get; }

        int IdealProcessor { get; }

        string UseProcessor { get; }

        string ProcessorPriorityClass { get; }

        bool CMDLaunch { get; }

        string[] CMDOptions { get; }

        bool HasDynamicWindowTitle { get; }

        string[] SymlinkFiles { get; }

        bool HookInitDelay { get; }

        bool HookInit { get; }

        string[] CopyFiles { get; }

        bool SetWindowHook { get; }

        bool HideTaskbar { get; }

        //int FakeFocusInterval { get; }

        bool PromptBetweenInstances { get; }

        bool HideCursor { get; }

        bool RenameNotKillMutex { get; }

        bool IdInWindowTitle { get; }

        bool ChangeExe { get; }

        string GamepadGuid { get; }

        bool UseX360ce { get; }

        string HookFocusInstances { get; }

        //bool UseAlpha8CustomDll { get; }

        bool KeepAspectRatio { get; }

        bool HideDesktop { get; }

        bool ResetWindows { get; }

        bool PartialMutexSearch { get; }

        bool UseGoldberg { get; }

        string OrigSteamDllPath { get; }

        bool GoldbergNeedSteamInterface { get; }

        bool XboxOneControllerFix { get; }

        bool UseForceBindIP { get; }

        string[] XInputPlusDll { get; }

        string[] CopyCustomUtils { get; }

        int PlayersPerInstance { get; }

        bool UseDevReorder { get; }

        string[] CustomUserGeneralValues { get; }

        string[] CustomUserPlayerValues { get; }

        string[] CustomUserInstanceValues { get; }

        bool InjectHookXinput { get; }

        bool InjectDinputToXinputTranslation { get; }

        bool UseDInputBlocker { get; }

        bool BlockRawInput { get; }

        bool PreventWindowDeactivation { get; }

        string[] X360ceDll { get; }

        string PostHookInstances { get; }

        string StartHookInstances { get; }

        string FakeFocusInstances { get; }

        string[] CMDBatchBefore { get; }

        string[] CMDBatchAfter { get; }

        string[] CMDBatchClose { get; }

        bool CMDStartArgsInside { get; }

        string[] PlayerSteamIDs { get; }

        bool XInputPlusOldDll { get; }

        //string[] HexEditExe { get; }

    }
}
