using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Media.Media3D;
using System;
using Nucleus.Gaming.Windows;

public class FlatTextBox : TextBox
{
    const int WM_NCPAINT = 0x85;
    const uint RDW_INVALIDATE = 0x1;
    const uint RDW_IUPDATENOW = 0x100;
    const uint RDW_FRAME = 0x400;
    [DllImport("user32.dll")]
    static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]

    static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);
    [DllImport("user32.dll")]
    static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprc, IntPtr hrgn, uint flags);

    string hint;
    public string Hint
    {
        get { return hint; }
        set { hint = value; this.Invalidate(); }
    }

    private Font hintFont;

    Color borderColor = Color.Blue;
    public Color BorderColor
    {
        get { return borderColor; }
        set
        {
            borderColor = value;
            RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
        }
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if (m.Msg == WM_NCPAINT && BorderColor != Color.Transparent &&
            BorderStyle == System.Windows.Forms.BorderStyle.Fixed3D)
        {
            var hdc = GetWindowDC(this.Handle);
            using (var g = Graphics.FromHdcInternal(hdc))
            using (var p = new Pen(BorderColor))
                g.DrawRectangle(p, new Rectangle(0, 0, Width - 3, Height - 3));
            ReleaseDC(this.Handle, hdc);
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);
    }

}