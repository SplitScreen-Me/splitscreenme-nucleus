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
using System.Net.NetworkInformation;
using Nucleus.Gaming.Coop.Generic;
using System.Linq;
using Microsoft.Win32;
using System.Drawing.Text;
using System.ComponentModel;
using System.Drawing.Imaging;
using System.Security.Principal;

namespace Nucleus.Coop
{
    /// <summary>
    /// Central UI class to the Nucleus Coop application
    /// </summary>
    public partial class MainForm : BaseForm, IDynamicSized
    {
        private readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        private IniFile iconsIni;
        public string version = "v" + Globals.Version;
        protected string faq_link = "https://www.splitscreen.me/docs/faq";
        protected string api = "https://hub.splitscreen.me/api/v1/";
        private string NucleusEnvironmentRoot = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        private string DocumentsRoot = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        public string ChoosenTheme;
        public string themePath;
        public string customFont;
        private string currentGameSetup;
        public IniFile theme;
        public Form splashscreen = new Splashscreen();
        public Form backgroundForm;
        private Settings settingsForm = null;
        private ContentManager content;
        private IGameHandler handler;
        private GameManager gameManager;
        private Dictionary<UserGameInfo, GameControl> controls;
        private SearchDisksForm searchDisksForm = null;
        private GameControl currentControl;
        private UserGameInfo currentGameInfo;
        private GenericGameInfo currentGame;
        public HubShowcase hubShowcase;
        private GameProfile currentProfile;
        private AssetsScraper getAssets;
        private List<UserInputControl> stepsList;
        private UserInputControl currentStep;
        public PositionsControl positionsControl;
        private PlayerOptionsControl optionsControl;
        private JSUserInputControl jsControl;
        private Handler hubHandler = null;
        private ScriptDownloader scriptDownloader;
        private DownloadPrompt downloadPrompt;
        private SoundPlayer splayer;
        private int KillProcess_HotkeyID = 1;
        private int TopMost_HotkeyID = 2;
        private int StopSession_HotkeyID = 3;
        private int SetFocus_HotkeyID = 4;
        private int currentStepIndex;
        public int blurValue;
        private List<string> profilePaths = new List<string>();
        private List<Control> ctrls = new List<Control>();
        private List<Form> backgroundForms = new List<Form>();
        private IDictionary<string, bool> updateStatuts;
        private Thread handlerThread;
        public Action<IntPtr> RawInputAction { get; set; }

        private Bitmap defBackground;
        private Bitmap coverImg;
        private Bitmap screenshotImg;
        public Bitmap AppButtons;
        private Bitmap k_Icon;
        private Bitmap xinputGamepad_Icon;
        private Bitmap dinputGamepad_Icon;
        private Bitmap favorite_Unselected;
        private Bitmap favorite_Selected;
        public bool connected;
        public bool themeSwith = false;
        private bool currentlyUpdatingScript = false;
        private bool Splash_On;
        private bool TopMostToggle = true;
        private bool formClosing;
        private bool noGamesPresent;
        public bool mouseClick;
        public bool roundedcorners;
        public bool useButtonsBorder;
        private bool DisableOfflineIcon;
        private bool showFavoriteOnly;
        private bool canResize = false;
        private Color ChoosenColor;
        public float cursScale;
        private System.Windows.Forms.Timer DisposeTimer;//dispose splash screen timer
        private System.Windows.Forms.Timer slideshow;
        private System.Windows.Forms.Timer loadTimer;
        private System.Windows.Forms.Timer rainbowTimer;

        public string[] rgb_font;
        public string[] rgb_MouseOverColor;
        public string[] rgb_MenuStripBackColor;
        public string[] rgb_MenuStripFontColor;
        public string[] rgb_TitleBarColor;
        public string[] rgb_HandlerNoteTitleFont;
        public string[] rgb_HandlerNoteFontColor;
        public string[] rgb_HandlerNoteTitleFontColor;
        public string[] rgb_ButtonsBorderColor;
        public string[] rgb_ThirdPartyToolsLinks;
        public string[] rgb_HandlerNoteBackColor;
        public string[] rgb_HandlerNoteMagnifierTitleBackColor;

        public Color TitleBarColor;
        public Color MouseOverBackColor;
        public Color MenuStripBackColor;
        public Color MenuStripFontColor;
        public Color ButtonsBorderColor;
        private Color HandlerNoteBackColor;
        private Color HandlerNoteFontColor;
        private Color HandlerNoteMagnifierTitleBackColor;
        private Color HandlerNoteTitleFont;

        public FileInfo fontPath;
        private Label inputsIconsDesc;
        private UI.InputIcons icon1;
        private UI.InputIcons icon2;
        private UI.InputIcons icon3;
        private UI.InputIcons icon4;
        private UI.InputIcons icon5;
        private UI.InputIcons icon6;

        private Label handlerUpdateLabel;
        private Label favoriteOnlyLabel;
        private PictureBox favoriteOnly;
        public Cursor hand_Cursor;
        public Cursor default_Cursor;

