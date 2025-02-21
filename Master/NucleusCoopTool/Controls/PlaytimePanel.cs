using Newtonsoft.Json.Linq;
using Nucleus.Gaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public partial class PlaytimePanel : UserControl
    {
        private string playtime;
        public string Playtime 
        {
            get => playtime;
            set 
            {
                playtime = value;
                playtimeLabelValue.Text = value; 
            }
        }

        private string lastPLayed;
        public string LastPlayed
        {
            get => lastPLayed;
            set
            {
                lastPLayed = value;
                lastPlayedLabelValue.Text = value;
            }   
        }

        private Label playtimeLabel;
        private Label lastPlayedLabel;

        private Label playtimeLabelValue;
        private Label lastPlayedLabelValue;

        public PlaytimePanel()
        {
            InitializeComponent();

            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw |
                     ControlStyles.UserPaint, true);

            AutoSize = true;
            AddLabels();
            Resize += OnResize;
            FontChanged += OnResize;
            
            //UI_Interface.On_GameChange += UpdateInfo;
        }

        private void OnResize(object sender , EventArgs e)
        {
            lastPlayedLabel.Location = new Point(0, 0);
            lastPlayedLabelValue.Location = new Point(lastPlayedLabel.Right, lastPlayedLabel.Location.Y);
            playtimeLabel.Location = new Point(0, lastPlayedLabel.Bottom + 2);
            playtimeLabelValue.Location = new Point(playtimeLabel.Right, playtimeLabel.Location.Y);
            Height = playtimeLabel.Bottom;
        }

        private void AddLabels()
        {
            lastPlayedLabel = new Label();         
            lastPlayedLabel.Text = $"Last Played:";
            lastPlayedLabel.AutoSize = true;
            lastPlayedLabel.TextAlign = ContentAlignment.MiddleLeft;
            lastPlayedLabel.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            lastPlayedLabel.ForeColor = Color.White;
            lastPlayedLabel.Size = new Size(20, 20);
            lastPlayedLabel.Location = new Point(0, 0);
            Controls.Add(lastPlayedLabel);


            lastPlayedLabelValue = new Label();           
            lastPlayedLabelValue.AutoSize = true;
            lastPlayedLabelValue.TextAlign = ContentAlignment.MiddleLeft;
            lastPlayedLabelValue.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            lastPlayedLabelValue.ForeColor = Color.SlateGray;
            lastPlayedLabelValue.Size = new Size(20, 20);
            lastPlayedLabelValue.Location = new Point(lastPlayedLabel.Right, lastPlayedLabel.Location.Y);
            Controls.Add(lastPlayedLabelValue);

            playtimeLabel = new Label();           
            playtimeLabel.Text = $"Play Time:";
            playtimeLabel.AutoSize = true;
            playtimeLabel.TextAlign = ContentAlignment.MiddleLeft;
            playtimeLabel.Font = new Font(Font.FontFamily, 10f , FontStyle.Bold, GraphicsUnit.Pixel, 0);
            playtimeLabel.ForeColor = Color.White;
            playtimeLabel.Size = new Size(20, 20);
            playtimeLabel.Location = new Point(0, lastPlayedLabel.Bottom + 2);
            Controls.Add(playtimeLabel);

            playtimeLabelValue = new Label();
            playtimeLabelValue.AutoSize = true;
            playtimeLabelValue.TextAlign = ContentAlignment.MiddleLeft;
            playtimeLabelValue.Font = new Font(Font.FontFamily, 10f, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            playtimeLabelValue.ForeColor = Color.SlateGray;
            playtimeLabelValue.Size = new Size(20, 20);
            playtimeLabelValue.Location = new Point(playtimeLabel.Right, playtimeLabel.Location.Y);
            Controls.Add(playtimeLabelValue);
        }

        public void UpdateInfo()
        {
            ////Size = new Size();

            //GenericGameInfo gameInfo = UI_Interface.Current_GameInfo;

            //Hide();

            //if (gameInfo == null)
            //{
            //    return;
            //}

            //LastPlayed = gameInfo.MetaInfo.LastPlayedAt;
            //Playtime = gameInfo.MetaInfo.TotalPlayTime;

            //Show();

            //Invalidate();
        }
    }
}
