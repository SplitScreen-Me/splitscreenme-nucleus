using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Gaming.Coop.Generic
{ 
    public partial class ProcessPicker : Form, IDynamicSized
    {

        private float oldScale;

        public ProcessPicker()
        {
            InitializeComponent();
            MaximizeBox = false;
            MinimizeBox = false;
            DPIManager.Register(this);
            DPIManager.AddForm(this);
            DPIManager.Update(this);
        }

        private bool scaled = false;

        public void UpdateSize(float scale)
        {
            if (IsDisposed)
            {
                DPIManager.Unregister(this);
                return;
            }
            if (!scaled)
            {
                oldScale = scale;
                scaled = true;
            }
        }

        public void ScaleList()
        {
            float newFontSize = Font.Size * oldScale;
            foreach (Control c in Controls)
            {
                if (c.GetType() == typeof(Panel))
                {
                    foreach (Control list in c.Controls)
                    {
                        list.Font = new Font("Microsoft Sans Serif", newFontSize, FontStyle.Regular, GraphicsUnit.Point, 0);
                        list.SuspendLayout();
                        list.ResumeLayout();
                    }
                }
            }
        }


    }

}
