using Newtonsoft.Json;
using Nucleus.Gaming.Coop.Generic;
using Nucleus.Gaming.Forms;
using Nucleus.Gaming.UI;
using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using static Nucleus.Gaming.Http_Client.Http_Client;

namespace Nucleus.Gaming.Coop
{
    public class UserGameInfo 
    {
        private HandlerInfo handlerInfo;
        private string exePath;
        private string gameGuid = "";

        private string fileFullPath;
        private bool updateAvailable;
        private string handlerVersion;
        private bool requiresUpdateCheck = true;

        [JsonIgnore]
        public Action<UserGameInfo> OnHandlerInfoRefresh;

        [JsonIgnore]
        public Action<bool> NotifyUpdateAvailable;

        [JsonIgnore]
        public HandlerInfo HandlerInfo
        {
            get => handlerInfo;

            set
            {
                if (value == null)
                {
                    handlerInfo = value;
                    OnHandlerInfoRefresh?.Invoke(this);
                    return;
                }

                handlerInfo = value;

                //JsFileHash = genericGameInfo.JsFileHash;
                FileFullPath = handlerInfo.FileFullPath;

                HubResponse hubAvailable = Http_Client.Http_Client.HubConnected ? HubResponse.Success : HubResponse.Error;

                string inputVersion = handlerInfo.GenericGameInfo.Hub.Handler.Version.ToString();

                if (handlerVersion == null)
                {
                    _ = CheckUpdateAsync(hubAvailable);
                }
                else if (handlerVersion != inputVersion)
                {
                    requiresUpdateCheck = true;
                    userCallOnce = false;
                    _ = CheckUpdateAsync(hubAvailable);
                }

                Http_Client.Http_Client.OnHubConnectionChange += async (hubResponse) =>
                {
                    await CheckUpdateAsync(hubResponse);
                };

                handlerVersion = inputVersion;
                OnHandlerInfoRefresh?.Invoke(this);
            }
        }

        //[JsonIgnore]
        //public string JsFileHash
        //{
        //    get;
        //    private set;
        //}

        [JsonIgnore]
        public Bitmap Icon
        {
            get;
            set;
        }

        [JsonIgnore]
        public bool UpdateAvailable
        {
            get => updateAvailable;
            set
            {
                updateAvailable = value;
            }     
        }

        public string GameGuid
        {
            get => gameGuid;
            set => gameGuid = value;
        }

        public string ExePath
        {
            get => exePath;
            set => exePath = value;
        }
   
        public string FileFullPath
        {
            get => fileFullPath;
            set => fileFullPath = value;
        }

        public UserGameInfo()
        {

        }

        public void InitializeDefault(HandlerInfo _handlerInfo, string _exePath)
        {
            HandlerInfo = _handlerInfo;
            gameGuid = _handlerInfo.GUID;
            exePath = _exePath?.Replace("I", "i");
            //JsFileHash = game.JsFileHash;
            fileFullPath = _handlerInfo.FileFullPath;
        }

        public async Task CheckUpdateAsync(HubResponse hubavailable)
        {
            try
            {
                switch (hubavailable)
                {
                    case HubResponse.Error:
                        {
                            NotifyUpdateAvailable?.Invoke(false);
                            break;
                        }
                    case HubResponse.Success:
                        {
                            if (requiresUpdateCheck)
                            {
                                await handlerInfo.GenericGameInfo.Hub.CheckForHandlerUpdate(handlerInfo.GenericGameInfo.MetaInfo.CheckUpdate);
                                UpdateAvailable = handlerInfo.GenericGameInfo.Hub.UpdateAvailable;    
                                requiresUpdateCheck = false;
                            }

                            NotifyUpdateAvailable?.Invoke(UpdateAvailable);

                            break;
                        }      
                }
            }
            catch
            {
                NotifyUpdateAvailable?.Invoke(UpdateAvailable);
            }
        }


        private bool userCallOnce;
        public async Task CheckUpdateAsyncUserCall(HubResponse hubvailable)
        {
            try
            {
                switch (hubvailable)
                {
                    case HubResponse.Error:
                        {
                            NotifyUpdateAvailable?.Invoke(false);
                            break;
                        }
                    case HubResponse.Success:
                        {
                            if (!userCallOnce)
                            {
                                await handlerInfo.GenericGameInfo.Hub.CheckForHandlerUpdate(handlerInfo.GenericGameInfo.MetaInfo.CheckUpdate);
                                UpdateAvailable = handlerInfo.GenericGameInfo.Hub.UpdateAvailable;     
                                userCallOnce = true;
                            }

                            NotifyUpdateAvailable?.Invoke(UpdateAvailable);

                            break;
                        }
                }
            }
            catch
            {
                NotifyUpdateAvailable?.Invoke(UpdateAvailable);
            }
        }
    }
}
