using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming;
using System;
using System.Windows.Forms;
using Nucleus.Gaming.Windows;
using System.Diagnostics;
using Nucleus.Gaming.App.Settings;
using System.Linq;
using System.Drawing;
using Nucleus.Gaming.Cache;

namespace Nucleus.Coop.UI
{
    public static class MainWindowFunc
    {
        private static System.Windows.Forms.Timer FadeInTimer;

        public static void SetWindowSizeAndLoc(MainForm mainForm)
        {
            if (App_Misc.WindowLocation != "")
            {
                if (App_Misc.WindowSize != "")
                {
                    string[] windowSize = App_Misc.WindowSize.Split('X');
                    mainForm.Size = new Size(int.Parse(windowSize[0]), int.Parse(windowSize[1]));
                }

                Rectangle area = Screen.PrimaryScreen.Bounds;
                UI_Interface.OsdBounds = area;

                string[] windowLocation = App_Misc.WindowLocation.Split('X');

                if (ScreensUtil.AllScreens().All(s => !s.MonitorBounds.Contains(int.Parse(windowLocation[0]), int.Parse(windowLocation[1]))))
                {
                    mainForm.StartPosition = FormStartPosition.CenterScreen;
                }
                else
                {
                    var destBoundsRect = ScreensUtil.AllScreens().Where(s => s.MonitorBounds.Contains(int.Parse(windowLocation[0]), int.Parse(windowLocation[1]))).FirstOrDefault().MonitorBounds;
                    UI_Interface.OsdBounds = destBoundsRect;
                    mainForm.Location = new Point(area.X + int.Parse(windowLocation[0]), area.Y + int.Parse(windowLocation[1]));
                }

                if (mainForm.Size == area.Size)
                {
                    mainForm.WindowState = FormWindowState.Maximized;
                }
            }
            else
            {
                mainForm.StartPosition = FormStartPosition.CenterScreen;
            }
        }

        public static void FadeIn()
        {
            FadeInTimer = new System.Windows.Forms.Timer();
            FadeInTimer.Interval = (50); //millisecond
            FadeInTimer.Tick += FadeInTick;
            FadeInTimer.Start();
        }

        private static void FadeInTick(object Object, EventArgs EventArgs)
        {
            if (UI_Interface.MainForm.Opacity < 1.0F)
            {
                UI_Interface.MainForm.Opacity += .1;
            }
            else
            {
                Globals.MainOSD = new WPF_OSD(UI_Interface.OsdBounds);
                FadeInTimer.Dispose();
            }
        }

        private static System.Windows.Forms.Timer FadeOutTimer;

        public static void FadeOut()
        {
            if (UI_Interface.WebView != null)
            {
                if (UI_Interface.WebView.Downloading)
                    return;
            }

            UI_Interface.IsFormClosing = true;
            Core_Interface.I_GameHandlerEndFunc("Close button clicked", true);

            FadeOutTimer = new System.Windows.Forms.Timer();
            FadeOutTimer.Interval = (50); //millisecond
            FadeOutTimer.Tick += FadeOutTick;
            FadeOutTimer.Start();
        }

        private static void FadeOutTick(object Object, EventArgs EventArgs)
        {
            if (UI_Interface.MainForm.Opacity > 0.0F)
            {
                UI_Interface.MainForm.Opacity -= .1;
            }
            else
            {
                Generic_Functions.SaveNucleusWindowPosAndLoc();

                Process[] processes = Process.GetProcessesByName("SplitCalculator");
                foreach (Process SplitCalculator in processes)
                {
                    SplitCalculator.Kill();
                }

                Process.GetCurrentProcess().Kill();
            }
        }

        public static void MainForm_ClientSizeChanged(object sender, EventArgs e)
        {
            MainForm mainForm = (MainForm)sender;

            UI_Graphics.Refreshing = true;

            mainForm.Invalidate(false);

            if (UI_Interface.WindowRoundedCorners)
            {
                if (mainForm.WindowState == FormWindowState.Maximized)
                {
                    FormGraphicsUtil.CreateRoundedControlRegion(mainForm, 0, 0, mainForm.Width, mainForm.Height, 0, 0);
                    FormGraphicsUtil.CreateRoundedControlRegion(UI_Interface.HomeScreen, 0, 0, UI_Interface.HomeScreen.Width, UI_Interface.HomeScreen.Height, 0, 0);
                }
                else
                {
                    FormGraphicsUtil.CreateRoundedControlRegion(mainForm, 0, 0, mainForm.Width, mainForm.Height, 20, 20);
                    FormGraphicsUtil.CreateRoundedControlRegion(UI_Interface.HomeScreen, 0, 0, UI_Interface.HomeScreen.Width, UI_Interface.HomeScreen.Height, 20, 20);
                }
            }

            if (UI_Interface.Tutorial != null)
            {
                Generic_Functions.SizeAndScaleTuto();
            }

            UI_Interface.MaximizeButton.BackgroundImage = mainForm.WindowState == FormWindowState.Maximized ? ImageCache.GetImage(Globals.ThemeFolder + "title_windowed.png") : ImageCache.GetImage(Globals.ThemeFolder + "title_maximize.png");

            if (UI_Interface.SetupScreen != null && Core_Interface.I_GameHandler == null)
            {
                GameProfile.Instance?.Reset();

                if (Core_Interface.StepsList != null)
                {
                    Core_Interface.GoToStep(0);
                }

                if (ProfilesList.Instance != null)
                {
                    ProfilesList.Instance.Locked = false;
                }
            }
        }

        public static void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            MainForm mainForm = (MainForm)sender;
            UI_Graphics.Refreshing = true;
            UI_Interface.HomeScreen.Visible = false;
            if (UI_Interface.Tutorial != null) { UI_Interface.Tutorial.Visible = false; }
            mainForm.Opacity = 0.6D;
        }

        public static void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            MainForm mainForm = (MainForm)sender;

            UI_Interface.HomeScreen.Visible = true;
            UI_Interface.GameListContainer.Refresh();
            UI_Interface.SetupPanel.Refresh();
            UI_Interface.WindowPanel.Refresh();
            UI_Interface.InfoPanel.Refresh();
            UI_Interface.BigLogo.Refresh();

            if (UI_Interface.Tutorial != null) { UI_Interface.Tutorial.Visible = true; }
            mainForm.Opacity = 1.0D;
            mainForm.Refresh();
            UI_Graphics.Refreshing = false;
        }

        public static void MainForm_FormClosing(object sender, FormClosingEventArgs e) => Generic_Functions.SaveNucleusWindowPosAndLoc();

        public static void MainForm_Closed(object sender ,FormClosedEventArgs e)
        {
            MainForm mainForm = (MainForm)sender;

            UI_Interface.IsFormClosing = true;

            Core_Interface.I_GameHandlerEndFunc("OnFormClosed", false);

            User32Util.ShowTaskBar();

            UI_Interface.WebView?.Dispose();

            if (!UI_Interface.RestartRequired)
            {
                Process.GetCurrentProcess().Kill();
            }
        }
    }
}
