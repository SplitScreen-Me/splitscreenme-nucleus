using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;


namespace Nucleus.Gaming.Forms
{
    public partial class GamePathForm : Form, IDynamicSized
    {
        private List<Control> ctrls = new List<Control>();
        private float fontSize;
        private Cursor hand_Cursor;
        private Cursor default_Cursor;
        public static GamePathForm Instance;

        public GamePathForm()
        {
            Instance = this;
            InitializeComponent();
            ShowInTaskbar = false;
            TopMost = true;
            default_Cursor = Theme_Settings.Default_Cursor;
            hand_Cursor = Theme_Settings.Hand_Cursor;

            fontSize = 10f;
            ForeColor = Theme_Settings.ControlsForeColor;
            BackColor = Color.Black;
            BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "other_backgrounds.jpg");
            btn_customPath.BackColor = Theme_Settings.ButtonsBackColor;
            btn_customPath.FlatAppearance.BorderColor = Color.White;
            btn_customPath.FlatAppearance.BorderSize = 1;
            btn_customPath.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;

            btn_delPath.BackColor = Theme_Settings.ButtonsBackColor;
            btn_delPath.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_delPath.FlatAppearance.BorderColor = Color.White;
            btn_delPath.FlatAppearance.BorderSize = 1;

            closeBtn.BackColor = Theme_Settings.ButtonsBackColor;
            closeBtn.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            closeBtn.FlatAppearance.BorderColor = Color.White;
            closeBtn.FlatAppearance.BorderSize = 1;
       
            bool iniValueExists = !string.IsNullOrEmpty(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));

            if(iniValueExists)
            {
                autoSearchChkBox.Checked = bool.Parse(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));
            }
            else
            {
                Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=False" });
                autoSearchChkBox.Checked = false;
            }

            controlscollect();

            foreach (Control control in ctrls)
            {             
                if (control.Name != "panel1")
                {
                    control.Cursor = hand_Cursor;
                }
                else
                {
                    control.Cursor = default_Cursor;
                }
            }

            closeBtn.Cursor = hand_Cursor;

            if(GameManager.Instance.GamesSearcheDirectories.Count > 0)
            {
                foreach (var path in GameManager.Instance.GamesSearcheDirectories)
                {
                    disksBox.Items.Add(path.ToString(), true);
                }
            }
            else
            {
                Shown += On_Shown;
            }
            CustomToolTips.SetToolTip(autoSearchChkBox, "Auto game executable search will be processed after handler installation.", "autoSearchChkBox", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            FormClosed += OnFormClosed;
            CenterToScreen();
            DPIManager.Register(this);
            DPIManager.Update(this);
        }

        private void On_Shown(object sender, EventArgs e)
        {
            btn_customPath.BackColor = Theme_Settings.MouseOverBackColor;
            Btn_customPath_Click(null,null);   
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            SuspendLayout();

            if (scale > 1.0F)
            {
                var heightField = typeof(CheckedListBox).GetField(
                "scaledListItemBordersHeight",
                BindingFlags.NonPublic | BindingFlags.Instance
                );

                var addedHeight = 10 * (int)scale;

                heightField.SetValue(disksBox, addedHeight);
            }
            else
            {
                var heightField = typeof(CheckedListBox).GetField(
               "scaledListItemBordersHeight",
               BindingFlags.NonPublic | BindingFlags.Instance
               );

                var addedHeight = 6;

                heightField.SetValue(disksBox, addedHeight);
            }

            float textBoxFontSize = (Font.Size + 4) * scale;

            foreach (Control c in ctrls)
            {
                if (c.GetType() == typeof(CheckedListBox))
                {
                    c.Font = new Font(Theme_Settings.CustomFont, c.Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    c.Size = new Size(c.Width, (int)(c.Height / scale));
                }

                if (c.GetType() == typeof(TextBox))
                {
                    c.Font = new Font(Theme_Settings.CustomFont, textBoxFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
            }

            ResumeLayout();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x020 || m.Msg == 0x0a0)//Do not reset our custom cursor when mouse hover over the Form background(needed because of the custom resizing/moving messges handling) 
            {
                Cursor.Current = default_Cursor;
                return;
            }

            base.WndProc(ref m);
        }

        private void Btn_customPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                fbd.ShowNewFolderButton = false;
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.Description = "Select the folder(s) to search for games (e.g., C:\\Program Files\\steam).";

                DialogResult result = fbd.ShowDialog();
               
                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    if (GameManager.Instance.GamesSearcheDirectories.Contains(fbd.SelectedPath))
                    {
                        return;
                    }

                    int index = disksBox.Items.Add(fbd.SelectedPath);
                    disksBox.SetItemCheckState(index, CheckState.Checked);
                    GameManager.Instance.AddGamesSearchDirectory(fbd.SelectedPath);
                }
            }

            for (int i = 0; i < disksBox.Items.Count; i++)
            {
                disksBox.SetItemCheckState(i, CheckState.Checked);
            }

            btn_customPath.BackColor = Theme_Settings.ButtonsBackColor;
        }

        private void Btn_delPath_Click(object sender, EventArgs e)
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the paths that are currently unchecked?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                List<string> pathsToRemove = new List<string>();

                for (int i = 0; i < GameManager.Instance.GamesSearcheDirectories.Count; i++)
                {
                    string dirPath = GameManager.Instance.GamesSearcheDirectories[i];

                    if (!disksBox.CheckedItems.Contains(dirPath))
                    {
                        disksBox.Items.Remove(dirPath);
                        pathsToRemove.Add(dirPath);
                    }
                }

                foreach (string path in pathsToRemove)
                {
                    GameManager.Instance.RemoveGamesSearchDirectory(path);
                }
            }
        }

        private void AutoSearchChkBox_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox chkbox = sender as CheckBox;

            bool iniValueExists = !string.IsNullOrEmpty(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));

            if (!iniValueExists)
            {
                if (chkbox.Checked)
                {
                    Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=True" });
                }
                else
                {
                    Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=False" });
                }
            }
            else
            {
                Globals.ini.IniWriteValue("Misc", "AutoSearchGame", chkbox.Checked.ToString());
            }
        }    

        private void controlscollect()
        {
            foreach (Control control in Controls)
            {
                ctrls.Add(control);
                foreach (Control container1 in control.Controls)
                {
                    ctrls.Add(container1);
                    foreach (Control container2 in container1.Controls)
                    {
                        ctrls.Add(container2);
                        foreach (Control container3 in container2.Controls)
                        {
                            ctrls.Add(container3);
                        }
                    }
                }
            }
        }

        private void closeButton(object sender, EventArgs e)
        {
            Instance = null;
            this.Close();
        }

        private void closeBtn_MouseEnter(object sender, EventArgs e)
        {
        }

        private void closeBtn_MouseLeave(object sender, EventArgs e)
        {
        }

        private void OnFormClosed(object sender , EventArgs e)
        {
            Instance = null;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void GamePathForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.White, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            e.Graphics.DrawRectangle(Pens.White, new Rectangle(disksBox.Left - 1,disksBox.Top - 1,disksBox.Width + 2,disksBox.Height + 2));
        }
    }
}
