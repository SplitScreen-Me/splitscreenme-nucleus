using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Coop.InputManagement.Gamepads;
using Nucleus.Gaming.Localization;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// Device ids and labels (pure). The id is PlayerInfo.DeviceKey, set where the device is detected
    /// ("dinput:{guid}", "xinput:{slot}", "sdl:{index}", "kb:single", "rawkb:{hid}", "rawmouse:{hid}", "virtual-mouse",
    /// "virtual-kb"). It stays the same while the device stays plugged in, and raw keyboards/mice keep it across rescans.
    /// </summary>
    internal static class DeviceIds
    {
        /// <summary>Contract kind of a Nucleus input type (KBM: a raw keyboard and mouse merged by a profile or by the launch).</summary>
        public static DeviceKind KindOf(InputType type)
        {
            switch (type)
            {
                case InputType.XInput: return DeviceKind.XInput;
                case InputType.DInput: return DeviceKind.DInput;
                case InputType.SDL2: return DeviceKind.Sdl;
                case InputType.SingleKB: return DeviceKind.Keyboard;
                case InputType.Mouse: return DeviceKind.RawMouse;
                case InputType.KBM: return DeviceKind.KeyboardMouse;
                default: return DeviceKind.RawKeyboard;
            }
        }

        /// <summary>
        /// Kind of device a saved profile player expects, from the flags a profile stores (IsXInput, IsDInput, IsSDL2,
        /// IsRawMouse, the single keyboard GUID, the two HID names of a merged keyboard and mouse).
        /// </summary>
        public static DeviceKind TemplateKind(bool xinput, bool dinput, bool sdl, bool singleKeyboard, bool rawMouse, int hidCount)
        {
            if (xinput) return DeviceKind.XInput;
            if (dinput) return DeviceKind.DInput;
            if (sdl) return DeviceKind.Sdl;
            if (singleKeyboard) return DeviceKind.Keyboard;
            if (hidCount >= 2) return DeviceKind.KeyboardMouse;
            return rawMouse ? DeviceKind.RawMouse : DeviceKind.RawKeyboard;
        }

        /// <summary>Id for a device created without a key (fallback, same scheme).</summary>
        public static string FallbackKey(InputType type, int apiIndex, Guid guid, string? hid)
        {
            switch (type)
            {
                case InputType.DInput: return "dinput:" + guid.ToString("D");
                case InputType.XInput: return "xinput:" + apiIndex;
                case InputType.SDL2: return "sdl:" + apiIndex;
                case InputType.SingleKB: return "kb:single";
                case InputType.Mouse: return hid == "MouseHandleZero" ? "virtual-mouse" : "rawmouse:" + hid;
                default: return hid == "KeyboardHandleZero" ? "virtual-kb" : "rawkb:" + hid;
            }
        }

        /// <summary>
        /// Makes ids unique in list order: the first device keeps its id, later duplicates get "#2", "#3"... Returns the
        /// new id for each position (the caller stores it so it stays stable).
        /// </summary>
        public static List<string> MakeUnique(IReadOnlyList<string> keys)
        {
            HashSet<string> seen = new HashSet<string>(StringComparer.Ordinal);
            List<string> result = new List<string>(keys.Count);

            foreach (string key in keys)
            {
                string unique = key;

                for (int n = 2; !seen.Add(unique); n++)
                {
                    unique = key + "#" + n;
                }

                result.Add(unique);
            }

            return result;
        }

        private static readonly Regex VidPid = new Regex(@"VID_([0-9A-F]{4}).*?PID_([0-9A-F]{4})", RegexOptions.IgnoreCase);

        /// <summary>Label of a device: the product name when known, else a readable name (in the language in use) from its kind and HID path.</summary>
        public static string Name(InputType type, int apiIndex, string? productName, string? hid)
        {
            if (!string.IsNullOrWhiteSpace(productName))
            {
                return productName!.Trim();
            }

            switch (type)
            {
                case InputType.XInput:
                    return I18n.T("host.setup.devices.xinput", new { number = apiIndex + 1 });
                case InputType.SDL2:
                    return apiIndex >= 0 ? I18n.T("host.setup.devices.controllerNumber", new { number = apiIndex + 1 }) : I18n.T("host.setup.devices.controller");
                case InputType.DInput:
                    return I18n.T("host.setup.devices.dinput", new { number = apiIndex + 1 });
                case InputType.SingleKB:
                    return I18n.T("host.setup.devices.keyboardMouse");
                case InputType.Mouse:
                    return hid == "MouseHandleZero" ? I18n.T("host.setup.devices.virtualMouse") : WithIds(I18n.T("host.setup.devices.mouse"), hid);
                case InputType.KBM:
                    return WithIds(I18n.T("host.setup.devices.keyboardMouse"), hid);
                default:
                    return hid == "KeyboardHandleZero" ? I18n.T("host.setup.devices.virtualKeyboard") : WithIds(I18n.T("host.setup.devices.keyboard"), hid);
            }
        }

        private static string WithIds(string label, string? hid)
        {
            Match match = hid == null ? Match.Empty : VidPid.Match(hid);
            return match.Success
                ? I18n.T("host.setup.devices.withIds", new { device = label, vid = match.Groups[1].Value.ToUpperInvariant(), pid = match.Groups[2].Value.ToUpperInvariant() })
                : label;
        }

        /// <summary>Whether a raw device is one of the virtual entries that catch injected input (device handle 0).</summary>
        public static bool IsVirtual(string? hid) => hid == "MouseHandleZero" || hid == "KeyboardHandleZero";

        private static readonly KeyValuePair<PadButtons, GamepadButton>[] ButtonNames =
        {
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.A, GamepadButton.A),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.B, GamepadButton.B),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.X, GamepadButton.X),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.Y, GamepadButton.Y),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.LeftShoulder, GamepadButton.LeftBumper),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.RightShoulder, GamepadButton.RightBumper),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.Back, GamepadButton.Back),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.Start, GamepadButton.Start),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.LeftThumb, GamepadButton.LeftStick),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.RightThumb, GamepadButton.RightStick),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.DPadUp, GamepadButton.DPadUp),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.DPadDown, GamepadButton.DPadDown),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.DPadLeft, GamepadButton.DPadLeft),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.DPadRight, GamepadButton.DPadRight),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.Guide, GamepadButton.Guide),
            new KeyValuePair<PadButtons, GamepadButton>(PadButtons.Other, GamepadButton.Other),
        };

        /// <summary>Contract names of a set of pad buttons, in a fixed order (face buttons first).</summary>
        public static List<GamepadButton> ButtonsOf(PadButtons buttons)
        {
            List<GamepadButton> names = new List<GamepadButton>();

            foreach (KeyValuePair<PadButtons, GamepadButton> pair in ButtonNames)
            {
                if ((buttons & pair.Key) != 0)
                {
                    names.Add(pair.Value);
                }
            }

            return names;
        }
    }
}
