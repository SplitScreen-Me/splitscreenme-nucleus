using Nucleus.Gaming;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

namespace Nucleus.Coop
{
    /// <summary>
    /// Form that all other forms inherit from. Has all
    /// the default design parameters to have the Nucleus Coop look and feel
    /// </summary>

	class RoundedButton : Button
		{
            public GraphicsPath GetRoundPath(RectangleF Rect)
            {
				int radius = 8;//8
				float r2 = radius / 2f;
				
				GraphicsPath buttonShape = new GraphicsPath();
				buttonShape.AddArc(Rect.X, Rect.Y, radius, radius, 180, 90);
				buttonShape.AddArc(Rect.X + Rect.Width - radius, Rect.Y, radius, radius, 270, 90);
				buttonShape.AddArc(Rect.X + Rect.Width - radius,Rect.Y + Rect.Height - radius, radius, radius, 0, 90);
				buttonShape.AddArc(Rect.X, Rect.Y + Rect.Height - radius, radius, radius, 90, 90);
				buttonShape.CloseFigure();
                return buttonShape;
            }
			
			protected override void OnPaint(PaintEventArgs e)
			{ 			       
					base.OnPaint(e);
					RectangleF Rect = new RectangleF(0, 0, this.Width, this.Height);
					GraphicsPath buttonShape = GetRoundPath(Rect);
					this.Region = new Region(buttonShape);				
			}
		}       
}