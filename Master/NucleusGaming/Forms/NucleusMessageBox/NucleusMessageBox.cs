
using System;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    public static class NucleusMessageBox
    {
        public static CustomMessageBox MessageBox;

        public static void Show(string title, string message, bool formating)
        {
            MessageBox?.Invoke((MethodInvoker)delegate ()
            {
                MessageBox.Close();    
            });

            MessageBox = new CustomMessageBox(title, message, formating);
            MessageBox.FormClosed += MessageBox_Dispose;
            
            MessageBox.ShowDialog();
            MessageBox?.BringToFront();
        }

        private static void MessageBox_Dispose(object sender, EventArgs e)
        {
            Form msgBox = (Form)sender;
            if (msgBox.IsHandleCreated)
            {
                MessageBox?.Invoke((MethodInvoker)delegate ()
                {

                    MessageBox.Dispose();
                });
            }

            MessageBox = null;
        }
    }
}
