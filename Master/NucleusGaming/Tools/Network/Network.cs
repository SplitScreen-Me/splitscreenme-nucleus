using Nucleus.Gaming.Coop;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Nucleus.Gaming.Forms;

namespace Nucleus.Gaming.Tools.Network
{
    public static class Network
    {
        [DllImport("iphlpapi.dll", CharSet = CharSet.Auto)]
        private static extern int GetBestInterface(UInt32 destAddr, out UInt32 bestIfIndex);

        public static string iniNetworkInterface;
        public static string currentIPaddress;
        public static string currentSubnetMask;
        public static bool isDHCPenabled;
        public static bool isDynamicDns;
        public static string currentGateway;
        public static int hostAddr = 169;
        public static List<string> dnsAddresses = new List<string>();
        public static string dnsServersStr;


        public static void ChangeIPPerInstance(int i, string networkInterfaceOverride = null)
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log(string.Format("Changing IP for instance {0}", i + 1));

            if (i == 0)
            {
                if (string.IsNullOrEmpty(iniNetworkInterface) || iniNetworkInterface == "Automatic")
                {
                    string localIP = GetLocalIP();

                    handlerInstance.Log("No network interface provided, attempting to automatically find it");
                    var ni = NetworkInterface.GetAllNetworkInterfaces();
                    bool foundNIC = false;
                    foreach (NetworkInterface item in ni)
                    {
                        if (item.OperationalStatus == OperationalStatus.Up)
                        {
                            foreach (UnicastIPAddressInformation ip in item.GetIPProperties().UnicastAddresses)
                            {
                                if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                                {
                                    string ipAddress = ip.Address.ToString();
                                    if (ipAddress == localIP)
                                    {
                                        iniNetworkInterface = item.Name;
                                        handlerInstance.Log("Found network interface: " + iniNetworkInterface);
                                        foundNIC = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (foundNIC)
                        {
                            break;
                        }
                    }
                }

                if (iniNetworkInterface == null)
                {
                    handlerInstance.Log("ERROR - Unable to resolve network interface");
                    UserPrompts.Report(PromptIcon.Error, "Error", "Unable to resolve network interface");
                    return;
                }

                try
                {
                    var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == iniNetworkInterface);
                    var ipProperties = networkInterface.GetIPProperties();
                    var ipInfo = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);
                    currentIPaddress = ipInfo.Address.ToString();
                    currentSubnetMask = ipInfo.IPv4Mask.ToString();
                    currentGateway = ipProperties.GatewayAddresses?.FirstOrDefault(g => g.Address.AddressFamily.ToString() == "InterNetwork")?.Address.ToString();
                    isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;
                    isDynamicDns = ipProperties.IsDynamicDnsEnabled;
                    IPAddressCollection dnsServers = ipProperties.DnsAddresses;
                    foreach (IPAddress dns in dnsServers)
                    {
                        if (dns.AddressFamily == AddressFamily.InterNetwork)
                        {
                            dnsAddresses.Add(dns.ToString());
                            dnsServersStr += dns.ToString() + " ";
                        }
                    }

                    handlerInstance.Log("Default IP settings for NetworkInterface: " + iniNetworkInterface + ", IP: " + currentIPaddress + " Subnet Mask: " + currentSubnetMask + " Default Gateway: " + currentGateway + " DHCP: " + isDHCPenabled + " Dnyamic DNS: " + isDynamicDns + " DNS: " + dnsServersStr);
                }
                catch (Exception ex)
                {
                    UserPrompts.Report(PromptIcon.Error, "Error", "Obtaining default IP settings error. " + ex.Message);
                }
            }

            if (networkInterfaceOverride == null)
            {
                UserPrompts.Report(PromptIcon.Warning, "Nucleus - Change IP Per Instance", "WARNING: This feature is highly experimental!\n\nYour computers IP is about to be changed. You may receive a prompt immediately after to complete this action. Your IP settings will be reverted back to normal upon exiting Nucleus normally. However, if Nucleus crashes, it is not gauranteed that your settings will be set back.\n\nPress OK when ready to have your IP changed.\n\nOriginal Settings:\nNetworkInterface: " + iniNetworkInterface + "\nIP: " + currentIPaddress + "\nSubnet Mask: " + currentSubnetMask + "\nDefault Gateway: " + currentGateway + "\nDHCP: " + isDHCPenabled + "\nDynamic DNS: " + isDynamicDns + "\nDNS: " + dnsServersStr);
            }
            else
            {
                Thread.Sleep(5000);
            }
            string ipNetwork = currentIPaddress.Substring(0, currentIPaddress.LastIndexOf('.') + 1);

            Ping pingSender = new Ping();
            for (int a = 0; a < 10; a++)
            {
                PingReply reply = pingSender.Send(ipNetwork + (hostAddr + i).ToString(), 1000);

                if (reply.Status == IPStatus.Success)
                {
                    hostAddr++;
                    continue;
                }
                else
                {
                    break;
                }
            }

            string shostAddr = (hostAddr + i).ToString();
            handlerInstance.Log("Changing IP to: " + ipNetwork + shostAddr);

            if (networkInterfaceOverride != null)
            {
                SetIP(networkInterfaceOverride, ipNetwork + shostAddr, currentSubnetMask, currentGateway);
            }
            else
            {
                SetIP(iniNetworkInterface, ipNetwork + shostAddr, currentSubnetMask, currentGateway);
            }
        }

        public static void ChangeIPPerInstanceRestoreIP()
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Reverting IP settings back to normal");

            UserPrompts.WaitForOk("Reverting IP settings back to normal. You may receive another prompt to action it.");
            if (isDHCPenabled)
            {
                SetDHCP(iniNetworkInterface);
            }
            else
            {
                SetIP(iniNetworkInterface, currentIPaddress, currentSubnetMask, currentGateway);
            }
        }

