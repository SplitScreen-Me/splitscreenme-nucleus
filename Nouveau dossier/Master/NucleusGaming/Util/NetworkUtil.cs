using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Nucleus
{
    public static class NetworkUtil
    {
        public static string GetLocalIP()
        {
            string localIP = "?";
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList)
            {
                if (ip.AddressFamily.ToString() == "InterNetwork")
                {
                    localIP = ip.ToString();
                    break;
                }
            }
            return localIP;
        }
    }
}
