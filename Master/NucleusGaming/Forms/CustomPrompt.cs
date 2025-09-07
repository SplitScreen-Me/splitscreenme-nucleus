using Nucleus.Gaming.Cache;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    public partial class CustomPrompt : Form
    {
        private int index;

        public CustomPrompt(string message, string prevAnswer, int i)
        {
            string theme = Globals.ThemeFolder;
            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            InitializeComponent();
            lbl_Desc.Text = message;
            SetDescLabelLinkArea(message);

            lbl_Desc.LinkColor = Color.Orange;
            lbl_Desc.ActiveLinkColor = Color.DimGray;
            lbl_Desc.LinkClicked += DescLabelLinkClicked;

            txt_UserInput.Text = prevAnswer;

            index = i;
            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
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
                        lbl_Desc.Tag = search;
                        lbl_Desc.LinkArea = new LinkArea(value.IndexOf(search), search.Length);
                        return;
                    }

                    lbl_Desc.LinkArea = new LinkArea(0, 0);
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
                    lbl_Desc.Tag = link;

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
                        lbl_Desc.Text = test;

                        lbl_Desc.LinkArea = new LinkArea(value.IndexOf(shorten) - 1, shorten.Length);

                        return;
                    }
                }

                lbl_Desc.LinkArea = new LinkArea(0, 0);
            }
            catch { }
        }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            CustomPromptRuntime.customValue[index] = txt_UserInput.Text;
            Close();
        }
    }
}
