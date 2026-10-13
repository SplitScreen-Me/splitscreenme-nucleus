using System;
using System.Threading;
using System.Windows.Forms;
using static Nucleus.Gaming.Localization.I18n;

namespace Nucleus.Gaming.Forms
{
    /// <summary>"Press OK when…": a plain topmost window shown over the game windows. Use <see cref="UserPrompts"/>.</summary>
    public partial class Prompt : Form
    {
        public bool onpaint = false;

        public Prompt(string message)
            : this(message, false)
        {
        }

        public Prompt(string message, bool onpaint)
        {
            InitializeComponent();
            ApplyTexts();
            PromptLinks.Apply(lbl_Msg, message);
            MakeTopMost();
            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
        }

        /// <summary>Asks for the launcher executable first (an open file dialog), then closes: see <see cref="SelectedPath"/>.</summary>
        public Prompt(string message, bool isOFD, string launcherFileName)
        {
            InitializeComponent();
            ApplyTexts();
            PromptLinks.Apply(lbl_Msg, message);
            MakeTopMost();

            if (isOFD)
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {
                    open.ShowHelp = true; //needed as a work-around to get the prompt to appear
                    if (!string.IsNullOrEmpty(launcherFileName))
                    {
                        open.Title = T("engine.launch.pickLauncher.dialogTitleNamed", new { file = launcherFileName });
                        open.Filter = launcherFileName + "|" + launcherFileName;
                    }
                    else
                    {
                        open.Title = T("engine.launch.pickLauncher.dialogTitle");
                        //"|" separates the filter's parts: it can't appear in the description.
                        open.Filter = T("engine.launch.pickLauncher.fileType").Replace("|", " ") + "|*.exe";
                    }

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        Thread.Sleep(1000);
                        SelectedPath = open.FileName.Replace(@"\", @"\\");
                    }
                }
            }

            btn_Ok.PerformClick();

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
        }

        /// <summary>The launcher picked by the open file dialog (backslashes doubled), or null.</summary>
        public string SelectedPath { get; private set; }

        private void ApplyTexts()
        {
            Text = T("engine.dialogs.prompt.windowTitle");
            btn_Ok.Text = T("common.ok");
        }

        private void MakeTopMost()
        {
            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
