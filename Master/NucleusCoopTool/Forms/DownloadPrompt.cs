using Ionic.Zip;
using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public partial class DownloadPrompt : Form
    {
        private Handler Handler;
        private string zipFile;
        private string scriptFolder = Gaming.GameManager.Instance.GetJsScriptsPath();
        private List<Control> ctrls = new List<Control>();
        private bool zipExtractFinished;
        private int numEntries;
        private int entriesDone = 0;
        private float fontSize;
        private bool overwriteWithoutAsking = false;
        public string game;

        private void controlscollect()
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

        public DownloadPrompt(Handler handler, string zipFileName)
        {
            fontSize = float.Parse(Globals.ThemeConfigFile.IniReadValue("Font", "DownloadPromptFontSize"));

            try
            {
                InitializeComponent();

                Handler = handler;

                lbl_Handler.Text = zipFile;

                SuspendLayout();

                BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");

                if (zipFileName == null)
                {
                    if (handler == null)
                    {
                        return;
                    }

                    Text = "Downloading Game Handler";
                    zipFile = string.Format("handler-{0}-v{1}.nc", Handler.Id, Handler.CurrentVersion);
                    BeginDownload();
                }
                else
                {
                    Text = "Extracting Game Handler";
                    zipFile = zipFileName;
                    ExtractHandler();
                }

                controlscollect();

                foreach (Control control in ctrls)
                {
                    control.Font = new Font(Theme_Settings.CustomFont, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                }

                ResumeLayout();

                Activate();
            }
            catch (Exception)
            {
            }
        }

        public DownloadPrompt(Handler handler, string zipFileName, bool overwriteWithoutAsking) : this(handler, zipFileName)
        {
            this.overwriteWithoutAsking = overwriteWithoutAsking;
        }

        private void BeginDownload()
        {
            using (WebClient wc = new WebClient())
            {
                wc.DownloadProgressChanged += wc_DownloadProgressChanged;
                wc.DownloadFileAsync(
                    // Param1 = Link of file
                    new System.Uri($@"https://hub.splitscreen.me/cdn/storage/packages/{Handler.CurrentPackage}/original/handler-{Handler.Id}-v{Handler.CurrentVersion}.nc?download=true"),
                    // Param2 = Path to save
                    Path.Combine(scriptFolder, zipFile)
                );
                
                wc.DownloadFileCompleted += new AsyncCompletedEventHandler(wc_DownloadFileCompleted);
            }
        }

        private void wc_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            prog_DownloadBar.Value = e.ProgressPercentage;
            lbl_ProgPerc.Text = e.ProgressPercentage + "%";
        }

        private void wc_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            ExtractHandler();
            Close();
        }

        private void ExtractProgress(object sender, ExtractProgressEventArgs e)
        {
            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry)
            {
                entriesDone++;
                prog_DownloadBar.Value = ((entriesDone / numEntries) * 100);
                lbl_ProgPerc.Text = ((entriesDone / numEntries) * 100) + "%";
            }

            if (e.EventType == ZipProgressEventType.Extracting_AfterExtractAll || (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry && entriesDone == numEntries))
            {
                zipExtractFinished = true;
            }
            else if (e.EventType == ZipProgressEventType.Extracting_BeforeExtractEntry)
            {
                lbl_Handler.Text = e.CurrentEntry.FileName;
            }
        }

        private void ExtractHandler()
        {
            label1.Text = "Extracting:";

            ZipFile zip = new ZipFile(Path.Combine(scriptFolder, zipFile));
            zip.ExtractProgress += ExtractProgress;
            numEntries = zip.Entries.Count;

            List<string> handlerFolders = new List<string>();

            string scriptTempFolder = scriptFolder + "\\temp";

            if (!Directory.Exists(scriptTempFolder))
            {
                Directory.CreateDirectory(scriptTempFolder);
            }

            foreach (ZipEntry ze in zip)
            {
                if (ze.IsDirectory)
                {
                    int count = 0;
                    foreach (char c in ze.FileName)
                    {
                        if (c == '/')
                        {
                            count++;
                        }
                    }

                    if (count == 1)
                    {
                        handlerFolders.Add(ze.FileName.TrimEnd('/'));
                    }

                    ze.Extract(scriptTempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
                else
                {
                    ze.Extract(scriptTempFolder, ExtractExistingFileAction.OverwriteSilently);
                }
            }

            Regex pattern = new Regex("[\\/:*?\"<>|]");
            string frmHandleTitle = pattern.Replace(zipFile, "");
            string exeName = null;
            int found = 0;

            foreach (string line in File.ReadAllLines(Path.Combine(scriptTempFolder, "handler.js")))
            {
                if (line.ToLower().StartsWith("game.executablename"))
                {
                    int start = line.IndexOf("\"");
                    int end = line.LastIndexOf("\"");
                    exeName = line.Substring(start + 1, (end - start) - 1);
                    found++;
                }
                else if (line.ToLower().StartsWith("game.gamename"))
                {
                    int start = line.IndexOf("\"");
                    int end = line.LastIndexOf("\"");
                    frmHandleTitle = pattern.Replace(line.Substring(start + 1, (end - start) - 1), "");
                    found++;
                }

                if (found == 2)
                {
                    break;
                }
            }

            if (File.Exists(Path.Combine(scriptFolder, frmHandleTitle + ".js")))
            {
                DialogResult ovdialogResult = overwriteWithoutAsking ? DialogResult.Yes : MessageBox.Show("A handler with the name " + (frmHandleTitle + ".js") + " already exists. Do you wish to overwrite it?", "Handler already exists", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (ovdialogResult != DialogResult.Yes)
                {
                    zip.Dispose();
                    Directory.Delete(scriptTempFolder, true);
                    File.Delete(Path.Combine(scriptFolder, zipFile));
                    Close();

                    return;
                }
            }

            if (Directory.Exists(Path.Combine(scriptFolder, frmHandleTitle)))
            {
                Directory.Delete(Path.Combine(scriptFolder, frmHandleTitle), true);
            }

            if (File.Exists(Path.Combine(scriptFolder, frmHandleTitle + ".js")))
            {
                File.Delete(Path.Combine(scriptFolder, frmHandleTitle + ".js"));
            }

            File.Move(Path.Combine(scriptTempFolder, "handler.js"), Path.Combine(scriptFolder, frmHandleTitle + ".js"));

            if (handlerFolders.Count > 0)
            {
                string gameFolder = Path.Combine(scriptFolder, frmHandleTitle);

                Directory.CreateDirectory(gameFolder);

                foreach (string hFolder in handlerFolders)
                {
                    string newFolder = Path.Combine(scriptTempFolder, hFolder);

                    foreach (string dir in Directory.GetDirectories(newFolder, "*", SearchOption.AllDirectories))
                    {
                        Directory.CreateDirectory(Path.Combine(gameFolder, dir.Substring(newFolder.Length + 1)));
                    }

                    foreach (string file_name in Directory.GetFiles(newFolder, "*", SearchOption.AllDirectories))
                    {
                        File.Move(file_name, Path.Combine(gameFolder, file_name.Substring(newFolder.Length + 1)));
                    }

                    Directory.Delete(newFolder, true);
                }
            }

            Directory.Delete(scriptTempFolder, true);

            while (!zipExtractFinished)
            {
                Application.DoEvents();
            }

            zip.Dispose();
            lbl_Handler.Text = "";
            label1.Text = "Finished!";

            File.Delete(Path.Combine(scriptFolder, zipFile));

            if (GameManager.Instance.IsGameAlreadyInUserProfile(exeName, frmHandleTitle))
            {
                GameManager.Instance.AddScript(frmHandleTitle, new bool[] { false, false });    
                return;
            }

            DialogResult dialogResult = MessageBox.Show(
                "Downloading and extraction of " + frmHandleTitle +
                " handler is complete. Would you like to add this game to Nucleus now? You will need to select the game executable to add it.",
                "Download finished! Add to Nucleus?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (dialogResult == DialogResult.Yes)
            {
                GenericGameInfo genericGameInfo = GameManager.Instance.AddScript(frmHandleTitle, new bool[] { false, false });
                SearchGame.Search(exeName, genericGameInfo);
            }
        }
    }
}
