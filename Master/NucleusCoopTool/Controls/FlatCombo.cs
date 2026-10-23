using Nucleus.Gaming.UI;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

public class FlatCombo : ComboBox
{
    private IntPtr brush = IntPtr.Zero;
    private Rectangle dropDownRect = Rectangle.Empty;

    private Color borderColor = Color.Gray;

    [DefaultValue(typeof(Color), "Gray")]
    public Color BorderColor
    {
        get { return borderColor; }
        set
        {
            if (borderColor == value) return;
            borderColor = value;
            Invalidate();
        }
    }

    private Color buttonColor = Color.LightGray;

    [DefaultValue(typeof(Color), "LightGray")]
    public Color ButtonColor
    {
        get { return buttonColor; }
        set
        {
            if (buttonColor == value) return;
            buttonColor = value;
            Invalidate();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (dropDownRect.IsEmpty) return;

        Cursor = dropDownRect.Contains(e.Location)
            ? Theme_Settings.Hand_Cursor
            : Theme_Settings.Default_Cursor;
    }

    protected override void OnBackColorChanged(EventArgs e)
    {
        base.OnBackColorChanged(e);
        ReleaseBrush();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        ReleaseBrush();
        base.Dispose(disposing);
    }

    private void ReleaseBrush()
    {
        IntPtr oldBrush = brush;
        brush = IntPtr.Zero;

        if (oldBrush != IntPtr.Zero)
            DeleteObject(oldBrush);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_CTLCOLORSTATIC)
        {
            IntPtr hdc = m.WParam;
            if (hdc != IntPtr.Zero)
            {
                SetBkColor(hdc, ColorTranslator.ToWin32(BackColor));

                if (brush == IntPtr.Zero)
                    brush = CreateSolidBrush(ColorTranslator.ToWin32(BackColor));

                if (brush != IntPtr.Zero)
                {
                    m.Result = brush;
                    return;
                }
            }

            base.WndProc(ref m);
            return;
        }

        if (m.Msg == WM_PAINT && DropDownStyle != ComboBoxStyle.Simple)
        {
            PaintComboBox(ref m);
            return;
        }

        base.WndProc(ref m);
    }

    private void PaintComboBox(ref Message m)
    {
        PAINTSTRUCT ps = default(PAINTSTRUCT);
        bool ownsPaintDc = false;
        IntPtr dc = m.WParam;
        IntPtr originalWParam = m.WParam;

        if (dc == IntPtr.Zero)
        {
            dc = BeginPaint(Handle, ref ps);
            ownsPaintDc = true;
        }

        if (dc == IntPtr.Zero)
        {
            base.WndProc(ref m);
            return;
        }

        int savedDc = SaveDC(dc);
        if (savedDc == 0)
        {
            if (ownsPaintDc) EndPaint(Handle, ref ps);
            base.WndProc(ref m);
            return;
        }

        try
        {
            Rectangle clientRect = ClientRectangle;
            int buttonWidth = SystemInformation.HorizontalScrollBarArrowWidth;

            Rectangle outerBorder = new Rectangle(
                clientRect.X, clientRect.Y,
                Math.Max(0, clientRect.Width - 1),
                Math.Max(0, clientRect.Height - 1));

            Rectangle innerBorder = new Rectangle(
                outerBorder.X + 1, outerBorder.Y + 1,
                Math.Max(0, outerBorder.Width - buttonWidth - 2),
                Math.Max(0, outerBorder.Height - 2));

            Rectangle innerInnerBorder = new Rectangle(
                innerBorder.X + 1, innerBorder.Y + 1,
                Math.Max(0, innerBorder.Width - 2),
                Math.Max(0, innerBorder.Height - 2));

            dropDownRect = new Rectangle(
                innerBorder.Right + 1, innerBorder.Y,
                Math.Max(0, buttonWidth),
                Math.Max(0, innerBorder.Height + 1));

            if (RightToLeft == RightToLeft.Yes)
            {
                innerBorder.X = clientRect.Width - innerBorder.Right;
                innerInnerBorder.X = clientRect.Width - innerInnerBorder.Right;
                dropDownRect.X = clientRect.Width - dropDownRect.Right;
                dropDownRect.Width = Math.Max(0, dropDownRect.Width + 1);
            }

            Color innerColor = BackColor;
            Color outerColor = BorderColor;
            Color currentButtonColor = ButtonColor;

            Point middle = new Point(
                dropDownRect.Left + dropDownRect.Width / 2,
                dropDownRect.Top + dropDownRect.Height / 2);

            Point[] arrow =
            {
                new Point(middle.X - 3, middle.Y - 2),
                new Point(middle.X + 4, middle.Y - 2),
                new Point(middle.X, middle.Y + 2)
            };

            IntPtr region = IntPtr.Zero;
            try
            {
                region = CreateRectRgn(
                    innerInnerBorder.Left, innerInnerBorder.Top,
                    innerInnerBorder.Right, innerInnerBorder.Bottom);

                if (region != IntPtr.Zero)
                    SelectClipRgn(dc, region);

                m.WParam = dc;
                DefWndProc(ref m);
            }
            finally
            {
                if (region != IntPtr.Zero) DeleteObject(region);
            }

            region = IntPtr.Zero;
            try
            {
                region = CreateRectRgn(
                    clientRect.Left, clientRect.Top,
                    clientRect.Right, clientRect.Bottom);

                if (region != IntPtr.Zero)
                    SelectClipRgn(dc, region);
                else
                    SelectClipRgn(dc, IntPtr.Zero);

                using (Graphics g = Graphics.FromHdc(dc))
                using (SolidBrush buttonBrush = new SolidBrush(currentButtonColor))
                using (SolidBrush arrowBrush = new SolidBrush(outerColor))
                using (Pen innerPen = new Pen(innerColor))
                using (Pen outerPen = new Pen(outerColor))
                {
                    g.FillRectangle(buttonBrush, dropDownRect);
                    g.FillPolygon(arrowBrush, arrow);
                    g.DrawRectangle(innerPen, innerBorder);
                    g.DrawRectangle(innerPen, innerInnerBorder);
                    g.DrawRectangle(outerPen, outerBorder);
                }
            }
            finally
            {
                if (region != IntPtr.Zero) DeleteObject(region);
            }
        }
        finally
        {
            RestoreDC(dc, savedDc);
            m.WParam = originalWParam;

            if (ownsPaintDc)
                EndPaint(Handle, ref ps);
        }
    }

    private const int WM_PAINT = 0x000F;
    private const int WM_CTLCOLORSTATIC = 0x0138;

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int L, T, R, B;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PAINTSTRUCT
    {
        public IntPtr hdc;
        public bool fErase;
        public int rcPaint_left;
        public int rcPaint_top;
        public int rcPaint_right;
        public int rcPaint_bottom;
        public bool fRestore;
        public bool fIncUpdate;
        public int reserved1;
        public int reserved2;
        public int reserved3;
        public int reserved4;
        public int reserved5;
        public int reserved6;
        public int reserved7;
        public int reserved8;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr BeginPaint(IntPtr hWnd, [In, Out] ref PAINTSTRUCT lpPaint);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool EndPaint(IntPtr hWnd, ref PAINTSTRUCT lpPaint);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int SaveDC(IntPtr hdc);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool RestoreDC(IntPtr hdc, int nSavedDC);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int SelectClipRgn(IntPtr hDC, IntPtr hRgn);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateRectRgn(int x1, int y1, int x2, int y2);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern IntPtr CreateSolidBrush(int color);

    [DllImport("gdi32.dll", SetLastError = true)]
    private static extern int SetBkColor(IntPtr hdc, int color);
}
