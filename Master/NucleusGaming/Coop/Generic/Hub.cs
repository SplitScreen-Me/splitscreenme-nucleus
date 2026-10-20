using System;
using System.Threading.Tasks;

namespace Nucleus.Gaming.Coop.Generic
{
    public class Hub
    {
        public Action<bool> NotifyConnected;

        private bool updateAvailable;
        public bool UpdateAvailable
        {
            get => updateAvailable;
            private set
            {
                updateAvailable = value;
            }      
        }

        public async Task CheckForHandlerUpdate(bool fetch)
        {
            if(!fetch)
            {
                return;
            }

            if (!Http_Client.Http_Client.HubConnected)
            {
                return;
            }

            bool available = await CheckUpdateAvailable();
            UpdateAvailable = available;
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

        private async Task<bool> CheckUpdateAvailable()
        {
            if (Handler.Version < 0 || string.IsNullOrEmpty(Handler.Id)) 
            { 
                return false;
            }

            string id = Handler.Id;
            int newVersion = -1;

            string url = "https://hub.splitscreen.me/api/v1/" + "handler/" + id;

            var result = await Http_Client.Http_Client.GetJsonPropertyAsync(url, "Handlers");

            if(result == null)
            {
                return false;
            }

            newVersion = int.TryParse(result[0]["currentVersion"].ToString(), out int _v) ? _v : -1;

            return newVersion > Handler.Version;
        }
    }
}