        public static void ChangeIPPerInstanceAltCreateAdapter(PlayerInfo player)
        {
            var handlerInstance = GenericGameHandler.Instance;
            List<string> adptrs = Network.GetNetAdapters();

            Process p = new Process();

            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = Globals.NucleusInstallRoot + "\\utils\\devcon";
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.Arguments = "/C devcon install %WINDIR%\\Inf\\Netloop.inf *MSLOOP";

            p.Start();

            string stdOut = p.StandardOutput.ReadToEnd();
            handlerInstance.Log("ChangeIPPerInstanceAlt install output " + stdOut);

            p.WaitForExit();

            List<string> newAdptrs = Network.GetNetAdapters();

            foreach (string adptr in newAdptrs)
            {
                if (!adptrs.Contains(adptr))
                {
                    player.Adapter = adptr;
                    break;
                }
            }

            if (player.Adapter == null)
            {
                handlerInstance.Log("Could not find new network adapter made for this player.");
            }

            Thread.Sleep(3000);

            ChangeIPPerInstance(player.PlayerID, player.Adapter);
        }

        public static void ChangeIPPerInstanceAltDeleteAdapter()
        {
            var handlerInstance = GenericGameHandler.Instance;

            handlerInstance.Log("Uninstalling loopback adapters");
            Process p = new Process();
            //string devconPath = Path.Combine(Directory.GetCurrentDirectory(), "utils\\devcon\\devcon.exe");
            p.StartInfo.FileName = "cmd.exe";
            p.StartInfo.WorkingDirectory = Globals.NucleusInstallRoot + "\\utils\\devcon";
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.Arguments = "/C devcon remove \"@ROOT\\NET\\*\"";

            p.Start();

            string stdOut = p.StandardOutput.ReadToEnd();
            handlerInstance.Log("ChangeIPPerInstanceAlt remove output \n" + stdOut);

            p.WaitForExit();
        }

