using Nucleus.Gaming.Coop.BasicTypes;
using Nucleus.Gaming.Coop.InputManagement;
using Nucleus.Gaming.Coop.InputManagement.Logging;
using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO.MemoryMappedFiles;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Nucleus.Gaming.Coop
{
    public class Window
    {
        //Window handle
        public readonly IntPtr hWnd;

        //Some games use an invisible window called DIEmWin. WM_INPUT needs to be sent to this hWnd instead of the visible game hWnd or it is ignored.
        public IntPtr DIEmWin_hWnd = IntPtr.Zero;

        public readonly int pid;//Process ID

        public IntPtr MouseAttached { get; set; } = new IntPtr(0);
        public IntVector2 MousePosition { get; } = new IntVector2();//This is a reference type. Relative to game window
        public (bool l, bool m, bool r, bool x1, bool x2) MouseState { get; set; } = (false, false, false, false, false);
        public byte WASD_State { get; set; } = 0;

        public IntPtr KeyboardAttached { get; set; } = new IntPtr(0);
        public readonly BitArray keysDown = new BitArray(0xFF);

        //public int ControllerIndex { get; set; } = 0;//0 = none, 1234 = 1234

        private RECT Bounds { get; set; }
        public int Width => Bounds.Right - Bounds.Left;
        public int Height => Bounds.Bottom - Bounds.Top;

        public HookPipe HookPipe { get; private set; }

        //Draws a fake cursor based on the game's calls to SetCursorPos
        private System.Threading.Timer cursorInternalInputUpdateTimer;
        private uint cursorInternalInputUpdateCounter = 0;
        private const uint CURSOR_INTERNAL_INPUT_UPDATE_CYCLES_BEFORE_UPDATING_BOUNDS = 500;

        #region Mouse Cursor
        /* How drawing the fake mouse cursor works:
		 * A transparent window (PointerForm) is created over the game window.
		 * When the mouse is moved, UpdateCursorPosition will tell the window to paint over the old cursor (wiping it) and draw the new one.
		 * If the mouse moves out of bounds of PointerForm, the centre of the window is moved to the mouse position.
		 * (Depends if Hook mouse visibility is enabled) In HooksCPP, SetCursor(NULL or not NULL) and ShowCursor(TRUE/FALSE) is monitored to show/hide cursor.
		 * When this is detected, it sends a message via a named pipe to set CursorVisibility, which which will wipe/draw the cursor.
		 */

        private class PointerForm : Form
        {
            private IntPtr hicon;

            //Screen coords (relative to primary monitor)
            public int screenX;
            public int screenY;

            public bool visible = true;

            private int oldRelativeScreenX;
            private int oldRelativeScreenY;
            private IntPtr hWnd;
            private Graphics g = null;
            private IntPtr h;
            private bool hasDrawn = false;//We need to paint the entire window once at the start.

            private const int WINDOW_WIDTH = 2000;//Minimum width
            private const int WINDOW_HEIGHT = 2000;

            private IntPtr hbr;
            private const int CURSOR_WIDTH_HEIGHT = 35;

            public int hasBroughtToTopCounter;

            private bool disabledAltTab;

            private readonly Timer lastDrawnTimer = new Timer();
            public bool readyToDraw;
            private const int MILLISECONDS_BETWEEN_DRAW = 5;

            //Hides in alt+tab menu
            protected override CreateParams CreateParams
            {
                get
                {
                    CreateParams cp = base.CreateParams;
                    cp.ExStyle |= 0x80;
                    return cp;
                }
            }
            public PointerForm(IntPtr hWnd, int gameWindowWidth, int gameWindowHeight, int gameWindowX, int gameWindowY) : base()
            {
                this.hWnd = hWnd;

                Width = Math.Max(WINDOW_WIDTH, gameWindowWidth + 100);
                Height = Math.Max(WINDOW_HEIGHT, gameWindowHeight + 100);
                Logger.WriteLine($"Cursor window width,height = {Width},{Height}");
                FormBorderStyle = FormBorderStyle.None;
                Text = "";
                StartPosition = FormStartPosition.Manual;
                Location = new System.Drawing.Point(gameWindowX + gameWindowWidth / 2, gameWindowY + gameWindowHeight / 2);
                TopMost = true;
                BackColor = Color.FromArgb(255, 0, 0, 1);
                TransparencyKey = BackColor;
                ShowInTaskbar = false;
                //WinApi.SetWindowLongPtr(pointerHandle, (-8), hWnd);//Sets owner. Always draws above owner.

                hicon = Cursors.Arrow.Handle;

                hbr = WinApi.CreateSolidBrush(0x00010000);//0x00bbggrr

                //lastDrawnStopwatch.Start();
                lastDrawnTimer.Start();
                lastDrawnTimer.Interval = MILLISECONDS_BETWEEN_DRAW;
                lastDrawnTimer.Tick += (o, e) => { readyToDraw = true; };
            }

            protected override void OnPaintBackground(PaintEventArgs e)
            {
                if (!hasDrawn)
                {
                    hasDrawn = true;
                    base.OnPaintBackground(e);
                    return;
                }

                if (g == null)
                {
                    g = CreateGraphics();
                    h = g.GetHdc();
                }

                //Wipe where the cursor was last time
                RECT r = new RECT
                {
                    Left = oldRelativeScreenX,
                    Top = oldRelativeScreenY
                };
                r.Bottom = r.Top + CURSOR_WIDTH_HEIGHT;
                r.Right = r.Left + CURSOR_WIDTH_HEIGHT;
                WinApi.FillRect(h, ref r, hbr);

                if (visible)
                {
                    WinApi.DrawIcon(h, screenX - Location.X, screenY - Location.Y, hicon); //Coordinates are relative to Location of PointerForm
                }

                oldRelativeScreenX = screenX - Location.X;
                oldRelativeScreenY = screenY - Location.Y;

                //Greatly reduces CPU usage, doesn't lock any input. Insignificant delay. Mouse cursor is only used in menus, not first person.
                //Problem: all pointers are drawn on the same thread. After resuming, a high polling rate pointer is always the most likely to draw first
                // hence throttles other cursors if moving at the same time.
                //Solution: measure time in UpdateCursorPosition and ignore if drawn too recently.
                //System.Threading.Thread.Sleep(5);
            }

            public void InvalidateMouse()
            {
                if (!disabledAltTab)
                {
                    long x = (long)WinApi.GetWindowLongPtr(Handle, (-20));
                    x |= 0x00000080L;
                    WinApi.SetWindowLongPtr(Handle, (-20), (IntPtr)x);
                    disabledAltTab = true;
                }

                //Causes this region to be re-drawn
                Invalidate();
            }

            //Wipes the entire window
            public void RepaintAll()
            {
                try
                {
                    RECT r = new RECT
                    {
                        Left = 0,
                        Top = 0,
                        Bottom = Height,
                        Right = Width
                    };

                    WinApi.FillRect(h, ref r, hbr);
                }
                catch (Exception e)
                {
                    Debug.WriteLine($"Error in RepaintAll: {e}");
                }
            }
        }

        private PointerForm pointerForm = null;
        public bool CursorVisibility
        {
            get => pointerForm != null && pointerForm.visible;
            set
            {
                if (pointerForm != null)
                {
                    pointerForm.visible = value;
                    if (value)
                    {
                        pointerForm.Show();
                    }
                    else
                    {
                        pointerForm.Hide();
                    }
                }
            }
        }

        public void CreateCursor(bool internalInputUpdate)
        {
            pointerForm = new PointerForm(hWnd, Width, Height, Bounds.Left, Bounds.Top);
            pointerForm.Show();

            if (internalInputUpdate)
            {
                cursorInternalInputUpdateTimer = new System.Threading.Timer(CursorInternalInputUpdate, null, 0, 7);
            }
        }

        private void CursorInternalInputUpdate(object state)
        {
            if (HookPipe != null && HookPipe.sharedMemView != null)
            {
                MemoryMappedViewAccessor view = HookPipe.sharedMemView;

                MousePosition.x = Math.Min(Width, Math.Max(view.ReadInt32(0), 0));
                MousePosition.y = Math.Min(Height, Math.Max(view.ReadInt32(4), 0));

                cursorInternalInputUpdateCounter = ++cursorInternalInputUpdateCounter % CURSOR_INTERNAL_INPUT_UPDATE_CYCLES_BEFORE_UPDATING_BOUNDS;
                if (cursorInternalInputUpdateCounter == 0)
                {
                    //Need to do this somehow. For mice it's done whenever the user clicks. (But this is for controllers).
                    UpdateBounds();
                }

                UpdateCursorPosition();
            }
        }

        private bool hasRepaintedSinceLastInvisible = false;

        public void UpdateCursorPosition()
        {
            if (pointerForm != null && !pointerForm.IsDisposed)//If Draw Mouse is selected
            {
                if (pointerForm.visible)
                {
                    //Prevent the cursor being drawn too often (wastes CPU with high polling rate mice)
                    if (!pointerForm.readyToDraw)
                    {
                        return;
                    }

                    pointerForm.readyToDraw = false;

                    hasRepaintedSinceLastInvisible = false;
                    Point p = new System.Drawing.Point(MousePosition.x + Bounds.Left, MousePosition.y + Bounds.Top);
                    //WinApi.ClientToScreen(hWnd, ref p); //p is screen coords

                    pointerForm.screenX = p.X;
                    pointerForm.screenY = p.Y;

                    const int padding = 35;
                    if (p.X <= pointerForm.Location.X + padding || p.Y <= pointerForm.Location.Y + padding ||
                        p.X >= pointerForm.Location.X + pointerForm.Width - padding ||
                        p.Y >= pointerForm.Location.Y + pointerForm.Height - padding)
                    {
                        //pointerForm.RepaintAll(); (unecessary waste of CPU)
                        pointerForm.Location = new System.Drawing.Point(p.X - pointerForm.Width / 2,
                            p.Y - pointerForm.Height / 2);
                    }

                    //NucleusCoop brings the game window the the top, this brings the mouse top of top.
                    //Must only be run once because if two pointers try and both do this, CPU usage is massive.
                    if (pointerForm.hasBroughtToTopCounter++ == 1000)
                    {
                        pointerForm.hasBroughtToTopCounter = 0;

                        WinApi.SetWindowPos(pointerForm.Handle, (IntPtr)(-1), 0, 0, 0, 0,
                            0x0002 | 0x0008 | 0x0001);
                    }


                    pointerForm.InvalidateMouse();
                }
                else if (!hasRepaintedSinceLastInvisible)
                {
                    //If the cursor should be invisible, make sure the last cursor has been wiped.
                    pointerForm.RepaintAll();
                    hasRepaintedSinceLastInvisible = true;
                }
            }
        }

        private void KillCursor()
        {
            pointerForm?.Hide();
            pointerForm?.Dispose();
            pointerForm = null;
        }
        #endregion

        public Window(IntPtr hWnd)
        {
            this.hWnd = hWnd;
            WinApi.GetWindowThreadProcessId(hWnd, out pid);
            UpdateBounds();

            //Logger.WriteLine($"Bounds for hWnd={hWnd}: Left={Bounds.Left}, Right={Bounds.Right}, Top={Bounds.Top}, Bottom={Bounds.Bottom}, WIDTH={Width}, HEIGHT={Height}");
        }

        public void UpdateBounds()
        {
            //TODO: on another thread?

            //Bring to top
            if (pointerForm != null)
            {
                WinApi.SetWindowPos(pointerForm.Handle, (IntPtr)(-1), 0, 0, 0, 0,
                    0x0002 | 0x0008 | 0x0001);
                WinApi.BringWindowToTop(pointerForm.Handle);
            }

            //Hide the cursor if window is gone
            bool ret = WinApi.GetWindowRect(hWnd, out RECT bounds);
            if (CursorVisibility && !ret)
            {
                CursorVisibility = false;
            }

            Bounds = bounds;
        }

        public void CreateHookPipe(GenericGameInfo gameInfo)
        {
            HookPipe = new HookPipe(hWnd, this, gameInfo.HookMouseVisibility, OnPipeClosed, gameInfo);
        }

        private void OnPipeClosed()
        {
            HookPipe = null;
        }

        public void End()
        {
            KillCursor();
        }
    }
}
