using System;
using System.Collections.Generic;
using System.Linq;
using NAudio.CoreAudioApi;
using Nucleus.Bridge.Contracts;
using Nucleus.Coop.Services.Startup;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// Active audio render endpoints, as ProfileSettings.RefreshAudioList lists them: "Default" first, then one entry
    /// per endpoint (value = endpoint id, label = friendly name). Cached: enumeration goes through COM.
    /// </summary>
    internal sealed class AudioOutputs
    {
        public const string Default = "Default";

        private List<SettingChoice> outputs = new List<SettingChoice> { new SettingChoice { Value = Default, Label = Default } };

        /// <summary>The outputs, "Default" labelled in the language in use.</summary>
        public IReadOnlyList<SettingChoice> Current =>
            outputs.Select(c => c.Value == Default ? new SettingChoice { Value = Default, Label = I18n.T("common.default") } : c).ToList();

        /// <summary>Enumerates the endpoints again (UI thread).</summary>
        public void Refresh()
        {
            List<SettingChoice> list = new List<SettingChoice> { new SettingChoice { Value = Default, Label = Default } };

            try
            {
                using (MMDeviceEnumerator enumerator = new MMDeviceEnumerator())
                {
                    foreach (MMDevice endpoint in enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active))
                    {
                        if (!list.Exists(c => c.Value == endpoint.ID))
                        {
                            list.Add(new SettingChoice { Value = endpoint.ID, Label = endpoint.FriendlyName });
                        }
                    }
                }
            }
            catch (Exception e)
            {
                ServiceLog.Warn("setup: audio endpoints enumeration failed", e);
            }

            outputs = list;
        }

        /// <summary>Whether <paramref name="id"/> is "Default" or a listed endpoint.</summary>
        public bool IsKnown(string id) => id == Default || outputs.Exists(c => c.Value == id);
    }
}
