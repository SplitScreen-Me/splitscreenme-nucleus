using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Nucleus.Gaming.Tools.GameStarter
{
    /// <summary>
    /// Util class for executing and reading output from the Nucleus.Coop.StartGame application
    /// </summary>
    public static class StartGameUtil
    {
        private static string lastLine;
        private static object locker = new object();
        private static readonly IniFile ini = new Gaming.IniFile(Path.Combine(Directory.GetCurrentDirectory(), "Settings.ini"));

        public static string GetStartGamePath()
        {
            return Path.Combine(Path.GetDirectoryName(Assembly.GetAssembly(typeof(GameManager)).Location), "StartGame.exe");
        }

        public static string GetArguments(string pathToGame, string args, int waitTime, string mutexType, params string[] mutex)
        {
            string mu = "";
            for (int i = 0; i < mutex.Length; i++)
            {
                mu += mutex[i];

                if (i != mutex.Length - 1)
                {
                    mu += "|";
                }
            }

            return "\"" + pathToGame + "\" \"" + args + "\" \"" + waitTime + "\" \"" + mutexType + "\" \"" + mu + "\"";
        }

        public static void KillMutex(Process p, string mutexType, bool partial, params string[] mutex)
        {
            lock (locker)
            {
                string startGamePath = GetStartGamePath();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = startGamePath
                };

                string mu = "";
                for (int i = 0; i < mutex.Length; i++)
                {
                    mu += mutex[i];
                    if (i != mutex.Length - 1)
                    {
                        mu += "|==|";
                    }
                }

                startInfo.Arguments = "\"proc|::|" + p.Id.ToString() + "\" \"partialmutex|::|" + partial + "\" \"mutextype|::|" + mutexType + "\" \"mutex|::|" + mu + "\"";
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;

                Process proc = Process.Start(startInfo);
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.BeginOutputReadLine();

                proc.WaitForExit();
            }
        }

        public static bool MutexExists(Process p, string mutexType, bool partial, params string[] mutex)
        {
            lock (locker)
            {
                string startGamePath = GetStartGamePath();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = startGamePath
                };

                string mu = "";
                for (int i = 0; i < mutex.Length; i++)
                {
                    mu += mutex[i];

                    if (i != mutex.Length - 1)
                    {
                        mu += "|==|";
                    }
                }

                startInfo.Arguments = $"\"proc|::|{p.Id}\" \"partialmutex|::|{partial}\" \"mutextype|::|{mutexType}\" \"output|::|{mu}\"";
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;

                Process proc = Process.Start(startInfo);
                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.BeginOutputReadLine();

                proc.WaitForExit();

                bool.TryParse(lastLine, out bool result);

                return result;
            }
        }

        /// <summary>
        /// NOT THREAD SAFE
        /// </summary>
        /// <param name="pathToGame"></param>
        /// <param name="args"></param>
        /// <param name="waitTime"></param>
        /// <param name="mutex"></param>
        /// <returns></returns>
        public static uint StartGame(string pathToGame, string args, bool hook, bool delay, bool renameMutex, string mutexNames, bool setWindow, bool isDebug, string nucleusFolder, bool blockRaw, bool UseNucleusEnvironment, string playerNick, bool startupHooksEnabled, bool createSingle, string rawHid, int width, int height, int posx, int posy, string docpath, bool usedocs, /*bool runAdmin, string rawHid,*/ string workingDir = null)
        {
            lock (locker)
            {
                string startGamePath = GetStartGamePath();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = startGamePath
                };

                if (!string.IsNullOrWhiteSpace(workingDir))
                {
                    workingDir = "|" + workingDir;
                }

                //string arguments = 
                startInfo.Arguments = "\"" + "hook|::|" + hook + "\" \"delay|::|" + delay + "\" \"renamemutex|::|" + renameMutex + "\" \"mutextorename|::|" + mutexNames + "\" \"setwindow|::|" + setWindow + "\" \"isdebug|::|" + isDebug + "\" \"nucleusfolderpath|::|" + nucleusFolder + "\" \"blockraw|::|" + blockRaw + "\" \"nucenv|::|" + UseNucleusEnvironment + "\" \"playernick|::|" + playerNick + "\" \"starthks|::|" + startupHooksEnabled + "\" \"createsingle|::|" + createSingle + "\" \"rawhid|::|" + rawHid + "\" \"width|::|" + width + "\" \"height|::|" + height + "\" \"posx|::|" + posx + "\" \"posy|::|" + posy + "\" \"docpath|::|" + docpath + "\" \"usedocs|::|" + usedocs  /*+ "\" \"rawhid|::|" + rawHid*/ + "\" \"game|::|" + pathToGame + workingDir + ";" + args + "\"";
                startInfo.RedirectStandardOutput = true;
                startInfo.UseShellExecute = false;

                Process proc = Process.Start(startInfo);

                proc.OutputDataReceived += proc_OutputDataReceived;
                proc.BeginOutputReadLine();

                proc.WaitForExit();

                //parse the last line for the process ID
                return uint.Parse(lastLine.Split(':')[1]);
            }
        }
        public static void proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data))
            {
                return;
            }
            Console.WriteLine($"Redirected output: {e.Data}");
            lastLine = e.Data;
        }

        public static bool SymlinkGame(string root, string destination, out int exitCode,
            string[] dirExclusions, string[] fileExclusions, string[] fileCopyInstead, bool hardLink, bool symFolders, int numPlayers)
        {

            exitCode = 1;

            lock (locker)
            {
                string startGamePath = GetStartGamePath();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = startGamePath
                };


                string de = "";
                for (int i = 0; i < dirExclusions.Length; i++)
                {
                    de += dirExclusions[i];
                    if (i != dirExclusions.Length - 1)
                    {
                        de += "|==|";
                    }
                }

                string fe = "";
                for (int i = 0; i < fileExclusions.Length; i++)
                {
                    fe += fileExclusions[i];
                    if (i != fileExclusions.Length - 1)
                    {
                        fe += "|==|";
                    }
                }

                string fc = "";
                for (int i = 0; i < fileCopyInstead.Length; i++)
                {
                    fc += fileCopyInstead[i];
                    if (i != fileCopyInstead.Length - 1)
                    {
                        fc += "|==|";
                    }
                }

                startInfo.Arguments = "\"" + "root|::|" + root.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) /*+ "\" \"currentdir|::|" + currentDir */+ "\" \"destination|::|" + destination + "\" \"direxclusions|::|" + de + "\" \"fileexclusions|::|" + fe + "\" \"filecopyinstead|::|" + fc + "\" \"hardlink|::|" + hardLink + "\" \"symfolders|::|" + symFolders + "\" \"numplayers|::|" + numPlayers /*+ "\" \"overridespecial|::|" + overrideSpecial*/ + "\" \"symlink|::|" + "true";
                //"\"" + "hook|::|" + hook + "\" \"delay|::|"
                startInfo.UseShellExecute = true;
                startInfo.Verb = "runas";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                Process proc = Process.Start(startInfo);//TODO: can throw error

                proc.WaitForExit();
            }

            return true;
        }
    }
}
