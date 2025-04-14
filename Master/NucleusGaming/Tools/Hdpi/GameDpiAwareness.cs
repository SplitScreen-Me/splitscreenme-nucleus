using Microsoft.Win32;
using Nucleus.Gaming.App.Settings;
using System;

namespace Nucleus.Gaming
{
    public enum DpiAwarenessMode
    {
        HighDpiAware,
        DpiUnaware,
        DpiAware,
        GdiDpiScaling_DpiUnaware,
        GdiDpiScaling_DpiUnaware_GdiScale
    }

    public static class GameDpiAwareness
    {
        //~ HIGHDPIAWARE → The application handles DPI scaling itself.
        //~ DPIUNAWARE → Disables all DPI scaling.
        //~ DPIAWARE → Enables DPI compatibility.
        //~ GDIDPISCALING DPIUNAWARE → Windows applies software-based DPI scaling.
        //~ GDIDPISCALING DPIUNAWARE GDISCALE → Forces GDI-based scaling.

        public static void SetExeDpiAwareness(string exePath)
        {
            var genericGameInfo = GenericGameHandler.Instance?.CurrentGameInfo;

            if(genericGameInfo == null)
            {
                return;
            }

            string dpiAwareness = string.Empty; 

            switch (genericGameInfo.DpiAwarenessMode)
            {
                case DpiAwarenessMode.HighDpiAware:
                    dpiAwareness = "~ HIGHDPIAWARE";
                    break;
                case DpiAwarenessMode.DpiUnaware:
                    dpiAwareness = "~ DPIUNAWARE";
                    break;
                case DpiAwarenessMode.DpiAware:
                    dpiAwareness = "~ DPIAWARE";
                    break;
                case DpiAwarenessMode.GdiDpiScaling_DpiUnaware:
                    dpiAwareness = "~ GDIDPISCALING DPIUNAWARE";
                    break;
                case DpiAwarenessMode.GdiDpiScaling_DpiUnaware_GdiScale:
                    dpiAwareness = "~ GDIDPISCALING DPIUNAWARE GDISCALE";
                    break;
            }

            string registryPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";

            try
            {
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(registryPath, true))
                {
                    if (key != null)
                    {
                        key.SetValue(exePath, dpiAwareness, RegistryValueKind.String);

                        if(App_Misc.DebugLog)
                        {
                            LogManager.Log($"DPI awareness set to {dpiAwareness} for {exePath}");
                        }             
                    }
                    else
                    {
                        if (App_Misc.DebugLog)
                        {
                            LogManager.Log("Can't access Windows Registry.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogManager.Log($"Error : {ex.Message}");
            }
        }
    }
}
