using Nucleus.Coop.Tools;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows.Forms;

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

        private float progress;
        private float lastProgress;

        private List<string> pathsToSearch;
        private List<Control> ctrls = new List<Control>();
        private bool searching;
        private bool closed;
        private MainForm mainForm;
        private float fontSize;

        private Cursor hand_Cursor;
        private Cursor default_Cursor;
        private List<string> paths = new List<string>()
;
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
            //for (int x = 0; x < disksBox.Items.Count; x++)
            //{
            //    mainForm.ini.IniWriteValue("SearchPaths", (x + 1).ToString(), disksBox.Items[x].ToString());
            //}

            txt_Stage.Visible = false;
            progressBar1.Visible = false;
            txt_Path.Visible = false;
            //ini.IniWriteValue("Misc", "AutoSearchLocation", Location.X + "X" + Location.Y);
            this.Visible = false;
        }

        public SearchDisksForm(MainForm mainForm)
        {
            this.mainForm = mainForm;

            InitializeComponent();

            default_Cursor = Theme_Settings.Default_Cursor;
            hand_Cursor = Theme_Settings.Hand_Cursor;

            fontSize = float.Parse(Globals.ThemeConfigFile.IniReadValue("Font", "AutoSearchFontSize"));
            ForeColor = Theme_Settings.ControlsForeColor;

            BackgroundImage = Image.FromFile(Globals.ThemeFolder + "other_backgrounds.jpg");
            closeBtn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");

            btn_addSelection.BackColor = Theme_Settings.ButtonsBackColor;
            btn_customPath.BackColor = Theme_Settings.ButtonsBackColor;
            btnSearch.BackColor = Theme_Settings.ButtonsBackColor;
            btn_delPath.BackColor = Theme_Settings.ButtonsBackColor;
            btn_selectAll.BackColor = Theme_Settings.ButtonsBackColor;
            btn_deselectAll.BackColor = Theme_Settings.ButtonsBackColor;

            btn_addSelection.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_customPath.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btnSearch.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_delPath.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_selectAll.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            btn_deselectAll.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;

            controlscollect();

            foreach (Control control in ctrls)
            {
                control.Font = new Font(Theme_Settings.CustomFont, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);

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

            for (int x = 1; x <= 100; x++)
            {
                //if (mainForm.ini.IniReadValue("SearchPaths", x.ToString()) == "")
                //{
                //    continue;
                //}
                //else
                //{
                //    //paths.Add(mainForm.ini.IniReadValue("SearchPaths", x.ToString()));
                //    //disksBox.Items.Add(mainForm.ini.IniReadValue("SearchPaths", x.ToString()), true);
                //}
            }

            Rectangle area = Screen.PrimaryScreen.Bounds;
            if (/*ini.IniReadValue("Misc", "AutoSearchLocation") != ""*/true)
            {
               // string[] windowLocation = ini.IniReadValue("Misc", "AutoSearchLocation").Split('X');

                //if (ScreensUtil.AllScreens().All(s => !s.MonitorBounds.Contains(int.Parse(windowLocation[0]), int.Parse(windowLocation[1]))))
                //{
                //    CenterToScreen();
                //}
                //else
                //{
                //    Location = new Point(area.X + int.Parse(windowLocation[0]), area.Y + int.Parse(windowLocation[1]));
                //}
            }
            else
            {
                //CenterToScreen();
            }

            DPIManager.Register(this);
            DPIManager.Update(this);
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
                if (c.GetType() == typeof(CheckedListBox))
                {
                    c.Font = new Font(Theme_Settings.CustomFont, c.Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
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

        private bool IsElevated => new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (btnSearch.Text == "Cancel")
            {
                closed = true;
                searching = false;
                btnSearch.Text = "Search";
                txt_Path.Text = "";
                txt_Stage.Text = "Cancelling";
                checkboxFoundGames.Items.Clear();
                Cancel();
                return;
            }

            if (searching || disksBox.CheckedItems.Count == 0)
            {
                return;
            }

            txt_Stage.Visible = true;
            progressBar1.Visible = true;
            txt_Path.Visible = true;
            btnSearch.Text = "Cancel";
            btn_customPath.Enabled = false;
            btn_delPath.Enabled = false;
            disksBox.Enabled = false;
            searching = true;
            closed = false;
            txt_Path.Text = "";
            txt_Stage.Text = "";
            label1.Enabled = false;
            checkboxFoundGames.Items.Clear();

            pathsToSearch = new List<string>();

            for (int i = 0; i < disksBox.CheckedItems.Count; i++)
            {
                pathsToSearch.Add(disksBox.CheckedItems[i].ToString());
            }

            ThreadPool.QueueUserWorkItem(SearchDrive, null);
        }

        private void UpdateProgress(float toAdd)
        {
            progress += toAdd;

            float dif = progress - lastProgress;
            if (dif > 0.005f || toAdd == 0) // only update after .5% or if the user has just requested an update
            {
                lastProgress = progress;
                if (IsDisposed || closing)
                {
                    return;
                }

                Invoke(new Action(delegate
                {
                    if (IsDisposed || progressBar1.IsDisposed || closing)
                    {
                        return;
                    }
                    progressBar1.Value = Math.Min(100, (int)(progress * 100));
                }));
            }
        }

        private IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                Application.DoEvents();

                if (closed)
                {
                    break;
                }

                path = queue.Dequeue();
                Invoke(new Action(delegate
                {
                    txt_Path.Text = path;
                }));

                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        if (closed)
                        {
                            break;
                        }
                        queue.Enqueue(subDir);

                        //foreach (string sub in Directory.GetDirectories(subDir))
                        //{
                        //{ continue; }
                        //if(Directory.GetFiles(path, "*.exe").Length == 0)
                        //{ 
                        //    continue;
                        //}

                        //queue.Enqueue(sub);
                        // Console.WriteLine(sub);
                        // }
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

                string[] files = null;

                try
                {
                    if (closed)
                    {
                        break;
                    }

                    files = Directory.GetFiles(path, "*.exe");
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex);
                }

                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        if (closed)
                        {
                            break;
                        }
                        yield return files[i];
                    }
                }
            }

            if (closed)
            {
                txt_Path.Text = "";
                yield return null;
            }
        }

        private void Cancel()
        {
            searching = false;
            btnSearch.Enabled = true;
            btn_customPath.Enabled = true;
            btn_delPath.Enabled = true;
            btn_addSelection.Enabled = false;
            btn_selectAll.Enabled = false;
            btn_deselectAll.Enabled = false;
            checkboxFoundGames.Enabled = false;
            checkboxFoundGames.Items.Clear();
            disksBox.Enabled = true;
            progressBar1.Value = 0;
            progress = 0;
            lastProgress = 0;
            UpdateProgress(0);
            label2.Enabled = false;
            label1.Enabled = true;
            txt_Stage.Text = "Cancelled";
            txt_Path.Text = "";
            txt_Stage.Visible = false;
            progressBar1.Visible = false;
            txt_Path.Visible = false;
        }

        private void SearchDrive(object state)
        {
            System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                for (int i = 0; i < pathsToSearch.Count; i++)
                {
                    Invoke(new Action(delegate
                    {
                        txt_Stage.Text = i + 1 + " of " + pathsToSearch.Count;
                    }));

                    string currentPath = pathsToSearch[i];

                    float totalDiskPc = 1 / (float)pathsToSearch.Count;
                    float thirdDiskPc = totalDiskPc / 3.0f;

                    // 1/3 done, we started the operation
                    UpdateProgress(thirdDiskPc);

                    List<string> result = new List<string>();

                    result = GetFiles(currentPath).ToList();


                    float increment = thirdDiskPc / result.Count;

                    foreach (string exeFilePath in result)
                    {
                        if (closed)
                        {
                            return;
                        }

                        //UpdateProgress(increment);
                        if (GameManager.Instance.User.Games.Any(c => c.ExePath.ToLower() == exeFilePath.ToLower()))
                        {
                            //continue;
                        }

                        if (GameManager.Instance.AnyGame(Path.GetFileName(exeFilePath).ToLower()))
                        {
                            if (exeFilePath.Contains("$Recycle.Bin") ||
                                exeFilePath.Contains(@"\Instance"))
                            {
                                // noope
                                continue;
                            }

                            GenericGameInfo uinfo = GameManager.Instance.GetGame(exeFilePath);

                            if (uinfo != null)
                            {
#if RELEASE
                    if (uinfo.Game.Debug)
                    {
                        continue;
                    }
#endif
                                //LogManager.Log("> Found new game {0} on drive {1}", uinfo.Game.GameName, info.drive.Name);
                                Invoke(new Action(delegate
                                {
                                    bool exists = false;
                                    foreach (object item in checkboxFoundGames.Items)
                                    {
                                        if (item.ToString() == uinfo.GameName + " | " + exeFilePath)
                                        {
                                            exists = true;
                                        }
                                    }
                                    if (!exists)
                                    {
                                        checkboxFoundGames.Items.Add(uinfo.GameName + " | " + exeFilePath, true);
                                        checkboxFoundGames.Refresh();
                                    }
                                }));
                            }
                        }
                    }

                    if (closed)
                    {
                        return;
                    }
                }

                searching = false;
                Invoke(new Action(delegate
                {
                    btnSearch.Text = "Search";
                }));

                watch.Stop();

                long elapsedMs = watch.ElapsedMilliseconds / 1000;

                if (checkboxFoundGames.Items.Count == 0)
                {
                    Invoke(new Action(delegate
                    {
                        btn_customPath.Enabled = true;
                        btn_delPath.Enabled = true;
                        disksBox.Enabled = true;

                        MessageBox.Show("Operation completed in " + elapsedMs + "s. No new games found.");
                        progressBar1.Value = 0;
                    }));
                    return;
                }

                progress = 1;
                UpdateProgress(0);
                Invoke(new Action(delegate
                {
                    btn_addSelection.Enabled = true;
                    btn_selectAll.Enabled = true;
                    btn_deselectAll.Enabled = true;
                    checkboxFoundGames.Enabled = true;
                    btnSearch.Enabled = false;
                    label2.Enabled = true;
                    label1.Enabled = false;
                    txt_Stage.Text = "Done";
                    txt_Path.Text = "";
                    Refresh();
                    Invalidate();
                }));

                MessageBox.Show("Search has completed, operation took " + elapsedMs + "s. Select the games you wish to add from the right-hand side.", "Search finished");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                MessageBox.Show(ex.Message);
            }
        }

        private bool closing;
        private void SearchDisksForm_FormClosing(object sender, FormClosingEventArgs e)
        {

            Refresh();
            closing = true;
        }

        private void Btn_customPath_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    List<bool> wasSelected = new List<bool>();

                    if (paths.Any(p => p == fbd.SelectedPath))
                    {
                        return;
                    }

                    paths.Add(fbd.SelectedPath);
                }

                for (int i = 0; i < paths.Count; i++)
                {
                    //mainForm.ini.IniWriteValue("SearchPaths", (i + 1).ToString(), paths[i]);
                    if (disksBox.Items.Contains(paths[i]))
                    {
                        continue;
                    }

                    disksBox.Items.Add(paths[i]);
                    disksBox.SetItemCheckState(i, CheckState.Checked);
                }
            }
        }

        private void Btn_addSelection_Click(object sender, EventArgs e)
        {
            if (checkboxFoundGames.CheckedItems.Count == 0)
            {
                DialogResult dialogResult = MessageBox.Show("No games have been selected. Do you wish to add any?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    return;
                }
            }

            if (checkboxFoundGames.CheckedItems.Count > 0)
            {
                List<string> gamesToAdd = new List<string>();
                int numAdded = 0;

                for (int i = 0; i < checkboxFoundGames.CheckedItems.Count; i++)
                {
                    string checkBoxFullname = checkboxFoundGames.CheckedItems[i].ToString();
                    string exePath = checkBoxFullname.Substring(checkBoxFullname.IndexOf(" | ") + " | ".Length);
                    gamesToAdd.Add(exePath);
                }

                foreach (string gameToAdd in gamesToAdd)
                {
                    string exeName = gameToAdd.Split('\\').Last();

                    //if (GameManager.Instance.IsGameAlreadyInUserProfile(exeName))
                    {
                        MessageBox.Show($"Executable {exeName} is already in your game list and will be skipped.", "Already in your list", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        continue;
                    }

                    //UserGameInfo uinfo = GameManager.Instance.TryAddGame(gameToAdd);
                    //if (uinfo != null)
                    //{
                    //    mainForm.NewUserGame(uinfo);
                    //    numAdded++;
                    //}

                }

                MessageBox.Show(string.Format("{0}/{1} selected games added!", numAdded, checkboxFoundGames.CheckedItems.Count), "Games added");
                SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
            }

            btnSearch.Enabled = true;
            btn_customPath.Enabled = true;
            btn_delPath.Enabled = true;
            btn_addSelection.Enabled = false;
            btn_selectAll.Enabled = false;
            btn_deselectAll.Enabled = false;
            checkboxFoundGames.Enabled = false;
            checkboxFoundGames.Items.Clear();
            disksBox.Enabled = true;
            progressBar1.Value = 0;
            progress = 0;
            lastProgress = 0;
            label2.Enabled = false;
            label1.Enabled = true;
            txt_Path.Text = "";
            txt_Stage.Text = "";
        }

        private void Btn_delPath_Click(object sender, EventArgs e)
        {
            if (disksBox.CheckedItems.Count == 0)
            {
                return;
            }

            DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete the paths that are currently unchecked?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.Yes)
            {
                // foreach (string item in disksBox.Items.OfType<string>().ToList())
                List<string> pathsToRemove = new List<string>();
                for (int i = 0; i < paths.Count; i++)
                {
                    if (i < disksBox.Items.Count)
                    {
                        if (disksBox.CheckedItems.Contains(disksBox.Items[i]))
                        {
                            //mainForm.ini.IniWriteValue("SearchPaths", (i + 1).ToString(), disksBox.Items[i].ToString());
                        }
                        else
                        {
                            pathsToRemove.Add(disksBox.Items[i].ToString());
                            //mainForm.ini.IniWriteValue("SearchPaths", (i + 1).ToString(), "");
                        }
                    }
                }

                foreach (string pathToRemove in pathsToRemove)
                {
                    disksBox.Items.Remove(pathToRemove);
                    paths.Remove(pathToRemove);
                }

            }
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

        private void DisksBox_ControlAdded(object sender, ControlEventArgs e)
        {

        }

        private void DisksBox_ControlRemoved(object sender, ControlEventArgs e)
        {

        }
    }
}