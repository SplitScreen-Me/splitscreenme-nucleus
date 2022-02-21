using NAudio.CoreAudioApi;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Windows.Interop;
using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Media;

namespace Nucleus.Coop
{
    public partial class Settings : BaseForm, IDynamicSized
    {
        private readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        private MainForm mainForm = null;
        private PositionsControl positionsControl;

        public int KillProcess_HotkeyID = 1;
        public int TopMost_HotkeyID = 2;
        public int StopSession_HotkeyID = 3;

        private TextBox[] controllerGuids;
        private TextBox[] controllerNicks;

        //private List<string> audioDevices;
        private IDictionary<string, string> audioDevices;
        private string epicLang;
        private string epicLangText;
        private DirectInput dinput;
       
        public void button_Click(object sender, EventArgs e)
        {
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            _ = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));

            SoundPlayer splayer = new SoundPlayer((Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\button_click.wav")));
            splayer.Play();
        }

        public Settings(MainForm mf, PositionsControl pc)
        {
            
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));
            bool MouseClick = Convert.ToBoolean(theme.IniReadValue("Sounds", "MouseClick"));
            string[] rgb_MouseOverColor = theme.IniReadValue("Colors", "MouseOverColor").Split(',');
            string[] rgb_font = theme.IniReadValue("Colors", "FontColor").Split(',');
            Color MouseOverBackColor = Color.FromArgb(Convert.ToInt32(rgb_MouseOverColor[0]), Convert.ToInt32(rgb_MouseOverColor[1]), Convert.ToInt32(rgb_MouseOverColor[2]));

            Image AppButtons = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\button.png"));

            InitializeComponent();

            //form Fore Color
            ForeColor = Color.FromArgb(Convert.ToInt32(rgb_font[0]), Convert.ToInt32(rgb_font[1]), Convert.ToInt32(rgb_font[2]));
            //
            BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            tabPage1.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            tabPage3.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            tabPage4.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            tabPage5.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\other_backgrounds.jpg"));
            setting_Label.BackgroundImage = AppButtons;
            btn_credits.BackgroundImage = AppButtons;
            settingsCloseBtn.BackgroundImage = AppButtons;
            settingsSaveBtn.BackgroundImage = AppButtons;
            //
            //MouseOverColor
            //
            btn_credits.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
            settingsCloseBtn.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
            settingsSaveBtn.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
            btn_Refresh.BackColor = MouseOverBackColor;
            audioRefresh.BackColor = MouseOverBackColor;

            if (MouseClick)
            {
                foreach (Control button in this.Controls) { if (button is Button) { button.Click += new System.EventHandler(this.button_Click); } }
            }

            controllerGuids = new TextBox[] { controllerOneGuid, controllerTwoGuid, controllerThreeGuid, controllerFourGuid, controllerFiveGuid, controllerSixGuid, controllerSevenGuid, controllerEightGuid, controllerNineGuid, controllerTenGuid, controllerElevenGuid, controllerTwelveGuid, controllerThirteenGuid, controllerFourteenGuid, controllerFifteenGuid, controllerSixteenGuid };
            controllerNicks = new TextBox[] { controllerOneNick, controllerTwoNick, controllerThreeNick, controllerFourNick, controllerFiveNick, controllerSixNick, controllerSevenNick, controllerEightNick, controllerNineNick, controllerTenNick, controllerElevenNick, controllerTwelveNick, controllerThirteenNick, controllerFourteenNick, controllerFifteenNick, controllerSixteenNick };

            mainForm = mf;
            positionsControl = pc;

            //epiclangs 
            IDictionary<string, string> epiclangs = new Dictionary<string, string>();
     
            epiclangs.Add("Arabic", "ar");
            epiclangs.Add("Brazilian", "pt-BR");
            epiclangs.Add("Bulgarian", "bg");
            epiclangs.Add("Chinese", "zh");
            epiclangs.Add("Czech", "cs");
            epiclangs.Add("Danish", "da");
            epiclangs.Add("Dutch", "nl");
            epiclangs.Add("English", "en");
            epiclangs.Add("Finnish", "fi");
            epiclangs.Add("French", "fr");
            epiclangs.Add("German", "de");
            epiclangs.Add("Greek", "el");
            epiclangs.Add("Hungarian", "hu");
            epiclangs.Add("Italian", "it");
            epiclangs.Add("Japanese", "ja");
            epiclangs.Add("Koreana", "ko");
            epiclangs.Add("Norwegian", "no");
            epiclangs.Add("Polish", "pl");
            epiclangs.Add("Portuguese", "pt");
            epiclangs.Add("Romanian", "ro");
            epiclangs.Add("Russian", "ru");
            epiclangs.Add("Spanish", "es");
            epiclangs.Add("Swedish", "sv");
            epiclangs.Add("Thai", "th");
            epiclangs.Add("Turkish", "tr");
            epiclangs.Add("Ukrainian", "uk");

