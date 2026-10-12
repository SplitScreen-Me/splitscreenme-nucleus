using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Windows.Forms;
using NAudio.CoreAudioApi;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming.Tools.MonitorsDpiScaling;

namespace Nucleus.Coop.Services.Settings;

/// <summary>
/// Lists built from the PC for choice settings, extracted from the Settings form: network adapters
/// (<c>RefreshCmbNetwork</c>), audio outputs (<c>RefreshAudioList</c>) and Windows merger resolutions
/// (<c>GetAllScreensResolutions</c>). Public so the profile settings can reuse them.
/// </summary>
public static class SystemChoices
{
    /// <summary>"Automatic" plus the name of each adapter that is up with an IPv4 address.</summary>
    public static List<SettingChoice> NetworkAdapters()
    {
        var choices = new List<SettingChoice> { new SettingChoice { Value = "Automatic", Label = "Automatic" } };
        try
        {
            foreach (NetworkInterface adapter in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (adapter.OperationalStatus == OperationalStatus.Up
                    && adapter.GetIPProperties().UnicastAddresses.Any(a => a.Address.AddressFamily == AddressFamily.InterNetwork)
                    && choices.All(c => c.Value != adapter.Name))
                {
                    choices.Add(new SettingChoice { Value = adapter.Name, Label = adapter.Name });
                }
            }
        }
        catch (Exception e)
        {
            ServiceLog.Warn("network adapters could not be listed", e);
        }

        return choices;
    }

    /// <summary>"Default" plus each active playback device: value = endpoint id (what Settings.ini stores), label = name. Duplicate names are dropped, as before.</summary>
    public static List<SettingChoice> AudioOutputs()
    {
        var choices = new List<SettingChoice> { new SettingChoice { Value = "Default", Label = "Default" } };
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            foreach (MMDevice device in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
            {
                if (choices.All(c => c.Label != device.FriendlyName))
                {
                    choices.Add(new SettingChoice { Value = device.ID, Label = device.FriendlyName });
                }
            }
        }
        catch (Exception e)
        {
            ServiceLog.Warn("audio outputs could not be listed", e);
        }

        return choices;
    }

    /// <summary>Name of the default playback device, or null.</summary>
    public static string? DefaultAudioOutput()
    {
        try
        {
            using var enumerator = new MMDeviceEnumerator();
            return enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console)?.FriendlyName;
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Display modes of every screen, no larger than the screen (value <c>1920X1080</c>, the stored format), labelled
    /// with the screens that offer them.
    /// </summary>
    public static List<SettingChoice> MergerResolutions()
    {
        var resolutions = new List<(int Width, int Height, List<string> Screens)>();
        try
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                string screenName = screen.DeviceName.Substring(screen.DeviceName.LastIndexOf('\\') + 1);
                var mode = new MonitorsDpiScaling.DEVMODE();
                for (int i = 0; MonitorsDpiScaling.EnumDisplaySettings(screen.DeviceName, i, ref mode); i++)
                {
                    if (mode.dmPelsWidth <= 0 || mode.dmPelsHeight <= 0 || mode.dmPelsWidth > screen.Bounds.Width || mode.dmPelsHeight > screen.Bounds.Height)
                    {
                        continue;
                    }

                    var existing = resolutions.FirstOrDefault(r => r.Width == mode.dmPelsWidth && r.Height == mode.dmPelsHeight);
                    if (existing.Screens == null)
                    {
                        resolutions.Add((mode.dmPelsWidth, mode.dmPelsHeight, new List<string> { screenName }));
                    }
                    else if (!existing.Screens.Contains(screenName))
                    {
                        existing.Screens.Add(screenName);
                    }
                }
            }
        }
        catch (Exception e)
        {
            ServiceLog.Warn("display modes could not be listed", e);
        }

        return resolutions
            .OrderByDescending(r => r.Width * r.Height)
            .ThenByDescending(r => r.Width)
            .Select(r => new SettingChoice { Value = $"{r.Width}X{r.Height}", Label = $"{r.Width} × {r.Height} ({string.Join(", ", r.Screens)})" })
            .ToList();
    }

    /// <summary>Merger resolution used when none is saved: the primary screen's size (the legacy form wrote it on display).</summary>
    public static string PrimaryScreenResolution()
    {
        Screen? primary = Screen.PrimaryScreen;
        return primary == null ? "" : $"{primary.Bounds.Width}X{primary.Bounds.Height}";
    }
}
