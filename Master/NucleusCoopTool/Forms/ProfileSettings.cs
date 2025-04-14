using NAudio.CoreAudioApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.Controls;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.Tools.MonitorsDpiScaling;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nucleus.Coop
{

    public partial class ProfileSettings : Form, IDynamicSized
    {
        private MainForm mainForm => UI_Interface.MainForm;
        private static ProfileSettings profileSettings;

        private string currentNickname;
        private string currentSteamId;

        private List<Panel> tabs = new List<Panel>();
        private List<Control> tabsButtons = new List<Control>();
        private List<Control> ctrls = new List<Control>();

        private FlatCombo[] controllerNicks;
        private FlatCombo[] steamIds;
        private FlatCombo[] IdealProcessors;
        private FlatTextBox[] Affinitys;
        private FlatCombo[] PriorityClasses;
        private Control[] activeControls;

        private float fontSize;
        private float _scale;

        private IDictionary<string, string> audioDevices;

        private Color selectionColor;

        private Rectangle[] tabBorders;
        private List<Point[]> tabLines;

        private Pen bordersPen;

        private bool refreshScreensData;
        private bool shouldSwapNick = true;

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams handleparams = base.CreateParams;
                handleparams.ExStyle = 0x02000000;
                return handleparams;
            }
        }

        public ProfileSettings()
        {

            fontSize = float.Parse(Globals.ThemeConfigFile.IniReadValue("Font", "SettingsFontSize"));
            profileSettings = this;

            InitializeComponent();

            Cursor = Theme_Settings.Default_Cursor;

            var borderscolor = Globals.ThemeConfigFile.IniReadValue("Colors", "ProfileSettingsBorder").Split(',');
            selectionColor = Theme_Settings.SelectedBackColor;
            bordersPen = new Pen(Color.FromArgb(int.Parse(borderscolor[0]), int.Parse(borderscolor[1]), int.Parse(borderscolor[2])));

            BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "other_backgrounds.jpg");

            Controlscollect();

            foreach (Control c in ctrls)
            {
                if (c is CustomCheckBox || c is Label || c is CustomRadio)
                {
                    if (c.Name != "audioWarningLabel" && c.Name != "warningLabel" && c.Name != "modeLabel")
                        c.Font = new Font(Theme_Settings.CustomFont, fontSize, c.Font.Style, GraphicsUnit.Pixel, 0);

                    if (c is CustomCheckBox checkbox)
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
                    if (c.Name != "notes_text" && c.Name != "profileTitle")
                        c.Font = new Font(Theme_Settings.CustomFont, fontSize, c.Font.Style, GraphicsUnit.Pixel, 0);
                   
                    if (c is FlatCombo fCB)
                    {
                        fCB.FlatStyle = FlatStyle.Flat;
                        fCB.ForeColor = Color.White;
                        fCB.BackColor = Color.FromArgb(31, 34, 35);
                        fCB.BorderColor = Color.White;
                        fCB.ButtonColor = Color.FromArgb(selectionColor.R, selectionColor.G, selectionColor.B);
                    }

                    if (c is FlatTextBox fTB)
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

                if (c.Name != "sharedTab" && c.Name != "playersTab" && c.Name != "audioTab" &&
                    c.Name != "processorTab" && c.Name != "layoutTab" && c.Name != "layoutSizer" && c.Name != "notes_text" && c.Name != "screen_panel"
                    && !(c is Label) && !(c is FlatTextBox) && !(c is GroupBox) && !(c is Panel))
                {
                    c.Cursor = Theme_Settings.Hand_Cursor;
                }

                if (c.Name == "sharedTab" || c.Name == "playersTab" || c.Name == "audioTab" || c.Name == "processorTab" || c.Name == "layoutTab")
                {
                    c.BackColor = Color.Transparent;
                    tabs.Add(c as Panel);
                }

                if ((string)c.Tag == "sharedTab" || (string)c.Tag == "playersTab" || (string)c.Tag == "audioTab" || (string)c.Tag == "processorTab" || (string)c.Tag == "layoutTab")
                {
                    c.Click += TabsButtons_highlight;
                    tabsButtons.Add(c);
                }

                if (c is Button)
                {
                    Button isButton = c as Button;
                    isButton.FlatAppearance.BorderSize = 0;
                    isButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
                }

                if (c.Name.Contains("pauseBetweenInstanceLaunch_TxtBox") || c.Name.Contains("WindowsSetupTiming_TextBox"))
                {
                    c.KeyPress += Num_KeyPress;
                }

                if (c.Name.Contains("steamid") && c is FlatCombo)
                {
                    c.KeyPress += Num_KeyPress;
                    c.Click += Steamid_Click;
                }
            }

            ForeColor = Theme_Settings.ControlsForeColor;

            audioBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "audio.png");
            playersBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "players.png");
            sharedBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "shared.png");
            processorBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "processor.png");
            layoutBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "layout.png");
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
            audioRefresh.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "refresh.png");
            profile_info_btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "profile_info.png");
            btnNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page1.png");
            btnProcessorNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page1.png");
            refreshScreenDatasButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "refresh.png");

            audioBtnPicture.Click += AudioTabBtn_Click;

            btnNext.BackColor = Theme_Settings.ButtonsBackColor;
            btnProcessorNext.BackColor = Theme_Settings.ButtonsBackColor;

            audioRefresh.BackColor = Color.Transparent;

            modeLabel.Cursor = Theme_Settings.Default_Cursor;

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

            IdealProcessors = new FlatCombo[] {
                IdealProcessor1, IdealProcessor2, IdealProcessor3, IdealProcessor4, IdealProcessor5, IdealProcessor6, IdealProcessor7, IdealProcessor8,
                IdealProcessor9, IdealProcessor10, IdealProcessor11, IdealProcessor12, IdealProcessor13, IdealProcessor14, IdealProcessor15,IdealProcessor16,
                IdealProcessor17, IdealProcessor18, IdealProcessor19, IdealProcessor20, IdealProcessor21, IdealProcessor22, IdealProcessor23, IdealProcessor24,
                IdealProcessor25, IdealProcessor26, IdealProcessor27, IdealProcessor28, IdealProcessor29, IdealProcessor30, IdealProcessor31, IdealProcessor32};

            Affinitys = new FlatTextBox[] {
                Affinity1, Affinity2, Affinity3, Affinity4, Affinity5, Affinity6, Affinity7, Affinity8,
                Affinity9, Affinity10, Affinity11, Affinity12, Affinity13, Affinity14, Affinity15,Affinity16,
                Affinity17, Affinity18, Affinity19, Affinity20, Affinity21, Affinity22, Affinity23, Affinity24,
                Affinity25, Affinity26, Affinity27, Affinity28, Affinity29, Affinity30, Affinity31, Affinity32};

            PriorityClasses = new FlatCombo[] {
                PriorityClass1, PriorityClass2, PriorityClass3, PriorityClass4, PriorityClass5, PriorityClass6, PriorityClass7, PriorityClass8,
                PriorityClass9, PriorityClass10, PriorityClass11,PriorityClass12, PriorityClass13, PriorityClass14, PriorityClass15,PriorityClass16,
                PriorityClass17, PriorityClass18, PriorityClass19, PriorityClass20, PriorityClass21, PriorityClass22, PriorityClass23, PriorityClass24,
                PriorityClass25, PriorityClass26, PriorityClass27, PriorityClass28, PriorityClass29, PriorityClass30, PriorityClass31, PriorityClass32};


            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                var nickField = controllerNicks[i];
                nickField.TextChanged += SwapNickname;
                nickField.KeyPress += CheckTypingNick;
                nickField.MouseHover += CacheNickname;
                nickField.LostFocus += UpdateControllerNickItems;

                var sidField = steamIds[i];
                sidField.TextChanged += SwapSteamId;
                sidField.MouseHover += CacheSteamId;
                sidField.LostFocus += UpdateSteamIdsItems;
            }

            for (int p = 0; p < Environment.ProcessorCount; p++)
            {
                for (int all = 0; all < IdealProcessors.Length; all++)
                {
                    var idealField = IdealProcessors[all];
                    idealField.DropDownStyle = ComboBoxStyle.DropDownList;

                    if (p == 0)
                    {
                        idealField.Items.Add("*");
                        idealField.SelectedIndex = 0;
                    }

                    idealField.Items.Add((idealField.Items.Count).ToString());
                    idealField.Click += IdealProcessor_Click;
                    idealField.SelectedIndexChanged += IdealProcessor_Click;
                    idealField.KeyPress += Affinity_KeyPress;

                    var affinityField = Affinitys[all];
                    affinityField.Text = "";
                    affinityField.Click += Affinity_Click;
                    affinityField.KeyPress += Affinity_KeyPress;
                }
            }

            coreCountLabel.Text = $"Cores/Threads {Environment.ProcessorCount} (Max Value)";
            coreCountLabel.ForeColor = Color.Yellow;

            foreach (KeyValuePair<string, System.Windows.Media.SolidColorBrush> color in BackgroundColors.ColorsDictionnary)
            {
                SplitColors.Items.Add(color.Key);
            }

            foreach (FlatCombo pClass in PriorityClasses)
            {
                pClass.DropDownStyle = ComboBoxStyle.DropDownList;
                pClass.SelectedIndex = 0;
                pClass.KeyPress += ReadOnly_KeyPress;
            }

            ///network setting
            RefreshCmbNetwork();

            sharedTab.Parent = this;
            sharedTab.Location = new Point(0, sharedTabBtn.Bottom);

            playersTab.Parent = this;
            playersTab.Location = sharedTab.Location;

            audioTab.Parent = this;
            audioTab.Location = sharedTab.Location;

            processorTab.Parent = this;
            processorTab.Location = sharedTab.Location;

            layoutTab.Parent = this;
            layoutTab.Location = sharedTab.Location;

            page1.Parent = playersTab;
            page1.Location = new Point(playersTab.Width / 2 - page1.Width / 2, playersTab.Height / 2 - page1.Height / 2);
            page1.BringToFront();

            page2.Parent = playersTab;
            page2.Location = page1.Location;

            btnNext.Parent = playersTab;
            btnNext.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnNext.Location = new Point(page1.Right - btnNext.Width, (page1.Top - btnNext.Height) - 5);

            processorPage1.Parent = processorTab;
            processorPage1.Location = new Point(processorTab.Width / 2 - processorPage1.Width / 2, processorTab.Height / 2 - processorPage1.Height / 2);
            processorPage1.BringToFront();

            processorPage2.Parent = processorTab;
            processorPage2.Location = processorPage1.Location;

            btnProcessorNext.Parent = processorTab;
            btnProcessorNext.FlatAppearance.MouseOverBackColor = Color.Transparent;
            btnProcessorNext.Location = new Point(processorPage1.Right - btnProcessorNext.Width, (processorPage1.Top - btnProcessorNext.Height) - 5);

            sharedTab.BringToFront();

            activeControls = new Control[] {sharedTabBtn,sharedBtnPicture };

            Vector2 dist1 = Vector2.Subtract(new Vector2(profile_info_btn.Location.X, profile_info_btn.Location.Y), new Vector2(processorBtnPicture.Location.X, processorBtnPicture.Location.Y));
            float modeLabelX = (processorBtnPicture.Right + (dist1.X / 2)) - modeLabel.Width / 2;

            modeLabel.Location = new Point((int)modeLabelX, sharedTabBtn.Location.Y + sharedTabBtn.Height / 2 - modeLabel.Height / 2);
            audioRefresh.Location = new Point((audioTab.Width / 2) - (audioRefresh.Width / 2), audioRefresh.Location.Y);
            warningLabel.ForeColor = Color.Yellow;

            numUpDownVer.MaxValue = 5;
            numUpDownVer.InvalidParent = true;

            numUpDownHor.MaxValue = 5;
            numUpDownHor.InvalidParent = true;

            RefreshAudioList();

            GetPlayersNickNameAndIds();

            Rectangle area = Screen.PrimaryScreen.Bounds;
            if (App_Misc.ProfileSettingsLocation != "")
            {
                string[] windowLocation = App_Misc.ProfileSettingsLocation.Split('X');

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

            if (scale > 1.0F)
            {
                float newFontSize = Font.Size * scale;

                foreach (Control c in ctrls)
                {
                    if (c is FlatCombo || c is FlatTextBox || c is GroupBox)
                    {
                        c.Font = new Font(c.Font.FontFamily, c is FlatTextBox ? newFontSize + 3 : newFontSize, c.Font.Style, GraphicsUnit.Pixel, 0);
                    }
                }

                notes_text.Size = new Size((int)(260 * scale), (int)(81 * scale));
            }

            Vector2 dist1 = Vector2.Subtract(new Vector2(profile_info_btn.Location.X, profile_info_btn.Location.Y), new Vector2(layoutBtnPicture.Location.X, layoutBtnPicture.Location.Y));
            float modeLabelX = (layoutBtnPicture.Right + (dist1.X / 2)) - modeLabel.Width / 2;

            modeLabel.Location = new Point((int)modeLabelX, sharedTabBtn.Location.Y + sharedTabBtn.Height / 2 - modeLabel.Height / 2);
            audioRefresh.Location = new Point((audioTab.Width / 2) - (audioRefresh.Width / 2), audioRefresh.Location.Y);
            warningLabel.Location = new Point(sharedTab.Width / 2 - warningLabel.Width / 2, warningLabel.Location.Y);
            audioWarningLabel.Location = new Point(audioTab.Width / 2 - audioWarningLabel.Width / 2, audioWarningLabel.Location.Y);

            int tabButtonsY = sharedTabBtn.Location.Y - 1;
            int tabButtonsHeight = sharedTabBtn.Height + 1;

           tabBorders = new Rectangle[]
           {
               new Rectangle(0,tabButtonsY,layoutBtnPicture.Right + 1,tabButtonsHeight),
               new Rectangle(sharedTab.Location.X, sharedTab.Location.Y, sharedTab.Width - 1, sharedTab.Height),
           };
             
            tabLines = new List<Point[]>()
            {  new Point[]{ new Point(sharedBtnPicture.Right, tabButtonsY + 1),new Point(sharedBtnPicture.Right, sharedTab.Location.Y) },
               new Point[]{ new Point(playersBtnPicture.Right, tabButtonsY + 1),new Point(playersBtnPicture.Right, sharedTab.Location.Y) },
               new Point[]{ new Point(processorBtnPicture.Right, tabButtonsY + 1),new Point(processorBtnPicture.Right, sharedTab.Location.Y) },
               new Point[]{ new Point(audioBtnPicture.Right, tabButtonsY + 1),new Point(audioBtnPicture.Right, sharedTab.Location.Y) },
            };
        }

        private void SetToolTips()
        {
            CustomToolTips.SetToolTip(autoPlay, "Automatically launch game instances on profile selection.", "autoPlay_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(WindowsSetupTiming_TextBox, "Speedup windows resizing and positioning (1000ms is fine in most cases).\n" +
                                                                  "Could break hooks or xinputplus for some games.", "WindowsSetupTiming_TextBox_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(pauseBetweenInstanceLaunch_TxtBox, "Time to wait before starting the next game instance.\n" +
                                                                         "Could break positioning/resizing for some games.", "pauseBetweenInstanceLaunch_TxtBox_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(splitDiv, "May not work for all games.", "splitDiv_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(hideDesktop, "Will only show the splitscreen division window without adjusting the game windows size and offset.", "hideDesktop_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(refreshScreenDatasButton, "Refresh screens info.", "refreshScreenDatasButton_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(enable_WMerger, "Game windows will be merged to a single window\n" +
                                                           "so upscaling tools such as Lossless Scaling\n" +
                                                           "can be used with Nucleus. Note that there's no multiple monitor support yet.", "enable_WMerger_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            CustomToolTips.SetToolTip(losslessHook, "Lossless will not stop upscaling if an other window get the focus, useful\n" +
                                                    "if game windows requires real focus to receive inputs.", "losslessHook_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
            CustomToolTips.SetToolTip(refreshScreenDatasButton, "Refresh screens info.", "refreshScreenDatasButton_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
        }

        private void GetPlayersNickNameAndIds()
        {
            List<PlayerInfo> playersList = GameProfile.ProfilePlayersList;

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                var nickField = controllerNicks[i];
                var sidField = steamIds[i];

                if (i < playersList.Count)
                {
                   PlayerInfo player = playersList[i];

                    sidField.Items.Clear();
                    sidField.Items.AddRange(PlayersIdentityCache.PlayersSteamId.Where(sid => sid != "").ToArray());
                    sidField.Items.AddRange(PlayersIdentityCache.SteamIdsBackup.Where(sid => sid != player.SteamID.ToString()).ToArray());

                    sidField.Text =  player.SteamID.ToString() != "-1" ? player.SteamID.ToString() : "";
                    sidField.SelectedItem =  player.SteamID.ToString() != "-1" ? player.SteamID.ToString() : "";
                    sidField.Enabled = true;

                    nickField.Items.Clear();
                    nickField.Items.AddRange(PlayersIdentityCache.PlayersNickname.Where(nic => nic != player.Nickname).ToArray());
                    nickField.Items.AddRange(PlayersIdentityCache.NicknamesBackup.ToArray());

                    nickField.SelectedItem = player.Nickname;
                    nickField.Text = player.Nickname.ToString();
                    nickField.Enabled = true;

                    if (GameProfile.Game?.UseHandlerSteamIds == true)//UseHandlerSteamIds force the use of Game.PlayerSteamIDs so lock them here
                    {
                        if (i < GameProfile.Game.PlayerSteamIDs.Count())
                        {
                            sidField.Text = GameProfile.Game.PlayerSteamIDs[i];
                            sidField.SelectedItem = sidField.Text;
                            sidField.Enabled = false;
                        }
                    }
                }
                else
                {
                    sidField.Items.Clear();
                    sidField.Items.AddRange(PlayersIdentityCache.PlayersSteamId.Where(sid => sid != "").ToArray());
                    sidField.Items.AddRange(PlayersIdentityCache.SteamIdsBackup.Where(sid => !sidField.Items.Contains(sid)).ToArray());

                    sidField.Text = GameProfile.IsNewProfile ? PlayersIdentityCache.GetSteamIdAt(i) : "";
                    sidField.SelectedItem = GameProfile.IsNewProfile ? PlayersIdentityCache.GetSteamIdAt(i) : "";
                    sidField.Enabled = false;
                    
                    nickField.Items.Clear();
                    nickField.Items.AddRange(PlayersIdentityCache.PlayersNickname.Where(nic => !nickField.Items.Contains(nic)).ToArray());
                    nickField.Items.AddRange(PlayersIdentityCache.NicknamesBackup.ToArray());

                    nickField.Text = GameProfile.IsNewProfile ? PlayersIdentityCache.GetNicknameAt(i) : "";
                    nickField.SelectedItem = GameProfile.IsNewProfile ? nickField.Text : "";
                    nickField.Enabled = false;

                    if (GameProfile.IsNewProfile)
                    {
                        sidField.Enabled = true;
                        nickField.Enabled = true;
                    }

                    if (GameProfile.Game?.UseHandlerSteamIds == true && GameProfile.IsNewProfile)//UseHandlerSteamIds force the use of Game.PlayerSteamIDs so lock them here
                    {
                        sidField.Text = GameProfile.Game.PlayerSteamIDs[i];
                        sidField.SelectedItem = sidField.Text;
                        sidField.Enabled = false;
                    }
                }

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

        public static void UpdateProfileSettingsUiValues()
        {
            profileSettings?.UpdateUiValues();
        }

        private void UpdateUiValues()
        {
            GetPlayersNickNameAndIds();
            GetAllScreensResolutions();

            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                PlayerInfo player = null;

                if (i < GameProfile.ProfilePlayersList.Count)
                {
                    player = GameProfile.ProfilePlayersList[i];
                }

                var idealField = IdealProcessors[i];
                var affinityField = Affinitys[i];
                var priorityField = PriorityClasses[i];

                if (player != null)
                {
                    if (player.IdealProcessor != null)
                    {
                        idealField.SelectedIndex = idealField.Items.IndexOf(player.IdealProcessor);
                    }
                    else
                    {
                        idealField.SelectedIndex = 0;
                    }

                    if (player.Affinity != null)
                    {
                        affinityField.Text = player.Affinity;
                    }
                    else
                    {
                        affinityField.Text = "";
                    }

                    if (player.PriorityClass != null)
                    {
                        priorityField.SelectedIndex = priorityField.Items.IndexOf(player.PriorityClass);
                    }
                    else
                    {
                        player.PriorityClass = "Normal";
                        priorityField.SelectedIndex = 0;
                    }

                    idealField.Enabled = true;
                    affinityField.Enabled = true;
                    priorityField.Enabled = true;
                }
                else
                {
                    idealField.Text = "*";
                    idealField.Enabled = false;

                    affinityField.Text = "";
                    affinityField.Enabled = false;

                    priorityField.Text = "Normal";
                    priorityField.Enabled = false;

                    if (GameProfile.IsNewProfile)
                    {
                        idealField.Enabled = true;
                        affinityField.Enabled = true;
                        priorityField.Enabled = true;
                    }
                }
            }

            RefreshAudioList();

            audioDefaultSettingsRadio.Checked = GameProfile.AudioDefaultSettings;
            audioCustomSettingsRadio.Checked = GameProfile.AudioCustomSettings;
            audioCustomSettingsBox.Enabled = audioCustomSettingsRadio.Checked;

            autoPlay.Checked = GameProfile.AutoPlay;
            pauseBetweenInstanceLaunch_TxtBox.Text = GameProfile.PauseBetweenInstanceLaunch.ToString();
            WindowsSetupTiming_TextBox.Text = GameProfile.HWndInterval.ToString();
            splitDiv.Checked = GameProfile.UseSplitDiv;
            hideDesktop.Checked = GameProfile.HideDesktopOnly;
            SplitColors.Text = GameProfile.SplitDivColor;
            scaleOptionCbx.Checked = GameProfile.AutoDesktopScaling;
            useNicksCheck.Checked = GameProfile.UseNicknames;
            cmb_Network.Text = GameProfile.Network;
            numUpDownVer.Value = GameProfile.CustomLayout_Ver;
            numUpDownHor.Value = GameProfile.CustomLayout_Hor;
            numMaxPlyrs.Value = GameProfile.CustomLayout_Max;
            cts_Mute.Checked = GameProfile.Cts_MuteAudioOnly;
            cts_kar.Checked = GameProfile.Cts_KeepAspectRatio;
            cts_unfocus.Checked = GameProfile.Cts_Unfocus;
            notes_text.Text = GameProfile.Notes;
            profileTitle.Text = GameProfile.Title;
            enable_WMerger.Checked = GameProfile.EnableWindowsMerger;
            losslessHook.Checked = GameProfile.EnableLosslessHook;
            selectedRes.Text = GameProfile.MergerResolution;

            if (GameProfile.Instance != null)
            {
                modeLabel.Text = GameProfile.ModeText;
            }
        }

        private void CloseBtnPicture_Click(object sender, EventArgs e)///Set GameProfile values
        {
            NucleusMessageBox.MessageBox?.Dispose();
            
            if (!Directory.Exists(Globals.GameProfilesFolder))
            {
                Directory.CreateDirectory(Globals.GameProfilesFolder);
            }

            bool sidWrongValue = false;
            bool IdealProcessorsWrongValue = false;
            bool affinitysWrongValue = false;
            bool hasEmptyNickname = false;

            if (GameProfile.ProfilePlayersList.Count == 0)
            {
                //Create profile players for new game profile
                for (int i = 0; i < GameProfile.Game.MaxPlayers; i++)
                {
                    PlayerInfo player = new PlayerInfo();
                    GameProfile.ProfilePlayersList.Add(player);
                }
            }

            for (int i = 0; i < GameProfile.ProfilePlayersList.Count; i++)
            {
                PlayerInfo  player = GameProfile.ProfilePlayersList[i];

                if(i >= Globals.NucleusMaxPlayers)//in case the handler supports more than the Nucleus max player value
                {
                    break;
                }

                //Set GameProfile Nicknames
                var nickField = controllerNicks[i];
                if (nickField.Text != "")
                {             
                    player.Nickname = nickField.Text;
                    PlayersIdentityCache.AddNicknameToCache(nickField.Text);
                }
                else
                {
                    hasEmptyNickname = true;
                }

                //Set GameProfile Steam ids
                var sidField = steamIds[i];
                if ((Regex.IsMatch(sidField.Text, "^[0-9]+$") && sidField.Text.Length == 17 || sidField.Text.Length == 0) || sidField.Text == "-1" )
                {
                    FlatCombo hasSameText = steamIds.Where(cb => cb != sidField &&  cb.Text != "" && sidField.Text != "" && cb.Text == sidField.Text).FirstOrDefault();

                    if (hasSameText != null)
                    {
                        sidField.BackColor = Color.Red;
                        hasSameText.BackColor = Color.Red;
                        sidWrongValue = true;
                        break;
                    }

                    if (sidField.Text != "")
                    {
                        player.SteamID = long.Parse(sidField.Text.ToString());

                        if(GameProfile.Game.PlayerSteamIDs != null)
                        {
                            if (GameProfile.Game.PlayerSteamIDs.All(sid => sid != sidField.Text))
                            {
                                PlayersIdentityCache.AddSteamIdToCache(sidField.Text);
                            }
                        }
                        else
                        {
                            PlayersIdentityCache.AddSteamIdToCache(sidField.Text);
                        }                                     
                    }
                    else
                    {
                        player.SteamID = PlayersIdentityCache.DefaultSteamIds[i];
                    }
                }
                else
                {
                    //Invalid Steam id detected
                    sidField.BackColor = Color.Red;
                    sidWrongValue = true;
                    break;
                }

                //Set GameProfile Ideal Processors
                var idealField = IdealProcessors[i];
                if (idealField.Text == "" || idealField.Text == null)
                {
                    idealField.Text = "*";
                }

                int wrongValue = Environment.ProcessorCount;

                if (idealField.Text != "*")
                {
                    if (int.Parse(idealField.Text) > wrongValue)
                    {
                        //Invalid Ideal Processor detected
                        idealField.BackColor = Color.Red;
                        IdealProcessorsWrongValue = true;
                        break;
                    }
                }

                player.IdealProcessor = idealField.Text;

                //Set GameProfile Processors Affinity
                var affinityField = Affinitys[i];
                if (affinityField.Text == null)
                {
                    affinityField.Text = "";
                }

                int maxValue = Environment.ProcessorCount;

                string[] values = affinityField.Text.Split(',');
                foreach (string val in values)
                {
                    if (val != "")
                    {
                        if (int.Parse(val) > maxValue)
                        {
                            //Invalid affinity detected
                            affinityField.BackColor = Color.Red;
                            affinitysWrongValue = true;
                            break;
                        }
                    }
                }

                player.Affinity = affinityField.Text;

                //Set GameProfile Priority Classes
                var priorityField = PriorityClasses[i];
                if (priorityField.Text == "" || priorityField.Text == null)
                {
                    priorityField.Text = "Normal";
                }

                player.PriorityClass = priorityField.Text;
            }

            ///Warn user for empty nickame fields
            if (hasEmptyNickname)
            {
                playersTab.BringToFront();
                MessageBox.Show("Nickname fields can't be empty!");
                return;
            }

            ///Warn user for invalid Steam ids
            if (sidWrongValue)
            {
                playersTab.BringToFront();
                MessageBox.Show("Must be 17 numbers e.g. \"76561199075562883\" and be different for each player.", "Incorrect steam id format!");
                return;
            }

            ///Warn user for invalid ideal processors 
            if (IdealProcessorsWrongValue)
            {
                processorTab.BringToFront();
                MessageBox.Show("This PC do not have so much cores ;)", "Invalid CPU core value!");
                return;
            }

            ///Warn user for invalid processors affinity
            if (affinitysWrongValue)
            {
                processorTab.BringToFront();
                MessageBox.Show("This PC do not have so much cores ;)", "Invalid CPU core value!");
                return;
            }

            PlayersIdentityCache.BackupNicknames();
            PlayersIdentityCache.BackupSteamIds();

            ///Set shared GameProfile options
            GameProfile.Title = profileTitle.Text;
            GameProfile.Notes = notes_text.Text;
            GameProfile.AutoPlay = autoPlay.Checked;
            GameProfile.AudioDefaultSettings = audioDefaultSettingsRadio.Checked;
            GameProfile.AudioCustomSettings = audioCustomSettingsRadio.Checked;
            GameProfile.Network = cmb_Network.Text;
            GameProfile.CustomLayout_Ver = (int)numUpDownVer.Value;
            GameProfile.CustomLayout_Hor = (int)numUpDownHor.Value;
            GameProfile.CustomLayout_Max = (int)numMaxPlyrs.Value;
            GameProfile.UseNicknames = useNicksCheck.Checked;
            GameProfile.UseSplitDiv = splitDiv.Checked;
            GameProfile.HideDesktopOnly = hideDesktop.Checked;
            GameProfile.SplitDivColor = SplitColors.Text;
            GameProfile.AutoDesktopScaling = scaleOptionCbx.Checked;
            GameProfile.PauseBetweenInstanceLaunch = pauseBetweenInstanceLaunch_TxtBox.Text == "" ? 0 : int.Parse(pauseBetweenInstanceLaunch_TxtBox.Text);
            GameProfile.HWndInterval = WindowsSetupTiming_TextBox.Text == "" ? 0 : int.Parse(WindowsSetupTiming_TextBox.Text);
            GameProfile.AudioInstances.Clear();
            GameProfile.Cts_MuteAudioOnly = cts_Mute.Checked;
            GameProfile.Cts_KeepAspectRatio = cts_kar.Checked;
            GameProfile.Cts_Unfocus = cts_unfocus.Checked;
            GameProfile.EnableWindowsMerger = enable_WMerger.Checked;
            GameProfile.EnableLosslessHook = losslessHook.Checked;
            GameProfile.MergerResolution = selectedRes.Text;

            ///Set GameProfile AudioInstances (part of shared GameProfile options)
            foreach (Control ctrl in audioCustomSettingsBox.Controls)
            {
                if (ctrl is FlatCombo cmb)
                {
                    if (audioDevices?.Count > 0 && audioDevices.Keys.Contains(cmb.Text))
                    {
                        GameProfile.AudioInstances.Add(cmb.Name, audioDevices[cmb.Text]);
                    }
                }
            }

            ///Unlock profiles list on close          
            ProfilesList.Instance.Locked = false;

            ///Update profile[#].json file
            if (!GameProfile.IsNewProfile)
            {
                GameProfile.UpdateGameProfile(GameProfile.Instance);
                ProfilesList.Instance.Update_ProfilesList();
                ProfilesList.Instance.Update_Reload();
            }

            App_Misc.ProfileSettingsLocation = Location.X + "X" + Location.Y;

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

        public static void ProfileRefreshAudioList()
        {
            profileSettings?.RefreshAudioList();
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
                    cmb.Items.Clear();
                    cmb.Items.AddRange(audioDevices.Keys.ToArray());

                    if (GameProfile.IsNewProfile)
                    {
                        if (GameProfile.AudioInstances.Any(device => device.Key == cmb.Name))
                        {
                            if (audioDevices.Values.Contains(GameProfile.AudioInstances[cmb.Name]))
                            {
                                cmb.SelectedItem = audioDevices.FirstOrDefault(x => x.Value == GameProfile.AudioInstances[cmb.Name]).Key;
                            }
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
                    else
                    {
                        if (GameProfile.AudioInstances.Count > 0)
                        {
                            if (GameProfile.AudioInstances.Any(device => device.Key == cmb.Name))
                            {
                                if (audioDevices.Values.Contains(GameProfile.AudioInstances[cmb.Name]))
                                {
                                    cmb.SelectedItem = audioDevices.FirstOrDefault(x => x.Value == GameProfile.AudioInstances[cmb.Name]).Key;
                                }
                            }
                            else
                            {
                                cmb.SelectedItem = "Default";
                            }
                        }
                        else
                        {
                            cmb.SelectedItem = "Default";
                        }
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

        private void AudioTabBtn_Click(object sender, EventArgs e)
        {
            MMDeviceEnumerator enumerator = new MMDeviceEnumerator();
            MMDevice audioDefault = enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console);
            audioDefaultDevice.Text = "Default: " + audioDefault.FriendlyName;
        }

        private void IdealProcessor_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                IdealProcessors[i].BackColor = Color.FromArgb(255, 31, 34, 35);
                Affinitys[i].BackColor = Color.Gray;
                Affinitys[i].Text = "";
            }
        }

        private void Affinity_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < Globals.NucleusMaxPlayers; i++)
            {
                Affinitys[i].BackColor = Color.FromArgb(255, 31, 34, 35);
                IdealProcessors[i].BackColor = Color.Gray;
                IdealProcessors[i].SelectedIndex = 0;
            }
        }

        private void Num_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void Affinity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.') && (e.KeyChar != ','))
            {
                e.Handled = true;
            }
        }

        private void ReadOnly_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = true;
        }

        private void CloseBtnPicture_MouseEnter(object sender, EventArgs e)
        {
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        }

        private void CloseBtnPicture_MouseLeave(object sender, EventArgs e)
        {
            closeBtnPicture.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
        }

        private void Profile_info_btn_MouseEnter(object sender, EventArgs e)
        {
            profile_info_btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "profile_info_mousehover.png");
        }

        private void Profile_info_btn_MouseLeave(object sender, EventArgs e)
        {
            profile_info_btn.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "profile_info.png");
        }

        private void Profile_info_btn_Click(object sender, EventArgs e)
        {
            NucleusMessageBox.Show(
                "Info", 
                "- Profiles will be saved after all instances have finished setting up correctly.\n\n" +
                "- To load a profile click the profile list icon in the game info panel (Right) and choose the\n" +
                "  profile to load by clicking on it in the list.\n\n" +
                "- Profile settings can be modified before pressing the play button.\n\n" +
                "- Some of the profile settings could break hooks, resizing or positioning\n" +
                "  functions so it's better to launch the game with the default profile settings once.", false);
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

        private void BtnProcessorNext_Click_1(object sender, EventArgs e)
        {
            if (processorPage1.Visible)
            {
                btnProcessorNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page2.png");
                processorPage1.Visible = false;
                processorPage2.BringToFront();
                processorPage2.Visible = true;
            }
            else
            {
                btnProcessorNext.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "page1.png");
                processorPage2.Visible = false;
                processorPage1.BringToFront();
                processorPage1.Visible = true;
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
            gs.FillRectangle(backBr, outline);
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

        private void ProfileSettings_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var _activeControls = activeControls.OrderBy(c => c is PictureBox).ToArray();

            SolidBrush lineBrush = new SolidBrush( Color.FromArgb(200,selectionColor.R,selectionColor.G,selectionColor.B) );
            Rectangle butt = new Rectangle(_activeControls[0].Left, _activeControls[0].Bottom + 2, _activeControls[1].Right - _activeControls[0].Left, 3);

            g.FillRectangle(lineBrush, butt);
            g.DrawRectangles(bordersPen, tabBorders);
            lineBrush.Dispose();

            foreach (var points in tabLines)
                e.Graphics.DrawLines(bordersPen, points);
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
        }

        private void SwapSteamId(object sender, EventArgs e)
        {
            FlatCombo cb = sender as FlatCombo;

            if (cb.Text == "")
            {
                return;
            }

            FlatCombo ch = steamIds.Where(tb => tb.Text == cb.Text && tb != cb).FirstOrDefault();

            if (ch?.Text == "" || ch?.Text == null)
            {
                return;
            }

            if (ch != null)
            {
                if (currentSteamId != null)
                {
                    ch.Text = currentSteamId;
                }

                currentSteamId = cb.Text;
            }
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
                var nickField = controllerNicks[i];
                if (!nickField.Items.Contains(cb.Text))
                {
                    nickField.Items.Add(cb.Text);
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
                var sidField = steamIds[i];
                if (!sidField.Items.Contains(cb.Text))
                {
                    sidField.Items.Add(cb.Text);
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

            foreach (Screen screen in Screen.AllScreens)
            {
                string mergerRes = GameProfile.MergerResolution;

                if (mergerRes == "")
                {
                    if (screen.Primary)
                    {
                        string newMergerRes = $"{screen.Bounds.Width}X{screen.Bounds.Height}";
                        GameProfile.MergerResolution = newMergerRes;
                        selectedRes.Text = newMergerRes;
                    }
                }
                else
                {
                    selectedRes.Text = GameProfile.MergerResolution;
                }

                FlatCombo resCmb = new FlatCombo();
                resCmb.BackColor = Color.FromArgb(255, 31, 34, 35);
                resCmb.ForeColor = Color.White;
                resCmb.FlatStyle = FlatStyle.Flat;
                resCmb.BorderColor = Color.White;
                resCmb.ButtonColor = Color.FromArgb(255, 31, 34, 35);
                resCmb.SelectedValueChanged += SaveSelectedRes;
                resCmb.DropDownStyle = ComboBoxStyle.DropDownList;
                resCmb.Cursor = Theme_Settings.Hand_Cursor;

                Label resLabel = new Label();
                resLabel.AutoSize = true;
                string cleanName = screen.DeviceName.Substring(screen.DeviceName.LastIndexOf('\\') + 1);
                resLabel.Text = $"⛶ {cleanName}";
                resLabel.Cursor = Theme_Settings.Hand_Cursor;

                CustomToolTips.SetToolTip(resLabel, "Click here to identify the screen", "resLabel_PRST", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

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
                    resLabel.Location = new Point(screensLabels[screensLabels.Count - 1].Location.X, screensLabels[screensLabels.Count - 1].Bottom + 4);
                    screensLabels.Add(resLabel);
                    resLabel.Refresh();
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
                resCmb.Location = new Point(resLabel.Right + 4, resLabel.Top);

                resLabel.Location = new Point(resLabel.Left, resCmb.Location.Y);
                resLabel.Tag = screen.Bounds.Location;
                resLabel.Click += IdentifyScreen;

                screensResControlsRow.Add(new Point[] { new Point(resLabel.Left + 2, resLabel.Bottom + 1), new Point(resLabel.Width - 2, resLabel.Bottom + 1) });

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
            GameProfile.MergerResolution = cmb.Text;
        }

        private const int WM_NCLBUTTONDOWN = 0xA1;
        private const int HT_CAPTION = 0x2;

        private void ProfileSettings_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void ModeLabel_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void Screen_panel_Paint(object sender, PaintEventArgs e)
        {
            if (!refreshScreensData)
                foreach (var points in screensResControlsRow)
                    e.Graphics.DrawLines(bordersPen, points);
        }

        private void RefreshScreenDatasButton_Click(object sender, EventArgs e)
        {
            refreshScreensData = true;
            GetAllScreensResolutions();
        }

        private void StopUINav_CheckedChanged(object sender, EventArgs e)
        {
            GameProfile.Stop_UINav = stopUINav.Checked;
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