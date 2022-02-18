using System;
using System.Drawing;
using System.Windows.Forms;

namespace Nucleus.Gaming
{
    public class ImageButton : UserControl
    {
        private Image image;
        private Image imageHover;
        private Image imagePressed;
        private Image imageDisabled;

        public Image Image
        {
            get => image;
            set
            {
                image = value;
                Invalidate();
            }
        }
        public Image ImageHover
        {
            get => imageHover;
            set
            {
                imageHover = value;
                Invalidate();
            }
        }
        public Image ImagePressed
        {
            get => imagePressed;
            set
            {
                imagePressed = value;
                Invalidate();
            }
        }

        public Image ImageDisabled
        {
            get => imageDisabled;
            set
            {
                imageDisabled = value;
                Invalidate();
            }
        }

        public ImageButtonState State { get; private set; }

        public ImageButton()
        {
            State = ImageButtonState.Default;
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            State = ImageButtonState.Hover;
            base.OnMouseEnter(e);
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            State = ImageButtonState.Default;
            base.OnMouseLeave(e);
            Invalidate();
        }

        private ImageButtonState lastState;

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                lastState = State;
                State = ImageButtonState.Click;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                State = lastState;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (Enabled)
            {
                switch (State)
                {
                    case ImageButtonState.Default:
                        if (Image != null)
                        {
                            e.Graphics.DrawImage(Image, e.ClipRectangle);
                        }
                        break;
                    case ImageButtonState.Hover:
                        if (ImageHover == null)
                        {
                            if (Image != null)
                            {
                                e.Graphics.DrawImage(Image, e.ClipRectangle);
                            }
                        }
                        else
                        {
                            e.Graphics.DrawImage(ImageHover, e.ClipRectangle);
                        }
                        break;
                    case ImageButtonState.Click:
                        if (ImagePressed == null)
                        {
                            if (Image != null)
                            {
                                e.Graphics.DrawImage(Image, e.ClipRectangle);
                            }
                        }
                        else
                        {
                            e.Graphics.DrawImage(ImagePressed, e.ClipRectangle);
                        }
                        break;
                }
            }
            else
            {
                if (ImageDisabled != null)
                {
                    e.Graphics.DrawImage(ImageDisabled, e.ClipRectangle);
                }
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // ImageButton
            // 
            Name = "ImageButton";
            ResumeLayout(false);

        }
    }
}
