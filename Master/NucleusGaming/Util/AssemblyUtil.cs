using System.IO;
using System.Reflection;

namespace Nucleus.Gaming
{
    public static class AssemblyUtil
    {
        public static string GetStartFolder()
        {
            Assembly entry = Assembly.GetAssembly(typeof(GameManager));
            return Path.GetDirectoryName(entry.Location);
        }
    }
}
