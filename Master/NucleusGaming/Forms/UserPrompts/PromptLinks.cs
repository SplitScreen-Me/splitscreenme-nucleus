using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace Nucleus.Gaming.Forms
{
    /// <summary>
    /// Links in prompt messages: either "[text]{url}" (the text becomes a link to the url) or, for older handlers, the
    /// first word that starts with a URL scheme.
    /// </summary>
    internal static class PromptLinks
    {
        private static readonly string[] Schemes =
        {
            "http:", "file:", "mailto:", "ftp:", "https:", "gopher:", "nntp:", "prospero:", "telnet:", "news:", "wais:", "outlook:",
        };

        /// <summary>Sets the text of <paramref name="label"/> and its link area, and opens the link on click.</summary>
        public static void Apply(LinkLabel label, string message)
        {
            label.Text = message;
            label.LinkArea = new LinkArea(0, 0);
            label.LinkClicked += Open;

            try
            {
                int shStart = message.IndexOf('[');
                int shEnd = message.IndexOf(']');
                string shorten = shStart != -1 && shEnd != -1 ? message.Substring(shStart + 1, shEnd - shStart - 1) : null;

                if (shorten == null)
                {
                    string search = message.Split(' ').FirstOrDefault(word => Schemes.Any(scheme => word.StartsWith(scheme)));

                    if (search != null)
                    {
                        label.Tag = search;
                        label.LinkArea = new LinkArea(message.IndexOf(search), search.Length);
                    }

                    return;
                }

                int linkStart = message.IndexOf('{');
                int linkEnd = message.IndexOf('}');

                if (linkStart == -1 || linkEnd == -1)
                {
                    return;
                }

                label.Tag = message.Substring(linkStart + 1, linkEnd - linkStart - 1);

                //Replace "[text]{url}" with "text".
                string holder = message.Substring(shStart, linkEnd + 1 - shStart);
                label.Text = message.Replace(holder, shorten);
                label.LinkArea = new LinkArea(message.IndexOf(shorten) - 1, shorten.Length);
            }
            catch
            {
                label.LinkArea = new LinkArea(0, 0);
            }
        }

        private static void Open(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if ((sender as LinkLabel)?.Tag is string link)
            {
                try
                {
                    Process.Start(link);
                }
                catch (Exception ex)
                {
                    LogManager.Log("Could not open the prompt link " + link + ": " + ex.Message);
                }
            }
        }
    }
}
