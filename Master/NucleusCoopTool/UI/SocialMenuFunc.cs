using Nucleus.Gaming;
using Nucleus.Gaming.Cache;
using Nucleus.Gaming.Forms;
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
    public static class SocialMenuFunc
    {
        public static void SocialMenu_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ContextMenuStrip socialMenu = (ContextMenuStrip)sender;

            UI_Interface.SocialMenuButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_dropdown_opened.png");

            ToolStripMenuItem fAQMenuItem = socialMenu.Items["fAQMenuItem"] as ToolStripMenuItem;
            fAQMenuItem.Click += Link_faq_Click;

            ToolStripMenuItem redditMenuItem = socialMenu.Items["redditMenuItem"] as ToolStripMenuItem;
            redditMenuItem.Click += RedditToolStripMenuItem_Click;

            ToolStripMenuItem discordMenuItem = socialMenu.Items["discordMenuItem"] as ToolStripMenuItem;
            discordMenuItem.Click += DiscordToolStripMenuItem_Click;

            ToolStripMenuItem splitCalculatorMenuItem = socialMenu.Items["splitCalculatorMenuItem"] as ToolStripMenuItem;
            splitCalculatorMenuItem.Click += SplitCalculatorToolStripMenuItem_Click;

            ToolStripMenuItem thirdPartyToolsToolStripMenuItem = socialMenu.Items["thirdPartyToolsToolStripMenuItem"] as ToolStripMenuItem;

            for (int i = 0; i < socialMenu.Items.Count; i++)
            {
                if (socialMenu.Items[i].GetType() == typeof(ToolStripSeparator))
                {
                    continue;
                }

                ToolStripMenuItem item = socialMenu.Items[i] as ToolStripMenuItem;

                item.ImageScaling = ToolStripItemImageScaling.None;
                item.ImageAlign = ContentAlignment.MiddleLeft;
               

                if (item == fAQMenuItem)
                {
                    item.ForeColor = Color.Aqua;
                    item.ToolTipText = "Nucleus Co-op FAQ.";
                }

                if (item == redditMenuItem)
                {
                    item.ForeColor = Color.Aqua;
                    item.ToolTipText = "Official Nucleus Co-op Subreddit.";
                }

                if (item == discordMenuItem)
                {
                    item.ForeColor = Color.Aqua;
                    item.ToolTipText = "Join the official Nucleus Co-op discord server.";
                }

                if (item == splitCalculatorMenuItem)
                {
                    item.ToolTipText = "This program can estimate the system requirements needed to run a game in split-screen.";
                }

                if (item == thirdPartyToolsToolStripMenuItem)
                {
                    item.DropDown.BackgroundImage = socialMenu.BackgroundImage;

                    if (item.DropDownItems.Count > 0)
                    {
                        ToolStripItem xOutputToolStripMenuItem = item.DropDownItems["xOutputToolStripMenuItem"];
                        xOutputToolStripMenuItem.Click += XOutputToolStripMenuItem_Click;
                        xOutputToolStripMenuItem.ToolTipText = "XOutput is a software that can convert DirectInput into XInput.";

                        ToolStripItem dS4WindowsToolStripMenuItem = item.DropDownItems["dS4WindowsToolStripMenuItem"];
                        dS4WindowsToolStripMenuItem.Click += DS4WindowsToolStripMenuItem_Click;
                        dS4WindowsToolStripMenuItem.ToolTipText = "Xinput emulator for Ps4 controllers.";

                        ToolStripItem hidHideToolStripMenuItem = item.DropDownItems["hidHideToolStripMenuItem"];
                        hidHideToolStripMenuItem.Click += HidHideToolStripMenuItem_Click;
                        hidHideToolStripMenuItem.ToolTipText = "With HidHide it is possible to deny a specific application access to one or more human interface devices.";

                        ToolStripItem scpToolkitToolStripMenuItem = item.DropDownItems["scpToolkitToolStripMenuItem"];
                        scpToolkitToolStripMenuItem.Click += ScpToolkitToolStripMenuItem_Click;
                        scpToolkitToolStripMenuItem.ToolTipText = "Xinput emulator for Ps3 controllers.";

                        for (int d = 0; d < item.DropDownItems.Count; d++)
                        {
                            ToolStripItem subItem = item.DropDownItems[d];

                            subItem.BackColor = Color.Transparent;
                            subItem.ForeColor = Color.Aqua;
                        }
                    }
                }
            }
        }

        public static void SocialMenu_Opened(object sender, EventArgs e)
        {
            ContextMenuStrip socialMenu = (ContextMenuStrip)sender;
            FormGraphicsUtil.CreateRoundedControlRegion(socialMenu, 2, 2, socialMenu.Width - 1, socialMenu.Height, 20, 20);
        }

        public static void SocialMenu_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            UI_Interface.SocialMenuButton.BackgroundImage = ImageCache.GetImage(Globals.ThemeFolder + "title_dropdown_closed.png");
        }

        private static void Link_faq_Click(object sender, EventArgs e) => Process.Start(Links.NC_Faq);

        private static void FAQToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.NC_Faq);

        private static void RedditToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.NC_Reddit);

        private static void DiscordToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.NC_Discord);

        private static void SplitCalculatorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (Process.GetProcessesByName("SplitCalculator").Length >= 1)
                {
                    return;
                }

                Process.Start(Path.Combine(Application.StartupPath, @"utils\SplitCalculator\SplitCalculator.exe"));
            }
            catch (Exception)
            {
                NucleusMessageBox.Show("Error",
                                       $"SplitCalculator.exe has not been found or is blocked by Windows SmartScreen, " +
                                       $"you can try to start it manually at " +
                                       $"\"{Globals.NucleusInstallRoot}\\utils\\SplitCalculator\\SplitCalculator.exe\" " +
                                       $"or try again with a fresh Nucleus Co-op installation.", true);
            }
        }

        private static void XOutputToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.Xouput);

        private static void DS4WindowsToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.DS4Windows);

        private static void HidHideToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.HidHide);

        private static void ScpToolkitToolStripMenuItem_Click(object sender, EventArgs e) => Process.Start(Links.SCPToolkit);

    }
}
