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
                g.DrawRectangle(p, new Rectangle(0, 0, Width - 1, Height - 1));
            ReleaseDC(this.Handle, hdc);
        }

        if (m.Msg == 0xf && hintFont != null)
        {
            if (!this.Focused && string.IsNullOrEmpty(this.Text)
                && !string.IsNullOrEmpty(this.Hint))
            {
                using (var g = this.CreateGraphics())
                {
                    TextRenderer.DrawText(g, this.Hint, hintFont ,
                        this.DisplayRectangle, SystemColors.GrayText, this.BackColor,
                        TextFormatFlags.Top | TextFormatFlags.Left);
                }
            }
        }
    }

    protected override void OnSizeChanged(EventArgs e)
    {
        base.OnSizeChanged(e);
        RedrawWindow(Handle, IntPtr.Zero, IntPtr.Zero, RDW_FRAME | RDW_IUPDATENOW | RDW_INVALIDATE);

        float scale = (float)User32Util.GetDpiForWindow(this.Handle) / (float)100;
        hintFont = new Font(Font.FontFamily, Font.Size - (scale > 1.0F ? (2 * scale) : 0F), Font.Style, GraphicsUnit.Pixel);
    }

}