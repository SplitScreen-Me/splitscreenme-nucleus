using Microsoft.Win32;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Windows;
using System;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    static class Program
    {      
        public static bool Connected;
        public static bool ForcedBadPath;

        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
                Application.ThreadException += Application_ThreadException;

                RunApplication(args);
            }
            catch (Exception ex)
            {
                LogFatalError(ex);
                MessageBox.Show($"A fatal error occurred:\n{ex.Message} {ex.StackTrace}", "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }     
        }

        static void RunApplication(string[] args)
        {
            // initialize DPIManager BEFORE setting 
            // the application to be DPI aware
            DPIManager.PreInitialize();
            User32Util.SetProcessDpiAwareness(ProcessDPIAwareness.ProcessDpiUnaware);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (StartChecks.IsInvalidDriveFormat())
            {
                return;
            }

            App_Settings_Loader.InitializeSettings();
            PlayersIdentityCache.LoadPlayersIdentityCache();

            if (!App_Misc.NucleusMultiInstances)
            {
                if (StartChecks.IsAlreadyRunning())
                    return;
            }

            StartChecks.Check_VCRVersion();
            StartChecks.CheckWebView2Runtime();

            if (!App_Misc.DisablePathCheck)
            {
                if (!StartChecks.StartCheck(true))
                    ForcedBadPath = true;
            }

            Connected = StartChecks.CheckHubResponse();

            StartChecks.CheckFilesIntegrity();
            StartChecks.CheckUserEnvironment();
            StartChecks.CheckAppUpdate();
            StartChecks.CheckDebugLogSize();
            StartChecks.CleanLogs();

            MainForm form = new MainForm(args);
            DPIManager.AddForm(form);
            DPIManager.ForceUpdate();
            Application.Run(form);
        }

        static void LogFatalError(Exception ex)
        {
            File.AppendAllText("crashlog.txt", $"[{DateTime.Now}] {ex}\n");
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            if (ex != null)
            {
                LogFatalError(ex);
            }
        }

        static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            LogFatalError(e.Exception);
        }
    }
}
