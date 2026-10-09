using Nucleus.Gaming.Coop.InputManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;


namespace Nucleus.Gaming.Coop.ProtoInput
{
    
    public static class ProtoInputLauncher
    {
        private static List<uint> trackedInstanceHandles = new List<uint>();

        public static void InjectStartup(
            string exePath,
            string commandLine,
            uint processCreationFlags,
            string dllFolderPath,
            int instanceIndex,
            GenericGameInfo gen,
            PlayerInfo player,
            out uint pid,
            IntPtr environment,
            int mouseHandle = -1,
            int keyboardHandle = -1,
            int controllerIndex = 0,
            int controllerIndex2 = 0,
            int controllerIndex3 = 0,
            int controllerIndex4 = 0
        )
        {
            if (!dllFolderPath.EndsWith("\\"))
            {
                dllFolderPath += "\\";
            }

            uint instanceHandle = ProtoInput.protoInput.EasyHookInjectStartup(exePath, commandLine, 0, dllFolderPath, out pid, environment);

            if (instanceHandle == 0)
            {
                MessageBox.Show("ProtoInput failed to startup inject", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                GenericGameHandler.Instance?.End(false);
            }
            else
            {
                SetupInstance(instanceHandle, instanceIndex, gen, player, mouseHandle, keyboardHandle, controllerIndex, controllerIndex2, controllerIndex3, controllerIndex4);

                ProtoInput.protoInput.WakeUpProcess(instanceHandle);
            }
        }

        public static void InjectRuntime(
            bool easyHookMethod,
            bool easyHookStealthMethod,
            bool remoteLoadLibMethod,
            uint pid,
            string dllFolderPath,
            int instanceIndex,
            GenericGameInfo gen,
            PlayerInfo player,
            int mouseHandle = -1,
            int keyboardHandle = -1,
            int controllerIndex = 0,
            int controllerIndex2 = 0,
            int controllerIndex3 = 0,
            int controllerIndex4 = 0
            )
        {
            if (!dllFolderPath.EndsWith("\\"))
            {
                dllFolderPath += "\\";
            }

            uint instanceHandle = 0;

            if (easyHookStealthMethod)
            {
                instanceHandle = ProtoInput.protoInput.EasyHookStealthInjectRuntime(pid, dllFolderPath);
            }
            else if (easyHookMethod)
            {
                instanceHandle = ProtoInput.protoInput.EasyHookInjectRuntime(pid, dllFolderPath);
            }
            else if (remoteLoadLibMethod)
            {
                instanceHandle = ProtoInput.protoInput.RemoteLoadLibraryInjectRuntime(pid, dllFolderPath);
            }

            if (instanceHandle == 0)
            {
                MessageBox.Show("ProtoInput failed to runtime inject", "Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                GenericGameHandler.Instance?.End(false);
            }
            else
            {
                SetupInstance(instanceHandle, instanceIndex, gen, player, mouseHandle, keyboardHandle,
                    controllerIndex, controllerIndex2, controllerIndex3, controllerIndex4);
            }
        }

        public static void SetProtoControllersIndex(PlayerInfo player, GenericGameInfo game)
        {
            if(game.UseManualProtoControllersSetup)
            {
                GenericGameHandler.Instance?.Log("Force manual ProtoInput controller setup from Game.Play function.");
                return;
            }

            if (game.ProtoInput.MultipleProtoControllers)
            {
                player.ProtoController1 = player.GamepadId + 1;

                if (player.InstanceGuests.Count > 0)
                {
                    for (int i = 0; i < player.InstanceGuests.Count; i++)
                    {
                        int guestIndex = player.InstanceGuests[i].GamepadId + 1;

                        switch (i)
                        {
                            case 0:
                                player.ProtoController2 = guestIndex;
                                break;
                            case 1:
                                player.ProtoController3 = guestIndex;
                                break;
                            case 2:
                                player.ProtoController4 = guestIndex;
                                break;
                        }
                    }
                }
                else
                {
                    player.ProtoController2 = player.GamepadId + 1;
                    player.ProtoController3 = player.GamepadId + 1;
                    player.ProtoController4 = player.GamepadId + 1;
                }
            }
            else
            {
                player.ProtoController1 = player.GamepadId + 1;
                player.ProtoController2 = 0;
                player.ProtoController3 = 0;
                player.ProtoController4 = 0;
            }
        }


        private static void SetupInstance(uint instanceHandle, int instanceIndex, GenericGameInfo gen, PlayerInfo player, int mouseHandle, int keyboardHandle, int controllerIndex, int controllerIndex2, int controllerIndex3, int controllerIndex4)
        {
            Debug.WriteLine("Setting up ProtoInput instance " + instanceIndex);

            player.ProtoInputInstanceHandle = instanceHandle;

            ProtoInput.protoInput.SetupState(instanceHandle, instanceIndex);

            if (mouseHandle != -1)
            {
                ProtoInput.protoInput.AddSelectedMouseHandle(instanceHandle, (uint)mouseHandle);
            }

            if (keyboardHandle != -1)
            {
                ProtoInput.protoInput.AddSelectedKeyboardHandle(instanceHandle, (uint)keyboardHandle);
            }

            ProtoInput.protoInput.SetTranslateXinputtoMKB(instanceHandle, gen.ProtoInput.TranslateXinputtoMKB);

            if (gen.ProtoInput.RegisterRawInputHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.RegisterRawInputHookID);
            }

            if (gen.ProtoInput.GetRawInputDataHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetRawInputDataHookID);
            }

            if (gen.ProtoInput.MessageFilterHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.MessageFilterHookID);
            }

