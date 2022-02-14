using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.IO;
using System.Net;

namespace Nucleus.Gaming.Coop.Generic
{
    public class Hub
    {
        private bool checkedUpdate = false;
        private bool updateAvailable = false;
        private Bitmap bmp;
        private string imgId;
        private string imageUrl;
        public bool IsUpdateAvailable(bool fetch)
        {
            if (fetch && !checkedUpdate)
            {
                checkedUpdate = true;
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

        public bool CheckUpdateAvailable()
        {
            if (Handler.Version < 0)
            {
                return false;
            }

            string id = Handler.Id;
            int newVersion = -1;

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.DefaultConnectionLimit = 9999;

            string resp = Get("https://hub.splitscreen.me/api/v1/"+"handler/"+id);

            if (resp == null)
            {
                return false;
            }
            else if (resp == "{}")
            {
                return false;
            }

            JObject jObject = JsonConvert.DeserializeObject(resp) as JObject;

            if (jObject == null)
            {
                return false;
            }

            JArray array = jObject["Handlers"] as JArray;

            if (array == null)
            {
                return false;
            }
            else if (array.Count != 1)
            {
                return false;
            }

            newVersion = int.TryParse(array[0]["currentVersion"].ToString(), out int _v) ? _v : -1;
            return newVersion > Handler.Version;
        }


        public string GetScreenshots()
        {
            string id = Handler.Id;
            if (id == null)
            {
                return null;
            }
            else if (id == "{}")
            {
                return null;
            }
            string resp = Get($@"https://hub.splitscreen.me/api/v1/screenshots/{id}");

            return resp;
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
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                using (Stream stream = response.GetResponseStream())
                using (StreamReader reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
            catch (Exception)
            {
                //MessageBox.Show(string.Format("{0}: {1}", ex.ToString(), ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
    }
}
