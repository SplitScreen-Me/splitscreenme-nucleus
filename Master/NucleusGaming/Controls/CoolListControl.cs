using Jint.Parser;
using Nucleus.Gaming;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.UI;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace SplitTool.Controls
{
    public class CoolListControl : UserControl, IDynamicSized
    {
        private Label titleLabel;
        protected LinkLabel descLabel;
        protected int defaultHeight = 72;
        protected int expandedHeight = 156;
        public object ImageUrl;
        private Color backBrushColor;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleparams = base.CreateParams;
                handleparams.ExStyle = 0x02000000;
                return handleparams;
            }
        }

        public string Title
        {
            get => titleLabel.Text;
            set => titleLabel.Text = value;
        }

        public string Details
        {
            get => descLabel.Text;
            set
            {
                descLabel.Text = value;
                SetDescLabelLinkArea(value);
            }
        }

        public bool EnableHighlighting { get; private set; }
        public object Data { get; set; }
        public event Action<object> OnSelected;

        public CoolListControl(bool enableHightlighting)
        {
            EnableHighlighting = enableHightlighting;

            string customFont = Globals.ThemeConfigFile.IniReadValue("Font", "FontFamily");
            backBrushColor = Color.FromArgb(60,Theme_Settings.BackgroundGradientColor.R, Theme_Settings.BackgroundGradientColor.G, Theme_Settings.BackgroundGradientColor.B);

            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            BackColor = Color.Transparent;
            Font font = new Font(customFont, 10.0f, FontStyle.Bold, GraphicsUnit.Point, 0);

            titleLabel = new Label
            {
                Font = font,
                BackColor = Color.Transparent
            };

            descLabel = new LinkLabel
            {
                Font = font,
                BackColor = Color.Transparent,
                LinkColor = Color.Orange,
                ActiveLinkColor = Color.DimGray
            };

            descLabel.LinkClicked += DescLabelLinkClicked;

            Controls.Add(titleLabel);
            Controls.Add(descLabel);
            
            DPIManager.Register(this);
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
                        descLabel.Tag = search;
                        descLabel.LinkArea = new LinkArea(value.IndexOf(search), search.Length);
                        return;
                    }

                    descLabel.LinkArea = new LinkArea(0, 0);
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
                    descLabel.Tag = link;

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
                        descLabel.Text = test;

                        descLabel.LinkArea = new LinkArea(value.IndexOf(shorten) - 1, shorten.Length);

                        return;
                    }
                }

                descLabel.LinkArea = new LinkArea(0, 0);
            }
            catch { }
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            titleLabel.AutoSize = true;
            titleLabel.MaximumSize = new Size(370, 0);
            titleLabel.Location = new Point(10, 10);

            descLabel.AutoSize = true;
            descLabel.MaximumSize = new Size((int)370, 0);
            descLabel.Location = new Point(titleLabel.Location.X, titleLabel.Bottom + 10);

            Height = descLabel.Location.Y + descLabel.Height + 10;
            foreach (Control picture in Controls)
            {
                if (picture.GetType() == typeof(PictureBox))
                {
                    Height = picture.Height + 20;
                    picture.BackColor = Color.Transparent;
                }
            }

            VerticalScroll.Value = 0;//avoid weird glitchs if scrolled before maximizing the main window.          
        }

        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            Control c = e.Control;
            c.Click += C_Click;

            DPIManager.Update(this);
        }

        private void C_Click(object sender, EventArgs e)
        {
            OnClick(e);
        }

        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            if (OnSelected != null)
            {
                OnSelected(Data);
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle bounds = new Rectangle(2, 1, Width - 3, Height-1);

            if (bounds.Width == 0 || bounds.Height == 0)
            {
                return;
            }

            GraphicsPath graphicsPath = FormGraphicsUtil.MakeRoundedRect(bounds, 15, 15, true, true, true, true);
            SolidBrush brush = new SolidBrush(backBrushColor);

            e.Graphics.FillPath(brush, graphicsPath);

            graphicsPath.Dispose();
            brush.Dispose();
            
        }
    }
}
