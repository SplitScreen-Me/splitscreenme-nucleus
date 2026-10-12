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

    /// <summary>How a button press identified (revealed) an XInput pad.</summary>
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
    /// Rules that turn a button press on an XInput pad into an identified (revealed) setup device. Without the
    /// "identify gamepads by slot" mode, the pad used to be revealed only when exactly one DirectInput device reported a
    /// button at the same moment, so a pad was never usable when DirectInput saw the press twice (DS4Windows/ViGEm,
    /// Steam Input, a stuck button) or not at all (D-pad, hidden device). Pure: unit tested without devices.
    /// </summary>
    public static class GamepadIdentity
    {
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
        /// The DirectInput device that reported the press of <paramref name="player"/>, or null when it isn't unique.
        /// Devices already identified as another pad are skipped; when several remain, the XInput HID views win.
        /// </summary>
        public static D_Joystick PickDInputMatch(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates)
        {
            List<D_Joystick> pressed = candidates
                .Where(c => c.Pressed && c.Joystick != null && !IsTakenByAnother(player, devices, c.Joystick))
                .Select(c => c.Joystick)
                .ToList();

            if (pressed.Count == 1)
            {
                return pressed[0];
            }

            if (pressed.Count > 1)
            {
                List<D_Joystick> xinputViews = pressed.Where(j => IsXInputHidPath(j.InterfacePath)).ToList();

                if (xinputViews.Count == 1)
                {
                    return xinputViews[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Identifies a pad after a button press (XInput reported buttons). Sets the identity and <see cref="PlayerInfo.IsInputUsed"/>
        /// on success, <see cref="PlayerInfo.IdentificationFailedUtc"/> on failure. The caller rumbles the pad on success.
        /// </summary>
        public static IdentifyResult Identify(PlayerInfo player, IEnumerable<PlayerInfo> devices, IReadOnlyList<DInputCandidate> candidates, bool needsDInputIdentity, DateTime nowUtc)
        {
            D_Joystick match = PickDInputMatch(player, devices ?? Enumerable.Empty<PlayerInfo>(), candidates ?? new List<DInputCandidate>());

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
