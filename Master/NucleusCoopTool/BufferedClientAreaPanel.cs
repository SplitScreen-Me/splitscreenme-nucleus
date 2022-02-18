
using System;
using System.Windows.Forms;

public class BufferedClientAreaPanel : Panel
{
    public BufferedClientAreaPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
    }
}