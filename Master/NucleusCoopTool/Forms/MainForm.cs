using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nucleus.Coop.Forms;
using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Generic.Step;
using Nucleus.Gaming.Windows;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowScrape.Constants;
using System.Media;
using System.Net;
using System.Net.NetworkInformation;
using Nucleus.Gaming.Coop.Generic;
using System.Linq;
using System.ComponentModel;

namespace Nucleus.Coop
{
    /// <summary>
    /// Central UI class to the Nucleus Coop application
    /// </summary>
    public partial class MainForm : BaseForm, IDynamicSized
    {
        public string version = "v2.0";
        private string faq_link = "https://www.splitscreen.me/docs/faq";
        private Settings settingsForm = null;

        private int currentStepIndex;
        private bool formClosing;
        private ContentManager content;
        private IGameHandler handler;

        private GameManager gameManager;
        private Dictionary<UserGameInfo, GameControl> controls;

        private SearchDisksForm form;

        private GameControl currentControl;
        private UserGameInfo currentGameInfo;
        private GenericGameInfo currentGame;
        private GameProfile currentProfile;
        private bool noGamesPresent;
        private List<UserInputControl> stepsList;
        private UserInputControl currentStep;
        private PositionsControl positionsControl;
        private PlayerOptionsControl optionsControl;
        private JSUserInputControl jsControl;
        private Handler hubHandler = null;
        private int KillProcess_HotkeyID = 1;
        private int TopMost_HotkeyID = 2;
        private int StopSession_HotkeyID = 3;
        private List<string> profilePaths = new List<string>();
        private List<Control> ctrls = new List<Control>();
        private readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        private Thread handlerThread;
        private int inst_click = 0;
        private int GameDesc_click = 0;
        private int thirdPartytools_click = 0;
        private bool TopMostToggle = true;
        private string gameDescription;
        protected string api = "https://hub.splitscreen.me/api/v1/";
        private string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string DocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private ScriptDownloader getId;
        private DownloadPrompt downloadPrompt;
        private Image coverImg;
        private Image screenshotImg;
        public Action<IntPtr> RawInputAction { get; set; }

        private bool currentlyUpdatingScript = false;
        private Image defBackground;
        private Image defglowingLine0;
        private Image defglowingLine1;
        private AssetsScraper getAssets;
        public bool connected;
        private bool Splash_On;
        private bool IntroSound_On;
        private bool stopSleep = false;
        private string offline;
        private Color ChoosenColor;
        public Form hform;
        //public PictureBox loading;

        //private System.Windows.Forms.Timer DisposeTimer;
        public void CheckNetCon()//should be re-worked
        {
            System.Threading.Tasks.Task.Run(() =>// Run this in another thread to not block UI 
             {
                 try
                 {
                     Ping myPing = new Ping();
                     String host = "hub.splitscreen.me";
                     byte[] buffer = new byte[32];
                     int timeout = 1000;//3000
                     PingOptions pingOptions = new PingOptions();
                     PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                     if (reply.Status == IPStatus.Success)
                     {
                         this.connected = true;
                         myPing.Dispose();
                     }
                     else
                     {
                         this.connected = false;
                         myPing.Dispose();
                     }
                 }
                 catch(Exception)
                 {
                     this.connected = false;                   
                 }

             });
           
        }
        public enum MachineType : ushort
        {
            IMAGE_FILE_MACHINE_UNKNOWN = 0x0,
            IMAGE_FILE_MACHINE_AM33 = 0x1d3,
            IMAGE_FILE_MACHINE_AMD64 = 0x8664,
            IMAGE_FILE_MACHINE_ARM = 0x1c0,
            IMAGE_FILE_MACHINE_EBC = 0xebc,
            IMAGE_FILE_MACHINE_I386 = 0x14c,
            IMAGE_FILE_MACHINE_IA64 = 0x200,
            IMAGE_FILE_MACHINE_M32R = 0x9041,
            IMAGE_FILE_MACHINE_MIPS16 = 0x266,
            IMAGE_FILE_MACHINE_MIPSFPU = 0x366,
            IMAGE_FILE_MACHINE_MIPSFPU16 = 0x466,
            IMAGE_FILE_MACHINE_POWERPC = 0x1f0,
            IMAGE_FILE_MACHINE_POWERPCFP = 0x1f1,
            IMAGE_FILE_MACHINE_R4000 = 0x166,
            IMAGE_FILE_MACHINE_SH3 = 0x1a2,
            IMAGE_FILE_MACHINE_SH3DSP = 0x1a3,
            IMAGE_FILE_MACHINE_SH4 = 0x1a6,
            IMAGE_FILE_MACHINE_SH5 = 0x1a8,
            IMAGE_FILE_MACHINE_THUMB = 0x1c2,
            IMAGE_FILE_MACHINE_WCEMIPSV2 = 0x169,
        }

        public static MachineType GetDllMachineType(string dllPath)
        {
            // See http://www.microsoft.com/whdc/system/platform/firmware/PECOFF.mspx
            // Offset to PE header is always at 0x3C.
            // The PE header starts with "PE\0\0" =  0x50 0x45 0x00 0x00,
            // followed by a 2-byte machine type field (see the document above for the enum).
            //
            FileStream fs = new FileStream(dllPath, FileMode.Open, FileAccess.Read);
            BinaryReader br = new BinaryReader(fs);
            fs.Seek(0x3c, SeekOrigin.Begin);
            int peOffset = br.ReadInt32();
            fs.Seek(peOffset, SeekOrigin.Begin);
            uint peHead = br.ReadUInt32();

            if (peHead != 0x00004550) // "PE\0\0", little-endian
            {
                throw new Exception("Can't find PE header");
            }

            MachineType machineType = (MachineType)br.ReadUInt16();
            br.Close();
            fs.Close();
            return machineType;
        }

        // Returns true if the exe is 64-bit, false if 32-bit, and null if unknown
        public static bool? Is64Bit(string exePath)
        {
            switch (GetDllMachineType(exePath))
            {
                case MachineType.IMAGE_FILE_MACHINE_AMD64:
                case MachineType.IMAGE_FILE_MACHINE_IA64:
                    return true;
                case MachineType.IMAGE_FILE_MACHINE_I386:
                    return false;
                default:
                    return null;
            }
        }

        private enum ShowWindowEnum
        {
            Hide = 0,
            ShowNormal = 1, ShowMinimized = 2, ShowMaximized = 3,
            Maximize = 3, ShowNormalNoActivate = 4, Show = 5,
            Minimize = 6, ShowMinNoActivate = 7, ShowNoActivate = 8,
            Restore = 9, ShowDefault = 10, ForceMinimized = 11
        };

        //Movable borderless window stuffs
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();

