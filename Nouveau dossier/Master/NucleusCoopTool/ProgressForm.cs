using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SplitTool
{
    public partial class ProgressForm : Form
    {
        public int Progress
        {
            set { progressBar1.Value = value; }
        }

        public ProgressForm()
        {
            InitializeComponent();
        }
    }
}
