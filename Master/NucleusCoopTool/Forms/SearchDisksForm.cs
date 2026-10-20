using NAudio.Gui;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Nucleus.Coop.Tools.GameAssets;
using static Nucleus.Gaming.Forms.SearchProgress;


namespace Nucleus.Coop
{
    public partial class SearchDisksForm : Form, IDynamicSized
    {
        public struct SearchDriveInfo
        {
            public DriveInfo drive;
            public string text;

            public override string ToString()
            {
                return text;
            }
        }

        public static SearchDisksForm Instance;

        private List<Control> ctrls = new List<Control>();
        private bool searching;
        private float fontSize;

        private Cursor hand_Cursor;
        private Cursor default_Cursor;
        private Label floatingLabel;

        public SearchDisksForm()
        {
            Instance = this;
            InitializeComponent();

            ShowInTaskbar = false;
            default_Cursor = Theme_Settings.Default_Cursor;
            hand_Cursor = Theme_Settings.Hand_Cursor;

            fontSize = 10f;
            ForeColor = Theme_Settings.ControlsForeColor;

            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            closeBtn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");

            btn_addSelection.BackColor = Theme_Settings.ButtonsBackColor;
            btn_customPath.BackColor = Theme_Settings.ButtonsBackColor;
            btnSearch.BackColor = Theme_Settings.ButtonsBackColor;
            btn_selectAll.BackColor = Theme_Settings.ButtonsBackColor;
            btn_deselectAll.BackColor = Theme_Settings.ButtonsBackColor;

            btn_addSelection.FlatAppearance.BorderColor = Color.White;
            btn_addSelection.FlatAppearance.BorderSize = 1;

            btn_customPath.FlatAppearance.BorderColor = Color.White;
            btn_customPath.FlatAppearance.BorderSize = 1;

            btnSearch.FlatAppearance.BorderColor = Color.White;
            btnSearch.FlatAppearance.BorderSize = 1;

            btn_selectAll.FlatAppearance.BorderColor = Color.White;
            btn_selectAll.FlatAppearance.BorderSize = 1;

            btn_deselectAll.FlatAppearance.BorderColor = Color.White;
            btn_deselectAll.FlatAppearance.BorderSize = 1;

            btn_addSelection.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_customPath.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btnSearch.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_selectAll.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_deselectAll.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;

            closeBtn.Cursor = hand_Cursor;

            floatingLabel = new Label();
            floatingLabel.Name = "floatingLabel";
            floatingLabel.AutoSize = true;
            floatingLabel.Font = new Font(this.Font.FontFamily, 10.25f);
            floatingLabel.Visible = false;
            floatingLabel.ForeColor = Color.Yellow;
            floatingLabel.BackColor = Color.Black;
            floatingLabel.BorderStyle = BorderStyle.FixedSingle;
            floatingLabel.Size = new Size(1, 1);
            floatingLabel.MaximumSize = new Size(Width, Height);
            Controls.Add(floatingLabel);

            checkboxFoundGames.MouseMove += CheckboxFoundGames_MouseMove;
            MouseMove += On_MouseMove;

            UpdatePathList(null, null);

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

            CenterToScreen();
            DPIManager.Register(this);
            DPIManager.Update(this);

            bool iniValueExists = !string.IsNullOrEmpty(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));

            if (iniValueExists)
            {
                bool.Parse(Globals.ini.IniReadValue("Misc", "AutoSearchGame"));
            }
            else
            {
                Globals.ini.IniAddKeyValuesToSection("Misc", new string[] { "AutoSearchGame=False" });               
            }

            if (GameManager.Instance.GamesSearcheDirectories.Count == 0)
            {
                Shown += Btn_customPath_Click;
            }
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
                heightField.SetValue(checkboxFoundGames, addedHeight);
            }
            else
            {
                var heightField = typeof(CheckedListBox).GetField(
               "scaledListItemBordersHeight",
               BindingFlags.NonPublic | BindingFlags.Instance
               );

                var addedHeight = 6;

                heightField.SetValue(disksBox, addedHeight);
                heightField.SetValue(checkboxFoundGames, addedHeight);

            }

            float textBoxFontSize = (Font.Size + 4) * scale;

