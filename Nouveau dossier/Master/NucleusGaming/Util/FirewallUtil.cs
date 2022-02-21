using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NetFwTypeLib;

namespace Nucleus
{
    public static class FirewallUtil
    {
        public static INetFwMgr WinFirewallManager()
        {
            Type type = Type.GetTypeFromCLSID(new Guid("{304CE942-6E39-40D8-943A-B913C40C9CD4}"));
            return (INetFwMgr)Activator.CreateInstance(type);
        }
        public static bool AuthorizeProgram(string title, string path)
        {
            return AuthorizeProgram(title, path, NET_FW_SCOPE_.NET_FW_SCOPE_ALL, NET_FW_IP_VERSION_.NET_FW_IP_VERSION_ANY);
        }
        public static bool AuthorizeProgram(string title, string path, NET_FW_SCOPE_ scope, NET_FW_IP_VERSION_ ipver)
        {
            Type type = Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication");
            INetFwAuthorizedApplication authapp = Activator.CreateInstance(type)
                as INetFwAuthorizedApplication;
            authapp.Name = title;
            authapp.ProcessImageFileName = path;
            authapp.Scope = scope;
            authapp.IpVersion = ipver;
            authapp.Enabled = true;

            INetFwMgr mgr = WinFirewallManager();
            try
            {
                mgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(authapp);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.Write(ex.Message);
                return false;
            }
            return true;
        }
    }
}
