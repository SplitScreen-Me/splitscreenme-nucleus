using System;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    /// <summary>A plain topmost text input shown over the game windows (handler custom prompts). Use <see cref="UserPrompts.AskText"/>.</summary>
    public partial class CustomPrompt : Form
    {
        public CustomPrompt(string message, string prevAnswer)
        {
            InitializeComponent();
            PromptLinks.Apply(lbl_Desc, message);

            txt_UserInput.Text = prevAnswer;

            TopMost = true;
            TopMost = false;
            TopMost = true;

            WindowScrape.Static.HwndInterface.MakeTopMost(Handle);

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
        }

        /// <summary>The text entered, or null when the window was closed without OK.</summary>
        public string Answer { get; private set; }

        private void btn_Ok_Click(object sender, EventArgs e)
        {
            Answer = txt_UserInput.Text;
            Close();
        }
    }
}
