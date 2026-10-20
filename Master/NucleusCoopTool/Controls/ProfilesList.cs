using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.Forms;
using Nucleus.Coop.UI;
using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Nucleus.Coop.Controls
{
    public partial class ProfilesList : ControlListBox
    {
        private float _scale => (float)User32Util.GetDpiForWindow(this.Handle) / (float)100;
        private float scale
        {
            get
            {
                if(_scale < 1.0f)
                {
                    return 1.0f;
                }
                else
                {
                    return _scale;
                }
            }
        }

        public static ProfilesList Instance;

        public static Action UIAction_OnUnload_Click;
        public static Action<string> UIAction_OnShowPreview;

        public bool Locked = false;

        private Color foreColor;
        public static readonly string PartialTitle = "Load profile:";

        private Color backGradient;

        private Control parentControl;
        private bool useGradient;
        private MouseEventArgs eventArgs;

        private Font titleFont;
        private Font previewFont;
        private Font unloadFont;

        private Button unloadButton;

        public ProfilesList(Control parent)
        {
            parentControl = parent;

            InitializeComponent();

            Name = "ProfilesList";
            Size = new Size(300,1);
            Location = new Point(0, 0);
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Visible = false;
            BorderStyle = BorderStyle.FixedSingle;

            foreColor = Theme_Settings.ControlsForeColor;          
            BackColor = Color.FromArgb(255,Theme_Settings.WindowPanelBackColor.R, Theme_Settings.WindowPanelBackColor.G, Theme_Settings.WindowPanelBackColor.B);

            VisibleChanged += On_VisibleChanged;
     
            eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

            GameProfile.OnReset -= Update_Unload;
            GameProfile.OnReset += Update_Unload;

            unloadButton = new Button();
            unloadButton.Name = "unloadButton";
            unloadButton.TextAlign = ContentAlignment.MiddleCenter;
            unloadButton.ForeColor =  Theme_Settings.ControlsForeColor; 
            unloadButton.BackColor = Theme_Settings.ButtonsBackColor;// this.BackColor;
            unloadButton.FlatAppearance.MouseOverBackColor = Theme_Settings.MouseOverBackColor;
            unloadButton.FlatAppearance.BorderColor = Theme_Settings.ButtonsBorderColor;
            unloadButton.FlatAppearance.BorderSize = 1;
            unloadButton.Text = "Unload Profile";
            unloadButton.FlatStyle = FlatStyle.Flat;
            unloadButton.Cursor = Theme_Settings.Hand_Cursor;
            unloadButton.Click += UnloadButton_Click;
            unloadButton.KeyPress += UnloadButton_Click;
            UI_Interface.ProfileButtonsPanel.Controls.Add(unloadButton);
            UI_Interface.UnloadProfileButton = unloadButton;

            Visible = false;

            Instance = this;
        }

        private void On_VisibleChanged(object sender,EventArgs e)
        {
            Control list = sender as Control;
            
            if(GameProfile.Loaded && Core_Interface.CurrentStepIndex != Core_Interface.StepsList.Count)
            {
                unloadButton.Visible = true;
                BringToFront();
            }
            else
            {
                unloadButton.Visible = false;
            }
        }

        private void UnloadButton_Click(object sender, EventArgs e)
        {
            //we don't want the profile to be unloaded while profile settings opened
            if (Locked)
            {
                //So we bring the profile settings window to front
                Generic_Functions.ClickAnyControl(null, null);
                return;
            }

            unloadButton.Visible = false;
            GameProfile.Instance.Reset();
            Globals.MainOSD.Show(500, $"Handler Profile Settings *@{OSDHighlightColors.Confirm}|Unloaded@*");
        }
   
        private void UpdateUnloadButton(bool visible)
        {
            unloadButton.Font = unloadFont;
            
            Control profBtn = UI_Interface.ProfileSettingsButton;
            unloadButton.Size = new Size((int)(100 * scale), profBtn.Height);
            unloadButton.Location = new Point(profBtn.Right + 5, profBtn.Top + (profBtn.Height/2) - unloadButton.Height/2);
            unloadButton.ForeColor = Color.White;
            unloadButton.BringToFront();
            unloadButton.Visible = visible;
        }

        public void Update_Reload()
        {
            int index = int.Parse(Regex.Match(GameProfile.ModeText, @"\d+").Value);
            
            string[] texts = GetControlsText(index - 1);
            string name = index.ToString();

            Label dummy = new Label
            {
                Name = name,
                Text = $"{PartialTitle} {name}",
                Tag = texts[1]
            };

            ProfileBtn_CheckedChanged(dummy, eventArgs);
            ScrollControlIntoView(Controls.Find(name,true).FirstOrDefault());

            dummy.Dispose();
        }

        public void Update_Unload()
        {
            CustomToolTips.SetToolTip(UI_Interface.ProfileSettingsButton, $"New profile settings", "profileSettingsButton", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            foreach (Control c in Controls)
            {
                c.Controls[0].ForeColor = foreColor;
            }

            UpdateUnloadButton(false);
        }

        public void ProfileBtn_CheckedChanged(object sender, MouseEventArgs e)
        {
            if (Locked)
            {
                return;
            }

            UIThread.Post(() =>
            {
                CustomToolTips.SetToolTip(UI_Interface.ProfileSettingsButton, "New profile settings", "profileSettingsButton", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

                Label selected = (Label)sender;

                if (e != null)
                {
                    if (e.Button == MouseButtons.Right)
                    {
                        string dllPath = AssemblyUtil.GetStartFolder() + "\\pl.lib.dll";

                        if (File.Exists(Application.StartupPath + "\\Profiles Launcher.exe") && File.Exists(dllPath))
                        {
                            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show($"Do you want to export a desktop shortcut for this handler profile?", "Export handler profile shortcut", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (dialogResult == DialogResult.Yes)
                            {
                                string jsonString = File.ReadAllText(GameProfile.profilesPathList[int.Parse(selected.Name) - 1]);
                                JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);
                                string userNotes = ((string)Jprofile["Notes"] != null && (string)Jprofile["Notes"] != "") ? (string)Jprofile["Notes"] : "";

                                string shortcutTitle = selected.Text.StartsWith("Load profile:") ? selected.Text.Split(':')[1] : selected.Text;
                                CreateShortcut(dllPath, GameProfile.Current_UserGameInfo.GameGuid, shortcutTitle, selected.Name, userNotes, GameProfile.Current_UserGameInfo.ExePath);
                            }
                        }

                        return;
                    }
                }

                foreach (Control c in Controls)
                {
                    if (c != selected)
                    {
                        c.Controls[0].ForeColor = foreColor;
                    }
                }

                if (GameProfile.Instance.LoadGameProfile(int.Parse(selected.Name)))//GameProfile auto reset on load
                {
                    Controls[int.Parse(selected.Name) - 1].Controls[0].ForeColor = Color.GreenYellow;
                    UpdateUnloadButton(true);
                    CustomToolTips.SetToolTip(UI_Interface.ProfileSettingsButton, $"Profile \"{selected.Tag}\" settings", "profileSettingsButton", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                }
            });
        }

        private static void CreateShortcut(string dllPath, string gameGUID, string shortcutId, string profileId, string description,string iconPath)
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);

            Type type = assembly.GetType("lib.Shortcuts");
            object instance = Activator.CreateInstance(type); 
            MethodInfo method = type.GetMethod("CreateShortcut"); 
            object[] param = new object[] { gameGUID, shortcutId, profileId, description, iconPath };
            method.Invoke(instance, param);
        }

        public void Update_ProfilesList()
        {
            Visible = false;

            while (Controls.Count > 0)
            {
                Controls[0].Dispose();
            }

            List<int> sizes = new List<int>();

            Size = new Size((int)(300 * scale), (int)(1 * scale));
            int offset = 5;

            if (titleFont == null)
            {
                titleFont = new Font(Theme_Settings.CustomFont, 10F, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                previewFont = new Font(Theme_Settings.CustomFont, 10F, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                unloadFont = unloadButton.Font = new Font(Theme_Settings.CustomFont, 10F, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            }

            for (int i = 0; i < GameProfile.profilesPathList.Count; i++)
            {
                string text;
                string tag = string.Empty;

                offset = 10;

                string[] texts = GetControlsText(i);
                text = texts[0];
                tag = texts[1];

                Panel cont = new Panel
                {
                    Name = (i + 1).ToString(),
                    BackgroundImageLayout = ImageLayout.Zoom,
                    Font = titleFont,
                    BackColor = Color.Transparent,
                    ForeColor = foreColor,
                    Text = text,
                    Height = (int)(20 * scale),
                    Cursor = Theme_Settings.Hand_Cursor,
                };

                Controls.Add(cont);

                Label deleteBtn = new Label
                {
                    Anchor = AnchorStyles.Right,
                    Size = new Size((int)(20 * scale), (int)(20 * scale)),
                    Font = new Font("Lucida Console", (float)12, FontStyle.Bold, GraphicsUnit.Pixel, 0),
                    ForeColor = Color.Red,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "X",
                    Cursor = Theme_Settings.Hand_Cursor
                };

                CustomToolTips.SetToolTip(deleteBtn, $"Delete handler profile {i + 1}.", $"Delete profile{i}.", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                deleteBtn.Click += DeleteBtn_Click;//Delete profile

                Label previewBtn = new Label
                {
                    Anchor = AnchorStyles.Right,
                    Size = new Size((int)(13 * scale), (int)(20 * scale)),
                    Font = previewFont,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "magnifier.png"),
                    BackColor = Color.Transparent,
                    ForeColor = Color.Green,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Cursor = Theme_Settings.Hand_Cursor
                };

                CustomToolTips.SetToolTip(previewBtn, "Show handler profile info .", $"previewBtn{i}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                previewBtn.Click += Profile_Preview;//view profile event 

                Label profileBtn = new Label
                {
                    Name = (i + 1).ToString(),
                    FlatStyle = FlatStyle.Flat,
                    BackgroundImageLayout = ImageLayout.Zoom,
                    Font = titleFont,
                    BackColor = Color.Transparent,
                    ForeColor = foreColor,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Text = text,
                    Height = (int)(20 * scale),
                    Cursor = Theme_Settings.Hand_Cursor,
                    AutoSize = true
                };

                cont.Controls.Add(profileBtn);

                profileBtn.Tag = tag;
                string profileBtnToolTipText = File.Exists(Application.StartupPath + "\\Profiles Launcher.exe") ? $"Load handler profile {profileBtn.Name}. Right click to export a shortcut to desktop." : $"Load handler profile {profileBtn.Name}.";

                profileBtn.MouseClick += ProfileBtn_CheckedChanged;

                cont.Controls.Add(deleteBtn);
                deleteBtn.Location = new Point(cont.Right - deleteBtn.Width, profileBtn.Location.Y);

                cont.Controls.Add(previewBtn);
                previewBtn.Location = new Point(deleteBtn.Left - previewBtn.Width, deleteBtn.Location.Y);      
                

                profileBtn.Location = new Point(profileBtn.Left,(cont.Height / 2) - (profileBtn.Height / 2));
                offset += deleteBtn.Width + previewBtn.Width;

                CustomToolTips.SetToolTip(profileBtn, profileBtnToolTipText, $"{profileBtnToolTipText} {i}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

                Graphics g = CreateGraphics();
                sizes.Add((int)g.MeasureString(profileBtn.Text, profileBtn.Font, profileBtn.Width).Width + offset);
                g.Dispose();

                if (i <= 5)
                {
                    Height += cont.Height + 1;
                }

                bool isVerticalVisible = VerticalScroll.Visible;
               
                if(!isVerticalVisible)
                {
                    Width -= SystemInformation.VerticalScrollBarWidth;
                }
               
            }

            var sortedSizes = sizes.OrderByDescending(x => x).ToList();//Sort profiles titles by Width so the list Width is set to the max value

            if (sortedSizes.Count > 0)
            {
                Width = (int)((sortedSizes[0]) * scale) + offset;
            }

            Location = new Point(UI_Interface.InfoPanel.Left + UI_Interface.ProfileButtonsPanel.Left + 2, UI_Interface.InfoPanel.Top + UI_Interface.ProfileButtonsPanel.Bottom + 3);

            if (Right > parentControl.Right)
            {
                Location = new Point(UI_Interface.InfoPanel.Right - Width + 1, UI_Interface.InfoPanel.Top + UI_Interface.ProfileButtonsPanel.Bottom + 3);
            }

            BringToFront();

            if (Controls.Count == 0)
            {
                Controls.Clear();
                Visible = false;
                unloadButton.Visible = false;
            }
            else
            {
                Visible = true;
            }
        }

        private string[] GetControlsText(int index)
        {
            string jsonString = File.ReadAllText(GameProfile.profilesPathList[index]);
            JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);
            string[] texts = new string[2];

            if ((string)Jprofile["Title"] != null && (string)Jprofile["Title"] != "")
            {
                texts[0] = (string)Jprofile["Title"];
                texts[1] = texts[0];
            }
            else
            {
                texts[0] = $"{PartialTitle} {index + 1}";
                texts[1] = (index + 1).ToString();
            }

            return texts;
        }

        //Show profile config or user notes in handler note "zoomed" textbox
        private void Profile_Preview(object sender, EventArgs e)
        {
            if (Locked)
            {
                return;
            }

            Label selected = (Label)sender;

            Control preview = (Control)selected.Parent;

            if (preview.Text == "Unload")
            {
                return;
            }

            string jsonString = File.ReadAllText(GameProfile.profilesPathList[int.Parse(preview.Name) - 1]);
            JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);

            UIAction_OnShowPreview?.Invoke(BuildPreviewText(Jprofile));
        }

        private string BuildPreviewText(JObject Jprofile)
        {
            StringBuilder sb = new StringBuilder();

            if (Jprofile["Title"].ToString() != "")
            {
                sb.Append($"Title: {Jprofile["Title"]}\n\n");
            }

            if (Jprofile["Notes"].ToString() != "")
            {
                sb.Append($"User Notes:\n");
                sb.Append($"{Jprofile["Notes"]}\n\n");
            }


            sb.Append($"Players Count: {Jprofile["Player(s)"]}\n\n");
            sb.Append($"Gamepads Used: {Jprofile["Controller(s)"]}\n");         
            sb.Append($"Keyboard/Mouse Combo Used: {Jprofile["K&M"]}\n\n");
            sb.Append($"Use Xinput Indexes: {Jprofile["Use XInput Index"]}\n");
            sb.Append($"AutoPlay: {Jprofile["AutoPlay"]["Enabled"]}\n");
            sb.Append($"Use Custom Nicknames: {Jprofile["UseNicknames"]["Use"]}\n");
            sb.Append($"Auto Desktop Scaling On: {Jprofile["AutoDesktopScaling"]["Enabled"]}\n\n");

            if (Jprofile["Options"].Count() > 0)
            {
                sb.Append($"Choosen Options:\n");

                JToken Joptions = Jprofile["Options"];

                foreach (JProperty Jopt in Joptions)
                {
                    sb.Append($" -{(string)Jopt.Name}: {Jopt.Value}\n");
                }

                sb.Append($"\n");
            }

            if ((bool)Jprofile["UseSplitDiv"]["Enabled"])
            {
                sb.Append($"Splitcreen Division Settings:\n");
                sb.Append($" -Color: {Jprofile["UseSplitDiv"]["Color"]}\n");
                sb.Append($" -Hide Desktop Only: {Jprofile["UseSplitDiv"]["HideOnly"]}\n\n");
            }
            else
            {
                sb.Append($"Splitcreen Division: Off\n\n");
            }

            sb.Append($"Custom Windows Setup Timing: {Jprofile["WindowsSetupTiming"]["Time"]}ms\n");
            sb.Append($"Custom Instances Startup Wait Time: {Jprofile["PauseBetweenInstanceLaunch"]["Time"]}s\n\n");

            sb.Append($"Cutscenes Mode Settings:\n");
            sb.Append($" -Keep Window Size: {Jprofile["CutscenesModeSettings"]["Cutscenes_KeepAspectRatio"]}\n");
            sb.Append($" -Mute Audio Only: {Jprofile["CutscenesModeSettings"]["Cutscenes_MuteAudioOnly"]}\n");
            sb.Append($" -UnFocus Game Windows On Exit: {Jprofile["CutscenesModeSettings"]["Cutscenes_Unfocus"]}\n\n");

            sb.Append($"Players Info:\n");

            int totalGuests = 0;

            for (int i = 0; i < Jprofile["Data"].Count(); i++)
            {
                var playerDatas = Jprofile["Data"][i];

                sb.Append($"------------------------\n");
                sb.Append($" -Nickname: {playerDatas["Nickname"]}\n");
                sb.Append($" -Index: {(int)playerDatas["PlayerID"] + 1}\n");
                sb.Append($" -Steam Id: {playerDatas["SteamID"]}\n");

                bool isSDL2 = false;
                bool isSDL3 = false;

                if(playerDatas["IsSDL2"] != null)
                {
                    isSDL2 = (bool)playerDatas["IsSDL2"];
                }

                if (playerDatas["IsSDL3"] != null)
                {
                    isSDL2 = (bool)playerDatas["IsSDL3"];
                }

                bool isController = (bool)playerDatas["IsDInput"] || (bool)playerDatas["IsXInput"] || isSDL2 || isSDL3;

                if (isController)
                {
                    sb.Append($" -Device Type: Gamepad\n");
                    sb.Append($" -Gamepad GUID: {playerDatas["GamepadGuid"]}\n");
                }
                else
                {
                    sb.Append($" -Device Type: Keyboard/Mouse\n");
                }

                bool validGuestField = playerDatas["Guests"] != null;

                if (validGuestField)
                {
                    var parseGuestCount = playerDatas["Guests"]["GUIDS"].ToString();

                    if (parseGuestCount != "")
                    {
                        int playerGuests = parseGuestCount.Split(',').Length;
                        if (playerGuests >= 1)
                        {
                            sb.Append($" -Guest Players: {playerGuests}\n");
                            totalGuests += playerGuests;
                        }
                    }
                }

                sb.Append($" -Screen Index: {playerDatas["ScreenIndex"]}\n");

                if (i == Jprofile["Data"].Count() - 1)
                {
                    sb.Append($"------------------------\n");
                }
            }

            if (totalGuests >= 1)
            {
                sb.Append($" -Total Guests: {totalGuests}\n");
            }

            string preview = sb.ToString();

            return preview;
        }

        private void DeleteBtn_Click(object sender, EventArgs e)//Delete game profile
        {
            if (Locked)
            {
                return;
            }

            Label deleteBtn = (Label)sender;

            DialogResult dialogResult = MessageBox.Show($"Are you sure you want to delete handler profile {deleteBtn.Parent.Name} ?", "Are you sure?!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (dialogResult == DialogResult.Yes)
            {
                //File.Delete(GameProfile.profilesPathList[int.Parse(deleteBtn.Parent.Name) - 1]);
                FileUtil.SendToRecycleBin(GameProfile.profilesPathList[int.Parse(deleteBtn.Parent.Name) - 1]);
                List<FileInfo> profilesPath = Directory.GetParent(GameProfile.profilesPathList[int.Parse(deleteBtn.Parent.Name) - 1]).
                                              EnumerateFiles().OrderBy(s => int.Parse(Regex.Match(s.Name, @"\d+").Value)).ToList();

                for (int i = 0; i < profilesPath.Count(); i++)
                {
                    if (profilesPath[i].Name == $"Profile[{i + 1}].json")
                    {
                        continue;
                    }

                    File.Move(profilesPath[i].FullName, $@"{Directory.GetParent(profilesPath[i].FullName)}\Profile[{i + 1}].json");
                }

                GameProfile.Instance.Reset();

                Update_ProfilesList();

                if (Controls.Count == 0)
                {
                    Visible = false;
                    UI_Interface.ProfileListButton.Visible = false;
                }

                Globals.MainOSD.Show(500, $"Handler Profile *@{OSDHighlightColors.Confirm}|Deleted@*");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            //Rectangle gradientBrushbounds = new Rectangle(0, 0, Width, Height);

            //if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
            //{
            //    return;
            //}

            //Color color = Color.FromArgb(useGradient ? 100 : 0, backGradient.R, backGradient.G, backGradient.B);
            //Color color2 = Color.FromArgb(useGradient ? 120 : 0, backGradient.R, backGradient.G, backGradient.B);
            //LinearGradientBrush lgb =
            //new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color, 90f);

            //ColorBlend topcblend = new ColorBlend(3);
            //topcblend.Colors = new Color[3] { Color.Transparent,color, color2};
            //topcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            //GraphicsPath graphicsPath = FormGraphicsUtil.MakeRoundedRect(gradientBrushbounds, 10, 10, false, false, false, true);
            
            //lgb.InterpolationColors = topcblend;
            //e.Graphics.FillPath(lgb, graphicsPath);

            //lgb.Dispose();
            //graphicsPath.Dispose();
        }
    }
}
