using Nucleus.Gaming;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SplitTool
{
    public partial class AutoFindGames : Form
    {
        public AutoFindGames()
        {
            InitializeComponent();

            var list = GameManager.Instance.User.FoldersToSearch;
            for (int i = 0; i < list.Count; i++)
            {
                list_FoldersToSearch.Items.Add(list[i]);
            }
        }

        private void button_auto_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog browser = new FolderBrowserDialog())
            {
                if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string folder = browser.SelectedPath;

                    if (!GameManager.Instance.User.FoldersToSearch.Contains(folder))
                    {
                        GameManager.Instance.User.FoldersToSearch.Add(folder);
                        GameManager.Instance.UpdateUserProfile();
                        list_FoldersToSearch.Items.Add(folder);
                    }
                }
            }
        }

        private void button_Remove_Click(object sender, EventArgs e)
        {
            int selected = list_FoldersToSearch.SelectedIndex;
            if (selected != -1)
            {
                GameManager.Instance.User.FoldersToSearch.RemoveAt(selected);
                GameManager.Instance.UpdateUserProfile();
                list_FoldersToSearch.Items.RemoveAt(selected);
            }
        }

        private void btn_Update_Click(object sender, EventArgs e)
        {
            GameManager.Instance.UpdateUserProfile();
        }
    }
}
