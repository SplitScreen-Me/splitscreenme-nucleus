using Nucleus.Coop.Forms;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using WindowScrape.Static;


namespace Nucleus.Gaming.Forms
{
    public static class HotkeyListenerThread
    {
        public static void StartHotkeyListener()
        {
            Thread hotkeyListenerThread = new Thread(delegate ()
            {
                HotkeyListener hotkeyListener = new HotkeyListener();
                System.Windows.Threading.Dispatcher.Run();
            });

            hotkeyListenerThread.SetApartmentState(ApartmentState.STA); // needs to be STA or throws exception
            hotkeyListenerThread.Start();
        }
    }

    public partial class HotkeyListener : Form
    {
        const int WS_EX_TOOLWINDOW = 0x00000080;

        private const int WM_CLOSE = 0x0010;

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TOOLWINDOW; //Hide from Alt + Tab
                return cp;
            }
        }

        private bool hotkeysCooldown = false;
        private System.Windows.Forms.Timer hotkeysCooldownTimer;//Avoid hotkeys spamming
        public Action<IntPtr> RawInputAction { get; set; }
        
        public HotkeyListener()
        {
            InitializeComponent();

            ShowInTaskbar = false;
            
            InitTimer();
            base.CreateHandle();
            RegisterHotkeys();

            Hide();

            GenericGameHandler.Instance?.AllRuntimeForms.Add(this);
        }

        private void InitTimer()
        {
            hotkeysCooldownTimer = new System.Windows.Forms.Timer();
            hotkeysCooldownTimer.Tick += HotkeysCooldownTimerTick;
        }

        private void RegisterHotkeys()
        {
            HotkeysRegistration.RegHotkeys(this.Handle);

            if (GenericGameHandler.Instance?.CurrentGameInfo?.CustomHotkeys != null)
            {
                HotkeysRegistration.RegCustomHotkeys(GenericGameHandler.Instance.CurrentGameInfo);
            }
        }

        private void UnregisterHotkeys()
        {
            hotkeysCooldownTimer?.Dispose();

            HotkeysRegistration.UnRegHotkeys();

            if (GenericGameHandler.Instance?.CurrentGameInfo?.CustomHotkeys != null)
            {
                HotkeysRegistration.UnRegCustomHotkeys();
            }
        }

        protected override void WndProc(ref Message m)
        {         
            if (GenericGameHandler.Instance == null || hotkeysCooldown)//should never happen but...
            {
                base.WndProc(ref m);
                return;
            }

            #region Cutscenes Hotkey is default pass through locked inputs state
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == HotkeysRegistration.Cutscenes_HotkeyID)
            {
                GlobalWindowMethods.ToggleCutScenesMode();
                HotkeyCoolDown();
                base.WndProc(ref m);
                return;
            }
            #endregion

            #region Custom Hotkeys pass through locked inputs state
            if (m.Msg == 0x0312)//WM_HOTKEY
            {
                if (m.WParam.ToInt32() == HotkeysRegistration.Custom_Hotkey_1)
                {
                    if (HotkeysRegistration.ChkPassThrough[0])
                    {
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_1.Invoke();
                        Globals.MainOSD.Show(800, $"Custom Hotkey 1");
                        HotkeyCoolDown();

                        base.WndProc(ref m);
                        return;
                    }
                }
                else if (m.WParam.ToInt32() == HotkeysRegistration.Custom_Hotkey_2)
                {
                    if (HotkeysRegistration.ChkPassThrough[1])
                    {
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_2.Invoke();
                        Globals.MainOSD.Show(800, $"Custom Hotkey 2");
                        HotkeyCoolDown();

                        base.WndProc(ref m);
                        return;
                    }
                }
                else if (m.WParam.ToInt32() == HotkeysRegistration.Custom_Hotkey_3)
                {
                    if (HotkeysRegistration.ChkPassThrough[2])
                    {
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_3.Invoke();
                        Globals.MainOSD.Show(800, $"Custom Hotkey 3");
                        HotkeyCoolDown();

                        base.WndProc(ref m);
                        return;
                    }
                }
            }
            #endregion

            //Pass through locked inputs state Hotkeys must be placed before this check.
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
                switch (m.WParam.ToInt32())
                {
                    case HotkeysRegistration.TopMost_HotkeyID:
                        HotkeyCoolDown();
                        GlobalWindowMethods.ShowHideWindows();              
                        break;

                    case HotkeysRegistration.StopSession_HotkeyID:
                        UnregisterHotkeys();                      
                        GenericGameHandler.Instance?.End(false);             
                        Globals.MainOSD.Show(2000, "Session Ended");
                        break;

                    case HotkeysRegistration.SetFocus_HotkeyID:
                        HotkeyCoolDown();
                        GlobalWindowMethods.ChangeForegroundWindow();
                        Globals.MainOSD.Show(2000, "Game Windows Unfocused");          
                        break;

                    case HotkeysRegistration.ResetWindows_HotkeyID:
                        HotkeyCoolDown();
                        GlobalWindowMethods.ResetingWindows = true;                
                        break;

                    case HotkeysRegistration.Switch_HotkeyID:
                        HotkeyCoolDown();
                        GlobalWindowMethods.SwitchLayout();        
                        break;

                    case HotkeysRegistration.KillProcess_HotkeyID:
                        //The close event has to be handled by the launcher
                        PostMessage(Globals.MainWindowHandle, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
                        UnregisterHotkeys();
                        break;

                    case HotkeysRegistration.Reminder_HotkeyID:
                        HotkeyCoolDown();
                        foreach (ShortcutsReminder reminder in GenericGameHandler.Instance.shortcutsReminders) { reminder.Toggle(7); }            
                        break;

                    case HotkeysRegistration.MergerFocusSwitch_HotkeyID:
                        HotkeyCoolDown();
                        WindowsMerger.Instance?.SwitchChildFocus();                    
                        break;

                    case HotkeysRegistration.Custom_Hotkey_1:
                        HotkeyCoolDown();
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_1.Invoke();                        
                        break;

                    case HotkeysRegistration.Custom_Hotkey_2:
                        HotkeyCoolDown();
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_2.Invoke();                    
                        break;

                    case HotkeysRegistration.Custom_Hotkey_3:
                        HotkeyCoolDown();
                        GenericGameHandler.Instance.CurrentGameInfo.OnCustomHotKey_3.Invoke();                
                        break;
                }
            
            }

            base.WndProc(ref m);
        }

        public void HotkeyCoolDown()
        {
            if (!hotkeysCooldown)
            {
                hotkeysCooldownTimer.Stop();
                hotkeysCooldownTimer.Interval = 700; //millisecond
                hotkeysCooldownTimer.Start();
                hotkeysCooldown = true;
            }
        }

        private void HotkeysCooldownTimerTick(object Object, EventArgs EventArgs)
        {
            hotkeysCooldown = false;
            hotkeysCooldownTimer.Stop();
        }

    }
}
