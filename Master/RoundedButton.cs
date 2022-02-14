using System;
using System.Drawing.Drawing2D;
namespace RoundedButton
{
class RoundedButton : Button
		{
            public GraphicsPath GetRoundPath(RectangleF Rect)
            {
				int radius = 20;
				float r2 = radius / 2f;
				
				GraphicsPath buttonShape = new GraphicsPath();
				buttonShape.AddArc(Rect.X, Rect.Y, radius, radius, 180, 90);
				buttonShape.AddArc(Rect.X + Rect.Width - radius, Rect.Y, radius, radius, 270, 90);
				buttonShape.AddArc(Rect.X + Rect.Width - radius, 
				Rect.Y + Rect.Height - radius, radius, radius, 0, 90);
				buttonShape.AddArc(Rect.X, Rect.Y + Rect.Height - radius, radius, radius, 90, 90);
				buttonShape.CloseFigure();
                return buttonShape;

            }
			protected override void OnPaint(PaintEventArgs e)
			{ 
			        var BorderColor = Color.DodgerBlue;
				    float border = 3.0f;
					base.OnPaint(e);
					RectangleF Rect = new RectangleF(0, 0, this.Width, this.Height);
					GraphicsPath buttonShape = GetRoundPath(Rect);//, 50);
					this.Region = new Region(buttonShape);
					using (Pen pen = new Pen(BorderColor,border))
				{
					
					pen.Alignment = PenAlignment.Inset;
					e.Graphics.DrawPath(pen, buttonShape);
				}
			}
		}       
}