using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;

namespace Nucleus.Gaming.Coop.Generic
{
    public class Hub
    {
        private bool updateAvailable = false;

        private static bool connected;
        public static bool Connected
        {
            get => connected;
            set
            {
                connected = value;
            }
        }

        public bool IsUpdateAvailable(bool fetch)
        {
            if (!connected)
            {
                return false;
            }

            if (fetch)
            {
                updateAvailable = CheckUpdateAvailable();
            }

            return updateAvailable;
        }

        public _Maintainer Maintainer = new _Maintainer();
        public _Handler Handler = new _Handler();

        public class _Maintainer
        {
            public string Name = "";
            public string Id = "";
        }

        public class _Handler
        {
            public int Version = -1;
            public string Id = "";
        }

        /// <summary>
        /// Whether the hub has a newer version than <see cref="_Handler.Version"/>: <c>GET /api/v3/handlers/{id}</c> on
        /// <see cref="HubSite"/>, whose <c>version</c> is the current version. False when the hub can't be reached or does
        /// not list the handler (404).
        /// </summary>
        public bool CheckUpdateAvailable()
        {
            if (Handler.Version < 0 || string.IsNullOrEmpty(Handler.Id))
            {
                return false;
            }

            string resp = Get(HubSite.ApiBase + "handlers/" + Uri.EscapeDataString(Handler.Id));

            if (string.IsNullOrEmpty(resp))
            {
                return false;
            }

            JObject jObject;
            try
            {
                jObject = JsonConvert.DeserializeObject(resp) as JObject;
            }
            catch (JsonException)
            {
                return false;
            }

            if (jObject == null || jObject["version"] == null)
            {
                return false;
            }

            int newVersion = int.TryParse(jObject["version"].ToString(), out int _v) ? _v : -1;

            return newVersion > Handler.Version;
        }

        public string Get(string uri)
        {
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);

                request.Timeout = 2500;//lower values make the timeout too short for some "big" handlers (GTAIV)             
                request.Method = "Get";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
