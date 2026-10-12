using Nucleus.Gaming.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop.Generic
{
    /// <summary>A plain topmost list of running processes, shown over the game windows. Use <see cref="UserPrompts.PickProcess"/>.</summary>
    public partial class ProcessPicker : Form
    {
        private readonly Func<IList<string>> choices;

        public ProcessPicker(Func<IList<string>> choices)
        {
            this.choices = choices ?? (() => new List<string>());

            InitializeComponent();
            MaximizeBox = false;
            MinimizeBox = false;

            pplistBox.DoubleClick += (s, e) => PickSelected();
            selBtn.Click += (s, e) => PickSelected();
            refrshBtn.Click += (s, e) => Fill();
            Fill();

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
        }

        /// <summary>The choice picked, or null.</summary>
        public string SelectedChoice { get; private set; }

        private void Fill()
        {
            pplistBox.Items.Clear();

            foreach (string choice in choices())
            {
                pplistBox.Items.Add(choice);
            }
        }

        private void PickSelected()
        {
            if (pplistBox.SelectedItem == null)
            {
                MessageBox.Show(this, "No process has been selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            SelectedChoice = pplistBox.SelectedItem.ToString();
            Close();
        }
    }

    public static class ProcessPickerRuntime
    {
        public static Process LaunchProcessPick(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Launching process picker");

            string choice = UserPrompts.PickProcess(() => ListProcesses(handlerInstance));

            if (choice != null)
            {
                Process proc = Process.GetProcessById(int.Parse(choice.Split(' ')[0]));
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

        /// <summary>"pid - name" of every process with a window, the game's (or launcher's) first; processes already used are marked.</summary>
        private static IList<string> ListProcesses(GenericGameHandler handlerInstance)
        {
            List<string> items = new List<string>();

            string exe = Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.ExecutableName).ToLower();
            string launcher = handlerInstance.CurrentGameInfo.LauncherExe?.Length > 0 ? Path.GetFileNameWithoutExtension(handlerInstance.CurrentGameInfo.LauncherExe).ToLower() : null;

            foreach (Process p in Process.GetProcesses())
            {
                if (p.Id == 0 || string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    continue;
                }

                string item = handlerInstance.attachedIds.Contains(p.Id)
                    ? p.Id + " - (DO NOT USE - Already assigned in Nucleus) " + p.ProcessName
                    : p.Id + " - " + p.ProcessName;

                string name = p.ProcessName.ToLower();

                //The game's processes go first, newest first (as the picker always listed them).
                if (name.Contains(exe) || (launcher != null && name.Contains(launcher)))
                {
                    items.Insert(0, item);
                }
                else
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}
