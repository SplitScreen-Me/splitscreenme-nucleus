using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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

        public ProfilesList(Control parent)
        {
            parentControl = parent;

            InitializeComponent();

            Name = "ProfilePanel";
            Size = new Size(300,1);
            Location = new Point(0, 0);
            Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Visible = false;
            BorderStyle = BorderStyle.None;

            foreColor = Theme_Settings.ControlsForeColor;

            if (Theme_Settings.BackgroundGradientColor.A == 1)
            {
                backGradient = Theme_Settings.BackgroundGradientColor;
                BackColor = Color.Transparent;
                useGradient = true;
            }
            else
            {
                backGradient = Color.FromArgb(0, 0, 0, 0);
                BackColor = Theme_Settings.WindowPanelBackColor;
            }

            eventArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);

            GameProfile.OnReset -= Update_Unload;
            GameProfile.OnReset += Update_Unload;

            Instance = this;
        }

        public void Update_Reload()
        {
            string name = int.Parse(Regex.Match(GameProfile.ModeText, @"\d+").Value).ToString();

            Label dummy = new Label
            {
                Name = name,
                Text = $"{PartialTitle} {name}"
            };

            ProfileBtn_CheckedChanged(dummy, eventArgs);
            ScrollControlIntoView(Controls.Find(name,true).FirstOrDefault());

            dummy.Dispose();
        }

        public void Update_Unload()
        {
            Label dummy = new Label();
            ProfileBtn_CheckedChanged(dummy, null);
            dummy.Dispose();
        }

        public void ProfileBtn_CheckedChanged(object sender, MouseEventArgs e)
        {
            if (Locked)
            {
                return;
            }

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
                            CreateShortcut(dllPath,GameProfile.GameInfo.GameGuid, shortcutTitle, selected.Name, userNotes, GameProfile.GameInfo.ExePath);
                        }
                    }

                    return;
                }
            }

            selected.BackColor = Color.Transparent;
            foreach (Control c in Controls)
            {
                if (c != selected && c.Text != "Unload")
                {
                    c.ForeColor = foreColor;
                }

                if (e == null && c.Text == "Unload")
                {
                    c.ForeColor = Color.Gray;
                    selected.Dispose();//dummy control use to reset the unload "button/label"
                }
            }

            if ((selected.Text == "Unload" && selected.ForeColor == Color.Gray) || e == null)
            {
                UIAction_OnUnload_Click?.Invoke();
                return;
            }

            if (selected.Text == "Unload")
            {
                selected.ForeColor = Color.Gray;
                GameProfile.Instance.Reset();
                Globals.MainOSD.Show(500, "Handler Profile Unloaded");
                return;
            }

            if (GameProfile.Instance.LoadGameProfile(int.Parse(selected.Name)))//GameProfile auto reset on load
            {
                Controls[int.Parse(selected.Name) - 1].ForeColor = Color.LightGreen;
                Label unloadBtn = Controls[Controls.Count - 1] as Label;
                unloadBtn.ForeColor = Color.Orange;
            }
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

            foreach (Control control in Controls)
            {
                control.Dispose();
            }

            Controls.Clear();

            List<int> sizes = new List<int>();

            Size = new Size((int)(300 * _scale), (int)(1 * _scale));
            int offset = 5;

            if (titleFont == null)
            {
                titleFont = new Font(Theme_Settings.CustomFont,10F, FontStyle.Bold, GraphicsUnit.Pixel, 0);
                previewFont = new Font(Theme_Settings.CustomFont, 10F, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            }

            for (int i = 0; i < GameProfile.profilesPathList.Count + 1; i++)
            {
                string text;
                offset = 5;

                if (i != GameProfile.profilesPathList.Count)
                {
                    string jsonString = File.ReadAllText(GameProfile.profilesPathList[i]);
                    JObject Jprofile = (JObject)JsonConvert.DeserializeObject(jsonString);

                    if ((string)Jprofile["Title"] != null && (string)Jprofile["Title"] != "")
                    {
                        text = (string)Jprofile["Title"];
                    }
                    else
                    {
                        text = $"{PartialTitle} {i + 1}";
                    }
                }
                else
                {
                    text = "Unload";
                }

                Label deleteBtn = new Label
                {
                    Anchor = AnchorStyles.Right,
                    Size = new Size((int)(20 * _scale), (int)(20 * _scale)),
                    Font = new Font("Lucida Console", (float)12, FontStyle.Bold, GraphicsUnit.Pixel, 0),
                    ForeColor = Color.Red,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Text = "X",
                    Cursor = Theme_Settings.Hand_Cursor
                };

                CustomToolTips.SetToolTip(deleteBtn, $"Delete handler profile {i + 1}.", $"Delete profile{i}.", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                deleteBtn.Click += DeleteBtn_Click;//Delete profile

                offset += deleteBtn.Width;

                Label previewBtn = new Label
                {
                    Anchor = AnchorStyles.Right,
                    Size = new Size((int)(13 * _scale), (int)(20 * _scale)),
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

                offset += previewBtn.Width;

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
                    Height = (int)(20 * _scale),
                    Cursor = Theme_Settings.Hand_Cursor
                };

                string profileBtnToolTipText = File.Exists(Application.StartupPath + "\\Profiles Launcher.exe") ? $"Load handler profile {profileBtn.Name}. Right click to export a shortcut to desktop." : $"Load handler profile {profileBtn.Name}.";

                profileBtn.MouseClick += ProfileBtn_CheckedChanged;

                if (i != GameProfile.profilesPathList.Count)
                {
                    deleteBtn.Location = new Point(profileBtn.Right - deleteBtn.Width, profileBtn.Location.Y);
                    previewBtn.Location = new Point(deleteBtn.Left - previewBtn.Width, deleteBtn.Location.Y);

                    profileBtn.Controls.Add(deleteBtn);
                    profileBtn.Controls.Add(previewBtn);

                    CustomToolTips.SetToolTip(profileBtn, profileBtnToolTipText, $"profileBtnToolTipText${i}", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                }
                else
                {
                    profileBtn.ForeColor = Color.Gray;
                    CustomToolTips.SetToolTip(profileBtn, "Unload current loaded handler profile.", $"Unload profile", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                }

                using (Graphics graphics = Graphics.FromImage(new Bitmap(1, 1)))
                {
                   sizes.Add((int)graphics.MeasureString(profileBtn.Text, profileBtn.Font, Size.Width, StringFormat.GenericDefault).Width + 40);
                   graphics.Dispose();
                }

                if (i <= 5)
                {
                    Height += profileBtn.Height+1;               
                }

                Controls.Add(profileBtn);
            }

            var sortedSizes = sizes.OrderByDescending(x => x).ToList();//Sort profiles titles by Width so the list Width is set to the max value
            Width = (int)((sortedSizes[0]) * _scale) + offset;

            Location = new Point((parentControl.Right - Width) + 1, parentControl.Top);

            BringToFront();

            if (Controls.Count == 1)
            {
                Controls.Clear();
                Visible = false;
            }
            else
            {
                Visible = true;
            }
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

                //sb.Append($"\n");
                sb.Append($"------------------------\n");
                sb.Append($" -Nickname: {playerDatas["Nickname"]}\n");
                sb.Append($" -Index: {(int)playerDatas["PlayerID"] + 1}\n");
                sb.Append($" -Steam Id: {playerDatas["SteamID"]}\n");

                bool isController = (bool)playerDatas["IsDInput"] || (bool)playerDatas["IsXInput"];
                if (isController)
                {
                    sb.Append($" -Device Type: Gamepad\n");
                }
                else
                {
                    sb.Append($" -Device Type: Keyboard/Mouse\n");
                }

               bool validGuestField = playerDatas["Guests"] != null;

                if(validGuestField)
                {
                    int playerGuests = playerDatas["Guests"]["GUIDS"].ToString().Split(',').Length -1;
                    if(playerGuests >= 1)
                    {
                        sb.Append($" -Guest Players: {playerGuests}\n");
                        totalGuests += playerGuests;
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
                File.Delete(GameProfile.profilesPathList[int.Parse(deleteBtn.Parent.Name) - 1]);

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

                Globals.MainOSD.Show(500, "Handler Profile Deleted");
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Rectangle gradientBrushbounds = new Rectangle(0, 0, Width, Height);

            if (gradientBrushbounds.Width == 0 || gradientBrushbounds.Height == 0)
            {
                return;
            }

            Color color = Color.FromArgb(useGradient ? 100 : 0, backGradient.R, backGradient.G, backGradient.B);
            Color color2 = Color.FromArgb(useGradient ? 120 : 0, backGradient.R, backGradient.G, backGradient.B);
            LinearGradientBrush lgb =
            new LinearGradientBrush(gradientBrushbounds, Color.Transparent, color, 90f);

            ColorBlend topcblend = new ColorBlend(3);
            topcblend.Colors = new Color[3] { Color.Transparent,color, color2};
            topcblend.Positions = new float[3] { 0f, 0.5f, 1f };

            GraphicsPath graphicsPath = FormGraphicsUtil.MakeRoundedRect(gradientBrushbounds, 10, 10, false, false, false, true);
            
            lgb.InterpolationColors = topcblend;
            e.Graphics.FillPath(lgb, graphicsPath);

            lgb.Dispose();
            graphicsPath.Dispose();
        }
    }
}
