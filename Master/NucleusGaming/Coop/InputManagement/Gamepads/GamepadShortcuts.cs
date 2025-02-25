using Nucleus.Coop.Forms;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Tools.GlobalWindowMethods;
using Nucleus.Gaming.Windows;
using SharpDX.XInput;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public static class GamepadShortcuts
    {
        //TODO: Add a button panel so we can shut down Nucleus and the pc with gamepads
        private static int SetFocus;
        private static int Close;
        private static int StopSession;
        private static int TopMost;
        private static int ResetWindows;
        private static int Cutscenes;
        private static int SwitchLayout;
        private static int LockInputs;
        private static int ReleaseCursor;
        private static int Pressed;
        private static int RightTriggerValue;
        private static int LeftTriggerValue;

        private static int RT = 9999;
        private static int LT = 10000;

        public static Thread GamepadShortcutsThread;
        private static State previousState;

        /// <summary>
        /// Controller shortcuts handling thread.
        /// </summary>
        public static void GamepadShortcutsUpdate()
        {
            while (true)
            {
                while (GenericGameHandler.Instance == null)
                {
                    Thread.Sleep(5000);
                }

                while (GenericGameHandler.Instance.hasEnded)
                {
                    Thread.Sleep(5000);
                }

                for (int i = 0; i < GamepadState.Controllers.Length; i++)
                {
                    if (!GamepadState.Controllers[i].IsConnected)
                    {
                        continue;
                    }

                    State currentState = (State)GamepadState.GetControllerState(i);

                    if (previousState.PacketNumber != currentState.PacketNumber)
                    {
                        int button = GamepadState.GetPressedButtons(i);
                        int rt = GamepadState.GetRightTriggerValue(i) > 0 ? button + RT : RT;///return RT + button or RT
                        int lt = GamepadState.GetLeftTriggerValue(i) > 0 ? button + LT : LT;///return LT + button or LT

                        if ((button == Cutscenes || rt == Cutscenes || lt == Cutscenes) && GameProfile.Saved)///cutscenes mode
                        {
                            GlobalWindowMethods.ToggleCutScenesMode();
                            Thread.Sleep(500);
                        }
                        else if ((button == SwitchLayout || rt == SwitchLayout || lt == SwitchLayout) && GameProfile.Saved)///Switch layout
                        {
                            GlobalWindowMethods.SwitchLayout();
                            Thread.Sleep(500);
                        }
                        else if ((button == ResetWindows || rt == ResetWindows || lt == ResetWindows) && GameProfile.Saved)///Reset windows
                        {
                            if (GenericGameHandler.Instance != null)
                                GlobalWindowMethods.ResetingWindows = true;
                            Thread.Sleep(500);

                        }
                        else if ((button == SetFocus || rt == SetFocus || lt == SetFocus))///Unfocus windows
                        {
                            GlobalWindowMethods.ChangeForegroundWindow();
                            Globals.MainOSD.Show(1600, $"Game Windows Unfocused");
                            Thread.Sleep(500);
                        }
                        else if ((button == TopMost || rt == TopMost || lt == TopMost))///Minimize/restore windows
                        {
                            GlobalWindowMethods.ShowHideWindows();
                            Thread.Sleep(500);
                        }
                        else if (button == StopSession || rt == StopSession || lt == StopSession)///End current session
                        {
                            if (!Gaming.Coop.InputManagement.LockInputRuntime.IsLocked)
                            {
                                if (GenericGameHandler.Instance != null)
                                {
                                    GenericGameHandler.Instance.End(true);
                                    Globals.MainOSD.Show(1600, $"Session Ended");
                                }
                            }
                            else
                            {
                                Globals.MainOSD.Show(1600, $"Unlock Inputs First");
                            }
                        }
                        else if (button == Close || rt == Close || lt == Close)///Close nucleus
                        {
                            GenericGameHandler.Instance?.End(false);

                            //User32Util.ShowTaskBar();

                            Thread.Sleep(5000);

                            if (GenericGameHandler.Instance.hasEnded)
                            {
                                Process.GetCurrentProcess().Kill();
                            }
                        }
                        else if ((button == LockInputs || rt == LockInputs || lt == LockInputs) && GameProfile.Saved)///Lock k&m inputs
                        {
                            if (!LockInputRuntime.IsLocked)
                            {
                                Globals.MainOSD.Show(1000, "Inputs Locked");

                                LockInputRuntime.Lock(GenericGameHandler.Instance.CurrentGameInfo?.LockInputSuspendsExplorer ?? true, GenericGameHandler.Instance.CurrentGameInfo?.ProtoInput.FreezeExternalInputWhenInputNotLocked ?? true, GameProfile.Game?.ProtoInput);

                                if (GenericGameHandler.Instance.CurrentGameInfo.ToggleUnfocusOnInputsLock)
                                {
                                    GlobalWindowMethods.ChangeForegroundWindow();
                                }

                                Thread.Sleep(1000);
                            }
                            else
                            {
                                LockInputRuntime.Unlock(GenericGameHandler.Instance.CurrentGameInfo?.ProtoInput.FreezeExternalInputWhenInputNotLocked ?? true, GenericGameHandler.Instance.CurrentGameInfo?.ProtoInput);
                                Globals.MainOSD.Show(1000, "Inputs Unlocked");
                            }
                        }
                        else if (button == ReleaseCursor || rt == LockInputs || lt == LockInputs)///Try to release the cursor from game window by alt+tab inputs
                        {
                            //Todo Try by pausing focus loop instead so we can release the cursor from its current window(but what for protoinput hooks?)
                            SendKeys.SendWait("%+{TAB}");
                            Thread.Sleep(500);
                        }

                        previousState = currentState;
                        Pressed = button;
                    }

                    RightTriggerValue = GamepadState.GetRightTriggerValue(i);
                    LeftTriggerValue = GamepadState.GetLeftTriggerValue(i);

                    #region Toggle shortcuts reminder window

                    if (Pressed == 1024)//Guide button
                    {
                        Thread.Sleep(900);//good enough to check for long press here

                        if (GamepadState.GetPressedButtons(i) == 1024)//good enough to check for long press here
                        {
                            foreach (ShortcutsReminder reminder in GenericGameHandler.Instance.shortcutsReminders)
                            {
                                reminder.Toggle(7);
                            }

                            Thread.Sleep(500);
                        }
                    }

                    #endregion                   
                }

                if (Pressed > 0)
                {
                    Thread.Sleep(350);
                }
                else
                {
                    Thread.Sleep(135);
                }
            }
        }

        public static void UpdateShortcutsValue()
        {
            SetFocus = App_GamePadShortcuts.SetFocus[0] + App_GamePadShortcuts.SetFocus[1];

            Close = App_GamePadShortcuts.Close[0] + App_GamePadShortcuts.Close[1];

            StopSession = App_GamePadShortcuts.StopSession[0] + App_GamePadShortcuts.StopSession[1];

            TopMost = App_GamePadShortcuts.TopMost[0] + App_GamePadShortcuts.TopMost[1];

            ResetWindows = App_GamePadShortcuts.ResetWindows[0] + App_GamePadShortcuts.ResetWindows[1];

            Cutscenes = App_GamePadShortcuts.CutscenesMode[0] + App_GamePadShortcuts.CutscenesMode[1];

            SwitchLayout = App_GamePadShortcuts.SwitchLayout[0] + App_GamePadShortcuts.SwitchLayout[1];

            LockInputs = App_GamePadShortcuts.LockInputs[0] + App_GamePadShortcuts.LockInputs[1];

            ReleaseCursor = App_GamePadShortcuts.TopMost[0] + App_GamePadShortcuts.TopMost[1];
        }
    }
}
