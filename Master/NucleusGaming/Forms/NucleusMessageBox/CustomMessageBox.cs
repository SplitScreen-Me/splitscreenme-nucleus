using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    /// <summary>A plain topmost message window (the text can be selected and copied). Use <see cref="UserPrompts.Notice"/>.</summary>
    public partial class CustomMessageBox : Form
    {
        private const int Gap = 12;
        private const int MaxTextWidth = 560;
        private const int MaxTextHeight = 420;

        public CustomMessageBox(string title, string message, bool format)
        {
            InitializeComponent();

            Text = title;
            okButton.Text = Localization.I18n.T("common.ok");
            TopMost = true;
            messageText.Text = (format ? FormatText(message) : message ?? "").Replace("\r\n", "\n").Replace("\n", Environment.NewLine);

            LayoutToText();
        }

        private void LayoutToText()
        {
            Size text = TextRenderer.MeasureText(messageText.Text, messageText.Font, new Size(MaxTextWidth, int.MaxValue), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

            int width = Math.Max(Math.Min(text.Width + SystemInformation.VerticalScrollBarWidth, MaxTextWidth), 240);
            int height = Math.Min(text.Height + 4, MaxTextHeight);

            messageText.ScrollBars = text.Height + 4 > MaxTextHeight ? ScrollBars.Vertical : ScrollBars.None;
            messageText.SetBounds(Gap, Gap, width, height);
            okButton.Location = new Point(Gap + width - okButton.Width, Gap + height + Gap);
            ClientSize = new Size(width + 2 * Gap, okButton.Bottom + Gap);
        }

        private static string FormatText(string message)
        {
            string[] words = (message ?? "").Split(' ');
            string formated = string.Empty;

            int wordsBeforeNewLine = 10;
            int start = wordsBeforeNewLine;

            for (int i = 0; i < words.Length; i++)
            {
                if (start == i)
                {
                    formated += words[i] + '\n';
                    start += wordsBeforeNewLine;
                }
                else if (words[i].EndsWith("."))
                {
                    formated += words[i] + '\n';
                    start += wordsBeforeNewLine;
                }
                else
                {
                    formated += words[i] + ' ';
                }
            }

            return formated;
        }

        private void OkButton_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
