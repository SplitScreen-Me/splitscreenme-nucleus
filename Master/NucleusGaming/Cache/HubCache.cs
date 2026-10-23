using Ionic.Zip;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Nucleus.Gaming.Cache
{
    public static class HubCache
    {
        private static string cacheFile = Path.Combine(Application.StartupPath, $"webview\\cache\\hubcache");
        private static string thumbnailFolder = Path.Combine(Application.StartupPath, $"webview\\cache\\thumbnails\\");
        private const string api = "https://hub.splitscreen.me/api/v1/";

        private static JObject cacheObject;
        private static JArray handlersArray;
        public static int CachedHandlersCount => handlersArray.Count;
        //private static JArray searchHandlers = new JArray();

        private static List<Bitmap> thumbnails = new List<Bitmap>();

        public static async Task<JArray> InitCache()
        {
            if (!File.Exists(cacheFile) || CheckCacheUpdate())
            {
                return await CreateCache();
            }

            string cache = File.ReadAllText(cacheFile);

            if(string.IsNullOrEmpty(cache))//corrupted?
            {
                return await CreateCache();
            }

            cacheObject = JsonConvert.DeserializeObject(cache) as JObject;

            handlersArray = cacheObject["Handlers"] as JArray;

            return handlersArray;
        }

        private static async Task<JArray> CreateCache()
        {
            string url = api + "allhandlers";
            var result = await Http_Client.Http_Client.GetJsonObjectAsync(url);

            if (result == null)
            {
                return null;
            }

            using (FileStream cachestream = new FileStream(cacheFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(cachestream))
                {
                    string jsonCache = JsonConvert.SerializeObject(result, Formatting.Indented);
                    writer.Write(jsonCache);

                    cacheObject = result;
                    handlersArray = result["Handlers"] as JArray;
                    return handlersArray;
                }
            }
        }

        private static bool CheckCacheUpdate()
        {
            if (!File.Exists(cacheFile))
            {
                return false;
            }

            DateTime lastWritten = File.GetLastWriteTime(cacheFile);
            DateTime now = DateTime.Now;

            if (now.Month == lastWritten.Month)
            {
                if (now.Day >= lastWritten.Day + 7)//chech every 7 days or more
                {
                    return true;
                }
            }

            if (now.Month > lastWritten.Month)
            {
                return true;
            }

            if (now.Month < lastWritten.Month)//new year since last update
            {
                if ((now.Day >= 7 && now.Month == 1) || now.Month > 1)
                {
                    return true;
                }
            }

            return false;
        }

        public static async Task<Handler> SearchById(string handlerId)
        {
            if (cacheObject == null)
            {
               await InitCache();
            }

            for (int i = 0; i < handlersArray.Count; i++)
            {
                string id = handlersArray[i]["_id"].ToString();

                if (id.Equals(handlerId, StringComparison.OrdinalIgnoreCase))
                {
                    Handler handler = new Handler
                    {
                        Id = handlersArray[i]["_id"].ToString(),
                        Owner = handlersArray[i]["owner"].ToString(),
                        OwnerName = handlersArray[i]["ownerName"].ToString(),
                        Description = handlersArray[i]["description"].ToString(),
                        Title = handlersArray[i]["title"].ToString(),
                        GameName = handlersArray[i]["gameName"].ToString(),
                        GameDescription = handlersArray[i]["gameDescription"].ToString(),
                        GameCover = handlersArray[i]["gameCover"].ToString(),
                        GameId = handlersArray[i]["gameId"].ToString(),
                        GameUrl = handlersArray[i]["gameUrl"].ToString(),
                        CreatedAt = handlersArray[i]["createdAt"].ToString(),
                        UpdatedAt = handlersArray[i]["updatedAt"].ToString(),
                        Stars = handlersArray[i]["stars"].ToString(),
                        DownloadCount = handlersArray[i]["downloadCount"].ToString(),
                        Verified = handlersArray[i]["verified"].ToString(),
                        Private = handlersArray[i]["private"].ToString(),
                        CommentCount = handlersArray[i]["commentCount"].ToString(),
                        CurrentVersion = handlersArray[i]["currentVersion"].ToString(),
                        CurrentPackage = handlersArray[i]["currentPackage"].ToString(),
                    };

                    return handler;
                }
            }

            return await SearchByIdOnline(handlerId);
        }

        public static async Task<Handler> SearchByIdOnline(string handlerId)
        {
            if (cacheObject == null)
            {
                await InitCache();
            }

            string url = api + "handler/" + handlerId;
            var result = await Http_Client.Http_Client.GetJsonPropertyAsync(url, "Handlers");

            if (result == null)
            {
                return null;
            }

            Handler handler = null;

            for (int i = 0; i < result.Count(); i++)
            {
                if (handlersArray.All(h => h["_id"] != result[i]))//Update cache from hub resp because those handlers were not in cache
                {
                    cacheObject["Handlers"][0].AddBeforeSelf(result[i]);

                    handler = new Handler
                    {
                        Id = result[i]["_id"].ToString(),
                        Owner = result[i]["owner"].ToString(),
                        OwnerName = result[i]["ownerName"].ToString(),
                        Description = result[i]["description"].ToString(),
                        Title = result[i]["title"].ToString(),
                        GameName = result[i]["gameName"].ToString(),
                        GameDescription = result[i]["gameDescription"].ToString(),
                        GameCover = result[i]["gameCover"].ToString(),
                        GameId = result[i]["gameId"].ToString(),
                        GameUrl = result[i]["gameUrl"].ToString(),
                        CreatedAt = result[i]["createdAt"].ToString(),
                        UpdatedAt = result[i]["updatedAt"].ToString(),
                        Stars = result[i]["stars"].ToString(),
                        DownloadCount = result[i]["downloadCount"].ToString(),
                        Verified = result[i]["verified"].ToString(),
                        Private = result[i]["private"].ToString(),
                        CommentCount = result[i]["commentCount"].ToString(),
                        CurrentVersion = result[i]["currentVersion"].ToString(),
                        CurrentPackage = result[i]["currentPackage"].ToString(),
                    };
                }


                using (FileStream stream = new FileStream(cacheFile, FileMode.Create))
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        string json = JsonConvert.SerializeObject(cacheObject, Formatting.Indented);
                        writer.Write(json);
                        stream.Flush();
                    }
                }

                return handler;
            }

            return null;
        }

        public static async Task <List<Handler>> GetAllHandlers()
        {
            if (cacheObject == null)
            {
              await InitCache();
            }

            List<Handler> handlers = new List<Handler>();

            for (int i = 0; i < handlersArray.Count; i++)
            {
                Handler handler = new Handler
                {
                    Id = handlersArray[i]["_id"].ToString(),
                    Owner = handlersArray[i]["owner"].ToString(),
                    OwnerName = handlersArray[i]["ownerName"].ToString(),
                    Description = handlersArray[i]["description"].ToString(),
                    Title = handlersArray[i]["title"].ToString(),
                    GameName = handlersArray[i]["gameName"].ToString(),
                    GameDescription = handlersArray[i]["gameDescription"].ToString(),
                    GameCover = handlersArray[i]["gameCover"].ToString(),
                    GameId = handlersArray[i]["gameId"].ToString(),
                    GameUrl = handlersArray[i]["gameUrl"].ToString(),
                    CreatedAt = handlersArray[i]["createdAt"].ToString(),
                    UpdatedAt = handlersArray[i]["updatedAt"].ToString(),
                    Stars = handlersArray[i]["stars"].ToString(),
                    DownloadCount = handlersArray[i]["downloadCount"].ToString(),
                    Verified = handlersArray[i]["verified"].ToString(),
                    Private = handlersArray[i]["private"].ToString(),
                    CommentCount = handlersArray[i]["commentCount"].ToString(),
                    CurrentVersion = handlersArray[i]["currentVersion"].ToString(),
                    CurrentPackage = handlersArray[i]["currentPackage"].ToString(),
                };

                handlers.Add(handler);
            }

            return handlers;
        }

        public static async Task<string> GetScreenshotsUri(string id)
        {
            if (string.IsNullOrEmpty(id)) 
            {
                return null; 
            }

            string url = $@"https://hub.splitscreen.me/api/v1/screenshots/{id}";

            return await Http_Client.Http_Client.GetAsync(url);
        }

        public static async Task<JArray> SearchByName(string searchParam)
        {
            if (cacheObject == null)
            {
                await InitCache();
            }

            JArray searchHandlers = new JArray();

            for (int i = 0; i < handlersArray.Count; i++)
            {
                string gameName = handlersArray[i]["gameName"].ToString();

                if (gameName.StartsWith(searchParam, StringComparison.OrdinalIgnoreCase))
                {
                    searchHandlers.Add(handlersArray[i]);

                    if (i < handlersArray.Count - 1)
                    {
                        i++;
                    }
                }
            }

            if (searchHandlers.Count > 0)
            {
                return searchHandlers;
            }

            searchParam = Uri.EscapeDataString(searchParam);
            return await SearchByNameOnline(searchParam);
        }

        private static async Task<JArray> SearchByNameOnline(string searchParam)//No results found in the cache
        {
            string url = api + "handlers/" + searchParam;

            var result = await Http_Client.Http_Client.GetJsonPropertyAsync(url, "Handlers");

            if (result == null)
            {
                return null;
            }

            List<string> newFound = new List<string>();

            for (int i = 0; i < result.Count(); i++)
            {
                if (handlersArray.All(h => h["gameName"] != result[i]))//Update cache from hub resp because those handlers were not in cache
                {
                    cacheObject["Handlers"][0].AddBeforeSelf(result[i]);
                }
            }

            using (FileStream stream = new FileStream(cacheFile, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    string json = JsonConvert.SerializeObject(cacheObject, Formatting.Indented);
                    writer.Write(json);
                    stream.Flush();
                }
            }

            return result as JArray;
        }

        public static async Task ThumbnailsCache(Handler handler)
        {
            try
            {
                Regex pattern = new Regex("[\\/:*?\"<>|]");

                string fileName = pattern.Replace(handler.Id.ToLower(), "");

                if(!Directory.Exists(thumbnailFolder))
                {
                    Directory.CreateDirectory(thumbnailFolder);
                }

                if (!File.Exists(thumbnailFolder + fileName + ".jpeg"))
                {
                    string _coverUrl = $@"https://images.igdb.com/igdb/image/upload/t_micro/{handler.GameCover}.jpg";

                    await Http_Client.Http_Client.DownloadImageAsync(_coverUrl, thumbnailFolder + fileName + ".jpeg", 15000);
                }
               
            }
            catch (Exception)
            {

            }
        }
    }
}
