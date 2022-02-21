using Microsoft.Win32;
using Nucleus.Gaming;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;


namespace Nucleus.Coop
{
    /// <summary>
    /// Form that all other forms inherit from. Has all
    /// the default design parameters to have the Nucleus Coop look and feel
    /// </summary>
    public class BaseForm : Form, IDynamicSized
    {
        public readonly IniFile ini = new IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));
        public BaseForm()
        {        
            //SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged); 

            Name = "BaseForm";
            Text = "BaseForm";
            // Default DPI = 96 = 100%
            // 1 pt = 1/72 inch
            // 12pt = 1/6 inch
            // 12 * 300% = 36
            // 12 * 125% = 15
            // 12 * 150% = 18
            //AutoScaleMode = AutoScaleMode.Font;

            // BackColor = Color.FromArgb(50, 50, 50);
            // ForeColor = Color.White;
            //  Margin = new Padding(4, 4, 4, 4);


            // create it here, else the desgienr will show the default windows font
            //Font = new Font("Segoe UI", 9, GraphicsUnit.Point);
            Font = new Font("Franklin Gothic Medium",9.75f, GraphicsUnit.Point);

            DPIManager.Register(this);
        }

     

        ~BaseForm()
        {       
           //DPIManager.Unregister(this);
        }

        //private void SystemEvents_DisplaySettingsChanged(object sender, EventArgs e)
        //{
        //    DPIManager.ForceUpdate();
        //}

        public void UpdateSize(float scale)
        {
        }

        /// <summary>
        /// Removes the flickering from constantly painting, if needed
        /// </summary>
        public void RemoveFlicker()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.DoubleBuffer, true);
        }

        /// <summary>
        /// Position the form on the same monitor the user has put our app!
        /// (by default, forms are open on the primary monitor, but if the user dragged
        /// our form to another monitor the child forms still get created on the main monitor.
        /// This is a small quality of life fix)
        /// </summary>
        /// <param name="f">The form to move</param>
        public void SetUpForm(System.Windows.Forms.Form f)
        {
            Point desktop = this.DesktopLocation;
            f.SetDesktopLocation(desktop.X + 100, desktop.Y + 100);
        }
    }
}