            foreach(KeyValuePair<string, string> lang in epiclangs)
            {
                if (lang.Key == ini.IniReadValue("Misc", "EpicLang"))
                {
                    epicLangText = lang.Key;
                    epicLang = lang.Value;
                }
               
            }

            //Console.WriteLine(epicLang);

            RefreshCmbNetwork();
            if (ini.IniReadValue("Misc", "Network") != "")
            {
                cmb_Network.Text = ini.IniReadValue("Misc", "Network");
            }
            else
            {
                cmb_Network.SelectedIndex = 0;
            }

            if (ini.IniReadValue("Misc", "SteamLang") != "")
            {
                cmb_Lang.Text = ini.IniReadValue("Misc", "SteamLang");
            }
            else
            {
                cmb_Lang.SelectedIndex = 0;
            }

            if (ini.IniReadValue("Misc", "EpicLang") != "")
            {
                cmb_EpicLang.Text = epicLangText;
            }
            else
            {
                cmb_EpicLang.SelectedIndex = 0;
            }

            if (ini.IniReadValue("Dev", "OfflineMod") == "On")
            {
                offlineMod.SelectedIndex = 1;
            }
            else 
            {
                offlineMod.SelectedIndex = 0;
            }
                
               

            if (ini.IniReadValue("Hotkeys", "Close").Contains('+'))
            {
                string[] closeHk = ini.IniReadValue("Hotkeys", "Close").Split('+');
                if ((closeHk[0] == "Ctrl" || closeHk[0] == "Alt" || closeHk[0] == "Shift") && closeHk[1].Length == 1 && Regex.IsMatch(closeHk[1], @"^[a-zA-Z0-9]+$"))
                {
                    settingsCloseCmb.SelectedItem = closeHk[0];
                    settingsCloseHKTxt.Text = closeHk[1];
                }
            }
            else
            {
                ini.IniWriteValue("Hotkeys", "Close", "");
            }

            if (ini.IniReadValue("Hotkeys", "Stop").Contains('+'))
            {
                string[] stopHk = ini.IniReadValue("Hotkeys", "Stop").Split('+');
                if ((stopHk[0] == "Ctrl" || stopHk[0] == "Alt" || stopHk[0] == "Shift") && stopHk[1].Length == 1 && Regex.IsMatch(stopHk[1], @"^[a-zA-Z0-9]+$"))
                {
                    settingsStopCmb.SelectedItem = stopHk[0];
                    settingsStopTxt.Text = stopHk[1];
                }
            }
            else
            {
                ini.IniWriteValue("Hotkeys", "Stop", "");
            }

            if (ini.IniReadValue("Hotkeys", "TopMost").Contains('+'))
            {
                string[] topHk = ini.IniReadValue("Hotkeys", "TopMost").Split('+');
                if ((topHk[0] == "Ctrl" || topHk[0] == "Alt" || topHk[0] == "Shift") && topHk[1].Length == 1 && Regex.IsMatch(topHk[1], @"^[a-zA-Z0-9]+$"))
                {
                    settingsTopCmb.SelectedItem = topHk[0];
                    settingsTopTxt.Text = topHk[1];
                }
            }

            //Controll
            GetControllers();

            if (ini.IniReadValue("ControllerMapping", "Keyboard") != "")
            {
                keyboardNick.Text = ini.IniReadValue("ControllerMapping", "Keyboard");
            }

            //Custom Layout
            //if (ini.IniReadValue("CustomLayout", "Enabled") != "")
            //{
            //    mutexLogCheck.Checked = Boolean.Parse(ini.IniReadValue("CustomLayout", "Enabled"));
            //}
            if (ini.IniReadValue("CustomLayout", "HorizontalLines") != "")
            {
                numHorDiv.Value = int.Parse(ini.IniReadValue("CustomLayout", "HorizontalLines"));
            }
            if (ini.IniReadValue("CustomLayout", "VerticalLines") != "")
            {
                numVerDiv.Value = int.Parse(ini.IniReadValue("CustomLayout", "VerticalLines"));
            }
            if (ini.IniReadValue("CustomLayout", "MaxPlayers") != "")
            {
                numMaxPlyrs.Value = int.Parse(ini.IniReadValue("CustomLayout", "MaxPlayers"));
            }

