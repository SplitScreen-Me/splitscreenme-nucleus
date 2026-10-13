using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Nucleus.Gaming.Localization
{
    /// <summary>
    /// Placeholders and tags of translated strings. <c>{name}</c> is replaced by the argument <c>name</c>; numbers are
    /// formatted for the language (grouping, decimal separator), so pass a string to print digits as they are. A
    /// placeholder without an argument stays as it is. Tags (<c>&lt;kbd&gt;End&lt;/kbd&gt;</c>) are left for the web UI,
    /// which renders them; native windows strip them (<see cref="StripTags"/>).
    /// </summary>
    public static class MessageFormat
    {
        private static readonly Regex Placeholder = new Regex(@"\{([A-Za-z_][A-Za-z0-9_]*)\}", RegexOptions.Compiled);
        private static readonly Regex Tag = new Regex(@"</?([A-Za-z][A-Za-z0-9]*)\s*/?>", RegexOptions.Compiled);
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> properties = new ConcurrentDictionary<Type, PropertyInfo[]>();

        /// <summary>Arguments as a dictionary: an <c>IDictionary</c> as it is, or the public properties of an object (an anonymous one, typically).</summary>
        public static IReadOnlyDictionary<string, object> Arguments(object args)
        {
            var result = new Dictionary<string, object>(StringComparer.Ordinal);
            if (args == null)
            {
                return result;
            }

            if (args is IDictionary dictionary)
            {
                foreach (DictionaryEntry entry in dictionary)
                {
                    if (entry.Key != null)
                    {
                        result[entry.Key.ToString()] = entry.Value;
                    }
                }

                return result;
            }

            if (args is IEnumerable<KeyValuePair<string, object>> pairs)
            {
                foreach (KeyValuePair<string, object> pair in pairs)
                {
                    result[pair.Key] = pair.Value;
                }

                return result;
            }

            foreach (PropertyInfo property in properties.GetOrAdd(args.GetType(), t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetIndexParameters().Length == 0).ToArray()))
            {
                result[property.Name] = property.GetValue(args);
            }

            return result;
        }

        /// <summary>Replaces the placeholders of <paramref name="template"/>.</summary>
        public static string Format(string template, IReadOnlyDictionary<string, object> args, CultureInfo culture)
        {
            if (string.IsNullOrEmpty(template) || args == null || args.Count == 0 || template.IndexOf('{') < 0)
            {
                return template;
            }

            return Placeholder.Replace(template, m => args.TryGetValue(m.Groups[1].Value, out object value) ? FormatValue(value, culture) : m.Value);
        }

        /// <summary>A value as text for <paramref name="culture"/>: numbers with grouping, other formattable values with the culture.</summary>
        public static string FormatValue(object value, CultureInfo culture)
        {
            switch (value)
            {
                case null:
                    return "";
                case string s:
                    return s;
                case int _:
                case long _:
                case short _:
                case byte _:
                case uint _:
                case ulong _:
                case ushort _:
                case sbyte _:
                    return ((IFormattable)value).ToString("#,0", culture);
                case double _:
                case float _:
                case decimal _:
                    return ((IFormattable)value).ToString("#,0.###", culture);
                case IFormattable formattable:
                    return formattable.ToString(null, culture);
                default:
                    return value.ToString();
            }
        }

        /// <summary>Names of the placeholders of a string, in order, without duplicates.</summary>
        public static IReadOnlyList<string> PlaceholdersOf(string template) =>
            Placeholder.Matches(template ?? "").Cast<Match>().Select(m => m.Groups[1].Value).Distinct().ToList();

        /// <summary>The text without its tags: "Press &lt;kbd&gt;End&lt;/kbd&gt;" → "Press End", &lt;br/&gt; → a line break. For native windows.</summary>
        public static string StripTags(string text) =>
            string.IsNullOrEmpty(text) || text.IndexOf('<') < 0
                ? text
                : Tag.Replace(text, m => string.Equals(m.Groups[1].Value, "br", StringComparison.OrdinalIgnoreCase) ? "\n" : "");
    }
}
