using System;
using System.Collections.Generic;
using System.Linq;

namespace Nucleus.Gaming.Coop.InputManagement.Gamepads
{
    /// <summary>A DirectInput device seen while an XInput pad reports a button press.</summary>
    public struct DInputCandidate
    {
        public D_Joystick Joystick;

        /// <summary>A button or the hat (D-pad) is pressed on this device right now.</summary>
        public bool Pressed;

        public DInputCandidate(D_Joystick joystick, bool pressed)
        {
            Joystick = joystick;
            Pressed = pressed;
        }
    }

    /// <summary>
    /// What the API that saw the press knows about the pad, to tell its DirectInput view apart from the others. XInput
    /// knows nothing (<see cref="GamepadIdentity.XInputPad"/>); SDL gives the device path and the USB ids.
    /// </summary>
    public struct PadHint
    {
        /// <summary>Device path (SDL: HIDAPI or raw input path, the same as the DirectInput interface path), or null.</summary>
        public string Path;

        /// <summary>USB vendor and product ids, 0 when unknown.</summary>
        public ushort VendorId;
        public ushort ProductId;

        /// <summary>The pad is an XInput device: when several devices saw the press, its "IG_" view is the likely one.</summary>
        public bool PreferXInputViews;
    }

    /// <summary>How a button press identified (revealed) an XInput or SDL pad.</summary>
    public enum IdentifyResult
    {
        /// <summary>The press matched exactly one DirectInput device: the pad got its GUID and HID path.</summary>
        Matched,

        /// <summary>No unique DirectInput match, and the handler only needs the XInput slot: revealed with the slot GUID.</summary>
        SlotFallback,

        /// <summary>No unique DirectInput match, and the handler needs the DirectInput identity: the pad stays hidden.</summary>
        Failed,
    }

    /// <summary>
    /// Rules that turn a button press on an XInput or SDL pad into an identified (revealed) setup device. Without the
    /// "identify gamepads by slot" mode, the pad used to be revealed only when exactly one DirectInput device reported a
    /// button at the same moment, so a pad was never usable when DirectInput saw the press twice (DS4Windows/ViGEm,
    /// Steam Input, a stuck button) or not at all (D-pad, hidden device). Pure: unit tested without devices.
    /// </summary>
    public static class GamepadIdentity
    {
        /// <summary>Hint of an XInput pad: no path or ids, and its DirectInput view is an "IG_" one.</summary>
        public static readonly PadHint XInputPad = new PadHint { PreferXInputViews = true };

        /// <summary>The GUID given to a pad identified by its API slot (same scheme as the slot mode).</summary>
        public static Guid SlotGuid(int gamepadId)
        {
            int n = gamepadId + 1;
            string guid = n >= 10 ? $"00000000-0000-0000-0000-2000000000{n}" : $"00000000-0000-0000-0000-20000000000{n}";
            return new Guid(guid);
        }

        /// <summary>Whether a DirectInput state counts as a press: any button, or a hat (D-pad) that is not centered.</summary>
        public static bool IsPressed(bool[] buttons, int[] pointOfViewControllers)
        {
            if (buttons != null && buttons.Any(b => b))
            {
                return true;
            }

            //A centered hat reports -1 (some drivers only set the low word: 0xFFFF).
            return pointOfViewControllers != null && pointOfViewControllers.Length > 0 && (pointOfViewControllers[0] & 0xFFFF) != 0xFFFF;
        }

