using Nucleus.Gaming;
using Nucleus.Gaming.Coop;
using Nucleus.Gaming.Forms;
using System.Diagnostics;
using System.Reflection;
using System.Security.Principal;
using System.Windows.Forms;

namespace Nucleus.Coop.Tools
{
    internal class CheckGameRequirements
    {
        public static bool MatchRequirements(UserGameInfo userGameInfo)
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);

            string message;
            string gamePath = userGameInfo.ExePath;
            bool imcompatibleGamePath = gamePath.StartsWith(@"C:\Users\") ||
                                         gamePath.StartsWith(@"C:\Windows\");
            bool skip = false;

            bool useUserProfileSavePath = userGameInfo.HandlerInfo.GenericGameInfo.UserProfileSavePath != null ? userGameInfo.HandlerInfo.GenericGameInfo.UserProfileSavePath.StartsWith("Documents") : false;
            bool useUserProfileConfigPath = userGameInfo.HandlerInfo.GenericGameInfo.UserProfileConfigPath != null ? userGameInfo.HandlerInfo.GenericGameInfo.UserProfileConfigPath.StartsWith("Documents") : false;
            bool useDocumentsSavePath = userGameInfo.HandlerInfo.GenericGameInfo.DocumentsSavePath != null ? userGameInfo.HandlerInfo.GenericGameInfo.DocumentsSavePath.StartsWith("Documents") : false;
            bool useDocumentsConfigPath = userGameInfo.HandlerInfo.GenericGameInfo.DocumentsConfigPath != null ? userGameInfo.HandlerInfo.GenericGameInfo.DocumentsConfigPath.StartsWith("Documents") : false;

            bool useUserDocuments = useUserProfileSavePath || useUserProfileConfigPath || useDocumentsSavePath || useDocumentsConfigPath;

            if (Globals.IsOneDriveEnabled && userGameInfo.HandlerInfo.GenericGameInfo.UseNucleusEnvironment && useUserDocuments)
            {
                message = $@"This handler can't run if your default Documents path is in OneDrive -> {Globals.UserDocumentsRoot} \n";

                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
               
                return false;
            }

            if ((userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsers || userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsersAlt) && imcompatibleGamePath)
            {
                message = $@"This game handler does not support the current {userGameInfo.GameGuid} installation path." + "\n\n" +
                $@"Do NOT install {userGameInfo.GameGuid} in any of these folders:" + "\n" +
                "- C:\\Program Files or C:\\Program Files (x86)\n" +
                "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                "- Any folder with security settings like C:\\Windows\n" +
                "\n";

                MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
           
            if ((userGameInfo.HandlerInfo.GenericGameInfo.RequiresAdmin || userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsersAlt || userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsers || userGameInfo.HandlerInfo.GenericGameInfo.ChangeIPPerInstanceAlt) && !principal.IsInRole(WindowsBuiltInRole.Administrator) ||
               ((userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsersAlt || userGameInfo.HandlerInfo.GenericGameInfo.LaunchAsDifferentUsers || userGameInfo.HandlerInfo.GenericGameInfo.ChangeIPPerInstanceAlt) && (Program.ForcedBadPath && principal.IsInRole(WindowsBuiltInRole.Administrator))))
            {
                if (Program.ForcedBadPath && principal.IsInRole(WindowsBuiltInRole.Administrator))
                {
                    message = "This game handler does not support the current Nucleus Co-op installation path.\n\n" +
                          "Do NOT install in any of these folders:\n" +
                          "- A folder containing any game files\n" +
                          "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                          "- Any folder with security settings like C:\\Windows\n" +
                          "\n" +
                          "A good place is C:\\Nucleus\\NucleusCoop.exe";
                }
                else
                {
                    message = "";

                    DialogResult dialogResult = MessageBox.Show("This handler requires you to run Nucleus as administrator.\n" +
                                                                "Restart Nucleus as administrator?", "Requires administrator rights", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                    if (dialogResult == DialogResult.Yes)
                    {
                        RestartAsAdmin();
                        return false;
                    }

                    skip = true;
                }

                if(!skip)
                {
                    MessageBox.Show(message, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                
                return false;
            }
         
            return true;        
        }


        private static void RestartAsAdmin()
        {
            try//If user select "no" in the admin prompt
            {
                var proc = new Process 
                {                   
                    StartInfo = 
                    {
                       FileName = Assembly.GetExecutingAssembly().Location,
                       UseShellExecute = true,
                       Verb = "runas"
                    }
                };

                proc.Start();

                //admin prompt give enough time
                Process.GetCurrentProcess().Kill();
            }
            catch
            { }
        }
    }
}
