using System;

namespace Nucleus.Gaming
{
    /// <summary>
    /// Where the SplitScreen.Me site lives: the hub pages (<c>/hub/...</c>) and the public API v3 (<c>/api/v3/</c>).
    /// <see cref="DefaultUrl"/> unless Settings.ini <c>[Dev] HubUrl</c> names another site (a local one during development).
    /// The services, the legacy update check (<see cref="Coop.Generic.Hub"/>) and the web UI (through <c>app.getInfo</c>)
    /// all use this value.
    /// </summary>
    public static class HubSite
    {
        /// <summary>The public site.</summary>
        public const string DefaultUrl = "https://v3.splitscreen.me";

        /// <summary>Settings.ini section of the override.</summary>
        public const string SettingSection = "Dev";

        /// <summary>Settings.ini key of the override: an absolute http(s) URL, empty for <see cref="DefaultUrl"/>.</summary>
        public const string SettingKey = "HubUrl";

        private const string ApiPath = "/api/v3";

        private static readonly Lazy<string> url = new Lazy<string>(ReadSetting);

        /// <summary>Site root without a trailing slash, for example <c>https://v3.splitscreen.me</c>. Read once.</summary>
        public static string Url => url.Value;

        /// <summary>API root with a trailing slash: <c>&lt;Url&gt;/api/v3/</c>.</summary>
        public static string ApiBase => ApiUrl(Url);

        /// <summary>Whether Settings.ini overrides the default site.</summary>
        public static bool IsCustom => !string.Equals(Url, DefaultUrl, StringComparison.Ordinal);

        /// <summary>API root (trailing slash) of a site root returned by <see cref="Normalize"/>.</summary>
        public static string ApiUrl(string siteUrl) => siteUrl + ApiPath + "/";

        /// <summary>
        /// The site root of a setting value: an absolute http or https URL, without query, fragment or trailing slash
        /// (a pasted <c>.../api/v3</c> is cut back to the site). Null when the value is empty or not such a URL.
        /// </summary>
        public static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            Uri uri;
            if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out uri)
                || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp)
                || string.IsNullOrEmpty(uri.Host)
                || !string.IsNullOrEmpty(uri.UserInfo))
            {
                return null;
            }

            string path = uri.AbsolutePath.TrimEnd('/');
            if (path.EndsWith(ApiPath, StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - ApiPath.Length).TrimEnd('/');
            }

            return uri.GetLeftPart(UriPartial.Authority) + path;
        }

        private static string ReadSetting()
        {
            string value = null;
            try
            {
                value = Globals.ini.IniReadValue(SettingSection, SettingKey);
            }
            catch (Exception)
            {
                // No readable Settings.ini: the default site.
            }

            return Normalize(value) ?? DefaultUrl;
        }
    }
}
