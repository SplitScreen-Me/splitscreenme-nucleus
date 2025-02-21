using Nucleus.Gaming.Controls;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;


namespace Nucleus.Coop.Controls
{
    public partial class CustomSwitch : UserControl
    {      
        [Browsable(true)]
        private bool radioChecked;
        public bool RadioChecked
        {
            get { return radioChecked; }
            set { 
                
                radioChecked = value;

                if (value)
                {
                     label.ForeColor = Color.FromArgb(200, 20, 255, 50);
                }
                else
                {
                    label.ForeColor = Color.Gray;            
                }
            }
        }

        [Browsable(true)]
        public string RadioText
        {
            get { return label.Text; }
            set { label.Text = value; }
        }

        [Browsable(true)]
        public Color TextColor
        {
            get { return label.ForeColor; }
            set { label.ForeColor = value; }
        }

        [Browsable(true)]
        public Color RadioBackColor
        {
            get { return BackColor; }
            set { 
                
                BackColor = value;
                label.BackColor = value; 
                tick.BackColor = value;             
            }
        }

        [Browsable(true)]
        public Cursor TickCursor;

        private Pen outline;

        [Browsable(true)]
        private string radioTooltipText;
        public string RadioTooltipText
        {
            get {return radioTooltipText;}
            set{radioTooltipText = value;}
        }

        public CustomSwitch()
        {
            InitializeComponent();
            SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.FixedHeight | ControlStyles.FixedWidth | ControlStyles.ResizeRedraw,
            true);                           
        }

        private bool init;
        private void Init()
        {
            tick.Cursor = TickCursor;        
            outline = new Pen(Color.Transparent);
            CustomToolTips.SetToolTip(tick, radioTooltipText, "CustomRadioButton", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            init = true;
        }

        private void Tick_Paint(object sender, PaintEventArgs e)
        {
            if(!init)
            {
                Init();
            }

            Scale();

            if (label.Height > tick.Height)
            {
                int diff = label.Height - tick.Height;
                Width += diff;
                tick.Width += diff;
                tick.Height = label.Height;
                label.Location = new Point(label.Location.X + diff,label.Location.Y); 
                Location = new Point(Location.X , Location.Y);
            }

            Graphics g = e.Graphics;

            g.SmoothingMode = SmoothingMode.HighQuality;
            g.CompositingQuality = CompositingQuality.HighQuality;

            g.FillEllipse(GetTickBrushes(Color.FromArgb(120, 255, 255, 255),90), new Rectangle(0, -3, tick.Width-1, tick.Height+5));

            if (radioChecked)
            {
                var brush = GetTickBrushes(Color.FromArgb(255, 0, 146, 60),57);
                outline.Color = g.GetNearestColor(Color.FromArgb(255, brush.LinearColors[1]));
                g.FillEllipse(brush, new Rectangle((tick.Width / 2), 1, (tick.Width / 2), tick.Height-3)); 
                g.DrawEllipse(outline, new Rectangle((tick.Width / 2), 1, (tick.Width / 2), tick.Height-3));
                brush.Dispose();
            }
            else
            {
                var brush = GetTickBrushes(Color.FromArgb(255, 41, 41, 41),-57);
                outline.Color = g.GetNearestColor(Color.FromArgb(255, brush.LinearColors[1]));
                g.FillEllipse(brush, new Rectangle(0, 0, (tick.Width / 2), tick.Height));
                g.DrawEllipse(outline, new Rectangle(0, 0, (tick.Width / 2), tick.Height));
                brush.Dispose();
            }

            label.Font = Font;
        }

        private void Scale()
        {
            if (label.Height > tick.Height)
            {
                int diff = label.Height - tick.Height;
                Width += diff;
                tick.Width += diff;
                tick.Height = label.Height;
                label.Location = new Point(label.Location.X + diff, label.Location.Y);
                Location = new Point(Location.X , Location.Y);
            }
        }

        private LinearGradientBrush GetTickBrushes(Color color, float angle)
        {
            Rectangle brushbounds = new Rectangle(0, 0, tick.Width, tick.Height);

            LinearGradientBrush lgb =
            new LinearGradientBrush(brushbounds, Color.Transparent, color, angle);

            ColorBlend colorBlend = new ColorBlend(3);
            colorBlend.Colors = new Color[3] { Color.Transparent, color, Color.Transparent };
            colorBlend.Positions = new float[3] { 0f, 0.5f, 1f };

            lgb.InterpolationColors = colorBlend;

            lgb.SetBlendTriangularShape(.5f, 1.0f); 
                       
            return lgb;
        }

        private void Tick_Click(object sender, EventArgs e)
        {
            if(radioChecked)
            {
                radioChecked = false;
                label.ForeColor = Color.Gray;  
            }
            else
            {
                radioChecked = true;
                label.ForeColor = Color.FromArgb(255, 20, 255, 50);
            }

            this.OnClick(e);
            tick.Invalidate(false);
        }

        private void CustomRadioButton_Paint(object sender, PaintEventArgs e)
        {
            Rectangle gradientBrushbounds = new Rectangle(0, 0, Width, Height);

            if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
            {
                return;
            }

            Color color = Color.FromArgb(0, 0, 0, 0);
            LinearGradientBrush lgb =
            new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color, 0f);

            ColorBlend topcblend = new ColorBlend(3);
            topcblend.Colors = new Color[3] { Color.Transparent, color, Color.Transparent };
            topcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            lgb.InterpolationColors = topcblend;
            lgb.SetBlendTriangularShape(.5f, 1.0f);

            e.Graphics.FillRectangle(lgb, new Rectangle(0, -3, Width - 1, Height + 5));
            lgb.Dispose();
        }

        private void Tick_MouseEnter(object sender, EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        private void Label_MouseEnter(object sender, EventArgs e)
        {
            base.OnMouseEnter(e);
        }

        private void Label_MouseHover(object sender, EventArgs e)
        {
            base.OnMouseHover(e);
        }
    }
}
