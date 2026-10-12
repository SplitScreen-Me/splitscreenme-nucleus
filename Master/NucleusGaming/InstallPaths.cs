using System;
using System.IO;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Where Nucleus Co-op is installed. Every process gets the same answer (NucleusCoop.exe, StartGame.exe,
    /// Nucleus.IJx86/x64.exe), wherever its own exe sits: the folder of NucleusCoop.exe.
    /// </summary>
    public static class InstallPaths
    {
        /// <summary>The main executable: its folder is the install root.</summary>
        public const string MainExeName = "NucleusCoop.exe";

        /// <summary>Subfolder of the install root that holds the managed assemblies (probing privatePath of the exes).</summary>
        public const string BinFolderName = "bin";

        // Parent levels searched above the process folder, for helper exes in bin\ (or one level deeper).
        private const int MaxLevelsUp = 2;

        private static readonly Lazy<string> root = new Lazy<string>(() => FindRoot(AppDomain.CurrentDomain.BaseDirectory));

        /// <summary>
        /// Install root, without a trailing separator (except for a drive root). It is the process folder (AppDomain base
        /// directory) when NucleusCoop.exe is in it, else the closest parent (up to 2 levels) that contains NucleusCoop.exe,
        /// else the process folder itself.
        /// </summary>
        public static string Root => root.Value;

        /// <summary><c>&lt;root&gt;\bin</c>: the Nucleus and third-party managed assemblies.</summary>
        public static string BinDir => Path.Combine(Root, BinFolderName);

        internal static string FindRoot(string startFolder)
        {
            string start = Normalize(startFolder);
            DirectoryInfo dir = new DirectoryInfo(start);
            for (int level = 0; dir != null && level <= MaxLevelsUp; level++, dir = dir.Parent)
            {
                if (File.Exists(Path.Combine(dir.FullName, MainExeName)))
                {
                    return Normalize(dir.FullName);
                }
            }

            return start;
        }

        private static string Normalize(string folder)
        {
            string full = Path.GetFullPath(folder);
            string trimmed = full.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            // Keep "C:\" (a bare "C:" means the current folder of drive C).
            return trimmed.Length < full.Length && trimmed.EndsWith(":") ? full : trimmed;
        }
    }
}
