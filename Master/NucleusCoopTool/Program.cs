using Nucleus.Coop.Forms;
using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.DPI;
using Nucleus.Gaming.Tools.UserDriveInfo;
using Nucleus.Gaming.Windows;
using System;
using System.Threading;
using System.Windows.Forms;
using static Nucleus.Gaming.DPI.ThreadDPIContext;

namespace Nucleus.Coop
{
    static class Program
    {      
        public static bool Connected;
        public static bool ForcedBadPath;

        [STAThread]
        static void Main(string[] args)
        {

            if(StartChecks.IsInvalidDriveFormat())
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


            //UserDriveInfo.PrintDrivesInfo();

            //args = new string[] {"bp"};

            //if (args.Length > 0)
            //{
            //    if (args[0] == "bp")
            //    {
            //        // Thread.Sleep(500);
            //        User32Util.SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);
            ////        //IntPtr acSucess = SetThreadDpiAwarenessContext((IntPtr)DpiAwarenessContext.DPI_AWARENESS_CONTEXT_UNAWARE);
            //        Application.EnableVisualStyles();
            //        Application.SetCompatibleTextRenderingDefault(false);
            //        BigPictureForm bp = new BigPictureForm();


            //        Application.Run(bp);
            //        return;
            //    }
                   
            //}

            // initialize DPIManager BEFORE setting 
            // the application to be DPI aware
            DPIManager.PreInitialize();
            User32Util.SetProcessDpiAwareness(ProcessDPIAwareness.ProcessPerMonitorDPIAware);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            MainForm form = new MainForm(args);
            DPIManager.AddForm(form);
            DPIManager.ForceUpdate();
            Application.Run(form);
        }
    }
}
