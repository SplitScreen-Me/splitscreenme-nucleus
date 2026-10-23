using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal class OpenGameContentFolder
    {
        public static void OpenContentFolder(UserGameInfo currentGameInfo)
        {
            if (Directory.Exists(currentGameInfo.HandlerInfo.GenericGameInfo.Content_Folder))
            {
                Process.Start(currentGameInfo.HandlerInfo.GenericGameInfo.Content_Folder);
            }
            else
            {
                MessageBox.Show("No data present for this game.", "No data found", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