            if (gen.ProtoInput.GetCursorPosHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetCursorPosHookID);
            }

            if (gen.ProtoInput.SetCursorPosHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.SetCursorPosHookID);
            }

            if (gen.ProtoInput.GetKeyStateHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetKeyStateHookID);
            }

            if (gen.ProtoInput.GetAsyncKeyStateHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetAsyncKeyStateHookID);
            }

            if (gen.ProtoInput.GetKeyboardStateHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetKeyboardStateHookID);
            }

            if (gen.ProtoInput.ReRegisterInput)
            {
                ProtoInput.protoInput.SetReregisterinput(instanceHandle, gen.ProtoInput.ReRegisterInput);
            }
            if (gen.ProtoInput.PointerInMouse)
            {
                ProtoInput.protoInput.SetPointerInMouse(instanceHandle, gen.ProtoInput.PointerInMouse);
            }

            if (gen.ProtoInput.MessageSubWindows)
            {
                ProtoInput.protoInput.SetSendMessagesToSubWindows(instanceHandle, gen.ProtoInput.MessageSubWindows);
            }

            ProtoInput.protoInput.SetShowCursorWhenImageUpdated(instanceHandle, !gen.ProtoInput.DontShowCursorWhenImageUpdated);
            if (gen.ProtoInput.CursorVisibilityHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.CursorVisibilityStateHookID);
            }

            ProtoInput.protoInput.SetCursorClipOptions(instanceHandle, gen.ProtoInput.ClipCursorHookCreatesFakeClip);
            if (gen.ProtoInput.ClipCursorHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.ClipCursorHookID);
            }

            if (gen.ProtoInput.FocusHooks)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.FocusHooksHookID);
            }

            if (gen.ProtoInput.RenameHandlesHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.RenameHandlesHookID);
            }

            if (gen.ProtoInput.BlockRawInputHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.BlockRawInputHookID);
            }

            if (gen.ProtoInput.FindWindowHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.FindWindowHookID);
            }

            ProtoInput.protoInput.SetCreateSingleHIDName(instanceHandle, player.RawHID);
            if (gen.ProtoInput.CreateSingleHIDHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.CreateSingleHIDHookID);
            }

            if (gen.ProtoInput.SetWindowStyleHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.WindowStyleHookID);
            }

            if (gen.ProtoInput.GetCursorInfoHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetCursorInfoID);
            }
            if (gen.ProtoInput.SetWindowsHookHook) 
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.SetWindowsHookHookID);
            }
            if (gen.ProtoInput.GetRawInputBufferHook) 
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.GetRawInputBufferHookID);
            }
            ProtoInput.protoInput.SetUseOpenXinput(instanceHandle, gen.ProtoInput.UseOpenXinput);
            ProtoInput.protoInput.SetTranslateMKBtoXinput(instanceHandle, gen.ProtoInput.TranslateMKBtoXinput);
            ProtoInput.protoInput.SetUseDinputRedirection(instanceHandle, gen.ProtoInput.UseDinputRedirection);
            if (gen.ProtoInput.XinputHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.XinputHookID);
            }

            if (player.IsDInput)
            {
                ProtoInput.protoInput.SetDinputDeviceGUID(instanceHandle, player.GamepadGuid);
            }

            if (gen.ProtoInput.DinputHookAlsoHooksGetDeviceState)
            {
                ProtoInput.protoInput.SetDinputHookAlsoHooksGetDeviceState(instanceHandle, true);
            }

            if (gen.ProtoInput.DinputDeviceHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.DinputOrderHookID);
            }


            ProtoInput.protoInput.SetSetWindowPosDontResize(instanceHandle, gen.ProtoInput.SetWindowPosHook == SetWindowPosHook.DontResize);
            ProtoInput.protoInput.SetSetWindowPosDontReposition(instanceHandle, gen.ProtoInput.SetWindowPosHook == SetWindowPosHook.DontReposition);
            ProtoInput.protoInput.SetSetWindowPosSettings(instanceHandle, player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height);

            if (gen.ProtoInput.SetWindowPosHook == SetWindowPosHook.DontResize || gen.ProtoInput.SetWindowPosHook == SetWindowPosHook.DontReposition || gen.ProtoInput.SetWindowPosHook != 0)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.SetWindowPosHookID);
            }

            if (gen.ProtoInput.RawInputFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.RawInputFilterID);
            }

            if (gen.ProtoInput.MouseMoveFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.MouseMoveFilterID);
            }

            if (gen.ProtoInput.MouseActivateFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.MouseActivateFilterID);
            }

            if (gen.ProtoInput.WindowActivateFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.WindowActivateFilterID);
            }

            if (gen.ProtoInput.WindowActvateAppFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.WindowActivateAppFilterID);
            }

            if (gen.ProtoInput.MouseWheelFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.MouseWheelFilterID);
            }

            if (gen.ProtoInput.MouseButtonFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.MouseButtonFilterID);
            }

            if (gen.ProtoInput.KeyboardButtonFilter)
            {
                ProtoInput.protoInput.EnableMessageFilter(instanceHandle, ProtoInput.ProtoMessageFilterIDs.KeyboardButtonFilterID);
            }

            if (gen.ProtoInput.MoveWindowHook == MoveWindowHook.DontResize || gen.ProtoInput.MoveWindowHook == MoveWindowHook.DontReposition || gen.ProtoInput.MoveWindowHook != 0)
            {
                ProtoInput.protoInput.SetMoveWindowDontResize(instanceHandle, gen.ProtoInput.MoveWindowHook == MoveWindowHook.DontResize);
                ProtoInput.protoInput.SetMoveWindowDontReposition(instanceHandle, gen.ProtoInput.MoveWindowHook == MoveWindowHook.DontReposition);
                ProtoInput.protoInput.SetMoveWindowSettings(instanceHandle, player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height);
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.MoveWindowHookID);
            }

            ProtoInput.protoInput.SetAdjustWindowRectSettings(instanceHandle, player.MonitorBounds.X, player.MonitorBounds.Y, player.MonitorBounds.Width, player.MonitorBounds.Height);

            ProtoInput.protoInput.SetManualScaling(instanceHandle, gen.ProtoInput.ScaleFromX, gen.ProtoInput.ScaleFromY, player.MonitorBounds.Width, player.MonitorBounds.Height);

            //standard values for XinputtoMKB
            if (gen.ProtoInput.TranslateXSensitivity == 0)
                gen.ProtoInput.TranslateXSensitivity = 12;

            if (gen.ProtoInput.TranslateXAccelration == 0)
                gen.ProtoInput.TranslateXAccelration = 4;

            if (gen.ProtoInput.TranslateXButtonA == 0)
                gen.ProtoInput.TranslateXButtonA = (int)Key.R;

            if (gen.ProtoInput.TranslateXButtonB == 0)
                gen.ProtoInput.TranslateXButtonB = (int)Key.C;

            if (gen.ProtoInput.TranslateXButtonX == 0)
                gen.ProtoInput.TranslateXButtonX = (int)Key.E;

            if (gen.ProtoInput.TranslateXButtonY == 0)
                gen.ProtoInput.TranslateXButtonY = (int)Key.G;

            if (gen.ProtoInput.TranslateXButtonRS == 0)
                gen.ProtoInput.TranslateXButtonRS = (int)Key.Space;

            if (gen.ProtoInput.TranslateXButtonLS == 0)
                gen.ProtoInput.TranslateXButtonLS = (int)Key.Control;

            if (gen.ProtoInput.TranslateXButtonUp == 0)
                gen.ProtoInput.TranslateXButtonUp = (int)Key.Up;

            if (gen.ProtoInput.TranslateXButtonDown == 0)
                gen.ProtoInput.TranslateXButtonDown = (int)Key.Down;

            if (gen.ProtoInput.TranslateXButtonRight == 0)
                gen.ProtoInput.TranslateXButtonRight = (int)Key.Right;

            if (gen.ProtoInput.TranslateXButtonLeft == 0)
                gen.ProtoInput.TranslateXButtonLeft = (int)Key.Left;

            if (gen.ProtoInput.TranslateXStickUp == 0)
                gen.ProtoInput.TranslateXStickUp = (int)Key.W;
            if (gen.ProtoInput.TranslateXStickDown == 0)
                gen.ProtoInput.TranslateXStickDown = (int)Key.S;

            if (gen.ProtoInput.TranslateXStickRight == 0)
                gen.ProtoInput.TranslateXStickRight = (int)Key.D;

            if (gen.ProtoInput.TranslateXStickLeft == 0)
                gen.ProtoInput.TranslateXStickLeft = (int)Key.A;

            if (gen.ProtoInput.TranslateXButtonStart == 0)
                gen.ProtoInput.TranslateXButtonStart = (int)Key.Return;

            if (gen.ProtoInput.TranslateXButtonOption == 0)
                gen.ProtoInput.TranslateXButtonOption = (int)Key.Escape;

            ProtoInput.protoInput.SetXinputtoMKBkeys(instanceHandle, gen.ProtoInput.TranslateXButtonA, gen.ProtoInput.TranslateXButtonB, gen.ProtoInput.TranslateXButtonX, gen.ProtoInput.TranslateXButtonY, gen.ProtoInput.TranslateXButtonRS, gen.ProtoInput.TranslateXButtonLS, gen.ProtoInput.TranslateXButtonRight, gen.ProtoInput.TranslateXButtonLeft, gen.ProtoInput.TranslateXButtonUp, gen.ProtoInput.TranslateXButtonDown, gen.ProtoInput.TranslateXStickR, gen.ProtoInput.TranslateXStickL, gen.ProtoInput.TranslateXStickRight, gen.ProtoInput.TranslateXStickLeft, gen.ProtoInput.TranslateXStickUp, gen.ProtoInput.TranslateXStickDown , gen.ProtoInput.TranslateXButtonOption, gen.ProtoInput.TranslateXButtonStart, gen.ProtoInput.TranslateXSensitivity, gen.ProtoInput.TranslateXAccelration, gen.ProtoInput.TranslateXDeadzone);
            ProtoInput.protoInput.SetXinputtoMKBCFG(instanceHandle, gen.ProtoInput.TranslateXSwapSticks, gen.ProtoInput.TranslateXScanner, gen.ProtoInput.TranslateXShouldersNextPoint, gen.ProtoInput.TranslateXAstatic, gen.ProtoInput.TranslateXAPointClick, gen.ProtoInput.TranslateXAPointMove, gen.ProtoInput.TranslateXBstatic, gen.ProtoInput.TranslateXBPointClick, gen.ProtoInput.TranslateXBPointMove, gen.ProtoInput.TranslateXXstatic, gen.ProtoInput.TranslateXXPointClick, gen.ProtoInput.TranslateXXPointMove, gen.ProtoInput.TranslateXYstatic, gen.ProtoInput.TranslateXYPointClick, gen.ProtoInput.TranslateXYPointMove);
            if (gen.ProtoInput.AdjustWindowRectHook)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.AdjustWindowRectHookID);
            }

            ProtoInput.protoInput.SetDontWaitWindowBorder(instanceHandle, gen.ProtoInput.SetRemoveBorderHook == SetRemoveBorderHook.DontWait);

            if (gen.ProtoInput.RemoveBorders || gen.ProtoInput.SetPosition)
                ProtoInput.protoInput.SetWindowSetup(instanceHandle, gen.ProtoInput.RemoveBorders, gen.ProtoInput.SetPosition);

            if (gen.ProtoInput.SetRemoveBorderHook == SetRemoveBorderHook.DontWait || gen.ProtoInput.SetRemoveBorderHook != 0)
            {
                ProtoInput.protoInput.InstallHook(instanceHandle, ProtoInput.ProtoHookIDs.RemoveBorderHookID);
            }

            if (gen.ProtoInput.BlockedMessages != null)
            {
                foreach (uint blockedMessage in gen.ProtoInput.BlockedMessages)
                {
                    ProtoInput.protoInput.EnableMessageBlock(instanceHandle, blockedMessage);
                }
            }

            ProtoInput.protoInput.SetupMessagesToSend(instanceHandle,
                                gen.ProtoInput.SendMouseWheelMessages,
                                gen.ProtoInput.SendMouseButtonMessages,
                                gen.ProtoInput.SendMouseMovementMessages,
                                gen.ProtoInput.SendKeyboardButtonMessages,
                                gen.ProtoInput.SendMouseDblClkMessages);

            if (gen.ProtoInput.EnableFocusMessageLoop)
            {
                ProtoInput.protoInput.StartFocusMessageLoop(instanceHandle,
                                      gen.ProtoInput.FocusLoopIntervalMilliseconds,
                                      gen.ProtoInput.FocusLoop_WM_ACTIVATE,
                                      gen.ProtoInput.FocusLoop_WM_ACTIVATEAPP,
                                      gen.ProtoInput.FocusLoop_WM_NCACTIVATE,
                                      gen.ProtoInput.FocusLoop_WM_SETFOCUS,
                                      gen.ProtoInput.FocusLoop_WM_MOUSEACTIVATE);
            }

            ProtoInput.protoInput.SetDrawFakeCursorFix(instanceHandle, gen.ProtoInput.DrawFakeCursor == DrawFakeCursor.Fix);
            if (gen.ProtoInput.SetCursorPosHookAlsoMessage)
            {
                ProtoInput.protoInput.SetSetCursorPosHookMessages(instanceHandle, true);
            }
            ProtoInput.protoInput.SetDrawFakeCursor(instanceHandle, gen.ProtoInput.DrawFakeCursor == DrawFakeCursor.Fix || gen.ProtoInput.DrawFakeCursor != 0);
            ProtoInput.protoInput.SetDefaultTopLeftMouseBounds(instanceHandle, gen.ProtoInput.PutMouseInsideWindow == PutMouseInsideWindow.IgnoreTopLeft);
            ProtoInput.protoInput.SetDefaultBottomRightMouseBounds(instanceHandle, gen.ProtoInput.PutMouseInsideWindow == PutMouseInsideWindow.IgnoreBottomRight);
            ProtoInput.protoInput.SetPutMouseInsideWindow(instanceHandle, gen.ProtoInput.PutMouseInsideWindow == PutMouseInsideWindow.IgnoreTopLeft || gen.ProtoInput.PutMouseInsideWindow == PutMouseInsideWindow.IgnoreBottomRight || gen.ProtoInput.PutMouseInsideWindow != 0);
            ProtoInput.protoInput.AllowFakeCursorOutOfBounds(instanceHandle, gen.ProtoInput.AllowFakeCursorOutOfBounds, gen.ProtoInput.ExtendFakeCursorBounds);
            ProtoInput.protoInput.SetToggleFakeCursorVisibilityShortcut(instanceHandle,
                gen.ProtoInput.EnableToggleFakeCursorVisibilityShortcut,
                gen.ProtoInput.ToggleFakeCursorVisibilityShortcutVkey);

            if (gen.ProtoInput.RenameHandles != null)
            {
                foreach (string renameHandle in gen.ProtoInput.RenameHandles)
                {
                    ProtoInput.protoInput.AddHandleToRename(instanceHandle, renameHandle);
                }
            }

            if (gen.ProtoInput.RenameNamedPipes != null)
            {
                foreach (string renamePipe in gen.ProtoInput.RenameNamedPipes)
                {
                    ProtoInput.protoInput.AddNamedPipeToRename(instanceHandle, renamePipe);
                }
            }



            ProtoInput.protoInput.SetControllerIndex(instanceHandle,
                controllerIndex < 0 ? 0 : (uint)controllerIndex,
                controllerIndex2 < 0 ? 0 : (uint)controllerIndex2,
                controllerIndex3 < 0 ? 0 : (uint)controllerIndex3,
                controllerIndex4 < 0 ? 0 : (uint)controllerIndex4
                );

            //SetExternalFreezeFakeInput(instanceHandle, !isInputCurrentlyLocked && freezeGameInputWhileInputNotLocked);
            if (gen.ForwardWindowToProtoInput)
            {
                ProtoInput.protoInput.UpdateMainWindowHandle(
                (uint)instanceHandle,
                    (ulong)GetMainWindow.NucleusGetMainWindowHandle(player.ProcessData?.Process)
                );

            }

            trackedInstanceHandles.Add(instanceHandle);


            if (gen.ProtoInput.FreezeExternalInputWhenInputNotLocked)
            {
                NotifyInputLockChange();

            }
        }
        
        public static void NotifyInputLockChange()
        {
            bool freezeExternal = !LockInputRuntime.IsLocked;

            foreach (uint instanceHandle in trackedInstanceHandles)
            {
                ProtoInput.protoInput.SetExternalFreezeFakeInput(instanceHandle, freezeExternal);
            }
        }
    }
}