            foreach (Control c in ctrls)
            {
                if (c is CheckedListBox)
                {
                    c.Font = new Font(Theme_Settings.CustomFont, c.Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    c.Size = new Size(c.Width, (int)(c.Height / scale));
                }             

                if (c is TextBox)
                {
                    c.Font = new Font(Theme_Settings.CustomFont, textBoxFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
            }

            ResumeLayout();

            disksBox.Focus();
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x020 || m.Msg == 0x0a0)//Do not reset our custom cursor when mouse hover over the Form background(needed because of the custom resizing/moving messges handling) 
            {
                Cursor.Current = default_Cursor;
            }

            base.WndProc(ref m);
        }

        private SortedDictionary<string, SearchResult> searchResults;

        private async void btnSearch_Click(object sender, EventArgs e)
        {
            searching = true;

            if (disksBox.CheckedItems.Count == 0)
            {
                Btn_customPath_Click(null,null);
                searching = false;
                return;
            }

            foreach (Control cont in ctrls)
            {
                cont.Enabled = false;
            }

            checkboxFoundGames.Items.Clear();

            btn_customPath.Enabled = false;
            btn_addSelection.Enabled = false;
            btn_selectAll.Enabled = false;
            btn_deselectAll.Enabled = false;
            checkboxFoundGames.Enabled = false;
            btnSearch.Enabled = false;

            List<string> searchPaths = new List<string>();

            foreach (var item in disksBox.CheckedItems)
            {
                searchPaths.Add(item.ToString());
            }

            SearchProgressForm searchF = new SearchProgressForm();
            var results = await searchF.StartSearchAsync(GameManager.Instance.HandlerInfos, searchPaths);
            searchResults = new SortedDictionary<string, SearchResult>();

            foreach (var result in results)
            {
                if (result.HandlerInfo.GenericGameInfo == null)
                {
                    continue;
                }

                (List<HandlerInfo>, string) possibility = GameManager.Instance.GetGames(result.ExecutablePath);

                if (possibility.Item1 == null)
                {
                    continue;
                }

                foreach (HandlerInfo handlerInfo in possibility.Item1)
                {
                    if (searchResults.Any(sr => sr.Key == result.HandlerInfo.FileName + " (" + result.HandlerInfo.LastWriteTimeUtc + ")"))
                    {
                        continue;
                    }

                    searchResults.Add(result.HandlerInfo.FileName + " (" + result.HandlerInfo.LastWriteTimeUtc + ")", result);
                }
            }

            foreach (KeyValuePair<string, SearchResult> result in searchResults)
            {
                if (!checkboxFoundGames.Items.Contains(result.Value.HandlerInfo.FileName + " (" + result.Value.HandlerInfo.LastWriteTimeUtc + ")"))
                {
                    checkboxFoundGames.Items.Add(result.Value.HandlerInfo.FileName + " (" + result.Value.HandlerInfo.LastWriteTimeUtc + ")");
                }
            }

            btn_customPath.Enabled = true;
            btn_addSelection.Enabled = true;
            btn_selectAll.Enabled = true;
            btn_deselectAll.Enabled = true;
            checkboxFoundGames.Enabled = true;
            btnSearch.Enabled = true;

            searching = false;

            foreach (Control cont in ctrls)
            {
                cont.Enabled = true;
            }
        }

        private void SearchDisksForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private async void Btn_customPath_Click(object sender, EventArgs e)
        {
            if (GamePathForm.Instance != null)
            {
                GamePathForm.Instance.Close();
                await Task.Delay(100);
            }

            GamePathForm gamePathForm = new GamePathForm();

            gamePathForm.FormClosing += UpdatePathList;
            gamePathForm.LocationChanged += GamePathFormMoveParent;
            gamePathForm.FormClosed += GamePathFormBringParentToFront;
            gamePathForm.Location = this.Location;
            gamePathForm.Size = this.Size;
            AddOwnedForm(gamePathForm);
            gamePathForm.Show/*Dialog*/();

            UpdatePathList(null, null);
        }


        private void UpdatePathList(object sender,EventArgs e)
        {
            disksBox.Items.Clear();

            foreach(var path in GameManager.Instance.GamesSearcheDirectories)
            {
                disksBox.Items.Add(path.ToString(), true);
            }
        }

        private void GamePathFormMoveParent(object sender, EventArgs e)
        {
            Form searchForm = sender as Form;
            Location = searchForm.Location;
        }

        private async void GamePathFormBringParentToFront(object sender, EventArgs e)
        {
            await Task.Delay(100);
            this.BringToFront();
        }

        private void Btn_addSelection_Click(object sender, EventArgs e)
        {
            if (checkboxFoundGames.CheckedItems.Count == 0)
            {

                Globals.MainOSD.Show(1000, $"*@{OSDHighlightColors.Info}|No game selected@*");
                return;
            }

            if (checkboxFoundGames.CheckedItems.Count > 0)
            {
                var itemsToRemove = new List<string>();
                List<GenericGameInfo> genericGameInfos = new List<GenericGameInfo>();

                for (int i = 0; i < checkboxFoundGames.CheckedItems.Count; i++)
                {
                    var item = checkboxFoundGames.CheckedItems[i].ToString();
                    var userGameInfo = GameManager.Instance.TryAddGame(searchResults[item].ExecutablePath, searchResults[item].HandlerInfo);

                    if (userGameInfo != null)
                    {
                        UIThread.Post(() =>
                        {
                            Core_Interface.AddGameToList(userGameInfo);
                            genericGameInfos.Add(userGameInfo.HandlerInfo.GenericGameInfo);
                        });

                        itemsToRemove.Add(item);
                    }
                }

                foreach (var item in itemsToRemove)
                {
                    checkboxFoundGames.Items.Remove(item);
                }

                DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Do you want to download games cover and screenshots?", "Download game assets?", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (dialogResult == DialogResult.Yes)
                {
                    _ = new AssetsDownloader().DownloadGamesAssets(genericGameInfos);
                }
            }

            UIThread.Post(() =>
            {
                btn_customPath.Enabled = true;
                btn_addSelection.Enabled = true;
                btn_selectAll.Enabled = true;
                btn_deselectAll.Enabled = true;
                checkboxFoundGames.Enabled = true;
                btnSearch.Enabled = true;
            });
        }

        private Rectangle itemBounds;

        private void CheckboxFoundGames_MouseMove(object sender, MouseEventArgs e)
        {
            CheckedListBox foundGamesBox = sender as CheckedListBox;
            bool draw = false;
            string textToDraw = string.Empty;

            for (int i = 0; i < foundGamesBox.Items.Count; i++)
            {
                itemBounds = foundGamesBox.GetItemRectangle(i);
                if (itemBounds.Contains(e.Location))
                {
                    draw = true;
                    textToDraw = searchResults[foundGamesBox.Items[i].ToString()]?.ExecutablePath;

                    break;
                }
            }

            if (draw)
            {
                floatingLabel.Text = textToDraw;
                floatingLabel.Location = new Point(foundGamesBox.Left, itemBounds.Bottom + itemBounds.Height * 2);

                if (floatingLabel.Right > this.Width)
                {
                    int diff = floatingLabel.Right - this.Width;

                    floatingLabel.Location = new Point(floatingLabel.Left - diff, itemBounds.Bottom + itemBounds.Height * 2);
                }

                floatingLabel.BringToFront();
                floatingLabel.Visible = true;
            }
            else
            {
                floatingLabel.Visible = false;
            }
        }

        private void On_MouseMove(object sender, MouseEventArgs e)
        {
            floatingLabel.Visible = false;
        }

        private void Btn_selectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkboxFoundGames.Items.Count; i++)
            {
                checkboxFoundGames.SetItemChecked(i, true);
            }
        }

        private void Btn_deselectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkboxFoundGames.Items.Count; i++)
            {
                checkboxFoundGames.SetItemChecked(i, false);
            }
        }

        private void closeBtn_MouseEnter(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        }

        private void closeBtn_MouseLeave(object sender, EventArgs e)
        {
            closeBtn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
        }

        private void closeButton(object sender, EventArgs e)
        {
            if (searching)
            {
                return;
            }
         
            this.Close();
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
                            foreach (Control container4 in container3.Controls)
                            {
                                ctrls.Add(container4);
                            }
                        }
                    }
                }
            }
        }

        private void SearchDisksForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(Pens.White, new Rectangle(0, 0, this.Width - 1, this.Height - 1));
            e.Graphics.DrawRectangles(Pens.White, new RectangleF[] {
            new RectangleF(disksBox.Left - 1,disksBox.Top - 1,disksBox.Width + 2,disksBox.Height + 2),
            new RectangleF(checkboxFoundGames.Left - 1,checkboxFoundGames.Top - 1,checkboxFoundGames.Width + 2,checkboxFoundGames.Height + 2)
            });
        }
    }
    
}