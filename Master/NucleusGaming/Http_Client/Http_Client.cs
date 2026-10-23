using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;

namespace Nucleus.Gaming.Http_Client
{
    public class Http_Client
    {
        public enum HubResponse
        {
            Success,
            Error
        }

        private static CancellationTokenSource cancelToken;

        //Subscribe to OnHubConnectionChange to refresh UI/Backend UserGameInfo/Hub, etc; 
        public static event Action<HubResponse> OnHubConnectionChange;

        public static bool HubConnected
        {
            get;
            private set;
        }

        static readonly HttpClient client = new HttpClient(new HttpClientHandler{
             UseCookies = false,
             Proxy = null,
             AutomaticDecompression = DecompressionMethods.None}){ Timeout = Timeout.InfiniteTimeSpan
        };

        public static void StartPollingHub()//start polling in a loop so we can keep checking hub connection
        {
            ServicePointManager.DnsRefreshTimeout = 0;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.DefaultConnectionLimit = 100;

            cancelToken = new CancellationTokenSource();
            _ = PollLoopAsync("hub.splitscreen.me", 15000);
        }

        public static void StopPolling()
        {
            cancelToken?.Cancel();
        }

        private static async Task PollLoopAsync(string host, int intervalMs)
        {
            bool isSuccess = false;

            while (true)
            {
                try
                {
                    //pause it when a handler is running
                    while (GenericGameHandler.IsRunning)
                    {
                        await Task.Delay(intervalMs);
                        continue;
                    }

                    try
                    {
                        bool isOnline = await CheckInternetAccessAsync(intervalMs);

                        if (!isOnline)
                        {
                            HubConnected = false;
                            OnHubConnectionChange?.Invoke(HubResponse.Error);
                            await Task.Delay(intervalMs);
                            continue;
                        }

                        Ping ping = new Ping();
                        PingReply reply = await ping.SendPingAsync(host);

                        if (reply.Status == IPStatus.Success)
                        {
                            isSuccess = true;
                        }
                        else if (reply.RoundtripTime > intervalMs)
                        {
                            isSuccess = false;
                        }
                        else
                        {
                            isSuccess = false;
                        }

                    }
                    catch (Exception ex)
                    {
                        isSuccess = false;
                    }
                }
                catch
                {
                    isSuccess = false;
                }

                if (HubConnected != isSuccess)
                {
                    HubConnected = isSuccess;
                    OnHubConnectionChange?.Invoke(isSuccess ? HubResponse.Success : HubResponse.Error);
                }

                await Task.Delay(intervalMs);
            }
        }

        public static async Task<bool> CheckInternetAccessAsync(int timeoutMs)
        {
            try
            {
                Ping ping = new Ping();
                PingReply reply = await ping.SendPingAsync("www.google.com");

                if (reply.Status == IPStatus.Success)
                {
                    return true;
                }
                else if (reply.RoundtripTime > timeoutMs)
                {
                    return false;
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public static async Task<bool> DownloadImageAsync(string url, string filePath, int timeoutMs = 15000, CancellationToken externalToken = default)
        {
            using (var timeoutCts = new CancellationTokenSource(timeoutMs))
            using (var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                timeoutCts.Token, externalToken))
            {
                try
                {
                    client.DefaultRequestHeaders.ConnectionClose = true;
                    HttpResponseMessage response = await client.GetAsync(url, linkedCts.Token);
                    response.EnsureSuccessStatusCode();

                    using (Stream input = await response.Content.ReadAsStreamAsync())
                    using (FileStream output = new FileStream(
                        filePath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
                    {
                        await input.CopyToAsync(output);
                    }

                    return true;
                }
                catch
                {
                    return false;
                }          
            }
        }

        /// <summary>
        /// Fetch JSON from a URL and return the object at the given property name.
        /// </summary>
        /// <param name="url">The URL of the JSON endpoint.</param>
        /// <param name="propertyName">The name of the property to extract.</param>
        /// <returns>The JObject of the property, or null if not found.</returns>
        public static async Task<JToken> GetJsonPropertyAsync(string url, string propertyName)
        {
            try
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                string json = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                   
                var jObject = JObject.Parse(json);

                if (jObject.TryGetValue(propertyName, out JToken token))
                {
                    return token;
                }
                    
                return null;
            }
            catch
            {
                return null;
            }      
        }

        public static async Task<JObject> GetJsonObjectAsync(string url)
        {
            try
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                string json = await client.GetStringAsync(url);

                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }
                    
                var jObject = JObject.Parse(json);

                if (jObject != null)
                {
                    return jObject;
                }           

                return null;
            }
            catch 
            {
                return null;
            }     
        }

        public static async Task<string> GetAsync(string url)
        {
            try
            {
                client.DefaultRequestHeaders.ConnectionClose = true;
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                string content = await response.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(content))
                {
                    return null;
                }

                return content;
            }
            catch
            {
                return null;
            }        
        }
    }
}
