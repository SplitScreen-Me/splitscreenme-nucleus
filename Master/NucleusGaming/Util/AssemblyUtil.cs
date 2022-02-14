using System.IO;
using System.Reflection;

namespace Nucleus.Gaming
{
    public static class AssemblyUtil
    {
        public static string GetStartFolder()
        {
            Assembly entry = Assembly.GetEntryAssembly();
            return Path.GetDirectoryName(entry.Location);
        }
    }
}
