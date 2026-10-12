using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Nucleus.Bridge.Contracts;
using Nucleus.Gaming.Coop;

namespace Nucleus.Coop.Services.Setup
{
    /// <summary>
    /// Handler option values (pure). A choice option stores the original <see cref="GameOption.List"/> item object (string,
    /// double, ExpandoObject) or enum value, exactly as the WinForms controls did, so handler scripts see the same types.
    /// A loaded profile stores every value as text: <see cref="Resolve"/> maps it back to the list item.
    /// </summary>
    internal static class OptionsCodec
    {
        /// <summary>
        /// Kind of an option, with the rules of PlayerOptionsControl: an enum value or a non-empty list is a choice, an
        /// empty list is a text, anything else (scalar values, which crash the WinForms options step) is unsupported.
        /// </summary>
        public static OptionKind KindOf(GameOption option)
        {
            if (option.Value is Enum)
            {
                return OptionKind.Choice;
            }

            if (option.List != null)
            {
                return option.List.Count > 0 ? OptionKind.Choice : OptionKind.Text;
            }

            return OptionKind.Unsupported;
        }

        /// <summary>Values of a choice option: the list items, or the enum values.</summary>
        public static IList Choices(GameOption option)
        {
            if (option.Value is Enum e)
            {
                return Enum.GetValues(e.GetType());
            }

            return option.List ?? new object[0];
        }

        /// <summary>Text shown for a choice: the item's "Name" for dictionary items (custom step cards), else its text.</summary>
        public static string Label(object item)
        {
            if (item is IDictionary<string, object> dict)
            {
                return dict.TryGetValue("Name", out object name) ? name?.ToString() ?? "" : "";
            }

            return Convert.ToString(item, CultureInfo.CurrentCulture) ?? "";
        }

        /// <summary>A text property of a dictionary item ("Details", "ImageUrl"), or null.</summary>
        public static string? DictionaryText(object item, string key)
        {
            if (item is IDictionary<string, object> dict && dict.TryGetValue(key, out object value) && value != null)
            {
                string text = value.ToString();
                return text.Length > 0 ? text : null;
            }

            return null;
        }

        /// <summary>
        /// Index of <paramref name="stored"/> in the choices of <paramref name="option"/>, or null. Tries the same object,
        /// equal values, equal text (numbers compared as numbers), the "Name" of dictionary items (also from the JSON text
        /// a profile saves), and enum names.
        /// </summary>
        public static int? Resolve(GameOption option, object? stored)
        {
            if (stored == null)
            {
                return null;
            }

            IList choices = Choices(option);

            if (choices.Count == 0)
            {
                return null;
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (ReferenceEquals(choices[i], stored))
                {
                    return i;
                }
            }

            for (int i = 0; i < choices.Count; i++)
            {
                if (Equals(choices[i], stored))
                {
                    return i;
                }
            }

            if (option.Value is Enum enumValue && stored is string enumText && enumText.Length > 0)
            {
                try
                {
                    object parsed = Enum.Parse(enumValue.GetType(), enumText, true);

                    for (int i = 0; i < choices.Count; i++)
                    {
                        if (Equals(choices[i], parsed))
                        {
                            return i;
                        }
                    }
                }
                catch (ArgumentException)
                {
                }

                return null;
            }

            string? storedName = NameOf(stored);

            if (storedName != null)
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    if (choices[i] is IDictionary<string, object> && Label(choices[i]) == storedName)
                    {
                        return i;
                    }
                }
            }

            string storedText = Convert.ToString(stored, CultureInfo.CurrentCulture) ?? "";

            for (int i = 0; i < choices.Count; i++)
            {
                if (choices[i] is IDictionary<string, object>)
                {
                    continue;
                }

                if (Convert.ToString(choices[i], CultureInfo.CurrentCulture) == storedText)
                {
                    return i;
                }
            }

            if (TryNumber(storedText, out double storedNumber))
            {
                for (int i = 0; i < choices.Count; i++)
                {
                    if (IsNumber(choices[i]) && Convert.ToDouble(choices[i], CultureInfo.InvariantCulture) == storedNumber)
                    {
                        return i;
                    }
                }
            }

            return null;
        }

        /// <summary>The "Name" of a dictionary item, or of the JSON text of one (as saved in profiles).</summary>
        private static string? NameOf(object stored)
        {
            if (stored is IDictionary<string, object> dict)
            {
                return dict.TryGetValue("Name", out object name) ? name?.ToString() : null;
            }

            if (stored is string text && text.TrimStart().StartsWith("{"))
            {
                try
                {
                    JObject json = JObject.Parse(text);
                    return (string?)json["Name"];
                }
                catch (Exception)
                {
                    return null;
                }
            }

            return null;
        }

        private static bool IsNumber(object value)
        {
            return value is double || value is float || value is int || value is long || value is decimal || value is short || value is byte;
        }

        private static bool TryNumber(string text, out double number)
        {
            return double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out number) ||
                   double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out number);
        }

        /// <summary>
        /// The value a visible option starts with: the first list item for choices (what the options step selected when
        /// it was shown), the enum default, "" for texts, the handler value otherwise.
        /// </summary>
        public static object DefaultValue(GameOption option)
        {
            switch (KindOf(option))
            {
                case OptionKind.Choice:
                    return option.Value is Enum ? option.Value : option.List[0];
                case OptionKind.Text:
                    return option.Value ?? "";
                default:
                    return option.Value ?? "";
            }
        }

        /// <summary>
        /// The value to store for <paramref name="stored"/>: the matching choice object, the text, or the default when
        /// nothing matches. Options driven by a custom step keep "" until an item is picked (no default card).
        /// </summary>
        /// <param name="matched">True when the stored value was recognised (a choice found, or a text kept).</param>
        public static object Normalize(GameOption option, object? stored, out bool matched)
        {
            matched = false;

            if (option.Hidden)
            {
                int? item = ResolveStepItem(option, stored);
                matched = item.HasValue;
                return item.HasValue ? option.List[item.Value] : "";
            }

            switch (KindOf(option))
            {
                case OptionKind.Choice:
                    {
                        int? index = Resolve(option, stored);
                        matched = index.HasValue;
                        return index.HasValue ? Choices(option)[index.Value] : DefaultValue(option);
                    }
                case OptionKind.Text:
                    matched = stored != null;
                    return stored?.ToString() ?? "";
                default:
                    matched = stored != null;
                    return stored ?? DefaultValue(option);
            }
        }

        /// <summary>Index of the selected item of a custom step option: only dictionary items count (the cards).</summary>
        public static int? ResolveStepItem(GameOption option, object? stored)
        {
            if (option.List == null || stored == null || (stored is string s && s.Length == 0))
            {
                return null;
            }

            int? index = Resolve(option, stored);

            if (index.HasValue && option.List[index.Value] is IDictionary<string, object>)
            {
                return index;
            }

            return null;
        }

        /// <summary>
        /// Merge rule for a profile load or a reset: start from the handler defaults and overlay the stored values, so an
        /// option added to the handler after the profile was saved still exists. Returns the keys of custom step options
        /// whose card was restored.
        /// </summary>
        public static HashSet<string> Merge(IList<GameOption> options, IDictionary<string, object> values)
        {
            HashSet<string> restoredSteps = new HashSet<string>();

            foreach (GameOption option in options)
            {
                values.TryGetValue(option.Key, out object stored);
                object value = Normalize(option, stored, out bool matched);
                values[option.Key] = value;

                if (option.Hidden && matched)
                {
                    restoredSteps.Add(option.Key);
                }
            }

            return restoredSteps;
        }
    }
}
