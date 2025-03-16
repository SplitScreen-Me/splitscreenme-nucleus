using Nucleus.Coop.Controls;
using Nucleus.Coop.Forms;
using Nucleus.Coop.Tools;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Controls.SetupScreen;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Nucleus.Coop.UI;

namespace Nucleus.Coop
{
    /// <summary>
    /// Central UI class to the Nucleus Coop application
    /// </summary>
    public partial class MainForm : Form, IDynamicSized
    {
        private string[] startArgs;

        public Action<IntPtr> RawInputAction { get; set; }

        private bool canResize = false;
        private bool hotkeysCooldown = false;

        private System.Windows.Forms.Timer WebStatusTimer;
        private System.Windows.Forms.Timer hotkeysCooldownTimer;//Avoid hotkeys spamming

        private static bool connected;
        public bool Connected
        {
            get => connected;
            set
            {
                connected = value;
                Hub.Connected = value;

                if (value)
                {
                    SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
                    UI_Interface.DwldAssetsButton.Enabled = true;

                    System.Threading.Tasks.Task.Run(() =>
                    {
                        foreach (KeyValuePair<string, GenericGameInfo> game in Core_Interface.GameManager.Games)
                        {
                            game.Value.UpdateAvailable = game.Value.Hub.IsUpdateAvailable(true);
                        }
                    });

                    if (UI_Interface.HubButton != null)
                    {
                        UI_Interface.HubButton.Update(value);
                    }
                }
            }
        }

        public MainForm(string[] args)
        {
            if (args.Length != 0)
            {
                startArgs = args;
            }
      
            connected = Program.Connected;
            Hub.Connected = connected;

            InitializeComponent();

            UI_Interface.MainForm = this;
            MainWindowFunc.FadeIn();

            UI_Interface.HomeScreen = HomeScreen;
            UI_Interface.WindowPanel = WindowPanel;
            UI_Interface.VersionTxt = txt_version;
            UI_Interface.CloseButton = closeBtn;
            UI_Interface.MinimizeButton = minimizeBtn;
            UI_Interface.MaximizeButton = maximizeBtn;
            UI_Interface.DonationButton = donationBtn;
            UI_Interface.ToggleVirtualMouse = VirtualMouseToggle;
            UI_Interface.SocialMenuButton = btn_Links;
            UI_Interface.MainButtonsPanel = MainButtonsPanel;
            UI_Interface.InputsTextLabel = InputsTextLabel;
            UI_Interface.DwldAssetsButton = btn_downloadAssets;
            UI_Interface.TutorialButton = Tutorial_btn;
            UI_Interface.ExtractHandlerButton = btn_Extract;
            UI_Interface.SearchGameButton = btnSearch;
            UI_Interface.OpenLogButton = btn_debuglog;

            
            UI_Interface.GameListContainer = GameListContainer;
            UI_Interface.GameList = GameList;
            UI_Interface.SortOptionsPanel = new SortOptionsPanel();
            UI_Interface.InfoPanel = InfoPanel;
            UI_Interface.IconsContainer = Icons_Container;
            UI_Interface.PlayTimePanel = PlayTimePanel;
            UI_Interface.HandlerNoteTitle = HandlerNoteTitle;
            UI_Interface.HandlerNotes = HandlerNotes;
            UI_Interface.HandlerNotesContainer = HandlerNotesContainer;
            UI_Interface.ExpandHandlerNotesButton = ExpandHandlerNotes_btn;

            UI_Interface.Cover = cover;
            UI_Interface.CoverFrame = coverFrame;

            UI_Interface.GotoPrev = btn_Prev;
            UI_Interface.GotoNext = btn_Next;
            UI_Interface.PlayButton = btn_Play;

            UI_Interface.ProfileSettingsButton = ProfileSettings_btn;
            UI_Interface.ProfileListButton = ProfilesList_btn;

            UI_Interface.SettingsButton = SettingsButton;

            UI_Interface.GameOptionMenu = GameOptionMenu;
            UI_Interface.SocialMenu = socialLinksMenu;
            UI_Interface.SaveProfileSwitch = SaveProfileSwitch;

            UI_Interface.Logo = logo;
            UI_Interface.BigLogo = BigLogo;
            UI_Interface.ProfileButtonsPanel = ProfileButtonsPanel;
            UI_Interface.ProfileButtonPanelLockPb = ProfileButtonPanelLockPb;

            UI_Interface.SetupPanel = SetupPanel;

            UI_Interface.SetupScreen = new SetupScreenControl();

            UI_Interface.Settings = new Settings();
            UI_Interface.ProfileSettings = new ProfileSettings();
            UI_Interface.Xinput_S_Setup = new XInputShortcutsSetup();

            UI_Actions.InitUIActions();

            Core_Interface.GameControlsInfo = new Dictionary<UserGameInfo, GameControl>();
            Core_Interface.GameManager = new GameManager();
            Core_Interface.OptionsControl = new PlayerOptionsControl();

            hotkeysCooldownTimer = new System.Windows.Forms.Timer();
            hotkeysCooldownTimer.Tick += HotkeysCooldownTimerTick;

            SetCommonControlsAttributes();

            SortGameFunction.SortGames(UI_Interface.SortOptionsPanel.SortGamesOptions);
            Core_Interface.InitializeGamepadThreads();

            if (!IsHandleCreated)//need this for custom scaling factors now(?) 
            {
                CreateHandle();
            }

            DPIManager.Register(this);
            DPIManager.AddForm(this);
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            float mainButtonFrameFont = UI_Interface.WindowPanel.Font.Size * 1.0f;

            if (scale > 1.0f)
            {
                foreach (Control button in UI_Interface.WindowPanel.Controls)
                {
                    if (button != InputsTextLabel)
                    {
                        button.Font = new Font(Theme_Settings.CustomFont, mainButtonFrameFont, button.Font.Style, GraphicsUnit.Pixel, 0);
                    }
                }
            }

            UI_Interface.GameOptionMenu.Font = new Font(GameOptionMenu.Font.FontFamily, 10.25f, UI_Interface.GameOptionMenu.Font.Style, GraphicsUnit.Pixel, 0);
            UI_Interface.SocialMenu.Font = new Font(GameOptionMenu.Font.FontFamily, 10.25f, UI_Interface.SocialMenu.Font.Style, GraphicsUnit.Pixel, 0);
            UI_Interface.HandlerNotes.Font = new Font(Theme_Settings.CustomFont, 12 * scale, UI_Interface.HandlerNotes.Font.Style, GraphicsUnit.Pixel, 0);

            UI_Interface.Logo.Location = new Point(UI_Interface.Logo.Location.X, UI_Interface.WindowPanel.Height / 2 - UI_Interface.Logo.Height / 2);
            UI_Interface.VersionTxt.Location = new Point(UI_Interface.Logo.Right, UI_Interface.Logo.Location.Y + UI_Interface.Logo.Height / 2 - UI_Interface.VersionTxt.Height / 2);
            UI_Interface.InputsTextLabel.Location = new Point(UI_Interface.WindowPanel.Width / 2 - UI_Interface.InputsTextLabel.Width / 2, (UI_Interface.WindowPanel.Bottom - UI_Interface.InputsTextLabel.Height) - 3);

            UI_Interface.MainButtonsPanelButton.Location = new Point(UI_Interface.WindowPanel.Width / 2 - UI_Interface.MainButtonsPanelButton.Width / 2, 4);
            UI_Interface.MainButtonsPanel.Location = new Point(UI_Interface.WindowPanel.Width / 2 - UI_Interface.MainButtonsPanel.Width / 2, 0);
            UI_Interface.MainButtonsPanelButton.Size = new Size((int)(39 * scale), (int)(10 * scale));

            UI_Interface.DefCoverLoc = UI_Interface.Cover.Location;
        }

