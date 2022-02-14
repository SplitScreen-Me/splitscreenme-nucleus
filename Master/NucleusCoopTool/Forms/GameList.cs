using Nucleus.Gaming;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public partial class GameList : BaseForm
    {
        private GenericGameInfo clicked;

        public GenericGameInfo Selected => clicked;

        protected override Size DefaultSize => new Size(440, 710);

        public GameList(List<GenericGameInfo> games)
        {
            InitializeComponent();

            GameManager manager = GameManager.Instance;
            foreach (GenericGameInfo game in games)
            {
                GameControl con = new GameControl(game, null)
                {
                    Width = listGames.Width
                };
                con.Click += Con_Click;

                listGames.Controls.Add(con);
            }
        }

        private void Con_Click(object sender, EventArgs e)
        {
            clicked = ((GameControl)sender).GameInfo;
            btnOk.Enabled = true;
        }

        private void btnOk_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