            //Misc
            if (ini.IniReadValue("Misc", "UseNicksInGame") != "")
            {
                useNicksCheck.Checked = bool.Parse(ini.IniReadValue("Misc", "UseNicksInGame"));
            }

            if (ini.IniReadValue("Misc", "IgnoreInputLockReminder") != "")
            {
                ignoreInputLockReminderCheckbox.Checked = bool.Parse(ini.IniReadValue("Misc", "IgnoreInputLockReminder"));
            }

            if (ini.IniReadValue("Misc", "DebugLog") != "")
            {
                debugLogCheck.Checked = bool.Parse(ini.IniReadValue("Misc", "DebugLog"));
            }

            if (ini.IniReadValue("Misc", "ShowStatus") != "")
            {
                statusCheck.Checked = bool.Parse(ini.IniReadValue("Misc", "ShowStatus"));
            }

            if (ini.IniReadValue("Misc", "KeepAccounts") != "")
            {
                keepAccountsCheck.Checked = bool.Parse(ini.IniReadValue("Misc", "KeepAccounts"));
            }

            if (ini.IniReadValue("Misc", "NucleusAccountPassword") != "")
            {
                nucUserPassTxt.Text = ini.IniReadValue("Misc", "NucleusAccountPassword");
            }

            if (ini.IniReadValue("Audio", "Custom") == "0")
            {
                audioDefaultSettingsRadio.Checked = true;
                audioCustomSettingsBox.Enabled = false;
            }
            else
            {
                audioCustomSettingsRadio.Checked = true;
            }
            Location = new Point(mf.Location.X + mf.Width / 2 - Width / 2, mf.Location.Y + mf.Height / 2 - Height / 2);
            RefreshAudioList();

