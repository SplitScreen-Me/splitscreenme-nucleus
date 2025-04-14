using NAudio.CoreAudioApi;
using Nucleus.Coop.Controls;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Tools.MonitorsDpiScaling;
using Nucleus.Gaming.Tools.Steam;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Coop
{
    public partial class Settings : Form, IDynamicSized
    {
        private string prevTheme;
        private string currentNickname;
        private string currentSteamId;

        private List<Panel> tabs = new List<Panel>();
        private List<Control> tabsButtons = new List<Control>();
        
        private Control[] activeControls;
        private FlatCombo[] controllerNicks;
        private FlatCombo[] steamIds;
        
        public static Button Ctrlr_Shorcuts;
        private float fontSize;
        private List<Control> ctrls = new List<Control>();
        private IDictionary<string, string> audioDevices;

        private Color selectionColor;

        private Rectangle[] tabBorders;
        private List<Point[]> tabLines;

        private Pen bordersPen;
        private bool shouldSwapNick = true;
        private bool shouldSwapSID = true;
        private float _scale;

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
            fontSize = float.Parse(Globals.ThemeConfigFile.IniReadValue("Font", "SettingsFontSize"));

            InitializeComponent();

            FormBorderStyle = FormBorderStyle.None;

            Cursor = Theme_Settings.Default_Cursor;

            var borderscolor = Globals.ThemeConfigFile.IniReadValue("Colors", "ProfileSettingsBorder").Split(',');
            selectionColor = Theme_Settings.SelectedBackColor;
            bordersPen = new Pen(Color.FromArgb(int.Parse(borderscolor[0]), int.Parse(borderscolor[1]), int.Parse(borderscolor[2])));
            BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "other_backgrounds.jpg");

            Ctrlr_Shorcuts = ctrlr_shorcutsBtn;

            Font = new Font(Theme_Settings.CustomFont, Font.Size, Font.Style, GraphicsUnit.Pixel, 0);

            Controlscollect();

            foreach (Control c in ctrls)
            {
                if ((string)c.Tag == "HotKeyTextBox")
                {
                    c.KeyPress += HKTxt_KeyPress;
                    c.TextChanged += HKTxt_TextChanged;
                }

                if (c is CustomCheckBox || c is Label || c is CustomRadio)
                {
                    if (c.Name != "audioWarningLabel" && c.Name != "warningLabel")
                    {
                        c.Font = new Font(Theme_Settings.CustomFont, fontSize, c.Font.Style, GraphicsUnit.Pixel, 0);
                    }

                    if(c is CustomCheckBox checkbox)
                    {
                        checkbox.SelectionColor = Color.FromArgb(255, 31, 34, 35);
                        checkbox.CheckColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                        checkbox.BorderColor = Color.White;
                        checkbox.BackColor = Color.Transparent;
                    }

                    if (c is CustomRadio customRadio)
                    {
                        customRadio.SelectionColor = Color.FromArgb(255, 31, 34, 35);
                        customRadio.CheckColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                        customRadio.BorderColor = Color.White;
                        customRadio.BackColor = Color.Transparent;
                    }
                }

                if (c is FlatCombo || c is FlatTextBox || c is GroupBox)
                {
                    c.Font = new Font(Theme_Settings.CustomFont, fontSize, c.Font.Style, GraphicsUnit.Pixel, 0);

                    if(c is FlatCombo fCB)
                    {                   
                        fCB.FlatStyle = FlatStyle.Flat;
                        fCB.ForeColor = Color.White;
                        fCB.BackColor = Color.FromArgb(31, 34, 35);
                        fCB.BorderColor = Color.White;
                        fCB.ButtonColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                    }

                    if(c is FlatTextBox fTB)
                    {
                        fTB.BackColor = Color.FromArgb(31, 34, 35);
                        fTB.BorderColor = Color.White;
                        fTB.ForeColor = Color.White;
                        fTB.BorderStyle = BorderStyle.FixedSingle;
                    }
                }

                if (c is CustomNumericUpDown num)
                {
                    num.Font = new Font(Theme_Settings.CustomFont, fontSize, c.Font.Style, GraphicsUnit.Pixel, 0);
                    num.UpdownBackColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                }

                if (c.Name != "settingsTab" && c.Name != "playersTab" && c.Name != "audioTab" && c.Name != "screen_panel" &&
                    c.Name != "layoutTab" && c.Name != "layoutSizer"
                    && !(c is Label) && !(c is FlatTextBox) && !(c is GroupBox) && !(c is Panel))
                {
                    c.Cursor = Theme_Settings.Hand_Cursor;
                }

                if (c.Name == "settingsTab" || c.Name == "playersTab" || c.Name == "audioTab" || c.Name == "layoutTab")
                {
                    c.BackColor = Color.Transparent;
                    tabs.Add(c as Panel);
                }

                if ((string)c.Tag == "settingsTab" || (string)c.Tag == "playersTab" || (string)c.Tag == "audioTab" || (string)c.Tag == "layoutTab")
                {
                    c.Click += TabsButtons_highlight;
                    tabsButtons.Add(c);
                }

                if (c.Name.Contains("steamid") && c is FlatCombo)
                {
                    c.KeyPress += Num_KeyPress;
                    c.Click += Steamid_Click;
                }

                if (c is Button button)
                {
                    button.FlatAppearance.BorderSize = 0;
                    button.FlatAppearance.MouseOverBackColor = Color.Transparent;
                }

                if (c.Parent.Name == "hotkeyBox")
                {
                    c.Font = new Font(Theme_Settings.CustomFont, fontSize, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                }

                c.Font = new Font(Theme_Settings.CustomFont,c.Font.Size, c.Font.Style, GraphicsUnit.Pixel, 0);

            }

            ForeColor = Theme_Settings.ControlsForeColor;

            ctrlr_shorcutsBtn.BackColor = Color.FromArgb(255, 31, 34, 35);
            ctrlr_shorcutsBtn.FlatAppearance.MouseOverBackColor = ctrlr_shorcutsBtn.BackColor;
            btn_Gb_Update.BackColor = Color.FromArgb(255, 31, 34, 35);
            btn_Gb_Update.FlatAppearance.MouseOverBackColor = btn_Gb_Update.BackColor;

            audioBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "audio.png");
            playersBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "players.png");
            settingsBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "shared.png");
            layoutBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "layout.png");
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
            audioRefresh.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "refresh.png");
            btnNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page1.png");
            refreshScreenDatasButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "refresh.png");

            plus1.ForeColor = ForeColor;
            plus2.ForeColor = ForeColor;
            plus3.ForeColor = ForeColor;
            plus4.ForeColor = ForeColor;
            plus5.ForeColor = ForeColor;
            plus6.ForeColor = ForeColor;
            plus7.ForeColor = ForeColor;
            plus8.ForeColor = ForeColor;
            plus9.ForeColor = ForeColor;

            audioBtnPicture.Click += AudioBtnPicture_Click;

            audioRefresh.BackColor = Color.Transparent;

            ctrlr_shorcutsBtn.FlatAppearance.BorderSize = 1;
            btn_Gb_Update.FlatAppearance.BorderSize = 1;

            settingsTab.Parent = this;
            settingsTab.Location = new Point(0, settingsTabBtn.Bottom);
            settingsTab.BringToFront();

            playersTab.Parent = this;
            playersTab.Location = settingsTab.Location;

            audioTab.Parent = this;
            audioTab.Location = settingsTab.Location;

            layoutTab.Parent = this;
            layoutTab.Location = settingsTab.Location;

            page1.Location = new Point(playersTab.Width / 2 - page1.Width / 2, playersTab.Height / 2 - page1.Height / 2);
            page2.Location = page1.Location;
            page1.BringToFront();

            btnNext.Parent = playersTab;
            btnNext.BackColor = Theme_Settings.ButtonsBackColor;
            btnNext.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnNext.Location = new Point(page1.Right - btnNext.Width, (page1.Top - btnNext.Height) - 5);
           
            activeControls = new Control[] { settingsTabBtn, settingsBtnPicture };

            audioRefresh.Location = new Point((audioTab.Width / 2) - (audioRefresh.Width / 2), audioRefresh.Location.Y);

            numUpDownVer.MaxValue = 5;
            numUpDownVer.InvalidParent = true;

            numUpDownHor.MaxValue = 5;
            numUpDownHor.InvalidParent = true;

            controllerNicks = new FlatCombo[] {
                player1N, player2N, player3N, player4N, player5N, player6N, player7N, player8N,
                player9N, player10N, player11N, player12N, player13N, player14N, player15N, player16N,
                player17N, player18N, player19N, player20N, player21N, player22N, player23N, player24N,
                player25N, player26N, player27N, player28N, player29N, player30N, player31N, player32N};

            steamIds = new FlatCombo[] {
                steamid1, steamid2, steamid3, steamid4, steamid5, steamid6, steamid7, steamid8,
                steamid9, steamid10, steamid11, steamid12, steamid13, steamid14, steamid15, steamid16,
                steamid17, steamid18, steamid19, steamid20, steamid21, steamid22, steamid23, steamid24,
                steamid25, steamid26, steamid27, steamid28, steamid29, steamid30, steamid31, steamid32};

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                var nickField = controllerNicks[i];
                nickField.TextChanged += SwapNickname;
                nickField.KeyPress += CheckTypingNick;
                nickField.MouseHover += CacheNickname;
                nickField.LostFocus += UpdateControllerNickItems;

                var sidField = steamIds[i];
                sidField.TextChanged += SwapSteamId;
                sidField.KeyPress += CheckTypingSID;
                sidField.MouseHover += CacheSteamId;
                sidField.LostFocus += UpdateSteamIdsItems;
            }

            splitDiv.Checked = App_Layouts.SplitDiv;
            hideDesktop.Checked = App_Layouts.HideOnly;
            cts_Mute.Checked = App_Layouts.Cts_MuteAudioOnly;
            cts_kar.Checked = App_Layouts.Cts_KeepAspectRatio;
            cts_unfocus.Checked = App_Layouts.Cts_Unfocus;

            enable_WMerger.Checked = App_Layouts.WindowsMerger;
            losslessHook.Checked = App_Layouts.LosslessHook;

            numUpDownHor.Value = App_Layouts.HorizontalLines;
            numUpDownVer.Value = App_Layouts.VerticalLines;
            numMaxPlyrs.Value = App_Layouts.MaxPlayers;

            disableGameProfiles.Checked = App_Misc.DisableGameProfiles;
            gamepadsAssignMethods.Checked = App_Misc.UseXinputIndex;
            gamepadsAssignMethods.Visible = !disableGameProfiles.Checked;

            DevicesFunctions.UseGamepadApiIndex = gamepadsAssignMethods.Checked;

            ///network setting
            RefreshCmbNetwork();

            if (App_Misc.Network != "")
            {
                cmb_Network.Text = App_Misc.Network;
                cmb_Network.SelectedIndex = cmb_Network.Items.IndexOf(App_Misc.Network);
            }
            else
            {
                cmb_Network.SelectedIndex = 0;
            }

            foreach (KeyValuePair<string, System.Windows.Media.SolidColorBrush> color in BackgroundColors.ColorsDictionnary)
            {
                SplitColors.Items.Add(color.Key);
            }

            SplitColors.SelectedItem = App_Layouts.SplitDivColor;

            string[] themeList = Directory.GetDirectories(Path.Combine(Application.StartupPath, @"gui\theme\"));

            foreach (string themePath in themeList)
            {
                string[] _path = themePath.Split('\\');
                int last = _path.Length - 1;

                themeCbx.Items.AddRange(new object[] {
                    _path[last]
                });

                string[] themeName = Globals.ThemeFolder.Split('\\');
                if (_path[last] != themeName[themeName.Length - 2])
                {
                    continue;
                }

                themeCbx.Text = _path[last];
                prevTheme = _path[last];
            }

            ///epic lang setting
            cmb_EpicLang.SelectedItem = App_Misc.EpicLang;

            useNicksCheck.Checked = App_Misc.UseNicksInGame;

            keepAccountsCheck.Checked = App_Misc.KeepAccounts;

            ///auto scale setting
            scaleOptionCbx.Checked = App_Misc.AutoDesktopScaling;

            ///Custom HotKey setting
            lockKey_Cmb.Text = App_Hotkeys.LockInputs;

            if (App_Misc.SteamLang != "")
            {
                cmb_Lang.Text = App_Misc.SteamLang;
            }
            else
            {
                cmb_Lang.SelectedIndex = 0;
            }

            if ((App_Hotkeys.CloseApp[0] == "Ctrl" || App_Hotkeys.CloseApp[0] == "Alt" || App_Hotkeys.CloseApp[0] == "Shift") && App_Hotkeys.CloseApp[1].Length == 1 && Regex.IsMatch(App_Hotkeys.CloseApp[1], @"^[a-zA-Z0-9]+$"))
            {
                cn_Cmb.SelectedItem = App_Hotkeys.CloseApp[0];
                cn_HKTxt.Text = App_Hotkeys.CloseApp[1];
            }

            if ((App_Hotkeys.StopSession[0] == "Ctrl" || App_Hotkeys.StopSession[0] == "Alt" || App_Hotkeys.StopSession[0] == "Shift") && App_Hotkeys.StopSession[1].Length == 1 && Regex.IsMatch(App_Hotkeys.StopSession[1], @"^[a-zA-Z0-9]+$"))
            {
                ss_Cmb.SelectedItem = App_Hotkeys.StopSession[0];
                ss_HKTxt.Text = App_Hotkeys.StopSession[1];
            }

            if ((App_Hotkeys.TopMost[0] == "Ctrl" || App_Hotkeys.TopMost[0] == "Alt" || App_Hotkeys.TopMost[0] == "Shift") && App_Hotkeys.TopMost[1].Length == 1 && Regex.IsMatch(App_Hotkeys.TopMost[1], @"^[a-zA-Z0-9]+$"))
            {
                ttm_Cmb.SelectedItem = App_Hotkeys.TopMost[0];
                ttm_HKTxt.Text = App_Hotkeys.TopMost[1];
            }

            if ((App_Hotkeys.SetFocus[0] == "Ctrl" || App_Hotkeys.SetFocus[0] == "Alt" || App_Hotkeys.SetFocus[0] == "Shift") && App_Hotkeys.SetFocus[1].Length == 1 && Regex.IsMatch(App_Hotkeys.SetFocus[1], @"^[a-zA-Z0-9]+$"))
            {
                tu_Cmb.SelectedItem = App_Hotkeys.SetFocus[0];
                tu_HKTxt.Text = App_Hotkeys.SetFocus[1];
            }

            if ((App_Hotkeys.ResetWindows[0] == "Ctrl" || App_Hotkeys.ResetWindows[0] == "Alt" || App_Hotkeys.ResetWindows[0] == "Shift") && App_Hotkeys.ResetWindows[1].Length == 1 && Regex.IsMatch(App_Hotkeys.ResetWindows[1], @"^[a-zA-Z0-9]+$"))
            {
                rw_Cmb.SelectedItem = App_Hotkeys.ResetWindows[0];
                rw_HKTxt.Text = App_Hotkeys.ResetWindows[1];
            }

            if ((App_Hotkeys.CutscenesMode[0] == "Ctrl" || App_Hotkeys.CutscenesMode[0] == "Alt" || App_Hotkeys.CutscenesMode[0] == "Shift") && App_Hotkeys.CutscenesMode[1].Length == 1 && Regex.IsMatch(App_Hotkeys.CutscenesMode[1], @"^[a-zA-Z0-9]+$"))
            {
                csm_Cmb.SelectedItem = App_Hotkeys.CutscenesMode[0];
                csm_HKTxt.Text = App_Hotkeys.CutscenesMode[1];
            }

            if ((App_Hotkeys.SwitchLayout[0] == "Ctrl" || App_Hotkeys.SwitchLayout[0] == "Alt" || App_Hotkeys.SwitchLayout[0] == "Shift") && App_Hotkeys.SwitchLayout[1].Length == 1 && Regex.IsMatch(App_Hotkeys.SwitchLayout[1], @"^[a-zA-Z0-9]+$"))
            {
                swl_Cmb.SelectedItem = App_Hotkeys.SwitchLayout[0];
                swl_HKTxt.Text = App_Hotkeys.SwitchLayout[1];
            }

            if ((App_Hotkeys.ShortcutsReminder[0] == "Ctrl" || App_Hotkeys.ShortcutsReminder[0] == "Alt" || App_Hotkeys.ShortcutsReminder[0] == "Shift") && App_Hotkeys.ShortcutsReminder[1].Length == 1 && Regex.IsMatch(App_Hotkeys.ShortcutsReminder[1], @"^[a-zA-Z0-9]+$"))
            {
                rm_Cmb.SelectedItem = App_Hotkeys.ShortcutsReminder[0];
                rm_HKTxt.Text = App_Hotkeys.ShortcutsReminder[1];
            }

            if ((App_Hotkeys.SwitchMergerForeGroundChild[0] == "Ctrl" || App_Hotkeys.SwitchMergerForeGroundChild[0] == "Alt" || App_Hotkeys.SwitchMergerForeGroundChild[0] == "Shift") && App_Hotkeys.SwitchMergerForeGroundChild[1].Length == 1 && Regex.IsMatch(App_Hotkeys.SwitchMergerForeGroundChild[1], @"^[a-zA-Z0-9]+$"))
            {
                smfw_Cmb.SelectedItem = App_Hotkeys.SwitchMergerForeGroundChild[0];
                smfw_HKTxt.Text = App_Hotkeys.SwitchMergerForeGroundChild[1];
            }

            ignoreInputLockReminderCheckbox.Checked = App_Misc.IgnoreInputLockReminder;

            debugLogCheck.Checked = App_Misc.DebugLog;

            if (App_Misc.NucleusAccountPassword != "")
            {
                nucUserPassTxt.Text = App_Misc.NucleusAccountPassword;
            }

            if (App_Audio.Custom == "0")
            {
                audioDefaultSettingsRadio.Checked = true;
                audioCustomSettingsBox.Enabled = false;
            }
            else
            {
                audioCustomSettingsRadio.Checked = true;
            }

            RefreshAudioList();

            GetPlayersNickNameAndSteamIds();

            Rectangle area = Screen.PrimaryScreen.Bounds;
            if (App_Misc.SettingsLocation != "")
            {
                string[] windowLocation = App_Misc.SettingsLocation.Split('X');

                if (ScreensUtil.AllScreens().All(s => !s.MonitorBounds.Contains(int.Parse(windowLocation[0]), int.Parse(windowLocation[1]))))
                {
                    CenterToScreen();
                }
                else
                {
                    Location = new Point(area.X + int.Parse(windowLocation[0]), area.Y + int.Parse(windowLocation[1]));
                }
            }
            else
            {
                CenterToScreen();
            }

            SetToolTips();

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

            _scale = scale;

            float newFontSize = Font.Size * scale;

            foreach (Control c in ctrls)
            {
                if (scale > 1.0F)
                {
                    if (c is FlatCombo || c is FlatTextBox || c is GroupBox)
                    {

                        c.Font = new Font(c.Font.FontFamily, c is FlatTextBox ? newFontSize + 3 : newFontSize, c.Font.Style, GraphicsUnit.Pixel, 0);
                    }

                    float wmBoxlocY = (float)plus9.Top + (((float)plus9.DisplayRectangle.Height / 2f) - ((float)smfw_HKTxt.DisplayRectangle.Height / 2f));

                    smfw_HKTxt.Location = new Point(smfw_HKTxt.Location.X, (int)wmBoxlocY);
                    smfw_Cmb.Location = new Point(smfw_Cmb.Location.X, (int)wmBoxlocY);
                }

                if (c is Label && !(c is CustomNumericUpDown))
                {
                    if (c.Name.Contains("hkLabel"))
                    {
                        c.Location = new Point(tu_Cmb.Left - c.Width, c.Location.Y);
                    }
                }
            }
   
            audioRefresh.Location = new Point((audioTab.Width / 2) - (audioRefresh.Width / 2), audioRefresh.Location.Y);
            audioWarningLabel.Location = new Point(audioTab.Width / 2 - audioWarningLabel.Width / 2, audioWarningLabel.Location.Y);
            gamepadsAssignMethods.Location = new Point((page1.Location.X + label7.Location.X) + 2, (page1.Top - 5) - gamepadsAssignMethods.Height);
            refreshScreenDatasButton.Location = new Point(mergerResSelectorLabel.Right, refreshScreenDatasButton.Location.Y);

            int tabButtonsY = settingsTabBtn.Location.Y - 1;
            int tabButtonsHeight = settingsTabBtn.Height + 1;

            tabBorders = new Rectangle[]
            {
               new Rectangle(0,tabButtonsY,layoutBtnPicture.Right + 1,tabButtonsHeight),
               new Rectangle(settingsTab.Location.X, settingsTab.Location.Y, settingsTab.Width - 1, settingsTab.Height),
            };

            tabLines = new List<Point[]>()
            {
               new Point[]{ new Point(settingsBtnPicture.Right, tabButtonsY + 1),new Point(settingsBtnPicture.Right, settingsTab.Location.Y) },
               new Point[]{ new Point(playersBtnPicture.Right, tabButtonsY + 1),new Point(playersBtnPicture.Right, settingsTab.Location.Y) },
               new Point[]{ new Point(audioBtnPicture.Right, tabButtonsY + 1),new Point(audioBtnPicture.Right, settingsTab.Location.Y) },
            };

            //Do it here so the merger settings ComboBox(es) scales correctly
            GetAllScreensResolutions();
        }

        private void SetToolTips()
        {
            CustomToolTips.SetToolTip(splitDiv, "May not work for all games", "splitDiv", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(hideDesktop, "Will only show the splitscreen division window without adjusting the game windows size and offset.", "hideDesktop", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(disableGameProfiles, "Disables profiles, Nucleus will use the global settings instead.", "disableGameProfiles", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(gamepadsAssignMethods, "Can break controller support in some handlers. If enabled profiles\n" +
                                                             "will not save per player gamepad but use XInput indexes instead \n" +
                                                             "(switching modes could prevent some profiles to load properly).\n" +
                                                             "Note: Nucleus will return to home screen.", "gamepadsAssignMethods" , new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(enable_WMerger, "Game windows will be merged to a single window\n" +
                                                       "so Lossless Scaling can be used with Nucleus.\n " +
                                                       "Note that there's no multiple monitor support yet.", "enable_WMerger", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            CustomToolTips.SetToolTip(losslessHook, "Lossless will not stop upscaling if an other window get the focus, useful\n" +
                                                    "if game windows requires real focus to receive inputs.", "losslessHook", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(refreshScreenDatasButton, "Refresh screens info.", "refreshScreenDatasButton", new int[] { 190, 0, 0, 0 },new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(mergerShortcutLabel, "Each press will set an other child window as foreground window(similar to Alt+Tab).", "mergerShortcutLabel", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
        }

        private void GetPlayersNickNameAndSteamIds()
        {
            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                var sidField = steamIds[i];
             
                sidField.Text = PlayersIdentityCache.GetSteamIdAt(i);
                sidField.Items.AddRange(PlayersIdentityCache.PlayersSteamId.Where(sid => sid != "" && sid != sidField.Text).ToArray());
                sidField.Items.AddRange(PlayersIdentityCache.SteamIdsBackup.ToArray());
                sidField.SelectedItem = sidField.Text;

                var nickField = controllerNicks[i];
               
                nickField.Items.AddRange(PlayersIdentityCache.PlayersNickname.ToArray());                       
                nickField.Items.AddRange(PlayersIdentityCache.NicknamesBackup.Where(nic => !nickField.Items.Contains(nic)).ToArray());
                nickField.Text = PlayersIdentityCache.GetNicknameAt(i);
                nickField.SelectedItem = nickField.Text;

                for (int j = 0; j < Globals.NucleusMaxPlayers; j++)
                {
                    string defNick = PlayersIdentityCache.DefaultNicknames[j];

                    if (!nickField.Items.Contains(defNick))
                    {
                        nickField.Items.Insert(nickField.Items.Count, defNick);
                    }

                    string defSid = PlayersIdentityCache.DefaultSteamIds[j].ToString();

                    if (!sidField.Items.Contains(defSid))
                    {
                        sidField.Items.Insert(sidField.Items.Count, defSid);
                    }
                }
            }
        }

        private void CloseBtnPicture_Click(object sender, EventArgs e)
        {
            if (!Directory.Exists(Globals.GameProfilesFolder))
            {
                Directory.CreateDirectory(Globals.GameProfilesFolder);
            }

            bool sidWrongValue = false;
            bool hasEmptyNickname = false;

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                var nickField = controllerNicks[i];

                if (nickField.Text != "")
                {                    
                    PlayersIdentityCache.SetNicknameAt(i, nickField.Text);

                    for (int n = 0; n < Globals.NucleusMaxPlayers; n++)
                    {
                        if (controllerNicks[n].Items.Contains(nickField.Text))
                        {
                            continue;
                        }

                        controllerNicks[n].Items.Add(nickField.Text);
                    }
                }
                else
                {
                    hasEmptyNickname = true;
                }

                var sidField = steamIds[i];

                if (Regex.IsMatch(sidField.Text, "^[0-9]+$") && sidField.Text.Length == 17 || sidField.Text.Length == 0)
                {
                    FlatCombo hasSameText = steamIds.Where(cb => cb != sidField && cb.Text == sidField.Text).FirstOrDefault();

                    if (hasSameText != null)
                    {
                        sidField.BackColor = Color.Red;
                        hasSameText.BackColor = Color.Red;
                        sidWrongValue = true;
                        break;
                    }

                    PlayersIdentityCache.SetSteamIdAt(i,sidField.Text);
                }
                else
                {
                    sidField.BackColor = Color.Red;
                    sidWrongValue = true;
                }
            }

            if (hasEmptyNickname)
            {
                playersTab.BringToFront();
                MessageBox.Show("Nickname fields can't be empty!");
                return;
            }

            if (sidWrongValue)
            {
                playersTab.BringToFront();
                MessageBox.Show("Must be 17 numbers e.g. \"76561199075562883\" and be different for each player.", "Incorrect steam id format!");
                return;
            }

            PlayersIdentityCache.BackupNicknames();
            PlayersIdentityCache.BackupSteamIds();

            if (audioDefaultSettingsRadio.Checked)
            {
                App_Audio.Custom = 0.ToString();
            }
            else
            {
                App_Audio.Custom = 1.ToString();
            }

            foreach (Control ctrl in audioCustomSettingsBox.Controls)
            {
                if (ctrl is FlatCombo cmb)
                {
                    if (audioDevices?.Count > 0 && audioDevices.Keys.Contains(cmb.Text))
                    {
                        App_Audio.SaveAudioOutput(cmb.Name, audioDevices[cmb.Text]);
                    }
                }
            }

            foreach (Control ht in hotkeyBox.Controls)
            {
                if (ht is FlatTextBox)
                {
                    if (ht.Text == "")
                    {
                        MessageBox.Show("Hotkeys values can't be empty", "Invalid hotkeys values!");
                        return;
                    }
                }
            }

            if (smfw_HKTxt.Text == "")
            {
                MessageBox.Show("Merger hotkey value can't be empty", "Invalid hotkey value!");
                return;
            }

            App_Hotkeys.CloseApp = new string[] { cn_Cmb.SelectedItem.ToString(), cn_HKTxt.Text };
            App_Hotkeys.StopSession = new string[] { ss_Cmb.SelectedItem.ToString(), ss_HKTxt.Text };
            App_Hotkeys.TopMost = new string[] { ttm_Cmb.SelectedItem.ToString(), ttm_HKTxt.Text };
            App_Hotkeys.SetFocus = new string[] { tu_Cmb.SelectedItem.ToString(), tu_HKTxt.Text };
            App_Hotkeys.ResetWindows = new string[] { rw_Cmb.SelectedItem.ToString(), rw_HKTxt.Text };
            App_Hotkeys.LockInputs = lockKey_Cmb.SelectedItem.ToString();
            App_Hotkeys.CutscenesMode = new string[] { csm_Cmb.SelectedItem.ToString(), csm_HKTxt.Text };
            App_Hotkeys.SwitchLayout = new string[] { swl_Cmb.SelectedItem.ToString(), swl_HKTxt.Text };
            App_Hotkeys.ShortcutsReminder = new string[] { rm_Cmb.SelectedItem.ToString(), rm_HKTxt.Text };
            App_Hotkeys.SwitchMergerForeGroundChild = new string[] { smfw_Cmb.SelectedItem.ToString(), smfw_HKTxt.Text };

            App_Misc.UseNicksInGame = useNicksCheck.Checked;
            App_Misc.KeepAccounts = keepAccountsCheck.Checked;
            App_Misc.Network = cmb_Network.Text.ToString();
            App_Misc.IgnoreInputLockReminder = ignoreInputLockReminderCheckbox.Checked;
            App_Misc.DebugLog = debugLogCheck.Checked;
            App_Misc.SteamLang = cmb_Lang.SelectedItem.ToString();
            App_Misc.EpicLang = cmb_EpicLang.SelectedItem.ToString();

            App_Misc.NucleusAccountPassword = nucUserPassTxt.Text;
            App_Misc.AutoDesktopScaling = scaleOptionCbx.Checked;

            App_Misc.UseXinputIndex = gamepadsAssignMethods.Checked;

            App_Layouts.SplitDiv = splitDiv.Checked;
            App_Layouts.HideOnly = hideDesktop.Checked;
            App_Layouts.SplitDivColor = SplitColors.Text.ToString();
            App_Layouts.HorizontalLines = numUpDownHor.Value;
            App_Layouts.VerticalLines = numUpDownVer.Value;
            App_Layouts.MaxPlayers = numMaxPlyrs.Value;

            App_Layouts.Cts_MuteAudioOnly = cts_Mute.Checked;
            App_Layouts.Cts_KeepAspectRatio = cts_kar.Checked;
            App_Layouts.Cts_Unfocus = cts_unfocus.Checked;
            App_Layouts.WindowsMerger = enable_WMerger.Checked;

            if (SetupScreenControl.Instance != null)
            {
                if (DevicesFunctions.UseGamepadApiIndex != gamepadsAssignMethods.Checked)
                {
                    DevicesFunctions.UseGamepadApiIndex = gamepadsAssignMethods.Checked;
                    UI_Functions.RefreshUI(true);
                }
            }

            if (disableGameProfiles.Checked != App_Misc.DisableGameProfiles)
            {
                App_Misc.DisableGameProfiles = disableGameProfiles.Checked;
            }

            bool needToRestart = false;

            if (themeCbx.SelectedItem.ToString() != prevTheme)
            {
                App_Misc.Theme = themeCbx.SelectedItem.ToString();
                UI_Interface.RestartRequired = true;
                needToRestart = true;
            }

            if (UI_Interface.Xinput_S_Setup.Visible)
            {
                UI_Interface.Xinput_S_Setup.Visible = false;
            }

            #region take a picture of the hotkeys

            Color defColor = hotkeyBox.BackColor;

            try
            {
                hotkeyBox.BackColor = Color.Black;

                Graphics g = hotkeyBox.CreateGraphics();
                Bitmap bmp = new Bitmap(hotkeyBox.Width, hotkeyBox.Height);
                hotkeyBox.DrawToBitmap(bmp, new Rectangle(0, 0, hotkeyBox.Width, hotkeyBox.Height));

                string savePath = Path.Combine(Application.StartupPath, $@"gui\shortcuts");

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                bmp.Save(Path.Combine(savePath, "KbShortcutsReminder.jpeg"), ImageFormat.Jpeg);
                bmp.Dispose();
                g.Dispose();

                hotkeyBox.BackColor = defColor;
            }
            catch
            {
                hotkeyBox.BackColor = defColor;
            }

            #endregion

            if (needToRestart)
            {
                Thread.Sleep(300);
                Application.Restart();
                Process.GetCurrentProcess().Kill();
                return;
            }

            Globals.MainOSD.Show(500, "Settings Saved");

            if (Location.X == -32000 || Width == 0)
            {
                Visible = false;
                return;
            }

            App_Misc.SettingsLocation = Location.X + "X" + Location.Y;
            UI_Interface.MainForm.BringToFront();
            Visible = false;
        }

        private void Steamid_Click(object sender, EventArgs e)
        {
            FlatCombo id = (FlatCombo)sender;
            id.BackColor = Color.FromArgb(255, 31, 34, 35);
        }

        private void Cmb_Network_DropDown(object sender, EventArgs e)
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

        private void Cmb_Network_DropDownClosed(object sender, EventArgs e)
        {
            if (cmb_Network.SelectedItem == null)
            {
                cmb_Network.SelectedIndex = 0;
            }
        }

        private void AudioCustomSettingsRadio_CheckedChanged(object sender, EventArgs e)
        {
            CustomRadio radio = (CustomRadio)sender;
            audioCustomSettingsBox.Enabled = radio.Checked;
        }

        private void RefreshAudioList()
        {
            audioDevices = new Dictionary<string, string>();
            audioDevices.Clear();

            if (!audioDevices.ContainsKey("Default"))
            {
                audioDevices.Add("Default", "Default");
            }

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
                if (ctrl is FlatCombo cmb)
                {
                    string lastItem = cmb.Text;
                    cmb.Items.Clear();
                    cmb.Items.AddRange(audioDevices.Keys.ToArray());

                    if (cmb.Items.Contains(lastItem))
                    {
                        cmb.SelectedItem = lastItem;
                    }
                    else if (audioDevices.Values.Contains(App_Audio.Instances_AudioOutput[cmb.Name]))
                    {
                        cmb.SelectedItem = audioDevices.FirstOrDefault(x => x.Value == App_Audio.Instances_AudioOutput[cmb.Name]).Key;
                    }
                    else
                    {
                        cmb.SelectedItem = "Default";
                    }
                }
            }
        }

        private void AudioRefresh_Click(object sender, EventArgs e)
        {
            RefreshAudioList();
        }

        private void TabsButtons_highlight(object sender, EventArgs e)
        {
            Control c = sender as Control;

            Panel activeTab = tabs.Where(t => (string)t.Name == (string)c.Tag).FirstOrDefault();

            foreach (var b in tabsButtons)
            {
                if (b.Tag == c.Tag && b != c)
                {
                    activeControls = new Control[] { c, b };
                }
            }

            activeTab.BringToFront();
        }

        private void AudioBtnPicture_Click(object sender, EventArgs e)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice audioDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            audioDefaultDevice.Text = "Default: " + audioDefault.FriendlyName;
        }

        private void CloseBtnPicture_MouseEnter(object sender, EventArgs e)
        {
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        }

        private void CloseBtnPicture_MouseLeave(object sender, EventArgs e)
        {
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
        }

        private void Ctrlr_shorcuts_Click(object sender, EventArgs e)
        {
            if (UI_Interface.Xinput_S_Setup.Visible)
            {
                UI_Interface.Xinput_S_Setup.BringToFront();
                return;
            }

            UI_Interface.Xinput_S_Setup.Show();
        }

        private void KeepAccountsCheck_Click(object sender, EventArgs e)
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

        private void Num_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void ReadOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void HKTxt_TextChanged(object sender, EventArgs e)
        {
            FlatTextBox textBox = (FlatTextBox)sender;
            textBox.Text = string.Concat(textBox.Text.Where(char.IsLetterOrDigit));
        }

        private void HKTxt_KeyPress(object sender, KeyPressEventArgs e)
        {
            FlatTextBox textBox = (FlatTextBox)sender;
            textBox.Text = "";
            e.Handled = !char.IsLetter(e.KeyChar) && !char.IsControl(e.KeyChar)
             && !char.IsSeparator(e.KeyChar) && !char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar);
        }

        private void Cts_Mute_CheckedChanged(object sender, EventArgs e)
        {
            CustomCheckBox mute = (CustomCheckBox)sender;

            if (mute.Checked)
            {
                cts_kar.Checked = false;
                cts_kar.Enabled = false;
                cts_unfocus.Checked = false;
                cts_unfocus.Enabled = false;
            }
            else
            {
                cts_kar.Enabled = true;
                cts_unfocus.Enabled = true;
            }
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            if (page1.Visible)
            {
                btnNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page2.png");
                page1.Visible = false;
                page2.BringToFront();
                page2.Visible = true;
            }
            else
            {
                btnNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page1.png");
                page2.Visible = false;
                page1.BringToFront();
                page1.Visible = true;
            }
        }

        private void Controlscollect()
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

        private void LayoutSizer_Paint(object sender, PaintEventArgs e)
        {
            Graphics gs = e.Graphics;

            int LayoutHeight = layoutSizer.Size.Height - 20;
            int LayoutWidth = layoutSizer.Size.Width - 20;

            Rectangle outline = new Rectangle(10, 10, LayoutWidth, LayoutHeight);
            SolidBrush backBr = new SolidBrush(Color.FromArgb(255, 31, 34, 35));
            gs.FillRectangle(backBr,outline);
            backBr.Dispose();

            gs.DrawRectangle(bordersPen, outline);

            int[] hlines = new int[(int)numUpDownHor.Value];
            int[] vlines = new int[(int)numUpDownVer.Value];

            for (int i = 0; i < (int)numUpDownHor.Value; i++)
            {
                int divisions = (int)numUpDownHor.Value + 1;

                int y = (LayoutHeight / divisions);
                if (i == 0)
                {
                    hlines[i] = y + 10;
                }
                else
                {
                    hlines[i] = y + hlines[i - 1];
                }
                gs.DrawLine(bordersPen, 10, hlines[i], 10 + LayoutWidth, hlines[i]);
            }

            for (int i = 0; i < (int)numUpDownVer.Value; i++)
            {

                int divisions = (int)numUpDownVer.Value + 1;

                int x = (LayoutWidth / divisions);
                if (i == 0)
                {
                    vlines[i] = x + 10;
                }
                else
                {
                    vlines[i] = x + vlines[i - 1];
                }
                gs.DrawLine(bordersPen, vlines[i], 10, vlines[i], 10 + LayoutHeight);
            }

            numMaxPlyrs.Value = (numUpDownHor.Value + 1) * (numUpDownVer.Value + 1);
            gs.Dispose();
        }

        private void Settings_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var _activeControls = activeControls.OrderBy(c => c is PictureBox).ToArray();

            SolidBrush lineBrush = new SolidBrush(Color.FromArgb(200, selectionColor.R, selectionColor.G, selectionColor.B));
            Rectangle butt = new Rectangle(_activeControls[0].Left, _activeControls[0].Bottom + 2, _activeControls[1].Right - _activeControls[0].Left, 3);

            g.FillRectangle(lineBrush, butt);
            g.DrawRectangles(bordersPen, tabBorders);
            lineBrush.Dispose();

            foreach (var points in tabLines)
                e.Graphics.DrawLines(bordersPen, points);
        }

        private void Btn_Gb_Update_Click(object sender, EventArgs e)
        {
            GoldbergUpdaterForm gbUpdater = new GoldbergUpdaterForm();
            gbUpdater.ShowDialog();
        }

        private void CacheNickname(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;
            currentNickname = cb.Text;
            shouldSwapNick = true;
        }

        private void SwapNickname(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;

            if (cb.Text == "" || !shouldSwapNick)
            {
                return;
            }

            FlatCombo ch = controllerNicks.Where(tb => tb.Text == cb.Text && tb != cb).FirstOrDefault();

            if (ch != null)
            {
                ch.Text = currentNickname;
                currentNickname = cb.Text;
                FlatCombo checkFroDouble = controllerNicks.Where(tb => tb.Text == cb.Text && tb != cb).FirstOrDefault();
                if (checkFroDouble != null)
                    checkFroDouble.Text = "";
            }
        }

        private void CheckTypingNick(object sender, KeyPressEventArgs e)
        {
            shouldSwapNick = false;
        }

        private void CacheSteamId(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;
            currentSteamId = cb.Text;
            shouldSwapSID = true;
        }

        private void SwapSteamId(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;

            if (cb.Text == "" || !shouldSwapSID)
            {
                return;
            }

            FlatCombo ch = steamIds.Where(tb => tb.Text == cb.Text && tb != cb).FirstOrDefault();

            if (ch != null)
            {
                ch.Text = currentSteamId;
                currentSteamId = cb.Text;
                FlatCombo duplicatedId= steamIds.Where(tb => tb.Text == cb.Text && tb != cb).FirstOrDefault();
                if(duplicatedId != null)
                    duplicatedId.Text = "";
            }
        }

        private void CheckTypingSID(object sender, KeyPressEventArgs e)
        {
            shouldSwapSID = false;
        }

        private void UpdateControllerNickItems(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;

            if (cb.Text == "")
            {
                return;
            }

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                if (!controllerNicks[i].Items.Contains(cb.Text))
                {
                    controllerNicks[i].Items.Add(cb.Text);
                }
            }
        }

        private void UpdateSteamIdsItems(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;

            if (cb.Text == "")
            {
                return;
            }

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                if (!steamIds[i].Items.Contains(cb.Text))
                {
                    steamIds[i].Items.Add(cb.Text);
                }
            }
        }

        private Dictionary<string, List<string>> AllScreensRes;
        private List<Label> screensLabels;
        private List<Point[]> screensResControlsRow;

        private void GetAllScreensResolutions()
        {
            screen_panel.Visible = false;
            screensResControlsRow = new List<Point[]>();
            screensLabels = new List<Label>();
            AllScreensRes = new Dictionary<string, List<string>>();

            foreach (Control con in screen_panel.Controls)
            {
                con.Dispose();
            }

            screen_panel.Controls.Clear();

            MonitorsDpiScaling.DEVMODE vDevMode = new MonitorsDpiScaling.DEVMODE();

            int i = 0;

            for(int j = 0; j < Screen.AllScreens.Length;j++)
            {
                var screen = Screen.AllScreens[j];

                string mergerRes = App_Layouts.WindowsMergerRes;

                if (mergerRes == "")
                {
                    if (screen.Primary)
                    {
                        App_Layouts.WindowsMergerRes = $"{screen.Bounds.Width}X{screen.Bounds.Height}";
                        selectedRes.Text = App_Layouts.WindowsMergerRes;
                    }
                }
                else
                {
                    selectedRes.Text = App_Layouts.WindowsMergerRes;
                }

                FlatCombo resCmb = new FlatCombo();
                resCmb.BackColor = Color.FromArgb(255, 31, 34, 35);
                resCmb.ForeColor = Color.White;
                resCmb.FlatStyle = FlatStyle.Flat;
                resCmb.BorderColor = Color.White;
                resCmb.ButtonColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                resCmb.SelectedValueChanged += SaveSelectedRes;
                resCmb.DropDownStyle = ComboBoxStyle.DropDownList;
                resCmb.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                resCmb.Width = (int)(80 * _scale);
                resCmb.Cursor = Theme_Settings.Hand_Cursor;

                Label resLabel = new Label();
                resLabel.AutoSize = true;
                resLabel.Font = Font;
                resLabel.Cursor = Theme_Settings.Hand_Cursor;
                string cleanName = screen.DeviceName.Substring(screen.DeviceName.LastIndexOf('\\') + 1);
                resLabel.Text = $"⛶ {cleanName}";

                CustomToolTips.SetToolTip(resLabel, "Click here to identify the screen", $"resLabel{j}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

                if (screensLabels.Count == 0)
                {
                    screen_panel.Controls.Add(resLabel);
                    resLabel.Location = new Point(0, 5);
                    screensLabels.Add(resLabel);
                    screen_panel.Controls.Add(resCmb);
                }
                else
                {
                    screen_panel.Controls.Add(resLabel);
                    resLabel.Location = new Point(screensLabels[screensLabels.Count - 1].Location.X, screensLabels[screensLabels.Count - 1].Bottom + 5);
                    screensLabels.Add(resLabel);
                    screen_panel.Controls.Add(resCmb);
                }

                List<string> resolutions = new List<string>();

                while (MonitorsDpiScaling.EnumDisplaySettings(screen.DeviceName, i, ref vDevMode))
                {
                    string resString = $"{vDevMode.dmPelsWidth}X{vDevMode.dmPelsHeight}";

                    if (resolutions.Contains(resString) || vDevMode.dmPelsWidth > screen.Bounds.Width || vDevMode.dmPelsHeight > screen.Bounds.Height)
                    {
                        i++;
                        continue;
                    }

                    resolutions.Add(resString);
                    i++;
                }

                string currentScreenRes = $"{screen.Bounds.Width}X{screen.Bounds.Height}";

                resCmb.Items.AddRange(resolutions.ToArray());
                resCmb.SelectedItem = mergerRes == "" ? currentScreenRes : mergerRes;
                resLabel.Text += $" ({currentScreenRes})";
                float ratio = ((float)resLabel.DisplayRectangle.Height / (float)resCmb.DisplayRectangle.Height);
                resCmb.Font = new Font(this.Font.FontFamily, (Font.Size + ratio) * _scale, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                resCmb.Location = new Point(resLabel.Right + 4,resLabel.Top);

                resLabel.Location = new Point(resLabel.Left, resCmb.Location.Y);
                resLabel.Tag = screen.Bounds.Location;
                resLabel.Click += IdentifyScreen;

                screensResControlsRow.Add(new Point[] {new Point(resLabel.Left + 2,resLabel.Bottom+1),new Point(resLabel.Width-2,resLabel.Bottom+1) });

                AllScreensRes.Add(screen.DeviceName, resolutions);

                i = 0;
            }

            refreshScreensData = false;
            screen_panel.Visible = true;
        }

        private void IdentifyScreen(object sender, EventArgs e)
        {
            Label lb = sender as Label;
            ScreenId identifierWindow = new ScreenId((Point)lb.Tag);
            identifierWindow.Show();
        }

        private void SaveSelectedRes(object sender, EventArgs e)
        {
            FlatCombo cmb = sender as FlatCombo;
            selectedRes.Text = cmb.Text;
            App_Layouts.WindowsMergerRes = cmb.Text;
        }

        private void Settings_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
            {
                //Reload do not cache so we always have fresh screen datas.
                GetAllScreensResolutions();
                //Update in case the logmanager prompt updated the value
                debugLogCheck.Checked = App_Misc.DebugLog;
            }

            Core_Interface.DisableGameProfiles = disableGameProfiles.Checked;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void Settings_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void LosslessHook_CheckedChanged(object sender, EventArgs e)
        {
            CustomCheckBox checkBox = sender as CustomCheckBox;
            App_Layouts.LosslessHook = checkBox.Checked;
        }

        private void Screen_panel_Paint(object sender, PaintEventArgs e)
        {           
            if (!refreshScreensData)
                foreach(var points in screensResControlsRow)
                e.Graphics.DrawLines(bordersPen, points);
        }

        private bool refreshScreensData;

        private void RefreshScreenDatasButton_Click(object sender, EventArgs e)
        {
            refreshScreensData = true;
            GetAllScreensResolutions();
        }

        public void SetInvisible()
        {
            Visible = false;
        }

        public void SetVisible()
        {
            Visible = true;
        }
    }
}