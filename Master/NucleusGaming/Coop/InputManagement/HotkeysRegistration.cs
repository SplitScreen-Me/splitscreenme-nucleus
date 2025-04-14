using Nucleus.Gaming.Windows.Interop;
using System.Windows.Forms;
using System;
using Nucleus.Gaming.App.Settings;

namespace Nucleus.Gaming.Coop.InputManagement
{
    public static class HotkeysRegistration
    {
        private static GenericGameInfo currentGameInfo;
        private static IntPtr formHandle;

        public const int KillProcess_HotkeyID = 1;
        public const int TopMost_HotkeyID = 2;
        public const int StopSession_HotkeyID = 3;
        public const int SetFocus_HotkeyID = 4;
        public const int ResetWindows_HotkeyID = 5;
        public const int Cutscenes_HotkeyID = 6;
        public const int Switch_HotkeyID = 7;
        public const int Reminder_HotkeyID = 8;
        public const int MergerFocusSwitch_HotkeyID = 9;

        public const int Custom_Hotkey_1 = 10;
        public const int Custom_Hotkey_2 = 11;
        public const int Custom_Hotkey_3 = 12;

        //Pass through input locked state       
        private static bool[] _chkPassThrough = new bool[] { false, false, false};
        public static bool[] ChkPassThrough => _chkPassThrough;

        private static int GetMod(string modifier)
        {
            int mod = 0;
            switch (modifier)
            {
                case "Ctrl":
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

        public static void RegHotkeys(IntPtr _formHandle)
        {
            formHandle = _formHandle;

            try
            {
                User32Interop.RegisterHotKey(_formHandle, KillProcess_HotkeyID, GetMod(App_Hotkeys.CloseApp[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.CloseApp[1]));
                User32Interop.RegisterHotKey(_formHandle, TopMost_HotkeyID, GetMod(App_Hotkeys.TopMost[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.TopMost[1]));
                User32Interop.RegisterHotKey(_formHandle, StopSession_HotkeyID, GetMod(App_Hotkeys.StopSession[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.StopSession[1]));
                User32Interop.RegisterHotKey(_formHandle, SetFocus_HotkeyID, GetMod(App_Hotkeys.SetFocus[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.SetFocus[1]));
                User32Interop.RegisterHotKey(_formHandle, ResetWindows_HotkeyID, GetMod(App_Hotkeys.ResetWindows[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.ResetWindows[1]));
                User32Interop.RegisterHotKey(_formHandle, Cutscenes_HotkeyID, GetMod(App_Hotkeys.CutscenesMode[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.CutscenesMode[1]));
                User32Interop.RegisterHotKey(_formHandle, Switch_HotkeyID, GetMod(App_Hotkeys.SwitchLayout[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.SwitchLayout[1]));
                User32Interop.RegisterHotKey(_formHandle, Reminder_HotkeyID, GetMod(App_Hotkeys.ShortcutsReminder[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.ShortcutsReminder[1]));
                User32Interop.RegisterHotKey(_formHandle, MergerFocusSwitch_HotkeyID, GetMod(App_Hotkeys.SwitchMergerForeGroundChild[0]), (int)Enum.Parse(typeof(Keys), App_Hotkeys.SwitchMergerForeGroundChild[1]));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering hotkeys " + ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }      

        public static void UnRegHotkeys()
        {
            try
            {
                User32Interop.UnregisterHotKey(formHandle, KillProcess_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, TopMost_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, StopSession_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, SetFocus_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, ResetWindows_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, Cutscenes_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, Switch_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, Reminder_HotkeyID);
                User32Interop.UnregisterHotKey(formHandle, MergerFocusSwitch_HotkeyID);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error unregistering hotkeys " + ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void RegCustomHotkeys(GenericGameInfo _currentGameInfo)
        {
            currentGameInfo = _currentGameInfo;

            try
            {
                if (_currentGameInfo.CustomHotkeys != null)
                {                 
                    for (int i = 0; i < _currentGameInfo.CustomHotkeys.Length; i++)
                    {
                        string[] keys = _currentGameInfo.CustomHotkeys[i].Split('|');

                        switch (i)
                        {
                            case 0:
                                _chkPassThrough[0] = bool.Parse(keys[0]);
                                User32Interop.RegisterHotKey(formHandle, Custom_Hotkey_1, GetMod(keys[1]), (int)Enum.Parse(typeof(Keys), keys[2]));
                                break;
                            case 1:
                                _chkPassThrough[1] = bool.Parse(keys[0]);
                                User32Interop.RegisterHotKey(formHandle, Custom_Hotkey_2, GetMod(keys[1]), (int)Enum.Parse(typeof(Keys), keys[2]));
                                break;
                            case 2:
                                _chkPassThrough[2] = bool.Parse(keys[0]);
                                User32Interop.RegisterHotKey(formHandle, Custom_Hotkey_3, GetMod(keys[1]), (int)Enum.Parse(typeof(Keys), keys[2]));
                                break;
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering hotkeys " + ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        public static void UnRegCustomHotkeys()
        {
            if (currentGameInfo == null)
            {
                return;
            }

            try
            {
                if (currentGameInfo.CustomHotkeys != null)
                {
                    for (int i = 0; i < currentGameInfo.CustomHotkeys.Length; i++)
                    {
                        switch (i)
                        {
                            case 0:
                                _chkPassThrough[0] = false;
                                User32Interop.UnregisterHotKey(formHandle, Custom_Hotkey_1);
                                break;
                            case 1:
                                _chkPassThrough[1] = false;
                                User32Interop.UnregisterHotKey(formHandle, Custom_Hotkey_2);
                                break;
                            case 2:
                                _chkPassThrough[2] = false;
                                User32Interop.UnregisterHotKey(formHandle, Custom_Hotkey_3);
                                break;
                        }                                          
                    }
                }

                currentGameInfo = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error registering hotkeys " + ex.Message, ex.ToString(), MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
