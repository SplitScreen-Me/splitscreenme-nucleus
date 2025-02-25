using System.Windows.Forms;

public class DoubleBufferPanel : Panel
{
    public DoubleBufferPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
    }
}