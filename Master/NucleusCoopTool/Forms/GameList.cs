using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public partial class GameList : Form
    {
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleparams = base.CreateParams;
                handleparams.ExStyle = 0x02000000;
                return handleparams;
            }
        }

        private GenericGameInfo clicked;

        public GenericGameInfo Selected => clicked;
        protected override Size DefaultSize => new Size(440, 710);

        public GameList(List<GenericGameInfo> games)
        {
            string[] rgb_MouseOverColor = Globals.ThemeConfigFile.IniReadValue("Colors", "MouseOver").Split(',');

            InitializeComponent();

            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            btnOk.FlatAppearance.MouseOverBackColor = Color.FromArgb(int.Parse(rgb_MouseOverColor[0]), int.Parse(rgb_MouseOverColor[1]), int.Parse(rgb_MouseOverColor[2]), int.Parse(rgb_MouseOverColor[3])); ;

            GameManager manager = GameManager.Instance;

            foreach (GenericGameInfo game in games)
            {
                GameControlAlt con = new GameControlAlt(game, null, false)
                {
                    ForeColor = Color.White,
                    Width = listGames.Width,
                    Image = ImageCache.GetImage(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png")),
                };

                con.Click += Con_Click;

                listGames.Controls.Add(con);                     
            }
        }

        private void Con_Click(object sender, EventArgs e)
        {
            clicked = ((GameControlAlt)sender).GameInfo;
            btnOk.Enabled = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
