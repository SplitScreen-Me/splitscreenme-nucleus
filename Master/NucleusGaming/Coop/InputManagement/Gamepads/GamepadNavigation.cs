using Nucleus.Gaming.App.Settings;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    public static class GamepadNavigation
    {
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_WHEEL = 0x0800;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x0008;
        public const int MOUSEEVENTF_RIGHTUP = 0x0010;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public static implicit operator Point(POINT point)
            {
                return new Point(point.X, point.Y);
            }
        }

        public static Thread GamepadNavigationThread;

        private static Point GetCursorPos()
        {
            GetCursorPos(out POINT cursor);
            return cursor;
        }

        private static int Deadzone;
        private static int Dragdrop;
        private static int RightClick;
        private static int LeftClick;
        private static int LockUIControl;
        private static int OpenOsk;
        private static int RT = 9999;
        private static int LT = 10000;

        private static bool _Enabled;
        public static bool Enabled => _Enabled;
        private static bool EnabledRuntime;///Can on/off UI navigation manually later on runtime
        private static int DefaultSpeed => 2000;//2200 v2.3.2 value
        private static int SlowDownSpeed = 2000;//2200 v2.3.2 value
        public static Action OnUpdateState;
        public static List<bool> ReceiveInputs =  new List<bool> {false,false,false,false};
        private static List<bool> prevReceiveInputs = new List<bool> {false,false,false,false};

        public static void StopUINavigation()
        {
            if (_Enabled) 
            EnabledRuntime = false;
        }

        public static void StartUINavigation()
        {
            if (_Enabled)
                EnabledRuntime = true;
        }

        public static void SetCursorSpeed()
        {
            if (SlowDownSpeed != DefaultSpeed * 3)
            {
                SlowDownSpeed *= 3;
            }
            else
            {
                SlowDownSpeed = DefaultSpeed;
            }
        }

        public static void GamepadNavigationUpdate()
        {
            int x = GetCursorPos().X;///Init current cursor X
            int y = GetCursorPos().Y;///Init current cursor Y

            int steps = 1;///How many pixels the cursor will move
            int scrollStep = 1;
            ///Deadzone => Joystick value from where the cursor will start moving
            int pollRate = 15;///How often the thread will sleep
            List<int> prevPressed = new List<int> {0,0,0,0};///previous Xinput button state 
            bool dragging = false;
            EnabledRuntime = _Enabled;

            while (true)
            {
                for (int i = 0; i < GamepadState.Controllers.Length; i++)
                {
                    if (!GamepadState.Controllers[i].IsConnected)
                    {
                        if (prevReceiveInputs[i] || ReceiveInputs[i]|| prevPressed[i] > 0)
                        {
                            prevPressed[i] = 0;
                            prevReceiveInputs[i] = false;
                            ReceiveInputs[i] = false;
                            OnUpdateState?.Invoke();
                        }

                        continue;
                    }

                    GetCursorPos(out POINT cursor);
                    x = cursor.X;
                    y = cursor.Y;

                    int pressed = GamepadState.GetPressedButtons(i);/// Current pressed Xinput button

                    if (pressed == 128 && prevPressed[i] != pressed)//R3/R right thumb stick click.
                    {
                        SetCursorSpeed();
                        Thread.Sleep(150);
                    }

                    bool isRT = false;
                    int rt = 0;

                    if (GamepadState.GetRightTriggerValue(i) > 0)
                    {
                        isRT = true;
                        rt = pressed > 0 ? pressed + RT : RT;
                        pressed = rt;
                    }

                    bool isLT = false;
                    int lt = 0;

                    if (GamepadState.GetLeftTriggerValue(i) > 0)
                    {
                        isLT = true;
                        lt = pressed > 0 ? pressed + LT : LT;
                        pressed = lt;
                    }

                    ///Adjust the cursor speed to the joystick value
                    int MouveLeftSpeed = (Math.Abs((GamepadState.GetLeftStickValue(i).Item1) / SlowDownSpeed));
                    int MouveRightSpeed = ((GamepadState.GetLeftStickValue(i).Item1) / SlowDownSpeed);
                    int MouveUpSpeed = (Math.Abs((GamepadState.GetLeftStickValue(i).Item2) / SlowDownSpeed));
                    int MouveDownSpeed = ((GamepadState.GetLeftStickValue(i).Item2) / SlowDownSpeed);

                    ///Check if the right joystick values are out of the deadzone and allow to move the cursor or not
                    bool canMouveLeft = GamepadState.GetLeftStickValue(i).Item1 <= -Deadzone;
                    bool canMouveRight = GamepadState.GetLeftStickValue(i).Item1 >= Deadzone;
                    bool canMouveUp = GamepadState.GetLeftStickValue(i).Item2 <= -Deadzone;
                    bool canMouveDown = GamepadState.GetLeftStickValue(i).Item2 >= Deadzone;

                    ///Adjust scrolling speed to the joystick value
                    int ScrollUpSpeed = Math.Abs((GamepadState.GetRightStickValue(i).Item2) / 1000);
                    int ScrollDownSpeed = (GamepadState.GetRightStickValue(i).Item2) / 1000;

                    ///Check if the left joystick value(Y axis only) is out of the deadzone and allow to scroll Up or Down or not
                    bool canScrollUp = GamepadState.GetRightStickValue(i).Item2 >= Deadzone;
                    bool canScrollDown = GamepadState.GetRightStickValue(i).Item2 <= -Deadzone;

                    if (Enabled)
                    {
                        if (EnabledRuntime && (pressed == LockUIControl))
                        {
                            EnabledRuntime = false;
                            Globals.MainOSD.Show(1600, $"UI Control Locked");
                            Thread.Sleep(500);
                        }
                        else if (!EnabledRuntime && (pressed == LockUIControl))
                        {
                            EnabledRuntime = true;
                            Globals.MainOSD.Show(1600, $"UI Control Unlocked");
                            Thread.Sleep(500);
                        }
                    }

                    if (!EnabledRuntime)
                    {
                        Thread.Sleep(500);
                        continue;
                    }

                    ReceiveInputs[i] = false;

                    if (canScrollUp)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, cursor.X, cursor.Y, scrollStep * ScrollUpSpeed, 0x00A1);///Mouse wheel Up //dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here                      
                        ReceiveInputs[i] = true;
                        if (pressed != prevPressed[i] || prevReceiveInputs[i] != ReceiveInputs[i])
                        {
                            OnUpdateState?.Invoke();
                        }
                    }
                    else if (canScrollDown)
                    {
                        mouse_event(MOUSEEVENTF_WHEEL, cursor.X, cursor.Y, scrollStep * ScrollDownSpeed, 0x00A1);///Mouse wheel Down//dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        ReceiveInputs[i] = true;
                        if (pressed != prevPressed[i] || prevReceiveInputs[i] != ReceiveInputs[i])
                        {
                            OnUpdateState?.Invoke();
                        }
                    }

                    if (canMouveRight)
                    {
                        x += steps * MouveRightSpeed;
                    }

                    if (canMouveLeft)
                    {
                        x -= steps * MouveLeftSpeed;
                    }

                    if (canMouveUp)
                    {
                        y += steps * MouveUpSpeed;
                    }

                    if (canMouveDown)
                    {
                        y -= steps * MouveDownSpeed;
                    }

                    ///Set cursor position accordingly to the possibilities and values
                    if (canMouveRight || canMouveLeft || canMouveUp || canMouveDown)
                    {
                        ReceiveInputs[i] = true;

                        if (pressed != prevPressed[i] || prevReceiveInputs[i] != ReceiveInputs[i])
                        {
                            OnUpdateState?.Invoke();
                        }

                        SetCursorPos(x, y);
                    }

                    if ((pressed == LeftClick || (isRT && rt == LeftClick) || (isLT && lt == LeftClick)) && prevPressed[i] != pressed)///Left click and release(single click) 
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, cursor.X, cursor.Y, 0, 0x00A1);///Left Mouse Button Down //dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        Thread.Sleep(200);
                        mouse_event(MOUSEEVENTF_LEFTUP, cursor.X, cursor.Y, 0, 0x00A1);///Right Mouse Button Up//dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        dragging = false;
                    }

                    if ((pressed == RightClick || (isRT && rt == RightClick) || (isLT && lt == RightClick)) && prevPressed[i] != pressed)///Right click and release(single click)
                    {
                        mouse_event(MOUSEEVENTF_RIGHTDOWN, cursor.X, cursor.Y, 0, 0x00A1);///Right Mouse Button Down//dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        Thread.Sleep(200);
                        mouse_event(MOUSEEVENTF_RIGHTUP, cursor.X, cursor.Y, 0, 0x00A1);///Right Mouse Button Up//dwExtraInfo 0x00A1 == 161 so can filter filter out the mouse created here
                        dragging = false;
                    }

                    if ((pressed == Dragdrop || (isRT && rt == Dragdrop) || (isLT && lt == Dragdrop)) && pressed != prevPressed[i] && !dragging)///Left click //catch/drag  
                    {
                        mouse_event(MOUSEEVENTF_LEFTDOWN, cursor.X, cursor.Y, 0, 0x00A1);//dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        dragging = true;
                        Thread.Sleep(200);
                    }
                    else if ((pressed == Dragdrop || (isRT && rt == Dragdrop) || (isLT && lt == Dragdrop)) && pressed != prevPressed[i] && dragging)///Left click //release 
                    {
                        mouse_event(MOUSEEVENTF_LEFTUP, cursor.X, cursor.Y, 0, 0x00A1);//dwExtraInfo 0x00A1 == 161 so can filter out the virtual mouse created here
                        dragging = false;
                        Thread.Sleep(200);
                    }
                    else if (pressed == OpenOsk || (isRT && rt == OpenOsk) || (isLT && lt == OpenOsk))///Open/close Onscreen Might have to focus nc window before opening need more testing
                    {
                        ToggleOsk();
                        Thread.Sleep(1100);
                    }

                    if (pressed > 0)
                    {
                        ReceiveInputs[i] = true;

                        if(prevPressed[i] != pressed)
                        {
                            OnUpdateState?.Invoke();
                        }         
                    }

                    if (pressed != prevPressed[i] || prevReceiveInputs[i] != ReceiveInputs[i])
                    {
                        OnUpdateState?.Invoke();
                    }

                    prevReceiveInputs[i] = ReceiveInputs[i];
                    prevPressed[i] = pressed;
                }

                Thread.Sleep(pollRate);
            }
        }

        private static void ToggleOsk()
        {
            Globals.MainOSD.Show(1600, $"On Screen Keyboard");
            OnScreenKeyboard.Show();
        }

        public static void UpdateUINavSettings()
        {
            _Enabled = App_GamePadNavigation.Enabled;
            Deadzone = App_GamePadNavigation.Deadzone;
            Dragdrop = App_GamePadNavigation.DragDrop;
            RightClick = App_GamePadNavigation.RightClick;
            LeftClick = App_GamePadNavigation.LeftClick;
            LockUIControl = App_GamePadNavigation.TogglekUINavigation[0] + App_GamePadNavigation.TogglekUINavigation[1];
            OpenOsk = App_GamePadNavigation.OpenOsk[0] + App_GamePadNavigation.OpenOsk[1];

            EnabledRuntime = _Enabled;
        }
    }
}