        private void SetCommonControlsAttributes()
        {
            foreach (Control control in Generic_Functions.ListAllFormControls(this))
            {
                if (control.Name != "HandlerNoteTitle" && control.Name != "HandlerNotes" && control.Name != "Warning")
                {
                    if (!(control is TransparentRichTextBox) && control != InputsTextLabel && control != btn_Next && control != btn_Prev && control != ProfileButtonPanelLockPb)
                    {
                        control.Font = new Font(Theme_Settings.CustomFont, Theme_Settings.FontSize, control.Font.Style, GraphicsUnit.Pixel, 0);
                    }
                }

                if (control is Button)
                {
                    control.Cursor = Theme_Settings.Hand_Cursor;
                }

                control.Click += Generic_Functions.ClickAnyControl;
            }
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            WebStatusTimer = new System.Windows.Forms.Timer();
            WebStatusTimer.Interval = (2000);
            WebStatusTimer.Tick += WebStatusTimerTick;
            WebStatusTimer.Start();
            DPIManager.ForceUpdate();

            if (startArgs != null)
            {
                GameControl con = Core_Interface.GameControlsInfo?.Where(g => g.Value.GameInfo.GUID == startArgs[0]).FirstOrDefault().Value;
                if (con != null)
                {
                    UI_Functions.GameList_SelectedChanged(con, null);
                }
            }

            UI_Interface.HomeScreen.Focus();    
        }

        private void WebStatusTimerTick(object Object, EventArgs EventArgs)
        {
            if (connected)
            {
                UI_Interface.DwldAssetsButton.Enabled = true;
                WebStatusTimer.Dispose();
            }
            else
            {
                UI_Interface.DwldAssetsButton.Enabled = false;
            }
        }