        public void Moveable(params Control[] controls)
        {
            foreach (var ctrl in controls)
            {
                ctrl.MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ReleaseCapture();
                        SendMessage(ctrl.FindForm().Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
                        // Checks if Y = 0, if so maximize the form
                        if (ctrl.FindForm().Location.Y == 0) { ctrl.FindForm().WindowState = FormWindowState.Maximized; }
                    }
                };
            }
        }
        //
        public void button_Click(object sender, EventArgs e)
        {
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            SoundPlayer splayer = new SoundPlayer((Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\button_click.wav")));
            splayer.Play();
        }  

        private void minimizeButton(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void maximizeButton(object sender, EventArgs e)
        {
            clientAreaPanel.SuspendLayout();
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
            clientAreaPanel.ResumeLayout();
            
        }

        private void closeButton(object sender, EventArgs e)
        {
            Process[] processes = Process.GetProcessesByName("SplitCalculator");

            foreach (Process SplitCalculator in processes)
            {
                SplitCalculator.Kill();
            }
            Application.Exit();
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

        public MainForm()
        {
           
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            IniFile theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));
            Splash_On = Convert.ToBoolean(ini.IniReadValue("Dev", "SplashScreen_On"));
            IntroSound_On = Convert.ToBoolean(ini.IniReadValue("Dev", "IntroSound_On"));
            offline = ini.IniReadValue("Dev", "OfflineMod");

            try
            {
                bool MouseClick = Convert.ToBoolean(theme.IniReadValue("Sounds", "MouseClick"));
                
                Image AppButtons = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\button.png"));
             
                string[] rgb_font = theme.IniReadValue("Colors", "FontColor").Split(',');
                string[] rgb_MouseOverColor = theme.IniReadValue("Colors", "MouseOverColor").Split(',');
                string[] rgb_MenuStripBackColor = theme.IniReadValue("Colors", "MenuStripBackColor").Split(',');
                string[] rgb_MenuStripFontColor = theme.IniReadValue("Colors", "MenuStripFontColor").Split(',');
                string[] rgb_TitleBarColor = theme.IniReadValue("Colors", "TitleBarColor").Split(',');
                Color TitleBarColor = Color.FromArgb(Convert.ToInt32(rgb_TitleBarColor[0]), Convert.ToInt32(rgb_TitleBarColor[1]), Convert.ToInt32(rgb_TitleBarColor[2]));
                Color MouseOverBackColor = Color.FromArgb(Convert.ToInt32(rgb_MouseOverColor[0]), Convert.ToInt32(rgb_MouseOverColor[1]), Convert.ToInt32(rgb_MouseOverColor[2]));
                Color MenuStripBackColor = Color.FromArgb(Convert.ToInt32(rgb_MenuStripBackColor[0]), Convert.ToInt32(rgb_MenuStripBackColor[1]), Convert.ToInt32(rgb_MenuStripBackColor[2]));
                Color MenuStripFontColor = Color.FromArgb(Convert.ToInt32(rgb_MenuStripFontColor[0]), Convert.ToInt32(rgb_MenuStripFontColor[1]), Convert.ToInt32(rgb_MenuStripFontColor[2]));
               
                InitializeComponent();

                var DisposeTimer = new System.Windows.Forms.Timer();//dispose splash screen timer
              
                if (ini.IniReadValue("Advanced", "Font") != "")
                {
                    btn_gameOptions.Font = new Font("Segoe UI", float.Parse((ini.IniReadValue("Advanced", "Font"))) - 1.75f);
                }

                if (Splash_On)
                {
                    DisposeTimer.Interval = (3500); //millisecond
                    splash.Image = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\splash.gif"));
                }
                else
                {
                    DisposeTimer.Interval = (1); //millisecond
                    splash.Dispose();
                }

                DisposeTimer.Tick += new EventHandler(TimerTick);
                DisposeTimer.Start();

                //Controls Pictures
                BackColor = TitleBarColor;
                ForeColor = Color.FromArgb(Convert.ToInt32(rgb_font[0]), Convert.ToInt32(rgb_font[1]), Convert.ToInt32(rgb_font[2]));
                clientAreaPanel.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\background.jpg"));
                mainButtonFrame.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\main_buttons_frame.png"));
                rightFrame.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\right_panel.png"));              
                list_Games.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\game_list.png"));
                btnAutoSearch.BackgroundImage = AppButtons;
                button_UpdateAvailable.BackgroundImage = AppButtons;
                btnSearch.BackgroundImage = AppButtons;
                btn_gameOptions.BackgroundImage = AppButtons;
                btn_Download.BackgroundImage = AppButtons;
                btn_Play.BackgroundImage = AppButtons;
                btn_Extract.BackgroundImage = AppButtons;
                btnBack.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\arrow_left.png"));
                btn_Next.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\arrow_right.png"));
                coverFrame.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\cover_layer.png"));
                stepPanelPictureBox.Image = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\logo.png"));
                label_StepTitle.BackgroundImage = AppButtons;
                btn_GameDesc.BackgroundImage = AppButtons;
                btn_scriptAuthorTxt.BackgroundImage = AppButtons;
                btn_dlFromHub.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\dlhub_btn.png"));
                glowingLine0.Image = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\lightbar_top.gif"));
                glowingLine1.Image = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\lightbar_bottom.gif"));
                //
                btn_Extract.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnAutoSearch.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                button_UpdateAvailable.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnSearch.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_gameOptions.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Download.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Play.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnBack.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Next.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_GameDesc.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_scriptAuthorTxt.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_dlFromHub.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                gameContextMenuStrip.BackColor = MenuStripBackColor;
                gameContextMenuStrip.ForeColor = MenuStripFontColor;

                minimizeBtn.Click += new EventHandler(this.minimizeButton);
                maximizeBtn.Click += new EventHandler(this.maximizeButton);
                closeBtn.Click += new EventHandler(this.closeButton);

                defBackground = clientAreaPanel.BackgroundImage;
                defglowingLine0 = glowingLine0.Image;
                defglowingLine1 = glowingLine1.Image;

                Moveable(this);//Make the main window moveable on screen.

                positionsControl = new PositionsControl();
                Settings settingsForm = new Settings(this, positionsControl);
                positionsControl.Paint += PositionsControl_Paint;

                //Log("positions control");
                settingsForm.RegHotkeys(this);

                //Log("referencing controls");
                controls = new Dictionary<UserGameInfo, GameControl>();
                gameManager = new GameManager(this);

                optionsControl = new PlayerOptionsControl();
                jsControl = new JSUserInputControl();

                positionsControl.OnCanPlayUpdated += StepCanPlay;
                optionsControl.OnCanPlayUpdated += StepCanPlay;
                jsControl.OnCanPlayUpdated += StepCanPlay;

                controlscollect();

                if (MouseClick)
                {
                    foreach (Control control in ctrls)
                    {
                        if (control.GetType() == typeof(Button))
                        {
                            control.Click += new EventHandler(this.button_Click);
                        }
                    }
                }

                foreach (Control titleBarButtons in Controls)//avoid "glitchs" while maximizing the window (aesthetic stuff only)
                {
                    titleBarButtons.BackColor = BackColor;
                }

                

                getId = new ScriptDownloader(this);
                downloadPrompt = new DownloadPrompt(hubHandler, this, null, true);

                // selects the list of games, so the buttons look equal
                list_Games.Select();
                gameManager.ReorderUserProfile();
            }
            catch (Exception ex)
            {                
                Log("ERROR - " + ex.Message + "\n\nSTACKTRACE: " + ex.StackTrace);
            }

            if (IntroSound_On && Splash_On)
            {//Play intro sound
                SoundPlayer splayer = new SoundPlayer((Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\intro.wav")));
                splayer.Play();
            }

            string[] ncFiles =  { "DotNetZip.dll", "EasyHook.dll", "EasyHook32.dll", "EasyHook32Svc.exe", "EasyHook64.dll", "EasyHook64Svc.exe", "EasyHookSvc.exe", "Jint.dll", "NAudio.dll", "Newtonsoft.Json.dll", "Nucleus.Gaming.dll", "Nucleus.Hook32.dll", "Nucleus.Hook64.dll", "Nucleus.IJx64.exe", "Nucleus.IJx86.exe", "Nucleus.SHook32.dll", "Nucleus.SHook64.dll", "openxinput1_3.dll", "ProtoInputHooks32.dll", "ProtoInputHooks64.dll", "ProtoInputHooks64.dll", "ProtoInputHost.exe", "ProtoInputIJ32.exe", "ProtoInputIJ64.exe", "ProtoInputIJP32.dll", "ProtoInputIJP64.dll", "ProtoInputLoader32.dll", "ProtoInputLoader64.dll", "ProtoInputUtilDynamic32.dll", "ProtoInputUtilDynamic64.dll", "SharpDX.DirectInput.dll", "SharpDX.dll", "SharpDX.XInput.dll", "StartGame.exe", "WindowScrape.dll" };
            
            foreach (string file in ncFiles)
            {
                if (!File.Exists(Path.Combine(Application.StartupPath, file)))
                {
                    MessageBox.Show(file + " is missing from your Nucleus Co-op installation folder.", "Missing file(s)", MessageBoxButtons.OK, MessageBoxIcon.Error);

                    Process[] processes = Process.GetProcessesByName("NucleusCoop");

                    foreach (Process NucleusCoop in processes)
                    {
                        NucleusCoop.Kill();
                    }
                }
            }
              

            if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\covers")))//Doing this where other folders get created/re-created would be better
            {
                Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\covers")));
            }
            if (!Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots")))
            {
                Directory.CreateDirectory((Path.Combine(Application.StartupPath, @"gui\screenshots")));
            }
       
            CheckNetCon();
            
            CenterToScreen();

            DPIManager.Register(this);
            DPIManager.AddForm(this);           
        }

        private void TimerTick(Object myObject, EventArgs myEventArgs)
        {
            if (connected)
            {
                btn_noHub.Visible = false;
                btn_downloadAssets.Enabled = true;
                btn_Download.Enabled = true;
                //DisposeTimer.Dispose();
            }
            else
            {
                btn_noHub.Visible = true;
                btn_downloadAssets.Enabled = false;
                btn_Download.Enabled = false;
            }

            if (Splash_On && !stopSleep)
            {              
                Thread.Sleep(1200);
                splash.Dispose();
                Controls.Remove(splash);
                stopSleep = true;
            }
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            float newFontSize = Font.Size * scale;
            float fontThis = mainButtonFrame.Font.Size*1.0f;
            if (scale > 1.0f)
            {
                foreach (Control button in mainButtonFrame.Controls)
                {
                    button.Font = new Font("Franklin Gothic Medium", fontThis, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
            }
            scriptAuthorTxt.Font = new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
            txt_GameDesc.Font =  new Font("Franklin Gothic Medium", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
            
        }

        private void PositionsControl_Paint(object sender, PaintEventArgs e)
        {
            if (positionsControl.isDisconnected)
            {
                DPIManager.ForceUpdate();
                positionsControl.isDisconnected = false;
            }
        }

        protected override Size DefaultSize => new Size(1050, 701);

        protected override void WndProc(ref Message m)
        {
            //TODO: if close message, kill application not just window

            if (m.Msg == 0x00FF)//WM_INPUT
            {
                RawInputAction(m.LParam);
            }
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == KillProcess_HotkeyID)
            {
                if (!Gaming.Coop.InputManagement.LockInput.IsLocked)
                {
                    User32Util.ShowTaskBar();
                    Close();
                }
            }
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == TopMost_HotkeyID)
            {
                if (!Gaming.Coop.InputManagement.LockInput.IsLocked)
                {
                    if (TopMostToggle && handler != null)
                    {
                        try
                        {

                            Process[] procs =Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentGame.ExecutableName.ToLower()));
                            if (procs.Length > 0)
                            {
                                for (int i = 0; i < procs.Length; i++)
                                {
                                    IntPtr hWnd = procs[i].NucleusGetMainWindowHandle();
                                    User32Interop.SetWindowPos(hWnd, new IntPtr(-2), 0, 0, 0, 0,
                                        (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                                    ShowWindow(hWnd, ShowWindowEnum.Minimize);
                                }
                            }
                        }
                        catch
                        {
                        }

                        User32Util.ShowTaskBar();
                        Activate();
                        BringToFront();
                        TopMostToggle = false;
                    }
                    else if (!TopMostToggle && handler != null)
                    {
                        Process[] procs =
                            Process.GetProcessesByName(
                                Path.GetFileNameWithoutExtension(currentGame.ExecutableName.ToLower()));
                        if (procs.Length > 0)
                        {
                            for (int i = 0; i < procs.Length; i++)
                            {
                                IntPtr hWnd = procs[i].NucleusGetMainWindowHandle();
                                ShowWindow(hWnd, ShowWindowEnum.Restore);
                                User32Interop.SetWindowPos(hWnd, new IntPtr(-1), 0, 0, 0, 0,
                                    (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                            }
                        }

                        User32Util.HideTaskbar();
                        TopMostToggle = true;
                    }
                }
            }
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == StopSession_HotkeyID)
            {
                if (!Gaming.Coop.InputManagement.LockInput.IsLocked)
                {
                    if (btn_Play.Text == "S T O P")
                    {
                        WindowState = FormWindowState.Normal;
                        BringToFront();
                        btn_Play.PerformClick();
                    }
                }
            }

            base.WndProc(ref m);
        }
        public void RefreshGames()
        {
            lock (controls)
            {
                foreach (KeyValuePair<UserGameInfo, GameControl> con in controls)
                {
                    if (con.Value != null)
                    {
                        con.Value.Dispose();
                    }
                }
                list_Games.Controls.Clear();
                controls.Clear();

                List<UserGameInfo> games = gameManager.User.Games;
                for (int i = 0; i < games.Count; i++)
                {
                    UserGameInfo game = games[i];
                    NewUserGame(game);
                }

                if (games.Count == 0)
                {
                    noGamesPresent = true;
                    GameControl con = new GameControl(null, null)
                    {
                        Width = game_listSizer.Width,
                        Text = "No games"
                    };

                    list_Games.Controls.Add(con);                  
                }
            }

            GameManager.Instance.SaveUserProfile();
        }

        public void NewUserGame(UserGameInfo game)
        {            
            if (game.Game == null || !game.IsGamePresent())
            {
                return;
            }

            if (noGamesPresent)
            {
                noGamesPresent = false;
                RefreshGames();
                return;
            }
            list_Games.SuspendLayout();
            GameControl con = new GameControl(game.Game, game)
            {
                Width = game_listSizer.Width              
            };

            controls.Add(game, con);
            list_Games.Controls.Add(con);
            list_Games.ResumeLayout();

            ThreadPool.QueueUserWorkItem(GetIcon, game);            
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RefreshGames();
            DPIManager.ForceUpdate();
        }

        private void GetIcon(object state)
        {
            UserGameInfo game = (UserGameInfo)state;
            Bitmap bmp;
            string iconPath = ini.IniReadValue("GameIcons", game.Game.GameName);
            if (!string.IsNullOrEmpty(iconPath))
            {
                if (iconPath.EndsWith(".exe"))
                {
                    Icon icon = Shell32.GetIcon(iconPath, false);
                    bmp = icon.ToBitmap();
                    icon.Dispose();
                }
                else
                {
                    bmp = new Bitmap(iconPath);
                }
            }
            else
            {
                Icon icon = Shell32.GetIcon(game.ExePath, false);
                bmp = icon.ToBitmap();
                icon.Dispose();
            }

            game.Icon = bmp;

            lock (controls)
            {
                if (controls.ContainsKey(game))
                {
                    GameControl control = controls[game];
                    control.Invoke((Action)delegate ()
                    {
                        control.Image = game.Icon;
                    });
                }
            }
        }

        public delegate void CheckForAssets(Label dllabel, bool visible,string game);

        public void DelegateCheckForAssets(Label dllabel, bool visible,string game)
        {           
            dllabel.BackColor = this.BackColor;
            dllabel.ForeColor = this.ForeColor;          
            dllabel.AutoSize = true;
            dllabel.Text = game;
            dllabel.Location = new Point(this.Width / 2 - dllabel.Width / 2, 12);
            dllabel.Visible = visible;

            Controls.Add(dllabel);
    
            if (game == "Download Completed")
            {
                glowingLine0.Image =  defglowingLine0;
                glowingLine1.Image = defglowingLine1;          
                mainButtonFrame.Enabled = true;
                btn_downloadAssets.Enabled = true;
                game_listSizer.Enabled = true;
                rightFrame.Enabled = true;
                StepPanel.Enabled = true;
                btn_settings.Enabled = true;
                Controls.Remove(dllabel);            
            }   
        }
        private void btn_downloadAssets_Click(object sender, EventArgs e)
        {
            string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
            glowingLine0.Image = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\download_bar.gif"));
            glowingLine1.Image = Image.FromFile(Path.Combine(Application.StartupPath,@"gui\theme\" + ChoosenTheme + "\\download_bar.gif"));
            Control dllabel = new Label();
            mainButtonFrame.Enabled = false;
            StepPanel.Enabled = false;
            rightFrame.Enabled = false;       
            btn_settings.Enabled = false;
            btn_downloadAssets.Enabled = false;
            game_listSizer.Enabled = false;
            CheckForCovers(dllabel);
        }

        private void CheckForCovers(Control dllabel)
        {
            List<UserGameInfo> games = gameManager.User.Games;
            System.Threading.Tasks.Task.Run(() =>// Run this in another thread to not block UI 
            {
                for (int i = 0; i < games.Count; i++)
                {
                    getAssets = new AssetsScraper();
                    UserGameInfo game = games[i];
                    this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, true, "Checking Cover for : " + game.GameGuid);
                    try
                    {
                        hubHandler = getId.GetHandler(game.Game.GetHubId());
                        string _covers = $@"https://images.igdb.com/igdb/image/upload/t_cover_big/{hubHandler.GameCover}.jpg";
                       
                        if (hubHandler.Id != null)
                        {                                 
                            getAssets.SaveCovers(_covers, game.GameGuid);                           
                        }
                    }
                    catch (Exception)
                    { }
                }
                CheckForScreenshots(dllabel);
            });
           
        }
        private void CheckForScreenshots(Control dllabel)
        {
            List<UserGameInfo> games = gameManager.User.Games;
            System.Threading.Tasks.Task.Run(() =>// Run this in another thread to not block UI 
            {
                for (int i = 0; i < games.Count; i++)
                {
                    getAssets = new AssetsScraper();
                    UserGameInfo game = games[i];
                    this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, true, "Checking Screenshots for : " + game.GameGuid);
                    try
                    {
                        hubHandler = getId.GetHandler(game.Game.GetHubId());
                        string _screenshots = game.Game.GetScreenshots();

                        if (hubHandler.Id != null)
                        {                         
                            getAssets.SaveScreenshots(_screenshots, game.GameGuid);                           
                        }
                    }
                    catch (Exception)
                    { }
                }               
                this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, false, "Download Completed");
            });
        }

        bool stepPanelPictureBoxDispose = false;
        private void list_Games_SelectedChanged(object arg1, Control arg2)
        {            
            currentControl = (GameControl)arg1;
            currentGameInfo = currentControl.UserGameInfo;

            if (currentGameInfo == null)
            {
                btn_gameOptions.Visible = false;
                button_UpdateAvailable.Visible = false;
                return;
            }
            else
            {           
                currentGame = currentGameInfo.Game;

                inst_click = 0;
                GameDesc_click = 0;
                if (!stepPanelPictureBoxDispose)
                {
                    stepPanelPictureBox.Dispose();
                    rightFrame.Visible = true;
                    btn_gameOptions.Visible = true;
                    btn_scriptAuthorTxt.Visible = true;
                    stepPanelPictureBoxDispose = true;
                }
                StepPanel.Visible = true;
                button_UpdateAvailable.Visible = false;
                
                gameDescription = "";

                string name = currentGame.GUID;
                try
                {
                    ///Apply covers
                    if (File.Exists(Path.Combine(Application.StartupPath, @"gui\covers\" + name + ".jpeg")))
                    {
                        coverImg = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\covers\" + name + ".jpeg"));
                        clientAreaPanel.SuspendLayout();
                        cover.BackgroundImage = coverImg;
                        clientAreaPanel.ResumeLayout();
                        coverFrame.Visible = true;
                        cover.Visible = true;
                    }
                    else
                    {
                        string ChoosenTheme = ini.IniReadValue("Theme", "Theme");
                        cover.Visible = true;
                        coverFrame.Visible = true;
                        cover.BackgroundImage = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme + "\\no_cover.png"));
                        clientAreaPanel.SuspendLayout();
                        clientAreaPanel.BackgroundImage = defBackground;
                        clientAreaPanel.ResumeLayout();
                    }

                    //Apply screenshots randomly

                    if (Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + name)))
                    {
                        string[] imgsPath = Directory.GetFiles((Path.Combine(Application.StartupPath, @"gui\screenshots\" + name)));
                        Random rNum = new Random();
                        int RandomIndex = rNum.Next(0, imgsPath.Count());

                        screenshotImg = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\screenshots\" + name + "\\" + RandomIndex + "_" + name + ".jpeg"));   //name(1) => directory name ; name(2) = partial image name 
                        clientAreaPanel.SuspendLayout();
                        clientAreaPanel.BackgroundImage = screenshotImg;
                        clientAreaPanel.ResumeLayout();

                    }
                    else
                    {
                        clientAreaPanel.SuspendLayout();
                        clientAreaPanel.BackgroundImage = defBackground;
                        clientAreaPanel.ResumeLayout();
                    }
                }
               
                catch
                { }
            }
           
            //if (connected)
            //{ 
            //    try//get game description (online)
            //    {
            //        hubHandler = getId.GetHandler(currentGameInfo.Game.GetHubId());
            //        gameDescription = hubHandler.GameDescription;

            //        if (gameDescription.EndsWith("."))
            //        {
            //            txt_GameDesc.Text = hubHandler.GameDescription;
            //            btn_GameDesc.Enabled = true;
            //            btn_GameDesc.Visible = true;
            //        }
            //        else
            //        {
            //            btn_GameDesc.Enabled = false;
            //            txt_GameDescSizer.Visible = false;
            //            txt_GameDesc.Visible = false;                        
            //        }
            //    }
            //    catch (Exception)
            //    {
            //        btn_GameDesc.Enabled = false;
            //        txt_GameDescSizer.Visible = false;
            //        txt_GameDesc.Visible = false;
            //    }
            // }
           

            btn_Play.Enabled = false;

            stepsList = new List<UserInputControl>
            {
                positionsControl,
                optionsControl
            };

            for (int i = 0; i < currentGame.CustomSteps.Count; i++)
            {
                stepsList.Add(jsControl); 
            }

            currentProfile = new GameProfile();
            currentProfile.InitializeDefault(currentGame);
            gameManager.UpdateCurrentGameProfile(currentProfile);
            
            btn_gameOptions.Enabled = true;

            button_UpdateAvailable.Visible = currentGameInfo.Game.IsUpdateAvailable(false);
            currentlyUpdatingScript = false;

            if (currentGame.Description?.Length > 0)
            {
                btn_scriptAuthorTxt.Enabled = true;              
                scriptAuthorTxt.Text = currentGame.Description;
                scriptAuthorTxt.Visible = true;
                scriptAuthorTxtSizer.Visible = true;
            }
            else
            {
                btn_scriptAuthorTxt.Enabled = false;
                scriptAuthorTxtSizer.Visible = false;
                scriptAuthorTxt.Visible = false;
            }

            if (content != null)
            {
                content.Dispose();
            }

            // content manager is shared withing the same game
            content = new ContentManager(currentGame);
            GoToStep(0);
        }

        private void EnablePlay()
        {
            btn_Play.Enabled = true;
        }

        private void StepCanPlay(UserControl obj, bool canProceed, bool autoProceed)
        {
            if (!canProceed)
            {
                btn_Next.Enabled = false;
                return;
            }

            if (currentStepIndex + 1 > stepsList.Count - 1)
            {
                EnablePlay();
                return;
            }

            if (autoProceed)
            {
                GoToStep(currentStepIndex + 1);
            }
            else
            {
                btn_Next.Enabled = true;
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            GoToStep(currentStepIndex + 1);
        }

        private void KillCurrentStep()
        {

            if (currentGameInfo.Game.Description?.Length > 0)
            {
                btn_scriptAuthorTxt.Visible = true;         
            }

            if (gameDescription?.Length > 0 && connected)
            {
                txt_GameDesc.Text = hubHandler.GameDescription;
                btn_GameDesc.Enabled = true;
            }

            foreach (Control c in StepPanel.Controls)
            {
                if (!c.Name.Equals("btn_GameDesc") && !c.Name.Equals("scriptAuthorTxt") && !c.Name.Equals("txt_GameDescSizer"))
                {
                    StepPanel.Controls.Remove(c);
                }
            }

        }

        private void GoToStep(int step)
        {
            btnBack.Enabled = step > 0;
            if (step >= stepsList.Count)
            {
                return;
            }

            if (step >= 2)
            {
                // Custom steps
                List<CustomStep> customSteps = currentGame.CustomSteps;
                int customStepIndex = step - 2;
                CustomStep customStep = customSteps[0];

                if (customStep.UpdateRequired != null)
                {
                    customStep.UpdateRequired();
                }

                if (customStep.Required)
                {
                    jsControl.CustomStep = customStep;
                    jsControl.Content = content;
                }
                else
                {
                    EnablePlay();
                    return;
                }
            }

            KillCurrentStep();

            currentStepIndex = step;
            currentStep = stepsList[step];
            currentStep.Size = StepPanel.Size;
            currentStep.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;

            currentStep.Initialize(currentGameInfo, currentProfile);

            btn_Next.Enabled = currentStep.CanProceed && step != stepsList.Count - 1;

            StepPanel.Controls.Add(currentStep);
            currentStep.Size = StepPanel.Size; // for some reason this line must exist or the PositionsControl get messed up

            label_StepTitle.Text = currentStep.Title;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            formClosing = true;

            if (handler != null)
            {
                Log("OnFormClosed method calling Handler End function");
                try
                {
                    handler.End(false);
                }
                catch { }
            }

            User32Util.ShowTaskBar();
        }

        private System.Windows.Forms.Timer slideshow;
        private List<Form> list = new List<Form>();
        private void btn_Play_Click(object sender, EventArgs e)
        {
            if (btn_Play.Text == "S T O P")
            {              
                try
                {
                    if (handler != null)
                    {
                        Log("Stop button clicked, calling Handler End function");
                        handler.End(true);
                    }

                    foreach (Form openForm in Application.OpenForms)
                    {
                        if (openForm.Name == "Hide Desktop")
                        {
                            openForm.Close();
                        }
                    }
                }
                catch { }
                User32Util.ShowTaskBar();
                SetBtnToPlay();
                btn_Play.Enabled = false;          
                RefreshGames();
                GoToStep(0);               
                return;
            }

            currentStep?.Ended();

            btn_Play.Text = "S T O P";
            btnBack.Enabled = false;

            gameManager.AddScript(Path.GetFileNameWithoutExtension(currentGame.JsFileName));

            currentGame = gameManager.GetGame(currentGameInfo.ExePath);
            currentGameInfo.InitializeDefault(currentGame, currentGameInfo.ExePath);

            handler = gameManager.MakeHandler(currentGame);
            handler.Initialize(currentGameInfo, GameProfile.CleanClone(currentProfile));
            handler.Ended += handler_Ended;

            gameManager.Play(handler);
            if (handler.TimerInterval > 0)
            {
                handlerThread = new Thread(UpdateGameManager);
                handlerThread.Start();
            }

            if (currentGame.HideTaskbar && ini.IniReadValue("CustomLayout", "SplitDiv") != "True")
            {
                User32Util.HideTaskbar();
            }

            if (currentGame.ProtoInput.AutoHideTaskbar || ini.IniReadValue("CustomLayout", "SplitDiv") == "True")
            {
                if (ProtoInput.protoInput.GetTaskbarAutohide())
                {
                    currentGame.ProtoInput.AutoHideTaskbar = false; // If already hidden don't change it, and dont set it unhidden after.
                }
                else
                {
                    ProtoInput.protoInput.SetTaskbarAutohide(true);
                }
            }
     
            if (ini.IniReadValue("CustomLayout", "SplitDiv") == "True")
            {                
                IDictionary<string, Color> splitColors = new Dictionary<string, Color>();

                splitColors.Add("Black", Color.Black);
                splitColors.Add("Gray", Color.DimGray);
                splitColors.Add("White", Color.White);
                splitColors.Add("Dark Blue", Color.DarkBlue);
                splitColors.Add("Blue", Color.Blue);
                splitColors.Add("Purple", Color.Purple);
                splitColors.Add("Pink", Color.Pink);
                splitColors.Add("Red", Color.Red);
                splitColors.Add("Orange", Color.Orange);
                splitColors.Add("Yellow", Color.Yellow);
                splitColors.Add("Green", Color.Green);

                foreach (KeyValuePair<string, Color> color in splitColors)
                {
                    if (color.Key == ini.IniReadValue("CustomLayout", "SplitDivColor"))
                    {
                        ChoosenColor = color.Value;
                    }
                }

                var loadTimer = new System.Windows.Forms.Timer
                {
                    Interval = (80000) //millisecond
                };

                loadTimer.Tick += new EventHandler(loadTimerTick);
                loadTimer.Start();

                slideshow = new System.Windows.Forms.Timer
                {
                    Interval = (8000) //millisecond
                };

                slideshow.Tick += new EventHandler(slideshowTick);
                slideshow.Start();

                foreach (Screen screen in Screen.AllScreens)
                {
                    Form hform = new Form
                    {
                        BackColor = Color.Black,
                        Location = new Point(screen.Bounds.X, screen.Bounds.Y),
                        Width = screen.WorkingArea.Size.Width,
                        Height = screen.WorkingArea.Size.Height+50,
                        BackgroundImage = clientAreaPanel.BackgroundImage,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        FormBorderStyle = FormBorderStyle.None,
                        StartPosition = FormStartPosition.Manual
                    };

                    list.Add(hform);
                    hform.Show();                   
                }
            }

            if (currentGame.HideDesktop && ini.IniReadValue("CustomLayout", "SplitDiv") != "True")
            {
                foreach (Screen screen in Screen.AllScreens)
                {
                    hform = new Form
                    {
                        BackColor = Color.Black,
                        Location = new Point(screen.Bounds.X, screen.Bounds.Y),
                        
                };
            
                    hform.Width = screen.WorkingArea.Size.Width;
                    hform.Height = screen.WorkingArea.Size.Height+50;
                    hform.FormBorderStyle = FormBorderStyle.None;
                    hform.StartPosition = FormStartPosition.Manual;
                    hform.Show();
                }
            }
           
            WindowState = FormWindowState.Minimized;
        }

        private void slideshowTick(Object myObject, EventArgs myEventArgs)
        {
           
            if (Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName)))
            {
               
                string[] imgsPath = Directory.GetFiles((Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName)));
                Random rNum = new Random();
                int RandomIndex = rNum.Next(0, imgsPath.Count());

                screenshotImg = Image.FromFile(Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName + "\\" + RandomIndex + "_" + currentGameInfo.Game.GameName + ".jpeg"));  
                foreach (Form l in list)
                {
                   l.SuspendLayout();
                   l.BackgroundImage = screenshotImg;
                   l.ResumeLayout();
                }

            }
        }

        private void loadTimerTick(Object myObject, EventArgs myEventArgs)
        {
            slideshow.Stop();
            foreach (Form l in list)
            {
                l.BackgroundImage = null;
                l.BackColor = ChoosenColor;
            }
        }

        private void SetBtnToPlay()
        {
            btn_Play.Text = "P L A Y";
            
        }

        private void handler_Ended()
        {
            hform.Dispose();
            Log("Handler ended method called");
            User32Util.ShowTaskBar();
            handler = null;
            try
            {
                if (handlerThread != null)
                {
                    handlerThread.Abort();
                    handlerThread = null;
                }
            }
            catch { }
            Invoke(new Action(SetBtnToPlay));
        }

        private void UpdateGameManager(object state)
        {
            for (; ; )
            {
                try
                {
                    if (gameManager == null || formClosing || handler == null)
                    {
                        break;
                    }

                    string error = gameManager.Error;
                    if (!string.IsNullOrEmpty(error))
                    {

                        Log("Restoring backed up registry files - method 3");
                        string[] regFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);
                        foreach (string regFilePath in regFiles)
                        {
                            Process proc = new Process();

                            try
                            {
                                proc.StartInfo.FileName = "reg.exe";
                                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                                proc.StartInfo.CreateNoWindow = true;
                                proc.StartInfo.UseShellExecute = false;

                                string command = "import \"" + regFilePath + "\"";
                                proc.StartInfo.Arguments = command;
                                proc.Start();

                                proc.WaitForExit();
                                Log($"Imported {Path.GetFileName(regFilePath)}");
                            }
                            catch (Exception)
                            {
                                proc.Dispose();
                            }

                            File.Delete(regFilePath);
                        }

                        handler_Ended();
                        return;
                    }

                    handler.Update(handler.TimerInterval);
                    Thread.Sleep(TimeSpan.FromMilliseconds(handler.TimerInterval));
                }
                catch (ThreadAbortException)
                {

                }
            }
        }

        private void arrow_Back_Click(object sender, EventArgs e)
        {
            currentStepIndex--;
            if (currentStepIndex < 0)
            {
                currentStepIndex = 0;
                return;
            }
            GoToStep(currentStepIndex);
        }

        private void arrow_Next_Click(object sender, EventArgs e)
        {
            currentStepIndex = Math.Min(currentStepIndex++, stepsList.Count - 1);
            GoToStep(currentStepIndex);
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            SearchGame();
        }

        public void SearchGame(string exeName = null)
        {
            try
            {
                using (OpenFileDialog open = new OpenFileDialog())
                {

                    if (string.IsNullOrEmpty(exeName))
                    {
                        open.Title = "Select a game executable to add to Nucleus";
                        open.Filter = "Game Executable Files|*.exe";
                    }
                    else
                    {
                        open.Title = string.Format("Select {0} to add the game to Nucleus", exeName);
                        open.Filter = "Game Exe|" + exeName;
                    }

                    if (open.ShowDialog() == DialogResult.OK)
                    {
                        string path = open.FileName;

                        List<GenericGameInfo> info = gameManager.GetGames(path);

                        if (info.Count > 1)
                        {
                            GameList list = new GameList(info);

                            if (list.ShowDialog() == DialogResult.OK)
                            {
                                UserGameInfo game = GameManager.Instance.TryAddGame(path, list.Selected);

                                if (game != null)
                                {
                                    MessageBox.Show(string.Format("The game {0} has been added!", game.Game.GameName), "Nucleus - Game added");
                                    RefreshGames();
                                }
                            }
                        }
                        else if (info.Count == 1)
                        {
                            UserGameInfo game = GameManager.Instance.TryAddGame(path, info[0]);
                            MessageBox.Show(string.Format("The game {0} has been added!", game.Game.GameName), "Nucleus - Game added");
                            RefreshGames();
                        }
                        else
                        {
                            MessageBox.Show(string.Format("The executable '{0}' was not found in any game handler's Game.ExecutableName field. Game has not been added.", Path.GetFileName(path)), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch(Exception)
            {

            }
        }

        private bool AutoSearchLoaded = false;
        private void btnAutoSearch_Click(object sender, EventArgs e)
        {
            if (!AutoSearchLoaded)
            {
                form = new SearchDisksForm(this);               
                form.FormClosed += Form_FormClosed;
                form.Show();
                AutoSearchLoaded = true;
            }
            else
            {
                form.Location = new Point(Location.X+Width/2-form.Width/2, Location.Y+Height/2-form.Height/2);
                form.Visible = true;
            }
        }

        private void Form_FormClosed(object sender, FormClosedEventArgs e)
        {
            User32Util.ShowTaskBar();
        }

        private void btnShowTaskbar_Click(object sender, EventArgs e)
        {
            User32Util.ShowTaskBar();
        }

        private bool SettingsLoaded = false;

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            if (!SettingsLoaded)
            {
                settingsForm = new Settings(this, positionsControl);
                settingsForm.Show();
                SettingsLoaded = true;
            }
            else
            {
                settingsForm.Location = new Point(Location.X + Width / 2 - settingsForm.Width / 2, Location.Y + Height / 2 - settingsForm.Height / 2);
                settingsForm.Visible = true;
            }           
        }

        private void DetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetDetails();
        }

        private void DeleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeleteGame();
        }

        private void GetDetails()
        {
            string userProfile = gameManager.GetUserProfilePath();

            if (File.Exists(userProfile))
            {
                string jsonString = File.ReadAllText(userProfile);
                JObject jObject = JsonConvert.DeserializeObject(jsonString) as JObject;

                JArray games = jObject["Games"] as JArray;
                for (int i = 0; i < games.Count; i++)
                {
                    string gameGuid = jObject["Games"][i]["GameGuid"].ToString();
                    string profiles = jObject["Games"][i]["Profiles"].ToString();
                    string exePath = jObject["Games"][i]["ExePath"].ToString();
                    
                    if (gameGuid == currentGameInfo.GameGuid && exePath == currentGameInfo.ExePath)
                    {
                        string arch = "";
                        if (Is64Bit(exePath) == true)
                        {
                            arch = "x64";
                        }
                        else if (Is64Bit(exePath) == false)
                        {
                            arch = "x86";
                        }
                        else
                        {
                            arch = "Unknown";
                        }

                        MessageBox.Show(string.Format("Game Name: {0}\nArchitecture: {1}\nSteam ID: {2}\n\nHandler Filename: {3}\nNucleus Game Content Path: {4}\nOrig Exe Path: {5}\n\nMax Players: {6}\nSupports XInput: {7}\nSupports DInput: {8}\nSupports Keyboard: {9}\nSupports multiple keyboards and mice: {10}", currentGameInfo.Game.GameName, arch, currentGameInfo.Game.SteamID, currentGameInfo.Game.JsFileName, Path.Combine(gameManager.GetAppContentPath(), gameGuid), exePath, currentGameInfo.Game.MaxPlayers, currentGameInfo.Game.Hook.XInputEnabled || currentGameInfo.Game.ProtoInput.XinputHook, currentGameInfo.Game.Hook.DInputEnabled, currentGameInfo.Game.SupportsKeyboard, currentGameInfo.Game.SupportsMultipleKeyboardsAndMice), "Game Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void DeleteGame(bool dontConfirm = false)
        {
            string userProfile = gameManager.GetUserProfilePath();

            if (File.Exists(userProfile))
            {
                string jsonString = File.ReadAllText(userProfile);
                JObject jObject = JsonConvert.DeserializeObject(jsonString) as JObject;

                JArray games = jObject["Games"] as JArray;
                for (int i = 0; i < games.Count; i++)
                {
                    string gameGuid = jObject["Games"][i]["GameGuid"].ToString();
                    string profiles = jObject["Games"][i]["Profiles"].ToString();
                    string exePath = jObject["Games"][i]["ExePath"].ToString();

                    if (gameGuid == currentGameInfo.GameGuid && exePath == currentGameInfo.ExePath)
                    {
                        DialogResult dialogResult = dontConfirm ? DialogResult.Yes :
                            MessageBox.Show("Are you sure you want to delete " + currentGameInfo.Game.GameName + "?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                        if (dialogResult == DialogResult.Yes)
                        {
                            gameManager.User.Games.RemoveAt(i);
                            jObject["Games"][i].Remove();
                            string output = JsonConvert.SerializeObject(jObject, Formatting.Indented);
                            File.WriteAllText(userProfile, output);
                            RefreshGames();
                        }
                    }
                }
            }
        }

        private void GameContextMenuStrip_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Control selectedControl = FindControlAtCursor(this);

            if (selectedControl.GetType() == typeof(Label) || selectedControl.GetType() == typeof(PictureBox))
            {
                selectedControl = selectedControl.Parent;
            }

            foreach (Control c in selectedControl.Controls)
            {
                if (c is Label)
                {
                    if (c.Text == "No games")
                    {
                        gameContextMenuStrip.Items[0].Text = "No game selected...";
                        for (int i = 1; i < gameContextMenuStrip.Items.Count; i++)
                        {
                            gameContextMenuStrip.Items[i].Visible = false;
                        }
                        return;
                    }
                }
            }
			 		
            if (selectedControl.GetType() == typeof(GameControl) || selectedControl.GetType() == typeof(Button))
            {

                bool btnClick = false;
                if (selectedControl.GetType() == typeof(GameControl))
                {

                    currentControl = (GameControl)selectedControl;
                    currentGameInfo = currentControl.UserGameInfo;
                    gameContextMenuStrip.Items[0].Visible = true;
                    gameContextMenuStrip.Items[2].Visible = true;
                }
                else
                {
                    btnClick = true;
                    gameContextMenuStrip.Items[0].Visible = false;
                }

                gameContextMenuStrip.Items[1].Visible = false;
                gameContextMenuStrip.Items[7].Visible = false;
                gameContextMenuStrip.Items[8].Visible = false;
                gameContextMenuStrip.Items[9].Visible = false;
                gameContextMenuStrip.Items[10].Visible = false;
                gameContextMenuStrip.Items[11].Visible = false;
                gameContextMenuStrip.Items[12].Visible = false;
                gameContextMenuStrip.Items[13].Visible = false;
                gameContextMenuStrip.Items[14].Visible = false;
                gameContextMenuStrip.Items[15].Visible = false;

                if (string.IsNullOrEmpty(currentGameInfo.GameGuid) || currentGameInfo == null)
                {
                    gameContextMenuStrip.Items[0].Text = "No game selected...";
                    for (int i = 1; i < gameContextMenuStrip.Items.Count; i++)
                    {
                        gameContextMenuStrip.Items[i].Visible = false;
                    }
                }
                else
                {
                    gameContextMenuStrip.Items[0].Text = currentGameInfo.Game.GameName;

                    bool userConfigPathExists = false;
                    bool userSavePathExists = false;
                    bool docConfigPathExists = false;
                    bool docSavePathExists = false;

                    //bool userConfigPathConverted = false;
                    if (currentGameInfo.Game.UserProfileConfigPath?.Length > 0 && currentGameInfo.Game.UserProfileConfigPath.ToLower().StartsWith(@"documents\"))
                    {
                        currentGameInfo.Game.DocumentsConfigPath = currentGameInfo.Game.UserProfileConfigPath.Substring(10);
                        currentGameInfo.Game.UserProfileConfigPath = null;
                        currentGameInfo.Game.DocumentsConfigPathNoCopy = currentGameInfo.Game.UserProfileConfigPathNoCopy;
                        currentGameInfo.Game.ForceDocumentsConfigCopy = currentGameInfo.Game.ForceUserProfileConfigCopy;
                        //userConfigPathConverted = true;
                    }

                    //bool userSavePathConverted = false;
                    if (currentGameInfo.Game.UserProfileSavePath?.Length > 0 && currentGameInfo.Game.UserProfileSavePath.ToLower().StartsWith(@"documents\"))
                    {
                        currentGameInfo.Game.DocumentsSavePath = currentGameInfo.Game.UserProfileSavePath.Substring(10);
                        currentGameInfo.Game.UserProfileSavePath = null;
                        currentGameInfo.Game.DocumentsSavePathNoCopy = currentGameInfo.Game.UserProfileSavePathNoCopy;
                        currentGameInfo.Game.ForceDocumentsSaveCopy = currentGameInfo.Game.ForceUserProfileSaveCopy;
                        //userSavePathConverted = true;
                    }

                    for (int i = 1; i < gameContextMenuStrip.Items.Count; i++)
                    {
                        gameContextMenuStrip.Items[i].Visible = true;

                        if (string.IsNullOrEmpty(currentGameInfo.Game.UserProfileConfigPath) && string.IsNullOrEmpty(currentGameInfo.Game.UserProfileSavePath) && string.IsNullOrEmpty(currentGameInfo.Game.DocumentsConfigPath) && string.IsNullOrEmpty(currentGameInfo.Game.DocumentsSavePath))
                        {
                            if (i == 7)
                            {
                                gameContextMenuStrip.Items[i].Visible = false;
                            }
                        }
                        else if (i == 1)
                        {
                            profilePaths.Clear();
                            profilePaths.Add(Environment.GetEnvironmentVariable("userprofile"));
                            profilePaths.Add(DocumentsRoot);

                            if (currentGameInfo.Game.UseNucleusEnvironment)
                            {
                                string targetDirectory = $@"{NucleusEnvironmentRoot}\NucleusCoop\";

                                if (Directory.Exists(targetDirectory))
                                {
                                    string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory, "*", SearchOption.TopDirectoryOnly);
                                    foreach (string subdirectory in subdirectoryEntries)
                                    {
                                        profilePaths.Add(subdirectory);
                                        if ($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\" == targetDirectory)
                                        {
                                            profilePaths.Add(subdirectory + "\\Documents");
                                        }
                                    }
                                }


                                if ($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\" != targetDirectory)
                                {
                                    targetDirectory = $@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\";
                                    if (Directory.Exists(targetDirectory))
                                    {
                                        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory, "*", SearchOption.TopDirectoryOnly);
                                        foreach (string subdirectory in subdirectoryEntries)
                                        {
                                            profilePaths.Add(subdirectory + "\\Documents");
                                        }
                                    }
                                }
                            }

                            //Path.GetDirectoryName(DocumentsRoot) + $@"\NucleusCoop\{player.Nickname}\Documents"
                        }

                        if (i == 9)
                        {
                            (gameContextMenuStrip.Items[8] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameContextMenuStrip.Items[9] as ToolStripMenuItem).DropDownItems.Clear();
                            if (currentGameInfo.Game.UserProfileConfigPath?.Length > 0)
                            {
                                if (profilePaths.Count > 0)
                                {
                                    foreach (string profilePath in profilePaths)
                                    {
                                        string currPath = Path.Combine(profilePath, currentGameInfo.Game.UserProfileConfigPath);
                                        if (Directory.Exists(currPath))
                                        {
                                            if (!userConfigPathExists)
                                            {
                                                userConfigPathExists = true;
                                            }

                                            string nucPrefix = "";
                                            if (Directory.GetParent(profilePath).Name == "NucleusCoop")
                                            {
                                                nucPrefix = "Nucleus: ";
                                            }

                                            (gameContextMenuStrip.Items[8] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profilePath.TrimEnd('\\')), null, new EventHandler(UserProfileOpenSubmenuItem_Click));
                                            (gameContextMenuStrip.Items[9] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profilePath.TrimEnd('\\')), null, new EventHandler(UserProfileDeleteSubmenuItem_Click));
                                        }
                                    }
                                }
                            }

                            if (!userConfigPathExists)
                            {
                                gameContextMenuStrip.Items[8].Visible = false;
                                gameContextMenuStrip.Items[9].Visible = false;
                            }
                        }

                        if (i == 11)
                        {
                            (gameContextMenuStrip.Items[10] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameContextMenuStrip.Items[11] as ToolStripMenuItem).DropDownItems.Clear();
                            if (currentGameInfo.Game.UserProfileSavePath?.Length > 0)
                            {
                                if (profilePaths.Count > 0)
                                {
                                    foreach (string profilePath in profilePaths)
                                    {
                                        string currPath = Path.Combine(profilePath, currentGameInfo.Game.UserProfileSavePath);
                                        if (Directory.Exists(currPath))
                                        {
                                            if (!userSavePathExists)
                                            {
                                                userSavePathExists = true;
                                            }

                                            string nucPrefix = "";
                                            if (Directory.GetParent(profilePath).Name == "NucleusCoop")
                                            {
                                                nucPrefix = "Nucleus: ";
                                            }

                                            (gameContextMenuStrip.Items[10] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profilePath.TrimEnd('\\')), null, new EventHandler(UserProfileOpenSubmenuItem_Click));
                                            (gameContextMenuStrip.Items[11] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Path.GetFileName(profilePath.TrimEnd('\\')), null, new EventHandler(UserProfileDeleteSubmenuItem_Click));
                                        }
                                    }
                                }
                            }

                            if (!userSavePathExists)
                            {
                                gameContextMenuStrip.Items[10].Visible = false;
                                gameContextMenuStrip.Items[11].Visible = false;
                            }
                        }

                        if (i == 13)
                        {
                            (gameContextMenuStrip.Items[12] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameContextMenuStrip.Items[13] as ToolStripMenuItem).DropDownItems.Clear();
                            if (currentGameInfo.Game.DocumentsConfigPath?.Length > 0)
                            {
                                if (profilePaths.Count > 0)
                                {
                                    foreach (string profilePath in profilePaths)
                                    {
                                        string currPath = Path.Combine(profilePath, currentGameInfo.Game.DocumentsConfigPath);
                                        if (Directory.Exists(currPath))
                                        {
                                            if (!docConfigPathExists)
                                            {
                                                docConfigPathExists = true;
                                            }

                                            string nucPrefix = "";
                                            if (Directory.GetParent(Directory.GetParent(profilePath).ToString()).Name == "NucleusCoop")
                                            {
                                                nucPrefix = "Nucleus: ";
                                            }

                                            (gameContextMenuStrip.Items[12] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, new EventHandler(DocOpenSubmenuItem_Click));
                                            (gameContextMenuStrip.Items[13] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, new EventHandler(DocDeleteSubmenuItem_Click));
                                        }
                                    }
                                }
                            }

                            if (!docConfigPathExists)
                            {
                                gameContextMenuStrip.Items[12].Visible = false;
                                gameContextMenuStrip.Items[13].Visible = false;
                            }
                        }

                        if (i == 15)
                        {
                            (gameContextMenuStrip.Items[14] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameContextMenuStrip.Items[15] as ToolStripMenuItem).DropDownItems.Clear();
                            if (currentGameInfo.Game.DocumentsSavePath?.Length > 0)
                            {
                                if (profilePaths.Count > 0)
                                {
                                    foreach (string profilePath in profilePaths)
                                    {
                                        string currPath = Path.Combine(profilePath, currentGameInfo.Game.DocumentsSavePath);
                                        if (Directory.Exists(currPath))
                                        {
                                            if (!docSavePathExists)
                                            {
                                                docSavePathExists = true;
                                            }

                                            string nucPrefix = "";
                                            if (Directory.GetParent(Directory.GetParent(profilePath).ToString()).Name == "NucleusCoop")
                                            {
                                                nucPrefix = "Nucleus: ";
                                            }

                                            (gameContextMenuStrip.Items[14] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, new EventHandler(DocOpenSubmenuItem_Click));
                                            (gameContextMenuStrip.Items[15] as ToolStripMenuItem).DropDownItems.Add(nucPrefix + Directory.GetParent(profilePath).Name, null, new EventHandler(DocDeleteSubmenuItem_Click));
                                        }
                                    }
                                }
                            }

                            if (!docSavePathExists)
                            {
                                gameContextMenuStrip.Items[14].Visible = false;
                                gameContextMenuStrip.Items[15].Visible = false;
                            }
                        }

                        if (i == 16 && !userConfigPathExists && !userSavePathExists && !docConfigPathExists && !docSavePathExists)
                        {
                            gameContextMenuStrip.Items[7].Visible = false;
                        }

                        if (i == 1 && currentGameInfo.Game.Description == null)
                        {
                            gameContextMenuStrip.Items[i].Visible = false;
                            if (btnClick)
                            {
                                gameContextMenuStrip.Items[2].Visible = false;
                                i++;
                            }
                        }
                    }
                }
            }
            else
            {
                gameContextMenuStrip.Items[0].Text = "No game selected...";
                for (int i = 1; i < gameContextMenuStrip.Items.Count; i++)
                {
                    gameContextMenuStrip.Items[i].Visible = false;
                }
            }
        }

        private void UserProfileOpenSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = currentGameInfo.Game.UserProfileConfigPath;
            }
            else
            {
                pathSuffix = currentGameInfo.Game.UserProfileSavePath;
            }

            string path;
            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\", pathSuffix);
            }
            else
            {
                path = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), pathSuffix);
            }

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        private void UserProfileDeleteSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = currentGameInfo.Game.UserProfileConfigPath;
            }
            else
            {
                pathSuffix = currentGameInfo.Game.UserProfileSavePath;
            }

            string path;
            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{NucleusEnvironmentRoot}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\", pathSuffix);
            }
            else
            {
                path = Path.Combine(Environment.GetEnvironmentVariable("userprofile"), pathSuffix);
            }

            if (Directory.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                }
            }
        }

        private void DocOpenSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = currentGameInfo.Game.DocumentsConfigPath;
            }
            else
            {
                pathSuffix = currentGameInfo.Game.DocumentsSavePath;
            }

            string path;
            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\Documents", pathSuffix);
            }
            else
            {
                path = Path.Combine(DocumentsRoot, pathSuffix);
            }

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

        private void DocDeleteSubmenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            ToolStripItem parent = item.OwnerItem;

            string pathSuffix;
            if (parent.Text.Contains("Config"))
            {
                pathSuffix = currentGameInfo.Game.DocumentsConfigPath;
            }
            else
            {
                pathSuffix = currentGameInfo.Game.DocumentsSavePath;
            }

            string path;
            if (item.Text.StartsWith("Nucleus: "))
            {
                path = Path.Combine($@"{Path.GetDirectoryName(DocumentsRoot)}\NucleusCoop\{item.Text.Substring("Nucleus: ".Length)}\Documents", pathSuffix);
            }
            else
            {
                path = Path.Combine(DocumentsRoot, pathSuffix);
            }

            if (Directory.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                }
            }
        }

