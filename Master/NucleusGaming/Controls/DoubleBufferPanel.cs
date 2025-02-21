using System.Windows.Forms;

public class DoubleBufferPanel : Panel
{
    //protected override CreateParams CreateParams
    //{
    //    get
    //    {
    //        CreateParams handleparams = base.CreateParams;
    //        handleparams.ExStyle = 0x02000000;
    //        return handleparams;
    //    }
    //}
    public DoubleBufferPanel()
    {
        this.DoubleBuffered = true;
        this.ResizeRedraw = true;
    }
}