        protected override Size DefaultSize => new Size(1050, 701);

        protected override void WndProc(ref Message m)
        {
            const int RESIZE_HANDLE_SIZE = 10;

            if (this.WindowState == FormWindowState.Normal)
            {
                if (m.Msg == 0x0084)/*NCHITTEST*/
                {
                    base.WndProc(ref m);

                    if ((int)m.Result == 0x01/*HTCLIENT*/)
                    {
                        Point screenPoint = new Point(m.LParam.ToInt32());
                        Point clientPoint = PointToClient(screenPoint);

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
            Rectangle outRect = new Rectangle(RESIZE_HANDLE_SIZE, RESIZE_HANDLE_SIZE, Width - RESIZE_HANDLE_SIZE * 2, Height - RESIZE_HANDLE_SIZE * 2);

            if (!outRect.Contains(cursorPos))
            {
                canResize = true;
            }
            else
            {
                if (Cursor.Current != Theme_Settings.Default_Cursor)
                {
                    Cursor.Current = Theme_Settings.Default_Cursor;
                }

                canResize = false;
            }

            if (!canResize)
            {
                if (m.Msg == 0x020)//Do not reset custom cursor when the mouse hover over the Form background(needed because of the custom resizing/moving messages handling) 
                {
                    m.Result = IntPtr.Zero;
                    base.WndProc(ref m);
                    return;
                }
            }

            if (hotkeysCooldown)
            {
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HotkeysRegistration.Cutscenes_HotkeyID &&
                Core_Interface.I_GameHandler != null)
            {
                GlobalWindowMethods.ToggleCutScenesMode();
                HotkeyCoolDown();
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == 0x0312 && LockInputRuntime.IsLocked)//WM_HOTKEY
            {
                Globals.MainOSD.Show(1600, $"Unlock Inputs First (Press {App_Hotkeys.LockInputs} Key)");
                base.WndProc(ref m);
                return;
            }

            if (m.Msg == 0x00FF)//WM_INPUT
            {
                RawInputAction(m.LParam);
            }
            else if (m.Msg == 0x0312)//WM_HOTKEY
            {
                if (Core_Interface.I_GameHandler != null)
                {
                    switch (m.WParam.ToInt32())
                    {
                        case HotkeysRegistration.TopMost_HotkeyID:
                            GlobalWindowMethods.ShowHideWindows();
                            break;

                        case HotkeysRegistration.StopSession_HotkeyID:
                            if ((string)btn_Play.Tag == "S T O P")
                            {
                                UI_Functions.Play_Click(btn_Play, null);
                                Globals.MainOSD.Show(2000, "Session Ended");
                            }
                            break;

                        case HotkeysRegistration.SetFocus_HotkeyID:
                            GlobalWindowMethods.ChangeForegroundWindow();
                            Globals.MainOSD.Show(2000, "Game Windows Unfocused");
                            break;

                        case HotkeysRegistration.ResetWindows_HotkeyID:
                            GlobalWindowMethods.ResetingWindows = true;
                            break;

                        case HotkeysRegistration.Switch_HotkeyID:
                            GlobalWindowMethods.SwitchLayout();
                            break;

                        case HotkeysRegistration.KillProcess_HotkeyID:
                            User32Util.ShowTaskBar();
                            Close();
                            break;

                        case HotkeysRegistration.Reminder_HotkeyID:
                            foreach (ShortcutsReminder reminder in GenericGameHandler.Instance.shortcutsReminders) { reminder.Toggle(7); }
                            break;

                        case HotkeysRegistration.MergerFocusSwitch_HotkeyID:
                            WindowsMerger.Instance?.SwitchChildFocus();
                            break;

                        case HotkeysRegistration.Custom_Hotkey_1:
                            GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_1.Invoke();
                            break;

                        case HotkeysRegistration.Custom_Hotkey_2:
                            GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_2.Invoke();
                            break;

                        case HotkeysRegistration.Custom_Hotkey_3:
                            GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_3.Invoke();
                            break;
                    }

                    HotkeyCoolDown();
                }
            }

            base.WndProc(ref m);
        }

        public void HotkeyCoolDown()
        {
            if (!hotkeysCooldown)
            {
                hotkeysCooldownTimer.Stop();
                hotkeysCooldownTimer.Interval = 600; //millisecond
                hotkeysCooldownTimer.Start();
                hotkeysCooldown = true;
            }
        }

        private void HotkeysCooldownTimerTick(object Object, EventArgs EventArgs)
        {
            hotkeysCooldown = false;
            hotkeysCooldownTimer.Stop();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            UI_Graphics.MainFormPaintBackground(e);
        }
    }
}
