using Nucleus.Gaming;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public partial class DependenciesDownloader : Form
    {
        private WebClient webClient;
        private string tempDir = Path.Combine(Globals.NucleusInstallRoot, "Temp");
        private List<Control> ctrls = new List<Control>();

        private float fontSize = 10;
        private string _downloadUri;
        private string _description;
        private string _outputFileName;
        private string _processName;

        private bool exiting;

        string[] knowDependencyInstallers = new string[]
        {
            "vc_redist.x86",
            "vc_redist.x64",
            "MicrosoftEdgeWebView2RuntimeInstallerX64",
        };

        public DependenciesDownloader(string downloadUri, string description, string outputFileName, string processName)
        {
            try
            {
                KillRunnigInstallers();
                DeleteTemp();

                InitializeComponent();
                GetAllControls();
                        
                lbl_Handler.Text = description;

                BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");

                _downloadUri = downloadUri;
                _description = description;
                _outputFileName = outputFileName;
                _processName = processName;

                Text = "Downloading " + outputFileName + ".";

                foreach (Control control in ctrls)
                {
                    control.Font = new Font(Theme_Settings.CustomFont, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                }

                FormClosing += OnFormClosing;

                Activate();

                BeginDownload();
            }
            catch (Exception)
            {
            }
        }

        private void BeginDownload()
        {
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }

            using (webClient = new WebClient())
            {
                webClient.DownloadProgressChanged += wc_DownloadProgressChanged;
                webClient.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri(_downloadUri),
                    // Param2 = Path to save
                    Path.Combine(tempDir, _outputFileName)
                );

                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prog_DownloadBar.Value = e.ProgressPercentage;
            lbl_ProgPerc.Text = e.ProgressPercentage + "%";
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            Visible = false;
            if (_processName != null)
            {
                ExecuteInstaller();
            }
            else
            {
                // If there's no process name provided (no installer to run), just close the form
                DeleteTemp();
                Close();
            }
        }

        private void ExecuteInstaller()
        {
            if (exiting)
            {
                return;
            }

            string installerPath = Path.Combine(tempDir, _outputFileName);
            ProcessStartInfo installer = new ProcessStartInfo(installerPath);
            installer.UseShellExecute = true;
            Process.Start(installerPath);

            while (Process.GetProcessesByName(_processName).Length > 0)
            {
                Thread.Sleep(1000);
            }

            DeleteTemp();
            Close();
        }

        private void DeleteTemp()
        {
            try
            {
                if (webClient != null)
                {
                    webClient.CancelAsync();
                }

                if (Directory.Exists(tempDir))
                {
                    Directory.Delete(tempDir, true);
                }
            }
            catch /*(Exception ex)*/
            {
                //MessageBox.Show(ex.Message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void KillRunnigInstallers()
        {
            //check if any know dependency installer is running
            foreach (string dep in knowDependencyInstallers)
            {
                Process[] instProcs = Process.GetProcessesByName(dep);
                if (instProcs.Length > 0)
                {
                    //Console.WriteLine("Installer is running");
                    foreach (Process proc in instProcs)
                    {
                        if (!proc.HasExited)
                            proc.Kill();
                        Thread.Sleep(80);
                    }
                }
            }
        }

        private void OnFormClosing(object sender, EventArgs e)
        {
            exiting = true;
            DeleteTemp();
        }

        private void GetAllControls()
        {
            foreach (Control control in Controls)
            {
                ctrls.Add(control);
                foreach (Control container1 in control.Controls)
                {
                    ctrls.Add(container1);
                    foreach (Control container2 in container1.Controls)
                    {
                        ctrls.Add(container2);
                        foreach (Control container3 in container2.Controls)
                        {
                            ctrls.Add(container3);
                        }
                    }
                }
            }
        }

    }
}