        public static Control FindControlAtPoint(Control container, Point pos)
        {
            Control child;
            foreach (Control c in container.Controls)
            {
                if (c.Visible && c.Bounds.Contains(pos))
                {
                    child = FindControlAtPoint(c, new Point(pos.X - c.Left, pos.Y - c.Top));
                    if (child == null)
                    {
                        return c;
                    }
                    else
                    {
                        return child;
                    }
                }
            }
            return null;
        }

        public static Control FindControlAtCursor(MainForm form)
        {
            Point pos = Cursor.Position;
            if (form.Bounds.Contains(pos))
            {
                return FindControlAtPoint(form, form.PointToClient(pos));
            }

            return null;
        }

        private void OpenRawGameScript()
        {
            try
            {
                Process.Start("notepad++.exe", "\"" + Path.Combine(gameManager.GetJsScriptsPath(), currentGameInfo.Game.JsFileName) + "\"");
            }
            catch
            {
                Process.Start("notepad.exe", Path.Combine(gameManager.GetJsScriptsPath(), currentGameInfo.Game.JsFileName));
            }
        }

        private void OpenScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenRawGameScript();
        }

        private void OpenDataFolder()
        {
            string path = Path.Combine(gameManager.GetAppContentPath(), currentGameInfo.Game.GUID);
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("No data present for this game.", "No data found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OpenDataFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenDataFolder();
        }

        private void ChangeIconToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "All Images Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe" +
                            "|PNG Portable Network Graphics (*.png)|*.png" +
                            "|JPEG File Interchange Format (*.jpg *.jpeg *jfif)|*.jpg;*.jpeg;*.jfif" +
                            "|BMP Windows Bitmap (*.bmp)|*.bmp" +
                            "|TIF Tagged Imaged File Format (*.tif *.tiff)|*.tif;*.tiff" +
                            "|Icon (*.ico)|*.ico" +
                            "|Executable (*.exe)|*.exe";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    // Create a new Bitmap object from the picture file on disk,
                    // and assign that to the PictureBox.Image property
                    //PictureBox1.Image = new Bitmap(dlg.FileName);

                    if (dlg.FileName.EndsWith(".exe"))
                    {
                        Icon icon = Shell32.GetIcon(dlg.FileName, false);

                        Bitmap bmp = icon.ToBitmap();
                        icon.Dispose();
                        currentGameInfo.Icon = bmp;
                    }
                    else
                    {
                        currentGameInfo.Icon = new Bitmap(dlg.FileName);
                    }
                    ini.IniWriteValue("GameIcons", currentGameInfo.Game.GameName, dlg.FileName);

                    GetIcon(currentGameInfo);
                    RefreshGames();
                }
            }
        }

        private void Log(string logMessage)
        {
            if (ini.IniReadValue("Misc", "DebugLog") == "True")
            {
                using (StreamWriter writer = new StreamWriter("debug-log.txt", true))
                {
                    writer.WriteLine($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}]MAIN: {logMessage}");
                    writer.Close();
                }
            }
        }

        private void ScriptNotesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show(currentGameInfo.Game.Description, "Handler Author's Notes", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void GameOptions_Click(object sender, EventArgs e)
        {
            Button btnSender = (Button)sender;
            Point ptLowerLeft = new Point(0, btnSender.Height);
            ptLowerLeft = btnSender.PointToScreen(ptLowerLeft);
            gameContextMenuStrip.Show(ptLowerLeft);
        }

        private void OpenOrigExePathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.GetDirectoryName(currentGameInfo.ExePath);
            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
            else
            {
                MessageBox.Show("Unable to open original executable path for this game.", "Not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void deleteContentFolderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string path = Path.Combine(gameManager.GetAppContentPath(), currentGameInfo.Game.GUID);
            if (Directory.Exists(path))
            {
                DialogResult dialogResult = MessageBox.Show("Are you sure you want to delete '" + path + "' and all its contents?", "Confirm deletion", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (dialogResult == DialogResult.Yes)
                {
                    Directory.Delete(path, true);
                }
            }
            else
            {
                MessageBox.Show("No data in content folder to delete.");
            }
        }

      
        private void btn_scriptAuthorTxt_Click(object sender, EventArgs e)
        {
            GameDesc_click = 0;
            inst_click++;
            txt_GameDescSizer.Visible = false;
            txt_GameDesc.Visible = false;
            scriptAuthorTxtSizer.Visible = true;
            scriptAuthorTxt.Visible = true;
            
            if (inst_click == 2)
            {
                scriptAuthorTxtSizer.Visible = false;
                scriptAuthorTxt.Visible = false;
                inst_click = 0;
            }
        }

        
        private void btn_GameDesc_Click(object sender, EventArgs e)
        {
            inst_click = 0;
            GameDesc_click++;
            scriptAuthorTxtSizer.Visible = false;
            scriptAuthorTxt.Visible = false;
            txt_GameDescSizer.Visible = true;
            txt_GameDesc.Visible = true;           

            if (GameDesc_click == 2)
            {
                txt_GameDescSizer.Visible = false;
                txt_GameDesc.Visible = false;
                GameDesc_click = 0;
            }
        }

        private void btn_Download_Click(object sender, EventArgs e)
        {
            getId.ShowDialog();
        }

      
        private void button_UpdateAvailable_Click(object sender, EventArgs e)
        {
             hubHandler = getId.GetHandler(currentGameInfo.Game.GetHubId());

            if (hubHandler == null)
            {
                MessageBox.Show("Error fetching update information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("An update to this handler is available, download it?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (dialogResult == DialogResult.Yes)
                {
                    currentlyUpdatingScript = true;

                    DeleteGame(true);
                    StepPanel.Visible = false;
                    label_StepTitle.Text = "Select a game";
                    btn_Play.Enabled = false;
                    btn_Next.Enabled = false;
                    btn_gameOptions.Visible = false;
                    button_UpdateAvailable.Visible = false;
                    stepsList.Clear();
                   
                    downloadPrompt = new DownloadPrompt(hubHandler, this, null, true);
                    list_Games.SuspendLayout();
                    downloadPrompt.ShowDialog();
                    list_Games.ResumeLayout();
                    StepPanel.Visible = true;
                   
                }
            }
        } 

        private void MainForm_Paint(object sender, PaintEventArgs e)
        {         

        }
        private void MainForm_MouseHover(object sender, EventArgs e)
        {
            if (!currentlyUpdatingScript)
            {
                button_UpdateAvailable.Visible = currentGameInfo?.Game?.IsUpdateAvailable(false) ?? button_UpdateAvailable.Visible;
                
            }
        }
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            Process.Start(faq_link);
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            Process.Start(faq_link);
        }
        private void button1_Click(object sender, EventArgs e)
        {
            Process.Start("https://discord.com/invite/QDUt8HpCvr");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("https://www.reddit.com/r/nucleuscoop/");
        }
        private void logo_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases");
        }
        private void link_faq_Click(object sender, EventArgs e)
        {
            Process.Start(faq_link);
        }
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/nefarius/ScpToolkit/releases");
            third_party_tools_container.Visible = false;
            thirdPartytools_click = 0;
        }
        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/ViGEm/HidHide/releases");    
            third_party_tools_container.Visible = false;
            thirdPartytools_click = 0;
        }
        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Ryochan7/DS4Windows/releases");
            third_party_tools_container.Visible = false;
            thirdPartytools_click = 0;
        }
        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/csutorasa/XOutput/releases");   
            third_party_tools_container.Visible = false;
            thirdPartytools_click = 0;
        }
        private void linkLabel5_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://discord.com/invite/QDUt8HpCvr");
        }
        private void btn_thirdPartytools_Click(object sender, EventArgs e)
        {
            thirdPartytools_click++;
            third_party_tools_container.Visible = true;
            if (thirdPartytools_click == 2)
            {
                third_party_tools_container.Visible = false;
                thirdPartytools_click = 0;
            }
        }
        private void btn_Extract_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog
            {
                Title = "Select a game handler to extract",
                DefaultExt = "nc",
                InitialDirectory = Gaming.GameManager.Instance.GetJsScriptsPath(),
                Filter = "nc files (*.nc)|*.nc"
            };

            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                DownloadPrompt downloadPrompt = new DownloadPrompt(null, this, ofd.FileName);
                downloadPrompt.ShowDialog();
            }
        }
        private void btn_SplitCalculator_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start((Path.Combine(Application.StartupPath, @"utils\SplitCalculator\SplitCalculator.exe")));
            }
            catch(Exception)
            {
                MessageBox.Show(@"SplitCalculator.exe has not been found in the utils\SplitCalculator folder. Try again with a fresh Nucleus Co-op installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
           // offline = ini.IniReadValue("Dev", "OfflineMod");

            //if (offline == "Off")
            //{
                CheckNetCon();
           // }
        }

        private void button1_Click_2(object sender, EventArgs e)
        {
            Process.Start("https://hub.splitscreen.me/");
        }
    }
}
