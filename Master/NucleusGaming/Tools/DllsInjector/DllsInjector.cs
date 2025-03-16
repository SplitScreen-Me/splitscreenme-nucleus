using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.ProtoInput;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;

namespace Nucleus.Gaming.Tools.DllsInjector
{
    public static class DllsInjector
    {

        public static void InjectDLLs(Process proc, Window window, PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Injecting hooks DLL");

            GlobalWindowMethods.GlobalWindowMethods.WaitForProcWindowHandleNotZero(proc);

            bool is64 = EasyHook.RemoteHooking.IsX64Process(proc.Id);
            string currDir = Globals.NucleusInstallRoot;

            bool windowNull = (window == null);

            try
            {
                string injectorPath = Path.Combine(currDir, $"Nucleus.IJ{(is64 ? "x64" : "x86")}.exe");
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = injectorPath;

                object[] args = new object[]
                {
                    1, // Tier. 0 == start up hook, 1 == runtime hook
		            proc.Id, // Target PID
		            0, // WakeUp Thread ID
		            0, // InInjectionOptions (EasyHook)
		            "Nucleus.Hook32.dll", // lib path x86. Inject32/64 will decide which one to use, so pass in both
		            "Nucleus.Hook64.dll", // lib path x64
		            proc.NucleusGetMainWindowHandle(), // Game hWnd
		            handlerInstance.CurrentGameInfo.HookFocus, // Hook GetForegroundWindow/etc
		            handlerInstance.CurrentGameInfo.HideCursor,
                    handlerInstance.isDebug,
                    Globals.NucleusInstallRoot, // Primarily for log output
		            handlerInstance.CurrentGameInfo.SetWindowHook, // SetWindow hook (prevents window from moving)
					handlerInstance.CurrentGameInfo.PreventWindowDeactivation,
                    player.MonitorBounds.Width,//Add Nucleus.DPIHandling support
                    player.MonitorBounds.Height,//Add Nucleus.DPIHandling support
                    player.MonitorBounds.X,//Add Nucleus.DPIHandling support
                    player.MonitorBounds.Y,//Add Nucleus.DPIHandling support
                    (player.IsRawMouse || player.IsRawKeyboard) ? 0 : (player.GamepadId+1),

                    //These options are enabled by default, but if the game isn't using these features the hooks are unwanted
					handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookSetCursorPos,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookGetCursorPos,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookGetKeyState,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookGetAsyncKeyState,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookGetKeyboardState,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookFilterRawInput,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookFilterMouseMessages,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookUseLegacyInput,
                    !handlerInstance.CurrentGameInfo.HookDontUpdateLegacyInMouseMsg,
                    handlerInstance.CurrentGameInfo.SupportsMultipleKeyboardsAndMice && handlerInstance.CurrentGameInfo.HookMouseVisibility,
                    handlerInstance.CurrentGameInfo.HookReRegisterRawInput,
                    handlerInstance.CurrentGameInfo.HookReRegisterRawInputMouse,
                    handlerInstance.CurrentGameInfo.HookReRegisterRawInputKeyboard,
                    handlerInstance.CurrentGameInfo.InjectHookXinput,
                    handlerInstance.CurrentGameInfo.InjectDinputToXinputTranslation,

                    windowNull ? "" : (window.HookPipe?.pipeNameWrite ?? ""),
                    windowNull ? "" : (window.HookPipe?.pipeNameRead ?? ""),
                    windowNull ? -1 : window.MouseAttached.ToInt32(),
                    windowNull ? -1 : window.KeyboardAttached.ToInt32()
                };

                var sbArgs = new StringBuilder();
                foreach (object arg in args)
                {
                    //Converting to base64 prevents characters like " or \ breaking the arguments
                    string arg64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(arg.ToString()));

                    sbArgs.Append(" \"");
                    sbArgs.Append(arg64);
                    sbArgs.Append("\"");
                }

                string arguments = sbArgs.ToString();
                startInfo.Arguments = arguments;
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                startInfo.CreateNoWindow = true;
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                Process injectProc = Process.Start(startInfo);
                injectProc.WaitForExit();
            }
            catch (Exception ex)
            {
                handlerInstance.Log(string.Format("ERROR - {0}", ex.Message));
            }
        }
    }
}
