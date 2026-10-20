using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.ProtoInput;
using Nucleus.Gaming.Windows.Interop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Nucleus.Gaming.Tools.WindowFakeFocus
{
    public static class WindowFakeFocus
    {
        public enum FocusMessages
        {
            WM_ACTIVATEAPP = 0x001C,
            WM_ACTIVATE = 0x0006,
            WM_NCACTIVATE = 0x0086,
            WM_SETFOCUS = 0x0007,
            WM_MOUSEACTIVATE = 0x0021,
        }

        public static Thread fakeFocus;

        private static GenericGameHandler genericGameHandler;
        private static GenericGameInfo gen;
        private static GameProfile profile;

        public static void Initialize()
        {
            genericGameHandler = GenericGameHandler.Instance;
            gen = GenericGameHandler.Instance.CurrentGameInfo;
            profile = GenericGameHandler.Instance.profile;
        }

        public static void SendFocusMsgs()
        {
            List<PlayerInfo> players = profile.DevicesList;
            if (players == null)
            {
                return;
            }

            List<Process> fakeFocusProcs = new List<Process>();
            var windows = RawInputManager.windows;
            string ffPIDs = "";

            if (gen.FakeFocusInstances?.Length > 0)
            {
                string[] fakeFocusInstances = gen.FakeFocusInstances.Split(',');
                for (int i = 0; i < fakeFocusInstances.Length; i++)
                {
                    if (int.Parse(fakeFocusInstances[i]) <= genericGameHandler.numPlayers)
                    {
                        fakeFocusProcs.Add(genericGameHandler.attached[int.Parse(fakeFocusInstances[i]) - 1]);
                    }
                }
            }
            else
            {
                fakeFocusProcs = genericGameHandler.attached;
            }

            if (gen.KeyboardPlayerSkipFakeFocus)
            {
                for (int i = 0; i < fakeFocusProcs.Count; i++)
                {
                    if (fakeFocusProcs[i].Id == genericGameHandler.keyboardProcId)
                    {
                        fakeFocusProcs.RemoveAt(i);
                    }
                }
            }

            foreach (Process p in fakeFocusProcs)
            {
                ffPIDs = ffPIDs + p.Id + " ";
            }

            try
            {
                while (!GenericGameHandler.Instance.HasEnded)//orig true
                {
                    Thread.Sleep(gen.FakeFocusInterval);

                    foreach (Process proc in genericGameHandler.attached)
                    {
                        IntPtr hWnd = proc.NucleusGetMainWindowHandle();
                        ////TODO: NCACTIVATE is a bad idea?

                        User32Interop.SendMessage(hWnd, (int)FocusMessages.WM_ACTIVATEAPP, (IntPtr)1, IntPtr.Zero);
                        User32Interop.SendMessage(hWnd, (int)FocusMessages.WM_NCACTIVATE, (IntPtr)0x00000001, IntPtr.Zero);
                        User32Interop.SendMessage(hWnd, (int)FocusMessages.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                        User32Interop.SendMessage(hWnd, (int)FocusMessages.WM_MOUSEACTIVATE, (IntPtr)hWnd, (IntPtr)1);

                        //Deep Rock Galactic doesn't work with this message
                        if (gen.FakeFocusSendActivate)
                        {
                            try
                            {
                                for (int p = 0; p < genericGameHandler.numPlayers; p++)
                                {

                                    if (gen.FakeFocusSendActivateIgnoreKB && (players[p].IsRawKeyboard || players[p].IsKeyboardPlayer) && players[p].ProcessID == proc.Id)
                                    {
                                        continue;
                                    }
                                    else if (players[p].ProcessID == proc.Id)
                                    {
                                        User32Interop.SendMessage(hWnd, (int)FocusMessages.WM_ACTIVATE, (IntPtr)0x00000002, IntPtr.Zero);
                                        break;
                                    }
                                }
                            }
                            catch { }
                        }
                    }

                    if (gen.PreventGameFocus)
                    {
                        foreach (var window in windows)
                        {
                            window.HookPipe.SendPreventForegroundWindow();
                        }
                    }
                }

                //Console.WriteLine("Fake focus window thread released");
            }
            catch (Exception)
            {
                //genericGameHandler.Log($"ThreadAbortException in FakeFocus. Exiting. Error: {e}");
            }
        }

        public static void SendFakeFocusMsg()
        {
            foreach (PlayerInfo plyr in profile.DevicesList)
            {
                Thread.Sleep(1000);
                genericGameHandler.Log("Send fake focus messages to process " + plyr.ProcessData.Process.Id);
                Process plyrProc = plyr.ProcessData.Process;

                if (gen.FakeFocusSendActivate)
                {
                    User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATE, (IntPtr)0x00000002, IntPtr.Zero);
                }

                User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_ACTIVATEAPP, (IntPtr)1, IntPtr.Zero);
                User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_NCACTIVATE, (IntPtr)0x00000001, IntPtr.Zero);
                User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_SETFOCUS, IntPtr.Zero, IntPtr.Zero);
                User32Interop.SendMessage(plyrProc.NucleusGetMainWindowHandle(), (int)FocusMessages.WM_MOUSEACTIVATE, (IntPtr)plyrProc.NucleusGetMainWindowHandle(), (IntPtr)1);
            }
        }
    }
}
