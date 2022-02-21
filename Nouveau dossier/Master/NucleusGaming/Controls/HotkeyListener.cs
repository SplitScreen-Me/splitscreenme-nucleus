using System;
using System.Windows.Forms;

namespace Nucleus.Gaming.Controls
{
    public partial class HotkeyListener : Form
    {
        public HotkeyListener()
        {
            InitializeComponent();
        }

        private const int MYACTION_HOTKEY_ID = 1;

        public event Action HotKeyPressed;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x0312 && m.WParam.ToInt32() == MYACTION_HOTKEY_ID)
            {
                // My hotkey has been typed

                // Do what you want here
                // ...
                if (HotKeyPressed != null)
                {
                    HotKeyPressed();
                }
            }
            base.WndProc(ref m);
        }
    }
}
