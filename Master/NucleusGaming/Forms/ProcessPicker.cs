using Nucleus.Gaming.Tools.GlobalWindowMethods;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media;

namespace Nucleus.Gaming.Coop.Generic
{
    public partial class ProcessPicker : Form, IDynamicSized
    {
        public ProcessPicker()
        {
            InitializeComponent();
            MaximizeBox = false;
            MinimizeBox = false;

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);

            DPIManager.Register(this);
            DPIManager.AddForm(this);
            DPIManager.Update(this);         
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            float ratio = ((float)Height / (float)Width);
            Height = (int)((float)Height / (float)ratio);
            pplistBox.Font = new Font("Franklin Gothic", pplistBox.Font.Size * scale, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 20, 20);
        }
    }

    public static class ProcessPickerRuntime
    {
        public static Process LaunchProcessPick(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            ProcessPicker ppform = new ProcessPicker();
            handlerInstance.Log("Launching process picker");

            ppform.pplistBox.DoubleClick += new EventHandler(SelBtn_Click);

            foreach (Control c in ppform.Controls)
            {
                if (c.Name == "refrshBtn")
                {
                    c.Click += new EventHandler(RefrshBtn_Click);
                }
            }

            foreach (Control c in ppform.Controls)
            {
                if (c.Name == "selBtn")
                {
                    c.Click += new EventHandler(SelBtn_Click);
                }
            }

            Process[] allProc = Process.GetProcesses();

            foreach (Process p in allProc)
            {
                if (p.Id == 0 || string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    continue;
                }
                if (handlerInstance.attachedIds.Contains(p.Id))
                {
                    if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName).ToLower()) || (handlerInstance.CurrentGameInfo.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe).ToLower())))
                    {
                        ppform.pplistBox.Items.Insert(0, p.Id + " - (DO NOT USE - Already assigned in Nucleus) " + p.ProcessName);
                    }
                    else
                    {
                        ppform.pplistBox.Items.Add(p.Id + " - (DO NOT USE - Already assigned in Nucleus) " + p.ProcessName);
                    }
                }
                else
                {
                    if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName).ToLower()) || (handlerInstance.CurrentGameInfo.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe).ToLower())))
                    {
                        ppform.pplistBox.Items.Insert(0, p.Id + " - " + p.ProcessName);
                    }
                    else
                    {
                        ppform.pplistBox.Items.Add(p.Id + " - " + p.ProcessName);
                    }
                }
            }

            ppform.ShowDialog();
            WindowScrape.Static.HwndInterface.MakeTopMost(ppform.Handle);

            if (ppform.pplistBox.SelectedItem != null)
            {
                Process proc = Process.GetProcessById(int.Parse(ppform.pplistBox.SelectedItem.ToString().Split(' ')[0]));
                handlerInstance.Log(string.Format("Obtained process {0} (pid {1}) via process picker", proc.ProcessName, proc.Id));
                handlerInstance.attached.Add(proc);
                handlerInstance.attachedIds.Add(proc.Id);
                player.ProcessID = proc.Id;

                if (player.IsKeyboardPlayer && !player.IsRawKeyboard)
                {
                    handlerInstance.keyboardProcId = proc.Id;
                }

                return proc;
            }

            return null;
        }

        private static void SelBtn_Click(object sender, EventArgs e)
        {
            Control control = (Control)sender;
            Form ppform = control.FindForm();

            if (control.GetType() == typeof(ListBox))
            {
                ListBox listBox = control as ListBox;
                if (listBox.SelectedItem == null)
                {
                    MessageBox.Show("No process has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else if (control.GetType() == typeof(Button))
            {
                foreach (Control c in ppform.Controls)
                {
                    if (c.GetType() == typeof(ListBox))
                    {
                        ListBox listBox = c as ListBox;
                        if (listBox.SelectedItem == null)
                        {
                            MessageBox.Show("No process has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }
                    }
                }
            }

            ppform.Close();
        }

        private static void RefrshBtn_Click(object sender, EventArgs e)
        {
            var handlerInstance = GenericGameHandler.Instance;

            Control control = (Button)sender;

            Form ppform = control.FindForm();
            foreach (Control l in ppform.Controls)
            {
                if (l.GetType() == typeof(ListBox))
                {
                    ListBox listBox = l as ListBox;
                    listBox.Items.Clear();

                    Process[] allProc = Process.GetProcesses();
                    foreach (Process p in allProc)
                    {
                        if (p.Id == 0 || string.IsNullOrEmpty(p.MainWindowTitle))
                        {
                            continue;
                        }
                        if (handlerInstance.attachedIds.Contains(p.Id))
                        {
                            if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName).ToLower()) || (handlerInstance.CurrentGameInfo.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe).ToLower())))
                            {
                                listBox.Items.Insert(0, p.Id + " - (DO NOT USE - Already assigned in Nucleus)" + p.ProcessName);
                            }
                            else
                            {
                                listBox.Items.Add(p.Id + " - (DO NOT USE - Already assigned in Nucleus)" + p.ProcessName);
                            }
                        }
                        else
                        {
                            if (p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName).ToLower()) || (handlerInstance.CurrentGameInfo.LauncherExe?.Length > 0 && p.ProcessName.ToLower().Contains(Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe).ToLower())))
                            {
                                listBox.Items.Insert(0, p.Id + " - " + p.ProcessName);
                            }
                            else
                            {
                                listBox.Items.Add(p.Id + " - " + p.ProcessName);
                            }
                        }
                    }
                }
            }
        }
    }
}
