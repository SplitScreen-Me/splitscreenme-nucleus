using System.Collections.Generic;

namespace Nucleus.Gaming
{
    public static class JoystickDatabase
    {
        public static Dictionary<string, int> JoystickIDs = new Dictionary<string, int>
        {
            { "05c4054c-0000-0000-0000-504944564944", 2 }
        };

        public static int GetID(string deviceGuid)
        {
            if (JoystickIDs.TryGetValue(deviceGuid, out int id))
            {
                return id;
            }
            return 0;
        }
    }
}