            //if (ini.IniReadValue("Misc", "VibrateOpen") != "")
            //{
            //    check_Vibrate.Checked = Boolean.Parse(ini.IniReadValue("Misc", "VibrateOpen"));
            //}
            //CenterToScreen();
            DPIManager.Register(this);
            DPIManager.AddForm(this);
            DPIManager.Update(this);

        }
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleparams = base.CreateParams;
                handleparams.ExStyle = 0x02000000;
                return handleparams;
            }
        }
        public Settings()
        {
            DPIManager.Unregister(this);
        }

        public void UpdateSize(float scale)
        {
           if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            if (scale > 1.0F)
            {

                float newTabsSize = (Font.Size - 2.0f) * scale;
                float newFontSize = Font.Size * scale;

                tabControl2.Font = new Font("Franklin Gothic Medium", newTabsSize, FontStyle.Regular, GraphicsUnit.Point, 0);


                foreach (Control c in tabPage3.Controls)
                {
                    if (c.GetType() == typeof(NumericUpDown) ^ c.GetType() == typeof(ComboBox) ^ c.GetType() == typeof(TextBox) ^ c.GetType() == typeof(GroupBox))
                    {
                        c.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);

                    }
                }

                foreach (Control c in tabPage1.Controls)
                {
                    if (c.GetType() == typeof(NumericUpDown) ^ c.GetType() == typeof(ComboBox) ^ c.GetType() == typeof(TextBox) ^ c.GetType() == typeof(GroupBox))
                    {
                        c.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);

                    }
                    else if (c.GetType() == typeof(Panel))
                    {
                        c.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                    }

                    else if (c.GetType() == typeof(Label) ^ c.GetType() == typeof(RadioButton) ^ c.GetType() == typeof(Button))
                    {
                        c.Font = new Font("Franklin Gothic Medium", Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    }
                }
                foreach (Control c in audioCustomSettingsBox.Controls)
                {
                    if (c.GetType() == typeof(Label))
                    {
                        c.Font = new Font("Franklin Gothic Medium", Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    }
                }

                foreach (Control c in groupBox1.Controls)
                {
                    if (c.GetType() == typeof(Label))
                    {
                        c.Font = new Font("Franklin Gothic Medium", Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    }
                }


                foreach (Control c in tabPage4.Controls)
                {
                    if (c.GetType() == typeof(NumericUpDown) ^ c.GetType() == typeof(ComboBox) ^ c.GetType() == typeof(TextBox) ^ c.GetType() == typeof(GroupBox) ^ c.GetType() == typeof(Panel))
                    {
                        c.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);

                    }

                }
                foreach (Control c in tabPage5.Controls)
                {
                    if (c.GetType() == typeof(NumericUpDown) ^ c.GetType() == typeof(ComboBox) ^ c.GetType() == typeof(TextBox) ^ c.GetType() == typeof(GroupBox))
                    {
                        c.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);

                    }

                    else if (c.GetType() == typeof(Button))
                    {
                        c.Font = new Font("Franklin Gothic Medium", Font.Size, FontStyle.Regular, GraphicsUnit.Point, 0);
                    }

                }
            }

        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            DPIManager.Register(this);
        }

        private void GetControllers()
        {
            foreach (TextBox tbox in controllerGuids)
            {
                tbox.Clear();
            }

            foreach (TextBox tbox in controllerNicks)
            {
                tbox.Clear();
            }


            dinput = new DirectInput();
            IList<DeviceInstance> devices = dinput.GetDevices(DeviceClass.GameControl, DeviceEnumerationFlags.AttachedOnly);
            int gcDevicesCnt = devices.Count;
            for (int i = 0; i < devices.Count; i++)
            {
                DeviceInstance device = devices[i];
                Joystick gamePad = new Joystick(dinput, device.InstanceGuid);
                string hid = gamePad.Properties.InterfacePath;
                int start = hid.IndexOf("hid#");
                int end = hid.LastIndexOf("#{");
                string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();
                controllerGuids[i].Text = fhid;
                if (ini.IniReadValue("ControllerMapping", fhid) != "")
                {
                    controllerNicks[i].Text = ini.IniReadValue("ControllerMapping", fhid);
                }
                gamePad.Dispose();
            }
            dinput.Dispose();


            foreach ((Gaming.Coop.InputManagement.Structs.RID_DEVICE_INFO deviceInfo, IntPtr deviceHandle) device in RawInputManager.GetDeviceList().Where(x => x.deviceInfo.dwType <= 1))
            {

                //string hid = device.deviceInfo.hid.ToString();
                //int start = hid.IndexOf("hid#");
                //int end = hid.LastIndexOf("#{");
                //string fhid = hid.Substring(start, end - start).Replace('#', '\\').ToUpper();

                //controllerGuids[gcDevicesCnt].Text = fhid;
                string did = string.Empty;
                if (device.deviceInfo.dwType == 0)
                {
                    did = "T" + device.deviceInfo.dwType + "PID" + device.deviceInfo.hid.dwProductId + "VID" + device.deviceInfo.hid.dwVendorId + "VN" + device.deviceInfo.hid.dwVersionNumber;
                    controllerGuids[gcDevicesCnt].Text = did;
                    gcDevicesCnt++;
                }


                if (ini.IniReadValue("ControllerMapping", did) != "")
                {
                    controllerNicks[gcDevicesCnt].Text = ini.IniReadValue("ControllerMapping", did);
                }
            }
        }

        private void SettingsSaveBtn_Click(object sender, EventArgs e)
        {
            try
            {
                ini.IniWriteValue("Hotkeys", "Close", settingsCloseCmb.SelectedItem.ToString() + "+" + settingsCloseHKTxt.Text);
                ini.IniWriteValue("Hotkeys", "Stop", settingsStopCmb.SelectedItem.ToString() + "+" + settingsStopTxt.Text);
                ini.IniWriteValue("Hotkeys", "TopMost", settingsTopCmb.SelectedItem.ToString() + "+" + settingsTopTxt.Text);

                User32Interop.UnregisterHotKey(mainForm.Handle, KillProcess_HotkeyID);
                User32Interop.UnregisterHotKey(mainForm.Handle, TopMost_HotkeyID);
                User32Interop.UnregisterHotKey(mainForm.Handle, StopSession_HotkeyID);

                //RegHotkeys(KillProcess_HotkeyID, GetMod(settingsCloseCmb.SelectedItem.ToString()), (int)Enum.Parse(typeof(Keys), settingsCloseHKTxt.Text));
                //RegHotkeys(TopMost_HotkeyID, GetMod(settingsTopCmb.SelectedItem.ToString()), (int)Enum.Parse(typeof(Keys), settingsTopTxt.Text));
                //RegHotkeys(StopSession_HotkeyID, GetMod(settingsStopCmb.SelectedItem.ToString()), (int)Enum.Parse(typeof(Keys), settingsStopTxt.Text));

                User32Interop.RegisterHotKey(mainForm.Handle, KillProcess_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "Close").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "Close").Split('+')[1].ToString()));
                User32Interop.RegisterHotKey(mainForm.Handle, TopMost_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "TopMost").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "TopMost").Split('+')[1].ToString()));
                User32Interop.RegisterHotKey(mainForm.Handle, StopSession_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "Stop").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "Stop").Split('+')[1].ToString()));

                for (int i = 0; i < controllerGuids.Length; i++)
                {
                    if (!string.IsNullOrEmpty(controllerGuids[i].Text)) //&& !string.IsNullOrEmpty(controllerNicks[i].Text))
                    {
                        ini.IniWriteValue("ControllerMapping", controllerGuids[i].Text, controllerNicks[i].Text);
                    }
                }
                if (positionsControl != null)
                {
                    positionsControl.Refresh();
                }

                if (!string.IsNullOrEmpty(keyboardNick.Text))
                {
                    ini.IniWriteValue("ControllerMapping", "Keyboard", keyboardNick.Text);
                }

                ini.IniWriteValue("Misc", "UseNicksInGame", useNicksCheck.Checked.ToString());
                ini.IniWriteValue("Misc", "IgnoreInputLockReminder", ignoreInputLockReminderCheckbox.Checked.ToString());
                ini.IniWriteValue("Misc", "DebugLog", debugLogCheck.Checked.ToString());
                ini.IniWriteValue("Misc", "Network", cmb_Network.SelectedItem.ToString());
                ini.IniWriteValue("Misc", "SteamLang", cmb_Lang.SelectedItem.ToString());
                ini.IniWriteValue("Misc", "EpicLang", cmb_EpicLang.SelectedItem.ToString());
                ini.IniWriteValue("Misc", "ShowStatus", statusCheck.Checked.ToString());
                ini.IniWriteValue("Misc", "KeepAccounts", keepAccountsCheck.Checked.ToString());
                ini.IniWriteValue("Misc", "NucleusAccountPassword", nucUserPassTxt.Text);

                //ini.IniWriteValue("CustomLayout", "Enabled", enableCustomCheckbox.Checked.ToString());
                ini.IniWriteValue("CustomLayout", "HorizontalLines", numHorDiv.Value.ToString());
                ini.IniWriteValue("CustomLayout", "VerticalLines", numVerDiv.Value.ToString());
                ini.IniWriteValue("CustomLayout", "MaxPlayers", numMaxPlyrs.Value.ToString());

                if (audioDefaultSettingsRadio.Checked)
                {
                    ini.IniWriteValue("Audio", "Custom", 0.ToString());
                }
                else
                {
                    ini.IniWriteValue("Audio", "Custom", 1.ToString());
                }

                //ini.IniWriteValue("Misc", "VibrateOpen", check_Vibrate.Checked.ToString());

                foreach (Control ctrl in audioCustomSettingsBox.Controls)
                {
                    if (ctrl is ComboBox)
                    {
                        ComboBox cmb = (ComboBox)ctrl;
                        if (audioDevices?.Count > 0 && audioDevices.Keys.Contains(cmb.Text))
                        {
                            ini.IniWriteValue("Audio", cmb.Name, audioDevices[cmb.Text]);
                        }
                    }
                }

                MessageBox.Show("Settings saved succesfully!", "Saved", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private int GetMod(string modifier)
        {
            int mod = 0;
            switch (modifier)
            {
                case "Ctrl": // Ctrl
                    mod = 2;
                    break;
                case "Alt":
                    mod = 1;
                    break;
                case "Shift":
                    mod = 4;
                    break;
            }
            return mod;
        }

        private void SettingsCloseBtn_Click(object sender, EventArgs e)
        {
            Hide();
               
        }

        private void SettingsCloseHKTxt_TextChanged(object sender, EventArgs e)
        {
            settingsCloseHKTxt.Text = string.Concat(settingsCloseHKTxt.Text.Where(char.IsLetterOrDigit));
        }

        public void RegHotkeys(MainForm form)
        {
            mainForm = form;

            try
            {
                User32Interop.RegisterHotKey(form.Handle, KillProcess_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "Close").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "Close").Split('+')[1].ToString()));
                User32Interop.RegisterHotKey(form.Handle, TopMost_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "TopMost").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "TopMost").Split('+')[1].ToString()));
                User32Interop.RegisterHotKey(form.Handle, StopSession_HotkeyID, GetMod(ini.IniReadValue("Hotkeys", "Stop").Split('+')[0].ToString()), (int)Enum.Parse(typeof(Keys), ini.IniReadValue("Hotkeys", "Stop").Split('+')[1].ToString()));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering hotkeys " + ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

        }

        public void RegHotkeys(int id, int mod, int key)
        {
            User32Interop.RegisterHotKey(mainForm.Handle, id, mod, key);
        }

        private void SettingsCloseHKTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            settingsCloseHKTxt.Text = "";
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)
             && !char.IsSeparator(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void SettingsStopTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            settingsStopTxt.Text = "";
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)
             && !char.IsSeparator(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void SettingsTopTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            settingsTopTxt.Text = "";
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)
             && !char.IsSeparator(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void Btn_Refresh_Click(object sender, EventArgs e)
        {
            GetControllers();
        }

        private void Btn_credits_Click(object sender, EventArgs e)
        {          
                MessageBox.Show("Nucleus Co-op - " + mainForm.version +
               "\n " +
               "\nOriginal Nucleus Co-op Project: Lucas Assis(lucasassislar)" +
               "\nNew Nucleus Co-op fork: ZeroFox" +
               "\nMultiple keyboards / mice & hooks: Ilyaki" +
               "\nWebsite & handler API: r - mach" +
               "\nNew UI design & bug fixes: nene27(Mikou27)" +
               "\nHandlers development & testing: Talos91, PoundlandBacon, Pizzo, dr.oldboi and many more." +
               "\nThis new & improved Nucleus Co-op brings a ton of enhancements, such as:" +
               "\n- Massive increase to the amount of compatible games, 400 + as of now." +
               "\n- Beautiful new overhauled user interface with support for themes, game covers & screenshots." +
               "\n- Many quality of life improvements & bug fixes." +
               "\nAnd so much more!" +
               "\nSpecial thanks to: Talos91, dr.oldboi, PoundlandBacon, Pizzo and the rest of the Splitscreen Dreams discord community.", "Credits", MessageBoxButtons.OK, MessageBoxIcon.Information);           
        }

        private void NumHorDiv_ValueChanged(object sender, EventArgs e)
        {    
            layoutSizer.Invalidate();
            tabControl2.TabPages[1].Invalidate();
            numMaxPlyrs.Value = (numHorDiv.Value + 1) * (numVerDiv.Value + 1);
        }

        private void NumVerDiv_ValueChanged(object sender, EventArgs e)
        {         
            layoutSizer.Invalidate();
            tabControl2.TabPages[1].Invalidate();
            numMaxPlyrs.Value = (numHorDiv.Value + 1) * (numVerDiv.Value + 1);
        }

        private void layoutSizer_Paint(object sender, PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics gs = e.Graphics;

            Pen p = new Pen(new SolidBrush(Color.White));
            int LayoutHeight = layoutSizer.Size.Height - 20;
            int LayoutWidth = layoutSizer.Size.Width - 20;
            int LayoutPosX = layoutSizer.Location.X + 10;
            int LayoutPosY = layoutSizer.Location.Y + 10;


            Rectangle outline = new Rectangle(10, 10, LayoutWidth, LayoutHeight);
            // Rectangle outline = new Rectangle(370, 45, 360, 240);
            gs.DrawRectangle(p, outline);

            int[] hlines = new int[(int)numHorDiv.Value];
            int[] vlines = new int[(int)numVerDiv.Value];

            for (int i = 0; i < (int)numHorDiv.Value; i++)
            {
                int divisions = (int)numHorDiv.Value + 1;

                //370-380
                int y = (LayoutHeight / divisions);
                if (i == 0)
                {
                    hlines[i] = y + 10;
                }
                else
                {
                    hlines[i] = y + hlines[i - 1];
                }
                gs.DrawLine(p, 10, hlines[i], 10 + LayoutWidth, hlines[i]);
            }

            for (int i = 0; i < (int)numVerDiv.Value; i++)
            {

                int divisions = (int)numVerDiv.Value + 1;

                //45-460
                int x = (LayoutWidth / divisions);
                if (i == 0)
                {
                    vlines[i] = x + 10;
                }
                else
                {
                    vlines[i] = x + vlines[i - 1];
                }
                gs.DrawLine(p, vlines[i], 10, vlines[i], 10 + LayoutHeight);
            }

            p.Dispose();
            gs.Dispose();
        }

        private void cmb_Network_DropDown(object sender, EventArgs e)
        {
            RefreshCmbNetwork();
        }

        private void RefreshCmbNetwork()
        {
            cmb_Network.Items.Clear();

            cmb_Network.Items.Add("Automatic");

            NetworkInterface[] ni = NetworkInterface.GetAllNetworkInterfaces();
            foreach (NetworkInterface item in ni)
            {
                if (item.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            cmb_Network.Items.Add(item.Name);
                        }
                    }
                }
            }
        }

        private void cmb_Network_DropDownClosed(object sender, EventArgs e)
        {
            if (cmb_Network.SelectedItem == null)
            {
                cmb_Network.SelectedIndex = 0;
            }
        }

        private void keepAccountsCheck_Click(object sender, EventArgs e)
        {
            if (!keepAccountsCheck.Checked)
            {
                DialogResult res = MessageBox.Show("Warning: by disabling this feature, the next time you run a game that uses different user accounts, all Nucleus-made user accounts (and their files, saves) will be deleted. Do you wish to proceed?", "Confirmation", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
                if (res != DialogResult.OK)
                {
                    keepAccountsCheck.Checked = true;
                }
            }
        }

        private void audioCustomSettingsRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (audioCustomSettingsRadio.Checked)
            {
                audioCustomSettingsBox.Enabled = true;
            }
            else
            {
                audioCustomSettingsBox.Enabled = false;
            }
        }

        private void audioBox_DropDown(object sender, EventArgs e)
        {
            //var cmb = (ComboBox)sender;
            //cmb.Items.Clear();
            //cmb.Items.Add("Default");
            //cmb.SelectedItem = "Default";


        }

        private void tabControl2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl2.SelectedTab.Text == "Audio")
            {
                RefreshAudioList();
                MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
                MMDevice audioDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
                audioDefaultDevice.Text = "Default: " + audioDefault.FriendlyName;
            }
        }



        private void RefreshAudioList()
        {
            audioDevices = new Dictionary<string, string>();
            audioDevices.Clear();
            if (!audioDevices.ContainsKey("Default"))
            {
                audioDevices.Add("Default", "Default");
            }

            //ManagementObjectSearcher objSearcher = new ManagementObjectSearcher("SELECT * FROM Win32_SoundDevice");

            //ManagementObjectCollection objCollection = objSearcher.Get();

            //foreach (ManagementObject obj in objCollection)
            //{
            //    audioDevices.Add(obj["Caption"].ToString(), obj["DeviceID"].ToString());
            //}

            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            foreach (MMDevice endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (!audioDevices.ContainsKey(endpoint.FriendlyName))
                {
                    audioDevices.Add(endpoint.FriendlyName, endpoint.ID);
                }
            }

            foreach (Control ctrl in audioCustomSettingsBox.Controls)
            {
                if (ctrl is ComboBox)
                {
                    ComboBox cmb = (ComboBox)ctrl;
                    string lastItem = cmb.Text;
                    cmb.Items.Clear();
                    cmb.Items.AddRange(audioDevices.Keys.ToArray());
                    if (cmb.Items.Contains(lastItem))
                    {
                        cmb.SelectedItem = lastItem;
                    }
                    else if (audioDevices.Values.Contains(ini.IniReadValue("Audio", cmb.Name)))
                    {
                        cmb.SelectedItem = audioDevices.FirstOrDefault(x => x.Value == ini.IniReadValue("Audio", cmb.Name)).Key;
                    }
                    else
                    {
                        cmb.SelectedItem = "Default";
                    }
                }
            }
        }

        private void audioRefresh_Click(object sender, EventArgs e)
        {
            RefreshAudioList();
        }

        private void OfflineMod_SelectedIndexChanged(object sender, EventArgs e)
        {
            ini.IniWriteValue("Dev", "OfflineMod", offlineMod.SelectedItem.ToString());
            if(offlineMod.SelectedIndex == 1)
            {
                mainForm.connected = false;
                mainForm.btn_GameDesc.Enabled = false;
                mainForm.txt_GameDescSizer.Visible = false;
                mainForm.txt_GameDesc.Visible = false;
            }
            else 
            {
                mainForm.CheckNetCon();
            }
        }
    }
}