        /// <summary>The DirectInput view of an XInput pad (the XInput HID-compatible collection) has "IG_" in its path.</summary>
        public static bool IsXInputHidPath(string interfacePath)
        {
            return !string.IsNullOrEmpty(interfacePath) && interfacePath.IndexOf("IG_", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        /// <summary>
        /// Whether the handler needs the DirectInput GUID or HID path of its XInput pads (x360ce, device reordering, single
        /// device file, XInput reroute, ProtoInput DirectInput hook, the Nucleus DirectInput hook, or a script that reads them).
        /// </summary>
        public static bool NeedsDInputIdentity(GenericGameInfo game)
        {
            if (game == null)
            {
                return true;
            }

            return NeedsDInputIdentity(game.UseX360ce, game.UseDevReorder, game.CreateSingleDeviceFile, game.Hook?.XInputReroute == true,
                game.ProtoInput?.DinputDeviceHook == true, game.Hook?.DInputEnabled == true, game.ScriptReadsGamepadIdentity);
        }

        public static bool NeedsDInputIdentity(bool useX360ce, bool useDevReorder, bool createSingleDeviceFile, bool xinputReroute,
            bool dinputDeviceHook, bool dinputEnabled, bool scriptReadsIdentity)
        {
            return useX360ce || useDevReorder || createSingleDeviceFile || xinputReroute || dinputDeviceHook || dinputEnabled || scriptReadsIdentity;
        }

        /// <summary>
        /// Whether two device paths name the same device interface: case and the "\\?\" / "\??\" prefix differ between
        /// APIs (SetupAPI and DirectInput write "\\?\hid#vid_…", raw input "\\?\HID#VID_…").
        /// </summary>
        public static bool SamePath(string a, string b)
        {
            string x = StripDevicePrefix(a);
            string y = StripDevicePrefix(b);
            return x.Length > 0 && string.Equals(x, y, StringComparison.OrdinalIgnoreCase);
        }

        private static string StripDevicePrefix(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return "";
            }

            path = path.Trim();

            foreach (string prefix in new[] { @"\\?\", @"\??\", @"\\.\" })
            {
                if (path.StartsWith(prefix, StringComparison.Ordinal))
                {
                    return path.Substring(prefix.Length);
                }
            }

            return path;
        }

        /// <summary>
        /// USB vendor and product ids of a DirectInput product GUID. HID devices have "{PIDVID-0000-0000-0000-504944564944}"
        /// (the last six bytes spell "PIDVID"); false for other GUIDs.
        /// </summary>
        public static bool TryUsbIdsOf(Guid productGuid, out ushort vendorId, out ushort productId)
        {
            byte[] bytes = productGuid.ToByteArray();//Data1 first, little endian: VID low word, PID high word
            vendorId = (ushort)(bytes[0] | bytes[1] << 8);
            productId = (ushort)(bytes[2] | bytes[3] << 8);

            return bytes[10] == (byte)'P' && bytes[11] == (byte)'I' && bytes[12] == (byte)'D' &&
                   bytes[13] == (byte)'V' && bytes[14] == (byte)'I' && bytes[15] == (byte)'D';
        }

        /// <summary>The DirectInput device whose interface path is <paramref name="path"/> and that no other pad uses, or null.</summary>
        public static D_Joystick FindByPath(PlayerInfo player, IEnumerable<PlayerInfo> devices, IEnumerable<D_Joystick> joysticks, string path)
        {
            if (string.IsNullOrEmpty(path) || joysticks == null)
            {
                return null;
            }

            List<D_Joystick> same = joysticks.Where(j => j != null && SamePath(j.InterfacePath, path)).ToList();

            //One device interface, one DirectInput device: two would mean a stale list, so don't guess.
            if (same.Count != 1 || IsTakenByAnother(player, devices ?? Enumerable.Empty<PlayerInfo>(), same[0]))
            {
                return null;
            }

            return same[0];
        }

        /// <summary>
        /// The DirectInput device that reported the press of <paramref name="player"/>, or null when it isn't unique.
        /// Devices already identified as another pad are skipped; when several remain, the XInput HID views win.
        /// </summary>
        public static D_Joystick PickDInputMatch(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates)
        {
            return PickDInputMatch(player, devices, candidates, XInputPad);
        }

        /// <summary>
        /// The DirectInput device of <paramref name="player"/>, or null when it isn't unique. In order: the device with the
        /// pad's path (pressed or not); the only pressed device (a device with other USB ids than the pad is another pad);
        /// among several, the only one with the pad's USB ids; then, for an XInput device, the only "IG_" view. Devices
        /// already identified as another pad are skipped.
        /// </summary>
        public static D_Joystick PickDInputMatch(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates, PadHint hint)
        {
            devices = devices ?? Enumerable.Empty<PlayerInfo>();
            candidates = candidates ?? new List<DInputCandidate>();

            D_Joystick byPath = FindByPath(player, devices, candidates.Select(c => c.Joystick), hint.Path);

            if (byPath != null)
            {
                return byPath;
            }

            bool idsKnown = hint.VendorId != 0 || hint.ProductId != 0;

            //Two players pressing at once, or a pad DirectInput can't see while another one is pressed: the USB ids tell them apart.
            List<D_Joystick> pressed = candidates
                .Where(c => c.Pressed && c.Joystick != null && !IsTakenByAnother(player, devices, c.Joystick))
                .Select(c => c.Joystick)
                .Where(j => !idsKnown || UsbIdsMatch(j, hint) != false)
                .ToList();

            if (pressed.Count == 1)
            {
                return pressed[0];
            }

            if (pressed.Count > 1)
            {
                if (idsKnown)
                {
                    List<D_Joystick> sameIds = pressed.Where(j => UsbIdsMatch(j, hint) == true).ToList();

                    if (sameIds.Count == 1)
                    {
                        return sameIds[0];
                    }

                    if (sameIds.Count > 1)
                    {
                        pressed = sameIds;
                    }
                }

                if (hint.PreferXInputViews)
                {
                    List<D_Joystick> xinputViews = pressed.Where(j => IsXInputHidPath(j.InterfacePath)).ToList();

                    if (xinputViews.Count == 1)
                    {
                        return xinputViews[0];
                    }
                }
            }

            return null;
        }

        /// <summary>Whether a DirectInput device has the pad's USB ids; null when its product GUID doesn't carry them.</summary>
        private static bool? UsbIdsMatch(D_Joystick joystick, PadHint hint)
        {
            if (!TryUsbIdsOf(joystick.ProductGuid, out ushort vid, out ushort pid))
            {
                return null;
            }

            return vid == hint.VendorId && pid == hint.ProductId;
        }

        /// <summary>
        /// Identifies an XInput pad after a button press (XInput reported buttons). Sets the identity and <see cref="PlayerInfo.IsInputUsed"/>
        /// on success, <see cref="PlayerInfo.IdentificationFailedUtc"/> on failure. The caller rumbles the pad on success.
        /// </summary>
        public static IdentifyResult Identify(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates, bool needsDInputIdentity, DateTime nowUtc)
        {
            return Identify(player, devices, candidates, XInputPad, needsDInputIdentity, nowUtc);
        }

        /// <summary>
        /// Identifies a pad after a button press seen by its API (XInput, SDL). Without a DirectInput match, a handler that
        /// only needs the API index reveals the pad with <see cref="SlotGuid"/> of <see cref="PlayerInfo.GamepadId"/>.
        /// </summary>
        public static IdentifyResult Identify(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates, PadHint hint, bool needsDInputIdentity, DateTime nowUtc)
        {
            D_Joystick match = PickDInputMatch(player, devices, candidates, hint);

            if (match != null)
            {
                Bind(player, match);
                player.IsInputUsed = true;
                player.IdentificationFailedUtc = DateTime.MinValue;
                return IdentifyResult.Matched;
            }

            if (needsDInputIdentity)
            {
                player.IdentificationFailedUtc = nowUtc;
                return IdentifyResult.Failed;
            }

            player.GamepadGuid = SlotGuid(player.GamepadId);
            player.IsInputUsed = true;
            player.IdentificationFailedUtc = DateTime.MinValue;
            return IdentifyResult.SlotFallback;
        }

        /// <summary>Copies the identity of a DirectInput device to a pad (GUIDs, name, HID path).</summary>
        public static void Bind(PlayerInfo player, D_Joystick joystick)
        {
            player.DInputJoystick = joystick;
            player.GamepadGuid = joystick.InstanceGuid;
            player.GamepadProductGuid = joystick.ProductGuid;
            player.GamepadName = joystick.InstanceName;
            player.RawHID = joystick.InterfacePath;
            player.HIDDeviceID = new string[] { HidIdOf(joystick.InterfacePath), "" };
        }

        /// <summary>"\\?\hid#vid_..&pid_..#...#{guid}" → "HID\VID_..&PID_..\..." (the form the device reordering tools use).</summary>
        public static string HidIdOf(string interfacePath)
        {
            if (string.IsNullOrEmpty(interfacePath))
            {
                return "";
            }

            int start = interfacePath.IndexOf("hid#", StringComparison.OrdinalIgnoreCase);
            int end = interfacePath.LastIndexOf("#{", StringComparison.Ordinal);

            if (start < 0 || end <= start)
            {
                return interfacePath.ToUpperInvariant();
            }

            return interfacePath.Substring(start, end - start).Replace('#', '\\').ToUpperInvariant();
        }

        private static bool IsTakenByAnother(PlayerInfo player, IEnumerable<PlayerInfo> devices, D_Joystick joystick)
        {
            return devices.Any(d => d != player && d.IsController && !d.IsFake &&
                                    (d.DInputJoystick == joystick || (d.GamepadGuid != Guid.Empty && d.GamepadGuid == joystick.InstanceGuid)));
        }
    }
}
