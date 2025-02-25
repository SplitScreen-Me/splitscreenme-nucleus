using Nucleus.Gaming.Cache;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    public partial class Prompt : Form
    {
        private bool hasOpenFileDialog = false;
        private string exeName;
        public bool onpaint = false;
        public Prompt(string message)
        {
            onpaint = false;
            InitializeComponent();
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            lbl_Msg.Text = message;

            lbl_Msg.LinkColor = Color.Orange;
            lbl_Msg.ActiveLinkColor = Color.DimGray;
            SetDescLabelLinkArea(message);
            lbl_Msg.LinkClicked += DescLabelLinkClicked;

            hasOpenFileDialog = false;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);
        }

        public Prompt(string message, bool onpaint)
        {
            InitializeComponent();
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            lbl_Msg.Text = message;
            SetDescLabelLinkArea(message);

            lbl_Msg.LinkColor = Color.Orange;
            lbl_Msg.ActiveLinkColor = Color.DimGray;
            lbl_Msg.LinkClicked += DescLabelLinkClicked;

            hasOpenFileDialog = false;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);
        }

        public Prompt(string message, bool isOFD, string launcherFileName)
        {
            onpaint = false;
            InitializeComponent();
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            lbl_Msg.Text = message;
            SetDescLabelLinkArea(message);

            lbl_Msg.LinkColor = Color.Orange;
            lbl_Msg.ActiveLinkColor = Color.DimGray;
            lbl_Msg.LinkClicked += DescLabelLinkClicked;

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

        private void DescLabelLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            var link = sender as LinkLabel;
            Process.Start(link.Tag.ToString());
        }

        private void SetDescLabelLinkArea(string value)
        {
            //Get shorten text
            string shorten = null;
            string search = null;

            try
            {
                int shStart = value.IndexOf('[');
                int shEnd = value.IndexOf(']');

                if (shStart != -1 && shEnd != -1)
                {
                    shorten = value.Substring(shStart + 1, (shEnd - shStart) - 1);
                }

                if (shorten == null)//to keep backward compat !!!
                {
                    var wordList = value.Split(' ').ToList();
                    search = wordList.Where(word => word.StartsWith("http:") || word.StartsWith("file:") ||
                                                                              word.StartsWith("mailto:") || word.StartsWith("ftp:") ||
                                                                              word.StartsWith("https:") || word.StartsWith("gopher:") ||
                                                                              word.StartsWith("nntp:") || word.StartsWith("prospero:") ||
                                                                              word.StartsWith("telnet:") || word.StartsWith("news:") ||
                                                                              word.StartsWith("wais:") || word.StartsWith("outlook:")).FirstOrDefault();

                    if (search != null)
                    {
                        lbl_Msg.Tag = search;
                        lbl_Msg.LinkArea = new LinkArea(value.IndexOf(search), search.Length);
                        return;
                    }

                    lbl_Msg.LinkArea = new LinkArea(0, 0);
                }

                string link = null;

                //search and build the link from value
                int linkStart = value.IndexOf('{');
                int linkEnd = value.IndexOf('}');

                if (linkStart != -1 && linkEnd != -1)
                {
                    link = value.Substring(linkStart + 1, (linkEnd - linkStart) - 1);
                }
                //

                if (link != null)
                {
                    lbl_Msg.Tag = link;

                    //Replace short value and link in the text("holder" in bellow code).
                    string replaceHolder = null;

                    int holderStart = value.IndexOf('[');
                    int holderEnd = value.IndexOf('}') + 1;

                    if (holderStart != -1 && holderEnd != -1)
                    {
                        replaceHolder = value.Substring(holderStart, holderEnd - holderStart);
                    }
                    //

                    if (replaceHolder != null)
                    {
                        //replace holder by shorten in value and update the descLabel text
                        string test = value.Replace(replaceHolder, shorten);
                        lbl_Msg.Text = test;

                        lbl_Msg.LinkArea = new LinkArea(value.IndexOf(shorten) - 1, shorten.Length);

                        return;
                    }
                }

                lbl_Msg.LinkArea = new LinkArea(0, 0);
            }
            catch { }
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {

            Close();
        }
    }
}
