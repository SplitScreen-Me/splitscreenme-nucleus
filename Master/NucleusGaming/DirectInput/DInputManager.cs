using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Nucleus.Gaming.DirectInput
{
    public static class DInputManager
    {
        private static DInputLibrary[] libraries = new DInputLibrary[]
            {
                new DInputLibrary()
                {
                    Hash= "00000000000000000000000000000000",
                    Hash1 = 0,
                    Hash2 = 0,
                    ID = 0
                },
                new DInputLibrary()
                {
                    Hash= "478044F1051EE570393A1D3752586676",
                    Hash1 = 0x70E51E05F1448047,//8134941312786137159,
                    Hash2 = 0x76665852371D3A39,//8531603654235208249,
                    ID = 1
                },
                new DInputLibrary()
                {
                    Hash = "BCDBADF88733E25BC18B412462082A18",
                    Hash1 = 0x5BE23387F8ADDBBC,
                    Hash2 = 0x182A086224418BC1,
                    ID = 2
                }
            };


        public class DInputLibrary
        {
            public string Hash;
            public long Hash1;
            public long Hash2;
            public int ID;
        };

        private static DInputLibrary library;
        public static DInputLibrary Library => library;

        static DInputManager()
        {
            bool is64OS = Environment.Is64BitOperatingSystem;
            string path;
            if (is64OS)
            {
                path = @"C:\Windows\SysWow64\dinput.dll";
            }
            else
            {
                // no freaking idea where it really is
                path = @"C:\Windows\System32\dinput.dll";
            }

            if (!File.Exists(path))
            {
                throw new NotSupportedException();
            }

            byte[] data = File.ReadAllBytes(path);
            MD5 md5 = MD5.Create();
            byte[] hash = md5.ComputeHash(data);

            string hashStr = BitConverter.ToString(hash).Replace("-", "");

            DInputLibrary def = libraries.FirstOrDefault(c => c.Hash == hashStr);
            if (def != null)
            {
                library = def;
            }
            else
            {
                library = libraries.First();
            }
        }
    }
}
