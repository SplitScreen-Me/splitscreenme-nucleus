using System;
using System.Threading;
using System.Windows.Forms;
//using System.IO;

namespace Nucleus.Gaming.Forms
{
    public partial class Prompt : Form
    {
        private bool hasOpenFileDialog = false;
        private string exeName;

        public Prompt(string message)
        {
            InitializeComponent();
            lbl_Msg.Text = message;

            hasOpenFileDialog = false;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);
        }

        public Prompt(string message, bool isOFD, string launcherFileName)
        {
            InitializeComponent();
            lbl_Msg.Text = message;

            hasOpenFileDialog = isOFD;
            exeName = launcherFileName;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);

            if (hasOpenFileDialog)
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.ShowHelp = true; //needed as a work-around to get the prompt to appear
                    if (!string.IsNullOrEmpty(exeName))
                    {
                        open.Title = $"Select the executable, {exeName}, as the launcher";
                        open.Filter = exeName + "|" + exeName;
                    }
                    else
                    {
                        open.Title = "Select the executable to be the launcher";
                        open.Filter = "Game Launcher Executable Files|*.exe";
                    }

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        Thread.Sleep(1000);
                        GenericGameHandler.ofdPath = open.FileName.Replace(@"\", @"\\");
                    }
                }
            }

            btn_Ok.PerformClick();
        }
        private void btn_Ok_Click(object sender, EventArgs e)
        {

            Close();
        }
    }
}
