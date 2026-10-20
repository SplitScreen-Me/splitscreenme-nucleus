using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Http_Client;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public partial class NewsFeed : UserControl
    {
        private Label feedLabel;
        private Label feedLabelCopy;
        private Button redirectToBtn;
        private System.Windows.Forms.Timer scrollTimer;

        private string rawFeedResultString;
        private string rawFeedResultStringCache;
        private string feedResultString;
        private string learnMoreLink;
        private bool hasLink;
        private bool timersInitialized;

        private bool scrollLabel = true;
        private bool scrollLabelCopy = false;
        private bool isConnected;

        private int startOffset = 0;
        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;
        private int loopOffset = 0;//where the label on right start scrolling


        public NewsFeed()
        {
            InitializeComponent();
            BackColor = Color.FromArgb(255, 31, 34, 35);
            Visible = false;
            MouseDown += SendMoveWindowMoveMessage;
            Anchor = AnchorStyles.Left/* | AnchorStyles.Right*/;
            Resize += Form_Resize;
            _ = StartUpdateLoop(600000, 15000, Links.NewsFeed);
        }

        private async Task StartUpdateLoop(int intervalMs, int timeoutMs, string url)
        {
            while (true)
            {
                try
                {
                    //pause it when a handler is running
                    while (GenericGameHandler.IsRunning)
                    {
                        await Task.Delay(intervalMs);
                    }

                    isConnected = await Http_Client.CheckInternetAccessAsync(timeoutMs);

                    if (isConnected)
                    {
                        await NewsFeed_Update();
                    }
                }
                catch
                {

                }

                await Task.Delay(intervalMs);
            }
        }

        private async Task NewsFeed_Update()
        {
            //grab github news and add to feed text
            rawFeedResultString = await Http_Client.GetAsync(Links.NewsFeed);

            if (rawFeedResultString == rawFeedResultStringCache)
            {
                return;
            }

            rawFeedResultStringCache = rawFeedResultString;

            if (string.IsNullOrEmpty(rawFeedResultString))
            {
                Visible = false;
                scrollTimer?.Stop();
            }
            else
            {
                string[] feedLines = rawFeedResultString.Split('§');

                if (feedLines.Length == 2)
                {
                    feedResultString = feedLines[0].Replace('\n', ' ');
                    learnMoreLink = feedLines[1];
                    hasLink = true;
                }
                else if (feedLines.Length == 1)
                {
                    feedResultString = feedLines[0].Replace('\n', ' ');
                    hasLink = false;
                }

                if (feedLines.Length > 0)
                {
                    UpdateControls();

                    if (scrollTimer != null)
                    {
                        if (!scrollTimer.Enabled)
                        {
                            scrollTimer.Start();
                        }
                    }

                    Visible = true;
                }
                else
                {
                    Visible = false;
                    scrollTimer?.Stop();
                }
            }
        }

        private void AddLabels()
        {
            feedLabel = new Label();
            feedLabel.AutoSize = true;
            feedLabel.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            feedLabel.ForeColor = Color.AntiqueWhite;
            feedLabel.BackColor = BackColor;
            feedLabel.Resize += On_FeedLabel_Resize;
            feedLabel.Text = "H";//just to initalize its height
            feedLabel.MouseDown += SendMoveWindowMoveMessage;
            Controls.Add(feedLabel);

            feedLabel.Text = feedResultString;
            feedLabel.Location = new Point(Right - startOffset, 0);

            feedLabelCopy = new Label();
            feedLabelCopy.AutoSize = true;
            feedLabelCopy.Font = feedLabel.Font;
            feedLabelCopy.ForeColor = feedLabel.ForeColor;
            feedLabelCopy.BackColor = BackColor;
            feedLabelCopy.MouseDown += SendMoveWindowMoveMessage;
            Controls.Add(feedLabelCopy);

            feedLabelCopy.Text = feedLabel.Text;
            feedLabelCopy.Location = new Point(Right - startOffset, feedLabel.Location.Y);

            Height = feedLabel.Height;
        }

        private void Init_Timer()
        {
            if (!timersInitialized)
            {
                Init_Timers();
            }
        }

        private void UpdateRedirectButton(bool visible)
        {
            if (visible)
            {
                if (redirectToBtn == null)
                {
                    redirectToBtn = new Button();
                    redirectToBtn.Font = new Font("Segoe UI", 8f, FontStyle.Regular | FontStyle.Underline);
                    redirectToBtn.Text = "Learn more";
                    redirectToBtn.AutoSize = false;
                    redirectToBtn.ForeColor = Color.Aqua;
                    redirectToBtn.FlatStyle = FlatStyle.Flat;
                    redirectToBtn.FlatAppearance.BorderSize = 0;
                    redirectToBtn.BackColor = Color.FromArgb(255, 50, 50, 50);
                    redirectToBtn.Dock = DockStyle.Right;
                    redirectToBtn.Click += RedirectToBtn_Click;
                    redirectToBtn.Cursor = Theme_Settings.Hand_Cursor;
                    redirectToBtn.Paint += RedirectToBtn_Paint;
                    Controls.Add(redirectToBtn);
                    redirectToBtn.BringToFront();
                    startOffset = redirectToBtn.Width;
                }
            }
            else
            {
                if (redirectToBtn != null)
                {
                    Controls.Remove(redirectToBtn);
                    redirectToBtn.Dispose();
                    redirectToBtn = null;
                    startOffset = 0;
                }
            }
        }

        private void UpdateControls()
        {
            Init_Timer();

            UIThread.Post(() =>
            {
                if (feedLabel == null)
                {
                    UpdateRedirectButton(hasLink);
                    AddLabels();
                    return;
                }

                UpdateRedirectButton(hasLink);

                scrollLabel = true;
                scrollLabelCopy = false;

                feedLabel.Text = feedResultString;
                feedLabel.Location = new Point(Right - startOffset, 0);
                feedLabelCopy.Text = feedResultString;
                feedLabelCopy.Location = new Point(Right - startOffset, feedLabel.Location.Y);
            });
        }

        private void RedirectToBtn_Paint(object sender, PaintEventArgs e)
        {
            Button btn = sender as Button;
            e.Graphics.DrawLine(Pens.White, 1, 1, 1, btn.Height - 1);
        }

        private void RedirectToBtn_Click(object sender, EventArgs e) => Process.Start(learnMoreLink);

        private void On_FeedLabel_Resize(object sender, EventArgs e)
        {
            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, this.Width, this.Height, 5, 5);

            if (redirectToBtn != null)
            {
                redirectToBtn.Location = new Point(Width - redirectToBtn.Width, feedLabel.Location.Y + (feedLabel.Height/2 - redirectToBtn.Height/2));
            }

            //MinimumSize = Size;
            Size = new Size(UI_Interface.WindowPanel.Width / 2, Height);
        }

        private void Init_Timers()
        {
            scrollTimer?.Dispose();
            scrollTimer = new System.Windows.Forms.Timer();
            scrollTimer.Interval = 16;
            scrollTimer.Tick += ScrollTimer_Tick;
            scrollTimer.Start();
            timersInitialized = true;
        }

        private void ScrollTimer_Tick(object state, EventArgs eventArgs)
        {

            if(loopOffset == 0)
            {
                return;
            }

            if (scrollLabel)
            {
                feedLabel.Location = new Point(feedLabel.Location.X - 1, feedLabel.Location.Y);
            }

            if (scrollLabelCopy)
            {
                feedLabelCopy.Location = new Point(feedLabelCopy.Location.X - 1, feedLabelCopy.Location.Y);
            }

            if (feedLabel.Right == loopOffset)
            {
                feedLabelCopy.Location = new Point(Right - startOffset, feedLabel.Location.Y);
                scrollLabelCopy = true;
            }

            if (feedLabel.Right <= 0)
            {
                scrollLabel = false;
            }

            if (feedLabelCopy.Right == loopOffset)
            {
                feedLabel.Location = new Point(Right - startOffset, feedLabel.Location.Y);
                scrollLabel = true;
            }

            if (feedLabel.Right <= loopOffset && feedLabel.Right > 0)
            {
                scrollLabelCopy = true;
            }

            if (feedLabel.Right <= 0)
            {
                scrollLabel = false;
            }

            if (feedLabelCopy.Right <= 0)
            {
                scrollLabel = true;
            }
        }

        private void Form_Resize(object sender, EventArgs e)
        {
            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, this.Width, this.Height, 5, 5);


            Size = new Size(UI_Interface.WindowPanel.Width / 2, Height);

            if(loopOffset == 0)
            {
                loopOffset = Width / 2;
            }

            if(feedLabel == null || feedLabelCopy == null)
            {
                return;
            }

            if (!scrollLabelCopy)
            {
                feedLabelCopy.Location = new Point(Right - startOffset, feedLabelCopy.Location.Y);
            }

            if (!scrollLabel)
            {
                feedLabel.Location = new Point(Right - startOffset, feedLabel.Location.Y);
            }

            if(feedLabel.Left > (Right - startOffset))
            {
                feedLabel.Location = new Point(Right - startOffset, feedLabel.Location.Y);
            }

            if (feedLabelCopy.Left > (Right - startOffset))
            {
                feedLabelCopy.Location = new Point(Right - startOffset, feedLabelCopy.Location.Y);
            }

            if (redirectToBtn != null)
            {
                redirectToBtn.Location = new Point(Width - redirectToBtn.Width, feedLabel.Location.Y + (feedLabel.Height / 2 - redirectToBtn.Height / 2));
            }
        }

        private void SendMoveWindowMoveMessage(object sender, EventArgs e)
        {
            User32Interop.ReleaseCapture();
            IntPtr nucHwnd = User32Interop.FindWindow(null, UI_Interface.MainWindowText);
            User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
        }

    }
}