        public void RefreshNetStatut()
        {
            try
            {
                Ping myPing = new Ping();
                String host = "hub.splitscreen.me";
                byte[] buffer = new byte[32];
                int timeout = 1000;
                PingOptions pingOptions = new PingOptions();
                PingReply reply = myPing.Send(host, timeout, buffer, pingOptions);
                if (reply.Status == IPStatus.Success)
                {
                    connected = true;
                    CheckHandlersUpdate(true);
                    myPing.Dispose();
                    return;
                }
                else
                {
                    connected = false;
                    myPing.Dispose();
                    return;
                }
            }
            catch (Exception)
            {
                connected = false;
                return;
            }
        }

        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        private IntPtr GetHandleWindow(string title)
        {
            return FindWindow(null, title);
        }

        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
           int nLeftRect,     // x-coordinate of upper-left corner
           int nTopRect,      // y-coordinate of upper-left corner
           int nRightRect,    // x-coordinate of lower-right corner
           int nBottomRect,   // y-coordinate of lower-right corner
           int nWidthEllipse, // width of ellipse
           int nHeightEllipse // height of ellipse
        );

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

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowEnum flags);

        public void SoundPlayer(string filePath)
        {
            splayer = new SoundPlayer(filePath);
            splayer.Play();
            splayer.Dispose();
        }

        public void button_Click(object sender, EventArgs e)
        {
            if (mouseClick)
            {
                SoundPlayer(themePath + "\\button_click.wav");
            }
        }

        private void minimizeButton(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void maximizeButton(object sender, EventArgs e)
        {
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }

        private void MainForm_ClientSizeChanged(object sender, EventArgs e)
        {
            Invalidate();

            if (roundedcorners)
            {
                if (WindowState == FormWindowState.Maximized)
                {
                    Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 0, 0));
                    clientAreaPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, clientAreaPanel.Width, clientAreaPanel.Height, 0, 0));
                    glowingLine0.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, clientAreaPanel.Width, clientAreaPanel.Height, 0, 0));
                }
                else
                {
                    Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
                    clientAreaPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, clientAreaPanel.Width, clientAreaPanel.Height, 20, 20));
                    glowingLine0.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, clientAreaPanel.Width, clientAreaPanel.Height, 20, 20));
                }
            }

            if (settingsForm != null) settingsForm.Visible = false;
            if (searchDisksForm != null) searchDisksForm.Visible = false;

            if (positionsControl != null)
            {
                positionsControl.textZoomContainer.Visible = false;
                btn_magnifier.Image = new Bitmap(themePath + "\\magnifier.png");
            }
        }

        private void MainForm_ResizeBegin(object sender, EventArgs e)
        {
            foreach (Control titleBarButtons in Controls)
            {
                titleBarButtons.Visible = false;
            }

            btn_Links.BackgroundImage = new Bitmap(themePath + "\\title_dropdown_closed.png");
            clientAreaPanel.Visible = false;
            Opacity = 0.6D;
        }

        private void MainForm_ResizeEnd(object sender, EventArgs e)
        {
            foreach (Control titleBarButtons in Controls)
            {
                titleBarButtons.Visible = titleBarButtons.Name != "third_party_tools_container" && titleBarButtons.Name != "linksPanel";
            }

            if (connected) { btn_noHub.Visible = false; }

            clientAreaPanel.Visible = true;
            Opacity = 1.0D;
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
            try
            {
                connected = Program.connected;
                ChoosenTheme = ini.IniReadValue("Theme", "Theme");
                theme = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\theme\\" + ChoosenTheme, "theme.ini"));
                iconsIni = new IniFile(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\icons.ini"));
                Splash_On = Convert.ToBoolean(ini.IniReadValue("Dev", "SplashScreen_On"));
                DisableOfflineIcon = Convert.ToBoolean(ini.IniReadValue("Dev", "DisableOfflineIcon"));
                showFavoriteOnly = Convert.ToBoolean(ini.IniReadValue("Dev", "ShowFavoriteOnly"));
                themePath = Path.Combine(Application.StartupPath, @"gui\theme\" + ChoosenTheme);
                mouseClick = Convert.ToBoolean(ini.IniReadValue("Dev", "MouseClick"));
                roundedcorners = Convert.ToBoolean(theme.IniReadValue("Misc", "UseRoundedCorners"));
                useButtonsBorder = Convert.ToBoolean(theme.IniReadValue("Misc", "UseButtonsBorder"));
                customFont = theme.IniReadValue("Font", "FontFamily");
                rgb_font = theme.IniReadValue("Colors", "Font").Split(',');
                rgb_MouseOverColor = theme.IniReadValue("Colors", "MouseOver").Split(',');
                rgb_MenuStripBackColor = theme.IniReadValue("Colors", "MenuStripBack").Split(',');
                rgb_MenuStripFontColor = theme.IniReadValue("Colors", "MenuStripFont").Split(',');
                rgb_TitleBarColor = theme.IniReadValue("Colors", "TitleBar").Split(',');
                rgb_HandlerNoteBackColor = theme.IniReadValue("Colors", "HandlerNoteBack").Split(',');
                rgb_HandlerNoteFontColor = theme.IniReadValue("Colors", "HandlerNoteFont").Split(',');
                rgb_HandlerNoteTitleFontColor = theme.IniReadValue("Colors", "HandlerNoteTitleFont").Split(',');
                rgb_ButtonsBorderColor = theme.IniReadValue("Colors", "ButtonsBorder").Split(',');
                rgb_HandlerNoteMagnifierTitleBackColor = theme.IniReadValue("Colors", "HandlerNoteMagnifierTitleBackColor ").Split(',');
                blurValue = Convert.ToInt32(ini.IniReadValue("Dev", "Blur"));
                float fontSize = float.Parse(theme.IniReadValue("Font", "MainFontSize"));
                bool coverBorderOff = Convert.ToBoolean(theme.IniReadValue("Misc", "DisableCoverBorder"));
                bool noteBorderOff = Convert.ToBoolean(theme.IniReadValue("Misc", "DisableNoteBorder"));

                TitleBarColor = Color.FromArgb(Convert.ToInt32(rgb_TitleBarColor[0]), Convert.ToInt32(rgb_TitleBarColor[1]), Convert.ToInt32(rgb_TitleBarColor[2]));
                MouseOverBackColor = Color.FromArgb(Convert.ToInt32(rgb_MouseOverColor[0]), Convert.ToInt32(rgb_MouseOverColor[1]), Convert.ToInt32(rgb_MouseOverColor[2]));
                MenuStripBackColor = Color.FromArgb(Convert.ToInt32(rgb_MenuStripBackColor[0]), Convert.ToInt32(rgb_MenuStripBackColor[1]), Convert.ToInt32(rgb_MenuStripBackColor[2]));
                MenuStripFontColor = Color.FromArgb(Convert.ToInt32(rgb_MenuStripFontColor[0]), Convert.ToInt32(rgb_MenuStripFontColor[1]), Convert.ToInt32(rgb_MenuStripFontColor[2]));
                HandlerNoteBackColor = Color.FromArgb(Convert.ToInt32(rgb_HandlerNoteBackColor[0]), Convert.ToInt32(rgb_HandlerNoteBackColor[1]), Convert.ToInt32(rgb_HandlerNoteBackColor[2]));
                HandlerNoteFontColor = Color.FromArgb(Convert.ToInt32(rgb_HandlerNoteFontColor[0]), Convert.ToInt32(rgb_HandlerNoteFontColor[1]), Convert.ToInt32(rgb_HandlerNoteFontColor[2]));
                HandlerNoteMagnifierTitleBackColor = Color.FromArgb(Convert.ToInt32(rgb_HandlerNoteMagnifierTitleBackColor[0]), Convert.ToInt32(rgb_HandlerNoteMagnifierTitleBackColor[1]), Convert.ToInt32(rgb_HandlerNoteMagnifierTitleBackColor[2]));
                HandlerNoteTitleFont = Color.FromArgb(Convert.ToInt32(rgb_HandlerNoteTitleFontColor[0]), Convert.ToInt32(rgb_HandlerNoteTitleFontColor[1]), Convert.ToInt32(rgb_HandlerNoteTitleFontColor[2]));
                ButtonsBorderColor = Color.FromArgb(Convert.ToInt32(rgb_ButtonsBorderColor[0]), Convert.ToInt32(rgb_ButtonsBorderColor[1]), Convert.ToInt32(rgb_ButtonsBorderColor[2]));

                InitializeComponent();

                SuspendLayout();

                default_Cursor = new Cursor(themePath + "\\cursor.ico");
                hand_Cursor = new Cursor(themePath + "\\cursor_hand.ico");

                Cursor = default_Cursor;

                if (roundedcorners)
                {
                    Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
                    clientAreaPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, clientAreaPanel.Width, clientAreaPanel.Height, 20, 20));
                }

                BackColor = TitleBarColor;
                linksPanel.BackColor = BackColor;
                Font = new Font(customFont, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                ForeColor = Color.FromArgb(Convert.ToInt32(rgb_font[0]), Convert.ToInt32(rgb_font[1]), Convert.ToInt32(rgb_font[2]));
                scriptAuthorTxt.BackColor = HandlerNoteBackColor;
                scriptAuthorTxt.ForeColor = HandlerNoteFontColor;
                scriptAuthorTxtSizer.BackColor = Color.Transparent;
                scriptAuthorTxtSizer.BackgroundImage = new Bitmap(themePath + "\\handlerNote_background.png");
                HandlerNoteTitle.ForeColor = HandlerNoteTitleFont;

                //Controls Pictures
                AppButtons = new Bitmap(themePath + "\\button.png");
                clientAreaPanel.BackgroundImage = new Bitmap(themePath + "\\background.jpg");
                mainButtonFrame.BackgroundImage = new Bitmap(themePath + "\\main_buttons_frame.png");
                rightFrame.BackgroundImage = new Bitmap(themePath + "\\right_panel.png");
                game_listSizer.BackgroundImage = new Bitmap(themePath + "\\game_list.png");
                btn_textSwitcher.BackgroundImage = new Bitmap(themePath + "\\text_switcher.png");
                btnAutoSearch.BackgroundImage = AppButtons;
                button_UpdateAvailable.BackgroundImage = AppButtons;
                btnSearch.BackgroundImage = AppButtons;
                btn_gameOptions.BackgroundImage = AppButtons;
                btn_Download.BackgroundImage = AppButtons;
                btn_Play.BackgroundImage = AppButtons;
                btn_Extract.BackgroundImage = AppButtons;
                btn_gameOptions.BackgroundImage = new Bitmap(themePath + "\\game_options.png");
                btnBack.BackgroundImage = new Bitmap(themePath + "\\arrow_left.png");
                btn_Next.BackgroundImage = new Bitmap(themePath + "\\arrow_right.png");
                coverFrame.BackgroundImage = new Bitmap(themePath + "\\cover_layer.png");
                stepPanelPictureBox.Image = new Bitmap(themePath + "\\logo.png");
                logo.BackgroundImage = new Bitmap(themePath + "\\title_logo.png");
                btn_Discord.BackgroundImage = new Bitmap(themePath + "\\discord.png");
                btn_downloadAssets.BackgroundImage = new Bitmap(themePath + "\\title_download_assets.png");
                btn_faq.BackgroundImage = new Bitmap(themePath + "\\faq.png");
                btn_Links.BackgroundImage = new Bitmap(themePath + "\\title_dropdown_closed.png");
                btn_noHub.BackgroundImage = new Bitmap(themePath + "\\title_no_hub.png");
                btn_reddit.BackgroundImage = new Bitmap(themePath + "\\reddit.png");
                btn_SplitCalculator.BackgroundImage = new Bitmap(themePath + "\\splitcalculator.png");
                btn_thirdPartytools.BackgroundImage = new Bitmap(themePath + "\\thirdpartytools.png");
                closeBtn.BackgroundImage = new Bitmap(themePath + "\\title_close.png");
                maximizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_maximize.png");
                minimizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_minimize.png");
                btn_settings.BackgroundImage = new Bitmap(themePath + "\\title_settings.png");
                btn_dlFromHub.BackgroundImage = AppButtons;
                glowingLine0.Image = new Bitmap(themePath + "\\lightbar_top.gif");
                StepPanel.BackgroundImage = new Bitmap(themePath + "\\setup_screen.png");
                btn_magnifier.Image = new Bitmap(themePath + "\\magnifier.png");
                k_Icon = new Bitmap(themePath + "\\keyboard_icon.png");
                xinputGamepad_Icon = new Bitmap(themePath + "\\xinput_icon.png");
                dinputGamepad_Icon = new Bitmap(themePath + "\\dinput_icon.png");
                favorite_Unselected = new Bitmap(themePath + "\\favorite_unselected.png");
                favorite_Selected = new Bitmap(themePath + "\\favorite_selected.png");

                btn_Extract.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnAutoSearch.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                button_UpdateAvailable.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnSearch.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_gameOptions.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Download.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Play.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btnBack.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_Next.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                btn_dlFromHub.FlatAppearance.MouseOverBackColor = MouseOverBackColor;
                gameContextMenuStrip.BackColor = MenuStripBackColor;
                gameContextMenuStrip.ForeColor = MenuStripFontColor;

                if (useButtonsBorder)
                {
                    btnAutoSearch.FlatAppearance.BorderSize = 1;
                    btnAutoSearch.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btnSearch.FlatAppearance.BorderSize = 1;
                    btnSearch.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_gameOptions.FlatAppearance.BorderSize = 1;
                    btn_gameOptions.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_Download.FlatAppearance.BorderSize = 1;
                    btn_Download.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_Play.FlatAppearance.BorderSize = 1;
                    btn_Play.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_Extract.FlatAppearance.BorderSize = 1;
                    btn_Extract.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btnBack.FlatAppearance.BorderSize = 1;
                    btnBack.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_Next.FlatAppearance.BorderSize = 1;
                    btn_Next.FlatAppearance.BorderColor = ButtonsBorderColor;
                    btn_dlFromHub.FlatAppearance.BorderSize = 1;
                    btn_dlFromHub.FlatAppearance.BorderColor = ButtonsBorderColor;
                }

                linksPanel.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, linksPanel.Width, linksPanel.Height, 15, 15));
                third_party_tools_container.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, third_party_tools_container.Width, third_party_tools_container.Height, 10, 10));
                scriptAuthorTxtSizer.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, scriptAuthorTxtSizer.Width, scriptAuthorTxtSizer.Height, 20, 20));

                btn_magnifier.Cursor = hand_Cursor;
                linkLabel1.Cursor = hand_Cursor;
                linkLabel2.Cursor = hand_Cursor;
                linkLabel3.Cursor = hand_Cursor;
                linkLabel4.Cursor = hand_Cursor;
                gameContextMenuStrip.Cursor = hand_Cursor;

                if (coverBorderOff)
                {
                    cover.BorderStyle = BorderStyle.None;
                }

                if (noteBorderOff)
                {
                    scriptAuthorTxtSizer.BorderStyle = BorderStyle.None;
                }

                foreach (Control titleBarButtons in Controls)
                {
                    titleBarButtons.BackColor = BackColor;//avoid "glitchs" while maximizing the window (aesthetic stuff only)            
                }

                controlscollect();

                foreach (Control control in ctrls)
                {
                    if (control.Name != "btn_Links" && control.Name != "btn_thirdPartytools" && control.Name != "HandlerNoteTitle")//Close "third_party_tools_container" control when an other control in the form is clicked.
                    {
                        control.Font = new Font(customFont, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                        control.Click += new EventHandler(this.this_Click);
                    }

                    if (control.GetType() == typeof(Button))
                    {
                        control.Cursor = hand_Cursor;
                    }

                    control.Click += new EventHandler(button_Click);

                    if (mouseClick)
                    {
                        handleClickSound(true);
                    }
                }

                txt_version.Text = version;

                ResumeLayout();

                minimizeBtn.Click += new EventHandler(this.minimizeButton);
                maximizeBtn.Click += new EventHandler(this.maximizeButton);
                closeBtn.Click += new EventHandler(this.closeButton);

                defBackground = new Bitmap(clientAreaPanel.BackgroundImage);

                positionsControl = new PositionsControl();

                positionsControl.textZoomContainer.BackColor = HandlerNoteMagnifierTitleBackColor;
                positionsControl.handlerNoteZoom.BackColor = HandlerNoteBackColor;
                positionsControl.handlerNoteZoom.ForeColor = HandlerNoteFontColor;
                positionsControl.playerSetup_btn.Click += new EventHandler(this.SettingsBtn_Click);
                positionsControl.playerSetup_btn.Click += new EventHandler(this.positionsControlPlayerSetup_Click);
                positionsControl.OnCanPlayUpdated += StepCanPlay;
                positionsControl.Click += new EventHandler(this_Click);

                settingsForm = new Settings(this, positionsControl);
                searchDisksForm = new SearchDisksForm(this);

                clientAreaPanel.Controls.Add(settingsForm);
                clientAreaPanel.Controls.Add(searchDisksForm);
                positionsControl.Paint += PositionsControl_Paint;

                settingsForm.RegHotkeys(this);

                controls = new Dictionary<UserGameInfo, GameControl>();
                gameManager = new GameManager(this);

                optionsControl = new PlayerOptionsControl();
                jsControl = new JSUserInputControl();

                optionsControl.OnCanPlayUpdated += StepCanPlay;
                jsControl.OnCanPlayUpdated += StepCanPlay;

                scriptDownloader = new ScriptDownloader(this);
                downloadPrompt = new DownloadPrompt(hubHandler, this, null, true);

                updateStatuts = new Dictionary<string, bool>();

                inputsIconsDesc = new Label();
                inputsIconsDesc.AutoSize = true;
                inputsIconsDesc.Visible = false;
                inputsIconsDesc.BackColor = Color.Transparent;
                inputsIconsDesc.ForeColor = Color.FromArgb(Convert.ToInt32(rgb_font[0]), Convert.ToInt32(rgb_font[1]), Convert.ToInt32(rgb_font[2]));
                inputsIconsDesc.Anchor = AnchorStyles.Top | AnchorStyles.Left;
                inputsIconsDesc.Font = new Font(customFont, 9.25f, FontStyle.Regular, GraphicsUnit.Pixel, 0);

                inputsIconsDesc.Text = "";

                rightFrame.Controls.Add(inputsIconsDesc);

                favoriteOnlyLabel = new Label
                {
                    AutoSize = true,
                    Text = "Favorite Games",
                    BackgroundImage = AppButtons,
                    BackColor = Color.Transparent,
                    ForeColor = this.ForeColor,
                };

                favoriteOnly = new PictureBox
                {
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    BackColor = Color.Transparent,
                    Cursor = hand_Cursor
                };

                favoriteOnly.Image = showFavoriteOnly ? favorite_Selected : favorite_Unselected;
                favoriteOnly.Click += new EventHandler(FavoriteOnly_Click);

                mainButtonFrame.Controls.Add(favoriteOnlyLabel);
                mainButtonFrame.Controls.Add(favoriteOnly);

                list_Games.Select();
                gameManager.ReorderUserProfile();

            }
            catch (Exception ex)
            {
                Log("ERROR - " + ex.Message + "\n\nSTACKTRACE: " + ex.StackTrace);
            }

            if (connected)
            {
                CheckHandlersUpdate(false);
            }

            CenterToScreen();

            DPIManager.Register(this);
            DPIManager.AddForm(this);
        }

        private void FavoriteOnly_Click(object sender, EventArgs e)
        {
            bool selected = favoriteOnly.Image.Equals(favorite_Selected);

            if (selected)
            {
                favoriteOnly.Image = favorite_Unselected;
                showFavoriteOnly = false;
            }
            else
            {
                favoriteOnly.Image = favorite_Selected;
                showFavoriteOnly = true;
            }

            ini.IniWriteValue("Dev", "ShowFavoriteOnly", showFavoriteOnly.ToString());
            RefreshGames();
        }

        public new void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            SuspendLayout();

            float newFontSize = Font.Size * scale;
            float mainButtonFrameFont = mainButtonFrame.Font.Size * 1.0f;

            if (scale > 1.0f)
            {
                foreach (Control button in mainButtonFrame.Controls)
                {
                    button.Font = new Font(customFont, mainButtonFrameFont, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                }
            }

            btn_Play.Font = new Font(customFont, mainButtonFrameFont, FontStyle.Bold, GraphicsUnit.Pixel, 0);
            scriptAuthorTxt.Font = new Font(customFont, newFontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            favoriteOnlyLabel.Font = new Font(customFont, mainButtonFrameFont, FontStyle.Regular, GraphicsUnit.Pixel, 0);
            favoriteOnlyLabel.Location = new Point(1, mainButtonFrame.Height / 2 - (favoriteOnlyLabel.Height / 2) * (int)scale);
            favoriteOnly.Size = new Size(favoriteOnlyLabel.Height, favoriteOnlyLabel.Height);
            float favoriteY = favoriteOnlyLabel.Right + (5 * scale);
            favoriteOnly.Location = new Point(Convert.ToInt32(favoriteY), mainButtonFrame.Height / 2 - (favoriteOnly.Height / 2) * (int)scale);
            cursScale = scale;
            ResumeLayout();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            RefreshGames();
            DisposeTimer = new System.Windows.Forms.Timer();//dispose splash screen and keep track on hub ping statut( refresh )
            DisposeTimer.Interval = (2900); //millisecond
            DisposeTimer.Tick += new EventHandler(MainTimerTick);
            DisposeTimer.Start();

            if (Splash_On)
            {
                splashscreen.Show();
            }

            DPIManager.ForceUpdate();
        }

        public void handleClickSound(bool enable)
        {
            mouseClick = enable;
        }

        private void MainTimerTick(Object Object, EventArgs EventArgs)
        {
            if (Splash_On)
            {
                splashscreen.Dispose();
            }

            if (connected)
            {
                btn_noHub.Visible = false;
                btn_downloadAssets.Enabled = true;
                btn_Download.Enabled = true;
                DisposeTimer.Dispose();
            }
            else
            {
                if (!DisableOfflineIcon) { btn_noHub.Visible = true; }
                btn_downloadAssets.Enabled = false;
                btn_Download.Enabled = false;
            }
        }

        private void CheckHandlersUpdate(bool refresh)
        {
            List<UserGameInfo> games;
            games = gameManager.User.Games;

            if (games.Count > 0)
            {
                List<UserGameInfo> _games = gameManager.User.Games;
                updateStatuts.Clear();

                for (int i = 0; i <= _games.Count - 1; i++)
                {
                    UserGameInfo game = _games[i];
                    if (game.Game != null)
                    {
                        try
                        {
                            Handler checkUpdateHandler = scriptDownloader.GetHandler(game.Game.GetHandlerId());
                            updateStatuts.Add(game.Game.GUID, game.Game.IsUpdateAvailable(true));
                        }
                        catch (Exception)
                        { }
                    }
                }
            }

            if (refresh)
            {
                gameManager.ReorderUserProfile();
                RefreshGames();
            }
        }

        private void TriggerHubShowCase()
        {
            hubShowcase = new HubShowcase(this);
            hubShowcase.Location = new Point(game_listSizer.Right, mainButtonFrame.Bottom + (clientAreaPanel.Height / 2 - hubShowcase.Height / 2));
            hubShowcase.Size = new Size(hubShowcase.Width - 40, hubShowcase.Height);
            hubShowcase.Visible = true;
            stepPanelPictureBox.Visible = false;
            clientAreaPanel.Controls.Add(hubShowcase);
        }

        private void PositionsControl_Paint(object sender, PaintEventArgs e)
        {
            if (positionsControl.isDisconnected)
            {
                DPIManager.ForceUpdate();
                positionsControl.isDisconnected = false;
            }
        }

        private void positionsControlPlayerSetup_Click(object sender, EventArgs e)
        {
            settingsForm.settingsTab_Group.SelectedIndex = 1;
        }

        protected override Size DefaultSize => new Size(1050, 701);

        protected override void WndProc(ref Message m)
        {
            //TODO: if close message, kill application not just window// Guess no since we need to catch the form closing event
            const int RESIZE_HANDLE_SIZE = 10;

            if (this.WindowState == FormWindowState.Normal)
            {
                switch (m.Msg)//resizing stuffs
                {
                    case 0x0084/*NCHITTEST*/ :
                        base.WndProc(ref m);

                        if ((int)m.Result == 0x01/*HTCLIENT*/)
                        {
                            Point screenPoint = new Point(m.LParam.ToInt32());
                            Point clientPoint = this.PointToClient(screenPoint);

                            if (clientPoint.Y <= RESIZE_HANDLE_SIZE)
                            {
                                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                    m.Result = (IntPtr)13/*HTTOPLEFT*/ ;
                                else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                    m.Result = (IntPtr)12/*HTTOP*/ ;
                                else
                                    m.Result = (IntPtr)14/*HTTOPRIGHT*/ ;
                            }
                            else if (clientPoint.Y <= (Size.Height - RESIZE_HANDLE_SIZE))
                            {
                                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                    m.Result = (IntPtr)10/*HTLEFT*/ ;
                                else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                    m.Result = (IntPtr)2/*HTCAPTION*/ ;
                                else
                                    m.Result = (IntPtr)11/*HTRIGHT*/ ;
                            }
                            else
                            {
                                if (clientPoint.X <= RESIZE_HANDLE_SIZE)
                                    m.Result = (IntPtr)16/*HTBOTTOMLEFT*/ ;
                                else if (clientPoint.X < (Size.Width - RESIZE_HANDLE_SIZE))
                                    m.Result = (IntPtr)15/*HTBOTTOM*/ ;
                                else
                                    m.Result = (IntPtr)17/*HTBOTTOMRIGHT*/ ;
                            }
                        }

                        return;
                }
            }

            Point cursorPos = PointToClient(Cursor.Position);
            Rectangle inRect = new Rectangle(10, 10, Width - 20, Height - 20);

            if (!inRect.Contains(cursorPos))
            {
                canResize = true;
            }
            else
            {
                if (Cursor.Current != default_Cursor)
                {
                    Cursor.Current = default_Cursor;

                }
                canResize = false;
            }

            if (!canResize )
            {
                if (m.Msg == 0x020)//Do not reset our custom cursor when mouse hover over the Form background(needed because of the custom resizing/moving messages handling) 
                {
                    m.Result = IntPtr.Zero;
                    return;
                }
            }

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
                IntPtr splitHandle = GetHandleWindow("SplitForm");

                if (!Gaming.Coop.InputManagement.LockInput.IsLocked)
                {
                    if (TopMostToggle && handler != null)
                    {
                        try
                        {
                            if (splitHandle != null && splitHandle != IntPtr.Zero)
                            {
                                User32Interop.SetWindowPos(splitHandle, new IntPtr(-2), 0, 0, 0, 0,
                                (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                                ShowWindow(splitHandle, ShowWindowEnum.Minimize);
                            }

                            Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentGame.ExecutableName.ToLower()));
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
                        { }

                        User32Util.ShowTaskBar();
                        Activate();
                        BringToFront();
                        TopMostToggle = false;
                    }
                    else if (!TopMostToggle && handler != null)
                    {
                        if (splitHandle != null && splitHandle != IntPtr.Zero)
                        {
                            ShowWindow(splitHandle, ShowWindowEnum.Restore);
                            User32Interop.SetWindowPos(splitHandle, new IntPtr(-1), 0, 0, 0, 0,
                                (uint)(PositioningFlags.SWP_NOSIZE | PositioningFlags.SWP_NOMOVE));
                        }

                        Process[] procs = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentGame.ExecutableName.ToLower()));
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
            else if (m.Msg == 0x0312 && m.WParam.ToInt32() == SetFocus_HotkeyID)
            {
                if (!Gaming.Coop.InputManagement.LockInput.IsLocked)
                {
                    IntPtr nucHwnd = User32Interop.FindWindow(null, "Nucleus Co-op");
                    ShowWindow(nucHwnd, ShowWindowEnum.Restore);
                    if (nucHwnd != IntPtr.Zero)
                    {
                        User32Interop.SetForegroundWindow(nucHwnd);
                        Log("Toggle unfocus");
                    }
                }
            }

            base.WndProc(ref m);
        }

        public void RefreshGames()
        {
            List<UserGameInfo> games;

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

                games = gameManager.User.Games;

                for (int i = 0; i < games.Count; i++)
                {
                    UserGameInfo game = games[i];
                    NewUserGame(game);
                }

                if (games.Count == 0)
                {
                    noGamesPresent = true;
                    GameControl con = new GameControl(null, null, false, false)
                    {
                        Width = game_listSizer.Width,
                        Text = "No games",
                        Font = this.Font,
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

            bool updateAvailable = false;
            bool favorite = game.Favorite;
            if (updateStatuts.ContainsKey(game.Game.GUID))
            {
                updateAvailable = updateStatuts[game.Game.GUID];
            }

            GameControl con = new GameControl(game.Game, game, updateAvailable, favorite)
            {
                Width = game_listSizer.Width,
            };

            if (showFavoriteOnly)
            {
                if (favorite)
                {
                    controls.Add(game, con);
                    list_Games.Controls.Add(con);
                    ThreadPool.QueueUserWorkItem(GetIcon, game);
                }
            }
            else
            {
                controls.Add(game, con);
                list_Games.Controls.Add(con);
                ThreadPool.QueueUserWorkItem(GetIcon, game);
            }

            list_Games.ResumeLayout();
        }

        private void GetIcon(object state)
        {
            UserGameInfo game = (UserGameInfo)state;
            Bitmap bmp = null;
            string iconPath = iconsIni.IniReadValue("GameIcons", game.Game.GameName);

            if (!string.IsNullOrEmpty(iconPath))
            {
                if (iconPath.EndsWith(".exe"))
                {
                    bmp = new Bitmap(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png"));
                    Icon icon = Shell32.GetIcon(iconPath, false);
                    bmp = icon.ToBitmap();
                    icon.Dispose();
                }
                else
                {
                    if (File.Exists(iconPath))
                    {
                        bmp = new Bitmap(iconPath);
                    }
                    else
                    {
                        if (File.Exists(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png")))
                        {
                            bmp = new Bitmap(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\default.png"));
                        }
                    }
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
                        control.Click += new EventHandler(button_Click);
                        control.MouseEnter += new EventHandler(gameControl_MouseEnter);
                        control.MouseLeave += new EventHandler(gameControl_MouseLeave);
                        control.Image = game.Icon;
                    });
                }
            }

        }

        private void gameControl_MouseEnter(object sender, EventArgs e)
        {
            GameControl c = sender as GameControl;

            if (c.updateAvailable)
            {
                handlerUpdateLabel = new Label
                {
                    Name = "UpdateLabel",
                    AutoSize = true,
                    BackColor = Color.Black,
                    ForeColor = Color.White,
                    Font = new Font(customFont, 7.25F),
                    Text = "There is an update available for this handler,\nright click and select \"Update Handler\" \nto quickly download the latest version."
                };

                handlerUpdateLabel.Location = new Point(c.Right, c.Top + c.Height - 10);
                clientAreaPanel.Controls.Add(handlerUpdateLabel);
                handlerUpdateLabel.BringToFront();
            }
        }

        private void gameControl_MouseLeave(object sender, EventArgs e)
        {
            foreach (Control updateLabel in clientAreaPanel.Controls)//Use this because for some reasons sometimes the label won't dispose.
            {
                if (updateLabel.Name == "UpdateLabel")
                    updateLabel.Dispose();
                break;
            }
        }

        public delegate void CheckForAssets(Label dllabel, bool visible, string game);
        public void DelegateCheckForAssets(Label dllabel, bool visible, string game)
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
                SuspendLayout();
                glowingLine0.Image = new Bitmap(themePath + "\\lightbar_top.gif");
                ResumeLayout();
                mainButtonFrame.Enabled = true;
                btn_downloadAssets.Enabled = true;
                game_listSizer.Enabled = true;
                button_UpdateAvailable.Enabled = true;
                btn_gameOptions.Enabled = true;
                StepPanel.Enabled = true;
                btn_settings.Enabled = true;
                Controls.Remove(dllabel);

                if (currentControl != null)
                {
                    ApplyBackgroundAndCover(currentControl.UserGameInfo.GameGuid);
                }
            }
        }

        private void btn_downloadAssets_Click(object sender, EventArgs e)
        {
            SuspendLayout();
            glowingLine0.Image = new Bitmap(themePath + "\\download_bar.gif");
            ResumeLayout();
            Control dllabel = new Label();
            mainButtonFrame.Enabled = false;
            StepPanel.Enabled = false;
            button_UpdateAvailable.Enabled = false;
            btn_gameOptions.Enabled = false;
            btn_settings.Enabled = false;
            btn_downloadAssets.Enabled = false;
            game_listSizer.Enabled = false;
            CheckForCovers(dllabel);
        }

        private void CheckForCovers(Control dllabel)
        {
            List<UserGameInfo> games = gameManager.User.Games;
            System.Threading.Tasks.Task.Run(() =>
            {
                for (int i = 0; i < games.Count; i++)
                {
                    getAssets = new AssetsScraper();
                    UserGameInfo game = games[i];
                    this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, true, "Checking Cover for : " + game.GameGuid);
                    dllabel.ForeColor = Color.Green;
                    try
                    {
                        if (game.Game == null)
                        {
                            continue;
                        }

                        hubHandler = scriptDownloader.GetHandler(game.Game.GetHandlerId());

                        string _covers = $@"https://images.igdb.com/igdb/image/upload/t_cover_big/{hubHandler.GameCover}.jpg";

                        if (hubHandler.Id != null)
                        {
                            getAssets.DownloadCovers(_covers, game.GameGuid);
                            try
                            {
                                if (!File.Exists(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + game.GameGuid + ".txt")))
                                {
                                    this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, true, "Checking description: " + game.GameGuid);
                                    using (FileStream stream = new FileStream(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + game.GameGuid + ".txt"), FileMode.Create))
                                    {
                                        using (StreamWriter writer = new StreamWriter(stream))
                                        {
                                            string json = JsonConvert.SerializeObject(hubHandler.GameDescription);
                                            writer.Write(json);
                                            stream.Flush();
                                            writer.Dispose();
                                        }
                                        stream.Dispose();
                                    }
                                }
                            }
                            catch { }
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
            System.Threading.Tasks.Task.Run(() =>
            {
                for (int i = 0; i < games.Count; i++)
                {
                    getAssets = new AssetsScraper();
                    UserGameInfo game = games[i];
                    this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, true, "Checking Screenshots for : " + game.GameGuid);
                    try
                    {
                        if (game.Game == null)
                        {
                            continue;
                        }
                        hubHandler = scriptDownloader.GetHandler(game.Game.GetHandlerId());
                        string _screenshots = game.Game.GetScreenshots();

                        if (hubHandler.Id != null)
                        {
                            getAssets.DownloadScreenshots(_screenshots, game.GameGuid);
                        }
                    }
                    catch (Exception)
                    { }
                }
                this.BeginInvoke(new CheckForAssets(DelegateCheckForAssets), dllabel, false, "Download Completed");
            });
        }

        private Bitmap ApplyBlur(Bitmap screenshot)
        {
            var blur = new GaussianBlur(screenshot as Bitmap);

            Bitmap result = blur.Process(blurValue);

            if (screenshot == null)
            {
                return defBackground;
            }
            if (screenshotImg != null)
            {
                screenshotImg.Dispose();
            }
            blur = null;
            return result;
        }

        private void ApplyBackgroundAndCover(string currentSelected)
        {
            string name = currentSelected;// <= GameGuid
            ///Apply covers
            if (File.Exists(Path.Combine(Application.StartupPath, @"gui\covers\" + name + ".jpeg")))
            {
                coverImg = new Bitmap(Path.Combine(Application.StartupPath, @"gui\covers\" + name + ".jpeg"));
                clientAreaPanel.SuspendLayout();
                cover.BackgroundImage = coverImg;
                clientAreaPanel.ResumeLayout();
                coverFrame.Visible = true;
                cover.Visible = true;
            }
            else
            {
                cover.BackgroundImage = new Bitmap(themePath + "\\no_cover.png");
                cover.Visible = true;
                coverFrame.Visible = true;
            }
            //Apply screenshots randomly
            if (Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + name)))
            {
                string[] imgsPath = Directory.GetFiles((Path.Combine(Application.StartupPath, @"gui\screenshots\" + name)));
                Random rNum = new Random();
                int RandomIndex = rNum.Next(0, imgsPath.Count());

                screenshotImg = new Bitmap(Path.Combine(Application.StartupPath, @"gui\screenshots\" + name + "\\" + RandomIndex + "_" + name + ".jpeg"));//name(1) => directory name ; name(2) = partial image name 
                clientAreaPanel.SuspendLayout();
                clientAreaPanel.BackgroundImage = ApplyBlur(screenshotImg);
                clientAreaPanel.ResumeLayout();
            }
            else
            {
                clientAreaPanel.SuspendLayout();
                clientAreaPanel.BackgroundImage = ApplyBlur(defBackground);
                clientAreaPanel.ResumeLayout();
            }

            btn_textSwitcher.Visible = !positionsControl.textZoomContainer.Visible && File.Exists(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt"));
        }

        private int r = 0;
        private int g = 0;
        private int b = 0;
        private bool loop = false;

        private void rainbowTimerTick(Object Object, EventArgs EventArgs)
        {
            if (HandlerNoteTitle.Text == "Handler Notes" || HandlerNoteTitle.Text == "Read First")
            {
                if (!loop)
                {
                    HandlerNoteTitle.Text = "Handler Notes";
                    if (r < 255 && b < 255) { r += 3; b += 3; };
                    if (b >= 255 && r >= 255)
                        loop = true;
                    HandlerNoteTitle.Font = new Font(HandlerNoteTitle.Font.FontFamily, HandlerNoteTitle.Font.Size, FontStyle.Bold);
                }
                else
                {
                    HandlerNoteTitle.Text = "Read First";
                    if (r > 0 && b > 0) { r -= 3; b -= 3; }
                    if (b <= 0 && r <= 0)
                        loop = false;
                    HandlerNoteTitle.Font = new Font(HandlerNoteTitle.Font.FontFamily, HandlerNoteTitle.Font.Size, FontStyle.Bold);
                }
                HandlerNoteTitle.ForeColor = Color.FromArgb(r, 255, b);
            }
            else
            {
                HandlerNoteTitle.ForeColor = HandlerNoteTitleFont;
                HandlerNoteTitle.Font = new Font(HandlerNoteTitle.Font.FontFamily, (float)HandlerNoteTitle.Font.Size, FontStyle.Regular);
            }

        }

        private bool rainbowTimerRunning = false;

        private bool MatchRequirements(GenericGameInfo game)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            if ((game.RequiresAdmin || game.LaunchAsDifferentUsersAlt || game.LaunchAsDifferentUsers || game.ChangeIPPerInstanceAlt) && !principal.IsInRole(WindowsBuiltInRole.Administrator) || 
               ((game.LaunchAsDifferentUsersAlt || game.LaunchAsDifferentUsers || game.ChangeIPPerInstanceAlt) && (Program.forcedBadPath && principal.IsInRole(WindowsBuiltInRole.Administrator))))
            {
                string message;

                if (Program.forcedBadPath && principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    message = "This game handler does not support the current Nucleus Co-op installation path.\n\n" +
                          "Do NOT install in any of these folders:\n" +
                          "- A folder containing any game files\n" +
                          "- C:\\Program Files or C:\\Program Files (x86)\n" +
                          "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                          "- Any folder with security settings like C:\\Windows\n" +
                          "\n" +
                          "A good place is C:\\Nucleus\\NucleusCoop.exe";
                }
                else
                {
                    message = "This handler requires you to run Nucleus as administrator.";
                }

                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                rightFrame.Visible = false;
                StepPanel.Visible = false;
                clientAreaPanel.BackgroundImage = defBackground;
                stepPanelPictureBox.Visible = true;                
                if (cover.BackgroundImage != null)
                {
                    cover.BackgroundImage.Dispose();
                }

                if (rainbowTimer != null)
                {
                    rainbowTimer.Dispose();
                    rainbowTimerRunning = false;
                }

                if (hubShowcase != null)
                {
                    hubShowcase.Dispose();
                }

                return false;
            }
            else 
            {
                return true;
            }
        }

        private void list_Games_SelectedChanged(object arg1, Control arg2)
        {
            currentControl = (GameControl)arg1;
            currentGameInfo = currentControl.UserGameInfo;

            if (currentGameInfo != null)
            {
                if (!MatchRequirements(currentGameInfo.Game))
                {
                    return;
                }
            }

            positionsControl.handlerNoteZoom.Visible = false;
            btn_magnifier.Image = new Bitmap(themePath + "\\magnifier.png");
            btn_textSwitcher.Visible = false;
            icons_Container.Visible = false;

            if (screenshotImg != null) { screenshotImg.Dispose(); }

            if (coverImg != null) { coverImg.Dispose(); }

            if (currentGameInfo == null)
            {
                btn_gameOptions.Visible = false;
                button_UpdateAvailable.Visible = false;
                return;
            }
            else
            {
                currentGame = currentGameInfo.Game;               
                currentGameSetup = currentControl.UserGameInfo.Game.GameName;

                HandlerNoteTitle.Text = "Handler Notes";

                if (hubShowcase != null)
                {
                    hubShowcase.Dispose();
                }

                positionsControl.textZoomContainer.Visible = false;

                stepPanelPictureBox.Visible = false;
                rightFrame.Visible = true;
                btn_gameOptions.Visible = true;

                StepPanel.Visible = true;

                if (!rainbowTimerRunning)
                {
                    rainbowTimer = new System.Windows.Forms.Timer();
                    rainbowTimer.Interval = (25); //millisecond                   
                    rainbowTimer.Tick += new EventHandler(rainbowTimerTick);
                    rainbowTimer.Start();
                    rainbowTimerRunning = true;
                }

                icons_Container.Controls.Clear();
                button_UpdateAvailable.Visible = false;

                ApplyBackgroundAndCover(currentControl.UserGameInfo.GameGuid);

                Size iconsSize = new Size(icons_Container.Height + 6, icons_Container.Height - 2);
                Size playersIconsSize = new Size(icons_Container.Height, icons_Container.Height);

                if ((currentGame.Hook.XInputEnabled && !currentGame.Hook.XInputReroute && !currentGame.ProtoInput.DinputDeviceHook) || currentGame.ProtoInput.XinputHook)
                {
                    icon1 = new UI.InputIcons(iconsSize, xinputGamepad_Icon);
                    icon1.MouseEnter += inputIcons_MouseEnter;
                    icon1.MouseLeave += inputIcons_MouseLeave;
                    icons_Container.Controls.Add(icon1);
                }

                if ((currentGame.Hook.DInputEnabled || currentGame.Hook.XInputReroute || currentGame.ProtoInput.DinputDeviceHook) && (currentGame.Hook.XInputEnabled || currentGame.ProtoInput.XinputHook))
                {
                    icon2 = new UI.InputIcons(iconsSize, dinputGamepad_Icon);
                    icon2.MouseEnter += inputIcons_MouseEnter;
                    icon2.MouseLeave += inputIcons_MouseLeave;
                    icons_Container.Controls.Add(icon2);
                }
                else if ((currentGame.Hook.DInputEnabled || currentGame.Hook.XInputReroute || currentGame.ProtoInput.DinputDeviceHook) && (!currentGame.Hook.XInputEnabled || !currentGame.ProtoInput.XinputHook))
                {
                    icon3 = new UI.InputIcons(iconsSize, dinputGamepad_Icon);
                    icon3.MouseEnter += inputIcons_MouseEnter;
                    icon3.MouseLeave += inputIcons_MouseLeave;
                    icons_Container.Controls.Add(icon3);
                }

                if (currentGame.SupportsKeyboard)
                {
                    icon4 = new UI.InputIcons(iconsSize, k_Icon);
                    icon4.MouseEnter += inputIcons_MouseEnter;
                    icon4.MouseLeave += inputIcons_MouseLeave;
                    icons_Container.Controls.Add(icon4);
                }

                if (currentGame.SupportsMultipleKeyboardsAndMice) //Raw mice/keyboards
                {
                    icon5 = new UI.InputIcons(iconsSize, k_Icon);
                    icon6 = new UI.InputIcons(iconsSize, k_Icon);
                    icon5.MouseEnter += inputIcons_MouseEnter;
                    icon5.MouseLeave += inputIcons_MouseLeave;
                    icon6.MouseEnter += inputIcons_MouseEnter;
                    icon6.MouseLeave += inputIcons_MouseLeave;
                    icons_Container.Controls.Add(icon5);
                    icons_Container.Controls.Add(icon6);
                }
            }

            icons_Container.Visible = true;

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

            foreach (KeyValuePair<string, bool> statut in updateStatuts)
            {
                if (statut.Key == currentControl.UserGameInfo.GameGuid)
                {
                    button_UpdateAvailable.Visible = updateStatuts[currentControl.UserGameInfo.GameGuid];
                    break;
                }
            }

            currentlyUpdatingScript = false;

            btn_textSwitcher.Visible = File.Exists(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt"));

            if (currentGame.Description?.Length > 0)
            {
                scriptAuthorTxt.Text = currentGame.Description;
                scriptAuthorTxt.Visible = true;
                scriptAuthorTxtSizer.Visible = true;
            }
            else if (File.Exists(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt")))
            {
                StreamReader desc = new StreamReader(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt"));

                HandlerNoteTitle.Text = "Game Description";
                scriptAuthorTxt.Text = desc.ReadToEnd();
                btn_textSwitcher.Visible = false;
                desc.Dispose();
            }
            else
            {
                scriptAuthorTxtSizer.Visible = false;
                scriptAuthorTxt.Visible = false;
            }

            if (content != null)
            {
                content.Dispose();
            }

            string path = Path.Combine(gameManager.GetAppContentPath(), currentGameInfo.Game.GUID);
            CleanContentFolder(path);
            // content manager is shared withing the same game
            content = new ContentManager(currentGame);

            GoToStep(0);
        }

        private void CleanContentFolder(string path)
        {
            if (Directory.Exists(path))
            {
                Log("Proceed game content cleanup.");
                string[] instances = Directory.GetDirectories(path, "*", SearchOption.AllDirectories);

                try
                {
                    foreach (string instance in instances)
                    {
                        if (Directory.Exists(instance) && currentGameInfo.Game.KeepSymLinkOnExit != true)
                        {
                            Directory.Delete(instance, true);
                        }
                    }
                }
                catch
                {
                    Log("Nucleus will try to unlock one or more files in order to cleanup game content.");
                    try
                    {
                        foreach (string instance in instances)
                        {
                            bool exists = Directory.Exists(instance);

                            if (exists)
                            {
                                string[] subs = Directory.GetFileSystemEntries(instance, "*", SearchOption.AllDirectories);

                                foreach (string locked in subs)
                                {
                                    File.SetAttributes(locked, FileAttributes.Normal);
                                }
                            }

                            if (exists)
                            {
                                Directory.Delete(instance, true);
                            }
                        }
                    }
                    catch
                    {
                        Log("Game content cleanup failed. One or more files can't be unlocked by Nucleus.");
                        System.Threading.Tasks.Task.Run(() =>
                        {
                            MessageBox.Show($"One or more files from {path} are locked by the system or used by an other program and Nucleus failed to unlock them, try to delete/unlock it manually or restart your computer to unlock the file because it could lead to a crash on game startup. You can ignore this message and risk a crash or unexpected behaviors.", "Risk of crash!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        });
                    }
                }
            }
        }

        private void inputIcons_MouseEnter(object sender, EventArgs e)
        {
            PictureBox inputIcon = sender as PictureBox;

            if (inputIcon.Equals(icon1))
            {
                inputsIconsDesc.Text = "Supports xinput gamepads (e.g., X360)";
            }
            else if (inputIcon.Equals(icon2) || inputIcon.Equals(icon3))
            {
                inputsIconsDesc.Text = "Supports dinput gamepads (e.g., Ps3)";
            }
            else if (inputIcon.Equals(icon4))
            {
                inputsIconsDesc.Text = @"Supports 1 keyboard\mouse";
            }
            else if (inputIcon.Equals(icon5) || inputIcon.Equals(icon6))
            {
                inputsIconsDesc.Text = @"Supports multiple keyboards/mice";
            }

            inputsIconsDesc.Location = new Point((int)icons_Container.Left - 5, icons_Container.Bottom);
            inputsIconsDesc.BringToFront();
            inputsIconsDesc.Visible = true;
        }

        private void inputIcons_MouseLeave(object sender, EventArgs e)
        {
            inputsIconsDesc.Visible = false;
            inputsIconsDesc.Text = "";
        }

        private void EnablePlay()
        {
            btn_Play.Enabled = true;
        }

        private void StepCanPlay(UserControl obj, bool canProceed, bool autoProceed)
        {
            if (btnBack.Enabled)
            {
                btnBack.BackgroundImage = new Bitmap(themePath + "\\arrow_left_mousehover.png");
            }
            else
            {
                btnBack.BackgroundImage = new Bitmap(themePath + "\\arrow_left.png");
            }

            if (!canProceed)
            {
                btnBack.Enabled = false;
                if (btn_Play.Text == "PLAY" || btn_Next.Enabled)
                {
                    btn_Play.Enabled = false;
                }

                btn_Next.Enabled = false;
                btn_Next.BackgroundImage = new Bitmap(themePath + "\\arrow_right.png");
                btnBack.BackgroundImage = new Bitmap(themePath + "\\arrow_left.png");
                return;
            }
            else
            {
                btn_Next.BackgroundImage = new Bitmap(themePath + "\\arrow_right.png");
            }

            if (currentGame.Options.Count == 0)
            {
                EnablePlay();
                return;
            }

            if (currentStepIndex + 1 > stepsList.Count - 1)
            {
                EnablePlay();
                return;
            }
            else
            {
                if (btn_Play.Text == "PLAY")
                {
                    btn_Play.Enabled = false;
                }
                else
                {
                    btn_Play.Enabled = true;
                }
            }

            if (autoProceed)
            {
                GoToStep(currentStepIndex + 1);
            }
            else
            {
                btn_Next.Enabled = true;
                btn_Next.BackgroundImage = new Bitmap(themePath + "\\arrow_right_mousehover.png");
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            GoToStep(currentStepIndex + 1);
        }

        private void KillCurrentStep()
        {
            foreach (Control c in StepPanel.Controls)
            {
                if (!c.Name.Equals("scriptAuthorTxtSizer"))
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

            StepPanel.Controls.Add(currentStep);

            label_StepTitle.Text = currentStep.Title;

            btn_Next.Enabled = currentStep.CanProceed && step != stepsList.Count - 1;
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
         
            if (!themeSwith)
            {
                Process[] processes = Process.GetProcessesByName("NucleusCoop");
                foreach (Process NucleusCoop in processes)
                {
                    NucleusCoop.Kill();
                }
            }
            themeSwith = false;
        }

        private void btn_Play_Click(object sender, EventArgs e)
        {
            bool useCustomLayout = Convert.ToBoolean(ini.IniReadValue("CustomLayout", "SplitDiv"));
            string splitDivisionBackColor = ini.IniReadValue("CustomLayout", "SplitDivColor");

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
                        if (openForm.Name == "Hide Desktop" || openForm.Name == "SplitForm")
                        {
                            openForm.Close();
                        }
                    }

                }
                catch { }

                positionsControl.gamepadTimer = new System.Threading.Timer(positionsControl.GamepadTimer_Tick, null, 0, 1000);
                positionsControl.gamepadPollTimer = new System.Threading.Timer(positionsControl.GamepadPollTimer_Tick, null, 0, 1001);

                User32Util.ShowTaskBar();
                SetBtnToPlay();
                btn_Play.Enabled = false;
                GoToStep(0);
                return;
            }

            rightFrame.Visible = false;
            StepPanel.Visible = false;           
            clientAreaPanel.BackgroundImage = defBackground;
            stepPanelPictureBox.Visible = true;            
            cover.BackgroundImage.Dispose();
            rainbowTimer.Dispose();
            rainbowTimerRunning = false;

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

            if (currentGame.HideTaskbar && !useCustomLayout)
            {
                User32Util.HideTaskbar();
            }

            if (currentGame.ProtoInput.AutoHideTaskbar || useCustomLayout)
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

            if (useCustomLayout || currentGame.HideDesktop)
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
                    if (color.Key == splitDivisionBackColor)
                    {
                        ChoosenColor = color.Value;
                    }
                }

                loadTimer = new System.Windows.Forms.Timer
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
                    Form backgroundForm = new Form
                    {
                        Name = "SplitForm",
                        Text = "SplitForm",
                        BackColor = ChoosenColor,
                        Location = new Point(screen.Bounds.X, screen.Bounds.Y),
                        Width = screen.WorkingArea.Size.Width,
                        Height = screen.WorkingArea.Size.Height + 50,
                        BackgroundImage = clientAreaPanel.BackgroundImage,
                        BackgroundImageLayout = ImageLayout.Stretch,
                        FormBorderStyle = FormBorderStyle.None,
                        StartPosition = FormStartPosition.Manual
                    };

                    backgroundForms.Add(backgroundForm);
                    backgroundForm.Show();
                }
            }

            if (btn_Play.ContainsFocus)
            {
               stepPanelPictureBox.Focus();
            }

            if(!currentGame.ToggleUnfocusOnInputsLock)
            {
                WindowState = FormWindowState.Minimized;
            }        
        }

        private void slideshowTick(Object myObject, EventArgs myEventArgs)
        {
            if (Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName)))
            {
                string[] imgsPath = Directory.GetFiles((Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName)));
                Random rNum = new Random();
                int RandomIndex = rNum.Next(0, imgsPath.Count());

                screenshotImg = new Bitmap(Path.Combine(Application.StartupPath, @"gui\screenshots\" + currentGameInfo.Game.GameName + "\\" + RandomIndex + "_" + currentGameInfo.Game.GameName + ".jpeg"));
                foreach (Form _backgroundForm in backgroundForms)
                {
                    _backgroundForm.SuspendLayout();
                    _backgroundForm.BackgroundImage = screenshotImg;
                    _backgroundForm.ResumeLayout();
                }
            }
        }

        private void loadTimerTick(Object myObject, EventArgs myEventArgs)
        {
            slideshow.Dispose();
            loadTimer.Dispose();

            foreach (Form _backgroundForm in backgroundForms)
            {
                _backgroundForm.BackgroundImage = null;
                _backgroundForm.BackColor = ChoosenColor;
            }
        }

        private void SetBtnToPlay()
        {
            btn_Play.Text = "P L A Y";
        }

        private void handler_Ended()
        {
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
                        string[] regFiles = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "utils\\backup"), "*.reg", SearchOption.AllDirectories);
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

                            if (!regFilePath.Contains("User Shell Folders"))
                            {
                                File.Delete(regFilePath);
                            }
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
                using (System.Windows.Forms.OpenFileDialog open = new System.Windows.Forms.OpenFileDialog())
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
                            MessageBox.Show(string.Format("The executable '{0}' was not found in any game handler's Game.ExecutableName field. Game has not been added.", Path.GetFileName(path)), "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                }
            }
            catch (Exception)
            { }
        }

        private void btnAutoSearch_Click(object sender, EventArgs e)
        {
            if (!searchDisksForm.Visible)
            {
                searchDisksForm.Location = new Point(Width / 2 - searchDisksForm.Width / 2, Height / 2 - searchDisksForm.Height / 2); ;    
                searchDisksForm.BringToFront();
                searchDisksForm.Visible = true;
            }
            else
            {
                searchDisksForm.Visible = false;
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

        private void SettingsBtn_Click(object sender, EventArgs e)
        {
            if (!settingsForm.Visible)
            {
                settingsForm.Location = new Point(Width / 2 - settingsForm.Width / 2, Height / 2 - settingsForm.Height / 2);
                settingsForm.BringToFront();
                settingsForm.Visible = true;
            }
            else
            {
                settingsForm.Visible = false;
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
                            if (!dontConfirm)
                            {
                                if (File.Exists(Path.Combine(Application.StartupPath, @"gui\covers\" + gameGuid + ".jpeg")))
                                {
                                    try
                                    {
                                        File.Delete(Path.Combine(Application.StartupPath, @"gui\covers\" + gameGuid + ".jpeg"));
                                    }
                                    catch (Exception)
                                    {
                                        cover.BackgroundImage.Dispose();
                                        File.Delete(Path.Combine(Application.StartupPath, @"gui\covers\" + gameGuid + ".jpeg"));
                                    }
                                }

                                if (Directory.Exists(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameGuid)))
                                {
                                    try
                                    {
                                        Directory.Delete(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameGuid), true);
                                    }
                                    catch (Exception)
                                    {
                                        screenshotImg.Dispose();
                                        Directory.Delete(Path.Combine(Application.StartupPath, @"gui\screenshots\" + gameGuid), true);
                                    }
                                }

                                if (File.Exists(Path.Combine(Application.StartupPath, @"gui\descriptions\" + gameGuid + ".txt")))
                                {
                                    try
                                    {
                                        File.Delete(Path.Combine(Application.StartupPath, @"gui\descriptions\" + gameGuid + ".txt"));
                                    }
                                    catch (Exception)
                                    {
                                        scriptAuthorTxt.Text = null;
                                        File.Delete(Path.Combine(Application.StartupPath, @"gui\descriptions\" + gameGuid + ".txt"));
                                    }
                                }

                                if (iconsIni.IniReadValue("GameIcons", gameGuid) != "")
                                {
                                    string[] iniContent = File.ReadAllLines(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\icons.ini"));
                                    List<string> newContent = new List<string>();
                                    for (int index = 0; index < iniContent.Length; index++)
                                    {
                                        if (iniContent[index].Contains(gameGuid + "=" + iconsIni.IniReadValue("GameIcons", gameGuid)))
                                        {
                                            string fullPath = gameGuid + "=" + iconsIni.IniReadValue("GameIcons", gameGuid).ToString();
                                            iniContent[index] = string.Empty;
                                        }

                                        if (iniContent[index] != string.Empty)
                                        {
                                            newContent.Add(iniContent[index]);
                                        }
                                    }

                                    File.WriteAllLines(Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons\\icons.ini"), newContent);
                                }
                            }
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
                gameContextMenuStrip.Items[20].Visible = false;

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

                        }

                        if (i == 9)
                        {
                            (gameContextMenuStrip.Items[8] as ToolStripMenuItem).DropDownItems.Clear();
                            (gameContextMenuStrip.Items[9] as ToolStripMenuItem).DropDownItems.Clear();
                            if (currentGameInfo.Game.UserProfileConfigPath?.Length > 0)
                            {
                                if (profilePaths.Count > 0)
                                {
                                    try
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
                                    catch (Exception)
                                    {

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
                                    try
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
                                    catch (Exception)
                                    {

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
                                    try
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
                                    catch (Exception)
                                    {

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
                                    try
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
                                    catch (Exception)
                                    {

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

                        if (i == 20)
                        {
                            gameContextMenuStrip.Items[20].Visible = false;

                            foreach (KeyValuePair<string, bool> infos in updateStatuts)
                            {
                                if (infos.Key == currentControl.UserGameInfo.GameGuid)
                                {
                                    gameContextMenuStrip.Items[20].Visible = updateStatuts[currentControl.UserGameInfo.GameGuid];
                                    break;
                                }
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

        private void gameContextMenuStrip_Opened(object sender, EventArgs e)
        {
            gameContextMenuStrip.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, gameContextMenuStrip.Width, gameContextMenuStrip.Height, 20, 20));
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
                if (ini.IniReadValue("Dev", "TextEditorPath") != "Default")
                {
                    Process.Start($"{ini.IniReadValue("Dev", "TextEditorPath")}", "\"" + Path.Combine(gameManager.GetJsScriptsPath(), currentGameInfo.Game.JsFileName) + "\"");
                }
                else
                {
                    Process.Start("notepad++.exe", "\"" + Path.Combine(gameManager.GetJsScriptsPath(), currentGameInfo.Game.JsFileName) + "\"");
                }

            }
            catch (Exception)
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
            using (System.Windows.Forms.OpenFileDialog dlg = new System.Windows.Forms.OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "All Images Files (*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe)|*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.tiff;*.tif;*.ico;*.exe" +
                            "|PNG Portable Network Graphics (*.png)|*.png" +
                            "|JPEG File Interchange Format (*.jpg *.jpeg *jfif)|*.jpg;*.jpeg;*.jfif" +
                            "|BMP Windows Bitmap (*.bmp)|*.bmp" +
                            "|TIF Tagged Imaged File Format (*.tif *.tiff)|*.tif;*.tiff" +
                            "|Icon (*.ico)|*.ico" +
                            "|Executable (*.exe)|*.exe";
                dlg.InitialDirectory = Path.Combine(Directory.GetCurrentDirectory() + "\\gui\\icons");

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
                    iconsIni.IniWriteValue("GameIcons", currentGameInfo.Game.GameName, dlg.FileName);

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

        private void btn_Download_Click(object sender, EventArgs e)
        {
            scriptDownloader.ShowDialog();
        }

        private void button_UpdateAvailable_Click(object sender, EventArgs e)
        {
            hubHandler = scriptDownloader.GetHandler(currentGameInfo.Game.GetHandlerId());

            if (hubHandler == null)
            {
                MessageBox.Show("Error fetching update information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                string updatedHandler = string.Empty;

                foreach (KeyValuePair<string, bool> infos in updateStatuts)
                {
                    if (infos.Key == currentGameInfo.Game.GUID)
                    {
                        DialogResult dialogResult = MessageBox.Show("An update to this handler is available, download it?", "Update Available", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                        if (dialogResult == DialogResult.Yes)
                        {
                            currentlyUpdatingScript = true;
                            updatedHandler = currentGameInfo.Game.GUID;
                            updateStatuts[updatedHandler] = false;
                            DeleteGame(true);
                            StepPanel.Visible = false;
                            label_StepTitle.Text = "Select a game";
                            btn_Play.Enabled = false;
                            btn_Next.Enabled = false;
                            button_UpdateAvailable.Visible = false;
                            stepsList.Clear();

                            downloadPrompt = new DownloadPrompt(hubHandler, this, null, true);
                            list_Games.SuspendLayout();
                            downloadPrompt.ShowDialog();
                            list_Games.ResumeLayout();
                            StepPanel.Visible = true;

                        }
                        break;
                    }
                }
            }
        }

        private void updateHandlerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            hubHandler = scriptDownloader.GetHandler(currentGameInfo.Game.GetHandlerId());

            if (hubHandler == null)
            {
                MessageBox.Show("Error fetching update information", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {

                currentlyUpdatingScript = true;
                btn_Play.Enabled = false;
                btn_Next.Enabled = false;

                downloadPrompt = new DownloadPrompt(hubHandler, this, null, true);
                downloadPrompt.gameExeNoUpdate = true;
                list_Games.SuspendLayout();
                downloadPrompt.ShowDialog();
                list_Games.ResumeLayout();

                if (currentGameSetup == currentGameInfo.Game.GameName)
                {
                    button_UpdateAvailable.Visible = false;
                }

                string updatedHandler = string.Empty;

                foreach (KeyValuePair<string, bool> infos in updateStatuts)
                {
                    if (infos.Key == currentGameInfo.Game.GUID)
                    {
                        updatedHandler = currentGameInfo.Game.GUID;
                        break;
                    }
                }

                updateStatuts[updatedHandler] = false;

                for (int i = 0; i < gameManager.User.Games.Count; i++)
                {
                    if (gameManager.User.Games[i].Game != null)
                    {
                        if (gameManager.User.Games[i].Game.GameName == currentGameInfo.Game.GameName)
                        {

                            string path = gameManager.User.Games[i].ExePath;
                            gameManager.User.Games.RemoveAt(i);
                            List<GenericGameInfo> info = gameManager.GetGames(path);

                            if (info.Count == 1)
                            {
                                UserGameInfo game = GameManager.Instance.TryAddGame(path, info[0]);
                                break;
                            }
                        }
                    }
                }

                if (StepPanel.Visible)
                {
                    rightFrame.Visible = false;
                    StepPanel.Visible = false;
                    clientAreaPanel.BackgroundImage = defBackground;
                    stepPanelPictureBox.Visible = true;
                }

                RefreshGames();
            }
        }

        private void btn_Extract_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog ofd = new System.Windows.Forms.OpenFileDialog
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
            catch (Exception)
            {
                MessageBox.Show(@"SplitCalculator.exe has not been found in the utils\SplitCalculator folder. Try again with a fresh Nucleus Co-op installation.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btn_noHub_Click(object sender, EventArgs e)
        {
            RefreshNetStatut();
        }

        private void btn_Links_Click(object sender, EventArgs e)
        {
            if (linksPanel.Visible)
            {
                linksPanel.Visible = false;
                btn_Links.BackgroundImage = new Bitmap(themePath + "\\title_dropdown_closed.png");

                if (third_party_tools_container.Visible)
                {
                    third_party_tools_container.Visible = false;
                }
            }
            else
            {
                linksPanel.BringToFront();
                linksPanel.Visible = true;
                btn_Links.BackgroundImage = new Bitmap(themePath + "\\title_dropdown_opened.png");
            }
        }

        private void this_Click(object sender, System.EventArgs e)
        {
            linksPanel.Visible = false;
            btn_Links.BackgroundImage = new Bitmap(themePath + "\\title_dropdown_closed.png");

            if (third_party_tools_container.Visible)
            {
                third_party_tools_container.Visible = false;
            }
        }

        private void button1_Click_2(object sender, EventArgs e) { Process.Start("https://hub.splitscreen.me/"); }

        private void button1_Click(object sender, EventArgs e) { Process.Start("https://discord.com/invite/QDUt8HpCvr"); }

        private void button2_Click(object sender, EventArgs e) { Process.Start("https://www.reddit.com/r/nucleuscoop/"); }

        private void logo_Click(object sender, EventArgs e) { Process.Start("https://github.com/SplitScreen-Me/splitscreenme-nucleus/releases"); }

        private void link_faq_Click(object sender, EventArgs e) { Process.Start(faq_link); }

        private void linkLabel4_LinkClicked(object sender, EventArgs e) { Process.Start("https://github.com/nefarius/ScpToolkit/releases"); third_party_tools_container.Visible = false; }

        private void linkLabel3_LinkClicked(object sender, EventArgs e) { Process.Start("https://github.com/ViGEm/HidHide/releases"); third_party_tools_container.Visible = false; }

        private void linkLabel2_LinkClicked(object sender, EventArgs e) { Process.Start("https://github.com/Ryochan7/DS4Windows/releases"); third_party_tools_container.Visible = false; }

        private void linkLabel1_LinkClicked(object sender, EventArgs e) { Process.Start("https://github.com/csutorasa/XOutput/releases"); third_party_tools_container.Visible = false; }

        private void btn_thirdPartytools_Click(object sender, EventArgs e) { if (third_party_tools_container.Visible) { third_party_tools_container.Visible = false; } else { third_party_tools_container.Visible = true; } }

        private void scriptAuthorTxt_LinkClicked(object sender, LinkClickedEventArgs e) { Process.Start(e.LinkText); }

        private void closeBtn_MouseEnter(object sender, EventArgs e) { closeBtn.BackgroundImage = new Bitmap(themePath + "\\title_close_mousehover.png"); }

        private void closeBtn_MouseLeave(object sender, EventArgs e) { closeBtn.BackgroundImage = new Bitmap(themePath + "\\title_close.png"); }

        private void maximizeBtn_MouseEnter(object sender, EventArgs e) { maximizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_maximize_mousehover.png"); }

        private void maximizeBtn_MouseLeave(object sender, EventArgs e) { maximizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_maximize.png"); }

        private void minimizeBtn_MouseLeave(object sender, EventArgs e) { minimizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_minimize.png"); }

        private void minimizeBtn_MouseEnter(object sender, EventArgs e) { minimizeBtn.BackgroundImage = new Bitmap(themePath + "\\title_minimize_mousehover.png"); }

        private void btn_settings_MouseEnter(object sender, EventArgs e) { btn_settings.BackgroundImage = new Bitmap(themePath + "\\title_settings_mousehover.png"); }

        private void btn_settings_MouseLeave(object sender, EventArgs e) { btn_settings.BackgroundImage = new Bitmap(themePath + "\\title_settings.png"); }

        private void btn_downloadAssets_MouseEnter(object sender, EventArgs e) { btn_downloadAssets.BackgroundImage = new Bitmap(themePath + "\\title_download_assets_mousehover.png"); }

        private void btn_downloadAssets_MouseLeave(object sender, EventArgs e) { btn_downloadAssets.BackgroundImage = new Bitmap(themePath + "\\title_download_assets.png"); }

        private void btn_faq_MouseEnter(object sender, EventArgs e) { btn_faq.BackgroundImage = new Bitmap(themePath + "\\faq_mousehover.png"); }

        private void btn_faq_MouseLeave(object sender, EventArgs e) { btn_faq.BackgroundImage = new Bitmap(themePath + "\\faq.png"); }

        private void btn_reddit_MouseEnter(object sender, EventArgs e) { btn_reddit.BackgroundImage = new Bitmap(themePath + "\\reddit_mousehover.png"); }

        private void btn_reddit_MouseLeave(object sender, EventArgs e) { btn_reddit.BackgroundImage = new Bitmap(themePath + "\\reddit.png"); }

        private void btn_Discord_MouseEnter(object sender, EventArgs e) { btn_Discord.BackgroundImage = new Bitmap(themePath + "\\discord_mousehover.png"); }

        private void btn_Discord_MouseLeave(object sender, EventArgs e) { btn_Discord.BackgroundImage = new Bitmap(themePath + "\\discord.png"); }

        private void btn_SplitCalculator_MouseEnter(object sender, EventArgs e) { btn_SplitCalculator.BackgroundImage = new Bitmap(themePath + "\\splitcalculator_mousehover.png"); }

        private void btn_SplitCalculator_MouseLeave(object sender, EventArgs e) { btn_SplitCalculator.BackgroundImage = new Bitmap(themePath + "\\splitcalculator.png"); }

        private void btn_thirdPartytools_MouseEnter(object sender, EventArgs e) { btn_thirdPartytools.BackgroundImage = new Bitmap(themePath + "\\thirdPartytools_mousehover.png"); }

        private void btn_thirdPartytools_MouseLeave(object sender, EventArgs e) { btn_thirdPartytools.BackgroundImage = new Bitmap(themePath + "\\thirdPartytools.png"); }

        private void btn_magnifier_Click(object sender, EventArgs e)
        {
            if (!positionsControl.textZoomContainer.Visible)
            {
                positionsControl.textZoomContainer.Region = Region.FromHrgn(CreateRoundRectRgn(0, 0, positionsControl.textZoomContainer.Width, positionsControl.textZoomContainer.Height, 15, 15));
                positionsControl.handlerNoteZoom.Text = scriptAuthorTxt.Text;
                positionsControl.handlerNoteZoom.Visible = true;
                positionsControl.textZoomContainer.Visible = true;
                btn_magnifier.Image = new Bitmap(themePath + "\\magnifier_close.png");
            }
            else
            {
                positionsControl.textZoomContainer.Visible = false;
                btn_magnifier.Image = new Bitmap(themePath + "\\magnifier.png");
            }
        }

        private void btn_textSwitcher_Click(object sender, EventArgs e)
        {
            if (!positionsControl.textZoomContainer.Visible && File.Exists(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt")))
            {
                StreamReader desc = new StreamReader(Path.Combine(Application.StartupPath, $@"gui\descriptions\" + currentGame.GUID + ".txt"));
                if (HandlerNoteTitle.Text == "Handler Notes" || HandlerNoteTitle.Text == "Read First")
                {
                    HandlerNoteTitle.Text = "Game Description";
                    scriptAuthorTxt.Text = desc.ReadToEnd();
                    desc.Dispose();
                }
                else
                {
                    HandlerNoteTitle.Text = "Handler Notes";
                    scriptAuthorTxt.Text = currentGame.Description;
                    desc.Dispose();
                }
            }
        }

        private int clickCount = 0;

        private void stepPanelPictureBox_Click(object sender, EventArgs e)
        {
            clickCount++;

            if (clickCount < 3)
            {
                return;
            }

            if (connected)
                TriggerHubShowCase();               
        }
    }
}