        public static string GetLocalIP()
        {
            var dadada = GetBestInterface(BitConverter.ToUInt32(IPAddress.Parse("8.8.8.8").GetAddressBytes(), 0), out uint interfaceIndex);
            IPAddress xxxd = NetworkInterface.GetAllNetworkInterfaces()
            .Where(netInterface => netInterface.GetIPProperties().GetIPv4Properties().Index == BitConverter.ToInt32(BitConverter.GetBytes(interfaceIndex), 0)).First().GetIPProperties().UnicastAddresses.Where(ipAdd => ipAdd.Address.AddressFamily == AddressFamily.InterNetwork).First().Address;

            return xxxd.ToString();
        }

        public static bool SetIP(string networkInterfaceName, string ipAddress, string subnetMask, string gateway = null)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == networkInterfaceName);
            var ipProperties = networkInterface.GetIPProperties();
            var ipInfo = ipProperties.UnicastAddresses.FirstOrDefault(ip => ip.Address.AddressFamily == AddressFamily.InterNetwork);
            var currentIPaddress = ipInfo.Address.ToString();
            var currentSubnetMask = ipInfo.IPv4Mask.ToString();
            var isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;

            if (!isDHCPenabled && currentIPaddress == ipAddress && currentSubnetMask == subnetMask)
            {
                return true;
            }

            Process netsh = new Process();
            netsh.StartInfo.FileName = "netsh";
            netsh.StartInfo.RedirectStandardInput = true;
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.Verb = "runas";
            netsh.Start();

            netsh.StandardInput.WriteLine($"interface ip set address \"{networkInterfaceName}\" static {ipAddress} {subnetMask} " + (string.IsNullOrWhiteSpace(gateway) ? "" : $"{gateway} 1"));

            if (dnsAddresses.Count > 0)
            {
                for (int i = 0; i < dnsAddresses.Count; i++)
                {
                    if (i == 0)
                    {
                        netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} static {dnsAddresses[i]} primary");
                    }
                    else
                    {
                        netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} static {dnsAddresses[i]}");
                    }
                }
            }

            netsh.StandardInput.Flush();
            netsh.StandardInput.Close();
            netsh.WaitForExit();
            var successful = netsh.ExitCode == 0;
            netsh.Dispose();
            return successful;
        }

        public static bool SetDHCP(string networkInterfaceName)
        {
            var networkInterface = NetworkInterface.GetAllNetworkInterfaces().FirstOrDefault(nw => nw.Name == networkInterfaceName);
            var ipProperties = networkInterface.GetIPProperties();
            var isDHCPenabled = ipProperties.GetIPv4Properties().IsDhcpEnabled;

            if (isDHCPenabled && ipProperties.DnsAddresses[0].ToString() == dnsAddresses[0])
            {
                return true;    // no change necessary
            }

            Process netsh = new Process();
            netsh.StartInfo.FileName = "netsh";
            netsh.StartInfo.RedirectStandardInput = true;
            //netsh.StartInfo.RedirectStandardOutput = true;
            //cmd.StartInfo.CreateNoWindow = true;
            netsh.StartInfo.UseShellExecute = false;
            netsh.StartInfo.Verb = "runas";
            netsh.Start();

            netsh.StandardInput.WriteLine($"interface ip set address \"{networkInterfaceName}\" dhcp");
            netsh.StandardInput.WriteLine($"interface ip set dnsservers {networkInterfaceName} dhcp");

            netsh.StandardInput.Flush();
            netsh.StandardInput.Close();
            netsh.WaitForExit();
            var successful = netsh.ExitCode == 0;
            netsh.Dispose();
            return successful;
        }

        public static List<string> GetNetAdapters()
        {
            List<String> values = new List<String>();
            foreach (NetworkInterface nic in NetworkInterface.GetAllNetworkInterfaces())
            {
                values.Add(nic.Name);
            }
            return values;
        }


    }
}
