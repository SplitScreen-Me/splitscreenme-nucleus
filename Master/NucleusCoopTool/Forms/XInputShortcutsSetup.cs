using Nucleus.Coop.Controls;
using Nucleus.Gaming;
using Nucleus.Gaming.App.Settings;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Controls;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.UI;
using Nucleus.Gaming.Windows.Interop;
using SharpDX.XInput;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows.Forms;

namespace Nucleus.Coop.Forms
{
    public partial class XInputShortcutsSetup : Form, IDynamicSized
    {
        private Bitmap controllerTop;
        private Bitmap controllerFront;
        private PictureBox activeControl;
        private Timer refreshTimer = new Timer();
        private float scale;
        
        private SolidBrush brush;
        public static new bool Enabled;
        private string gamepadType;
        private RectangleF top;
        private RectangleF front;

        public XInputShortcutsSetup()
        {
            InitializeComponent();
            bool roundedcorners = bool.Parse(Globals.ThemeConfigFile.IniReadValue("Misc", "UseRoundedCorners"));
            gamepadType = App_GamePadNavigation.Type;

            Cursor = Theme_Settings.Default_Cursor;

            shortContainer.Cursor = Theme_Settings.Default_Cursor;
            enabled_chk.Cursor = Theme_Settings.Hand_Cursor;

            refreshTimer.Tick += RefreshTimerTick;
            refreshTimer.Interval = 200;
            refreshTimer.Start();

            BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "xinput_background.jpg");
            BackgroundImageLayout = ImageLayout.Stretch;

            controllerTop = Nucleus.Coop.Properties.Resources.xboxControllerTop;
            controllerFront = Nucleus.Coop.Properties.Resources.xboxControllerFront;
            Close.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");

            CustomToolTips.SetToolTip(enabled_chk, "Automatically locked when all instances are set and ready.", "enabled_chk", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            brush = new SolidBrush(Color.FromArgb(120, 0, 255, 60));
            switch15.KeyPress += Num_KeyPress;

            if (roundedcorners)
            {
                FormGraphicsUtil.CreateRoundedControlRegion(this, 0, 0, Width, Height, 20, 20);
            }

            switch1.Tag = App_GamePadShortcuts.SetFocus[0];
            slave1.Tag = App_GamePadShortcuts.SetFocus[1];

            switch2.Tag = App_GamePadShortcuts.Close[0];
            slave2.Tag = App_GamePadShortcuts.Close[1];

            switch3.Tag = App_GamePadShortcuts.StopSession[0];
            slave3.Tag = App_GamePadShortcuts.StopSession[1];

            switch4.Tag = App_GamePadShortcuts.TopMost[0];
            slave4.Tag = App_GamePadShortcuts.TopMost[1];

            switch5.Tag = App_GamePadShortcuts.ResetWindows[0];
            slave5.Tag = App_GamePadShortcuts.ResetWindows[1];

            switch6.Tag = App_GamePadShortcuts.CutscenesMode[0];
            slave6.Tag = App_GamePadShortcuts.CutscenesMode[1];

            switch7.Tag = App_GamePadShortcuts.SwitchLayout[0];
            slave7.Tag = App_GamePadShortcuts.SwitchLayout[1];

            switch8.Tag = App_GamePadShortcuts.LockInputs[0];
            slave8.Tag = App_GamePadShortcuts.LockInputs[1];

            switch9.Tag = App_GamePadShortcuts.ReleaseCursor[0];
            slave9.Tag = App_GamePadShortcuts.ReleaseCursor[1];

            switch10.Tag = App_GamePadNavigation.TogglekUINavigation[0];
            slave10.Tag = App_GamePadNavigation.TogglekUINavigation[1];

            switch11.Tag = App_GamePadNavigation.OpenOsk[0];
            slave11.Tag = App_GamePadNavigation.OpenOsk[1];


            enabled_chk.Checked = App_GamePadNavigation.Enabled;
            CustomToolTips.SetToolTip(enabled_chk, "Requires admin rights for full controller support.", "enabled_chk", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });

            switch12.Tag = App_GamePadNavigation.DragDrop;
            switch13.Tag = App_GamePadNavigation.RightClick;
            switch14.Tag = App_GamePadNavigation.LeftClick;
            switch15.Text = App_GamePadNavigation.Deadzone.ToString();


            enabled_chk.SelectionColor = Color.FromArgb(255, 31, 34, 35);
            enabled_chk.CheckColor = Color.FromArgb(Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B);
            enabled_chk.BorderColor = Color.White;
            enabled_chk.BackColor = Color.Transparent;

            foreach (CustomRadio rb in groupBoxType.Controls)
            {
                if (rb.Tag.ToString() == gamepadType)
                {
                    rb.Checked = true;             
                }

                rb.SelectionColor = Color.FromArgb(255, 31, 34, 35);
                rb.CheckColor = Color.FromArgb(Theme_Settings.SelectedBackColor.R, Theme_Settings.SelectedBackColor.G, Theme_Settings.SelectedBackColor.B);
                rb.BorderColor = Color.White;
                rb.BackColor = Color.Transparent;
            }

            foreach (Control c in shortContainer.Controls)
            {
                if (c is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)c;
                    pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
                    pictureBox.MouseClick += SetActiveControl;
                    pictureBox.BackgroundImage = GamepadButtons.Image((int)pictureBox.Tag, gamepadType);
                    pictureBox.Cursor = Theme_Settings.Hand_Cursor;
                }
            }

            foreach (Control c in UINavContainer.Controls)
            {
                if (c is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)c;
                    pictureBox.BackgroundImageLayout = ImageLayout.Stretch;
                    pictureBox.MouseClick += SetActiveControl;
                    pictureBox.BackgroundImage = GamepadButtons.Image((int)pictureBox.Tag, gamepadType);
                    pictureBox.Cursor = Theme_Settings.Hand_Cursor;
                }

