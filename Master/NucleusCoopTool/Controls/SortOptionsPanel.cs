using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.UI;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{

    public partial class SortCheckBox : CustomCheckBox
    {
        public SortOptions SortOptions { get; set; }
    }

    public partial class SortOptionsPanel : UserControl
    {
        private SortCheckBox last;
        List<string> saveToIniList = new List<string>();

        public List<SortOptions> SortGamesOptions => sortOptions;
        private List<SortOptions> sortOptions = new List<SortOptions>();

        private Color backGradient;

        private bool useGradient;

        public SortOptionsPanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
         ControlStyles.OptimizedDoubleBuffer |
         ControlStyles.ResizeRedraw |
         ControlStyles.UserPaint, true);

            AutoSize = true;
            Visible = false; 
            
            if (Theme_Settings.BackgroundGradientColor.A == 1)
            {
                backGradient = Theme_Settings.BackgroundGradientColor;
                BackColor = Color.Transparent;
                useGradient = true;
            }
            else
            {
                backGradient = Color.FromArgb(0, 0, 0, 0);
                BackColor = Theme_Settings.WindowPanelBackColor;
            }

            Font = new Font(Theme_Settings.CustomFont, 9.75F, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            Resize += On_Resize;
            AddCheckboxes();
        }      

        private void AddCheckboxes()
        {
            SortCheckBox descending = new SortCheckBox();
            descending.Name = "Sort_1";
            descending.Text = "Sort Descending";
            descending.Location = new Point(10, 15);
            descending.AutoSize = true;
            descending.SortOptions = SortOptions.Descending;

            descending.Checked = App_Misc.GamesSorting.Contains(descending.Name);
            Controls.Add(descending);

            SortCheckBox byMaxPlayer = new SortCheckBox();
            byMaxPlayer.Name = "Sort_2";
            byMaxPlayer.Text = "Sort By Max Supported Players";
            byMaxPlayer.Location = new Point(10, descending.Bottom + 2);
            byMaxPlayer.AutoSize = true;
            byMaxPlayer.SortOptions = SortOptions.MaxPLayer;
            byMaxPlayer.Checked =  App_Misc.GamesSorting.Contains(byMaxPlayer.Name);
            Controls.Add(byMaxPlayer);

            SortCheckBox byPlayTime = new SortCheckBox();
            byPlayTime.Name = "Sort_3";
            byPlayTime.Text = "Sort By Play Time";
            byPlayTime.Location = new Point(10, byMaxPlayer.Bottom + 2);
            byPlayTime.AutoSize = true;
            byPlayTime.SortOptions = SortOptions.PlayTime;
            byPlayTime.Checked = App_Misc.GamesSorting.Contains(byPlayTime.Name);
            Controls.Add(byPlayTime);

            SortCheckBox byLastPlayed = new SortCheckBox();
            byLastPlayed.Name = "Sort_4";
            byLastPlayed.Text = "Sort By Last Played";
            byLastPlayed.Location = new Point(10, byPlayTime.Bottom + 2);
            byLastPlayed.AutoSize = true;
            byLastPlayed.SortOptions = SortOptions.LastPLayed;
            byLastPlayed.Checked = App_Misc.GamesSorting.Contains(byLastPlayed.Name);
            Controls.Add(byLastPlayed);


            SortCheckBox byGamepad = new SortCheckBox();
            byGamepad.Name = "Show_Type1";
            byGamepad.Text = "Supports Gamepads";
            byGamepad.Location = new Point(10, byLastPlayed.Bottom + 7);//+7 to have a space
            byGamepad.AutoSize = true;
            byGamepad.SortOptions = SortOptions.Gamepads;
            byGamepad.Checked = App_Misc.GamesSorting.Contains(byGamepad.Name);
            Controls.Add(byGamepad);

            SortCheckBox bySKbm = new SortCheckBox();
            bySKbm.Name = "Show_Type2";
            bySKbm.Text = "Supports Single Keyboard and Mouse";
            bySKbm.Location = new Point(10, byGamepad.Bottom + 2);
            bySKbm.AutoSize = true;
            bySKbm.SortOptions = SortOptions.SKbm;
            bySKbm.Checked = App_Misc.GamesSorting.Contains(bySKbm.Name);
            Controls.Add(bySKbm);


            SortCheckBox byKbm = new SortCheckBox();
            byKbm.Name = "Show_Type3";
            byKbm.Text = "Supports Multiple Keyboards And Mice";
            byKbm.Location = new Point(10, bySKbm.Bottom + 2);
            byKbm.AutoSize = true;
            byKbm.SortOptions = SortOptions.Kbm;
            byKbm.Checked = App_Misc.GamesSorting.Contains(byKbm.Name);
            Controls.Add(byKbm);

            last = byKbm;

            foreach (SortCheckBox cb in Controls)
            {
                cb.SelectionColor = Color.FromArgb(255, 31, 34, 35);
                cb.CheckColor = Color.FromArgb(255,Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B);
                cb.BorderColor = Color.White;
                cb.BackColor = Color.Transparent;
                cb.CheckedChanged += CheckChanged;

                if(cb.Checked)
                {
                    sortOptions.Add(cb.SortOptions);
                    saveToIniList.Add(cb.Name);
                }
            }

            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 20, 20);
        }

        private void CheckChanged(object sender, object e)
        {
            SortCheckBox cb = sender as SortCheckBox;
       
            if(cb.Checked)
            {
                if (cb.Name.StartsWith("Sort_"))
                {
                    foreach (SortCheckBox scb in Controls)
                    {
                        if (cb != scb && scb.Name.StartsWith("Sort_"))
                        {
                            scb.Checked = false;
                            saveToIniList.Remove(scb.Name);
                        }
                    }      
                }

                saveToIniList.Add(cb.Name);
                sortOptions.Add(cb.SortOptions);
            }
            else
            {
                saveToIniList.Remove(cb.Name);
                sortOptions.Remove(cb.SortOptions);        
            }

            App_Misc.GamesSorting = saveToIniList;
            SortGameFunction.SortGames(sortOptions);
        }

        private void On_Resize(object sender, object e)
        {
            Height = last.Bottom + 15;
            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 20, 20);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle gradientBrushbounds = new Rectangle(0, 0, Width, Height);

            if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
            {
                return;
            }

            Color color = Color.FromArgb(useGradient ? 100 : 0, backGradient.R, backGradient.G, backGradient.B);
            Color color2 = Color.FromArgb(useGradient ? 120 : 0, backGradient.R, backGradient.G, backGradient.B);
            LinearGradientBrush lgb =
            new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color, 90f);

            ColorBlend topcblend = new ColorBlend(3);
            topcblend.Colors = new Color[3] { Color.Transparent, color, color2 };
            topcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            GraphicsPath graphicsPath = FormGraphicsUtil.MakeRoundedRect(gradientBrushbounds, 10, 10, false, false, false, true);

            lgb.InterpolationColors = topcblend;
            e.Graphics.FillPath(lgb, graphicsPath);

            lgb.Dispose();
            graphicsPath.Dispose();
        }
    }
}
