using Nucleus.Coop.Forms;
using Nucleus.Gaming;
using Nucleus.Gaming.Http_Client;
using Nucleus.Gaming.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Coop.UI
{
    public static class PatreonMenuFunc
    {
        public static void PatreonMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ContextMenuStrip socialMenu = (ContextMenuStrip)sender;

            ToolStripMenuItem patreonRedirectItem = socialMenu.Items["PatreonRedirect"] as ToolStripMenuItem;
            patreonRedirectItem.ForeColor = Color.Aqua;
            patreonRedirectItem.Click -= RedirectToPatreon_Click;
            patreonRedirectItem.Click += RedirectToPatreon_Click;

            ToolStripMenuItem SupportersListMenuItem = socialMenu.Items["SupportersList"] as ToolStripMenuItem;
            SupportersListMenuItem.Click -= SupportersListMenuItem_Click;
            SupportersListMenuItem.Click += SupportersListMenuItem_Click;           
        }

        public static void SocialMenu_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip socialMenu = (ContextMenuStrip)sender;
            if (Theme_Settings.WindowRoundedCorners)
                FormGraphicsUtil.CreateRoundedControlRegion(socialMenu, 2, 2, socialMenu.Width - 1, socialMenu.Height, 20, 20);
        }

        private static void RedirectToPatreon_Click(object sender, EventArgs e) => Process.Start(Links.NC_Patreon);

        private static void SupportersListMenuItem_Click(object sender, EventArgs e)
        {
            _ = StartSupportersShowcase();
        }

        private static async Task StartSupportersShowcase()
        {
            await Patrons_Init.Start();
        }
    }
}