                if (c.Name == "switch15")
                {
                    c.KeyPress += Num_KeyPress;
                    CustomToolTips.SetToolTip(c, "If the cursor move without touching the\r\nleft stick set a higher value(steps of +1000).", "switch15", new int[] { 190, 0, 0, 0 }, new int[] { 255, 255, 255, 255 });
                }
            }

            reminderPict.BackgroundImage = GamepadButtons.Image(1024, gamepadType);//Guide

            Rectangle area = Screen.PrimaryScreen.Bounds;
            if (App_Misc.GamepadSettingsLocation != "")
            {
                string[] windowLocation = App_Misc.GamepadSettingsLocation.Split('X');

                if (ScreensUtil.AllScreens().All(s => !s.MonitorBounds.Contains(int.Parse(windowLocation[0]), int.Parse(windowLocation[1]))))
                {
                    CenterToScreen();
                }
                else
                {
                    Location = new Point(area.X + int.Parse(windowLocation[0]), area.Y + int.Parse(windowLocation[1]));
                }
            }
            else
            {
                CenterToScreen();
            }

            DPIManager.Register(this);
            DPIManager.Update(this);
        }

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }

            this.scale = scale;

            float newFontSize = 7.25F * scale;

            foreach (Control c in shortContainer.Controls)
            {
                if (c is Label)
                {
                    if (c.Text != ("+"))
                    {
                        c.Location = new Point(switch1.Left - c.Width, c.Location.Y);
                    }
                }
            }

            foreach (Control c in UINavContainer.Controls)
            {
                if (scale > 1.0F)
                {
                    if (c is TextBox)
                    {
                        c.Font = new Font(c.Font.FontFamily, newFontSize, FontStyle.Regular, GraphicsUnit.Pixel, 0);
                    }
                }

                if (c is Label)
                {
                    if (c.Text != ("+"))
                    {
                        c.Location = new Point(switch10.Left - c.Width, c.Location.Y);
                    }
                }
            }

            switch15.Location = new Point(switch15.Location.X, label_15.Top + ((label_15.DisplayRectangle.Height / 2) - (switch15.DisplayRectangle.Height / 2)));
        }

        private void RefreshTimerTick(Object Object, EventArgs EventArgs)
        {
            if (Visible)
                gamepadTopFront.Invalidate();
        }

        private void GamepadTopFront_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            float factor = 1.5F;

            //Draw top & front picture
            float topWidth = (controllerTop.Width / factor) * scale;
            float topHeight = (controllerTop.Height / factor) * scale;
            float frontWidth = (controllerFront.Width / factor) * scale;
            float frontHeight = (controllerFront.Height / factor) * scale;


            Vector2 p1 = new Vector2(gamepadTopFront.Location.X, gamepadTopFront.Location.Y);
            Vector2 p2 = new Vector2(this.Width, gamepadTopFront.Location.X);
            Vector2 size1 = Vector2.Subtract(p2, p1);
            float locX = size1.X / 2 - topWidth / 2;

            top = new RectangleF(locX, 10, topWidth, topHeight);
            front = new RectangleF(top.X, top.Bottom + 20, frontWidth, frontHeight);

            g.DrawImage(controllerTop, top);
            g.DrawImage(controllerFront, front);

            float radius = 25;
            float offset = ((radius / 2) / factor) * scale;
            float rec = (30 / factor) * scale;
            RectangleF Guide = new RectangleF((front.X + (125 / factor) * scale) - offset, (front.Y + (46 / factor) * scale) - offset, rec, rec);
            RectangleF RB = new RectangleF((top.X + (194 / factor) * scale) - offset, (top.Y + (64 / factor) * scale) - offset, rec, rec);
            RectangleF LB = new RectangleF((top.X + (56 / factor) * scale) - offset, (top.Y + (64 / factor) * scale) - offset, rec, rec);
            RectangleF RS = new RectangleF((front.X + (158 / factor) * scale) - offset, (front.Y + (85 / factor) * scale) - offset, rec, rec);
            RectangleF LS = new RectangleF((front.X + (56 / factor) * scale) - offset, (front.Y + (44 / factor) * scale) - offset, rec, rec);
            RectangleF A = new RectangleF((front.X + (193 / factor) * scale) - offset, (front.Y + (63 / factor) * scale) - offset, rec, rec);
            RectangleF B = new RectangleF((front.X + (211 / factor) * scale) - offset, (front.Y + (44 / factor) * scale) - offset, rec, rec);
            RectangleF X = new RectangleF((front.X + (175 / factor) * scale) - offset, (front.Y + (44 / factor) * scale) - offset, rec, rec);
            RectangleF Y = new RectangleF((front.X + (193 / factor) * scale) - offset, (front.Y + (26 / factor) * scale) - offset, rec, rec);
            RectangleF Back = new RectangleF((front.X + (100 / factor) * scale) - offset, (front.Y + (45 / factor) * scale) - offset, rec, rec);
            RectangleF Start = new RectangleF((front.X + (150 / factor) * scale) - offset, (front.Y + (45 / factor) * scale) - offset, rec, rec);

            RectangleF D_Up = new RectangleF((front.X + (91.5F / factor) * scale) - offset, (front.Y + (70 / factor) * scale) - offset, rec, rec);
            RectangleF D_Right = new RectangleF((front.X + (104 / factor) * scale) - offset, (front.Y + (85 / factor) * scale) - offset, rec, rec);
            RectangleF D_Down = new RectangleF((front.X + (91.5F / factor) * scale) - offset, (front.Y + (101 / factor) * scale) - offset, rec, rec);
            RectangleF D_Left = new RectangleF((front.X + (76 / factor) * scale) - offset, (front.Y + (85 / factor) * scale) - offset, rec, rec);
            RectangleF LT = new RectangleF((top.X + (59 / factor) * scale) - offset, (top.Y + (32 / factor) * scale) - offset, rec, rec);
            RectangleF RT = new RectangleF((top.X + (187 / factor) * scale) - offset, (top.Y + (32 / factor) * scale) - offset, rec, rec);

            RectangleF highlight = RectangleF.Empty;

            for (int i = 0; i < GamepadState.Controllers.Length; i++)
            {
                {////draw a circle at the pressed button location
                    int pressed = GamepadState.GetPressedButtons(i);
                    int rtValue = GamepadState.GetRightTriggerValue(i);
                    int rlValue = GamepadState.GetLeftTriggerValue(i);
                    bool isRT = false;
                    bool isRL = false;
                    bool isTRigger = false;

                    if (rtValue > 0)
                    {
                        isRT = true;
                        isTRigger = true;
                    }

                    if (rlValue > 0)
                    {
                        isRL = true;
                        isTRigger = true;
                    }

                    int.TryParse("None", out int noButtonPressed);
                    if (pressed == noButtonPressed && !isRL && !isRT)
                    {
                        continue;
                    }

                    switch (pressed)
                    {
                        case 1024://Guide button
                            {
                                highlight = Guide;
                                break;
                            }
                        case (int)GamepadButtonFlags.RightShoulder:
                            {
                                highlight = RB;
                                break;
                            }
                        case (int)GamepadButtonFlags.LeftShoulder:
                            {
                                highlight = LB;
                                break;
                            }
                        case (int)GamepadButtonFlags.RightThumb:
                            {
                                highlight = RS;
                                break;
                            }
                        case (int)GamepadButtonFlags.LeftThumb:
                            {
                                highlight = LS;
                                break;
                            }
                        case (int)GamepadButtonFlags.A:
                            {
                                highlight = A;
                                break;
                            }
                        case (int)GamepadButtonFlags.B:
                            {
                                highlight = B;
                                break;
                            }
                        case (int)GamepadButtonFlags.X:
                            {
                                highlight = X;
                                break;
                            }
                        case (int)GamepadButtonFlags.Y:
                            {
                                highlight = Y;
                                break;
                            }
                        case (int)GamepadButtonFlags.Back:
                            {
                                highlight = Back;
                                break;
                            }
                        case (int)GamepadButtonFlags.Start:
                            {
                                highlight = Start;
                                break;
                            }
                        case (int)GamepadButtonFlags.DPadUp:
                            {
                                highlight = D_Up;
                                break;
                            }
                        case (int)GamepadButtonFlags.DPadRight:
                            {
                                highlight = D_Right;
                                break;
                            }
                        case (int)GamepadButtonFlags.DPadDown:
                            {
                                highlight = D_Down;
                                break;
                            }
                        case (int)GamepadButtonFlags.DPadLeft:
                            {
                                highlight = D_Left;
                                break;
                            }
                    }

                    if (isRT)
                    {
                        highlight = RT;
                    }
                    else if (isRL)
                    {
                        highlight = LT;
                    }

                    g.ResetClip();

                    g.Clip = new Region(new RectangleF(highlight.X, highlight.Y, highlight.Width, highlight.Height));

                    g.FillEllipse(brush, highlight);

                    if (activeControl != null)
                    {
                        if (!isTRigger)
                        {
                            if ((int)pressed == noButtonPressed)
                            {
                                return;
                            }

                            activeControl.Tag = pressed;

                        }
                        else if (isRT)
                        {
                            activeControl.Tag = 9999;

                        }
                        else if (isRL)
                        {
                            activeControl.Tag = 10000;
                        }

                        activeControl.BackgroundImage = GamepadButtons.Image((int)activeControl.Tag, gamepadType);
                    }
                }
            }
        }

        private void Num_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && (e.KeyChar != '.'))
            {
                e.Handled = true;
            }
        }

        private void SetActiveControl(object sender, EventArgs e)
        {
            if (!(sender is PictureBox))
            {
                return;
            }

            PictureBox pb = sender as PictureBox;

            if (activeControl != null)
            {
                activeControl.BorderStyle = BorderStyle.None;
            }

            pb.BorderStyle = BorderStyle.FixedSingle;
            activeControl = pb;
        }

        private void Enabled_chk_CheckedChanged(object sender, EventArgs e)
        {
            App_GamePadNavigation.Enabled = enabled_chk.Checked;
            GamepadNavigation.UpdateUINavSettings();
        }

        private void radioButtonXbox_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked)
            {
                App_GamePadNavigation.Type = radio.Tag.ToString();
                gamepadType = radio.Tag.ToString();
                UpdateGamepadType();
            }
        }

        private void RadioButtonPs_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radio = (RadioButton)sender;
            if (radio.Checked)
            {
                App_GamePadNavigation.Type = radio.Tag.ToString();
                gamepadType = radio.Tag.ToString();
                UpdateGamepadType();
            }
        }

        private void UpdateGamepadType()
        {
            foreach (Control c in shortContainer.Controls)
            {
                if (c is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)c;
                    pictureBox.BackgroundImage = GamepadButtons.Image((int)pictureBox.Tag, gamepadType);
                }
            }

            foreach (Control c in UINavContainer.Controls)
            {
                if (c is PictureBox)
                {
                    PictureBox pictureBox = (PictureBox)c;
                    pictureBox.BackgroundImage = GamepadButtons.Image((int)pictureBox.Tag, gamepadType);
                }
            }

            reminderPict.BackgroundImage = GamepadButtons.Image(1024, gamepadType);//Guide
        }

        private void Close_Click(object sender, EventArgs e)
        {
            #region take a picture of the controller shortcuts

            Color defColor = shortContainer.BackColor;

            try
            {
                shortContainer.BackColor = Color.Black;
                Graphics g = shortContainer.CreateGraphics();
                Bitmap bmp = new Bitmap(shortContainer.Width, shortContainer.Height);
                shortContainer.DrawToBitmap(bmp, new Rectangle(0, 0, shortContainer.Width, shortContainer.Height));

                string savePath = Path.Combine(Application.StartupPath, $@"gui\shortcuts");

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                bmp.Save(Path.Combine(savePath, "XShortcutsReminder.jpeg"), ImageFormat.Jpeg);
                bmp.Dispose();
                g.Dispose();

                shortContainer.BackColor = defColor;
            }
            catch
            {
                shortContainer.BackColor = defColor;
            }

            #endregion

            App_GamePadShortcuts.SetFocus = new int[]{(int) switch1.Tag, (int)slave1.Tag};
            App_GamePadShortcuts.Close = new int[] { (int)switch2.Tag, (int)slave2.Tag };
            App_GamePadShortcuts.StopSession = new int[] { (int)switch3.Tag, (int)slave3.Tag };
            App_GamePadShortcuts.TopMost = new int[] { (int)switch4.Tag, (int)slave4.Tag };
            App_GamePadShortcuts.ResetWindows = new int[] { (int)switch5.Tag, (int)slave5.Tag };
            App_GamePadShortcuts.CutscenesMode = new int[] { (int)switch6.Tag, (int)slave6.Tag };
            App_GamePadShortcuts.SwitchLayout = new int[] { (int)switch7.Tag, (int)slave7.Tag };
            App_GamePadShortcuts.LockInputs = new int[] { (int)switch8.Tag, (int)slave8.Tag };
            App_GamePadShortcuts.ReleaseCursor = new int[] { (int)switch9.Tag, (int)slave9.Tag };

            App_GamePadNavigation.TogglekUINavigation = new int[] { (int)switch10.Tag, (int)slave10.Tag };
            App_GamePadNavigation.OpenOsk = new int[] { (int)switch11.Tag, (int)slave11.Tag };
            App_GamePadNavigation.DragDrop = int.Parse(switch12.Tag.ToString());
            App_GamePadNavigation.RightClick = int.Parse(switch13.Tag.ToString());
            App_GamePadNavigation.LeftClick = int.Parse(switch14.Tag.ToString());

            if (switch15.Text == "")
            {
                switch15.Text = "10000";
            }

            App_GamePadNavigation.Deadzone = int.Parse(switch15.Text);
            App_Misc.GamepadSettingsLocation = Location.X + "X" + Location.Y;

            GamepadShortcuts.UpdateShortcutsValue();
            GamepadNavigation.UpdateUINavSettings();

            Visible = false;
        }

        private void Close_MouseEnter(object sender, EventArgs e)
        {
            Close.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close_mousehover.png");
        }

        private void Close_MouseLeave(object sender, EventArgs e)
        {
            Close.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_close.png");
        }

        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        private void XInputShortcutsSetup_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }

        private void GamepadTopFront_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                User32Interop.ReleaseCapture();
                IntPtr nucHwnd = User32Interop.FindWindow(null, Text);
                User32Interop.SendMessage(nucHwnd, WM_NCLBUTTONDOWN, (IntPtr)HT_CAPTION, (IntPtr)0);
            }
        }
    }
}
