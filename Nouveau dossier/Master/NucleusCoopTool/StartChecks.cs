using System.Windows.Forms;

namespace Nucleus.Coop
{
    internal static class StartChecks
    {
        public static bool StartCheck()
        {
            return CheckInstallFolder();
        }

        private static bool CheckInstallFolder()
        {
            string exePath = System.Reflection.Assembly.GetExecutingAssembly().Location.ToLower();

            bool problematic = exePath.StartsWith(@"C:\Program Files\".ToLower()) ||
                               exePath.StartsWith(@"C:\Program Files (x86)\".ToLower()) ||
                               exePath.StartsWith(@"C:\Users\".ToLower()) ||
                               exePath.StartsWith(@"C:\Windows\".ToLower());

            if (problematic)
            {

                string message = "Nucleus Co-Op should not be installed here.\n\n" +
                                "Do NOT install in any of these folders:\n" +
                                "- A folder containing any game files\n" +
                                "- C:\\Program Files or C:\\Program Files (x86)\n" +
                                "- C:\\Users (including Documents, Desktop, or Downloads)\n" +
                                "- Any folder with security settings like C:\\Windows\n" +
                                "\n" +
                                "A good place is C:\\Nucleus\\NucleusCoop.exe";

                return MessageBox.Show(message, "Warning", MessageBoxButtons.OKCancel) == DialogResult.OK;

            }

            return true;
        }
    }
}
